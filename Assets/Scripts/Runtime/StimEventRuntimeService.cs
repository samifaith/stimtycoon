using System;
using System.Collections.Generic;
using StimTycoon.Abstractions;
using StimTycoon.Events;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// Runtime service that validates authored events and derives player-facing labels.
    /// </summary>
    public sealed class StimEventRuntimeService
    {
        private readonly IStimEventCatalog eventCatalog;

        public StimEventRuntimeService(IStimEventCatalog eventCatalog)
        {
            this.eventCatalog = eventCatalog ?? throw new ArgumentNullException(nameof(eventCatalog));
        }

        public bool CanRunEvent(string eventId, out string validationSummary)
        {
            validationSummary = string.Empty;

            if (!TryGetValidatedEvent(eventId, out var evt, out validationSummary))
            {
                return false;
            }

            return true;
        }

        public string GetRiskLabel(string eventId, string choiceId)
        {
            if (!TryGetChoice(eventId, choiceId, out var choice))
            {
                return "Unknown";
            }

            var riskLevel = RiskRewardCalculator.GetRiskLevel(choice.baseSuccessChance);
            return riskLevel.ToString();
        }

        public bool TryStartEvent(string eventId)
        {
            return CanRunEvent(eventId, out _);
        }

        public bool TryResolveChoice(string eventId, string choiceId, out string resolutionSummary)
        {
            resolutionSummary = string.Empty;

            if (!TryGetValidatedEvent(eventId, out var evt, out resolutionSummary))
            {
                return false;
            }

            if (!TryGetChoice(evt, choiceId, out var choice, out resolutionSummary))
            {
                return false;
            }

            if (choice.outcomes == null || choice.outcomes.Count == 0)
            {
                resolutionSummary = $"Choice {choiceId} has no outcomes.";
                return false;
            }

            resolutionSummary = $"Choice {choiceId} is structurally valid and ready for outcome resolution.";
            return true;
        }

        private bool TryGetValidatedEvent(string eventId, out StimEvent evt, out string validationSummary)
        {
            validationSummary = string.Empty;
            evt = null;

            if (string.IsNullOrWhiteSpace(eventId))
            {
                validationSummary = "Event id is required.";
                return false;
            }

            if (!eventCatalog.TryGetEvent(eventId, out evt) || evt == null)
            {
                validationSummary = $"Event {eventId} was not found in the catalog.";
                return false;
            }

            var result = StimEventValidator.ValidateEvent(evt);
            validationSummary = StimEventValidator.GetValidationSummary(result, eventId);
            return result.isValid;
        }

        private bool TryGetChoice(string eventId, string choiceId, out Choice choice)
        {
            choice = null;

            if (!eventCatalog.TryGetEvent(eventId, out var evt) || evt?.choices == null)
            {
                return false;
            }

            return TryGetChoice(evt, choiceId, out choice, out _);
        }

        private static bool TryGetChoice(StimEvent evt, string choiceId, out Choice choice, out string validationSummary)
        {
            validationSummary = string.Empty;
            choice = null;

            if (evt?.choices == null)
            {
                validationSummary = "Event has no choices.";
                return false;
            }

            foreach (var candidate in evt.choices)
            {
                if (candidate != null && string.Equals(candidate.id, choiceId, StringComparison.Ordinal))
                {
                    choice = candidate;
                    return true;
                }
            }

            validationSummary = $"Choice {choiceId} was not found on event {evt.id}.";
            return false;
        }
    }

    public sealed class InMemoryStimEventCatalog : IStimEventCatalog
    {
        private readonly Dictionary<string, StimEvent> events = new Dictionary<string, StimEvent>(StringComparer.Ordinal);

        public int Count => events.Count;

        public bool TryGetEvent(string eventId, out StimEvent evt)
        {
            return events.TryGetValue(eventId, out evt);
        }

        public IReadOnlyList<StimEvent> GetAllEvents()
        {
            var result = new List<StimEvent>(events.Values);
            result.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
            return result;
        }

        public void Upsert(StimEvent evt)
        {
            if (evt == null || string.IsNullOrWhiteSpace(evt.id))
            {
                return;
            }

            events[evt.id] = evt;
        }
    }

    internal sealed class StimDialogueBridge : IStimDialogueBridge
    {
        private readonly StimEventRuntimeService eventRuntimeService;

        public StimDialogueBridge(StimEventRuntimeService eventRuntimeService)
        {
            this.eventRuntimeService = eventRuntimeService ?? throw new ArgumentNullException(nameof(eventRuntimeService));
        }

        public bool CanRunEvent(string eventId)
        {
            return eventRuntimeService.CanRunEvent(eventId, out _);
        }

        public bool TryStartEvent(string eventId)
        {
            return eventRuntimeService.TryStartEvent(eventId);
        }

        public string GetRiskLabel(string eventId, string choiceId)
        {
            return eventRuntimeService.GetRiskLabel(eventId, choiceId);
        }

        public bool ResolveChoice(string eventId, string choiceId)
        {
            return eventRuntimeService.TryResolveChoice(eventId, choiceId, out _);
        }

        public void ApplyResolvedOutcome()
        {
        }

        public void ScheduleFollowUps()
        {
        }
    }
}
