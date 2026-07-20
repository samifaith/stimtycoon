using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class PrivacyConsentBinder : IDisposable
    {
        private const string PrivacyUrl = "https://unity.com/legal/game-player-and-app-user-privacy-policy";
        private readonly VisualElement screen;
        private readonly Toggle analytics;
        private readonly Toggle ads;
        private readonly Label feedback;
        private readonly Button open;
        private readonly Button cancel;
        private readonly Button save;
        private readonly Button policy;
        private readonly Button delete;

        public PrivacyConsentBinder(VisualElement root)
        {
            screen = root.Q<VisualElement>("privacy-consent-screen");
            analytics = root.Q<Toggle>("privacy-analytics-toggle");
            ads = root.Q<Toggle>("privacy-ads-toggle");
            feedback = root.Q<Label>("privacy-feedback");
            open = root.Q<Button>("open-privacy-settings");
            cancel = root.Q<Button>("privacy-cancel");
            save = root.Q<Button>("privacy-save");
            policy = root.Q<Button>("privacy-policy-link");
            delete = root.Q<Button>("privacy-delete-analytics");
            open.clicked += Show;
            cancel.clicked += Hide;
            save.clicked += Save;
            policy.clicked += OpenPolicy;
            delete.clicked += DeleteAnalytics;
            if (!PrivacyConsentService.HasRecordedChoice) Show();
        }

        private void Show()
        {
            analytics.SetValueWithoutNotify(PrivacyConsentService.AnalyticsAllowed);
            ads.SetValueWithoutNotify(PrivacyConsentService.PersonalizedAdsAllowed);
            feedback.text = PrivacyConsentService.HasRecordedChoice ? "Current choices loaded." : "No choice recorded. Both remain denied.";
            cancel.style.display = PrivacyConsentService.HasRecordedChoice ? DisplayStyle.Flex : DisplayStyle.None;
            screen.RemoveFromClassList("hidden");
        }

        private void Hide() => screen.AddToClassList("hidden");
        private void Save()
        {
            PrivacyConsentService.SaveAndApply(analytics.value, ads.value);
            Hide();
        }
        private static void OpenPolicy() => Application.OpenURL(PrivacyUrl);
        private async void DeleteAnalytics()
        {
            delete.SetEnabled(false);
            feedback.text = "Requesting analytics deletion…";
            try
            {
                await PrivacyConsentService.RequestAnalyticsDataDeletionAsync();
                analytics.SetValueWithoutNotify(false);
                feedback.text = "Analytics disabled. Deletion request submitted.";
            }
            catch (Exception exception)
            {
                feedback.text = "Deletion could not be requested. Try again when online.";
                Debug.LogWarning($"Analytics deletion request failed: {exception.Message}");
            }
            finally { delete.SetEnabled(true); }
        }

        public void Dispose()
        {
            open.clicked -= Show; cancel.clicked -= Hide; save.clicked -= Save;
            policy.clicked -= OpenPolicy; delete.clicked -= DeleteAnalytics;
        }
    }
}
