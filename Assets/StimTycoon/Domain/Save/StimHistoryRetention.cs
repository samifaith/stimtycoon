using System;
using System.Collections.Generic;

namespace StimTycoon.Saves
{
    public static class StimHistoryRetention
    {
        public const int MaxLifeFeedEntries = 256;
        public const int MaxEventHistoryEntries = 128;
        public const int MaxMajorArchiveSummaries = 32;

        public static void Apply(StimGameState state)
        {
            if (state == null) return;
            state.lifeFeed ??= new List<StimLifeFeedEntry>();
            state.eventHistory ??= new List<StimEventHistoryEntry>();
            state.historyArchive ??= new StimHistoryArchiveState();
            state.historyArchive.majorSummaries ??= new List<string>();

            while (state.lifeFeed.Count > MaxLifeFeedEntries)
            {
                var removalIndex = FindOldestRoutineEntry(state.lifeFeed);
                var removed = state.lifeFeed[removalIndex];
                if (IsMajor(removed?.category) && !string.IsNullOrWhiteSpace(removed.text))
                    AddMajorSummary(state.historyArchive, removed.text);
                state.lifeFeed.RemoveAt(removalIndex);
                state.historyArchive.lifeFeedArchivedCount++;
            }

            while (state.eventHistory.Count > MaxEventHistoryEntries)
            {
                var removed = state.eventHistory[0];
                if (removed != null && !string.IsNullOrWhiteSpace(removed.eventId))
                    AddMajorSummary(state.historyArchive,
                        $"Age {removed.age}: {removed.eventId} → {removed.choiceId}");
                state.eventHistory.RemoveAt(0);
                state.historyArchive.eventHistoryArchivedCount++;
            }
        }

        private static int FindOldestRoutineEntry(IReadOnlyList<StimLifeFeedEntry> entries)
        {
            for (var index = 0; index < entries.Count; index++)
                if (!IsMajor(entries[index]?.category)) return index;
            return 0;
        }

        private static bool IsMajor(string category) =>
            category == "event" || category == "milestone" || category == "achievement" ||
            category == "education" || category == "career" || category == "relationship";

        private static void AddMajorSummary(StimHistoryArchiveState archive, string summary)
        {
            archive.majorSummaries.Add(summary);
            while (archive.majorSummaries.Count > MaxMajorArchiveSummaries)
                archive.majorSummaries.RemoveAt(0);
        }
    }
}
