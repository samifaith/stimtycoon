using System;
using System.Collections.Generic;
using StimTycoon.Abstractions;
using StimTycoon.Events;
using StimTycoon.Saves;
using UnityEngine;
using Event = StimTycoon.Events.Event;

namespace StimTycoon.Runtime
{
    public enum ActivityType
    {
        Study,
        Workout,
        Play,
        Rest,
        FamilyTime,
        FamilyMovie,
        FamilyMovieCredit,
        Explore,
        AttendSchool,
        JoinClub,
        WorkShift,
        Overtime,
        Training,
        Socialize,
        Hobby,
        Checkup
    }

    public enum RelationshipInteractionType
    {
        Talk,
        PlayTogether,
        AskForHelp,
        SpendTime,
        Argue,
        Compete,
        Reconcile,
        DeepenFriendship,
        AskOnDate,
        DateNight,
        Commit,
        BreakUp,
        Separate,
        Recover
    }

    public enum FamilyPlanningAction
    {
        Discuss,
        TryForChild,
        PursueAdoption,
        OptOut
    }

    public enum ParentingAction
    {
        QualityTime,
        SupportNeeds,
        Teach,
        SetBoundaries
    }

    public enum PaymentMethod
    {
        Cash,
        Credit
    }

    public enum SavingsTransferType
    {
        Deposit,
        Withdrawal
    }

    public enum HomeActionType
    {
        Read,
        Train,
        Rest,
        Maintain,
        HouseholdTime
    }

    public enum EducationActionType
    {
        Read,
        Homework,
        StudyGroup,
        AdvancedProject
    }

    public enum SchoolPathChoice
    {
        PublicSchool,
        Homeschool,
        AcademicTrack,
        VocationalTrack,
        LeaveSchool
    }

    public enum StudyTrack
    {
        General,
        Academic,
        Vocational
    }

    public enum StudyDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum CareerActionType
    {
        Apply,
        Interview,
        WorkHard,
        AskForPromotion,
        Retrain,
        Quit,
        Retire
    }

    public enum BusinessActionType { Start, Work, Upgrade, HireStaff, ExpandLocation, Sell }
    public enum PropertyActionType { BuyStarterRental, Maintain, FindTenant, Sell }

    /// <summary>
    /// Owns the active life, migrates loaded saves, and commits each action as one transaction.
    /// </summary>
    public sealed class GameSessionService
    {
        public const int IndexInvestmentMinimumAge = ProgressionStandards.IndexInvestmentMinimumAge;
        public const int IndexInvestmentMinimumSmarts = ProgressionStandards.IndexInvestmentMinimumSmarts;
        public const long IndexInvestmentMinimumEmergencySavingsMinorUnits =
            ProgressionStandards.IndexInvestmentMinimumEmergencySavingsMinorUnits;

        private readonly IEventCatalog eventCatalog;
        private readonly ISaveRepository saveRepository;
        private readonly OutcomeResolver outcomeResolver;
        private readonly Func<DateTimeOffset> utcNow;
        private readonly SaveTransactionRunner transactionRunner;
        private readonly EducationActionService educationActionService;
        private readonly ActionLifecycleService actionLifecycleService;
        private readonly MatchEngine matchEngine;
        private readonly IEffectValueResolver effectValueResolver;

        public SaveEnvelope ActiveSave { get; private set; }
        public ChoiceResolution LastResolution { get; private set; }
        public long LastFinancialImpactMinorUnits { get; private set; }
        public bool HasPendingEvent => !string.IsNullOrEmpty(ActiveSave?.state?.pendingEventId);
        public IEffectValueResolver EffectValueResolver => effectValueResolver;

        public GameSessionService(
            IEventCatalog eventCatalog,
            ISaveRepository saveRepository,
            OutcomeResolver outcomeResolver = null,
            Func<DateTimeOffset> utcNow = null,
            IEffectValueResolver effectValueResolver = null)
        {
            this.eventCatalog = eventCatalog ?? throw new ArgumentNullException(nameof(eventCatalog));
            this.saveRepository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
            this.outcomeResolver = outcomeResolver ?? new OutcomeResolver();
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
            this.effectValueResolver = effectValueResolver ?? new EffectValueResolver();
            transactionRunner = new SaveTransactionRunner(this.saveRepository, this.utcNow);
            educationActionService = new EducationActionService();
            actionLifecycleService = new ActionLifecycleService();
            matchEngine = new MatchEngine();
        }

        public void Start(SaveEnvelope save)
        {
            var validation = SaveValidator.ValidateSave(save);
            if (!validation.isValid)
            {
                throw new ArgumentException(
                    SaveValidator.GetValidationSummary(validation, save?.saveId ?? "unknown-save"),
                    nameof(save));
            }

            ClearAgeIneligiblePendingEvent(save);
            ActiveSave = save;
        }

        public bool TryStartNewLife(SaveEnvelope save, out string summary)
        {
            var validation = SaveValidator.ValidateSave(save);
            if (!validation.isValid)
            {
                summary = SaveValidator.GetValidationSummary(validation, save?.saveId ?? "unknown-save");
                return false;
            }

            EnqueueTransition(save, "new_life", "A new life begins",
                $"{save.state.character.firstName} {save.state.character.lastName} begins a new story.");
            EvaluateAchievements(save);
            var serializedSave = JsonUtility.ToJson(save);
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

            if (!SaveMigrator.TryMigrate(serializedSave, out var save, out var migration, out summary))
            {
                return false;
            }
            var validation = SaveValidator.ValidateSave(save);
            summary = SaveValidator.GetValidationSummary(validation, save?.saveId ?? "unknown-save");
            if (!validation.isValid)
            {
                return false;
            }

            var clearedAgeIneligibleEvent = ClearAgeIneligiblePendingEvent(save);
            ActiveSave = save;
            if (migration.changed)
            {
                summary += $" Migrated {migration.changes.Count} additive v1 field(s).";
            }
            if (clearedAgeIneligibleEvent)
            {
                summary += " Removed an age-ineligible pending event.";
            }
            return true;
        }

        public bool TryResolveChoice(string eventId, string choiceId, out string summary)
        {
            return TryResolveChoice(eventId, choiceId, PaymentMethod.Cash, out summary);
        }

        public bool TryResolveChoice(
            string eventId,
            string choiceId,
            PaymentMethod paymentMethod,
            out string summary)
        {
            LastResolution = null;
            LastFinancialImpactMinorUnits = 0;
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }

            if (!eventCatalog.TryGetEvent(eventId, out var evt) || evt == null)
            {
                summary = $"Event {eventId} was not found.";
                return false;
            }
            if (eventId == RepresentativeEvents.YearInReviewId &&
                !string.Equals(ActiveSave.state.pendingEventId, eventId, StringComparison.Ordinal))
            {
                summary = "The Year in Review reward is not pending or was already claimed.";
                return false;
            }

            if (eventId == RepresentativeEvents.PromInvitationId && choiceId == "attend_prom_together")
            {
                var friend = ActiveSave.state.relationships?.Find(candidate =>
                    candidate != null && candidate.relationshipId == "school_peer_primary");
                if (friend == null ||
                    (friend.relationshipType != "friend" && friend.relationshipType != "best_friend") ||
                    friend.value < 60)
                {
                    summary = "Dating requires an existing friendship with at least 60 relationship strength.";
                    return false;
                }
            }
            if (eventId == RepresentativeEvents.ProposalId)
            {
                var partner = ActiveSave.state.relationships?.Find(candidate =>
                    candidate != null && candidate.relationshipId == "school_peer_primary");
                if (ActiveSave.state.character.age < 24 || partner == null ||
                    partner.relationshipType != "partner" || partner.value < 80)
                {
                    summary = "Marriage proposals require age 24+, a committed partner, and 80 relationship strength.";
                    return false;
                }
            }
            if (eventId == RepresentativeEvents.WeddingId)
            {
                var fiance = ActiveSave.state.relationships?.Find(candidate =>
                    candidate != null && candidate.relationshipId == "school_peer_primary");
                if (ActiveSave.state.character.age < 25 || fiance == null || fiance.relationshipType != "engaged")
                {
                    summary = "Wedding decisions require age 25+ and an active engagement.";
                    return false;
                }
            }
            if (eventId == RepresentativeEvents.MarriageCrossroadsId)
            {
                var spouse = ActiveSave.state.relationships?.Find(candidate =>
                    candidate != null && candidate.relationshipId == "school_peer_primary");
                if (ActiveSave.state.character.age < 25 || spouse == null ||
                    spouse.relationshipType != "married" || spouse.value > 55)
                {
                    summary = "Marriage crossroads require a strained marriage at 55 relationship strength or below.";
                    return false;
                }
            }

            var validation = EventValidator.ValidateEvent(evt);
            if (!validation.isValid)
            {
                summary = EventValidator.GetValidationSummary(validation, eventId);
                return false;
            }
            var characterAge = ActiveSave.state.character.age;
            if (!EventAgeEligibility.IsEligible(evt, characterAge))
            {
                summary = $"{evt.titleKey} is not available at age {characterAge}.";
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
            var financialImpact = ApplyResolution(candidateSave, resolution);
            financialImpact += ApplyAnnualReviewReward(candidateSave, resolution.eventId, resolution.choiceId);
            QueueDeferredAnnualReview(candidateSave, resolution.eventId);
            if (!TryApplyFinancialImpact(candidateSave.state, financialImpact, paymentMethod, out summary))
            {
                if (financialImpact < 0 && paymentMethod == PaymentMethod.Credit)
                {
                    TryCommitCreditDenial(-financialImpact, summary, out summary);
                }
                return false;
            }
            if (financialImpact < 0 && paymentMethod == PaymentMethod.Credit)
            {
                candidateSave.state.character.happiness = ClampStat(
                    candidateSave.state.character.happiness + 1);
                AddLifeFeedEntry(candidateSave, "money",
                    $"Credit approved for {FormatMoney(-financialImpact)} at " +
                    $"{candidateSave.state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR · Happiness +1.");
            }
            EvaluateAchievements(candidateSave);
            if (eventId == RepresentativeEvents.YearInReviewId)
                BeginAnnualCycleIfNeeded(candidateSave.state);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out summary))
            {
                return false;
            }

            ActiveSave = candidateSave;
            LastResolution = resolution;
            LastFinancialImpactMinorUnits = financialImpact;
            summary = resolution.outcome.resultTextKey +
                      (financialImpact == 0 ? string.Empty :
                          $" Financial impact: {FormatSignedMoney(financialImpact)}" +
                          (financialImpact < 0 ? $" paid by {paymentMethod.ToString().ToLowerInvariant()}" : string.Empty) +
                          (financialImpact < 0 && paymentMethod == PaymentMethod.Credit
                              ? $" at {candidateSave.state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR"
                              : string.Empty) + ".");
            return true;
        }

        public bool TryGetPendingEvent(out Event pendingEvent)
        {
            pendingEvent = null;
            var pendingEventId = ActiveSave?.state?.pendingEventId;
            return HasPendingEvent &&
                   eventCatalog.TryGetEvent(pendingEventId, out pendingEvent) &&
                   pendingEvent != null;
        }

        private bool ClearAgeIneligiblePendingEvent(SaveEnvelope save)
        {
            var pendingEventId = save?.state?.pendingEventId;
            if (string.IsNullOrEmpty(pendingEventId) ||
                !eventCatalog.TryGetEvent(pendingEventId, out var pendingEvent) ||
                pendingEvent?.ageRange == null)
            {
                return false;
            }

            var age = save.state.character.age;
            if (EventAgeEligibility.IsEligible(pendingEvent, age))
            {
                return false;
            }

            save.state.pendingEventId = null;
            return true;
        }

        private bool TryCommitCreditDenial(
            long requestedAmount,
            string denialReason,
            out string summary)
        {
            var denialSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(denialSave.state);
            denialSave.revision++;
            denialSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            denialSave.state.character.happiness = ClampStat(denialSave.state.character.happiness - 2);
            AddLifeFeedEntry(denialSave, "money",
                $"Credit denied for {FormatMoney(requestedAmount)} · Happiness −2. {denialReason}");
            var serializedSave = JsonUtility.ToJson(denialSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }
            ActiveSave = denialSave;
            summary = denialReason + " Credit was denied · Happiness −2.";
            return true;
        }

        public bool TryAdvanceMonth(out Event nextEvent, out string summary)
        {
            nextEvent = null;
            LastResolution = null;
            LastFinancialImpactMinorUnits = 0;
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }

            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before advancing another month.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.education?.awaitingDecisionId))
            {
                summary = $"Choose a school path for {ActiveSave.state.education.awaitingDecisionId} before advancing.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            BeginAnnualCycleIfNeeded(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            var paidMonth = candidateSave.state.calendar.monthOfYear;
            var creditInterest = AccrueHouseholdCreditInterest(candidateSave.state.finances);
            var savingsInterest = AccrueSavingsInterest(candidateSave, paidMonth);
            var playerPaycheck = CalculateMonthlyPaycheck(candidateSave.state.career.annualSalaryMinorUnits, paidMonth);
            var spousePaycheck = CalculateMonthlyPaycheck(
                candidateSave.state.finances.spouseAnnualIncomeMinorUnits, paidMonth);
            var paycheck = playerPaycheck + spousePaycheck;
            var taxes = CalculateTaxWithholding(paycheck, candidateSave.state.finances.taxRateBasisPoints);
            var childExpenses = (candidateSave.state.family?.children?.FindAll(child => child.age < 18).Count ?? 0) * 25000L;
            var baseExpenses = candidateSave.state.finances.monthlyLivingExpensesMinorUnits + childExpenses;
            var homeConditionExpense = CalculateHomeConditionExpense(baseExpenses, candidateSave.state.home?.condition ?? 100);
            var expenses = baseExpenses + homeConditionExpense;
            var netCashFlow = paycheck - taxes - expenses;
            ApplyMonthlyCashFlow(candidateSave.state.finances, paycheck, taxes, expenses);
            var businessProfit = ProcessMonthlyBusiness(candidateSave);
            var propertyProfit = ProcessMonthlyProperties(candidateSave, out var propertyIncome, out var propertyExpenses);
            candidateSave.state.finances.lastGrossIncomeMinorUnits = paycheck + propertyIncome;
            candidateSave.state.finances.lastTaxesMinorUnits = taxes;
            candidateSave.state.finances.lastExpensesMinorUnits = expenses + propertyExpenses;
            candidateSave.state.finances.lastCreditInterestMinorUnits = creditInterest;
            candidateSave.state.finances.lastSavingsInterestMinorUnits = savingsInterest;
            candidateSave.state.finances.lastNetCashFlowMinorUnits =
                netCashFlow + businessProfit + propertyProfit + savingsInterest - creditInterest;
            if (!string.IsNullOrEmpty(candidateSave.state.career.roleTitle) &&
                candidateSave.state.career.roleTitle != "Retired")
            {
                candidateSave.state.career.careerProgress = ClampStat(
                    candidateSave.state.career.careerProgress + 1);
            }
            else if (candidateSave.state.career.roleTitle != "Retired")
            {
                candidateSave.state.career.employmentStatus = "unemployed";
                candidateSave.state.career.monthsUnemployed++;
            }
            candidateSave.state.character.happiness = ClampStat(
                candidateSave.state.character.happiness + (netCashFlow >= 0 ? 1 : -2));
            ApplyMonthlyHomeCondition(candidateSave.state, homeConditionExpense);
            AdvanceStatuses(candidateSave.state.statuses);
            AdvanceRelationships(candidateSave);

            var completedYear = paidMonth == 12;
            var previousLifeStage = candidateSave.state.character.lifeStage;
            var previousEducationStage = candidateSave.state.education?.stage;
            candidateSave.rng.step++;
            if (completedYear)
            {
                candidateSave.state.calendar.monthOfYear = 1;
                candidateSave.state.character.age++;
                AdvanceChildAges(candidateSave);
                UpdateLifeAndEducationStage(candidateSave.state);
                if (candidateSave.state.education?.graduatedSecondary == true &&
                    previousEducationStage != "completed_secondary")
                    EnqueueTransition(candidateSave, "graduation", "Graduation",
                        BuildEducationMilestoneSummary(candidateSave.state));
                var healthDecline = GetAnnualHealthDecline(candidateSave.state.character.age);
                candidateSave.state.character.health = ClampStat(
                    candidateSave.state.character.health - healthDecline);
                if (candidateSave.state.character.health <= 0)
                {
                    FinalizeLife(candidateSave.state.character, "deceased", "health_decline");
                    EnqueueTransition(candidateSave, "death", "Life remembered",
                        $"{candidateSave.state.character.firstName}'s life ended at age {candidateSave.state.character.age}.");
                }
                ApplyAnnualIndexFundReturn(candidateSave);
                EvaluateAnnualCareerStability(candidateSave);
                CompleteAnnualCycle(candidateSave.state);
            }
            else
            {
                candidateSave.state.annualReview.monthsAccumulated++;
            }
            AdvancePendingFamilyPath(candidateSave);
            if (!completedYear) candidateSave.state.calendar.monthOfYear++;
            var lifeEnded = IsLifeEnded(candidateSave.state.character);
            nextEvent = lifeEnded ? null : SelectEligibleEvent(candidateSave, paidMonth, completedYear);
            candidateSave.state.pendingEventId = nextEvent?.id;
            candidateSave.state.calendar.quietMonthsSinceEvent = nextEvent == null
                ? candidateSave.state.calendar.quietMonthsSinceEvent + 1
                : 0;

            var hasCareer = !string.IsNullOrEmpty(candidateSave.state.career.roleTitle) &&
                            candidateSave.state.career.roleTitle != "Retired";
            var cashFlowSummary = $"gross {FormatMoney(paycheck)}, taxes {FormatMoney(taxes)}, expenses {FormatMoney(expenses)}, net {FormatSignedMoney(netCashFlow)}" +
                                  (businessProfit != 0 ? $", business {FormatSignedMoney(businessProfit)}" : string.Empty) +
                                  (propertyIncome > 0 || propertyExpenses > 0 ? $", property {FormatSignedMoney(propertyProfit)}" : string.Empty) +
                                  (homeConditionExpense > 0 ? $", home repair overhead {FormatMoney(homeConditionExpense)}" : string.Empty) +
                                  (creditInterest > 0 ? $", credit interest {FormatMoney(creditInterest)}" : string.Empty) +
                                  (savingsInterest > 0 ? $", savings interest +{FormatMoney(savingsInterest)}" : string.Empty);
            var stageChanged = !string.Equals(previousLifeStage, candidateSave.state.character.lifeStage, StringComparison.Ordinal);
            var educationStageChanged = !string.Equals(
                previousEducationStage,
                candidateSave.state.education?.stage,
                StringComparison.Ordinal);
            if (!hasCareer)
            {
                var stageName = ToDisplayName(candidateSave.state.character.lifeStage);
                summary = nextEvent != null
                    ? $"Age {candidateSave.state.character.age}, month {candidateSave.state.calendar.monthOfYear}: a new {nextEvent.category.ToString().ToLowerInvariant()} event is ready."
                    : completedYear
                        ? educationStageChanged
                            ? BuildEducationMilestoneSummary(candidateSave.state)
                            : stageChanged
                                ? $"Age {candidateSave.state.character.age}: entered {stageName}."
                                : $"Age {candidateSave.state.character.age}: another year of {stageName} began."
                        : $"Age {candidateSave.state.character.age}, month {candidateSave.state.calendar.monthOfYear}: {stageName} continued.";
            }
            else
            {
                summary = nextEvent != null
                    ? $"Month {paidMonth}: {cashFlowSummary}; a new {nextEvent.category.ToString().ToLowerInvariant()} event is ready."
                    : completedYear
                        ? educationStageChanged
                            ? $"{BuildEducationMilestoneSummary(candidateSave.state)} {cashFlowSummary}."
                            : $"Age {candidateSave.state.character.age}: {cashFlowSummary}; completed a quiet year."
                        : $"Month {paidMonth}: {cashFlowSummary}.";
            }

            if (lifeEnded)
            {
                EvaluateAchievements(candidateSave);
                summary = BuildFinalLifeSummary(candidateSave);
            }

            AddLifeFeedEntry(
                candidateSave,
                nextEvent != null ? "event" : lifeEnded || stageChanged || educationStageChanged ? "milestone" : "time",
                summary);
            EvaluateAchievements(candidateSave);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                nextEvent = null;
                summary = commitSummary;
                return false;
            }

            ActiveSave = candidateSave;
            return true;
        }

        public bool TryTransferSavings(
            SavingsTransferType transferType,
            long amountMinorUnits,
            out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (amountMinorUnits <= 0)
            {
                summary = "Transfer amount must be at least one cent.";
                return false;
            }

            if (!transactionRunner.TryExecute(
                    ActiveSave,
                    candidate => ApplySavingsTransfer(candidate, transferType, amountMinorUnits),
                    out var committedSave,
                    out summary))
            {
                return false;
            }
            ActiveSave = committedSave;
            return true;
        }

        public bool TryTransferSavingsPercentage(
            SavingsTransferType transferType,
            int percentage,
            out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            var available = transferType == SavingsTransferType.Deposit
                ? ActiveSave.state.finances.cashMinorUnits
                : ActiveSave.state.finances.savingsMinorUnits;
            if (!ActionAmountService.TrySelectPercentage(
                    available, percentage, out var amountMinorUnits, out summary))
            {
                return false;
            }
            return TryTransferSavings(transferType, amountMinorUnits, out summary);
        }

        public bool TryRepayHouseholdCredit(long amountMinorUnits, out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (amountMinorUnits <= 0)
            {
                summary = "Repayment amount must be at least one cent.";
                return false;
            }
            if (!transactionRunner.TryExecute(
                    ActiveSave,
                    candidate => ApplyHouseholdCreditRepayment(candidate, amountMinorUnits),
                    out var committedSave,
                    out summary)) return false;
            ActiveSave = committedSave;
            return true;
        }

        public bool TryRepayHouseholdCreditPercentage(int percentage, out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            var available = Math.Min(
                ActiveSave.state.finances.cashMinorUnits,
                ActiveSave.state.finances.householdCreditBalanceMinorUnits);
            if (!ActionAmountService.TrySelectPercentage(
                    available, percentage, out var amountMinorUnits, out summary)) return false;
            return TryRepayHouseholdCredit(amountMinorUnits, out summary);
        }

        private static TransactionMutationResult ApplyHouseholdCreditRepayment(
            SaveEnvelope save,
            long amountMinorUnits)
        {
            NormalizeProgressCollections(save.state);
            var finances = save.state.finances;
            if (finances.householdCreditBalanceMinorUnits <= 0)
                return TransactionMutationResult.Failure("There is no revolving credit balance to repay.");
            if (amountMinorUnits > finances.cashMinorUnits)
                return TransactionMutationResult.Failure("Repayment exceeds available cash.");
            if (amountMinorUnits > finances.householdCreditBalanceMinorUnits)
                return TransactionMutationResult.Failure("Repayment exceeds the revolving credit balance.");

            finances.cashMinorUnits -= amountMinorUnits;
            finances.householdCreditBalanceMinorUnits -= amountMinorUnits;
            finances.debtMinorUnits = Math.Max(0, finances.debtMinorUnits - amountMinorUnits);
            if (finances.householdCreditBalanceMinorUnits == 0)
                finances.householdCreditAprBasisPoints = 0;
            save.state.moneyTransactions.Add(new MoneyTransactionState
            {
                transactionId = $"money_{save.revision}_credit_repayment",
                type = "credit_repayment",
                amountMinorUnits = amountMinorUnits,
                cashBalanceMinorUnits = finances.cashMinorUnits,
                savingsBalanceMinorUnits = finances.savingsMinorUnits,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (save.state.moneyTransactions.Count > 100)
                save.state.moneyTransactions.RemoveAt(0);
            AddLifeFeedEntry(save, "money",
                $"Repaid {FormatMoney(amountMinorUnits)} of revolving credit · Balance {FormatMoney(finances.householdCreditBalanceMinorUnits)}.");
            return TransactionMutationResult.Success(
                $"Repaid {FormatMoney(amountMinorUnits)} · Credit balance {FormatMoney(finances.householdCreditBalanceMinorUnits)}" +
                $" · Cash {FormatMoney(finances.cashMinorUnits)}.");
        }

        public static bool TryGetIndexInvestmentRequirement(GameState state, out string requirement)
        {
            if (state?.character == null || state.finances == null)
            {
                requirement = "Financial information is unavailable.";
                return false;
            }
            if (state.character.age < IndexInvestmentMinimumAge)
            {
                requirement = $"Index investing unlocks at age {IndexInvestmentMinimumAge}.";
                return false;
            }
            if (state.character.smarts < IndexInvestmentMinimumSmarts)
            {
                requirement = $"Reach {IndexInvestmentMinimumSmarts} Smarts to understand long-term investment risk.";
                return false;
            }
            if (state.education == null ||
                !state.education.graduatedSecondary &&
                state.education.qualificationExperience < EducationActionService.CertificateQualificationExperience)
            {
                requirement = "Complete secondary school or reach the Certificate qualification tier.";
                return false;
            }
            var requiredSavings = Math.Max(
                IndexInvestmentMinimumEmergencySavingsMinorUnits,
                state.finances.monthlyLivingExpensesMinorUnits);
            if (state.finances.savingsMinorUnits < requiredSavings)
            {
                requirement = $"Build an emergency savings cushion of {FormatMoney(requiredSavings)} first.";
                return false;
            }
            if (state.finances.cashMinorUnits < 1000)
            {
                requirement = "At least $10 available cash is required to invest.";
                return false;
            }
            requirement = string.Empty;
            return true;
        }

        public bool TryInvestInIndexFund(long amountMinorUnits, out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (amountMinorUnits < 1000)
            {
                summary = "Index contributions must be at least $10.";
                return false;
            }
            if (!transactionRunner.TryExecute(
                    ActiveSave,
                    candidate => ApplyIndexInvestment(candidate, amountMinorUnits),
                    out var committedSave,
                    out summary)) return false;
            ActiveSave = committedSave;
            return true;
        }

        private static TransactionMutationResult ApplyIndexInvestment(
            SaveEnvelope save,
            long amountMinorUnits)
        {
            NormalizeProgressCollections(save.state);
            if (!TryGetIndexInvestmentRequirement(save.state, out var requirement))
                return TransactionMutationResult.Failure(requirement);
            if (amountMinorUnits > save.state.finances.cashMinorUnits)
                return TransactionMutationResult.Failure("Investment exceeds available cash.");
            var finances = save.state.finances;
            finances.cashMinorUnits -= amountMinorUnits;
            finances.indexFundMinorUnits += amountMinorUnits;
            finances.indexFundContributionsMinorUnits += amountMinorUnits;
            save.state.moneyTransactions.Add(new MoneyTransactionState
            {
                transactionId = $"money_{save.revision}_index_investment",
                type = "index_investment",
                amountMinorUnits = amountMinorUnits,
                cashBalanceMinorUnits = finances.cashMinorUnits,
                savingsBalanceMinorUnits = finances.savingsMinorUnits,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (save.state.moneyTransactions.Count > 100)
                save.state.moneyTransactions.RemoveAt(0);
            AddLifeFeedEntry(save, "money",
                $"Contributed {FormatMoney(amountMinorUnits)} to a broad index fund · Market returns are not guaranteed.");
            return TransactionMutationResult.Success(
                $"Invested {FormatMoney(amountMinorUnits)} · Index fund {FormatMoney(finances.indexFundMinorUnits)}" +
                " · Returns can rise or fall.");
        }

        private static TransactionMutationResult ApplySavingsTransfer(
            SaveEnvelope save,
            SavingsTransferType transferType,
            long amountMinorUnits)
        {
            NormalizeProgressCollections(save.state);
            var finances = save.state.finances;
            var available = transferType == SavingsTransferType.Deposit
                ? finances.cashMinorUnits
                : finances.savingsMinorUnits;
            if (amountMinorUnits > available)
            {
                return TransactionMutationResult.Failure(
                    transferType == SavingsTransferType.Deposit
                        ? "Deposit exceeds available cash."
                        : "Withdrawal exceeds available savings.");
            }

            if (transferType == SavingsTransferType.Deposit)
            {
                finances.cashMinorUnits -= amountMinorUnits;
                finances.savingsMinorUnits += amountMinorUnits;
            }
            else
            {
                finances.savingsMinorUnits -= amountMinorUnits;
                finances.cashMinorUnits += amountMinorUnits;
            }

            var type = transferType == SavingsTransferType.Deposit
                ? "savings_deposit"
                : "savings_withdrawal";
            save.state.moneyTransactions.Add(new MoneyTransactionState
            {
                transactionId = $"money_{save.revision}_{type}",
                type = type,
                amountMinorUnits = amountMinorUnits,
                cashBalanceMinorUnits = finances.cashMinorUnits,
                savingsBalanceMinorUnits = finances.savingsMinorUnits,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (save.state.moneyTransactions.Count > 100)
                save.state.moneyTransactions.RemoveAt(0);
            var verb = transferType == SavingsTransferType.Deposit ? "Deposited" : "Withdrew";
            AddLifeFeedEntry(save, "money",
                $"{verb} {FormatMoney(amountMinorUnits)} · Savings {FormatMoney(finances.savingsMinorUnits)}.");
            return TransactionMutationResult.Success(
                $"{verb} {FormatMoney(amountMinorUnits)} · Cash {FormatMoney(finances.cashMinorUnits)}" +
                $" · Savings {FormatMoney(finances.savingsMinorUnits)}.");
        }

        public bool TryAdvanceYear(
            out int monthsProcessed,
            out Event nextEvent,
            out string summary)
        {
            return TryAdvanceMonthsCore(
                12, "Twelve months completed.", out monthsProcessed, out nextEvent, out summary);
        }

        public bool TryPersistUiWorkflow(
            int queuedYearMonthsRemaining,
            bool completionPending,
            string completionSummary,
            string pendingStudyDifficulty,
            string pendingStudyActionId,
            out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    candidate.state.uiWorkflow ??= new UiWorkflowState();
                    candidate.state.uiWorkflow.queuedYearMonthsRemaining =
                        Math.Max(0, Math.Min(12, queuedYearMonthsRemaining));
                    candidate.state.uiWorkflow.queuedYearCompletionPending = completionPending;
                    candidate.state.uiWorkflow.queuedYearCompletionSummary = completionSummary ?? string.Empty;
                    candidate.state.uiWorkflow.pendingStudyDifficulty = pendingStudyDifficulty ?? string.Empty;
                    candidate.state.uiWorkflow.pendingStudyActionId = pendingStudyActionId ?? string.Empty;
                    return TransactionMutationResult.Success("UI workflow saved.");
                },
                out var committed,
                out summary);
            if (succeeded) ActiveSave = committed;
            return succeeded;
        }

        public bool TryPersistUiNavigation(
            string activeDestination,
            string selectedTabId,
            string selectedEntityId,
            float activeScrollX,
            float activeScrollY,
            out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    candidate.state.uiWorkflow ??= new UiWorkflowState();
                    candidate.state.uiWorkflow.activeDestination = activeDestination ?? string.Empty;
                    candidate.state.uiWorkflow.selectedTabId = selectedTabId ?? string.Empty;
                    candidate.state.uiWorkflow.selectedEntityId = selectedEntityId ?? string.Empty;
                    candidate.state.uiWorkflow.activeScrollX = Math.Max(0f, activeScrollX);
                    candidate.state.uiWorkflow.activeScrollY = Math.Max(0f, activeScrollY);
                    return TransactionMutationResult.Success("UI navigation saved.");
                },
                out var committed,
                out summary);
            if (succeeded) ActiveSave = committed;
            return succeeded;
        }

        public bool TryAdvanceMonths(
            int maximumMonths,
            out int monthsProcessed,
            out Event nextEvent,
            out string summary)
        {
            if (maximumMonths < 1 || maximumMonths > 12)
            {
                monthsProcessed = 0;
                nextEvent = null;
                summary = "Advance span must be between 1 and 12 months.";
                return false;
            }

            var completion = maximumMonths == 1
                ? "Requested month completed."
                : $"Requested {maximumMonths} months completed.";
            return TryAdvanceMonthsCore(
                maximumMonths, completion, out monthsProcessed, out nextEvent, out summary);
        }

        private bool TryAdvanceMonthsCore(
            int maximumMonths,
            string completionSummary,
            out int monthsProcessed,
            out Event nextEvent,
            out string summary)
        {
            monthsProcessed = 0;
            nextEvent = null;
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }

            var startingAge = ActiveSave.state.character.age;
            var startingCash = ActiveSave.state.finances?.cashMinorUnits ?? 0L;
            for (var month = 0; month < maximumMonths; month++)
            {
                var workflow = ActiveSave.state.uiWorkflow;
                var queuedBeforeMonth = workflow?.queuedYearMonthsRemaining ?? 0;
                if (workflow != null && queuedBeforeMonth > 0)
                    workflow.queuedYearMonthsRemaining = queuedBeforeMonth - 1;
                if (!TryAdvanceMonth(out nextEvent, out var monthlySummary))
                {
                    if (workflow != null) workflow.queuedYearMonthsRemaining = queuedBeforeMonth;
                    summary = monthsProcessed == 0
                        ? monthlySummary
                        : BuildAdvanceYearSummary(
                            monthsProcessed, startingAge, startingCash,
                            $"Stopped: {monthlySummary}");
                    if (monthsProcessed > 0) TryCommitAdvanceYearFeed(summary, out summary);
                    return monthsProcessed > 0;
                }

                monthsProcessed++;
                if (nextEvent != null || IsLifeEnded(ActiveSave.state.character) ||
                    !string.IsNullOrEmpty(ActiveSave.state.education?.awaitingDecisionId))
                {
                    var stopReason = nextEvent != null
                        ? $"Stopped for {nextEvent.titleKey}."
                        : IsLifeEnded(ActiveSave.state.character)
                            ? "Stopped because this life ended."
                            : "Stopped for a required school-path decision.";
                    summary = BuildAdvanceYearSummary(
                        monthsProcessed, startingAge, startingCash, stopReason);
                    TryCommitAdvanceYearFeed(summary, out summary);
                    return true;
                }
            }

            summary = BuildAdvanceYearSummary(
                monthsProcessed, startingAge, startingCash, completionSummary);
            TryCommitAdvanceYearFeed(summary, out summary);
            return true;
        }

        private bool TryCommitAdvanceYearFeed(string yearSummary, out string summary)
        {
            var candidateSave = CloneSave(ActiveSave);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            AddLifeFeedEntry(candidateSave, "year", yearSummary);
            if (!saveRepository.TryCommitAutosave(JsonUtility.ToJson(candidateSave), out var commitSummary))
            {
                summary = yearSummary + $" Annual feed summary was not saved: {commitSummary}";
                return false;
            }

            ActiveSave = candidateSave;
            summary = yearSummary;
            return true;
        }

        private string BuildAdvanceYearSummary(
            int monthsProcessed,
            int startingAge,
            long startingCash,
            string outcome)
        {
            var ageDelta = ActiveSave.state.character.age - startingAge;
            var currentCash = ActiveSave.state.finances?.cashMinorUnits ?? 0L;
            var cashDelta = currentCash - startingCash;
            var summary = $"Advanced {monthsProcessed} month{(monthsProcessed == 1 ? string.Empty : "s")}" +
                   $" · Age {(ageDelta >= 0 ? "+" : string.Empty)}{ageDelta}" +
                   $" · Cash {FormatSignedMoney(cashDelta)} · {outcome}";
            if (ageDelta > 0 && ActiveSave.state.annualReview?.completedAtAge == ActiveSave.state.character.age)
                summary += " " + BuildAnnualReviewSummary(ActiveSave.state);
            return summary;
        }

        public static string BuildAnnualReviewSummary(GameState state)
        {
            var review = state?.annualReview;
            if (review == null || review.completedAtAge < 0) return "A full year has passed.";
            return $"Age {review.completedAtAge} review · Cash {FormatSignedMoney(review.cashDeltaMinorUnits)}" +
                   $" · Savings {FormatSignedMoney(review.savingsDeltaMinorUnits)}" +
                   $" · Investments {FormatSignedMoney(review.indexFundDeltaMinorUnits)}" +
                   $" · Debt {FormatSignedMoney(review.debtDeltaMinorUnits)}" +
                   $" · Health {FormatSignedNumber(review.healthDelta)}" +
                   $" · Happiness {FormatSignedNumber(review.happinessDelta)}" +
                   $" · Smarts {FormatSignedNumber(review.smartsDelta)}" +
                   $" · Career {FormatSignedNumber(review.careerProgressDelta)}" +
                   $" · Qualification XP {FormatSignedNumber(review.qualificationExperienceDelta)}" +
                   $" · Skills {FormatSignedNumber(review.skillExperienceDelta)}" +
                   $" · Relationships {FormatSignedNumber(review.relationshipValueDelta)}" +
                   BuildAnnualOutcomeText(review.majorOutcomeSummaries) + ". Choose a focus for next year.";
        }

        private static string BuildAnnualOutcomeText(List<string> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0) return string.Empty;
            return " · Highlights: " + string.Join(" | ", outcomes);
        }

        private static string FormatSignedNumber(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
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

        public static int GetAnnualHealthDecline(int age)
        {
            if (age < 50) return 0;
            if (age < 65) return 1;
            if (age < 80) return 2;
            return 4;
        }

        public static string BuildFinalLifeSummary(SaveEnvelope save)
        {
            var state = save.state;
            var character = state.character;
            var name = string.IsNullOrEmpty(character.firstName)
                ? "This life"
                : $"{character.firstName}'s life";
            var ending = character.lifeStatus == "retired" ? "retirement" : "death";
            var netWorth = CalculateNetWorth(state);
            return $"{name} ended in {ending} at age {character.endedAtAge} · Net worth {FormatMoney(netWorth)}" +
                   $" · {state.achievements.Count} achievements · {state.eventHistory.Count} events" +
                   $" · {state.lifeFeed.Count} life chapters";
        }

        private static void UpdateLifeAndEducationStage(GameState state)
        {
            state.character.lifeStage = GetLifeStage(state.character.age);
            state.education ??= new EducationState();
            if (state.education.schoolPath == "left_school")
            {
                state.education.stage = "left_school";
                state.education.awaitingDecisionId = string.Empty;
                state.education.graduatedSecondary = false;
                return;
            }
            state.education.stage = GetEducationStage(state.character.age);
            switch (state.character.age)
            {
                case 6:
                    state.education.awaitingDecisionId = "education_primary_enrollment";
                    break;
                case 12:
                    state.education.awaitingDecisionId = "education_middle_transition";
                    break;
                case 15:
                    state.education.awaitingDecisionId = "education_high_transition";
                    break;
                case 18:
                    state.education.awaitingDecisionId = string.Empty;
                    state.education.graduatedSecondary = state.education.schoolPath != "left_school";
                    break;
            }
        }

        public bool TryChooseSchoolPath(SchoolPathChoice choice, out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before choosing a school path.";
                return false;
            }

            NormalizeProgressCollections(ActiveSave.state);
            NormalizeHomeInventory(ActiveSave.state.home);
            var decisionId = ActiveSave.state.education?.awaitingDecisionId;
            if (string.IsNullOrEmpty(decisionId))
            {
                summary = "No school transition decision is currently required.";
                return false;
            }
            if (!IsSchoolPathChoiceAvailable(decisionId, choice))
            {
                summary = $"{ToDisplayName(choice.ToString())} is not available for {decisionId}.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            var education = candidateSave.state.education;
            var choiceId = ToSchoolPathChoiceId(choice);
            education.schoolPath = choiceId;
            education.awaitingDecisionId = string.Empty;
            if (education != null && candidateSave.state.annualReview?.completedAtAge == candidateSave.state.character.age &&
                candidateSave.state.annualReview.rewardedAtAge != candidateSave.state.character.age &&
                eventCatalog.TryGetEvent(RepresentativeEvents.YearInReviewId, out _))
            {
                candidateSave.state.pendingEventId = RepresentativeEvents.YearInReviewId;
            }

            switch (choice)
            {
                case SchoolPathChoice.PublicSchool:
                    ApplySkillXp(candidateSave.state.skills, "social", 8);
                    summary = "Enrolled in public school · Social XP +8";
                    break;
                case SchoolPathChoice.Homeschool:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 2);
                    ApplySkillXp(candidateSave.state.skills, "learning", 10);
                    summary = "Started homeschooling · Smarts +2 · Learning XP +10";
                    break;
                case SchoolPathChoice.AcademicTrack:
                    ApplySkillXp(candidateSave.state.skills, "learning", 15);
                    summary = "Chose the academic track · Learning XP +15";
                    break;
                case SchoolPathChoice.VocationalTrack:
                    ApplySkillXp(candidateSave.state.skills, "practical", 15);
                    summary = "Chose the vocational track · Practical XP +15";
                    break;
                case SchoolPathChoice.LeaveSchool:
                    education.stage = "left_school";
                    education.schoolPath = "left_school";
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 3);
                    summary = "Left school · Happiness +3 · Some careers and education paths are now locked";
                    break;
                default:
                    summary = $"School path {choice} is not supported.";
                    return false;
            }

            var newPeer = EnsureSchoolPeer(candidateSave, decisionId, choice);
            if (newPeer != null)
            {
                summary += $" · Met {newPeer.displayName}";
            }

            candidateSave.state.lifeDecisions.Add(new LifeDecisionState
            {
                decisionId = decisionId,
                choiceId = choiceId,
                age = candidateSave.state.character.age,
                monthOfYear = candidateSave.state.calendar.monthOfYear,
                revision = candidateSave.revision,
                timestampUtc = candidateSave.updatedAtUtc
            });
            AddLifeFeedEntry(candidateSave, "education", summary);
            EvaluateAchievements(candidateSave);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }

            ActiveSave = candidateSave;
            return true;
        }

        private static RelationshipState EnsureSchoolPeer(
            SaveEnvelope save,
            string decisionId,
            SchoolPathChoice choice)
        {
            if (choice == SchoolPathChoice.LeaveSchool) return null;
            var stageId = decisionId == "education_primary_enrollment" ? "primary" :
                decisionId == "education_middle_transition" ? "middle" : "high";
            var relationshipId = $"school_peer_{stageId}";
            if (save.state.relationships.Exists(candidate =>
                    candidate != null && candidate.relationshipId == relationshipId))
            {
                return null;
            }

            var names = new[] { "Alex", "Jordan", "Maya", "Noah", "Zoe", "Eli", "Avery", "Kai" };
            var nameIndex = (int)(Math.Abs(
                (long)save.rng.seed + save.state.character.age + decisionId.Length) % names.Length);
            var peer = new RelationshipState
            {
                relationshipId = relationshipId,
                displayName = names[nameIndex],
                relationshipType = "friend",
                origin = choice == SchoolPathChoice.Homeschool ? "neighborhood" : $"{stageId}_school",
                introducedAtAge = save.state.character.age,
                value = choice == SchoolPathChoice.Homeschool ? 48 : 55
            };
            save.state.relationships.Add(peer);
            return peer;
        }

        public static bool IsSchoolPathChoiceAvailable(string decisionId, SchoolPathChoice choice)
        {
            switch (decisionId)
            {
                case "education_primary_enrollment":
                    return choice == SchoolPathChoice.PublicSchool || choice == SchoolPathChoice.Homeschool;
                case "education_middle_transition":
                    return choice == SchoolPathChoice.PublicSchool || choice == SchoolPathChoice.Homeschool ||
                           choice == SchoolPathChoice.LeaveSchool;
                case "education_high_transition":
                    return choice == SchoolPathChoice.AcademicTrack || choice == SchoolPathChoice.VocationalTrack ||
                           choice == SchoolPathChoice.LeaveSchool;
                default:
                    return false;
            }
        }

        private static string ToSchoolPathChoiceId(SchoolPathChoice choice)
        {
            switch (choice)
            {
                case SchoolPathChoice.PublicSchool: return "public_school";
                case SchoolPathChoice.Homeschool: return "homeschool";
                case SchoolPathChoice.AcademicTrack: return "academic_track";
                case SchoolPathChoice.VocationalTrack: return "vocational_track";
                case SchoolPathChoice.LeaveSchool: return "left_school";
                default: return choice.ToString().ToLowerInvariant();
            }
        }

        private static string BuildEducationMilestoneSummary(GameState state)
        {
            var age = state.character.age;
            switch (state.education?.stage)
            {
                case "primary_school": return $"Age {age}: started primary school.";
                case "middle_school": return $"Age {age}: started middle school.";
                case "high_school": return $"Age {age}: started high school.";
                case "completed_secondary": return $"Age {age}: completed secondary school.";
                default: return $"Age {age}: entered {ToDisplayName(state.education?.stage)}.";
            }
        }

        private static string ToDisplayName(string id)
        {
            return string.IsNullOrEmpty(id)
                ? "life"
                : id.Replace('_', ' ');
        }

        public bool TryPerformActivity(ActivityType activityType, out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }

            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before choosing an activity.";
                return false;
            }

            if (!TryGetActivityRequirement(ActiveSave.state, activityType, out var activityRequirement))
            {
                summary = activityRequirement;
                if (activityType == ActivityType.FamilyMovieCredit &&
                    ActiveSave.state.character.age >= 18)
                {
                    TryCommitCreditDenial(
                        CalculateFamilyMovieCost(ActiveSave.state), activityRequirement, out summary);
                }
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
                case ActivityType.Study:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 2);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness - 1);
                    ApplySkillXp(candidateSave.state.skills, "learning", 10);
                    summary = "Study complete · Smarts +2 · Happiness −1 · Learning XP +10";
                    break;
                case ActivityType.Workout:
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 2);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 1);
                    ApplySkillXp(candidateSave.state.skills, "fitness", 10);
                    summary = "Workout complete · Health +2 · Happiness +1 · Fitness XP +10";
                    break;
                case ActivityType.Play:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 3);
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 1);
                    ApplySkillXp(candidateSave.state.skills, "play", 8);
                    summary = "Play complete · Happiness +3 · Health +1 · Play XP +8";
                    break;
                case ActivityType.Rest:
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 2);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 1);
                    summary = "Rest complete · Health +2 · Happiness +1";
                    break;
                case ActivityType.FamilyTime:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 2);
                    candidateSave.state.household.happiness = ClampStat(candidateSave.state.household.happiness + 2);
                    candidateSave.state.household.cohesion = ClampStat(candidateSave.state.household.cohesion + 2);
                    foreach (var relationship in candidateSave.state.relationships)
                    {
                        if (IsHouseholdRelationship(relationship))
                            relationship.value = ClampStat(relationship.value + 2);
                    }
                    summary = "Spent time with family · Happiness +2 · Household happiness +2 · Cohesion +2 · Family relationships +2";
                    break;
                case ActivityType.FamilyMovie:
                    var movieCost = CalculateFamilyMovieCost(candidateSave.state);
                    candidateSave.state.finances.cashMinorUnits -= movieCost;
                    ApplyFamilyMovieBenefits(candidateSave.state);
                    summary = BuildFamilyMovieSummary(movieCost, false);
                    break;
                case ActivityType.FamilyMovieCredit:
                    var creditMovieCost = CalculateFamilyMovieCost(candidateSave.state);
                    AddHouseholdCreditPurchase(candidateSave.state, creditMovieCost);
                    ApplyFamilyMovieBenefits(candidateSave.state);
                    candidateSave.state.character.happiness = ClampStat(
                        candidateSave.state.character.happiness + 1);
                    summary = BuildFamilyMovieSummary(creditMovieCost, true) +
                              $" · APR {candidateSave.state.finances.householdCreditAprBasisPoints / 100m:0.00}%" +
                              " · Credit approved · Happiness +1";
                    break;
                case ActivityType.Explore:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 2);
                    ApplySkillXp(candidateSave.state.skills, "curiosity", 8);
                    summary = "Explored something new · Happiness +2 · Curiosity XP +8";
                    break;
                case ActivityType.AttendSchool:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 1);
                    ApplySkillXp(candidateSave.state.skills, "learning", 8);
                    summary = "Attended school · Smarts +1 · Learning XP +8";
                    break;
                case ActivityType.JoinClub:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 1);
                    ApplySkillXp(candidateSave.state.skills, "social", 10);
                    summary = "Joined a school club · Happiness +1 · Social XP +10";
                    break;
                case ActivityType.WorkShift:
                    candidateSave.state.career.careerProgress = ClampStat(candidateSave.state.career.careerProgress + 4);
                    summary = "Completed a work shift · Career progress +4";
                    break;
                case ActivityType.Overtime:
                    var overtimePay = CalculateHourlyRateMinorUnits(candidateSave.state.career.annualSalaryMinorUnits) * 4;
                    var fitnessLevel = GetSkillLevel(GetSkillExperience(candidateSave.state.skills, "fitness"));
                    var overtimeHealthCost = fitnessLevel >= 2 ? 1 : 2;
                    candidateSave.state.finances.cashMinorUnits += overtimePay;
                    candidateSave.state.character.health = ClampStat(
                        candidateSave.state.character.health - overtimeHealthCost);
                    candidateSave.state.career.careerProgress = ClampStat(candidateSave.state.career.careerProgress + 6);
                    summary = $"Worked overtime · Cash +{FormatPreciseMoney(overtimePay)} · Career progress +6" +
                              $" · Health −{overtimeHealthCost}" +
                              (fitnessLevel >= 2 ? " · Fitness reduced strain" : string.Empty);
                    break;
                case ActivityType.Training:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 1);
                    ApplySkillXp(candidateSave.state.skills, "professional", 10);
                    summary = "Completed professional training · Smarts +1 · Professional XP +10";
                    break;
                case ActivityType.Socialize:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 2);
                    ApplySkillXp(candidateSave.state.skills, "social", 8);
                    summary = "Socialized · Happiness +2 · Social XP +8";
                    break;
                case ActivityType.Hobby:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 3);
                    ApplySkillXp(candidateSave.state.skills, "hobby", 8);
                    summary = "Enjoyed a hobby · Happiness +3 · Hobby XP +8";
                    break;
                case ActivityType.Checkup:
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 3);
                    summary = "Completed a health checkup · Health +3";
                    break;
                default:
                    summary = $"Activity {activityType} is not supported.";
                    return false;
            }

            AddOrRefreshStatus(candidateSave.state.statuses, cooldownStatusId, 1);
            AddLifeFeedEntry(candidateSave, "activity", summary);
            EvaluateAchievements(candidateSave);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }

            ActiveSave = candidateSave;
            return true;
        }

        public bool TryPerformHomeAction(HomeActionType actionType, out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before using the home.";
                return false;
            }

            NormalizeProgressCollections(ActiveSave.state);
            var cooldownId = $"home_{actionType.ToString().ToLowerInvariant()}_used";
            if (ActiveSave.state.statuses.Exists(status => status.statusId == cooldownId))
            {
                summary = $"You already completed this home action this month. Advance the month to use it again.";
                return false;
            }

            var actionDefinition = HomeContentCatalog.GetAction(ActiveSave.state.home.homeId, actionType);
            if (actionDefinition == null)
            {
                summary = $"Home action {actionType} is not authored for {ActiveSave.state.home.homeId}.";
                return false;
            }
            var caregiverHandlesMaintenance =
                actionType == HomeActionType.Maintain && ActiveSave.state.character.age < 18;
            var cost = caregiverHandlesMaintenance ? 0 : actionDefinition.costMinorUnits;
            if (ActiveSave.state.finances.cashMinorUnits < cost)
            {
                summary = $"This home action costs {FormatMoney(cost)}, but only {FormatMoney(ActiveSave.state.finances.cashMinorUnits)} is available.";
                return false;
            }
            if (actionType == HomeActionType.Read && ActiveSave.state.home.readingMaterialStock <= 0)
            {
                summary = "No reading materials remain. Maintain the home to restock them.";
                return false;
            }
            if (actionType == HomeActionType.Train && ActiveSave.state.home.trainingEquipmentCondition < 10)
            {
                summary = "The training equipment needs maintenance before it can be used safely.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.state.home ??= new HomeState();
            NormalizeHomeInventory(candidateSave.state.home);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            candidateSave.state.finances.cashMinorUnits -= cost;
            var homeBonus = candidateSave.state.home.upgradeLevel;

            switch (actionType)
            {
                case HomeActionType.Read:
                    candidateSave.state.home.readingMaterialStock -= actionDefinition.capacityConsumed;
                    FindHomeInventoryItem(candidateSave.state.home, "starter_books").quantity = candidateSave.state.home.readingMaterialStock;
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 1);
                    ApplySkillXp(candidateSave.state.skills, "learning", 8 + homeBonus * 2);
                    candidateSave.state.home.condition = ClampStat(candidateSave.state.home.condition + actionDefinition.conditionDelta);
                    candidateSave.state.home.improvementProgress = ClampStat(candidateSave.state.home.improvementProgress + actionDefinition.improvementProgressDelta);
                    summary = $"Read at home · Cost {FormatMoney(cost)} · Smarts +1 · Learning XP +{8 + homeBonus * 2} · Home condition −1";
                    break;
                case HomeActionType.Train:
                    candidateSave.state.home.trainingEquipmentCondition = ClampStat(
                        candidateSave.state.home.trainingEquipmentCondition - actionDefinition.capacityConsumed);
                    FindHomeInventoryItem(candidateSave.state.home, "starter_training_kit").condition = candidateSave.state.home.trainingEquipmentCondition;
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 1);
                    ApplySkillXp(candidateSave.state.skills, "fitness", 10 + homeBonus * 2);
                    candidateSave.state.home.condition = ClampStat(candidateSave.state.home.condition + actionDefinition.conditionDelta);
                    candidateSave.state.home.improvementProgress = ClampStat(candidateSave.state.home.improvementProgress + actionDefinition.improvementProgressDelta);
                    summary = $"Trained at home · Cost {FormatMoney(cost)} · Health +1 · Fitness XP +{10 + homeBonus * 2} · Home condition −2";
                    break;
                case HomeActionType.Rest:
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health + 3 + homeBonus);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 2 + homeBonus);
                    summary = $"Rested at home · Free · Health +{3 + homeBonus} · Happiness +{2 + homeBonus}";
                    break;
                case HomeActionType.Maintain:
                    candidateSave.state.home.condition = ClampStat(candidateSave.state.home.condition + actionDefinition.conditionDelta);
                    candidateSave.state.home.improvementProgress = ClampStat(candidateSave.state.home.improvementProgress + actionDefinition.improvementProgressDelta);
                    candidateSave.state.home.readingMaterialStock = candidateSave.state.home.readingMaterialCapacity;
                    candidateSave.state.home.trainingEquipmentCondition = ClampStat(
                        candidateSave.state.home.trainingEquipmentCondition + 30);
                    var books = FindHomeInventoryItem(candidateSave.state.home, "starter_books");
                    books.quantity = candidateSave.state.home.readingMaterialStock;
                    books.capacity = candidateSave.state.home.readingMaterialCapacity;
                    books.acquisitionSource = "home_maintenance";
                    var equipment = FindHomeInventoryItem(candidateSave.state.home, "starter_training_kit");
                    equipment.condition = candidateSave.state.home.trainingEquipmentCondition;
                    equipment.acquisitionSource = "home_maintenance";
                    summary = caregiverHandlesMaintenance
                        ? "Asked a caregiver to maintain the home · No child payment · Home condition +20 · Supplies restocked · Equipment repaired"
                        : $"Maintained the home · Cost {FormatMoney(cost)} · Home condition +20 · Supplies restocked · Equipment repaired · Improvement progress +5";
                    break;
                case HomeActionType.HouseholdTime:
                    candidateSave.state.household.happiness = ClampStat(candidateSave.state.household.happiness + 4);
                    candidateSave.state.household.cohesion = ClampStat(candidateSave.state.household.cohesion + 3);
                    foreach (var relationship in candidateSave.state.relationships)
                    {
                        if (IsHouseholdRelationship(relationship))
                            relationship.value = ClampStat(relationship.value + 2);
                    }
                    summary = $"Spent time with the household · Cost {FormatMoney(cost)} · Household happiness +4 · Cohesion +3 · Relationships +2";
                    break;
                default:
                    summary = $"Home action {actionType} is not supported.";
                    return false;
            }

            AddOrRefreshStatus(candidateSave.state.statuses, cooldownId, 1);
            AddLifeFeedEntry(candidateSave, "home", summary);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }
            ActiveSave = candidateSave;
            return true;
        }

        public bool TryUpgradeHome(out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before upgrading the home.";
                return false;
            }
            if (ActiveSave.state.character.age < 18)
            {
                summary = $"Independent home upgrades unlock at age 18; age {ActiveSave.state.character.age} characters do not pay for them.";
                return false;
            }
            var home = ActiveSave.state.home;
            if (home == null || home.upgradeLevel >= 3)
            {
                summary = "This home is already fully upgraded.";
                return false;
            }
            var requiredProgress = GetHomeUpgradeRequiredProgress(home.upgradeLevel);
            var cost = GetHomeUpgradeCost(home.upgradeLevel);
            if (home.improvementProgress < requiredProgress)
            {
                summary = $"Build {requiredProgress} improvement progress before upgrading ({home.improvementProgress}/{requiredProgress}).";
                return false;
            }
            if (ActiveSave.state.finances.cashMinorUnits < cost)
            {
                summary = $"The next home upgrade costs {FormatMoney(cost)}, but only {FormatMoney(ActiveSave.state.finances.cashMinorUnits)} is available.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            candidateSave.state.finances.cashMinorUnits -= cost;
            candidateSave.state.home.upgradeLevel++;
            candidateSave.state.home.improvementProgress -= requiredProgress;
            candidateSave.state.home.condition = 100;
            summary = $"Upgraded the home to level {candidateSave.state.home.upgradeLevel} · Cost {FormatMoney(cost)} · Condition restored · Home activity benefits improved";
            AddLifeFeedEntry(candidateSave, "home", summary);
            if (!saveRepository.TryCommitAutosave(JsonUtility.ToJson(candidateSave), out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }
            ActiveSave = candidateSave;
            return true;
        }

        public static int GetHomeUpgradeRequiredProgress(int currentLevel)
        {
            return currentLevel >= 0 && currentLevel < 3 ? 10 + currentLevel * 10 : 0;
        }

        public static long GetHomeUpgradeCost(int currentLevel)
        {
            switch (currentLevel)
            {
                case 0: return 50000;
                case 1: return 150000;
                case 2: return 300000;
                default: return 0;
            }
        }

        public static long GetHomeActionCost(HomeActionType actionType)
        {
            return HomeContentCatalog.GetAction("starter_home", actionType)?.costMinorUnits ?? 0;
        }

        public static long CalculateHomeConditionExpense(long baseMonthlyExpenses, int condition)
        {
            if (baseMonthlyExpenses <= 0 || condition >= 50) return 0;
            var basisPoints = condition < 25 ? 1000 : 500;
            return (long)Math.Round(baseMonthlyExpenses * (basisPoints / 10000m), MidpointRounding.AwayFromZero);
        }

        private static void ApplyMonthlyHomeCondition(GameState state, long conditionExpense)
        {
            if (state.home == null) return;
            if (conditionExpense > 0)
            {
                state.character.happiness = ClampStat(state.character.happiness - (state.home.condition < 25 ? 2 : 1));
                state.household.cohesion = ClampStat(state.household.cohesion - 1);
                foreach (var relationship in state.relationships)
                {
                    if (IsHouseholdRelationship(relationship))
                        relationship.value = ClampStat(relationship.value - 1);
                }
            }
            state.home.condition = ClampStat(state.home.condition - 1);
        }

        public bool TryDiscoverCompatiblePerson(out string relationshipId, out string summary)
        {
            relationshipId = string.Empty;
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (ActiveSave.state.character.age < 18)
            {
                summary = "Compatible-person discovery is available to adults age 18 and older.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before meeting someone new.";
                return false;
            }
            NormalizeProgressCollections(ActiveSave.state);
            const string cooldownId = "relationship_discovery_used";
            if (ActiveSave.state.statuses.Exists(status => status.statusId == cooldownId))
            {
                summary = "You already met someone new this month. Advance the month to discover again.";
                return false;
            }
            if (ActiveSave.state.relationships.FindAll(relationship =>
                    relationship != null && relationship.origin == "compatible_discovery").Count >= 20)
            {
                summary = "Your discovery list is full. Develop the relationships already in your life.";
                return false;
            }
            if (ActiveSave.state.relationships.Count >= RelationshipState.MaxEntries)
            {
                summary = "Your relationship list is full. Existing people remain available and no records were replaced.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            var step = candidateSave.rng.step++;
            var names = new[] { "Alex Morgan", "Casey Rivera", "Jordan Lee", "Morgan Patel", "Riley Bennett", "Taylor Okafor" };
            var contexts = new[] { "community event", "mutual friends", "local class", "volunteer project", "neighborhood gathering" };
            var name = names[StableIndex(candidateSave.rng.seed, step + 101, names.Length)];
            var context = contexts[StableIndex(candidateSave.rng.seed, step + 211, contexts.Length)];
            relationshipId = $"compatible_{candidateSave.rng.seed}_{step}_{candidateSave.state.relationships.Count}";
            var person = new RelationshipState
            {
                relationshipId = relationshipId,
                identityId = $"identity_{candidateSave.rng.seed}_{step}",
                displayName = name,
                pronouns = step % 3 == 0 ? "she/her" : step % 3 == 1 ? "he/him" : "they/them",
                genderIdentity = step % 3 == 0 ? "woman" : step % 3 == 1 ? "man" : "nonbinary",
                orientation = "compatible_with_player",
                relationshipType = "friend",
                relationshipStage = "introduced",
                origin = "compatible_discovery",
                introductionContext = context,
                introducedAtAge = candidateSave.state.character.age,
                warmth = 50,
                value = 45
            };
            AppendRelationshipHistory(candidateSave, person, "introduced", $"Met through {context}.");
            candidateSave.state.relationships.Add(person);
            AddOrRefreshStatus(candidateSave.state.statuses, cooldownId, 1);
            summary = $"Met {name} through {context} · Friendship 45 · Warmth 50";
            AddLifeFeedEntry(candidateSave, "relationship", summary);
            if (!saveRepository.TryCommitAutosave(JsonUtility.ToJson(candidateSave), out var commitSummary))
            {
                relationshipId = string.Empty;
                summary = commitSummary;
                return false;
            }
            ActiveSave = candidateSave;
            return true;
        }

        public bool TryChooseFamilyPlanning(
            string partnerRelationshipId,
            FamilyPlanningAction action,
            out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (ActiveSave.state.character.age < 18)
            {
                summary = "Family planning is available only to adults.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character) || !string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the current life state before making a family-planning decision.";
                return false;
            }
            var partner = ActiveSave.state.relationships.Find(relationship =>
                relationship != null && relationship.relationshipId == partnerRelationshipId);
            if (partner == null ||
                partner.relationshipType != "partner" && partner.relationshipType != "engaged" &&
                partner.relationshipType != "married")
            {
                summary = "Family planning requires an active adult partner, engagement, or marriage.";
                return false;
            }
            if (action != FamilyPlanningAction.OptOut && partner.value < 70)
            {
                summary = "Discuss family planning after the relationship reaches 70 strength.";
                return false;
            }
            var family = ActiveSave.state.family;
            if (action != FamilyPlanningAction.Discuss && action != FamilyPlanningAction.OptOut &&
                (family.planningPreference != "open" || !family.partnerConsent ||
                 family.planningPartnerId != partnerRelationshipId))
            {
                summary = "Both partners must discuss and agree before beginning this family path.";
                return false;
            }
            if ((action == FamilyPlanningAction.TryForChild || action == FamilyPlanningAction.PursueAdoption) &&
                (!string.IsNullOrEmpty(family.pendingPath) || family.children.Count >= 12))
            {
                summary = !string.IsNullOrEmpty(family.pendingPath)
                    ? "A pregnancy or adoption path is already pending."
                    : "This household has reached the supported child-record capacity.";
                return false;
            }
            const string cooldownId = "family_planning_used";
            if (ActiveSave.state.statuses.Exists(status => status.statusId == cooldownId))
            {
                summary = "You already made a family-planning decision this month.";
                return false;
            }
            var adoptionCost = action == FamilyPlanningAction.PursueAdoption ? 50000L : 0L;
            if (ActiveSave.state.finances.cashMinorUnits < adoptionCost)
            {
                summary = $"Beginning adoption costs {FormatMoney(adoptionCost)} in fees and preparation.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            var candidateFamily = candidateSave.state.family;
            var candidatePartner = candidateSave.state.relationships.Find(relationship =>
                relationship.relationshipId == partnerRelationshipId);
            switch (action)
            {
                case FamilyPlanningAction.Discuss:
                    candidateFamily.planningPreference = "open";
                    candidateFamily.planningPartnerId = partnerRelationshipId;
                    candidateFamily.partnerConsent = candidatePartner.value >= 70 && candidatePartner.warmth >= 60;
                    summary = candidateFamily.partnerConsent
                        ? $"Discussed family planning with {candidatePartner.displayName} · Both partners agreed to keep the path open"
                        : $"Discussed family planning with {candidatePartner.displayName} · No mutual agreement was reached";
                    break;
                case FamilyPlanningAction.TryForChild:
                    candidateFamily.pendingPath = "pregnancy";
                    candidateFamily.monthsUntilResolution = 9;
                    summary = "Both partners agreed to try for a child · Pregnancy path pending for 9 months";
                    break;
                case FamilyPlanningAction.PursueAdoption:
                    candidateSave.state.finances.cashMinorUnits -= adoptionCost;
                    candidateFamily.pendingPath = "adoption";
                    candidateFamily.monthsUntilResolution = 6;
                    summary = $"Both partners began the adoption process · Cost {FormatMoney(adoptionCost)} · Review pending for 6 months";
                    break;
                case FamilyPlanningAction.OptOut:
                    candidateFamily.planningPreference = "not_now";
                    if (string.IsNullOrEmpty(candidateFamily.pendingPath))
                        candidateFamily.planningPartnerId = partnerRelationshipId;
                    candidateFamily.partnerConsent = false;
                    summary = "Chose not to pursue a new family path right now · Existing pending paths are unchanged";
                    break;
                default:
                    summary = "Unsupported family-planning action.";
                    return false;
            }
            AddOrRefreshStatus(candidateSave.state.statuses, cooldownId, 1);
            candidatePartner.warmth = ClampStat(candidatePartner.warmth +
                (action == FamilyPlanningAction.OptOut ? 0 : 1));
            AppendRelationshipHistory(candidateSave, candidatePartner,
                $"family_{action.ToString().ToLowerInvariant()}", summary);
            AddLifeFeedEntry(candidateSave, "family", summary);
            if (!saveRepository.TryCommitAutosave(JsonUtility.ToJson(candidateSave), out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }
            ActiveSave = candidateSave;
            return true;
        }

        private static void AdvancePendingFamilyPath(SaveEnvelope save)
        {
            var family = save.state.family;
            if (family == null || string.IsNullOrEmpty(family.pendingPath) || family.monthsUntilResolution <= 0) return;
            family.monthsUntilResolution--;
            if (family.monthsUntilResolution > 0) return;
            var path = family.pendingPath;
            var childNumber = family.children.Count + 1;
            var childId = $"child_{save.lifeId}_{childNumber}";
            var childName = new[] { "Ari", "Kai", "Maya", "Noah", "Zoe", "Eli" }[
                StableIndex(save.rng.seed, save.rng.step + childNumber * 17, 6)];
            var planningPartner = save.state.relationships.Find(relationship =>
                relationship != null && relationship.relationshipId == family.planningPartnerId);
            var custodyStatus = planningPartner != null && planningPartner.relationshipType == "ex_partner"
                ? "shared"
                : "household";
            family.children.Add(new ChildState
            {
                childId = childId,
                displayName = childName,
                path = path,
                parentRelationshipId = family.planningPartnerId,
                joinedAtParentAge = save.state.character.age,
                birthMonth = save.state.calendar.monthOfYear,
                age = 0,
                custodyStatus = custodyStatus
            });
            var childRelationship = new RelationshipState
            {
                relationshipId = childId,
                identityId = $"identity_{childId}",
                displayName = childName,
                relationshipType = "child",
                relationshipStage = "dependent_child",
                origin = path,
                introductionContext = path == "adoption" ? "joined through adoption" : "born into the household",
                introducedAtAge = save.state.character.age,
                warmth = 70,
                value = 70
            };
            AppendRelationshipHistory(save, childRelationship, path == "adoption" ? "adopted" : "born",
                path == "adoption" ? "Joined the household through adoption." : "Was born into the household.");
            save.state.relationships.Add(childRelationship);
            family.pendingPath = string.Empty;
            family.monthsUntilResolution = 0;
            save.state.household.happiness = ClampStat(save.state.household.happiness + 5);
            save.state.household.cohesion = ClampStat(save.state.household.cohesion + 4);
            AddLifeFeedEntry(save, "family",
                path == "adoption" ? $"Welcomed {childName} through adoption." : $"Welcomed {childName} after childbirth.");
            EnqueueTransition(save, "parenthood", "Welcome to the family",
                path == "adoption" ? $"{childName} joined the household through adoption." : $"{childName} was born into the household.");
        }

        public bool TryPerformRelationshipInteraction(
            string relationshipId,
            RelationshipInteractionType interactionType,
            out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }

            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before interacting.";
                return false;
            }

            NormalizeProgressCollections(ActiveSave.state);
            var relationship = ActiveSave.state.relationships.Find(
                candidate => candidate != null && candidate.relationshipId == relationshipId);
            if (relationship == null)
            {
                summary = $"Relationship {relationshipId} was not found.";
                return false;
            }

            var age = ActiveSave.state.character.age;
            if (!IsRelationshipInteractionAgeAppropriate(interactionType, age))
            {
                summary = $"{ToDisplayName(interactionType.ToString())} is not available at age {age}.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.Reconcile && relationship.relationshipType != "rival")
            {
                summary = "Reconciliation is available after this relationship becomes a rivalry.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.DeepenFriendship &&
                ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                 relationship.value < 65))
            {
                summary = "Deepening friendship requires a friendship with at least 65 strength.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.Compete && relationship.relationshipType == "parent")
            {
                summary = "Competition is available with peers and rivals.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.AskOnDate &&
                ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                 relationship.value < 60))
            {
                summary = "Dating requires an adult friendship with at least 60 relationship strength.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.Commit &&
                (relationship.relationshipType != "dating" || relationship.value < 75))
            {
                summary = "Commitment requires a dating relationship with at least 75 strength.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.DateNight &&
                relationship.relationshipType != "dating" && relationship.relationshipType != "partner" &&
                relationship.relationshipType != "engaged" && relationship.relationshipType != "married")
            {
                summary = "A date night requires an active adult romantic relationship.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.BreakUp &&
                relationship.relationshipType != "dating" && relationship.relationshipType != "partner")
            {
                summary = "There is no active romantic relationship to end.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.Separate &&
                relationship.relationshipType != "partner" && relationship.relationshipType != "engaged")
            {
                summary = "Separation is available to partners and engaged couples; marriage uses the authored crossroads process.";
                return false;
            }
            if (interactionType == RelationshipInteractionType.Recover &&
                relationship.relationshipType != "ex_partner" && relationship.relationshipType != "estranged")
            {
                summary = "Recovery is available after a separation or estrangement.";
                return false;
            }

            var cooldownStatusId = $"relationship_interaction_used_{relationshipId}";
            if (ActiveSave.state.statuses.Exists(status => status.statusId == cooldownStatusId))
            {
                summary = $"You already spent focused time with {relationship.displayName} this month.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            var candidateRelationship = candidateSave.state.relationships.Find(
                candidate => candidate != null && candidate.relationshipId == relationshipId);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");

            int relationshipDelta;
            int happinessDelta;
            int smartsDelta;
            switch (interactionType)
            {
                case RelationshipInteractionType.Talk:
                    relationshipDelta = 2;
                    happinessDelta = 1;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.PlayTogether:
                    relationshipDelta = 4;
                    happinessDelta = 2;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.AskForHelp:
                    relationshipDelta = 3;
                    happinessDelta = 0;
                    smartsDelta = 1;
                    break;
                case RelationshipInteractionType.SpendTime:
                    relationshipDelta = 3;
                    happinessDelta = 2;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.Argue:
                    relationshipDelta = -4;
                    happinessDelta = -1;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.Compete:
                    relationshipDelta = -2;
                    happinessDelta = 0;
                    smartsDelta = 1;
                    break;
                case RelationshipInteractionType.Reconcile:
                    relationshipDelta = 6;
                    happinessDelta = 1;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.DeepenFriendship:
                    relationshipDelta = 6;
                    happinessDelta = 2;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.AskOnDate:
                    relationshipDelta = 5;
                    happinessDelta = 3;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.DateNight:
                    relationshipDelta = 4;
                    happinessDelta = 3;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.Commit:
                    relationshipDelta = 5;
                    happinessDelta = 4;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.BreakUp:
                    relationshipDelta = -20;
                    happinessDelta = -5;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.Separate:
                    relationshipDelta = -15;
                    happinessDelta = -4;
                    smartsDelta = 0;
                    break;
                case RelationshipInteractionType.Recover:
                    relationshipDelta = 5;
                    happinessDelta = 1;
                    smartsDelta = 0;
                    break;
                default:
                    summary = $"Relationship interaction {interactionType} is not supported.";
                    return false;
            }

            candidateRelationship.value = ClampStat(candidateRelationship.value + relationshipDelta);
            candidateRelationship.monthsSinceInteraction = 0;
            UpdateRelationshipStage(candidateRelationship, interactionType == RelationshipInteractionType.Reconcile);
            if (interactionType == RelationshipInteractionType.AskOnDate)
            {
                candidateRelationship.relationshipType = "dating";
                candidateRelationship.relationshipStage = "dating";
            }
            else if (interactionType == RelationshipInteractionType.Commit)
            {
                candidateRelationship.relationshipType = "partner";
                candidateRelationship.relationshipStage = "partnered";
            }
            else if (interactionType == RelationshipInteractionType.BreakUp)
            {
                candidateRelationship.relationshipType = "ex_partner";
                candidateRelationship.relationshipStage = "separated";
            }
            else if (interactionType == RelationshipInteractionType.DeepenFriendship)
            {
                candidateRelationship.relationshipType = "best_friend";
                candidateRelationship.relationshipStage = "close_friendship";
            }
            else if (interactionType == RelationshipInteractionType.DateNight)
                candidateRelationship.relationshipStage = "romantic_growth";
            else if (interactionType == RelationshipInteractionType.Separate)
            {
                candidateRelationship.relationshipType = "ex_partner";
                candidateRelationship.relationshipStage = "separated";
                ApplyChildCustodyAfterSeparation(candidateSave.state, candidateRelationship.relationshipId);
            }
            else if (interactionType == RelationshipInteractionType.Recover)
            {
                candidateRelationship.relationshipType = "friend";
                candidateRelationship.relationshipStage = "recovered_friendship";
            }
            candidateSave.state.character.happiness = ClampStat(
                candidateSave.state.character.happiness + happinessDelta);
            candidateSave.state.character.smarts = ClampStat(
                candidateSave.state.character.smarts + smartsDelta);
            AddOrRefreshStatus(candidateSave.state.statuses, cooldownStatusId, 1);

            var displayName = string.IsNullOrEmpty(candidateRelationship.displayName)
                ? "this person"
                : candidateRelationship.displayName;
            summary = $"{ToDisplayName(interactionType.ToString())} with {displayName} complete" +
                      $" · Relationship {FormatSignedValue(relationshipDelta)}" +
                      (happinessDelta == 0 ? string.Empty : $" · Happiness {FormatSignedValue(happinessDelta)}") +
                      (smartsDelta == 0 ? string.Empty : $" · Smarts {FormatSignedValue(smartsDelta)}");
            candidateRelationship.warmth = ClampStat(candidateRelationship.warmth + relationshipDelta);
            AppendRelationshipHistory(candidateSave, candidateRelationship,
                interactionType.ToString().ToLowerInvariant(), summary);
            AddLifeFeedEntry(candidateSave, "relationship", summary);
            ScheduleNpcRelationshipEvent(candidateSave, candidateRelationship, interactionType);

            EvaluateAchievements(candidateSave);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }

            ActiveSave = candidateSave;
            return true;
        }

        public bool TryPerformParentingAction(
            string childId,
            ParentingAction action,
            out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            var child = ActiveSave.state.family?.children?.Find(record => record.childId == childId);
            var relationship = ActiveSave.state.relationships.Find(candidate =>
                candidate != null && candidate.relationshipId == childId && candidate.relationshipType == "child");
            if (child == null || relationship == null)
            {
                summary = "This child record is not available for parenting.";
                return false;
            }
            if (child.age >= 18)
            {
                summary = "This child is now an adult; use normal relationship actions to stay connected.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character) || !string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the current life state before choosing a parenting action.";
                return false;
            }
            var cooldownId = $"parenting_used_{childId}";
            if (ActiveSave.state.statuses.Exists(status => status.statusId == cooldownId))
            {
                summary = $"You already chose focused parenting time with {child.displayName} this month.";
                return false;
            }
            var cost = action == ParentingAction.SupportNeeds ? 2500L : 0L;
            if (ActiveSave.state.finances.cashMinorUnits < cost)
            {
                summary = $"Supporting these needs costs {FormatMoney(cost)}.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            var candidateChild = candidateSave.state.family.children.Find(record => record.childId == childId);
            var candidateRelationship = candidateSave.state.relationships.Find(candidate => candidate.relationshipId == childId);
            candidateSave.state.finances.cashMinorUnits -= cost;
            int relationshipDelta;
            switch (action)
            {
                case ParentingAction.QualityTime:
                    candidateChild.wellbeing = ClampStat(candidateChild.wellbeing + 5);
                    relationshipDelta = 5;
                    summary = $"Spent quality time with {candidateChild.displayName} · Wellbeing +5 · Relationship +5";
                    break;
                case ParentingAction.SupportNeeds:
                    candidateChild.wellbeing = ClampStat(candidateChild.wellbeing + 7);
                    relationshipDelta = 3;
                    summary = $"Supported {candidateChild.displayName}'s needs · Cost {FormatMoney(cost)} · Wellbeing +7 · Relationship +3";
                    break;
                case ParentingAction.Teach:
                    candidateChild.learning = ClampStat(candidateChild.learning + (candidateChild.age < 6 ? 4 : 7));
                    candidateChild.independence = ClampStat(candidateChild.independence + (candidateChild.age >= 12 ? 3 : 1));
                    relationshipDelta = 2;
                    summary = $"Taught {candidateChild.displayName} · Learning improved · Independence improved · Relationship +2";
                    break;
                case ParentingAction.SetBoundaries:
                    candidateChild.independence = ClampStat(candidateChild.independence + 5);
                    candidateChild.wellbeing = ClampStat(candidateChild.wellbeing + (candidateChild.age >= 6 ? 1 : -1));
                    relationshipDelta = candidateChild.age >= 6 ? 1 : -1;
                    summary = $"Set age-appropriate boundaries with {candidateChild.displayName} · Independence +5 · Relationship {FormatSignedValue(relationshipDelta)}";
                    break;
                default:
                    summary = "Unsupported parenting action.";
                    return false;
            }
            candidateRelationship.value = ClampStat(candidateRelationship.value + relationshipDelta);
            candidateRelationship.warmth = ClampStat(candidateRelationship.warmth + relationshipDelta);
            candidateRelationship.monthsSinceInteraction = 0;
            AddOrRefreshStatus(candidateSave.state.statuses, cooldownId, 1);
            AppendRelationshipHistory(candidateSave, candidateRelationship,
                $"parenting_{action.ToString().ToLowerInvariant()}", summary);
            AddLifeFeedEntry(candidateSave, "family", summary);
            if (!saveRepository.TryCommitAutosave(JsonUtility.ToJson(candidateSave), out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }
            ActiveSave = candidateSave;
            return true;
        }

        private static void AdvanceChildAges(SaveEnvelope save)
        {
            if (save.state.family?.children == null) return;
            foreach (var child in save.state.family.children)
            {
                child.age++;
                if (child.age != 18) continue;
                child.custodyStatus = "independent";
                child.independence = Math.Max(child.independence, 60);
                var relationship = save.state.relationships.Find(candidate =>
                    candidate != null && candidate.relationshipId == child.childId);
                if (relationship == null) continue;
                relationship.relationshipType = "adult_child";
                relationship.relationshipStage = "adult_child";
                AppendRelationshipHistory(save, relationship, "reached_adulthood",
                    $"{child.displayName} reached adulthood.");
                AddLifeFeedEntry(save, "family", $"{child.displayName} reached adulthood and became independent.");
            }
        }

        private static void ApplyChildCustodyAfterSeparation(GameState state, string partnerRelationshipId)
        {
            if (state.family?.children == null) return;
            foreach (var child in state.family.children)
            {
                if (child.age < 18 && child.parentRelationshipId == partnerRelationshipId)
                    child.custodyStatus = "shared";
            }
        }

        private static void AppendRelationshipHistory(
            SaveEnvelope save,
            RelationshipState relationship,
            string type,
            string summary)
        {
            relationship.relationshipHistory ??= new List<RelationshipHistoryState>();
            relationship.relationshipHistory.Add(new RelationshipHistoryState
            {
                historyId = $"{relationship.relationshipId}_{save.revision}_{relationship.relationshipHistory.Count}",
                type = type,
                summary = summary,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (relationship.relationshipHistory.Count > 50)
                relationship.relationshipHistory.RemoveAt(0);
        }

        public static bool IsRelationshipInteractionAgeAppropriate(
            RelationshipInteractionType interactionType,
            int age)
        {
            switch (interactionType)
            {
                case RelationshipInteractionType.PlayTogether: return age <= 12;
                case RelationshipInteractionType.AskForHelp: return age <= 17;
                case RelationshipInteractionType.Argue: return age >= 8;
                case RelationshipInteractionType.Compete: return age >= 8;
                case RelationshipInteractionType.Reconcile: return age >= 10;
                case RelationshipInteractionType.DeepenFriendship: return age >= 10;
                case RelationshipInteractionType.AskOnDate:
                case RelationshipInteractionType.DateNight:
                case RelationshipInteractionType.BreakUp:
                case RelationshipInteractionType.Separate:
                case RelationshipInteractionType.Recover: return age >= 18;
                case RelationshipInteractionType.Commit: return age >= 21;
                case RelationshipInteractionType.Talk:
                case RelationshipInteractionType.SpendTime:
                    return true;
                default:
                    return false;
            }
        }

        private static string FormatSignedValue(int value)
        {
            return $"{(value >= 0 ? "+" : "−")}{Math.Abs(value)}";
        }

        public bool TryPerformEducationAction(EducationActionType actionType, out string summary)
        {
            var request = ActiveSave == null
                ? new ActionRequest(EducationActionService.GetActionId(actionType), string.Empty)
                : new ActionRequest(
                    EducationActionService.GetActionId(actionType),
                    $"{ActiveSave.lifeId}:education:{ActiveSave.state.character.age}:" +
                    $"{ActiveSave.state.calendar.monthOfYear}:{actionType}");
            return TryPerformEducationAction(actionType, request, out summary);
        }

        public bool TryPerformEducationAction(
            EducationActionType actionType,
            ActionRequest request,
            out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidateSave =>
                {
                    var result = educationActionService.Apply(candidateSave, actionType, request);
                    if (result.Succeeded)
                    {
                        EvaluateAchievements(candidateSave);
                    }
                    return result;
                },
                out var committedSave,
                out summary);
            if (succeeded)
            {
                ActiveSave = committedSave;
            }
            return succeeded;
        }

        public IReadOnlyList<ActionDefinition> GetEducationActionDefinitions()
        {
            return EducationActionService.GetDefinitions(ActiveSave?.state);
        }

        public bool TryChooseStudyTrack(StudyTrack track, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => educationActionService.ChooseStudyTrack(candidate, track),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public IReadOnlyList<ActionDefinition> GetStudySessionDefinitions()
        {
            return EducationActionService.GetStudySessionDefinitions(ActiveSave?.state);
        }

        public bool TryStartStudySession(
            StudyDifficulty difficulty, string instanceId, out string summary)
        {
            var definitions = EducationActionService.GetStudySessionDefinitions(ActiveSave?.state);
            var actionId = EducationActionService.GetStudySessionActionId(difficulty);
            var definition = definitions.Find(candidate => candidate.id == actionId);
            var request = new ActionRequest(actionId, instanceId);
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    var result = actionLifecycleService.Start(candidate, definition, request, utcNow());
                    if (!result.Succeeded) return result;
                    candidate.state.statuses ??= new List<StatusState>();
                    candidate.state.statuses.Add(new StatusState
                    {
                        statusId = EducationActionService.MonthlyCooldownStatusId,
                        remainingMonths = 1
                    });
                    return TransactionMutationResult.Success(
                        $"{difficulty} study session started · rewards available when the timer completes.");
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryClaimStudySession(string instanceId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    actionLifecycleService.Reconcile(candidate, utcNow());
                    var action = candidate.state.actionProgress?.Find(item => item?.instanceId == instanceId);
                    const string prefix = "education.study.";
                    if (action == null || string.IsNullOrEmpty(action.actionId) ||
                        !action.actionId.StartsWith(prefix, StringComparison.Ordinal) ||
                        !Enum.TryParse(action.actionId.Substring(prefix.Length), true,
                            out StudyDifficulty difficulty))
                        return TransactionMutationResult.Failure("Study session was not found.");
                    var claim = actionLifecycleService.Claim(candidate, instanceId, utcNow());
                    if (!claim.Succeeded) return claim;
                    candidate.state.statuses?.RemoveAll(status =>
                        status?.statusId == EducationActionService.MonthlyCooldownStatusId);
                    var reward = educationActionService.ApplyStudySession(candidate, difficulty);
                    if (!reward.Succeeded) return reward;
                    action.resultSummary = reward.Summary;
                    EvaluateAchievements(candidate);
                    return reward;
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool IsActionReadyToClaim(ActionProgressState action)
        {
            if (action == null) return false;
            if (action.state == ActionState.Claimable.ToString()) return true;
            return action.state == ActionState.InProgress.ToString() &&
                   DateTimeOffset.TryParse(action.completesAtUtc, out var completesAt) &&
                   utcNow() >= completesAt;
        }

        public bool TryStartAction(
            ActionDefinition definition,
            ActionRequest request,
            out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => actionLifecycleService.Start(candidate, definition, request, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryReconcileActionProgress(out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    var count = actionLifecycleService.Reconcile(candidate, utcNow());
                    return count > 0
                        ? TransactionMutationResult.Success($"{count} action(s) are ready to claim.")
                        : TransactionMutationResult.Failure("No action progress changed.");
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryClaimAction(string instanceId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => actionLifecycleService.Claim(candidate, instanceId, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryPauseAction(string instanceId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => actionLifecycleService.Pause(candidate, instanceId, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryResumeAction(string instanceId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => actionLifecycleService.Resume(candidate, instanceId, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryStartMatch(string themeId, string instanceId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => matchEngine.Start(candidate, themeId, instanceId, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TrySwapMatchTiles(int firstIndex, int secondIndex, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => matchEngine.Swap(candidate, firstIndex, secondIndex, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryReconcileMatch(out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => matchEngine.Reconcile(candidate, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryPauseMatch(out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => matchEngine.Pause(candidate, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryResumeMatch(out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => matchEngine.Resume(candidate, utcNow()),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryClaimMatchReward(out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    var session = candidate.state.matchSession;
                    var claim = matchEngine.Claim(candidate, utcNow());
                    if (!claim.Succeeded) return claim;
                    var rewardAmount = session.rewardAmount *
                                       (session.rewardMultiplierApplied ? session.rewardMultiplier : 1);
                    switch (session.rewardType)
                    {
                        case "qualification_xp":
                            candidate.state.education.qualificationExperience = Math.Min(100000,
                                candidate.state.education.qualificationExperience + rewardAmount);
                            break;
                        case "career_progress":
                            candidate.state.career.careerProgress = ClampStat(
                                candidate.state.career.careerProgress + rewardAmount);
                            break;
                        case "cash_minor_units":
                            candidate.state.finances.cashMinorUnits += rewardAmount;
                            break;
                        default:
                            return TransactionMutationResult.Failure("Match reward type is unsupported.");
                    }
                    AddLifeFeedEntry(candidate, "activity", $"{ToDisplayName(session.theme)} · {claim.Summary}");
                    return claim;
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public static bool TryGetEducationActionRequirement(
            GameState state,
            EducationActionType actionType,
            out string requirement)
        {
            return EducationActionService.TryGetRequirement(state, actionType, out requirement);
        }

        public static int GetSkillExperience(List<SkillState> skills, string skillId)
        {
            return EducationActionService.GetSkillExperience(skills, skillId);
        }

        public static int GetSkillLevel(int experience)
        {
            return EducationActionService.GetSkillLevel(experience);
        }

        public static int GetExperienceForSkillLevel(int level)
        {
            return EducationActionService.GetExperienceForSkillLevel(level);
        }

        public bool TryPerformCareerAction(CareerActionType actionType, out string summary)
        {
            return TryPerformCareerAction(actionType, CareerCatalog.FinanceIndustryId, out summary);
        }

        public bool TryApplyForCareer(string industryId, out string summary)
        {
            return TryPerformCareerAction(CareerActionType.Apply, industryId, out summary);
        }

        private bool TryPerformCareerAction(
            CareerActionType actionType,
            string applicationIndustryId,
            out string summary)
        {
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before taking a career action.";
                return false;
            }

            NormalizeProgressCollections(ActiveSave.state);
            if (!TryGetCareerActionRequirement(
                    ActiveSave.state, actionType, applicationIndustryId, out var requirement))
            {
                summary = requirement;
                return false;
            }
            const string cooldownStatusId = "monthly_career_action_used";
            if (ActiveSave.state.statuses.Exists(status => status.statusId == cooldownStatusId))
            {
                summary = "You already completed a career action this month. Advance the month to act again.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.state.career ??= new CareerState();
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            switch (actionType)
            {
                case CareerActionType.Apply:
                    candidateSave.state.career.pendingIndustryId = applicationIndustryId;
                    AddOrRefreshStatus(candidateSave.state.statuses, "career_interview_ready", 2);
                    CareerCatalog.TryGetIndustry(applicationIndustryId, out var appliedIndustry);
                    summary = $"{appliedIndustry.displayName} application submitted · Interview unlocked next month";
                    break;
                case CareerActionType.Interview:
                    RemoveStatus(candidateSave.state.statuses, "career_interview_ready");
                    var pendingIndustryId = string.IsNullOrEmpty(candidateSave.state.career.pendingIndustryId)
                        ? CareerCatalog.FinanceIndustryId
                        : candidateSave.state.career.pendingIndustryId;
                    CareerCatalog.TryGetIndustry(pendingIndustryId, out var interviewIndustry);
                    candidateSave.state.career.pendingIndustryId = string.Empty;
                    var successChance = CalculateInterviewSuccessChance(candidateSave.state, interviewIndustry);
                    var interviewRoll = StableUnit(candidateSave.rng.seed, candidateSave.rng.step + 7001);
                    candidateSave.rng.step++;
                    if (interviewRoll <= successChance)
                    {
                        var entryRole = interviewIndustry.roles[0];
                        candidateSave.state.career.industryId = interviewIndustry.industryId;
                        candidateSave.state.career.employerId = interviewIndustry.employerId;
                        candidateSave.state.career.roleTitle = entryRole.title;
                        candidateSave.state.career.annualSalaryMinorUnits = entryRole.annualSalaryMinorUnits;
                        candidateSave.state.career.careerProgress = 0;
                        candidateSave.state.career.employmentStatus = "employed";
                        candidateSave.state.career.monthsUnemployed = 0;
                        candidateSave.state.career.performanceWarnings = 0;
                        summary = $"Interview succeeded · Hired as {entryRole.title} · Salary +{FormatMoney(entryRole.annualSalaryMinorUnits)}";
                    }
                    else
                    {
                        candidateSave.state.career.employmentStatus = "unemployed";
                        summary = $"{interviewIndustry.displayName} interview complete · Not selected this time · Retraining is available";
                    }
                    break;
                case CareerActionType.WorkHard:
                    var professionalLevel = GetSkillLevel(
                        GetSkillExperience(candidateSave.state.skills, "professional"));
                    var careerGain = 10 + Math.Min(6, (professionalLevel - 1) * 2);
                    candidateSave.state.career.careerProgress = ClampStat(
                        candidateSave.state.career.careerProgress + careerGain);
                    candidateSave.state.character.happiness = ClampStat(
                        candidateSave.state.character.happiness - 1);
                    summary = $"Worked hard · Career +{careerGain} · Happiness −1" +
                              (professionalLevel > 1 ? $" · Professional Level {professionalLevel} bonus" : string.Empty);
                    break;
                case CareerActionType.AskForPromotion:
                    ApplyPromotion(candidateSave.state.career, out var previousRole, out var newRole, out var salaryDelta);
                    summary = $"Promoted from {previousRole} to {newRole} · Salary +{FormatMoney(salaryDelta)}";
                    break;
                case CareerActionType.Retrain:
                    ApplySkillXp(candidateSave.state.skills, "professional", 15);
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness - 1);
                    summary = "Completed career retraining · Professional XP +15 · Happiness −1";
                    break;
                case CareerActionType.Quit:
                    var formerRole = candidateSave.state.career.roleTitle;
                    candidateSave.state.career = new CareerState
                        { employmentStatus = "unemployed", monthsUnemployed = 0 };
                    summary = $"Quit {formerRole} · Salary −{FormatMoney(ActiveSave.state.career.annualSalaryMinorUnits)}";
                    break;
                case CareerActionType.Retire:
                    var retirementRole = candidateSave.state.career.roleTitle;
                    candidateSave.state.career = new CareerState
                        { roleTitle = "Retired", employmentStatus = "retired" };
                    FinalizeLife(candidateSave.state.character, "retired", "retirement");
                    summary = $"Retired from {retirementRole} · Salary −{FormatMoney(ActiveSave.state.career.annualSalaryMinorUnits)}";
                    EnqueueTransition(candidateSave, "retirement", "Retirement",
                        $"A career as {retirementRole} closes at age {candidateSave.state.character.age}.");
                    break;
                default:
                    summary = $"Career action {actionType} is not supported.";
                    return false;
            }

            AddOrRefreshStatus(candidateSave.state.statuses, cooldownStatusId, 1);
            AddLifeFeedEntry(candidateSave, "career", summary);
            EvaluateAchievements(candidateSave);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                return false;
            }
            ActiveSave = candidateSave;
            return true;
        }

        public bool TryPerformManualWorkTap(out long earningsMinorUnits, out string summary)
        {
            earningsMinorUnits = 0;
            if (ActiveSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (IsLifeEnded(ActiveSave.state.character))
            {
                summary = "This life has ended. Start a new life to continue playing.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.pendingEventId))
            {
                summary = "Resolve the pending life event before working a paid hour.";
                return false;
            }

            var career = ActiveSave.state.career;
            if (career == null || string.IsNullOrEmpty(career.roleTitle) || career.roleTitle == "Retired" ||
                career.annualSalaryMinorUnits <= 0)
            {
                summary = "Get a salaried job before using manual work.";
                return false;
            }

            earningsMinorUnits = CalculateHourlyRateMinorUnits(career.annualSalaryMinorUnits);
            if (earningsMinorUnits <= 0)
            {
                summary = "This job does not currently provide an hourly payout.";
                earningsMinorUnits = 0;
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            candidateSave.state.finances.cashMinorUnits += earningsMinorUnits;
            EvaluateAchievements(candidateSave);
            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var commitSummary))
            {
                summary = commitSummary;
                earningsMinorUnits = 0;
                return false;
            }

            ActiveSave = candidateSave;
            summary = $"Worked 1 hour as {career.roleTitle} · Cash +{FormatPreciseMoney(earningsMinorUnits)}";
            return true;
        }

        public bool TryPerformBusinessAction(BusinessActionType actionType, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => ApplyBusinessAction(candidate, actionType),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryPerformPropertyAction(
            PropertyActionType actionType, string propertyId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => ApplyPropertyAction(candidate, actionType, propertyId),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        private static TransactionMutationResult ApplyPropertyAction(
            SaveEnvelope save, PropertyActionType actionType, string propertyId)
        {
            NormalizeProgressCollections(save.state);
            if (save.state.character.age < 18)
                return TransactionMutationResult.Failure("Rental property unlocks at age 18.");
            if (IsLifeEnded(save.state.character) || !string.IsNullOrEmpty(save.state.pendingEventId))
                return TransactionMutationResult.Failure("Resolve the current life state before managing property.");
            var portfolio = save.state.propertyPortfolio;
            RentalPropertyState property = null;
            if (!string.IsNullOrEmpty(propertyId))
                property = portfolio.properties.Find(item => item != null && item.propertyId == propertyId);
            string summary;
            switch (actionType)
            {
                case PropertyActionType.BuyStarterRental:
                    const long purchasePrice = 7500000;
                    const long downPayment = 1500000;
                    if (portfolio.properties.FindAll(item => item != null && item.status == "owned").Count >=
                        PropertyPortfolioState.MaxProperties)
                        return TransactionMutationResult.Failure("The rental portfolio is full.");
                    if (portfolio.properties.Exists(item => item != null && item.status == "owned" &&
                            item.propertyType == "rental_home"))
                        return TransactionMutationResult.Failure("The starter rental is already owned.");
                    if (save.state.finances.cashMinorUnits < downPayment)
                        return TransactionMutationResult.Failure("The starter rental requires a $15,000 down payment.");
                    save.state.finances.cashMinorUnits -= downPayment;
                    property = new RentalPropertyState
                    {
                        propertyId = $"property_{save.lifeId}_{save.revision}",
                        propertyType = "rental_home",
                        displayName = "Starter Rental Home",
                        status = "owned",
                        purchasePriceMinorUnits = purchasePrice,
                        currentValueMinorUnits = purchasePrice,
                        mortgageBalanceMinorUnits = purchasePrice - downPayment,
                        monthlyMortgagePaymentMinorUnits = 40000,
                        monthlyRentMinorUnits = 75000,
                        monthlyPropertyTaxMinorUnits = 7500,
                        condition = 80,
                        occupied = true
                    };
                    portfolio.properties.Add(property);
                    portfolio.selectedPropertyId = property.propertyId;
                    summary = "Bought Starter Rental Home · Down payment $15,000 · Mortgage $60,000 · Rent $750/month";
                    break;
                case PropertyActionType.Maintain:
                    if (property == null || property.status != "owned")
                        return TransactionMutationResult.Failure("Select an owned property before maintenance.");
                    if (property.condition >= 100)
                        return TransactionMutationResult.Failure("This property is already at full condition.");
                    if (save.state.finances.cashMinorUnits < 50000)
                        return TransactionMutationResult.Failure("Property maintenance costs $500.");
                    if (HasPropertyActionCooldown(save.state, property.propertyId))
                        return TransactionMutationResult.Failure("This property already received focused management this month.");
                    save.state.finances.cashMinorUnits -= 50000;
                    property.condition = ClampStat(property.condition + 25);
                    AddOrRefreshStatus(save.state.statuses, $"property_action_used_{property.propertyId}", 1);
                    summary = $"Maintained {property.displayName} · Cost $500 · Condition {property.condition}/100";
                    break;
                case PropertyActionType.FindTenant:
                    if (property == null || property.status != "owned")
                        return TransactionMutationResult.Failure("Select an owned property before finding a tenant.");
                    if (property.occupied)
                        return TransactionMutationResult.Failure("This property already has a tenant.");
                    if (property.condition < 50)
                        return TransactionMutationResult.Failure("Raise property condition to 50 before finding a tenant.");
                    if (HasPropertyActionCooldown(save.state, property.propertyId))
                        return TransactionMutationResult.Failure("This property already received focused management this month.");
                    property.occupied = true;
                    property.vacancyMonths = 0;
                    AddOrRefreshStatus(save.state.statuses, $"property_action_used_{property.propertyId}", 1);
                    summary = $"Found a tenant for {property.displayName} · Rent {FormatMoney(property.monthlyRentMinorUnits)}/month";
                    break;
                case PropertyActionType.Sell:
                    if (property == null || property.status != "owned")
                        return TransactionMutationResult.Failure("Select an owned property before selling.");
                    var proceeds = property.currentValueMinorUnits - property.mortgageBalanceMinorUnits;
                    if (proceeds < 0 && save.state.finances.cashMinorUnits < -proceeds)
                        return TransactionMutationResult.Failure($"Selling requires {FormatMoney(-proceeds)} to clear the mortgage.");
                    save.state.finances.cashMinorUnits += proceeds;
                    property.mortgageBalanceMinorUnits = 0;
                    property.occupied = false;
                    property.status = "sold";
                    summary = $"Sold {property.displayName} · Net proceeds {FormatSignedMoney(proceeds)}";
                    break;
                default:
                    return TransactionMutationResult.Failure("Unsupported property action.");
            }
            AppendPropertyLedger(save, property, actionType.ToString().ToLowerInvariant(),
                actionType == PropertyActionType.Sell
                    ? property.currentValueMinorUnits
                    : actionType == PropertyActionType.BuyStarterRental ? -1500000 :
                    actionType == PropertyActionType.Maintain ? -50000 : 0);
            AddLifeFeedEntry(save, "property", summary);
            return TransactionMutationResult.Success(summary);
        }

        private static bool HasPropertyActionCooldown(GameState state, string propertyId) =>
            state.statuses.Exists(status => status != null &&
                status.statusId == $"property_action_used_{propertyId}");

        private static TransactionMutationResult ApplyBusinessAction(
            SaveEnvelope save,
            BusinessActionType actionType)
        {
            if (save.state.character.age < 18)
                return TransactionMutationResult.Failure("Business ownership unlocks at age 18.");
            if (IsLifeEnded(save.state.character))
                return TransactionMutationResult.Failure("This life has ended. Start a new life to continue playing.");
            if (!string.IsNullOrEmpty(save.state.pendingEventId))
                return TransactionMutationResult.Failure("Resolve the pending event before operating a business.");
            NormalizeBusinessPortfolio(save.state);
            save.state.business ??= new BusinessState();
            save.state.statuses ??= new List<StatusState>();
            var business = save.state.business;
            string summary;
            switch (actionType)
            {
                case BusinessActionType.Start:
                    const long startupCost = 100000;
                    if (business.status != "none")
                        return TransactionMutationResult.Failure("This life already has a completed or active business path.");
                    if (GetSkillLevel(GetSkillExperience(save.state.skills, "professional")) < 2)
                        return TransactionMutationResult.Failure("Starting Local Services requires Professional Level 2.");
                    if (save.state.finances.cashMinorUnits < startupCost)
                        return TransactionMutationResult.Failure("Starting Local Services costs $1,000.");
                    save.state.finances.cashMinorUnits -= startupCost;
                    save.state.business = business = new BusinessState
                    {
                        businessId = $"business_{save.lifeId}_local_services",
                        businessType = "local_services",
                        displayName = "Local Services Co.",
                        status = "operating",
                        level = 1,
                        locationLevel = 1,
                        actionPoints = 3,
                        maxActionPoints = 3,
                        valuationMinorUnits = startupCost
                    };
                    summary = "Started Local Services Co. · Cost $1,000 · Level 1";
                    break;
                case BusinessActionType.Work:
                    if (business.status != "operating")
                        return TransactionMutationResult.Failure("Start an operating business before working in it.");
                    if (business.actionPoints < 1)
                        return TransactionMutationResult.Failure("No business action points remain this month.");
                    business.actionPoints--;
                    business.operatingProgress = ClampStat(business.operatingProgress + 20 + business.staffCount * 2);
                    summary = $"Worked in Local Services Co. · Operating progress +{20 + business.staffCount * 2} · Action points {business.actionPoints}/{business.maxActionPoints}";
                    break;
                case BusinessActionType.Upgrade:
                    if (business.status != "operating")
                        return TransactionMutationResult.Failure("Start an operating business before upgrading it.");
                    if (business.level >= 3)
                        return TransactionMutationResult.Failure("Local Services Co. has reached its current maximum level.");
                    if (business.actionPoints < 1)
                        return TransactionMutationResult.Failure("No business action points remain this month.");
                    var progressRequired =
                        ProgressionStandards.GetBusinessUpgradeProgressRequired(business.level);
                    var upgradeCost = business.level * 150000L;
                    if (business.operatingProgress < progressRequired)
                        return TransactionMutationResult.Failure($"Upgrade requires {progressRequired} operating progress.");
                    if (save.state.finances.cashMinorUnits < upgradeCost)
                        return TransactionMutationResult.Failure($"This upgrade costs {FormatMoney(upgradeCost)}.");
                    save.state.finances.cashMinorUnits -= upgradeCost;
                    business.operatingProgress -= progressRequired;
                    business.actionPoints--;
                    business.level++;
                    business.valuationMinorUnits = CalculateBusinessValuation(business);
                    summary = $"Upgraded Local Services Co. to Level {business.level} · Cost {FormatMoney(upgradeCost)}";
                    break;
                case BusinessActionType.HireStaff:
                    if (business.status != "operating")
                        return TransactionMutationResult.Failure("Start an operating business before hiring staff.");
                    if (business.staffCount >= business.level * 2)
                        return TransactionMutationResult.Failure("Upgrade the business before hiring more staff.");
                    if (business.actionPoints < 1)
                        return TransactionMutationResult.Failure("No business action points remain this month.");
                    const long hiringCost = 75000;
                    if (save.state.finances.cashMinorUnits < hiringCost)
                        return TransactionMutationResult.Failure("Hiring and onboarding costs $750.");
                    save.state.finances.cashMinorUnits -= hiringCost;
                    business.actionPoints--;
                    business.staffCount++;
                    business.maxActionPoints = Math.Min(9, 3 + business.staffCount);
                    summary = $"Hired a team member · Cost $750 · Staff {business.staffCount} · Action points {business.actionPoints}/{business.maxActionPoints}";
                    break;
                case BusinessActionType.ExpandLocation:
                    if (business.status != "operating")
                        return TransactionMutationResult.Failure("Start an operating business before expanding locations.");
                    if (business.level < 2)
                        return TransactionMutationResult.Failure("Reach business Level 2 before expanding the location.");
                    if (business.locationLevel >= 3)
                        return TransactionMutationResult.Failure("The current location plan is fully expanded.");
                    if (business.actionPoints < 1)
                        return TransactionMutationResult.Failure("No business action points remain this month.");
                    var expansionCost = business.locationLevel * 300000L;
                    if (save.state.finances.cashMinorUnits < expansionCost)
                        return TransactionMutationResult.Failure($"Location expansion costs {FormatMoney(expansionCost)}.");
                    save.state.finances.cashMinorUnits -= expansionCost;
                    business.actionPoints--;
                    business.locationLevel++;
                    business.valuationMinorUnits = CalculateBusinessValuation(business);
                    summary = $"Expanded Local Services Co. to Location Tier {business.locationLevel} · Cost {FormatMoney(expansionCost)}";
                    break;
                case BusinessActionType.Sell:
                    if (business.status != "operating")
                        return TransactionMutationResult.Failure("Only an operating business can be sold.");
                    business.valuationMinorUnits = CalculateBusinessValuation(business);
                    var proceeds = business.valuationMinorUnits;
                    save.state.finances.cashMinorUnits += proceeds;
                    business.status = "sold";
                    summary = $"Sold Local Services Co. · Cash +{FormatMoney(proceeds)}";
                    break;
                default:
                    return TransactionMutationResult.Failure("Unsupported business action.");
            }
            AppendBusinessLedger(save, actionType.ToString().ToLowerInvariant(),
                actionType == BusinessActionType.Sell ? business.valuationMinorUnits : 0);
            AddLifeFeedEntry(save, "business", summary);
            NormalizeBusinessPortfolio(save.state);
            return TransactionMutationResult.Success(summary);
        }

        private static long ProcessMonthlyBusiness(SaveEnvelope save)
        {
            var business = save.state.business;
            if (business == null || business.status != "operating") return 0;
            var profit = CalculateBusinessMonthlyProfit(
                save.rng.seed, save.rng.step, business, out var revenue, out var expenses, out var disruption);
            if (disruption)
            {
                business.riskEventsExperienced++;
                AddLifeFeedEntry(save, "business",
                    "An operational disruption reduced Local Services Co. revenue this month.");
            }
            business.lastRevenueMinorUnits = revenue;
            business.lastExpensesMinorUnits = expenses;
            business.lastProfitMinorUnits = profit;
            business.lifetimeProfitMinorUnits += profit;
            business.monthsOperating++;
            business.consecutiveLossMonths = profit < 0 ? business.consecutiveLossMonths + 1 : 0;
            if (profit >= 0) save.state.finances.cashMinorUnits += profit;
            else
            {
                var loss = -profit;
                var cashUsed = Math.Min(save.state.finances.cashMinorUnits, loss);
                save.state.finances.cashMinorUnits -= cashUsed;
                save.state.finances.debtMinorUnits += loss - cashUsed;
            }
            business.valuationMinorUnits = CalculateBusinessValuation(business);
            business.maxActionPoints = Math.Min(9, 3 + business.staffCount);
            business.actionPoints = business.maxActionPoints;
            AppendBusinessLedger(save, disruption ? "monthly_disruption" : "monthly_result", profit);
            if (business.consecutiveLossMonths >= 3)
            {
                business.status = "failed";
                business.valuationMinorUnits = 0;
                AddLifeFeedEntry(save, "business",
                    "Local Services Co. closed after three consecutive loss months.");
            }
            return profit;
        }

        private static long ProcessMonthlyProperties(
            SaveEnvelope save, out long totalIncome, out long totalExpenses)
        {
            totalIncome = 0;
            totalExpenses = 0;
            var properties = save.state.propertyPortfolio?.properties;
            if (properties == null) return 0;
            long totalProfit = 0;
            for (var index = 0; index < properties.Count; index++)
            {
                var property = properties[index];
                if (property == null || property.status != "owned") continue;
                if (property.occupied && StableUnit(save.rng.seed, save.rng.step + 21001 + index * 31) < 0.08f)
                    property.occupied = false;
                if (!property.occupied && property.vacancyMonths >= 2 && property.condition >= 60 &&
                    StableUnit(save.rng.seed, save.rng.step + 22003 + index * 37) < 0.50f)
                    property.occupied = true;
                property.vacancyMonths = property.occupied ? 0 : property.vacancyMonths + 1;
                var income = property.occupied ? property.monthlyRentMinorUnits : 0;
                var maintenance = 2500L + Math.Max(0, 100 - property.condition) * 200L;
                var mortgagePayment = Math.Min(
                    property.monthlyMortgagePaymentMinorUnits, property.mortgageBalanceMinorUnits);
                var expenses = mortgagePayment +
                               property.monthlyPropertyTaxMinorUnits + maintenance;
                var principal = property.mortgageBalanceMinorUnits <= property.monthlyMortgagePaymentMinorUnits
                    ? property.mortgageBalanceMinorUnits
                    : property.monthlyMortgagePaymentMinorUnits * 3 / 4;
                property.mortgageBalanceMinorUnits -= principal;
                var appreciationBasisPoints = StableIndex(
                    save.rng.seed, save.rng.step + 23003 + index * 41, 112) - 40;
                property.currentValueMinorUnits = Math.Max(property.purchasePriceMinorUnits / 2,
                    property.currentValueMinorUnits * (10000 + appreciationBasisPoints) / 10000);
                property.condition = Math.Max(0, property.condition - 1 -
                    StableIndex(save.rng.seed, save.rng.step + 24007 + index * 43, 3));
                var profit = income - expenses;
                property.lastIncomeMinorUnits = income;
                property.lastExpensesMinorUnits = expenses;
                property.lifetimeNetIncomeMinorUnits += profit;
                property.monthsOwned++;
                if (profit >= 0) save.state.finances.cashMinorUnits += profit;
                else
                {
                    var loss = -profit;
                    var cashUsed = Math.Min(save.state.finances.cashMinorUnits, loss);
                    save.state.finances.cashMinorUnits -= cashUsed;
                    save.state.finances.debtMinorUnits += loss - cashUsed;
                }
                totalIncome += income;
                totalExpenses += expenses;
                totalProfit += profit;
                AppendPropertyLedger(save, property,
                    property.occupied ? "monthly_rent" : "monthly_vacancy", profit);
            }
            return totalProfit;
        }

        public static long CalculateBusinessMonthlyProfit(
            int seed,
            int step,
            BusinessState business,
            out long revenueMinorUnits,
            out long expensesMinorUnits,
            out bool disruption)
        {
            if (business == null || business.status != "operating")
            {
                revenueMinorUnits = 0;
                expensesMinorUnits = 0;
                disruption = false;
                return 0;
            }
            var variancePercent = StableIndex(seed, step + 11003, 51) - 25;
            disruption = StableUnit(seed, step + 13007) < 0.10f;
            var baseRevenue = 60000L * business.level * Math.Max(1, business.locationLevel) +
                              business.operatingProgress * 1000L + business.staffCount * 30000L;
            revenueMinorUnits = Math.Max(0, baseRevenue * (100 + variancePercent) / 100);
            if (disruption) revenueMinorUnits = revenueMinorUnits * 60 / 100;
            expensesMinorUnits = 65000L * business.level * Math.Max(1, business.locationLevel) +
                                 business.staffCount * 40000L;
            return revenueMinorUnits - expensesMinorUnits;
        }

        public static long CalculateBusinessValuation(BusinessState business)
        {
            if (business == null || business.status == "failed") return 0;
            return Math.Max(100000, business.level * 150000L + business.locationLevel * 100000L +
                business.staffCount * 50000L + Math.Max(0, business.lifetimeProfitMinorUnits * 2));
        }

        public static long CalculateNetWorth(GameState state)
        {
            if (state?.finances == null) return 0;
            long value = state.finances.cashMinorUnits + state.finances.savingsMinorUnits +
                         state.finances.indexFundMinorUnits - state.finances.debtMinorUnits;
            var countedBusinesses = new HashSet<string>();
            if (state.businessPortfolio?.businesses != null)
            {
                foreach (var business in state.businessPortfolio.businesses)
                    if (business != null && business.status != "failed" && business.status != "sold" &&
                        countedBusinesses.Add(business.businessId ?? string.Empty))
                        value += business.valuationMinorUnits;
            }
            if (state.business != null && state.business.status != "failed" && state.business.status != "sold" &&
                !countedBusinesses.Contains(state.business.businessId ?? string.Empty))
                value += state.business.valuationMinorUnits;
            if (state.propertyPortfolio?.properties != null)
                foreach (var property in state.propertyPortfolio.properties)
                    if (property != null && property.status == "owned")
                        value += property.currentValueMinorUnits - property.mortgageBalanceMinorUnits;
            return value;
        }

        private static void AppendBusinessLedger(SaveEnvelope save, string type, long amount)
        {
            var business = save.state.business;
            business.ledger ??= new List<BusinessLedgerEntry>();
            business.ledger.Add(new BusinessLedgerEntry
            {
                entryId = $"{save.lifeId}_{save.revision}_{type}_{business.ledger.Count}",
                type = type,
                amountMinorUnits = amount,
                valuationMinorUnits = business.valuationMinorUnits,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (business.ledger.Count > 60) business.ledger.RemoveAt(0);
        }

        private static void AppendPropertyLedger(
            SaveEnvelope save, RentalPropertyState property, string type, long amount)
        {
            var portfolio = save.state.propertyPortfolio;
            portfolio.ledger ??= new List<PropertyLedgerEntry>();
            portfolio.ledger.Add(new PropertyLedgerEntry
            {
                entryId = $"{save.lifeId}_{save.revision}_{type}_{portfolio.ledger.Count}",
                propertyId = property.propertyId,
                type = type,
                amountMinorUnits = amount,
                propertyValueMinorUnits = property.currentValueMinorUnits,
                mortgageBalanceMinorUnits = property.mortgageBalanceMinorUnits,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (portfolio.ledger.Count > PropertyPortfolioState.MaxLedgerEntries)
                portfolio.ledger.RemoveAt(0);
        }

        public static long CalculateHourlyRateMinorUnits(long annualSalaryMinorUnits)
        {
            if (annualSalaryMinorUnits <= 0) return 0;
            return (long)Math.Round(annualSalaryMinorUnits / 2080m, MidpointRounding.AwayFromZero);
        }

        public static bool TryGetCareerActionRequirement(
            GameState state,
            CareerActionType actionType,
            out string requirement)
        {
            return TryGetCareerActionRequirement(
                state, actionType, CareerCatalog.FinanceIndustryId, out requirement);
        }

        public static bool TryGetCareerActionRequirement(
            GameState state,
            CareerActionType actionType,
            string applicationIndustryId,
            out string requirement)
        {
            if (state?.character == null || state.character.age < 18)
            {
                requirement = "Career actions unlock at age 18.";
                return false;
            }
            var career = state.career ?? new CareerState();
            var retired = career.roleTitle == "Retired";
            var employed = !string.IsNullOrEmpty(career.roleTitle) && !retired;
            var interviewReady = state.statuses?.Exists(status => status.statusId == "career_interview_ready") == true;
            switch (actionType)
            {
                case CareerActionType.Apply:
                    if (retired)
                    {
                        requirement = "This career ended at retirement.";
                        return false;
                    }
                    if (employed)
                    {
                        requirement = "Quit your current role before applying.";
                        return false;
                    }
                    if (interviewReady)
                    {
                        requirement = "Your interview is already ready.";
                        return false;
                    }
                    if (!CareerCatalog.TryGetIndustry(applicationIndustryId, out _))
                    {
                        requirement = $"Career industry {applicationIndustryId} is not available.";
                        return false;
                    }
                    if (applicationIndustryId == CareerCatalog.FinanceIndustryId &&
                        !TryGetEntryCareerQualificationRequirement(state, out requirement))
                        return false;
                    if (!CareerCatalog.TryGetApplicationRequirement(state, applicationIndustryId, out requirement))
                        return false;
                    return true;
                case CareerActionType.Interview:
                    requirement = retired ? "This career ended at retirement." :
                        interviewReady ? string.Empty : "Submit an application first.";
                    return !retired && !employed && interviewReady;
                case CareerActionType.WorkHard:
                    requirement = employed ? string.Empty : "Get hired before working toward promotion.";
                    return employed;
                case CareerActionType.AskForPromotion:
                    if (!employed)
                    {
                        requirement = "Get hired before requesting promotion.";
                        return false;
                    }
                    if (!TryGetNextCareerStep(career, out _, out _, out var progressRequired))
                    {
                        requirement = "You have reached the top of this career ladder.";
                        return false;
                    }
                    requirement = career.careerProgress >= progressRequired
                        ? string.Empty
                        : $"Requires {progressRequired} career progress.";
                    return career.careerProgress >= progressRequired;
                case CareerActionType.Retrain:
                    requirement = employed ? "Retraining is available while unemployed." : string.Empty;
                    return !employed && !retired;
                case CareerActionType.Quit:
                    requirement = employed ? string.Empty : "You do not have a role to quit.";
                    return employed;
                case CareerActionType.Retire:
                    requirement = state.character.age < 65 ? "Retirement unlocks at age 65." :
                        !employed ? "You need an active career before retiring." : string.Empty;
                    return state.character.age >= 65 && employed;
                default:
                    requirement = "Unsupported career action.";
                    return false;
            }
        }

        public static bool TryGetNextCareerStep(
            string roleTitle,
            out string nextRole,
            out long nextSalaryMinorUnits,
            out int progressRequired)
        {
            switch (roleTitle)
            {
                case "Junior Associate": nextRole = "Associate"; nextSalaryMinorUnits = 5500000; progressRequired = ProgressionStandards.FirstCareerPromotionProgress; return true;
                case "Associate": nextRole = "Senior Associate"; nextSalaryMinorUnits = 7500000; progressRequired = ProgressionStandards.SecondCareerPromotionProgress; return true;
                case "Senior Associate": nextRole = "Manager"; nextSalaryMinorUnits = 10000000; progressRequired = ProgressionStandards.ThirdCareerPromotionProgress; return true;
                default: nextRole = null; nextSalaryMinorUnits = 0; progressRequired = 0; return false;
            }
        }

        private static bool TryGetNextCareerStep(
            CareerState career,
            out string nextRole,
            out long nextSalaryMinorUnits,
            out int progressRequired)
        {
            var industryId = string.IsNullOrEmpty(career?.industryId)
                ? CareerCatalog.FinanceIndustryId
                : career.industryId;
            if (CareerCatalog.TryGetNextRole(industryId, career?.roleTitle, out var role, out progressRequired))
            {
                nextRole = role.title;
                nextSalaryMinorUnits = role.annualSalaryMinorUnits;
                return true;
            }
            nextRole = null;
            nextSalaryMinorUnits = 0;
            progressRequired = 0;
            return false;
        }

        private static bool TryGetEntryCareerQualificationRequirement(
            GameState state,
            out string requirement)
        {
            var education = state.education;
            if (education == null || string.IsNullOrEmpty(education.studyTrack))
            {
                requirement = string.Empty;
                return true;
            }
            var requiredExperience = education.studyTrack == "general"
                ? ProgressionStandards.DiplomaQualificationExperience
                : ProgressionStandards.CertificateQualificationExperience;
            if (education.qualificationExperience >= requiredExperience)
            {
                requirement = string.Empty;
                return true;
            }
            var tier = requiredExperience == ProgressionStandards.DiplomaQualificationExperience
                ? "Diploma"
                : "Certificate";
            requirement = $"Requires a {tier} qualification ({requiredExperience} XP) for the selected track.";
            return false;
        }

        public static float CalculateInterviewSuccessChance(
            GameState state,
            CareerIndustryDefinition industry)
        {
            if (state?.character == null || industry == null) return 0f;
            var professionalLevel = GetSkillLevel(GetSkillExperience(state.skills, "professional"));
            var qualificationSurplus = Math.Max(0,
                (state.education?.qualificationExperience ?? 0) - industry.requiredQualificationExperience);
            return Math.Min(0.90f, 0.55f + state.character.smarts / 500f +
                professionalLevel * 0.05f + Math.Min(0.10f, qualificationSurplus / 1000f));
        }

        private static void EvaluateAnnualCareerStability(SaveEnvelope save)
        {
            var career = save?.state?.career;
            if (career == null || string.IsNullOrEmpty(career.roleTitle) || career.roleTitle == "Retired") return;
            if (career.careerProgress >= 10)
            {
                career.performanceWarnings = Math.Max(0, career.performanceWarnings - 1);
                return;
            }
            if (StableUnit(save.rng.seed, save.rng.step + 9001) >= 0.35f) return;
            career.performanceWarnings++;
            if (career.performanceWarnings < 2)
            {
                AddLifeFeedEntry(save, "career",
                    $"Received a performance warning as {career.roleTitle} · Build career progress before the next review");
                return;
            }

            var formerRole = career.roleTitle;
            save.state.career = new CareerState
                { employmentStatus = "unemployed", monthsUnemployed = 0 };
            save.state.character.happiness = ClampStat(save.state.character.happiness - 5);
            AddLifeFeedEntry(save, "career",
                $"Was dismissed from {formerRole} after repeated performance warnings · Happiness −5 · Retraining available");
        }

        private static void ApplyPromotion(
            CareerState career,
            out string previousRole,
            out string newRole,
            out long salaryDelta)
        {
            previousRole = career.roleTitle;
            TryGetNextCareerStep(career, out newRole, out var nextSalary, out _);
            salaryDelta = nextSalary - career.annualSalaryMinorUnits;
            career.roleTitle = newRole;
            career.annualSalaryMinorUnits = nextSalary;
            career.careerProgress = 0;
        }

        private static void RemoveStatus(List<StatusState> statuses, string statusId)
        {
            statuses.RemoveAll(status => status != null && status.statusId == statusId);
        }

        private static bool IsLifeEnded(CharacterState character)
        {
            return character != null && character.lifeStatus != "active";
        }

        private static void FinalizeLife(CharacterState character, string status, string reason)
        {
            character.lifeStatus = status;
            character.endingReason = reason;
            character.endedAtAge = character.age;
        }

        public static bool IsActivityAgeAppropriate(ActivityType activityType, int age)
        {
            switch (activityType)
            {
                case ActivityType.Play: return age <= 12;
                case ActivityType.Study: return age >= 5;
                case ActivityType.Workout: return age >= 13;
                case ActivityType.Rest: return true;
                case ActivityType.FamilyTime: return true;
                case ActivityType.FamilyMovie: return true;
                case ActivityType.FamilyMovieCredit: return age >= 18;
                case ActivityType.Explore: return age < 18;
                case ActivityType.AttendSchool: return age >= 6 && age < 18;
                case ActivityType.JoinClub: return age >= 10 && age < 18;
                case ActivityType.WorkShift:
                case ActivityType.Overtime:
                case ActivityType.Training: return age >= 18 && age < 65;
                case ActivityType.Socialize: return age >= 10;
                case ActivityType.Hobby: return age >= 65;
                case ActivityType.Checkup: return age >= 50;
                default: return false;
            }
        }

        private static bool IsHouseholdRelationship(RelationshipState relationship)
        {
            if (relationship == null) return false;
            return relationship.relationshipType == "parent" || relationship.relationshipType == "child" ||
                   relationship.relationshipType == "partner" || relationship.relationshipType == "engaged" ||
                   relationship.relationshipType == "married";
        }

        public static long CalculateFamilyMovieCost(GameState state)
        {
            const long fixedTicketPriceMinorUnits = 1500;
            var attendees = 1 + (state?.relationships?.FindAll(IsHouseholdRelationship).Count ?? 0);
            return fixedTicketPriceMinorUnits * attendees;
        }

        public static long CalculateHouseholdCreditLimit(GameState state)
        {
            if (state?.finances == null || state.character == null || state.character.age < 18) return 0;
            var annualIncome = Math.Max(0L, state.career?.annualSalaryMinorUnits ?? 0L) +
                               Math.Max(0L, state.finances.spouseAnnualIncomeMinorUnits);
            if (annualIncome <= 0) return 0;
            var careerProgress = Math.Max(0, Math.Min(100, state.career?.careerProgress ?? 0));
            var cohesion = Math.Max(0, Math.Min(100, state.household?.cohesion ?? 50));
            return Math.Min(5000000L,
                Math.Max(50000L, annualIncome / 10L + careerProgress * 1000L + cohesion * 500L));
        }

        public static int CalculateHouseholdCreditAprBasisPoints(GameState state)
        {
            if (state?.finances == null) return 2999;
            var annualIncome = Math.Max(0L, state.career?.annualSalaryMinorUnits ?? 0L) +
                               Math.Max(0L, state.finances.spouseAnnualIncomeMinorUnits);
            if (annualIncome <= 0) return 2999;
            var careerProgress = Math.Max(0, Math.Min(100, state.career?.careerProgress ?? 0));
            var cohesion = Math.Max(0, Math.Min(100, state.household?.cohesion ?? 50));
            var incomeDiscount = (int)Math.Min(800L, annualIncome / 100000L * 20L);
            var debtRatioBasisPoints = (int)Math.Min(10000L,
                state.finances.debtMinorUnits * 10000L / Math.Max(1L, annualIncome));
            var debtPenalty = Math.Min(1000, debtRatioBasisPoints / 10);
            return Math.Max(800, Math.Min(2999,
                2800 - incomeDiscount - careerProgress * 8 - cohesion * 5 + debtPenalty));
        }

        private static long AccrueHouseholdCreditInterest(FinancesState finances)
        {
            if (finances == null || finances.householdCreditBalanceMinorUnits <= 0 ||
                finances.householdCreditAprBasisPoints <= 0) return 0;
            var interest = Math.Max(1L, (long)Math.Round(
                finances.householdCreditBalanceMinorUnits *
                (finances.householdCreditAprBasisPoints / 120000m),
                MidpointRounding.AwayFromZero));
            finances.householdCreditBalanceMinorUnits += interest;
            finances.debtMinorUnits += interest;
            return interest;
        }

        public static long CalculateProjectedAnnualSavingsInterest(FinancesState finances)
        {
            if (finances == null || finances.savingsMinorUnits <= 0 || finances.savingsApyBasisPoints <= 0)
                return 0;
            return (long)Math.Round(
                finances.savingsMinorUnits * (finances.savingsApyBasisPoints / 10000m),
                MidpointRounding.AwayFromZero);
        }

        public static int CalculateAnnualIndexReturnBasisPoints(int seed, int age)
        {
            unchecked
            {
                var value = (uint)(seed * 1103515245 + age * 12345 + 0x9E3779B9);
                value ^= value >> 16;
                return -1200 + (int)(value % 3001u);
            }
        }

        private static void ApplyAnnualIndexFundReturn(SaveEnvelope save)
        {
            var finances = save.state.finances;
            if (finances.indexFundMinorUnits <= 0) return;
            var basisPoints = CalculateAnnualIndexReturnBasisPoints(save.rng.seed, save.state.character.age);
            var change = (long)Math.Round(
                finances.indexFundMinorUnits * (basisPoints / 10000m),
                MidpointRounding.AwayFromZero);
            if (change == 0) return;
            if (change < 0) change = -Math.Min(finances.indexFundMinorUnits, -change);
            finances.indexFundMinorUnits += change;
            save.state.moneyTransactions ??= new List<MoneyTransactionState>();
            save.state.moneyTransactions.Add(new MoneyTransactionState
            {
                transactionId = $"money_{save.revision}_index_{(change > 0 ? "gain" : "loss")}",
                type = change > 0 ? "index_gain" : "index_loss",
                amountMinorUnits = Math.Abs(change),
                cashBalanceMinorUnits = finances.cashMinorUnits,
                savingsBalanceMinorUnits = finances.savingsMinorUnits,
                age = save.state.character.age,
                monthOfYear = 12,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (save.state.moneyTransactions.Count > 100)
                save.state.moneyTransactions.RemoveAt(0);
            AddLifeFeedEntry(save, "money",
                $"Annual index fund change {FormatSignedMoney(change)} ({basisPoints / 100m:+0.00;-0.00;0.00}%) · " +
                $"Balance {FormatMoney(finances.indexFundMinorUnits)}.");
        }

        private static long AccrueSavingsInterest(SaveEnvelope save, int paidMonth)
        {
            var finances = save.state.finances;
            if (finances.savingsMinorUnits <= 0 || finances.savingsApyBasisPoints <= 0) return 0;
            var interest = (long)Math.Round(
                finances.savingsMinorUnits * (finances.savingsApyBasisPoints / 120000m),
                MidpointRounding.AwayFromZero);
            if (interest <= 0) return 0;
            finances.savingsMinorUnits += interest;
            save.state.moneyTransactions ??= new List<MoneyTransactionState>();
            save.state.moneyTransactions.Add(new MoneyTransactionState
            {
                transactionId = $"money_{save.revision}_savings_interest",
                type = "savings_interest",
                amountMinorUnits = interest,
                cashBalanceMinorUnits = finances.cashMinorUnits,
                savingsBalanceMinorUnits = finances.savingsMinorUnits,
                age = save.state.character.age,
                monthOfYear = paidMonth,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (save.state.moneyTransactions.Count > 100)
                save.state.moneyTransactions.RemoveAt(0);
            return interest;
        }

        private static void ApplyFamilyMovieBenefits(GameState state)
        {
            state.character.happiness = ClampStat(state.character.happiness + 3);
            state.household.happiness = ClampStat(state.household.happiness + 4);
            state.household.cohesion = ClampStat(state.household.cohesion + 3);
            foreach (var relationship in state.relationships)
            {
                if (IsHouseholdRelationship(relationship))
                    relationship.value = ClampStat(relationship.value + 3);
            }
        }

        private static string BuildFamilyMovieSummary(long cost, bool usedCredit)
        {
            return $"Family movie night · Happiness +3 · Household happiness +4 · Cohesion +3 · " +
                   $"Family relationships +3 · {(usedCredit ? "Charged to credit" : "Cost")} {FormatMoney(cost)}";
        }

        public static bool TryGetActivityRequirement(
            GameState state,
            ActivityType activityType,
            out string requirement)
        {
            if (state?.character == null || !IsActivityAgeAppropriate(activityType, state.character.age))
            {
                requirement = $"{ToDisplayName(activityType.ToString())} is not available at age {state?.character?.age ?? 0}.";
                return false;
            }
            var employed = !string.IsNullOrEmpty(state.career?.roleTitle) && state.career.roleTitle != "Retired";
            if (activityType == ActivityType.FamilyMovie)
            {
                var cost = CalculateFamilyMovieCost(state);
                if (state.finances == null || state.finances.cashMinorUnits < cost)
                {
                    requirement = $"Family movie night costs {FormatMoney(cost)}. Add cash or use the credit option.";
                    return false;
                }
            }
            if (activityType == ActivityType.FamilyMovieCredit)
            {
                var cost = CalculateFamilyMovieCost(state);
                var availableCredit = state.finances == null
                    ? 0
                    : Math.Max(0, CalculateHouseholdCreditLimit(state) - state.finances.debtMinorUnits);
                if (availableCredit < cost)
                {
                    requirement = $"Available household credit is {FormatMoney(availableCredit)}; {FormatMoney(cost)} is required.";
                    return false;
                }
            }
            if ((activityType == ActivityType.WorkShift || activityType == ActivityType.Overtime) && !employed)
            {
                requirement = "Get an active job first.";
                return false;
            }
            if ((activityType == ActivityType.AttendSchool || activityType == ActivityType.JoinClub) &&
                (state.education?.stage == "left_school" || !string.IsNullOrEmpty(state.education?.awaitingDecisionId)))
            {
                requirement = state.education?.stage == "left_school"
                    ? "This life left school."
                    : "Choose a school path first.";
                return false;
            }
            requirement = string.Empty;
            return true;
        }

        private long ApplyResolution(SaveEnvelope save, ChoiceResolution resolution)
        {
            NormalizeProgressCollections(save.state);
            save.revision++;
            save.rng.step++;
            save.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            save.state.pendingEventId = null;

            long authoredCashImpact = 0;
            foreach (var effect in resolution.outcome.effects)
            {
                if (effect?.type == EffectType.CashDelta)
                    authoredCashImpact += (long)Math.Round(effectValueResolver.Resolve(effect));
                else
                    ApplyEffect(save, effect);
            }

            ApplyIdentityChoice(save.state.character, resolution.eventId, resolution.choiceId);
            var spouseIncomeBefore = save.state.finances.spouseAnnualIncomeMinorUnits;
            var cashBeforeRelationshipTransition = save.state.finances.cashMinorUnits;
            var debtBeforeRelationshipTransition = save.state.finances.debtMinorUnits;
            ApplyRomanceChoice(save, resolution.eventId, resolution.choiceId);
            if (resolution.eventId == RepresentativeEvents.WeddingId && resolution.choiceId == "get_married")
                EnqueueTransition(save, "marriage", "Just married",
                    "The partnership became a marriage and household finances were combined.");
            if (save.state.finances.spouseAnnualIncomeMinorUnits > spouseIncomeBefore)
            {
                AddLifeFeedEntry(save, "money",
                    $"Combined household finances · Spouse income {FormatMoney(save.state.finances.spouseAnnualIncomeMinorUnits)}/year" +
                    $" · Savings {FormatSignedMoney(save.state.finances.cashMinorUnits - cashBeforeRelationshipTransition)}" +
                    $" · Debt {FormatSignedMoney(save.state.finances.debtMinorUnits - debtBeforeRelationshipTransition)}.");
            }
            var financialImpact = authoredCashImpact + CalculateEventFinancialImpact(
                resolution.eventId, resolution.choiceId, save.state);

            save.state.eventHistory.Add(new EventHistoryEntry
            {
                eventId = resolution.eventId,
                choiceId = resolution.choiceId,
                outcomeId = resolution.outcome.id,
                age = save.state.character.age,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            HistoryRetention.Apply(save.state);
            AddLifeFeedEntry(save, "event", BuildOutcomeFeedText(resolution.outcome));
            if (financialImpact != 0)
            {
                AddLifeFeedEntry(save, "money",
                    $"{ToDisplayName(resolution.eventId)} · Financial impact {FormatSignedMoney(financialImpact)}.");
            }

            foreach (var followUp in resolution.outcome.followUps)
            {
                AddScheduledEvent(save.state, new ScheduledEventRecord
                {
                    eventId = followUp.eventId,
                    earliestTriggerAge = save.state.character.age + followUp.minYearsFromNow,
                    latestTriggerAge = save.state.character.age + followUp.maxYearsFromNow,
                    priority = 50,
                    cooldownMonths = 12,
                    chance = followUp.probability,
                    sourceEventId = resolution.eventId,
                    cancellationRule = followUp.cancellationRule
                });
            }
            return financialImpact;
        }

        private static bool TryApplyFinancialImpact(
            GameState state,
            long impact,
            PaymentMethod paymentMethod,
            out string failure)
        {
            failure = string.Empty;
            if (impact == 0) return true;
            if (impact > 0)
            {
                state.finances.cashMinorUnits += impact;
                return true;
            }

            var cost = -impact;
            if (paymentMethod == PaymentMethod.Cash)
            {
                if (state.finances.cashMinorUnits < cost)
                {
                    failure = $"This choice costs {FormatMoney(cost)}. You need more cash or must choose the credit option.";
                    return false;
                }
                state.finances.cashMinorUnits -= cost;
                return true;
            }

            var availableCredit = Math.Max(0, CalculateHouseholdCreditLimit(state) - state.finances.debtMinorUnits);
            if (availableCredit < cost)
            {
                failure = $"This choice costs {FormatMoney(cost)}, but only {FormatMoney(availableCredit)} of household credit is available.";
                return false;
            }
            AddHouseholdCreditPurchase(state, cost);
            return true;
        }

        private static void AddHouseholdCreditPurchase(GameState state, long cost)
        {
            var apr = CalculateHouseholdCreditAprBasisPoints(state);
            var existingBalance = state.finances.householdCreditBalanceMinorUnits;
            state.finances.debtMinorUnits += cost;
            state.finances.householdCreditBalanceMinorUnits += cost;
            state.finances.householdCreditAprBasisPoints = existingBalance <= 0
                ? apr
                : (int)Math.Round(
                    (state.finances.householdCreditAprBasisPoints * (decimal)existingBalance + apr * (decimal)cost) /
                    (existingBalance + cost), MidpointRounding.AwayFromZero);
        }

        public static long CalculateEventFinancialImpact(
            string eventId,
            string choiceId,
            GameState state)
        {
            if (state?.finances == null) return 0;
            var salary = Math.Max(0L, state.career?.annualSalaryMinorUnits ?? 0L);
            var progress = Math.Max(0, Math.Min(100, state.career?.careerProgress ?? 0));
            var relationship = state.relationships?.Find(candidate =>
                candidate != null && candidate.relationshipId == "school_peer_primary");
            var relationshipValue = relationship?.value ?? 0;

            if (eventId == RepresentativeEvents.PromInvitationId)
            {
                if (choiceId == "attend_prom_together")
                    return -(7500L + salary / 1000L + progress * 50L);
                if (choiceId == "attend_prom_as_friends")
                    return -(4000L + salary / 2000L);
            }
            else if (eventId == RepresentativeEvents.ProposalId)
            {
                if (choiceId == "propose_marriage")
                    return -Math.Max(20000L, Math.Min(500000L, salary / 100L));
            }
            else if (eventId == RepresentativeEvents.WeddingId)
            {
                if (choiceId == "get_married")
                {
                    var gifts = relationshipValue * 5000L + progress * 1000L;
                    var celebrationCost = Math.Max(100000L, salary / 12L + state.finances.cashMinorUnits / 50L);
                    return gifts - celebrationCost;
                }
                if (choiceId == "postpone_wedding")
                    return -Math.Max(5000L, salary / 1000L);
                if (choiceId == "call_off_wedding")
                    return -Math.Max(10000L, salary / 200L);
            }
            else if (eventId == RepresentativeEvents.MarriageCrossroadsId)
            {
                if (choiceId == "seek_counseling")
                    return -Math.Max(10000L, salary / 500L);
                if (choiceId == "divorce")
                    return -(state.finances.cashMinorUnits / 4L + Math.Max(50000L, salary / 50L));
            }
            return 0;
        }

        public static long CalculateChoicePotentialCost(
            Event evt,
            Choice choice,
            GameState state,
            IEffectValueResolver effectValueResolver = null)
        {
            if (evt == null || choice == null) return 0;
            effectValueResolver ??= new EffectValueResolver();
            var dynamicImpact = CalculateEventFinancialImpact(evt.id, choice.id, state);
            var largestCost = dynamicImpact < 0 ? -dynamicImpact : 0;
            foreach (var outcome in choice.outcomes)
            {
                if (outcome?.effects == null) continue;
                long authoredCashImpact = 0;
                foreach (var effect in outcome.effects)
                {
                    if (effect?.type == EffectType.CashDelta)
                        authoredCashImpact += (long)Math.Round(effectValueResolver.Resolve(effect));
                }
                var combinedImpact = authoredCashImpact + dynamicImpact;
                if (combinedImpact < 0) largestCost = Math.Max(largestCost, -combinedImpact);
            }
            return largestCost;
        }

        public static string BuildOutcomeFeedText(Outcome outcome)
        {
            if (outcome == null) return "An event concluded.";
            var feedSummary = outcome.feedEntryKey?.Trim();
            var result = outcome.resultTextKey?.Trim();
            if (string.IsNullOrEmpty(feedSummary)) return string.IsNullOrEmpty(result) ? "An event concluded." : result;
            if (string.IsNullOrEmpty(result) || string.Equals(feedSummary, result, StringComparison.Ordinal))
            {
                return feedSummary;
            }
            return $"{feedSummary} {result}";
        }

        private static void ApplyIdentityChoice(
            CharacterState character,
            string eventId,
            string choiceId)
        {
            if (character == null) return;
            if (eventId == RepresentativeEvents.ComingOfAgeGenderId)
            {
                character.genderIdentity = choiceId switch
                {
                    "identify_woman" => "woman",
                    "identify_man" => "man",
                    "identify_nonbinary" => "nonbinary",
                    "still_questioning_gender" => "questioning",
                    _ => character.genderIdentity
                };
            }
            else if (eventId == RepresentativeEvents.ComingOfAgeOrientationId)
            {
                character.sexualOrientation = choiceId switch
                {
                    "orientation_straight" => "straight",
                    "orientation_gay_lesbian" => "gay_or_lesbian",
                    "orientation_bisexual" => "bisexual",
                    "orientation_pansexual" => "pansexual",
                    "orientation_asexual" => "asexual",
                    "still_questioning_orientation" => "questioning",
                    _ => character.sexualOrientation
                };
            }
        }

        private static void ApplyRomanceChoice(SaveEnvelope save, string eventId, string choiceId)
        {
            var state = save?.state;
            if (state?.relationships == null) return;
            var relationship = state.relationships.Find(candidate =>
                candidate != null && candidate.relationshipId == "school_peer_primary");
            if (relationship == null) return;
            var previousType = relationship.relationshipType;
            if (eventId == RepresentativeEvents.PromInvitationId)
            {
                relationship.relationshipType = choiceId == "attend_prom_together" ? "dating" : "friend";
                relationship.relationshipStage = choiceId == "attend_prom_together" ? "dating" : "friendship";
            }
            else if (eventId == RepresentativeEvents.ProposalId)
            {
                if (choiceId == "propose_marriage")
                {
                    relationship.relationshipType = "engaged";
                    relationship.relationshipStage = "engaged";
                }
                else if (choiceId == "end_partnership")
                {
                    relationship.relationshipType = "ex_partner";
                    relationship.relationshipStage = "separated";
                }
            }
            else if (eventId == RepresentativeEvents.WeddingId)
            {
                if (choiceId == "get_married")
                {
                    relationship.relationshipType = "married";
                    relationship.relationshipStage = "married";
                    MergeSpouseFinances(state, relationship);
                }
                else if (choiceId == "call_off_wedding")
                {
                    relationship.relationshipType = "ex_partner";
                    relationship.relationshipStage = "separated";
                }
            }
            else if (eventId == RepresentativeEvents.MarriageCrossroadsId &&
                     (choiceId == "separate" || choiceId == "divorce"))
            {
                relationship.relationshipType = "ex_partner";
                relationship.relationshipStage = choiceId == "divorce" ? "divorced" : "separated";
                state.finances.spouseAnnualIncomeMinorUnits = 0;
                ApplyChildCustodyAfterSeparation(state, relationship.relationshipId);
            }
            if (!string.Equals(previousType, relationship.relationshipType, StringComparison.Ordinal))
            {
                relationship.warmth = ClampStat(relationship.warmth +
                    (relationship.relationshipType == "ex_partner" ? -10 : 5));
                AppendRelationshipHistory(save, relationship, choiceId,
                    $"Relationship changed from {ToDisplayName(previousType)} to {ToDisplayName(relationship.relationshipType)}.");
            }
        }

        private static void MergeSpouseFinances(GameState state, RelationshipState spouse)
        {
            if (state?.finances == null || spouse == null || spouse.financesMerged) return;
            EnsureNpcFinancialProfile(spouse);
            state.finances.cashMinorUnits += spouse.npcCashMinorUnits;
            state.finances.debtMinorUnits += spouse.npcDebtMinorUnits;
            state.finances.spouseAnnualIncomeMinorUnits = spouse.npcAnnualIncomeMinorUnits;
            spouse.financesMerged = true;
        }

        public static void EnsureNpcFinancialProfile(RelationshipState npc)
        {
            if (npc == null || npc.npcAnnualIncomeMinorUnits > 0) return;
            var nameScore = StableStringScore(npc.displayName);
            npc.npcSmarts = npc.npcSmarts > 0 ? npc.npcSmarts : 35 + nameScore % 61;
            npc.npcCareerLevel = npc.npcCareerLevel > 0
                ? npc.npcCareerLevel
                : Math.Max(1, Math.Min(5, 1 + (npc.npcSmarts + npc.value) / 40));
            npc.npcAnnualIncomeMinorUnits = 2400000L + npc.npcCareerLevel * 1200000L + npc.npcSmarts * 20000L;
            npc.npcCashMinorUnits = npc.npcAnnualIncomeMinorUnits * (5L + npc.npcCareerLevel) / 20L;
            npc.npcDebtMinorUnits = Math.Max(0, 60 - npc.npcSmarts) * 5000L;
        }

        private static int StableStringScore(string value)
        {
            unchecked
            {
                var hash = 17;
                foreach (var character in value ?? string.Empty) hash = hash * 31 + character;
                return (int)((uint)hash % 1000u);
            }
        }

        private void ApplyEffect(SaveEnvelope save, Effect effect)
        {
            if (effect == null)
            {
                return;
            }

            var value = effectValueResolver.Resolve(effect);
            switch (effect.type)
            {
                case EffectType.CashDelta:
                    save.state.finances.cashMinorUnits = Math.Max(
                        0,
                        save.state.finances.cashMinorUnits + (long)Math.Round(value));
                    break;
                case EffectType.SalaryDelta:
                    save.state.career.annualSalaryMinorUnits = Math.Max(
                        0,
                        save.state.career.annualSalaryMinorUnits + (long)Math.Round(value));
                    break;
                case EffectType.DebtDelta:
                    save.state.finances.debtMinorUnits = Math.Max(
                        0,
                        save.state.finances.debtMinorUnits + (long)Math.Round(value));
                    break;
                case EffectType.StatDelta:
                    ApplyStatDelta(save, effect.targetId, value);
                    break;
                case EffectType.CareerProgressDelta:
                    save.state.career.careerProgress = ClampStat(
                        save.state.career.careerProgress + (int)Math.Round(value));
                    break;
                case EffectType.SkillXp:
                    ApplySkillXp(save.state.skills, effect.targetId, value);
                    break;
                case EffectType.RelationshipDelta:
                    ApplyRelationshipDelta(save.state.relationships, effect.targetId, value);
                    break;
                case EffectType.StatusAdd:
                    AddOrRefreshStatus(save.state.statuses, effect.targetId, value);
                    break;
                case EffectType.StatusRemove:
                    save.state.statuses.RemoveAll(status => status.statusId == effect.targetId);
                    break;
            }
        }

        private static void ApplySkillXp(List<SkillState> skills, string skillId, float value)
        {
            var skill = skills.Find(candidate => candidate.skillId == skillId);
            if (skill == null)
            {
                skill = new SkillState { skillId = skillId };
                skills.Add(skill);
            }
            skill.experience = Math.Max(0, skill.experience + (int)Math.Round(value));
        }

        private static void ApplyRelationshipDelta(
            List<RelationshipState> relationships,
            string relationshipId,
            float value)
        {
            RelationshipState relationship;
            if (relationshipId == "closest_relationship")
            {
                relationship = null;
                foreach (var candidate in relationships)
                {
                    if (candidate != null && (relationship == null || candidate.value > relationship.value))
                        relationship = candidate;
                }
                if (relationship == null) return;
            }
            else
            {
                relationship = relationships.Find(candidate => candidate.relationshipId == relationshipId);
            }
            if (relationship == null)
            {
                relationship = new RelationshipState { relationshipId = relationshipId };
                relationships.Add(relationship);
            }
            relationship.value = ClampStat(relationship.value + (int)Math.Round(value));
            relationship.monthsSinceInteraction = 0;
            UpdateRelationshipStage(relationship, false);
        }

        private static void AddOrRefreshStatus(List<StatusState> statuses, string statusId, float value)
        {
            var duration = Math.Max(1, (int)Math.Round(value));
            var status = statuses.Find(candidate => candidate.statusId == statusId);
            if (status == null)
            {
                statuses.Add(new StatusState { statusId = statusId, remainingMonths = duration });
                return;
            }
            status.remainingMonths = Math.Max(status.remainingMonths, duration);
        }

        private static void AdvanceStatuses(List<StatusState> statuses)
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

        private static void AdvanceRelationships(SaveEnvelope save)
        {
            if (save.state.relationships == null) return;
            foreach (var relationship in save.state.relationships)
            {
                if (relationship == null) continue;
                relationship.monthsSinceInteraction++;
                if (relationship.relationshipType == "parent" || relationship.relationshipType == "child") continue;

                var previousType = relationship.relationshipType;
                if ((previousType == "friend" && relationship.monthsSinceInteraction % 3 == 0) ||
                    (previousType == "best_friend" && relationship.monthsSinceInteraction % 4 == 0))
                {
                    relationship.value = ClampStat(relationship.value - 1);
                }
                else if (previousType == "rival" && relationship.monthsSinceInteraction % 6 == 0)
                {
                    relationship.value = ClampStat(relationship.value + 1);
                }
                else if (((previousType == "dating" || previousType == "engaged") &&
                          relationship.monthsSinceInteraction % 3 == 0) ||
                         (previousType == "partner" && relationship.monthsSinceInteraction % 4 == 0) ||
                         (previousType == "married" && relationship.monthsSinceInteraction % 6 == 0))
                {
                    relationship.value = ClampStat(relationship.value - 1);
                }

                UpdateRelationshipStage(relationship, false);
                if (!string.Equals(previousType, relationship.relationshipType, StringComparison.Ordinal))
                {
                    AddLifeFeedEntry(save, "relationship",
                        $"{relationship.displayName}'s relationship changed from {ToDisplayName(previousType)} to {ToDisplayName(relationship.relationshipType)}.");
                }
            }
        }

        private static void UpdateRelationshipStage(RelationshipState relationship, bool reconciled)
        {
            if (relationship == null || relationship.relationshipType == "parent" ||
                relationship.relationshipType == "child" || relationship.relationshipType == "adult_child")
                return;
            if (relationship.relationshipType == "dating" || relationship.relationshipType == "partner" ||
                relationship.relationshipType == "engaged" || relationship.relationshipType == "married" ||
                relationship.relationshipType == "ex_partner") return;
            if (relationship.value <= 15)
                relationship.relationshipType = "estranged";
            else if (relationship.value <= 25)
                relationship.relationshipType = "rival";
            else if (relationship.value >= 85)
                relationship.relationshipType = "best_friend";
            else if (relationship.relationshipType == "best_friend" && relationship.value < 75)
                relationship.relationshipType = "friend";
            else if (reconciled || relationship.relationshipType == "estranged" && relationship.value >= 35)
                relationship.relationshipType = "friend";
        }

        private static void BeginAnnualCycleIfNeeded(GameState state)
        {
            state.annualReview ??= new AnnualReviewState();
            var review = state.annualReview;
            if (review.monthsAccumulated != 0) return;
            review.cycleStartAge = state.character.age;
            review.startingCashMinorUnits = state.finances.cashMinorUnits;
            review.startingSavingsMinorUnits = state.finances.savingsMinorUnits;
            review.startingIndexFundMinorUnits = state.finances.indexFundMinorUnits;
            review.startingDebtMinorUnits = state.finances.debtMinorUnits;
            review.startingHealth = state.character.health;
            review.startingHappiness = state.character.happiness;
            review.startingSmarts = state.character.smarts;
            review.startingCareerProgress = state.career?.careerProgress ?? 0;
            review.startingQualificationExperience = state.education?.qualificationExperience ?? 0;
            review.startingRelationshipValue = TotalRelationshipValue(state.relationships);
            review.startingSkillExperience = TotalSkillExperience(state.skills);
            review.startingLifeFeedCount = state.lifeFeed?.Count ?? 0;
            review.majorOutcomeSummaries ??= new List<string>();
            review.majorOutcomeSummaries.Clear();
        }

        private static void CompleteAnnualCycle(GameState state)
        {
            var review = state.annualReview;
            review.monthsAccumulated = 12;
            review.completedAtAge = state.character.age;
            review.cashDeltaMinorUnits = state.finances.cashMinorUnits - review.startingCashMinorUnits;
            review.savingsDeltaMinorUnits = state.finances.savingsMinorUnits - review.startingSavingsMinorUnits;
            review.indexFundDeltaMinorUnits = state.finances.indexFundMinorUnits - review.startingIndexFundMinorUnits;
            review.debtDeltaMinorUnits = state.finances.debtMinorUnits - review.startingDebtMinorUnits;
            review.healthDelta = state.character.health - review.startingHealth;
            review.happinessDelta = state.character.happiness - review.startingHappiness;
            review.smartsDelta = state.character.smarts - review.startingSmarts;
            review.careerProgressDelta = (state.career?.careerProgress ?? 0) - review.startingCareerProgress;
            review.qualificationExperienceDelta = (state.education?.qualificationExperience ?? 0) - review.startingQualificationExperience;
            review.relationshipValueDelta = TotalRelationshipValue(state.relationships) - review.startingRelationshipValue;
            review.skillExperienceDelta = TotalSkillExperience(state.skills) - review.startingSkillExperience;
            CaptureAnnualOutcomes(state, review);
        }

        private static int TotalSkillExperience(List<SkillState> skills)
        {
            if (skills == null) return 0;
            var total = 0;
            foreach (var skill in skills) total += skill?.experience ?? 0;
            return total;
        }

        private static void CaptureAnnualOutcomes(GameState state, AnnualReviewState review)
        {
            review.majorOutcomeSummaries ??= new List<string>();
            review.majorOutcomeSummaries.Clear();
            if (state.lifeFeed == null) return;
            var candidates = new List<LifeFeedEntry>();
            for (var index = Math.Min(review.startingLifeFeedCount, state.lifeFeed.Count);
                 index < state.lifeFeed.Count; index++)
            {
                var entry = state.lifeFeed[index];
                if (entry != null && IsMajorAnnualOutcomeCategory(entry.category)) candidates.Add(entry);
            }
            candidates.Sort((left, right) =>
            {
                var revision = left.revision.CompareTo(right.revision);
                return revision != 0 ? revision : string.CompareOrdinal(left.entryId, right.entryId);
            });
            var start = Math.Max(0, candidates.Count - 5);
            for (var index = start; index < candidates.Count; index++)
                review.majorOutcomeSummaries.Add(candidates[index].text);
        }

        private static bool IsMajorAnnualOutcomeCategory(string category)
        {
            return category == "event" || category == "milestone" || category == "achievement" ||
                   category == "education" || category == "career" || category == "relationship";
        }

        private static int TotalRelationshipValue(List<RelationshipState> relationships)
        {
            if (relationships == null) return 0;
            var total = 0;
            foreach (var relationship in relationships) total += relationship?.value ?? 0;
            return total;
        }

        private static long ApplyAnnualReviewReward(SaveEnvelope save, string eventId, string choiceId)
        {
            if (eventId != RepresentativeEvents.YearInReviewId) return 0;
            var review = save.state.annualReview;
            if (review == null || review.completedAtAge < 0 || review.rewardedAtAge == review.completedAtAge)
                return 0;

            review.rewardedAtAge = review.completedAtAge;
            save.state.annualReviewHistory ??= new List<AnnualReviewHistoryState>();
            save.state.annualReviewHistory.Add(new AnnualReviewHistoryState
            {
                completedAtAge = review.completedAtAge,
                rewardChoiceId = choiceId,
                summary = BuildAnnualReviewSummary(save.state),
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            while (save.state.annualReviewHistory.Count > 10)
                save.state.annualReviewHistory.RemoveAt(0);
            review.monthsAccumulated = 0;
            review.cycleStartAge = -1;
            switch (choiceId)
            {
                case "build_security":
                    return 0;
                case "invest_in_growth":
                    if (save.state.education != null) save.state.education.qualificationExperience += 12;
                    return 0;
                case "nurture_connections":
                    save.state.household.happiness = ClampStat(save.state.household.happiness + 4);
                    return 0;
                default:
                    return 0;
            }
        }

        private void QueueDeferredAnnualReview(SaveEnvelope save, string resolvedEventId)
        {
            var review = save.state.annualReview;
            if (resolvedEventId == RepresentativeEvents.YearInReviewId || review == null ||
                review.completedAtAge != save.state.character.age ||
                review.rewardedAtAge == review.completedAtAge ||
                !eventCatalog.TryGetEvent(RepresentativeEvents.YearInReviewId, out _))
            {
                return;
            }
            save.state.pendingEventId = RepresentativeEvents.YearInReviewId;
        }

        private static void NormalizeProgressCollections(GameState state)
        {
            if (state == null)
            {
                return;
            }
            state.skills ??= new List<SkillState>();
            state.relationships ??= new List<RelationshipState>();
            state.statuses ??= new List<StatusState>();
            state.achievements ??= new List<AchievementState>();
            state.goals ??= new List<GoalState>();
            state.orientation ??= new OrientationState();
            state.transitionPresentations ??= new List<TransitionPresentationState>();
            state.lifeDecisions ??= new List<LifeDecisionState>();
            state.household ??= new HouseholdState();
            state.annualReview ??= new AnnualReviewState();
            state.annualReviewHistory ??= new List<AnnualReviewHistoryState>();
            state.moneyTransactions ??= new List<MoneyTransactionState>();
            state.business ??= new BusinessState();
            NormalizeBusinessPortfolio(state);
            state.propertyPortfolio ??= new PropertyPortfolioState();
            state.propertyPortfolio.properties ??= new List<RentalPropertyState>();
            state.propertyPortfolio.ledger ??= new List<PropertyLedgerEntry>();
            state.lifeFeed ??= new List<LifeFeedEntry>();
            state.scheduledEvents ??= new List<ScheduledEventRecord>();
            state.matchSession ??= new MatchSessionState();
            state.home ??= new HomeState();
            NormalizeHomeInventory(state.home);
        }

        private static void NormalizeBusinessPortfolio(GameState state)
        {
            state.businessPortfolio ??= new BusinessPortfolioState();
            state.businessPortfolio.businesses ??= new List<BusinessState>();
            var primary = state.business;
            if (primary == null || primary.status == "none" || string.IsNullOrEmpty(primary.businessId)) return;
            var existing = state.businessPortfolio.businesses.FindIndex(item =>
                item != null && item.businessId == primary.businessId);
            if (existing >= 0) state.businessPortfolio.businesses[existing] = primary;
            else if (state.businessPortfolio.businesses.Count < BusinessPortfolioState.MaxBusinesses)
                state.businessPortfolio.businesses.Insert(0, primary);
            if (string.IsNullOrEmpty(state.businessPortfolio.selectedBusinessId))
                state.businessPortfolio.selectedBusinessId = primary.businessId;
        }

        private static void NormalizeHomeInventory(HomeState home)
        {
            if (home == null) return;
            home.inventory ??= HomeState.CreateDefaultInventory();
            var books = FindHomeInventoryItem(home, "starter_books");
            if (books == null && home.inventory.Count < HomeState.MaxInventoryItems)
            {
                books = HomeState.CreateDefaultInventory()[0];
                home.inventory.Add(books);
            }
            var equipment = FindHomeInventoryItem(home, "starter_training_kit");
            if (equipment == null && home.inventory.Count < HomeState.MaxInventoryItems)
            {
                equipment = HomeState.CreateDefaultInventory()[1];
                home.inventory.Add(equipment);
            }
            if (books != null)
            {
                books.quantity = home.readingMaterialStock;
                books.capacity = home.readingMaterialCapacity;
            }
            if (equipment != null) equipment.condition = home.trainingEquipmentCondition;
        }

        private static HomeInventoryItemState FindHomeInventoryItem(HomeState home, string itemId)
        {
            return home?.inventory?.Find(item => item != null && item.itemId == itemId);
        }

        private static void EvaluateAchievements(SaveEnvelope save)
        {
            NormalizeProgressCollections(save.state);
            RefreshGoals(save);
            var state = save.state;
            TryUnlockAchievement(save, "first_year", state.character.age >= 1);
            TryUnlockAchievement(save, "school_days", state.education != null && state.education.stage != "not_started");
            TryUnlockAchievement(save, "learning_level_2", GetSkillLevel(GetSkillExperience(state.skills, "learning")) >= 2);
            TryUnlockAchievement(save, "family_bond", state.relationships.Exists(
                relationship => relationship != null && relationship.relationshipType == "parent" && relationship.value >= 75));
            var role = state.career?.roleTitle;
            TryUnlockAchievement(save, "first_job", !string.IsNullOrEmpty(role) && role != "Retired");
            TryUnlockAchievement(save, "moving_up", role == "Associate" || role == "Senior Associate" || role == "Manager");
            TryUnlockAchievement(save, "six_figures", state.finances.cashMinorUnits >= 10000000);
            TryUnlockAchievement(save, "first_choice", state.eventHistory.Count > 0);
            TryUnlockAchievement(save, "retirement", state.character.lifeStatus == "retired");
            TryUnlockAchievement(save, "life_complete", state.character.lifeStatus != "active");
        }

        public IReadOnlyList<GoalState> GetGoals()
        {
            return ActiveSave?.state?.goals ?? (IReadOnlyList<GoalState>)Array.Empty<GoalState>();
        }

        public bool TryClaimGoalReward(string goalId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    NormalizeProgressCollections(candidate.state);
                    RefreshGoals(candidate);
                    var goal = candidate.state.goals.Find(item => item != null && item.goalId == goalId);
                    if (goal == null) return TransactionMutationResult.Failure("This goal is not available.");
                    if (goal.status == "claimed")
                        return TransactionMutationResult.Failure("This goal reward was already claimed.");
                    if (goal.status != "claimable")
                        return TransactionMutationResult.Failure("Complete the goal before claiming its reward.");
                    candidate.state.finances.cashMinorUnits += goal.rewardMinorUnits;
                    goal.status = "claimed";
                    goal.claimedRevision = candidate.revision;
                    goal.claimedAtUtc = candidate.updatedAtUtc;
                    var claimSummary = $"Claimed {goal.title} · Cash +{FormatMoney(goal.rewardMinorUnits)}";
                    AddLifeFeedEntry(candidate, "goal", claimSummary);
                    return TransactionMutationResult.Success(claimSummary);
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryPinGoal(string goalId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    var goal = candidate.state.goals?.Find(item => item?.goalId == goalId);
                    if (goal == null || goal.status == "expired")
                        return TransactionMutationResult.Failure("This goal is unavailable.");
                    var pin = !goal.pinned;
                    foreach (var item in candidate.state.goals)
                        if (item != null) item.pinned = false;
                    goal.pinned = pin;
                    return TransactionMutationResult.Success(pin ? $"Pinned {goal.title}." : $"Unpinned {goal.title}.");
                },
                out var committed,
                out summary);
            if (succeeded) ActiveSave = committed;
            return succeeded;
        }

        private static void RefreshGoals(SaveEnvelope save)
        {
            var state = save.state;
            state.goals ??= new List<GoalState>();
            EnsureGoal(state.goals, new GoalState
            {
                goalId = "main_first_career", category = "main", title = "Start a Career",
                description = "Get hired into your first career.", destination = "career",
                progressRequired = 1, rewardMinorUnits = ProgressionStandards.MainGoalRewardMinorUnits,
                createdAtAge = state.character.age, createdAtMonth = state.calendar.monthOfYear
            });
            EnsureGoal(state.goals, new GoalState
            {
                goalId = "life_net_worth_100k", category = "life", title = "Build Security",
                description = "Reach $100,000 net worth.", destination = "money",
                progressRequired = 10000000, rewardMinorUnits = ProgressionStandards.LifeGoalRewardMinorUnits,
                createdAtAge = state.character.age, createdAtMonth = state.calendar.monthOfYear
            });
            var dailyId = $"daily_focus_{state.character.age}_{state.calendar.monthOfYear}";
            foreach (var oldDaily in state.goals)
            {
                if (oldDaily != null && oldDaily.category == "daily" && oldDaily.goalId != dailyId &&
                    oldDaily.status != "claimed") oldDaily.status = "expired";
            }
            EnsureGoal(state.goals, new GoalState
            {
                goalId = dailyId, category = "daily", title = "Choose a Focus",
                description = "Complete one contextual focus activity this month.", destination = "life",
                progressRequired = 1, rewardMinorUnits = ProgressionStandards.DailyGoalRewardMinorUnits,
                createdAtAge = state.character.age, createdAtMonth = state.calendar.monthOfYear
            });

            var employed = !string.IsNullOrEmpty(state.career?.roleTitle) && state.career.roleTitle != "Retired";
            UpdateGoal(state.goals, "main_first_career", employed ? 1 : 0);
            var netWorth = Math.Max(0L, CalculateNetWorth(state));
            UpdateGoal(state.goals, "life_net_worth_100k", (int)Math.Min(10000000L, netWorth));
            var focused = state.statuses.Exists(status => status.statusId == "monthly_focus_used");
            UpdateGoal(state.goals, dailyId, focused ? 1 : 0);
            while (state.goals.Count > 20)
            {
                var removable = state.goals.FindIndex(goal => goal.category == "daily" && goal.status == "expired");
                state.goals.RemoveAt(removable >= 0 ? removable : 0);
            }
        }

        private static void EnsureGoal(List<GoalState> goals, GoalState goal)
        {
            if (!goals.Exists(existing => existing != null && existing.goalId == goal.goalId)) goals.Add(goal);
        }

        private static void UpdateGoal(List<GoalState> goals, string goalId, int progress)
        {
            var goal = goals.Find(item => item != null && item.goalId == goalId);
            if (goal == null || goal.status == "claimed" || goal.status == "expired") return;
            goal.progress = Math.Max(0, Math.Min(goal.progressRequired, progress));
            goal.status = goal.progress >= goal.progressRequired ? "claimable" : "active";
        }

        private static void TryUnlockAchievement(SaveEnvelope save, string achievementId, bool condition)
        {
            if (!condition || save.state.achievements.Exists(
                    achievement => achievement != null && achievement.achievementId == achievementId))
            {
                return;
            }
            save.state.achievements.Add(new AchievementState
            {
                achievementId = achievementId,
                unlockedAtAge = save.state.character.age,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            AddLifeFeedEntry(save, "achievement", $"Achievement unlocked · {GetAchievementDisplayName(achievementId)}");
        }

        public static string GetAchievementDisplayName(string achievementId)
        {
            switch (achievementId)
            {
                case "first_year": return "First Steps";
                case "school_days": return "School Days";
                case "learning_level_2": return "Curious Mind";
                case "family_bond": return "Family Bond";
                case "first_job": return "Hired";
                case "moving_up": return "Moving Up";
                case "six_figures": return "Six Figures";
                case "first_choice": return "Decision Maker";
                case "retirement": return "Golden Years";
                case "life_complete": return "A Life Lived";
                default: return ToDisplayName(achievementId);
            }
        }

        public static string GetAchievementDescription(string achievementId)
        {
            switch (achievementId)
            {
                case "first_year": return "Reach age 1.";
                case "school_days": return "Begin formal education.";
                case "learning_level_2": return "Reach Learning Level 2.";
                case "family_bond": return "Reach 75 relationship strength with a parent.";
                case "first_job": return "Start your first career.";
                case "moving_up": return "Earn your first promotion.";
                case "six_figures": return "Hold $100,000 in cash.";
                case "first_choice": return "Resolve your first life event.";
                case "retirement": return "Retire from an active career.";
                case "life_complete": return "Complete a life through retirement or death.";
                default: return "A milestone from this life.";
            }
        }

        public static long GetAchievementRewardMinorUnits(string achievementId)
        {
            switch (achievementId)
            {
                case "first_year": return 10000;
                case "school_days": return 15000;
                case "learning_level_2": return 25000;
                case "family_bond": return 20000;
                case "first_job": return 50000;
                case "moving_up": return 75000;
                case "six_figures": return 100000;
                case "first_choice": return 10000;
                case "retirement": return 150000;
                case "life_complete": return 200000;
                default: return 0;
            }
        }

        public bool TryClaimAchievementReward(string achievementId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    NormalizeProgressCollections(candidate.state);
                    var achievement = candidate.state.achievements.Find(item =>
                        item != null && item.achievementId == achievementId);
                    if (achievement == null)
                        return TransactionMutationResult.Failure("This achievement has not been unlocked.");
                    if (achievement.rewardClaimed)
                        return TransactionMutationResult.Failure("This achievement prize was already claimed.");
                    var reward = GetAchievementRewardMinorUnits(achievementId);
                    if (reward <= 0)
                        return TransactionMutationResult.Failure("This achievement does not have a claimable prize.");
                    candidate.state.finances.cashMinorUnits += reward;
                    achievement.rewardClaimed = true;
                    achievement.rewardClaimedRevision = candidate.revision;
                    achievement.rewardClaimedAtUtc = candidate.updatedAtUtc;
                    var claimSummary = $"Claimed {GetAchievementDisplayName(achievementId)} prize · Cash +{FormatMoney(reward)}";
                    AddLifeFeedEntry(candidate, "achievement", claimSummary);
                    return TransactionMutationResult.Success(claimSummary);
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public TransitionPresentationState GetPendingTransition()
        {
            return ActiveSave?.state?.transitionPresentations?.Find(item =>
                item != null && !item.acknowledged);
        }

        public bool ShouldPresentFirstLifeOrientation()
        {
            return ActiveSave?.state?.character?.age == 0 &&
                   ActiveSave.state.orientation?.status != "completed";
        }

        public bool TryCompleteFirstLifeOrientation(out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    NormalizeProgressCollections(candidate.state);
                    if (candidate.state.orientation.status == "completed")
                        return TransactionMutationResult.Failure("First-life orientation was already completed.");
                    candidate.state.orientation.status = "completed";
                    candidate.state.orientation.completedRevision = candidate.revision;
                    candidate.state.orientation.completedAtUtc = candidate.updatedAtUtc;
                    AddLifeFeedEntry(candidate, "milestone", "Orientation complete · The Life Feed, time controls, requirements, and autosave are ready.");
                    return TransactionMutationResult.Success("First-life orientation completed.");
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryAcknowledgeTransition(string transitionId, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    NormalizeProgressCollections(candidate.state);
                    var transition = candidate.state.transitionPresentations.Find(item =>
                        item != null && item.transitionId == transitionId);
                    if (transition == null)
                        return TransactionMutationResult.Failure("This transition presentation no longer exists.");
                    if (transition.acknowledged)
                        return TransactionMutationResult.Failure("This transition presentation was already acknowledged.");
                    transition.acknowledged = true;
                    transition.acknowledgedRevision = candidate.revision;
                    transition.acknowledgedAtUtc = candidate.updatedAtUtc;
                    return TransactionMutationResult.Success($"Acknowledged {transition.title}.");
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        private static void EnqueueTransition(
            SaveEnvelope save, string transitionType, string title, string summary)
        {
            NormalizeProgressCollections(save.state);
            var transitionId = $"{transitionType}_{save.lifeId}_{save.revision}";
            if (save.state.transitionPresentations.Exists(item =>
                    item != null && item.transitionId == transitionId)) return;
            save.state.transitionPresentations.Add(new TransitionPresentationState
            {
                transitionId = transitionId,
                transitionType = transitionType,
                title = title,
                summary = summary,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                createdAtUtc = save.updatedAtUtc
            });
            while (save.state.transitionPresentations.Count > 20)
                save.state.transitionPresentations.RemoveAt(0);
            AddLifeFeedEntry(save, "milestone", $"{title} · {summary}");
        }

        private static void AddLifeFeedEntry(SaveEnvelope save, string category, string text)
        {
            save.state.lifeFeed ??= new List<LifeFeedEntry>();
            save.state.historyArchive ??= new HistoryArchiveState();
            save.state.lifeFeed.Add(new LifeFeedEntry
            {
                entryId = $"{save.revision}_{category}_{save.state.historyArchive.lifeFeedArchivedCount + save.state.lifeFeed.Count}",
                category = category,
                text = text,
                age = save.state.character.age,
                monthOfYear = save.state.calendar.monthOfYear,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            HistoryRetention.Apply(save.state);
        }

        private void ApplyStatDelta(SaveEnvelope save, string targetId, float value)
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
                case "home_condition":
                    if (save.state.home != null)
                        save.state.home.condition = ClampStat(save.state.home.condition + delta);
                    break;
            }
        }

        private static int ClampStat(int value)
        {
            return Math.Max(SaveSchema.MinCoreStatValue, Math.Min(SaveSchema.MaxCoreStatValue, value));
        }

        private Event SelectEligibleEvent(SaveEnvelope save, int processedMonth, bool completedYear)
        {
            var scheduled = SelectScheduledEvent(save, completedYear);
            if (scheduled != null)
            {
                return scheduled;
            }

            if (!string.IsNullOrEmpty(save.state.education?.awaitingDecisionId))
            {
                return null;
            }

            if (completedYear && save.state.character.age == 16 &&
                save.state.character.genderIdentity == "undiscovered" &&
                eventCatalog.TryGetEvent(RepresentativeEvents.ComingOfAgeGenderId, out var identityEvent) &&
                IsEligible(identityEvent, save))
            {
                return identityEvent;
            }

            if (completedYear && eventCatalog.TryGetEvent(RepresentativeEvents.YearInReviewId, out var review) &&
                IsEligible(review, save))
            {
                return review;
            }

            var eligible = new List<Event>();
            var timingPriority = new List<Event>();
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

        private Event SelectScheduledEvent(SaveEnvelope save, bool completedYear)
        {
            if (save.state.scheduledEvents == null) return null;
            var age = save.state.character.age;
            var absoluteMonth = GetAbsoluteMonth(save.state);
            var candidates = new List<ScheduledEventRecord>();
            for (var index = save.state.scheduledEvents.Count - 1; index >= 0; index--)
            {
                var record = save.state.scheduledEvents[index];
                var monthlyWindow = record?.earliestTriggerMonth > 0 || record?.latestTriggerMonth > 0;
                var expired = monthlyWindow
                    ? absoluteMonth > record.latestTriggerMonth
                    : record != null && age > record.latestTriggerAge;
                if (record == null || expired || IsScheduledEventCancelled(save.state, record))
                {
                    save.state.scheduledEvents.RemoveAt(index);
                    continue;
                }
                if (monthlyWindow)
                {
                    if (absoluteMonth < record.earliestTriggerMonth) continue;
                }
                else
                {
                    if (!completedYear || age < record.earliestTriggerAge) continue;
                }
                if (!eventCatalog.TryGetEvent(record.eventId, out var evt) || !IsEligible(evt, save))
                {
                    var atWindowEnd = monthlyWindow
                        ? absoluteMonth >= record.latestTriggerMonth
                        : age >= record.latestTriggerAge;
                    if (atWindowEnd) save.state.scheduledEvents.RemoveAt(index);
                    continue;
                }

                if (save.state.statuses.Exists(status =>
                        status != null && status.statusId == $"scheduled_event_cooldown_{record.eventId}"))
                    continue;
                candidates.Add(record);
            }

            candidates.Sort((left, right) =>
            {
                var priority = right.priority.CompareTo(left.priority);
                if (priority != 0) return priority;
                var timing = left.earliestTriggerMonth.CompareTo(right.earliestTriggerMonth);
                return timing != 0 ? timing : string.CompareOrdinal(left.eventId, right.eventId);
            });
            foreach (var record in candidates)
            {
                var index = save.state.scheduledEvents.IndexOf(record);
                if (index < 0) continue;

                var chance = Math.Max(0f, Math.Min(1f, record.chance));
                var roll = StableUnit(save.rng.seed, save.rng.step + 7919 + StableStringHash(record.eventId));
                if (roll > chance)
                {
                    var monthlyWindow = record.earliestTriggerMonth > 0 || record.latestTriggerMonth > 0;
                    var atWindowEnd = monthlyWindow
                        ? absoluteMonth >= record.latestTriggerMonth
                        : age >= record.latestTriggerAge;
                    if (atWindowEnd) save.state.scheduledEvents.RemoveAt(index);
                    continue;
                }

                save.state.scheduledEvents.RemoveAt(index);
                if (record.cooldownMonths > 0)
                    AddOrRefreshStatus(save.state.statuses,
                        $"scheduled_event_cooldown_{record.eventId}", record.cooldownMonths);
                eventCatalog.TryGetEvent(record.eventId, out var evt);
                return evt;
            }
            return null;
        }

        private void ScheduleNpcRelationshipEvent(SaveEnvelope save, RelationshipState relationship,
            RelationshipInteractionType interactionType)
        {
            if (save?.state == null || relationship == null) return;
            string eventId = null;
            var priority = 0;
            var cancellationRule = "relationship_missing_or_unavailable";
            if (relationship.relationshipId == "school_peer_primary" &&
                save.state.character.age >= 10 && save.state.character.age <= 17 &&
                (interactionType == RelationshipInteractionType.Argue ||
                 interactionType == RelationshipInteractionType.Compete))
            {
                eventId = RepresentativeEvents.PeerTrustConflictId;
                priority = 60;
            }
            else if (relationship.relationshipType == "married" && relationship.value <= 55 &&
                     interactionType == RelationshipInteractionType.Argue)
            {
                eventId = RepresentativeEvents.MarriageCrossroadsId;
                priority = 90;
            }
            if (string.IsNullOrEmpty(eventId) || !eventCatalog.TryGetEvent(eventId, out _)) return;
            if (save.state.pendingEventId == eventId || save.state.scheduledEvents.Exists(record =>
                    record != null && record.eventId == eventId)) return;
            if (save.state.statuses.Exists(status => status != null &&
                    status.statusId == $"scheduled_event_cooldown_{eventId}")) return;

            var now = GetAbsoluteMonth(save.state);
            AddScheduledEvent(save.state, new ScheduledEventRecord
            {
                eventId = eventId,
                earliestTriggerAge = save.state.character.age,
                latestTriggerAge = save.state.character.age + 1,
                earliestTriggerMonth = now + 1,
                latestTriggerMonth = now + 3,
                priority = priority,
                cooldownMonths = 12,
                relationshipId = relationship.relationshipId,
                chance = 1f,
                sourceEventId = $"relationship_interaction_{interactionType.ToString().ToLowerInvariant()}",
                cancellationRule = cancellationRule
            });
        }

        private static void AddScheduledEvent(GameState state, ScheduledEventRecord record)
        {
            state.scheduledEvents ??= new List<ScheduledEventRecord>();
            state.scheduledEvents.Add(record);
            while (state.scheduledEvents.Count > ScheduledEventRecord.MaxScheduledEvents)
            {
                var removeIndex = 0;
                for (var index = 1; index < state.scheduledEvents.Count; index++)
                    if ((state.scheduledEvents[index]?.priority ?? 0) <
                        (state.scheduledEvents[removeIndex]?.priority ?? 0)) removeIndex = index;
                state.scheduledEvents.RemoveAt(removeIndex);
            }
        }

        private static bool IsScheduledEventCancelled(GameState state, ScheduledEventRecord record)
        {
            if (record.cancellationRule == "life_ended" && IsLifeEnded(state.character)) return true;
            if (record.cancellationRule == "engagement_ended" &&
                !state.relationships.Exists(item => item != null && item.relationshipType == "engaged")) return true;
            if (record.cancellationRule == "school_peer_primary_missing" &&
                !state.relationships.Exists(item => item != null && item.relationshipId == "school_peer_primary")) return true;
            if (record.cancellationRule == "school_peer_primary_missing_or_not_close")
            {
                var peer = state.relationships.Find(item => item != null && item.relationshipId == "school_peer_primary");
                if (peer == null || peer.value < 40) return true;
            }
            if (!string.IsNullOrEmpty(record.relationshipId))
            {
                var relationship = state.relationships.Find(item => item != null &&
                    item.relationshipId == record.relationshipId);
                if (relationship == null || relationship.relationshipType == "deceased" ||
                    relationship.relationshipType == "unavailable") return true;
            }
            return false;
        }

        private static int GetAbsoluteMonth(GameState state)
        {
            return Math.Max(0, state.character.age * 12 + Math.Max(0, state.calendar.monthOfYear - 1));
        }

        private static int StableStringHash(string value)
        {
            unchecked
            {
                var hash = 17;
                foreach (var character in value ?? string.Empty) hash = hash * 31 + character;
                return hash & int.MaxValue;
            }
        }

        private static void AvoidImmediateRepeatWhenPossible(
            List<Event> candidates,
            List<EventHistoryEntry> history)
        {
            if (candidates.Count < 2 || history == null || history.Count == 0)
            {
                return;
            }

            var latestEventId = history[history.Count - 1]?.eventId;
            candidates.RemoveAll(evt => evt != null && evt.id == latestEventId);
        }

        private static Event SelectEventWeightedByLuck(List<Event> eligible, SaveEnvelope save)
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

        private static float GetEventSelectionWeight(Event evt, int luck)
        {
            return GetLuckEventWeight(evt, luck) * Math.Max(0.01f, evt.monthlyTriggerChance);
        }

        public static float GetLuckEventWeight(Event evt, int luck)
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

        private static bool IsTimingEligible(Event evt, int processedMonth, bool completedYear)
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

        private static bool IsEligible(Event evt, SaveEnvelope save)
        {
            if (evt == null || !EventValidator.ValidateEvent(evt).isValid || evt.ageRange == null)
            {
                return false;
            }

            var age = save.state.character.age;
            if (!EventAgeEligibility.IsEligible(evt, age))
            {
                return false;
            }
            if (!MeetsAuthoredRequirements(evt.requirementsJson, save.state))
            {
                return false;
            }

            EventHistoryEntry latest = null;
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

        [Serializable]
        private sealed class AuthoredEventRequirements
        {
            public string relationshipId = string.Empty;
            public string secondaryRelationshipId = string.Empty;
            public string relationshipType = string.Empty;
            public int minimumRelationshipValue = 0;
            public int maximumRelationshipValue = 100;
            public string decisionId = string.Empty;
            public string decisionChoiceId = string.Empty;
            public string studyTrack = string.Empty;
            public int minimumQualificationExperience = 0;
            public int minimumHomeCondition = 0;
            public int maximumHomeCondition = 100;
        }

        private static bool MeetsAuthoredRequirements(string json, GameState state)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "{}") return true;
            AuthoredEventRequirements requirements;
            try
            {
                requirements = JsonUtility.FromJson<AuthoredEventRequirements>(json);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (requirements == null) return false;
            if (!string.IsNullOrEmpty(requirements.relationshipId))
            {
                var relationship = state.relationships?.Find(candidate =>
                    candidate != null && candidate.relationshipId == requirements.relationshipId);
                if (relationship == null) return false;
                if (!string.IsNullOrEmpty(requirements.relationshipType) &&
                    relationship.relationshipType != requirements.relationshipType) return false;
                if (relationship.value < requirements.minimumRelationshipValue) return false;
                if (relationship.value > requirements.maximumRelationshipValue) return false;
            }
            if (!string.IsNullOrEmpty(requirements.secondaryRelationshipId) &&
                state.relationships?.Exists(candidate => candidate != null &&
                    candidate.relationshipId == requirements.secondaryRelationshipId) != true)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(requirements.decisionId))
            {
                var decision = state.lifeDecisions?.Find(candidate =>
                    candidate != null && candidate.decisionId == requirements.decisionId);
                if (decision == null) return false;
                if (!string.IsNullOrEmpty(requirements.decisionChoiceId) &&
                    decision.choiceId != requirements.decisionChoiceId) return false;
            }
            if (!string.IsNullOrEmpty(requirements.studyTrack) &&
                state.education?.studyTrack != requirements.studyTrack)
            {
                return false;
            }
            if (requirements.minimumQualificationExperience > 0 &&
                (state.education?.qualificationExperience ?? 0) < requirements.minimumQualificationExperience)
            {
                return false;
            }
            var homeCondition = state.home?.condition ?? 100;
            if (homeCondition < requirements.minimumHomeCondition ||
                homeCondition > requirements.maximumHomeCondition)
            {
                return false;
            }
            return true;
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

        private static SaveEnvelope CloneSave(SaveEnvelope save)
        {
            return JsonUtility.FromJson<SaveEnvelope>(JsonUtility.ToJson(save));
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
            FinancesState finances,
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
            return MoneyFormatter.Format(minorUnits);
        }

        private static string FormatPreciseMoney(long minorUnits)
        {
            return MoneyFormatter.FormatPrecise(minorUnits);
        }

        private static string FormatSignedMoney(long minorUnits)
        {
            return $"{(minorUnits >= 0 ? "+" : "-")}{FormatMoney(Math.Abs(minorUnits))}";
        }

    }
}
