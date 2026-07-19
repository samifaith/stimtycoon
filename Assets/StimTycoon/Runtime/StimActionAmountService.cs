using System;
using System.Globalization;

namespace StimTycoon.Runtime
{
    public static class StimActionAmountService
    {
        public static readonly int[] SupportedPercentages = { 5, 10, 25, 50, 100 };

        public static bool TrySelectPercentage(
            long availableMinorUnits,
            int percentage,
            out long amountMinorUnits,
            out string error)
        {
            amountMinorUnits = 0;
            if (availableMinorUnits <= 0)
            {
                error = "No available balance.";
                return false;
            }
            if (Array.IndexOf(SupportedPercentages, percentage) < 0)
            {
                error = "Percentage must be 5%, 10%, 25%, 50%, or 100%.";
                return false;
            }
            amountMinorUnits = availableMinorUnits * percentage / 100;
            if (amountMinorUnits <= 0)
            {
                error = "Selected percentage is less than one cent.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        public static bool TryParseExact(
            string text,
            long availableMinorUnits,
            out long amountMinorUnits,
            out string error)
        {
            amountMinorUnits = 0;
            if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
                amount <= 0 || decimal.Round(amount, 2) != amount)
            {
                error = "Enter a positive amount with no more than two decimal places.";
                return false;
            }
            if (amount > long.MaxValue / 100m)
            {
                error = "Amount is too large.";
                return false;
            }
            amountMinorUnits = decimal.ToInt64(amount * 100m);
            if (amountMinorUnits > availableMinorUnits)
            {
                error = "Amount exceeds the available balance.";
                amountMinorUnits = 0;
                return false;
            }
            error = string.Empty;
            return true;
        }

        public static bool TryValidatePayment(
            StimActionDefinition definition,
            StimActionPaymentOption payment,
            long availableCashMinorUnits,
            long availableCreditMinorUnits,
            out string error)
        {
            if (definition == null || !definition.paymentOptions.Contains(payment))
            {
                error = "Selected payment method is not available for this action.";
                return false;
            }
            var available = payment == StimActionPaymentOption.Cash
                ? availableCashMinorUnits
                : payment == StimActionPaymentOption.Credit ? availableCreditMinorUnits : 0;
            if (definition.costMinorUnits > available)
            {
                error = payment == StimActionPaymentOption.Cash
                    ? "Not enough cash."
                    : "Not enough available credit.";
                return false;
            }
            error = string.Empty;
            return true;
        }
    }
}
