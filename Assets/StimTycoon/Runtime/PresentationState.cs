using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public enum PresentationState
    {
        Available,
        Locked,
        Disabled,
        Selected,
        Active,
        Cooldown,
        Claimable,
        Claimed,
        Empty,
        Loading,
        Error,
        Offline,
        Terminal
    }

    public static class PresentationStateStyler
    {
        private static readonly string[] StateClasses =
        {
            "st-state-available", "st-state-locked", "st-state-disabled", "st-state-selected",
            "st-state-active", "st-state-cooldown", "st-state-claimable", "st-state-claimed",
            "st-state-empty", "st-state-loading", "st-state-error", "st-state-offline",
            "st-state-terminal"
        };

        private static readonly string[] LegacyStateClasses =
        {
            "locked", "selected", "active", "cooldown", "claimable", "claimed"
        };

        public static void Apply(VisualElement element, PresentationState state)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            foreach (var className in StateClasses) element.RemoveFromClassList(className);
            foreach (var className in LegacyStateClasses) element.RemoveFromClassList(className);

            var stateName = state.ToString().ToLowerInvariant();
            element.AddToClassList("st-state-" + stateName);
            if (state == PresentationState.Locked || state == PresentationState.Selected ||
                state == PresentationState.Active || state == PresentationState.Cooldown ||
                state == PresentationState.Claimable || state == PresentationState.Claimed)
                element.AddToClassList(stateName);
        }

        public static PresentationState FromActionState(ActionState state)
        {
            switch (state)
            {
                case ActionState.Locked: return PresentationState.Locked;
                case ActionState.InProgress: return PresentationState.Active;
                case ActionState.Paused: return PresentationState.Cooldown;
                case ActionState.Claimable: return PresentationState.Claimable;
                case ActionState.Complete: return PresentationState.Claimed;
                case ActionState.Expired: return PresentationState.Terminal;
                default: return PresentationState.Available;
            }
        }
    }

    public enum FeedbackKind { Confirmation, Success, Error, Rollback, Offline, Terminal }

    public static class FeedbackPresenter
    {
        public static void Show(Label label, string message, FeedbackKind kind, bool retryAvailable = false)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            var visibleMessage = message ?? string.Empty;
            label.text = retryAvailable && !string.IsNullOrWhiteSpace(visibleMessage)
                ? visibleMessage + " Try again."
                : visibleMessage;
            label.EnableInClassList("hidden", string.IsNullOrWhiteSpace(message));
            label.EnableInClassList("st-feedback-retry", retryAvailable);
            label.EnableInClassList("is-error", kind == FeedbackKind.Error ||
                kind == FeedbackKind.Rollback || kind == FeedbackKind.Offline ||
                kind == FeedbackKind.Terminal);
            PresentationStateStyler.Apply(label, ToPresentationState(kind));
            var prefix = kind == FeedbackKind.Rollback ? "Save rolled back" : kind.ToString();
            label.tooltip = $"{prefix}. {message}{(retryAvailable ? " Retry is available." : string.Empty)}".Trim();
        }

        public static void ShowTransactionResult(Label label, bool succeeded, string summary)
        {
            var kind = succeeded ? FeedbackKind.Success : ClassifyFailure(summary);
            Show(label, summary, kind, !succeeded && kind != FeedbackKind.Terminal);
        }

        public static void Clear(Label label)
        {
            if (label == null) return;
            label.text = string.Empty;
            label.tooltip = string.Empty;
            label.AddToClassList("hidden");
            label.RemoveFromClassList("is-error");
            label.RemoveFromClassList("st-feedback-retry");
            PresentationStateStyler.Apply(label, PresentationState.Empty);
        }

        public static FeedbackKind ClassifyFailure(string summary)
        {
            var value = summary ?? string.Empty;
            if (ContainsAny(value, "already claimed", "life has ended", "life is over", "deceased",
                    "permanently unavailable", "no longer exists", "cannot be restored"))
                return FeedbackKind.Terminal;
            if (value.IndexOf("offline", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("network", StringComparison.OrdinalIgnoreCase) >= 0)
                return FeedbackKind.Offline;
            if (value.IndexOf("save", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("persist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("commit", StringComparison.OrdinalIgnoreCase) >= 0)
                return FeedbackKind.Rollback;
            return FeedbackKind.Error;
        }

        public static bool IsRetryable(string summary) =>
            ClassifyFailure(summary) != FeedbackKind.Terminal;

        private static PresentationState ToPresentationState(FeedbackKind kind)
        {
            switch (kind)
            {
                case FeedbackKind.Confirmation: return PresentationState.Selected;
                case FeedbackKind.Success: return PresentationState.Available;
                case FeedbackKind.Offline: return PresentationState.Offline;
                case FeedbackKind.Terminal: return PresentationState.Terminal;
                default: return PresentationState.Error;
            }
        }

        private static bool ContainsAny(string value, params string[] fragments)
        {
            foreach (var fragment in fragments)
                if (value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
    }
}
