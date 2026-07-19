using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public enum VisualRole { Hero, Thumbnail, Avatar, Icon, Background, Object, Badge }

    [Serializable]
    public sealed class VisualPlaceholderDefinition
    {
        public string visualId;
        public VisualRole role;
        public string aspectRatio = "1:1";
        public string accessibilityLabelKey;
        public bool decorative;
        public string fallbackGlyph = "◆";
        public string themeToken = "cyan";
        public string sourceStatus = "placeholder";
    }

    public static class VisualPlaceholderFactory
    {
        public static VisualElement Create(VisualPlaceholderDefinition definition)
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

        private static string GetDefaultEmoji(VisualRole role)
        {
            switch (role)
            {
                case VisualRole.Hero: return "✨";
                case VisualRole.Avatar: return "👤";
                case VisualRole.Thumbnail: return "🖼️";
                case VisualRole.Object: return "📦";
                case VisualRole.Badge: return "🏅";
                case VisualRole.Background: return "🌤️";
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
