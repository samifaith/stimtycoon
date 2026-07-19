using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public static class ActionInputFactory
    {
        public static VisualElement CreateAmountSelector(
            long availableMinorUnits,
            Action<long> onSelected,
            Action onValidationFailed = null,
            string quickAmountsLabel = "Quick amounts",
            string exactAmountLabel = "Or enter a custom amount",
            string applyButtonLabel = "Continue")
        {
            var root = new VisualElement { name = "action-amount-selector" };
            root.AddToClassList("st-amount-selector");
            long selectedQuickAmount = 0;
            var percentageButtons = new List<Button>();
            var exact = new TextField("Amount ($)") { name = "amount-exact-input" };
            var feedback = new Label { name = "amount-validation-feedback" };

            var quickLabel = new Label(quickAmountsLabel) { name = "amount-quick-label" };
            quickLabel.AddToClassList("st-amount-section-label");
            root.Add(quickLabel);
            var percentages = new VisualElement();
            percentages.AddToClassList("st-percentage-options");
            foreach (var percentage in ActionAmountService.SupportedPercentages)
            {
                var captured = percentage;
                Button button = null;
                button = new Button(() =>
                {
                    if (!ActionAmountService.TrySelectPercentage(
                            availableMinorUnits, captured, out var amount, out var error))
                    {
                        selectedQuickAmount = 0;
                        feedback.text = error;
                        onValidationFailed?.Invoke();
                        return;
                    }

                    selectedQuickAmount = amount;
                    exact.SetValueWithoutNotify(string.Empty);
                    foreach (var percentageButton in percentageButtons)
                        percentageButton.EnableInClassList("active", percentageButton == button);
                    feedback.text = string.Empty;
                }) { name = $"amount-{percentage}-percent", text = $"{percentage}%" };
                button.AddToClassList("st-percentage-button");
                percentageButtons.Add(button);
                percentages.Add(button);
            }
            root.Add(percentages);

            var exactLabel = new Label(exactAmountLabel) { name = "amount-exact-label" };
            exactLabel.AddToClassList("st-amount-section-label");
            exactLabel.AddToClassList("st-custom-amount-label");
            root.Add(exactLabel);
            exact.AddToClassList("st-exact-amount");
            exact.RegisterValueChangedCallback(_ =>
            {
                selectedQuickAmount = 0;
                foreach (var percentageButton in percentageButtons)
                    percentageButton.RemoveFromClassList("active");
                feedback.text = string.Empty;
            });
            root.Add(exact);
            feedback.AddToClassList("st-amount-feedback");
            root.Add(feedback);
            var apply = new Button(() =>
            {
                if (selectedQuickAmount > 0)
                {
                    feedback.text = string.Empty;
                    onSelected?.Invoke(selectedQuickAmount);
                    return;
                }

                if (ActionAmountService.TryParseExact(
                        exact.value, availableMinorUnits, out var exactAmount, out var error))
                {
                    feedback.text = string.Empty;
                    onSelected?.Invoke(exactAmount);
                }
                else
                {
                    feedback.text = error;
                    onValidationFailed?.Invoke();
                }
            }) { name = "amount-apply", text = applyButtonLabel };
            apply.AddToClassList("st-amount-apply");
            root.Add(apply);
            return root;
        }

        public static VisualElement CreatePaymentSelector(
            ActionDefinition definition,
            Action<ActionPaymentOption> onSelected)
        {
            var root = new VisualElement { name = "action-payment-selector" };
            root.AddToClassList("st-payment-selector");
            foreach (var option in definition.paymentOptions)
            {
                var captured = option;
                var button = new Button(() => onSelected?.Invoke(captured))
                {
                    name = $"payment-{option.ToString().ToLowerInvariant()}",
                    text = option.ToString()
                };
                button.AddToClassList("st-payment-option");
                root.Add(button);
            }
            return root;
        }
    }
}
