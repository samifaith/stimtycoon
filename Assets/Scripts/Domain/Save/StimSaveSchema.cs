using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace StimTycoon.Saves
{
    /// <summary>
    /// Locked save schema v1 for Phase 0.
    /// </summary>
    public static class StimSaveSchema
    {
        public const int SupportedSaveFormatVersion = 1;
        public const int SupportedMinimumReaderVersion = 1;
        public const int MinCoreStatValue = 0;
        public const int MaxCoreStatValue = 100;
    }

    [Serializable]
    public class StimSaveEnvelope
    {
        public int saveFormatVersion = StimSaveSchema.SupportedSaveFormatVersion;
        public int minimumReaderVersion = StimSaveSchema.SupportedMinimumReaderVersion;
        public string gameBuildVersion;
        public string contentVersion;
        public string saveId;
        public string playerAccountId;
        public string lifeId;
        public string createdAtUtc;
        public string updatedAtUtc;
        public int revision;
        public string deviceIdHash;
        public StimRngState rng = new StimRngState();
        public StimSaveIntegrity integrity = new StimSaveIntegrity();
        public StimGameState state = new StimGameState();
    }

    [Serializable]
    public class StimRngState
    {
        public int seed;
        public int step;
    }

    [Serializable]
    public class StimSaveIntegrity
    {
        public string payloadHash;
        public string previousRevisionHash;
    }

    [Serializable]
    public class StimGameState
    {
        public StimCharacterState character = new StimCharacterState();
        public StimCalendarState calendar = new StimCalendarState();
        public StimFinancesState finances = new StimFinancesState();
        public StimCareerState career = new StimCareerState();
        public StimEducationState education = new StimEducationState();
        public StimHouseholdState household = new StimHouseholdState();
        public List<StimSkillState> skills = new List<StimSkillState>();
        public List<StimRelationshipState> relationships = new List<StimRelationshipState>();
        public List<StimStatusState> statuses = new List<StimStatusState>();
        public List<StimAchievementState> achievements = new List<StimAchievementState>();
        public List<StimLifeDecisionState> lifeDecisions = new List<StimLifeDecisionState>();
        public List<StimActionProgressState> actionProgress = new List<StimActionProgressState>();
        public List<StimLifeFeedEntry> lifeFeed = new List<StimLifeFeedEntry>();
        public string pendingEventId;
        public List<StimEventHistoryEntry> eventHistory = new List<StimEventHistoryEntry>();
        public List<StimScheduledEventRecord> scheduledEvents = new List<StimScheduledEventRecord>();
    }

    [Serializable]
    public class StimCharacterState
    {
        public string firstName;
        public string lastName;
        public string pronouns;
        public string genderIdentity = "undiscovered";
        public string sexualOrientation = "undiscovered";
        public string country;
        public string backgroundId;
        public string avatarId;
        public int appearanceSeed;
        public string lifeStage = "infant";
        public string lifeStatus = "active";
        public string endingReason;
        public int endedAtAge = -1;
        public int age;
        public int health;
        public int happiness;
        public int smarts;
        public int looks = 50;
        public int luck = 50;
    }

    [Serializable]
    public class StimCalendarState
    {
        public int monthOfYear = 1;
        public int quietMonthsSinceEvent;
    }

    [Serializable]
    public class StimFinancesState
    {
        public long cashMinorUnits;
        public long debtMinorUnits;
        public long monthlyLivingExpensesMinorUnits;
        public int taxRateBasisPoints;
        public long spouseAnnualIncomeMinorUnits;
        public long householdCreditBalanceMinorUnits;
        public int householdCreditAprBasisPoints;
    }

    [Serializable]
    public class StimHouseholdState
    {
        public int happiness = 50;
        public int cohesion = 50;
    }

    [Serializable]
    public class StimCareerState
    {
        public string employerId;
        public string roleTitle;
        public long annualSalaryMinorUnits;
        public int careerProgress;
    }

    [Serializable]
    public class StimEducationState
    {
        public string stage = "not_started";
        public string schoolPath;
        public string awaitingDecisionId;
        public bool graduatedSecondary;
    }

    [Serializable]
    public class StimSkillState
    {
        public string skillId;
        public int experience;
    }

    [Serializable]
    public class StimRelationshipState
    {
        public string relationshipId;
        public string displayName;
        public string relationshipType;
        public string origin;
        public int introducedAtAge;
        public int monthsSinceInteraction;
        public bool isGeneticParent;
        public int geneticHealth;
        public int geneticLooks;
        public int geneticSmarts;
        public int npcSmarts;
        public int npcCareerLevel;
        public long npcAnnualIncomeMinorUnits;
        public long npcCashMinorUnits;
        public long npcDebtMinorUnits;
        public bool financesMerged;
        public int value = 50;
    }

    [Serializable]
    public class StimStatusState
    {
        public string statusId;
        public int remainingMonths;
    }

    [Serializable]
    public class StimAchievementState
    {
        public string achievementId;
        public int unlockedAtAge;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class StimLifeDecisionState
    {
        public string decisionId;
        public string choiceId;
        public int age;
        public int monthOfYear = 1;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class StimActionProgressState
    {
        public string instanceId;
        public string actionId;
        public string state = "Ready";
        public int progress;
        public int progressRequired = 1;
        public string resultSummary;
        public int revision;
        public string startedAtUtc;
        public string completedAtUtc;
    }

    [Serializable]
    public class StimLifeFeedEntry
    {
        public string entryId;
        public string category;
        public string text;
        public int age;
        public int monthOfYear;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class StimEventHistoryEntry
    {
        public string eventId;
        public string choiceId;
        public string outcomeId;
        public int age;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class StimScheduledEventRecord
    {
        public string eventId;
        public int earliestTriggerAge;
        public int latestTriggerAge;
        public float chance;
        public string sourceEventId;
        public string cancellationRule;
    }

    public class StimSaveValidationResult
    {
        public bool isValid = true;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    /// <summary>
    /// Validator for the locked Phase 0 save envelope.
    /// </summary>
    public static class StimSaveValidator
    {
        public static StimSaveValidationResult ValidateSave(StimSaveEnvelope save)
        {
            var result = new StimSaveValidationResult();

            if (save == null)
            {
                result.isValid = false;
                result.errors.Add("Save is null");
                return result;
            }

            if (save.saveFormatVersion != StimSaveSchema.SupportedSaveFormatVersion)
            {
                result.isValid = false;
                result.errors.Add($"Save format version {save.saveFormatVersion} not supported. Expected {StimSaveSchema.SupportedSaveFormatVersion}.");
            }

            if (save.minimumReaderVersion > StimSaveSchema.SupportedMinimumReaderVersion)
            {
                result.isValid = false;
                result.errors.Add($"Minimum reader version {save.minimumReaderVersion} is newer than supported {StimSaveSchema.SupportedMinimumReaderVersion}.");
            }

            ValidateRequiredString(result, save.saveId, "saveId");
            ValidateRequiredString(result, save.playerAccountId, "playerAccountId");
            ValidateRequiredString(result, save.lifeId, "lifeId");
            ValidateRequiredString(result, save.gameBuildVersion, "gameBuildVersion");
            ValidateRequiredString(result, save.contentVersion, "contentVersion");
            ValidateRequiredString(result, save.deviceIdHash, "deviceIdHash");

            if (save.revision < 1)
            {
                result.isValid = false;
                result.errors.Add("revision must be at least 1");
            }

            if (!TryParseUtcTimestamp(save.createdAtUtc, out var createdAtUtc))
            {
                result.isValid = false;
                result.errors.Add("createdAtUtc must be a valid UTC timestamp");
            }

            if (!TryParseUtcTimestamp(save.updatedAtUtc, out var updatedAtUtc))
            {
                result.isValid = false;
                result.errors.Add("updatedAtUtc must be a valid UTC timestamp");
            }

            if (createdAtUtc != default && updatedAtUtc != default && updatedAtUtc < createdAtUtc)
            {
                result.isValid = false;
                result.errors.Add("updatedAtUtc cannot be earlier than createdAtUtc");
            }

            if (save.rng == null)
            {
                result.isValid = false;
                result.errors.Add("rng is null");
            }
            else if (save.rng.step < 0)
            {
                result.isValid = false;
                result.errors.Add("rng.step cannot be negative");
            }

            if (save.integrity == null)
            {
                result.isValid = false;
                result.errors.Add("integrity is null");
            }
            else
            {
                ValidateRequiredString(result, save.integrity.payloadHash, "integrity.payloadHash");
            }

            if (save.state == null)
            {
                result.isValid = false;
                result.errors.Add("state is null");
                return result;
            }

            ValidateCharacterState(result, save.state.character);
            ValidateCalendarState(result, save.state.calendar);
            ValidateFinancesState(result, save.state.finances);
            ValidateCareerState(result, save.state.career);
            ValidateHouseholdState(result, save.state.household);
            ValidateSkills(result, save.state.skills);
            ValidateRelationships(result, save.state.relationships);
            ValidateStatuses(result, save.state.statuses);
            ValidateAchievements(result, save.state.achievements);
            ValidateLifeDecisions(result, save.state.lifeDecisions);
            ValidateActionProgress(result, save.state.actionProgress);
            ValidateEventHistory(result, save.state.eventHistory);
            ValidateScheduledEvents(result, save.state.scheduledEvents);

            return result;
        }

        public static string GetValidationSummary(StimSaveValidationResult result, string saveId)
        {
            if (result.isValid && result.warnings.Count == 0)
            {
                return $"✓ {saveId} is valid";
            }

            var summary = new StringBuilder();
            summary.AppendLine($"Validation for {saveId}:");

            if (!result.isValid)
            {
                summary.AppendLine("ERRORS:");
                foreach (var error in result.errors)
                {
                    summary.AppendLine($"  ✗ {error}");
                }
            }

            if (result.warnings.Count > 0)
            {
                summary.AppendLine("WARNINGS:");
                foreach (var warning in result.warnings)
                {
                    summary.AppendLine($"  ⚠ {warning}");
                }
            }

            return summary.ToString();
        }

        private static void ValidateCharacterState(StimSaveValidationResult result, StimCharacterState character)
        {
            if (character == null)
            {
                result.isValid = false;
                result.errors.Add("state.character is null");
                return;
            }

            if (character.age < 0)
            {
                result.isValid = false;
                result.errors.Add("state.character.age cannot be negative");
            }

            if (character.lifeStatus != "active" && character.lifeStatus != "deceased" &&
                character.lifeStatus != "retired")
            {
                result.isValid = false;
                result.errors.Add("state.character.lifeStatus must be active, deceased, or retired");
            }
            if (character.lifeStatus == "active" && character.endedAtAge != -1)
            {
                result.isValid = false;
                result.errors.Add("active lives must use endedAtAge=-1");
            }
            if (character.lifeStatus != "active" &&
                (character.endedAtAge < 0 || string.IsNullOrEmpty(character.endingReason)))
            {
                result.isValid = false;
                result.errors.Add("ended lives require endedAtAge and endingReason");
            }

            ValidateStatRange(result, character.health, "state.character.health");
            ValidateStatRange(result, character.happiness, "state.character.happiness");
            ValidateStatRange(result, character.smarts, "state.character.smarts");
            ValidateStatRange(result, character.looks, "state.character.looks");
            ValidateStatRange(result, character.luck, "state.character.luck");
        }

        private static void ValidateFinancesState(StimSaveValidationResult result, StimFinancesState finances)
        {
            if (finances == null)
            {
                result.isValid = false;
                result.errors.Add("state.finances is null");
                return;
            }

            if (finances.cashMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.cashMinorUnits cannot be negative");
            }

            if (finances.debtMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.debtMinorUnits cannot be negative");
            }
            if (finances.spouseAnnualIncomeMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.spouseAnnualIncomeMinorUnits cannot be negative");
            }
            if (finances.householdCreditBalanceMinorUnits < 0 ||
                finances.householdCreditBalanceMinorUnits > finances.debtMinorUnits)
            {
                result.isValid = false;
                result.errors.Add("household credit balance must be non-negative and cannot exceed total debt");
            }
            if (finances.householdCreditAprBasisPoints < 0 || finances.householdCreditAprBasisPoints > 10000)
            {
                result.isValid = false;
                result.errors.Add("household credit APR must be within [0, 10000] basis points");
            }


            if (finances.monthlyLivingExpensesMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.monthlyLivingExpensesMinorUnits cannot be negative");
            }

            if (finances.taxRateBasisPoints < 0 || finances.taxRateBasisPoints > 10000)
            {
                result.isValid = false;
                result.errors.Add("state.finances.taxRateBasisPoints must be within [0, 10000]");
            }
        }

        private static void ValidateHouseholdState(StimSaveValidationResult result, StimHouseholdState household)
        {
            if (household == null)
            {
                result.isValid = false;
                result.errors.Add("state.household is null");
                return;
            }
            ValidateStatRange(result, household.happiness, "state.household.happiness");
            ValidateStatRange(result, household.cohesion, "state.household.cohesion");
        }

        private static void ValidateCalendarState(StimSaveValidationResult result, StimCalendarState calendar)
        {
            if (calendar == null)
            {
                result.isValid = false;
                result.errors.Add("state.calendar is null");
                return;
            }

            if (calendar.monthOfYear < 1 || calendar.monthOfYear > 12)
            {
                result.isValid = false;
                result.errors.Add("state.calendar.monthOfYear must be within [1, 12]");
            }


            if (calendar.quietMonthsSinceEvent < 0)
            {
                result.isValid = false;
                result.errors.Add("state.calendar.quietMonthsSinceEvent cannot be negative");
            }
        }

        private static void ValidateCareerState(StimSaveValidationResult result, StimCareerState career)
        {
            if (career == null)
            {
                result.isValid = false;
                result.errors.Add("state.career is null");
                return;
            }

            if (career.annualSalaryMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.career.annualSalaryMinorUnits cannot be negative");
            }

            if (career.careerProgress < 0 || career.careerProgress > 100)
            {
                result.isValid = false;
                result.errors.Add("state.career.careerProgress must be within [0, 100]");
            }
        }

        private static void ValidateSkills(StimSaveValidationResult result, List<StimSkillState> skills)
        {
            ValidateProgressRecords(
                result,
                skills,
                "state.skills",
                skill => skill?.skillId,
                skill => skill == null || skill.experience >= 0,
                "experience cannot be negative");
        }

        private static void ValidateRelationships(StimSaveValidationResult result, List<StimRelationshipState> relationships)
        {
            ValidateProgressRecords(
                result,
                relationships,
                "state.relationships",
                relationship => relationship?.relationshipId,
                relationship => relationship == null ||
                                relationship.value >= 0 && relationship.value <= 100 &&
                                relationship.monthsSinceInteraction >= 0 &&
                                relationship.npcSmarts >= 0 && relationship.npcSmarts <= 100 &&
                                relationship.npcCareerLevel >= 0 && relationship.npcCareerLevel <= 5 &&
                                relationship.npcAnnualIncomeMinorUnits >= 0 &&
                                relationship.npcCashMinorUnits >= 0 && relationship.npcDebtMinorUnits >= 0,
                "value/NPC fields must be within range and monthsSinceInteraction cannot be negative");
        }

        private static void ValidateStatuses(StimSaveValidationResult result, List<StimStatusState> statuses)
        {
            ValidateProgressRecords(
                result,
                statuses,
                "state.statuses",
                status => status?.statusId,
                status => status == null || status.remainingMonths > 0,
                "remainingMonths must be greater than zero");
        }

        private static void ValidateAchievements(
            StimSaveValidationResult result,
            List<StimAchievementState> achievements)
        {
            ValidateProgressRecords(
                result,
                achievements,
                "state.achievements",
                achievement => achievement?.achievementId,
                achievement => achievement == null || achievement.unlockedAtAge >= 0 && achievement.revision >= 1,
                "unlock age must be non-negative and revision must be positive");
        }

        private static void ValidateLifeDecisions(
            StimSaveValidationResult result,
            List<StimLifeDecisionState> decisions)
        {
            ValidateProgressRecords(
                result,
                decisions,
                "state.lifeDecisions",
                decision => decision?.decisionId,
                decision => decision == null ||
                            !string.IsNullOrWhiteSpace(decision.choiceId) &&
                            decision.age >= 0 &&
                            decision.monthOfYear >= 1 && decision.monthOfYear <= 12 &&
                            decision.revision >= 0,
                "choiceId is required; age/revision cannot be negative; monthOfYear must be within [1, 12]");
        }

        private static void ValidateActionProgress(
            StimSaveValidationResult result,
            List<StimActionProgressState> actions)
        {
            var validStates = new HashSet<string>(StringComparer.Ordinal)
                { "Ready", "InProgress", "Complete", "Claimable", "Locked" };
            ValidateProgressRecords(
                result,
                actions,
                "state.actionProgress",
                action => action?.instanceId,
                action => action == null ||
                          !string.IsNullOrWhiteSpace(action.actionId) &&
                          validStates.Contains(action.state) &&
                          action.progressRequired > 0 &&
                          action.progress >= 0 && action.progress <= action.progressRequired &&
                          action.revision >= 0,
                "actionId/state must be valid; progress must be within range; revision cannot be negative");
        }

        private static void ValidateProgressRecords<T>(
            StimSaveValidationResult result,
            List<T> records,
            string fieldName,
            Func<T, string> getId,
            Func<T, bool> isValueValid,
            string invalidValueMessage)
            where T : class
        {
            if (records == null)
            {
                result.isValid = false;
                result.errors.Add($"{fieldName} is null");
                return;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < records.Count; index++)
            {
                var record = records[index];
                if (record == null)
                {
                    result.isValid = false;
                    result.errors.Add($"{fieldName}[{index}] is null");
                    continue;
                }

                var id = getId(record);
                ValidateRequiredString(result, id, $"{fieldName}[{index}].id");
                if (!string.IsNullOrEmpty(id) && !ids.Add(id))
                {
                    result.isValid = false;
                    result.errors.Add($"{fieldName} contains duplicate id {id}");
                }

                if (!isValueValid(record))
                {
                    result.isValid = false;
                    result.errors.Add($"{fieldName}[{index}] {invalidValueMessage}");
                }
            }
        }

        private static void ValidateEventHistory(StimSaveValidationResult result, List<StimEventHistoryEntry> eventHistory)
        {
            if (eventHistory == null)
            {
                result.isValid = false;
                result.errors.Add("state.eventHistory is null");
                return;
            }

            for (var index = 0; index < eventHistory.Count; index++)
            {
                var entry = eventHistory[index];
                if (entry == null)
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}] is null");
                    continue;
                }

                ValidateRequiredString(result, entry.eventId, $"state.eventHistory[{index}].eventId");
                ValidateRequiredString(result, entry.choiceId, $"state.eventHistory[{index}].choiceId");
                ValidateRequiredString(result, entry.outcomeId, $"state.eventHistory[{index}].outcomeId");

                if (entry.age < 0)
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}].age cannot be negative");
                }

                if (entry.revision < 1)
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}].revision must be at least 1");
                }

                if (!TryParseUtcTimestamp(entry.timestampUtc, out _))
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}].timestampUtc must be a valid UTC timestamp");
                }
            }
        }

        private static void ValidateScheduledEvents(StimSaveValidationResult result, List<StimScheduledEventRecord> scheduledEvents)
        {
            if (scheduledEvents == null)
            {
                result.isValid = false;
                result.errors.Add("state.scheduledEvents is null");
                return;
            }

            for (var index = 0; index < scheduledEvents.Count; index++)
            {
                var record = scheduledEvents[index];
                if (record == null)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}] is null");
                    continue;
                }

                ValidateRequiredString(result, record.eventId, $"state.scheduledEvents[{index}].eventId");
                ValidateRequiredString(result, record.sourceEventId, $"state.scheduledEvents[{index}].sourceEventId");

                if (record.earliestTriggerAge < 0)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].earliestTriggerAge cannot be negative");
                }

                if (record.latestTriggerAge < record.earliestTriggerAge)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].latestTriggerAge cannot be earlier than earliestTriggerAge");
                }

                if (record.chance < 0f || record.chance > 1f)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].chance must be within [0, 1]");
                }
            }
        }

        private static void ValidateStatRange(StimSaveValidationResult result, int value, string fieldName)
        {
            if (value < StimSaveSchema.MinCoreStatValue || value > StimSaveSchema.MaxCoreStatValue)
            {
                result.isValid = false;
                result.errors.Add($"{fieldName} must be within [{StimSaveSchema.MinCoreStatValue}, {StimSaveSchema.MaxCoreStatValue}]");
            }
        }

        private static void ValidateRequiredString(StimSaveValidationResult result, string value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
            {
                result.isValid = false;
                result.errors.Add($"{fieldName} is required but empty or null");
            }
        }

        private static bool TryParseUtcTimestamp(string value, out DateTimeOffset timestamp)
        {
            return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out timestamp);
        }
    }
}
