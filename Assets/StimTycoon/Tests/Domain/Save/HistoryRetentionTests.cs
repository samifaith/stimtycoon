using System.Collections.Generic;
using NUnit.Framework;
using StimTycoon.Saves;

namespace StimTycoon.Tests.Domain.Save
{
    public sealed class HistoryRetentionTests
    {
        [Test]
        public void Apply_BoundsLifeFeedAndPreservesRecentMajorEntries()
        {
            var state = new GameState();
            for (var index = 0; index < HistoryRetention.MaxLifeFeedEntries + 40; index++)
                state.lifeFeed.Add(new LifeFeedEntry
                {
                    entryId = "feed_" + index,
                    category = index < 10 ? "milestone" : "activity",
                    text = "Entry " + index,
                    age = index / 12,
                    revision = index + 1
                });

            HistoryRetention.Apply(state);

            Assert.That(state.lifeFeed, Has.Count.EqualTo(HistoryRetention.MaxLifeFeedEntries));
            Assert.That(state.historyArchive.lifeFeedArchivedCount, Is.EqualTo(40));
            Assert.That(state.lifeFeed.Exists(entry => entry.entryId == "feed_0"), Is.True,
                "Routine entries should archive before major milestones.");
        }

        [Test]
        public void Apply_BoundsEventHistoryAndRetainsArchiveSummaries()
        {
            var state = new GameState { eventHistory = new List<EventHistoryEntry>() };
            for (var index = 0; index < HistoryRetention.MaxEventHistoryEntries + 50; index++)
                state.eventHistory.Add(new EventHistoryEntry
                {
                    eventId = "event_" + index,
                    choiceId = "choice",
                    outcomeId = "result",
                    age = index / 12,
                    revision = index + 1
                });

            HistoryRetention.Apply(state);

            Assert.That(state.eventHistory, Has.Count.EqualTo(HistoryRetention.MaxEventHistoryEntries));
            Assert.That(state.eventHistory[0].eventId, Is.EqualTo("event_50"));
            Assert.That(state.historyArchive.eventHistoryArchivedCount, Is.EqualTo(50));
            Assert.That(state.historyArchive.majorSummaries,
                Has.Count.EqualTo(HistoryRetention.MaxMajorArchiveSummaries));
            Assert.That(state.historyArchive.majorSummaries[0], Does.Contain("event_18"));
            Assert.That(state.historyArchive.majorSummaries[^1], Does.Contain("event_49"));
        }

        [Test]
        public void Apply_IsIdempotentAtTheRetentionBoundary()
        {
            var state = new GameState();
            for (var index = 0; index < HistoryRetention.MaxLifeFeedEntries; index++)
                state.lifeFeed.Add(new LifeFeedEntry { entryId = "feed_" + index });

            HistoryRetention.Apply(state);
            HistoryRetention.Apply(state);

            Assert.That(state.lifeFeed, Has.Count.EqualTo(HistoryRetention.MaxLifeFeedEntries));
            Assert.That(state.historyArchive.lifeFeedArchivedCount, Is.Zero);
        }
    }
}
