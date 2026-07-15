using System;
using System.Collections.Generic;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    public static class StimLifeFeedPresentation
    {
        public static List<StimLifeFeedEntry> GetNewestFirst(IReadOnlyList<StimLifeFeedEntry> entries)
        {
            var ordered = new List<StimLifeFeedEntry>();
            if (entries == null) return ordered;

            for (var index = 0; index < entries.Count; index++)
                if (entries[index] != null) ordered.Add(entries[index]);

            ordered.Sort(CompareNewestFirst);
            return ordered;
        }

        private static int CompareNewestFirst(StimLifeFeedEntry left, StimLifeFeedEntry right)
        {
            var comparison = right.age.CompareTo(left.age);
            if (comparison != 0) return comparison;
            comparison = right.monthOfYear.CompareTo(left.monthOfYear);
            if (comparison != 0) return comparison;
            comparison = right.revision.CompareTo(left.revision);
            if (comparison != 0) return comparison;
            comparison = GetCategoryPriority(right.category).CompareTo(GetCategoryPriority(left.category));
            if (comparison != 0) return comparison;
            comparison = ParseTimestamp(right.timestampUtc).CompareTo(ParseTimestamp(left.timestampUtc));
            if (comparison != 0) return comparison;
            return string.Compare(right.entryId, left.entryId, StringComparison.Ordinal);
        }

        private static int GetCategoryPriority(string category)
        {
            switch (category?.ToLowerInvariant())
            {
                case "transition": case "milestone": return 7;
                case "event": return 6;
                case "goal": case "achievement": return 5;
                case "relationship": case "family": return 4;
                case "career": case "business": case "money": return 3;
                case "education": case "home": case "activity": return 2;
                case "year": case "time": return 1;
                default: return 0;
            }
        }

        private static DateTime ParseTimestamp(string timestampUtc)
        {
            return DateTime.TryParse(timestampUtc, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed.ToUniversalTime()
                : DateTime.MinValue;
        }
    }
}
