using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Tests.Domain.Save
{
    public sealed class SaveMigratorTests
    {
        [Test]
        public void Migrate_NormalizesOlderAdditiveVersionOneFields()
        {
            var save = CreateValidSave();
            save.state.skills = null;
            save.state.lifeDecisions = null;
            save.state.actionProgress = null;
            save.state.matchSession = null;
            save.state.relationships = null;
            save.state.statuses = null;
            save.state.achievements = null;
            var serializedHome = JsonUtility.ToJson(save.state.home);
            var json = JsonUtility.ToJson(save)
                .Replace("\"orientation\":{\"status\":\"not_started\",\"completedRevision\":0,\"completedAtUtc\":\"\"},", string.Empty)
                .Replace("\"family\":{\"planningPreference\":\"undiscussed\",\"planningPartnerId\":\"\",\"partnerConsent\":false,\"pendingPath\":\"\",\"monthsUntilResolution\":0,\"children\":[]},", string.Empty)
                .Replace($"\"home\":{serializedHome},", string.Empty)
                .Replace("\"moneyTransactions\":[],", string.Empty)
                .Replace("\"annualReviewHistory\":[],", string.Empty)
                .Replace("\"majorOutcomeSummaries\":[],", string.Empty)
                .Replace("\"savingsMinorUnits\":0,", string.Empty)
                .Replace("\"lifeStatus\":\"active\",", string.Empty)
                .Replace("\"endingReason\":\"\",", string.Empty)
                .Replace("\"endedAtAge\":-1,", string.Empty)
                .Replace("\"looks\":50,", string.Empty)
                .Replace("\"luck\":50", "\"luckRemoved\":0");

            var migrated = SaveMigrator.TryMigrate(json, out var result, out var report, out var error);

            Assert.IsTrue(migrated, error);
            Assert.IsTrue(report.changed);
            Assert.That(result.state.character.looks, Is.EqualTo(50));
            Assert.That(result.state.character.luck, Is.EqualTo(50));
            Assert.That(result.state.character.lifeStatus, Is.EqualTo("active"));
            Assert.That(result.state.character.endedAtAge, Is.EqualTo(-1));
            Assert.That(result.state.skills, Is.Not.Null);
            Assert.That(result.state.lifeDecisions, Is.Not.Null);
            Assert.That(result.state.actionProgress, Is.Not.Null);
            Assert.That(result.state.matchSession, Is.Not.Null);
            Assert.That(result.state.matchSession.state, Is.EqualTo("none"));
            Assert.That(result.state.relationships, Is.Not.Null);
            Assert.That(result.state.statuses, Is.Not.Null);
            Assert.That(result.state.achievements, Is.Not.Null);
            Assert.That(result.state.moneyTransactions, Is.Not.Null.And.Empty);
            Assert.That(result.state.finances.savingsMinorUnits, Is.Zero);
            Assert.That(result.state.annualReviewHistory, Is.Not.Null.And.Empty);
            Assert.That(result.state.annualReview.majorOutcomeSummaries, Is.Not.Null.And.Empty);
            Assert.That(result.state.home, Is.Not.Null);
            Assert.That(result.state.home.homeId, Is.EqualTo("starter_home"));
            Assert.That(result.state.home.condition, Is.EqualTo(80));
            Assert.That(result.state.home.readingMaterialStock, Is.EqualTo(3));
            Assert.That(result.state.home.trainingEquipmentCondition, Is.EqualTo(100));
            Assert.That(result.state.home.inventory, Has.Count.EqualTo(2));
            Assert.That(result.state.home.inventory[0].itemId, Is.EqualTo("starter_books"));
            Assert.That(result.state.family, Is.Not.Null);
            Assert.That(result.state.family.children, Is.Not.Null.And.Empty);
            Assert.That(result.state.business, Is.Not.Null);
            Assert.That(result.state.business.status, Is.EqualTo("none"));
            Assert.That(result.state.businessPortfolio, Is.Not.Null);
            Assert.That(result.state.businessPortfolio.businesses, Is.Empty);
            Assert.That(result.state.propertyPortfolio, Is.Not.Null);
            Assert.That(result.state.propertyPortfolio.properties, Is.Empty);
            Assert.That(result.state.propertyPortfolio.ledger, Is.Empty);
            Assert.That(result.state.goals, Is.Not.Null.And.Empty);
            Assert.That(result.state.orientation.status, Is.EqualTo("completed"));
            Assert.That(result.state.orientation.completedRevision, Is.GreaterThan(0));
            Assert.That(result.integrity.payloadHash, Is.Empty);
        }

        [Test]
        public void Migrate_IsIdempotentAfterNormalization()
        {
            var save = CreateValidSave();
            var legacyJson = JsonUtility.ToJson(save)
                .Replace("\"looks\":50,", string.Empty);
            Assert.IsTrue(SaveMigrator.TryMigrate(
                legacyJson, out var first, out var firstReport, out var firstError), firstError);

            Assert.IsTrue(SaveMigrator.TryMigrate(
                JsonUtility.ToJson(first), out _, out var secondReport, out var secondError), secondError);

            Assert.IsTrue(firstReport.changed);
            Assert.IsFalse(secondReport.changed);
            Assert.That(secondReport.changes, Is.Empty);
        }

        [Test]
        public void Migrate_PreservesLegacyHomeCountsInStableInventory()
        {
            var save = CreateValidSave();
            save.state.home.readingMaterialStock = 2;
            save.state.home.readingMaterialCapacity = 5;
            save.state.home.trainingEquipmentCondition = 40;
            var json = JsonUtility.ToJson(save);
            var inventoryJson = JsonUtility.ToJson(new InventoryWrapper { inventory = save.state.home.inventory });
            inventoryJson = "," + inventoryJson.Substring(1, inventoryJson.Length - 2);
            json = json.Replace(inventoryJson, string.Empty);

            Assert.IsTrue(SaveMigrator.TryMigrate(json, out var migrated, out var report, out var error), error);
            Assert.That(migrated.state.home.inventory.Find(item => item.itemId == "starter_books").quantity, Is.EqualTo(2));
            Assert.That(migrated.state.home.inventory.Find(item => item.itemId == "starter_books").capacity, Is.EqualTo(5));
            Assert.That(migrated.state.home.inventory.Find(item => item.itemId == "starter_training_kit").condition, Is.EqualTo(40));
            Assert.That(report.changes, Has.Some.Contains("stable inventory"));
        }

        [System.Serializable]
        private sealed class InventoryWrapper
        {
            public List<HomeInventoryItemState> inventory;
        }

        [Test]
        public void Migrate_InfersFinanceIndustryForLegacyEmployedCareer()
        {
            var save = CreateValidSave();
            save.state.career = new CareerState
            {
                employerId = "stim_financial_group",
                roleTitle = "Associate",
                annualSalaryMinorUnits = 5500000,
                careerProgress = 20
            };
            var legacyJson = JsonUtility.ToJson(save)
                .Replace("\"industryId\":\"\",", string.Empty)
                .Replace("\"pendingIndustryId\":\"\",", string.Empty)
                .Replace("\"employmentStatus\":\"unemployed\",", string.Empty)
                .Replace("\"monthsUnemployed\":0,", string.Empty)
                .Replace("\"performanceWarnings\":0,", string.Empty);

            Assert.IsTrue(SaveMigrator.TryMigrate(
                legacyJson, out var migrated, out var report, out var error), error);

            Assert.That(migrated.state.career.industryId, Is.EqualTo("finance"));
            Assert.That(migrated.state.career.employmentStatus, Is.EqualTo("employed"));
            Assert.That(report.changes, Has.Some.Matches<string>(change =>
                change.Contains("state.career.industryId=finance")));
            Assert.IsTrue(SaveMigrator.TryMigrate(
                JsonUtility.ToJson(migrated), out _, out var repeated, out error), error);
            Assert.IsFalse(repeated.changed);
        }

        [Test]
        public void Migrate_AddsUnclaimedRewardAuditFieldsToLegacyAchievements()
        {
            var save = CreateValidSave();
            save.state.achievements.Add(new AchievementState
            {
                achievementId = "first_job", unlockedAtAge = 18, revision = 1,
                timestampUtc = "2026-07-13T12:00:00Z"
            });
            var legacyJson = JsonUtility.ToJson(save).Replace(
                ",\"rewardClaimed\":false,\"rewardClaimedRevision\":0,\"rewardClaimedAtUtc\":\"\"",
                string.Empty);

            Assert.IsTrue(SaveMigrator.TryMigrate(
                legacyJson, out var migrated, out var report, out var error), error);

            Assert.That(migrated.state.achievements[0].rewardClaimed, Is.False);
            Assert.That(report.changes, Has.Some.Matches<string>(change =>
                change.Contains("reward claims created")));
        }

        [Test]
        public void Migrate_PreservesLegacyIndexValueAsContributionsBaseline()
        {
            var save = CreateValidSave();
            save.state.finances.indexFundMinorUnits = 125000;
            save.state.finances.indexFundContributionsMinorUnits = 0;
            var legacyJson = JsonUtility.ToJson(save)
                .Replace("\"indexFundContributionsMinorUnits\":0,", string.Empty);

            Assert.IsTrue(SaveMigrator.TryMigrate(
                legacyJson, out var migrated, out var report, out var error), error);

            Assert.That(migrated.state.finances.indexFundContributionsMinorUnits,
                Is.EqualTo(125000));
            Assert.That(report.changes, Has.Some.Matches<string>(change =>
                change.Contains("indexFundContributionsMinorUnits")));
            Assert.IsTrue(SaveMigrator.TryMigrate(
                JsonUtility.ToJson(migrated), out _, out var repeated, out error), error);
            Assert.IsFalse(repeated.changed);
        }

        [Test]
        public void Migrate_RejectsUnknownSaveFormatVersion()
        {
            var save = CreateValidSave();
            save.saveFormatVersion = 99;

            var migrated = SaveMigrator.TryMigrate(
                JsonUtility.ToJson(save), out _, out _, out var error);

            Assert.IsFalse(migrated);
            Assert.That(error, Contains.Substring("No migration path"));
        }

        [Test]
        public void Migrate_AddsReloadSafeUiWorkflowStateToEstablishedSaves()
        {
            var legacy = CreateValidSave();
            legacy.state.uiWorkflow = null;
            var legacyJson = Regex.Replace(
                JsonUtility.ToJson(legacy),
                "\\\"uiWorkflow\\\":\\{[^{}]*\\},?",
                string.Empty);
            Assert.That(legacyJson, Does.Not.Contain("\"uiWorkflow\""),
                "The fixture must represent a save written before UI workflow state existed.");

            Assert.IsTrue(SaveMigrator.TryMigrate(
                legacyJson, out var migrated, out var report, out var error), error);
            Assert.That(migrated.state.uiWorkflow, Is.Not.Null);
            Assert.That(migrated.state.uiWorkflow.queuedYearMonthsRemaining, Is.Zero);
            Assert.That(report.changes, Has.Some.Contains("state.uiWorkflow created"));
        }

        [Test]
        public void Migrate_AddsArchiveStateAndBoundsLegacyHistories()
        {
            var legacy = CreateValidSave();
            for (var index = 0; index < HistoryRetention.MaxLifeFeedEntries + 5; index++)
                legacy.state.lifeFeed.Add(new LifeFeedEntry
                {
                    entryId = "legacy_" + index,
                    category = "activity",
                    text = "Legacy entry " + index,
                    revision = index + 1
                });
            legacy.state.historyArchive = null;
            var legacyJson = Regex.Replace(
                JsonUtility.ToJson(legacy),
                "\"historyArchive\":(?:null|\\{[^{}]*\\}),?",
                string.Empty);
            Assert.That(legacyJson, Does.Not.Contain("\"historyArchive\""));

            Assert.IsTrue(SaveMigrator.TryMigrate(
                legacyJson, out var migrated, out var report, out var error), error);

            Assert.That(migrated.state.historyArchive, Is.Not.Null);
            Assert.That(migrated.state.historyArchive.lifeFeedArchivedCount, Is.EqualTo(5));
            Assert.That(migrated.state.lifeFeed, Has.Count.EqualTo(HistoryRetention.MaxLifeFeedEntries));
            Assert.That(report.changes, Has.Some.Contains("state.historyArchive created"));
            Assert.That(report.changes, Has.Some.Contains("unbounded histories archived"));
        }

        private static SaveEnvelope CreateValidSave()
        {
            return new SaveEnvelope
            {
                gameBuildVersion = "0.1.0",
                contentVersion = "1",
                saveId = "migration-save",
                playerAccountId = "local-player",
                lifeId = "migration-life",
                createdAtUtc = "2026-07-13T12:00:00Z",
                updatedAtUtc = "2026-07-13T12:00:00Z",
                revision = 1,
                deviceIdHash = "device-hash",
                rng = new RngState { seed = 42, step = 0 },
                integrity = new SaveIntegrity { payloadHash = "sha256:legacy" },
                state = new GameState
                {
                    character = new CharacterState
                    {
                        age = 18,
                        health = 80,
                        happiness = 70,
                        smarts = 60,
                        looks = 50,
                        luck = 50
                    },
                    eventHistory = new List<EventHistoryEntry>(),
                    scheduledEvents = new List<ScheduledEventRecord>()
                }
            };
        }
    }
}
