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

    public enum StimRelationshipInteractionType
    {
        Talk,
        PlayTogether,
        AskForHelp,
        SpendTime,
        Argue,
        Compete,
        Reconcile,
        AskOnDate,
        Commit,
        BreakUp
    }

    public enum StimPaymentMethod
    {
        Cash,
        Credit
    }

    public enum StimEducationActionType
    {
        Read,
        Homework,
        StudyGroup,
        AdvancedProject
    }

    public enum StimSchoolPathChoice
    {
        PublicSchool,
        Homeschool,
        AcademicTrack,
        VocationalTrack,
        LeaveSchool
    }

    public enum StimStudyTrack
    {
        General,
        Academic,
        Vocational
    }

    public enum StimStudyDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum StimCareerActionType
    {
        Apply,
        Interview,
        WorkHard,
        AskForPromotion,
        Quit,
        Retire
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
        private readonly StimSaveTransactionRunner transactionRunner;
        private readonly StimEducationActionService educationActionService;
        private readonly StimActionLifecycleService actionLifecycleService;

        public StimSaveEnvelope ActiveSave { get; private set; }
        public StimChoiceResolution LastResolution { get; private set; }
        public long LastFinancialImpactMinorUnits { get; private set; }

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
            transactionRunner = new StimSaveTransactionRunner(this.saveRepository, this.utcNow);
            educationActionService = new StimEducationActionService();
            actionLifecycleService = new StimActionLifecycleService();
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
            return TryResolveChoice(eventId, choiceId, StimPaymentMethod.Cash, out summary);
        }

        public bool TryResolveChoice(
            string eventId,
            string choiceId,
            StimPaymentMethod paymentMethod,
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

            if (eventId == RepresentativeStimEvents.PromInvitationId && choiceId == "attend_prom_together")
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
            if (eventId == RepresentativeStimEvents.ProposalId)
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
            if (eventId == RepresentativeStimEvents.WeddingId)
            {
                var fiance = ActiveSave.state.relationships?.Find(candidate =>
                    candidate != null && candidate.relationshipId == "school_peer_primary");
                if (ActiveSave.state.character.age < 25 || fiance == null || fiance.relationshipType != "engaged")
                {
                    summary = "Wedding decisions require age 25+ and an active engagement.";
                    return false;
                }
            }
            if (eventId == RepresentativeStimEvents.MarriageCrossroadsId)
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
            var financialImpact = ApplyResolution(candidateSave, resolution);
            if (!TryApplyFinancialImpact(candidateSave.state, financialImpact, paymentMethod, out summary))
            {
                if (financialImpact < 0 && paymentMethod == StimPaymentMethod.Credit)
                {
                    TryCommitCreditDenial(-financialImpact, summary, out summary);
                }
                return false;
            }
            if (financialImpact < 0 && paymentMethod == StimPaymentMethod.Credit)
            {
                candidateSave.state.character.happiness = ClampStat(
                    candidateSave.state.character.happiness + 1);
                AddLifeFeedEntry(candidateSave, "money",
                    $"Credit approved for {FormatMoney(-financialImpact)} at " +
                    $"{candidateSave.state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR · Happiness +1.");
            }
            EvaluateAchievements(candidateSave);
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
                          (financialImpact < 0 && paymentMethod == StimPaymentMethod.Credit
                              ? $" at {candidateSave.state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR"
                              : string.Empty) + ".");
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

        public bool TryAdvanceMonth(out StimEvent nextEvent, out string summary)
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
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before advancing another month.";
                return false;
            }
            if (!string.IsNullOrEmpty(ActiveSave.state.education?.awaitingDecisionId))
            {
                summary = $"Choose a school path for {ActiveSave.state.education.awaitingDecisionId} before advancing.";
                return false;
            }

            var candidateSave = CloneSave(ActiveSave);
            NormalizeProgressCollections(candidateSave.state);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            var paidMonth = candidateSave.state.calendar.monthOfYear;
            var creditInterest = AccrueHouseholdCreditInterest(candidateSave.state.finances);
            var playerPaycheck = CalculateMonthlyPaycheck(candidateSave.state.career.annualSalaryMinorUnits, paidMonth);
            var spousePaycheck = CalculateMonthlyPaycheck(
                candidateSave.state.finances.spouseAnnualIncomeMinorUnits, paidMonth);
            var paycheck = playerPaycheck + spousePaycheck;
            var taxes = CalculateTaxWithholding(paycheck, candidateSave.state.finances.taxRateBasisPoints);
            var expenses = candidateSave.state.finances.monthlyLivingExpensesMinorUnits;
            var netCashFlow = paycheck - taxes - expenses;
            ApplyMonthlyCashFlow(candidateSave.state.finances, paycheck, taxes, expenses);
            if (!string.IsNullOrEmpty(candidateSave.state.career.roleTitle) &&
                candidateSave.state.career.roleTitle != "Retired")
            {
                candidateSave.state.career.careerProgress = ClampStat(
                    candidateSave.state.career.careerProgress + 1);
            }
            candidateSave.state.character.happiness = ClampStat(
                candidateSave.state.character.happiness + (netCashFlow >= 0 ? 1 : -2));
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
                UpdateLifeAndEducationStage(candidateSave.state);
                var healthDecline = GetAnnualHealthDecline(candidateSave.state.character.age);
                candidateSave.state.character.health = ClampStat(
                    candidateSave.state.character.health - healthDecline);
                if (candidateSave.state.character.health <= 0)
                {
                    FinalizeLife(candidateSave.state.character, "deceased", "health_decline");
                }
            }
            else
            {
                candidateSave.state.calendar.monthOfYear++;
            }
            var lifeEnded = IsLifeEnded(candidateSave.state.character);
            nextEvent = lifeEnded ? null : SelectEligibleEvent(candidateSave, paidMonth, completedYear);
            candidateSave.state.pendingEventId = nextEvent?.id;
            candidateSave.state.calendar.quietMonthsSinceEvent = nextEvent == null
                ? candidateSave.state.calendar.quietMonthsSinceEvent + 1
                : 0;

            var hasCareer = !string.IsNullOrEmpty(candidateSave.state.career.roleTitle) &&
                            candidateSave.state.career.roleTitle != "Retired";
            var cashFlowSummary = $"gross {FormatMoney(paycheck)}, taxes {FormatMoney(taxes)}, expenses {FormatMoney(expenses)}, net {FormatSignedMoney(netCashFlow)}" +
                                  (creditInterest > 0 ? $", credit interest {FormatMoney(creditInterest)}" : string.Empty);
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

        public static string BuildFinalLifeSummary(StimSaveEnvelope save)
        {
            var state = save.state;
            var character = state.character;
            var name = string.IsNullOrEmpty(character.firstName)
                ? "This life"
                : $"{character.firstName}'s life";
            var ending = character.lifeStatus == "retired" ? "retirement" : "death";
            var netWorth = state.finances.cashMinorUnits - state.finances.debtMinorUnits;
            return $"{name} ended in {ending} at age {character.endedAtAge} · Net worth {FormatMoney(netWorth)}" +
                   $" · {state.achievements.Count} achievements · {state.eventHistory.Count} events" +
                   $" · {state.lifeFeed.Count} life chapters";
        }

        private static void UpdateLifeAndEducationStage(StimGameState state)
        {
            state.character.lifeStage = GetLifeStage(state.character.age);
            state.education ??= new StimEducationState();
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

        public bool TryChooseSchoolPath(StimSchoolPathChoice choice, out string summary)
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
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before choosing a school path.";
                return false;
            }

            NormalizeProgressCollections(ActiveSave.state);
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

            switch (choice)
            {
                case StimSchoolPathChoice.PublicSchool:
                    ApplySkillXp(candidateSave.state.skills, "social", 8);
                    summary = "Enrolled in public school · Social XP +8";
                    break;
                case StimSchoolPathChoice.Homeschool:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 2);
                    ApplySkillXp(candidateSave.state.skills, "learning", 10);
                    summary = "Started homeschooling · Smarts +2 · Learning XP +10";
                    break;
                case StimSchoolPathChoice.AcademicTrack:
                    ApplySkillXp(candidateSave.state.skills, "learning", 15);
                    summary = "Chose the academic track · Learning XP +15";
                    break;
                case StimSchoolPathChoice.VocationalTrack:
                    ApplySkillXp(candidateSave.state.skills, "practical", 15);
                    summary = "Chose the vocational track · Practical XP +15";
                    break;
                case StimSchoolPathChoice.LeaveSchool:
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

            candidateSave.state.lifeDecisions.Add(new StimLifeDecisionState
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

        private static StimRelationshipState EnsureSchoolPeer(
            StimSaveEnvelope save,
            string decisionId,
            StimSchoolPathChoice choice)
        {
            if (choice == StimSchoolPathChoice.LeaveSchool) return null;
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
            var peer = new StimRelationshipState
            {
                relationshipId = relationshipId,
                displayName = names[nameIndex],
                relationshipType = "friend",
                origin = choice == StimSchoolPathChoice.Homeschool ? "neighborhood" : $"{stageId}_school",
                introducedAtAge = save.state.character.age,
                value = choice == StimSchoolPathChoice.Homeschool ? 48 : 55
            };
            save.state.relationships.Add(peer);
            return peer;
        }

        public static bool IsSchoolPathChoiceAvailable(string decisionId, StimSchoolPathChoice choice)
        {
            switch (decisionId)
            {
                case "education_primary_enrollment":
                    return choice == StimSchoolPathChoice.PublicSchool || choice == StimSchoolPathChoice.Homeschool;
                case "education_middle_transition":
                    return choice == StimSchoolPathChoice.PublicSchool || choice == StimSchoolPathChoice.Homeschool ||
                           choice == StimSchoolPathChoice.LeaveSchool;
                case "education_high_transition":
                    return choice == StimSchoolPathChoice.AcademicTrack || choice == StimSchoolPathChoice.VocationalTrack ||
                           choice == StimSchoolPathChoice.LeaveSchool;
                default:
                    return false;
            }
        }

        private static string ToSchoolPathChoiceId(StimSchoolPathChoice choice)
        {
            switch (choice)
            {
                case StimSchoolPathChoice.PublicSchool: return "public_school";
                case StimSchoolPathChoice.Homeschool: return "homeschool";
                case StimSchoolPathChoice.AcademicTrack: return "academic_track";
                case StimSchoolPathChoice.VocationalTrack: return "vocational_track";
                case StimSchoolPathChoice.LeaveSchool: return "left_school";
                default: return choice.ToString().ToLowerInvariant();
            }
        }

        private static string BuildEducationMilestoneSummary(StimGameState state)
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

        public bool TryPerformActivity(StimActivityType activityType, out string summary)
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
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before choosing an activity.";
                return false;
            }

            if (!TryGetActivityRequirement(ActiveSave.state, activityType, out var activityRequirement))
            {
                summary = activityRequirement;
                if (activityType == StimActivityType.FamilyMovieCredit &&
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
                case StimActivityType.FamilyTime:
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
                case StimActivityType.FamilyMovie:
                    var movieCost = CalculateFamilyMovieCost(candidateSave.state);
                    candidateSave.state.finances.cashMinorUnits -= movieCost;
                    ApplyFamilyMovieBenefits(candidateSave.state);
                    summary = BuildFamilyMovieSummary(movieCost, false);
                    break;
                case StimActivityType.FamilyMovieCredit:
                    var creditMovieCost = CalculateFamilyMovieCost(candidateSave.state);
                    AddHouseholdCreditPurchase(candidateSave.state, creditMovieCost);
                    ApplyFamilyMovieBenefits(candidateSave.state);
                    candidateSave.state.character.happiness = ClampStat(
                        candidateSave.state.character.happiness + 1);
                    summary = BuildFamilyMovieSummary(creditMovieCost, true) +
                              $" · APR {candidateSave.state.finances.householdCreditAprBasisPoints / 100m:0.00}%" +
                              " · Credit approved · Happiness +1";
                    break;
                case StimActivityType.Explore:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 2);
                    ApplySkillXp(candidateSave.state.skills, "curiosity", 8);
                    summary = "Explored something new · Happiness +2 · Curiosity XP +8";
                    break;
                case StimActivityType.AttendSchool:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 1);
                    ApplySkillXp(candidateSave.state.skills, "learning", 8);
                    summary = "Attended school · Smarts +1 · Learning XP +8";
                    break;
                case StimActivityType.JoinClub:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 1);
                    ApplySkillXp(candidateSave.state.skills, "social", 10);
                    summary = "Joined a school club · Happiness +1 · Social XP +10";
                    break;
                case StimActivityType.WorkShift:
                    candidateSave.state.career.careerProgress = ClampStat(candidateSave.state.career.careerProgress + 4);
                    summary = "Completed a work shift · Career progress +4";
                    break;
                case StimActivityType.Overtime:
                    var overtimePay = CalculateHourlyRateMinorUnits(candidateSave.state.career.annualSalaryMinorUnits) * 4;
                    candidateSave.state.finances.cashMinorUnits += overtimePay;
                    candidateSave.state.character.health = ClampStat(candidateSave.state.character.health - 2);
                    candidateSave.state.career.careerProgress = ClampStat(candidateSave.state.career.careerProgress + 6);
                    summary = $"Worked overtime · Cash +{FormatPreciseMoney(overtimePay)} · Career progress +6 · Health −2";
                    break;
                case StimActivityType.Training:
                    candidateSave.state.character.smarts = ClampStat(candidateSave.state.character.smarts + 1);
                    ApplySkillXp(candidateSave.state.skills, "professional", 10);
                    summary = "Completed professional training · Smarts +1 · Professional XP +10";
                    break;
                case StimActivityType.Socialize:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 2);
                    ApplySkillXp(candidateSave.state.skills, "social", 8);
                    summary = "Socialized · Happiness +2 · Social XP +8";
                    break;
                case StimActivityType.Hobby:
                    candidateSave.state.character.happiness = ClampStat(candidateSave.state.character.happiness + 3);
                    ApplySkillXp(candidateSave.state.skills, "hobby", 8);
                    summary = "Enjoyed a hobby · Happiness +3 · Hobby XP +8";
                    break;
                case StimActivityType.Checkup:
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

        public bool TryPerformRelationshipInteraction(
            string relationshipId,
            StimRelationshipInteractionType interactionType,
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
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before interacting.";
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
            if (interactionType == StimRelationshipInteractionType.Reconcile && relationship.relationshipType != "rival")
            {
                summary = "Reconciliation is available after this relationship becomes a rivalry.";
                return false;
            }
            if (interactionType == StimRelationshipInteractionType.Compete && relationship.relationshipType == "parent")
            {
                summary = "Competition is available with peers and rivals.";
                return false;
            }
            if (interactionType == StimRelationshipInteractionType.AskOnDate &&
                ((relationship.relationshipType != "friend" && relationship.relationshipType != "best_friend") ||
                 relationship.value < 60))
            {
                summary = "Dating requires an adult friendship with at least 60 relationship strength.";
                return false;
            }
            if (interactionType == StimRelationshipInteractionType.Commit &&
                (relationship.relationshipType != "dating" || relationship.value < 75))
            {
                summary = "Commitment requires a dating relationship with at least 75 strength.";
                return false;
            }
            if (interactionType == StimRelationshipInteractionType.BreakUp &&
                relationship.relationshipType != "dating" && relationship.relationshipType != "partner")
            {
                summary = "There is no active romantic relationship to end.";
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
                case StimRelationshipInteractionType.Talk:
                    relationshipDelta = 2;
                    happinessDelta = 1;
                    smartsDelta = 0;
                    break;
                case StimRelationshipInteractionType.PlayTogether:
                    relationshipDelta = 4;
                    happinessDelta = 2;
                    smartsDelta = 0;
                    break;
                case StimRelationshipInteractionType.AskForHelp:
                    relationshipDelta = 3;
                    happinessDelta = 0;
                    smartsDelta = 1;
                    break;
                case StimRelationshipInteractionType.SpendTime:
                    relationshipDelta = 3;
                    happinessDelta = 2;
                    smartsDelta = 0;
                    break;
                case StimRelationshipInteractionType.Argue:
                    relationshipDelta = -4;
                    happinessDelta = -1;
                    smartsDelta = 0;
                    break;
                case StimRelationshipInteractionType.Compete:
                    relationshipDelta = -2;
                    happinessDelta = 0;
                    smartsDelta = 1;
                    break;
                case StimRelationshipInteractionType.Reconcile:
                    relationshipDelta = 6;
                    happinessDelta = 1;
                    smartsDelta = 0;
                    break;
                case StimRelationshipInteractionType.AskOnDate:
                    relationshipDelta = 5;
                    happinessDelta = 3;
                    smartsDelta = 0;
                    break;
                case StimRelationshipInteractionType.Commit:
                    relationshipDelta = 5;
                    happinessDelta = 4;
                    smartsDelta = 0;
                    break;
                case StimRelationshipInteractionType.BreakUp:
                    relationshipDelta = -20;
                    happinessDelta = -5;
                    smartsDelta = 0;
                    break;
                default:
                    summary = $"Relationship interaction {interactionType} is not supported.";
                    return false;
            }

            candidateRelationship.value = ClampStat(candidateRelationship.value + relationshipDelta);
            candidateRelationship.monthsSinceInteraction = 0;
            UpdateRelationshipStage(candidateRelationship, interactionType == StimRelationshipInteractionType.Reconcile);
            if (interactionType == StimRelationshipInteractionType.AskOnDate)
                candidateRelationship.relationshipType = "dating";
            else if (interactionType == StimRelationshipInteractionType.Commit)
                candidateRelationship.relationshipType = "partner";
            else if (interactionType == StimRelationshipInteractionType.BreakUp)
                candidateRelationship.relationshipType = "ex_partner";
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
            AddLifeFeedEntry(candidateSave, "relationship", summary);

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

        public static bool IsRelationshipInteractionAgeAppropriate(
            StimRelationshipInteractionType interactionType,
            int age)
        {
            switch (interactionType)
            {
                case StimRelationshipInteractionType.PlayTogether: return age <= 12;
                case StimRelationshipInteractionType.AskForHelp: return age <= 17;
                case StimRelationshipInteractionType.Argue: return age >= 8;
                case StimRelationshipInteractionType.Compete: return age >= 8;
                case StimRelationshipInteractionType.Reconcile: return age >= 10;
                case StimRelationshipInteractionType.AskOnDate:
                case StimRelationshipInteractionType.BreakUp: return age >= 18;
                case StimRelationshipInteractionType.Commit: return age >= 21;
                case StimRelationshipInteractionType.Talk:
                case StimRelationshipInteractionType.SpendTime:
                    return true;
                default:
                    return false;
            }
        }

        private static string FormatSignedValue(int value)
        {
            return $"{(value >= 0 ? "+" : "−")}{Math.Abs(value)}";
        }

        public bool TryPerformEducationAction(StimEducationActionType actionType, out string summary)
        {
            var request = ActiveSave == null
                ? new StimActionRequest(StimEducationActionService.GetActionId(actionType), string.Empty)
                : new StimActionRequest(
                    StimEducationActionService.GetActionId(actionType),
                    $"{ActiveSave.lifeId}:education:{ActiveSave.state.character.age}:" +
                    $"{ActiveSave.state.calendar.monthOfYear}:{actionType}");
            return TryPerformEducationAction(actionType, request, out summary);
        }

        public bool TryPerformEducationAction(
            StimEducationActionType actionType,
            StimActionRequest request,
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

        public IReadOnlyList<StimActionDefinition> GetEducationActionDefinitions()
        {
            return StimEducationActionService.GetDefinitions(ActiveSave?.state);
        }

        public bool TryChooseStudyTrack(StimStudyTrack track, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate => educationActionService.ChooseStudyTrack(candidate, track),
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public IReadOnlyList<StimActionDefinition> GetStudySessionDefinitions()
        {
            return StimEducationActionService.GetStudySessionDefinitions(ActiveSave?.state);
        }

        public bool TryPerformStudySession(StimStudyDifficulty difficulty, out string summary)
        {
            var succeeded = transactionRunner.TryExecute(
                ActiveSave,
                candidate =>
                {
                    var result = educationActionService.ApplyStudySession(candidate, difficulty);
                    if (result.Succeeded) EvaluateAchievements(candidate);
                    return result;
                },
                out var committedSave,
                out summary);
            if (succeeded) ActiveSave = committedSave;
            return succeeded;
        }

        public bool TryStartAction(
            StimActionDefinition definition,
            StimActionRequest request,
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
                        ? StimTransactionMutationResult.Success($"{count} action(s) are ready to claim.")
                        : StimTransactionMutationResult.Failure("No action progress changed.");
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

        public static bool TryGetEducationActionRequirement(
            StimGameState state,
            StimEducationActionType actionType,
            out string requirement)
        {
            return StimEducationActionService.TryGetRequirement(state, actionType, out requirement);
        }

        public static int GetSkillExperience(List<StimSkillState> skills, string skillId)
        {
            return StimEducationActionService.GetSkillExperience(skills, skillId);
        }

        public static int GetSkillLevel(int experience)
        {
            return StimEducationActionService.GetSkillLevel(experience);
        }

        public static int GetExperienceForSkillLevel(int level)
        {
            return StimEducationActionService.GetExperienceForSkillLevel(level);
        }

        public bool TryPerformCareerAction(StimCareerActionType actionType, out string summary)
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
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before taking a career action.";
                return false;
            }

            NormalizeProgressCollections(ActiveSave.state);
            if (!TryGetCareerActionRequirement(ActiveSave.state, actionType, out var requirement))
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
            candidateSave.state.career ??= new StimCareerState();
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");
            switch (actionType)
            {
                case StimCareerActionType.Apply:
                    AddOrRefreshStatus(candidateSave.state.statuses, "career_interview_ready", 2);
                    summary = "Application submitted · Interview unlocked next month";
                    break;
                case StimCareerActionType.Interview:
                    RemoveStatus(candidateSave.state.statuses, "career_interview_ready");
                    candidateSave.state.career.employerId = "stim_financial_group";
                    candidateSave.state.career.roleTitle = "Junior Associate";
                    candidateSave.state.career.annualSalaryMinorUnits = 4000000;
                    candidateSave.state.career.careerProgress = 0;
                    summary = "Interview complete · Hired as Junior Associate · Salary +$40,000";
                    break;
                case StimCareerActionType.WorkHard:
                    candidateSave.state.career.careerProgress = ClampStat(
                        candidateSave.state.career.careerProgress + 10);
                    candidateSave.state.character.happiness = ClampStat(
                        candidateSave.state.character.happiness - 1);
                    summary = "Worked hard · Career +10 · Happiness −1";
                    break;
                case StimCareerActionType.AskForPromotion:
                    ApplyPromotion(candidateSave.state.career, out var previousRole, out var newRole, out var salaryDelta);
                    summary = $"Promoted from {previousRole} to {newRole} · Salary +{FormatMoney(salaryDelta)}";
                    break;
                case StimCareerActionType.Quit:
                    var formerRole = candidateSave.state.career.roleTitle;
                    candidateSave.state.career = new StimCareerState();
                    summary = $"Quit {formerRole} · Salary −{FormatMoney(ActiveSave.state.career.annualSalaryMinorUnits)}";
                    break;
                case StimCareerActionType.Retire:
                    var retirementRole = candidateSave.state.career.roleTitle;
                    candidateSave.state.career = new StimCareerState { roleTitle = "Retired" };
                    FinalizeLife(candidateSave.state.character, "retired", "retirement");
                    summary = $"Retired from {retirementRole} · Salary −{FormatMoney(ActiveSave.state.career.annualSalaryMinorUnits)}";
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
                summary = $"Resolve pending event {ActiveSave.state.pendingEventId} before working a paid hour.";
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

        public static long CalculateHourlyRateMinorUnits(long annualSalaryMinorUnits)
        {
            if (annualSalaryMinorUnits <= 0) return 0;
            return (long)Math.Round(annualSalaryMinorUnits / 2080m, MidpointRounding.AwayFromZero);
        }

        public static bool TryGetCareerActionRequirement(
            StimGameState state,
            StimCareerActionType actionType,
            out string requirement)
        {
            if (state?.character == null || state.character.age < 18)
            {
                requirement = "Career actions unlock at age 18.";
                return false;
            }
            var career = state.career ?? new StimCareerState();
            var retired = career.roleTitle == "Retired";
            var employed = !string.IsNullOrEmpty(career.roleTitle) && !retired;
            var interviewReady = state.statuses?.Exists(status => status.statusId == "career_interview_ready") == true;
            switch (actionType)
            {
                case StimCareerActionType.Apply:
                    requirement = retired ? "This career ended at retirement." :
                        employed ? "Quit your current role before applying." :
                        interviewReady ? "Your interview is already ready." : string.Empty;
                    return !retired && !employed && !interviewReady;
                case StimCareerActionType.Interview:
                    requirement = retired ? "This career ended at retirement." :
                        interviewReady ? string.Empty : "Submit an application first.";
                    return !retired && !employed && interviewReady;
                case StimCareerActionType.WorkHard:
                    requirement = employed ? string.Empty : "Get hired before working toward promotion.";
                    return employed;
                case StimCareerActionType.AskForPromotion:
                    if (!employed)
                    {
                        requirement = "Get hired before requesting promotion.";
                        return false;
                    }
                    if (!TryGetNextCareerStep(career.roleTitle, out _, out _, out var progressRequired))
                    {
                        requirement = "You have reached the top of this career ladder.";
                        return false;
                    }
                    requirement = career.careerProgress >= progressRequired
                        ? string.Empty
                        : $"Requires {progressRequired} career progress.";
                    return career.careerProgress >= progressRequired;
                case StimCareerActionType.Quit:
                    requirement = employed ? string.Empty : "You do not have a role to quit.";
                    return employed;
                case StimCareerActionType.Retire:
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
                case "Junior Associate": nextRole = "Associate"; nextSalaryMinorUnits = 5500000; progressRequired = 25; return true;
                case "Associate": nextRole = "Senior Associate"; nextSalaryMinorUnits = 7500000; progressRequired = 50; return true;
                case "Senior Associate": nextRole = "Manager"; nextSalaryMinorUnits = 10000000; progressRequired = 75; return true;
                default: nextRole = null; nextSalaryMinorUnits = 0; progressRequired = 0; return false;
            }
        }

        private static void ApplyPromotion(
            StimCareerState career,
            out string previousRole,
            out string newRole,
            out long salaryDelta)
        {
            previousRole = career.roleTitle;
            TryGetNextCareerStep(previousRole, out newRole, out var nextSalary, out _);
            salaryDelta = nextSalary - career.annualSalaryMinorUnits;
            career.roleTitle = newRole;
            career.annualSalaryMinorUnits = nextSalary;
            career.careerProgress = 0;
        }

        private static void RemoveStatus(List<StimStatusState> statuses, string statusId)
        {
            statuses.RemoveAll(status => status != null && status.statusId == statusId);
        }

        private static bool IsLifeEnded(StimCharacterState character)
        {
            return character != null && character.lifeStatus != "active";
        }

        private static void FinalizeLife(StimCharacterState character, string status, string reason)
        {
            character.lifeStatus = status;
            character.endingReason = reason;
            character.endedAtAge = character.age;
        }

        public static bool IsActivityAgeAppropriate(StimActivityType activityType, int age)
        {
            switch (activityType)
            {
                case StimActivityType.Play: return age <= 12;
                case StimActivityType.Study: return age >= 5;
                case StimActivityType.Workout: return age >= 13;
                case StimActivityType.Rest: return true;
                case StimActivityType.FamilyTime: return true;
                case StimActivityType.FamilyMovie: return true;
                case StimActivityType.FamilyMovieCredit: return age >= 18;
                case StimActivityType.Explore: return age < 18;
                case StimActivityType.AttendSchool: return age >= 6 && age < 18;
                case StimActivityType.JoinClub: return age >= 10 && age < 18;
                case StimActivityType.WorkShift:
                case StimActivityType.Overtime:
                case StimActivityType.Training: return age >= 18 && age < 65;
                case StimActivityType.Socialize: return age >= 10;
                case StimActivityType.Hobby: return age >= 65;
                case StimActivityType.Checkup: return age >= 50;
                default: return false;
            }
        }

        private static bool IsHouseholdRelationship(StimRelationshipState relationship)
        {
            if (relationship == null) return false;
            return relationship.relationshipType == "parent" || relationship.relationshipType == "child" ||
                   relationship.relationshipType == "partner" || relationship.relationshipType == "engaged" ||
                   relationship.relationshipType == "married";
        }

        public static long CalculateFamilyMovieCost(StimGameState state)
        {
            const long fixedTicketPriceMinorUnits = 1500;
            var attendees = 1 + (state?.relationships?.FindAll(IsHouseholdRelationship).Count ?? 0);
            return fixedTicketPriceMinorUnits * attendees;
        }

        public static long CalculateHouseholdCreditLimit(StimGameState state)
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

        public static int CalculateHouseholdCreditAprBasisPoints(StimGameState state)
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

        private static long AccrueHouseholdCreditInterest(StimFinancesState finances)
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

        private static void ApplyFamilyMovieBenefits(StimGameState state)
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
            StimGameState state,
            StimActivityType activityType,
            out string requirement)
        {
            if (state?.character == null || !IsActivityAgeAppropriate(activityType, state.character.age))
            {
                requirement = $"{ToDisplayName(activityType.ToString())} is not available at age {state?.character?.age ?? 0}.";
                return false;
            }
            var employed = !string.IsNullOrEmpty(state.career?.roleTitle) && state.career.roleTitle != "Retired";
            if (activityType == StimActivityType.FamilyMovie)
            {
                var cost = CalculateFamilyMovieCost(state);
                if (state.finances == null || state.finances.cashMinorUnits < cost)
                {
                    requirement = $"Family movie night costs {FormatMoney(cost)}. Add cash or use the credit option.";
                    return false;
                }
            }
            if (activityType == StimActivityType.FamilyMovieCredit)
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
            if ((activityType == StimActivityType.WorkShift || activityType == StimActivityType.Overtime) && !employed)
            {
                requirement = "Get an active job first.";
                return false;
            }
            if ((activityType == StimActivityType.AttendSchool || activityType == StimActivityType.JoinClub) &&
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

        private long ApplyResolution(StimSaveEnvelope save, StimChoiceResolution resolution)
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
                    authoredCashImpact += (long)Math.Round(effect.value);
                else
                    ApplyEffect(save, effect);
            }

            ApplyIdentityChoice(save.state.character, resolution.eventId, resolution.choiceId);
            var spouseIncomeBefore = save.state.finances.spouseAnnualIncomeMinorUnits;
            var cashBeforeRelationshipTransition = save.state.finances.cashMinorUnits;
            var debtBeforeRelationshipTransition = save.state.finances.debtMinorUnits;
            ApplyRomanceChoice(save.state, resolution.eventId, resolution.choiceId);
            if (save.state.finances.spouseAnnualIncomeMinorUnits > spouseIncomeBefore)
            {
                AddLifeFeedEntry(save, "money",
                    $"Combined household finances · Spouse income {FormatMoney(save.state.finances.spouseAnnualIncomeMinorUnits)}/year" +
                    $" · Savings {FormatSignedMoney(save.state.finances.cashMinorUnits - cashBeforeRelationshipTransition)}" +
                    $" · Debt {FormatSignedMoney(save.state.finances.debtMinorUnits - debtBeforeRelationshipTransition)}.");
            }
            var financialImpact = authoredCashImpact + CalculateEventFinancialImpact(
                resolution.eventId, resolution.choiceId, save.state);

            save.state.eventHistory.Add(new StimEventHistoryEntry
            {
                eventId = resolution.eventId,
                choiceId = resolution.choiceId,
                outcomeId = resolution.outcome.id,
                age = save.state.character.age,
                revision = save.revision,
                timestampUtc = save.updatedAtUtc
            });
            AddLifeFeedEntry(save, "event", BuildOutcomeFeedText(resolution.outcome));
            if (financialImpact != 0)
            {
                AddLifeFeedEntry(save, "money",
                    $"{ToDisplayName(resolution.eventId)} · Financial impact {FormatSignedMoney(financialImpact)}.");
            }

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
            return financialImpact;
        }

        private static bool TryApplyFinancialImpact(
            StimGameState state,
            long impact,
            StimPaymentMethod paymentMethod,
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
            if (paymentMethod == StimPaymentMethod.Cash)
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

        private static void AddHouseholdCreditPurchase(StimGameState state, long cost)
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
            StimGameState state)
        {
            if (state?.finances == null) return 0;
            var salary = Math.Max(0L, state.career?.annualSalaryMinorUnits ?? 0L);
            var progress = Math.Max(0, Math.Min(100, state.career?.careerProgress ?? 0));
            var relationship = state.relationships?.Find(candidate =>
                candidate != null && candidate.relationshipId == "school_peer_primary");
            var relationshipValue = relationship?.value ?? 0;

            if (eventId == RepresentativeStimEvents.PromInvitationId)
            {
                if (choiceId == "attend_prom_together")
                    return -(7500L + salary / 1000L + progress * 50L);
                if (choiceId == "attend_prom_as_friends")
                    return -(4000L + salary / 2000L);
            }
            else if (eventId == RepresentativeStimEvents.ProposalId)
            {
                if (choiceId == "propose_marriage")
                    return -Math.Max(20000L, Math.Min(500000L, salary / 100L));
            }
            else if (eventId == RepresentativeStimEvents.WeddingId)
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
            else if (eventId == RepresentativeStimEvents.MarriageCrossroadsId)
            {
                if (choiceId == "seek_counseling")
                    return -Math.Max(10000L, salary / 500L);
                if (choiceId == "divorce")
                    return -(state.finances.cashMinorUnits / 4L + Math.Max(50000L, salary / 50L));
            }
            return 0;
        }

        public static long CalculateChoicePotentialCost(
            StimEvent evt,
            Choice choice,
            StimGameState state)
        {
            if (evt == null || choice == null) return 0;
            var dynamicImpact = CalculateEventFinancialImpact(evt.id, choice.id, state);
            var largestCost = dynamicImpact < 0 ? -dynamicImpact : 0;
            foreach (var outcome in choice.outcomes)
            {
                if (outcome?.effects == null) continue;
                long authoredCashImpact = 0;
                foreach (var effect in outcome.effects)
                {
                    if (effect?.type == EffectType.CashDelta)
                        authoredCashImpact += (long)Math.Round(effect.value);
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
            StimCharacterState character,
            string eventId,
            string choiceId)
        {
            if (character == null) return;
            if (eventId == RepresentativeStimEvents.ComingOfAgeGenderId)
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
            else if (eventId == RepresentativeStimEvents.ComingOfAgeOrientationId)
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

        private static void ApplyRomanceChoice(StimGameState state, string eventId, string choiceId)
        {
            if (state?.relationships == null) return;
            var relationship = state.relationships.Find(candidate =>
                candidate != null && candidate.relationshipId == "school_peer_primary");
            if (relationship == null) return;
            if (eventId == RepresentativeStimEvents.PromInvitationId)
                relationship.relationshipType = choiceId == "attend_prom_together" ? "dating" : "friend";
            else if (eventId == RepresentativeStimEvents.ProposalId)
            {
                if (choiceId == "propose_marriage") relationship.relationshipType = "engaged";
                else if (choiceId == "end_partnership") relationship.relationshipType = "ex_partner";
            }
            else if (eventId == RepresentativeStimEvents.WeddingId)
            {
                if (choiceId == "get_married")
                {
                    relationship.relationshipType = "married";
                    MergeSpouseFinances(state, relationship);
                }
                else if (choiceId == "call_off_wedding") relationship.relationshipType = "ex_partner";
            }
            else if (eventId == RepresentativeStimEvents.MarriageCrossroadsId &&
                     (choiceId == "separate" || choiceId == "divorce"))
            {
                relationship.relationshipType = "ex_partner";
                state.finances.spouseAnnualIncomeMinorUnits = 0;
            }
        }

        private static void MergeSpouseFinances(StimGameState state, StimRelationshipState spouse)
        {
            if (state?.finances == null || spouse == null || spouse.financesMerged) return;
            EnsureNpcFinancialProfile(spouse);
            state.finances.cashMinorUnits += spouse.npcCashMinorUnits;
            state.finances.debtMinorUnits += spouse.npcDebtMinorUnits;
            state.finances.spouseAnnualIncomeMinorUnits = spouse.npcAnnualIncomeMinorUnits;
            spouse.financesMerged = true;
        }

        public static void EnsureNpcFinancialProfile(StimRelationshipState npc)
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
            relationship.monthsSinceInteraction = 0;
            UpdateRelationshipStage(relationship, false);
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

        private static void AdvanceRelationships(StimSaveEnvelope save)
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

        private static void UpdateRelationshipStage(StimRelationshipState relationship, bool reconciled)
        {
            if (relationship == null || relationship.relationshipType == "parent" || relationship.relationshipType == "child")
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

        private static void NormalizeProgressCollections(StimGameState state)
        {
            if (state == null)
            {
                return;
            }
            state.skills ??= new List<StimSkillState>();
            state.relationships ??= new List<StimRelationshipState>();
            state.statuses ??= new List<StimStatusState>();
            state.achievements ??= new List<StimAchievementState>();
            state.lifeDecisions ??= new List<StimLifeDecisionState>();
            state.household ??= new StimHouseholdState();
            state.lifeFeed ??= new List<StimLifeFeedEntry>();
            state.scheduledEvents ??= new List<StimScheduledEventRecord>();
        }

        private static void EvaluateAchievements(StimSaveEnvelope save)
        {
            NormalizeProgressCollections(save.state);
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

        private static void TryUnlockAchievement(StimSaveEnvelope save, string achievementId, bool condition)
        {
            if (!condition || save.state.achievements.Exists(
                    achievement => achievement != null && achievement.achievementId == achievementId))
            {
                return;
            }
            save.state.achievements.Add(new StimAchievementState
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
            var scheduled = SelectScheduledEvent(save, completedYear);
            if (scheduled != null)
            {
                return scheduled;
            }

            if (completedYear && save.state.character.age == 16 &&
                save.state.character.genderIdentity == "undiscovered" &&
                eventCatalog.TryGetEvent(RepresentativeStimEvents.ComingOfAgeGenderId, out var identityEvent) &&
                IsEligible(identityEvent, save))
            {
                return identityEvent;
            }

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

        private StimEvent SelectScheduledEvent(StimSaveEnvelope save, bool completedYear)
        {
            if (!completedYear || save.state.scheduledEvents == null) return null;
            var age = save.state.character.age;
            for (var index = save.state.scheduledEvents.Count - 1; index >= 0; index--)
            {
                var record = save.state.scheduledEvents[index];
                if (record == null || age > record.latestTriggerAge)
                {
                    save.state.scheduledEvents.RemoveAt(index);
                    continue;
                }
                if (age < record.earliestTriggerAge) continue;
                if (!eventCatalog.TryGetEvent(record.eventId, out var evt) || !IsEligible(evt, save))
                {
                    if (age >= record.latestTriggerAge) save.state.scheduledEvents.RemoveAt(index);
                    continue;
                }

                var chance = Math.Max(0f, Math.Min(1f, record.chance));
                var roll = StableUnit(save.rng.seed, save.rng.step + 7919 + index);
                if (roll > chance)
                {
                    if (age >= record.latestTriggerAge) save.state.scheduledEvents.RemoveAt(index);
                    continue;
                }

                save.state.scheduledEvents.RemoveAt(index);
                return evt;
            }
            return null;
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
            if (!MeetsAuthoredRequirements(evt.requirementsJson, save.state))
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
        }

        private static bool MeetsAuthoredRequirements(string json, StimGameState state)
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

        private static string FormatPreciseMoney(long minorUnits)
        {
            return (minorUnits / 100m).ToString("C2");
        }

        private static string FormatSignedMoney(long minorUnits)
        {
            return $"{(minorUnits >= 0 ? "+" : "-")}{FormatMoney(Math.Abs(minorUnits))}";
        }

    }
}
