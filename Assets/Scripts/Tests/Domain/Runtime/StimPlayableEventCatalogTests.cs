using System;
using System.Linq;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class StimPlayableEventCatalogTests
    {
        [Test]
        public void DefaultBuild_RegistersLaunchCatalogAndNoStagedContent()
        {
            var build = StimPlayableEventCatalog.Build();
            var launchIds = RepresentativeStimEvents.CreateLaunchAlphaCatalog()
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

            var build = StimPlayableEventCatalog.Build(rollout);

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
            var first = StimPlayableEventCatalog.Build(CreateRollout(5, 100, EventCategory.School));
            var repeated = StimPlayableEventCatalog.Build(CreateRollout(5, 100, EventCategory.School));
            var alternate = StimPlayableEventCatalog.Build(CreateRollout(5, 101, EventCategory.School));
            var stagedStart = first.launchCount;

            Assert.That(repeated.events.Skip(stagedStart).Select(evt => evt.id),
                Is.EqualTo(first.events.Skip(stagedStart).Select(evt => evt.id)));
            Assert.That(alternate.events.Skip(stagedStart).Select(evt => evt.id),
                Is.Not.EqualTo(first.events.Skip(stagedStart).Select(evt => evt.id)));
        }

        [Test]
        public void Rollout_RejectsUnboundedDuplicateAndUnsupportedConfiguration()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => StimPlayableEventCatalog.Build(
                CreateRollout(21, 1, EventCategory.Childhood)));
            Assert.Throws<ArgumentException>(() => StimPlayableEventCatalog.Build(
                CreateRollout(1, 1, EventCategory.Childhood, EventCategory.Childhood)));
            Assert.Throws<ArgumentException>(() => StimPlayableEventCatalog.Build(
                CreateRollout(1, 1, EventCategory.World)));
        }

        [Test]
        public void EveryRolloutBuild_RemainsProductionAndFollowUpValid()
        {
            var rollout = CreateRollout(20, 55, EventCategory.Childhood, EventCategory.School,
                EventCategory.Career, EventCategory.Health, EventCategory.Money);
            var build = StimPlayableEventCatalog.Build(rollout);

            foreach (var evt in build.events)
            {
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
            }
            var followUps = StimFollowUpCatalogValidator.Validate(build.events);
            Assert.That(followUps.isValid, Is.True, string.Join(Environment.NewLine, followUps.errors));
        }

        private static StimStagedContentRollout CreateRollout(
            int count, int seed, params EventCategory[] categories)
        {
            var rollout = new StimStagedContentRollout
            {
                eventsPerCategory = count,
                selectionSeed = seed
            };
            rollout.enabledCategories.AddRange(categories);
            return rollout;
        }
    }
}
