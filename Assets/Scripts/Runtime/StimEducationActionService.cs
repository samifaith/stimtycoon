using System;
using System.Collections.Generic;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// Owns Education-action eligibility and candidate-save mutation.
    /// Persistence remains the responsibility of the transaction runner.
    /// </summary>
    public sealed class StimEducationActionService
    {
        public const string MonthlyCooldownStatusId = "monthly_education_action_used";

        public StimTransactionMutationResult ChooseStudyTrack(
            StimSaveEnvelope save,
            StimStudyTrack track)
        {
            if (save?.state?.character == null || save.state.character.age < 14 ||
                save.state.character.age >= 18)
                return StimTransactionMutationResult.Failure("Study tracks are available from ages 14 through 17.");
            save.state.education ??= new StimEducationState();
            if (!string.IsNullOrEmpty(save.state.education.studyTrack))
                return StimTransactionMutationResult.Failure("A study track has already been selected.");
            var cost = track == StimStudyTrack.Academic ? 5000L :
                track == StimStudyTrack.Vocational ? 7500L : 0L;
            if (save.state.finances.cashMinorUnits < cost)
                return StimTransactionMutationResult.Failure("Not enough cash for the selected study track materials.");
            save.state.finances.cashMinorUnits -= cost;
            save.state.education.studyTrack = track.ToString().ToLowerInvariant();
            var summary = $"{track} study track selected" +
                          (cost > 0 ? $" · Materials −${cost / 100m:0.00}" : string.Empty);
            AddLifeFeedEntry(save, summary);
            return StimTransactionMutationResult.Success(summary);
        }

        public static string GetActionId(StimEducationActionType actionType) =>
            $"education.{actionType.ToString().ToLowerInvariant()}";

        public static List<StimActionDefinition> GetDefinitions(StimGameState state)
        {
            var definitions = new List<StimActionDefinition>();
            foreach (StimEducationActionType actionType in Enum.GetValues(typeof(StimEducationActionType)))
            {
                var unlocked = TryGetRequirement(state, actionType, out var requirement);
                TryGetDeltas(actionType, out var xp, out var smarts, out var happiness);
                var coolingDown = state?.statuses?.Exists(
                    status => status != null && status.statusId == MonthlyCooldownStatusId) == true;
                definitions.Add(new StimActionDefinition
                {
                    id = GetActionId(actionType),
                    title = ToDisplayName(actionType.ToString()),
                    description = "Build Learning experience through focused school work.",
                    destination = StimActionDestination.Education,
                    state = unlocked && !coolingDown ? StimActionState.Ready : StimActionState.Locked,
                    lockedReason = !unlocked ? requirement : coolingDown
                        ? "School action already completed this month."
                        : string.Empty,
                    cooldownMonths = 1,
                    previews = new List<StimActionDeltaPreview>
                    {
                        new StimActionDeltaPreview("Learning XP", xp),
                        new StimActionDeltaPreview("Smarts", smarts)
                    }
                });
                if (happiness != 0)
                {
                    definitions[definitions.Count - 1].previews.Add(
                        new StimActionDeltaPreview("Happiness", happiness));
                }
            }
            return definitions;
        }

        public StimTransactionMutationResult Apply(
            StimSaveEnvelope candidateSave,
            StimEducationActionType actionType)
        {
            var instanceId = candidateSave == null
                ? string.Empty
                : $"{candidateSave.lifeId}:education:{candidateSave.state.character.age}:" +
                  $"{candidateSave.state.calendar.monthOfYear}:{actionType}";
            return Apply(candidateSave, actionType, new StimActionRequest(GetActionId(actionType), instanceId));
        }

        public StimTransactionMutationResult Apply(
            StimSaveEnvelope candidateSave,
            StimEducationActionType actionType,
            StimActionRequest request)
        {
            if (candidateSave?.state?.character == null)
            {
                return StimTransactionMutationResult.Failure("No active save is loaded.");
            }
            if (candidateSave.state.character.lifeStatus != "active")
            {
                return StimTransactionMutationResult.Failure(
                    "This life has ended. Start a new life to continue playing.");
            }
            if (!string.IsNullOrEmpty(candidateSave.state.pendingEventId))
            {
                return StimTransactionMutationResult.Failure(
                    $"Resolve pending event {candidateSave.state.pendingEventId} before studying.");
            }

            NormalizeCollections(candidateSave.state);
            if (string.IsNullOrWhiteSpace(request.InstanceId) || request.ActionId != GetActionId(actionType))
            {
                return StimTransactionMutationResult.Failure("A valid action instance is required.");
            }
            var previousAction = candidateSave.state.actionProgress.Find(
                action => action != null && action.instanceId == request.InstanceId);
            if (previousAction != null)
            {
                return StimTransactionMutationResult.Failure(
                    string.IsNullOrEmpty(previousAction.resultSummary)
                        ? "This action was already completed."
                        : previousAction.resultSummary);
            }
            if (!TryGetRequirement(candidateSave.state, actionType, out var requirement))
            {
                return StimTransactionMutationResult.Failure(requirement);
            }
            if (candidateSave.state.statuses.Exists(
                    status => status != null && status.statusId == MonthlyCooldownStatusId))
            {
                return StimTransactionMutationResult.Failure(
                    "You already completed a school action this month. Advance the month to study again.");
            }

            if (!TryGetDeltas(actionType, out var xpDelta, out var smartsDelta, out var happinessDelta))
            {
                return StimTransactionMutationResult.Failure(
                    $"Education action {actionType} is not supported.");
            }

            var previousLevel = GetSkillLevel(GetSkillExperience(candidateSave.state.skills, "learning"));
            ApplySkillXp(candidateSave.state.skills, "learning", xpDelta);
            var experience = GetSkillExperience(candidateSave.state.skills, "learning");
            var newLevel = GetSkillLevel(experience);
            candidateSave.state.character.smarts = ClampStat(
                candidateSave.state.character.smarts + smartsDelta);
            candidateSave.state.character.happiness = ClampStat(
                candidateSave.state.character.happiness + happinessDelta);
            candidateSave.state.statuses.Add(new StimStatusState
            {
                statusId = MonthlyCooldownStatusId,
                remainingMonths = 1
            });

            var summary = $"{ToDisplayName(actionType.ToString())} complete · Learning XP +{xpDelta}" +
                          $" · Smarts {FormatSignedValue(smartsDelta)}" +
                          (happinessDelta == 0
                              ? string.Empty
                              : $" · Happiness {FormatSignedValue(happinessDelta)}") +
                          (newLevel > previousLevel
                              ? $" · Learning Level +{newLevel - previousLevel}"
                              : string.Empty);
            AddLifeFeedEntry(candidateSave, summary);
            candidateSave.state.actionProgress.Add(new StimActionProgressState
            {
                instanceId = request.InstanceId,
                actionId = request.ActionId,
                state = StimActionState.Complete.ToString(),
                progress = 1,
                progressRequired = 1,
                resultSummary = summary,
                revision = candidateSave.revision,
                startedAtUtc = candidateSave.updatedAtUtc,
                completedAtUtc = candidateSave.updatedAtUtc
            });
            return StimTransactionMutationResult.Success(summary);
        }

        public static bool TryGetRequirement(
            StimGameState state,
            StimEducationActionType actionType,
            out string requirement)
        {
            if (state?.character == null || state.character.age < 6 || state.character.age >= 18)
            {
                requirement = "Available while enrolled in primary or secondary school.";
                return false;
            }
            var level = GetSkillLevel(GetSkillExperience(state.skills, "learning"));
            switch (actionType)
            {
                case StimEducationActionType.Read:
                case StimEducationActionType.Homework:
                    requirement = string.Empty;
                    return true;
                case StimEducationActionType.StudyGroup:
                    requirement = level >= 2 ? string.Empty : "Unlocks at Learning Level 2.";
                    return level >= 2;
                case StimEducationActionType.AdvancedProject:
                    if (state.character.age < 14)
                    {
                        requirement = "Unlocks at age 14.";
                        return false;
                    }
                    requirement = level >= 3 ? string.Empty : "Unlocks at Learning Level 3.";
                    return level >= 3;
                default:
                    requirement = "Unsupported school action.";
                    return false;
            }
        }

        public static int GetSkillExperience(List<StimSkillState> skills, string skillId)
        {
            var skill = skills?.Find(candidate => candidate != null && candidate.skillId == skillId);
            return Math.Max(0, skill?.experience ?? 0);
        }

        public static int GetSkillLevel(int experience)
        {
            experience = Math.Max(0, experience);
            var level = 1;
            while (25L * level * (level + 1) <= experience) level++;
            return level;
        }

        public static int GetExperienceForSkillLevel(int level)
        {
            if (level <= 1) return 0;
            return (int)Math.Min(int.MaxValue, 25L * (level - 1) * level);
        }

        private static bool TryGetDeltas(
            StimEducationActionType actionType,
            out int xpDelta,
            out int smartsDelta,
            out int happinessDelta)
        {
            switch (actionType)
            {
                case StimEducationActionType.Read:
                    xpDelta = 12; smartsDelta = 1; happinessDelta = 0; return true;
                case StimEducationActionType.Homework:
                    xpDelta = 18; smartsDelta = 1; happinessDelta = -1; return true;
                case StimEducationActionType.StudyGroup:
                    xpDelta = 25; smartsDelta = 1; happinessDelta = 1; return true;
                case StimEducationActionType.AdvancedProject:
                    xpDelta = 35; smartsDelta = 2; happinessDelta = -1; return true;
                default:
                    xpDelta = 0; smartsDelta = 0; happinessDelta = 0; return false;
            }
        }

        private static void NormalizeCollections(StimGameState state)
        {
            state.skills ??= new List<StimSkillState>();
            state.statuses ??= new List<StimStatusState>();
            state.lifeFeed ??= new List<StimLifeFeedEntry>();
            state.actionProgress ??= new List<StimActionProgressState>();
        }

        private static void ApplySkillXp(List<StimSkillState> skills, string skillId, int value)
        {
            var skill = skills.Find(candidate => candidate != null && candidate.skillId == skillId);
            if (skill == null)
            {
                skill = new StimSkillState { skillId = skillId };
                skills.Add(skill);
            }
            skill.experience = Math.Max(0, skill.experience + value);
        }

        private static void AddLifeFeedEntry(StimSaveEnvelope save, string text)
        {
            save.state.lifeFeed.Add(new StimLifeFeedEntry
            {
                entryId = $"{save.revision}_education_{save.state.lifeFeed.Count}",
                category = "education",
                text = text,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
        }

        private static int ClampStat(int value) =>
            Math.Max(StimSaveSchema.MinCoreStatValue, Math.Min(StimSaveSchema.MaxCoreStatValue, value));

        private static string FormatSignedValue(int value) =>
            $"{(value >= 0 ? "+" : "−")}{Math.Abs(value)}";

        private static string ToDisplayName(string id) =>
            string.IsNullOrEmpty(id) ? "life" : id.Replace('_', ' ');
    }
}
