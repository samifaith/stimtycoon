using StimTycoon.Abstractions;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// First-party composition boundary for the game. Vendor systems plug in behind these services later.
    /// </summary>
    public sealed class StimRuntimeCompositionRoot
    {
        public IStimEventCatalog EventCatalog { get; }
        public StimEventRuntimeService EventRuntimeService { get; }
        public IStimDialogueBridge DialogueBridge { get; }
        public IStimSaveRepository SaveRepository { get; }
        public IStimAccountService AccountService { get; }
        public IStimCloudSaveService CloudSaveService { get; }
        public IStimAdsService AdsService { get; }

        public StimRuntimeCompositionRoot(
            IStimEventCatalog eventCatalog,
            StimEventRuntimeService eventRuntimeService,
            IStimDialogueBridge dialogueBridge,
            IStimSaveRepository saveRepository,
            IStimAccountService accountService,
            IStimCloudSaveService cloudSaveService,
            IStimAdsService adsService)
        {
            EventCatalog = eventCatalog;
            EventRuntimeService = eventRuntimeService;
            DialogueBridge = dialogueBridge;
            SaveRepository = saveRepository;
            AccountService = accountService;
            CloudSaveService = cloudSaveService;
            AdsService = adsService;
        }

        public static StimRuntimeCompositionRoot CreateDefault()
        {
            var eventCatalog = new InMemoryStimEventCatalog();
            var eventRuntimeService = new StimEventRuntimeService(eventCatalog);

#if STIM_EASY_SAVE_3
            IStimSaveRepository saveRepository = new EasySave3StimSaveRepository();
#else
            IStimSaveRepository saveRepository = new NoOpSaveRepository();
#endif

            return new StimRuntimeCompositionRoot(
                eventCatalog,
                eventRuntimeService,
                new StimDialogueBridge(eventRuntimeService),
                saveRepository,
                new NoOpAccountService(),
                new NoOpCloudSaveService(),
                new NoOpAdsService());
        }
    }

    internal sealed class NoOpSaveRepository : IStimSaveRepository
    {
        public bool TryCommitAutosave(string serializedSave, out string persistenceSummary)
        {
            persistenceSummary = "Save repository is not yet connected.";
            return false;
        }

        public bool TryLoadLatestSave(out string serializedSave)
        {
            serializedSave = null;
            return false;
        }

        public bool TryValidateSave(string serializedSave, out string validationSummary)
        {
            validationSummary = "Save repository is not yet connected.";
            return false;
        }
    }

    internal sealed class NoOpAccountService : IStimAccountService
    {
        public string PlayerAccountId => null;

        public bool IsAuthenticated => false;

        public void SignInAnonymously()
        {
        }

        public void LinkAppleGameCenter()
        {
        }
    }

    internal sealed class NoOpCloudSaveService : IStimCloudSaveService
    {
        public void QueueSync()
        {
        }

        public bool TryPushPendingSync() => false;

        public bool TryResolveConflict(out string resolutionSummary)
        {
            resolutionSummary = "Cloud save is not yet connected.";
            return false;
        }
    }

    internal sealed class NoOpAdsService : IStimAdsService
    {
        public bool IsRewardedAvailable(string placementId) => false;

        public void ShowRewarded(string placementId)
        {
        }

        public void ShowInterstitial(string placementId)
        {
        }
    }
}
