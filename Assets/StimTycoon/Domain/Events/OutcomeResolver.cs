using System;
using System.Collections.Generic;

namespace StimTycoon.Events
{
    public sealed class ChoiceResolution
    {
        public string eventId;
        public string choiceId;
        public Outcome outcome;
        public float finalSuccessChance;
        public float successRoll;
        public float outcomeRoll;
    }

    /// <summary>
    /// Resolves choices with a stable, save-backed random stream.
    /// </summary>
    public sealed class OutcomeResolver
    {
        public bool TryResolve(
            Event evt,
            string choiceId,
            int seed,
            int rngStep,
            out ChoiceResolution resolution,
            out string failure)
        {
            resolution = null;
            failure = string.Empty;

            if (evt?.choices == null)
            {
                failure = "Event has no choices.";
                return false;
            }

            var choice = evt.choices.Find(candidate =>
                candidate != null && string.Equals(candidate.id, choiceId, StringComparison.Ordinal));
            if (choice == null)
            {
                failure = $"Choice {choiceId} was not found on event {evt.id}.";
                return false;
            }

            if (choice.outcomes == null || choice.outcomes.Count == 0)
            {
                failure = $"Choice {choiceId} has no outcomes.";
                return false;
            }

            var finalChance = RiskRewardCalculator.CalculateFinalSuccessChance(choice.baseSuccessChance);
            var successRoll = StableRoll(seed, rngStep, 0);
            var preferredClass = successRoll < finalChance
                ? OutcomeClassification.Positive
                : OutcomeClassification.Negative;
            var candidates = choice.outcomes.FindAll(outcome =>
                outcome != null && outcome.classification == preferredClass);

            if (candidates.Count == 0)
            {
                candidates = choice.outcomes.FindAll(outcome =>
                    outcome != null && outcome.classification == OutcomeClassification.Neutral);
            }

            if (candidates.Count == 0)
            {
                candidates = choice.outcomes.FindAll(outcome => outcome != null);
            }

            var outcomeRoll = StableRoll(seed, rngStep, 1);
            var outcome = SelectWeighted(candidates, outcomeRoll);
            if (outcome == null)
            {
                failure = $"Choice {choiceId} has no selectable outcomes.";
                return false;
            }

            resolution = new ChoiceResolution
            {
                eventId = evt.id,
                choiceId = choiceId,
                outcome = outcome,
                finalSuccessChance = finalChance,
                successRoll = successRoll,
                outcomeRoll = outcomeRoll
            };
            return true;
        }

        private static Outcome SelectWeighted(IReadOnlyList<Outcome> outcomes, float roll)
        {
            var totalWeight = 0f;
            foreach (var outcome in outcomes)
            {
                totalWeight += Math.Max(0f, outcome.weightWithinResultGroup);
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            var target = roll * totalWeight;
            var cumulative = 0f;
            foreach (var outcome in outcomes)
            {
                cumulative += Math.Max(0f, outcome.weightWithinResultGroup);
                if (target < cumulative)
                {
                    return outcome;
                }
            }

            return outcomes[outcomes.Count - 1];
        }

        private static float StableRoll(int seed, int step, int stream)
        {
            unchecked
            {
                var value = (uint)seed;
                value ^= 0x9E3779B9u * (uint)(step + 1);
                value ^= 0x85EBCA6Bu * (uint)(stream + 1);
                value ^= value >> 16;
                value *= 0x7FEB352Du;
                value ^= value >> 15;
                value *= 0x846CA68Bu;
                value ^= value >> 16;
                return (value & 0x00FFFFFFu) / 16777216f;
            }
        }
    }
}
