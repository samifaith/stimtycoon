using System;
using System.Collections.Generic;
using UnityEngine;

namespace StimTycoon.Events
{
    /// <summary>
    /// Stim Tycoon Event Schema v1.0
    /// 
    /// This defines the contract for all authored events. All events must conform to this schema.
    /// Schema version and field requirements are locked; breaking changes create a new version.
    /// </summary>

    /// <summary>
    /// Event classification for organization and filtering.
    /// </summary>
    public enum EventCategory
    {
        Childhood = 0,
        School = 1,
        Career = 2,
        Health = 3,
        Money = 4,
        Relationship = 5,
        Business = 6,
        World = 7,
        Legacy = 8
    }

    /// <summary>
    /// Player-facing risk label for a choice.
    /// </summary>
    public enum RiskLevel
    {
        Safe = 0,           // 70–100% success
        Moderate = 1,       // 50–69% success
        Risky = 2,          // 30–49% success
        Extreme = 3,        // 0–29% success
        Calculated = 4      // Determined at runtime
    }

    /// <summary>
    /// Player-facing reward label for a choice.
    /// </summary>
    public enum RewardLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Exceptional = 3
    }

    /// <summary>
    /// Outcome classification.
    /// </summary>
    public enum OutcomeClassification
    {
        Positive = 0,
        Neutral = 1,
        Negative = 2
    }

    /// <summary>
    /// Effect type enumeration for state mutations.
    /// </summary>
    public enum EffectType
    {
        StatDelta,
        SkillXp,
        CashDelta,
        DebtDelta,
        RelationshipDelta,
        ReputationDelta,
        HealthConditionAdd,
        HealthConditionRemove,
        TraitAdd,
        TraitRemove,
        StatusAdd,
        StatusRemove,
        CareerProgressDelta,
        BusinessMetricDelta,
        AssetAdd,
        AssetRemove,
        ScheduleEvent,
        UnlockContent,
        SalaryDelta
    }

    /// <summary>
    /// Event repeat policy.
    /// </summary>
    public enum RepeatPolicy
    {
        Never,
        OncePerLifeStage,
        Repeatable
    }

    public enum EventTimingPolicy
    {
        AnyMonth = 0,
        AnnualRollover = 1,
        SpecificMonth = 2
    }

    /// <summary>
    /// Single effect that mutates game state when applied.
    /// </summary>
    [System.Serializable]
    public class Effect
    {
        public EffectType type;
        public string targetId;        // e.g., "health", "negotiation", "cash"
        public float value;            // magnitude of change
        public string metadata;        // JSON string for complex effects
    }

    /// <summary>
    /// Single outcome branch within a choice.
    /// </summary>
    [System.Serializable]
    public class Outcome
    {
        public string id;                                          // e.g., "negotiation_success"
        public OutcomeClassification classification;               // positive, neutral, negative
        public string resultTextKey;                               // Localization key for result copy
        public float weightWithinResultGroup;                      // Relative weight among outcomes
        public List<Effect> effects = new List<Effect>();         // State mutations to apply
        public string feedEntryKey;                                // Life feed summary localization
        public List<ScheduledEventRef> followUps = new List<ScheduledEventRef>();
        public string telemetryCode;                               // Analytics tag
    }

    /// <summary>
    /// Reference to an event scheduled for later.
    /// </summary>
    [System.Serializable]
    public class ScheduledEventRef
    {
        public string eventId;
        public int minYearsFromNow;
        public int maxYearsFromNow;
        public float probability;      // 0–1
        public string cancellationRule; // Metadata for when this doesn't fire
    }

    /// <summary>
    /// Single player choice within an event.
    /// </summary>
    [System.Serializable]
    public class Choice
    {
        public string id;                                      // e.g., "accept_increase"
        public string labelKey;                                // Localization key for button text
        public RiskLevel riskPreview;                          // Safe, Moderate, Risky, Extreme
        public RewardLevel rewardPreview;                      // Low, Medium, High, Exceptional
        public string requirements;                            // JSON: skill levels, traits, age, etc.
        public float baseSuccessChance;                        // 0–1, before modifiers
        public List<string> modifierRuleIds = new List<string>();  // e.g., "skill_negotiation_2"
        public List<Outcome> outcomes = new List<Outcome>();  // Possible result branches
    }

    /// <summary>
    /// Age range constraint for event eligibility.
    /// </summary>
    [System.Serializable]
    public class AgeRange
    {
        public int minAge;
        public int maxAge;
    }

    /// <summary>
    /// Complete event definition conforming to Stim Event Schema v1.
    /// </summary>
    [System.Serializable]
    public class StimEvent
    {
        // Required fields
        public int schemaVersion = 1;
        public string id;                                      // e.g., "career_salary_negotiation_001"
        public EventCategory category;
        public string titleKey;                                // Localization key
        public string bodyKey;                                 // Localization key
        public List<string> toneTags = new List<string>();    // e.g., "grounded", "tense", "warm"
        public AgeRange ageRange;
        public List<string> locations = new List<string>();   // "USA", "Jamaica"
        public string requirementsJson;                        // JSON object with eligibility rules
        public string exclusionsJson;                          // JSON object; if true, event can't run
        public List<Choice> choices = new List<Choice>();
        public int cooldownYears;
        public RepeatPolicy repeatPolicy;
        public List<string> analyticsTags = new List<string>();

        // Timing and frequency. AnyMonth is the default so ordinary events are not tied to birthdays.
        public EventTimingPolicy timingPolicy = EventTimingPolicy.AnyMonth;
        public int requiredMonth;
        public float monthlyTriggerChance = 0.25f;

        // Optional fields
        public string exclusiveToTraits;                       // JSON: only runs if player has any of these traits
        public string forbiddenByTraits;                       // JSON: can't run if player has any of these
        public string gameVersion;                             // First version this event appeared in
    }

    /// <summary>
    /// Validation result for a single event.
    /// </summary>
    public class EventValidationResult
    {
        public bool isValid = true;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }
}
