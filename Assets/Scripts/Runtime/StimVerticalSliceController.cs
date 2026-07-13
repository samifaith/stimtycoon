using System;
using System.Collections.Generic;
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
        private Label resultText;
        private Label feedEntry;
        private VisualElement choices;
        private VisualElement resultCard;

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTreeAsset;
            var root = document.rootVisualElement;
            cashValue = root.Q<Label>("cash-value");
            resultText = root.Q<Label>("result-text");
            feedEntry = root.Q<Label>("feed-entry");
            choices = root.Q<VisualElement>("choices");
            resultCard = root.Q<VisualElement>("result-card");

            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            gameSession = new StimGameSessionService(catalog, new NativeStimSaveRepository());
            if (!gameSession.TryLoadLatest(out _))
            {
                gameSession.Start(CreateNewLife());
            }

            var makeTheCase = root.Q<Button>("make-the-case");
            var letItPass = root.Q<Button>("let-it-pass");
            if (cashValue == null || resultText == null || feedEntry == null || choices == null ||
                resultCard == null || makeTheCase == null || letItPass == null)
            {
                Debug.LogError("Vertical slice UXML is missing one or more required named elements.", this);
                return;
            }

            makeTheCase.clicked += () => Resolve("make_the_case");
            letItPass.clicked += () => Resolve("let_it_pass");
            RefreshHeader();
            RefreshFeed();
        }

        public void Configure(PanelSettings settings, VisualTreeAsset tree)
        {
            panelSettings = settings;
            visualTreeAsset = tree;
        }

        private void Resolve(string choiceId)
        {
            if (!gameSession.TryResolveChoice(
                    RepresentativeStimEvents.SalaryNegotiationId,
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
            RefreshHeader();
            RefreshFeed();
        }

        private void RefreshHeader()
        {
            cashValue.text = (gameSession.ActiveSave.state.finances.cashMinorUnits / 100m).ToString("C0");
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
                    eventHistory = new List<StimEventHistoryEntry>(),
                    scheduledEvents = new List<StimScheduledEventRecord>()
                }
            };
        }
    }
}
