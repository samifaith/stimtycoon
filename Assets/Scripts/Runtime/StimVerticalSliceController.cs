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
        private StimActivityType primaryFocusActivity;
        private StimActivityType secondaryFocusActivity;
        private StimEvent currentEvent;

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTreeAsset;
            var root = document.rootVisualElement;
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
                focusWorkoutTitle == null || focusWorkoutEffect == null)
            {
                Debug.LogError("Vertical slice UXML is missing one or more required named elements.", this);
                return;
            }

            advanceMonth.clicked += AdvanceMonth;
            toggleOverview.clicked += ToggleOverview;
            eventContinue.clicked += CloseEventSheet;
            focusStudy.clicked += () => PerformActivity(primaryFocusActivity);
            focusWorkout.clicked += () => PerformActivity(secondaryFocusActivity);
            ConfigureNewLifeControls();

            if (!loadedExistingLife)
            {
                ShowNewLifeSetup(false, false);
                return;
            }

            EnsurePrototypeCareer();

            if (!string.IsNullOrEmpty(gameSession.ActiveSave.state.pendingEventId))
            {
                catalog.TryGetEvent(gameSession.ActiveSave.state.pendingEventId, out currentEvent);
            }
            else if (gameSession.ActiveSave.state.character.age >= 18 && gameSession.ActiveSave.state.eventHistory.Count == 0)
            {
                catalog.TryGetEvent(RepresentativeStimEvents.SalaryNegotiationId, out currentEvent);
            }

            RefreshHeader();
            RefreshFeed();
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

        public void Configure(PanelSettings settings, VisualTreeAsset tree)
        {
            panelSettings = settings;
            visualTreeAsset = tree;
        }

        private void Resolve(string choiceId)
        {
            if (currentEvent == null)
            {
                return;
            }

            if (!gameSession.TryResolveChoice(
                    currentEvent.id,
                    choiceId,
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
                var button = new Button { name = $"choice-{choice.id}" };
                button.AddToClassList("choice-button");
                if (index > 0)
                {
                    button.AddToClassList("secondary");
                }

                var title = new Label(choice.labelKey);
                title.AddToClassList("choice-title");
                button.Add(title);
                var choiceId = choice.id;
                button.clicked += () => Resolve(choiceId);
                choices.Add(button);
            }
            choices.RemoveFromClassList("hidden");
            resultCard.AddToClassList("hidden");
            eventContinue.AddToClassList("hidden");
            eventSheet.RemoveFromClassList("hidden");
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
            }
        }

        private void ToggleOverview()
        {
            var opening = playerOverview.ClassListContains("hidden");
            playerOverview.EnableInClassList("hidden", !opening);
            toggleOverview.text = opening ? "HIDE PLAYER OVERVIEW" : "VIEW PLAYER OVERVIEW";
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
                newLifeSetup.AddToClassList("hidden");
                eventSheet.AddToClassList("hidden");
                choices.AddToClassList("hidden");
                advanceMonth.RemoveFromClassList("hidden");
                RefreshHeader();
                RefreshFeed();
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

        private void EnsurePrototypeCareer()
        {
            if (gameSession.ActiveSave.state.character.age < 18)
            {
                return;
            }

            if (gameSession.ActiveSave.state.career == null)
            {
                gameSession.ActiveSave.state.career = new StimCareerState();
            }

            var career = gameSession.ActiveSave.state.career;
            if (!string.IsNullOrEmpty(career.roleTitle))
            {
                return;
            }

            career.employerId = "stim_financial_group";
            career.roleTitle = "Analyst";
            career.annualSalaryMinorUnits = 5000000;
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

            for (var index = entries.Count - 1; index >= 0; index--)
            {
                var entry = entries[index];
                if (entry == null) continue;
                var row = new VisualElement();
                row.AddToClassList("st-feed-entry");
                if (!string.IsNullOrEmpty(entry.category))
                {
                    row.AddToClassList("category-" + entry.category.ToLowerInvariant());
                }

                var timing = new Label($"AGE {entry.age}  ·  MONTH {entry.monthOfYear}");
                timing.AddToClassList("st-feed-timing");
                var text = new Label(entry.text);
                text.AddToClassList("st-feed-text");
                row.Add(timing);
                row.Add(text);
                lifeFeedList.Add(row);
            }
        }

    }
}
