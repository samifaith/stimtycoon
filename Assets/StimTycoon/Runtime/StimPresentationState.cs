using System;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    public enum StimPresentationState
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

    public static class StimPresentationStateStyler
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

        public static void Apply(VisualElement element, StimPresentationState state)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            foreach (var className in StateClasses) element.RemoveFromClassList(className);
            foreach (var className in LegacyStateClasses) element.RemoveFromClassList(className);

            var stateName = state.ToString().ToLowerInvariant();
            element.AddToClassList("st-state-" + stateName);
            if (state == StimPresentationState.Locked || state == StimPresentationState.Selected ||
                state == StimPresentationState.Active || state == StimPresentationState.Cooldown ||
                state == StimPresentationState.Claimable || state == StimPresentationState.Claimed)
                element.AddToClassList(stateName);
        }

        public static StimPresentationState FromActionState(StimActionState state)
        {
            switch (state)
            {
                case StimActionState.Locked: return StimPresentationState.Locked;
                case StimActionState.InProgress: return StimPresentationState.Active;
                case StimActionState.Paused: return StimPresentationState.Cooldown;
                case StimActionState.Claimable: return StimPresentationState.Claimable;
                case StimActionState.Complete: return StimPresentationState.Claimed;
                case StimActionState.Expired: return StimPresentationState.Terminal;
                default: return StimPresentationState.Available;
            }
        }
    }

    public enum StimFeedbackKind { Confirmation, Success, Error, Rollback, Offline, Terminal }

    public static class StimFeedbackPresenter
    {
        public static void Show(Label label, string message, StimFeedbackKind kind, bool retryAvailable = false)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            var visibleMessage = message ?? string.Empty;
            label.text = retryAvailable && !string.IsNullOrWhiteSpace(visibleMessage)
                ? visibleMessage + " Try again."
                : visibleMessage;
            label.EnableInClassList("hidden", string.IsNullOrWhiteSpace(message));
            label.EnableInClassList("st-feedback-retry", retryAvailable);
            label.EnableInClassList("is-error", kind == StimFeedbackKind.Error ||
                kind == StimFeedbackKind.Rollback || kind == StimFeedbackKind.Offline ||
                kind == StimFeedbackKind.Terminal);
            StimPresentationStateStyler.Apply(label, ToPresentationState(kind));
            var prefix = kind == StimFeedbackKind.Rollback ? "Save rolled back" : kind.ToString();
            label.tooltip = $"{prefix}. {message}{(retryAvailable ? " Retry is available." : string.Empty)}".Trim();
        }

        public static void ShowTransactionResult(Label label, bool succeeded, string summary)
        {
            var kind = succeeded ? StimFeedbackKind.Success : ClassifyFailure(summary);
            Show(label, summary, kind, !succeeded && kind != StimFeedbackKind.Terminal);
        }

        public static void Clear(Label label)
        {
            if (label == null) return;
            label.text = string.Empty;
            label.tooltip = string.Empty;
            label.AddToClassList("hidden");
            label.RemoveFromClassList("is-error");
            label.RemoveFromClassList("st-feedback-retry");
            StimPresentationStateStyler.Apply(label, StimPresentationState.Empty);
        }

        public static StimFeedbackKind ClassifyFailure(string summary)
        {
            var value = summary ?? string.Empty;
            if (ContainsAny(value, "already claimed", "life has ended", "life is over", "deceased",
                    "permanently unavailable", "no longer exists", "cannot be restored"))
                return StimFeedbackKind.Terminal;
            if (value.IndexOf("offline", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("network", StringComparison.OrdinalIgnoreCase) >= 0)
                return StimFeedbackKind.Offline;
            if (value.IndexOf("save", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("persist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("commit", StringComparison.OrdinalIgnoreCase) >= 0)
                return StimFeedbackKind.Rollback;
            return StimFeedbackKind.Error;
        }

        public static bool IsRetryable(string summary) =>
            ClassifyFailure(summary) != StimFeedbackKind.Terminal;

        private static StimPresentationState ToPresentationState(StimFeedbackKind kind)
        {
            switch (kind)
            {
                case StimFeedbackKind.Confirmation: return StimPresentationState.Selected;
                case StimFeedbackKind.Success: return StimPresentationState.Available;
                case StimFeedbackKind.Offline: return StimPresentationState.Offline;
                case StimFeedbackKind.Terminal: return StimPresentationState.Terminal;
                default: return StimPresentationState.Error;
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
