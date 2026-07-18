using System;
using System.Collections.Generic;
using UnityEngine;

namespace StimTycoon.Saves
{
    public sealed class StimSaveMigrationReport
    {
        public int sourceVersion;
        public int targetVersion;
        public bool changed;
        public readonly List<string> changes = new List<string>();
    }

    /// <summary>
    /// Forward-only entry point for save upgrades. Version 1 currently performs
    /// additive normalization so saves created before newer v1 fields still load.
    /// </summary>
    public static class StimSaveMigrator
    {
        public static bool TryMigrate(
            string serializedSave,
            out StimSaveEnvelope save,
            out StimSaveMigrationReport report,
            out string error)
        {
            save = null;
            report = new StimSaveMigrationReport();
            if (string.IsNullOrWhiteSpace(serializedSave))
            {
                error = "Serialized save is required.";
                return false;
            }

            try
            {
                save = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);
            }
            catch (Exception exception)
            {
                error = $"Serialized save is not valid JSON: {exception.Message}";
                return false;
            }

            if (save == null)
            {
                error = "Serialized save produced a null envelope.";
                return false;
            }

            report.sourceVersion = save.saveFormatVersion;
            report.targetVersion = StimSaveSchema.SupportedSaveFormatVersion;
            if (save.saveFormatVersion != StimSaveSchema.SupportedSaveFormatVersion)
            {
                error = $"No migration path exists from save format {save.saveFormatVersion}.";
                return false;
            }

            NormalizeV1(save, serializedSave, report);
            if (report.changed && save.integrity != null)
            {
                save.integrity.payloadHash = string.Empty;
            }

            error = string.Empty;
            return true;
        }

        private static void NormalizeV1(
            StimSaveEnvelope save,
            string serializedSave,
            StimSaveMigrationReport report)
        {
            if (save.state == null)
            {
                return;
            }

            if (save.state.character != null)
            {
                if (!serializedSave.Contains("\"lifeStatus\""))
                {
                    save.state.character.lifeStatus = "active";
                    save.state.character.endedAtAge = -1;
                    Record(report, "state.character.lifeStatus=active");
                    Record(report, "state.character.endedAtAge=-1");
                }
                if (!serializedSave.Contains("\"looks\""))
                {
                    save.state.character.looks = 50;
                    Record(report, "state.character.looks=50");
                }
                if (!serializedSave.Contains("\"luck\""))
                {
                    save.state.character.luck = 50;
                    Record(report, "state.character.luck=50");
                }
                if (!serializedSave.Contains("\"genderIdentity\""))
                {
                    save.state.character.genderIdentity = "undiscovered";
                    Record(report, "state.character.genderIdentity=undiscovered");
                }
                if (!serializedSave.Contains("\"sexualOrientation\""))
                {
                    save.state.character.sexualOrientation = "undiscovered";
                    Record(report, "state.character.sexualOrientation=undiscovered");
                }
            }

            if (save.state.calendar == null)
            {
                save.state.calendar = new StimCalendarState();
                Record(report, "state.calendar created");
            }
            if (save.state.annualReview == null || !serializedSave.Contains("\"annualReview\""))
            {
                save.state.annualReview = new StimAnnualReviewState();
                Record(report, "state.annualReview created");
            }
            if (save.state.annualReviewHistory == null || !serializedSave.Contains("\"annualReviewHistory\""))
            {
                save.state.annualReviewHistory = new List<StimAnnualReviewHistoryState>();
                Record(report, "state.annualReviewHistory created");
            }
            if (save.state.annualReview != null && !serializedSave.Contains("\"majorOutcomeSummaries\""))
            {
                save.state.annualReview.majorOutcomeSummaries = new List<string>();
                Record(report, "state.annualReview.majorOutcomeSummaries created");
            }
            if (save.state.finances == null)
            {
                save.state.finances = new StimFinancesState();
                Record(report, "state.finances created");
            }
            if (save.state.finances != null && !serializedSave.Contains("\"spouseAnnualIncomeMinorUnits\""))
            {
                save.state.finances.spouseAnnualIncomeMinorUnits = 0;
                Record(report, "state.finances.spouseAnnualIncomeMinorUnits=0");
            }
            if (save.state.finances != null && !serializedSave.Contains("\"savingsMinorUnits\""))
            {
                save.state.finances.savingsMinorUnits = 0;
                Record(report, "state.finances.savingsMinorUnits=0");
            }
            if (save.state.finances != null && !serializedSave.Contains("\"indexFundMinorUnits\""))
            {
                save.state.finances.indexFundMinorUnits = 0;
                Record(report, "state.finances.indexFundMinorUnits=0");
            }
            if (save.state.finances != null && !serializedSave.Contains("\"indexFundContributionsMinorUnits\""))
            {
                save.state.finances.indexFundContributionsMinorUnits = save.state.finances.indexFundMinorUnits;
                Record(report, "state.finances.indexFundContributionsMinorUnits=current index fund value");
            }
            if (save.state.finances != null && !serializedSave.Contains("\"savingsApyBasisPoints\""))
            {
                save.state.finances.savingsApyBasisPoints = 350;
                Record(report, "state.finances.savingsApyBasisPoints=350");
            }
            if (save.state.finances != null && !serializedSave.Contains("\"lastNetCashFlowMinorUnits\""))
            {
                save.state.finances.lastGrossIncomeMinorUnits = 0;
                save.state.finances.lastTaxesMinorUnits = 0;
                save.state.finances.lastExpensesMinorUnits = 0;
                save.state.finances.lastCreditInterestMinorUnits = 0;
                save.state.finances.lastSavingsInterestMinorUnits = 0;
                save.state.finances.lastNetCashFlowMinorUnits = 0;
                Record(report, "state.finances last-month cash-flow fields created");
            }
            if (save.state.moneyTransactions == null || !serializedSave.Contains("\"moneyTransactions\""))
            {
                save.state.moneyTransactions = new List<StimMoneyTransactionState>();
                Record(report, "state.moneyTransactions created");
            }
            if (save.state.finances != null && !serializedSave.Contains("\"householdCreditBalanceMinorUnits\""))
            {
                save.state.finances.householdCreditBalanceMinorUnits = 0;
                save.state.finances.householdCreditAprBasisPoints = 0;
                Record(report, "state.finances household credit fields created");
            }
            if (save.state.career == null)
            {
                save.state.career = new StimCareerState();
                Record(report, "state.career created");
            }
            if (save.state.business == null || !serializedSave.Contains("\"business\""))
            {
                save.state.business = new StimBusinessState();
                Record(report, "state.business created");
            }
            else if (!serializedSave.Contains("\"actionPoints\""))
            {
                save.state.business.locationLevel = save.state.business.status == "operating" ? 1 : 0;
                save.state.business.maxActionPoints = save.state.business.status == "operating" ? 3 : 0;
                save.state.business.actionPoints = save.state.business.maxActionPoints;
                save.state.business.staffCount = 0;
                save.state.business.riskEventsExperienced = 0;
                Record(report, "state.business staffing, location, and action points created");
            }
            if (save.state.career != null && !serializedSave.Contains("\"industryId\"") &&
                !string.IsNullOrEmpty(save.state.career.roleTitle) &&
                save.state.career.roleTitle != "Retired")
            {
                save.state.career.industryId = "finance";
                Record(report, "state.career.industryId=finance");
            }
            if (save.state.career != null && !serializedSave.Contains("\"employmentStatus\""))
            {
                save.state.career.employmentStatus = save.state.career.roleTitle == "Retired"
                    ? "retired"
                    : string.IsNullOrEmpty(save.state.career.roleTitle) ? "unemployed" : "employed";
                save.state.career.monthsUnemployed = 0;
                save.state.career.performanceWarnings = 0;
                Record(report, $"state.career.employmentStatus={save.state.career.employmentStatus}");
            }
            if (save.state.education == null)
            {
                save.state.education = new StimEducationState();
                Record(report, "state.education created");
            }
            if (save.state.household == null || !serializedSave.Contains("\"household\""))
            {
                save.state.household = new StimHouseholdState();
                Record(report, "state.household created");
            }
            if (save.state.family == null || !serializedSave.Contains("\"family\""))
            {
                save.state.family = new StimFamilyState();
                Record(report, "state.family created");
            }
            else if (save.state.family.children != null && save.state.family.children.Count > 0 &&
                     !serializedSave.Contains("\"custodyStatus\""))
            {
                foreach (var child in save.state.family.children)
                {
                    if (child == null) continue;
                    child.wellbeing = 60;
                    child.custodyStatus = child.age >= 18 ? "independent" : "household";
                }
                Record(report, "state.family child development and custody created");
            }
            if (save.state.home == null || !serializedSave.Contains("\"home\""))
            {
                save.state.home = new StimHomeState();
                Record(report, "state.home created");
            }
            else if (!serializedSave.Contains("\"readingMaterialCapacity\""))
            {
                save.state.home.readingMaterialCapacity = 3;
                save.state.home.readingMaterialStock = 3;
                save.state.home.trainingEquipmentCondition = 100;
                Record(report, "state.home inventory and capacity created");
            }
            if (save.state.skills == null)
            {
                save.state.skills = new List<StimSkillState>();
                Record(report, "state.skills created");
            }
            if (save.state.relationships == null)
            {
                save.state.relationships = new List<StimRelationshipState>();
                Record(report, "state.relationships created");
            }
            if (save.state.relationships != null && save.state.relationships.Count > 0 &&
                !serializedSave.Contains("\"relationshipHistory\""))
            {
                foreach (var relationship in save.state.relationships)
                    if (relationship != null) relationship.relationshipHistory = new List<StimRelationshipHistoryState>();
                Record(report, "state.relationships relationship history created");
            }
            if (save.state.statuses == null)
            {
                save.state.statuses = new List<StimStatusState>();
                Record(report, "state.statuses created");
            }
            if (save.state.achievements == null || !serializedSave.Contains("\"achievements\""))
            {
                save.state.achievements = new List<StimAchievementState>();
                Record(report, "state.achievements created");
            }
            else if (save.state.achievements.Count > 0 && !serializedSave.Contains("\"rewardClaimed\""))
            {
                foreach (var achievement in save.state.achievements)
                {
                    if (achievement == null) continue;
                    achievement.rewardClaimed = false;
                    achievement.rewardClaimedRevision = 0;
                    achievement.rewardClaimedAtUtc = string.Empty;
                }
                Record(report, "state.achievements reward claims created");
            }
            if (save.state.goals == null || !serializedSave.Contains("\"goals\""))
            {
                save.state.goals = new List<StimGoalState>();
                Record(report, "state.goals created");
            }
            if (save.state.orientation == null || !serializedSave.Contains("\"orientation\""))
            {
                save.state.orientation = new StimOrientationState
                {
                    status = "completed",
                    completedRevision = Math.Max(1, save.revision),
                    completedAtUtc = save.updatedAtUtc
                };
                Record(report, "state.orientation completed for established life");
            }
            if (save.state.uiWorkflow == null || !serializedSave.Contains("\"uiWorkflow\""))
            {
                save.state.uiWorkflow = new StimUiWorkflowState();
                Record(report, "state.uiWorkflow created");
            }
            if (save.state.transitionPresentations == null ||
                !serializedSave.Contains("\"transitionPresentations\""))
            {
                save.state.transitionPresentations = new List<StimTransitionPresentationState>();
                Record(report, "state.transitionPresentations created");
            }
            if (save.state.lifeDecisions == null || !serializedSave.Contains("\"lifeDecisions\""))
            {
                save.state.lifeDecisions = new List<StimLifeDecisionState>();
                Record(report, "state.lifeDecisions created");
            }
            if (save.state.actionProgress == null || !serializedSave.Contains("\"actionProgress\""))
            {
                save.state.actionProgress = new List<StimActionProgressState>();
                Record(report, "state.actionProgress created");
            }
            if (save.state.lifeFeed == null)
            {
                save.state.lifeFeed = new List<StimLifeFeedEntry>();
                Record(report, "state.lifeFeed created");
            }
            if (save.state.historyArchive == null || !serializedSave.Contains("\"historyArchive\""))
            {
                save.state.historyArchive = new StimHistoryArchiveState();
                Record(report, "state.historyArchive created");
            }
            if (save.state.eventHistory == null)
            {
                save.state.eventHistory = new List<StimEventHistoryEntry>();
                Record(report, "state.eventHistory created");
            }
            if (save.state.scheduledEvents == null)
            {
                save.state.scheduledEvents = new List<StimScheduledEventRecord>();
                Record(report, "state.scheduledEvents created");
            }
            var feedCountBeforeRetention = save.state.lifeFeed.Count;
            var eventCountBeforeRetention = save.state.eventHistory.Count;
            StimHistoryRetention.Apply(save.state);
            if (save.state.lifeFeed.Count != feedCountBeforeRetention ||
                save.state.eventHistory.Count != eventCountBeforeRetention)
                Record(report, "unbounded histories archived");
        }

        private static void Record(StimSaveMigrationReport report, string change)
        {
            report.changed = true;
            report.changes.Add(change);
        }
    }
}
