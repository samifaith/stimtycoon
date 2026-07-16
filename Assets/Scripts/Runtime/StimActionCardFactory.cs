using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public static class StimActionCardFactory
    {
        public static VisualElement Create(StimActionDefinition definition, Action onCommit)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var card = new VisualElement { name = $"action-card-{definition.id.Replace('.', '-')}" };
            card.AddToClassList("st-action-card");
            card.AddToClassList("st-brand-skyden-panel");
            card.EnableInClassList("locked", definition.state == StimActionState.Locked);

            var title = new Label(definition.title);
            title.AddToClassList("st-action-card-title");
            card.Add(title);

            if (definition.previews != null && definition.previews.Count > 0)
            {
                var preview = new Label(string.Join(" · ", definition.previews.ConvertAll(
                    delta => $"{delta.targetId} {(delta.amount >= 0 ? "+" : "−")}{Math.Abs(delta.amount)}")));
                preview.AddToClassList("st-action-card-preview");
                card.Add(preview);
            }

            if (!string.IsNullOrEmpty(definition.lockedReason))
            {
                var requirement = new Label(definition.lockedReason);
                requirement.AddToClassList("st-action-requirement-chip");
                card.Add(requirement);
            }

            if (definition.progressRequired > 1 || definition.state == StimActionState.InProgress)
            {
                var progress = new Label(
                    $"Progress {definition.progress} / {Math.Max(1, definition.progressRequired)}" +
                    (definition.durationSeconds > 0 ? $" · {definition.durationSeconds}s" : string.Empty));
                progress.AddToClassList("st-action-card-progress");
                card.Add(progress);
            }

            var suffix = definition.id.StartsWith("education.", StringComparison.Ordinal)
                ? definition.id.Substring("education.".Length).Replace('.', '-')
                : definition.id.Replace('.', '-');
            var button = new Button(onCommit)
            {
                name = $"education-action-{suffix}",
                text = GetButtonText(definition),
                tooltip = string.IsNullOrEmpty(definition.lockedReason)
                    ? definition.description
                    : definition.lockedReason
            };
            button.AddToClassList("st-action-commit");
            button.AddToClassList(definition.state == StimActionState.Claimable
                ? "st-brand-jelly-claim"
                : "st-brand-skyden-primary");
            button.SetEnabled(definition.state == StimActionState.Ready ||
                              definition.state == StimActionState.Claimable);
            card.Add(button);
            return card;
        }

        private static string GetButtonText(StimActionDefinition definition)
        {
            switch (definition.state)
            {
                case StimActionState.Locked: return $"{definition.title}\n{definition.lockedReason}";
                case StimActionState.InProgress: return "In progress";
                case StimActionState.Claimable: return "Claim";
                case StimActionState.Complete: return "Complete";
                default: return definition.title;
            }
        }
    }
}
