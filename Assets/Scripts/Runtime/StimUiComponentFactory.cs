using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public static class StimUiComponentFactory
    {
        public static VisualElement CreateFeedRow(StimLifeFeedEntry entry, int index, int total)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var category = string.IsNullOrWhiteSpace(entry.category) ? "life" : entry.category.ToLowerInvariant();
            var row = new VisualElement
            {
                name = $"feed-item-{index + 1}",
                tooltip = $"Item {index + 1} of {total}. {ToDisplayName(category)}. " +
                          $"Age {entry.age}, month {entry.monthOfYear}. {entry.text}"
            };
            row.AddToClassList("st-feed-entry");
            row.AddToClassList("category-" + category);

            var timeline = new VisualElement();
            timeline.AddToClassList("st-feed-timeline");
            var dot = new VisualElement();
            dot.AddToClassList("st-feed-dot");
            timeline.Add(dot);
            row.Add(timeline);

            var icon = new Label(GetCategoryGlyph(category));
            icon.AddToClassList("st-feed-icon");
            row.Add(icon);

            var copy = new VisualElement();
            copy.AddToClassList("st-feed-copy");
            var title = new Label(ToCompactTitle(entry.text));
            title.AddToClassList("st-feed-title");
            copy.Add(title);
            row.Add(copy);

            var chip = new Label(ToDisplayName(category));
            chip.AddToClassList("st-feed-result-chip");
            chip.AddToClassList("result-" + category);
            row.Add(chip);
            return row;
        }

        public static VisualElement CreateInfoBanner(string title, string body)
        {
            var banner = new VisualElement();
            banner.AddToClassList("st-info-callout");
            var icon = new Label("ℹ️");
            icon.AddToClassList("st-info-icon");
            banner.Add(icon);
            var copy = new VisualElement();
            copy.AddToClassList("st-info-banner-copy");
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("st-info-banner-title");
            var bodyLabel = new Label(body);
            bodyLabel.AddToClassList("st-info-copy");
            copy.Add(titleLabel);
            copy.Add(bodyLabel);
            banner.Add(copy);
            return banner;
        }

        public static VisualElement CreateAchievementRow(
            string stableId,
            string badge,
            string title,
            string category,
            string progress,
            string reward,
            string actionText,
            bool actionEnabled,
            Action onAction)
        {
            var row = new VisualElement { name = "achievement-row-" + Sanitize(stableId) };
            row.AddToClassList("st-achievement-row");

            var icon = new Label(string.IsNullOrEmpty(badge) ? "🏆" : badge);
            icon.AddToClassList("st-achievement-icon");
            row.Add(icon);

            var copy = new VisualElement();
            copy.AddToClassList("st-achievement-copy");
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("st-achievement-name");
            copy.Add(titleLabel);
            var meta = new VisualElement();
            meta.AddToClassList("st-achievement-meta");
            var categoryLabel = new Label(category);
            categoryLabel.AddToClassList("st-achievement-category");
            var progressLabel = new Label(progress);
            progressLabel.AddToClassList("st-achievement-progress");
            meta.Add(categoryLabel);
            meta.Add(progressLabel);
            copy.Add(meta);
            row.Add(copy);

            var rewardLabel = new Label(reward);
            rewardLabel.AddToClassList("st-achievement-reward");
            row.Add(rewardLabel);

            var action = new Button(onAction)
            {
                name = "achievement-action-" + Sanitize(stableId),
                text = actionText
            };
            action.AddToClassList(actionText == "CLAIM" ? "stim-pack-reward-button" : "stim-pack-secondary-button");
            action.AddToClassList("stim-pack-interaction-pop");
            action.SetEnabled(actionEnabled);
            row.Add(action);
            return row;
        }

        private static string ToCompactTitle(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Life update";
            var normalized = text.Replace('\n', ' ').Trim();
            var sentenceEnd = normalized.IndexOf(". ", StringComparison.Ordinal);
            if (sentenceEnd > 0) normalized = normalized.Substring(0, sentenceEnd + 1);
            return normalized.Length <= 64 ? normalized : normalized.Substring(0, 61).TrimEnd() + "…";
        }

        private static string GetCategoryGlyph(string category)
        {
            switch (category)
            {
                case "education": return "📚";
                case "career": case "business": return "💼";
                case "money": return "💵";
                case "relationship": case "family": return "❤️";
                case "goal": case "achievement": return "⭐";
                case "activity": return "✅";
                case "event": return "✨";
                default: return "📝";
            }
        }

        private static string ToDisplayName(string value)
        {
            return string.IsNullOrEmpty(value)
                ? "Life"
                : char.ToUpperInvariant(value[0]) + value.Substring(1).Replace('_', ' ');
        }

        private static string Sanitize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "item"
                : value.Trim().ToLowerInvariant().Replace('.', '-').Replace('_', '-').Replace(' ', '-');
        }
    }
}
