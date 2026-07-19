using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class EventSheetBinder
    {
        private readonly Label category;
        private readonly Label title;
        private readonly Label body;
        private readonly Label resultText;
        private readonly Label resultEffects;
        private readonly VisualElement choices;
        private readonly VisualElement resultCard;

        public EventSheetBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            category = root.Q<Label>("event-category");
            title = root.Q<Label>("event-title");
            body = root.Q<Label>("event-body");
            resultText = root.Q<Label>("result-text");
            resultEffects = root.Q<Label>("result-effects");
            choices = root.Q<VisualElement>("choices");
            resultCard = root.Q<VisualElement>("result-card");
            Continue = root.Q<Button>("event-continue");
        }

        public Button Continue { get; }
        public bool IsValid => category != null && title != null && body != null && resultText != null &&
            resultEffects != null && choices != null && resultCard != null && Continue != null;

        public string CategoryText { set => category.text = value; }
        public string TitleText { set => title.text = value; }
        public string BodyText { set => body.text = value; }
        public string ResultText { set => resultText.text = value; }
        public string EffectsText { set => resultEffects.text = value; }
        public string ContinueText { set => Continue.text = value; }

        public bool EffectsVisible
        {
            set => resultEffects.EnableInClassList("hidden", !value);
        }

        public bool ChoicesVisible
        {
            set => choices.EnableInClassList("hidden", !value);
        }

        public bool ResultVisible
        {
            set => resultCard.EnableInClassList("hidden", !value);
        }

        public bool ContinueVisible
        {
            set => Continue.EnableInClassList("hidden", !value);
        }

        public void ClearChoices() => choices.Clear();
        public void AddChoice(VisualElement choice) => choices.Add(choice);
    }
}
