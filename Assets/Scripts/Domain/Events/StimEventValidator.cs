using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StimTycoon.Events
{
    /// <summary>
    /// Validator for Stim Event Schema v1.0
    /// 
    /// Ensures all authored events meet the locked contract before they can be used in gameplay.
    /// Validation is strict: events fail if they don't meet requirements.
    /// </summary>
    public static class StimEventValidator
    {
        private const int SUPPORTED_SCHEMA_VERSION = 1;

        /// <summary>
        /// Validate a complete event against the schema.
        /// </summary>
        public static EventValidationResult ValidateEvent(StimEvent evt)
        {
            var result = new EventValidationResult();

            if (evt == null)
            {
                result.isValid = false;
                result.errors.Add("Event is null");
                return result;
            }

            // Check schema version
            if (evt.schemaVersion != SUPPORTED_SCHEMA_VERSION)
            {
                result.isValid = false;
                result.errors.Add($"Schema version {evt.schemaVersion} not supported. Expected {SUPPORTED_SCHEMA_VERSION}.");
            }

            // Required string fields
            ValidateRequiredString(ref result, evt.id, "id");
            ValidateRequiredString(ref result, evt.titleKey, "titleKey");
            ValidateRequiredString(ref result, evt.bodyKey, "bodyKey");

            // Category
            if (string.IsNullOrEmpty(evt.category.ToString()))
            {
                result.errors.Add("category is not set");
                result.isValid = false;
            }

            // Locations
            if (evt.locations == null || evt.locations.Count == 0)
            {
                result.errors.Add("locations list is empty; must include 'USA' and/or 'Jamaica'");
                result.isValid = false;
            }
            else
            {
                foreach (var loc in evt.locations)
                {
                    if (loc != "USA" && loc != "Jamaica")
                    {
                        result.warnings.Add($"Unknown location: {loc}. Expected 'USA' or 'Jamaica'.");
                    }
                }
            }

            // Age range
            if (evt.ageRange == null)
            {
                result.errors.Add("ageRange is null");
                result.isValid = false;
            }
            else if (evt.ageRange.minAge < 0 || evt.ageRange.maxAge < 0 || evt.ageRange.minAge > evt.ageRange.maxAge)
            {
                result.errors.Add($"ageRange invalid: min={evt.ageRange.minAge}, max={evt.ageRange.maxAge}");
                result.isValid = false;
            }

            // Tone tags
            if (evt.toneTags == null || evt.toneTags.Count == 0)
            {
                result.warnings.Add("toneTags list is empty; consider adding editorial guidance");
            }

            // Choices
            if (evt.choices == null || evt.choices.Count < 2)
            {
                result.errors.Add("Event must have at least 2 choices");
                result.isValid = false;
            }
            else
            {
                var choiceIds = new HashSet<string>();
                for (int i = 0; i < evt.choices.Count; i++)
                {
                    var choiceResult = ValidateChoice(evt.choices[i], i);
                    if (!choiceResult.isValid)
                    {
                        result.isValid = false;
                    }
                    result.errors.AddRange(choiceResult.errors);
                    result.warnings.AddRange(choiceResult.warnings);

                    // Check for duplicate choice IDs
                    if (choiceIds.Contains(evt.choices[i].id))
                    {
                        result.errors.Add($"Duplicate choice ID: {evt.choices[i].id}");
                        result.isValid = false;
                    }
                    choiceIds.Add(evt.choices[i].id);
                }
            }

            // Repeat policy
            if (evt.cooldownYears < 0)
            {
                result.errors.Add("cooldownYears cannot be negative");
                result.isValid = false;
            }


            if (evt.monthlyTriggerChance <= 0f || evt.monthlyTriggerChance > 1f)
            {
                result.errors.Add("monthlyTriggerChance must be within (0, 1]");
                result.isValid = false;
            }

            if (evt.timingPolicy == EventTimingPolicy.SpecificMonth &&
                (evt.requiredMonth < 1 || evt.requiredMonth > 12))
            {
                result.errors.Add("requiredMonth must be within [1, 12] for SpecificMonth events");
                result.isValid = false;
            }

            // Analytics tags (recommended but not required)
            if (evt.analyticsTags == null || evt.analyticsTags.Count == 0)
            {
                result.warnings.Add("analyticsTags list is empty; add tags for telemetry");
            }

            return result;
        }

        /// <summary>
        /// Validate a single choice.
        /// </summary>
        private static EventValidationResult ValidateChoice(Choice choice, int index)
        {
            var result = new EventValidationResult();

            if (choice == null)
            {
                result.isValid = false;
                result.errors.Add($"Choice {index} is null");
                return result;
            }

            ValidateRequiredString(ref result, choice.id, $"Choice {index} id");
            ValidateRequiredString(ref result, choice.labelKey, $"Choice {index} labelKey");

            // Risk and reward
            if (choice.riskPreview == RiskLevel.Calculated)
            {
                result.warnings.Add($"Choice {index} uses Calculated risk; ensure modifiers are defined");
            }
            else if (!RiskRewardCalculator.ValidateRiskRewardOffset(
                         choice.riskPreview,
                         choice.rewardPreview,
                         out var riskRewardFeedback))
            {
                result.isValid = false;
                result.errors.Add($"Choice {index}: {riskRewardFeedback}");
            }

            // Base success chance
            if (choice.baseSuccessChance < 0f || choice.baseSuccessChance > 1f)
            {
                result.errors.Add($"Choice {index} baseSuccessChance out of range [0, 1]: {choice.baseSuccessChance}");
                result.isValid = false;
            }

            // Outcomes
            if (choice.outcomes == null || choice.outcomes.Count == 0)
            {
                result.errors.Add($"Choice {index} has no outcomes; must have at least 1");
                result.isValid = false;
            }
            else
            {
                var outcomeIds = new HashSet<string>();
                var totalPositive = 0;
                var totalNegative = 0;
                var totalNeutral = 0;

                for (int i = 0; i < choice.outcomes.Count; i++)
                {
                    var outcomeResult = ValidateOutcome(choice.outcomes[i], index, i);
                    if (!outcomeResult.isValid)
                    {
                        result.isValid = false;
                    }
                    result.errors.AddRange(outcomeResult.errors);
                    result.warnings.AddRange(outcomeResult.warnings);

                    // Check for duplicate outcome IDs within choice
                    if (outcomeIds.Contains(choice.outcomes[i].id))
                    {
                        result.errors.Add($"Choice {index} has duplicate outcome ID: {choice.outcomes[i].id}");
                        result.isValid = false;
                    }
                    outcomeIds.Add(choice.outcomes[i].id);

                    // Count outcome types
                    switch (choice.outcomes[i].classification)
                    {
                        case OutcomeClassification.Positive:
                            totalPositive++;
                            break;
                        case OutcomeClassification.Negative:
                            totalNegative++;
                            break;
                        case OutcomeClassification.Neutral:
                            totalNeutral++;
                            break;
                    }
                }

                // Validate outcome distribution
                if (totalPositive == 0 && totalNegative == 0 && totalNeutral > 0)
                {
                    result.warnings.Add($"Choice {index} has only neutral outcomes; consider adding risk/reward variation");
                }

                if ((choice.riskPreview == RiskLevel.Risky || choice.riskPreview == RiskLevel.Extreme) &&
                    (totalPositive == 0 || totalNegative == 0))
                {
                    result.isValid = false;
                    result.errors.Add($"Choice {index} is {choice.riskPreview} and must include both positive and negative outcomes");
                }
            }

            // Modifier rules (not required, but warn if missing for risky choices)
            if ((choice.modifierRuleIds == null || choice.modifierRuleIds.Count == 0) &&
                (choice.riskPreview == RiskLevel.Risky || choice.riskPreview == RiskLevel.Extreme))
            {
                result.warnings.Add($"Choice {index} is {choice.riskPreview} but has no modifiers; verify intent");
            }

            return result;
        }

        /// <summary>
        /// Validate a single outcome.
        /// </summary>
        private static EventValidationResult ValidateOutcome(Outcome outcome, int choiceIdx, int outcomeIdx)
        {
            var result = new EventValidationResult();

            if (outcome == null)
            {
                result.isValid = false;
                result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} is null");
                return result;
            }

            ValidateRequiredString(ref result, outcome.id, $"Choice {choiceIdx} Outcome {outcomeIdx} id");
            ValidateRequiredString(ref result, outcome.resultTextKey, $"Choice {choiceIdx} Outcome {outcomeIdx} resultTextKey");
            ValidateRequiredString(ref result, outcome.feedEntryKey, $"Choice {choiceIdx} Outcome {outcomeIdx} feedEntryKey");
            ValidateRequiredString(ref result, outcome.telemetryCode, $"Choice {choiceIdx} Outcome {outcomeIdx} telemetryCode");

            // Weight
            if (outcome.weightWithinResultGroup <= 0)
            {
                result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} weight must be > 0");
                result.isValid = false;
            }

            // Effects
            if (outcome.effects == null || outcome.effects.Count == 0)
            {
                result.isValid = false;
                result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} must change at least one stat");
            }
            else
            {
                var hasStatChange = false;
                for (int i = 0; i < outcome.effects.Count; i++)
                {
                    var effectResult = ValidateEffect(outcome.effects[i], choiceIdx, outcomeIdx, i);
                    if (!effectResult.isValid)
                    {
                        result.isValid = false;
                    }
                    result.errors.AddRange(effectResult.errors);
                    result.warnings.AddRange(effectResult.warnings);

                    if (IsNumericStatEffect(outcome.effects[i]) && Math.Abs(outcome.effects[i].value) > float.Epsilon)
                    {
                        hasStatChange = true;
                    }
                }

                if (!hasStatChange)
                {
                    result.isValid = false;
                    result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} must include a non-zero numeric stat change");
                }
            }

            return result;
        }

        private static bool IsNumericStatEffect(Effect effect)
        {
            if (effect == null)
            {
                return false;
            }

            switch (effect.type)
            {
                case EffectType.StatDelta:
                case EffectType.SkillXp:
                case EffectType.CashDelta:
                case EffectType.DebtDelta:
                case EffectType.RelationshipDelta:
                case EffectType.ReputationDelta:
                case EffectType.CareerProgressDelta:
                case EffectType.BusinessMetricDelta:
                case EffectType.SalaryDelta:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Validate a single effect.
        /// </summary>
        private static EventValidationResult ValidateEffect(Effect effect, int choiceIdx, int outcomeIdx, int effectIdx)
        {
            var result = new EventValidationResult();

            if (effect == null)
            {
                result.isValid = false;
                result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} Effect {effectIdx} is null");
                return result;
            }

            if (string.IsNullOrEmpty(effect.targetId))
            {
                result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} Effect {effectIdx} has no targetId");
                result.isValid = false;
            }

            // Validate effect type is known
            if (!Enum.IsDefined(typeof(EffectType), effect.type))
            {
                result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} Effect {effectIdx} has unknown type");
                result.isValid = false;
            }

            return result;
        }

        /// <summary>
        /// Helper to validate a required string field.
        /// </summary>
        private static void ValidateRequiredString(ref EventValidationResult result, string value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
            {
                result.isValid = false;
                result.errors.Add($"{fieldName} is required but empty or null");
            }
        }

        /// <summary>
        /// Get a human-readable summary of validation results.
        /// </summary>
        public static string GetValidationSummary(EventValidationResult result, string eventId)
        {
            if (result.isValid && result.warnings.Count == 0)
            {
                return $"✓ {eventId} is valid";
            }

            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Validation for {eventId}:");

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
    }
}
