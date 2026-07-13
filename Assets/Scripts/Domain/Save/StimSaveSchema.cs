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
        public StimFinancesState finances = new StimFinancesState();
        public List<StimEventHistoryEntry> eventHistory = new List<StimEventHistoryEntry>();
        public List<StimScheduledEventRecord> scheduledEvents = new List<StimScheduledEventRecord>();
    }

    [Serializable]
    public class StimCharacterState
    {
        public int age;
        public int health;
        public int happiness;
        public int smarts;
    }

    [Serializable]
    public class StimFinancesState
    {
        public long cashMinorUnits;
        public long debtMinorUnits;
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
            ValidateFinancesState(result, save.state.finances);
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

            ValidateStatRange(result, character.health, "state.character.health");
            ValidateStatRange(result, character.happiness, "state.character.happiness");
            ValidateStatRange(result, character.smarts, "state.character.smarts");
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