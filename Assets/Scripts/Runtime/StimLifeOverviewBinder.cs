using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimLifeOverviewBinder
    {
        private readonly Label overviewCareer;
        private readonly Label overviewCalendar;
        private readonly Label careerProgressValue;
        private readonly Label monthlyPaycheckValue;
        private readonly Label annualSalaryValue;
        private readonly Label netWorthValue;
        private readonly VisualElement careerProgressFill;
        private readonly Label summaryStageDetail;
        private readonly Label summaryCalendarDetail;
        private readonly Label summaryCareerDetail;
        private readonly StatBinding health;
        private readonly StatBinding happiness;
        private readonly StatBinding smarts;
        private readonly StatBinding looks;
        private readonly StatBinding luck;

        public StimLifeOverviewBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            overviewCareer = root.Q<Label>("overview-career");
            overviewCalendar = root.Q<Label>("overview-calendar");
            careerProgressValue = root.Q<Label>("career-progress-value");
            monthlyPaycheckValue = root.Q<Label>("monthly-paycheck-value");
            annualSalaryValue = root.Q<Label>("annual-salary-value");
            netWorthValue = root.Q<Label>("net-worth-value");
            careerProgressFill = root.Q<VisualElement>("career-progress-fill");
            summaryStageDetail = root.Q<Label>("summary-stage-detail");
            summaryCalendarDetail = root.Q<Label>("summary-calendar-detail");
            summaryCareerDetail = root.Q<Label>("summary-career-detail");
            health = new StatBinding(root, "health");
            happiness = new StatBinding(root, "happiness");
            smarts = new StatBinding(root, "smarts");
            looks = new StatBinding(root, "looks");
            luck = new StatBinding(root, "luck");
        }

        public bool IsValid => overviewCareer != null && overviewCalendar != null &&
            careerProgressValue != null && monthlyPaycheckValue != null && annualSalaryValue != null &&
            netWorthValue != null && careerProgressFill != null && summaryStageDetail != null &&
            summaryCalendarDetail != null && summaryCareerDetail != null && health.IsValid &&
            happiness.IsValid && smarts.IsValid && looks.IsValid && luck.IsValid;

        public void Render(
            StimGameState state,
            long netWorth,
            long estimatedNetMonthlyPay,
            Func<long, string> formatMoney,
            Func<long, string> formatSignedMoney,
            Func<string, string> formatDisplayName)
        {
            var career = state.career;
            var stage = formatDisplayName(state.character.lifeStage);
            netWorthValue.text = formatMoney(netWorth);
            netWorthValue.tooltip = $"Total net worth {formatMoney(netWorth)}";
            overviewCareer.text = string.IsNullOrEmpty(career.roleTitle)
                ? stage
                : $"{career.roleTitle} · Stim Financial Group";
            overviewCalendar.text = $"Age {state.character.age} · Month {state.calendar.monthOfYear} of 12";
            summaryStageDetail.text = stage;
            summaryCalendarDetail.text = overviewCalendar.text;
            summaryCareerDetail.text = string.IsNullOrEmpty(career.roleTitle)
                ? "Not started"
                : $"{career.roleTitle} · {formatMoney(career.annualSalaryMinorUnits)} gross";
            health.Render(state.character.health);
            happiness.Render(state.character.happiness);
            smarts.Render(state.character.smarts);
            looks.Render(state.character.looks);
            luck.Render(state.character.luck);
            careerProgressValue.text = $"{career.careerProgress} / 100";
            careerProgressFill.style.width = Length.Percent(ClampPercent(career.careerProgress));
            monthlyPaycheckValue.text = formatSignedMoney(estimatedNetMonthlyPay);
            annualSalaryValue.text = $"{formatMoney(career.annualSalaryMinorUnits)} gross · {state.finances.taxRateBasisPoints / 100m:0.#}% tax";
        }

        private static float ClampPercent(float value) => Math.Max(0f, Math.Min(100f, value));

        private readonly struct StatBinding
        {
            private readonly Label value;
            private readonly VisualElement fill;
            private readonly Label summaryValue;
            private readonly VisualElement summaryFill;

            public StatBinding(VisualElement root, string id)
            {
                value = root.Q<Label>($"{id}-value");
                fill = root.Q<VisualElement>($"{id}-fill");
                summaryValue = root.Q<Label>($"summary-{id}-value");
                summaryFill = root.Q<VisualElement>($"summary-{id}-fill");
            }

            public bool IsValid => value != null && fill != null && summaryValue != null && summaryFill != null;

            public void Render(int amount)
            {
                value.text = $"{amount} / 100";
                summaryValue.text = value.text;
                var width = Length.Percent(ClampPercent(amount));
                fill.style.width = width;
                summaryFill.style.width = width;
            }
        }
    }
}
