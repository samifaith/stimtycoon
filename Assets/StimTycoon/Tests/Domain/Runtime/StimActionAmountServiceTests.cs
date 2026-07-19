using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class StimActionAmountServiceTests
    {
        [TestCase(5, 500)]
        [TestCase(10, 1000)]
        [TestCase(25, 2500)]
        [TestCase(50, 5000)]
        [TestCase(100, 10000)]
        public void PercentageSelection_UsesIntegerMinorUnits(int percentage, long expected)
        {
            Assert.IsTrue(StimActionAmountService.TrySelectPercentage(
                10000, percentage, out var amount, out var error), error);
            Assert.That(amount, Is.EqualTo(expected));
        }

        [TestCase("12.34", 1234)]
        [TestCase("0.01", 1)]
        [TestCase("100", 10000)]
        public void ExactAmount_ParsesInvariantCurrency(string input, long expected)
        {
            Assert.IsTrue(StimActionAmountService.TryParseExact(
                input, 10000, out var amount, out var error), error);
            Assert.That(amount, Is.EqualTo(expected));
        }

        [TestCase("0")]
        [TestCase("-1")]
        [TestCase("1.001")]
        [TestCase("not-money")]
        public void ExactAmount_RejectsInvalidValues(string input)
        {
            Assert.IsFalse(StimActionAmountService.TryParseExact(
                input, 10000, out _, out var error));
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void PaymentValidation_EnforcesAuthoredOptionsAndAvailableFunds()
        {
            var definition = new StimActionDefinition
            {
                id = "money.payment",
                costMinorUnits = 2500,
                paymentOptions = new List<StimActionPaymentOption>
                    { StimActionPaymentOption.Cash, StimActionPaymentOption.Credit }
            };
            Assert.IsTrue(StimActionAmountService.TryValidatePayment(
                definition, StimActionPaymentOption.Cash, 2500, 0, out var cashError), cashError);
            Assert.IsFalse(StimActionAmountService.TryValidatePayment(
                definition, StimActionPaymentOption.Credit, 0, 2000, out var creditError));
            Assert.That(creditError, Does.Contain("credit"));
        }

        [Test]
        public void AmountSelector_ContainsStandardButtonsAndExactInput()
        {
            var root = StimActionInputFactory.CreateAmountSelector(10000, _ => { });
            Assert.That(root.Query<Button>(className: "st-percentage-button").ToList(), Has.Count.EqualTo(5));
            Assert.That(root.Q<Label>("amount-quick-label").text, Is.EqualTo("Quick amounts"));
            Assert.That(root.Q<Label>("amount-exact-label").text, Is.EqualTo("Or enter a custom amount"));
            Assert.That(root.Q<TextField>("amount-exact-input"), Is.Not.Null);
            Assert.That(root.Q<Button>("amount-apply").enabledSelf, Is.True);
            Assert.That(root.Q<Button>("amount-apply").text, Is.EqualTo("Continue"));
        }

        [Test]
        public void PaymentSelector_OnlyRendersAuthoredMethods()
        {
            var definition = new StimActionDefinition
            {
                paymentOptions = new List<StimActionPaymentOption> { StimActionPaymentOption.Cash }
            };
            var root = StimActionInputFactory.CreatePaymentSelector(definition, _ => { });
            Assert.That(root.Q<Button>("payment-cash"), Is.Not.Null);
            Assert.That(root.Q<Button>("payment-credit"), Is.Null);
        }

        [Test]
        public void MoneyFormatting_IsStableWhenRunnerCultureIsNotUsEnglish()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

                Assert.That(StimMoneyFormatter.Format(123456), Is.EqualTo("$1,235"));
                Assert.That(StimMoneyFormatter.FormatPrecise(1923), Is.EqualTo("$19.23"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }
    }
}
