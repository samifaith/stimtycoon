using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimNewLifeBinder
    {
        private readonly Label error;

        public StimNewLifeBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            error = root.Q<Label>("new-life-error");
            Cancel = root.Q<Button>("cancel-new-life");
            Continue = root.Q<Button>("continue-current-life");
            Create = root.Q<Button>("create-new-life");
        }

        public Button Cancel { get; }
        public Button Continue { get; }
        public Button Create { get; }
        public bool IsValid => error != null && Cancel != null && Continue != null && Create != null;

        public void Configure(bool canContinue, bool canCancel, string currentFirstName)
        {
            StimFeedbackPresenter.Clear(error);
            Continue.EnableInClassList("hidden", !canContinue);
            Cancel.EnableInClassList("hidden", !canCancel);
            if (!canContinue) return;
            Continue.text = string.IsNullOrEmpty(currentFirstName)
                ? "CONTINUE CURRENT LIFE  ›"
                : $"CONTINUE {currentFirstName.ToUpperInvariant()}'S LIFE  ›";
        }

        public void ShowError(string message)
        {
            StimFeedbackPresenter.Show(error, message, StimFeedbackKind.Error, true);
        }
    }
}
