using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StimTycoon.Events;
using StimTycoon.Saves;
using UnityEngine;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal enum StimDestination
    {
        Life,
        Study,
        Work,
        Bank,
        Social,
        Goals
    }

    internal enum StimBankTab
    {
        Savings,
        Credit,
        Investing
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class StimVerticalSliceController : MonoBehaviour
    {
        [SerializeField, Range(1f, 1.3f)] private float accessibilityTextScale = 1f;
        [SerializeField] private VisualTreeAsset feedRowTemplate;
        [SerializeField] private VisualTreeAsset achievementRowTemplate;
        [SerializeField] private VisualTreeAsset actionCardTemplate;

        private StimGameSessionService gameSession;
        private Label cashValue;
        private Label lifeSummary;
        private Label calendarSummary;
        private Label headerNetWorthValue;
        private Label eventCategory;
        private Label eventTitle;
        private Label eventBody;
        private Label resultText;
        private Label resultEffects;
        private VisualElement lifeFeedList;
        private Label overviewCareer;
        private Label overviewCalendar;
        private Label healthValue;
        private Label happinessValue;
        private Label smartsValue;
        private Label looksValue;
        private Label luckValue;
        private Label careerProgressValue;
        private Label monthlyPaycheckValue;
        private Label annualSalaryValue;
        private Label netWorthValue;
        private Label avatarGlyph;
        private VisualElement choices;
        private VisualElement resultCard;
        private VisualElement playerOverview;
        private VisualElement careerProgressFill;
        private VisualElement eventSheet;
        private VisualElement studySessionSheet;
        private Label studySessionTitle;
        private Label studySessionDescription;
        private Label studySessionEffects;
        private Label studySessionTiming;
        private Label studySessionRequirement;
        private Button studySessionCancel;
        private Button studySessionConfirm;
        private VisualElement healthFill;
        private VisualElement happinessFill;
        private VisualElement smartsFill;
        private VisualElement looksFill;
        private VisualElement luckFill;
        private Button advanceMonth;
        private Button advanceYear;
        private Button toggleOverview;
        private Button eventContinue;
        private string presentedTransitionId;
        private bool presentingFirstLifeOrientation;
        private int queuedYearMonthsRemaining;
        private bool queuedYearCompletionPending;
        private string queuedYearCompletionSummary;
        private VisualElement newLifeSetup;
        private Label newLifeError;
        private Button cancelNewLife;
        private Button continueCurrentLife;
        private Button createNewLife;
        private Button openNewLife;
        private Button focusStudy;
        private Button focusWorkout;
        private Label focusStudyTitle;
        private Label focusStudyEffect;
        private Label focusWorkoutTitle;
        private Label focusWorkoutEffect;
        private VisualElement contextActivities;
        private Label homeCondition;
        private Label homeProgress;
        private VisualElement homeActions;
        private Label homeUpgradeFeedback;
        private ScrollView lifeScroll;
        private ScrollView lifeSummaryView;
        private ScrollView socialView;
        private VisualElement timeDock;
        private Button openLifeSummary;
        private Button closeLifeSummary;
        private Button addCash;
        private Button navLife;
        private Button navEducation;
        private Button navCareer;
        private Button navMoney;
        private Button navSocial;
        private Button navGoals;
        private ScrollView moneyView;
        private ScrollView educationView;
        private ScrollView careerView;
        private ScrollView goalsView;
        private Label summaryStageDetail;
        private Label summaryCalendarDetail;
        private Label summaryCareerDetail;
        private Label summaryHealthValue;
        private Label summaryHappinessValue;
        private Label summarySmartsValue;
        private Label summaryLooksValue;
        private Label summaryLuckValue;
        private VisualElement summaryHealthFill;
        private VisualElement summaryHappinessFill;
        private VisualElement summarySmartsFill;
        private VisualElement summaryLooksFill;
        private VisualElement summaryLuckFill;
        private VisualElement educationDestinationContent;
        private VisualElement careerDestinationContent;
        private VisualElement goalsDestinationContent;
        private VisualElement educationEmptyState;
        private Label educationUnavailableCopy;
        private VisualElement educationCatalog;
        private Label educationCatalogStatus;
        private VisualElement educationCatalogList;
        private VisualElement careerEmptyState;
        private Label careerContextCopy;
        private VisualElement careerPathPreview;
        private Label manualWorkRole;
        private Label manualWorkRate;
        private Label moneyCashValue;
        private Button manualWorkTap;
        private Label manualWorkFeedback;
        private Label savingsBalanceValue;
        private Label savingsAvailableValue;
        private Button savingsDepositMode;
        private Button savingsWithdrawMode;
        private VisualElement savingsAmountInput;
        private Label savingsTransferFeedback;
        private VisualElement moneyTransactionHistory;
        private VisualElement moneyAccountsList;
        private Label cashFlowGross;
        private Label cashFlowTaxes;
        private Label cashFlowExpenses;
        private Label cashFlowCreditInterest;
        private Label cashFlowSavingsInterest;
        private Label cashFlowNet;
        private Label savingsProjection;
        private Label creditBalanceValue;
        private Label creditDetailValue;
        private Label availableCreditValue;
        private VisualElement creditRepaymentInput;
        private Label creditRepaymentFeedback;
        private Label indexFundValue;
        private Label indexFundContributions;
        private Label indexFundPerformance;
        private Label indexInvestmentRequirement;
        private VisualElement indexInvestmentInput;
        private Label indexInvestmentFeedback;
        private Button bankTabSavings;
        private Button bankTabCredit;
        private Button bankTabInvesting;
        private VisualElement bankPanelSavings;
        private VisualElement bankPanelCredit;
        private VisualElement bankPanelInvesting;
        private StimSavingsTransferType savingsTransferType = StimSavingsTransferType.Deposit;
        private StimBankTab selectedBankTab = StimBankTab.Savings;
        private VisualElement relationshipListView;
        private VisualElement relationshipList;
        private Button discoverCompatiblePerson;
        private Label relationshipDiscoveryFeedback;
        private VisualElement relationshipDetailView;
        private Button relationshipBack;
        private Label relationshipAvatar;
        private Label relationshipName;
        private Label relationshipType;
        private Label relationshipStrength;
        private VisualElement relationshipFill;
        private Label relationshipGenetics;
        private VisualElement relationshipActions;
        private string selectedRelationshipId;
        private VisualElement educationCard;
        private Label educationStage;
        private Label learningLevel;
        private VisualElement learningFill;
        private Label learningProgress;
        private VisualElement educationActions;
        private VisualElement skillsList;
        private VisualElement careerCard;
        private Label careerRole;
        private Label careerSalary;
        private Label careerNextStep;
        private VisualElement careerActionFill;
        private Label careerActionProgress;
        private VisualElement careerActions;
        private VisualElement careerActionsCard;
        private VisualElement finalLifeSummary;
        private Label endingName;
        private Label endingStatus;
        private Label endingSummary;
        private Button endingNewLife;
        private Label achievementsCount;
        private VisualElement achievementsList;
        private VisualElement rootElement;
        private StimShellBinder shellBinder;
        private StimActivityType primaryFocusActivity;
        private StimActivityType secondaryFocusActivity;
        private StimEvent currentEvent;
        private StimStudyDifficulty selectedStudyDifficulty;
        private StimActionDefinition selectedStudyDefinition;
        private StimDestination activeDestination = StimDestination.Life;
        private readonly Dictionary<StimDestination, Vector2> destinationScrollOffsets =
            new Dictionary<StimDestination, Vector2>();
        private readonly List<PersistentButtonBinding> persistentButtonBindings =
            new List<PersistentButtonBinding>();

        private readonly struct PersistentButtonBinding
        {
            public readonly Button button;
            public readonly Action callback;

            public PersistentButtonBinding(Button button, Action callback)
            {
                this.button = button;
                this.callback = callback;
            }
        }

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            if (document.panelSettings == null || document.visualTreeAsset == null)
            {
                Debug.LogError(
                    "Stim Vertical Slice requires Panel Settings and Source Asset on its UIDocument. " +
                    "Assign both in the scene so UI Builder and runtime use the same assets.",
                    this);
                return;
            }
            if (feedRowTemplate == null || achievementRowTemplate == null || actionCardTemplate == null)
            {
                Debug.LogError(
                    "Stim Vertical Slice requires the UI Builder-authored Feed Row, Achievement Row, " +
                    "and Action Card templates on its controller.",
                    this);
                return;
            }
            var root = document.rootVisualElement;
            rootElement = root;
            ApplyAccessibilityTextLayout(root, accessibilityTextScale);
            if (!StimUiBindingManifest.TryValidate(root, out var bindingError))
            {
                Debug.LogError($"Vertical slice binding manifest failed. {bindingError}", this);
                rootElement = null;
                return;
            }
            shellBinder = new StimShellBinder(root, HandleRootGeometryChanged);
            cashValue = shellBinder.CashValue;
            lifeSummary = shellBinder.LifeSummary;
            calendarSummary = shellBinder.CalendarSummary;
            headerNetWorthValue = shellBinder.HeaderNetWorthValue;
            eventCategory = root.Q<Label>("event-category");
            eventTitle = root.Q<Label>("event-title");
            eventBody = root.Q<Label>("event-body");
            resultText = root.Q<Label>("result-text");
            resultEffects = root.Q<Label>("result-effects");
            lifeFeedList = root.Q<VisualElement>("life-feed-list");
            overviewCareer = root.Q<Label>("overview-career");
            overviewCalendar = root.Q<Label>("overview-calendar");
            healthValue = root.Q<Label>("health-value");
            happinessValue = root.Q<Label>("happiness-value");
            smartsValue = root.Q<Label>("smarts-value");
            looksValue = root.Q<Label>("looks-value");
            luckValue = root.Q<Label>("luck-value");
            careerProgressValue = shellBinder.CareerProgressValue;
            monthlyPaycheckValue = root.Q<Label>("monthly-paycheck-value");
            annualSalaryValue = root.Q<Label>("annual-salary-value");
            netWorthValue = root.Q<Label>("net-worth-value");
            avatarGlyph = shellBinder.AvatarGlyph;
            choices = root.Q<VisualElement>("choices");
            resultCard = root.Q<VisualElement>("result-card");
            playerOverview = root.Q<VisualElement>("player-overview");
            careerProgressFill = shellBinder.CareerProgressFill;
            eventSheet = root.Q<VisualElement>("event-sheet");
            studySessionSheet = root.Q<VisualElement>("study-session-sheet");
            studySessionTitle = root.Q<Label>("study-session-title");
            studySessionDescription = root.Q<Label>("study-session-description");
            studySessionEffects = root.Q<Label>("study-session-effects");
            studySessionTiming = root.Q<Label>("study-session-timing");
            studySessionRequirement = root.Q<Label>("study-session-requirement");
            studySessionCancel = root.Q<Button>("study-session-cancel");
            studySessionConfirm = root.Q<Button>("study-session-confirm");
            healthFill = root.Q<VisualElement>("health-fill");
            happinessFill = root.Q<VisualElement>("happiness-fill");
            smartsFill = root.Q<VisualElement>("smarts-fill");
            looksFill = root.Q<VisualElement>("looks-fill");
            luckFill = root.Q<VisualElement>("luck-fill");
            newLifeSetup = root.Q<VisualElement>("new-life-setup");
            newLifeError = root.Q<Label>("new-life-error");
            cancelNewLife = root.Q<Button>("cancel-new-life");
            continueCurrentLife = root.Q<Button>("continue-current-life");
            createNewLife = root.Q<Button>("create-new-life");
            openNewLife = root.Q<Button>("open-new-life");
            focusStudy = root.Q<Button>("focus-study");
            focusWorkout = root.Q<Button>("focus-workout");
            focusStudyTitle = root.Q<Label>("focus-study-title");
            focusStudyEffect = root.Q<Label>("focus-study-effect");
            focusWorkoutTitle = root.Q<Label>("focus-workout-title");
            focusWorkoutEffect = root.Q<Label>("focus-workout-effect");
            contextActivities = root.Q<VisualElement>("context-activities");
            homeCondition = root.Q<Label>("home-condition");
            homeProgress = root.Q<Label>("home-progress");
            homeActions = root.Q<VisualElement>("home-actions");
            homeUpgradeFeedback = root.Q<Label>("home-upgrade-feedback");
            lifeScroll = shellBinder.LifeScroll;
            lifeSummaryView = shellBinder.LifeSummaryView;
            socialView = shellBinder.SocialView;
            timeDock = shellBinder.TimeDock;
            openLifeSummary = shellBinder.OpenLifeSummary;
            closeLifeSummary = root.Q<Button>("close-life-summary");
            addCash = shellBinder.AddCash;
            navLife = shellBinder.NavLife;
            navEducation = shellBinder.NavEducation;
            navCareer = shellBinder.NavCareer;
            navMoney = shellBinder.NavMoney;
            navSocial = shellBinder.NavSocial;
            navGoals = shellBinder.NavGoals;
            moneyView = shellBinder.MoneyView;
            educationView = shellBinder.EducationView;
            careerView = shellBinder.CareerView;
            goalsView = shellBinder.GoalsView;
            summaryStageDetail = root.Q<Label>("summary-stage-detail");
            summaryCalendarDetail = root.Q<Label>("summary-calendar-detail");
            summaryCareerDetail = root.Q<Label>("summary-career-detail");
            summaryHealthValue = root.Q<Label>("summary-health-value");
            summaryHappinessValue = root.Q<Label>("summary-happiness-value");
            summarySmartsValue = root.Q<Label>("summary-smarts-value");
            summaryLooksValue = root.Q<Label>("summary-looks-value");
            summaryLuckValue = root.Q<Label>("summary-luck-value");
            summaryHealthFill = root.Q<VisualElement>("summary-health-fill");
            summaryHappinessFill = root.Q<VisualElement>("summary-happiness-fill");
            summarySmartsFill = root.Q<VisualElement>("summary-smarts-fill");
            summaryLooksFill = root.Q<VisualElement>("summary-looks-fill");
            summaryLuckFill = root.Q<VisualElement>("summary-luck-fill");
            educationDestinationContent = root.Q<VisualElement>("education-destination-content");
            careerDestinationContent = root.Q<VisualElement>("career-destination-content");
            goalsDestinationContent = root.Q<VisualElement>("goals-destination-content");
            educationEmptyState = root.Q<VisualElement>("education-empty-state");
            educationUnavailableCopy = root.Q<Label>("education-unavailable-copy");
            educationCatalog = root.Q<VisualElement>("education-catalog");
            educationCatalogStatus = root.Q<Label>("education-catalog-status");
            educationCatalogList = root.Q<VisualElement>("education-catalog-list");
            careerEmptyState = root.Q<VisualElement>("career-empty-state");
            careerContextCopy = root.Q<Label>("career-context-copy");
            careerPathPreview = root.Q<VisualElement>("career-path-preview");
            manualWorkRole = root.Q<Label>("manual-work-role");
            manualWorkRate = root.Q<Label>("manual-work-rate");
            moneyCashValue = root.Q<Label>("money-cash-value");
            manualWorkTap = root.Q<Button>("manual-work-tap");
            manualWorkFeedback = root.Q<Label>("manual-work-feedback");
            savingsBalanceValue = root.Q<Label>("savings-balance-value");
            savingsAvailableValue = root.Q<Label>("savings-available-value");
            savingsDepositMode = root.Q<Button>("savings-deposit-mode");
            savingsWithdrawMode = root.Q<Button>("savings-withdraw-mode");
            savingsAmountInput = root.Q<VisualElement>("savings-amount-input");
            savingsTransferFeedback = root.Q<Label>("savings-transfer-feedback");
            moneyTransactionHistory = root.Q<VisualElement>("money-transaction-history");
            moneyAccountsList = root.Q<VisualElement>("money-accounts-list");
            cashFlowGross = root.Q<Label>("cash-flow-gross");
            cashFlowTaxes = root.Q<Label>("cash-flow-taxes");
            cashFlowExpenses = root.Q<Label>("cash-flow-expenses");
            cashFlowCreditInterest = root.Q<Label>("cash-flow-credit-interest");
            cashFlowSavingsInterest = root.Q<Label>("cash-flow-savings-interest");
            cashFlowNet = root.Q<Label>("cash-flow-net");
            savingsProjection = root.Q<Label>("savings-projection");
            creditBalanceValue = root.Q<Label>("credit-balance-value");
            creditDetailValue = root.Q<Label>("credit-detail-value");
            availableCreditValue = root.Q<Label>("available-credit-value");
            creditRepaymentInput = root.Q<VisualElement>("credit-repayment-input");
            creditRepaymentFeedback = root.Q<Label>("credit-repayment-feedback");
            indexFundValue = root.Q<Label>("index-fund-value");
            indexFundContributions = root.Q<Label>("index-fund-contributions");
            indexFundPerformance = root.Q<Label>("index-fund-performance");
            indexInvestmentRequirement = root.Q<Label>("index-investment-requirement");
            indexInvestmentInput = root.Q<VisualElement>("index-investment-input");
            indexInvestmentFeedback = root.Q<Label>("index-investment-feedback");
            bankTabSavings = root.Q<Button>("bank-tab-savings");
            bankTabCredit = root.Q<Button>("bank-tab-credit");
            bankTabInvesting = root.Q<Button>("bank-tab-investing");
            bankPanelSavings = root.Q<VisualElement>("bank-panel-savings");
            bankPanelCredit = root.Q<VisualElement>("bank-panel-credit");
            bankPanelInvesting = root.Q<VisualElement>("bank-panel-investing");
            relationshipListView = root.Q<VisualElement>("relationship-list-view");
            relationshipList = root.Q<VisualElement>("relationship-list");
            discoverCompatiblePerson = root.Q<Button>("discover-compatible-person");
            relationshipDiscoveryFeedback = root.Q<Label>("relationship-discovery-feedback");
            relationshipDetailView = root.Q<VisualElement>("relationship-detail-view");
            relationshipBack = root.Q<Button>("relationship-back");
            relationshipAvatar = root.Q<Label>("relationship-avatar");
            relationshipName = root.Q<Label>("relationship-name");
            relationshipType = root.Q<Label>("relationship-type");
            relationshipStrength = root.Q<Label>("relationship-strength");
            relationshipFill = root.Q<VisualElement>("relationship-fill");
            relationshipGenetics = root.Q<Label>("relationship-genetics");
            relationshipActions = root.Q<VisualElement>("relationship-actions");
            educationCard = root.Q<VisualElement>("education-card");
            educationStage = root.Q<Label>("education-stage");
            learningLevel = root.Q<Label>("learning-level");
            learningFill = root.Q<VisualElement>("learning-fill");
            learningProgress = root.Q<Label>("learning-progress");
            educationActions = root.Q<VisualElement>("education-actions");
            skillsList = root.Q<VisualElement>("skills-list");
            careerCard = root.Q<VisualElement>("career-card");
            careerRole = root.Q<Label>("career-role");
            careerSalary = root.Q<Label>("career-salary");
            careerNextStep = root.Q<Label>("career-next-step");
            careerActionFill = root.Q<VisualElement>("career-action-fill");
            careerActionProgress = root.Q<Label>("career-action-progress");
            careerActions = root.Q<VisualElement>("career-actions");
            careerActionsCard = root.Q<VisualElement>("career-actions-card");
            finalLifeSummary = root.Q<VisualElement>("final-life-summary");
            endingName = root.Q<Label>("ending-name");
            endingStatus = root.Q<Label>("ending-status");
            endingSummary = root.Q<Label>("ending-summary");
            endingNewLife = root.Q<Button>("ending-new-life");
            achievementsCount = root.Q<Label>("achievements-count");
            achievementsList = root.Q<VisualElement>("achievements-list");

            var catalog = new InMemoryStimEventCatalog();
            foreach (var authoredEvent in RepresentativeStimEvents.CreateLaunchAlphaCatalog())
            {
                catalog.Upsert(authoredEvent);
            }
            gameSession = new StimGameSessionService(catalog, new NativeStimSaveRepository());
            var loadedExistingLife = gameSession.TryLoadLatest(out _);

            advanceMonth = shellBinder.AdvanceMonth;
            advanceYear = shellBinder.AdvanceYear;
            toggleOverview = root.Q<Button>("toggle-overview");
            eventContinue = root.Q<Button>("event-continue");
            if (cashValue == null || lifeSummary == null || eventCategory == null || eventTitle == null ||
                eventBody == null || resultText == null || resultEffects == null || lifeFeedList == null || choices == null ||
                resultCard == null || advanceMonth == null || advanceYear == null || toggleOverview == null || playerOverview == null ||
                overviewCareer == null || overviewCalendar == null || healthValue == null ||
                happinessValue == null || smartsValue == null || looksValue == null || luckValue == null ||
                careerProgressValue == null ||
                careerProgressFill == null || monthlyPaycheckValue == null || annualSalaryValue == null ||
                netWorthValue == null || avatarGlyph == null || eventSheet == null || eventContinue == null ||
                studySessionSheet == null || studySessionTitle == null || studySessionDescription == null ||
                studySessionEffects == null || studySessionTiming == null || studySessionRequirement == null ||
                studySessionCancel == null || studySessionConfirm == null ||
                healthFill == null || happinessFill == null || smartsFill == null || looksFill == null ||
                luckFill == null || newLifeSetup == null || newLifeError == null ||
                cancelNewLife == null || continueCurrentLife == null || createNewLife == null || openNewLife == null ||
                focusStudy == null || focusWorkout == null || focusStudyTitle == null || focusStudyEffect == null ||
                focusWorkoutTitle == null || focusWorkoutEffect == null || lifeScroll == null || lifeSummaryView == null ||
                openLifeSummary == null || closeLifeSummary == null || addCash == null || socialView == null ||
                contextActivities == null || homeCondition == null || homeProgress == null ||
                homeActions == null || homeUpgradeFeedback == null ||
                timeDock == null || navLife == null || navEducation == null || navCareer == null ||
                navMoney == null || navSocial == null || navGoals == null || moneyView == null ||
                educationView == null || careerView == null || goalsView == null ||
                summaryStageDetail == null || summaryCalendarDetail == null || summaryCareerDetail == null ||
                summaryHealthValue == null || summaryHappinessValue == null || summarySmartsValue == null ||
                summaryLooksValue == null || summaryLuckValue == null || summaryHealthFill == null ||
                summaryHappinessFill == null || summarySmartsFill == null || summaryLooksFill == null ||
                summaryLuckFill == null ||
                educationDestinationContent == null || careerDestinationContent == null ||
                goalsDestinationContent == null || educationEmptyState == null || careerEmptyState == null ||
                educationUnavailableCopy == null ||
                educationCatalog == null || educationCatalogStatus == null || educationCatalogList == null ||
                careerContextCopy == null || careerPathPreview == null ||
                manualWorkRole == null || manualWorkRate == null || moneyCashValue == null ||
                manualWorkTap == null || manualWorkFeedback == null || savingsBalanceValue == null ||
                savingsAvailableValue == null || savingsDepositMode == null || savingsWithdrawMode == null ||
                savingsAmountInput == null || savingsTransferFeedback == null || moneyTransactionHistory == null ||
                moneyAccountsList == null ||
                cashFlowGross == null || cashFlowTaxes == null || cashFlowExpenses == null ||
                cashFlowCreditInterest == null || cashFlowSavingsInterest == null || cashFlowNet == null ||
                savingsProjection == null ||
                creditBalanceValue == null || creditDetailValue == null || availableCreditValue == null ||
                creditRepaymentInput == null || creditRepaymentFeedback == null ||
                indexFundValue == null || indexFundContributions == null || indexFundPerformance == null ||
                indexInvestmentRequirement == null ||
                indexInvestmentInput == null || indexInvestmentFeedback == null ||
                bankTabSavings == null || bankTabCredit == null || bankTabInvesting == null ||
                bankPanelSavings == null || bankPanelCredit == null || bankPanelInvesting == null ||
                relationshipListView == null ||
                relationshipList == null || discoverCompatiblePerson == null || relationshipDiscoveryFeedback == null ||
                relationshipDetailView == null || relationshipBack == null ||
                relationshipAvatar == null || relationshipName == null || relationshipType == null ||
                relationshipStrength == null || relationshipFill == null || relationshipGenetics == null ||
                relationshipActions == null || educationCard == null || educationStage == null ||
                learningLevel == null || learningFill == null || learningProgress == null ||
                educationActions == null || skillsList == null || careerCard == null || careerRole == null || careerSalary == null ||
                careerNextStep == null || careerActionFill == null || careerActionProgress == null ||
                careerActions == null || careerActionsCard == null || finalLifeSummary == null || endingName == null || endingStatus == null ||
                endingSummary == null || endingNewLife == null || achievementsCount == null ||
                achievementsList == null)
            {
                LogMissingUiBindings();
                return;
            }

            PopulateVisualPlaceholders(root);
            NavigateTo(StimDestination.Life);

            BindPersistentCallbacks();

            if (!loadedExistingLife)
            {
                ShowNewLifeSetup(false, false);
                return;
            }

            if (!string.IsNullOrEmpty(gameSession.ActiveSave.state.pendingEventId))
            {
                gameSession.TryGetPendingEvent(out currentEvent);
            }
            else if (gameSession.ActiveSave.state.character.age >= 18 &&
                     !string.IsNullOrEmpty(gameSession.ActiveSave.state.career.roleTitle) &&
                     gameSession.ActiveSave.state.career.roleTitle != "Retired" &&
                     gameSession.ActiveSave.state.eventHistory.Count == 0)
            {
                catalog.TryGetEvent(RepresentativeStimEvents.SalaryNegotiationId, out currentEvent);
            }

            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            if (gameSession.ActiveSave.state.character.lifeStatus != "active")
            {
                ShowFinalLifeSummary();
                return;
            }
            if (currentEvent != null)
            {
                PresentEvent(currentEvent);
            }
            else if (gameSession.HasPendingEvent)
            {
                PresentPendingEventIfAvailable();
            }
            else if (gameSession.GetPendingTransition() != null)
            {
                PresentPendingTransition();
            }
            else if (gameSession.ShouldPresentFirstLifeOrientation())
            {
                PresentFirstLifeOrientation();
            }
            else
            {
                choices.AddToClassList("hidden");
                advanceMonth.RemoveFromClassList("hidden");
                eventSheet.AddToClassList("hidden");
            }

            ShowNewLifeSetup(true, false);
        }

        private static void PopulateVisualPlaceholders(VisualElement root)
        {
            AddVisualPlaceholder(root, "event-visual-slot", new StimVisualPlaceholderDefinition
            {
                visualId = "event.generic.hero", role = StimVisualRole.Hero, aspectRatio = "16:9",
                accessibilityLabelKey = "visual.event.generic", fallbackGlyph = "✨", themeToken = "event"
            });
            AddVisualPlaceholder(root, "home-visual-slot", new StimVisualPlaceholderDefinition
            {
                visualId = "home.starter.thumbnail", role = StimVisualRole.Thumbnail, aspectRatio = "4:3",
                accessibilityLabelKey = "visual.home.starter", fallbackGlyph = "🏠", themeToken = "home"
            });
            AddVisualPlaceholder(root, "education-visual-slot", new StimVisualPlaceholderDefinition
            {
                visualId = "education.current.thumbnail", role = StimVisualRole.Thumbnail, aspectRatio = "4:3",
                accessibilityLabelKey = "visual.education.current", fallbackGlyph = "🎒", themeToken = "education"
            });
            AddVisualPlaceholder(root, "relationship-visual-slot", new StimVisualPlaceholderDefinition
            {
                visualId = "relationship.selected.avatar", role = StimVisualRole.Avatar, aspectRatio = "1:1",
                accessibilityLabelKey = "visual.relationship.selected", fallbackGlyph = "👤", themeToken = "social"
            });
        }

        private static void AddVisualPlaceholder(
            VisualElement root,
            string slotName,
            StimVisualPlaceholderDefinition definition)
        {
            var slot = root.Q<VisualElement>(slotName);
            if (slot == null) return;
            if (slot.childCount > 0) return;
            slot.Add(StimVisualPlaceholderFactory.Create(definition));
        }

        private void OnDisable()
        {
            UnbindPersistentCallbacks();
            shellBinder?.Dispose();
            shellBinder = null;
            rootElement = null;
        }

        private void BindPersistentCallbacks()
        {
            // Defensive teardown keeps this method idempotent if lifecycle order changes.
            UnbindPersistentCallbacks();
            shellBinder?.BindActions(
                AdvanceMonth,
                AdvanceYear,
                ShowLifeSummary,
                ShowMoneyDestination,
                ShowLifeDestination,
                ShowEducationDestination,
                ShowCareerDestination,
                ShowSocialDestination,
                ShowGoalsDestination);
            BindPersistentButton(toggleOverview, ToggleOverview);
            BindPersistentButton(eventContinue, CloseEventSheet);
            BindPersistentButton(studySessionCancel, CloseStudySessionSheet);
            BindPersistentButton(studySessionConfirm, ConfirmSelectedStudySession);
            BindPersistentButton(focusStudy, PerformPrimaryFocusActivity);
            BindPersistentButton(focusWorkout, PerformSecondaryFocusActivity);
            BindPersistentButton(closeLifeSummary, CloseLifeSummary);
            BindPersistentButton(manualWorkTap, PerformManualWorkTap);
            BindPersistentButton(savingsDepositMode, SelectSavingsDeposit);
            BindPersistentButton(savingsWithdrawMode, SelectSavingsWithdrawal);
            BindPersistentButton(bankTabSavings, SelectSavingsBankTab);
            BindPersistentButton(bankTabCredit, SelectCreditBankTab);
            BindPersistentButton(bankTabInvesting, SelectInvestingBankTab);
            BindPersistentButton(relationshipBack, ShowRelationshipList);
            BindPersistentButton(discoverCompatiblePerson, DiscoverCompatiblePerson);
            BindPersistentButton(endingNewLife, OpenNewLifeFromEnding);
            BindPersistentButton(openNewLife, OpenNewLifeSetup);
            BindPersistentButton(cancelNewLife, HideNewLifeSetup);
            BindPersistentButton(continueCurrentLife, HideNewLifeSetup);
            BindPersistentButton(createNewLife, CreateLifeFromSetup);
        }

        private void BindPersistentButton(Button button, Action callback)
        {
            if (button == null || callback == null) return;
            button.clicked -= callback;
            button.clicked += callback;
            persistentButtonBindings.Add(new PersistentButtonBinding(button, callback));
        }

        private void UnbindPersistentCallbacks()
        {
            foreach (var binding in persistentButtonBindings)
            {
                if (binding.button != null) binding.button.clicked -= binding.callback;
            }
            persistentButtonBindings.Clear();
        }

        private void PerformPrimaryFocusActivity()
        {
            PerformActivity(primaryFocusActivity);
        }

        private void PerformSecondaryFocusActivity()
        {
            PerformActivity(secondaryFocusActivity);
        }

        private void SelectSavingsDeposit()
        {
            SetSavingsTransferType(StimSavingsTransferType.Deposit);
        }

        private void SelectSavingsWithdrawal()
        {
            SetSavingsTransferType(StimSavingsTransferType.Withdrawal);
        }

        private void SelectSavingsBankTab()
        {
            SetBankTab(StimBankTab.Savings);
        }

        private void SelectCreditBankTab()
        {
            SetBankTab(StimBankTab.Credit);
        }

        private void SelectInvestingBankTab()
        {
            SetBankTab(StimBankTab.Investing);
        }

        private void OpenNewLifeSetup()
        {
            ShowNewLifeSetup(false, true);
        }

        private void HideNewLifeSetup()
        {
            newLifeSetup?.AddToClassList("hidden");
        }

        private void HandleRootGeometryChanged(GeometryChangedEvent evt)
        {
            ApplyResponsiveLayout(rootElement, evt.newRect.width);
            ApplySafeAreaLayout(
                rootElement?.Q<VisualElement>("screen"),
                CalculateSafeAreaInsets(
                    Screen.width,
                    Screen.height,
                    Screen.safeArea,
                    evt.newRect.width,
                    evt.newRect.height));
        }

        public static void ApplyResponsiveLayout(VisualElement root, float width)
        {
            if (root == null) return;
            root.EnableInClassList("st-compact-width", width > 0f && width <= 360f);
        }

        public static void ApplyAccessibilityTextLayout(VisualElement root, float textScale)
        {
            if (root == null) return;
            root.EnableInClassList("st-large-text", textScale >= 1.3f);
        }

        public static Vector4 CalculateSafeAreaInsets(
            float screenWidth,
            float screenHeight,
            Rect safeArea,
            float panelWidth,
            float panelHeight)
        {
            if (screenWidth <= 0f || screenHeight <= 0f || panelWidth <= 0f || panelHeight <= 0f)
            {
                return Vector4.zero;
            }

            var scaleX = panelWidth / screenWidth;
            var scaleY = panelHeight / screenHeight;
            return new Vector4(
                Mathf.Max(0f, safeArea.xMin) * scaleX,
                Mathf.Max(0f, screenHeight - safeArea.yMax) * scaleY,
                Mathf.Max(0f, screenWidth - safeArea.xMax) * scaleX,
                Mathf.Max(0f, safeArea.yMin) * scaleY);
        }

        public static void ApplySafeAreaLayout(VisualElement element, Vector4 insets)
        {
            if (element == null) return;
            element.style.paddingLeft = insets.x;
            element.style.paddingTop = insets.y;
            element.style.paddingRight = insets.z;
            element.style.paddingBottom = insets.w;
        }

        public void SetAccessibilityTextScale(float textScale)
        {
            accessibilityTextScale = Mathf.Clamp(textScale, 1f, 1.3f);
            ApplyAccessibilityTextLayout(rootElement, accessibilityTextScale);
        }

        private void Resolve(string choiceId, StimPaymentMethod paymentMethod = StimPaymentMethod.Cash)
        {
            if (currentEvent == null)
            {
                return;
            }

            if (!gameSession.TryResolveChoice(
                    currentEvent.id,
                    choiceId,
                    paymentMethod,
                    out var summary))
            {
                resultText.text = summary;
                resultEffects.text = "No changes applied";
                resultEffects.RemoveFromClassList("hidden");
                resultCard.RemoveFromClassList("hidden");
                return;
            }

            resultText.text = summary;
            resultEffects.text = BuildEffectSummary(gameSession.LastResolution.outcome.effects);
            resultEffects.RemoveFromClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            choices.AddToClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            currentEvent = null;
            RefreshHeader();
            RefreshFeed();
            PresentPendingTransition();
        }

        private void AdvanceMonth()
        {
            if (PresentPendingEventIfAvailable()) return;
            var cashBefore = gameSession.ActiveSave.state.finances.cashMinorUnits;
            var ageBefore = gameSession.ActiveSave.state.character.age;
            var happinessBefore = gameSession.ActiveSave.state.character.happiness;
            var careerProgressBefore = gameSession.ActiveSave.state.career.careerProgress;
            if (!gameSession.TryAdvanceMonth(out var nextEvent, out var summary))
            {
                resultText.text = summary;
                resultEffects.text = "No changes applied";
                resultEffects.RemoveFromClassList("hidden");
                resultCard.RemoveFromClassList("hidden");
                return;
            }

            currentEvent = nextEvent;
            resultText.text = summary;
            var cashDelta = gameSession.ActiveSave.state.finances.cashMinorUnits - cashBefore;
            var careerDelta = gameSession.ActiveSave.state.career.careerProgress - careerProgressBefore;
            var ageDelta = gameSession.ActiveSave.state.character.age - ageBefore;
            var happinessDelta = gameSession.ActiveSave.state.character.happiness - happinessBefore;
            resultEffects.text = BuildMonthlyEffectSummary(cashDelta, careerDelta, happinessDelta, ageDelta);
            resultEffects.RemoveFromClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            RefreshHeader();
            RefreshFeed();

            if (gameSession.ActiveSave.state.character.lifeStatus != "active")
            {
                ShowFinalLifeSummary();
                return;
            }

            if (currentEvent == null)
            {
                if (gameSession.GetPendingTransition() != null)
                {
                    PresentPendingTransition();
                    return;
                }
                choices.AddToClassList("hidden");
                eventCategory.text = "MONTHLY SUMMARY";
                eventTitle.text = $"Month {gameSession.ActiveSave.state.calendar.monthOfYear} complete";
                eventBody.text = string.IsNullOrEmpty(gameSession.ActiveSave.state.career.roleTitle)
                    ? "Time moved forward and this month's life changes were applied."
                    : "Your income, expenses, and monthly stat changes were applied.";
                eventSheet.RemoveFromClassList("hidden");
                eventContinue.RemoveFromClassList("hidden");
                return;
            }

            PresentEvent(currentEvent);
        }

        private void AdvanceYear()
        {
            if (PresentPendingEventIfAvailable()) return;
            queuedYearMonthsRemaining = 12;
            queuedYearCompletionPending = false;
            queuedYearCompletionSummary = string.Empty;
            ContinueQueuedYearAdvance();
        }

        private void ContinueQueuedYearAdvance()
        {
            if (queuedYearMonthsRemaining <= 0) return;
            var requestedMonths = queuedYearMonthsRemaining;
            if (!gameSession.TryAdvanceMonths(
                    requestedMonths, out var monthsProcessed, out var nextEvent, out var summary))
            {
                resultText.text = summary;
                resultEffects.text = "No changes applied";
                resultEffects.RemoveFromClassList("hidden");
                resultCard.RemoveFromClassList("hidden");
                eventSheet.RemoveFromClassList("hidden");
                eventContinue.RemoveFromClassList("hidden");
                return;
            }

            queuedYearMonthsRemaining = Math.Max(0, queuedYearMonthsRemaining - monthsProcessed);
            currentEvent = nextEvent;
            resultText.text = summary;
            var totalCommitted = 12 - queuedYearMonthsRemaining;
            resultEffects.text = $"{totalCommitted} of 12 monthly transactions committed";
            resultEffects.RemoveFromClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            RefreshHeader();
            RefreshFeed();

            if (gameSession.ActiveSave.state.character.lifeStatus != "active")
            {
                queuedYearMonthsRemaining = 0;
                ShowFinalLifeSummary();
                return;
            }
            if (currentEvent != null)
            {
                if (queuedYearMonthsRemaining == 0)
                {
                    queuedYearCompletionPending = true;
                    queuedYearCompletionSummary = summary;
                }
                PresentEvent(currentEvent);
                return;
            }
            if (gameSession.GetPendingTransition() != null)
            {
                if (queuedYearMonthsRemaining == 0)
                {
                    queuedYearCompletionPending = true;
                    queuedYearCompletionSummary = summary;
                }
                PresentPendingTransition();
                return;
            }

            choices.AddToClassList("hidden");
            eventCategory.text = queuedYearMonthsRemaining == 0 ? "YEAR SUMMARY" : "TIME PAUSED";
            eventTitle.text = queuedYearMonthsRemaining == 0
                ? "A full year moved forward"
                : $"Year advance paused · {queuedYearMonthsRemaining} month{(queuedYearMonthsRemaining == 1 ? string.Empty : "s")} remaining";
            eventBody.text = queuedYearMonthsRemaining == 0
                ? "Every normal monthly change was processed and autosaved in sequence."
                : "Resolve the required choice, then Continue. The remaining months will resume automatically.";
            eventSheet.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
        }

        private void PresentEvent(StimEvent evt)
        {
            eventCategory.text = $"{evt.category.ToString().ToUpperInvariant()} EVENT";
            eventTitle.text = evt.titleKey;
            eventBody.text = evt.id == RepresentativeStimEvents.YearInReviewId
                ? StimGameSessionService.BuildAnnualReviewSummary(gameSession.ActiveSave.state)
                : evt.bodyKey;
            choices.Clear();
            for (var index = 0; index < evt.choices.Count; index++)
            {
                var choice = evt.choices[index];
                var potentialCost = StimGameSessionService.CalculateChoicePotentialCost(
                    evt, choice, gameSession.ActiveSave.state);
                AddEventChoiceButton(choice, index, StimPaymentMethod.Cash,
                    potentialCost > 0 ? $" · Pay cash (up to {FormatMoney(potentialCost)})" : string.Empty);
                if (potentialCost > 0 && gameSession.ActiveSave.state.character.age >= 18)
                {
                    AddEventChoiceButton(choice, index + 1, StimPaymentMethod.Credit,
                        $" · Use credit (up to {FormatMoney(potentialCost)})");
                }
            }
            choices.RemoveFromClassList("hidden");
            resultCard.AddToClassList("hidden");
            eventContinue.AddToClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
        }

        private void AddEventChoiceButton(
            Choice choice,
            int visualIndex,
            StimPaymentMethod paymentMethod,
            string paymentLabel)
        {
            var button = new Button
            {
                name = $"choice-{choice.id}-{paymentMethod.ToString().ToLowerInvariant()}"
            };
            button.AddToClassList("choice-button");
            if (visualIndex > 0) button.AddToClassList("secondary");
            var title = new Label(choice.labelKey + paymentLabel);
            title.AddToClassList("choice-title");
            button.Add(title);
            var choiceId = choice.id;
            button.clicked += () => Resolve(choiceId, paymentMethod);
            choices.Add(button);
        }

        private void RefreshHeader()
        {
            var state = gameSession.ActiveSave.state;
            var career = state.career;
            var netWorth = state.finances.cashMinorUnits + state.finances.savingsMinorUnits +
                           state.finances.indexFundMinorUnits - state.finances.debtMinorUnits;
            cashValue.text = FormatCompactMoney(state.finances.cashMinorUnits);
            netWorthValue.text = FormatMoney(netWorth);
            headerNetWorthValue.text = $"Net {FormatCompactMoney(netWorth)}";
            netWorthValue.tooltip = $"Total net worth {FormatMoney(netWorth)}";
            moneyCashValue.tooltip = $"Current cash {FormatMoney(state.finances.cashMinorUnits)}";
            lifeSummary.text = string.IsNullOrEmpty(state.character.firstName)
                ? $"Age {state.character.age}"
                : $"{state.character.firstName} · {state.character.age}";
            lifeSummary.tooltip = $"{lifeSummary.text}. {ToDisplayName(state.character.lifeStage)} life stage.";
            calendarSummary.text = $"{ToDisplayName(state.character.lifeStage)} · Month {state.calendar.monthOfYear}";
            calendarSummary.tooltip = $"{ToDisplayName(state.character.lifeStage)}. Month {state.calendar.monthOfYear} of 12.";
            cashValue.tooltip = $"Current cash {FormatMoney(state.finances.cashMinorUnits)}";
            headerNetWorthValue.tooltip = $"Net worth {FormatMoney(netWorth)}";
            avatarGlyph.text = state.character.age < 6 ? "👶" : state.character.age < 18 ? "🧒" :
                state.character.age < 65 ? "🧑" : "🧓";
            RefreshAgeProgress(state.character.age);
            overviewCareer.text = string.IsNullOrEmpty(career.roleTitle)
                ? ToDisplayName(state.character.lifeStage)
                : $"{career.roleTitle} · Stim Financial Group";
            overviewCalendar.text = $"Age {state.character.age} · Month {state.calendar.monthOfYear} of 12";
            summaryStageDetail.text = ToDisplayName(state.character.lifeStage);
            summaryCalendarDetail.text = $"Age {state.character.age} · Month {state.calendar.monthOfYear} of 12";
            summaryCareerDetail.text = string.IsNullOrEmpty(career.roleTitle)
                ? "Not started"
                : $"{career.roleTitle} · {FormatMoney(career.annualSalaryMinorUnits)} gross";
            healthValue.text = $"{state.character.health} / 100";
            happinessValue.text = $"{state.character.happiness} / 100";
            smartsValue.text = $"{state.character.smarts} / 100";
            looksValue.text = $"{state.character.looks} / 100";
            luckValue.text = $"{state.character.luck} / 100";
            summaryHealthValue.text = healthValue.text;
            summaryHappinessValue.text = happinessValue.text;
            summarySmartsValue.text = smartsValue.text;
            summaryLooksValue.text = looksValue.text;
            summaryLuckValue.text = luckValue.text;
            healthFill.style.width = Length.Percent(ClampFillPercent(state.character.health));
            happinessFill.style.width = Length.Percent(ClampFillPercent(state.character.happiness));
            smartsFill.style.width = Length.Percent(ClampFillPercent(state.character.smarts));
            looksFill.style.width = Length.Percent(ClampFillPercent(state.character.looks));
            luckFill.style.width = Length.Percent(ClampFillPercent(state.character.luck));
            summaryHealthFill.style.width = healthFill.style.width;
            summaryHappinessFill.style.width = happinessFill.style.width;
            summarySmartsFill.style.width = smartsFill.style.width;
            summaryLooksFill.style.width = looksFill.style.width;
            summaryLuckFill.style.width = luckFill.style.width;
            careerProgressValue.text = $"{career.careerProgress} / 100";
            careerProgressFill.style.width = Length.Percent(ClampFillPercent(career.careerProgress));
            var grossMonthlyPay = career.annualSalaryMinorUnits / 12;
            var estimatedTaxes = (long)Math.Round(
                grossMonthlyPay * (state.finances.taxRateBasisPoints / 10000m),
                MidpointRounding.AwayFromZero);
            var estimatedNet = grossMonthlyPay - estimatedTaxes - state.finances.monthlyLivingExpensesMinorUnits;
            monthlyPaycheckValue.text = FormatSignedMoney(estimatedNet);
            annualSalaryValue.text = $"{FormatMoney(career.annualSalaryMinorUnits)} gross · {state.finances.taxRateBasisPoints / 100m:0.#}% tax";
            ConfigureAgeAppropriateActivities(state.character.age);
            RefreshEducation();
            RefreshSkills();
            RefreshCareer();
            RefreshAchievements();
            RefreshHome();
            RefreshMoney();
        }

        private void RefreshAgeProgress(int age)
        {
            if (rootElement == null) return;
            var activeIndex = age <= 12 ? 0 : age <= 18 ? 1 : age <= 26 ? 2 : 3;
            var summary = rootElement.Q<Label>("age-stage-summary");
            if (summary != null)
                summary.text = activeIndex == 0 ? "Childhood" : activeIndex == 1 ? "Teen" :
                    activeIndex == 2 ? "Young Adult" : "Adult";
            for (var index = 0; index < 4; index++)
            {
                var node = rootElement.Q<VisualElement>($"age-stage-{index}");
                node?.EnableInClassList("complete", index < activeIndex);
                node?.EnableInClassList("active", index == activeIndex);
                node?.EnableInClassList("locked", index > activeIndex);
            }
        }

        private void RefreshHome()
        {
            var state = gameSession.ActiveSave.state;
            var home = state.home ?? new StimHomeState();
            var requiredProgress = StimGameSessionService.GetHomeUpgradeRequiredProgress(home.upgradeLevel);
            var definition = StimHomeContentCatalog.Get(home.homeId) ??
                             StimHomeContentCatalog.Get("starter_home");
            homeCondition.text = $"{definition.displayName} · Condition {home.condition} / 100";
            homeProgress.text = home.upgradeLevel >= 3
                ? $"Level 3 · Fully upgraded · Reading stock {home.readingMaterialStock}/{home.readingMaterialCapacity} · Equipment {home.trainingEquipmentCondition}%"
                : $"Level {home.upgradeLevel} · Improvement {home.improvementProgress}/{requiredProgress} · Reading stock {home.readingMaterialStock}/{home.readingMaterialCapacity} · Equipment {home.trainingEquipmentCondition}%";
            homeActions.Clear();
            foreach (var action in definition.actions)
                AddHomeAction(action);

            if (home.upgradeLevel < 3 && state.character.age >= 18)
            {
                var cost = StimGameSessionService.GetHomeUpgradeCost(home.upgradeLevel);
                var button = new Button
                {
                    name = "home-upgrade",
                    text = $"UPGRADE TO LEVEL {home.upgradeLevel + 1}\n{FormatMoney(cost)} · Requires {requiredProgress} progress · Improves home benefits"
                };
                button.AddToClassList("st-home-action");
                button.AddToClassList("st-home-upgrade");
                button.SetEnabled(home.improvementProgress >= requiredProgress && state.finances.cashMinorUnits >= cost);
                button.clicked += PerformHomeUpgrade;
                homeActions.Add(button);
            }
        }

        private void AddHomeAction(StimHomeActionDefinition definition)
        {
            var state = gameSession.ActiveSave.state;
            var actionType = definition.actionType;
            var caregiverHandlesMaintenance =
                actionType == StimHomeActionType.Maintain && state.character.age < 18;
            var cost = caregiverHandlesMaintenance ? 0 : definition.costMinorUnits;
            var cooldownId = $"home_{actionType.ToString().ToLowerInvariant()}_used";
            var coolingDown = state.statuses.Exists(status => status.statusId == cooldownId);
            var hasCapacity = actionType != StimHomeActionType.Read || state.home.readingMaterialStock > 0;
            hasCapacity &= actionType != StimHomeActionType.Train || state.home.trainingEquipmentCondition >= 10;
            var button = new Button
            {
                name = $"home-action-{actionType.ToString().ToLowerInvariant()}",
                text = $"{(caregiverHandlesMaintenance ? "Ask caregiver to maintain" : definition.displayName)}\n" +
                       $"{(cost == 0 ? "Free" : FormatMoney(cost))} · {definition.benefitPreview}" +
                       $" · {ToDisplayName(definition.roomObjectId)}" +
                       (coolingDown ? "\nAvailable next month" : string.Empty)
            };
            button.AddToClassList("st-home-action");
            button.SetEnabled(!coolingDown && hasCapacity && state.finances.cashMinorUnits >= cost);
            var capturedAction = actionType;
            button.clicked += () => PerformHomeAction(capturedAction);
            homeActions.Add(button);
        }

        private void PerformHomeAction(StimHomeActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformHomeAction(actionType, out var summary);
            homeUpgradeFeedback.text = summary;
            homeUpgradeFeedback.EnableInClassList("is-error", !succeeded);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
        }

        private void PerformHomeUpgrade()
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryUpgradeHome(out var summary);
            homeUpgradeFeedback.text = summary;
            homeUpgradeFeedback.EnableInClassList("is-error", !succeeded);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
        }

        private void RefreshSkills()
        {
            skillsList.Clear();
            AddSkillProgress("fitness", "Fitness", "Reduces overtime strain from demanding work.");
            AddSkillProgress("professional", "Professional", "Improves progress earned from focused career work.");
        }

        private void AddSkillProgress(string skillId, string displayName, string tooltip)
        {
            var experience = StimGameSessionService.GetSkillExperience(
                gameSession.ActiveSave.state.skills, skillId);
            var level = StimGameSessionService.GetSkillLevel(experience);
            var levelStart = StimGameSessionService.GetExperienceForSkillLevel(level);
            var nextLevelAt = StimGameSessionService.GetExperienceForSkillLevel(level + 1);
            var levelSpan = Math.Max(1, nextLevelAt - levelStart);
            var levelProgress = experience - levelStart;
            var row = new VisualElement { name = $"skill-{skillId}" };
            row.AddToClassList("st-skill-row");
            row.tooltip = tooltip;
            var heading = new VisualElement();
            heading.AddToClassList("st-skill-heading");
            var name = new Label(displayName);
            name.AddToClassList("st-skill-name");
            var levelLabel = new Label($"Level {level}") { name = $"skill-{skillId}-level" };
            levelLabel.AddToClassList("st-skill-level");
            heading.Add(name);
            heading.Add(levelLabel);
            var track = new VisualElement();
            track.AddToClassList("st-skill-track");
            var fill = new VisualElement { name = $"skill-{skillId}-fill" };
            fill.AddToClassList("st-skill-fill");
            fill.style.width = Length.Percent(ClampFillPercent(levelProgress * 100f / levelSpan));
            track.Add(fill);
            var progress = new Label($"{levelProgress} / {levelSpan} XP to Level {level + 1}")
            {
                name = $"skill-{skillId}-progress"
            };
            progress.AddToClassList("st-skill-progress");
            row.Add(heading);
            row.Add(track);
            row.Add(progress);
            skillsList.Add(row);
        }

        private void RefreshMoney()
        {
            var state = gameSession.ActiveSave.state;
            var adult = state.character.age >= 18;
            var career = state.career ?? new StimCareerState();
            var employed = !string.IsNullOrEmpty(career.roleTitle) && career.roleTitle != "Retired" &&
                           career.annualSalaryMinorUnits > 0 && state.character.lifeStatus == "active";
            var hourlyRate = StimGameSessionService.CalculateHourlyRateMinorUnits(career.annualSalaryMinorUnits);
            manualWorkRole.text = employed ? career.roleTitle : "Get a salaried job to begin";
            manualWorkRate.text = employed ? $"{FormatPreciseMoney(hourlyRate)} per hour" : "$0.00 per hour";
            moneyCashValue.text = FormatMoney(state.finances.cashMinorUnits);
            savingsBalanceValue.text = FormatMoney(state.finances.savingsMinorUnits);
            moneyAccountsList.Clear();
            moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow(
                "cash-wallet", "💵", "Cash Wallet", FormatMoney(state.finances.cashMinorUnits),
                "Available for actions and purchases"));
            moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow(
                "savings", "🏦", "Savings", FormatMoney(state.finances.savingsMinorUnits),
                $"{state.finances.savingsApyBasisPoints / 100m:0.00}% APY"));
            moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow(
                "revolving-credit", "▣", "Revolving Credit",
                FormatMoney(state.finances.householdCreditBalanceMinorUnits),
                $"{state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR"));
            if (adult)
                moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow(
                    "index-fund", "↗", "Index Fund", FormatMoney(state.finances.indexFundMinorUnits),
                    "Long-term investment value"));
            indexInvestmentInput.parent?.EnableInClassList("hidden", !adult);
            manualWorkTap.parent?.EnableInClassList("hidden", !adult);
            manualWorkTap.text = employed
                ? $"WORK 1 HOUR  ·  +{FormatPreciseMoney(hourlyRate)}"
                : "WORK 1 HOUR";
            manualWorkTap.SetEnabled(employed && string.IsNullOrEmpty(state.pendingEventId));
            var available = savingsTransferType == StimSavingsTransferType.Deposit
                ? state.finances.cashMinorUnits
                : state.finances.savingsMinorUnits;
            savingsAvailableValue.text = savingsTransferType == StimSavingsTransferType.Deposit
                ? $"Available cash: {FormatMoney(available)}"
                : $"Available savings: {FormatMoney(available)}";
            savingsDepositMode.EnableInClassList("active", savingsTransferType == StimSavingsTransferType.Deposit);
            savingsWithdrawMode.EnableInClassList("active", savingsTransferType == StimSavingsTransferType.Withdrawal);
            savingsAmountInput.Clear();
            var depositing = savingsTransferType == StimSavingsTransferType.Deposit;
            savingsAmountInput.Add(StimActionInputFactory.CreateAmountSelector(
                available,
                amount => PerformSavingsTransfer(amount),
                () => savingsTransferFeedback.text = string.Empty,
                depositing
                    ? "Quick deposit · percentage of available cash"
                    : "Quick withdrawal · percentage of savings",
                "Or enter a custom amount",
                depositing ? "Deposit" : "Withdraw"));
            RefreshMoneyTransactionHistory(state);
            cashFlowGross.text = $"Gross income: {FormatMoney(state.finances.lastGrossIncomeMinorUnits)}";
            cashFlowTaxes.text = $"Taxes: −{FormatMoney(state.finances.lastTaxesMinorUnits)}";
            cashFlowExpenses.text = $"Expenses: −{FormatMoney(state.finances.lastExpensesMinorUnits)}";
            cashFlowCreditInterest.text = $"Credit interest: −{FormatMoney(state.finances.lastCreditInterestMinorUnits)}";
            cashFlowSavingsInterest.text = $"Savings interest: +{FormatMoney(state.finances.lastSavingsInterestMinorUnits)}";
            cashFlowNet.text = $"Net change: {FormatSignedMoney(state.finances.lastNetCashFlowMinorUnits)}";
            var projectedInterest = StimGameSessionService.CalculateProjectedAnnualSavingsInterest(state.finances);
            savingsProjection.text = $"{state.finances.savingsApyBasisPoints / 100m:0.00}% APY · " +
                                     $"about {FormatMoney(projectedInterest)} interest over one year at the current balance; rate may change.";
            var creditBalance = state.finances.householdCreditBalanceMinorUnits;
            var creditLimit = StimGameSessionService.CalculateHouseholdCreditLimit(state);
            var availableCredit = Math.Max(0, creditLimit - state.finances.debtMinorUnits);
            creditBalanceValue.text = $"Balance: {FormatMoney(creditBalance)}";
            creditDetailValue.text = creditBalance > 0
                ? $"{state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR · Total debt {FormatMoney(state.finances.debtMinorUnits)}"
                : $"No active revolving balance · Total debt {FormatMoney(state.finances.debtMinorUnits)}";
            availableCreditValue.text = $"Available credit: {FormatMoney(availableCredit)} of {FormatMoney(creditLimit)}";
            creditRepaymentInput.Clear();
            creditRepaymentInput.Add(StimActionInputFactory.CreateAmountSelector(
                Math.Min(state.finances.cashMinorUnits, creditBalance), PerformCreditRepayment));
            indexFundValue.text = $"Index fund: {FormatMoney(state.finances.indexFundMinorUnits)}";
            indexFundContributions.text = $"Contributions: {FormatMoney(state.finances.indexFundContributionsMinorUnits)}";
            var indexFundPerformanceMinorUnits =
                state.finances.indexFundMinorUnits - state.finances.indexFundContributionsMinorUnits;
            indexFundPerformance.text = $"Market performance: {FormatSignedMoney(indexFundPerformanceMinorUnits)}";
            var canInvest = StimGameSessionService.TryGetIndexInvestmentRequirement(state, out var investmentRequirement);
            indexInvestmentRequirement.text = canInvest
                ? "Unlocked · Minimum contribution $10"
                : $"Locked · {investmentRequirement}";
            indexInvestmentInput.Clear();
            if (canInvest)
                indexInvestmentInput.Add(StimActionInputFactory.CreateAmountSelector(
                    state.finances.cashMinorUnits, PerformIndexInvestment));
            bankTabCredit.EnableInClassList("hidden", !adult);
            bankTabInvesting.EnableInClassList("hidden", !adult);
            if (!adult && selectedBankTab != StimBankTab.Savings)
                selectedBankTab = StimBankTab.Savings;
            SetBankTab(selectedBankTab);
        }

        private void SetBankTab(StimBankTab tab)
        {
            var adult = (gameSession?.ActiveSave?.state?.character?.age ?? 0) >= 18;
            if (!adult && tab != StimBankTab.Savings)
                tab = StimBankTab.Savings;
            selectedBankTab = tab;
            bankPanelSavings.EnableInClassList("hidden", tab != StimBankTab.Savings);
            bankPanelCredit.EnableInClassList("hidden", tab != StimBankTab.Credit);
            bankPanelInvesting.EnableInClassList("hidden", tab != StimBankTab.Investing);
            bankTabSavings.EnableInClassList("active", tab == StimBankTab.Savings);
            bankTabCredit.EnableInClassList("active", tab == StimBankTab.Credit);
            bankTabInvesting.EnableInClassList("active", tab == StimBankTab.Investing);
        }

        private void SetSavingsTransferType(StimSavingsTransferType transferType)
        {
            savingsTransferType = transferType;
            savingsTransferFeedback.text = string.Empty;
            RefreshMoney();
        }

        private void PerformSavingsTransfer(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryTransferSavings(
                savingsTransferType, amountMinorUnits, out var summary);
            savingsTransferFeedback.text = summary;
            savingsTransferFeedback.EnableInClassList("is-error", !succeeded);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            savingsTransferFeedback.text = summary;
        }

        private void RefreshMoneyTransactionHistory(StimGameState state)
        {
            moneyTransactionHistory.Clear();
            var history = state.moneyTransactions;
            if (history == null || history.Count == 0)
            {
                moneyTransactionHistory.Add(new Label("No savings transfers yet.")
                    { name = "money-history-empty" });
                return;
            }
            var first = Math.Max(0, history.Count - 10);
            for (var index = history.Count - 1; index >= first; index--)
            {
                var entry = history[index];
                var row = new VisualElement();
                row.AddToClassList("st-money-history-row");
                var transactionName = entry.type == "savings_deposit" ? "Deposit" :
                    entry.type == "savings_withdrawal" ? "Withdrawal" :
                    entry.type == "credit_repayment" ? "Credit repayment" : "Savings interest";
                if (entry.type == "index_investment") transactionName = "Index contribution";
                else if (entry.type == "index_gain") transactionName = "Index gain";
                else if (entry.type == "index_loss") transactionName = "Index loss";
                var title = new Label($"{transactionName} · {FormatMoney(entry.amountMinorUnits)}");
                title.AddToClassList("st-money-history-title");
                var detail = new Label(
                    $"Age {entry.age}, month {entry.monthOfYear} · Cash {FormatMoney(entry.cashBalanceMinorUnits)} · Savings {FormatMoney(entry.savingsBalanceMinorUnits)}");
                detail.AddToClassList("st-money-history-detail");
                row.Add(title);
                row.Add(detail);
                moneyTransactionHistory.Add(row);
            }
        }

        private void PerformCreditRepayment(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryRepayHouseholdCredit(amountMinorUnits, out var summary);
            creditRepaymentFeedback.text = summary;
            creditRepaymentFeedback.EnableInClassList("is-error", !succeeded);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            creditRepaymentFeedback.text = summary;
        }

        private void PerformIndexInvestment(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryInvestInIndexFund(amountMinorUnits, out var summary);
            indexInvestmentFeedback.text = summary;
            indexInvestmentFeedback.EnableInClassList("is-error", !succeeded);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            indexInvestmentFeedback.text = summary;
        }

        private void RefreshAchievements()
        {
            achievementsList.Clear();
            var achievements = gameSession.ActiveSave.state.achievements;
            var goals = gameSession.GetGoals();
            achievementsCount.text = $"{achievements.Count} unlocked";
            foreach (var goal in goals)
            {
                if (goal == null || goal.status == "expired") continue;
                var capturedGoalId = goal.goalId;
                var actionText = goal.status == "claimable" ? "CLAIM" : goal.status == "claimed" ? "DONE" : "GO";
                var row = StimUiComponentFactory.CreateAchievementRow(
                    goal.goalId,
                    goal.category == "daily" ? "📅" : goal.category == "main" ? "🎯" : "🏆",
                    goal.title,
                    ToDisplayName(goal.category),
                    FormatCompactProgress(goal.progress, goal.progressRequired),
                    FormatMoney(goal.rewardMinorUnits),
                    actionText,
                    goal.status != "claimed",
                    () =>
                {
                    if (PresentPendingEventIfAvailable()) return;
                    if (goal.status == "claimable")
                    {
                        gameSession.TryClaimGoalReward(capturedGoalId, out _);
                        RefreshHeader();
                        RefreshFeed();
                        RefreshMoney();
                        RefreshAchievements();
                    }
                    else if (goal.destination == "money") ShowMoneyDestination();
                    else if (goal.destination == "education") ShowEducationDestination();
                    else if (goal.destination == "career" || goal.destination == "business") ShowCareerDestination();
                    else if (goal.destination == "social" || goal.destination == "family") ShowSocialDestination();
                    else ShowLifeDestination();
                },
                    accessibleProgress: $"{goal.progress:N0} / {goal.progressRequired:N0}",
                    template: achievementRowTemplate);
                row.Q<Button>().name = $"goal-action-{goal.goalId}";
                achievementsList.Add(row);
            }
            if (achievements.Count == 0 && goals.Count == 0)
            {
                var empty = new Label("Your goals and milestones will appear here as this life unfolds.");
                empty.AddToClassList("st-feed-empty");
                achievementsList.Add(empty);
            }

            for (var index = achievements.Count - 1; index >= 0; index--)
            {
                var achievement = achievements[index];
                if (achievement == null) continue;
                var capturedAchievementId = achievement.achievementId;
                var reward = StimGameSessionService.GetAchievementRewardMinorUnits(achievement.achievementId);
                var row = StimUiComponentFactory.CreateAchievementRow(
                    achievement.achievementId,
                    "🏆",
                    StimGameSessionService.GetAchievementDisplayName(achievement.achievementId),
                    "Achievement",
                    $"Age {achievement.unlockedAtAge}",
                    FormatMoney(reward),
                    achievement.rewardClaimed ? "DONE" : "CLAIM",
                    !achievement.rewardClaimed && reward > 0,
                    () =>
                {
                    if (PresentPendingEventIfAvailable()) return;
                    gameSession.TryClaimAchievementReward(capturedAchievementId, out _);
                    RefreshHeader();
                    RefreshFeed();
                    RefreshMoney();
                    RefreshAchievements();
                }, template: achievementRowTemplate);
                row.Q<Button>().name = $"achievement-claim-{achievement.achievementId}";
                achievementsList.Add(row);
            }
        }

        private void RefreshEducation()
        {
            var state = gameSession.ActiveSave.state;
            var enrolled = state.character.age >= 6 && state.character.age < 18;
            educationEmptyState.EnableInClassList("hidden", enrolled);
            educationCard.EnableInClassList("hidden", !enrolled);
            educationCatalog.EnableInClassList("hidden", !enrolled || state.character.age < 14);
            if (!enrolled)
            {
                educationUnavailableCopy.text = state.character.age < 6
                    ? "Formal school actions begin at age 6. Childhood choices still shape future learning."
                    : "This life has no active school enrollment. Completed education remains part of the saved life state.";
                return;
            }

            RefreshEducationCatalog(state);

            var experience = StimGameSessionService.GetSkillExperience(state.skills, "learning");
            var level = StimGameSessionService.GetSkillLevel(experience);
            var levelStart = StimGameSessionService.GetExperienceForSkillLevel(level);
            var nextLevelAt = StimGameSessionService.GetExperienceForSkillLevel(level + 1);
            var levelSpan = Math.Max(1, nextLevelAt - levelStart);
            var levelProgress = experience - levelStart;
            educationStage.text = ToDisplayName(state.education.stage);
            learningLevel.text = $"Learning Level {level}";
            learningProgress.text = $"{levelProgress} / {levelSpan} XP to Level {level + 1}";
            learningFill.style.width = Length.Percent(ClampFillPercent(levelProgress * 100f / levelSpan));

            educationActions.Clear();
            if (!string.IsNullOrEmpty(state.education.awaitingDecisionId))
            {
                var pathChoices = state.education.awaitingDecisionId == "education_high_transition"
                    ? new[] { StimSchoolPathChoice.AcademicTrack, StimSchoolPathChoice.VocationalTrack, StimSchoolPathChoice.LeaveSchool }
                    : state.education.awaitingDecisionId == "education_middle_transition"
                        ? new[] { StimSchoolPathChoice.PublicSchool, StimSchoolPathChoice.Homeschool, StimSchoolPathChoice.LeaveSchool }
                        : new[] { StimSchoolPathChoice.PublicSchool, StimSchoolPathChoice.Homeschool };
                educationStage.text = "School path decision required";
                foreach (var pathChoice in pathChoices)
                {
                    var capturedChoice = pathChoice;
                    var button = new Button
                    {
                        name = $"school-path-{pathChoice.ToString().ToLowerInvariant()}",
                        text = ToDisplayName(pathChoice.ToString())
                    };
                    button.AddToClassList("st-education-action");
                    button.clicked += () => PerformSchoolPathChoice(capturedChoice);
                    educationActions.Add(button);
                }
                return;
            }
            if (state.character.age >= 14 && state.character.age < 18 &&
                string.IsNullOrEmpty(state.education.studyTrack))
            {
                educationStage.text = "Choose a study track";
                AddStudyTrackCard(StimStudyTrack.General, 0L,
                    "A flexible curriculum with no material fee.");
                AddStudyTrackCard(StimStudyTrack.Academic, 5000L,
                    "A theory-focused path preparing for advanced study.");
                AddStudyTrackCard(StimStudyTrack.Vocational, 7500L,
                    "A practical path preparing for skilled work.");
                return;
            }
            if (state.character.age >= 14 && state.character.age < 18 &&
                !string.IsNullOrEmpty(state.education.studyTrack))
            {
                AddQualificationSummary();
                AddStudySessionProgress(state);
                foreach (var definition in gameSession.GetStudySessionDefinitions())
                {
                    var suffix = definition.id.Substring("education.study.".Length);
                    if (!Enum.TryParse(suffix, true, out StimStudyDifficulty difficulty)) continue;
                    var capturedDifficulty = difficulty;
                    educationActions.Add(StimActionCardFactory.Create(
                        definition,
                        () => ShowStudySessionSheet(capturedDifficulty, definition),
                        actionCardTemplate));
                }
                return;
            }
            foreach (var definition in gameSession.GetEducationActionDefinitions())
            {
                var suffix = definition.id.Substring("education.".Length);
                if (!Enum.TryParse(suffix, true, out StimEducationActionType action))
                {
                    continue;
                }
                var capturedAction = action;
                educationActions.Add(StimActionCardFactory.Create(
                    definition,
                    () => PerformEducationAction(capturedAction),
                    actionCardTemplate));
            }
        }

        private void RefreshEducationCatalog(StimGameState state)
        {
            educationCatalogList.Clear();
            if (state.character.age < 14 || state.character.age >= 18) return;

            var currentTrack = state.education?.studyTrack ?? string.Empty;
            var qualificationXp = Math.Max(0, state.education?.qualificationExperience ?? 0);
            educationCatalogStatus.text = string.IsNullOrEmpty(currentTrack)
                ? "Choose one path below. The choice persists and shapes later qualifications."
                : $"Current: {ToDisplayName(currentTrack)} · {StimEducationActionService.GetQualificationTier(qualificationXp)} · {qualificationXp} XP";

            foreach (var discipline in StimEducationDisciplineCatalog.GetAll())
            {
                var cost = discipline.studyTrack == StimStudyTrack.Academic ? 5000L :
                    discipline.studyTrack == StimStudyTrack.Vocational ? 7500L : 0L;
                AddEducationCatalogRow(
                    state, discipline.studyTrack, discipline.displayName, cost,
                    discipline.consequenceSummary, currentTrack);
            }
        }

        private void AddEducationCatalogRow(
            StimGameState state,
            StimStudyTrack track,
            string disciplineName,
            long costMinorUnits,
            string consequence,
            string currentTrack)
        {
            var trackId = track.ToString().ToLowerInvariant();
            var selected = string.Equals(currentTrack, trackId, StringComparison.OrdinalIgnoreCase);
            var choiceOpen = string.IsNullOrEmpty(currentTrack) &&
                             string.IsNullOrEmpty(state.education.awaitingDecisionId);
            var affordable = state.finances.cashMinorUnits >= costMinorUnits;
            var materialText = costMinorUnits == 0 ? "Free materials" : $"{FormatMoney(costMinorUnits)} materials";
            var trailing = selected ? "CURRENT" : choiceOpen && affordable ? "GO" : materialText;
            Action onOpen = null;
            if (choiceOpen && affordable)
            {
                onOpen = () =>
                {
                    var target = educationActions.Q<Button>($"study-track-{trackId}");
                    target?.Focus();
                };
            }

            var row = StimUiComponentFactory.CreatePathRow(
                $"study-{trackId}", "🎓", disciplineName,
                $"{track} Track · {consequence} · {materialText}", trailing, affordable || selected, onOpen);
            row.AddToClassList("st-education-catalog-row");
            row.EnableInClassList("selected", selected);
            educationCatalogList.Add(row);
        }

        private void AddStudyTrackCard(StimStudyTrack track, long costMinorUnits, string description)
        {
            var affordable = gameSession.ActiveSave.state.finances != null &&
                             gameSession.ActiveSave.state.finances.cashMinorUnits >= costMinorUnits;
            var card = new VisualElement { name = $"study-track-card-{track.ToString().ToLowerInvariant()}" };
            card.AddToClassList("st-action-card");
            card.EnableInClassList("locked", !affordable);

            var discipline = StimEducationDisciplineCatalog.GetForTrack(track);
            var title = new Label(discipline == null
                ? $"{track} Track"
                : $"{discipline.displayName} · {track} Track");
            title.AddToClassList("st-action-card-title");
            card.Add(title);

            var preview = new Label(costMinorUnits == 0
                ? "Materials: Free"
                : $"Materials: −${costMinorUnits / 100m:0.00}");
            preview.AddToClassList("st-action-card-preview");
            card.Add(preview);

            var detail = new Label(description);
            detail.AddToClassList("st-action-card-progress");
            card.Add(detail);

            if (!affordable)
            {
                var requirement = new Label($"Requires ${costMinorUnits / 100m:0.00} cash");
                requirement.AddToClassList("st-action-requirement-chip");
                card.Add(requirement);
            }

            var capturedTrack = track;
            var button = new Button(() => PerformStudyTrackChoice(capturedTrack))
            {
                name = $"study-track-{track.ToString().ToLowerInvariant()}",
                text = $"Choose {track}",
                tooltip = affordable ? description : "Not enough cash for these materials."
            };
            button.AddToClassList("st-action-commit");
            button.SetEnabled(affordable);
            card.Add(button);
            educationActions.Add(card);
        }

        private void AddQualificationSummary()
        {
            var education = gameSession.ActiveSave.state.education;
            var experience = Math.Max(0, education.qualificationExperience);
            var nextTierAt = StimEducationActionService.GetNextQualificationTierAt(experience);
            var summary = new VisualElement { name = "qualification-summary" };
            summary.AddToClassList("st-qualification-summary");
            var track = new Label($"{ToDisplayName(education.studyTrack)} Track");
            track.AddToClassList("st-action-card-title");
            var tier = new Label(StimEducationActionService.GetQualificationTier(experience));
            tier.AddToClassList("st-education-level");
            var badges = new VisualElement { name = "qualification-badges" };
            badges.AddToClassList("st-qualification-badges");
            AddQualificationBadge(badges, "foundation", "Foundation", 0, experience);
            AddQualificationBadge(badges, "certificate", "Certificate",
                StimEducationActionService.CertificateQualificationExperience, experience);
            AddQualificationBadge(badges, "diploma", "Diploma",
                StimEducationActionService.DiplomaQualificationExperience, experience);
            AddQualificationBadge(badges, "advanced", "Advanced",
                StimEducationActionService.AdvancedQualificationExperience, experience);
            var progress = new Label(
                experience >= StimEducationActionService.AdvancedQualificationExperience
                ? $"{experience} XP · Highest tier reached"
                : $"{experience} / {nextTierAt} Qualification XP");
            progress.name = "qualification-progress";
            progress.AddToClassList("st-education-progress");
            summary.Add(track);
            summary.Add(tier);
            summary.Add(badges);
            summary.Add(progress);
            educationActions.Add(summary);
        }

        private static void AddQualificationBadge(
            VisualElement badges, string id, string label, int threshold, int experience)
        {
            var earned = experience >= threshold;
            var currentThreshold = experience >= StimEducationActionService.AdvancedQualificationExperience
                ? StimEducationActionService.AdvancedQualificationExperience
                : experience >= StimEducationActionService.DiplomaQualificationExperience
                    ? StimEducationActionService.DiplomaQualificationExperience
                    : experience >= StimEducationActionService.CertificateQualificationExperience
                        ? StimEducationActionService.CertificateQualificationExperience
                        : 0;
            var current = threshold == currentThreshold;
            var badge = new Label(earned ? $"✓ {label}" : $"🔒 {label} · {threshold} XP")
            {
                name = $"qualification-badge-{id}",
                tooltip = earned ? $"{label} qualification earned" : $"Requires {threshold} qualification XP"
            };
            badge.AddToClassList("st-qualification-badge");
            badge.EnableInClassList("earned", earned);
            badge.EnableInClassList("current", current);
            badge.EnableInClassList("locked", !earned);
            badges.Add(badge);
        }

        private void ShowStudySessionSheet(
            StimStudyDifficulty difficulty,
            StimActionDefinition definition)
        {
            if (definition == null) return;
            selectedStudyDifficulty = difficulty;
            selectedStudyDefinition = definition;
            studySessionTitle.text = definition.title;
            studySessionDescription.text = definition.description;
            studySessionEffects.text = definition.previews == null || definition.previews.Count == 0
                ? "No numeric changes"
                : string.Join(" · ", definition.previews.ConvertAll(delta =>
                    $"{delta.targetId} {(delta.amount >= 0 ? "+" : "−")}{Math.Abs(delta.amount)}"));
            studySessionTiming.text = definition.cooldownMonths > 0
                ? $"{definition.durationSeconds} seconds · Uses this month's school action · Available again after advancing a month"
                : "No monthly cooldown";
            studySessionRequirement.text = string.IsNullOrEmpty(definition.lockedReason)
                ? "Ready to begin"
                : definition.lockedReason;
            studySessionConfirm.SetEnabled(definition.state == StimActionState.Ready);
            studySessionSheet.RemoveFromClassList("hidden");
        }

        private void CloseStudySessionSheet()
        {
            studySessionSheet.AddToClassList("hidden");
            selectedStudyDefinition = null;
        }

        private void ConfirmSelectedStudySession()
        {
            if (selectedStudyDefinition == null) return;
            var difficulty = selectedStudyDifficulty;
            CloseStudySessionSheet();
            StartTimedStudySession(difficulty);
        }

        private void StartTimedStudySession(StimStudyDifficulty difficulty)
        {
            if (PresentPendingEventIfAvailable()) return;
            var instanceId = $"focused-study-{gameSession.ActiveSave.revision + 1}-{difficulty.ToString().ToLowerInvariant()}";
            var succeeded = gameSession.TryStartStudySession(difficulty, instanceId, out var summary);
            eventCategory.text = succeeded ? "STUDY IN PROGRESS" : "SESSION LOCKED";
            eventTitle.text = $"{difficulty} Study Session";
            eventBody.text = succeeded
                ? "Your session is saved. Return to Education to claim its rewards when the timer completes."
                : "Review the study-track, Smarts, and monthly-action requirements.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshEducation();
            RefreshHeader();
            RefreshFeed();
        }

        private void AddStudySessionProgress(StimGameState state)
        {
            if (state.actionProgress == null) return;
            foreach (var action in state.actionProgress)
            {
                if (action == null || string.IsNullOrEmpty(action.actionId) ||
                    !action.actionId.StartsWith("education.study.", StringComparison.Ordinal) ||
                    action.state == StimActionState.Complete.ToString()) continue;
                var ready = gameSession.IsActionReadyToClaim(action);
                var card = new VisualElement { name = $"study-progress-{action.instanceId}" };
                card.AddToClassList("st-action-card");
                card.AddToClassList("st-study-progress-card");
                var title = new Label(ToDisplayName(action.actionId.Substring("education.study.".Length)) + " Study Session");
                title.AddToClassList("st-action-card-title");
                var status = new Label(ready ? "Complete · reward ready to claim" : "In progress · rewards pending");
                status.AddToClassList("st-action-card-progress");
                var capturedInstanceId = action.instanceId;
                var claim = new Button(() => ClaimTimedStudySession(capturedInstanceId))
                {
                    name = $"study-claim-{action.instanceId}",
                    text = ready ? "CLAIM REWARD" : "IN PROGRESS"
                };
                claim.AddToClassList("st-action-commit");
                claim.AddToClassList("st-brand-jelly-claim");
                claim.SetEnabled(ready);
                card.Add(title);
                card.Add(status);
                card.Add(claim);
                educationActions.Add(card);
            }
        }

        private void ClaimTimedStudySession(string instanceId)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryClaimStudySession(instanceId, out var summary);
            eventCategory.text = succeeded ? "QUALIFICATION PROGRESS" : "SESSION NOT READY";
            eventTitle.text = "Study Session Reward";
            eventBody.text = succeeded
                ? "Your completed focused study advanced your selected qualification."
                : "The session must finish before its reward can be claimed.";
            resultText.text = summary;
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshEducation();
            RefreshHeader();
            RefreshFeed();
        }

        private void PerformStudyTrackChoice(StimStudyTrack track)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryChooseStudyTrack(track, out var summary);
            eventCategory.text = succeeded ? "STUDY TRACK" : "TRACK LOCKED";
            eventTitle.text = $"{track} Track";
            eventBody.text = succeeded
                ? "This track shapes the qualifications, careers, and events available later."
                : "Review the age, cash, and previous-selection requirements.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
        }

        private void PerformSchoolPathChoice(StimSchoolPathChoice choice)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryChooseSchoolPath(choice, out var summary);
            eventCategory.text = succeeded ? "LIFE PATH" : "PATH LOCKED";
            eventTitle.text = ToDisplayName(choice.ToString());
            eventBody.text = succeeded
                ? "This decision is now part of your permanent life history and can affect later opportunities."
                : "This path is not available at the current school transition.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
        }

        private void PerformEducationAction(StimEducationActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformEducationAction(actionType, out var summary);
            eventCategory.text = succeeded ? "SCHOOL PROGRESS" : "ACTION LOCKED";
            eventTitle.text = ToDisplayName(actionType.ToString());
            eventBody.text = succeeded
                ? "Your school effort improved your learning path."
                : "Meet the requirement or advance the month before trying again.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            if (gameSession.ActiveSave.state.character.lifeStatus != "active")
            {
                ShowFinalLifeSummary();
            }
        }

        private void ShowFinalLifeSummary()
        {
            var save = gameSession.ActiveSave;
            var character = save.state.character;
            endingName.text = string.IsNullOrEmpty(character.firstName)
                ? "Your story"
                : $"{character.firstName} {character.lastName}";
            endingStatus.text = character.lifeStatus == "retired"
                ? $"Retired at age {character.endedAtAge}"
                : $"Remembered at age {character.endedAtAge}";
            endingSummary.text = StimGameSessionService.BuildFinalLifeSummary(save);
            advanceMonth.SetEnabled(false);
            advanceYear.SetEnabled(false);
            eventSheet.AddToClassList("hidden");
            finalLifeSummary.RemoveFromClassList("hidden");
        }

        private void OpenNewLifeFromEnding()
        {
            finalLifeSummary.AddToClassList("hidden");
            ShowNewLifeSetup(false, true);
        }

        private void RefreshCareer()
        {
            var state = gameSession.ActiveSave.state;
            var adult = state.character.age >= 18;
            careerEmptyState.EnableInClassList("hidden", !adult);
            careerCard.EnableInClassList("hidden", !adult);
            careerActionsCard.EnableInClassList("hidden", !adult);
            RefreshCareerPathPreview(state, adult);
            if (!adult) return;

            var career = state.career ?? new StimCareerState();
            var retired = career.roleTitle == "Retired";
            var employed = !string.IsNullOrEmpty(career.roleTitle) && !retired;
            careerRole.text = retired ? "Retired" : employed ? career.roleTitle : "Unemployed";
            careerSalary.text = retired
                ? "Career complete"
                : $"{FormatMoney(career.annualSalaryMinorUnits)} annual salary";

            if (StimGameSessionService.TryGetNextCareerStep(
                    career.roleTitle,
                    out var nextRole,
                    out _,
                    out var progressRequired))
            {
                careerNextStep.text = $"Next step: {nextRole}";
                careerActionProgress.text = $"{career.careerProgress} / {progressRequired} career progress";
                careerActionFill.style.width = Length.Percent(
                    ClampFillPercent(career.careerProgress * 100f / progressRequired));
            }
            else
            {
                careerNextStep.text = retired ? "A lifetime of work is behind you."
                    : employed ? "Top of the current career ladder"
                    : state.statuses.Exists(status => status.statusId == "career_interview_ready")
                        ? "Your interview is ready"
                        : "Apply to begin your career";
                careerActionProgress.text = employed ? $"Career progress {career.careerProgress}" : string.Empty;
                careerActionFill.style.width = Length.Percent(employed ? career.careerProgress : 0);
            }

            careerActions.Clear();
            var actions = new[]
            {
                StimCareerActionType.Apply,
                StimCareerActionType.Interview,
                StimCareerActionType.WorkHard,
                StimCareerActionType.AskForPromotion,
                StimCareerActionType.Retrain,
                StimCareerActionType.Quit,
                StimCareerActionType.Retire
            };
            var usedThisMonth = state.statuses.Exists(status => status.statusId == "monthly_career_action_used");
            foreach (var action in actions)
            {
                if (action == StimCareerActionType.Retire && state.character.age < 65) continue;
                var unlocked = StimGameSessionService.TryGetCareerActionRequirement(state, action, out var requirement);
                var button = new Button
                {
                    name = $"career-action-{action.ToString().ToLowerInvariant()}",
                    text = unlocked ? ToDisplayName(action.ToString()) : $"{ToDisplayName(action.ToString())}\n{requirement}"
                };
                button.AddToClassList("st-career-action");
                button.SetEnabled(unlocked && !usedThisMonth);
                var capturedAction = action;
                button.clicked += () => PerformCareerAction(capturedAction);
                careerActions.Add(button);
            }
            var business = state.business ?? new StimBusinessState();
            foreach (var action in new[]
                     {
                         StimBusinessActionType.Start, StimBusinessActionType.Work,
                         StimBusinessActionType.Upgrade, StimBusinessActionType.HireStaff,
                         StimBusinessActionType.ExpandLocation, StimBusinessActionType.Sell
                     })
            {
                var available = TryGetBusinessActionRequirement(state, business, action, out var requirement);
                var actionLabel = action == StimBusinessActionType.Start
                    ? "Start Local Services\n$1,000 · Professional Level 2"
                    : action == StimBusinessActionType.Work
                        ? $"Work Business\nProgress {business.operatingProgress} / 100"
                        : action == StimBusinessActionType.Upgrade
                            ? $"Upgrade Business\nLevel {business.level} / 3"
                            : action == StimBusinessActionType.HireStaff
                                ? $"Hire Staff\n{business.staffCount} / {business.level * 2} · $750"
                                : action == StimBusinessActionType.ExpandLocation
                                    ? $"Expand Location\nTier {business.locationLevel} / 3"
                                    : $"Sell Business\nValuation {FormatMoney(business.valuationMinorUnits)}";
                var button = new Button
                {
                    name = $"business-action-{action.ToString().ToLowerInvariant()}",
                    text = available ? actionLabel : $"{actionLabel}\n{requirement}",
                    tooltip = available ? actionLabel.Replace('\n', ' ') : requirement
                };
                button.AddToClassList("st-career-action");
                button.SetEnabled(available);
                var capturedAction = action;
                button.clicked += () => PerformBusinessAction(capturedAction);
                careerActions.Add(button);
            }
        }

        private static bool TryGetBusinessActionRequirement(
            StimGameState state,
            StimBusinessState business,
            StimBusinessActionType action,
            out string requirement)
        {
            requirement = string.Empty;
            if (state.character.age < 18) return Fail("Unlocks at age 18", out requirement);
            if (state.character.lifeStatus != "active") return Fail("This life has ended", out requirement);
            if (!string.IsNullOrEmpty(state.pendingEventId)) return Fail("Resolve the pending event", out requirement);

            switch (action)
            {
                case StimBusinessActionType.Start:
                    if (business.status != "none") return Fail("A business path already exists", out requirement);
                    var professionalLevel = StimGameSessionService.GetSkillLevel(
                        StimGameSessionService.GetSkillExperience(state.skills, "professional"));
                    if (professionalLevel < 2) return Fail("Requires Professional Level 2", out requirement);
                    if (state.finances.cashMinorUnits < 100000) return Fail("Requires $1,000 cash", out requirement);
                    return true;
                case StimBusinessActionType.Work:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    return true;
                case StimBusinessActionType.Upgrade:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.level >= 3) return Fail("Maximum business level reached", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    var progressRequired =
                        StimProgressionStandards.GetBusinessUpgradeProgressRequired(business.level);
                    if (business.operatingProgress < progressRequired)
                        return Fail($"Requires {progressRequired} operating progress", out requirement);
                    if (state.finances.cashMinorUnits < business.level * 150000L)
                        return Fail($"Requires {FormatMoney(business.level * 150000L)} cash", out requirement);
                    return true;
                case StimBusinessActionType.HireStaff:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.staffCount >= business.level * 2) return Fail("Upgrade before hiring more staff", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    if (state.finances.cashMinorUnits < 75000) return Fail("Requires $750 cash", out requirement);
                    return true;
                case StimBusinessActionType.ExpandLocation:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.level < 2) return Fail("Requires business Level 2", out requirement);
                    if (business.locationLevel >= 3) return Fail("Maximum location tier reached", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    if (state.finances.cashMinorUnits < business.locationLevel * 300000L)
                        return Fail($"Requires {FormatMoney(business.locationLevel * 300000L)} cash", out requirement);
                    return true;
                case StimBusinessActionType.Sell:
                    return business.status == "operating" || Fail("Start a business first", out requirement);
                default:
                    return Fail("Action unavailable", out requirement);
            }
        }

        private static bool Fail(string message, out string requirement)
        {
            requirement = message;
            return false;
        }

        private void RefreshCareerPathPreview(StimGameState state, bool adult)
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

        private void PerformCareerAction(StimCareerActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformCareerAction(actionType, out var summary);
            eventCategory.text = succeeded ? "CAREER UPDATE" : "CAREER ACTION LOCKED";
            eventTitle.text = ToDisplayName(actionType.ToString());
            eventBody.text = succeeded
                ? "Your career path changed and the result was saved."
                : "Meet the displayed requirement or advance the month before trying again.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            if (gameSession.ActiveSave.state.character.lifeStatus != "active")
            {
                ShowFinalLifeSummary();
            }
        }

        private void PerformBusinessAction(StimBusinessActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformBusinessAction(actionType, out var summary);
            eventCategory.text = succeeded ? "BUSINESS UPDATE" : "BUSINESS ACTION LOCKED";
            eventTitle.text = ToDisplayName(actionType.ToString());
            eventBody.text = succeeded
                ? "Your business changed and the result was saved."
                : "Meet the displayed requirement or advance the month before trying again.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
        }

        private void ConfigureAgeAppropriateActivities(int age)
        {
            if (age < 5)
            {
                primaryFocusActivity = StimActivityType.Play;
                secondaryFocusActivity = StimActivityType.Rest;
                focusStudyTitle.text = "Play";
                focusStudyEffect.text = "+ Happiness · + Health";
                focusWorkoutTitle.text = "Rest";
                focusWorkoutEffect.text = "+ Health · + Happiness";
            }
            else if (age < 13)
            {
                primaryFocusActivity = StimActivityType.Study;
                secondaryFocusActivity = StimActivityType.Play;
                focusStudyTitle.text = "Study";
                focusStudyEffect.text = "+ Smarts";
                focusWorkoutTitle.text = "Play";
                focusWorkoutEffect.text = "+ Happiness · + Health";
            }
            else
            {
                primaryFocusActivity = StimActivityType.Study;
                secondaryFocusActivity = StimActivityType.Workout;
                focusStudyTitle.text = "Study";
                focusStudyEffect.text = "+ Smarts";
                focusWorkoutTitle.text = "Workout";
                focusWorkoutEffect.text = "+ Health";
            }
            focusStudy.SetEnabled(true);
            focusWorkout.SetEnabled(true);
            RefreshContextActivities(gameSession.ActiveSave.state);
        }

        private void RefreshContextActivities(StimGameState state)
        {
            contextActivities.Clear();
            StimActivityType[] activities;
            var employed = !string.IsNullOrEmpty(state.career?.roleTitle) && state.career.roleTitle != "Retired";
            if (state.character.age < 5)
                activities = new[] { StimActivityType.FamilyTime, StimActivityType.FamilyMovie, StimActivityType.Explore };
            else if (state.character.age < 13)
                activities = new[] { StimActivityType.AttendSchool, StimActivityType.FamilyTime, StimActivityType.FamilyMovie, StimActivityType.Explore };
            else if (state.character.age < 18)
                activities = new[] { StimActivityType.AttendSchool, StimActivityType.JoinClub, StimActivityType.FamilyMovie, StimActivityType.Socialize };
            else if (state.character.age >= 65)
                activities = new[] { StimActivityType.Hobby, StimActivityType.FamilyTime, StimActivityType.FamilyMovie, StimActivityType.FamilyMovieCredit, StimActivityType.Socialize, StimActivityType.Checkup };
            else if (employed)
                activities = new[] { StimActivityType.WorkShift, StimActivityType.Overtime, StimActivityType.Training, StimActivityType.FamilyTime, StimActivityType.FamilyMovie, StimActivityType.FamilyMovieCredit, StimActivityType.Socialize };
            else
                activities = new[] { StimActivityType.Training, StimActivityType.FamilyTime, StimActivityType.FamilyMovie, StimActivityType.FamilyMovieCredit, StimActivityType.Socialize, StimActivityType.Rest };

            var usedThisMonth = state.statuses.Exists(status => status.statusId == "monthly_focus_used");
            foreach (var activity in activities)
            {
                var available = StimGameSessionService.TryGetActivityRequirement(state, activity, out var requirement);
                var capturedActivity = activity;
                var button = new Button
                {
                    name = $"context-activity-{activity.ToString().ToLowerInvariant()}",
                    text = available ? ToDisplayName(activity.ToString()) : $"{ToDisplayName(activity.ToString())}\n{requirement}"
                };
                button.AddToClassList("st-context-activity");
                button.SetEnabled(available && !usedThisMonth);
                button.clicked += () => PerformActivity(capturedActivity);
                contextActivities.Add(button);
            }
        }

        private void CloseEventSheet()
        {
            if (!string.IsNullOrEmpty(presentedTransitionId))
            {
                gameSession.TryAcknowledgeTransition(presentedTransitionId, out _);
                presentedTransitionId = null;
                RefreshFeed();
                if (gameSession.GetPendingTransition() != null)
                {
                    PresentPendingTransition();
                    return;
                }
                if (PresentPendingEventIfAvailable()) return;
                if (gameSession.ShouldPresentFirstLifeOrientation())
                {
                    PresentFirstLifeOrientation();
                    return;
                }
            }
            if (presentingFirstLifeOrientation)
            {
                if (!gameSession.TryCompleteFirstLifeOrientation(out var orientationSummary))
                {
                    resultText.text = orientationSummary;
                    resultEffects.text = "Orientation remains open until progress can be saved";
                    return;
                }
                presentingFirstLifeOrientation = false;
                RefreshFeed();
            }
            if (PresentPendingEventIfAvailable()) return;
            if (queuedYearMonthsRemaining > 0 &&
                !string.IsNullOrEmpty(gameSession.ActiveSave.state.education?.awaitingDecisionId))
            {
                eventSheet.AddToClassList("hidden");
                resultCard.AddToClassList("hidden");
                eventContinue.AddToClassList("hidden");
                ShowEducationDestination();
                return;
            }
            if (queuedYearMonthsRemaining > 0)
            {
                ContinueQueuedYearAdvance();
                return;
            }
            if (queuedYearCompletionPending)
            {
                PresentQueuedYearCompletion();
                return;
            }
            eventSheet.AddToClassList("hidden");
            resultCard.AddToClassList("hidden");
            eventContinue.AddToClassList("hidden");
        }

        private void PresentQueuedYearCompletion()
        {
            queuedYearCompletionPending = false;
            eventCategory.text = "YEAR SUMMARY";
            eventTitle.text = "A full year moved forward";
            eventBody.text = "Every normal monthly change was processed and autosaved in sequence.";
            resultText.text = queuedYearCompletionSummary;
            resultEffects.text = "12 of 12 monthly transactions committed";
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            resultEffects.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            RefreshHeader();
            RefreshFeed();
        }

        private void PresentPendingTransition()
        {
            var transition = gameSession.GetPendingTransition();
            if (transition == null) return;
            presentedTransitionId = transition.transitionId;
            currentEvent = null;
            eventCategory.text = "LIFE MILESTONE";
            eventTitle.text = transition.title;
            eventBody.text = transition.summary;
            resultText.text = $"Age {transition.age} · {GetMonthName(transition.monthOfYear)}";
            resultEffects.text = "Saved to your Life Feed";
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            resultEffects.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
        }

        private void PresentFirstLifeOrientation()
        {
            presentingFirstLifeOrientation = true;
            currentEvent = null;
            eventCategory.text = "WELCOME TO STIM TYCOON";
            eventTitle.text = "Your life, one choice at a time";
            eventBody.text =
                "The Life Feed records what changes. Advance Month for detail or Advance Year for faster pacing—time always pauses for choices. Locked actions show what they require, and every completed action autosaves locally.";
            resultText.text = "Nothing here costs premium currency, and ordinary progression remains available.";
            resultEffects.text = "One screen · Continue when ready";
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            resultEffects.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
        }

        private void PerformActivity(StimActivityType activityType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformActivity(activityType, out var summary);
            eventCategory.text = succeeded ? "FOCUS COMPLETE" : "FOCUS UNAVAILABLE";
            eventTitle.text = $"{ToDisplayName(activityType.ToString())} session";
            eventBody.text = succeeded
                ? "Your focused action has been applied to this month."
                : "Choose another step after resolving the current requirement.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (succeeded)
            {
                RefreshHeader();
                RefreshFeed();
                RefreshSocial();
            }
        }

        private void ToggleOverview()
        {
            var opening = playerOverview.ClassListContains("hidden");
            playerOverview.EnableInClassList("hidden", !opening);
            toggleOverview.text = opening ? "HIDE PLAYER OVERVIEW" : "VIEW PLAYER OVERVIEW";
        }

        private void ShowLifeDestination()
        {
            NavigateTo(StimDestination.Life);
            PresentPendingEventIfAvailable();
        }

        private void ShowEducationDestination()
        {
            RefreshEducation();
            RefreshSkills();
            NavigateTo(StimDestination.Study);
            PresentPendingEventIfAvailable();
        }

        private void ShowCareerDestination()
        {
            RefreshCareer();
            NavigateTo(StimDestination.Work);
            PresentPendingEventIfAvailable();
        }

        private void ShowMoneyDestination()
        {
            RefreshMoney();
            NavigateTo(StimDestination.Bank);
            PresentPendingEventIfAvailable();
        }

        private void ShowSocialDestination()
        {
            RefreshSocial();
            NavigateTo(StimDestination.Social);
            if (string.IsNullOrEmpty(selectedRelationshipId)) ShowRelationshipList();
            PresentPendingEventIfAvailable();
        }

        private void ShowGoalsDestination()
        {
            RefreshAchievements();
            NavigateTo(StimDestination.Goals);
            PresentPendingEventIfAvailable();
        }

        private bool PresentPendingEventIfAvailable()
        {
            if (gameSession.TryGetPendingEvent(out var pendingEvent))
            {
                currentEvent = pendingEvent;
                PresentEvent(pendingEvent);
                return true;
            }
            if (!gameSession.HasPendingEvent) return false;

            currentEvent = null;
            eventCategory.text = "CONTENT RECOVERY";
            eventTitle.text = "A pending life event could not be loaded";
            eventBody.text = "This save needs event content that is unavailable in the current build. Update or restore the matching build, then reopen this life.";
            resultText.text = "No progress was changed.";
            resultEffects.text = "The unavailable event was preserved for safe recovery.";
            resultEffects.RemoveFromClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.AddToClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            return true;
        }

        private void NavigateTo(StimDestination destination, bool restoreScroll = true)
        {
            if (lifeSummaryView != null && lifeSummaryView.ClassListContains("hidden") &&
                activeDestination != destination)
            {
                var previousView = GetDestinationView(activeDestination);
                if (previousView != null)
                    destinationScrollOffsets[activeDestination] = previousView.scrollOffset;
            }

            activeDestination = destination;
            VisualElement selectedView;
            Button selectedButton;
            switch (destination)
            {
                case StimDestination.Study:
                    selectedView = educationView;
                    selectedButton = navEducation;
                    break;
                case StimDestination.Work:
                    selectedView = careerView;
                    selectedButton = navCareer;
                    break;
                case StimDestination.Bank:
                    selectedView = moneyView;
                    selectedButton = navMoney;
                    break;
                case StimDestination.Social:
                    selectedView = socialView;
                    selectedButton = navSocial;
                    break;
                case StimDestination.Goals:
                    selectedView = goalsView;
                    selectedButton = navGoals;
                    break;
                default:
                    selectedView = lifeScroll;
                    selectedButton = navLife;
                    break;
            }

            foreach (var view in new VisualElement[]
                     { lifeScroll, educationView, careerView, moneyView, socialView, goalsView, lifeSummaryView })
                view.EnableInClassList("hidden", view != selectedView);

            foreach (var button in new[]
                     { navLife, navEducation, navCareer, navMoney, navSocial, navGoals })
                button.EnableInClassList("active", button == selectedButton);

            timeDock.EnableInClassList("hidden", destination != StimDestination.Life);

            if (selectedView is ScrollView selectedScroll)
            {
                var targetOffset = restoreScroll && destinationScrollOffsets.TryGetValue(destination, out var savedOffset)
                    ? savedOffset
                    : Vector2.zero;
                selectedScroll.scrollOffset = targetOffset;
                // Apply once more after display/content rebuild, when the final scroll range is known.
                selectedScroll.schedule.Execute(() => selectedScroll.scrollOffset = targetOffset);
            }
        }

        private void ShowLifeSummary()
        {
            if (!lifeSummaryView.ClassListContains("hidden")) return;
            var previousView = GetDestinationView(activeDestination);
            destinationScrollOffsets[activeDestination] = previousView.scrollOffset;
            RefreshHeader();

            foreach (var view in new VisualElement[]
                     { lifeScroll, educationView, careerView, moneyView, socialView, goalsView })
                view.AddToClassList("hidden");

            lifeSummaryView.RemoveFromClassList("hidden");
            timeDock.AddToClassList("hidden");
            lifeSummaryView.schedule.Execute(() => lifeSummaryView.scrollOffset = Vector2.zero);
        }

        private void CloseLifeSummary()
        {
            NavigateTo(activeDestination);
        }

        private ScrollView GetDestinationView(StimDestination destination)
        {
            switch (destination)
            {
                case StimDestination.Study: return educationView;
                case StimDestination.Work: return careerView;
                case StimDestination.Bank: return moneyView;
                case StimDestination.Social: return socialView;
                case StimDestination.Goals: return goalsView;
                default: return lifeScroll;
            }
        }

        private void PerformManualWorkTap()
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformManualWorkTap(out _, out var summary);
            manualWorkFeedback.text = summary;
            if (!succeeded)
            {
                manualWorkFeedback.AddToClassList("is-error");
                RefreshMoney();
                return;
            }

            manualWorkFeedback.RemoveFromClassList("is-error");
            RefreshHeader();
            RefreshFeed();
        }

        private void ShowRelationshipList()
        {
            selectedRelationshipId = null;
            relationshipDetailView.AddToClassList("hidden");
            relationshipListView.RemoveFromClassList("hidden");
        }

        private void RefreshSocial()
        {
            relationshipList.Clear();
            var adult = (gameSession.ActiveSave?.state?.character?.age ?? 0) >= 18;
            var discoveryUsed = gameSession.ActiveSave?.state?.statuses?.Exists(
                status => status.statusId == "relationship_discovery_used") == true;
            discoverCompatiblePerson.EnableInClassList("hidden", !adult);
            discoverCompatiblePerson.SetEnabled(
                adult && !discoveryUsed &&
                string.IsNullOrEmpty(gameSession.ActiveSave?.state?.pendingEventId));
            var relationships = gameSession.ActiveSave?.state?.relationships;
            if (relationships == null || relationships.Count == 0)
            {
                var empty = new Label("Important people will appear here as your life grows.");
                empty.AddToClassList("st-feed-empty");
                relationshipList.Add(empty);
                return;
            }

            foreach (var relationship in relationships)
            {
                if (relationship == null) continue;
                var relationshipId = relationship.relationshipId;
                relationshipList.Add(StimUiComponentFactory.CreateRelationshipRow(
                    relationship.relationshipId,
                    relationship.displayName,
                    relationship.relationshipType,
                    relationship.value,
                    () => ShowRelationshipDetail(relationshipId)));
            }
        }

        private void ShowRelationshipDetail(string relationshipId)
        {
            var relationship = gameSession.ActiveSave.state.relationships.Find(
                candidate => candidate != null && candidate.relationshipId == relationshipId);
            if (relationship == null) return;

            selectedRelationshipId = relationshipId;
            relationshipListView.AddToClassList("hidden");
            relationshipDetailView.RemoveFromClassList("hidden");
            relationshipName.text = string.IsNullOrEmpty(relationship.displayName)
                ? ToDisplayName(relationship.relationshipId)
                : relationship.displayName;
            relationshipAvatar.text = relationshipName.text.Substring(0, 1).ToUpperInvariant();
            relationshipType.text = ToDisplayName(string.IsNullOrEmpty(relationship.relationshipStage)
                ? relationship.relationshipType
                : relationship.relationshipStage).ToUpperInvariant();
            relationshipStrength.text = $"Relationship {relationship.value} / 100 · Warmth {relationship.warmth} / 100";
            relationshipFill.style.width = Length.Percent(ClampFillPercent(relationship.value));
            relationshipGenetics.text = relationship.isGeneticParent
                ? $"Inherited profile · Health {relationship.geneticHealth} · Looks {relationship.geneticLooks} · Smarts {relationship.geneticSmarts}"
                : string.IsNullOrEmpty(relationship.origin)
                    ? "This relationship is part of your growing social story."
                    : $"{(string.IsNullOrEmpty(relationship.pronouns) ? string.Empty : relationship.pronouns + " · ")}" +
                      $"Met through {ToDisplayName(string.IsNullOrEmpty(relationship.introductionContext) ? relationship.origin : relationship.introductionContext)} at age {relationship.introducedAtAge} · " +
                      (relationship.monthsSinceInteraction == 0
                          ? "Connected this month."
                          : $"{relationship.monthsSinceInteraction} months since focused time together.");
            BuildRelationshipActions(relationship);
        }

        private void DiscoverCompatiblePerson()
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryDiscoverCompatiblePerson(out var relationshipId, out var summary);
            relationshipDiscoveryFeedback.text = summary;
            relationshipDiscoveryFeedback.EnableInClassList("is-error", !succeeded);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(relationshipId);
        }

        private void BuildRelationshipActions(StimRelationshipState relationship)
        {
            relationshipActions.Clear();
            var interactions = new[]
            {
                StimRelationshipInteractionType.Talk,
                StimRelationshipInteractionType.PlayTogether,
                StimRelationshipInteractionType.AskForHelp,
                StimRelationshipInteractionType.SpendTime,
                StimRelationshipInteractionType.Argue,
                StimRelationshipInteractionType.Compete,
                StimRelationshipInteractionType.Reconcile,
                StimRelationshipInteractionType.DeepenFriendship,
                StimRelationshipInteractionType.AskOnDate,
                StimRelationshipInteractionType.DateNight,
                StimRelationshipInteractionType.Commit,
                StimRelationshipInteractionType.BreakUp,
                StimRelationshipInteractionType.Separate,
                StimRelationshipInteractionType.Recover
            };
            var age = gameSession.ActiveSave.state.character.age;
            var cooldownId = $"relationship_interaction_used_{relationship.relationshipId}";
            var usedThisMonth = gameSession.ActiveSave.state.statuses.Exists(status => status.statusId == cooldownId);
            foreach (var interaction in interactions)
            {
                if (!StimGameSessionService.IsRelationshipInteractionAgeAppropriate(interaction, age)) continue;
                if (interaction == StimRelationshipInteractionType.Compete && relationship.relationshipType == "parent") continue;
                if (interaction == StimRelationshipInteractionType.Reconcile && relationship.relationshipType != "rival") continue;
                if (interaction == StimRelationshipInteractionType.DeepenFriendship &&
                    ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                     relationship.value < 65)) continue;
                if (interaction == StimRelationshipInteractionType.AskOnDate &&
                    ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                     relationship.value < 60)) continue;
                if (interaction == StimRelationshipInteractionType.Commit &&
                    (relationship.relationshipType != "dating" || relationship.value < 75)) continue;
                if (interaction == StimRelationshipInteractionType.DateNight &&
                    relationship.relationshipType != "dating" && relationship.relationshipType != "partner" &&
                    relationship.relationshipType != "engaged" && relationship.relationshipType != "married") continue;
                if (interaction == StimRelationshipInteractionType.BreakUp &&
                    relationship.relationshipType != "dating" && relationship.relationshipType != "partner") continue;
                if (interaction == StimRelationshipInteractionType.Separate &&
                    relationship.relationshipType != "partner" && relationship.relationshipType != "engaged") continue;
                if (interaction == StimRelationshipInteractionType.Recover &&
                    relationship.relationshipType != "ex_partner" && relationship.relationshipType != "estranged") continue;
                var button = new Button
                {
                    name = $"relationship-action-{interaction.ToString().ToLowerInvariant()}",
                    text = ToDisplayName(interaction.ToString())
                };
                button.AddToClassList("st-relationship-action");
                button.SetEnabled(!usedThisMonth);
                var capturedInteraction = interaction;
                button.clicked += () => PerformRelationshipInteraction(capturedInteraction);
                relationshipActions.Add(button);
            }
            if (age >= 18 && (relationship.relationshipType == "partner" ||
                              relationship.relationshipType == "engaged" ||
                              relationship.relationshipType == "married"))
            {
                AddFamilyPlanningAction(relationship, StimFamilyPlanningAction.Discuss, "Discuss family planning");
                AddFamilyPlanningAction(relationship, StimFamilyPlanningAction.TryForChild, "Try for a child · 9 months");
                AddFamilyPlanningAction(relationship, StimFamilyPlanningAction.PursueAdoption, "Pursue adoption · $500 · 6 months");
                AddFamilyPlanningAction(relationship, StimFamilyPlanningAction.OptOut, "Not now");
            }
            if (relationship.relationshipType == "child")
            {
                AddParentingAction(relationship, StimParentingAction.QualityTime, "Quality time · Wellbeing and relationship");
                AddParentingAction(relationship, StimParentingAction.SupportNeeds, "Support needs · $25 · Wellbeing");
                AddParentingAction(relationship, StimParentingAction.Teach, "Teach · Learning and independence");
                AddParentingAction(relationship, StimParentingAction.SetBoundaries, "Set boundaries · Independence");
            }
        }

        private void AddParentingAction(
            StimRelationshipState relationship,
            StimParentingAction action,
            string label)
        {
            var child = gameSession.ActiveSave.state.family.children.Find(record =>
                record.childId == relationship.relationshipId);
            if (child == null || child.age >= 18) return;
            var used = gameSession.ActiveSave.state.statuses.Exists(
                status => status.statusId == $"parenting_used_{child.childId}");
            var button = new Button
            {
                name = $"parenting-action-{action.ToString().ToLowerInvariant()}",
                text = $"{label}\nAge {child.age} · Wellbeing {child.wellbeing} · Learning {child.learning} · Independence {child.independence}"
            };
            button.AddToClassList("st-relationship-action");
            button.SetEnabled(!used && (action != StimParentingAction.SupportNeeds ||
                                        gameSession.ActiveSave.state.finances.cashMinorUnits >= 2500));
            var capturedAction = action;
            button.clicked += () => PerformParentingAction(capturedAction);
            relationshipActions.Add(button);
        }

        private void PerformParentingAction(StimParentingAction action)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformParentingAction(selectedRelationshipId, action, out var summary);
            eventCategory.text = succeeded ? "PARENTING MOMENT" : "PARENTING ACTION UNAVAILABLE";
            eventTitle.text = ToDisplayName(action.ToString());
            eventBody.text = "Parenting choices affect child wellbeing, learning, independence, and your relationship over time.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(selectedRelationshipId);
        }

        private void AddFamilyPlanningAction(
            StimRelationshipState relationship,
            StimFamilyPlanningAction action,
            string label)
        {
            var family = gameSession.ActiveSave.state.family;
            var agreed = family.planningPreference == "open" && family.partnerConsent &&
                         family.planningPartnerId == relationship.relationshipId;
            var pending = !string.IsNullOrEmpty(family.pendingPath);
            var used = gameSession.ActiveSave.state.statuses.Exists(
                status => status.statusId == "family_planning_used");
            var enabled = !used && (action == StimFamilyPlanningAction.Discuss ||
                                    action == StimFamilyPlanningAction.OptOut || agreed && !pending);
            if (action == StimFamilyPlanningAction.PursueAdoption)
                enabled &= gameSession.ActiveSave.state.finances.cashMinorUnits >= 50000;
            var button = new Button
            {
                name = $"family-action-{action.ToString().ToLowerInvariant()}",
                text = label + (pending ? $"\nPending {ToDisplayName(family.pendingPath)} · {family.monthsUntilResolution} months" : string.Empty)
            };
            button.AddToClassList("st-relationship-action");
            button.SetEnabled(enabled);
            var capturedAction = action;
            button.clicked += () => PerformFamilyPlanning(capturedAction);
            relationshipActions.Add(button);
        }

        private void PerformFamilyPlanning(StimFamilyPlanningAction action)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryChooseFamilyPlanning(selectedRelationshipId, action, out var summary);
            eventCategory.text = succeeded ? "FAMILY DECISION" : "FAMILY PATH UNAVAILABLE";
            eventTitle.text = ToDisplayName(action.ToString());
            eventBody.text = "Family planning requires an eligible adult partnership and mutual agreement. Choosing not now remains available.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(selectedRelationshipId);
        }

        private void PerformRelationshipInteraction(StimRelationshipInteractionType interactionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformRelationshipInteraction(
                selectedRelationshipId,
                interactionType,
                out var summary);
            eventCategory.text = succeeded ? "SOCIAL MOMENT" : "INTERACTION UNAVAILABLE";
            eventTitle.text = ToDisplayName(interactionType.ToString());
            eventBody.text = succeeded
                ? "This moment changed your relationship and became part of your life story."
                : "Choose another step after resolving the current requirement.";
            resultText.text = summary;
            resultEffects.text = string.Empty;
            resultEffects.AddToClassList("hidden");
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(selectedRelationshipId);
        }

        private void ShowNewLifeSetup(bool canContinue, bool canCancel)
        {
            newLifeError.AddToClassList("hidden");
            continueCurrentLife.EnableInClassList("hidden", !canContinue);
            cancelNewLife.EnableInClassList("hidden", !canCancel);
            if (canContinue && gameSession.ActiveSave != null)
            {
                var firstName = gameSession.ActiveSave.state.character.firstName;
                continueCurrentLife.text = string.IsNullOrEmpty(firstName)
                    ? "CONTINUE CURRENT LIFE  ›"
                    : $"CONTINUE {firstName.ToUpperInvariant()}'S LIFE  ›";
            }
            newLifeSetup.RemoveFromClassList("hidden");
        }

        private void CreateLifeFromSetup()
        {
            try
            {
                var save = StimNewLifeFactory.Create(
                    new StimNewLifeRequest(),
                    Application.version,
                    DateTimeOffset.UtcNow,
                    Environment.TickCount);

                if (!gameSession.TryStartNewLife(save, out var summary))
                {
                    ShowNewLifeError(summary);
                    return;
                }

                currentEvent = null;
                finalLifeSummary.AddToClassList("hidden");
                advanceMonth.SetEnabled(true);
                advanceYear.SetEnabled(true);
                newLifeSetup.AddToClassList("hidden");
                eventSheet.AddToClassList("hidden");
                choices.AddToClassList("hidden");
                advanceMonth.RemoveFromClassList("hidden");
                RefreshHeader();
                RefreshFeed();
                RefreshSocial();
                ShowLifeDestination();
                PresentPendingTransition();
            }
            catch (ArgumentException exception)
            {
                ShowNewLifeError(exception.Message);
            }
        }

        private void ShowNewLifeError(string message)
        {
            newLifeError.text = message;
            newLifeError.RemoveFromClassList("hidden");
        }

        private static string FormatMoney(long minorUnits)
        {
            return StimMoneyFormatter.Format(minorUnits);
        }

        private static string FormatPreciseMoney(long minorUnits)
        {
            return StimMoneyFormatter.FormatPrecise(minorUnits);
        }

        private static string FormatCompactProgress(long value, long maximum)
        {
            return $"{FormatCompactNumber(value)} / {FormatCompactNumber(maximum)}";
        }

        private static string FormatCompactMoney(long minorUnits)
        {
            var amount = minorUnits / 100m;
            var absolute = Math.Abs(amount);
            var sign = amount < 0 ? "−" : string.Empty;
            if (absolute >= 1000000000m) return $"{sign}${absolute / 1000000000m:0.#}B";
            if (absolute >= 1000000m) return $"{sign}${absolute / 1000000m:0.#}M";
            if (absolute >= 10000m) return $"{sign}${absolute / 1000m:0.#}K";
            return StimMoneyFormatter.Format(minorUnits);
        }

        private static string FormatCompactNumber(long value)
        {
            var absolute = Math.Abs(value);
            if (absolute >= 1000000000) return $"{value / 1000000000m:0.#}B";
            if (absolute >= 1000000) return $"{value / 1000000m:0.#}M";
            if (absolute >= 1000) return $"{value / 1000m:0.#}K";
            return value.ToString("N0");
        }

        public static float ClampFillPercent(float value)
        {
            return Math.Max(0f, Math.Min(100f, value));
        }

        private static string FormatSignedMoney(long minorUnits)
        {
            var prefix = minorUnits >= 0 ? "+" : "−";
            return $"{prefix}{FormatMoney(Math.Abs(minorUnits))}";
        }

        private static string BuildMonthlyEffectSummary(
            long cashDelta,
            int careerDelta,
            int happinessDelta,
            int ageDelta)
        {
            var summary = new StringBuilder($"Cash {FormatSignedMoney(cashDelta)}");
            if (careerDelta != 0)
            {
                summary.Append($"  ·  Career {(careerDelta > 0 ? "+" : "−")}{Math.Abs(careerDelta)}");
            }
            if (happinessDelta != 0)
            {
                summary.Append($"  ·  Happiness {(happinessDelta > 0 ? "+" : "−")}{Math.Abs(happinessDelta)}");
            }
            if (ageDelta != 0)
            {
                summary.Append($"  ·  Age {(ageDelta > 0 ? "+" : "−")}{Math.Abs(ageDelta)}");
            }
            return summary.ToString();
        }

        private static string BuildEffectSummary(IReadOnlyList<Effect> effects)
        {
            var summary = new StringBuilder();
            for (var index = 0; index < effects.Count; index++)
            {
                var effect = effects[index];
                if (effect == null || Math.Abs(effect.value) <= float.Epsilon)
                {
                    continue;
                }

                var formatted = FormatEffect(effect);
                if (string.IsNullOrEmpty(formatted))
                {
                    continue;
                }

                if (summary.Length > 0)
                {
                    summary.Append("  ·  ");
                }
                summary.Append(formatted);
            }

            return summary.Length > 0 ? summary.ToString() : "No stat change";
        }

        private static string FormatEffect(Effect effect)
        {
            var sign = effect.value >= 0 ? "+" : "−";
            var absoluteValue = Math.Abs(effect.value);
            switch (effect.type)
            {
                case EffectType.SalaryDelta:
                    return $"Salary {sign}{FormatMoney((long)Math.Round(absoluteValue))}";
                case EffectType.CashDelta:
                    return $"Cash {sign}{FormatMoney((long)Math.Round(absoluteValue))}";
                case EffectType.DebtDelta:
                    return $"Debt {sign}{FormatMoney((long)Math.Round(absoluteValue))}";
                case EffectType.CareerProgressDelta:
                    return $"Career {sign}{absoluteValue:0}";
                case EffectType.SkillXp:
                    return $"{ToDisplayName(effect.targetId)} XP {sign}{absoluteValue:0}";
                case EffectType.StatDelta:
                case EffectType.RelationshipDelta:
                case EffectType.ReputationDelta:
                case EffectType.BusinessMetricDelta:
                    return $"{ToDisplayName(effect.targetId)} {sign}{absoluteValue:0}";
                default:
                    return string.Empty;
            }
        }

        private static string ToDisplayName(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return "Stat";
            }

            return char.ToUpperInvariant(id[0]) + id.Substring(1).Replace('_', ' ');
        }

        private void RefreshFeed()
        {
            lifeFeedList.Clear();
            var entries = StimLifeFeedPresentation.GetNewestFirst(gameSession.ActiveSave.state.lifeFeed);
            if (entries == null || entries.Count == 0)
            {
                var empty = new Label("Your life is ready for its next chapter.");
                empty.AddToClassList("st-feed-empty");
                lifeFeedList.Add(empty);
                return;
            }

            VisualElement currentGroup = null;
            var currentAge = -1;
            var currentMonth = -1;
            const int maximumVisibleEntries = 8;
            var visibleEntries = Math.Min(maximumVisibleEntries, entries.Count);
            for (var index = 0; index < visibleEntries; index++)
            {
                var entry = entries[index];
                if (entry == null) continue;
                if (currentGroup == null || entry.age != currentAge || entry.monthOfYear != currentMonth)
                {
                    currentAge = entry.age;
                    currentMonth = entry.monthOfYear;
                    currentGroup = new VisualElement
                    {
                        name = $"feed-month-{currentAge}-{currentMonth}"
                    };
                    currentGroup.AddToClassList("st-feed-month-group");
                    var header = new Label($"AGE {currentAge}  ·  {GetMonthName(currentMonth).ToUpperInvariant()}");
                    header.AddToClassList("st-feed-month-header");
                    currentGroup.Add(header);
                    lifeFeedList.Add(currentGroup);
                }
                currentGroup.Add(StimUiComponentFactory.CreateFeedRow(
                    entry,
                    index,
                    entries.Count,
                    feedRowTemplate));
            }

            if (entries.Count > visibleEntries)
            {
                var remaining = new Label($"Showing the latest {visibleEntries} of {entries.Count} life updates.");
                remaining.AddToClassList("st-feed-more");
                lifeFeedList.Add(remaining);
            }
        }

        private static string GetMonthName(int month)
        {
            return month >= 1 && month <= 12
                ? new DateTime(2000, month, 1).ToString("MMMM")
                : $"Month {month}";
        }

        private void LogMissingUiBindings()
        {
            var missing = new List<string>();
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!typeof(VisualElement).IsAssignableFrom(field.FieldType)) continue;
                if (field.GetValue(this) == null) missing.Add(field.Name);
            }

            Debug.LogError(
                missing.Count == 0
                    ? "Vertical slice UXML binding validation failed, but no null UI field was identified. Reimport Assets/UI/StimVerticalSlice.uxml."
                    : $"Vertical slice UXML is missing required named elements for: {string.Join(", ", missing)}.",
                this);
        }

    }
}
