using System;
using NUnit.Framework;
using StimTycoon.Runtime;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class StimLifeOverviewBinderTests
    {
        [Test]
        public void Constructor_RequiresCompleteNamedContract()
        {
            Assert.Throws<ArgumentNullException>(() => new StimLifeOverviewBinder(null));
            Assert.That(new StimLifeOverviewBinder(new VisualElement()).IsValid, Is.False);
            Assert.That(new StimLifeOverviewBinder(CreateRoot()).IsValid, Is.True);
        }

        [Test]
        public void Render_BindsOverviewSummaryStatsAndFinancialCopy()
        {
            var root = CreateRoot();
            var binder = new StimLifeOverviewBinder(root);
            var state = new StimGameState();
            state.character.age = 30;
            state.character.lifeStage = "adult";
            state.character.health = 120;
            state.character.happiness = 75;
            state.character.smarts = 64;
            state.character.looks = 55;
            state.character.luck = -5;
            state.calendar.monthOfYear = 7;
            state.career.roleTitle = "Analyst";
            state.career.annualSalaryMinorUnits = 7200000;
            state.career.careerProgress = 42;
            state.finances.taxRateBasisPoints = 1250;

            binder.Render(
                state, 1234500, 250000,
                value => $"${value / 100m:0.00}",
                value => $"+${value / 100m:0.00}",
                value => char.ToUpperInvariant(value[0]) + value.Substring(1));

            Assert.That(root.Q<Label>("overview-career").text, Is.EqualTo("Analyst · Stim Financial Group"));
            Assert.That(root.Q<Label>("summary-calendar-detail").text, Is.EqualTo("Age 30 · Month 7 of 12"));
            Assert.That(root.Q<Label>("health-value").text, Is.EqualTo("120 / 100"));
            Assert.That(root.Q<Label>("summary-health-value").text, Is.EqualTo("120 / 100"));
            Assert.That(root.Q("health-fill").style.width.value.value, Is.EqualTo(100f));
            Assert.That(root.Q("summary-luck-fill").style.width.value.value, Is.EqualTo(0f));
            Assert.That(root.Q<Label>("career-progress-value").text, Is.EqualTo("42 / 100"));
            Assert.That(root.Q<Label>("net-worth-value").tooltip, Is.EqualTo("Total net worth $12345.00"));
            Assert.That(root.Q<Label>("monthly-paycheck-value").text, Is.EqualTo("+$2500.00"));
        }

        private static VisualElement CreateRoot()
        {
            var root = new VisualElement();
            foreach (var name in new[]
                     {
                         "overview-career", "overview-calendar", "career-progress-value",
                         "monthly-paycheck-value", "annual-salary-value", "net-worth-value",
                         "summary-stage-detail", "summary-calendar-detail", "summary-career-detail"
                     })
                root.Add(new Label { name = name });
            root.Add(new VisualElement { name = "career-progress-fill" });
            foreach (var stat in new[] { "health", "happiness", "smarts", "looks", "luck" })
            {
                root.Add(new Label { name = $"{stat}-value" });
                root.Add(new VisualElement { name = $"{stat}-fill" });
                root.Add(new Label { name = $"summary-{stat}-value" });
                root.Add(new VisualElement { name = $"summary-{stat}-fill" });
            }
            return root;
        }
    }
}
