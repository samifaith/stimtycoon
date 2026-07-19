namespace StimTycoon.Abstractions
{
    /// <summary>
    /// Wraps Unity LevelPlay so gameplay never depends directly on the mediation SDK.
    /// </summary>
    public interface IAdsService
    {
        bool IsRewardedAvailable(string placementId);
        void ShowRewarded(string placementId);
        void ShowInterstitial(string placementId);
    }
}