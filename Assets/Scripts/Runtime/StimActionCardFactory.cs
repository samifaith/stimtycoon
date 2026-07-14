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

            var suffix = definition.id.StartsWith("education.", StringComparison.Ordinal)
                ? definition.id.Substring("education.".Length)
                : definition.id.Replace('.', '-');
            var button = new Button(onCommit)
            {
                name = $"education-action-{suffix}",
                text = definition.state == StimActionState.Locked
                    ? $"{definition.title}\n{definition.lockedReason}"
                    : definition.title,
                tooltip = string.IsNullOrEmpty(definition.lockedReason)
                    ? definition.description
                    : definition.lockedReason
            };
            button.AddToClassList("st-action-commit");
            button.SetEnabled(definition.state == StimActionState.Ready);
            card.Add(button);
            return card;
        }
    }
}
