using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Content
{
    [Category("ContentContract")]
    public sealed class StimContentAgeBoundaryAuditTests
    {
        [Test]
        public void CompleteAuthoredCatalog_HasUniqueValidatedAgeRanges()
        {
            var events = CompleteCatalog().ToList();
            var duplicateIds = events.GroupBy(evt => evt.id, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            Assert.That(duplicateIds, Is.Empty,
                "The complete authored catalog must have one owner per event ID.");
            foreach (var evt in events)
            {
                var validation = StimEventValidator.ValidateProductionEvent(evt);
                Assert.That(validation.isValid, Is.True,
                    StimEventValidator.GetValidationSummary(validation, evt.id));
                Assert.That(evt.ageRange, Is.Not.Null, $"{evt.id} requires an age range.");
            }
        }

        [TestCaseSource(nameof(BoundaryCases))]
        public void EveryAuthoredEvent_EnforcesExactAndAdjacentAges(
            string eventId, int age, bool expected)
        {
            var evt = CompleteCatalog().Single(candidate => candidate.id == eventId);
            Assert.That(StimEventAgeEligibility.IsEligible(evt, age), Is.EqualTo(expected),
                $"{eventId} age range {evt.ageRange.minAge}–{evt.ageRange.maxAge} at age {age}.");
        }

        private static IEnumerable<TestCaseData> BoundaryCases()
        {
            foreach (var evt in CompleteCatalog().OrderBy(candidate => candidate.id, StringComparer.Ordinal))
            {
                var range = evt.ageRange;
                yield return Case(evt.id, range.minAge, true, "min");
                yield return Case(evt.id, range.maxAge, true, "max");
                yield return Case(evt.id, range.minAge - 1, false, "below_min");
                yield return Case(evt.id, range.maxAge + 1, false, "above_max");
            }
        }

        private static TestCaseData Case(string eventId, int age, bool expected, string boundary)
        {
            return new TestCaseData(eventId, age, expected)
                .SetName($"{eventId}_{boundary}_{age}");
        }

        private static IEnumerable<StimEvent> CompleteCatalog()
        {
            return RepresentativeStimEvents.CreateLaunchAlphaCatalog()
                .Concat(StagedStimEventCatalog.CreateAllStagedEvents());
        }
    }
}
