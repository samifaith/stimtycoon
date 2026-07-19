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
    public static class EventValidator
    {
        private const int SUPPORTED_SCHEMA_VERSION = 1;
        private static readonly IEffectValueResolver EffectValues = new EffectValueResolver();

        /// <summary>
        /// Validate a complete event against the schema.
        /// </summary>
        public static EventValidationResult ValidateEvent(Event evt)
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

            ValidateEditorialSafety(evt, result);

            return result;
        }

        /// <summary>Stricter shipping gate layered over the schema contract.</summary>
        public static EventValidationResult ValidateProductionEvent(Event evt)
        {
            var result = ValidateEvent(evt);
            if (evt == null) return result;
            if (!IsLocalizationSegment(evt.id))
                AddProductionError(result, $"Event id {evt.id} is not localization-key safe.");
            ValidateJsonObject(evt.requirementsJson, "requirementsJson", true, result);
            ValidateJsonObject(evt.exclusionsJson, "exclusionsJson", false, result);
            var localizationKeys = new HashSet<string>(StringComparer.Ordinal);
            AddLocalizationKey(localizationKeys, $"event.{evt.id}.title", result);
            AddLocalizationKey(localizationKeys, $"event.{evt.id}.body", result);
            var telemetryCodes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var choice in evt.choices ?? new List<Choice>())
            {
                if (choice == null) continue;
                if (!IsLocalizationSegment(choice.id))
                    AddProductionError(result, $"Choice id {choice.id} is not localization-key safe.");
                AddLocalizationKey(localizationKeys, $"event.{evt.id}.choice.{choice.id}", result);
                ValidateJsonObject(choice.requirements, $"choice {choice.id} requirements", false, result);
                if (!RiskRewardCalculator.ValidateRiskLevelAccuracy(
                        choice.baseSuccessChance, choice.riskPreview,
                        choice.modifierRuleIds ?? new List<string>(), out var riskFeedback))
                    AddProductionError(result, $"Choice {choice.id}: {riskFeedback}");
                if (choice.outcomes != null && choice.outcomes.Count > 1 &&
                    (choice.baseSuccessChance <= 0f || choice.baseSuccessChance >= 1f))
                    AddProductionError(result, $"Choice {choice.id} has unreachable outcome branches.");
                foreach (var outcome in choice.outcomes ?? new List<Outcome>())
                {
                    if (outcome == null) continue;
                    if (!IsLocalizationSegment(outcome.id))
                        AddProductionError(result, $"Outcome id {outcome.id} is not localization-key safe.");
                    AddLocalizationKey(localizationKeys,
                        $"event.{evt.id}.choice.{choice.id}.outcome.{outcome.id}.result", result);
                    AddLocalizationKey(localizationKeys,
                        $"event.{evt.id}.choice.{choice.id}.outcome.{outcome.id}.feed", result);
                    if (!telemetryCodes.Add(outcome.telemetryCode ?? string.Empty))
                        AddProductionError(result, $"Duplicate telemetry code {outcome.telemetryCode}.");
                    if (float.IsNaN(outcome.weightWithinResultGroup) || float.IsInfinity(outcome.weightWithinResultGroup))
                        AddProductionError(result, $"Outcome {outcome.id} has a non-finite weight.");
                    foreach (var followUp in outcome.followUps ?? new List<ScheduledEventRef>())
                        if (followUp == null || followUp.probability <= 0f || followUp.probability > 1f ||
                            followUp.minYearsFromNow < 0 || followUp.maxYearsFromNow < followUp.minYearsFromNow ||
                            string.IsNullOrWhiteSpace(followUp.cancellationRule))
                            AddProductionError(result, $"Outcome {outcome.id} has invalid follow-up eligibility metadata.");
                }
            }
            return result;
        }

        private static void ValidateJsonObject(
            string json, string field, bool required, EventValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                if (required) AddProductionError(result, $"{field} requires an explicit JSON object.");
                return;
            }
            var trimmed = json.Trim();
            if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}"))
            {
                AddProductionError(result, $"{field} must be a JSON object.");
                return;
            }
            try { JsonUtility.FromJson<EligibilityJsonProbe>(trimmed); }
            catch (ArgumentException) { AddProductionError(result, $"{field} contains malformed JSON."); }
        }

        private static void AddLocalizationKey(
            HashSet<string> keys, string key, EventValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(key) || !keys.Add(key))
                AddProductionError(result, $"Localization key {key} is invalid or duplicated.");
        }

        private static bool IsLocalizationSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            foreach (var character in value)
                if (!(character == '_' || character >= 'a' && character <= 'z' ||
                      character >= '0' && character <= '9')) return false;
            return true;
        }

        private static void AddProductionError(EventValidationResult result, string error)
        {
            result.isValid = false;
            result.errors.Add(error);
        }

        [Serializable]
        private sealed class EligibilityJsonProbe { }

        private static void ValidateEditorialSafety(Event evt, EventValidationResult result)
        {
            var tags = evt.analyticsTags ?? new List<string>();
            var toneTags = evt.toneTags ?? new List<string>();
            var isRomance = tags.Contains("romance") || toneTags.Contains("romance");
            if (isRomance && evt.ageRange != null && evt.ageRange.minAge < 18)
            {
                result.isValid = false;
                result.errors.Add("Romance events must exclude characters under age 18");
            }

            var isIntimateMilestone = tags.Contains("first_kiss") || tags.Contains("intimacy");
            if (isIntimateMilestone && !tags.Contains("consent") && !toneTags.Contains("consent"))
            {
                result.isValid = false;
                result.errors.Add("Intimate relationship events require an explicit consent tag");
            }

            if (tags.Contains("identity") && evt.ageRange != null && evt.ageRange.minAge < 16)
            {
                result.isValid = false;
                result.errors.Add("Authored identity-choice events must begin at age 16 or later");
            }

            var hasNegativeCashOutcome = (evt.choices ?? new List<Choice>())
                .Where(choice => choice != null)
                .SelectMany(choice => choice.outcomes ?? new List<Outcome>())
                .Where(outcome => outcome != null)
                .SelectMany(outcome => outcome.effects ?? new List<Effect>())
                .Any(effect => effect != null && effect.type == EffectType.CashDelta && effect.value < 0f);
            if (hasNegativeCashOutcome && evt.ageRange != null && evt.ageRange.minAge < 18 &&
                !tags.Contains("minor_cash_agency"))
            {
                result.isValid = false;
                result.errors.Add(
                    "Events that can charge cash to a character under age 18 require an explicit minor_cash_agency tag");
            }
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

            if (!string.IsNullOrWhiteSpace(effect.valueRuleId) &&
                !EffectValues.ContainsRule(effect.valueRuleId))
            {
                result.errors.Add($"Choice {choiceIdx} Outcome {outcomeIdx} Effect {effectIdx} " +
                                  $"has unknown value rule {effect.valueRuleId}");
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
