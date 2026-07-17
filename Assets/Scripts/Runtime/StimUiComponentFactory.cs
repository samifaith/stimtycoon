using System;
using StimTycoon.Saves;
using UnityEngine;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public static class StimUiComponentFactory
    {
        public static VisualElement CreateFeedRow(
            StimLifeFeedEntry entry,
            int index,
            int total,
            VisualTreeAsset template = null)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var category = string.IsNullOrWhiteSpace(entry.category) ? "life" : entry.category.ToLowerInvariant();
            var row = CloneTemplateRoot(template, "feed-row") ?? CreateFeedRowFallback();
            row.name = $"feed-item-{index + 1}";
            row.tooltip = $"Item {index + 1} of {total}. {ToDisplayName(category)}. " +
                          $"Age {entry.age}, month {entry.monthOfYear}. {entry.text}";
            row.AddToClassList("st-feed-entry");
            row.AddToClassList("st-brand-skyden-list");
            row.AddToClassList("category-" + category);

            var icon = row.Q<Label>("feed-row-icon");
            var title = row.Q<Label>("feed-row-title");
            var timestamp = row.Q<Label>("feed-row-timestamp");
            var chip = row.Q<Label>("feed-row-result");
            icon.text = GetCategoryGlyph(category);
            title.text = ToCompactTitle(entry.text);
            timestamp.text = ToCompactTimestamp(entry);
            chip.text = ToResultChip(entry.text, category);
            chip.AddToClassList("result-" + category);
            return row;
        }

        public static VisualElement CreateInfoBanner(string title, string body)
        {
            var banner = new VisualElement();
            banner.AddToClassList("st-info-callout");
            var icon = new Label("ℹ️");
            icon.AddToClassList("st-info-icon");
            icon.AddToClassList("st-brand-space-icon");
            icon.AddToClassList("icon-info");
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
            Action onAction,
            string accessibleProgress = null,
            VisualTreeAsset template = null)
        {
            var row = CloneTemplateRoot(template, "achievement-row") ?? CreateAchievementRowFallback();
            row.name = "achievement-row-" + Sanitize(stableId);
            row.tooltip = $"{title}. {category}. Progress {accessibleProgress ?? progress}. Reward {reward}.";
            row.AddToClassList("st-achievement-row");
            row.AddToClassList("st-brand-jelly-reward-row");

            var icon = row.Q<Label>("achievement-row-icon");
            var titleLabel = row.Q<Label>("achievement-row-title");
            var categoryLabel = row.Q<Label>("achievement-row-category");
            var progressLabel = row.Q<Label>("achievement-row-progress");
            var rewardLabel = row.Q<Label>("achievement-row-reward");
            var action = row.Q<Button>("achievement-row-action");
            icon.text = string.IsNullOrEmpty(badge) ? "🏆" : badge;
            titleLabel.text = title;
            categoryLabel.text = category;
            progressLabel.text = progress;
            rewardLabel.text = reward;
            if (actionText == "CLAIM" || string.Equals(category, "Achievement", StringComparison.OrdinalIgnoreCase))
                icon.AddToClassList("stim-pack-reward-icon");
            action.name = "achievement-action-" + Sanitize(stableId);
            action.text = actionText;
            if (onAction != null) action.clicked += onAction;
            action.RemoveFromClassList("stim-pack-reward-button");
            action.RemoveFromClassList("stim-pack-secondary-button");
            action.RemoveFromClassList("st-brand-jelly-claim");
            action.RemoveFromClassList("st-brand-skyden-secondary");
            action.AddToClassList(actionText == "CLAIM" ? "stim-pack-reward-button" : "stim-pack-secondary-button");
            action.AddToClassList(actionText == "CLAIM" ? "st-brand-jelly-claim" : "st-brand-skyden-secondary");
            action.AddToClassList("stim-pack-interaction-pop");
            action.SetEnabled(actionEnabled);
            return row;
        }

        private static VisualElement CreateFeedRowFallback()
        {
            var row = new VisualElement();
            var timeline = new VisualElement();
            timeline.AddToClassList("st-feed-timeline");
            var dot = new VisualElement();
            dot.AddToClassList("st-feed-dot");
            timeline.Add(dot);
            row.Add(timeline);
            row.Add(NamedLabel("feed-row-icon", "st-feed-icon"));
            var copy = new VisualElement();
            copy.AddToClassList("st-feed-copy");
            copy.Add(NamedLabel("feed-row-title", "st-feed-title"));
            copy.Add(NamedLabel("feed-row-timestamp", "st-feed-timestamp"));
            row.Add(copy);
            row.Add(NamedLabel("feed-row-result", "st-feed-result-chip"));
            return row;
        }

        private static VisualElement CreateAchievementRowFallback()
        {
            var row = new VisualElement();
            row.Add(NamedLabel("achievement-row-icon", "st-achievement-icon"));
            var copy = new VisualElement();
            copy.AddToClassList("st-achievement-copy");
            copy.Add(NamedLabel("achievement-row-title", "st-achievement-name"));
            var meta = new VisualElement();
            meta.AddToClassList("st-achievement-meta");
            meta.Add(NamedLabel("achievement-row-category", "st-achievement-category"));
            meta.Add(NamedLabel("achievement-row-progress", "st-achievement-progress"));
            copy.Add(meta);
            row.Add(copy);
            row.Add(NamedLabel("achievement-row-reward", "st-achievement-reward"));
            var action = new Button { name = "achievement-row-action" };
            action.AddToClassList("st-achievement-action");
            row.Add(action);
            return row;
        }

        private static Label NamedLabel(string name, string className)
        {
            var label = new Label { name = name };
            label.AddToClassList(className);
            return label;
        }

        private static VisualElement CloneTemplateRoot(VisualTreeAsset template, string rootName)
        {
            if (template == null) return null;
            var container = template.CloneTree();
            var root = container.Q<VisualElement>(rootName);
            if (root == null)
            {
                Debug.LogError($"UI template '{template.name}' is missing required root '{rootName}'.");
                return null;
            }
            root.RemoveFromHierarchy();
            return root;
        }

        public static Button CreateRelationshipRow(
            string stableId,
            string displayName,
            string relationshipType,
            int strength,
            Action onOpen)
        {
            var safeName = string.IsNullOrWhiteSpace(displayName) ? ToDisplayName(stableId) : displayName.Trim();
            var safeType = ToDisplayName(relationshipType);
            var safeStrength = Math.Max(0, Math.Min(100, strength));
            var row = new Button(onOpen)
            {
                name = "relationship-" + Sanitize(stableId),
                tooltip = $"{safeName}. {safeType}. Relationship strength {safeStrength} out of 100."
            };
            row.AddToClassList("st-relationship-card");

            var avatar = new Label(safeName.Length == 0 ? "?" : safeName.Substring(0, 1).ToUpperInvariant());
            avatar.AddToClassList("st-relationship-card-avatar");
            row.Add(avatar);

            var copy = new VisualElement();
            copy.AddToClassList("st-relationship-card-copy");
            var name = new Label(safeName);
            name.AddToClassList("st-relationship-card-name");
            var meta = new Label($"{safeType} · Relationship {safeStrength} / 100");
            meta.AddToClassList("st-relationship-card-meta");
            var track = new VisualElement();
            track.AddToClassList("st-relationship-card-track");
            var fill = new VisualElement();
            fill.AddToClassList("st-relationship-card-fill");
            fill.style.width = Length.Percent(safeStrength);
            track.Add(fill);
            copy.Add(name);
            copy.Add(meta);
            copy.Add(track);
            row.Add(copy);

            var arrow = new Label("›");
            arrow.AddToClassList("st-relationship-card-arrow");
            row.Add(arrow);
            return row;
        }

        public static VisualElement CreateAccountRow(
            string stableId,
            string glyph,
            string title,
            string balance,
            string detail,
            Action onOpen = null)
        {
            var row = onOpen == null ? new VisualElement() : new Button(onOpen);
            row.name = "account-row-" + Sanitize(stableId);
            row.tooltip = $"{title}. {balance}. {detail}";
            row.AddToClassList("st-account-row");
            row.EnableInClassList("interactive", onOpen != null);
            row.Add(CreateRowIcon(glyph, "st-account-row-icon"));
            row.Add(CreateRowCopy(title, detail, "st-account-row-title", "st-account-row-detail"));
            var value = new Label(balance);
            value.AddToClassList("st-account-row-value");
            row.Add(value);
            return row;
        }

        public static VisualElement CreatePathRow(
            string stableId,
            string glyph,
            string title,
            string detail,
            string trailingText,
            bool available,
            Action onOpen = null)
        {
            var row = onOpen == null ? new VisualElement() : new Button(onOpen);
            row.name = "path-row-" + Sanitize(stableId);
            row.tooltip = $"{title}. {detail}. {trailingText}";
            row.AddToClassList("st-path-row");
            row.EnableInClassList("locked", !available);
            row.Add(CreateRowIcon(glyph, "st-path-icon"));
            row.Add(CreateRowCopy(title, detail, "st-path-title", "st-path-body"));
            var trailing = new Label(trailingText);
            trailing.AddToClassList(available ? "st-path-reward" : "st-path-lock");
            row.Add(trailing);
            return row;
        }

        public static VisualElement CreateStatRow(
            string stableId,
            string glyph,
            string title,
            int value,
            int maximum = 100)
        {
            var safeMaximum = Math.Max(1, maximum);
            var safeValue = Math.Max(0, Math.Min(safeMaximum, value));
            var row = new VisualElement
            {
                name = "stat-row-" + Sanitize(stableId),
                tooltip = $"{title}: {safeValue} out of {safeMaximum}."
            };
            row.AddToClassList("st-stat-row");
            row.AddToClassList("st-component-stat-row");
            row.Add(CreateRowIcon(glyph, "st-stat-icon"));
            var main = new VisualElement();
            main.AddToClassList("st-stat-main");
            var name = new Label(title);
            name.AddToClassList("st-stat-name");
            var track = new VisualElement();
            track.AddToClassList("st-stat-track");
            var fill = new VisualElement();
            fill.AddToClassList("st-stat-fill");
            fill.style.width = Length.Percent(safeValue * 100f / safeMaximum);
            track.Add(fill);
            main.Add(name);
            main.Add(track);
            row.Add(main);
            var number = new Label($"{safeValue} / {safeMaximum}");
            number.AddToClassList("st-stat-number");
            row.Add(number);
            return row;
        }

        public static VisualElement CreateSectionRow(
            string stableId,
            string title,
            string metadata,
            string trailingText)
        {
            var row = new VisualElement
            {
                name = "section-row-" + Sanitize(stableId),
                tooltip = $"{title}. {metadata}. {trailingText}"
            };
            row.AddToClassList("st-section-row");
            row.Add(CreateRowCopy(title, metadata, "st-section-row-title", "st-section-row-meta"));
            var trailing = new Label(trailingText);
            trailing.AddToClassList("st-section-row-trailing");
            row.Add(trailing);
            return row;
        }

        private static Label CreateRowIcon(string glyph, string className)
        {
            var icon = new Label(string.IsNullOrWhiteSpace(glyph) ? "•" : glyph);
            icon.AddToClassList(className);
            return icon;
        }

        private static VisualElement CreateRowCopy(
            string title,
            string detail,
            string titleClass,
            string detailClass)
        {
            var copy = new VisualElement();
            copy.AddToClassList("st-row-copy");
            var titleLabel = new Label(string.IsNullOrWhiteSpace(title) ? "Untitled" : title);
            titleLabel.AddToClassList(titleClass);
            var detailLabel = new Label(detail ?? string.Empty);
            detailLabel.AddToClassList(detailClass);
            copy.Add(titleLabel);
            copy.Add(detailLabel);
            return copy;
        }

        private static string ToCompactTitle(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Life update";
            var normalized = text.Replace('\n', ' ').Trim();
            var sentenceEnd = normalized.IndexOf(". ", StringComparison.Ordinal);
            if (sentenceEnd > 0) normalized = normalized.Substring(0, sentenceEnd + 1);
            return normalized.Length <= 64 ? normalized : normalized.Substring(0, 61).TrimEnd() + "…";
        }

        private static string ToResultChip(string text, string category)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var normalized = text.Replace('\n', ' ').Trim();
                var resultStart = normalized.IndexOf(". ", StringComparison.Ordinal);
                if (resultStart >= 0 && resultStart + 2 < normalized.Length)
                {
                    var result = normalized.Substring(resultStart + 2).Trim().TrimEnd('.');
                    if (result.Length > 18) result = result.Substring(0, 17).TrimEnd() + "…";
                    if (!string.IsNullOrWhiteSpace(result)) return result;
                }
            }

            return ToDisplayName(category);
        }

        private static string ToCompactTimestamp(StimLifeFeedEntry entry)
        {
            if (DateTime.TryParse(entry.timestampUtc, out var timestamp))
                return timestamp.ToLocalTime().ToString("MMM d");
            return $"Month {entry.monthOfYear}";
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
