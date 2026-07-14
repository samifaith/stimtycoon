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

        public StimTransactionMutationResult Apply(
            StimSaveEnvelope candidateSave,
            StimEducationActionType actionType)
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
