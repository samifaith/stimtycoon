using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class MatchBinder
    {
        private readonly Label status;
        private readonly Label score;
        private readonly VisualElement board;
        private readonly Label feedback;
        private int selectedIndex = -1;

        public MatchBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            status = root.Q<Label>("study-match-status");
            score = root.Q<Label>("study-match-score");
            board = root.Q<VisualElement>("study-match-board");
            feedback = root.Q<Label>("study-match-feedback");
            Card = root.Q<VisualElement>("study-match-card");
            Start = root.Q<Button>("study-match-start");
            Pause = root.Q<Button>("study-match-pause");
            Claim = root.Q<Button>("study-match-claim");
        }

        public Button Start { get; }
        public Button Pause { get; }
        public Button Claim { get; }
        public VisualElement Card { get; }
        public bool IsValid => Card != null && status != null && score != null && board != null && feedback != null && Start != null && Pause != null && Claim != null;

        public void Render(MatchSessionState session, Action<int, int> swap)
        {
            session ??= new MatchSessionState();
            score.text = $"Score {session.score:N0} / {Math.Max(300, session.targetScore):N0}";
            status.text = session.state == "active" ? $"Active · Ends {session.completesAtUtc} · {session.rewardPreview}" :
                session.state == "paused" ? $"Paused · {session.remainingSeconds} seconds remain · {session.rewardPreview}" :
                session.state == "success" ? $"Success · {session.rewardPreview} ready" :
                session.state == "failure" ? "Time expired · Start again when ready" :
                session.state == "claimed" ? $"Claimed · Cooldown until age {session.cooldownUntilAge}, month {session.cooldownUntilMonth}" :
                "Match patterns to earn Qualification XP +10. No ad or Sparks required.";
            Start.EnableInClassList("hidden", session.state == "active" || session.state == "paused" || session.state == "success");
            Pause.EnableInClassList("hidden", session.state != "active" && session.state != "paused");
            Pause.text = session.state == "paused" ? "RESUME MATCH" : "PAUSE MATCH";
            Claim.EnableInClassList("hidden", session.state != "success");
            board.EnableInClassList("hidden", session.state == "none" || session.board == null || session.board.Count == 0);
            board.Clear();
            if (session.board == null) return;
            for (var index = 0; index < session.board.Count; index++)
            {
                var captured = index;
                var tile = new Button { name = $"study-match-tile-{index}", text = TileGlyph(session.board[index]) };
                tile.AddToClassList("st-match-tile");
                tile.tooltip = $"Row {index / Math.Max(1, session.width) + 1}, column {index % Math.Max(1, session.width) + 1}";
                tile.SetEnabled(session.state == "active");
                PresentationStateStyler.Apply(tile, index == selectedIndex
                    ? PresentationState.Selected : PresentationState.Available);
                tile.clicked += () => Select(captured, session.width, swap);
                board.Add(tile);
            }
        }

        public void ShowResult(bool succeeded, string summary)
        {
            selectedIndex = -1;
            FeedbackPresenter.ShowTransactionResult(feedback, succeeded, summary);
        }

        private void Select(int index, int width, Action<int, int> swap)
        {
            if (selectedIndex < 0) { selectedIndex = index; return; }
            var previous = selectedIndex;
            selectedIndex = -1;
            var adjacent = Math.Abs(previous - index) == width ||
                           Math.Abs(previous - index) == 1 && previous / width == index / width;
            if (adjacent) swap(previous, index);
            else selectedIndex = index;
        }

        private static string TileGlyph(int value)
        {
            var glyphs = new[] { "📘", "✏️", "🧠", "🔬", "📐", "💡", "⭐", "📎" };
            return glyphs[Math.Max(0, Math.Min(glyphs.Length - 1, value))];
        }
    }
}
