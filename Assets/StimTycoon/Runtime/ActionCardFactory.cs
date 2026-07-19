using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public static class ActionCardFactory
    {
        public static VisualElement Create(
            ActionDefinition definition,
            Action onCommit,
            VisualTreeAsset template = null)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var card = CloneTemplateRoot(template, "action-card") ?? CreateFallback();
            card.name = $"action-card-{definition.id.Replace('.', '-')}";
            card.AddToClassList("st-action-card");
            card.AddToClassList("st-brand-skyden-panel");
            PresentationStateStyler.Apply(card,
                PresentationStateStyler.FromActionState(definition.state));

            var title = card.Q<Label>("action-card-title");
            var preview = card.Q<Label>("action-card-preview");
            var progress = card.Q<Label>("action-card-progress");
            var requirement = card.Q<Label>("action-card-requirement");
            var button = card.Q<Button>("action-card-commit");
            title.text = definition.title;
            var hasPreview = definition.previews != null && definition.previews.Count > 0;
            preview.text = hasPreview
                ? string.Join(" · ", definition.previews.ConvertAll(
                    delta => $"{delta.targetId} {(delta.amount >= 0 ? "+" : "−")}{Math.Abs(delta.amount)}"))
                : string.Empty;
            preview.EnableInClassList("hidden", !hasPreview);
            requirement.text = definition.lockedReason ?? string.Empty;
            requirement.EnableInClassList("hidden", string.IsNullOrEmpty(definition.lockedReason));
            var hasProgress = definition.progressRequired > 1 || definition.state == ActionState.InProgress;
            progress.text = hasProgress
                ? $"Progress {definition.progress} / {Math.Max(1, definition.progressRequired)}" +
                  (definition.durationSeconds > 0 ? $" · {definition.durationSeconds}s" : string.Empty)
                : string.Empty;
            progress.EnableInClassList("hidden", !hasProgress);

            var suffix = definition.id.StartsWith("education.", StringComparison.Ordinal)
                ? definition.id.Substring("education.".Length).Replace('.', '-')
                : definition.id.Replace('.', '-');
            button.name = $"education-action-{suffix}";
            button.text = GetButtonText(definition);
            button.tooltip = string.IsNullOrEmpty(definition.lockedReason)
                ? definition.description
                : definition.lockedReason;
            if (onCommit != null) button.clicked += onCommit;
            button.AddToClassList("st-action-commit");
            button.RemoveFromClassList("st-brand-jelly-claim");
            button.RemoveFromClassList("st-brand-skyden-primary");
            button.AddToClassList(definition.state == ActionState.Claimable
                ? "st-brand-jelly-claim"
                : "st-brand-skyden-primary");
            button.SetEnabled(definition.state == ActionState.Ready ||
                              definition.state == ActionState.Claimable);
            return card;
        }

        private static VisualElement CreateFallback()
        {
            var card = new VisualElement();
            card.Add(NamedLabel("action-card-title", "st-action-card-title"));
            card.Add(NamedLabel("action-card-preview", "st-action-card-preview"));
            card.Add(NamedLabel("action-card-progress", "st-action-card-progress"));
            card.Add(NamedLabel("action-card-requirement", "st-action-requirement-chip"));
            var button = new Button { name = "action-card-commit" };
            button.AddToClassList("st-action-commit");
            card.Add(button);
            return card;
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

        private static string GetButtonText(ActionDefinition definition)
        {
            switch (definition.state)
            {
                case ActionState.Locked: return $"{definition.title}\n{definition.lockedReason}";
                case ActionState.InProgress: return "In progress";
                case ActionState.Paused: return "Paused";
                case ActionState.Claimable: return "Claim";
                case ActionState.Complete: return "Complete";
                case ActionState.Expired: return "Expired";
                default: return definition.title;
            }
        }
    }
}
