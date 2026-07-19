using System;
using System.Collections.Generic;
using System.Linq;
using StimTycoon.Events;

namespace StimTycoon.Runtime
{
    public sealed class StagedContentRollout
    {
        public int eventsPerCategory;
        public int selectionSeed;
        public readonly List<EventCategory> enabledCategories = new List<EventCategory>();
    }

    public sealed class PlayableEventCatalogBuild
    {
        public readonly List<Event> events = new List<Event>();
        public readonly Dictionary<EventCategory, int> stagedCounts =
            new Dictionary<EventCategory, int>();
        public int launchCount;
        public int stagedCount;
    }

    /// <summary>
    /// One registration boundary for the playable catalog. Staged content remains off unless an
    /// explicit, bounded rollout is supplied after review approval.
    /// </summary>
    public static class PlayableEventCatalog
    {
        public const int MaximumEventsPerStagedCategory = 20;

        public static PlayableEventCatalogBuild Build(StagedContentRollout rollout = null)
        {
            var build = new PlayableEventCatalogBuild();
            build.events.AddRange(RepresentativeEvents.CreateLaunchAlphaCatalog());
            build.launchCount = build.events.Count;
            if (rollout == null || rollout.eventsPerCategory == 0 ||
                rollout.enabledCategories.Count == 0) return build;
            if (rollout.eventsPerCategory < 0 ||
                rollout.eventsPerCategory > MaximumEventsPerStagedCategory)
                throw new ArgumentOutOfRangeException(nameof(rollout.eventsPerCategory),
                    $"Staged rollout size must be between 0 and {MaximumEventsPerStagedCategory}.");

            var supported = StagedEventCatalog.CreateAllStagedEvents()
                .GroupBy(evt => evt.category)
                .ToDictionary(group => group.Key, group => group.ToList());
            var enabled = new HashSet<EventCategory>();
            foreach (var category in rollout.enabledCategories)
            {
                if (!enabled.Add(category))
                    throw new ArgumentException($"Staged rollout category {category} is duplicated.");
                if (!supported.TryGetValue(category, out var candidates))
                    throw new ArgumentException($"Staged rollout category {category} is not authored.");
                var selected = candidates
                    .OrderBy(evt => StableRank(evt.id, rollout.selectionSeed))
                    .ThenBy(evt => evt.id, StringComparer.Ordinal)
                    .Take(rollout.eventsPerCategory)
                    .ToList();
                build.events.AddRange(selected);
                build.stagedCounts[category] = selected.Count;
                build.stagedCount += selected.Count;
            }

            if (build.events.Select(evt => evt.id).Distinct(StringComparer.Ordinal).Count() != build.events.Count)
                throw new InvalidOperationException("Playable event registration produced duplicate event IDs.");
            return build;
        }

        private static uint StableRank(string eventId, int seed)
        {
            unchecked
            {
                var hash = 2166136261u ^ (uint)seed;
                foreach (var character in eventId)
                {
                    hash ^= character;
                    hash *= 16777619u;
                }
                return hash;
            }
        }
    }
}
