using System;
using System.Collections.Generic;
using System.Linq;

namespace StimTycoon.Events
{
    public static class EffectValueRules
    {
        public const string StagedStatGain = "staged.stat.small_gain";
        public const string StagedStatLoss = "staged.stat.small_loss";
        public const string StagedSkillXpGain = "staged.skill_xp.small_gain";
        public const string StagedCareerProgressGain = "staged.career_progress.small_gain";
        public const string StagedCashSmallGain = "staged.cash.small_gain";
        public const string StagedCashMediumGain = "staged.cash.medium_gain";
        public const string StagedCashSmallCost = "staged.cash.small_cost";
    }

    public interface IEffectValueResolver
    {
        float Resolve(Effect effect);
        bool ContainsRule(string ruleId);
    }

    public sealed class EffectValueRuleDefinition
    {
        public string id;
        public float defaultValue;
        public float minimumValue;
        public float maximumValue;
        public bool requiresWholeUnits;
    }

    /// <summary>
    /// Resolves balance keys independently from event prose and choice structure. Overrides allow
    /// a composition/config layer to rebalance rewards without rewriting authored events.
    /// </summary>
    public sealed class EffectValueResolver : IEffectValueResolver
    {
        public const string BalanceProfileId = "staged_rewards_v1";

        private static readonly IReadOnlyDictionary<string, EffectValueRuleDefinition> Definitions =
            new Dictionary<string, EffectValueRuleDefinition>(StringComparer.Ordinal)
            {
                { EffectValueRules.StagedStatGain, Rule(EffectValueRules.StagedStatGain, 1f, 0f, 10f) },
                { EffectValueRules.StagedStatLoss, Rule(EffectValueRules.StagedStatLoss, -1f, -10f, 0f) },
                { EffectValueRules.StagedSkillXpGain, Rule(EffectValueRules.StagedSkillXpGain, 1f, 0f, 100f) },
                { EffectValueRules.StagedCareerProgressGain, Rule(EffectValueRules.StagedCareerProgressGain, 1f, 0f, 100f) },
                { EffectValueRules.StagedCashSmallGain, Rule(EffectValueRules.StagedCashSmallGain, 500f, 0f, 100000f) },
                { EffectValueRules.StagedCashMediumGain, Rule(EffectValueRules.StagedCashMediumGain, 1000f, 0f, 250000f) },
                { EffectValueRules.StagedCashSmallCost, Rule(EffectValueRules.StagedCashSmallCost, -500f, -100000f, 0f) }
            };

        private readonly Dictionary<string, float> values;

        public EffectValueResolver(IReadOnlyDictionary<string, float> overrides = null)
        {
            values = Definitions.ToDictionary(item => item.Key, item => item.Value.defaultValue,
                StringComparer.Ordinal);
            if (overrides == null) return;
            foreach (var item in overrides)
            {
                if (!Definitions.TryGetValue(item.Key, out var definition))
                    throw new ArgumentException($"Unknown effect value rule {item.Key}.", nameof(overrides));
                if (float.IsNaN(item.Value) || float.IsInfinity(item.Value))
                    throw new ArgumentException($"Effect value rule {item.Key} must be finite.", nameof(overrides));
                if (item.Value < definition.minimumValue || item.Value > definition.maximumValue)
                    throw new ArgumentOutOfRangeException(nameof(overrides),
                        $"Effect value rule {item.Key} must be within " +
                        $"[{definition.minimumValue}, {definition.maximumValue}].");
                if (definition.requiresWholeUnits &&
                    Math.Abs(item.Value - (float)Math.Round(item.Value)) > float.Epsilon)
                    throw new ArgumentException(
                        $"Effect value rule {item.Key} requires whole units.", nameof(overrides));
                values[item.Key] = item.Value;
            }
        }

        public float Resolve(Effect effect)
        {
            if (effect == null) return 0f;
            if (string.IsNullOrWhiteSpace(effect.valueRuleId)) return effect.value;
            if (!values.TryGetValue(effect.valueRuleId, out var resolved))
                throw new InvalidOperationException($"Unknown effect value rule {effect.valueRuleId}.");
            return resolved;
        }

        public bool ContainsRule(string ruleId) =>
            !string.IsNullOrWhiteSpace(ruleId) && values.ContainsKey(ruleId);

        public static IReadOnlyList<EffectValueRuleDefinition> GetDefinitions() =>
            Definitions.Values.OrderBy(item => item.id, StringComparer.Ordinal).ToList();

        private static EffectValueRuleDefinition Rule(
            string id, float defaultValue, float minimumValue, float maximumValue) =>
            new EffectValueRuleDefinition
            {
                id = id,
                defaultValue = defaultValue,
                minimumValue = minimumValue,
                maximumValue = maximumValue,
                requiresWholeUnits = true
            };
    }
}
