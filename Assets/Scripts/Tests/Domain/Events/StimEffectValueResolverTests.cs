using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Events
{
    public sealed class StimEffectValueResolverTests
    {
        [Test]
        public void AuthoredFallback_RemainsCompatibleWithoutBalanceKey()
        {
            var effect = new Effect { type = EffectType.CashDelta, targetId = "cash", value = 125f };

            Assert.That(new StimEffectValueResolver().Resolve(effect), Is.EqualTo(125f));
        }

        [Test]
        public void BalanceOverride_ChangesValueWithoutChangingAuthoredEffect()
        {
            var effect = new Effect
            {
                type = EffectType.SkillXp,
                targetId = "learning",
                value = 1f,
                valueRuleId = StimEffectValueRules.StagedSkillXpGain
            };
            var resolver = new StimEffectValueResolver(new Dictionary<string, float>
            {
                { StimEffectValueRules.StagedSkillXpGain, 7f }
            });

            Assert.That(resolver.Resolve(effect), Is.EqualTo(7f));
            Assert.That(effect.value, Is.EqualTo(1f), "The authored fallback remains migration-safe.");
        }

        [Test]
        public void Resolver_RejectsUnknownAndNonFiniteOverrides()
        {
            Assert.Throws<ArgumentException>(() => new StimEffectValueResolver(
                new Dictionary<string, float> { { "unknown.reward", 1f } }));
            Assert.Throws<ArgumentException>(() => new StimEffectValueResolver(
                new Dictionary<string, float>
                    { { StimEffectValueRules.StagedStatGain, float.PositiveInfinity } }));
            Assert.Throws<InvalidOperationException>(() => new StimEffectValueResolver().Resolve(
                new Effect { valueRuleId = "unknown.reward" }));
        }

        [Test]
        public void BalanceProfile_RejectsSignFlipsFractionsAndOutOfBandValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new StimEffectValueResolver(
                new Dictionary<string, float> { { StimEffectValueRules.StagedCashSmallCost, 1f } }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StimEffectValueResolver(
                new Dictionary<string, float> { { StimEffectValueRules.StagedSkillXpGain, -1f } }));
            Assert.Throws<ArgumentException>(() => new StimEffectValueResolver(
                new Dictionary<string, float> { { StimEffectValueRules.StagedStatGain, 1.5f } }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StimEffectValueResolver(
                new Dictionary<string, float> { { StimEffectValueRules.StagedStatGain, 11f } }));
        }

        [Test]
        public void VersionedBalanceProfile_HasUniqueBoundedDefaults()
        {
            var definitions = StimEffectValueResolver.GetDefinitions();

            Assert.That(StimEffectValueResolver.BalanceProfileId, Is.EqualTo("staged_rewards_v1"));
            Assert.That(definitions, Has.Count.EqualTo(7));
            Assert.That(definitions.Select(item => item.id).Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(definitions.Count));
            Assert.That(definitions.All(item => item.minimumValue <= item.defaultValue &&
                                                item.defaultValue <= item.maximumValue), Is.True);
            Assert.That(definitions.All(item => item.requiresWholeUnits), Is.True);
        }

        [Test]
        public void EveryStagedConsequence_UsesARegisteredDynamicValueRule()
        {
            var resolver = new StimEffectValueResolver();
            var effects = StagedStimEventCatalog.CreateAllStagedEvents()
                .SelectMany(evt => evt.choices)
                .SelectMany(choice => choice.outcomes)
                .SelectMany(outcome => outcome.effects)
                .ToList();

            Assert.That(effects, Has.Count.EqualTo(200));
            Assert.That(effects.All(effect => resolver.ContainsRule(effect.valueRuleId)), Is.True);
            Assert.That(effects.All(effect => Math.Abs(resolver.Resolve(effect)) > float.Epsilon), Is.True);
        }
    }
}
