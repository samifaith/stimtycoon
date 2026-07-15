using System.Collections.Generic;
using NUnit.Framework;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Tests.Domain.Save
{
    public sealed class StimSaveMigratorTests
    {
        [Test]
        public void Migrate_NormalizesOlderAdditiveVersionOneFields()
        {
            var save = CreateValidSave();
            save.state.skills = null;
            save.state.lifeDecisions = null;
            save.state.actionProgress = null;
            save.state.relationships = null;
            save.state.statuses = null;
            save.state.achievements = null;
            var json = JsonUtility.ToJson(save)
                .Replace("\"orientation\":{\"status\":\"not_started\",\"completedRevision\":0,\"completedAtUtc\":\"\"},", string.Empty)
                .Replace("\"family\":{\"planningPreference\":\"undiscussed\",\"planningPartnerId\":\"\",\"partnerConsent\":false,\"pendingPath\":\"\",\"monthsUntilResolution\":0,\"children\":[]},", string.Empty)
                .Replace("\"home\":{\"homeId\":\"starter_home\",\"condition\":80,\"upgradeLevel\":0,\"improvementProgress\":0,\"readingMaterialStock\":3,\"readingMaterialCapacity\":3,\"trainingEquipmentCondition\":100},", string.Empty)
                .Replace("\"moneyTransactions\":[],", string.Empty)
                .Replace("\"annualReviewHistory\":[],", string.Empty)
                .Replace("\"majorOutcomeSummaries\":[],", string.Empty)
                .Replace("\"savingsMinorUnits\":0,", string.Empty)
                .Replace("\"lifeStatus\":\"active\",", string.Empty)
                .Replace("\"endingReason\":\"\",", string.Empty)
                .Replace("\"endedAtAge\":-1,", string.Empty)
                .Replace("\"looks\":50,", string.Empty)
                .Replace("\"luck\":50", "\"luckRemoved\":0");

            var migrated = StimSaveMigrator.TryMigrate(json, out var result, out var report, out var error);

            Assert.IsTrue(migrated, error);
            Assert.IsTrue(report.changed);
            Assert.That(result.state.character.looks, Is.EqualTo(50));
            Assert.That(result.state.character.luck, Is.EqualTo(50));
            Assert.That(result.state.character.lifeStatus, Is.EqualTo("active"));
            Assert.That(result.state.character.endedAtAge, Is.EqualTo(-1));
            Assert.That(result.state.skills, Is.Not.Null);
            Assert.That(result.state.lifeDecisions, Is.Not.Null);
            Assert.That(result.state.actionProgress, Is.Not.Null);
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
            Assert.That(result.state.family, Is.Not.Null);
            Assert.That(result.state.family.children, Is.Not.Null.And.Empty);
            Assert.That(result.state.business, Is.Not.Null);
            Assert.That(result.state.business.status, Is.EqualTo("none"));
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
            Assert.IsTrue(StimSaveMigrator.TryMigrate(
                legacyJson, out var first, out var firstReport, out var firstError), firstError);

            Assert.IsTrue(StimSaveMigrator.TryMigrate(
                JsonUtility.ToJson(first), out _, out var secondReport, out var secondError), secondError);

            Assert.IsTrue(firstReport.changed);
            Assert.IsFalse(secondReport.changed);
            Assert.That(secondReport.changes, Is.Empty);
        }

        [Test]
        public void Migrate_InfersFinanceIndustryForLegacyEmployedCareer()
        {
            var save = CreateValidSave();
            save.state.career = new StimCareerState
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

            Assert.IsTrue(StimSaveMigrator.TryMigrate(
                legacyJson, out var migrated, out var report, out var error), error);

            Assert.That(migrated.state.career.industryId, Is.EqualTo("finance"));
            Assert.That(migrated.state.career.employmentStatus, Is.EqualTo("employed"));
            Assert.That(report.changes, Has.Some.Matches<string>(change =>
                change.Contains("state.career.industryId=finance")));
            Assert.IsTrue(StimSaveMigrator.TryMigrate(
                JsonUtility.ToJson(migrated), out _, out var repeated, out error), error);
            Assert.IsFalse(repeated.changed);
        }

        [Test]
        public void Migrate_AddsUnclaimedRewardAuditFieldsToLegacyAchievements()
        {
            var save = CreateValidSave();
            save.state.achievements.Add(new StimAchievementState
            {
                achievementId = "first_job", unlockedAtAge = 18, revision = 1,
                timestampUtc = "2026-07-13T12:00:00Z"
            });
            var legacyJson = JsonUtility.ToJson(save).Replace(
                ",\"rewardClaimed\":false,\"rewardClaimedRevision\":0,\"rewardClaimedAtUtc\":\"\"",
                string.Empty);

            Assert.IsTrue(StimSaveMigrator.TryMigrate(
                legacyJson, out var migrated, out var report, out var error), error);

            Assert.That(migrated.state.achievements[0].rewardClaimed, Is.False);
            Assert.That(report.changes, Has.Some.Matches<string>(change =>
                change.Contains("reward claims created")));
        }

        [Test]
        public void Migrate_RejectsUnknownSaveFormatVersion()
        {
            var save = CreateValidSave();
            save.saveFormatVersion = 99;

            var migrated = StimSaveMigrator.TryMigrate(
                JsonUtility.ToJson(save), out _, out _, out var error);

            Assert.IsFalse(migrated);
            Assert.That(error, Contains.Substring("No migration path"));
        }

        private static StimSaveEnvelope CreateValidSave()
        {
            return new StimSaveEnvelope
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
                rng = new StimRngState { seed = 42, step = 0 },
                integrity = new StimSaveIntegrity { payloadHash = "sha256:legacy" },
                state = new StimGameState
                {
                    character = new StimCharacterState
                    {
                        age = 18,
                        health = 80,
                        happiness = 70,
                        smarts = 60,
                        looks = 50,
                        luck = 50
                    },
                    eventHistory = new List<StimEventHistoryEntry>(),
                    scheduledEvents = new List<StimScheduledEventRecord>()
                }
            };
        }
    }
}
