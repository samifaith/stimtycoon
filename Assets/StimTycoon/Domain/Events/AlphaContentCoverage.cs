using System;
using System.Collections.Generic;
using System.Linq;

namespace StimTycoon.Events
{
    public sealed class AlphaContentCoverageResult
    {
        public bool isValid = true;
        public readonly List<string> errors = new List<string>();
        public readonly Dictionary<string, int> stageCounts = new Dictionary<string, int>();
        public readonly Dictionary<string, int> destinationCounts = new Dictionary<string, int>();
        public int chainStartCount;
        public int terminalOutcomeCount;
    }

    /// <summary>Executable launch-alpha event breadth and anti-repetition contract.</summary>
    public static class AlphaContentCoverage
    {
        private static readonly (string id, int age, int minimum)[] StageMinimums =
        {
            ("early_childhood", 3, 2),
            ("childhood", 9, 4),
            ("teen", 15, 6),
            ("adult", 30, 8),
            ("senior", 70, 6)
        };

        private static readonly Dictionary<EventCategory, int> DestinationMinimums =
            new Dictionary<EventCategory, int>
            {
                { EventCategory.Childhood, 2 }, { EventCategory.School, 1 },
                { EventCategory.Career, 1 }, { EventCategory.Health, 1 },
                { EventCategory.Money, 3 }, { EventCategory.Relationship, 6 },
                { EventCategory.World, 3 }
            };

        public static AlphaContentCoverageResult Validate(IReadOnlyList<Event> events)
        {
            var result = new AlphaContentCoverageResult();
            if (events == null) return Fail(result, "Authored event catalog is null.");
            var authored = events.Where(item => item != null).ToList();
            var ids = new HashSet<string>(authored.Select(item => item.id), StringComparer.Ordinal);
            if (ids.Count != authored.Count) AddError(result, "Authored event IDs must be unique.");

            foreach (var evt in authored)
            {
                var validation = EventValidator.ValidateProductionEvent(evt);
                if (!validation.isValid) AddError(result, $"{evt.id} fails schema/editorial validation.");
                if (evt.analyticsTags == null || evt.analyticsTags.Count == 0)
                    AddError(result, $"{evt.id} requires diagnostic tags.");
                if (evt.repeatPolicy == RepeatPolicy.Repeatable && evt.cooldownYears == 0 &&
                    !(evt.analyticsTags?.Contains("annual_review") ?? false))
                    AddError(result, $"{evt.id} requires a cooldown for anti-repetition coverage.");

                foreach (var choice in evt.choices ?? new List<Choice>())
                foreach (var outcome in choice?.outcomes ?? new List<Outcome>())
                {
                    if (outcome.followUps == null || outcome.followUps.Count == 0)
                    {
                        result.terminalOutcomeCount++;
                        continue;
                    }
                    result.chainStartCount++;
                    foreach (var followUp in outcome.followUps)
                        if (followUp == null || !ids.Contains(followUp.eventId))
                            AddError(result, $"{evt.id} references a missing follow-up event.");
                }
                // A chain-start event may intentionally route every branch into a follow-up.
                // Catalog-level terminal coverage below ensures those chains still have endings.
            }

            foreach (var minimum in StageMinimums)
            {
                var count = authored.Count(evt => evt.ageRange != null &&
                    evt.ageRange.minAge <= minimum.age && evt.ageRange.maxAge >= minimum.age);
                result.stageCounts[minimum.id] = count;
                if (count < minimum.minimum)
                    AddError(result, $"{minimum.id} requires {minimum.minimum} events; found {count}.");
            }
            foreach (var minimum in DestinationMinimums)
            {
                var count = authored.Count(evt => evt.category == minimum.Key);
                result.destinationCounts[minimum.Key.ToString()] = count;
                if (count < minimum.Value)
                    AddError(result, $"{minimum.Key} requires {minimum.Value} events; found {count}.");
            }
            if (!authored.Any(evt => evt.analyticsTags?.Contains("home") == true))
                AddError(result, "Home requires at least one authored event.");
            if (result.chainStartCount < 5) AddError(result, "The alpha catalog requires at least five chain starts.");
            if (result.terminalOutcomeCount < authored.Count)
                AddError(result, "The alpha catalog requires at least one terminal outcome per authored event on average.");
            return result;
        }

        private static AlphaContentCoverageResult Fail(AlphaContentCoverageResult result, string error)
        {
            AddError(result, error);
            return result;
        }

        private static void AddError(AlphaContentCoverageResult result, string error)
        {
            result.isValid = false;
            result.errors.Add(error);
        }
    }
}
