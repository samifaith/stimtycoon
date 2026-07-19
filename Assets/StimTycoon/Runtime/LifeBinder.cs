using System;
using System.Collections.Generic;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class LifeBinder
    {
        private const int MaximumVisibleEntries = 8;
        private readonly VisualElement lifeFeedList;
        private readonly VisualTreeAsset feedRowTemplate;

        public LifeBinder(VisualElement root, VisualTreeAsset feedRowTemplate)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            this.feedRowTemplate = feedRowTemplate ?? throw new ArgumentNullException(nameof(feedRowTemplate));
            lifeFeedList = root.Q<VisualElement>("life-feed-list");
        }

        public bool IsValid => lifeFeedList != null;

        public void RenderFeed(IReadOnlyList<LifeFeedEntry> sourceEntries)
        {
            if (lifeFeedList == null) return;
            lifeFeedList.Clear();
            var entries = LifeFeedPresentation.GetNewestFirst(sourceEntries);
            if (entries.Count == 0)
            {
                var empty = new Label("Your life is ready for its next chapter.");
                empty.AddToClassList("st-feed-empty");
                lifeFeedList.Add(empty);
                return;
            }

            VisualElement currentGroup = null;
            var currentAge = -1;
            var currentMonth = -1;
            var visibleEntries = Math.Min(MaximumVisibleEntries, entries.Count);
            for (var index = 0; index < visibleEntries; index++)
            {
                var entry = entries[index];
                if (entry == null) continue;
                if (currentGroup == null || entry.age != currentAge || entry.monthOfYear != currentMonth)
                {
                    currentAge = entry.age;
                    currentMonth = entry.monthOfYear;
                    currentGroup = new VisualElement { name = $"feed-month-{currentAge}-{currentMonth}" };
                    currentGroup.AddToClassList("st-feed-month-group");
                    var header = new Label($"AGE {currentAge}  ·  {GetMonthName(currentMonth).ToUpperInvariant()}");
                    header.AddToClassList("st-feed-month-header");
                    currentGroup.Add(header);
                    lifeFeedList.Add(currentGroup);
                }
                currentGroup.Add(UiComponentFactory.CreateFeedRow(
                    entry, index, entries.Count, feedRowTemplate));
            }

            if (entries.Count > visibleEntries)
            {
                var remaining = new Label($"Showing the latest {visibleEntries} of {entries.Count} life updates.");
                remaining.AddToClassList("st-feed-more");
                lifeFeedList.Add(remaining);
            }
        }

        private static string GetMonthName(int month) =>
            month >= 1 && month <= 12
                ? new DateTime(2000, month, 1).ToString("MMMM")
                : $"Month {month}";
    }
}
