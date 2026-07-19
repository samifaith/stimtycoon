using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public enum UiBindingOwner
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

    public static class UiBindingManifest
    {
        private static readonly IReadOnlyDictionary<UiBindingOwner, IReadOnlyList<string>> bindings =
            new Dictionary<UiBindingOwner, IReadOnlyList<string>>
            {
                [UiBindingOwner.Shell] = new[]
                {
                    "screen", "life-summary", "calendar-summary", "career-progress-value",
                    "career-progress-fill", "cash-value", "header-net-worth-value", "avatar-glyph",
                    "open-life-summary", "add-cash", "life-scroll", "life-summary-view", "education-view",
                    "career-view", "money-view", "social-view", "goals-view", "time-dock", "advance-month",
                    "advance-year", "nav-life", "nav-education", "nav-career", "nav-money", "nav-social",
                    "nav-goals"
                },
                [UiBindingOwner.Life] = new[]
                {
                    "life-feed-list", "player-overview", "toggle-overview", "overview-career",
                    "overview-calendar", "health-value", "happiness-value", "smarts-value", "looks-value",
                    "luck-value", "health-fill", "happiness-fill", "smarts-fill", "looks-fill", "luck-fill",
                    "monthly-paycheck-value", "annual-salary-value", "net-worth-value", "focus-study",
                    "focus-workout", "focus-study-title", "focus-study-effect", "focus-workout-title",
                    "focus-workout-effect", "context-activities", "home-condition", "home-progress",
                    "home-object-list", "home-actions", "home-upgrade-feedback", "home-action-retry", "close-life-summary", "summary-stage-detail",
                    "age-stage-summary",
                    "summary-calendar-detail", "summary-career-detail", "summary-health-value",
                    "summary-happiness-value", "summary-smarts-value", "summary-looks-value",
                    "summary-luck-value", "summary-health-fill", "summary-happiness-fill",
                    "summary-smarts-fill", "summary-looks-fill", "summary-luck-fill"
                },
                [UiBindingOwner.Study] = new[]
                {
                    "education-destination-content", "education-empty-state", "education-unavailable-copy",
                    "education-catalog", "education-catalog-status", "education-catalog-list", "education-card",
                    "education-stage", "learning-level", "learning-fill", "learning-progress",
                    "education-actions", "skills-list", "study-session-sheet", "study-session-title",
                    "study-session-description", "study-session-effects", "study-session-timing",
                    "study-session-requirement", "study-session-cancel", "study-session-confirm",
                    "study-match-card", "study-match-status", "study-match-score", "study-match-board",
                    "study-match-start", "study-match-pause", "study-match-claim", "study-match-feedback"
                },
                [UiBindingOwner.Work] = new[]
                {
                    "career-destination-content", "career-empty-state", "career-context-copy",
                    "career-path-preview", "career-card", "career-role", "career-salary", "career-next-step",
                    "career-action-fill", "career-action-progress", "career-actions", "career-actions-card",
                    "work-tab-career", "work-tab-business", "business-summary",
                    "manual-work-role", "manual-work-rate", "manual-work-tap", "manual-work-feedback", "manual-work-retry"
                },
                [UiBindingOwner.Bank] = new[]
                {
                    "money-cash-value", "savings-balance-value", "savings-available-value",
                    "savings-deposit-mode", "savings-withdraw-mode", "savings-amount-input",
                    "savings-transfer-feedback", "savings-transfer-retry", "money-transaction-history", "money-accounts-list",
                    "cash-flow-gross", "cash-flow-taxes", "cash-flow-expenses", "cash-flow-credit-interest",
                    "cash-flow-savings-interest", "cash-flow-net", "savings-projection", "credit-balance-value",
                    "credit-detail-value", "available-credit-value", "credit-repayment-input",
                    "credit-repayment-feedback", "credit-repayment-retry", "index-fund-value", "index-fund-contributions",
                    "index-fund-performance", "index-investment-requirement", "index-investment-input",
                    "index-investment-feedback", "index-investment-retry", "bank-tab-savings", "bank-tab-credit", "bank-tab-investing", "bank-tab-property", "bank-context-tip",
                    "bank-panel-savings", "bank-panel-credit", "bank-panel-investing", "bank-panel-property",
                    "property-portfolio-summary", "property-cash-flow", "property-actions", "property-feedback"
                },
                [UiBindingOwner.Social] = new[]
                {
                    "relationship-list-view", "relationship-list", "discover-compatible-person",
                    "social-filter-all", "social-filter-family", "social-filter-friends", "social-filter-romance", "family-summary",
                    "relationship-discovery-feedback", "relationship-discovery-retry", "relationship-detail-view", "relationship-back",
                    "relationship-avatar", "relationship-name", "relationship-type", "relationship-strength",
                    "relationship-fill", "relationship-genetics", "relationship-status", "relationship-history", "relationship-actions"
                },
                [UiBindingOwner.Goals] = new[]
                {
                    "goals-destination-content", "goals-tab-main", "goals-tab-daily", "goals-tab-life",
                    "goals-tab-achievements", "pinned-goal-summary", "achievements-count", "achievements-list"
                },
                [UiBindingOwner.Modal] = new[]
                {
                    "event-sheet", "event-category", "event-title", "event-body", "choices", "result-card",
                    "result-text", "result-effects", "event-continue", "new-life-setup", "new-life-error",
                    "open-new-life", "cancel-new-life", "continue-current-life", "create-new-life",
                    "final-life-summary", "ending-name", "ending-status", "ending-summary", "ending-new-life"
                }
            };

        public static IReadOnlyDictionary<UiBindingOwner, IReadOnlyList<string>> Bindings => bindings;

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
