using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Resolves balance keys independently from event prose and choice structure. Overrides allow
    /// a composition/config layer to rebalance rewards without rewriting authored events.
    /// </summary>
    public sealed class EffectValueResolver : IEffectValueResolver
    {
        private static readonly IReadOnlyDictionary<string, float> Defaults =
            new Dictionary<string, float>(StringComparer.Ordinal)
            {
                { EffectValueRules.StagedStatGain, 1f },
                { EffectValueRules.StagedStatLoss, -1f },
                { EffectValueRules.StagedSkillXpGain, 1f },
                { EffectValueRules.StagedCareerProgressGain, 1f },
                { EffectValueRules.StagedCashSmallGain, 500f },
                { EffectValueRules.StagedCashMediumGain, 1000f },
                { EffectValueRules.StagedCashSmallCost, -500f }
            };

        private readonly Dictionary<string, float> values;

        public EffectValueResolver(IReadOnlyDictionary<string, float> overrides = null)
        {
            values = new Dictionary<string, float>(Defaults, StringComparer.Ordinal);
            if (overrides == null) return;
            foreach (var item in overrides)
            {
                if (!values.ContainsKey(item.Key))
                    throw new ArgumentException($"Unknown effect value rule {item.Key}.", nameof(overrides));
                if (float.IsNaN(item.Value) || float.IsInfinity(item.Value))
                    throw new ArgumentException($"Effect value rule {item.Key} must be finite.", nameof(overrides));
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
    }
}
