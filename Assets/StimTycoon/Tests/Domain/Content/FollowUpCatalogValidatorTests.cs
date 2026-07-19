using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Content
{
    [Category("ContentContract")]
    public sealed class FollowUpCatalogValidatorTests
    {
        [Test]
        public void CompleteAuthoredCatalog_HasReachableValidFollowUps()
        {
            var result = FollowUpCatalogValidator.Validate(CompleteCatalog());

            Assert.That(result.isValid, Is.True, string.Join(Environment.NewLine, result.errors));
            Assert.That(result.followUpCount, Is.GreaterThan(0));
        }

        [Test]
        public void MissingFollowUpTarget_IsRejected()
        {
            var events = CompleteCatalog();
            FirstFollowUp(events).eventId = "missing_follow_up_target";

            var result = FollowUpCatalogValidator.Validate(events);

            Assert.That(result.isValid, Is.False);
            Assert.That(result.errors.Any(error => error.Contains("references missing follow-up")), Is.True);
        }

        [Test]
        public void UnreachableTargetAgeWindow_IsRejected()
        {
            var events = CompleteCatalog();
            var followUp = FirstFollowUp(events);
            var target = events.Single(evt => evt.id == followUp.eventId);
            target.ageRange = new AgeRange { minAge = 0, maxAge = 0 };

            var result = FollowUpCatalogValidator.Validate(events);

            Assert.That(result.isValid, Is.False);
            Assert.That(result.errors.Any(error => error.Contains("cannot reach")), Is.True);
        }

        [Test]
        public void InvalidSchedulingMetadata_IsRejected()
        {
            var events = CompleteCatalog();
            var followUp = FirstFollowUp(events);
            followUp.probability = 0f;
            followUp.minYearsFromNow = 2;
            followUp.maxYearsFromNow = 1;
            followUp.cancellationRule = "Not valid!";

            var result = FollowUpCatalogValidator.Validate(events);

            Assert.That(result.isValid, Is.False);
            Assert.That(result.errors.Any(error => error.Contains("invalid follow-up probability")), Is.True);
            Assert.That(result.errors.Any(error => error.Contains("invalid follow-up delay window")), Is.True);
            Assert.That(result.errors.Any(error => error.Contains("invalid cancellation rule")), Is.True);
        }

        private static List<Event> CompleteCatalog()
        {
            return RepresentativeEvents.CreateLaunchAlphaCatalog()
                .Concat(StagedEventCatalog.CreateAllStagedEvents())
                .ToList();
        }

        private static ScheduledEventRef FirstFollowUp(IEnumerable<Event> events)
        {
            return events.SelectMany(evt => evt.choices ?? new List<Choice>())
                .SelectMany(choice => choice.outcomes ?? new List<Outcome>())
                .SelectMany(outcome => outcome.followUps ?? new List<ScheduledEventRef>())
                .First();
        }
    }
}
