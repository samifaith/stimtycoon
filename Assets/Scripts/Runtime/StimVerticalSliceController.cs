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
        private Label feedEntry;
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
        private VisualElement choices;
        private VisualElement resultCard;
        private VisualElement playerOverview;
        private VisualElement careerProgressFill;
        private Button advanceMonth;
        private Button toggleOverview;
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
            feedEntry = root.Q<Label>("feed-entry");
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
            choices = root.Q<VisualElement>("choices");
            resultCard = root.Q<VisualElement>("result-card");
            playerOverview = root.Q<VisualElement>("player-overview");
            careerProgressFill = root.Q<VisualElement>("career-progress-fill");

            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            catalog.Upsert(RepresentativeStimEvents.CreateHealthBurnout());
            catalog.Upsert(RepresentativeStimEvents.CreateMoneyFastReturn());
            catalog.Upsert(RepresentativeStimEvents.CreateSchoolGroupProject());
            catalog.Upsert(RepresentativeStimEvents.CreateChildhoodGrownFolksTable());
            gameSession = new StimGameSessionService(catalog, new NativeStimSaveRepository());
            if (!gameSession.TryLoadLatest(out _))
            {
                gameSession.Start(CreateNewLife());
            }
            EnsurePrototypeCareer();

            advanceMonth = root.Q<Button>("advance-month");
            toggleOverview = root.Q<Button>("toggle-overview");
            if (cashValue == null || lifeSummary == null || eventCategory == null || eventTitle == null ||
                eventBody == null || resultText == null || resultEffects == null || feedEntry == null || choices == null ||
                resultCard == null || advanceMonth == null || toggleOverview == null || playerOverview == null ||
                overviewCareer == null || overviewCalendar == null || healthValue == null ||
                happinessValue == null || smartsValue == null || looksValue == null || luckValue == null ||
                careerProgressValue == null ||
                careerProgressFill == null || monthlyPaycheckValue == null || annualSalaryValue == null)
            {
                Debug.LogError("Vertical slice UXML is missing one or more required named elements.", this);
                return;
            }

            advanceMonth.clicked += AdvanceMonth;
            toggleOverview.clicked += ToggleOverview;

            if (!string.IsNullOrEmpty(gameSession.ActiveSave.state.pendingEventId))
            {
                catalog.TryGetEvent(gameSession.ActiveSave.state.pendingEventId, out currentEvent);
            }
            else if (gameSession.ActiveSave.state.eventHistory.Count == 0)
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
            }
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
                resultCard.RemoveFromClassList("hidden");
                return;
            }

            resultText.text = summary;
            resultEffects.text = BuildEffectSummary(gameSession.LastResolution.outcome.effects);
            resultCard.RemoveFromClassList("hidden");
            choices.AddToClassList("hidden");
            advanceMonth.RemoveFromClassList("hidden");
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
            resultCard.RemoveFromClassList("hidden");
            RefreshHeader();

            if (currentEvent == null)
            {
                choices.AddToClassList("hidden");
                advanceMonth.RemoveFromClassList("hidden");
                feedEntry.text = summary;
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
            advanceMonth.AddToClassList("hidden");
        }

        private void RefreshHeader()
        {
            var state = gameSession.ActiveSave.state;
            var career = state.career;
            cashValue.text = FormatMoney(state.finances.cashMinorUnits);
            lifeSummary.text = $"Age {state.character.age} · Month {state.calendar.monthOfYear} · {career.roleTitle}";
            overviewCareer.text = $"{career.roleTitle} · Stim Financial Group";
            overviewCalendar.text = $"Age {state.character.age} · Month {state.calendar.monthOfYear} of 12";
            healthValue.text = $"{state.character.health} / 100";
            happinessValue.text = $"{state.character.happiness} / 100";
            smartsValue.text = $"{state.character.smarts} / 100";
            looksValue.text = $"{state.character.looks} / 100";
            luckValue.text = $"{state.character.luck} / 100";
            careerProgressValue.text = $"{career.careerProgress} / 100";
            careerProgressFill.style.width = Length.Percent(career.careerProgress);
            var grossMonthlyPay = career.annualSalaryMinorUnits / 12;
            var estimatedTaxes = (long)Math.Round(
                grossMonthlyPay * (state.finances.taxRateBasisPoints / 10000m),
                MidpointRounding.AwayFromZero);
            var estimatedNet = grossMonthlyPay - estimatedTaxes - state.finances.monthlyLivingExpensesMinorUnits;
            monthlyPaycheckValue.text = FormatSignedMoney(estimatedNet);
            annualSalaryValue.text = $"{FormatMoney(career.annualSalaryMinorUnits)} gross · {state.finances.taxRateBasisPoints / 100m:0.#}% tax";
        }

        private void ToggleOverview()
        {
            var opening = playerOverview.ClassListContains("hidden");
            playerOverview.EnableInClassList("hidden", !opening);
            toggleOverview.text = opening ? "HIDE PLAYER OVERVIEW" : "VIEW PLAYER OVERVIEW";
        }

        private static string FormatMoney(long minorUnits)
        {
            return (minorUnits / 100m).ToString("C0");
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
            var history = gameSession.ActiveSave.state.eventHistory;
            if (history.Count == 0)
            {
                return;
            }

            var latest = history[history.Count - 1];
            feedEntry.text = $"Age {latest.age}: {gameSession.LastResolution?.outcome.feedEntryKey ?? latest.outcomeId}";
        }

        private static StimSaveEnvelope CreateNewLife()
        {
            var now = DateTimeOffset.UtcNow.ToString("O");
            return new StimSaveEnvelope
            {
                gameBuildVersion = Application.version,
                contentVersion = "1",
                saveId = Guid.NewGuid().ToString("N"),
                playerAccountId = "local-player",
                lifeId = Guid.NewGuid().ToString("N"),
                createdAtUtc = now,
                updatedAtUtc = now,
                revision = 1,
                deviceIdHash = "local-device",
                rng = new StimRngState { seed = Environment.TickCount, step = 0 },
                integrity = new StimSaveIntegrity { payloadHash = "pending" },
                state = new StimGameState
                {
                    character = new StimCharacterState
                    {
                        age = 24,
                        health = 80,
                        happiness = 70,
                        smarts = 65,
                        looks = 65,
                        luck = 50
                    },
                    finances = new StimFinancesState
                    {
                        cashMinorUnits = 100000,
                        monthlyLivingExpensesMinorUnits = 250000,
                        taxRateBasisPoints = 2000
                    },
                    calendar = new StimCalendarState { monthOfYear = 1 },
                    career = new StimCareerState
                    {
                        employerId = "stim_financial_group",
                        roleTitle = "Analyst",
                        annualSalaryMinorUnits = 5000000
                    },
                    pendingEventId = RepresentativeStimEvents.SalaryNegotiationId,
                    eventHistory = new List<StimEventHistoryEntry>(),
                    scheduledEvents = new List<StimScheduledEventRecord>()
                }
            };
        }
    }
}
