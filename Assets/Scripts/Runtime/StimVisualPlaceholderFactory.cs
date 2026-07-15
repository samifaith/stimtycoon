using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public enum StimVisualRole { Hero, Thumbnail, Avatar, Icon, Background, Object, Badge }

    [Serializable]
    public sealed class StimVisualPlaceholderDefinition
    {
        public string visualId;
        public StimVisualRole role;
        public string aspectRatio = "1:1";
        public string accessibilityLabelKey;
        public bool decorative;
        public string fallbackGlyph = "◆";
        public string themeToken = "cyan";
        public string sourceStatus = "placeholder";
    }

    public static class StimVisualPlaceholderFactory
    {
        public static VisualElement Create(StimVisualPlaceholderDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(definition.visualId))
                throw new ArgumentException("Visual placeholders require a stable visualId.", nameof(definition));
            if (!definition.decorative && string.IsNullOrWhiteSpace(definition.accessibilityLabelKey))
                throw new ArgumentException("Non-decorative visuals require an accessibility label key.", nameof(definition));

            var root = new VisualElement
            {
                name = "visual-" + Sanitize(definition.visualId),
                tooltip = definition.decorative ? string.Empty : definition.accessibilityLabelKey,
                userData = definition
            };
            root.AddToClassList("st-visual-placeholder");
            root.AddToClassList("role-" + definition.role.ToString().ToLowerInvariant());
            root.AddToClassList("theme-" + Sanitize(definition.themeToken));

            var glyph = new Label(string.IsNullOrEmpty(definition.fallbackGlyph)
                ? GetDefaultEmoji(definition.role)
                : definition.fallbackGlyph);
            glyph.AddToClassList("st-visual-placeholder-glyph");
            root.Add(glyph);
            return root;
        }

        private static string GetDefaultEmoji(StimVisualRole role)
        {
            switch (role)
            {
                case StimVisualRole.Hero: return "✨";
                case StimVisualRole.Avatar: return "👤";
                case StimVisualRole.Thumbnail: return "🖼️";
                case StimVisualRole.Object: return "📦";
                case StimVisualRole.Badge: return "🏅";
                case StimVisualRole.Background: return "🌤️";
                default: return "🔹";
            }
        }

        private static string Sanitize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "default" :
                value.Trim().ToLowerInvariant().Replace('.', '-').Replace('_', '-').Replace(' ', '-');
        }
    }
}
