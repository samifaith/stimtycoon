using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Runtime
{
    /// <summary>
    /// Tests for the first-party runtime event service.
    /// </summary>
    public class StimEventRuntimeServiceTests
    {
        [Test]
        public void CanRunEvent_ReturnsFalseWhenEventIsMissing()
        {
            var service = new StimEventRuntimeService(new InMemoryStimEventCatalog());

            var canRun = service.CanRunEvent("missing_event", out var summary);

            Assert.IsFalse(canRun);
            Assert.That(summary, Contains.Substring("was not found"));
        }

        [Test]
        public void CanRunEvent_ReturnsTrueForValidEvent()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(CreateValidEvent());
            var service = new StimEventRuntimeService(catalog);

            var canRun = service.CanRunEvent("test_event_001", out var summary);

            Assert.IsTrue(canRun);
            Assert.That(summary, Contains.Substring("is valid"));
        }

        [Test]
        public void GetRiskLabel_ReturnsDerivedLabel()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(CreateValidEvent());
            var service = new StimEventRuntimeService(catalog);

            var riskLabel = service.GetRiskLabel("test_event_001", "choice_1");

            Assert.That(riskLabel, Is.EqualTo(RiskLevel.Moderate.ToString()));
        }

        [Test]
        public void ResolveChoice_ReturnsFalseForUnknownChoice()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(CreateValidEvent());
            var service = new StimEventRuntimeService(catalog);

            var resolved = service.TryResolveChoice("test_event_001", "missing_choice", out var summary);

            Assert.IsFalse(resolved);
            Assert.That(summary, Contains.Substring("was not found"));
        }

        private static StimEvent CreateValidEvent()
        {
            return new StimEvent
            {
                schemaVersion = 1,
                id = "test_event_001",
                category = EventCategory.Career,
                titleKey = "event.title",
                bodyKey = "event.body",
                toneTags = new System.Collections.Generic.List<string> { "grounded" },
                ageRange = new AgeRange { minAge = 18, maxAge = 65 },
                locations = new System.Collections.Generic.List<string> { "USA", "Jamaica" },
                requirementsJson = "{}",
                cooldownYears = 1,
                repeatPolicy = RepeatPolicy.Repeatable,
                analyticsTags = new System.Collections.Generic.List<string> { "career" },
                choices = new System.Collections.Generic.List<Choice>
                {
                    new Choice
                    {
                        id = "choice_1",
                        labelKey = "choice.label",
                        riskPreview = RiskLevel.Moderate,
                        rewardPreview = RewardLevel.Medium,
                        baseSuccessChance = 0.5f,
                        modifierRuleIds = new System.Collections.Generic.List<string> { "skill_negotiation_2" },
                        outcomes = new System.Collections.Generic.List<Outcome>
                        {
                            new Outcome
                            {
                                id = "outcome_1",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "outcome.result",
                                feedEntryKey = "outcome.feed",
                                telemetryCode = "outcome_1",
                                weightWithinResultGroup = 1f,
                                effects = new System.Collections.Generic.List<Effect>
                                {
                                    new Effect { type = EffectType.StatDelta, targetId = "happiness", value = 1 }
                                }
                            }
                        }
                    },
                    new Choice
                    {
                        id = "choice_2",
                        labelKey = "choice.alternative",
                        riskPreview = RiskLevel.Safe,
                        rewardPreview = RewardLevel.Low,
                        baseSuccessChance = 0.9f,
                        modifierRuleIds = new System.Collections.Generic.List<string>(),
                        outcomes = new System.Collections.Generic.List<Outcome>
                        {
                            new Outcome
                            {
                                id = "outcome_2",
                                classification = OutcomeClassification.Positive,
                                resultTextKey = "outcome.alternative",
                                feedEntryKey = "outcome.alternative.feed",
                                telemetryCode = "outcome_2",
                                weightWithinResultGroup = 1f,
                                effects = new System.Collections.Generic.List<Effect>
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
