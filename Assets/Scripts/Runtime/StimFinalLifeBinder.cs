using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimFinalLifeBinder
    {
        private readonly Label name;
        private readonly Label status;
        private readonly Label summary;

        public StimFinalLifeBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            name = root.Q<Label>("ending-name");
            status = root.Q<Label>("ending-status");
            summary = root.Q<Label>("ending-summary");
            StartNewLife = root.Q<Button>("ending-new-life");
        }

        public Button StartNewLife { get; }
        public bool IsValid => name != null && status != null && summary != null && StartNewLife != null;

        public void Render(string displayName, string statusText, string summaryText)
        {
            name.text = displayName;
            status.text = statusText;
            summary.text = summaryText;
        }
    }
}
