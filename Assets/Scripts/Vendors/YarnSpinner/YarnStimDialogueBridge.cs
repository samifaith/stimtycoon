using System;
using System.Linq;
using StimTycoon.Abstractions;
using StimTycoon.Runtime;
using UnityEngine;
using Yarn.Unity;

namespace StimTycoon.YarnSpinner
{
    /// <summary>
    /// Connects authored Yarn nodes to Stim's validated event runtime.
    /// Yarn controls presentation and branching; Stim remains authoritative
    /// for requirements, internal risk bands, outcomes, and state mutations.
    /// </summary>
    public sealed class YarnStimDialogueBridge : MonoBehaviour, IStimDialogueBridge
    {
        [SerializeField] private DialogueRunner dialogueRunner;

        private StimEventRuntimeService eventRuntimeService;
        private StimGameSessionService gameSessionService;

        public event Action<StimTycoon.Events.StimChoiceResolution> ChoiceResolved;

        public void Initialize(StimEventRuntimeService runtimeService)
        {
            eventRuntimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
        }

        public void Initialize(
            StimEventRuntimeService runtimeService,
            StimGameSessionService sessionService)
        {
            Initialize(runtimeService);
            gameSessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

            if (dialogueRunner == null)
            {
                throw new InvalidOperationException("A DialogueRunner reference is required.");
            }

            dialogueRunner.AddCommandHandler<string, string>("stim_resolve_choice", ResolveChoiceCommand);
        }

        public bool CanRunEvent(string eventId)
        {
            if (eventRuntimeService == null || dialogueRunner == null)
            {
                return false;
            }

            if (!eventRuntimeService.CanRunEvent(eventId, out _))
            {
                return false;
            }

            var project = dialogueRunner.YarnProject;
            return project != null && project.NodeNames.Contains(eventId, StringComparer.Ordinal);
        }

        public bool TryStartEvent(string eventId)
        {
            if (!CanRunEvent(eventId) || dialogueRunner.IsDialogueRunning)
            {
                return false;
            }

            _ = dialogueRunner.StartDialogue(eventId);
            return true;
        }

        public string GetRiskLabel(string eventId, string choiceId)
        {
            return eventRuntimeService == null
                ? "Unknown"
                : eventRuntimeService.GetRiskLabel(eventId, choiceId);
        }

        public bool ResolveChoice(string eventId, string choiceId)
        {
            return eventRuntimeService != null &&
                   eventRuntimeService.TryResolveChoice(eventId, choiceId, out _);
        }

        public void ApplyResolvedOutcome()
        {
        }

        public void ScheduleFollowUps()
        {
        }

        private void ResolveChoiceCommand(string eventId, string choiceId)
        {
            if (gameSessionService == null)
            {
                Debug.LogError("Stim game session has not been initialized.", this);
                return;
            }

            if (!gameSessionService.TryResolveChoice(eventId, choiceId, out var summary))
            {
                Debug.LogError($"Could not resolve {eventId}/{choiceId}: {summary}", this);
                return;
            }

            ChoiceResolved?.Invoke(gameSessionService.LastResolution);
        }
    }
}
