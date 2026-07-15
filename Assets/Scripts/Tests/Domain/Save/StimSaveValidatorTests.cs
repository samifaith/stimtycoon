using System.Collections.Generic;
using NUnit.Framework;
using StimTycoon.Saves;

namespace StimTycoon.Tests.Domain.Save
{
    /// <summary>
    /// Unit tests for the Phase 0 save schema validator.
    /// </summary>
    public class StimSaveValidatorTests
    {
        [Test]
        public void ValidateSave_RejectsNullSave()
        {
            var result = StimSaveValidator.ValidateSave(null);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Contains.Item("Save is null"));
        }

        [Test]
        public void ValidateSave_RejectsWrongFormatVersion()
        {
            var save = CreateValidSave();
            save.saveFormatVersion = 2;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("Save format version")));
        }

        [Test]
        public void ValidateSave_RejectsNewerMinimumReaderVersion()
        {
            var save = CreateValidSave();
            save.minimumReaderVersion = 2;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("Minimum reader version")));
        }

        [Test]
        public void ValidateSave_RejectsInvalidRevision()
        {
            var save = CreateValidSave();
            save.revision = 0;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("revision")));
        }

        [Test]
        public void ValidateSave_RejectsInvalidEventHistoryTimestamp()
        {
            var save = CreateValidSave();
            save.state.eventHistory[0].timestampUtc = "not-a-timestamp";

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(e => e.Contains("timestampUtc")));
        }

        [Test]
        public void ValidateSave_PassesValidSave()
        {
            var save = CreateValidSave();

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void ValidateSave_RejectsLooksOutsideCoreStatRange()
        {
            var save = CreateValidSave();
            save.state.character.looks = 101;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("looks")));
        }

        [Test]
        public void ValidateSave_RejectsLuckOutsideCoreStatRange()
        {
            var save = CreateValidSave();
            save.state.character.luck = -1;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("luck")));
        }

        [Test]
        public void ValidateSave_RejectsNegativeSkillExperience()
        {
            var save = CreateValidSave();
            save.state.skills.Add(new StimSkillState { skillId = "negotiation", experience = -1 });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("experience")));
        }

        [Test]
        public void ValidateSave_RejectsUnknownCareerIndustry()
        {
            var save = CreateValidSave();
            save.state.career.industryId = "unknown_industry";

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("industryId")));
        }

        [Test]
        public void ValidateSave_RejectsInvalidEmploymentStateAndWarningCount()
        {
            var save = CreateValidSave();
            save.state.career.employmentStatus = "between_things";
            save.state.career.performanceWarnings = 4;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("employmentStatus")));
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("warning counters")));
        }

        [Test]
        public void ValidateSave_RejectsInvalidBusinessAndOversizedLedger()
        {
            var save = CreateValidSave();
            save.state.business.status = "operating";
            save.state.business.businessId = "business_1";
            save.state.business.businessType = "local_services";
            save.state.business.displayName = "Local Services Co.";
            save.state.business.level = 4;
            for (var index = 0; index < 61; index++)
                save.state.business.ledger.Add(new StimBusinessLedgerEntry());

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("operating values")));
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("at most 60")));
        }

        [Test]
        public void ValidateSave_RejectsDuplicateOrInvalidGoalState()
        {
            var save = CreateValidSave();
            save.state.goals.Add(new StimGoalState
            {
                goalId = "goal_1", category = "daily", title = "Goal", description = "Do it",
                destination = "life", progress = 2, progressRequired = 1,
                status = "claimed", createdAtMonth = 1
            });
            save.state.goals.Add(new StimGoalState
            {
                goalId = "goal_1", category = "daily", title = "Duplicate", description = "Do it",
                destination = "life", progressRequired = 1, createdAtMonth = 1
            });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("state.goals")));
        }

        [Test]
        public void ValidateSave_RejectsClaimedAchievementWithoutClaimAuditMetadata()
        {
            var save = CreateValidSave();
            save.state.achievements.Add(new StimAchievementState
            {
                achievementId = "first_job", unlockedAtAge = 18, revision = 1,
                timestampUtc = "2026-07-13T17:00:00Z", rewardClaimed = true
            });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error =>
                error.Contains("reward claim metadata")));
        }

        [Test]
        public void ValidateSave_RejectsDuplicateRelationshipIds()
        {
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState { relationshipId = "parent", value = 50 });
            save.state.relationships.Add(new StimRelationshipState { relationshipId = "parent", value = 60 });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("duplicate id parent")));
        }

        [Test]
        public void ValidateSave_RejectsNegativeRelationshipNeglectCounter()
        {
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "friend",
                relationshipType = "friend",
                value = 50,
                monthsSinceInteraction = -1
            });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error =>
                error.Contains("monthsSinceInteraction")));
        }

        [Test]
        public void ValidateSave_RejectsUnderageOrIncompleteCompatibleDiscoveryIdentity()
        {
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "compatible_1",
                identityId = "identity_compatible_1",
                displayName = "Alex",
                pronouns = "they/them",
                genderIdentity = "nonbinary",
                orientation = "compatible_with_player",
                origin = "compatible_discovery",
                introducedAtAge = 17,
                relationshipType = "friend"
            });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error =>
                error.Contains("compatible identity is invalid")));
        }

        [Test]
        public void ValidateSave_AcceptsCompleteAdultCompatibleDiscoveryIdentity()
        {
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "compatible_1",
                identityId = "identity_compatible_1",
                displayName = "Alex",
                pronouns = "they/them",
                genderIdentity = "nonbinary",
                orientation = "compatible_with_player",
                origin = "compatible_discovery",
                introducedAtAge = 18,
                relationshipType = "friend"
            });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsTrue(result.isValid, StimSaveValidator.GetValidationSummary(result, save.saveId));
        }

        [TestCase(10, "independent")]
        [TestCase(18, "shared")]
        [TestCase(18, "household")]
        [TestCase(10, "unknown")]
        public void ValidateSave_RejectsAgeInappropriateOrUnknownChildCustody(int age, string custodyStatus)
        {
            var save = CreateValidSave();
            AddChild(save, age, custodyStatus);

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error =>
                error.Contains("state.family.children[0] is invalid")));
        }

        [Test]
        public void ValidateSave_RequiresDurableChildRelationshipWithMatchingLifeStage()
        {
            var save = CreateValidSave();
            AddChild(save, 8, "shared");
            save.state.relationships[0].relationshipType = "friend";

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error =>
                error.Contains("requires a matching child relationship")));
        }

        [Test]
        public void ValidateSave_AcceptsDependentAndIndependentChildBoundaries()
        {
            var dependent = CreateValidSave();
            AddChild(dependent, 17, "shared");
            var adult = CreateValidSave();
            AddChild(adult, 18, "independent");

            Assert.IsTrue(StimSaveValidator.ValidateSave(dependent).isValid);
            Assert.IsTrue(StimSaveValidator.ValidateSave(adult).isValid);
        }

        [Test]
        public void ValidateSave_AcceptsPersistentLifeDecision()
        {
            var save = CreateValidSave();
            save.state.lifeDecisions.Add(new StimLifeDecisionState
            {
                decisionId = "education_primary_enrollment",
                choiceId = "public_school",
                age = 6,
                monthOfYear = 1,
                revision = 12,
                timestampUtc = "2026-07-11T19:04:12Z"
            });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsTrue(result.isValid, StimSaveValidator.GetValidationSummary(result, save.saveId));
        }

        [Test]
        public void ValidateSave_RejectsDuplicateLifeDecisionIds()
        {
            var save = CreateValidSave();
            save.state.lifeDecisions.Add(new StimLifeDecisionState
                { decisionId = "education_primary_enrollment", choiceId = "public_school", monthOfYear = 1 });
            save.state.lifeDecisions.Add(new StimLifeDecisionState
                { decisionId = "education_primary_enrollment", choiceId = "homeschool", monthOfYear = 1 });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error =>
                error.Contains("state.lifeDecisions contains duplicate id education_primary_enrollment")));
        }

        [Test]
        public void ValidateSave_AcceptsActionProgressAndRejectsDuplicateInstances()
        {
            var save = CreateValidSave();
            save.state.actionProgress.Add(new StimActionProgressState
            {
                instanceId = "life:education:10:1:Read",
                actionId = "education.read",
                state = "Complete",
                progress = 1,
                progressRequired = 1,
                revision = 2
            });

            var valid = StimSaveValidator.ValidateSave(save);
            Assert.IsTrue(valid.isValid, StimSaveValidator.GetValidationSummary(valid, save.saveId));

            save.state.actionProgress.Add(new StimActionProgressState
            {
                instanceId = "life:education:10:1:Read",
                actionId = "education.read",
                state = "Ready",
                progressRequired = 1
            });
            var duplicate = StimSaveValidator.ValidateSave(save);
            Assert.IsFalse(duplicate.isValid);
            Assert.That(duplicate.errors, Has.Some.Matches<string>(error =>
                error.Contains("state.actionProgress contains duplicate id")));
        }

        [Test]
        public void ValidateSave_RejectsExpiredPersistedStatus()
        {
            var save = CreateValidSave();
            save.state.statuses.Add(new StimStatusState { statusId = "worried", remainingMonths = 0 });

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("remainingMonths")));
        }

        [Test]
        public void ValidateSave_RejectsNegativeMonthlyLivingExpenses()
        {
            var save = CreateValidSave();
            save.state.finances.monthlyLivingExpensesMinorUnits = -1;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("monthlyLivingExpenses")));
        }

        [Test]
        public void ValidateSave_RejectsTaxRateAboveOneHundredPercent()
        {
            var save = CreateValidSave();
            save.state.finances.taxRateBasisPoints = 10001;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("taxRateBasisPoints")));
        }

        [Test]
        public void ValidateSave_RejectsNegativeQuietMonthCounter()
        {
            var save = CreateValidSave();
            save.state.calendar.quietMonthsSinceEvent = -1;

            var result = StimSaveValidator.ValidateSave(save);

            Assert.IsFalse(result.isValid);
            Assert.That(result.errors, Has.Some.Matches<string>(error => error.Contains("quietMonthsSinceEvent")));
        }

        [Test]
        public void GetValidationSummary_CreatesHumanReadableSummary()
        {
            var result = new StimSaveValidationResult
            {
                isValid = false
            };
            result.errors.Add("Test error");
            result.warnings.Add("Test warning");

            var summary = StimSaveValidator.GetValidationSummary(result, "save_001");

            Assert.That(summary, Contains.Substring("save_001"));
            Assert.That(summary, Contains.Substring("ERRORS"));
            Assert.That(summary, Contains.Substring("Test error"));
            Assert.That(summary, Contains.Substring("WARNINGS"));
            Assert.That(summary, Contains.Substring("Test warning"));
        }

        private static StimSaveEnvelope CreateValidSave()
        {
            return new StimSaveEnvelope
            {
                saveFormatVersion = StimSaveSchema.SupportedSaveFormatVersion,
                minimumReaderVersion = StimSaveSchema.SupportedMinimumReaderVersion,
                gameBuildVersion = "0.1.0",
                contentVersion = "2026.07.11.1",
                saveId = "save_001",
                playerAccountId = "unity-player-id",
                lifeId = "life_001",
                createdAtUtc = "2026-07-11T19:00:00Z",
                updatedAtUtc = "2026-07-11T19:04:12Z",
                revision = 1,
                deviceIdHash = "device-hash",
                rng = new StimRngState
                {
                    seed = 742981,
                    step = 188
                },
                integrity = new StimSaveIntegrity
                {
                    payloadHash = "sha256",
                    previousRevisionHash = null
                },
                state = new StimGameState
                {
                    character = new StimCharacterState
                    {
                        age = 18,
                        health = 75,
                        happiness = 60,
                        smarts = 55
                    },
                    finances = new StimFinancesState
                    {
                        cashMinorUnits = 250000,
                        debtMinorUnits = 0
                    },
                    eventHistory = new List<StimEventHistoryEntry>
                    {
                        new StimEventHistoryEntry
                        {
                            eventId = "career_salary_negotiation_001",
                            choiceId = "make_the_case",
                            outcomeId = "success",
                            age = 18,
                            revision = 1,
                            timestampUtc = "2026-07-11T19:04:12Z"
                        }
                    },
                    scheduledEvents = new List<StimScheduledEventRecord>
                    {
                        new StimScheduledEventRecord
                        {
                            eventId = "health_follow_up_001",
                            earliestTriggerAge = 19,
                            latestTriggerAge = 20,
                            chance = 0.35f,
                            sourceEventId = "health_persistent_fatigue_001",
                            cancellationRule = "resolved_by_rest"
                        }
                    }
                }
            };
        }

        private static void AddChild(StimSaveEnvelope save, int age, string custodyStatus)
        {
            save.state.family.children.Add(new StimChildState
            {
                childId = "child_1",
                displayName = "Ari",
                path = "adoption",
                parentRelationshipId = "partner_1",
                joinedAtParentAge = 30,
                birthMonth = 4,
                age = age,
                custodyStatus = custodyStatus
            });
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "child_1",
                relationshipType = age >= 18 ? "adult_child" : "child",
                relationshipStage = age >= 18 ? "adult_child" : "dependent_child",
                value = 70,
                warmth = 70
            });
        }
    }
}
