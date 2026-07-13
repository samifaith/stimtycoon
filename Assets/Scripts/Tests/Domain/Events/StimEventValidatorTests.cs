using NUnit.Framework;
using System.Collections.Generic;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Events
{
    /// <summary>
    /// Unit tests for StimEventValidator and StimEvent schema compliance.
    /// </summary>
    public class StimEventValidatorTests
    {
        [Test]
        public void ValidateEvent_RejectsNullEvent()
        {
            var result = StimEventValidator.ValidateEvent(null);
            
            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Contains.Item("Event is null"));
        }

        [Test]
        public void ValidateEvent_RejectsWrongSchemaVersion()
        {
            var evt = CreateValidEvent();
            evt.schemaVersion = 99;

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ValidateEvent_RejectsMissingRequiredFields()
        {
            var evt = CreateValidEvent();
            evt.id = "";

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("id")));
        }

        [Test]
        public void ValidateEvent_RejectsInvalidAgeRange()
        {
            var evt = CreateValidEvent();
            evt.ageRange = new AgeRange { minAge = 50, maxAge = 10 };

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
        }

        [Test]
        public void ValidateEvent_RejectsNoChoices()
        {
            var evt = CreateValidEvent();
            evt.choices = new List<Choice>();

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("at least 2 choices")));
        }

        [Test]
        public void ValidateEvent_RejectsDuplicateChoiceIds()
        {
            var evt = CreateValidEvent();
            evt.choices[1].id = evt.choices[0].id; // Duplicate ID

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("Duplicate choice ID")));
        }

        [Test]
        public void ValidateEvent_RejectsChoiceWithNoOutcomes()
        {
            var evt = CreateValidEvent();
            evt.choices[0].outcomes = new List<Outcome>();

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
        }

        [Test]
        public void ValidateEvent_RejectsInvalidBaseSuccessChance()
        {
            var evt = CreateValidEvent();
            evt.choices[0].baseSuccessChance = 1.5f;

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("baseSuccessChance")));
        }

        [Test]
        public void ValidateEvent_RejectsOutcomeWithZeroWeight()
        {
            var evt = CreateValidEvent();
            evt.choices[0].outcomes[0].weightWithinResultGroup = 0;

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("weight")));
        }

        [Test]
        public void ValidateEvent_RejectsOutcomeWithoutStatChange()
        {
            var evt = CreateValidEvent();
            evt.choices[0].outcomes[0].effects = new List<Effect>();

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("must change at least one stat")));
        }

        [Test]
        public void ValidateEvent_WarnsMissingLocations()
        {
            var evt = CreateValidEvent();
            evt.locations = new List<string>();

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
        }

        [Test]
        public void ValidateEvent_WarnsUnknownLocation()
        {
            var evt = CreateValidEvent();
            evt.locations = new List<string> { "Atlantis" };

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.That(result.warnings, Has.Some.Matches<string>(w => w.Contains("Unknown location")));
        }

        [Test]
        public void ValidateEvent_PassesValidEvent()
        {
            var evt = CreateValidEvent();

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void ValidateEvent_RejectsInvalidMonthlyTriggerChance()
        {
            var evt = CreateValidEvent();
            evt.monthlyTriggerChance = 0f;

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("monthlyTriggerChance")));
        }

        [Test]
        public void ValidateEvent_RejectsSpecificTimingWithoutValidMonth()
        {
            var evt = CreateValidEvent();
            evt.timingPolicy = EventTimingPolicy.SpecificMonth;
            evt.requiredMonth = 13;

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("requiredMonth")));
        }

        [Test]
        public void ValidateEvent_PassesRepresentativeHealthEvent()
        {
            var result = StimEventValidator.ValidateEvent(RepresentativeStimEvents.CreateHealthBurnout());

            Assert.IsTrue(result.isValid, StimEventValidator.GetValidationSummary(result, RepresentativeStimEvents.HealthBurnoutId));
        }

        [Test]
        public void ValidateEvent_PassesRepresentativeMoneyEvent()
        {
            var result = StimEventValidator.ValidateEvent(RepresentativeStimEvents.CreateMoneyFastReturn());

            Assert.IsTrue(result.isValid, StimEventValidator.GetValidationSummary(result, RepresentativeStimEvents.MoneyFastReturnId));
        }

        [Test]
        public void ValidateEvent_PassesRepresentativeSchoolEvent()
        {
            var result = StimEventValidator.ValidateEvent(RepresentativeStimEvents.CreateSchoolGroupProject());

            Assert.IsTrue(result.isValid, StimEventValidator.GetValidationSummary(result, RepresentativeStimEvents.SchoolGroupProjectId));
        }

        [Test]
        public void ValidateEvent_PassesRepresentativeChildhoodEvent()
        {
            var result = StimEventValidator.ValidateEvent(RepresentativeStimEvents.CreateChildhoodGrownFolksTable());

            Assert.IsTrue(result.isValid, StimEventValidator.GetValidationSummary(result, RepresentativeStimEvents.ChildhoodGrownFolksTableId));
        }

        [Test]
        public void ValidateEvent_WarnsRiskyChoiceWithoutModifiers()
        {
            var evt = CreateValidEvent();
            evt.choices[0].riskPreview = RiskLevel.Risky;
            evt.choices[0].modifierRuleIds = new List<string>();

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.That(result.warnings.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ValidateEvent_RejectsRiskyChoiceWithLowReward()
        {
            var evt = CreateValidEvent();
            evt.choices[0].riskPreview = RiskLevel.Risky;
            evt.choices[0].rewardPreview = RewardLevel.Low;

            var result = StimEventValidator.ValidateEvent(evt);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("not balanced")));
        }

        [Test]
        public void GetValidationSummary_CreatesHumanReadableSummary()
        {
            var result = new EventValidationResult();
            result.isValid = false;
            result.errors.Add("Test error");
            result.warnings.Add("Test warning");

            var summary = StimEventValidator.GetValidationSummary(result, "test_event");

            Assert.That(summary, Contains.Substring("test_event"));
            Assert.That(summary, Contains.Substring("ERRORS"));
            Assert.That(summary, Contains.Substring("Test error"));
            Assert.That(summary, Contains.Substring("WARNINGS"));
            Assert.That(summary, Contains.Substring("Test warning"));
        }

        // ----

        /// <summary>
        /// Create a minimal valid event for testing.
        /// </summary>
        private StimEvent CreateValidEvent()
        {
            return new StimEvent
            {
                schemaVersion = 1,
                id = "test_event_001",
                category = EventCategory.Career,
                titleKey = "event.title",
                bodyKey = "event.body",
                toneTags = new List<string> { "grounded" },
                ageRange = new AgeRange { minAge = 18, maxAge = 65 },
                locations = new List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                analyticsTags = new List<string> { "career" },
                choices = new List<Choice>
                {
                    new Choice
                    {
                        id = "choice_1",
                        labelKey = "choice.label",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.5f,
                        modifierRuleIds = new List<string> { "skill_negotiation_2" },
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "outcome_positive",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "outcome.result",
                                feedEntryKey = "outcome.feed",
                                telemetryCode = "outcome_positive",
                                weightWithinResultGroup = 0.6f,
                                effects = new List<Effect>
                                {
                                    new Effect
                                    {
                                        type = EffectType.StatDelta,
                                        targetId = "smarts",
                                        value = 5
                                    }
                                }
                            },
                            new Outcome
                            {
                                id = "outcome_negative",
                                classification = OutcomeClassification.Negative,
                                resultTextKey = "outcome.result.neg",
                                feedEntryKey = "outcome.feed.neg",
                                telemetryCode = "outcome_negative",
                                weightWithinResultGroup = 0.4f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = -2 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "choice_2",
                        labelKey = "choice.label.2",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Low,
                        baseSuccessChance = 0.8f,
                        modifierRuleIds = new List<string>(),
                        outcomes = new List<Outcome>
                        {
                            new Outcome
                            {
                                id = "outcome_safe",
                                classification = OutcomeClassification.Neutral,
                                resultTextKey = "outcome.safe",
                                feedEntryKey = "outcome.safe.feed",
                                telemetryCode = "outcome_safe",
                                weightWithinResultGroup = 1.0f,
                                effects = new List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
