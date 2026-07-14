using System;
using System.Collections.Generic;
using System.Text;
using StimTycoon.Events;
using StimTycoon.Saves;
using UnityEngine;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class StimVerticalSliceController : MonoBehaviour
    {
        [SerializeField] private PanelSettings panelSettings;
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        private StimGameSessionService gameSession;
        private Label cashValue;
        private Label lifeSummary;
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
        private VisualElement healthFill;
        private VisualElement happinessFill;
        private VisualElement smartsFill;
        private VisualElement looksFill;
        private VisualElement luckFill;
        private Button advanceMonth;
        private Button toggleOverview;
        private Button eventContinue;
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
        private ScrollView lifeScroll;
        private ScrollView socialView;
        private VisualElement timeDock;
        private Button navLife;
        private Button navMoney;
        private Button navSocial;
        private ScrollView moneyView;
        private Label manualWorkRole;
        private Label manualWorkRate;
        private Label moneyCashValue;
        private Button manualWorkTap;
        private Label manualWorkFeedback;
        private VisualElement relationshipListView;
        private VisualElement relationshipList;
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
        private VisualElement finalLifeSummary;
        private Label endingName;
        private Label endingStatus;
        private Label endingSummary;
        private Button endingNewLife;
        private Label achievementsCount;
        private VisualElement achievementsList;
        private VisualElement rootElement;
        private StimActivityType primaryFocusActivity;
        private StimActivityType secondaryFocusActivity;
        private StimEvent currentEvent;

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTreeAsset;
            var root = document.rootVisualElement;
            rootElement = root;
            root.RegisterCallback<GeometryChangedEvent>(HandleRootGeometryChanged);
            cashValue = root.Q<Label>("cash-value");
            lifeSummary = root.Q<Label>("life-summary");
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
            careerProgressValue = root.Q<Label>("career-progress-value");
            monthlyPaycheckValue = root.Q<Label>("monthly-paycheck-value");
            annualSalaryValue = root.Q<Label>("annual-salary-value");
            netWorthValue = root.Q<Label>("net-worth-value");
            avatarGlyph = root.Q<Label>("avatar-glyph");
            choices = root.Q<VisualElement>("choices");
            resultCard = root.Q<VisualElement>("result-card");
            playerOverview = root.Q<VisualElement>("player-overview");
            careerProgressFill = root.Q<VisualElement>("career-progress-fill");
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
            lifeScroll = root.Q<ScrollView>("life-scroll");
            socialView = root.Q<ScrollView>("social-view");
            timeDock = root.Q<VisualElement>("time-dock");
            navLife = root.Q<Button>("nav-life");
            navMoney = root.Q<Button>("nav-money");
            navSocial = root.Q<Button>("nav-social");
            moneyView = root.Q<ScrollView>("money-view");
            manualWorkRole = root.Q<Label>("manual-work-role");
            manualWorkRate = root.Q<Label>("manual-work-rate");
            moneyCashValue = root.Q<Label>("money-cash-value");
            manualWorkTap = root.Q<Button>("manual-work-tap");
            manualWorkFeedback = root.Q<Label>("manual-work-feedback");
            relationshipListView = root.Q<VisualElement>("relationship-list-view");
            relationshipList = root.Q<VisualElement>("relationship-list");
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
            finalLifeSummary = root.Q<VisualElement>("final-life-summary");
            endingName = root.Q<Label>("ending-name");
            endingStatus = root.Q<Label>("ending-status");
            endingSummary = root.Q<Label>("ending-summary");
            endingNewLife = root.Q<Button>("ending-new-life");
            achievementsCount = root.Q<Label>("achievements-count");
            achievementsList = root.Q<VisualElement>("achievements-list");

            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            catalog.Upsert(RepresentativeStimEvents.CreateHealthBurnout());
            catalog.Upsert(RepresentativeStimEvents.CreateMoneyFastReturn());
            catalog.Upsert(RepresentativeStimEvents.CreateSchoolGroupProject());
            catalog.Upsert(RepresentativeStimEvents.CreateChildhoodGrownFolksTable());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomGain());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomLoss());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomGainRefund());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomLossRepair());
            catalog.Upsert(RepresentativeStimEvents.CreateLuckCrossroads());
            catalog.Upsert(RepresentativeStimEvents.CreateChildhoodDiscovery());
            catalog.Upsert(RepresentativeStimEvents.CreateChildhoodComfort());
            catalog.Upsert(RepresentativeStimEvents.CreatePeerTrustConflict());
            catalog.Upsert(RepresentativeStimEvents.CreatePeerTrustAftermath());
            catalog.Upsert(RepresentativeStimEvents.CreatePeerJealousy());
            catalog.Upsert(RepresentativeStimEvents.CreateComingOfAgeGender());
            catalog.Upsert(RepresentativeStimEvents.CreateComingOfAgeOrientation());
            catalog.Upsert(RepresentativeStimEvents.CreatePromInvitation());
            catalog.Upsert(RepresentativeStimEvents.CreateFirstKiss());
            catalog.Upsert(RepresentativeStimEvents.CreateProposal());
            catalog.Upsert(RepresentativeStimEvents.CreateWedding());
            catalog.Upsert(RepresentativeStimEvents.CreateMarriageCrossroads());
            gameSession = new StimGameSessionService(catalog, new NativeStimSaveRepository());
            var loadedExistingLife = gameSession.TryLoadLatest(out _);

            advanceMonth = root.Q<Button>("advance-month");
            toggleOverview = root.Q<Button>("toggle-overview");
            eventContinue = root.Q<Button>("event-continue");
            if (cashValue == null || lifeSummary == null || eventCategory == null || eventTitle == null ||
                eventBody == null || resultText == null || resultEffects == null || lifeFeedList == null || choices == null ||
                resultCard == null || advanceMonth == null || toggleOverview == null || playerOverview == null ||
                overviewCareer == null || overviewCalendar == null || healthValue == null ||
                happinessValue == null || smartsValue == null || looksValue == null || luckValue == null ||
                careerProgressValue == null ||
                careerProgressFill == null || monthlyPaycheckValue == null || annualSalaryValue == null ||
                netWorthValue == null || avatarGlyph == null || eventSheet == null || eventContinue == null ||
                healthFill == null || happinessFill == null || smartsFill == null || looksFill == null ||
                luckFill == null || newLifeSetup == null || newLifeError == null ||
                cancelNewLife == null || continueCurrentLife == null || createNewLife == null || openNewLife == null ||
                focusStudy == null || focusWorkout == null || focusStudyTitle == null || focusStudyEffect == null ||
                focusWorkoutTitle == null || focusWorkoutEffect == null || lifeScroll == null || socialView == null ||
                contextActivities == null ||
                timeDock == null || navLife == null || navMoney == null || navSocial == null || moneyView == null ||
                manualWorkRole == null || manualWorkRate == null || moneyCashValue == null ||
                manualWorkTap == null || manualWorkFeedback == null || relationshipListView == null ||
                relationshipList == null || relationshipDetailView == null || relationshipBack == null ||
                relationshipAvatar == null || relationshipName == null || relationshipType == null ||
                relationshipStrength == null || relationshipFill == null || relationshipGenetics == null ||
                relationshipActions == null || educationCard == null || educationStage == null ||
                learningLevel == null || learningFill == null || learningProgress == null ||
                educationActions == null || skillsList == null || careerCard == null || careerRole == null || careerSalary == null ||
                careerNextStep == null || careerActionFill == null || careerActionProgress == null ||
                careerActions == null || finalLifeSummary == null || endingName == null || endingStatus == null ||
                endingSummary == null || endingNewLife == null || achievementsCount == null ||
                achievementsList == null)
            {
                Debug.LogError("Vertical slice UXML is missing one or more required named elements.", this);
                return;
            }

            advanceMonth.clicked += AdvanceMonth;
            toggleOverview.clicked += ToggleOverview;
            eventContinue.clicked += CloseEventSheet;
            focusStudy.clicked += () => PerformActivity(primaryFocusActivity);
            focusWorkout.clicked += () => PerformActivity(secondaryFocusActivity);
            navLife.clicked += ShowLifeDestination;
            navMoney.clicked += ShowMoneyDestination;
            navSocial.clicked += ShowSocialDestination;
            manualWorkTap.clicked += PerformManualWorkTap;
            relationshipBack.clicked += ShowRelationshipList;
            endingNewLife.clicked += OpenNewLifeFromEnding;
            ConfigureNewLifeControls();

            if (!loadedExistingLife)
            {
                ShowNewLifeSetup(false, false);
                return;
            }

            if (!string.IsNullOrEmpty(gameSession.ActiveSave.state.pendingEventId))
            {
                catalog.TryGetEvent(gameSession.ActiveSave.state.pendingEventId, out currentEvent);
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
            else
            {
                choices.AddToClassList("hidden");
                advanceMonth.RemoveFromClassList("hidden");
                eventSheet.AddToClassList("hidden");
            }

            ShowNewLifeSetup(true, false);
        }

        private void OnDisable()
        {
            rootElement?.UnregisterCallback<GeometryChangedEvent>(HandleRootGeometryChanged);
        }

        private void HandleRootGeometryChanged(GeometryChangedEvent evt)
        {
            ApplyResponsiveLayout(rootElement, evt.newRect.width);
        }

        public static void ApplyResponsiveLayout(VisualElement root, float width)
        {
            if (root == null) return;
            root.EnableInClassList("st-compact-width", width > 0f && width <= 360f);
        }

        public void Configure(PanelSettings settings, VisualTreeAsset tree)
        {
            panelSettings = settings;
            visualTreeAsset = tree;
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
        }

        private void AdvanceMonth()
        {
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

        private void PresentEvent(StimEvent evt)
        {
            eventCategory.text = $"{evt.category.ToString().ToUpperInvariant()} EVENT";
            eventTitle.text = evt.titleKey;
            eventBody.text = evt.bodyKey;
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
            cashValue.text = FormatMoney(state.finances.cashMinorUnits);
            netWorthValue.text = FormatMoney(state.finances.cashMinorUnits - state.finances.debtMinorUnits);
            lifeSummary.text = string.IsNullOrEmpty(state.character.firstName)
                ? $"Age {state.character.age}"
                : $"{state.character.firstName} · {state.character.age}";
            avatarGlyph.text = string.IsNullOrEmpty(state.character.firstName)
                ? "☺"
                : state.character.firstName.Substring(0, 1).ToUpperInvariant();
            overviewCareer.text = string.IsNullOrEmpty(career.roleTitle)
                ? ToDisplayName(state.character.lifeStage)
                : $"{career.roleTitle} · Stim Financial Group";
            overviewCalendar.text = $"Age {state.character.age} · Month {state.calendar.monthOfYear} of 12";
            healthValue.text = $"{state.character.health} / 100";
            happinessValue.text = $"{state.character.happiness} / 100";
            smartsValue.text = $"{state.character.smarts} / 100";
            looksValue.text = $"{state.character.looks} / 100";
            luckValue.text = $"{state.character.luck} / 100";
            healthFill.style.width = Length.Percent(ClampFillPercent(state.character.health));
            happinessFill.style.width = Length.Percent(ClampFillPercent(state.character.happiness));
            smartsFill.style.width = Length.Percent(ClampFillPercent(state.character.smarts));
            looksFill.style.width = Length.Percent(ClampFillPercent(state.character.looks));
            luckFill.style.width = Length.Percent(ClampFillPercent(state.character.luck));
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
            RefreshMoney();
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
            var career = state.career ?? new StimCareerState();
            var employed = !string.IsNullOrEmpty(career.roleTitle) && career.roleTitle != "Retired" &&
                           career.annualSalaryMinorUnits > 0 && state.character.lifeStatus == "active";
            var hourlyRate = StimGameSessionService.CalculateHourlyRateMinorUnits(career.annualSalaryMinorUnits);
            manualWorkRole.text = employed ? career.roleTitle : "Get a salaried job to begin";
            manualWorkRate.text = employed ? $"{FormatPreciseMoney(hourlyRate)} per hour" : "$0.00 per hour";
            moneyCashValue.text = FormatMoney(state.finances.cashMinorUnits);
            manualWorkTap.text = employed
                ? $"WORK 1 HOUR  ·  +{FormatPreciseMoney(hourlyRate)}"
                : "WORK 1 HOUR";
            manualWorkTap.SetEnabled(employed && string.IsNullOrEmpty(state.pendingEventId));
        }

        private void RefreshAchievements()
        {
            achievementsList.Clear();
            var achievements = gameSession.ActiveSave.state.achievements;
            achievementsCount.text = $"{achievements.Count} unlocked";
            if (achievements.Count == 0)
            {
                var empty = new Label("Your milestones will appear here as this life unfolds.");
                empty.AddToClassList("st-feed-empty");
                achievementsList.Add(empty);
                return;
            }

            for (var index = achievements.Count - 1; index >= 0; index--)
            {
                var achievement = achievements[index];
                if (achievement == null) continue;
                var row = new VisualElement();
                row.AddToClassList("st-achievement-row");
                var icon = new Label("★");
                icon.AddToClassList("st-achievement-icon");
                var copy = new VisualElement();
                copy.AddToClassList("st-achievement-copy");
                var name = new Label(StimGameSessionService.GetAchievementDisplayName(achievement.achievementId));
                name.AddToClassList("st-achievement-name");
                var description = new Label(
                    $"{StimGameSessionService.GetAchievementDescription(achievement.achievementId)} · Age {achievement.unlockedAtAge}");
                description.AddToClassList("st-achievement-description");
                copy.Add(name);
                copy.Add(description);
                row.Add(icon);
                row.Add(copy);
                achievementsList.Add(row);
            }
        }

        private void RefreshEducation()
        {
            var state = gameSession.ActiveSave.state;
            var enrolled = state.character.age >= 6 && state.character.age < 18;
            educationCard.EnableInClassList("hidden", !enrolled);
            if (!enrolled) return;

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
                foreach (var definition in gameSession.GetStudySessionDefinitions())
                {
                    var suffix = definition.id.Substring("education.study.".Length);
                    if (!Enum.TryParse(suffix, true, out StimStudyDifficulty difficulty)) continue;
                    var capturedDifficulty = difficulty;
                    educationActions.Add(StimActionCardFactory.Create(
                        definition,
                        () => PerformStudySession(capturedDifficulty)));
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
                    () => PerformEducationAction(capturedAction)));
            }
        }

        private void AddStudyTrackCard(StimStudyTrack track, long costMinorUnits, string description)
        {
            var affordable = gameSession.ActiveSave.state.finances != null &&
                             gameSession.ActiveSave.state.finances.cashMinorUnits >= costMinorUnits;
            var card = new VisualElement { name = $"study-track-card-{track.ToString().ToLowerInvariant()}" };
            card.AddToClassList("st-action-card");
            card.EnableInClassList("locked", !affordable);

            var title = new Label($"{track} Track");
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
            var progress = new Label(experience >= 250
                ? $"{experience} XP · Highest tier reached"
                : $"{experience} / {nextTierAt} Qualification XP");
            progress.name = "qualification-progress";
            progress.AddToClassList("st-education-progress");
            summary.Add(track);
            summary.Add(tier);
            summary.Add(progress);
            educationActions.Add(summary);
        }

        private void PerformStudySession(StimStudyDifficulty difficulty)
        {
            var succeeded = gameSession.TryPerformStudySession(difficulty, out var summary);
            eventCategory.text = succeeded ? "QUALIFICATION PROGRESS" : "SESSION LOCKED";
            eventTitle.text = $"{difficulty} Study Session";
            eventBody.text = succeeded
                ? "Your focused study advanced your selected qualification."
                : "Review the study-track, Smarts, and monthly-action requirements.";
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

        private void PerformStudyTrackChoice(StimStudyTrack track)
        {
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
            careerCard.EnableInClassList("hidden", !adult);
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
                StimCareerActionType.Quit,
                StimCareerActionType.Retire
            };
            var usedThisMonth = state.statuses.Exists(status => status.statusId == "monthly_career_action_used");
            foreach (var action in actions)
            {
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
        }

        private void PerformCareerAction(StimCareerActionType actionType)
        {
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
            eventSheet.AddToClassList("hidden");
            resultCard.AddToClassList("hidden");
            eventContinue.AddToClassList("hidden");
        }

        private void PerformActivity(StimActivityType activityType)
        {
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
            lifeScroll.RemoveFromClassList("hidden");
            timeDock.RemoveFromClassList("hidden");
            moneyView.AddToClassList("hidden");
            socialView.AddToClassList("hidden");
            navLife.AddToClassList("active");
            navMoney.RemoveFromClassList("active");
            navSocial.RemoveFromClassList("active");
        }

        private void ShowMoneyDestination()
        {
            RefreshMoney();
            lifeScroll.AddToClassList("hidden");
            timeDock.AddToClassList("hidden");
            socialView.AddToClassList("hidden");
            moneyView.RemoveFromClassList("hidden");
            navLife.RemoveFromClassList("active");
            navMoney.AddToClassList("active");
            navSocial.RemoveFromClassList("active");
        }

        private void ShowSocialDestination()
        {
            RefreshSocial();
            lifeScroll.AddToClassList("hidden");
            timeDock.AddToClassList("hidden");
            moneyView.AddToClassList("hidden");
            socialView.RemoveFromClassList("hidden");
            navLife.RemoveFromClassList("active");
            navMoney.RemoveFromClassList("active");
            navSocial.AddToClassList("active");
            ShowRelationshipList();
        }

        private void PerformManualWorkTap()
        {
            var succeeded = gameSession.TryPerformManualWorkTap(out _, out var summary);
            manualWorkFeedback.text = summary;
            if (!succeeded)
            {
                manualWorkFeedback.style.color = new StyleColor(Color.red);
                RefreshMoney();
                return;
            }

            manualWorkFeedback.style.color = StyleKeyword.Null;
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
                var card = new Button { name = $"relationship-{relationship.relationshipId}" };
                card.AddToClassList("st-relationship-card");
                var initial = string.IsNullOrEmpty(relationship.displayName)
                    ? "?"
                    : relationship.displayName.Substring(0, 1).ToUpperInvariant();
                var avatar = new Label(initial);
                avatar.AddToClassList("st-relationship-card-avatar");
                var copy = new VisualElement();
                copy.AddToClassList("st-relationship-card-copy");
                var name = new Label(string.IsNullOrEmpty(relationship.displayName)
                    ? ToDisplayName(relationship.relationshipId)
                    : relationship.displayName);
                name.AddToClassList("st-relationship-card-name");
                var meta = new Label($"{ToDisplayName(relationship.relationshipType)} · Relationship {relationship.value} / 100");
                meta.AddToClassList("st-relationship-card-meta");
                copy.Add(name);
                copy.Add(meta);
                var arrow = new Label("›");
                arrow.AddToClassList("st-relationship-card-arrow");
                card.Add(avatar);
                card.Add(copy);
                card.Add(arrow);
                var relationshipId = relationship.relationshipId;
                card.clicked += () => ShowRelationshipDetail(relationshipId);
                relationshipList.Add(card);
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
            relationshipType.text = ToDisplayName(relationship.relationshipType).ToUpperInvariant();
            relationshipStrength.text = $"Relationship {relationship.value} / 100";
            relationshipFill.style.width = Length.Percent(ClampFillPercent(relationship.value));
            relationshipGenetics.text = relationship.isGeneticParent
                ? $"Inherited profile · Health {relationship.geneticHealth} · Looks {relationship.geneticLooks} · Smarts {relationship.geneticSmarts}"
                : string.IsNullOrEmpty(relationship.origin)
                    ? "This relationship is part of your growing social story."
                    : $"Met through {ToDisplayName(relationship.origin)} at age {relationship.introducedAtAge} · " +
                      (relationship.monthsSinceInteraction == 0
                          ? "Connected this month."
                          : $"{relationship.monthsSinceInteraction} months since focused time together.");
            BuildRelationshipActions(relationship);
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
                StimRelationshipInteractionType.AskOnDate,
                StimRelationshipInteractionType.Commit,
                StimRelationshipInteractionType.BreakUp
            };
            var age = gameSession.ActiveSave.state.character.age;
            var cooldownId = $"relationship_interaction_used_{relationship.relationshipId}";
            var usedThisMonth = gameSession.ActiveSave.state.statuses.Exists(status => status.statusId == cooldownId);
            foreach (var interaction in interactions)
            {
                if (!StimGameSessionService.IsRelationshipInteractionAgeAppropriate(interaction, age)) continue;
                if (interaction == StimRelationshipInteractionType.Compete && relationship.relationshipType == "parent") continue;
                if (interaction == StimRelationshipInteractionType.Reconcile && relationship.relationshipType != "rival") continue;
                if (interaction == StimRelationshipInteractionType.AskOnDate &&
                    ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                     relationship.value < 60)) continue;
                if (interaction == StimRelationshipInteractionType.Commit &&
                    (relationship.relationshipType != "dating" || relationship.value < 75)) continue;
                if (interaction == StimRelationshipInteractionType.BreakUp &&
                    relationship.relationshipType != "dating" && relationship.relationshipType != "partner") continue;
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
        }

        private void PerformRelationshipInteraction(StimRelationshipInteractionType interactionType)
        {
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

        private void ConfigureNewLifeControls()
        {
            openNewLife.clicked += () => ShowNewLifeSetup(false, true);
            cancelNewLife.clicked += () => newLifeSetup.AddToClassList("hidden");
            continueCurrentLife.clicked += () => newLifeSetup.AddToClassList("hidden");
            createNewLife.clicked += CreateLifeFromSetup;
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
                newLifeSetup.AddToClassList("hidden");
                eventSheet.AddToClassList("hidden");
                choices.AddToClassList("hidden");
                advanceMonth.RemoveFromClassList("hidden");
                RefreshHeader();
                RefreshFeed();
                RefreshSocial();
                ShowLifeDestination();
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
            return (minorUnits / 100m).ToString("C0");
        }

        private static string FormatPreciseMoney(long minorUnits)
        {
            return (minorUnits / 100m).ToString("C2");
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
            var entries = gameSession.ActiveSave.state.lifeFeed;
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
            for (var index = entries.Count - 1; index >= 0; index--)
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
                var row = new VisualElement();
                row.AddToClassList("st-feed-entry");
                if (!string.IsNullOrEmpty(entry.category))
                {
                    row.AddToClassList("category-" + entry.category.ToLowerInvariant());
                }

                var text = new Label(entry.text);
                text.AddToClassList("st-feed-text");
                row.Add(text);
                currentGroup.Add(row);
            }
        }

        private static string GetMonthName(int month)
        {
            return month >= 1 && month <= 12
                ? new DateTime(2000, month, 1).ToString("MMMM")
                : $"Month {month}";
        }

    }
}
