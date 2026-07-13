using System;
using System.Collections.Generic;
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
        private Label feedEntry;
        private VisualElement choices;
        private VisualElement resultCard;
        private Button advanceMonth;
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
            feedEntry = root.Q<Label>("feed-entry");
            choices = root.Q<VisualElement>("choices");
            resultCard = root.Q<VisualElement>("result-card");

            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            catalog.Upsert(RepresentativeStimEvents.CreateHealthBurnout());
            gameSession = new StimGameSessionService(catalog, new NativeStimSaveRepository());
            if (!gameSession.TryLoadLatest(out _))
            {
                gameSession.Start(CreateNewLife());
            }
            EnsurePrototypeCareer();

            advanceMonth = root.Q<Button>("advance-month");
            if (cashValue == null || lifeSummary == null || eventCategory == null || eventTitle == null ||
                eventBody == null || resultText == null || feedEntry == null || choices == null ||
                resultCard == null || advanceMonth == null)
            {
                Debug.LogError("Vertical slice UXML is missing one or more required named elements.", this);
                return;
            }

            advanceMonth.clicked += AdvanceMonth;

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
                resultCard.RemoveFromClassList("hidden");
                return;
            }

            resultText.text = summary;
            resultCard.RemoveFromClassList("hidden");
            choices.AddToClassList("hidden");
            advanceMonth.RemoveFromClassList("hidden");
            currentEvent = null;
            RefreshHeader();
            RefreshFeed();
        }

        private void AdvanceMonth()
        {
            if (!gameSession.TryAdvanceMonth(out var nextEvent, out var summary))
            {
                resultText.text = summary;
                resultCard.RemoveFromClassList("hidden");
                return;
            }

            currentEvent = nextEvent;
            resultText.text = summary;
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
                var meta = new Label($"{choice.riskPreview} risk · {choice.rewardPreview} reward");
                meta.AddToClassList("choice-meta");
                button.Add(meta);
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
            cashValue.text = (gameSession.ActiveSave.state.finances.cashMinorUnits / 100m).ToString("C0");
            lifeSummary.text = $"Age {gameSession.ActiveSave.state.character.age} · Month {gameSession.ActiveSave.state.calendar.monthOfYear} · {gameSession.ActiveSave.state.career.roleTitle}";
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
                        smarts = 65
                    },
                    finances = new StimFinancesState { cashMinorUnits = 100000 },
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
