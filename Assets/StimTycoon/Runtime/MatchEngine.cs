using System;
using System.Collections.Generic;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    public sealed class MatchThemeDefinition
    {
        public string activityId;
        public string theme;
        public int tileKinds;
        public int durationSeconds;
        public int targetScore;
        public int rewardAmount;
        public string rewardType;
        public string rewardPreview;
    }

    public sealed class MatchEngine
    {
        private const int Width = 8;
        private const int Height = 8;

        public static MatchThemeDefinition GetTheme(string theme)
        {
            switch (theme)
            {
                case "study_match": return new MatchThemeDefinition
                {
                    activityId = "match.study", theme = theme, tileKinds = 5, durationSeconds = 90,
                    targetScore = 300, rewardAmount = 10, rewardType = "qualification_xp",
                    rewardPreview = "Qualification XP +10"
                };
                case "shift_match": return new MatchThemeDefinition
                {
                    activityId = "match.shift", theme = theme, tileKinds = 6, durationSeconds = 90,
                    targetScore = 350, rewardAmount = 1, rewardType = "career_progress",
                    rewardPreview = "Career progress +1"
                };
                case "legacy_gems": return new MatchThemeDefinition
                {
                    activityId = "match.legacy", theme = theme, tileKinds = 6, durationSeconds = 120,
                    targetScore = 400, rewardAmount = 1000, rewardType = "cash_minor_units",
                    rewardPreview = "Cash +$10"
                };
                default: return null;
            }
        }

        public TransactionMutationResult Start(
            SaveEnvelope save, string themeId, string instanceId, DateTimeOffset now)
        {
            var theme = GetTheme(themeId);
            if (save?.state == null || theme == null || string.IsNullOrWhiteSpace(instanceId))
                return TransactionMutationResult.Failure("A valid match theme and instance are required.");
            var previous = save.state.matchSession ?? new MatchSessionState();
            if (previous.state == "active")
                return TransactionMutationResult.Failure("Finish the active match before starting another.");
            var currentMonth = save.state.character.age * 12 + save.state.calendar.monthOfYear;
            var cooldownMonth = previous.cooldownUntilAge * 12 + previous.cooldownUntilMonth;
            if (previous.state == "claimed" && currentMonth < cooldownMonth)
                return TransactionMutationResult.Failure("This match is on cooldown until next month.");

            var seed = StableHash(save.rng.seed, instanceId, themeId);
            var session = new MatchSessionState
            {
                activityId = theme.activityId,
                instanceId = instanceId,
                theme = theme.theme,
                state = "active",
                boardSeed = seed,
                width = Width,
                height = Height,
                durationSeconds = theme.durationSeconds,
                remainingSeconds = theme.durationSeconds,
                startedAtUtc = now.ToUniversalTime().ToString("O"),
                completesAtUtc = now.AddSeconds(theme.durationSeconds).ToUniversalTime().ToString("O"),
                targetScore = theme.targetScore,
                rewardAmount = theme.rewardAmount,
                rewardType = theme.rewardType,
                rewardPreview = theme.rewardPreview,
                rewardMultiplier = 1
            };
            GeneratePlayableBoard(session, theme.tileKinds);
            save.state.matchSession = session;
            return TransactionMutationResult.Success($"{themeId} started · Target {theme.targetScore} · {theme.rewardPreview}");
        }

        public TransactionMutationResult Swap(
            SaveEnvelope save, int first, int second, DateTimeOffset now)
        {
            var session = save?.state?.matchSession;
            if (session == null || session.state != "active")
                return TransactionMutationResult.Failure("No active match is available.");
            if (now >= ParseUtc(session.completesAtUtc))
            {
                Complete(session, false, now);
                return TransactionMutationResult.Success("Time expired · Match failed.");
            }
            if (!AreAdjacent(first, second, session.width, session.board.Count))
                return TransactionMutationResult.Failure("Select two adjacent tiles.");
            SwapTiles(session.board, first, second);
            if (!HasMatches(session.board, session.width, session.height))
            {
                SwapTiles(session.board, first, second);
                return TransactionMutationResult.Failure("That swap does not create a match.");
            }
            var removed = ResolveCascades(session, GetTheme(session.theme).tileKinds);
            if (!HasLegalMove(session.board, session.width, session.height))
                GeneratePlayableBoard(session, GetTheme(session.theme).tileKinds);
            if (session.score >= session.targetScore) Complete(session, true, now);
            return TransactionMutationResult.Success(
                $"Matched {removed} tiles · Score {session.score}/{session.targetScore}" +
                (session.state == "success" ? " · Reward ready" : string.Empty));
        }

        public TransactionMutationResult Reconcile(SaveEnvelope save, DateTimeOffset now)
        {
            var session = save?.state?.matchSession;
            if (session == null || session.state != "active")
                return TransactionMutationResult.Failure("No active match needs reconciliation.");
            if (now < ParseUtc(session.completesAtUtc))
                return TransactionMutationResult.Failure("The match timer is still active.");
            Complete(session, session.score >= session.targetScore, now);
            return TransactionMutationResult.Success(
                session.state == "success" ? "Match complete · Reward ready." : "Time expired · Match failed.");
        }

        public TransactionMutationResult Pause(SaveEnvelope save, DateTimeOffset now)
        {
            var session = save?.state?.matchSession;
            if (session == null || session.state != "active")
                return TransactionMutationResult.Failure("Only an active match can be paused.");
            var remaining = (int)Math.Ceiling((ParseUtc(session.completesAtUtc) - now).TotalSeconds);
            if (remaining <= 0)
            {
                Complete(session, session.score >= session.targetScore, now);
                return TransactionMutationResult.Success("Time expired while pausing.");
            }
            session.remainingSeconds = remaining;
            session.pausedAtUtc = now.ToUniversalTime().ToString("O");
            session.state = "paused";
            return TransactionMutationResult.Success($"Match paused · {remaining} seconds remain.");
        }

        public TransactionMutationResult Resume(SaveEnvelope save, DateTimeOffset now)
        {
            var session = save?.state?.matchSession;
            if (session == null || session.state != "paused" || session.remainingSeconds < 1)
                return TransactionMutationResult.Failure("No paused match is available.");
            session.completesAtUtc = now.AddSeconds(session.remainingSeconds).ToUniversalTime().ToString("O");
            session.pausedAtUtc = string.Empty;
            session.state = "active";
            return TransactionMutationResult.Success($"Match resumed · {session.remainingSeconds} seconds remain.");
        }

        public TransactionMutationResult Claim(SaveEnvelope save, DateTimeOffset now)
        {
            var session = save?.state?.matchSession;
            if (session == null) return TransactionMutationResult.Failure("Match session was not found.");
            if (session.rewardClaimed || session.state == "claimed")
                return TransactionMutationResult.Failure("This match reward was already claimed.");
            if (session.state != "success")
                return TransactionMutationResult.Failure("A successful match is required before claiming.");
            session.rewardClaimed = true;
            session.state = "claimed";
            session.claimedAtUtc = now.ToUniversalTime().ToString("O");
            session.claimedRevision = save.revision;
            var next = NextMonth(save.state.character.age, save.state.calendar.monthOfYear);
            session.cooldownUntilAge = next.age;
            session.cooldownUntilMonth = next.month;
            return TransactionMutationResult.Success($"Claimed {session.rewardPreview}.");
        }

        public static bool HasLegalMove(IReadOnlyList<int> board, int width, int height)
        {
            var copy = new List<int>(board);
            for (var index = 0; index < copy.Count; index++)
            {
                var x = index % width;
                if (x + 1 < width && CreatesMatch(copy, width, height, index, index + 1)) return true;
                if (index + width < copy.Count && CreatesMatch(copy, width, height, index, index + width)) return true;
            }
            return false;
        }

        private static bool CreatesMatch(List<int> board, int width, int height, int first, int second)
        {
            SwapTiles(board, first, second);
            var matched = HasMatches(board, width, height);
            SwapTiles(board, first, second);
            return matched;
        }

        private static int ResolveCascades(MatchSessionState session, int tileKinds)
        {
            var total = 0;
            for (var cascade = 0; cascade < 50; cascade++)
            {
                var matched = FindMatches(session.board, session.width, session.height);
                var count = 0;
                for (var index = 0; index < matched.Length; index++)
                    if (matched[index]) { session.board[index] = -1; count++; }
                if (count == 0) break;
                total += count;
                session.score = Math.Min(999999, session.score + count * 10);
                for (var x = 0; x < session.width; x++)
                {
                    var writeY = session.height - 1;
                    for (var y = session.height - 1; y >= 0; y--)
                    {
                        var value = session.board[y * session.width + x];
                        if (value < 0) continue;
                        session.board[writeY-- * session.width + x] = value;
                    }
                    while (writeY >= 0)
                        session.board[writeY-- * session.width + x] = NextTile(session, tileKinds);
                }
            }
            return total;
        }

        private static void GeneratePlayableBoard(MatchSessionState session, int tileKinds)
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                session.board.Clear();
                for (var index = 0; index < session.width * session.height; index++)
                {
                    var tile = NextTile(session, tileKinds);
                    var x = index % session.width;
                    var y = index / session.width;
                    for (var retry = 0; retry < tileKinds * 2 &&
                         (x >= 2 && session.board[index - 1] == tile && session.board[index - 2] == tile ||
                          y >= 2 && session.board[index - session.width] == tile && session.board[index - session.width * 2] == tile); retry++)
                        tile = (tile + 1) % tileKinds;
                    session.board.Add(tile);
                }
                if (HasLegalMove(session.board, session.width, session.height)) return;
            }
            throw new InvalidOperationException("Unable to generate a playable deterministic match board.");
        }

        private static bool HasMatches(IReadOnlyList<int> board, int width, int height)
        {
            var matches = FindMatches(board, width, height);
            for (var index = 0; index < matches.Length; index++) if (matches[index]) return true;
            return false;
        }

        private static bool[] FindMatches(IReadOnlyList<int> board, int width, int height)
        {
            var result = new bool[board.Count];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width - 2; x++)
                {
                    var index = y * width + x;
                    if (board[index] < 0 || board[index] != board[index + 1] || board[index] != board[index + 2]) continue;
                    var value = board[index];
                    while (x < width && board[y * width + x] == value) result[y * width + x++] = true;
                    x--;
                }
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height - 2; y++)
                {
                    var index = y * width + x;
                    if (board[index] < 0 || board[index] != board[index + width] || board[index] != board[index + width * 2]) continue;
                    var value = board[index];
                    while (y < height && board[y * width + x] == value) result[y++ * width + x] = true;
                    y--;
                }
            return result;
        }

        private static int NextTile(MatchSessionState session, int tileKinds)
        {
            unchecked
            {
                var value = (uint)session.boardSeed + 0x9E3779B9u * (uint)++session.rngStep;
                value ^= value >> 16;
                value *= 0x7FEB352D;
                value ^= value >> 15;
                return (int)(value % (uint)tileKinds);
            }
        }

        private static int StableHash(int seed, string instanceId, string theme)
        {
            unchecked
            {
                var hash = seed == 0 ? 17 : seed;
                foreach (var character in instanceId) hash = hash * 31 + character;
                foreach (var character in theme) hash = hash * 31 + character;
                return hash;
            }
        }

        private static void Complete(MatchSessionState session, bool success, DateTimeOffset now)
        {
            session.state = success ? "success" : "failure";
            session.remainingSeconds = 0;
            session.pausedAtUtc = string.Empty;
            session.completedAtUtc = now.ToUniversalTime().ToString("O");
        }

        private static DateTimeOffset ParseUtc(string value) =>
            DateTimeOffset.TryParse(value, out var parsed) ? parsed : DateTimeOffset.MinValue;

        private static bool AreAdjacent(int first, int second, int width, int count) =>
            first >= 0 && second >= 0 && first < count && second < count &&
            (Math.Abs(first - second) == width || Math.Abs(first - second) == 1 && first / width == second / width);

        private static void SwapTiles(List<int> board, int first, int second)
        {
            var value = board[first]; board[first] = board[second]; board[second] = value;
        }

        private static (int age, int month) NextMonth(int age, int month) =>
            month >= 12 ? (age + 1, 1) : (age, month + 1);
    }
}
