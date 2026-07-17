using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public enum StimUiBindingOwner
    {
        Shell,
        Life,
        Study,
        Work,
        Bank,
        Social,
        Goals,
        Modal
    }

    public static class StimUiBindingManifest
    {
        private static readonly IReadOnlyDictionary<StimUiBindingOwner, IReadOnlyList<string>> bindings =
            new Dictionary<StimUiBindingOwner, IReadOnlyList<string>>
            {
                [StimUiBindingOwner.Shell] = new[]
                {
                    "screen", "life-summary", "calendar-summary", "career-progress-value",
                    "career-progress-fill", "cash-value", "header-net-worth-value", "avatar-glyph",
                    "open-life-summary", "add-cash", "life-scroll", "life-summary-view", "education-view",
                    "career-view", "money-view", "social-view", "goals-view", "time-dock", "advance-month",
                    "advance-year", "nav-life", "nav-education", "nav-career", "nav-money", "nav-social",
                    "nav-goals"
                },
                [StimUiBindingOwner.Life] = new[]
                {
                    "life-feed-list", "player-overview", "toggle-overview", "overview-career",
                    "overview-calendar", "health-value", "happiness-value", "smarts-value", "looks-value",
                    "luck-value", "health-fill", "happiness-fill", "smarts-fill", "looks-fill", "luck-fill",
                    "monthly-paycheck-value", "annual-salary-value", "net-worth-value", "focus-study",
                    "focus-workout", "focus-study-title", "focus-study-effect", "focus-workout-title",
                    "focus-workout-effect", "context-activities", "home-condition", "home-progress",
                    "home-actions", "home-upgrade-feedback", "close-life-summary", "summary-stage-detail",
                    "age-stage-summary",
                    "summary-calendar-detail", "summary-career-detail", "summary-health-value",
                    "summary-happiness-value", "summary-smarts-value", "summary-looks-value",
                    "summary-luck-value", "summary-health-fill", "summary-happiness-fill",
                    "summary-smarts-fill", "summary-looks-fill", "summary-luck-fill"
                },
                [StimUiBindingOwner.Study] = new[]
                {
                    "education-destination-content", "education-empty-state", "education-unavailable-copy",
                    "education-catalog", "education-catalog-status", "education-catalog-list", "education-card",
                    "education-stage", "learning-level", "learning-fill", "learning-progress",
                    "education-actions", "skills-list", "study-session-sheet", "study-session-title",
                    "study-session-description", "study-session-effects", "study-session-timing",
                    "study-session-requirement", "study-session-cancel", "study-session-confirm"
                },
                [StimUiBindingOwner.Work] = new[]
                {
                    "career-destination-content", "career-empty-state", "career-context-copy",
                    "career-path-preview", "career-card", "career-role", "career-salary", "career-next-step",
                    "career-action-fill", "career-action-progress", "career-actions", "career-actions-card",
                    "manual-work-role", "manual-work-rate", "manual-work-tap", "manual-work-feedback"
                },
                [StimUiBindingOwner.Bank] = new[]
                {
                    "money-cash-value", "savings-balance-value", "savings-available-value",
                    "savings-deposit-mode", "savings-withdraw-mode", "savings-amount-input",
                    "savings-transfer-feedback", "money-transaction-history", "money-accounts-list",
                    "cash-flow-gross", "cash-flow-taxes", "cash-flow-expenses", "cash-flow-credit-interest",
                    "cash-flow-savings-interest", "cash-flow-net", "savings-projection", "credit-balance-value",
                    "credit-detail-value", "available-credit-value", "credit-repayment-input",
                    "credit-repayment-feedback", "index-fund-value", "index-fund-contributions",
                    "index-fund-performance", "index-investment-requirement", "index-investment-input",
                    "index-investment-feedback", "bank-tab-savings", "bank-tab-credit", "bank-tab-investing",
                    "bank-panel-savings", "bank-panel-credit", "bank-panel-investing"
                },
                [StimUiBindingOwner.Social] = new[]
                {
                    "relationship-list-view", "relationship-list", "discover-compatible-person",
                    "relationship-discovery-feedback", "relationship-detail-view", "relationship-back",
                    "relationship-avatar", "relationship-name", "relationship-type", "relationship-strength",
                    "relationship-fill", "relationship-genetics", "relationship-actions"
                },
                [StimUiBindingOwner.Goals] = new[]
                {
                    "goals-destination-content", "achievements-count", "achievements-list"
                },
                [StimUiBindingOwner.Modal] = new[]
                {
                    "event-sheet", "event-category", "event-title", "event-body", "choices", "result-card",
                    "result-text", "result-effects", "event-continue", "new-life-setup", "new-life-error",
                    "open-new-life", "cancel-new-life", "continue-current-life", "create-new-life",
                    "final-life-summary", "ending-name", "ending-status", "ending-summary", "ending-new-life"
                }
            };

        public static IReadOnlyDictionary<StimUiBindingOwner, IReadOnlyList<string>> Bindings => bindings;

        public static bool TryValidate(VisualElement root, out string error)
        {
            if (root == null)
            {
                error = "The UI root is unavailable.";
                return false;
            }

            var missing = new List<string>();
            var duplicates = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var group in bindings)
            {
                foreach (var name in group.Value)
                {
                    if (!seen.Add(name)) duplicates.Add($"{group.Key}/{name}");
                    if (root.Q(name) == null) missing.Add($"{group.Key}/{name}");
                }
            }

            if (missing.Count == 0 && duplicates.Count == 0)
            {
                error = string.Empty;
                return true;
            }

            var problems = new List<string>();
            if (missing.Count > 0) problems.Add("Missing: " + string.Join(", ", missing));
            if (duplicates.Count > 0) problems.Add("Duplicate ownership: " + string.Join(", ", duplicates));
            error = string.Join(". ", problems);
            return false;
        }
    }
}
