using System;
using System.Collections.Generic;
using StimTycoon.Abstractions;
using StimTycoon.Events;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Runtime
{
    public enum StimActivityType
    {
        Study,
        Workout,
        Play,
        Rest
    }

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

        public bool TryStartNewLife(StimSaveEnvelope save, out string summary)
        {
            var validation = StimSaveValidator.ValidateSave(save);
            if (!validation.isValid)
            {
                summary = StimSaveValidator.GetValidationSummary(validation, save?.saveId ?? "unknown-save");
                return false;
            }

            var serializedSave = JsonUtility.ToJson(save, true);
            if (!saveRepository.TryCommitAutosave(serializedSave, out summary))
            {
                return false;
            }

            ActiveSave = save;
            summary = $"Started a new life for {save.state.character.firstName} {save.state.character.lastName}.";
            return true;
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
            if (!string.IsNullOrEmpty(candidateSave.state.career.roleTitle))
            {
                candidateSave.state.career.careerProgress = ClampStat(
                    candidateSave.state.career.careerProgress + 1);
            }
            candidateSave.state.character.happiness = ClampStat(
                candidateSave.state.character.happiness + (netCashFlow >= 0 ? 1 : -2));
            AdvanceStatuses(candidateSave.state.statuses);

            var completedYear = paidMonth == 12;
            var previousLifeStage = candidateSave.state.character.lifeStage;
            candidateSave.rng.step++;
            if (completedYear)
            {
                candidateSave.state.calendar.monthOfYear = 1;
                candidateSave.state.character.age++;
                UpdateLifeAndEducationStage(candidateSave.state);
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

            var hasCareer = !string.IsNullOrEmpty(candidateSave.state.career.roleTitle);
            var cashFlowSummary = $"gross {FormatMoney(paycheck)}, taxes {FormatMoney(taxes)}, expenses {FormatMoney(expenses)}, net {FormatSignedMoney(netCashFlow)}";
            var stageChanged = !string.Equals(previousLifeStage, candidateSave.state.character.lifeStage, StringComparison.Ordinal);
            if (!hasCareer)
            {
                var stageName = ToDisplayName(candidateSave.state.character.lifeStage);
                summary = nextEvent != null
                    ? $"Age {candidateSave.state.character.age}, month {candidateSave.state.calendar.monthOfYear}: a new {nextEvent.category.ToString().ToLowerInvariant()} event is ready."
                    : completedYear
                        ? stageChanged
                            ? $"Age {candidateSave.state.character.age}: entered {stageName}."
                            : $"Age {candidateSave.state.character.age}: another year of {stageName} began."
                        : $"Age {candidateSave.state.character.age}, month {candidateSave.state.calendar.monthOfYear}: {stageName} continued.";
            }
            else
            {
                summary = nextEvent != null
                    ? $"Month {paidMonth}: {cashFlowSummary}; a new {nextEvent.category.ToString().ToLowerInvariant()} event is ready."
                    : completedYear
                        ? $"Age {candidateSave.state.character.age}: {cashFlowSummary}; completed a quiet year."
                        : $"Month {paidMonth}: {cashFlowSummary}.";
            }

            AddLifeFeedEntry(candidateSave, nextEvent != null ? "event" : stageChanged ? "milestone" : "time", summary);
            var serializedSave = JsonUtility.ToJson(candidateSave, true);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                nextEvent = null;
                summary = commitSummary;
                return false;
            }

            ActiveSave = candidateSave;
            return true;
        }

        public static string GetLifeStage(int age)
        {
            if (age < 3) return "infant";
            if (age < 6) return "early_childhood";
            if (age < 12) return "primary_school";
            if (age < 18) return "secondary_school";
            if (age < 65) return "adult";
            return "retirement";
        }

        public static string GetEducationStage(int age)
        {
            if (age < 6) return "not_started";
            if (age < 12) return "primary_school";
            if (age < 15) return "middle_school";
            if (age < 18) return "high_school";
            return "completed_secondary";
        }

        private static void UpdateLifeAndEducationStage(StimGameState state)
        {
            state.character.lifeStage = GetLifeStage(state.character.age);
            state.education ??= new StimEducationState();
            state.education.stage = GetEducationStage(state.character.age);
        }

        private static string ToDisplayName(string id)
        {
            return string.IsNullOrEmpty(id)
                ? "life"
                : id.Replace('_', ' ');
        }

        public bool TryPerformActivity(StimActivityType activityType, out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }

            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before choosing an activity.";
                return false;
            }

            if (!IsActivityAgeAppropriate(activityType, ActiveSave.state.character.age))
            {
                summary = $"{ToDisplayName(activityType.ToString())} is not available at age {ActiveSave.state.character.age}.";
                return false;
            }

            const string cooldownStatusId = "monthly_focus_used";
            NormalizeProgressCollections(ActiveSave.state);
            if (ActiveSave.state.statuses.Exists(status => status.statusId == cooldownStatusId))
            {
                summary = "You already used this month's focus. Advance the month to choose another.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");

            switch (activityType)
            {
                case StimActivityType.Study:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 2);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness - 1);
                    ApplySkillXp(candidateSave.state.skills, "learning", 10);
                    summary = "Study complete · Smarts +2 · Happiness −1 · Learning XP +10";
                    break;
                case StimActivityType.Workout:
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 2);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 1);
                    ApplySkillXp(candidateSave.state.skills, "fitness", 10);
                    summary = "Workout complete · Health +2 · Happiness +1 · Fitness XP +10";
                    break;
                case StimActivityType.Play:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 3);
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 1);
                    ApplySkillXp(candidateSave.state.skills, "play", 8);
                    summary = "Play complete · Happiness +3 · Health +1 · Play XP +8";
                    break;
                case StimActivityType.Rest:
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 2);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 1);
                    summary = "Rest complete · Health +2 · Happiness +1";
                    break;
                default:
                    summary = $"Activity {activityType} is not supported.";
                    return false;
            }

            AddOrRefreshStatus(candidateSave.state.statuses, cooldownStatusId, 1);
            AddLifeFeedEntry(candidateSave, "activity", summary);
            var serializedSave = JsonUtility.ToJson(candidateSave, true);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }

            ActiveSave = candidateSave;
            return true;
        }

        public static bool IsActivityAgeAppropriate(StimActivityType activityType, int age)
        {
            switch (activityType)
            {
                case StimActivityType.Play: return age <= 12;
                case StimActivityType.Study: return age >= 5;
                case StimActivityType.Workout: return age >= 13;
                case StimActivityType.Rest: return true;
                default: return false;
            }
        }

        private void ApplyResolution(StimSaveEnvelope save, StimChoiceResolution resolution)
        {
            NormalizeProgressCollections(save.state);
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
            AddLifeFeedEntry(save, "event", resolution.outcome.feedEntryKey);

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
            state.lifeFeed ??= new List<StimLifeFeedEntry>();
        }

        private static void AddLifeFeedEntry(StimSaveEnvelope save, string category, string text)
        {
            save.state.lifeFeed ??= new List<StimLifeFeedEntry>();
            save.state.lifeFeed.Add(new StimLifeFeedEntry
            {
                entryId = $"{save.revision}_{category}_{save.state.lifeFeed.Count}",
                category = category,
                text = text,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
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
            var timingPriority = new List<StimEvent>();
            foreach (var evt in eventCatalog.GetAllEvents())
            {
                if (IsEligible(evt, save) && IsTimingEligible(evt, processedMonth, completedYear))
                {
                    eligible.Add(evt);
                    if (evt.timingPolicy != EventTimingPolicy.AnyMonth)
                    {
                        timingPriority.Add(evt);
                    }
                }
            }

            var candidates = timingPriority.Count > 0 ? timingPriority : eligible;
            if (candidates.Count == 0)
            {
                return null;
            }

            AvoidImmediateRepeatWhenPossible(candidates, save.state.eventHistory);
            return SelectEventWeightedByLuck(candidates, save);
        }

        private static void AvoidImmediateRepeatWhenPossible(
            List<StimEvent> candidates,
            List<StimEventHistoryEntry> history)
        {
            if (candidates.Count < 2 || history == null || history.Count == 0)
            {
                return;
            }

            var latestEventId = history[history.Count - 1]?.eventId;
            candidates.RemoveAll(evt => evt != null && evt.id == latestEventId);
        }

        private static StimEvent SelectEventWeightedByLuck(List<StimEvent> eligible, StimSaveEnvelope save)
        {
            var totalWeight = 0f;
            foreach (var evt in eligible)
            {
                totalWeight += GetEventSelectionWeight(evt, save.state.character.luck);
            }

            var roll = StableUnit(save.rng.seed, save.rng.step + 3571) * totalWeight;
            foreach (var evt in eligible)
            {
                roll -= GetEventSelectionWeight(evt, save.state.character.luck);
                if (roll < 0f) return evt;
            }
            return eligible[eligible.Count - 1];
        }

        private static float GetEventSelectionWeight(StimEvent evt, int luck)
        {
            return GetLuckEventWeight(evt, luck) * Math.Max(0.01f, evt.monthlyTriggerChance);
        }

        public static float GetLuckEventWeight(StimEvent evt, int luck)
        {
            var normalizedLuck = ClampStat(luck) / 100f;
            if (evt?.analyticsTags != null && evt.analyticsTags.Contains("random_gain"))
            {
                return 0.5f + normalizedLuck * 2f;
            }
            if (evt?.analyticsTags != null && evt.analyticsTags.Contains("random_loss"))
            {
                return 2.5f - normalizedLuck * 2f;
            }
            return 1f;
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
