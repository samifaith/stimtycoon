using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace StimTycoon.Saves
{
    /// <summary>
    /// Locked save schema v1 for Phase 0.
    /// </summary>
    public static class SaveSchema
    {
        public const int SupportedSaveFormatVersion = 1;
        public const int SupportedMinimumReaderVersion = 1;
        public const int MinCoreStatValue = 0;
        public const int MaxCoreStatValue = 100;
    }

    [Serializable]
    public class SaveEnvelope
    {
        public int saveFormatVersion = SaveSchema.SupportedSaveFormatVersion;
        public int minimumReaderVersion = SaveSchema.SupportedMinimumReaderVersion;
        public string gameBuildVersion;
        public string contentVersion;
        public string saveId;
        public string playerAccountId;
        public string lifeId;
        public string createdAtUtc;
        public string updatedAtUtc;
        public int revision;
        public string deviceIdHash;
        public RngState rng = new RngState();
        public SaveIntegrity integrity = new SaveIntegrity();
        public GameState state = new GameState();
    }

    [Serializable]
    public class RngState
    {
        public int seed;
        public int step;
    }

    [Serializable]
    public class SaveIntegrity
    {
        public string payloadHash;
        public string previousRevisionHash;
    }

    [Serializable]
    public class GameState
    {
        public CharacterState character = new CharacterState();
        public CalendarState calendar = new CalendarState();
        public AnnualReviewState annualReview = new AnnualReviewState();
        public List<AnnualReviewHistoryState> annualReviewHistory = new List<AnnualReviewHistoryState>();
        public FinancesState finances = new FinancesState();
        public List<MoneyTransactionState> moneyTransactions = new List<MoneyTransactionState>();
        public CareerState career = new CareerState();
        public BusinessState business = new BusinessState();
        public BusinessPortfolioState businessPortfolio = new BusinessPortfolioState();
        public PropertyPortfolioState propertyPortfolio = new PropertyPortfolioState();
        public EducationState education = new EducationState();
        public HouseholdState household = new HouseholdState();
        public FamilyState family = new FamilyState();
        public HomeState home = new HomeState();
        public List<SkillState> skills = new List<SkillState>();
        public List<RelationshipState> relationships = new List<RelationshipState>();
        public List<StatusState> statuses = new List<StatusState>();
        public List<AchievementState> achievements = new List<AchievementState>();
        public List<GoalState> goals = new List<GoalState>();
        public OrientationState orientation = new OrientationState();
        public UiWorkflowState uiWorkflow = new UiWorkflowState();
        public List<TransitionPresentationState> transitionPresentations = new List<TransitionPresentationState>();
        public List<LifeDecisionState> lifeDecisions = new List<LifeDecisionState>();
        public List<ActionProgressState> actionProgress = new List<ActionProgressState>();
        public MatchSessionState matchSession = new MatchSessionState();
        public List<LifeFeedEntry> lifeFeed = new List<LifeFeedEntry>();
        public HistoryArchiveState historyArchive = new HistoryArchiveState();
        public string pendingEventId;
        public List<EventHistoryEntry> eventHistory = new List<EventHistoryEntry>();
        public List<ScheduledEventRecord> scheduledEvents = new List<ScheduledEventRecord>();
    }

    [Serializable]
    public class UiWorkflowState
    {
        public int queuedYearMonthsRemaining;
        public bool queuedYearCompletionPending;
        public string queuedYearCompletionSummary;
        public string pendingStudyDifficulty;
        public string pendingStudyActionId;
        public string activeDestination;
        public string selectedTabId;
        public string selectedEntityId;
        public float activeScrollX;
        public float activeScrollY;
    }

    [Serializable]
    public class CharacterState
    {
        public string firstName;
        public string lastName;
        public string pronouns;
        public string genderIdentity = "undiscovered";
        public string sexualOrientation = "undiscovered";
        public string country;
        public string backgroundId;
        public string avatarId;
        public int appearanceSeed;
        public string lifeStage = "infant";
        public string lifeStatus = "active";
        public string endingReason;
        public int endedAtAge = -1;
        public int age;
        public int health;
        public int happiness;
        public int smarts;
        public int looks = 50;
        public int luck = 50;
    }

    [Serializable]
    public class CalendarState
    {
        public int monthOfYear = 1;
        public int quietMonthsSinceEvent;
    }

    [Serializable]
    public class AnnualReviewState
    {
        public int cycleStartAge = -1;
        public int monthsAccumulated;
        public long startingCashMinorUnits;
        public long startingSavingsMinorUnits;
        public long startingIndexFundMinorUnits;
        public long startingDebtMinorUnits;
        public int startingHealth;
        public int startingHappiness;
        public int startingSmarts;
        public int startingCareerProgress;
        public int startingQualificationExperience;
        public int startingRelationshipValue;
        public int startingSkillExperience;
        public int startingLifeFeedCount;
        public int completedAtAge = -1;
        public long cashDeltaMinorUnits;
        public long savingsDeltaMinorUnits;
        public long indexFundDeltaMinorUnits;
        public long debtDeltaMinorUnits;
        public int healthDelta;
        public int happinessDelta;
        public int smartsDelta;
        public int careerProgressDelta;
        public int qualificationExperienceDelta;
        public int relationshipValueDelta;
        public int skillExperienceDelta;
        public List<string> majorOutcomeSummaries = new List<string>();
        public int rewardedAtAge = -1;
    }

    [Serializable]
    public class AnnualReviewHistoryState
    {
        public int completedAtAge;
        public string rewardChoiceId;
        public string summary;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class FinancesState
    {
        public long cashMinorUnits;
        public long savingsMinorUnits;
        public long indexFundMinorUnits;
        public long indexFundContributionsMinorUnits;
        public int savingsApyBasisPoints = 350;
        public long lastGrossIncomeMinorUnits;
        public long lastTaxesMinorUnits;
        public long lastExpensesMinorUnits;
        public long lastCreditInterestMinorUnits;
        public long lastSavingsInterestMinorUnits;
        public long lastNetCashFlowMinorUnits;
        public long debtMinorUnits;
        public long monthlyLivingExpensesMinorUnits;
        public int taxRateBasisPoints;
        public long spouseAnnualIncomeMinorUnits;
        public long householdCreditBalanceMinorUnits;
        public int householdCreditAprBasisPoints;
    }

    [Serializable]
    public class MoneyTransactionState
    {
        public string transactionId;
        public string type;
        public long amountMinorUnits;
        public long cashBalanceMinorUnits;
        public long savingsBalanceMinorUnits;
        public int age;
        public int monthOfYear;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class HouseholdState
    {
        public int happiness = 50;
        public int cohesion = 50;
    }

    [Serializable]
    public class FamilyState
    {
        public string planningPreference = "undiscussed";
        public string planningPartnerId;
        public bool partnerConsent;
        public string pendingPath;
        public int monthsUntilResolution;
        public List<ChildState> children = new List<ChildState>();
    }

    [Serializable]
    public class ChildState
    {
        public string childId;
        public string displayName;
        public string path;
        public string parentRelationshipId;
        public int joinedAtParentAge;
        public int birthMonth;
        public int age;
        public int wellbeing = 60;
        public int learning;
        public int independence;
        public string custodyStatus = "household";
    }

    [Serializable]
    public class HomeState
    {
        public const int MaxInventoryItems = 20;
        public string homeId = "starter_home";
        public int condition = 80;
        public int upgradeLevel;
        public int improvementProgress;
        public int readingMaterialStock = 3;
        public int readingMaterialCapacity = 3;
        public int trainingEquipmentCondition = 100;
        public List<HomeInventoryItemState> inventory = CreateDefaultInventory();

        public static List<HomeInventoryItemState> CreateDefaultInventory()
        {
            return new List<HomeInventoryItemState>
            {
                new HomeInventoryItemState
                {
                    itemId = "starter_books", category = "book", quantity = 3,
                    capacity = 3, condition = 100, acquisitionSource = "new_life"
                },
                new HomeInventoryItemState
                {
                    itemId = "starter_training_kit", category = "equipment", quantity = 1,
                    capacity = 1, condition = 100, acquisitionSource = "new_life"
                }
            };
        }
    }

    [Serializable]
    public class HomeInventoryItemState
    {
        public string itemId;
        public string category;
        public int quantity;
        public int capacity;
        public int condition = 100;
        public string acquisitionSource;
    }

    [Serializable]
    public class CareerState
    {
        public string industryId;
        public string pendingIndustryId;
        public string employmentStatus = "unemployed";
        public int monthsUnemployed;
        public int performanceWarnings;
        public string employerId;
        public string roleTitle;
        public long annualSalaryMinorUnits;
        public int careerProgress;
    }

    [Serializable]
    public class BusinessState
    {
        public string businessId;
        public string businessType;
        public string displayName;
        public string status = "none";
        public int level;
        public int staffCount;
        public int locationLevel;
        public int actionPoints;
        public int maxActionPoints;
        public int riskEventsExperienced;
        public int operatingProgress;
        public int monthsOperating;
        public int consecutiveLossMonths;
        public long lastRevenueMinorUnits;
        public long lastExpensesMinorUnits;
        public long lastProfitMinorUnits;
        public long lifetimeProfitMinorUnits;
        public long valuationMinorUnits;
        public List<BusinessLedgerEntry> ledger = new List<BusinessLedgerEntry>();
    }

    [Serializable]
    public class BusinessLedgerEntry
    {
        public string entryId;
        public string type;
        public long amountMinorUnits;
        public long valuationMinorUnits;
        public int age;
        public int monthOfYear;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class BusinessPortfolioState
    {
        public const int MaxBusinesses = 3;
        public string selectedBusinessId;
        public List<BusinessState> businesses = new List<BusinessState>();
    }

    [Serializable]
    public class PropertyPortfolioState
    {
        public const int MaxProperties = 6;
        public const int MaxLedgerEntries = 60;
        public string selectedPropertyId;
        public List<RentalPropertyState> properties = new List<RentalPropertyState>();
        public List<PropertyLedgerEntry> ledger = new List<PropertyLedgerEntry>();
    }

    [Serializable]
    public class RentalPropertyState
    {
        public string propertyId;
        public string propertyType;
        public string displayName;
        public string status = "owned";
        public long purchasePriceMinorUnits;
        public long currentValueMinorUnits;
        public long mortgageBalanceMinorUnits;
        public long monthlyMortgagePaymentMinorUnits;
        public long monthlyRentMinorUnits;
        public long monthlyPropertyTaxMinorUnits;
        public int condition = 80;
        public bool occupied;
        public int vacancyMonths;
        public int monthsOwned;
        public long lastIncomeMinorUnits;
        public long lastExpensesMinorUnits;
        public long lifetimeNetIncomeMinorUnits;
    }

    [Serializable]
    public class PropertyLedgerEntry
    {
        public string entryId;
        public string propertyId;
        public string type;
        public long amountMinorUnits;
        public long propertyValueMinorUnits;
        public long mortgageBalanceMinorUnits;
        public int age;
        public int monthOfYear;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class EducationState
    {
        public string stage = "not_started";
        public string schoolPath;
        public string studyTrack;
        public int qualificationExperience;
        public string awaitingDecisionId;
        public bool graduatedSecondary;
    }

    [Serializable]
    public class SkillState
    {
        public string skillId;
        public int experience;
    }

    [Serializable]
    public class RelationshipState
    {
        public const int MaxEntries = 64;

        public string relationshipId;
        public string identityId;
        public string displayName;
        public string pronouns;
        public string genderIdentity;
        public string orientation;
        public string relationshipType;
        public string relationshipStage;
        public string origin;
        public string introductionContext;
        public int introducedAtAge;
        public int monthsSinceInteraction;
        public int warmth = 50;
        public bool isGeneticParent;
        public int geneticHealth;
        public int geneticLooks;
        public int geneticSmarts;
        public int npcSmarts;
        public int npcCareerLevel;
        public long npcAnnualIncomeMinorUnits;
        public long npcCashMinorUnits;
        public long npcDebtMinorUnits;
        public bool financesMerged;
        public int value = 50;
        public List<RelationshipHistoryState> relationshipHistory = new List<RelationshipHistoryState>();
    }

    [Serializable]
    public class RelationshipHistoryState
    {
        public string historyId;
        public string type;
        public string summary;
        public int age;
        public int monthOfYear;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class StatusState
    {
        public string statusId;
        public int remainingMonths;
    }

    [Serializable]
    public class AchievementState
    {
        public string achievementId;
        public int unlockedAtAge;
        public int revision;
        public string timestampUtc;
        public bool rewardClaimed;
        public int rewardClaimedRevision;
        public string rewardClaimedAtUtc;
    }

    [Serializable]
    public class GoalState
    {
        public string goalId;
        public string category;
        public string title;
        public string description;
        public string destination;
        public int progress;
        public int progressRequired = 1;
        public long rewardMinorUnits;
        public string status = "active";
        public int createdAtAge;
        public int createdAtMonth;
        public int claimedRevision;
        public string claimedAtUtc;
        public bool pinned;
    }

    [Serializable]
    public class LifeDecisionState
    {
        public string decisionId;
        public string choiceId;
        public int age;
        public int monthOfYear = 1;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class ActionProgressState
    {
        public const int MaxEntries = 32;
        public string instanceId;
        public string actionId;
        public string state = "Ready";
        public int progress;
        public int progressRequired = 1;
        public string resultSummary;
        public int revision;
        public int durationSeconds;
        public int claimWindowSeconds;
        public int remainingSeconds;
        public string startedAtUtc;
        public string completesAtUtc;
        public string pausedAtUtc;
        public string expiresAtUtc;
        public string completedAtUtc;
        public string claimedAtUtc;
    }

    [Serializable]
    public class MatchSessionState
    {
        public string activityId;
        public string instanceId;
        public string theme;
        public string state = "none";
        public int boardSeed;
        public int rngStep;
        public int width = 8;
        public int height = 8;
        public List<int> board = new List<int>();
        public int durationSeconds;
        public int remainingSeconds;
        public string startedAtUtc;
        public string completesAtUtc;
        public string pausedAtUtc;
        public string completedAtUtc;
        public int score;
        public int targetScore;
        public int rewardAmount;
        public string rewardType;
        public string rewardPreview;
        public bool rewardMultiplierAvailable;
        public bool rewardMultiplierApplied;
        public int rewardMultiplier = 1;
        public bool rewardClaimed;
        public string claimedAtUtc;
        public int claimedRevision;
        public int cooldownUntilAge;
        public int cooldownUntilMonth;
    }

    [Serializable]
    public class LifeFeedEntry
    {
        public string entryId;
        public string category;
        public string text;
        public int age;
        public int monthOfYear;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class TransitionPresentationState
    {
        public string transitionId;
        public string transitionType;
        public string title;
        public string summary;
        public int age;
        public int monthOfYear;
        public int revision;
        public string createdAtUtc;
        public bool acknowledged;
        public int acknowledgedRevision;
        public string acknowledgedAtUtc;
    }

    [Serializable]
    public class OrientationState
    {
        public string status = "not_started";
        public int completedRevision;
        public string completedAtUtc;
    }

    [Serializable]
    public class EventHistoryEntry
    {
        public string eventId;
        public string choiceId;
        public string outcomeId;
        public int age;
        public int revision;
        public string timestampUtc;
    }

    [Serializable]
    public class HistoryArchiveState
    {
        public int lifeFeedArchivedCount;
        public int eventHistoryArchivedCount;
        public List<string> majorSummaries = new List<string>();
    }

    [Serializable]
    public class ScheduledEventRecord
    {
        public const int MaxScheduledEvents = 32;

        public string eventId;
        public int earliestTriggerAge;
        public int latestTriggerAge;
        public int earliestTriggerMonth;
        public int latestTriggerMonth;
        public int priority;
        public int cooldownMonths;
        public string relationshipId;
        public float chance;
        public string sourceEventId;
        public string cancellationRule;
    }

    public class SaveValidationResult
    {
        public bool isValid = true;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    /// <summary>
    /// Validator for the locked Phase 0 save envelope.
    /// </summary>
    public static class SaveValidator
    {
        public static SaveValidationResult ValidateSave(SaveEnvelope save)
        {
            var result = new SaveValidationResult();

            if (save == null)
            {
                result.isValid = false;
                result.errors.Add("Save is null");
                return result;
            }

            if (save.saveFormatVersion != SaveSchema.SupportedSaveFormatVersion)
            {
                result.isValid = false;
                result.errors.Add($"Save format version {save.saveFormatVersion} not supported. Expected {SaveSchema.SupportedSaveFormatVersion}.");
            }

            if (save.minimumReaderVersion > SaveSchema.SupportedMinimumReaderVersion)
            {
                result.isValid = false;
                result.errors.Add($"Minimum reader version {save.minimumReaderVersion} is newer than supported {SaveSchema.SupportedMinimumReaderVersion}.");
            }

            ValidateRequiredString(result, save.saveId, "saveId");
            ValidateRequiredString(result, save.playerAccountId, "playerAccountId");
            ValidateRequiredString(result, save.lifeId, "lifeId");
            ValidateRequiredString(result, save.gameBuildVersion, "gameBuildVersion");
            ValidateRequiredString(result, save.contentVersion, "contentVersion");
            ValidateRequiredString(result, save.deviceIdHash, "deviceIdHash");

            if (save.revision < 1)
            {
                result.isValid = false;
                result.errors.Add("revision must be at least 1");
            }

            if (!TryParseUtcTimestamp(save.createdAtUtc, out var createdAtUtc))
            {
                result.isValid = false;
                result.errors.Add("createdAtUtc must be a valid UTC timestamp");
            }

            if (!TryParseUtcTimestamp(save.updatedAtUtc, out var updatedAtUtc))
            {
                result.isValid = false;
                result.errors.Add("updatedAtUtc must be a valid UTC timestamp");
            }

            if (createdAtUtc != default && updatedAtUtc != default && updatedAtUtc < createdAtUtc)
            {
                result.isValid = false;
                result.errors.Add("updatedAtUtc cannot be earlier than createdAtUtc");
            }

            if (save.rng == null)
            {
                result.isValid = false;
                result.errors.Add("rng is null");
            }
            else if (save.rng.step < 0)
            {
                result.isValid = false;
                result.errors.Add("rng.step cannot be negative");
            }

            if (save.integrity == null)
            {
                result.isValid = false;
                result.errors.Add("integrity is null");
            }
            else
            {
                ValidateRequiredString(result, save.integrity.payloadHash, "integrity.payloadHash");
            }

            if (save.state == null)
            {
                result.isValid = false;
                result.errors.Add("state is null");
                return result;
            }

            ValidateCharacterState(result, save.state.character);
            ValidateCalendarState(result, save.state.calendar);
            ValidateAnnualReviewState(result, save.state.annualReview);
            ValidateAnnualReviewHistory(result, save.state.annualReviewHistory);
            ValidateFinancesState(result, save.state.finances);
            ValidateMoneyTransactions(result, save.state.moneyTransactions);
            ValidateCareerState(result, save.state.career);
            ValidateBusinessState(result, save.state.business);
            ValidateBusinessPortfolio(result, save.state.businessPortfolio);
            ValidatePropertyPortfolio(result, save.state.propertyPortfolio);
            ValidateOrientation(result, save.state.orientation);
            ValidateTransitionPresentations(result, save.state.transitionPresentations);
            ValidateHouseholdState(result, save.state.household);
            ValidateFamilyState(result, save.state.family);
            ValidateHomeState(result, save.state.home);
            ValidateSkills(result, save.state.skills);
            ValidateRelationships(result, save.state.relationships);
            ValidateFamilyRelationshipConsistency(result, save.state.family, save.state.relationships);
            ValidateStatuses(result, save.state.statuses);
            ValidateAchievements(result, save.state.achievements);
            ValidateGoals(result, save.state.goals);
            ValidateLifeDecisions(result, save.state.lifeDecisions);
            ValidateActionProgress(result, save.state.actionProgress);
            ValidateMatchSession(result, save.state.matchSession);
            ValidateHistoryRetention(result, save.state);
            ValidateEventHistory(result, save.state.eventHistory);
            ValidateScheduledEvents(result, save.state.scheduledEvents);

            return result;
        }

        private static void ValidateHistoryRetention(SaveValidationResult result, GameState state)
        {
            if (state.lifeFeed == null)
            {
                result.isValid = false;
                result.errors.Add("state.lifeFeed is null");
            }
            else if (state.lifeFeed.Count > HistoryRetention.MaxLifeFeedEntries)
            {
                result.isValid = false;
                result.errors.Add("state.lifeFeed exceeds retention limit");
            }

            if (state.eventHistory != null &&
                state.eventHistory.Count > HistoryRetention.MaxEventHistoryEntries)
            {
                result.isValid = false;
                result.errors.Add("state.eventHistory exceeds retention limit");
            }

            if (state.historyArchive == null)
            {
                result.isValid = false;
                result.errors.Add("state.historyArchive is null");
                return;
            }
            if (state.historyArchive.lifeFeedArchivedCount < 0 ||
                state.historyArchive.eventHistoryArchivedCount < 0)
            {
                result.isValid = false;
                result.errors.Add("state.historyArchive counts cannot be negative");
            }
            if (state.historyArchive.majorSummaries == null ||
                state.historyArchive.majorSummaries.Count > HistoryRetention.MaxMajorArchiveSummaries)
            {
                result.isValid = false;
                result.errors.Add("state.historyArchive.majorSummaries is invalid");
            }
        }

        public static string GetValidationSummary(SaveValidationResult result, string saveId)
        {
            if (result.isValid && result.warnings.Count == 0)
            {
                return $"✓ {saveId} is valid";
            }

            var summary = new StringBuilder();
            summary.AppendLine($"Validation for {saveId}:");

            if (!result.isValid)
            {
                summary.AppendLine("ERRORS:");
                foreach (var error in result.errors)
                {
                    summary.AppendLine($"  ✗ {error}");
                }
            }

            if (result.warnings.Count > 0)
            {
                summary.AppendLine("WARNINGS:");
                foreach (var warning in result.warnings)
                {
                    summary.AppendLine($"  ⚠ {warning}");
                }
            }

            return summary.ToString();
        }

        private static void ValidateCharacterState(SaveValidationResult result, CharacterState character)
        {
            if (character == null)
            {
                result.isValid = false;
                result.errors.Add("state.character is null");
                return;
            }

            if (character.age < 0)
            {
                result.isValid = false;
                result.errors.Add("state.character.age cannot be negative");
            }

            if (character.lifeStatus != "active" && character.lifeStatus != "deceased" &&
                character.lifeStatus != "retired")
            {
                result.isValid = false;
                result.errors.Add("state.character.lifeStatus must be active, deceased, or retired");
            }
            if (character.lifeStatus == "active" && character.endedAtAge != -1)
            {
                result.isValid = false;
                result.errors.Add("active lives must use endedAtAge=-1");
            }
            if (character.lifeStatus != "active" &&
                (character.endedAtAge < 0 || string.IsNullOrEmpty(character.endingReason)))
            {
                result.isValid = false;
                result.errors.Add("ended lives require endedAtAge and endingReason");
            }

            ValidateStatRange(result, character.health, "state.character.health");
            ValidateStatRange(result, character.happiness, "state.character.happiness");
            ValidateStatRange(result, character.smarts, "state.character.smarts");
            ValidateStatRange(result, character.looks, "state.character.looks");
            ValidateStatRange(result, character.luck, "state.character.luck");
        }

        private static void ValidateFinancesState(SaveValidationResult result, FinancesState finances)
        {
            if (finances == null)
            {
                result.isValid = false;
                result.errors.Add("state.finances is null");
                return;
            }

            if (finances.cashMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.cashMinorUnits cannot be negative");
            }
            if (finances.savingsMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.savingsMinorUnits cannot be negative");
            }
            if (finances.indexFundMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.indexFundMinorUnits cannot be negative");
            }
            if (finances.indexFundContributionsMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.indexFundContributionsMinorUnits cannot be negative");
            }
            if (finances.savingsApyBasisPoints < 0 || finances.savingsApyBasisPoints > 1000)
            {
                result.isValid = false;
                result.errors.Add("state.finances.savingsApyBasisPoints must be within [0, 1000]");
            }
            if (finances.lastGrossIncomeMinorUnits < 0 || finances.lastTaxesMinorUnits < 0 ||
                finances.lastExpensesMinorUnits < 0 || finances.lastCreditInterestMinorUnits < 0 ||
                finances.lastSavingsInterestMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances last-month components cannot be negative");
            }

            if (finances.debtMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.debtMinorUnits cannot be negative");
            }
            if (finances.spouseAnnualIncomeMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.spouseAnnualIncomeMinorUnits cannot be negative");
            }
            if (finances.householdCreditBalanceMinorUnits < 0 ||
                finances.householdCreditBalanceMinorUnits > finances.debtMinorUnits)
            {
                result.isValid = false;
                result.errors.Add("household credit balance must be non-negative and cannot exceed total debt");
            }
            if (finances.householdCreditAprBasisPoints < 0 || finances.householdCreditAprBasisPoints > 10000)
            {
                result.isValid = false;
                result.errors.Add("household credit APR must be within [0, 10000] basis points");
            }


            if (finances.monthlyLivingExpensesMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.finances.monthlyLivingExpensesMinorUnits cannot be negative");
            }

            if (finances.taxRateBasisPoints < 0 || finances.taxRateBasisPoints > 10000)
            {
                result.isValid = false;
                result.errors.Add("state.finances.taxRateBasisPoints must be within [0, 10000]");
            }
        }

        private static void ValidateMoneyTransactions(
            SaveValidationResult result,
            List<MoneyTransactionState> transactions)
        {
            if (transactions == null)
            {
                result.isValid = false;
                result.errors.Add("state.moneyTransactions is null");
                return;
            }
            if (transactions.Count > 100)
            {
                result.isValid = false;
                result.errors.Add("state.moneyTransactions cannot contain more than 100 entries");
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < transactions.Count; index++)
            {
                var entry = transactions[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.transactionId) ||
                    !ids.Add(entry.transactionId) ||
                    (entry.type != "savings_deposit" && entry.type != "savings_withdrawal" &&
                     entry.type != "savings_interest" && entry.type != "credit_repayment" &&
                     entry.type != "index_investment" && entry.type != "index_gain" &&
                     entry.type != "index_loss") ||
                    entry.amountMinorUnits <= 0 || entry.cashBalanceMinorUnits < 0 ||
                    entry.savingsBalanceMinorUnits < 0 || entry.age < 0 ||
                    entry.monthOfYear < 1 || entry.monthOfYear > 12 || entry.revision < 1 ||
                    !TryParseUtcTimestamp(entry.timestampUtc, out _))
                {
                    result.isValid = false;
                    result.errors.Add($"state.moneyTransactions[{index}] is invalid");
                }
            }
        }

        private static void ValidateHouseholdState(SaveValidationResult result, HouseholdState household)
        {
            if (household == null)
            {
                result.isValid = false;
                result.errors.Add("state.household is null");
                return;
            }
            ValidateStatRange(result, household.happiness, "state.household.happiness");
            ValidateStatRange(result, household.cohesion, "state.household.cohesion");
        }

        private static void ValidateFamilyState(SaveValidationResult result, FamilyState family)
        {
            if (family == null)
            {
                result.isValid = false;
                result.errors.Add("state.family is null");
                return;
            }
            if (family.monthsUntilResolution < 0 || family.monthsUntilResolution > 12 ||
                (string.IsNullOrEmpty(family.pendingPath) != (family.monthsUntilResolution == 0)))
            {
                result.isValid = false;
                result.errors.Add("state.family pending path and resolution timing are inconsistent");
            }
            if (!string.IsNullOrEmpty(family.pendingPath) &&
                family.pendingPath != "pregnancy" && family.pendingPath != "adoption")
            {
                result.isValid = false;
                result.errors.Add("state.family.pendingPath must be pregnancy or adoption");
            }
            if (family.planningPreference != "undiscussed" && family.planningPreference != "open" &&
                family.planningPreference != "not_now")
            {
                result.isValid = false;
                result.errors.Add("state.family.planningPreference is invalid");
            }
            if (family.partnerConsent &&
                (family.planningPreference != "open" || string.IsNullOrWhiteSpace(family.planningPartnerId)))
            {
                result.isValid = false;
                result.errors.Add("state.family partner consent requires an open preference and planning partner");
            }
            if (family.children == null || family.children.Count > 12)
            {
                result.isValid = false;
                result.errors.Add("state.family.children must contain at most 12 records");
                return;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < family.children.Count; index++)
            {
                var child = family.children[index];
                if (child == null || string.IsNullOrWhiteSpace(child.childId) || !ids.Add(child.childId) ||
                    string.IsNullOrWhiteSpace(child.displayName) ||
                    (child.path != "pregnancy" && child.path != "adoption") ||
                    string.IsNullOrWhiteSpace(child.parentRelationshipId) || child.joinedAtParentAge < 18 ||
                    child.birthMonth < 1 || child.birthMonth > 12 || child.age < 0 ||
                    child.wellbeing < 0 || child.wellbeing > 100 || child.learning < 0 || child.learning > 100 ||
                    child.independence < 0 || child.independence > 100 ||
                    (child.custodyStatus != "household" && child.custodyStatus != "shared" &&
                     child.custodyStatus != "independent") ||
                    (child.age < 18 && child.custodyStatus == "independent") ||
                    (child.age >= 18 && child.custodyStatus != "independent"))
                {
                    result.isValid = false;
                    result.errors.Add($"state.family.children[{index}] is invalid");
                }
            }
        }

        private static void ValidateFamilyRelationshipConsistency(
            SaveValidationResult result,
            FamilyState family,
            List<RelationshipState> relationships)
        {
            if (family?.children == null || relationships == null) return;
            foreach (var child in family.children)
            {
                if (child == null || string.IsNullOrWhiteSpace(child.childId)) continue;
                var relationship = relationships.Find(candidate =>
                    candidate != null && candidate.relationshipId == child.childId);
                var expectedType = child.age >= 18 ? "adult_child" : "child";
                var expectedStage = child.age >= 18 ? "adult_child" : "dependent_child";
                if (relationship == null || relationship.relationshipType != expectedType ||
                    relationship.relationshipStage != expectedStage)
                {
                    result.isValid = false;
                    result.errors.Add($"state.family child {child.childId} requires a matching {expectedType} relationship");
                }
            }
        }

        private static void ValidateHomeState(SaveValidationResult result, HomeState home)
        {
            if (home == null)
            {
                result.isValid = false;
                result.errors.Add("state.home is null");
                return;
            }
            ValidateRequiredString(result, home.homeId, "state.home.homeId");
            ValidateStatRange(result, home.condition, "state.home.condition");
            if (home.upgradeLevel < 0 || home.upgradeLevel > 3)
            {
                result.isValid = false;
                result.errors.Add("state.home.upgradeLevel must be within [0, 3]");
            }
            if (home.improvementProgress < 0 || home.improvementProgress > 100)
            {
                result.isValid = false;
                result.errors.Add("state.home.improvementProgress must be within [0, 100]");
            }
            if (home.readingMaterialCapacity < 1 || home.readingMaterialCapacity > 20 ||
                home.readingMaterialStock < 0 || home.readingMaterialStock > home.readingMaterialCapacity)
            {
                result.isValid = false;
                result.errors.Add("state.home reading-material stock/capacity is invalid");
            }
            ValidateStatRange(result, home.trainingEquipmentCondition, "state.home.trainingEquipmentCondition");
            if (home.inventory == null || home.inventory.Count > HomeState.MaxInventoryItems)
            {
                result.isValid = false;
                result.errors.Add($"state.home.inventory must contain at most {HomeState.MaxInventoryItems} items");
                return;
            }
            var itemIds = new HashSet<string>();
            foreach (var item in home.inventory)
            {
                if (item == null)
                {
                    result.isValid = false;
                    result.errors.Add("state.home.inventory cannot contain null items");
                    continue;
                }
                ValidateRequiredString(result, item.itemId, "state.home.inventory.itemId");
                ValidateRequiredString(result, item.category, $"state.home.inventory[{item.itemId}].category");
                ValidateRequiredString(result, item.acquisitionSource, $"state.home.inventory[{item.itemId}].acquisitionSource");
                if (!string.IsNullOrWhiteSpace(item.itemId) && !itemIds.Add(item.itemId))
                {
                    result.isValid = false;
                    result.errors.Add($"state.home.inventory itemId {item.itemId} is duplicated");
                }
                if (item.capacity < 1 || item.capacity > 100 || item.quantity < 0 || item.quantity > item.capacity)
                {
                    result.isValid = false;
                    result.errors.Add($"state.home.inventory[{item.itemId}] quantity/capacity is invalid");
                }
                ValidateStatRange(result, item.condition, $"state.home.inventory[{item.itemId}].condition");
            }
        }

        private static void ValidateCalendarState(SaveValidationResult result, CalendarState calendar)
        {
            if (calendar == null)
            {
                result.isValid = false;
                result.errors.Add("state.calendar is null");
                return;
            }

            if (calendar.monthOfYear < 1 || calendar.monthOfYear > 12)
            {
                result.isValid = false;
                result.errors.Add("state.calendar.monthOfYear must be within [1, 12]");
            }


            if (calendar.quietMonthsSinceEvent < 0)
            {
                result.isValid = false;
                result.errors.Add("state.calendar.quietMonthsSinceEvent cannot be negative");
            }
        }

        private static void ValidateAnnualReviewState(SaveValidationResult result, AnnualReviewState annualReview)
        {
            if (annualReview == null)
            {
                result.isValid = false;
                result.errors.Add("state.annualReview is null");
                return;
            }
            if (annualReview.monthsAccumulated < 0 || annualReview.monthsAccumulated > 12)
            {
                result.isValid = false;
                result.errors.Add("state.annualReview.monthsAccumulated must be within [0, 12]");
            }
            if (annualReview.startingLifeFeedCount < 0 || annualReview.majorOutcomeSummaries == null ||
                annualReview.majorOutcomeSummaries.Count > 5)
            {
                result.isValid = false;
                result.errors.Add("state.annualReview outcome tracking is invalid");
            }
        }

        private static void ValidateAnnualReviewHistory(
            SaveValidationResult result,
            List<AnnualReviewHistoryState> history)
        {
            if (history == null || history.Count > 10)
            {
                result.isValid = false;
                result.errors.Add("state.annualReviewHistory must contain at most 10 entries");
                return;
            }
            var ages = new HashSet<int>();
            for (var index = 0; index < history.Count; index++)
            {
                var entry = history[index];
                if (entry == null || entry.completedAtAge < 0 || !ages.Add(entry.completedAtAge) ||
                    string.IsNullOrWhiteSpace(entry.rewardChoiceId) || string.IsNullOrWhiteSpace(entry.summary) ||
                    entry.revision < 1 || !TryParseUtcTimestamp(entry.timestampUtc, out _))
                {
                    result.isValid = false;
                    result.errors.Add($"state.annualReviewHistory[{index}] is invalid");
                }
            }
        }

        private static void ValidateCareerState(SaveValidationResult result, CareerState career)
        {
            if (career == null)
            {
                result.isValid = false;
                result.errors.Add("state.career is null");
                return;
            }

            if (career.annualSalaryMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.career.annualSalaryMinorUnits cannot be negative");
            }

            if (career.careerProgress < 0 || career.careerProgress > 100)
            {
                result.isValid = false;
                result.errors.Add("state.career.careerProgress must be within [0, 100]");
            }
            if (!string.IsNullOrEmpty(career.industryId) && career.industryId != "finance" &&
                career.industryId != "healthcare" && career.industryId != "skilled_trades")
            {
                result.isValid = false;
                result.errors.Add("state.career.industryId is invalid");
            }
            if (!string.IsNullOrEmpty(career.pendingIndustryId) && career.pendingIndustryId != "finance" &&
                career.pendingIndustryId != "healthcare" && career.pendingIndustryId != "skilled_trades")
            {
                result.isValid = false;
                result.errors.Add("state.career.pendingIndustryId is invalid");
            }
            if (career.employmentStatus != "unemployed" && career.employmentStatus != "employed" &&
                career.employmentStatus != "retired")
            {
                result.isValid = false;
                result.errors.Add("state.career.employmentStatus is invalid");
            }
            if (career.monthsUnemployed < 0 || career.performanceWarnings < 0 || career.performanceWarnings > 3)
            {
                result.isValid = false;
                result.errors.Add("state.career unemployment or warning counters are invalid");
            }
        }

        private static void ValidateBusinessState(SaveValidationResult result, BusinessState business)
        {
            if (business == null)
            {
                result.isValid = false;
                result.errors.Add("state.business is null");
                return;
            }
            if (business.status != "none" && business.status != "operating" &&
                business.status != "sold" && business.status != "failed")
            {
                result.isValid = false;
                result.errors.Add("state.business.status is invalid");
            }
            if (business.level < 0 || business.level > 3 || business.operatingProgress < 0 ||
                business.operatingProgress > 100 || business.monthsOperating < 0 ||
                business.consecutiveLossMonths < 0 || business.consecutiveLossMonths > 3 ||
                business.staffCount < 0 || business.staffCount > 6 ||
                business.locationLevel < 0 || business.locationLevel > 3 ||
                business.actionPoints < 0 || business.maxActionPoints < 0 ||
                business.actionPoints > business.maxActionPoints || business.maxActionPoints > 9 ||
                business.riskEventsExperienced < 0 ||
                business.lastRevenueMinorUnits < 0 || business.lastExpensesMinorUnits < 0 ||
                business.valuationMinorUnits < 0)
            {
                result.isValid = false;
                result.errors.Add("state.business operating values are invalid");
            }
            if (business.status == "operating" &&
                (string.IsNullOrWhiteSpace(business.businessId) || business.businessType != "local_services" ||
                 string.IsNullOrWhiteSpace(business.displayName) || business.level < 1 ||
                 business.locationLevel < 1 || business.maxActionPoints < 3))
            {
                result.isValid = false;
                result.errors.Add("operating business identity is invalid");
            }
            if (business.ledger == null || business.ledger.Count > 60)
            {
                result.isValid = false;
                result.errors.Add("state.business.ledger must contain at most 60 entries");
                return;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < business.ledger.Count; index++)
            {
                var entry = business.ledger[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.entryId) || !ids.Add(entry.entryId) ||
                    string.IsNullOrWhiteSpace(entry.type) || entry.valuationMinorUnits < 0 || entry.age < 0 ||
                    entry.monthOfYear < 1 || entry.monthOfYear > 12 || entry.revision < 1 ||
                    !TryParseUtcTimestamp(entry.timestampUtc, out _))
                {
                    result.isValid = false;
                    result.errors.Add($"state.business.ledger[{index}] is invalid");
                }
            }
        }

        private static void ValidateBusinessPortfolio(
            SaveValidationResult result, BusinessPortfolioState portfolio)
        {
            if (portfolio?.businesses == null || portfolio.businesses.Count > BusinessPortfolioState.MaxBusinesses)
            {
                result.isValid = false;
                result.errors.Add($"state.businessPortfolio must contain at most {BusinessPortfolioState.MaxBusinesses} businesses");
                return;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var business in portfolio.businesses)
            {
                if (business == null || string.IsNullOrWhiteSpace(business.businessId) ||
                    !ids.Add(business.businessId) || business.status == "none")
                {
                    result.isValid = false;
                    result.errors.Add("state.businessPortfolio contains an invalid or duplicate business");
                }
            }
            if (!string.IsNullOrEmpty(portfolio.selectedBusinessId) && !ids.Contains(portfolio.selectedBusinessId))
            {
                result.isValid = false;
                result.errors.Add("state.businessPortfolio.selectedBusinessId was not found");
            }
        }

        private static void ValidatePropertyPortfolio(
            SaveValidationResult result, PropertyPortfolioState portfolio)
        {
            if (portfolio?.properties == null || portfolio.ledger == null ||
                portfolio.properties.Count > PropertyPortfolioState.MaxProperties ||
                portfolio.ledger.Count > PropertyPortfolioState.MaxLedgerEntries)
            {
                result.isValid = false;
                result.errors.Add("state.propertyPortfolio is null or exceeds its retention limits");
                return;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in portfolio.properties)
            {
                if (property == null || string.IsNullOrWhiteSpace(property.propertyId) ||
                    !ids.Add(property.propertyId) || property.propertyType != "rental_home" ||
                    string.IsNullOrWhiteSpace(property.displayName) ||
                    property.status != "owned" && property.status != "sold" ||
                    property.purchasePriceMinorUnits <= 0 || property.currentValueMinorUnits < 0 ||
                    property.mortgageBalanceMinorUnits < 0 ||
                    property.mortgageBalanceMinorUnits > property.purchasePriceMinorUnits ||
                    property.monthlyMortgagePaymentMinorUnits < 0 || property.monthlyRentMinorUnits < 0 ||
                    property.monthlyPropertyTaxMinorUnits < 0 || property.condition < 0 || property.condition > 100 ||
                    property.vacancyMonths < 0 || property.monthsOwned < 0 ||
                    property.lastIncomeMinorUnits < 0 || property.lastExpensesMinorUnits < 0)
                {
                    result.isValid = false;
                    result.errors.Add("state.propertyPortfolio contains an invalid property");
                }
            }
            if (!string.IsNullOrEmpty(portfolio.selectedPropertyId) && !ids.Contains(portfolio.selectedPropertyId))
            {
                result.isValid = false;
                result.errors.Add("state.propertyPortfolio.selectedPropertyId was not found");
            }
            var ledgerIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in portfolio.ledger)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.entryId) || !ledgerIds.Add(entry.entryId) ||
                    string.IsNullOrWhiteSpace(entry.propertyId) || string.IsNullOrWhiteSpace(entry.type) ||
                    entry.propertyValueMinorUnits < 0 || entry.mortgageBalanceMinorUnits < 0 ||
                    entry.age < 0 || entry.monthOfYear < 1 || entry.monthOfYear > 12 || entry.revision < 1 ||
                    !TryParseUtcTimestamp(entry.timestampUtc, out _))
                {
                    result.isValid = false;
                    result.errors.Add("state.propertyPortfolio contains an invalid ledger entry");
                }
            }
        }

        private static void ValidateGoals(SaveValidationResult result, List<GoalState> goals)
        {
            if (goals == null || goals.Count > 20)
            {
                result.isValid = false;
                result.errors.Add("state.goals must contain at most 20 entries");
                return;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < goals.Count; index++)
            {
                var goal = goals[index];
                if (goal == null || string.IsNullOrWhiteSpace(goal.goalId) || !ids.Add(goal.goalId) ||
                    (goal.category != "main" && goal.category != "daily" && goal.category != "life") ||
                    string.IsNullOrWhiteSpace(goal.title) || string.IsNullOrWhiteSpace(goal.description) ||
                    string.IsNullOrWhiteSpace(goal.destination) || goal.progress < 0 ||
                    goal.progressRequired < 1 || goal.progress > goal.progressRequired ||
                    goal.rewardMinorUnits < 0 ||
                    (goal.status != "active" && goal.status != "claimable" &&
                     goal.status != "claimed" && goal.status != "expired") ||
                    goal.createdAtAge < 0 || goal.createdAtMonth < 1 || goal.createdAtMonth > 12 ||
                    (goal.status == "claimed" &&
                     (goal.claimedRevision < 1 || !TryParseUtcTimestamp(goal.claimedAtUtc, out _))))
                {
                    result.isValid = false;
                    result.errors.Add($"state.goals[{index}] is invalid");
                }
            }
        }

        private static void ValidateTransitionPresentations(
            SaveValidationResult result, List<TransitionPresentationState> transitions)
        {
            if (transitions == null || transitions.Count > 20)
            {
                result.isValid = false;
                result.errors.Add("state.transitionPresentations must contain at most 20 entries");
                return;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < transitions.Count; index++)
            {
                var item = transitions[index];
                if (item == null || string.IsNullOrWhiteSpace(item.transitionId) ||
                    !ids.Add(item.transitionId) || string.IsNullOrWhiteSpace(item.transitionType) ||
                    string.IsNullOrWhiteSpace(item.title) || string.IsNullOrWhiteSpace(item.summary) ||
                    item.age < 0 || item.monthOfYear < 1 || item.monthOfYear > 12 || item.revision < 1 ||
                    !TryParseUtcTimestamp(item.createdAtUtc, out _) ||
                    (item.acknowledged && (item.acknowledgedRevision < 1 ||
                     !TryParseUtcTimestamp(item.acknowledgedAtUtc, out _))))
                {
                    result.isValid = false;
                    result.errors.Add($"state.transitionPresentations[{index}] is invalid");
                }
            }
        }

        private static void ValidateOrientation(
            SaveValidationResult result, OrientationState orientation)
        {
            if (orientation == null ||
                (orientation.status != "not_started" && orientation.status != "completed") ||
                (orientation.status == "completed" &&
                 (orientation.completedRevision < 1 ||
                  !TryParseUtcTimestamp(orientation.completedAtUtc, out _))))
            {
                result.isValid = false;
                result.errors.Add("state.orientation is invalid");
            }
        }

        private static void ValidateSkills(SaveValidationResult result, List<SkillState> skills)
        {
            ValidateProgressRecords(
                result,
                skills,
                "state.skills",
                skill => skill?.skillId,
                skill => skill == null || skill.experience >= 0,
                "experience cannot be negative");
        }

        private static void ValidateRelationships(SaveValidationResult result, List<RelationshipState> relationships)
        {
            ValidateProgressRecords(
                result,
                relationships,
                "state.relationships",
                relationship => relationship?.relationshipId,
                relationship => relationship == null ||
                                relationship.value >= 0 && relationship.value <= 100 &&
                                relationship.warmth >= 0 && relationship.warmth <= 100 &&
                                relationship.monthsSinceInteraction >= 0 &&
                                relationship.npcSmarts >= 0 && relationship.npcSmarts <= 100 &&
                                relationship.npcCareerLevel >= 0 && relationship.npcCareerLevel <= 5 &&
                                relationship.npcAnnualIncomeMinorUnits >= 0 &&
                                relationship.npcCashMinorUnits >= 0 && relationship.npcDebtMinorUnits >= 0,
                "value/NPC fields must be within range and monthsSinceInteraction cannot be negative");
            if (relationships == null) return;
            if (relationships.Count > RelationshipState.MaxEntries)
            {
                result.isValid = false;
                result.errors.Add($"state.relationships must contain at most {RelationshipState.MaxEntries} entries");
            }
            for (var relationshipIndex = 0; relationshipIndex < relationships.Count; relationshipIndex++)
            {
                var relationship = relationships[relationshipIndex];
                if (relationship == null) continue;
                if (relationship.origin == "compatible_discovery" &&
                    (relationship.introducedAtAge < 18 || string.IsNullOrWhiteSpace(relationship.identityId) ||
                     string.IsNullOrWhiteSpace(relationship.pronouns) ||
                     string.IsNullOrWhiteSpace(relationship.genderIdentity) ||
                     relationship.orientation != "compatible_with_player"))
                {
                    result.isValid = false;
                    result.errors.Add($"state.relationships[{relationshipIndex}] compatible identity is invalid");
                }
                if (relationship.relationshipHistory == null || relationship.relationshipHistory.Count > 50)
                {
                    result.isValid = false;
                    result.errors.Add($"state.relationships[{relationshipIndex}].relationshipHistory must contain at most 50 entries");
                    continue;
                }
                var historyIds = new HashSet<string>(StringComparer.Ordinal);
                for (var historyIndex = 0; historyIndex < relationship.relationshipHistory.Count; historyIndex++)
                {
                    var entry = relationship.relationshipHistory[historyIndex];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.historyId) ||
                        !historyIds.Add(entry.historyId) || string.IsNullOrWhiteSpace(entry.type) ||
                        string.IsNullOrWhiteSpace(entry.summary) || entry.age < 0 ||
                        entry.monthOfYear < 1 || entry.monthOfYear > 12 || entry.revision < 1 ||
                        !TryParseUtcTimestamp(entry.timestampUtc, out _))
                    {
                        result.isValid = false;
                        result.errors.Add($"state.relationships[{relationshipIndex}].relationshipHistory[{historyIndex}] is invalid");
                    }
                }
            }
        }

        private static void ValidateStatuses(SaveValidationResult result, List<StatusState> statuses)
        {
            ValidateProgressRecords(
                result,
                statuses,
                "state.statuses",
                status => status?.statusId,
                status => status == null || status.remainingMonths > 0,
                "remainingMonths must be greater than zero");
        }

        private static void ValidateAchievements(
            SaveValidationResult result,
            List<AchievementState> achievements)
        {
            ValidateProgressRecords(
                result,
                achievements,
                "state.achievements",
                achievement => achievement?.achievementId,
                achievement => achievement == null || achievement.unlockedAtAge >= 0 &&
                               achievement.revision >= 1 &&
                               (!achievement.rewardClaimed || achievement.rewardClaimedRevision >= 1 &&
                                TryParseUtcTimestamp(achievement.rewardClaimedAtUtc, out _)),
                "unlock age/revision and reward claim metadata must be valid");
        }

        private static void ValidateLifeDecisions(
            SaveValidationResult result,
            List<LifeDecisionState> decisions)
        {
            ValidateProgressRecords(
                result,
                decisions,
                "state.lifeDecisions",
                decision => decision?.decisionId,
                decision => decision == null ||
                            !string.IsNullOrWhiteSpace(decision.choiceId) &&
                            decision.age >= 0 &&
                            decision.monthOfYear >= 1 && decision.monthOfYear <= 12 &&
                            decision.revision >= 0,
                "choiceId is required; age/revision cannot be negative; monthOfYear must be within [1, 12]");
        }

        private static void ValidateActionProgress(
            SaveValidationResult result,
            List<ActionProgressState> actions)
        {
            if (actions != null && actions.Count > ActionProgressState.MaxEntries)
            {
                result.isValid = false;
                result.errors.Add($"state.actionProgress must contain at most {ActionProgressState.MaxEntries} entries");
            }
            var validStates = new HashSet<string>(StringComparer.Ordinal)
                { "Ready", "InProgress", "Paused", "Complete", "Claimable", "Expired", "Locked" };
            ValidateProgressRecords(
                result,
                actions,
                "state.actionProgress",
                action => action?.instanceId,
                action => action == null ||
                          !string.IsNullOrWhiteSpace(action.actionId) &&
                          validStates.Contains(action.state) &&
                          action.progressRequired > 0 &&
                          action.progress >= 0 && action.progress <= action.progressRequired &&
                          action.durationSeconds >= 0 &&
                          action.claimWindowSeconds >= 0 && action.remainingSeconds >= 0 &&
                          action.revision >= 0,
                "actionId/state must be valid; progress/duration must be within range; revision cannot be negative");
        }

        private static void ValidateMatchSession(SaveValidationResult result, MatchSessionState session)
        {
            if (session == null)
            {
                result.isValid = false;
                result.errors.Add("state.matchSession is null");
                return;
            }
            var states = new HashSet<string>(StringComparer.Ordinal)
                { "none", "active", "paused", "success", "failure", "claimed" };
            if (!states.Contains(session.state) || session.score < 0 || session.targetScore < 0 ||
                session.rewardAmount < 0 || session.durationSeconds < 0 || session.rngStep < 0 ||
                session.remainingSeconds < 0 || session.rewardMultiplier < 1 || session.rewardMultiplier > 2 ||
                session.rewardMultiplierApplied && !session.rewardMultiplierAvailable)
            {
                result.isValid = false;
                result.errors.Add("state.matchSession lifecycle values are invalid");
            }
            if (session.state == "none") return;
            if (string.IsNullOrWhiteSpace(session.activityId) || string.IsNullOrWhiteSpace(session.instanceId) ||
                string.IsNullOrWhiteSpace(session.theme) || session.width < 3 || session.width > 10 ||
                session.height < 3 || session.height > 10 || session.board == null ||
                session.board.Count != session.width * session.height || session.targetScore < 1 ||
                session.durationSeconds < 1 || !TryParseUtcTimestamp(session.startedAtUtc, out _) ||
                !TryParseUtcTimestamp(session.completesAtUtc, out _) ||
                session.board.Exists(tile => tile < 0 || tile > 7))
            {
                result.isValid = false;
                result.errors.Add("state.matchSession active session data is invalid");
            }
            if (session.state == "paused" && (session.remainingSeconds < 1 ||
                                               !TryParseUtcTimestamp(session.pausedAtUtc, out _)))
            {
                result.isValid = false;
                result.errors.Add("state.matchSession paused metadata is invalid");
            }
            if (session.rewardClaimed && (session.state != "claimed" || session.claimedRevision < 1 ||
                                          !TryParseUtcTimestamp(session.claimedAtUtc, out _)))
            {
                result.isValid = false;
                result.errors.Add("state.matchSession claim metadata is invalid");
            }
        }

        private static void ValidateProgressRecords<T>(
            SaveValidationResult result,
            List<T> records,
            string fieldName,
            Func<T, string> getId,
            Func<T, bool> isValueValid,
            string invalidValueMessage)
            where T : class
        {
            if (records == null)
            {
                result.isValid = false;
                result.errors.Add($"{fieldName} is null");
                return;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < records.Count; index++)
            {
                var record = records[index];
                if (record == null)
                {
                    result.isValid = false;
                    result.errors.Add($"{fieldName}[{index}] is null");
                    continue;
                }

                var id = getId(record);
                ValidateRequiredString(result, id, $"{fieldName}[{index}].id");
                if (!string.IsNullOrEmpty(id) && !ids.Add(id))
                {
                    result.isValid = false;
                    result.errors.Add($"{fieldName} contains duplicate id {id}");
                }

                if (!isValueValid(record))
                {
                    result.isValid = false;
                    result.errors.Add($"{fieldName}[{index}] {invalidValueMessage}");
                }
            }
        }

        private static void ValidateEventHistory(SaveValidationResult result, List<EventHistoryEntry> eventHistory)
        {
            if (eventHistory == null)
            {
                result.isValid = false;
                result.errors.Add("state.eventHistory is null");
                return;
            }

            for (var index = 0; index < eventHistory.Count; index++)
            {
                var entry = eventHistory[index];
                if (entry == null)
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}] is null");
                    continue;
                }

                ValidateRequiredString(result, entry.eventId, $"state.eventHistory[{index}].eventId");
                ValidateRequiredString(result, entry.choiceId, $"state.eventHistory[{index}].choiceId");
                ValidateRequiredString(result, entry.outcomeId, $"state.eventHistory[{index}].outcomeId");

                if (entry.age < 0)
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}].age cannot be negative");
                }

                if (entry.revision < 1)
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}].revision must be at least 1");
                }

                if (!TryParseUtcTimestamp(entry.timestampUtc, out _))
                {
                    result.isValid = false;
                    result.errors.Add($"state.eventHistory[{index}].timestampUtc must be a valid UTC timestamp");
                }
            }
        }

        private static void ValidateScheduledEvents(SaveValidationResult result, List<ScheduledEventRecord> scheduledEvents)
        {
            if (scheduledEvents == null)
            {
                result.isValid = false;
                result.errors.Add("state.scheduledEvents is null");
                return;
            }

            if (scheduledEvents.Count > ScheduledEventRecord.MaxScheduledEvents)
            {
                result.isValid = false;
                result.errors.Add($"state.scheduledEvents cannot exceed {ScheduledEventRecord.MaxScheduledEvents} records");
            }

            for (var index = 0; index < scheduledEvents.Count; index++)
            {
                var record = scheduledEvents[index];
                if (record == null)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}] is null");
                    continue;
                }

                ValidateRequiredString(result, record.eventId, $"state.scheduledEvents[{index}].eventId");
                ValidateRequiredString(result, record.sourceEventId, $"state.scheduledEvents[{index}].sourceEventId");

                if (record.earliestTriggerAge < 0)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].earliestTriggerAge cannot be negative");
                }

                if (record.latestTriggerAge < record.earliestTriggerAge)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].latestTriggerAge cannot be earlier than earliestTriggerAge");
                }


                if (record.earliestTriggerMonth < 0 || record.latestTriggerMonth < 0)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}] trigger months cannot be negative");
                }
                else if (record.latestTriggerMonth > 0 &&
                         record.latestTriggerMonth < record.earliestTriggerMonth)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].latestTriggerMonth cannot be earlier than earliestTriggerMonth");
                }

                if (record.priority < 0 || record.priority > 100)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].priority must be within [0, 100]");
                }

                if (record.cooldownMonths < 0 || record.cooldownMonths > 1200)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].cooldownMonths must be within [0, 1200]");
                }

                if (record.chance < 0f || record.chance > 1f)
                {
                    result.isValid = false;
                    result.errors.Add($"state.scheduledEvents[{index}].chance must be within [0, 1]");
                }
            }
        }

        private static void ValidateStatRange(SaveValidationResult result, int value, string fieldName)
        {
            if (value < SaveSchema.MinCoreStatValue || value > SaveSchema.MaxCoreStatValue)
            {
                result.isValid = false;
                result.errors.Add($"{fieldName} must be within [{SaveSchema.MinCoreStatValue}, {SaveSchema.MaxCoreStatValue}]");
            }
        }

        private static void ValidateRequiredString(SaveValidationResult result, string value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
            {
                result.isValid = false;
                result.errors.Add($"{fieldName} is required but empty or null");
            }
        }

        private static bool TryParseUtcTimestamp(string value, out DateTimeOffset timestamp)
        {
            return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out timestamp);
        }
    }
}
