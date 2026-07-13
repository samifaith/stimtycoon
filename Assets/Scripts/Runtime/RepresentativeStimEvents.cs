using System.Collections.Generic;
using StimTycoon.Events;

namespace StimTycoon.Runtime
{
    public static class RepresentativeStimEvents
    {
        public const string SalaryNegotiationId = "career_salary_negotiation_001";
        public const string HealthBurnoutId = "health_body_asking_for_pause_001";

        public static StimEvent CreateSalaryNegotiation()
        {
            return new StimEvent
            {
                id = SalaryNegotiationId,
                category = EventCategory.Career,
                titleKey = "The annual review",
                bodyKey = "Your annual review is wrapping up. Your manager asks whether there is anything else you want to discuss.",
                toneTags = new List<string> { "grounded", "direct" },
                ageRange = new AgeRange { minAge = 18, maxAge = 75 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 2,
                repeatPolicy = RepeatPolicy.Repeatable,
                analyticsTags = new List<string> { "career", "negotiation" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "make_the_case",
                        labelKey = "Make the case for a raise",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.High,
                        baseSuccessChance = 0.55f,
                        modifierRuleIds = new List<string> { "skill_negotiation_2" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "raise_approved",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "Your manager approves a meaningful raise.",
                                feedEntryKey = "Negotiated a salary increase.",
                                telemetryCode = "salary_raise_approved",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.SalaryDelta, targetId = "annual_salary", value = 500000 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 5 }
                                }
                            },
                            new Outcome
                            {
                                id = "raise_declined",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "Your manager declines and asks you to revisit it next year.",
                                feedEntryKey = "A salary request was declined.",
                                telemetryCode = "salary_raise_declined",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -4 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "let_it_pass",
                        labelKey = "Let it pass for now",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Low,
                        baseSuccessChance = 0.9f,
                        modifierRuleIds = new List<string>(),
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "status_quo",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "You leave the review with your current salary unchanged.",
                                feedEntryKey = "Kept the current salary.",
                                telemetryCode = "salary_status_quo",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>()
                            }
                        }
                    }
                }
            };
        }

        public static StimEvent CreateHealthBurnout()
        {
            return new StimEvent
            {
                id = HealthBurnoutId,
                category = EventCategory.Health,
                titleKey = "Your body is asking for a pause",
                bodyKey = "You have been waking up exhausted, losing focus, and carrying work stress into every evening.",
                toneTags = new List<string> { "grounded", "reflective" },
                ageRange = new AgeRange { minAge = 18, maxAge = 80 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 3,
                repeatPolicy = RepeatPolicy.Repeatable,
                analyticsTags = new List<string> { "health", "burnout" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "take_a_break",
                        labelKey = "Take a few days to recover",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.85f,
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "restored_energy",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "The break gives your body and mind room to recover.",
                                feedEntryKey = "Took time to recover from burnout.",
                                telemetryCode = "health_burnout_restored",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = 8 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 5 }
                                }
                            },
                            new Outcome
                            {
                                id = "rest_was_not_enough",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "A short break helps, but the exhaustion returns quickly.",
                                feedEntryKey = "Burnout symptoms returned after a short break.",
                                telemetryCode = "health_burnout_persisted",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = -2 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "push_through",
                        labelKey = "Push through the exhaustion",
                        riskPreview = RiskLevel.Risky,
                        rewardPreview = RewardLevel.High,
                        baseSuccessChance = 0.4f,
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "deadline_met",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "You meet the deadline, but the pace is not sustainable.",
                                feedEntryKey = "Pushed through burnout to meet a deadline.",
                                telemetryCode = "health_burnout_deadline_met",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CareerProgressDelta, targetId = "career", value = 10 },
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = -2 }
                                }
                            },
                            new Outcome
                            {
                                id = "burnout_crash",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "Your body forces the pause you would not take voluntarily.",
                                feedEntryKey = "Burnout caused a serious crash.",
                                telemetryCode = "health_burnout_crash",
                                weightWithinResultGroup = 1f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.CareerProgressDelta, targetId = "career", value = -5 },
                                    new Effect { type = EffectType.StatDelta, targetId = "health", value = -12 },
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -8 }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
