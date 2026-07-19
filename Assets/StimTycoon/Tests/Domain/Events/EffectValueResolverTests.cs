using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Events
{
    public sealed class EffectValueResolverTests
    {
        [Test]
        public void AuthoredFallback_RemainsCompatibleWithoutBalanceKey()
        {
            var effect = new Effect { type = EffectType.CashDelta, targetId = "cash", value = 125f };

            Assert.That(new EffectValueResolver().Resolve(effect), Is.EqualTo(125f));
        }

        [Test]
        public void BalanceOverride_ChangesValueWithoutChangingAuthoredEffect()
        {
            var effect = new Effect
            {
                type = EffectType.SkillXp,
                targetId = "learning",
                value = 1f,
                valueRuleId = EffectValueRules.StagedSkillXpGain
            };
            var resolver = new EffectValueResolver(new Dictionary<string, float>
            {
                { EffectValueRules.StagedSkillXpGain, 7f }
            });

            Assert.That(resolver.Resolve(effect), Is.EqualTo(7f));
            Assert.That(effect.value, Is.EqualTo(1f), "The authored fallback remains migration-safe.");
        }

        [Test]
        public void Resolver_RejectsUnknownAndNonFiniteOverrides()
        {
            Assert.Throws<ArgumentException>(() => new EffectValueResolver(
                new Dictionary<string, float> { { "unknown.reward", 1f } }));
            Assert.Throws<ArgumentException>(() => new EffectValueResolver(
                new Dictionary<string, float>
                    { { EffectValueRules.StagedStatGain, float.PositiveInfinity } }));
            Assert.Throws<InvalidOperationException>(() => new EffectValueResolver().Resolve(
                new Effect { valueRuleId = "unknown.reward" }));
        }

        [Test]
        public void EveryStagedConsequence_UsesARegisteredDynamicValueRule()
        {
            var resolver = new EffectValueResolver();
            var effects = StagedEventCatalog.CreateAllStagedEvents()
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
