using System;
using System.Linq;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class PlayableEventCatalogTests
    {
        [Test]
        public void DefaultBuild_RegistersLaunchCatalogAndNoStagedContent()
        {
            var build = PlayableEventCatalog.Build();
            var launchIds = RepresentativeEvents.CreateLaunchAlphaCatalog()
                .Select(evt => evt.id).OrderBy(id => id, StringComparer.Ordinal);

            Assert.That(build.events.Select(evt => evt.id).OrderBy(id => id, StringComparer.Ordinal),
                Is.EqualTo(launchIds));
            Assert.That(build.launchCount, Is.EqualTo(build.events.Count));
            Assert.That(build.stagedCount, Is.Zero);
            Assert.That(build.stagedCounts, Is.Empty);
        }

        [Test]
        public void BoundedRollout_RegistersOnlySelectedCategoriesAtExactCaps()
        {
            var rollout = CreateRollout(5, 742981, EventCategory.Childhood, EventCategory.Health);

            var build = PlayableEventCatalog.Build(rollout);

            Assert.That(build.stagedCount, Is.EqualTo(10));
            Assert.That(build.stagedCounts[EventCategory.Childhood], Is.EqualTo(5));
            Assert.That(build.stagedCounts[EventCategory.Health], Is.EqualTo(5));
            Assert.That(build.stagedCounts.Keys,
                Is.EquivalentTo(new[] { EventCategory.Childhood, EventCategory.Health }));
            Assert.That(build.events, Has.Count.EqualTo(build.launchCount + 10));
        }

        [Test]
        public void BoundedRollout_IsDeterministicAndSeeded()
        {
            var first = PlayableEventCatalog.Build(CreateRollout(5, 100, EventCategory.School));
            var repeated = PlayableEventCatalog.Build(CreateRollout(5, 100, EventCategory.School));
            var alternate = PlayableEventCatalog.Build(CreateRollout(5, 101, EventCategory.School));
            var stagedStart = first.launchCount;

            Assert.That(repeated.events.Skip(stagedStart).Select(evt => evt.id),
                Is.EqualTo(first.events.Skip(stagedStart).Select(evt => evt.id)));
            Assert.That(alternate.events.Skip(stagedStart).Select(evt => evt.id),
                Is.Not.EqualTo(first.events.Skip(stagedStart).Select(evt => evt.id)));
        }

        [Test]
        public void Rollout_RejectsUnboundedDuplicateAndUnsupportedConfiguration()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PlayableEventCatalog.Build(
                CreateRollout(21, 1, EventCategory.Childhood)));
            Assert.Throws<ArgumentException>(() => PlayableEventCatalog.Build(
                CreateRollout(1, 1, EventCategory.Childhood, EventCategory.Childhood)));
            Assert.Throws<ArgumentException>(() => PlayableEventCatalog.Build(
                CreateRollout(1, 1, EventCategory.World)));
        }

        [Test]
        public void EveryRolloutBuild_RemainsProductionAndFollowUpValid()
        {
            var rollout = CreateRollout(20, 55, EventCategory.Childhood, EventCategory.School,
                EventCategory.Career, EventCategory.Health, EventCategory.Money);
            var build = PlayableEventCatalog.Build(rollout);

            foreach (var evt in build.events)
            {
                var validation = EventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    EventValidator.GetValidationSummary(validation, evt.id));
            }
            var followUps = FollowUpCatalogValidator.Validate(build.events);
            Assert.That(followUps.isValid, Is.True, string.Join(Environment.NewLine, followUps.errors));
        }

        private static StagedContentRollout CreateRollout(
            int count, int seed, params EventCategory[] categories)
        {
            var rollout = new StagedContentRollout
            {
                eventsPerCategory = count,
                selectionSeed = seed
            };
            rollout.enabledCategories.AddRange(categories);
            return rollout;
        }
    }
}
