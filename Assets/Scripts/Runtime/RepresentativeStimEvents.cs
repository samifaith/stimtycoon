using System.Collections.Generic;
using StimTycoon.Events;

namespace StimTycoon.Runtime
{
    public static class RepresentativeStimEvents
    {
        public const string SalaryNegotiationId = "career_salary_negotiation_001";

        public static StimEvent CreateSalaryNegotiation()
        {
            return new StimEvent
            {
                id = SalaryNegotiationId,
                category = EventCategory.Career,
                titleKey = "event.career.salary_negotiation.title",
                bodyKey = "event.career.salary_negotiation.body",
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
                                    new Effect { type = EffectType.CashDelta, targetId = "cash", value = 50000 },
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
    }
}
