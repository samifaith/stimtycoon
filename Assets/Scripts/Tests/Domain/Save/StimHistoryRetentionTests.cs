using System.Collections.Generic;
using NUnit.Framework;
using StimTycoon.Saves;

namespace StimTycoon.Tests.Domain.Save
{
    public sealed class StimHistoryRetentionTests
    {
        [Test]
        public void Apply_BoundsLifeFeedAndPreservesRecentMajorEntries()
        {
            var state = new StimGameState();
            for (var index = 0; index < StimHistoryRetention.MaxLifeFeedEntries + 40; index++)
                state.lifeFeed.Add(new StimLifeFeedEntry
                {
                    entryId = "feed_" + index,
                    category = index < 10 ? "milestone" : "activity",
                    text = "Entry " + index,
                    age = index / 12,
                    revision = index + 1
                });

            StimHistoryRetention.Apply(state);

            Assert.That(state.lifeFeed, Has.Count.EqualTo(StimHistoryRetention.MaxLifeFeedEntries));
            Assert.That(state.historyArchive.lifeFeedArchivedCount, Is.EqualTo(40));
            Assert.That(state.lifeFeed.Exists(entry => entry.entryId == "feed_0"), Is.True,
                "Routine entries should archive before major milestones.");
        }

        [Test]
        public void Apply_BoundsEventHistoryAndRetainsArchiveSummaries()
        {
            var state = new StimGameState { eventHistory = new List<StimEventHistoryEntry>() };
            for (var index = 0; index < StimHistoryRetention.MaxEventHistoryEntries + 50; index++)
                state.eventHistory.Add(new StimEventHistoryEntry
                {
                    eventId = "event_" + index,
                    choiceId = "choice",
                    outcomeId = "result",
                    age = index / 12,
                    revision = index + 1
                });

            StimHistoryRetention.Apply(state);

            Assert.That(state.eventHistory, Has.Count.EqualTo(StimHistoryRetention.MaxEventHistoryEntries));
            Assert.That(state.eventHistory[0].eventId, Is.EqualTo("event_50"));
            Assert.That(state.historyArchive.eventHistoryArchivedCount, Is.EqualTo(50));
            Assert.That(state.historyArchive.majorSummaries,
                Has.Count.EqualTo(StimHistoryRetention.MaxMajorArchiveSummaries));
            Assert.That(state.historyArchive.majorSummaries[0], Does.Contain("event_18"));
            Assert.That(state.historyArchive.majorSummaries[^1], Does.Contain("event_49"));
        }

        [Test]
        public void Apply_IsIdempotentAtTheRetentionBoundary()
        {
            var state = new StimGameState();
            for (var index = 0; index < StimHistoryRetention.MaxLifeFeedEntries; index++)
                state.lifeFeed.Add(new StimLifeFeedEntry { entryId = "feed_" + index });

            StimHistoryRetention.Apply(state);
            StimHistoryRetention.Apply(state);

            Assert.That(state.lifeFeed, Has.Count.EqualTo(StimHistoryRetention.MaxLifeFeedEntries));
            Assert.That(state.historyArchive.lifeFeedArchivedCount, Is.Zero);
        }
    }
}
