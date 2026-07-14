using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public static class StimActionInputFactory
    {
        public static VisualElement CreateAmountSelector(
            long availableMinorUnits,
            Action<long> onSelected)
        {
            var root = new VisualElement { name = "action-amount-selector" };
            root.AddToClassList("st-amount-selector");
            var percentages = new VisualElement();
            percentages.AddToClassList("st-percentage-options");
            foreach (var percentage in StimActionAmountService.SupportedPercentages)
            {
                var captured = percentage;
                var button = new Button(() =>
                {
                    if (StimActionAmountService.TrySelectPercentage(
                            availableMinorUnits, captured, out var amount, out _)) onSelected?.Invoke(amount);
                }) { name = $"amount-{percentage}-percent", text = $"{percentage}%" };
                button.AddToClassList("st-percentage-button");
                percentages.Add(button);
            }
            root.Add(percentages);

            var exact = new TextField("Exact amount") { name = "amount-exact-input" };
            exact.AddToClassList("st-exact-amount");
            root.Add(exact);
            var feedback = new Label { name = "amount-validation-feedback" };
            feedback.AddToClassList("st-amount-feedback");
            root.Add(feedback);
            var apply = new Button(() =>
            {
                if (StimActionAmountService.TryParseExact(
                        exact.value, availableMinorUnits, out var amount, out var error))
                {
                    feedback.text = string.Empty;
                    onSelected?.Invoke(amount);
                }
                else feedback.text = error;
            }) { name = "amount-apply", text = "Use amount" };
            apply.AddToClassList("st-amount-apply");
            root.Add(apply);
            return root;
        }

        public static VisualElement CreatePaymentSelector(
            StimActionDefinition definition,
            Action<StimActionPaymentOption> onSelected)
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
