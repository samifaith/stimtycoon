using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class WorkBinder
    {
        private readonly Label careerContextCopy;
        private readonly VisualElement careerPathPreview;
        private readonly Label manualWorkRole;
        private readonly Label manualWorkRate;
        private readonly Label businessSummary;

        public WorkBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            careerContextCopy = root.Q<Label>("career-context-copy");
            careerPathPreview = root.Q<VisualElement>("career-path-preview");
            manualWorkRole = root.Q<Label>("manual-work-role");
            manualWorkRate = root.Q<Label>("manual-work-rate");
            businessSummary = root.Q<Label>("business-summary");
            WorkTabCareer = root.Q<Button>("work-tab-career");
            WorkTabBusiness = root.Q<Button>("work-tab-business");
            ManualWorkCard = root.Q<VisualElement>("manual-work-card");
            ManualWorkTap = root.Q<Button>("manual-work-tap");
        }

        public Button ManualWorkTap { get; }
        public Button WorkTabCareer { get; }
        public Button WorkTabBusiness { get; }
        public VisualElement ManualWorkCard { get; }
        public bool IsValid => careerContextCopy != null && careerPathPreview != null &&
                               manualWorkRole != null && manualWorkRate != null && businessSummary != null &&
                               WorkTabCareer != null && WorkTabBusiness != null && ManualWorkCard != null && ManualWorkTap != null;

        public void RenderWorkspace(GameState state, bool businessSelected, Func<long, string> formatMoney)
        {
            PresentationStateStyler.Apply(WorkTabCareer,
                businessSelected ? PresentationState.Available : PresentationState.Selected);
            PresentationStateStyler.Apply(WorkTabBusiness,
                businessSelected ? PresentationState.Selected : PresentationState.Available);
            businessSummary.EnableInClassList("hidden", !businessSelected);
            ManualWorkCard.EnableInClassList("hidden", businessSelected);
            if (!businessSelected) return;
            var business = state.business ?? new BusinessState();
            var portfolioCount = state.businessPortfolio?.businesses?.Count ??
                                 (business.status == "none" ? 0 : 1);
            businessSummary.text = $"Business portfolio {portfolioCount}/{BusinessPortfolioState.MaxBusinesses}\n" +
                (business.status == "operating"
                ? $"{business.displayName} · Level {business.level} · AP {business.actionPoints}/{business.maxActionPoints}\n" +
                  $"Revenue {formatMoney(business.lastRevenueMinorUnits)} · Expenses/payroll {formatMoney(business.lastExpensesMinorUnits)} · " +
                  $"Staff {business.staffCount} · Location {business.locationLevel} · Valuation {formatMoney(business.valuationMinorUnits)}\n" +
                  $"Operating progress {business.operatingProgress} · Lifetime profit {formatMoney(business.lifetimeProfitMinorUnits)} · " +
                  $"Disruptions {business.riskEventsExperienced} · Loss streak {business.consecutiveLossMonths}"
                : business.status == "sold" ? "Local Services Co. was sold. Its final value remains in your life history."
                : business.status == "failed" ? "Local Services Co. closed after sustained losses. Recovery remains possible through career work."
                : "Local Services Co. · Requires age 18, Professional Level 2, and $1,000 startup cash.") +
                "\nAdditional business slots are save-ready; launch actions arrive after MVP balancing.";
        }

        public void RenderPathPreview(GameState state, bool adult)
        {
            careerPathPreview.Clear();
            careerContextCopy.text = adult
                ? "Career and business actions use the current life state. Requirements remain visible before an action is available."
                : "Childhood choices, education, and skills shape the paths that will become relevant later.";
            if (!adult) return;

            var career = state.career ?? new CareerState();
            var employed = !string.IsNullOrEmpty(career.roleTitle) && career.roleTitle != "Retired";
            careerPathPreview.Add(UiComponentFactory.CreatePathRow(
                "entry-career", "💼", "Entry-level Career",
                "Apply, interview, and grow through the supported career catalog.",
                employed ? career.roleTitle : "Available", true));
            careerPathPreview.Add(UiComponentFactory.CreatePathRow(
                "career-ladder", "↗", "Career Ladder",
                "Build career progress to qualify for the next role.",
                employed ? $"{career.careerProgress} progress" : "Apply first", employed));

            var business = state.business ?? new BusinessState();
            var professionalLevel = GameSessionService.GetSkillLevel(
                GameSessionService.GetSkillExperience(state.skills, "professional"));
            var canStartBusiness = business.status == "none" && professionalLevel >= 2 &&
                                   state.finances.cashMinorUnits >= 100000;
            var businessStatus = business.status == "operating"
                ? $"Level {business.level}"
                : business.status != "none" ? ToDisplayName(business.status)
                : professionalLevel < 2 ? "Professional 2"
                : state.finances.cashMinorUnits < 100000 ? "$1,000 needed"
                : "Available";
            careerPathPreview.Add(UiComponentFactory.CreatePathRow(
                "local-services", "🏢", "Local Services Business",
                "Requires age 18, Professional Level 2, and $1,000 startup cash.",
                businessStatus, canStartBusiness || business.status == "operating"));
            careerPathPreview.Add(UiComponentFactory.CreatePathRow(
                "retail-shop", "🏪", "Retail Shop",
                "Additional business slot with persistent portfolio support.", "Planned", false));
            careerPathPreview.Add(UiComponentFactory.CreatePathRow(
                "digital-studio", "💻", "Digital Studio",
                "Additional business slot with persistent portfolio support.", "Planned", false));
        }

        public void RenderManualWork(GameState state, Func<long, string> formatPreciseMoney)
        {
            var adult = state.character.age >= 18;
            var career = state.career ?? new CareerState();
            var employed = !string.IsNullOrEmpty(career.roleTitle) && career.roleTitle != "Retired" &&
                           career.annualSalaryMinorUnits > 0 && state.character.lifeStatus == "active";
            var hourlyRate = GameSessionService.CalculateHourlyRateMinorUnits(career.annualSalaryMinorUnits);
            manualWorkRole.text = employed ? career.roleTitle : "Get a salaried job to begin";
            manualWorkRate.text = employed ? $"{formatPreciseMoney(hourlyRate)} per hour" : "$0.00 per hour";
            ManualWorkTap.parent?.EnableInClassList("hidden", !adult);
            ManualWorkTap.text = employed
                ? $"WORK 1 HOUR  ·  +{formatPreciseMoney(hourlyRate)}"
                : "WORK 1 HOUR";
            ManualWorkTap.SetEnabled(employed && string.IsNullOrEmpty(state.pendingEventId));
        }

        private static string ToDisplayName(string id) =>
            string.IsNullOrEmpty(id) ? "" : char.ToUpperInvariant(id[0]) + id.Substring(1).Replace('_', ' ');
    }
}
