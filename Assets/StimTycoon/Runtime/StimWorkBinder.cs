using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimWorkBinder
    {
        private readonly Label careerContextCopy;
        private readonly VisualElement careerPathPreview;
        private readonly Label manualWorkRole;
        private readonly Label manualWorkRate;
        private readonly Label businessSummary;

        public StimWorkBinder(VisualElement root)
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

        public void RenderWorkspace(StimGameState state, bool businessSelected, Func<long, string> formatMoney)
        {
            StimPresentationStateStyler.Apply(WorkTabCareer,
                businessSelected ? StimPresentationState.Available : StimPresentationState.Selected);
            StimPresentationStateStyler.Apply(WorkTabBusiness,
                businessSelected ? StimPresentationState.Selected : StimPresentationState.Available);
            businessSummary.EnableInClassList("hidden", !businessSelected);
            ManualWorkCard.EnableInClassList("hidden", businessSelected);
            if (!businessSelected) return;
            var business = state.business ?? new StimBusinessState();
            businessSummary.text = business.status == "operating"
                ? $"{business.displayName} · Level {business.level} · AP {business.actionPoints}/{business.maxActionPoints}\n" +
                  $"Revenue {formatMoney(business.lastRevenueMinorUnits)} · Expenses/payroll {formatMoney(business.lastExpensesMinorUnits)} · " +
                  $"Staff {business.staffCount} · Location {business.locationLevel} · Valuation {formatMoney(business.valuationMinorUnits)}\n" +
                  $"Operating progress {business.operatingProgress} · Lifetime profit {formatMoney(business.lifetimeProfitMinorUnits)} · " +
                  $"Disruptions {business.riskEventsExperienced} · Loss streak {business.consecutiveLossMonths}"
                : business.status == "sold" ? "Local Services Co. was sold. Its final value remains in your life history."
                : business.status == "failed" ? "Local Services Co. closed after sustained losses. Recovery remains possible through career work."
                : "Local Services Co. · Requires age 18, Professional Level 2, and $1,000 startup cash.";
        }

        public void RenderPathPreview(StimGameState state, bool adult)
        {
            careerPathPreview.Clear();
            careerContextCopy.text = adult
                ? "Career and business actions use the current life state. Requirements remain visible before an action is available."
                : "Childhood choices, education, and skills shape the paths that will become relevant later.";
            if (!adult) return;

            var career = state.career ?? new StimCareerState();
            var employed = !string.IsNullOrEmpty(career.roleTitle) && career.roleTitle != "Retired";
            careerPathPreview.Add(StimUiComponentFactory.CreatePathRow(
                "entry-career", "💼", "Entry-level Career",
                "Apply, interview, and grow through the supported career catalog.",
                employed ? career.roleTitle : "Available", true));
            careerPathPreview.Add(StimUiComponentFactory.CreatePathRow(
                "career-ladder", "↗", "Career Ladder",
                "Build career progress to qualify for the next role.",
                employed ? $"{career.careerProgress} progress" : "Apply first", employed));

            var business = state.business ?? new StimBusinessState();
            var professionalLevel = StimGameSessionService.GetSkillLevel(
                StimGameSessionService.GetSkillExperience(state.skills, "professional"));
            var canStartBusiness = business.status == "none" && professionalLevel >= 2 &&
                                   state.finances.cashMinorUnits >= 100000;
            var businessStatus = business.status == "operating"
                ? $"Level {business.level}"
                : business.status != "none" ? ToDisplayName(business.status)
                : professionalLevel < 2 ? "Professional 2"
                : state.finances.cashMinorUnits < 100000 ? "$1,000 needed"
                : "Available";
            careerPathPreview.Add(StimUiComponentFactory.CreatePathRow(
                "local-services", "🏢", "Local Services Business",
                "Requires age 18, Professional Level 2, and $1,000 startup cash.",
                businessStatus, canStartBusiness || business.status == "operating"));
        }

        public void RenderManualWork(StimGameState state, Func<long, string> formatPreciseMoney)
        {
            var adult = state.character.age >= 18;
            var career = state.career ?? new StimCareerState();
            var employed = !string.IsNullOrEmpty(career.roleTitle) && career.roleTitle != "Retired" &&
                           career.annualSalaryMinorUnits > 0 && state.character.lifeStatus == "active";
            var hourlyRate = StimGameSessionService.CalculateHourlyRateMinorUnits(career.annualSalaryMinorUnits);
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
