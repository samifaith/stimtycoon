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
        private StimLifeBinder lifeBinder;
        private StimStudyBinder studyBinder;
        private StimWorkBinder workBinder;
        private StimBankBinder bankBinder;
        private StimSocialBinder socialBinder;
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
        private Button homeActionRetry;
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
        private VisualElement careerEmptyState;
        private Label moneyCashValue;
        private Label manualWorkFeedback;
        private Button manualWorkRetry;
        private Label savingsTransferFeedback;
        private Button savingsTransferRetry;
        private Label creditRepaymentFeedback;
        private Button creditRepaymentRetry;
        private Label indexInvestmentFeedback;
        private Button indexInvestmentRetry;
        private StimSavingsTransferType savingsTransferType = StimSavingsTransferType.Deposit;
        private StimBankTab selectedBankTab = StimBankTab.Savings;
        private Label relationshipDiscoveryFeedback;
        private Button relationshipDiscoveryRetry;
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
        private readonly StimRetryCommandRegistry retryCommands = new StimRetryCommandRegistry();
        private Action workflowPersistenceRetry;

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
            lifeBinder = new StimLifeBinder(root, feedRowTemplate);
            studyBinder = new StimStudyBinder(root);
            workBinder = new StimWorkBinder(root);
            bankBinder = new StimBankBinder(root);
            socialBinder = new StimSocialBinder(root);
            cashValue = shellBinder.CashValue;
            lifeSummary = shellBinder.LifeSummary;
            calendarSummary = shellBinder.CalendarSummary;
            headerNetWorthValue = shellBinder.HeaderNetWorthValue;
            eventCategory = root.Q<Label>("event-category");
            eventTitle = root.Q<Label>("event-title");
            eventBody = root.Q<Label>("event-body");
            resultText = root.Q<Label>("result-text");
            resultEffects = root.Q<Label>("result-effects");
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
            homeActionRetry = root.Q<Button>("home-action-retry");
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
            careerEmptyState = root.Q<VisualElement>("career-empty-state");
            moneyCashValue = root.Q<Label>("money-cash-value");
            manualWorkFeedback = root.Q<Label>("manual-work-feedback");
            manualWorkRetry = root.Q<Button>("manual-work-retry");
            savingsTransferFeedback = root.Q<Label>("savings-transfer-feedback");
            savingsTransferRetry = root.Q<Button>("savings-transfer-retry");
            creditRepaymentFeedback = root.Q<Label>("credit-repayment-feedback");
            creditRepaymentRetry = root.Q<Button>("credit-repayment-retry");
            indexInvestmentFeedback = root.Q<Label>("index-investment-feedback");
            indexInvestmentRetry = root.Q<Button>("index-investment-retry");
            relationshipDiscoveryFeedback = root.Q<Label>("relationship-discovery-feedback");
            relationshipDiscoveryRetry = root.Q<Button>("relationship-discovery-retry");
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
                eventBody == null || resultText == null || resultEffects == null || lifeBinder == null || !lifeBinder.IsValid || choices == null ||
                resultCard == null || advanceMonth == null || advanceYear == null || toggleOverview == null || playerOverview == null ||
                overviewCareer == null || overviewCalendar == null || healthValue == null ||
                happinessValue == null || smartsValue == null || looksValue == null || luckValue == null ||
                careerProgressValue == null ||
                careerProgressFill == null || monthlyPaycheckValue == null || annualSalaryValue == null ||
                netWorthValue == null || avatarGlyph == null || eventSheet == null || eventContinue == null ||
                studyBinder == null || !studyBinder.IsValid ||
                healthFill == null || happinessFill == null || smartsFill == null || looksFill == null ||
                luckFill == null || newLifeSetup == null || newLifeError == null ||
                cancelNewLife == null || continueCurrentLife == null || createNewLife == null || openNewLife == null ||
                focusStudy == null || focusWorkout == null || focusStudyTitle == null || focusStudyEffect == null ||
                focusWorkoutTitle == null || focusWorkoutEffect == null || lifeScroll == null || lifeSummaryView == null ||
                openLifeSummary == null || closeLifeSummary == null || addCash == null || socialView == null ||
                contextActivities == null || homeCondition == null || homeProgress == null ||
                homeActions == null || homeUpgradeFeedback == null || homeActionRetry == null ||
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
                workBinder == null || !workBinder.IsValid || bankBinder == null || !bankBinder.IsValid ||
                moneyCashValue == null || manualWorkFeedback == null || manualWorkRetry == null ||
                savingsTransferFeedback == null || savingsTransferRetry == null ||
                creditRepaymentFeedback == null || creditRepaymentRetry == null ||
                indexInvestmentFeedback == null || indexInvestmentRetry == null ||
                socialBinder == null || !socialBinder.IsValid || relationshipDiscoveryFeedback == null ||
                relationshipDiscoveryRetry == null || educationCard == null || educationStage == null ||
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
            NavigateTo(StimDestination.Life, persistState: false);

            BindPersistentCallbacks();
            RestorePersistedWorkflowState();

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
            RestorePersistedNavigationState();
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
                shellBinder.CloseModal(StimShellModal.Event);
            }

            if (!shellBinder.HasBlockingModal && queuedYearMonthsRemaining > 0)
            {
                ContinueQueuedYearAdvance();
                return;
            }
            if (!shellBinder.HasBlockingModal && TryPresentPersistedStudyConfirmation()) return;
            if (shellBinder.HasBlockingModal) return;
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
            PersistNavigationState();
            UnbindPersistentCallbacks();
            retryCommands.ClearAll();
            shellBinder?.Dispose();
            shellBinder = null;
            lifeBinder = null;
            studyBinder = null;
            workBinder = null;
            bankBinder = null;
            socialBinder = null;
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
            BindPersistentButton(eventContinue, CloseEventSheet, StimShellModal.Event);
            BindPersistentButton(studyBinder.Cancel, CloseStudySessionSheet, StimShellModal.StudySession);
            BindPersistentButton(studyBinder.Confirm, ConfirmSelectedStudySession, StimShellModal.StudySession);
            BindPersistentButton(focusStudy, PerformPrimaryFocusActivity);
            BindPersistentButton(focusWorkout, PerformSecondaryFocusActivity);
            BindPersistentButton(closeLifeSummary, CloseLifeSummary);
            BindPersistentButton(workBinder.ManualWorkTap, PerformManualWorkTap);
            BindPersistentButton(manualWorkRetry, () => TryRetryCommand("work.manual"));
            BindPersistentButton(homeActionRetry, () => TryRetryCommand("home.last-action"));
            BindPersistentButton(bankBinder.SavingsDepositMode, SelectSavingsDeposit);
            BindPersistentButton(bankBinder.SavingsWithdrawMode, SelectSavingsWithdrawal);
            BindPersistentButton(savingsTransferRetry, () => TryRetryCommand("bank.savings-transfer"));
            BindPersistentButton(creditRepaymentRetry, () => TryRetryCommand("bank.credit-repayment"));
            BindPersistentButton(indexInvestmentRetry, () => TryRetryCommand("bank.index-investment"));
            BindPersistentButton(bankBinder.BankTabSavings, SelectSavingsBankTab);
            BindPersistentButton(bankBinder.BankTabCredit, SelectCreditBankTab);
            BindPersistentButton(bankBinder.BankTabInvesting, SelectInvestingBankTab);
            BindPersistentButton(socialBinder.RelationshipBack, ShowRelationshipList);
            BindPersistentButton(socialBinder.DiscoverCompatiblePerson, DiscoverCompatiblePerson);
            BindPersistentButton(relationshipDiscoveryRetry, () => TryRetryCommand("social.discovery"));
            BindPersistentButton(endingNewLife, OpenNewLifeFromEnding, StimShellModal.FinalLifeSummary);
            BindPersistentButton(openNewLife, OpenNewLifeSetup);
            BindPersistentButton(cancelNewLife, HideNewLifeSetup, StimShellModal.NewLife);
            BindPersistentButton(continueCurrentLife, HideNewLifeSetup, StimShellModal.NewLife);
            BindPersistentButton(createNewLife, CreateLifeFromSetup, StimShellModal.NewLife);
        }

        private void BindPersistentButton(
            Button button, Action callback, StimShellModal owningModal = StimShellModal.None)
        {
            if (button == null || callback == null) return;
            Action guardedCallback = () => shellBinder?.TryRunAction(callback, owningModal);
            button.clicked += guardedCallback;
            persistentButtonBindings.Add(new PersistentButtonBinding(button, guardedCallback));
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
            PersistNavigationState();
        }

        private void SelectCreditBankTab()
        {
            SetBankTab(StimBankTab.Credit);
            PersistNavigationState();
        }

        private void SelectInvestingBankTab()
        {
            SetBankTab(StimBankTab.Investing);
            PersistNavigationState();
        }

        private void OpenNewLifeSetup()
        {
            ShowNewLifeSetup(false, true);
        }

        private void HideNewLifeSetup()
        {
            shellBinder.CloseModal(StimShellModal.NewLife);
            RestoreModalReturnContext();
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
                OpenShellModal(StimShellModal.Event);
                eventContinue.RemoveFromClassList("hidden");
                return;
            }

            PresentEvent(currentEvent);
        }

        private void AdvanceYear()
        {
            if (PresentPendingEventIfAvailable()) return;
            var previousMonths = queuedYearMonthsRemaining;
            var previousCompletionPending = queuedYearCompletionPending;
            var previousCompletionSummary = queuedYearCompletionSummary;
            queuedYearMonthsRemaining = 12;
            queuedYearCompletionPending = false;
            queuedYearCompletionSummary = string.Empty;
            if (!TryPersistWorkflowState(out var persistSummary))
            {
                queuedYearMonthsRemaining = previousMonths;
                queuedYearCompletionPending = previousCompletionPending;
                queuedYearCompletionSummary = previousCompletionSummary;
                PresentWorkflowPersistenceFailure(persistSummary, AdvanceYear);
                return;
            }
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
                OpenShellModal(StimShellModal.Event);
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
                    if (!TryPersistWorkflowState(out var persistSummary))
                    {
                        PresentWorkflowPersistenceFailure(persistSummary, RetryQueuedYearCompletionPersistence);
                        return;
                    }
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
                    if (!TryPersistWorkflowState(out var persistSummary))
                    {
                        PresentWorkflowPersistenceFailure(persistSummary, RetryQueuedYearCompletionPersistence);
                        return;
                    }
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
                if (node == null) continue;
                StimPresentationStateStyler.Apply(node,
                    index == activeIndex ? StimPresentationState.Active :
                    index > activeIndex ? StimPresentationState.Locked : StimPresentationState.Available);
                node.EnableInClassList("complete", index < activeIndex);
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
            StimFeedbackPresenter.ShowTransactionResult(homeUpgradeFeedback, succeeded, summary);
            if (succeeded || !StimFeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("home.last-action");
            else retryCommands.Register("home.last-action", () => PerformHomeAction(actionType));
            RefreshDestinationRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
        }

        private void PerformHomeUpgrade()
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryUpgradeHome(out var summary);
            StimFeedbackPresenter.ShowTransactionResult(homeUpgradeFeedback, succeeded, summary);
            if (succeeded || !StimFeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("home.last-action");
            else retryCommands.Register("home.last-action", PerformHomeUpgrade);
            RefreshDestinationRetryButtons();
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
            workBinder.RenderManualWork(state, FormatPreciseMoney);
            moneyCashValue.text = FormatMoney(state.finances.cashMinorUnits);
            selectedBankTab = bankBinder.Render(state, savingsTransferType, selectedBankTab,
                FormatMoney, FormatSignedMoney, PerformSavingsTransfer,
                () => StimFeedbackPresenter.Clear(savingsTransferFeedback),
                PerformCreditRepayment, PerformIndexInvestment);
        }

        private void SetBankTab(StimBankTab tab)
        {
            var adult = (gameSession?.ActiveSave?.state?.character?.age ?? 0) >= 18;
            selectedBankTab = bankBinder.SelectTab(!adult ? StimBankTab.Savings : tab);
        }

        private void SetSavingsTransferType(StimSavingsTransferType transferType)
        {
            savingsTransferType = transferType;
            StimFeedbackPresenter.Clear(savingsTransferFeedback);
            RefreshMoney();
        }

        private void PerformSavingsTransfer(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var requestedTransferType = savingsTransferType;
            var succeeded = gameSession.TryTransferSavings(
                savingsTransferType, amountMinorUnits, out var summary);
            StimFeedbackPresenter.ShowTransactionResult(savingsTransferFeedback, succeeded, summary);
            if (succeeded || !StimFeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("bank.savings-transfer");
            else retryCommands.Register("bank.savings-transfer", () =>
            {
                savingsTransferType = requestedTransferType;
                PerformSavingsTransfer(amountMinorUnits);
            });
            RefreshBankRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            StimFeedbackPresenter.ShowTransactionResult(savingsTransferFeedback, true, summary);
        }

        private void PerformCreditRepayment(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryRepayHouseholdCredit(amountMinorUnits, out var summary);
            StimFeedbackPresenter.ShowTransactionResult(creditRepaymentFeedback, succeeded, summary);
            if (succeeded || !StimFeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("bank.credit-repayment");
            else retryCommands.Register("bank.credit-repayment", () => PerformCreditRepayment(amountMinorUnits));
            RefreshBankRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            StimFeedbackPresenter.ShowTransactionResult(creditRepaymentFeedback, true, summary);
        }

        private void PerformIndexInvestment(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryInvestInIndexFund(amountMinorUnits, out var summary);
            StimFeedbackPresenter.ShowTransactionResult(indexInvestmentFeedback, succeeded, summary);
            if (succeeded || !StimFeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("bank.index-investment");
            else retryCommands.Register("bank.index-investment", () => PerformIndexInvestment(amountMinorUnits));
            RefreshBankRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            StimFeedbackPresenter.ShowTransactionResult(indexInvestmentFeedback, true, summary);
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
            var education = state.education;
            var educationAvailable = education != null;
            var leftSchool = education?.stage == "left_school" || education?.schoolPath == "left_school";
            var enrolled = educationAvailable && !leftSchool &&
                           state.character.age >= 6 && state.character.age < 18;
            educationEmptyState.EnableInClassList("hidden", enrolled);
            educationCard.EnableInClassList("hidden", !enrolled);
            studyBinder.SetCatalogVisible(enrolled && state.character.age >= 14);
            if (!enrolled)
            {
                if (!educationAvailable)
                {
                    educationUnavailableCopy.text =
                        "Education state is unavailable. Reload this life or restore a valid save before studying.";
                    StimPresentationStateStyler.Apply(educationEmptyState, StimPresentationState.Error);
                }
                else if (leftSchool)
                {
                    educationUnavailableCopy.text =
                        "School was left before graduation. Completed learning remains saved, while qualification-gated paths stay locked.";
                    StimPresentationStateStyler.Apply(educationEmptyState, StimPresentationState.Terminal);
                }
                else if (state.character.age < 6)
                {
                    educationUnavailableCopy.text =
                        "Formal school actions begin at age 6. Childhood choices still shape future learning.";
                    StimPresentationStateStyler.Apply(educationEmptyState, StimPresentationState.Locked);
                }
                else if (education.graduatedSecondary || education.stage == "completed_secondary")
                {
                    var qualificationXp = Math.Max(0, education.qualificationExperience);
                    var track = string.IsNullOrEmpty(education.studyTrack)
                        ? "No specialist track"
                        : $"{ToDisplayName(education.studyTrack)} track";
                    educationUnavailableCopy.text =
                        $"Secondary education completed · {track} · " +
                        $"{StimEducationActionService.GetQualificationTier(qualificationXp)} · {qualificationXp} qualification XP.";
                    StimPresentationStateStyler.Apply(educationEmptyState, StimPresentationState.Claimed);
                }
                else
                {
                    educationUnavailableCopy.text =
                        "This life has no active school enrollment. Completed learning remains part of the saved life state.";
                    StimPresentationStateStyler.Apply(educationEmptyState, StimPresentationState.Empty);
                }
                return;
            }

            StimPresentationStateStyler.Apply(educationCard, StimPresentationState.Active);

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
            studyBinder.RenderCatalog(state, educationActions, FormatMoney);
        }

        private void AddStudyTrackCard(StimStudyTrack track, long costMinorUnits, string description)
        {
            var affordable = gameSession.ActiveSave.state.finances != null &&
                             gameSession.ActiveSave.state.finances.cashMinorUnits >= costMinorUnits;
            var card = new VisualElement { name = $"study-track-card-{track.ToString().ToLowerInvariant()}" };
            card.AddToClassList("st-action-card");
            StimPresentationStateStyler.Apply(card,
                affordable ? StimPresentationState.Available : StimPresentationState.Locked);

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
            StimPresentationStateStyler.Apply(badge,
                earned ? StimPresentationState.Claimed : StimPresentationState.Locked);
            badges.Add(badge);
        }

        private void ShowStudySessionSheet(
            StimStudyDifficulty difficulty,
            StimActionDefinition definition)
        {
            if (definition == null) return;
            selectedStudyDifficulty = difficulty;
            selectedStudyDefinition = definition;
            if (!TryPersistWorkflowState(out var persistSummary))
            {
                selectedStudyDefinition = null;
                PresentWorkflowPersistenceFailure(
                    persistSummary, () => ShowStudySessionSheet(difficulty, definition));
                return;
            }
            studyBinder.RenderSession(definition);
            OpenShellModal(StimShellModal.StudySession);
        }

        private void CloseStudySessionSheet()
        {
            TryCloseStudySessionSheet();
        }

        private bool TryCloseStudySessionSheet()
        {
            var previousDefinition = selectedStudyDefinition;
            selectedStudyDefinition = null;
            if (!TryPersistWorkflowState(out var persistSummary))
            {
                selectedStudyDefinition = previousDefinition;
                studyBinder.ShowPersistenceError(
                    $"{persistSummary} The confirmation remains open; retry Cancel.");
                return false;
            }
            shellBinder.CloseModal(StimShellModal.StudySession);
            RestoreModalReturnContext();
            return true;
        }

        private void ConfirmSelectedStudySession()
        {
            if (selectedStudyDefinition == null) return;
            var difficulty = selectedStudyDifficulty;
            if (!TryCloseStudySessionSheet()) return;
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
            shellBinder.CloseModal(StimShellModal.Event);
            OpenShellModal(StimShellModal.FinalLifeSummary);
        }

        private void OpenNewLifeFromEnding()
        {
            shellBinder.CloseModal(StimShellModal.FinalLifeSummary);
            ShowNewLifeSetup(false, true);
        }

        private void RefreshCareer()
        {
            var state = gameSession.ActiveSave.state;
            var adult = state.character.age >= 18;
            careerEmptyState.EnableInClassList("hidden", !adult);
            careerCard.EnableInClassList("hidden", !adult);
            careerActionsCard.EnableInClassList("hidden", !adult);
            workBinder.RenderPathPreview(state, adult);
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
            if (workflowPersistenceRetry != null)
            {
                var retry = workflowPersistenceRetry;
                workflowPersistenceRetry = null;
                eventContinue.text = "Continue";
                retry();
                return;
            }
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
                shellBinder.CloseModal(StimShellModal.Event);
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
            shellBinder.CloseModal(StimShellModal.Event);
            resultCard.AddToClassList("hidden");
            eventContinue.AddToClassList("hidden");
            RestoreModalReturnContext();
        }

        private void PresentQueuedYearCompletion()
        {
            var completionSummary = queuedYearCompletionSummary;
            queuedYearCompletionPending = false;
            queuedYearMonthsRemaining = 0;
            queuedYearCompletionSummary = string.Empty;
            if (!TryPersistWorkflowState(out var persistSummary))
            {
                queuedYearCompletionPending = true;
                queuedYearCompletionSummary = completionSummary;
                PresentWorkflowPersistenceFailure(persistSummary, PresentQueuedYearCompletion);
                return;
            }
            eventCategory.text = "YEAR SUMMARY";
            eventTitle.text = "A full year moved forward";
            eventBody.text = "Every normal monthly change was processed and autosaved in sequence.";
            resultText.text = completionSummary;
            resultEffects.text = "12 of 12 monthly transactions committed";
            choices.AddToClassList("hidden");
            resultCard.RemoveFromClassList("hidden");
            resultEffects.RemoveFromClassList("hidden");
            eventContinue.RemoveFromClassList("hidden");
            OpenShellModal(StimShellModal.Event);
            RefreshHeader();
            RefreshFeed();
        }

        private void RetryQueuedYearCompletionPersistence()
        {
            if (!TryPersistWorkflowState(out var persistSummary))
            {
                PresentWorkflowPersistenceFailure(persistSummary, RetryQueuedYearCompletionPersistence);
                return;
            }
            if (currentEvent != null)
            {
                PresentEvent(currentEvent);
                return;
            }
            if (gameSession.GetPendingTransition() != null)
            {
                PresentPendingTransition();
                return;
            }
            PresentQueuedYearCompletion();
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
            return true;
        }

        private void NavigateTo(
            StimDestination destination, bool restoreScroll = true, bool persistState = true)
        {
            if (lifeSummaryView != null && lifeSummaryView.ClassListContains("hidden") &&
                activeDestination != destination)
            {
                var previousView = GetDestinationView(activeDestination);
                if (previousView != null)
                    destinationScrollOffsets[activeDestination] = previousView.scrollOffset;
            }

            activeDestination = destination;
            var selectedView = shellBinder.RenderDestination(destination);

            if (selectedView is ScrollView selectedScroll)
            {
                var targetOffset = restoreScroll && destinationScrollOffsets.TryGetValue(destination, out var savedOffset)
                    ? savedOffset
                    : Vector2.zero;
                selectedScroll.scrollOffset = targetOffset;
                // Apply once more after display/content rebuild, when the final scroll range is known.
                selectedScroll.schedule.Execute(() => selectedScroll.scrollOffset = targetOffset);
            }
            if (persistState) PersistNavigationState();
        }

        private void ShowLifeSummary()
        {
            if (!lifeSummaryView.ClassListContains("hidden")) return;
            var previousView = GetDestinationView(activeDestination);
            destinationScrollOffsets[activeDestination] = previousView.scrollOffset;
            RefreshHeader();

            shellBinder.RenderLifeSummary();
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
            StimFeedbackPresenter.ShowTransactionResult(manualWorkFeedback, succeeded, summary);
            if (succeeded || !StimFeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("work.manual");
            else retryCommands.Register("work.manual", PerformManualWorkTap);
            RefreshDestinationRetryButtons();
            if (!succeeded)
            {
                RefreshMoney();
                return;
            }

            RefreshHeader();
            RefreshFeed();
        }

        private void ShowRelationshipList()
        {
            selectedRelationshipId = null;
            socialBinder.ShowList();
            PersistNavigationState();
        }

        private void RefreshSocial()
        {
            socialBinder.RenderList(gameSession.ActiveSave.state, id => ShowRelationshipDetail(id));
        }

        private void ShowRelationshipDetail(string relationshipId, bool persistState = true)
        {
            var relationship = gameSession.ActiveSave.state.relationships.Find(
                candidate => candidate != null && candidate.relationshipId == relationshipId);
            if (relationship == null) return;

            selectedRelationshipId = relationshipId;
            socialBinder.ShowDetail(relationship);
            BuildRelationshipActions(relationship);
            if (persistState) PersistNavigationState();
        }

        private void DiscoverCompatiblePerson()
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryDiscoverCompatiblePerson(out var relationshipId, out var summary);
            StimFeedbackPresenter.ShowTransactionResult(relationshipDiscoveryFeedback, succeeded, summary);
            if (succeeded || !StimFeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("social.discovery");
            else retryCommands.Register("social.discovery", DiscoverCompatiblePerson);
            RefreshDestinationRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(relationshipId);
        }

        private void BuildRelationshipActions(StimRelationshipState relationship)
        {
            socialBinder.RelationshipActions.Clear();
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
                socialBinder.RelationshipActions.Add(button);
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
            socialBinder.RelationshipActions.Add(button);
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
            OpenShellModal(StimShellModal.Event);
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
            socialBinder.RelationshipActions.Add(button);
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
            OpenShellModal(StimShellModal.Event);
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
            OpenShellModal(StimShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(selectedRelationshipId);
        }

        private void ShowNewLifeSetup(bool canContinue, bool canCancel)
        {
            StimFeedbackPresenter.Clear(newLifeError);
            continueCurrentLife.EnableInClassList("hidden", !canContinue);
            cancelNewLife.EnableInClassList("hidden", !canCancel);
            if (canContinue && gameSession.ActiveSave != null)
            {
                var firstName = gameSession.ActiveSave.state.character.firstName;
                continueCurrentLife.text = string.IsNullOrEmpty(firstName)
                    ? "CONTINUE CURRENT LIFE  ›"
                    : $"CONTINUE {firstName.ToUpperInvariant()}'S LIFE  ›";
            }
            OpenShellModal(StimShellModal.NewLife);
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
                shellBinder.CloseModal(StimShellModal.FinalLifeSummary);
                advanceMonth.SetEnabled(true);
                advanceYear.SetEnabled(true);
                shellBinder.CloseModal(StimShellModal.NewLife);
                shellBinder.CloseModal(StimShellModal.Event);
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
            StimFeedbackPresenter.Show(newLifeError, message, StimFeedbackKind.Error, true);
        }

        internal bool TryRetryCommand(string commandId) => retryCommands.TryExecute(commandId);

        private void OpenShellModal(StimShellModal modal)
        {
            shellBinder.CaptureModalReturnContext(
                activeDestination == StimDestination.Bank ? selectedBankTab.ToString() : string.Empty,
                activeDestination == StimDestination.Social ? selectedRelationshipId : string.Empty);
            shellBinder.OpenModal(modal);
        }

        private void RestoreModalReturnContext()
        {
            var destination = shellBinder.ModalReturnDestination;
            NavigateTo(destination);
            if (destination == StimDestination.Bank &&
                Enum.TryParse(shellBinder.ModalReturnTabId, out StimBankTab bankTab))
            {
                SetBankTab(bankTab);
            }
            else if (destination == StimDestination.Social)
            {
                var relationshipId = shellBinder.ModalReturnEntityId;
                var relationshipExists = !string.IsNullOrEmpty(relationshipId) &&
                    gameSession?.ActiveSave?.state?.relationships?.Exists(
                        relationship => relationship?.relationshipId == relationshipId) == true;
                if (relationshipExists) ShowRelationshipDetail(relationshipId);
                else ShowRelationshipList();
            }
        }

        private void RefreshBankRetryButtons()
        {
            savingsTransferRetry?.EnableInClassList("hidden", !retryCommands.IsAvailable("bank.savings-transfer"));
            creditRepaymentRetry?.EnableInClassList("hidden", !retryCommands.IsAvailable("bank.credit-repayment"));
            indexInvestmentRetry?.EnableInClassList("hidden", !retryCommands.IsAvailable("bank.index-investment"));
        }

        private void RefreshDestinationRetryButtons()
        {
            homeActionRetry?.EnableInClassList("hidden", !retryCommands.IsAvailable("home.last-action"));
            manualWorkRetry?.EnableInClassList("hidden", !retryCommands.IsAvailable("work.manual"));
            relationshipDiscoveryRetry?.EnableInClassList("hidden", !retryCommands.IsAvailable("social.discovery"));
        }

        private void RestorePersistedWorkflowState()
        {
            var workflow = gameSession?.ActiveSave?.state?.uiWorkflow;
            queuedYearMonthsRemaining = Math.Max(0, Math.Min(12, workflow?.queuedYearMonthsRemaining ?? 0));
            queuedYearCompletionPending = workflow?.queuedYearCompletionPending ?? false;
            queuedYearCompletionSummary = workflow?.queuedYearCompletionSummary ?? string.Empty;
        }

        private void RestorePersistedNavigationState()
        {
            var navigation = gameSession?.ActiveSave?.state?.uiWorkflow;
            if (navigation == null ||
                !Enum.TryParse(navigation.activeDestination, out StimDestination destination))
                return;

            if (destination == StimDestination.Bank &&
                Enum.TryParse(navigation.selectedTabId, out StimBankTab bankTab))
                SetBankTab(bankTab);

            var offset = new Vector2(
                Math.Max(0f, navigation.activeScrollX),
                Math.Max(0f, navigation.activeScrollY));
            destinationScrollOffsets[destination] = offset;
            NavigateTo(destination, persistState: false);

            if (destination != StimDestination.Social || string.IsNullOrEmpty(navigation.selectedEntityId))
                return;
            var relationshipExists = gameSession.ActiveSave.state.relationships?.Exists(
                relationship => relationship?.relationshipId == navigation.selectedEntityId) == true;
            if (relationshipExists)
                ShowRelationshipDetail(navigation.selectedEntityId, persistState: false);
        }

        private void PersistNavigationState()
        {
            if (gameSession?.ActiveSave == null) return;
            var activeView = GetDestinationView(activeDestination);
            var scrollOffset = activeView?.scrollOffset ?? Vector2.zero;
            gameSession.TryPersistUiNavigation(
                activeDestination.ToString(),
                activeDestination == StimDestination.Bank ? selectedBankTab.ToString() : string.Empty,
                activeDestination == StimDestination.Social ? selectedRelationshipId : string.Empty,
                scrollOffset.x,
                scrollOffset.y,
                out _);
        }

        private bool TryPersistWorkflowState(out string summary)
        {
            if (gameSession?.ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            return gameSession.TryPersistUiWorkflow(
                queuedYearMonthsRemaining,
                queuedYearCompletionPending,
                queuedYearCompletionSummary,
                selectedStudyDefinition == null ? string.Empty : selectedStudyDifficulty.ToString(),
                selectedStudyDefinition?.id,
                out summary);
        }

        private void PresentWorkflowPersistenceFailure(string summary, Action retry)
        {
            workflowPersistenceRetry = retry;
            choices.AddToClassList("hidden");
            eventCategory.text = "SAVE REQUIRED";
            eventTitle.text = "Progress could not be saved";
            eventBody.text = "No further workflow steps will run until this save succeeds.";
            resultText.text = string.IsNullOrEmpty(summary) ? "The save transaction was rolled back." : summary;
            resultEffects.text = "No workflow progress applied · Retry is safe";
            resultCard.RemoveFromClassList("hidden");
            resultEffects.RemoveFromClassList("hidden");
            eventContinue.text = "RETRY SAVE";
            eventContinue.RemoveFromClassList("hidden");
            OpenShellModal(StimShellModal.Event);
        }

        private bool TryPresentPersistedStudyConfirmation()
        {
            var workflow = gameSession?.ActiveSave?.state?.uiWorkflow;
            if (workflow == null || string.IsNullOrEmpty(workflow.pendingStudyActionId) ||
                !Enum.TryParse(workflow.pendingStudyDifficulty, out StimStudyDifficulty difficulty))
                return false;
            StimActionDefinition definition = null;
            foreach (var candidate in gameSession.GetStudySessionDefinitions())
                if (candidate?.id == workflow.pendingStudyActionId)
                {
                    definition = candidate;
                    break;
                }
            if (definition == null) return false;
            ShowEducationDestination();
            ShowStudySessionSheet(difficulty, definition);
            return true;
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
            lifeBinder?.RenderFeed(gameSession?.ActiveSave?.state?.lifeFeed);
        }

        private static string GetMonthName(int month) =>
            month >= 1 && month <= 12
                ? new DateTime(2000, month, 1).ToString("MMMM")
                : $"Month {month}";

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
