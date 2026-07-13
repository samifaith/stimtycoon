using System;
using System.Collections.Generic;
using StimTycoon.Abstractions;
using StimTycoon.Events;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// Owns the active life, migrates loaded saves, and commits each action as one transaction.
    /// </summary>
    public sealed class StimGameSessionService
    {
        private readonly IStimEventCatalog eventCatalog;
        private readonly IStimSaveRepository saveRepository;
        private readonly StimOutcomeResolver outcomeResolver;
        private readonly Func<DateTimeOffset> utcNow;

        public StimSaveEnvelope ActiveSave { get; private set; }
        public StimChoiceResolution LastResolution { get; private set; }

        public StimGameSessionService(
            IStimEventCatalog eventCatalog,
            IStimSaveRepository saveRepository,
            StimOutcomeResolver outcomeResolver = null,
            Func<DateTimeOffset> utcNow = null)
        {
            this.eventCatalog = eventCatalog ?? throw new ArgumentNullException(nameof(eventCatalog));
            this.saveRepository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
            this.outcomeResolver = outcomeResolver ?? new StimOutcomeResolver();
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        public void Start(StimSaveEnvelope save)
        {
            var validation = StimSaveValidator.ValidateSave(save);
            if (!validation.isValid)
            {
                throw new ArgumentException(
                    StimSaveValidator.GetValidationSummary(validation, save?.saveId ?? "unknown-save"),
                    nameof(save));
            }

            ActiveSave = save;
        }

        public bool TryLoadLatest(out string summary)
        {
            if (!saveRepository.TryLoadLatestSave(out var serializedSave))
            {
                summary = "No valid local autosave was found.";
                return false;
            }

            if (!StimSaveMigrator.TryMigrate(serializedSave, out var save, out var migration, out summary))
            {
                return false;
            }
            var validation = StimSaveValidator.ValidateSave(save);
            summary = StimSaveValidator.GetValidationSummary(validation, save?.saveId ?? "unknown-save");
            if (!validation.isValid)
            {
                return false;
            }

            ActiveSave = save;
            if (migration.changed)
            {
                summary += $" Migrated {migration.changes.Count} additive v1 field(s).";
            }
            return true;
        }

        public bool TryResolveChoice(string eventId, string choiceId, out string summary)
        {
            LastResolution = null;
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }

            if (!eventCatalog.TryGetEvent(eventId, out var evt) || evt == null)
            {
                summary = $"Event {eventId} was not found.";
                return false;
            }

            var validation = StimEventValidator.ValidateEvent(evt);
            if (!validation.isValid)
            {
                summary = StimEventValidator.GetValidationSummary(validation, eventId);
                return false;
            }

            if (!outcomeResolver.TryResolve(
                    evt,
                    choiceId,
                    ActiveSave.rng.seed,
                    ActiveSave.rng.step,
                    out var resolution,
                    out summary))
            {
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            ApplyResolution(candidateSave, resolution);
            var serializedSave = JsonUtility.ToJson(candidateSave, true);
            if (!saveRepository.TryCommitAutosave(serializedSave, out summary))
            {
                return false;
            }

            ActiveSave = candidateSave;
            LastResolution = resolution;
            summary = resolution.outcome.resultTextKey;
            return true;
        }

        public bool TryAdvanceMonth(out StimEvent nextEvent, out string summary)
        {
            nextEvent = null;
            LastResolution = null;
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }

            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before advancing another month.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            var paidMonth = candidateSave.state.calendar.monthOfYear;
            var paycheck = CalculateMonthlyPaycheck(candidateSave.state.career.annualSalaryMinorUnits, paidMonth);
            var taxes = CalculateTaxWithholding(paycheck, candidateSave.state.finances.taxRateBasisPoints);
            var expenses = candidateSave.state.finances.monthlyLivingExpensesMinorUnits;
            var netCashFlow = paycheck - taxes - expenses;
            ApplyMonthlyCashFlow(candidateSave.state.finances, paycheck, taxes, expenses);
            candidateSave.state.career.careerProgress = ClampStat(
                candidateSave.state.career.careerProgress + 1);
            candidateSave.state.character.happiness = ClampStat(
                candidateSave.state.character.happiness + (netCashFlow >= 0 ? 1 : -2));
            AdvanceStatuses(candidateSave.state.statuses);

            var completedYear = paidMonth == 12;
            candidateSave.rng.step++;
            if (completedYear)
            {
                candidateSave.state.calendar.monthOfYear = 1;
                candidateSave.state.character.age++;
            }
            else
            {
                candidateSave.state.calendar.monthOfYear++;
            }
            nextEvent = SelectEligibleEvent(candidateSave, paidMonth, completedYear);
            candidateSave.state.pendingEventId = nextEvent?.id;
            candidateSave.state.calendar.quietMonthsSinceEvent = nextEvent == null
                ? candidateSave.state.calendar.quietMonthsSinceEvent + 1
                : 0;

            var serializedSave = JsonUtility.ToJson(candidateSave, true);
            if (!saveRepository.TryCommitAutosave(serializedSave, out summary))
            {
                nextEvent = null;
                return false;
            }

            ActiveSave = candidateSave;
            var cashFlowSummary = $"gross {FormatMoney(paycheck)}, taxes {FormatMoney(taxes)}, expenses {FormatMoney(expenses)}, net {FormatSignedMoney(netCashFlow)}";
            summary = nextEvent != null
                ? $"Month {paidMonth}: {cashFlowSummary}; a new {nextEvent.category.ToString().ToLowerInvariant()} event is ready."
                : completedYear
                    ? $"Age {candidateSave.state.character.age}: {cashFlowSummary}; completed a quiet year."
                    : $"Month {paidMonth}: {cashFlowSummary}.";
            return true;
        }

        private void ApplyResolution(StimSaveEnvelope save, StimChoiceResolution resolution)
        {
            save.revision++;
            save.rng.step++;
            save.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            save.state.pendingEventId = null;

            foreach (var effect in resolution.outcome.effects)
            {
                ApplyEffect(save, effect);
            }

            save.state.eventHistory.Add(new StimEventHistoryEntry
            {
                eventId = resolution.eventId,
                choiceId = resolution.choiceId,
                outcomeId = resolution.outcome.id,
                age = save.state.character.age,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });

            foreach (var followUp in resolution.outcome.followUps)
            {
                save.state.scheduledEvents.Add(new StimScheduledEventRecord
                {
                    eventId = followUp.eventId,
                    earliestTriggerAge = save.state.character.age + followUp.minYearsFromNow,
                    latestTriggerAge = save.state.character.age + followUp.maxYearsFromNow,
                    chance = followUp.probability,
                    sourceEventId = resolution.eventId,
                    cancellationRule = followUp.cancellationRule
                });
            }
        }

        private void ApplyEffect(StimSaveEnvelope save, Effect effect)
        {
            if (effect == null)
            {
                return;
            }

            switch (effect.type)
            {
                case EffectType.CashDelta:
                    save.state.finances.cashMinorUnits = Math.Max(
                        0,
                        save.state.finances.cashMinorUnits + (long)Math.Round(effect.value));
                    break;
                case EffectType.SalaryDelta:
                    save.state.career.annualSalaryMinorUnits = Math.Max(
                        0,
                        save.state.career.annualSalaryMinorUnits + (long)Math.Round(effect.value));
                    break;
                case EffectType.DebtDelta:
                    save.state.finances.debtMinorUnits = Math.Max(
                        0,
                        save.state.finances.debtMinorUnits + (long)Math.Round(effect.value));
                    break;
                case EffectType.StatDelta:
                    ApplyStatDelta(save, effect.targetId, effect.value);
                    break;
                case EffectType.CareerProgressDelta:
                    save.state.career.careerProgress = ClampStat(
                        save.state.career.careerProgress + (int)Math.Round(effect.value));
                    break;
                case EffectType.SkillXp:
                    ApplySkillXp(save.state.skills, effect.targetId, effect.value);
                    break;
                case EffectType.RelationshipDelta:
                    ApplyRelationshipDelta(save.state.relationships, effect.targetId, effect.value);
                    break;
                case EffectType.StatusAdd:
                    AddOrRefreshStatus(save.state.statuses, effect.targetId, effect.value);
                    break;
                case EffectType.StatusRemove:
                    save.state.statuses.RemoveAll(status => status.statusId == effect.targetId);
                    break;
            }
        }

        private static void ApplySkillXp(List<StimSkillState> skills, string skillId, float value)
        {
            var skill = skills.Find(candidate => candidate.skillId == skillId);
            if (skill == null)
            {
                skill = new StimSkillState { skillId = skillId };
                skills.Add(skill);
            }
            skill.experience = Math.Max(0, skill.experience + (int)Math.Round(value));
        }

        private static void ApplyRelationshipDelta(
            List<StimRelationshipState> relationships,
            string relationshipId,
            float value)
        {
            var relationship = relationships.Find(candidate => candidate.relationshipId == relationshipId);
            if (relationship == null)
            {
                relationship = new StimRelationshipState { relationshipId = relationshipId };
                relationships.Add(relationship);
            }
            relationship.value = ClampStat(relationship.value + (int)Math.Round(value));
        }

        private static void AddOrRefreshStatus(List<StimStatusState> statuses, string statusId, float value)
        {
            var duration = Math.Max(1, (int)Math.Round(value));
            var status = statuses.Find(candidate => candidate.statusId == statusId);
            if (status == null)
            {
                statuses.Add(new StimStatusState { statusId = statusId, remainingMonths = duration });
                return;
            }
            status.remainingMonths = Math.Max(status.remainingMonths, duration);
        }

        private static void AdvanceStatuses(List<StimStatusState> statuses)
        {
            for (var index = statuses.Count - 1; index >= 0; index--)
            {
                statuses[index].remainingMonths--;
                if (statuses[index].remainingMonths <= 0)
                {
                    statuses.RemoveAt(index);
                }
            }
        }

        private static void NormalizeProgressCollections(StimGameState state)
        {
            if (state == null)
            {
                return;
            }
            state.skills ??= new List<StimSkillState>();
            state.relationships ??= new List<StimRelationshipState>();
            state.statuses ??= new List<StimStatusState>();
        }

        private void ApplyStatDelta(StimSaveEnvelope save, string targetId, float value)
        {
            var delta = (int)Math.Round(value);
            switch (targetId)
            {
                case "health":
                    save.state.character.health = ClampStat(save.state.character.health + delta);
                    break;
                case "happiness":
                    save.state.character.happiness = ClampStat(save.state.character.happiness + delta);
                    break;
                case "smarts":
                    save.state.character.smarts = ClampStat(save.state.character.smarts + delta);
                    break;
                case "looks":
                    save.state.character.looks = ClampStat(save.state.character.looks + delta);
                    break;
                case "luck":
                    save.state.character.luck = ClampStat(save.state.character.luck + delta);
                    break;
            }
        }

        private static int ClampStat(int value)
        {
            return Math.Max(StimSaveSchema.MinCoreStatValue, Math.Min(StimSaveSchema.MaxCoreStatValue, value));
        }

        private StimEvent SelectEligibleEvent(StimSaveEnvelope save, int processedMonth, bool completedYear)
        {
            var eligible = new List<StimEvent>();
            foreach (var evt in eventCatalog.GetAllEvents())
            {
                if (IsEligible(evt, save) && IsTimingEligible(evt, processedMonth, completedYear))
                {
                    eligible.Add(evt);
                }
            }

            if (eligible.Count == 0)
            {
                return null;
            }

            var selected = eligible[StableIndex(save.rng.seed, save.rng.step, eligible.Count)];
            var triggerChance = selected.timingPolicy == EventTimingPolicy.AnyMonth
                ? Math.Min(1f, selected.monthlyTriggerChance + 0.25f * save.state.calendar.quietMonthsSinceEvent)
                : selected.monthlyTriggerChance;
            return StableUnit(save.rng.seed, save.rng.step + 7919) < triggerChance
                ? selected
                : null;
        }

        private static bool IsTimingEligible(StimEvent evt, int processedMonth, bool completedYear)
        {
            switch (evt.timingPolicy)
            {
                case EventTimingPolicy.AnnualRollover:
                    return completedYear;
                case EventTimingPolicy.SpecificMonth:
                    return processedMonth == evt.requiredMonth;
                default:
                    return true;
            }
        }

        private static bool IsEligible(StimEvent evt, StimSaveEnvelope save)
        {
            if (evt == null || !StimEventValidator.ValidateEvent(evt).isValid || evt.ageRange == null)
            {
                return false;
            }

            var age = save.state.character.age;
            if (age < evt.ageRange.minAge || age > evt.ageRange.maxAge)
            {
                return false;
            }

            StimEventHistoryEntry latest = null;
            foreach (var entry in save.state.eventHistory)
            {
                if (entry != null && string.Equals(entry.eventId, evt.id, StringComparison.Ordinal) &&
                    (latest == null || entry.age > latest.age))
                {
                    latest = entry;
                }
            }

            if (latest == null)
            {
                return true;
            }

            if (evt.repeatPolicy == RepeatPolicy.Never || evt.repeatPolicy == RepeatPolicy.OncePerLifeStage)
            {
                return false;
            }

            return age - latest.age >= evt.cooldownYears;
        }

        private static int StableIndex(int seed, int step, int count)
        {
            unchecked
            {
                var value = (uint)seed ^ (0x9E3779B9u * (uint)(step + 1));
                value ^= value >> 16;
                value *= 0x7FEB352Du;
                value ^= value >> 15;
                return (int)(value % (uint)count);
            }
        }

        private static float StableUnit(int seed, int step)
        {
            return StableIndex(seed, step, 1000000) / 1000000f;
        }

        private static StimSaveEnvelope CloneSave(StimSaveEnvelope save)
        {
            return JsonUtility.FromJson<StimSaveEnvelope>(JsonUtility.ToJson(save));
        }

        private static long CalculateMonthlyPaycheck(long annualSalaryMinorUnits, int monthOfYear)
        {
            var basePaycheck = annualSalaryMinorUnits / 12;
            var remainder = annualSalaryMinorUnits % 12;
            return basePaycheck + (monthOfYear <= remainder ? 1 : 0);
        }

        private static long CalculateTaxWithholding(long grossPayMinorUnits, int taxRateBasisPoints)
        {
            return (long)Math.Round(
                grossPayMinorUnits * (taxRateBasisPoints / 10000m),
                MidpointRounding.AwayFromZero);
        }

        private static void ApplyMonthlyCashFlow(
            StimFinancesState finances,
            long grossPay,
            long taxes,
            long expenses)
        {
            var availableCash = finances.cashMinorUnits + grossPay;
            var outflow = taxes + expenses;
            if (availableCash >= outflow)
            {
                finances.cashMinorUnits = availableCash - outflow;
                return;
            }

            finances.debtMinorUnits += outflow - availableCash;
            finances.cashMinorUnits = 0;
        }

        private static string FormatMoney(long minorUnits)
        {
            return (minorUnits / 100m).ToString("C0");
        }

        private static string FormatSignedMoney(long minorUnits)
        {
            return $"{(minorUnits >= 0 ? "+" : "-")}{FormatMoney(Math.Abs(minorUnits))}";
        }

    }
}
