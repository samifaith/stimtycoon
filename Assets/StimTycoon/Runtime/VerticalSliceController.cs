using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StimTycoon.Events;
using StimTycoon.Saves;
using UnityEngine;
using UnityEngine.UIElements;
using Event = StimTycoon.Events.Event;

namespace StimTycoon.Runtime
{
    internal enum Destination
    {
        Life,
        Study,
        Work,
        Bank,
        Social,
        Goals
    }

    internal enum BankTab
    {
        Savings,
        Credit,
        Investing,
        Property
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class VerticalSliceController : MonoBehaviour
    {
        [SerializeField, Range(1f, 1.3f)] private float accessibilityTextScale = 1f;
        [SerializeField] private VisualTreeAsset feedRowTemplate;
        [SerializeField] private VisualTreeAsset achievementRowTemplate;
        [SerializeField] private VisualTreeAsset actionCardTemplate;

        private GameSessionService gameSession;
        private Label cashValue;
        private Label lifeSummary;
        private Label calendarSummary;
        private Label headerNetWorthValue;
        private LifeBinder lifeBinder;
        private StudyBinder studyBinder;
        private WorkBinder workBinder;
        private BankBinder bankBinder;
        private SocialBinder socialBinder;
        private GoalsBinder goalsBinder;
        private NewLifeBinder newLifeBinder;
        private EventSheetBinder eventSheetBinder;
        private FinalLifeBinder finalLifeBinder;
        private HomeBinder homeBinder;
        private MatchBinder matchBinder;
        private LifeOverviewBinder lifeOverviewBinder;
        private Label avatarGlyph;
        private VisualElement playerOverview;
        private Button advanceMonth;
        private Button advanceYear;
        private Button toggleOverview;
        private string presentedTransitionId;
        private bool presentingFirstLifeOrientation;
        private int queuedYearMonthsRemaining;
        private bool queuedYearCompletionPending;
        private string queuedYearCompletionSummary;
        private VisualElement newLifeSetup;
        private Button openNewLife;
        private Button focusStudy;
        private Button focusWorkout;
        private Label focusStudyTitle;
        private Label focusStudyEffect;
        private Label focusWorkoutTitle;
        private Label focusWorkoutEffect;
        private VisualElement contextActivities;
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
        private SavingsTransferType savingsTransferType = SavingsTransferType.Deposit;
        private BankTab selectedBankTab = BankTab.Savings;
        private Label relationshipDiscoveryFeedback;
        private Button relationshipDiscoveryRetry;
        private string selectedRelationshipId;
        private string selectedSocialFilter = "all";
        private bool businessWorkspaceSelected;
        private string selectedGoalsBoard = "main";
        private string selectedHomeObjectId = "bookshelf";
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
        private VisualElement rootElement;
        private ShellBinder shellBinder;
        private ActivityType primaryFocusActivity;
        private ActivityType secondaryFocusActivity;
        private Event currentEvent;
        private StudyDifficulty selectedStudyDifficulty;
        private ActionDefinition selectedStudyDefinition;
        private Destination activeDestination = Destination.Life;
        private readonly Dictionary<Destination, Vector2> destinationScrollOffsets =
            new Dictionary<Destination, Vector2>();
        private readonly List<PersistentButtonBinding> persistentButtonBindings =
            new List<PersistentButtonBinding>();
        private readonly RetryCommandRegistry retryCommands = new RetryCommandRegistry();
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
            if (!UiBindingManifest.TryValidate(root, out var bindingError))
            {
                Debug.LogError($"Vertical slice binding manifest failed. {bindingError}", this);
                rootElement = null;
                return;
            }
            shellBinder = new ShellBinder(root, HandleRootGeometryChanged);
            lifeBinder = new LifeBinder(root, feedRowTemplate);
            studyBinder = new StudyBinder(root);
            workBinder = new WorkBinder(root);
            bankBinder = new BankBinder(root);
            socialBinder = new SocialBinder(root);
            goalsBinder = new GoalsBinder(root, achievementRowTemplate);
            newLifeBinder = new NewLifeBinder(root);
            eventSheetBinder = new EventSheetBinder(root);
            finalLifeBinder = new FinalLifeBinder(root);
            homeBinder = new HomeBinder(root);
            matchBinder = new MatchBinder(root);
            lifeOverviewBinder = new LifeOverviewBinder(root);
            cashValue = shellBinder.CashValue;
            lifeSummary = shellBinder.LifeSummary;
            calendarSummary = shellBinder.CalendarSummary;
            headerNetWorthValue = shellBinder.HeaderNetWorthValue;
            avatarGlyph = shellBinder.AvatarGlyph;
            playerOverview = root.Q<VisualElement>("player-overview");
            newLifeSetup = root.Q<VisualElement>("new-life-setup");
            openNewLife = root.Q<Button>("open-new-life");
            focusStudy = root.Q<Button>("focus-study");
            focusWorkout = root.Q<Button>("focus-workout");
            focusStudyTitle = root.Q<Label>("focus-study-title");
            focusStudyEffect = root.Q<Label>("focus-study-effect");
            focusWorkoutTitle = root.Q<Label>("focus-workout-title");
            focusWorkoutEffect = root.Q<Label>("focus-workout-effect");
            contextActivities = root.Q<VisualElement>("context-activities");
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
            var catalog = new InMemoryEventCatalog();
            foreach (var authoredEvent in PlayableEventCatalog.Build().events)
            {
                catalog.Upsert(authoredEvent);
            }
            gameSession = new GameSessionService(catalog, new NativeSaveRepository());
            var loadedExistingLife = gameSession.TryLoadLatest(out _);

            advanceMonth = shellBinder.AdvanceMonth;
            advanceYear = shellBinder.AdvanceYear;
            toggleOverview = root.Q<Button>("toggle-overview");
            if (cashValue == null || lifeSummary == null || eventSheetBinder == null || !eventSheetBinder.IsValid ||
                lifeBinder == null || !lifeBinder.IsValid ||
                advanceMonth == null || advanceYear == null || toggleOverview == null || playerOverview == null ||
                lifeOverviewBinder == null || !lifeOverviewBinder.IsValid || avatarGlyph == null ||
                studyBinder == null || !studyBinder.IsValid ||
                newLifeSetup == null || newLifeBinder == null || !newLifeBinder.IsValid ||
                openNewLife == null ||
                focusStudy == null || focusWorkout == null || focusStudyTitle == null || focusStudyEffect == null ||
                focusWorkoutTitle == null || focusWorkoutEffect == null || lifeScroll == null || lifeSummaryView == null ||
                openLifeSummary == null || closeLifeSummary == null || addCash == null || socialView == null ||
                contextActivities == null || homeBinder == null || !homeBinder.IsValid ||
                matchBinder == null || !matchBinder.IsValid ||
                timeDock == null || navLife == null || navEducation == null || navCareer == null ||
                navMoney == null || navSocial == null || navGoals == null || moneyView == null ||
                educationView == null || careerView == null || goalsView == null ||
                educationDestinationContent == null || careerDestinationContent == null ||
                goalsDestinationContent == null || educationEmptyState == null || careerEmptyState == null ||
                educationUnavailableCopy == null ||
                workBinder == null || !workBinder.IsValid || bankBinder == null || !bankBinder.IsValid ||
                moneyCashValue == null || manualWorkFeedback == null || manualWorkRetry == null ||
                savingsTransferFeedback == null || savingsTransferRetry == null ||
                creditRepaymentFeedback == null || creditRepaymentRetry == null ||
                indexInvestmentFeedback == null || indexInvestmentRetry == null ||
                socialBinder == null || !socialBinder.IsValid || goalsBinder == null || !goalsBinder.IsValid ||
                relationshipDiscoveryFeedback == null ||
                relationshipDiscoveryRetry == null || educationCard == null || educationStage == null ||
                learningLevel == null || learningFill == null || learningProgress == null ||
                educationActions == null || skillsList == null || careerCard == null || careerRole == null || careerSalary == null ||
                careerNextStep == null || careerActionFill == null || careerActionProgress == null ||
                careerActions == null || careerActionsCard == null || finalLifeBinder == null || !finalLifeBinder.IsValid)
            {
                LogMissingUiBindings();
                return;
            }

            PopulateVisualPlaceholders(root);
            NavigateTo(Destination.Life, persistState: false);

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
                catalog.TryGetEvent(RepresentativeEvents.SalaryNegotiationId, out currentEvent);
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
                eventSheetBinder.ChoicesVisible = false;
                advanceMonth.RemoveFromClassList("hidden");
                shellBinder.CloseModal(ShellModal.Event);
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
            AddVisualPlaceholder(root, "event-visual-slot", new VisualPlaceholderDefinition
            {
                visualId = "event.generic.hero", role = VisualRole.Hero, aspectRatio = "16:9",
                accessibilityLabelKey = "visual.event.generic", fallbackGlyph = "✨", themeToken = "event"
            });
            AddVisualPlaceholder(root, "home-visual-slot", new VisualPlaceholderDefinition
            {
                visualId = "home.starter.thumbnail", role = VisualRole.Thumbnail, aspectRatio = "4:3",
                accessibilityLabelKey = "visual.home.starter", fallbackGlyph = "🏠", themeToken = "home"
            });
            AddVisualPlaceholder(root, "education-visual-slot", new VisualPlaceholderDefinition
            {
                visualId = "education.current.thumbnail", role = VisualRole.Thumbnail, aspectRatio = "4:3",
                accessibilityLabelKey = "visual.education.current", fallbackGlyph = "🎒", themeToken = "education"
            });
            AddVisualPlaceholder(root, "relationship-visual-slot", new VisualPlaceholderDefinition
            {
                visualId = "relationship.selected.avatar", role = VisualRole.Avatar, aspectRatio = "1:1",
                accessibilityLabelKey = "visual.relationship.selected", fallbackGlyph = "👤", themeToken = "social"
            });
        }

        private static void AddVisualPlaceholder(
            VisualElement root,
            string slotName,
            VisualPlaceholderDefinition definition)
        {
            var slot = root.Q<VisualElement>(slotName);
            if (slot == null) return;
            if (slot.childCount > 0) return;
            slot.Add(VisualPlaceholderFactory.Create(definition));
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
            goalsBinder = null;
            newLifeBinder = null;
            eventSheetBinder = null;
            finalLifeBinder = null;
            homeBinder = null;
            lifeOverviewBinder = null;
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
            BindPersistentButton(eventSheetBinder.Continue, CloseEventSheet, ShellModal.Event);
            BindPersistentButton(studyBinder.Cancel, CloseStudySessionSheet, ShellModal.StudySession);
            BindPersistentButton(studyBinder.Confirm, ConfirmSelectedStudySession, ShellModal.StudySession);
            BindPersistentButton(matchBinder.Start, StartStudyMatch);
            BindPersistentButton(matchBinder.Pause, ToggleStudyMatchPause);
            BindPersistentButton(matchBinder.Claim, ClaimStudyMatch);
            BindPersistentButton(focusStudy, PerformPrimaryFocusActivity);
            BindPersistentButton(focusWorkout, PerformSecondaryFocusActivity);
            BindPersistentButton(closeLifeSummary, CloseLifeSummary);
            BindPersistentButton(workBinder.ManualWorkTap, PerformManualWorkTap);
            BindPersistentButton(workBinder.WorkTabCareer, () => SetWorkWorkspace(false));
            BindPersistentButton(workBinder.WorkTabBusiness, () => SetWorkWorkspace(true));
            BindPersistentButton(manualWorkRetry, () => TryRetryCommand("work.manual"));
            BindPersistentButton(homeBinder.Retry, () => TryRetryCommand("home.last-action"));
            BindPersistentButton(bankBinder.SavingsDepositMode, SelectSavingsDeposit);
            BindPersistentButton(bankBinder.SavingsWithdrawMode, SelectSavingsWithdrawal);
            BindPersistentButton(savingsTransferRetry, () => TryRetryCommand("bank.savings-transfer"));
            BindPersistentButton(creditRepaymentRetry, () => TryRetryCommand("bank.credit-repayment"));
            BindPersistentButton(indexInvestmentRetry, () => TryRetryCommand("bank.index-investment"));
            BindPersistentButton(bankBinder.BankTabSavings, SelectSavingsBankTab);
            BindPersistentButton(bankBinder.BankTabCredit, SelectCreditBankTab);
            BindPersistentButton(bankBinder.BankTabInvesting, SelectInvestingBankTab);
            BindPersistentButton(bankBinder.BankTabProperty, SelectPropertyBankTab);
            BindPersistentButton(socialBinder.RelationshipBack, ShowRelationshipList);
            BindPersistentButton(socialBinder.DiscoverCompatiblePerson, DiscoverCompatiblePerson);
            BindPersistentButton(socialBinder.FilterAll, () => SetSocialFilter("all"));
            BindPersistentButton(socialBinder.FilterFamily, () => SetSocialFilter("family"));
            BindPersistentButton(socialBinder.FilterFriends, () => SetSocialFilter("friends"));
            BindPersistentButton(socialBinder.FilterRomance, () => SetSocialFilter("romance"));
            BindPersistentButton(goalsBinder.TabMain, () => SetGoalsBoard("main"));
            BindPersistentButton(goalsBinder.TabDaily, () => SetGoalsBoard("daily"));
            BindPersistentButton(goalsBinder.TabLife, () => SetGoalsBoard("life"));
            BindPersistentButton(goalsBinder.TabAchievements, () => SetGoalsBoard("achievements"));
            BindPersistentButton(relationshipDiscoveryRetry, () => TryRetryCommand("social.discovery"));
            BindPersistentButton(finalLifeBinder.StartNewLife, OpenNewLifeFromEnding, ShellModal.FinalLifeSummary);
            BindPersistentButton(openNewLife, OpenNewLifeSetup);
            BindPersistentButton(newLifeBinder.Cancel, HideNewLifeSetup, ShellModal.NewLife);
            BindPersistentButton(newLifeBinder.Continue, HideNewLifeSetup, ShellModal.NewLife);
            BindPersistentButton(newLifeBinder.Create, CreateLifeFromSetup, ShellModal.NewLife);
        }

        private void BindPersistentButton(
            Button button, Action callback, ShellModal owningModal = ShellModal.None)
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
            SetSavingsTransferType(SavingsTransferType.Deposit);
        }

        private void SelectSavingsWithdrawal()
        {
            SetSavingsTransferType(SavingsTransferType.Withdrawal);
        }

        private void SelectSavingsBankTab()
        {
            SetBankTab(BankTab.Savings);
            PersistNavigationState();
        }

        private void SelectCreditBankTab()
        {
            SetBankTab(BankTab.Credit);
            PersistNavigationState();
        }

        private void SelectInvestingBankTab()
        {
            SetBankTab(BankTab.Investing);
            PersistNavigationState();
        }

        private void SelectPropertyBankTab()
        {
            SetBankTab(BankTab.Property);
            PersistNavigationState();
        }

        private void OpenNewLifeSetup()
        {
            ShowNewLifeSetup(false, true);
        }

        private void HideNewLifeSetup()
        {
            shellBinder.CloseModal(ShellModal.NewLife);
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

        private void Resolve(string choiceId, PaymentMethod paymentMethod = PaymentMethod.Cash)
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
                eventSheetBinder.ResultText = summary;
                eventSheetBinder.EffectsText = "No changes applied";
                eventSheetBinder.EffectsVisible = true;
                eventSheetBinder.ResultVisible = true;
                return;
            }

            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = BuildEffectSummary(gameSession.LastResolution.outcome.effects);
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ContinueVisible = true;
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
                eventSheetBinder.ResultText = summary;
                eventSheetBinder.EffectsText = "No changes applied";
                eventSheetBinder.EffectsVisible = true;
                eventSheetBinder.ResultVisible = true;
                return;
            }

            currentEvent = nextEvent;
            eventSheetBinder.ResultText = summary;
            var cashDelta = gameSession.ActiveSave.state.finances.cashMinorUnits - cashBefore;
            var careerDelta = gameSession.ActiveSave.state.career.careerProgress - careerProgressBefore;
            var ageDelta = gameSession.ActiveSave.state.character.age - ageBefore;
            var happinessDelta = gameSession.ActiveSave.state.character.happiness - happinessBefore;
            eventSheetBinder.EffectsText = BuildMonthlyEffectSummary(cashDelta, careerDelta, happinessDelta, ageDelta);
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ResultVisible = true;
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
                eventSheetBinder.ChoicesVisible = false;
                eventSheetBinder.CategoryText = "MONTHLY SUMMARY";
                eventSheetBinder.TitleText = $"Month {gameSession.ActiveSave.state.calendar.monthOfYear} complete";
                eventSheetBinder.BodyText = string.IsNullOrEmpty(gameSession.ActiveSave.state.career.roleTitle)
                    ? "Time moved forward and this month's life changes were applied."
                    : "Your income, expenses, and monthly stat changes were applied.";
                OpenShellModal(ShellModal.Event);
                eventSheetBinder.ContinueVisible = true;
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
                eventSheetBinder.ResultText = summary;
                eventSheetBinder.EffectsText = "No changes applied";
                eventSheetBinder.EffectsVisible = true;
                eventSheetBinder.ResultVisible = true;
                OpenShellModal(ShellModal.Event);
                eventSheetBinder.ContinueVisible = true;
                return;
            }

            queuedYearMonthsRemaining = Math.Max(0, queuedYearMonthsRemaining - monthsProcessed);
            currentEvent = nextEvent;
            eventSheetBinder.ResultText = summary;
            var totalCommitted = 12 - queuedYearMonthsRemaining;
            eventSheetBinder.EffectsText = $"{totalCommitted} of 12 monthly transactions committed";
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ResultVisible = true;
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

            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.CategoryText = queuedYearMonthsRemaining == 0 ? "YEAR SUMMARY" : "TIME PAUSED";
            eventSheetBinder.TitleText = queuedYearMonthsRemaining == 0
                ? "A full year moved forward"
                : $"Year advance paused · {queuedYearMonthsRemaining} month{(queuedYearMonthsRemaining == 1 ? string.Empty : "s")} remaining";
            eventSheetBinder.BodyText = queuedYearMonthsRemaining == 0
                ? "Every normal monthly change was processed and autosaved in sequence."
                : "Resolve the required choice, then Continue. The remaining months will resume automatically.";
            OpenShellModal(ShellModal.Event);
            eventSheetBinder.ContinueVisible = true;
        }

        private void PresentEvent(Event evt)
        {
            eventSheetBinder.CategoryText = $"{evt.category.ToString().ToUpperInvariant()} EVENT";
            eventSheetBinder.TitleText = evt.titleKey;
            eventSheetBinder.BodyText = evt.id == RepresentativeEvents.YearInReviewId
                ? GameSessionService.BuildAnnualReviewSummary(gameSession.ActiveSave.state)
                : evt.bodyKey;
            eventSheetBinder.ClearChoices();
            for (var index = 0; index < evt.choices.Count; index++)
            {
                var choice = evt.choices[index];
                var potentialCost = GameSessionService.CalculateChoicePotentialCost(
                    evt, choice, gameSession.ActiveSave.state, gameSession.EffectValueResolver);
                AddEventChoiceButton(choice, index, PaymentMethod.Cash,
                    potentialCost > 0 ? $" · Pay cash (up to {FormatMoney(potentialCost)})" : string.Empty);
                if (potentialCost > 0 && gameSession.ActiveSave.state.character.age >= 18)
                {
                    AddEventChoiceButton(choice, index + 1, PaymentMethod.Credit,
                        $" · Use credit (up to {FormatMoney(potentialCost)})");
                }
            }
            eventSheetBinder.ChoicesVisible = true;
            eventSheetBinder.ResultVisible = false;
            eventSheetBinder.ContinueVisible = false;
            OpenShellModal(ShellModal.Event);
        }

        private void AddEventChoiceButton(
            Choice choice,
            int visualIndex,
            PaymentMethod paymentMethod,
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
            eventSheetBinder.AddChoice(button);
        }

        private void RefreshHeader()
        {
            var state = gameSession.ActiveSave.state;
            var career = state.career;
            var netWorth = GameSessionService.CalculateNetWorth(state);
            cashValue.text = FormatCompactMoney(state.finances.cashMinorUnits);
            headerNetWorthValue.text = $"Net {FormatCompactMoney(netWorth)}";
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
            var grossMonthlyPay = career.annualSalaryMinorUnits / 12;
            var estimatedTaxes = (long)Math.Round(
                grossMonthlyPay * (state.finances.taxRateBasisPoints / 10000m),
                MidpointRounding.AwayFromZero);
            var estimatedNet = grossMonthlyPay - estimatedTaxes - state.finances.monthlyLivingExpensesMinorUnits;
            lifeOverviewBinder.Render(
                state, netWorth, estimatedNet, FormatMoney, FormatSignedMoney, ToDisplayName);
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
                PresentationStateStyler.Apply(node,
                    index == activeIndex ? PresentationState.Active :
                    index > activeIndex ? PresentationState.Locked : PresentationState.Available);
                node.EnableInClassList("complete", index < activeIndex);
            }
        }

        private void RefreshHome()
        {
            var state = gameSession.ActiveSave.state;
            var home = state.home ?? new HomeState();
            var requiredProgress = GameSessionService.GetHomeUpgradeRequiredProgress(home.upgradeLevel);
            var definition = HomeContentCatalog.Get(home.homeId) ??
                             HomeContentCatalog.Get("starter_home");
            homeBinder.Render(
                state, definition, requiredProgress, FormatMoney, ToDisplayName,
                selectedHomeObjectId, SelectHomeObject,
                PerformHomeAction, PerformHomeUpgrade);
        }

        private void SelectHomeObject(string objectId)
        {
            selectedHomeObjectId = objectId;
            RefreshHome();
            PersistNavigationState();
        }

        private void PerformHomeAction(HomeActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformHomeAction(actionType, out var summary);
            homeBinder.ShowTransactionResult(succeeded, summary);
            if (succeeded || !FeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("home.last-action");
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
            homeBinder.ShowTransactionResult(succeeded, summary);
            if (succeeded || !FeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("home.last-action");
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
            var experience = GameSessionService.GetSkillExperience(
                gameSession.ActiveSave.state.skills, skillId);
            var level = GameSessionService.GetSkillLevel(experience);
            var levelStart = GameSessionService.GetExperienceForSkillLevel(level);
            var nextLevelAt = GameSessionService.GetExperienceForSkillLevel(level + 1);
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
                () => FeedbackPresenter.Clear(savingsTransferFeedback),
                PerformCreditRepayment, PerformIndexInvestment, PerformPropertyAction);
        }

        private void SetBankTab(BankTab tab)
        {
            var adult = (gameSession?.ActiveSave?.state?.character?.age ?? 0) >= 18;
            selectedBankTab = bankBinder.SelectTab(!adult ? BankTab.Savings : tab);
        }

        private void SetSavingsTransferType(SavingsTransferType transferType)
        {
            savingsTransferType = transferType;
            FeedbackPresenter.Clear(savingsTransferFeedback);
            RefreshMoney();
        }

        private void PerformSavingsTransfer(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var requestedTransferType = savingsTransferType;
            var succeeded = gameSession.TryTransferSavings(
                savingsTransferType, amountMinorUnits, out var summary);
            FeedbackPresenter.ShowTransactionResult(savingsTransferFeedback, succeeded, summary);
            if (succeeded || !FeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("bank.savings-transfer");
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
            FeedbackPresenter.ShowTransactionResult(savingsTransferFeedback, true, summary);
        }

        private void PerformCreditRepayment(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryRepayHouseholdCredit(amountMinorUnits, out var summary);
            FeedbackPresenter.ShowTransactionResult(creditRepaymentFeedback, succeeded, summary);
            if (succeeded || !FeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("bank.credit-repayment");
            else retryCommands.Register("bank.credit-repayment", () => PerformCreditRepayment(amountMinorUnits));
            RefreshBankRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            FeedbackPresenter.ShowTransactionResult(creditRepaymentFeedback, true, summary);
        }

        private void PerformIndexInvestment(long amountMinorUnits)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryInvestInIndexFund(amountMinorUnits, out var summary);
            FeedbackPresenter.ShowTransactionResult(indexInvestmentFeedback, succeeded, summary);
            if (succeeded || !FeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("bank.index-investment");
            else retryCommands.Register("bank.index-investment", () => PerformIndexInvestment(amountMinorUnits));
            RefreshBankRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            FeedbackPresenter.ShowTransactionResult(indexInvestmentFeedback, true, summary);
        }

        private void PerformPropertyAction(PropertyActionType actionType, string propertyId)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformPropertyAction(actionType, propertyId, out var summary);
            bankBinder.ShowPropertyResult(succeeded, summary);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            bankBinder.ShowPropertyResult(true, summary);
        }

        private void RefreshAchievements()
        {
            goalsBinder.Render(
                gameSession.ActiveSave.state.achievements,
                gameSession.GetGoals(),
                FormatMoney,
                FormatCompactProgress,
                ToDisplayName,
                selectedGoalsBoard,
                HandleGoalAction,
                ToggleGoalPin,
                ClaimAchievementReward);
        }

        private void SetGoalsBoard(string board)
        {
            selectedGoalsBoard = board;
            RefreshAchievements();
            PersistNavigationState();
        }

        private void ToggleGoalPin(GoalState goal)
        {
            if (goal == null) return;
            gameSession.TryPinGoal(goal.goalId, out _);
            RefreshAchievements();
        }

        private void HandleGoalAction(GoalState goal)
        {
            if (goal == null || PresentPendingEventIfAvailable()) return;
            if (goal.status == "claimable")
            {
                gameSession.TryClaimGoalReward(goal.goalId, out _);
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
        }

        private void ClaimAchievementReward(string achievementId)
        {
            if (PresentPendingEventIfAvailable()) return;
            gameSession.TryClaimAchievementReward(achievementId, out _);
            RefreshHeader();
            RefreshFeed();
            RefreshMoney();
            RefreshAchievements();
        }

        private void RefreshEducation()
        {
            var state = gameSession.ActiveSave.state;
            matchBinder.Card.EnableInClassList("hidden", state.character.age < 14 || state.character.age >= 18);
            matchBinder.Render(state.matchSession, SwapStudyMatchTiles);
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
                    PresentationStateStyler.Apply(educationEmptyState, PresentationState.Error);
                }
                else if (leftSchool)
                {
                    educationUnavailableCopy.text =
                        "School was left before graduation. Completed learning remains saved, while qualification-gated paths stay locked.";
                    PresentationStateStyler.Apply(educationEmptyState, PresentationState.Terminal);
                }
                else if (state.character.age < 6)
                {
                    educationUnavailableCopy.text =
                        "Formal school actions begin at age 6. Childhood choices still shape future learning.";
                    PresentationStateStyler.Apply(educationEmptyState, PresentationState.Locked);
                }
                else if (education.graduatedSecondary || education.stage == "completed_secondary")
                {
                    var qualificationXp = Math.Max(0, education.qualificationExperience);
                    var track = string.IsNullOrEmpty(education.studyTrack)
                        ? "No specialist track"
                        : $"{ToDisplayName(education.studyTrack)} track";
                    educationUnavailableCopy.text =
                        $"Secondary education completed · {track} · " +
                        $"{EducationActionService.GetQualificationTier(qualificationXp)} · {qualificationXp} qualification XP.";
                    PresentationStateStyler.Apply(educationEmptyState, PresentationState.Claimed);
                }
                else
                {
                    educationUnavailableCopy.text =
                        "This life has no active school enrollment. Completed learning remains part of the saved life state.";
                    PresentationStateStyler.Apply(educationEmptyState, PresentationState.Empty);
                }
                return;
            }

            PresentationStateStyler.Apply(educationCard, PresentationState.Active);

            RefreshEducationCatalog(state);

            var experience = GameSessionService.GetSkillExperience(state.skills, "learning");
            var level = GameSessionService.GetSkillLevel(experience);
            var levelStart = GameSessionService.GetExperienceForSkillLevel(level);
            var nextLevelAt = GameSessionService.GetExperienceForSkillLevel(level + 1);
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
                    ? new[] { SchoolPathChoice.AcademicTrack, SchoolPathChoice.VocationalTrack, SchoolPathChoice.LeaveSchool }
                    : state.education.awaitingDecisionId == "education_middle_transition"
                        ? new[] { SchoolPathChoice.PublicSchool, SchoolPathChoice.Homeschool, SchoolPathChoice.LeaveSchool }
                        : new[] { SchoolPathChoice.PublicSchool, SchoolPathChoice.Homeschool };
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
                AddStudyTrackCard(StudyTrack.General, 0L,
                    "A flexible curriculum with no material fee.");
                AddStudyTrackCard(StudyTrack.Academic, 5000L,
                    "A theory-focused path preparing for advanced study.");
                AddStudyTrackCard(StudyTrack.Vocational, 7500L,
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
                    if (!Enum.TryParse(suffix, true, out StudyDifficulty difficulty)) continue;
                    var capturedDifficulty = difficulty;
                    educationActions.Add(ActionCardFactory.Create(
                        definition,
                        () => ShowStudySessionSheet(capturedDifficulty, definition),
                        actionCardTemplate));
                }
                return;
            }
            foreach (var definition in gameSession.GetEducationActionDefinitions())
            {
                var suffix = definition.id.Substring("education.".Length);
                if (!Enum.TryParse(suffix, true, out EducationActionType action))
                {
                    continue;
                }
                var capturedAction = action;
                educationActions.Add(ActionCardFactory.Create(
                    definition,
                    () => PerformEducationAction(capturedAction),
                    actionCardTemplate));
            }
        }

        private void StartStudyMatch()
        {
            if (PresentPendingEventIfAvailable()) return;
            var instanceId = $"study-match-{gameSession.ActiveSave.lifeId}-{gameSession.ActiveSave.revision + 1}";
            var succeeded = gameSession.TryStartMatch("study_match", instanceId, out var summary);
            matchBinder.ShowResult(succeeded, summary);
            RefreshEducation();
        }

        private void SwapStudyMatchTiles(int first, int second)
        {
            var succeeded = gameSession.TrySwapMatchTiles(first, second, out var summary);
            matchBinder.ShowResult(succeeded, summary);
            RefreshEducation();
        }

        private void ToggleStudyMatchPause()
        {
            var paused = gameSession.ActiveSave.state.matchSession?.state == "paused";
            var succeeded = paused
                ? gameSession.TryResumeMatch(out var summary)
                : gameSession.TryPauseMatch(out summary);
            matchBinder.ShowResult(succeeded, summary);
            RefreshEducation();
        }

        private void ClaimStudyMatch()
        {
            var succeeded = gameSession.TryClaimMatchReward(out var summary);
            matchBinder.ShowResult(succeeded, summary);
            RefreshEducation();
            RefreshHeader();
            RefreshFeed();
        }

        private void RefreshEducationCatalog(GameState state)
        {
            studyBinder.RenderCatalog(state, educationActions, FormatMoney);
        }

        private void AddStudyTrackCard(StudyTrack track, long costMinorUnits, string description)
        {
            var affordable = gameSession.ActiveSave.state.finances != null &&
                             gameSession.ActiveSave.state.finances.cashMinorUnits >= costMinorUnits;
            var card = new VisualElement { name = $"study-track-card-{track.ToString().ToLowerInvariant()}" };
            card.AddToClassList("st-action-card");
            PresentationStateStyler.Apply(card,
                affordable ? PresentationState.Available : PresentationState.Locked);

            var discipline = EducationDisciplineCatalog.GetForTrack(track);
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
            var nextTierAt = EducationActionService.GetNextQualificationTierAt(experience);
            var summary = new VisualElement { name = "qualification-summary" };
            summary.AddToClassList("st-qualification-summary");
            var track = new Label($"{ToDisplayName(education.studyTrack)} Track");
            track.AddToClassList("st-action-card-title");
            var tier = new Label(EducationActionService.GetQualificationTier(experience));
            tier.AddToClassList("st-education-level");
            var badges = new VisualElement { name = "qualification-badges" };
            badges.AddToClassList("st-qualification-badges");
            AddQualificationBadge(badges, "foundation", "Foundation", 0, experience);
            AddQualificationBadge(badges, "certificate", "Certificate",
                EducationActionService.CertificateQualificationExperience, experience);
            AddQualificationBadge(badges, "diploma", "Diploma",
                EducationActionService.DiplomaQualificationExperience, experience);
            AddQualificationBadge(badges, "advanced", "Advanced",
                EducationActionService.AdvancedQualificationExperience, experience);
            var progress = new Label(
                experience >= EducationActionService.AdvancedQualificationExperience
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
            var currentThreshold = experience >= EducationActionService.AdvancedQualificationExperience
                ? EducationActionService.AdvancedQualificationExperience
                : experience >= EducationActionService.DiplomaQualificationExperience
                    ? EducationActionService.DiplomaQualificationExperience
                    : experience >= EducationActionService.CertificateQualificationExperience
                        ? EducationActionService.CertificateQualificationExperience
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
            PresentationStateStyler.Apply(badge,
                earned ? PresentationState.Claimed : PresentationState.Locked);
            badges.Add(badge);
        }

        private void ShowStudySessionSheet(
            StudyDifficulty difficulty,
            ActionDefinition definition)
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
            OpenShellModal(ShellModal.StudySession);
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
            shellBinder.CloseModal(ShellModal.StudySession);
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

        private void StartTimedStudySession(StudyDifficulty difficulty)
        {
            if (PresentPendingEventIfAvailable()) return;
            var instanceId = $"focused-study-{gameSession.ActiveSave.revision + 1}-{difficulty.ToString().ToLowerInvariant()}";
            var succeeded = gameSession.TryStartStudySession(difficulty, instanceId, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "STUDY IN PROGRESS" : "SESSION LOCKED";
            eventSheetBinder.TitleText = $"{difficulty} Study Session";
            eventSheetBinder.BodyText = succeeded
                ? "Your session is saved. Return to Education to claim its rewards when the timer completes."
                : "Review the study-track, Smarts, and monthly-action requirements.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshEducation();
            RefreshHeader();
            RefreshFeed();
        }

        private void AddStudySessionProgress(GameState state)
        {
            if (state.actionProgress == null) return;
            foreach (var action in state.actionProgress)
            {
                if (action == null || string.IsNullOrEmpty(action.actionId) ||
                    !action.actionId.StartsWith("education.study.", StringComparison.Ordinal) ||
                    action.state == ActionState.Complete.ToString()) continue;
                var ready = gameSession.IsActionReadyToClaim(action);
                var paused = action.state == ActionState.Paused.ToString();
                var expired = action.state == ActionState.Expired.ToString();
                var card = new VisualElement { name = $"study-progress-{action.instanceId}" };
                card.AddToClassList("st-action-card");
                card.AddToClassList("st-study-progress-card");
                var title = new Label(ToDisplayName(action.actionId.Substring("education.study.".Length)) + " Study Session");
                title.AddToClassList("st-action-card-title");
                var status = new Label(expired ? "Expired · no reward granted" :
                    paused ? "Paused · timer safely retained" :
                    ready ? "Complete · reward ready to claim" : "In progress · rewards pending");
                status.AddToClassList("st-action-card-progress");
                var capturedInstanceId = action.instanceId;
                var claim = new Button(() => ClaimTimedStudySession(capturedInstanceId))
                {
                    name = $"study-claim-{action.instanceId}",
                    text = expired ? "EXPIRED" : paused ? "PAUSED" : ready ? "CLAIM REWARD" : "IN PROGRESS"
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
            eventSheetBinder.CategoryText = succeeded ? "QUALIFICATION PROGRESS" : "SESSION NOT READY";
            eventSheetBinder.TitleText = "Study Session Reward";
            eventSheetBinder.BodyText = succeeded
                ? "Your completed focused study advanced your selected qualification."
                : "The session must finish before its reward can be claimed.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshEducation();
            RefreshHeader();
            RefreshFeed();
        }

        private void PerformStudyTrackChoice(StudyTrack track)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryChooseStudyTrack(track, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "STUDY TRACK" : "TRACK LOCKED";
            eventSheetBinder.TitleText = $"{track} Track";
            eventSheetBinder.BodyText = succeeded
                ? "This track shapes the qualifications, careers, and events available later."
                : "Review the age, cash, and previous-selection requirements.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
        }

        private void PerformSchoolPathChoice(SchoolPathChoice choice)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryChooseSchoolPath(choice, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "LIFE PATH" : "PATH LOCKED";
            eventSheetBinder.TitleText = ToDisplayName(choice.ToString());
            eventSheetBinder.BodyText = succeeded
                ? "This decision is now part of your permanent life history and can affect later opportunities."
                : "This path is not available at the current school transition.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
        }

        private void PerformEducationAction(EducationActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformEducationAction(actionType, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "SCHOOL PROGRESS" : "ACTION LOCKED";
            eventSheetBinder.TitleText = ToDisplayName(actionType.ToString());
            eventSheetBinder.BodyText = succeeded
                ? "Your school effort improved your learning path."
                : "Meet the requirement or advance the month before trying again.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
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
            var displayName = string.IsNullOrEmpty(character.firstName)
                ? "Your story"
                : $"{character.firstName} {character.lastName}";
            var status = character.lifeStatus == "retired"
                ? $"Retired at age {character.endedAtAge}"
                : $"Remembered at age {character.endedAtAge}";
            finalLifeBinder.Render(displayName, status, GameSessionService.BuildFinalLifeSummary(save));
            advanceMonth.SetEnabled(false);
            advanceYear.SetEnabled(false);
            shellBinder.CloseModal(ShellModal.Event);
            OpenShellModal(ShellModal.FinalLifeSummary);
        }

        private void OpenNewLifeFromEnding()
        {
            shellBinder.CloseModal(ShellModal.FinalLifeSummary);
            ShowNewLifeSetup(false, true);
        }

        private void RefreshCareer()
        {
            var state = gameSession.ActiveSave.state;
            var adult = state.character.age >= 18;
            workBinder.RenderWorkspace(state, businessWorkspaceSelected, FormatMoney);
            careerEmptyState.EnableInClassList("hidden", !adult);
            careerCard.EnableInClassList("hidden", !adult || businessWorkspaceSelected);
            careerActionsCard.EnableInClassList("hidden", !adult);
            workBinder.RenderPathPreview(state, adult);
            if (!adult) return;

            var career = state.career ?? new CareerState();
            var retired = career.roleTitle == "Retired";
            var employed = !string.IsNullOrEmpty(career.roleTitle) && !retired;
            careerRole.text = retired ? "Retired" : employed ? career.roleTitle : "Unemployed";
            careerSalary.text = retired
                ? "Career complete"
                : $"{FormatMoney(career.annualSalaryMinorUnits)} annual salary";

            if (GameSessionService.TryGetNextCareerStep(
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
                CareerActionType.Apply,
                CareerActionType.Interview,
                CareerActionType.WorkHard,
                CareerActionType.AskForPromotion,
                CareerActionType.Retrain,
                CareerActionType.Quit,
                CareerActionType.Retire
            };
            var usedThisMonth = state.statuses.Exists(status => status.statusId == "monthly_career_action_used");
            if (!businessWorkspaceSelected) foreach (var action in actions)
            {
                if (action == CareerActionType.Retire && state.character.age < 65) continue;
                var unlocked = GameSessionService.TryGetCareerActionRequirement(state, action, out var requirement);
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
            var business = state.business ?? new BusinessState();
            if (businessWorkspaceSelected) foreach (var action in new[]
                     {
                         BusinessActionType.Start, BusinessActionType.Work,
                         BusinessActionType.Upgrade, BusinessActionType.HireStaff,
                         BusinessActionType.ExpandLocation, BusinessActionType.Sell
                     })
            {
                var available = TryGetBusinessActionRequirement(state, business, action, out var requirement);
                var actionLabel = action == BusinessActionType.Start
                    ? "Start Local Services\n$1,000 · Professional Level 2"
                    : action == BusinessActionType.Work
                        ? $"Work Business\nProgress {business.operatingProgress} / 100"
                        : action == BusinessActionType.Upgrade
                            ? $"Upgrade Business\nLevel {business.level} / 3"
                            : action == BusinessActionType.HireStaff
                                ? $"Hire Staff\n{business.staffCount} / {business.level * 2} · $750"
                                : action == BusinessActionType.ExpandLocation
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

        private void SetWorkWorkspace(bool businessSelected)
        {
            businessWorkspaceSelected = businessSelected;
            RefreshCareer();
            PersistNavigationState();
        }

        private static bool TryGetBusinessActionRequirement(
            GameState state,
            BusinessState business,
            BusinessActionType action,
            out string requirement)
        {
            requirement = string.Empty;
            if (state.character.age < 18) return Fail("Unlocks at age 18", out requirement);
            if (state.character.lifeStatus != "active") return Fail("This life has ended", out requirement);
            if (!string.IsNullOrEmpty(state.pendingEventId)) return Fail("Resolve the pending event", out requirement);

            switch (action)
            {
                case BusinessActionType.Start:
                    if (business.status != "none") return Fail("A business path already exists", out requirement);
                    var professionalLevel = GameSessionService.GetSkillLevel(
                        GameSessionService.GetSkillExperience(state.skills, "professional"));
                    if (professionalLevel < 2) return Fail("Requires Professional Level 2", out requirement);
                    if (state.finances.cashMinorUnits < 100000) return Fail("Requires $1,000 cash", out requirement);
                    return true;
                case BusinessActionType.Work:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    return true;
                case BusinessActionType.Upgrade:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.level >= 3) return Fail("Maximum business level reached", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    var progressRequired =
                        ProgressionStandards.GetBusinessUpgradeProgressRequired(business.level);
                    if (business.operatingProgress < progressRequired)
                        return Fail($"Requires {progressRequired} operating progress", out requirement);
                    if (state.finances.cashMinorUnits < business.level * 150000L)
                        return Fail($"Requires {FormatMoney(business.level * 150000L)} cash", out requirement);
                    return true;
                case BusinessActionType.HireStaff:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.staffCount >= business.level * 2) return Fail("Upgrade before hiring more staff", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    if (state.finances.cashMinorUnits < 75000) return Fail("Requires $750 cash", out requirement);
                    return true;
                case BusinessActionType.ExpandLocation:
                    if (business.status != "operating") return Fail("Start a business first", out requirement);
                    if (business.level < 2) return Fail("Requires business Level 2", out requirement);
                    if (business.locationLevel >= 3) return Fail("Maximum location tier reached", out requirement);
                    if (business.actionPoints < 1) return Fail("No action points remain this month", out requirement);
                    if (state.finances.cashMinorUnits < business.locationLevel * 300000L)
                        return Fail($"Requires {FormatMoney(business.locationLevel * 300000L)} cash", out requirement);
                    return true;
                case BusinessActionType.Sell:
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

        private void PerformCareerAction(CareerActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformCareerAction(actionType, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "CAREER UPDATE" : "CAREER ACTION LOCKED";
            eventSheetBinder.TitleText = ToDisplayName(actionType.ToString());
            eventSheetBinder.BodyText = succeeded
                ? "Your career path changed and the result was saved."
                : "Meet the displayed requirement or advance the month before trying again.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            if (gameSession.ActiveSave.state.character.lifeStatus != "active")
            {
                ShowFinalLifeSummary();
            }
        }

        private void PerformBusinessAction(BusinessActionType actionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformBusinessAction(actionType, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "BUSINESS UPDATE" : "BUSINESS ACTION LOCKED";
            eventSheetBinder.TitleText = ToDisplayName(actionType.ToString());
            eventSheetBinder.BodyText = succeeded
                ? "Your business changed and the result was saved."
                : "Meet the displayed requirement or advance the month before trying again.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
        }

        private void ConfigureAgeAppropriateActivities(int age)
        {
            if (age < 5)
            {
                primaryFocusActivity = ActivityType.Play;
                secondaryFocusActivity = ActivityType.Rest;
                focusStudyTitle.text = "Play";
                focusStudyEffect.text = "+ Happiness · + Health";
                focusWorkoutTitle.text = "Rest";
                focusWorkoutEffect.text = "+ Health · + Happiness";
            }
            else if (age < 13)
            {
                primaryFocusActivity = ActivityType.Study;
                secondaryFocusActivity = ActivityType.Play;
                focusStudyTitle.text = "Study";
                focusStudyEffect.text = "+ Smarts";
                focusWorkoutTitle.text = "Play";
                focusWorkoutEffect.text = "+ Happiness · + Health";
            }
            else
            {
                primaryFocusActivity = ActivityType.Study;
                secondaryFocusActivity = ActivityType.Workout;
                focusStudyTitle.text = "Study";
                focusStudyEffect.text = "+ Smarts";
                focusWorkoutTitle.text = "Workout";
                focusWorkoutEffect.text = "+ Health";
            }
            focusStudy.SetEnabled(true);
            focusWorkout.SetEnabled(true);
            RefreshContextActivities(gameSession.ActiveSave.state);
        }

        private void RefreshContextActivities(GameState state)
        {
            contextActivities.Clear();
            ActivityType[] activities;
            var employed = !string.IsNullOrEmpty(state.career?.roleTitle) && state.career.roleTitle != "Retired";
            if (state.character.age < 5)
                activities = new[] { ActivityType.FamilyTime, ActivityType.FamilyMovie, ActivityType.Explore };
            else if (state.character.age < 13)
                activities = new[] { ActivityType.AttendSchool, ActivityType.FamilyTime, ActivityType.FamilyMovie, ActivityType.Explore };
            else if (state.character.age < 18)
                activities = new[] { ActivityType.AttendSchool, ActivityType.JoinClub, ActivityType.FamilyMovie, ActivityType.Socialize };
            else if (state.character.age >= 65)
                activities = new[] { ActivityType.Hobby, ActivityType.FamilyTime, ActivityType.FamilyMovie, ActivityType.FamilyMovieCredit, ActivityType.Socialize, ActivityType.Checkup };
            else if (employed)
                activities = new[] { ActivityType.WorkShift, ActivityType.Overtime, ActivityType.Training, ActivityType.FamilyTime, ActivityType.FamilyMovie, ActivityType.FamilyMovieCredit, ActivityType.Socialize };
            else
                activities = new[] { ActivityType.Training, ActivityType.FamilyTime, ActivityType.FamilyMovie, ActivityType.FamilyMovieCredit, ActivityType.Socialize, ActivityType.Rest };

            var usedThisMonth = state.statuses.Exists(status => status.statusId == "monthly_focus_used");
            foreach (var activity in activities)
            {
                var available = GameSessionService.TryGetActivityRequirement(state, activity, out var requirement);
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
                eventSheetBinder.ContinueText = "Continue";
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
                    eventSheetBinder.ResultText = orientationSummary;
                    eventSheetBinder.EffectsText = "Orientation remains open until progress can be saved";
                    return;
                }
                presentingFirstLifeOrientation = false;
                RefreshFeed();
            }
            if (PresentPendingEventIfAvailable()) return;
            if (queuedYearMonthsRemaining > 0 &&
                !string.IsNullOrEmpty(gameSession.ActiveSave.state.education?.awaitingDecisionId))
            {
                shellBinder.CloseModal(ShellModal.Event);
                eventSheetBinder.ResultVisible = false;
                eventSheetBinder.ContinueVisible = false;
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
            shellBinder.CloseModal(ShellModal.Event);
            eventSheetBinder.ResultVisible = false;
            eventSheetBinder.ContinueVisible = false;
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
            eventSheetBinder.CategoryText = "YEAR SUMMARY";
            eventSheetBinder.TitleText = "A full year moved forward";
            eventSheetBinder.BodyText = "Every normal monthly change was processed and autosaved in sequence.";
            eventSheetBinder.ResultText = completionSummary;
            eventSheetBinder.EffectsText = "12 of 12 monthly transactions committed";
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
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
            eventSheetBinder.CategoryText = "LIFE MILESTONE";
            eventSheetBinder.TitleText = transition.title;
            eventSheetBinder.BodyText = transition.summary;
            eventSheetBinder.ResultText = $"Age {transition.age} · {GetMonthName(transition.monthOfYear)}";
            eventSheetBinder.EffectsText = "Saved to your Life Feed";
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
        }

        private void PresentFirstLifeOrientation()
        {
            presentingFirstLifeOrientation = true;
            currentEvent = null;
            eventSheetBinder.CategoryText = "WELCOME TO STIM TYCOON";
            eventSheetBinder.TitleText = "Your life, one choice at a time";
            eventSheetBinder.BodyText =
                "The Life Feed records what changes. Advance Month for detail or Advance Year for faster pacing—time always pauses for choices. Locked actions show what they require, and every completed action autosaves locally.";
            eventSheetBinder.ResultText = "Nothing here costs premium currency, and ordinary progression remains available.";
            eventSheetBinder.EffectsText = "One screen · Continue when ready";
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
        }

        private void PerformActivity(ActivityType activityType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformActivity(activityType, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "FOCUS COMPLETE" : "FOCUS UNAVAILABLE";
            eventSheetBinder.TitleText = $"{ToDisplayName(activityType.ToString())} session";
            eventSheetBinder.BodyText = succeeded
                ? "Your focused action has been applied to this month."
                : "Choose another step after resolving the current requirement.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
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
            NavigateTo(Destination.Life);
            PresentPendingEventIfAvailable();
        }

        private void ShowEducationDestination()
        {
            RefreshEducation();
            RefreshSkills();
            NavigateTo(Destination.Study);
            PresentPendingEventIfAvailable();
        }

        private void ShowCareerDestination()
        {
            RefreshCareer();
            NavigateTo(Destination.Work);
            PresentPendingEventIfAvailable();
        }

        private void ShowMoneyDestination()
        {
            RefreshMoney();
            NavigateTo(Destination.Bank);
            PresentPendingEventIfAvailable();
        }

        private void ShowSocialDestination()
        {
            RefreshSocial();
            NavigateTo(Destination.Social);
            if (string.IsNullOrEmpty(selectedRelationshipId)) ShowRelationshipList();
            PresentPendingEventIfAvailable();
        }

        private void ShowGoalsDestination()
        {
            RefreshAchievements();
            NavigateTo(Destination.Goals);
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
            eventSheetBinder.CategoryText = "CONTENT RECOVERY";
            eventSheetBinder.TitleText = "A pending life event could not be loaded";
            eventSheetBinder.BodyText = "This save needs event content that is unavailable in the current build. Update or restore the matching build, then reopen this life.";
            eventSheetBinder.ResultText = "No progress was changed.";
            eventSheetBinder.EffectsText = "The unavailable event was preserved for safe recovery.";
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = false;
            OpenShellModal(ShellModal.Event);
            return true;
        }

        private void NavigateTo(
            Destination destination, bool restoreScroll = true, bool persistState = true)
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

        private ScrollView GetDestinationView(Destination destination)
        {
            switch (destination)
            {
                case Destination.Study: return educationView;
                case Destination.Work: return careerView;
                case Destination.Bank: return moneyView;
                case Destination.Social: return socialView;
                case Destination.Goals: return goalsView;
                default: return lifeScroll;
            }
        }

        private void PerformManualWorkTap()
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformManualWorkTap(out _, out var summary);
            FeedbackPresenter.ShowTransactionResult(manualWorkFeedback, succeeded, summary);
            if (succeeded || !FeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("work.manual");
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
            socialBinder.RenderList(gameSession.ActiveSave.state, selectedSocialFilter, id => ShowRelationshipDetail(id));
        }

        private void SetSocialFilter(string filter)
        {
            selectedSocialFilter = filter;
            ShowRelationshipList();
            RefreshSocial();
        }

        private void ShowRelationshipDetail(string relationshipId, bool persistState = true)
        {
            var relationship = gameSession.ActiveSave.state.relationships.Find(
                candidate => candidate != null && candidate.relationshipId == relationshipId);
            if (relationship == null) return;

            selectedRelationshipId = relationshipId;
            socialBinder.ShowDetail(relationship);
            BuildRelationshipActions(relationship);
            if (relationship.relationshipType == "deceased" || relationship.relationshipType == "unavailable")
                socialBinder.RelationshipActions.Clear();
            if (persistState) PersistNavigationState();
        }

        private void DiscoverCompatiblePerson()
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryDiscoverCompatiblePerson(out var relationshipId, out var summary);
            FeedbackPresenter.ShowTransactionResult(relationshipDiscoveryFeedback, succeeded, summary);
            if (succeeded || !FeedbackPresenter.IsRetryable(summary)) retryCommands.Clear("social.discovery");
            else retryCommands.Register("social.discovery", DiscoverCompatiblePerson);
            RefreshDestinationRetryButtons();
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(relationshipId);
        }

        private void BuildRelationshipActions(RelationshipState relationship)
        {
            socialBinder.RelationshipActions.Clear();
            var interactions = new[]
            {
                RelationshipInteractionType.Talk,
                RelationshipInteractionType.PlayTogether,
                RelationshipInteractionType.AskForHelp,
                RelationshipInteractionType.SpendTime,
                RelationshipInteractionType.Argue,
                RelationshipInteractionType.Compete,
                RelationshipInteractionType.Reconcile,
                RelationshipInteractionType.DeepenFriendship,
                RelationshipInteractionType.AskOnDate,
                RelationshipInteractionType.DateNight,
                RelationshipInteractionType.Commit,
                RelationshipInteractionType.BreakUp,
                RelationshipInteractionType.Separate,
                RelationshipInteractionType.Recover
            };
            var age = gameSession.ActiveSave.state.character.age;
            var cooldownId = $"relationship_interaction_used_{relationship.relationshipId}";
            var usedThisMonth = gameSession.ActiveSave.state.statuses.Exists(status => status.statusId == cooldownId);
            foreach (var interaction in interactions)
            {
                if (!GameSessionService.IsRelationshipInteractionAgeAppropriate(interaction, age)) continue;
                if (interaction == RelationshipInteractionType.Compete && relationship.relationshipType == "parent") continue;
                if (interaction == RelationshipInteractionType.Reconcile && relationship.relationshipType != "rival") continue;
                if (interaction == RelationshipInteractionType.DeepenFriendship &&
                    ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                     relationship.value < 65)) continue;
                if (interaction == RelationshipInteractionType.AskOnDate &&
                    ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                     relationship.value < 60)) continue;
                if (interaction == RelationshipInteractionType.Commit &&
                    (relationship.relationshipType != "dating" || relationship.value < 75)) continue;
                if (interaction == RelationshipInteractionType.DateNight &&
                    relationship.relationshipType != "dating" && relationship.relationshipType != "partner" &&
                    relationship.relationshipType != "engaged" && relationship.relationshipType != "married") continue;
                if (interaction == RelationshipInteractionType.BreakUp &&
                    relationship.relationshipType != "dating" && relationship.relationshipType != "partner") continue;
                if (interaction == RelationshipInteractionType.Separate &&
                    relationship.relationshipType != "partner" && relationship.relationshipType != "engaged") continue;
                if (interaction == RelationshipInteractionType.Recover &&
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
                AddFamilyPlanningAction(relationship, FamilyPlanningAction.Discuss, "Discuss family planning");
                AddFamilyPlanningAction(relationship, FamilyPlanningAction.TryForChild, "Try for a child · 9 months");
                AddFamilyPlanningAction(relationship, FamilyPlanningAction.PursueAdoption, "Pursue adoption · $500 · 6 months");
                AddFamilyPlanningAction(relationship, FamilyPlanningAction.OptOut, "Not now");
            }
            if (relationship.relationshipType == "child")
            {
                AddParentingAction(relationship, ParentingAction.QualityTime, "Quality time · Wellbeing and relationship");
                AddParentingAction(relationship, ParentingAction.SupportNeeds, "Support needs · $25 · Wellbeing");
                AddParentingAction(relationship, ParentingAction.Teach, "Teach · Learning and independence");
                AddParentingAction(relationship, ParentingAction.SetBoundaries, "Set boundaries · Independence");
            }
        }

        private void AddParentingAction(
            RelationshipState relationship,
            ParentingAction action,
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
            button.SetEnabled(!used && (action != ParentingAction.SupportNeeds ||
                                        gameSession.ActiveSave.state.finances.cashMinorUnits >= 2500));
            var capturedAction = action;
            button.clicked += () => PerformParentingAction(capturedAction);
            socialBinder.RelationshipActions.Add(button);
        }

        private void PerformParentingAction(ParentingAction action)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformParentingAction(selectedRelationshipId, action, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "PARENTING MOMENT" : "PARENTING ACTION UNAVAILABLE";
            eventSheetBinder.TitleText = ToDisplayName(action.ToString());
            eventSheetBinder.BodyText = "Parenting choices affect child wellbeing, learning, independence, and your relationship over time.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(selectedRelationshipId);
        }

        private void AddFamilyPlanningAction(
            RelationshipState relationship,
            FamilyPlanningAction action,
            string label)
        {
            var family = gameSession.ActiveSave.state.family;
            var agreed = family.planningPreference == "open" && family.partnerConsent &&
                         family.planningPartnerId == relationship.relationshipId;
            var pending = !string.IsNullOrEmpty(family.pendingPath);
            var used = gameSession.ActiveSave.state.statuses.Exists(
                status => status.statusId == "family_planning_used");
            var enabled = !used && (action == FamilyPlanningAction.Discuss ||
                                    action == FamilyPlanningAction.OptOut || agreed && !pending);
            if (action == FamilyPlanningAction.PursueAdoption)
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

        private void PerformFamilyPlanning(FamilyPlanningAction action)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryChooseFamilyPlanning(selectedRelationshipId, action, out var summary);
            eventSheetBinder.CategoryText = succeeded ? "FAMILY DECISION" : "FAMILY PATH UNAVAILABLE";
            eventSheetBinder.TitleText = ToDisplayName(action.ToString());
            eventSheetBinder.BodyText = "Family planning requires an eligible adult partnership and mutual agreement. Choosing not now remains available.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(selectedRelationshipId);
        }

        private void PerformRelationshipInteraction(RelationshipInteractionType interactionType)
        {
            if (PresentPendingEventIfAvailable()) return;
            var succeeded = gameSession.TryPerformRelationshipInteraction(
                selectedRelationshipId,
                interactionType,
                out var summary);
            eventSheetBinder.CategoryText = succeeded ? "SOCIAL MOMENT" : "INTERACTION UNAVAILABLE";
            eventSheetBinder.TitleText = ToDisplayName(interactionType.ToString());
            eventSheetBinder.BodyText = succeeded
                ? "This moment changed your relationship and became part of your life story."
                : "Choose another step after resolving the current requirement.";
            eventSheetBinder.ResultText = summary;
            eventSheetBinder.EffectsText = string.Empty;
            eventSheetBinder.EffectsVisible = false;
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
            if (!succeeded) return;
            RefreshHeader();
            RefreshFeed();
            RefreshSocial();
            ShowRelationshipDetail(selectedRelationshipId);
        }

        private void ShowNewLifeSetup(bool canContinue, bool canCancel)
        {
            var firstName = canContinue && gameSession.ActiveSave != null
                ? gameSession.ActiveSave.state.character.firstName
                : null;
            newLifeBinder.Configure(canContinue, canCancel, firstName);
            OpenShellModal(ShellModal.NewLife);
        }

        private void CreateLifeFromSetup()
        {
            try
            {
                var save = NewLifeFactory.Create(
                    new NewLifeRequest(),
                    Application.version,
                    DateTimeOffset.UtcNow,
                    Environment.TickCount);

                if (!gameSession.TryStartNewLife(save, out var summary))
                {
                    ShowNewLifeError(summary);
                    return;
                }

                currentEvent = null;
                shellBinder.CloseModal(ShellModal.FinalLifeSummary);
                advanceMonth.SetEnabled(true);
                advanceYear.SetEnabled(true);
                shellBinder.CloseModal(ShellModal.NewLife);
                shellBinder.CloseModal(ShellModal.Event);
                eventSheetBinder.ChoicesVisible = false;
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
            newLifeBinder.ShowError(message);
        }

        internal bool TryRetryCommand(string commandId) => retryCommands.TryExecute(commandId);

        private void OpenShellModal(ShellModal modal)
        {
            shellBinder.CaptureModalReturnContext(
                activeDestination == Destination.Bank ? selectedBankTab.ToString() :
                activeDestination == Destination.Work ? (businessWorkspaceSelected ? "business" : "career") :
                activeDestination == Destination.Goals ? selectedGoalsBoard :
                activeDestination == Destination.Social ? selectedSocialFilter : string.Empty,
                activeDestination == Destination.Social ? selectedRelationshipId :
                activeDestination == Destination.Life ? selectedHomeObjectId : string.Empty);
            shellBinder.OpenModal(modal);
        }

        private void RestoreModalReturnContext()
        {
            var destination = shellBinder.ModalReturnDestination;
            NavigateTo(destination);
            if (destination == Destination.Bank &&
                Enum.TryParse(shellBinder.ModalReturnTabId, out BankTab bankTab))
            {
                SetBankTab(bankTab);
            }
            else if (destination == Destination.Work)
            {
                SetWorkWorkspace(shellBinder.ModalReturnTabId == "business");
            }
            else if (destination == Destination.Goals && !string.IsNullOrEmpty(shellBinder.ModalReturnTabId))
            {
                SetGoalsBoard(shellBinder.ModalReturnTabId);
            }
            else if (destination == Destination.Social)
            {
                if (!string.IsNullOrEmpty(shellBinder.ModalReturnTabId))
                    selectedSocialFilter = shellBinder.ModalReturnTabId;
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
            homeBinder?.Retry.EnableInClassList("hidden", !retryCommands.IsAvailable("home.last-action"));
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
                !Enum.TryParse(navigation.activeDestination, out Destination destination))
                return;

            if (destination == Destination.Bank &&
                Enum.TryParse(navigation.selectedTabId, out BankTab bankTab))
                SetBankTab(bankTab);
            else if (destination == Destination.Work)
                businessWorkspaceSelected = navigation.selectedTabId == "business";
            else if (destination == Destination.Goals &&
                     (navigation.selectedTabId == "main" || navigation.selectedTabId == "daily" ||
                      navigation.selectedTabId == "life" || navigation.selectedTabId == "achievements"))
                selectedGoalsBoard = navigation.selectedTabId;
            else if (destination == Destination.Social &&
                     (navigation.selectedTabId == "all" || navigation.selectedTabId == "family" ||
                      navigation.selectedTabId == "friends" || navigation.selectedTabId == "romance"))
                selectedSocialFilter = navigation.selectedTabId;

            var offset = new Vector2(
                Math.Max(0f, navigation.activeScrollX),
                Math.Max(0f, navigation.activeScrollY));
            destinationScrollOffsets[destination] = offset;
            NavigateTo(destination, persistState: false);

            if (destination != Destination.Social || string.IsNullOrEmpty(navigation.selectedEntityId))
            {
                if (destination == Destination.Life && !string.IsNullOrEmpty(navigation.selectedEntityId))
                {
                    var definition = HomeContentCatalog.Get(gameSession.ActiveSave.state.home?.homeId ?? "starter_home");
                    if (definition?.actions.Exists(action => action.roomObjectId == navigation.selectedEntityId) == true)
                        selectedHomeObjectId = navigation.selectedEntityId;
                    RefreshHome();
                }
                return;
            }
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
                activeDestination == Destination.Bank ? selectedBankTab.ToString() :
                activeDestination == Destination.Work ? (businessWorkspaceSelected ? "business" : "career") :
                activeDestination == Destination.Goals ? selectedGoalsBoard :
                activeDestination == Destination.Social ? selectedSocialFilter : string.Empty,
                activeDestination == Destination.Social ? selectedRelationshipId :
                activeDestination == Destination.Life ? selectedHomeObjectId : string.Empty,
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
            eventSheetBinder.ChoicesVisible = false;
            eventSheetBinder.CategoryText = "SAVE REQUIRED";
            eventSheetBinder.TitleText = "Progress could not be saved";
            eventSheetBinder.BodyText = "No further workflow steps will run until this save succeeds.";
            eventSheetBinder.ResultText = string.IsNullOrEmpty(summary) ? "The save transaction was rolled back." : summary;
            eventSheetBinder.EffectsText = "No workflow progress applied · Retry is safe";
            eventSheetBinder.ResultVisible = true;
            eventSheetBinder.EffectsVisible = true;
            eventSheetBinder.ContinueText = "RETRY SAVE";
            eventSheetBinder.ContinueVisible = true;
            OpenShellModal(ShellModal.Event);
        }

        private bool TryPresentPersistedStudyConfirmation()
        {
            var workflow = gameSession?.ActiveSave?.state?.uiWorkflow;
            if (workflow == null || string.IsNullOrEmpty(workflow.pendingStudyActionId) ||
                !Enum.TryParse(workflow.pendingStudyDifficulty, out StudyDifficulty difficulty))
                return false;
            ActionDefinition definition = null;
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
            return MoneyFormatter.Format(minorUnits);
        }

        private static string FormatPreciseMoney(long minorUnits)
        {
            return MoneyFormatter.FormatPrecise(minorUnits);
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
            return MoneyFormatter.Format(minorUnits);
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

        private string BuildEffectSummary(IReadOnlyList<Effect> effects)
        {
            var summary = new StringBuilder();
            for (var index = 0; index < effects.Count; index++)
            {
                var effect = effects[index];
                var value = gameSession.EffectValueResolver.Resolve(effect);
                if (effect == null || Math.Abs(value) <= float.Epsilon)
                {
                    continue;
                }

                var formatted = FormatEffect(effect, value);
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

        private static string FormatEffect(Effect effect, float value)
        {
            var sign = value >= 0 ? "+" : "−";
            var absoluteValue = Math.Abs(value);
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
                    ? "Vertical slice UXML binding validation failed, but no null UI field was identified. Reimport Assets/StimTycoon/UI/VerticalSlice.uxml."
                    : $"Vertical slice UXML is missing required named elements for: {string.Join(", ", missing)}.",
                this);
        }

    }
}
