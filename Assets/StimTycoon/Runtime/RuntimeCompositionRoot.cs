using StimTycoon.Abstractions;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// First-party composition boundary for the game. Vendor systems plug in behind these services later.
    /// </summary>
    public sealed class RuntimeCompositionRoot
    {
        public IEventCatalog EventCatalog { get; }
        public EventRuntimeService EventRuntimeService { get; }
        public IDialogueBridge DialogueBridge { get; }
        public ISaveRepository SaveRepository { get; }
        public IAccountService AccountService { get; }
        public ICloudSaveService CloudSaveService { get; }
        public IAdsService AdsService { get; }

        public RuntimeCompositionRoot(
            IEventCatalog eventCatalog,
            EventRuntimeService eventRuntimeService,
            IDialogueBridge dialogueBridge,
            ISaveRepository saveRepository,
            IAccountService accountService,
            ICloudSaveService cloudSaveService,
            IAdsService adsService)
        {
            EventCatalog = eventCatalog;
            EventRuntimeService = eventRuntimeService;
            DialogueBridge = dialogueBridge;
            SaveRepository = saveRepository;
            AccountService = accountService;
            CloudSaveService = cloudSaveService;
            AdsService = adsService;
        }

        public static RuntimeCompositionRoot CreateDefault()
        {
            var eventCatalog = new InMemoryEventCatalog();
            var eventRuntimeService = new EventRuntimeService(eventCatalog);

#if STIM_EASY_SAVE_3
            ISaveRepository saveRepository = new EasySave3SaveRepository();
#else
            ISaveRepository saveRepository = new NativeSaveRepository();
#endif

            return new RuntimeCompositionRoot(
                eventCatalog,
                eventRuntimeService,
                new DialogueBridge(eventRuntimeService),
                saveRepository,
                new NoOpAccountService(),
                new NoOpCloudSaveService(),
                new NoOpAdsService());
        }
    }

    internal sealed class NoOpSaveRepository : ISaveRepository
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

    internal sealed class NoOpAccountService : IAccountService
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

    internal sealed class NoOpCloudSaveService : ICloudSaveService
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

    internal sealed class NoOpAdsService : IAdsService
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
