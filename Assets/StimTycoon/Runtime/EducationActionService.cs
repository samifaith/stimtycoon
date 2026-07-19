using System;
using System.Collections.Generic;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    public sealed class EducationDisciplineDefinition
    {
        public string disciplineId;
        public string displayName;
        public StudyTrack studyTrack;
        public string consequenceSummary;
    }

    public static class EducationDisciplineCatalog
    {
        private static readonly IReadOnlyList<EducationDisciplineDefinition> Definitions =
            new List<EducationDisciplineDefinition>
            {
                Create("applied_finance", "Applied Finance", StudyTrack.General,
                    "Builds broad business foundations and supports the Finance career ladder."),
                Create("community_health", "Community Health", StudyTrack.Academic,
                    "Builds theory-led qualifications required by the Healthcare career path."),
                Create("sustainable_trades", "Sustainable Trades", StudyTrack.Vocational,
                    "Builds practical qualifications required by the Skilled Trades career path.")
            };

        public static IReadOnlyList<EducationDisciplineDefinition> GetAll() => Definitions;

        public static EducationDisciplineDefinition GetForTrack(StudyTrack track)
        {
            foreach (var definition in Definitions)
                if (definition.studyTrack == track) return definition;
            return null;
        }

        private static EducationDisciplineDefinition Create(
            string id, string name, StudyTrack track, string consequence) =>
            new EducationDisciplineDefinition
            {
                disciplineId = id,
                displayName = name,
                studyTrack = track,
                consequenceSummary = consequence
            };
    }

    /// <summary>
    /// Owns Education-action eligibility and candidate-save mutation.
    /// Persistence remains the responsibility of the transaction runner.
    /// </summary>
    public sealed class EducationActionService
    {
        public const int CertificateQualificationExperience =
            ProgressionStandards.CertificateQualificationExperience;
        public const int DiplomaQualificationExperience =
            ProgressionStandards.DiplomaQualificationExperience;
        public const int AdvancedQualificationExperience =
            ProgressionStandards.AdvancedQualificationExperience;
        public const int EasyStudyExperience = 10;
        public const int MediumStudyExperience = 20;
        public const int HardStudyExperience = 35;

        public const string MonthlyCooldownStatusId = "monthly_education_action_used";

        public TransactionMutationResult ChooseStudyTrack(
            SaveEnvelope save,
            StudyTrack track)
        {
            if (save?.state?.character == null || save.state.character.age < 14 ||
                save.state.character.age >= 18)
                return TransactionMutationResult.Failure("Study tracks are available from ages 14 through 17.");
            if (!Enum.IsDefined(typeof(StudyTrack), track))
                return TransactionMutationResult.Failure("That study track is not available.");
            if (save.state.finances == null)
                return TransactionMutationResult.Failure("Financial information is unavailable.");
            save.state.education ??= new EducationState();
            if (!string.IsNullOrEmpty(save.state.education.studyTrack))
                return TransactionMutationResult.Failure("A study track has already been selected.");
            var cost = track == StudyTrack.Academic ? 5000L :
                track == StudyTrack.Vocational ? 7500L : 0L;
            if (save.state.finances.cashMinorUnits < cost)
                return TransactionMutationResult.Failure("Not enough cash for the selected study track materials.");
            save.state.finances.cashMinorUnits -= cost;
            save.state.education.studyTrack = track.ToString().ToLowerInvariant();
            var discipline = EducationDisciplineCatalog.GetForTrack(track);
            var summary = $"{track} study track selected" +
                          (discipline == null ? string.Empty : $" · {discipline.displayName}") +
                          (cost > 0 ? $" · Materials −${cost / 100m:0.00}" : string.Empty);
            AddLifeFeedEntry(save, summary);
            return TransactionMutationResult.Success(summary);
        }

        public static string GetActionId(EducationActionType actionType) =>
            $"education.{actionType.ToString().ToLowerInvariant()}";

        public static string GetStudySessionActionId(StudyDifficulty difficulty) =>
            $"education.study.{difficulty.ToString().ToLowerInvariant()}";

        public static List<ActionDefinition> GetStudySessionDefinitions(GameState state)
        {
            var definitions = new List<ActionDefinition>();
            foreach (StudyDifficulty difficulty in Enum.GetValues(typeof(StudyDifficulty)))
            {
                TryGetStudySessionDeltas(difficulty, out var qualificationXp, out var smarts, out var happiness);
                var available = TryGetStudySessionRequirement(state, difficulty, out var requirement);
                var coolingDown = state?.statuses?.Exists(
                    status => status != null && status.statusId == MonthlyCooldownStatusId) == true;
                definitions.Add(new ActionDefinition
                {
                    id = GetStudySessionActionId(difficulty),
                    title = $"{difficulty} Study Session",
                    description = "Advance your selected qualification through one month of focused study.",
                    destination = ActionDestination.Education,
                    state = available && !coolingDown ? ActionState.Ready : ActionState.Locked,
                    lockedReason = !available ? requirement : coolingDown
                        ? "School action already completed this month."
                        : string.Empty,
                    durationSeconds = difficulty == StudyDifficulty.Easy ? 60 :
                        difficulty == StudyDifficulty.Medium ? 120 : 180,
                    cooldownMonths = 1,
                    previews = new List<ActionDeltaPreview>
                    {
                        new ActionDeltaPreview("Qualification XP", qualificationXp),
                        new ActionDeltaPreview("Smarts", smarts),
                        new ActionDeltaPreview("Happiness", happiness)
                    }
                });
            }
            return definitions;
        }

        public TransactionMutationResult ApplyStudySession(
            SaveEnvelope candidateSave,
            StudyDifficulty difficulty)
        {
            if (candidateSave?.state?.character == null)
                return TransactionMutationResult.Failure("No active save is loaded.");
            if (!string.IsNullOrEmpty(candidateSave.state.pendingEventId))
                return TransactionMutationResult.Failure(
                    "Resolve the pending life event before studying.");
            NormalizeCollections(candidateSave.state);
            if (!TryGetStudySessionRequirement(candidateSave.state, difficulty, out var requirement))
                return TransactionMutationResult.Failure(requirement);
            if (candidateSave.state.statuses.Exists(
                    status => status != null && status.statusId == MonthlyCooldownStatusId))
                return TransactionMutationResult.Failure(
                    "You already completed a school action this month. Advance the month to study again.");
            if (!TryGetStudySessionDeltas(difficulty, out var xpDelta, out var smartsDelta,
                    out var happinessDelta))
                return TransactionMutationResult.Failure("That study difficulty is not available.");

            candidateSave.state.education.qualificationExperience = Math.Max(0,
                candidateSave.state.education.qualificationExperience + xpDelta);
            candidateSave.state.character.smarts = ClampStat(
                candidateSave.state.character.smarts + smartsDelta);
            candidateSave.state.character.happiness = ClampStat(
                candidateSave.state.character.happiness + happinessDelta);
            candidateSave.state.statuses.Add(new StatusState
            {
                statusId = MonthlyCooldownStatusId,
                remainingMonths = 1
            });
            var tier = GetQualificationTier(candidateSave.state.education.qualificationExperience);
            var summary = $"{difficulty} study session complete · Qualification XP +{xpDelta}" +
                          $" · Smarts {FormatSignedValue(smartsDelta)}" +
                          $" · Happiness {FormatSignedValue(happinessDelta)}" +
                          $" · {tier}";
            AddLifeFeedEntry(candidateSave, summary);
            return TransactionMutationResult.Success(summary);
        }

        public static bool TryGetStudySessionRequirement(
            GameState state,
            StudyDifficulty difficulty,
            out string requirement)
        {
            if (state?.character == null || state.character.lifeStatus != "active" ||
                state.character.age < 14 || state.character.age >= 18 ||
                state.education == null || string.IsNullOrEmpty(state.education.studyTrack))
            {
                requirement = "Choose a study track while enrolled at ages 14 through 17.";
                return false;
            }
            if (!Enum.IsDefined(typeof(StudyDifficulty), difficulty))
            {
                requirement = "Unsupported study difficulty.";
                return false;
            }
            if (difficulty == StudyDifficulty.Hard &&
                state.character.smarts < ProgressionStandards.StrongCoreStatStartsAt)
            {
                requirement = $"Requires {ProgressionStandards.StrongCoreStatStartsAt} Smarts.";
                return false;
            }
            requirement = string.Empty;
            return true;
        }

        public static string GetQualificationTier(int experience)
        {
            if (experience >= AdvancedQualificationExperience) return "Advanced Qualification";
            if (experience >= DiplomaQualificationExperience) return "Diploma Qualification";
            if (experience >= CertificateQualificationExperience) return "Certificate Qualification";
            return "Foundation Qualification";
        }

        public static int GetNextQualificationTierAt(int experience)
        {
            if (experience < CertificateQualificationExperience) return CertificateQualificationExperience;
            if (experience < DiplomaQualificationExperience) return DiplomaQualificationExperience;
            if (experience < AdvancedQualificationExperience) return AdvancedQualificationExperience;
            return AdvancedQualificationExperience;
        }

        private static bool TryGetStudySessionDeltas(
            StudyDifficulty difficulty,
            out int qualificationXp,
            out int smarts,
            out int happiness)
        {
            switch (difficulty)
            {
                case StudyDifficulty.Easy:
                    qualificationXp = EasyStudyExperience; smarts = 1; happiness = 0; return true;
                case StudyDifficulty.Medium:
                    qualificationXp = MediumStudyExperience; smarts = 1; happiness = -1; return true;
                case StudyDifficulty.Hard:
                    qualificationXp = HardStudyExperience; smarts = 2; happiness = -3; return true;
                default:
                    qualificationXp = 0; smarts = 0; happiness = 0; return false;
            }
        }

        public static List<ActionDefinition> GetDefinitions(GameState state)
        {
            var definitions = new List<ActionDefinition>();
            foreach (EducationActionType actionType in Enum.GetValues(typeof(EducationActionType)))
            {
                var unlocked = TryGetRequirement(state, actionType, out var requirement);
                TryGetDeltas(actionType, out var xp, out var smarts, out var happiness);
                var coolingDown = state?.statuses?.Exists(
                    status => status != null && status.statusId == MonthlyCooldownStatusId) == true;
                definitions.Add(new ActionDefinition
                {
                    id = GetActionId(actionType),
                    title = ToDisplayName(actionType.ToString()),
                    description = "Build Learning experience through focused school work.",
                    destination = ActionDestination.Education,
                    state = unlocked && !coolingDown ? ActionState.Ready : ActionState.Locked,
                    lockedReason = !unlocked ? requirement : coolingDown
                        ? "School action already completed this month."
                        : string.Empty,
                    cooldownMonths = 1,
                    previews = new List<ActionDeltaPreview>
                    {
                        new ActionDeltaPreview("Learning XP", xp),
                        new ActionDeltaPreview("Smarts", smarts)
                    }
                });
                if (happiness != 0)
                {
                    definitions[definitions.Count - 1].previews.Add(
                        new ActionDeltaPreview("Happiness", happiness));
                }
            }
            return definitions;
        }

        public TransactionMutationResult Apply(
            SaveEnvelope candidateSave,
            EducationActionType actionType)
        {
            var instanceId = candidateSave == null
                ? string.Empty
                : $"{candidateSave.lifeId}:education:{candidateSave.state.character.age}:" +
                  $"{candidateSave.state.calendar.monthOfYear}:{actionType}";
            return Apply(candidateSave, actionType, new ActionRequest(GetActionId(actionType), instanceId));
        }

        public TransactionMutationResult Apply(
            SaveEnvelope candidateSave,
            EducationActionType actionType,
            ActionRequest request)
        {
            if (candidateSave?.state?.character == null)
            {
                return TransactionMutationResult.Failure("No active save is loaded.");
            }
            if (candidateSave.state.character.lifeStatus != "active")
            {
                return TransactionMutationResult.Failure(
                    "This life has ended. Start a new life to continue playing.");
            }
            if (!string.IsNullOrEmpty(candidateSave.state.pendingEventId))
            {
                return TransactionMutationResult.Failure(
                    "Resolve the pending life event before studying.");
            }

            NormalizeCollections(candidateSave.state);
            if (string.IsNullOrWhiteSpace(request.InstanceId) || request.ActionId != GetActionId(actionType))
            {
                return TransactionMutationResult.Failure("A valid action instance is required.");
            }
            var previousAction = candidateSave.state.actionProgress.Find(
                action => action != null && action.instanceId == request.InstanceId);
            if (previousAction != null)
            {
                return TransactionMutationResult.Failure(
                    string.IsNullOrEmpty(previousAction.resultSummary)
                        ? "This action was already completed."
                        : previousAction.resultSummary);
            }
            if (!TryGetRequirement(candidateSave.state, actionType, out var requirement))
            {
                return TransactionMutationResult.Failure(requirement);
            }
            if (candidateSave.state.statuses.Exists(
                    status => status != null && status.statusId == MonthlyCooldownStatusId))
            {
                return TransactionMutationResult.Failure(
                    "You already completed a school action this month. Advance the month to study again.");
            }

            if (!TryGetDeltas(actionType, out var xpDelta, out var smartsDelta, out var happinessDelta))
            {
                return TransactionMutationResult.Failure(
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
            candidateSave.state.statuses.Add(new StatusState
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
            candidateSave.state.actionProgress.Add(new ActionProgressState
            {
                instanceId = request.InstanceId,
                actionId = request.ActionId,
                state = ActionState.Complete.ToString(),
                progress = 1,
                progressRequired = 1,
                resultSummary = summary,
                revision = candidateSave.revision,
                startedAtUtc = candidateSave.updatedAtUtc,
                completedAtUtc = candidateSave.updatedAtUtc
            });
            return TransactionMutationResult.Success(summary);
        }

        public static bool TryGetRequirement(
            GameState state,
            EducationActionType actionType,
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
                case EducationActionType.Read:
                case EducationActionType.Homework:
                    requirement = string.Empty;
                    return true;
                case EducationActionType.StudyGroup:
                    requirement = level >= 2 ? string.Empty : "Unlocks at Learning Level 2.";
                    return level >= 2;
                case EducationActionType.AdvancedProject:
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

        public static int GetSkillExperience(List<SkillState> skills, string skillId)
        {
            var skill = skills?.Find(candidate => candidate != null && candidate.skillId == skillId);
            return Math.Max(0, skill?.experience ?? 0);
        }

        public static int GetSkillLevel(int experience)
        {
            return ProgressionStandards.GetSkillLevel(experience);
        }

        public static int GetExperienceForSkillLevel(int level)
        {
            return ProgressionStandards.GetSkillExperienceForLevel(level);
        }

        private static bool TryGetDeltas(
            EducationActionType actionType,
            out int xpDelta,
            out int smartsDelta,
            out int happinessDelta)
        {
            switch (actionType)
            {
                case EducationActionType.Read:
                    xpDelta = 12; smartsDelta = 1; happinessDelta = 0; return true;
                case EducationActionType.Homework:
                    xpDelta = 18; smartsDelta = 1; happinessDelta = -1; return true;
                case EducationActionType.StudyGroup:
                    xpDelta = 25; smartsDelta = 1; happinessDelta = 1; return true;
                case EducationActionType.AdvancedProject:
                    xpDelta = 35; smartsDelta = 2; happinessDelta = -1; return true;
                default:
                    xpDelta = 0; smartsDelta = 0; happinessDelta = 0; return false;
            }
        }

        private static void NormalizeCollections(GameState state)
        {
            state.skills ??= new List<SkillState>();
            state.statuses ??= new List<StatusState>();
            state.lifeFeed ??= new List<LifeFeedEntry>();
            state.actionProgress ??= new List<ActionProgressState>();
        }

        private static void ApplySkillXp(List<SkillState> skills, string skillId, int value)
        {
            var skill = skills.Find(candidate => candidate != null && candidate.skillId == skillId);
            if (skill == null)
            {
                skill = new SkillState { skillId = skillId };
                skills.Add(skill);
            }
            skill.experience = Math.Max(0, skill.experience + value);
        }

        private static void AddLifeFeedEntry(SaveEnvelope save, string text)
        {
            save.state.historyArchive ??= new HistoryArchiveState();
            save.state.lifeFeed.Add(new LifeFeedEntry
            {
                entryId = $"{save.revision}_education_{save.state.historyArchive.lifeFeedArchivedCount + save.state.lifeFeed.Count}",
                category = "education",
                text = text,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            HistoryRetention.Apply(save.state);
        }

        private static int ClampStat(int value) =>
            Math.Max(SaveSchema.MinCoreStatValue, Math.Min(SaveSchema.MaxCoreStatValue, value));

        private static string FormatSignedValue(int value) =>
            $"{(value >= 0 ? "+" : "−")}{Math.Abs(value)}";

        private static string ToDisplayName(string id) =>
            string.IsNullOrEmpty(id) ? "life" : id.Replace('_', ' ');
    }
}
