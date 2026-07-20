using System.Threading.Tasks;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UnityConsent;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// Owns the persisted Developer Data consent choices independently from any UI.
    /// Missing choices resolve to denied so services never interpret first launch as opt-in.
    /// </summary>
    public static class PrivacyConsentService
    {
        private const string ChoiceRecordedKey = "stim.privacy.choice-recorded.v1";
        private const string AnalyticsAllowedKey = "stim.privacy.analytics-allowed.v1";
        private const string PersonalizedAdsAllowedKey = "stim.privacy.personalized-ads-allowed.v1";

        public static bool HasRecordedChoice => PlayerPrefs.GetInt(ChoiceRecordedKey, 0) == 1;
        public static bool AnalyticsAllowed => ResolveStoredChoice(
            HasRecordedChoice,
            PlayerPrefs.GetInt(AnalyticsAllowedKey, 0) == 1);
        public static bool PersonalizedAdsAllowed => ResolveStoredChoice(
            HasRecordedChoice,
            PlayerPrefs.GetInt(PersonalizedAdsAllowedKey, 0) == 1);

        public static bool ResolveStoredChoice(bool hasRecordedChoice, bool storedValue)
        {
            return hasRecordedChoice && storedValue;
        }

        public static void ApplyStoredConsentBeforeServices()
        {
            ApplyToUnity(AnalyticsAllowed, PersonalizedAdsAllowed);
        }

        public static void SaveAndApply(bool allowAnalytics, bool allowPersonalizedAds)
        {
            PlayerPrefs.SetInt(ChoiceRecordedKey, 1);
            PlayerPrefs.SetInt(AnalyticsAllowedKey, allowAnalytics ? 1 : 0);
            PlayerPrefs.SetInt(PersonalizedAdsAllowedKey, allowPersonalizedAds ? 1 : 0);
            PlayerPrefs.Save();
            ApplyToUnity(allowAnalytics, allowPersonalizedAds);
        }

        public static void DenyAnalytics()
        {
            SaveAndApply(false, PersonalizedAdsAllowed);
        }

        public static void DenyPersonalizedAds()
        {
            SaveAndApply(AnalyticsAllowed, false);
        }

        public static async Task RequestAnalyticsDataDeletionAsync()
        {
            DenyAnalytics();
            await UnityServicesBootstrap.InitializeAsync();
            AnalyticsService.Instance.RequestDataDeletion();
        }

        private static void ApplyToUnity(bool allowAnalytics, bool allowPersonalizedAds)
        {
            var consent = EndUserConsent.GetConsentState();
            consent.AnalyticsIntent = allowAnalytics ? ConsentStatus.Granted : ConsentStatus.Denied;
            consent.AdsIntent = allowPersonalizedAds ? ConsentStatus.Granted : ConsentStatus.Denied;
            EndUserConsent.SetConsentState(consent);
        }
    }
}
