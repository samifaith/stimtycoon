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
    /// for requirements, risk labels, outcomes, and state mutations.
    /// </summary>
    public sealed class YarnStimDialogueBridge : MonoBehaviour, IStimDialogueBridge
    {
        [SerializeField] private DialogueRunner dialogueRunner;

        private StimEventRuntimeService eventRuntimeService;

        public void Initialize(StimEventRuntimeService runtimeService)
        {
            eventRuntimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
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
    }
}
