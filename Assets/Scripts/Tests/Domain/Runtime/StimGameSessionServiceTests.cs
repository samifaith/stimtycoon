using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using StimTycoon.Abstractions;
using StimTycoon.Events;
using StimTycoon.Runtime;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class StimGameSessionServiceTests
    {
        [Test]
        public void ResolveChoice_IsDeterministicForSameSeedAndStep()
        {
            var resolver = new StimOutcomeResolver();
            var evt = RepresentativeStimEvents.CreateSalaryNegotiation();

            Assert.IsTrue(resolver.TryResolve(evt, "make_the_case", 42, 7, out var first, out _));
            Assert.IsTrue(resolver.TryResolve(evt, "make_the_case", 42, 7, out var second, out _));

            Assert.That(second.successRoll, Is.EqualTo(first.successRoll));
            Assert.That(second.outcomeRoll, Is.EqualTo(first.outcomeRoll));
            Assert.That(second.outcome.id, Is.EqualTo(first.outcome.id));
        }

        [Test]
        public void StartNewLife_CommitsBeforeReplacingActiveLife()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = StimNewLifeFactory.Create(
                new StimNewLifeRequest
                {
                    firstName = "Noah",
                    lastName = "Grant",
                    country = "USA",
                    backgroundId = StimNewLifeFactory.WorkingClassBackground
                },
                "0.1.0",
                DateTimeOffset.Parse("2026-07-13T20:00:00Z"),
                77);

            Assert.IsTrue(service.TryStartNewLife(save, out var summary), summary);
            Assert.That(repository.CommitCount, Is.EqualTo(1));
            Assert.That(service.ActiveSave.lifeId, Is.EqualTo(save.lifeId));
            Assert.That(service.ActiveSave.state.character.age, Is.Zero);
        }

        [TestCase(0, "infant", "not_started")]
        [TestCase(3, "early_childhood", "not_started")]
        [TestCase(6, "primary_school", "primary_school")]
        [TestCase(12, "secondary_school", "middle_school")]
        [TestCase(15, "secondary_school", "high_school")]
        [TestCase(18, "adult", "completed_secondary")]
        [TestCase(65, "retirement", "completed_secondary")]
        public void LifeAndEducationStages_FollowAge(int age, string lifeStage, string educationStage)
        {
            Assert.That(StimGameSessionService.GetLifeStage(age), Is.EqualTo(lifeStage));
            Assert.That(StimGameSessionService.GetEducationStage(age), Is.EqualTo(educationStage));
        }

        [TestCase(5, "primary_school", "started primary school")]
        [TestCase(11, "middle_school", "started middle school")]
        [TestCase(14, "high_school", "started high school")]
        [TestCase(17, "completed_secondary", "completed secondary school")]
        public void AnnualProgression_RecordsEducationTransitionsAsMilestones(
            int startingAge,
            string expectedStage,
            string expectedSummary)
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = startingAge;
            save.state.character.lifeStage = StimGameSessionService.GetLifeStage(startingAge);
            save.state.education.stage = StimGameSessionService.GetEducationStage(startingAge);
            save.state.calendar.monthOfYear = 12;
            save.state.career = new StimCareerState();
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);

            Assert.That(nextEvent, Is.Null);
            Assert.That(service.ActiveSave.state.education.stage, Is.EqualTo(expectedStage));
            Assert.That(summary, Does.Contain(expectedSummary));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "milestone" && entry.text.Contains(expectedSummary)), Is.True);
        }

        [Test]
        public void BirthLife_AdvancesIntoEarlyChildhoodWithoutCareerProgress()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = StimNewLifeFactory.Create(
                new StimNewLifeRequest
                {
                    firstName = "Kai",
                    lastName = "Reid",
                    country = "Jamaica",
                    backgroundId = StimNewLifeFactory.MiddleIncomeBackground
                },
                "0.1.0",
                DateTimeOffset.Parse("2026-07-13T20:00:00Z"),
                88);
            service.Start(save);

            string summary = null;
            for (var month = 0; month < 36; month++)
            {
                Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out summary), summary);
                Assert.That(nextEvent, Is.Null);
            }

            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(3));
            Assert.That(service.ActiveSave.state.character.lifeStage, Is.EqualTo("early_childhood"));
            Assert.That(service.ActiveSave.state.education.stage, Is.EqualTo("not_started"));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.Zero);
            Assert.That(summary, Does.Contain("entered early childhood"));
        }

        [Test]
        public void ResolveChoice_AppliesStateHistoryRevisionAndAutosave()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(
                catalog,
                repository,
                utcNow: () => DateTimeOffset.Parse("2026-07-13T18:00:00Z"));
            service.Start(CreateValidSave());

            var resolved = service.TryResolveChoice(
                RepresentativeStimEvents.SalaryNegotiationId,
                "let_it_pass",
                out var summary);

            Assert.IsTrue(resolved, summary);
            Assert.That(service.ActiveSave.revision, Is.EqualTo(2));
            Assert.That(service.ActiveSave.rng.step, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.eventHistory, Has.Count.EqualTo(1));
            Assert.That(service.ActiveSave.state.eventHistory[0].choiceId, Is.EqualTo("let_it_pass"));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
            Assert.That(repository.LastCommittedSave, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ResolveChoice_DoesNotMutateActiveSaveWhenCommitFails()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(catalog, repository);
            service.Start(CreateValidSave());

            var resolved = service.TryResolveChoice(
                RepresentativeStimEvents.SalaryNegotiationId,
                "let_it_pass",
                out _);

            Assert.IsFalse(resolved);
            Assert.That(service.ActiveSave.revision, Is.EqualTo(1));
            Assert.That(service.ActiveSave.rng.step, Is.EqualTo(0));
            Assert.That(service.ActiveSave.state.eventHistory, Is.Empty);
        }

        [Test]
        public void ResolveChoice_AppliesPersistentSalaryIncrease()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(catalog, repository);
            var save = CreateValidSave();
            save.rng.seed = 0;
            service.Start(save);

            var resolved = service.TryResolveChoice(
                RepresentativeStimEvents.SalaryNegotiationId,
                "make_the_case",
                out var summary);

            Assert.IsTrue(resolved, summary);
            Assert.That(service.LastResolution.outcome.id, Is.EqualTo("raise_approved"));
            Assert.That(service.ActiveSave.state.career.annualSalaryMinorUnits, Is.EqualTo(5500000));
            var resolvedFeedEntry = service.ActiveSave.state.lifeFeed.FindLast(entry =>
                entry != null && entry.category == "event");
            Assert.That(resolvedFeedEntry, Is.Not.Null);
            Assert.That(resolvedFeedEntry.category, Is.EqualTo("event"));
            Assert.That(resolvedFeedEntry.text, Does.Contain("Negotiated a salary increase."));
            Assert.That(resolvedFeedEntry.text, Does.Contain("Your manager approves a meaningful raise."));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var paycheckSummary), paycheckSummary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(558334));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(76));
        }

        [Test]
        public void OutcomeFeedText_FallsBackToTheResolvedResult()
        {
            var outcome = new Outcome
            {
                resultTextKey = "The exact resolved outcome remains visible.",
                feedEntryKey = string.Empty
            };

            Assert.That(
                StimGameSessionService.BuildOutcomeFeedText(outcome),
                Is.EqualTo("The exact resolved outcome remains visible."));
        }

        [Test]
        public void AdvanceMonth_PaysIncrementallyThenEnforcesAnnualCooldown()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(catalog, repository);
            var save = CreateValidSave();
            save.state.eventHistory.Add(new StimEventHistoryEntry
            {
                eventId = RepresentativeStimEvents.SalaryNegotiationId,
                choiceId = "let_it_pass",
                outcomeId = "status_quo",
                age = 24,
                revision = 1,
                timestampUtc = "2026-07-13T17:00:00Z"
            });
            service.Start(save);

            StimEvent quietYearEvent = null;
            string quietSummary = null;
            for (var month = 1; month <= 12; month++)
            {
                Assert.IsTrue(service.TryAdvanceMonth(out quietYearEvent, out quietSummary), quietSummary);
                if (month == 1)
                {
                    Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(516667));
                    Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(24));
                }
            }

            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(25));
            Assert.That(service.ActiveSave.state.calendar.monthOfYear, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.EqualTo(12));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(82));
            Assert.That(quietYearEvent, Is.Null);

            StimEvent nextEvent = null;
            string eventSummary = null;
            for (var month = 1; month <= 12; month++)
            {
                Assert.IsTrue(service.TryAdvanceMonth(out nextEvent, out eventSummary), eventSummary);
            }

            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(26));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.EqualTo(24));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(94));
            Assert.That(nextEvent?.id, Is.EqualTo(RepresentativeStimEvents.SalaryNegotiationId));
            Assert.That(service.ActiveSave.state.pendingEventId, Is.EqualTo(nextEvent.id));
            Assert.That(repository.CommitCount, Is.EqualTo(24));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(10100000));
            Assert.That(repository.LastCommittedSave, Contains.Substring(RepresentativeStimEvents.SalaryNegotiationId));
        }

        [Test]
        public void AdvanceMonth_DoesNotMutateActiveSaveWhenCommitFails()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(catalog, repository);
            service.Start(CreateValidSave());

            var advanced = service.TryAdvanceMonth(out var nextEvent, out _);

            Assert.IsFalse(advanced);
            Assert.That(nextEvent, Is.Null);
            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(24));
            Assert.That(service.ActiveSave.state.calendar.monthOfYear, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(100000));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.EqualTo(0));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(70));
            Assert.That(service.ActiveSave.revision, Is.EqualTo(1));
            Assert.That(service.ActiveSave.rng.step, Is.EqualTo(0));
        }

        [Test]
        public void AdvanceMonth_RejectsUnresolvedPendingEvent()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(catalog, repository);
            var save = CreateValidSave();
            save.state.pendingEventId = RepresentativeStimEvents.SalaryNegotiationId;
            service.Start(save);

            var advanced = service.TryAdvanceMonth(out var nextEvent, out var summary);

            Assert.IsFalse(advanced);
            Assert.That(nextEvent, Is.Null);
            Assert.That(summary, Contains.Substring("Resolve pending event"));
            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(24));
            Assert.That(repository.CommitCount, Is.EqualTo(0));
        }

        [Test]
        public void AdvanceYear_ReusesTwelveMonthlyTransactionsAndAutosavesEachMonth()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 24;
            save.state.calendar.monthOfYear = 1;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceYear(
                out var monthsProcessed, out var nextEvent, out var summary), summary);

            Assert.That(monthsProcessed, Is.EqualTo(12));
            Assert.That(nextEvent, Is.Null);
            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(25));
            Assert.That(service.ActiveSave.state.calendar.monthOfYear, Is.EqualTo(1));
            Assert.That(repository.CommitCount, Is.EqualTo(13));
            Assert.That(summary, Does.Contain("Advanced 12 months").And.Contain("Twelve months completed"));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].category, Is.EqualTo("year"));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].text,
                Does.Contain("Advanced 12 months").And.Contain("Age +1").And.Contain("Twelve months completed"));
        }

        [Test]
        public void AdvanceYear_StopsAtFirstAuthoredEvent()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 12;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceYear(
                out var monthsProcessed, out var nextEvent, out var summary), summary);

            Assert.That(monthsProcessed, Is.EqualTo(1));
            Assert.That(nextEvent?.id, Is.EqualTo(RepresentativeStimEvents.SalaryNegotiationId));
            Assert.That(service.ActiveSave.state.pendingEventId,
                Is.EqualTo(RepresentativeStimEvents.SalaryNegotiationId));
            Assert.That(summary, Does.Contain("Stopped for The annual review"));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].category, Is.EqualTo("year"));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].text,
                Does.Contain("Advanced 1 month").And.Contain("Stopped for The annual review"));
        }

        [Test]
        public void AdvanceMonth_TwelveCreatesPersistedYearReviewWithAccumulatedChanges()
        {
            var catalog = new InMemoryStimEventCatalog();
            var reviewEvent = RepresentativeStimEvents.CreateYearInReview();
            Assert.That(StimEventValidator.ValidateEvent(reviewEvent).isValid, Is.True);
            catalog.Upsert(reviewEvent);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 12;
            save.state.character.age = 24;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);

            Assert.That(nextEvent?.id, Is.EqualTo(RepresentativeStimEvents.YearInReviewId));
            Assert.That(service.ActiveSave.state.pendingEventId, Is.EqualTo(RepresentativeStimEvents.YearInReviewId));
            Assert.That(service.ActiveSave.state.annualReview.completedAtAge, Is.EqualTo(25));
            Assert.That(service.ActiveSave.state.annualReview.monthsAccumulated, Is.EqualTo(12));
            Assert.That(StimGameSessionService.BuildAnnualReviewSummary(service.ActiveSave.state),
                Does.Contain("Age 25 review").And.Contain("Cash"));
        }

        [Test]
        public void YearInReviewReward_IsPersistedAndDuplicateSafe()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateYearInReview());
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(catalog, repository);
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 12;
            save.state.character.age = 24;
            service.Start(save);
            Assert.IsTrue(service.TryAdvanceMonth(out _, out _));
            var cashBeforeReward = service.ActiveSave.state.finances.cashMinorUnits;

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.YearInReviewId, "build_security", out var firstSummary), firstSummary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBeforeReward + 50000));
            Assert.That(service.ActiveSave.state.annualReview.rewardedAtAge, Is.EqualTo(25));

            Assert.IsFalse(service.TryResolveChoice(
                RepresentativeStimEvents.YearInReviewId, "build_security", out var duplicateSummary));
            Assert.That(duplicateSummary, Does.Contain("already claimed"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBeforeReward + 50000));
            Assert.That(repository.CommitCount, Is.EqualTo(2));
        }

        [Test]
        public void AnnualAccumulator_CapturesSkillsSavingsAndDeterministicallyOrderedHighlights()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateYearInReview());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 1;
            service.Start(save);
            Assert.IsTrue(service.TryAdvanceMonth(out _, out _));
            service.ActiveSave.state.skills.Add(new StimSkillState { skillId = "learning", experience = 10 });
            service.ActiveSave.state.finances.savingsMinorUnits = 25000;
            service.ActiveSave.state.lifeFeed.Add(new StimLifeFeedEntry
            {
                entryId = "later", category = "career", text = "Career highlight", revision = 20
            });
            service.ActiveSave.state.lifeFeed.Add(new StimLifeFeedEntry
            {
                entryId = "earlier", category = "education", text = "Education highlight", revision = 10
            });
            service.ActiveSave.state.calendar.monthOfYear = 12;

            Assert.IsTrue(service.TryAdvanceMonth(out var reviewEvent, out var summary), summary);
            var review = service.ActiveSave.state.annualReview;
            Assert.That(reviewEvent?.id, Is.EqualTo(RepresentativeStimEvents.YearInReviewId));
            Assert.That(review.skillExperienceDelta, Is.EqualTo(10));
            Assert.That(review.savingsDeltaMinorUnits, Is.EqualTo(25073));
            Assert.That(service.ActiveSave.state.finances.lastSavingsInterestMinorUnits, Is.EqualTo(73));
            Assert.That(review.majorOutcomeSummaries, Does.Contain("Education highlight"));
            Assert.That(review.majorOutcomeSummaries, Does.Contain("Career highlight"));
            Assert.That(review.majorOutcomeSummaries.IndexOf("Education highlight"),
                Is.LessThan(review.majorOutcomeSummaries.IndexOf("Career highlight")));
            Assert.That(review.majorOutcomeSummaries, Has.Count.LessThanOrEqualTo(5));
        }

        [Test]
        public void YearInReview_PersistenceFailureRollsBackRewardAndArchive()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateYearInReview());
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(catalog, repository);
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 12;
            service.Start(save);
            Assert.IsTrue(service.TryAdvanceMonth(out _, out _));
            var cashBefore = service.ActiveSave.state.finances.cashMinorUnits;
            repository.ShouldCommit = false;

            Assert.IsFalse(service.TryResolveChoice(
                RepresentativeStimEvents.YearInReviewId, "build_security", out _));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore));
            Assert.That(service.ActiveSave.state.annualReview.rewardedAtAge, Is.EqualTo(-1));
            Assert.That(service.ActiveSave.state.annualReviewHistory, Is.Empty);
            Assert.That(service.ActiveSave.state.pendingEventId, Is.EqualTo(RepresentativeStimEvents.YearInReviewId));
        }

        [Test]
        public void YearInReview_ReloadPreservesPendingEntitlementAndSingleClaim()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateYearInReview());
            var repository = new RecordingSaveRepository();
            var firstSession = new StimGameSessionService(catalog, repository);
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 12;
            firstSession.Start(save);
            Assert.IsTrue(firstSession.TryAdvanceMonth(out _, out _));

            var resumed = new StimGameSessionService(catalog, repository);
            Assert.IsTrue(resumed.TryLoadLatest(out var loadSummary), loadSummary);
            Assert.That(resumed.ActiveSave.state.pendingEventId,
                Is.EqualTo(RepresentativeStimEvents.YearInReviewId));
            Assert.IsTrue(resumed.TryResolveChoice(
                RepresentativeStimEvents.YearInReviewId, "nurture_connections", out var claimSummary), claimSummary);
            Assert.IsFalse(resumed.TryResolveChoice(
                RepresentativeStimEvents.YearInReviewId, "nurture_connections", out _));
            Assert.That(resumed.ActiveSave.state.annualReviewHistory, Has.Count.EqualTo(1));
        }

        [Test]
        public void AnnualReviewArchive_RetainsNewestTenCompletedYears()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateYearInReview());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            service.Start(save);

            for (var age = 25; age <= 35; age++)
            {
                service.ActiveSave.state.character.age = age;
                service.ActiveSave.state.annualReview.completedAtAge = age;
                service.ActiveSave.state.annualReview.rewardedAtAge = age - 1;
                service.ActiveSave.state.pendingEventId = RepresentativeStimEvents.YearInReviewId;
                Assert.IsTrue(service.TryResolveChoice(
                    RepresentativeStimEvents.YearInReviewId, "invest_in_growth", out var summary), summary);
            }

            Assert.That(service.ActiveSave.state.annualReviewHistory, Has.Count.EqualTo(10));
            Assert.That(service.ActiveSave.state.annualReviewHistory[0].completedAtAge, Is.EqualTo(26));
            Assert.That(service.ActiveSave.state.annualReviewHistory[9].completedAtAge, Is.EqualTo(35));
            Assert.That(StimSaveValidator.ValidateSave(service.ActiveSave).isValid, Is.True);
        }

        [Test]
        public void AdvanceYear_StopsForRequiredSchoolPathDecision()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 11;
            save.state.character.lifeStage = StimGameSessionService.GetLifeStage(11);
            save.state.education.stage = StimGameSessionService.GetEducationStage(11);
            save.state.calendar.monthOfYear = 12;
            save.state.career = new StimCareerState();
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceYear(
                out var monthsProcessed, out var nextEvent, out var summary), summary);

            Assert.That(monthsProcessed, Is.EqualTo(1));
            Assert.That(nextEvent, Is.Null);
            Assert.That(service.ActiveSave.state.education.awaitingDecisionId, Is.Not.Null.And.Not.Empty);
            Assert.That(summary, Does.Contain("required school-path decision"));
        }

        [Test]
        public void ResolveChoice_AppliesSkillRelationshipAndTimedStatusEffects()
        {
            var evt = RepresentativeStimEvents.CreateSalaryNegotiation();
            var choice = evt.choices.Find(candidate => candidate.id == "make_the_case");
            foreach (var outcome in choice.outcomes)
            {
                outcome.effects.Add(new Effect { type = EffectType.SkillXp, targetId = "negotiation", value = 15 });
                outcome.effects.Add(new Effect { type = EffectType.RelationshipDelta, targetId = "manager", value = -7 });
                outcome.effects.Add(new Effect { type = EffectType.StatusAdd, targetId = "focused", value = 2 });
            }

            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(evt);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            service.Start(CreateValidSave());

            Assert.IsTrue(service.TryResolveChoice(evt.id, choice.id, out var summary), summary);
            Assert.That(service.ActiveSave.state.skills.Find(skill => skill.skillId == "negotiation").experience, Is.EqualTo(15));
            Assert.That(service.ActiveSave.state.relationships.Find(relationship => relationship.relationshipId == "manager").value, Is.EqualTo(43));
            Assert.That(service.ActiveSave.state.statuses.Find(status => status.statusId == "focused").remainingMonths, Is.EqualTo(2));
        }

        [Test]
        public void AdvanceMonth_DecrementsAndExpiresTimedStatuses()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.statuses.Add(new StimStatusState { statusId = "focused", remainingMonths = 2 });
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var firstSummary), firstSummary);
            Assert.That(service.ActiveSave.state.statuses[0].remainingMonths, Is.EqualTo(1));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var secondSummary), secondSummary);
            Assert.That(service.ActiveSave.state.statuses, Is.Empty);
        }

        [Test]
        public void PerformActivity_StudyAppliesStatsXpCooldownAndAutosave()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                repository,
                utcNow: () => DateTimeOffset.Parse("2026-07-13T20:00:00Z"));
            service.Start(CreateValidSave());

            var performed = service.TryPerformActivity(StimActivityType.Study, out var summary);

            Assert.IsTrue(performed, summary);
            Assert.That(summary, Does.Contain("Smarts +2"));
            Assert.That(summary, Does.Contain("Happiness −1"));
            Assert.That(service.ActiveSave.state.character.smarts, Is.EqualTo(62));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(69));
            Assert.That(service.ActiveSave.state.skills.Find(skill => skill.skillId == "learning").experience, Is.EqualTo(10));
            Assert.That(service.ActiveSave.state.statuses.Find(status => status.statusId == "monthly_focus_used").remainingMonths, Is.EqualTo(1));
            Assert.That(service.ActiveSave.revision, Is.EqualTo(2));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void PerformActivity_WorkoutAppliesHealthHappinessAndFitnessXp()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                repository);
            service.Start(CreateValidSave());

            var performed = service.TryPerformActivity(StimActivityType.Workout, out var summary);

            Assert.IsTrue(performed, summary);
            Assert.That(summary, Does.Contain("Health +2"));
            Assert.That(summary, Does.Contain("Happiness +1"));
            Assert.That(service.ActiveSave.state.character.health, Is.EqualTo(82));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(71));
            Assert.That(service.ActiveSave.state.skills.Find(skill => skill.skillId == "fitness").experience, Is.EqualTo(10));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void HomeActions_ApplyCostsBenefitsFeedAndIndependentMonthlyCooldowns()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            var startingCash = save.state.finances.cashMinorUnits;
            service.Start(save);

            Assert.IsTrue(service.TryPerformHomeAction(StimHomeActionType.Read, out var readSummary), readSummary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(startingCash - 500));
            Assert.That(service.ActiveSave.state.character.smarts, Is.EqualTo(61));
            Assert.That(service.ActiveSave.state.skills.Find(skill => skill.skillId == "learning").experience, Is.EqualTo(8));
            Assert.That(service.ActiveSave.state.home.condition, Is.EqualTo(79));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].category, Is.EqualTo("home"));
            Assert.IsFalse(service.TryPerformHomeAction(StimHomeActionType.Read, out var cooldownSummary));
            Assert.That(cooldownSummary, Does.Contain("already completed"));

            Assert.IsTrue(service.TryPerformHomeAction(StimHomeActionType.Rest, out var restSummary), restSummary);
            Assert.That(service.ActiveSave.state.character.health, Is.EqualTo(83));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(72));
            Assert.That(repository.CommitCount, Is.EqualTo(2));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var advanceSummary), advanceSummary);
            Assert.IsTrue(service.TryPerformHomeAction(StimHomeActionType.Read, out readSummary), readSummary);
        }

        [Test]
        public void HomeAction_RejectsUnaffordableCostAndRollsBackFailedCommit()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 499;
            service.Start(save);

            Assert.IsFalse(service.TryPerformHomeAction(StimHomeActionType.Read, out var fundsSummary));
            Assert.That(fundsSummary, Does.Contain("costs"));
            Assert.That(service.ActiveSave.state.character.smarts, Is.EqualTo(60));

            service.ActiveSave.state.finances.cashMinorUnits = 5000;
            repository.ShouldCommit = false;
            Assert.IsFalse(service.TryPerformHomeAction(StimHomeActionType.Maintain, out var failedSummary));
            Assert.That(failedSummary, Is.EqualTo("failed"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(5000));
            Assert.That(service.ActiveSave.state.home.condition, Is.EqualTo(80));
            Assert.That(service.ActiveSave.state.statuses.Exists(status => status.statusId == "home_maintain_used"), Is.False);
        }

        [Test]
        public void HomeUpgrade_ConsumesCashAndProgressAndImprovesBenefits()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 100000;
            save.state.home.improvementProgress = 10;
            service.Start(save);

            Assert.IsTrue(service.TryUpgradeHome(out var upgradeSummary), upgradeSummary);
            Assert.That(service.ActiveSave.state.home.upgradeLevel, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.home.improvementProgress, Is.Zero);
            Assert.That(service.ActiveSave.state.home.condition, Is.EqualTo(100));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(50000));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].text, Does.Contain("level 1"));

            Assert.IsTrue(service.TryPerformHomeAction(StimHomeActionType.Read, out var readSummary), readSummary);
            Assert.That(service.ActiveSave.state.skills.Find(skill => skill.skillId == "learning").experience, Is.EqualTo(10));
            Assert.That(readSummary, Does.Contain("Learning XP +10"));
        }

        [Test]
        public void HomeInventory_IsConsumedBlockedAndRestoredByMaintenance()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.home.readingMaterialStock = 1;
            save.state.home.trainingEquipmentCondition = 10;
            service.Start(save);

            Assert.IsTrue(service.TryPerformHomeAction(StimHomeActionType.Read, out var read), read);
            Assert.That(service.ActiveSave.state.home.readingMaterialStock, Is.Zero);
            service.ActiveSave.state.statuses.RemoveAll(status => status.statusId == "home_read_used");
            Assert.IsFalse(service.TryPerformHomeAction(StimHomeActionType.Read, out var empty));
            Assert.That(empty, Does.Contain("No reading materials"));

            Assert.IsTrue(service.TryPerformHomeAction(StimHomeActionType.Train, out var train), train);
            Assert.That(service.ActiveSave.state.home.trainingEquipmentCondition, Is.Zero);
            service.ActiveSave.state.statuses.RemoveAll(status => status.statusId == "home_train_used");
            Assert.IsFalse(service.TryPerformHomeAction(StimHomeActionType.Train, out var unsafeSummary));
            Assert.That(unsafeSummary, Does.Contain("needs maintenance"));

            Assert.IsTrue(service.TryPerformHomeAction(StimHomeActionType.Maintain, out var maintain), maintain);
            Assert.That(service.ActiveSave.state.home.readingMaterialStock, Is.EqualTo(3));
            Assert.That(service.ActiveSave.state.home.trainingEquipmentCondition, Is.EqualTo(30));
        }

        [Test]
        public void HomeContentContract_HasStableUniqueRoomObjectActionsAndRejectsDuplicates()
        {
            var starter = StimHomeContentCatalog.Get("starter_home");
            var valid = StimHomeContentCatalog.Validate(starter);

            Assert.IsTrue(valid.isValid, string.Join("; ", valid.errors));
            Assert.That(starter.actions, Has.Count.EqualTo(5));
            Assert.That(starter.actions.ConvertAll(action => action.actionId), Is.Unique);
            Assert.That(starter.actions.ConvertAll(action => action.roomObjectId),
                Does.Contain("bookshelf").And.Contain("training_corner").And.Contain("bed"));
            Assert.That(StimHomeContentCatalog.GetAction("starter_home", StimHomeActionType.Read).costMinorUnits,
                Is.EqualTo(500));

            var duplicate = new StimHomeDefinition
            {
                homeId = "duplicate_home",
                displayName = "Duplicate Home",
                startingCondition = 80,
                maxUpgradeLevel = 1,
                actions = new List<StimHomeActionDefinition>
                {
                    starter.actions[0],
                    starter.actions[0]
                }
            };
            Assert.IsFalse(StimHomeContentCatalog.Validate(duplicate).isValid);
        }

        [Test]
        public void AdvanceMonth_LowHomeConditionAddsExpensesAndHouseholdConsequences()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.home.condition = 20;
            save.state.finances.monthlyLivingExpensesMinorUnits = 100000;
            save.state.finances.cashMinorUnits = 1000000;
            save.state.character.happiness = 70;
            save.state.household.cohesion = 50;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var summary), summary);

            Assert.That(service.ActiveSave.state.finances.lastExpensesMinorUnits, Is.EqualTo(110000));
            Assert.That(service.ActiveSave.state.home.condition, Is.EqualTo(19));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(69));
            Assert.That(service.ActiveSave.state.household.cohesion, Is.EqualTo(49));
        }

        [Test]
        public void LowHomeCondition_TriggersAuthoredRepairEventAndRepairRestoresCondition()
        {
            var evt = RepresentativeStimEvents.CreateHomeDeferredMaintenance();
            Assert.IsTrue(StimEventValidator.ValidateEvent(evt).isValid);
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(evt);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.home.condition = 20;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var selected, out var advanceSummary), advanceSummary);
            Assert.That(selected?.id, Is.EqualTo(RepresentativeStimEvents.HomeDeferredMaintenanceId));
            Assert.That(service.ActiveSave.state.home.condition, Is.EqualTo(19));
            var cashBeforeRepair = service.ActiveSave.state.finances.cashMinorUnits;

            Assert.IsTrue(service.TryResolveChoice(evt.id, "repair_now", out var repairSummary), repairSummary);
            Assert.That(service.ActiveSave.state.home.condition, Is.EqualTo(49));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBeforeRepair - 7500));
        }

        [Test]
        public void PerformActivity_EnforcesLifeStageAndSupportsChildPlay()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 4;
            service.Start(save);

            Assert.IsFalse(service.TryPerformActivity(StimActivityType.Study, out var unavailable));
            Assert.That(unavailable, Does.Contain("not available at age 4"));
            Assert.IsTrue(service.TryPerformActivity(StimActivityType.Play, out var summary), summary);
            Assert.That(summary, Does.Contain("Happiness +3"));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(73));
            Assert.That(service.ActiveSave.state.character.health, Is.EqualTo(81));
        }

        [Test]
        public void PerformRelationshipInteraction_AppliesEffectsFeedCooldownAndAutosave()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 10;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "parent_1",
                displayName = "Jordan Grant",
                relationshipType = "parent",
                value = 60
            });
            service.Start(save);

            var performed = service.TryPerformRelationshipInteraction(
                "parent_1",
                StimRelationshipInteractionType.PlayTogether,
                out var summary);

            Assert.IsTrue(performed, summary);
            Assert.That(summary, Does.Contain("Relationship +4").And.Contain("Happiness +2"));
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(64));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(72));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry => entry.category == "relationship"), Is.True);
            Assert.That(service.ActiveSave.state.statuses.Exists(
                status => status.statusId == "relationship_interaction_used_parent_1"), Is.True);
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void CompatibleDiscovery_PersistsIdentityContextWarmthStageAndBoundedHistory()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), repository,
                utcNow: () => DateTimeOffset.Parse("2026-07-14T12:00:00Z"));
            service.Start(CreateValidSave());

            Assert.IsTrue(service.TryDiscoverCompatiblePerson(out var relationshipId, out var summary), summary);
            var person = service.ActiveSave.state.relationships.Find(
                relationship => relationship.relationshipId == relationshipId);
            Assert.That(person, Is.Not.Null);
            Assert.That(person.identityId, Is.Not.Empty);
            Assert.That(person.displayName, Is.Not.Empty);
            Assert.That(person.pronouns, Is.Not.Empty);
            Assert.That(person.orientation, Is.EqualTo("compatible_with_player"));
            Assert.That(person.relationshipStage, Is.EqualTo("introduced"));
            Assert.That(person.introductionContext, Is.Not.Empty);
            Assert.That(person.warmth, Is.EqualTo(50));
            Assert.That(person.relationshipHistory, Has.Count.EqualTo(1));
            Assert.That(person.relationshipHistory[0].type, Is.EqualTo("introduced"));
            Assert.IsFalse(service.TryDiscoverCompatiblePerson(out _, out var cooldown));
            Assert.That(cooldown, Does.Contain("already met someone"));

            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                relationshipId, StimRelationshipInteractionType.Talk, out var talk), talk);
            person = service.ActiveSave.state.relationships.Find(
                relationship => relationship.relationshipId == relationshipId);
            Assert.That(person.warmth, Is.EqualTo(52));
            Assert.That(person.relationshipHistory, Has.Count.EqualTo(2));

            var reloaded = JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave);
            Assert.IsTrue(StimSaveValidator.ValidateSave(reloaded).isValid);
            Assert.That(reloaded.state.relationships.Find(
                relationship => relationship.relationshipId == relationshipId).relationshipHistory,
                Has.Count.EqualTo(2));
        }

        [Test]
        public void CompatibleDiscovery_EnforcesAdultGateAndRollsBackFailedCommit()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 17;
            service.Start(save);

            Assert.IsFalse(service.TryDiscoverCompatiblePerson(out _, out var ageSummary));
            Assert.That(ageSummary, Does.Contain("18 and older"));
            service.ActiveSave.state.character.age = 18;
            repository.ShouldCommit = false;

            Assert.IsFalse(service.TryDiscoverCompatiblePerson(out var relationshipId, out var failedSummary));
            Assert.That(relationshipId, Is.Empty);
            Assert.That(failedSummary, Is.EqualTo("failed"));
            Assert.That(service.ActiveSave.state.relationships, Is.Empty);
            Assert.That(service.ActiveSave.state.statuses, Is.Empty);
        }

        [Test]
        public void PerformRelationshipInteraction_LimitsEachRelationshipOncePerMonth()
        {
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
                { relationshipId = "parent_1", displayName = "Jordan", relationshipType = "parent", value = 50 });
            save.state.relationships.Add(new StimRelationshipState
                { relationshipId = "parent_2", displayName = "Morgan", relationshipType = "parent", value = 50 });
            service.Start(save);

            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "parent_1", StimRelationshipInteractionType.Talk, out var firstSummary), firstSummary);
            Assert.IsFalse(service.TryPerformRelationshipInteraction(
                "parent_1", StimRelationshipInteractionType.SpendTime, out var duplicateSummary));
            Assert.That(duplicateSummary, Does.Contain("already spent focused time"));
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "parent_2", StimRelationshipInteractionType.SpendTime, out var secondSummary), secondSummary);
        }

        [TestCase(7, StimRelationshipInteractionType.Argue, false)]
        [TestCase(8, StimRelationshipInteractionType.Argue, true)]
        [TestCase(12, StimRelationshipInteractionType.PlayTogether, true)]
        [TestCase(13, StimRelationshipInteractionType.PlayTogether, false)]
        [TestCase(17, StimRelationshipInteractionType.AskForHelp, true)]
        [TestCase(18, StimRelationshipInteractionType.AskForHelp, false)]
        [TestCase(17, StimRelationshipInteractionType.AskOnDate, false)]
        [TestCase(18, StimRelationshipInteractionType.AskOnDate, true)]
        [TestCase(20, StimRelationshipInteractionType.Commit, false)]
        [TestCase(21, StimRelationshipInteractionType.Commit, true)]
        [TestCase(17, StimRelationshipInteractionType.DateNight, false)]
        [TestCase(18, StimRelationshipInteractionType.DateNight, true)]
        [TestCase(17, StimRelationshipInteractionType.Separate, false)]
        [TestCase(18, StimRelationshipInteractionType.Separate, true)]
        [TestCase(17, StimRelationshipInteractionType.BreakUp, false)]
        [TestCase(18, StimRelationshipInteractionType.BreakUp, true)]
        [TestCase(70, StimRelationshipInteractionType.Talk, true)]
        public void RelationshipInteractions_RespectAgeRules(
            int age,
            StimRelationshipInteractionType interactionType,
            bool expected)
        {
            Assert.That(
                StimGameSessionService.IsRelationshipInteractionAgeAppropriate(interactionType, age),
                Is.EqualTo(expected));
        }

        [Test]
        public void AdultFriendship_CanBecomePartnershipAndEndWithPersistentFeedHistory()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "friend_adult_1",
                displayName = "Avery",
                relationshipType = "friend",
                value = 70
            });
            service.Start(save);

            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "friend_adult_1", StimRelationshipInteractionType.AskOnDate, out var dateSummary), dateSummary);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("dating"));
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(75));
            Assert.IsFalse(service.TryPerformRelationshipInteraction(
                "friend_adult_1", StimRelationshipInteractionType.Commit, out var cooldownSummary));
            Assert.That(cooldownSummary, Does.Contain("already spent focused time"));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var firstAdvanceSummary), firstAdvanceSummary);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "friend_adult_1", StimRelationshipInteractionType.Commit, out var commitSummary), commitSummary);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("partner"));
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(80));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var secondAdvanceSummary), secondAdvanceSummary);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "friend_adult_1", StimRelationshipInteractionType.BreakUp, out var breakupSummary), breakupSummary);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("ex_partner"));
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(60));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(74));
            Assert.That(service.ActiveSave.state.lifeFeed.FindAll(
                entry => entry != null && entry.category == "relationship"), Has.Count.EqualTo(3));
            Assert.That(repository.CommitCount, Is.EqualTo(5));
        }

        [Test]
        public void RelationshipLifecycle_RequiresAndPersistsFriendshipRomanceSeparationAndRecoveryStages()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 24;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "compatible_1",
                identityId = "identity_1",
                displayName = "Alex Morgan",
                relationshipType = "friend",
                relationshipStage = "friendship",
                value = 65,
                warmth = 60
            });
            service.Start(save);

            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "compatible_1", StimRelationshipInteractionType.DeepenFriendship, out var deepen), deepen);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("best_friend"));
            Assert.That(service.ActiveSave.state.relationships[0].relationshipStage, Is.EqualTo("close_friendship"));

            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "compatible_1", StimRelationshipInteractionType.AskOnDate, out var date), date);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipStage, Is.EqualTo("dating"));

            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "compatible_1", StimRelationshipInteractionType.DateNight, out var dateNight), dateNight);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipStage, Is.EqualTo("romantic_growth"));

            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "compatible_1", StimRelationshipInteractionType.Commit, out var commit), commit);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("partner"));

            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "compatible_1", StimRelationshipInteractionType.Separate, out var separate), separate);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipStage, Is.EqualTo("separated"));

            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "compatible_1", StimRelationshipInteractionType.Recover, out var recover), recover);
            var relationship = service.ActiveSave.state.relationships[0];
            Assert.That(relationship.relationshipType, Is.EqualTo("friend"));
            Assert.That(relationship.relationshipStage, Is.EqualTo("recovered_friendship"));
            Assert.That(relationship.relationshipHistory, Has.Count.EqualTo(6));
            Assert.That(relationship.relationshipHistory.ConvertAll(entry => entry.type),
                Is.EqualTo(new[] { "deepenfriendship", "askondate", "datenight", "commit", "separate", "recover" }));
            Assert.That(JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave)
                .state.relationships[0].relationshipStage, Is.EqualTo("recovered_friendship"));
        }

        [Test]
        public void FamilyPlanning_RequiresMutualDiscussionAndPregnancyCreatesDurableChild()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "partner_1", displayName = "Alex", relationshipType = "partner",
                relationshipStage = "partnered", value = 80, warmth = 70
            });
            service.Start(save);

            Assert.IsFalse(service.TryChooseFamilyPlanning(
                "partner_1", StimFamilyPlanningAction.TryForChild, out var consentRequired));
            Assert.That(consentRequired, Does.Contain("Both partners"));
            Assert.IsTrue(service.TryChooseFamilyPlanning(
                "partner_1", StimFamilyPlanningAction.Discuss, out var discussed), discussed);
            Assert.IsTrue(service.ActiveSave.state.family.partnerConsent);

            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryChooseFamilyPlanning(
                "partner_1", StimFamilyPlanningAction.TryForChild, out var started), started);
            Assert.That(service.ActiveSave.state.family.monthsUntilResolution, Is.EqualTo(9));
            for (var month = 0; month < 9; month++) AdvanceAndAssert(service);

            Assert.That(service.ActiveSave.state.family.pendingPath, Is.Empty);
            Assert.That(service.ActiveSave.state.family.children, Has.Count.EqualTo(1));
            var child = service.ActiveSave.state.family.children[0];
            Assert.That(child.path, Is.EqualTo("pregnancy"));
            Assert.That(child.age, Is.Zero);
            Assert.That(service.ActiveSave.state.relationships.Exists(relationship =>
                relationship.relationshipId == child.childId && relationship.relationshipType == "child"), Is.True);
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "family" && entry.text.Contains("childbirth")), Is.True);
            Assert.That(service.GetPendingTransition()?.transitionType, Is.EqualTo("parenthood"));
            Assert.IsTrue(StimSaveValidator.ValidateSave(
                JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave)).isValid);
        }

        [Test]
        public void Adoption_IsAtomicTimedAndAddsOngoingChildExpenses()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 100000;
            save.state.finances.monthlyLivingExpensesMinorUnits = 100000;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "spouse_1", displayName = "Morgan", relationshipType = "married",
                relationshipStage = "married", value = 85, warmth = 75
            });
            save.state.family.planningPreference = "open";
            save.state.family.planningPartnerId = "spouse_1";
            save.state.family.partnerConsent = true;
            service.Start(save);

            Assert.IsTrue(service.TryChooseFamilyPlanning(
                "spouse_1", StimFamilyPlanningAction.PursueAdoption, out var adoption), adoption);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(50000));
            for (var month = 0; month < 6; month++) AdvanceAndAssert(service);
            Assert.That(service.ActiveSave.state.family.children[0].path, Is.EqualTo("adoption"));

            AdvanceAndAssert(service);
            Assert.That(service.ActiveSave.state.finances.lastExpensesMinorUnits, Is.EqualTo(125000));
        }

        [Test]
        public void ParentingActions_UpdateChildDevelopmentHistoryAndMonthlyCooldown()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            AddTestChild(save, "child_1", 8, "partner_1");
            service.Start(save);

            Assert.IsTrue(service.TryPerformParentingAction(
                "child_1", StimParentingAction.Teach, out var teach), teach);
            var child = service.ActiveSave.state.family.children[0];
            var relationship = service.ActiveSave.state.relationships.Find(item => item.relationshipId == "child_1");
            Assert.That(child.learning, Is.EqualTo(7));
            Assert.That(child.independence, Is.EqualTo(1));
            Assert.That(relationship.value, Is.EqualTo(72));
            Assert.That(relationship.relationshipHistory[^1].type, Is.EqualTo("parenting_teach"));
            Assert.IsFalse(service.TryPerformParentingAction(
                "child_1", StimParentingAction.QualityTime, out var cooldown));
            Assert.That(cooldown, Does.Contain("already chose focused parenting"));

            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformParentingAction(
                "child_1", StimParentingAction.SupportNeeds, out var support), support);
            Assert.That(service.ActiveSave.state.family.children[0].wellbeing, Is.EqualTo(67));
        }

        [Test]
        public void Child_TransitionsToIndependentAdultAndStopsAddingChildExpenses()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 12;
            save.state.finances.monthlyLivingExpensesMinorUnits = 100000;
            AddTestChild(save, "child_1", 17, "partner_1");
            service.Start(save);

            AdvanceAndAssert(service);
            Assert.That(service.ActiveSave.state.family.children[0].age, Is.EqualTo(18));
            Assert.That(service.ActiveSave.state.family.children[0].custodyStatus, Is.EqualTo("independent"));
            Assert.That(service.ActiveSave.state.relationships.Find(item => item.relationshipId == "child_1")
                .relationshipType, Is.EqualTo("adult_child"));

            AdvanceAndAssert(service);
            Assert.That(service.ActiveSave.state.finances.lastExpensesMinorUnits, Is.EqualTo(100000));
        }

        [Test]
        public void AdultChildRole_IsProtectedFromFriendshipAndRomanceStageConversion()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            AddTestChild(save, "adult_child_1", 18, "partner_1");
            save.state.relationships.Find(item => item.relationshipId == "adult_child_1").value = 90;
            service.Start(save);

            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "adult_child_1", StimRelationshipInteractionType.Talk, out var summary), summary);
            var relationship = service.ActiveSave.state.relationships.Find(item =>
                item.relationshipId == "adult_child_1");
            Assert.That(relationship.relationshipType, Is.EqualTo("adult_child"));
            Assert.That(relationship.relationshipStage, Is.EqualTo("adult_child"));
            Assert.IsFalse(service.TryPerformRelationshipInteraction(
                "adult_child_1", StimRelationshipInteractionType.AskOnDate, out var dating));
            Assert.That(dating, Does.Contain("friendship"));
        }

        [Test]
        public void PartnerSeparation_AssignsSharedCustodyWithoutDeletingChildHistory()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "partner_1", displayName = "Alex", relationshipType = "partner",
                relationshipStage = "partnered", value = 80, warmth = 70
            });
            AddTestChild(save, "child_1", 5, "partner_1");
            service.Start(save);

            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "partner_1", StimRelationshipInteractionType.Separate, out var summary), summary);
            Assert.That(service.ActiveSave.state.family.children[0].custodyStatus, Is.EqualTo("shared"));
            Assert.That(service.ActiveSave.state.relationships.Exists(item => item.relationshipId == "child_1"), Is.True);
        }

        [Test]
        public void SharedCustodyAndChildHistory_SurviveReloadAndRemainPlayable()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "partner_1", displayName = "Alex", relationshipType = "partner",
                relationshipStage = "partnered", value = 80, warmth = 70
            });
            AddTestChild(save, "child_1", 9, "partner_1");
            service.Start(save);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "partner_1", StimRelationshipInteractionType.Separate, out var separation), separation);

            var reloaded = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            reloaded.Start(JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave));
            Assert.That(reloaded.ActiveSave.state.family.children[0].custodyStatus, Is.EqualTo("shared"));
            var before = reloaded.ActiveSave.state.relationships.Find(item => item.relationshipId == "child_1")
                .relationshipHistory.Count;
            Assert.IsTrue(reloaded.TryPerformParentingAction(
                "child_1", StimParentingAction.QualityTime, out var parenting), parenting);
            Assert.That(reloaded.ActiveSave.state.relationships.Find(item => item.relationshipId == "child_1")
                .relationshipHistory, Has.Count.EqualTo(before + 1));
        }

        [Test]
        public void EndedLife_BlocksParentingWithoutChangingChildOrHistory()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            AddTestChild(save, "child_1", 7, "partner_1");
            save.state.character.lifeStatus = "deceased";
            save.state.character.endingReason = "death";
            save.state.character.endedAtAge = save.state.character.age;
            service.Start(save);
            var historyCount = service.ActiveSave.state.relationships.Find(item =>
                item.relationshipId == "child_1").relationshipHistory.Count;

            Assert.IsFalse(service.TryPerformParentingAction(
                "child_1", StimParentingAction.QualityTime, out var summary));
            Assert.That(summary, Does.Contain("current life state"));
            Assert.That(service.ActiveSave.state.family.children[0].wellbeing, Is.EqualTo(60));
            Assert.That(service.ActiveSave.state.relationships.Find(item => item.relationshipId == "child_1")
                .relationshipHistory, Has.Count.EqualTo(historyCount));
        }

        [Test]
        public void StartNewLife_DoesNotCarryFamilyOrChildRelationshipsAcrossLifeBoundary()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var oldLife = CreateValidSave();
            AddTestChild(oldLife, "child_old_life", 12, "partner_1");
            service.Start(oldLife);
            var newLife = StimNewLifeFactory.Create(
                new StimNewLifeRequest(), "0.1.0", new System.DateTime(2026, 7, 14, 12, 0, 0, System.DateTimeKind.Utc), 404);

            Assert.IsTrue(service.TryStartNewLife(newLife, out var summary), summary);
            Assert.That(service.ActiveSave.lifeId, Is.EqualTo(newLife.lifeId));
            Assert.That(service.ActiveSave.state.family.children, Is.Empty);
            Assert.That(service.ActiveSave.state.relationships.Exists(item =>
                item.relationshipId == "child_old_life"), Is.False);
        }

        [Test]
        public void ComingOfAge_RecordsPlayerChosenGenderAndOrientationAcrossEventChain()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateComingOfAgeGender());
            catalog.Upsert(RepresentativeStimEvents.CreateComingOfAgeOrientation());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.character.lifeStage = StimGameSessionService.GetLifeStage(15);
            save.state.character.genderIdentity = "undiscovered";
            save.state.character.sexualOrientation = "undiscovered";
            save.state.calendar.monthOfYear = 12;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var genderEvent, out var birthdaySummary), birthdaySummary);
            Assert.That(genderEvent?.id, Is.EqualTo(RepresentativeStimEvents.ComingOfAgeGenderId));
            Assert.IsTrue(service.TryResolveChoice(
                genderEvent.id, "identify_nonbinary", out var genderSummary), genderSummary);
            Assert.That(service.ActiveSave.state.character.genderIdentity, Is.EqualTo("nonbinary"));
            Assert.That(service.ActiveSave.state.scheduledEvents.Exists(
                record => record.eventId == RepresentativeStimEvents.ComingOfAgeOrientationId), Is.True);

            StimEvent orientationEvent = null;
            for (var month = 0; month < 12; month++)
            {
                Assert.IsTrue(service.TryAdvanceMonth(out orientationEvent, out var advanceSummary), advanceSummary);
            }

            Assert.That(orientationEvent?.id, Is.EqualTo(RepresentativeStimEvents.ComingOfAgeOrientationId));
            Assert.IsTrue(service.TryResolveChoice(
                orientationEvent.id, "orientation_bisexual", out var orientationSummary), orientationSummary);
            Assert.That(service.ActiveSave.state.character.sexualOrientation, Is.EqualTo("bisexual"));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "event" && entry.text.Contains("nonbinary")), Is.True);
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "event" && entry.text.Contains("bisexual")), Is.True);
        }

        [Test]
        public void PromDate_CreatesDatingRelationshipAndSchedulesConsentAwareFirstKiss()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreatePromInvitation());
            catalog.Upsert(RepresentativeStimEvents.CreateFirstKiss());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 18;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                displayName = "Taylor",
                relationshipType = "friend",
                value = 65
            });
            service.Start(save);

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.PromInvitationId,
                "attend_prom_together",
                out var promSummary), promSummary);
            var partner = service.ActiveSave.state.relationships.Find(
                relationship => relationship.relationshipId == "school_peer_primary");
            Assert.That(partner, Is.Not.Null);
            Assert.That(partner.relationshipType, Is.EqualTo("dating"));
            Assert.That(partner.value, Is.EqualTo(80));
            Assert.That(service.ActiveSave.state.scheduledEvents.Exists(
                record => record.eventId == RepresentativeStimEvents.FirstKissId), Is.True);

            service.ActiveSave.state.character.age = 19;
            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.FirstKissId,
                "wait_to_kiss",
                out var kissSummary), kissSummary);
            Assert.That(partner.value, Is.EqualTo(80),
                "The active object remains transactional and must not be mutated in place.");
            var persistedPartner = service.ActiveSave.state.relationships.Find(
                relationship => relationship.relationshipId == "school_peer_primary");
            Assert.That(persistedPartner.value, Is.EqualTo(85));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "event" && entry.text.Contains("respected your boundary")), Is.True);
        }

        [Test]
        public void PromDate_RejectsFriendshipBelowDatingThreshold()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreatePromInvitation());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 18;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                displayName = "Taylor",
                relationshipType = "friend",
                value = 59
            });
            service.Start(save);

            Assert.IsFalse(service.TryResolveChoice(
                RepresentativeStimEvents.PromInvitationId,
                "attend_prom_together",
                out var summary));
            Assert.That(summary, Does.Contain("at least 60"));
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("friend"));
            Assert.That(service.ActiveSave.state.lifeFeed, Is.Empty);
        }

        [Test]
        public void StrongAdultPartnership_CanBecomeEngagementAndMarriage()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateProposal());
            catalog.Upsert(RepresentativeStimEvents.CreateWedding());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 25;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                displayName = "Taylor",
                relationshipType = "partner",
                value = 85,
                npcAnnualIncomeMinorUnits = 6000000,
                npcCashMinorUnits = 0,
                npcDebtMinorUnits = 0
            });
            service.Start(save);

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.ProposalId, "propose_marriage", out var proposalSummary), proposalSummary);
            var engaged = service.ActiveSave.state.relationships.Find(
                relationship => relationship.relationshipId == "school_peer_primary");
            Assert.That(engaged.relationshipType, Is.EqualTo("engaged"));
            Assert.That(engaged.value, Is.EqualTo(90));
            Assert.That(service.LastFinancialImpactMinorUnits, Is.EqualTo(-50000));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(50000));
            Assert.That(service.ActiveSave.state.scheduledEvents.Exists(
                record => record.eventId == RepresentativeStimEvents.WeddingId), Is.True);

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.WeddingId, "get_married", out var weddingSummary), weddingSummary);
            var married = service.ActiveSave.state.relationships.Find(
                relationship => relationship.relationshipId == "school_peer_primary");
            Assert.That(married.relationshipType, Is.EqualTo("married"));
            Assert.That(married.value, Is.EqualTo(95));
            Assert.That(service.ActiveSave.state.finances.spouseAnnualIncomeMinorUnits, Is.EqualTo(6000000));
            Assert.That(service.LastFinancialImpactMinorUnits, Is.EqualTo(57334));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(107334));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "event" && entry.text.Contains("Got married")), Is.True);
            Assert.That(service.GetPendingTransition()?.transitionType, Is.EqualTo("marriage"));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "money" && entry.text.Contains("Financial impact")), Is.True);
        }

        [Test]
        public void Marriage_MergesNpcFundsOnceAndCombinesFutureMonthlyIncome()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateWedding());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 25;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                displayName = "Taylor",
                relationshipType = "engaged",
                value = 95,
                npcSmarts = 80,
                npcCareerLevel = 4,
                npcAnnualIncomeMinorUnits = 12000000,
                npcCashMinorUnits = 500000,
                npcDebtMinorUnits = 100000
            });
            service.Start(save);

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.WeddingId, "get_married", out var weddingSummary), weddingSummary);
            Assert.That(service.ActiveSave.state.finances.spouseAnnualIncomeMinorUnits, Is.EqualTo(12000000));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(671334));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(100000));
            Assert.That(service.ActiveSave.state.relationships[0].financesMerged, Is.True);
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "money" && entry.text.Contains("Combined household finances")), Is.True);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var monthSummary), monthSummary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(2088001));
        }

        [Test]
        public void WeddingFinancialImpact_CanBePositiveOrNegativeBasedOnLifeFactors()
        {
            var modestState = CreateValidSave().state;
            modestState.career.annualSalaryMinorUnits = 3000000;
            modestState.career.careerProgress = 80;
            modestState.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                relationshipType = "engaged",
                value = 95
            });
            var affluentState = CreateValidSave().state;
            affluentState.career.annualSalaryMinorUnits = 30000000;
            affluentState.career.careerProgress = 10;
            affluentState.finances.cashMinorUnits = 10000000;
            affluentState.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                relationshipType = "engaged",
                value = 80
            });

            Assert.That(StimGameSessionService.CalculateEventFinancialImpact(
                RepresentativeStimEvents.WeddingId, "get_married", modestState), Is.GreaterThan(0));
            Assert.That(StimGameSessionService.CalculateEventFinancialImpact(
                RepresentativeStimEvents.WeddingId, "get_married", affluentState), Is.LessThan(0));
        }

        [TestCase(RepresentativeStimEvents.HealthBurnoutId, "take_a_break")]
        [TestCase(RepresentativeStimEvents.PeerTrustConflictId, "keep_confidence")]
        [TestCase(RepresentativeStimEvents.ComingOfAgeGenderId, "identify_nonbinary")]
        [TestCase(RepresentativeStimEvents.FirstKissId, "share_first_kiss")]
        [TestCase(RepresentativeStimEvents.ProposalId, "end_partnership")]
        public void NonFinancialEventChoices_HaveNoImplicitFinancialImpact(string eventId, string choiceId)
        {
            var state = CreateValidSave().state;

            Assert.That(
                StimGameSessionService.CalculateEventFinancialImpact(eventId, choiceId, state),
                Is.Zero);
        }

        [Test]
        public void Proposal_RejectsWeakOrUnderagePartnership()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateProposal());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 23;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                relationshipType = "partner",
                value = 79
            });
            service.Start(save);

            Assert.IsFalse(service.TryResolveChoice(
                RepresentativeStimEvents.ProposalId, "propose_marriage", out var summary));
            Assert.That(summary, Does.Contain("age 24+").And.Contain("80 relationship strength"));
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("partner"));
        }

        [Test]
        public void CostedEvent_OffersCreditWhenCashIsInsufficientAndLineIsAvailable()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateProposal());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 25;
            save.state.finances.cashMinorUnits = 0;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                relationshipType = "partner",
                value = 85
            });
            service.Start(save);

            Assert.IsFalse(service.TryResolveChoice(
                RepresentativeStimEvents.ProposalId, "propose_marriage", out var cashFailure));
            Assert.That(cashFailure, Does.Contain("more cash").And.Contain("credit option"));
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("partner"));

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.ProposalId,
                "propose_marriage",
                StimPaymentMethod.Credit,
                out var creditSummary), creditSummary);
            Assert.That(creditSummary, Does.Contain("paid by credit").And.Contain("APR"));
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("engaged"));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(77));
            Assert.That(service.ActiveSave.state.finances.householdCreditBalanceMinorUnits, Is.EqualTo(50000));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(50000));
        }

        [Test]
        public void CreditDenial_ReducesHappinessWithoutApplyingPurchaseEffects()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateProposal());
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(catalog, repository);
            var save = CreateValidSave();
            save.state.character.age = 25;
            save.state.finances.cashMinorUnits = 0;
            save.state.finances.debtMinorUnits = 2000000;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                relationshipType = "partner",
                value = 85
            });
            service.Start(save);

            Assert.IsFalse(service.TryResolveChoice(
                RepresentativeStimEvents.ProposalId,
                "propose_marriage",
                StimPaymentMethod.Credit,
                out var summary));

            Assert.That(summary, Does.Contain("Credit was denied").And.Contain("Happiness −2"));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(68));
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("partner"));
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(85));
            Assert.That(service.ActiveSave.state.eventHistory, Is.Empty);
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "money" && entry.text.Contains("Credit denied")), Is.True);
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void AuthoredCashCost_UsesTheSameCashOrCreditPaymentPath()
        {
            var evt = RepresentativeStimEvents.CreateRandomLoss();
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(evt);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 0;
            service.Start(save);

            Assert.That(StimGameSessionService.CalculateChoicePotentialCost(
                evt, evt.choices[0], save.state), Is.EqualTo(4000));
            Assert.IsFalse(service.TryResolveChoice(
                evt.id, "handle_it_now", out var cashFailure));
            Assert.That(cashFailure, Does.Contain("credit option"));
            Assert.IsTrue(service.TryResolveChoice(
                evt.id, "handle_it_now", StimPaymentMethod.Credit, out var creditSummary), creditSummary);
            Assert.That(service.ActiveSave.state.finances.householdCreditBalanceMinorUnits, Is.EqualTo(4000));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(4000));
        }

        [Test]
        public void NeglectedMarriage_DecaysAndDivorceAppliesAuthoredSettlement()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateMarriageCrossroads());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 30;
            save.state.career = new StimCareerState();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                displayName = "Taylor",
                relationshipType = "married",
                value = 56
            });
            service.Start(save);

            for (var month = 0; month < 6; month++)
                Assert.IsTrue(service.TryAdvanceMonth(out _, out var advanceSummary), advanceSummary);
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(55));

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.MarriageCrossroadsId, "divorce", out var divorceSummary), divorceSummary);
            Assert.That(service.ActiveSave.state.relationships[0].relationshipType, Is.EqualTo("ex_partner"));
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(35));
            Assert.That(service.LastFinancialImpactMinorUnits, Is.EqualTo(-75000));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(25000));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "money" && entry.text.Contains("Financial impact")), Is.True);
        }

        [Test]
        public void MarriageCrossroads_RejectsHealthyMarriage()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateMarriageCrossroads());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 30;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                relationshipType = "married",
                value = 56
            });
            service.Start(save);

            Assert.IsFalse(service.TryResolveChoice(
                RepresentativeStimEvents.MarriageCrossroadsId, "divorce", out var summary));
            Assert.That(summary, Does.Contain("55 relationship strength or below"));
        }

        [Test]
        public void PerformRelationshipInteraction_DoesNotMutateWhenCommitFails()
        {
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
                { relationshipId = "parent_1", displayName = "Jordan", relationshipType = "parent", value = 50 });
            service.Start(save);

            var performed = service.TryPerformRelationshipInteraction(
                "parent_1", StimRelationshipInteractionType.Talk, out _);

            Assert.IsFalse(performed);
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(50));
            Assert.That(service.ActiveSave.state.lifeFeed, Is.Empty);
            Assert.That(service.ActiveSave.state.statuses, Is.Empty);
        }

        [Test]
        public void FamilyMovie_ImprovesHouseholdAndFamilyRelationshipsAndChargesTickets()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 30;
            save.state.finances.spouseAnnualIncomeMinorUnits = 6000000;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "spouse_1",
                relationshipType = "married",
                value = 70
            });
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "child_1",
                relationshipType = "child",
                value = 60
            });
            service.Start(save);

            Assert.IsTrue(service.TryPerformActivity(
                StimActivityType.FamilyMovie, out var summary), summary);

            Assert.That(summary, Does.Contain("Household happiness +4").And.Contain("Cost $45"));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(73));
            Assert.That(service.ActiveSave.state.household.happiness, Is.EqualTo(54));
            Assert.That(service.ActiveSave.state.household.cohesion, Is.EqualTo(53));
            Assert.That(service.ActiveSave.state.relationships[0].value, Is.EqualTo(73));
            Assert.That(service.ActiveSave.state.relationships[1].value, Is.EqualTo(63));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(95500));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "activity" && entry.text.Contains("Family movie night")), Is.True);
        }

        [Test]
        public void FamilyMovieCredit_RequiresCreditLineAndAddsOnlyFixedTicketDebt()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 30;
            save.state.finances.cashMinorUnits = 0;
            save.state.finances.debtMinorUnits = 10000;
            save.state.relationships.Add(new StimRelationshipState
                { relationshipId = "spouse_1", relationshipType = "married", value = 70 });
            save.state.relationships.Add(new StimRelationshipState
                { relationshipId = "child_1", relationshipType = "child", value = 60 });
            service.Start(save);

            Assert.IsFalse(service.TryPerformActivity(
                StimActivityType.FamilyMovie, out var cashSummary));
            Assert.That(cashSummary, Does.Contain("costs $45").And.Contain("credit option"));
            Assert.IsTrue(service.TryPerformActivity(
                StimActivityType.FamilyMovieCredit, out var creditSummary), creditSummary);
            Assert.That(creditSummary, Does.Contain("Charged to credit $45"));
            Assert.That(creditSummary, Does.Contain("Credit approved").And.Contain("Happiness +1"));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(74));
            Assert.That(creditSummary, Does.Contain("APR 17.52%"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.Zero);
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(14500));
            Assert.That(service.ActiveSave.state.finances.householdCreditBalanceMinorUnits, Is.EqualTo(4500));
            Assert.That(service.ActiveSave.state.finances.householdCreditAprBasisPoints, Is.EqualTo(1752));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var monthSummary), monthSummary);
            Assert.That(monthSummary, Does.Contain("credit interest $1"));
            Assert.That(service.ActiveSave.state.finances.householdCreditBalanceMinorUnits, Is.EqualTo(4566));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(14566));
        }

        [Test]
        public void HouseholdCreditApr_RespondsToIncomeStabilityCohesionAndDebtLoad()
        {
            var strong = CreateValidSave().state;
            strong.career.annualSalaryMinorUnits = 20000000;
            strong.career.careerProgress = 100;
            strong.household.cohesion = 90;
            strong.finances.debtMinorUnits = 0;
            var risky = CreateValidSave().state;
            risky.career.annualSalaryMinorUnits = 2000000;
            risky.career.careerProgress = 0;
            risky.household.cohesion = 10;
            risky.finances.debtMinorUnits = 1500000;

            Assert.That(StimGameSessionService.CalculateHouseholdCreditAprBasisPoints(strong), Is.EqualTo(800));
            Assert.That(StimGameSessionService.CalculateHouseholdCreditAprBasisPoints(risky), Is.EqualTo(2999));
        }

        [TestCase(0, 1)]
        [TestCase(49, 1)]
        [TestCase(50, 2)]
        [TestCase(149, 2)]
        [TestCase(150, 3)]
        [TestCase(300, 4)]
        [TestCase(499, 4)]
        [TestCase(500, 5)]
        [TestCase(749, 5)]
        [TestCase(750, 6)]
        [TestCase(1049, 6)]
        [TestCase(1050, 7)]
        public void SkillLevels_FollowCumulativeXpThresholds(int experience, int expectedLevel)
        {
            Assert.That(StimGameSessionService.GetSkillLevel(experience), Is.EqualTo(expectedLevel));
        }

        [TestCase(0, StimCoreStatBand.Critical)]
        [TestCase(19, StimCoreStatBand.Critical)]
        [TestCase(20, StimCoreStatBand.Low)]
        [TestCase(39, StimCoreStatBand.Low)]
        [TestCase(40, StimCoreStatBand.Stable)]
        [TestCase(59, StimCoreStatBand.Stable)]
        [TestCase(60, StimCoreStatBand.Strong)]
        [TestCase(79, StimCoreStatBand.Strong)]
        [TestCase(80, StimCoreStatBand.Exceptional)]
        [TestCase(94, StimCoreStatBand.Exceptional)]
        [TestCase(95, StimCoreStatBand.Peak)]
        [TestCase(100, StimCoreStatBand.Peak)]
        public void CoreStatBands_UseDocumentedExactBoundaries(
            int value, StimCoreStatBand expectedBand)
        {
            Assert.That(StimProgressionStandards.GetCoreStatBand(value), Is.EqualTo(expectedBand));
            Assert.That(StimProgressionStandards.MaximumMainPathCoreStatRequirement,
                Is.EqualTo(StimProgressionStandards.StrongCoreStatStartsAt));
        }

        [TestCase(1, 0)]
        [TestCase(2, 50)]
        [TestCase(3, 150)]
        [TestCase(4, 300)]
        [TestCase(5, 500)]
        [TestCase(6, 750)]
        [TestCase(7, 1050)]
        public void SkillExperienceThresholds_MatchDocumentedCumulativeFormula(
            int level, int expectedExperience)
        {
            Assert.That(StimGameSessionService.GetExperienceForSkillLevel(level),
                Is.EqualTo(expectedExperience));
        }

        [Test]
        public void ProgressionStandards_AreWiredIntoLiveThresholdsAndGoalRewards()
        {
            Assert.That(StimEducationActionService.CertificateQualificationExperience,
                Is.EqualTo(StimProgressionStandards.CertificateQualificationExperience));
            Assert.That(StimEducationActionService.DiplomaQualificationExperience,
                Is.EqualTo(StimProgressionStandards.DiplomaQualificationExperience));
            Assert.That(StimEducationActionService.AdvancedQualificationExperience,
                Is.EqualTo(StimProgressionStandards.AdvancedQualificationExperience));
            Assert.That(StimGameSessionService.IndexInvestmentMinimumAge,
                Is.EqualTo(StimProgressionStandards.IndexInvestmentMinimumAge));
            Assert.That(StimGameSessionService.IndexInvestmentMinimumSmarts,
                Is.EqualTo(StimProgressionStandards.IndexInvestmentMinimumSmarts));

            Assert.IsTrue(StimCareerCatalog.TryGetIndustry(
                StimCareerCatalog.FinanceIndustryId, out var finance));
            Assert.That(finance.roles.Take(3).Select(role => role.promotionProgressRequired),
                Is.EqualTo(new[]
                {
                    StimProgressionStandards.FirstCareerPromotionProgress,
                    StimProgressionStandards.SecondCareerPromotionProgress,
                    StimProgressionStandards.ThirdCareerPromotionProgress
                }));
            Assert.That(StimProgressionStandards.GetBusinessUpgradeProgressRequired(1), Is.EqualTo(25));
            Assert.That(StimProgressionStandards.GetBusinessUpgradeProgressRequired(2), Is.EqualTo(50));
            Assert.That(StimProgressionStandards.GetBusinessUpgradeProgressRequired(3), Is.EqualTo(75));

            var educationState = CreateValidSave().state;
            educationState.character.age = 14;
            educationState.skills.Add(new StimSkillState { skillId = "learning", experience = 150 });
            var educationActions = StimEducationActionService.GetDefinitions(educationState);
            var readExperience = educationActions.Single(action => action.id == "education.read")
                .previews.Single(delta => delta.targetId == "Learning XP").amount;
            Assert.That(readExperience, Is.InRange(
                StimProgressionStandards.RoutineSkillExperienceMinimum,
                StimProgressionStandards.RoutineSkillExperienceMaximum));
            foreach (var action in educationActions.Where(action => action.id != "education.read"))
            {
                var experience = action.previews.Single(delta => delta.targetId == "Learning XP").amount;
                Assert.That(experience, Is.InRange(
                    StimProgressionStandards.CommittedSkillExperienceMinimum,
                    StimProgressionStandards.CommittedSkillExperienceMaximum), action.id);
            }

            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            service.Start(CreateValidSave());
            Assert.IsTrue(service.TryPerformActivity(StimActivityType.Rest, out var summary), summary);
            var goals = service.GetGoals();
            Assert.That(goals, Is.Not.Empty);
            Assert.That(goals.All(goal => StimProgressionStandards.IsGoalRewardWithinDocumentedBand(
                goal.category, goal.rewardMinorUnits)), Is.True);
            Assert.That(goals.Single(goal => goal.category == "daily").rewardMinorUnits,
                Is.EqualTo(StimProgressionStandards.DailyGoalRewardMinorUnits));
            Assert.That(goals.Single(goal => goal.category == "main").rewardMinorUnits,
                Is.EqualTo(StimProgressionStandards.MainGoalRewardMinorUnits));
            Assert.That(goals.Single(goal => goal.category == "life").rewardMinorUnits,
                Is.EqualTo(StimProgressionStandards.LifeGoalRewardMinorUnits));
        }

        [Test]
        public void EducationActions_ExposeVisibleAgeAndLevelRequirements()
        {
            var state = CreateValidSave().state;
            state.character.age = 10;

            Assert.IsTrue(StimGameSessionService.TryGetEducationActionRequirement(
                state, StimEducationActionType.Read, out var readRequirement), readRequirement);
            Assert.IsFalse(StimGameSessionService.TryGetEducationActionRequirement(
                state, StimEducationActionType.StudyGroup, out var groupRequirement));
            Assert.That(groupRequirement, Does.Contain("Learning Level 2"));
            Assert.IsFalse(StimGameSessionService.TryGetEducationActionRequirement(
                state, StimEducationActionType.AdvancedProject, out var projectRequirement));
            Assert.That(projectRequirement, Does.Contain("age 14"));

            state.skills.Add(new StimSkillState { skillId = "learning", experience = 150 });
            state.character.age = 14;
            Assert.IsTrue(StimGameSessionService.TryGetEducationActionRequirement(
                state, StimEducationActionType.AdvancedProject, out var unlockedRequirement), unlockedRequirement);
        }

        [Test]
        public void EducationActionDefinitions_ExposeStableIdsStatesAndSignedPreviews()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 10;
            service.Start(save);

            var definitions = service.GetEducationActionDefinitions();

            Assert.That(definitions, Has.Count.EqualTo(4));
            Assert.That(definitions, Has.Some.Matches<StimActionDefinition>(definition =>
                definition.id == "education.read" &&
                definition.destination == StimActionDestination.Education &&
                definition.state == StimActionState.Ready &&
                definition.previews.Exists(delta => delta.targetId == "Learning XP" && delta.amount == 12)));
            Assert.That(definitions, Has.Some.Matches<StimActionDefinition>(definition =>
                definition.id == "education.studygroup" &&
                definition.state == StimActionState.Locked &&
                definition.lockedReason.Contains("Learning Level 2")));
        }

        [Test]
        public void PerformEducationAction_AppliesXpStatsFeedCooldownAndAutosave()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 10;
            save.state.education.stage = "primary_school";
            save.state.skills.Add(new StimSkillState { skillId = "learning", experience = 45 });
            service.Start(save);

            Assert.IsTrue(service.TryPerformEducationAction(
                StimEducationActionType.Homework, out var summary), summary);

            Assert.That(summary, Does.Contain("Learning XP +18").And.Contain("Learning Level +1"));
            Assert.That(StimGameSessionService.GetSkillExperience(
                service.ActiveSave.state.skills, "learning"), Is.EqualTo(63));
            Assert.That(service.ActiveSave.state.character.smarts, Is.EqualTo(61));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(69));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry => entry.category == "education"), Is.True);
            Assert.That(repository.CommitCount, Is.EqualTo(1));
            Assert.IsFalse(service.TryPerformEducationAction(
                StimEducationActionType.Read, out var cooldownSummary));
            Assert.That(cooldownSummary, Does.Contain("already completed a school action"));
        }

        [Test]
        public void PerformEducationAction_WhenAutosaveFails_RollsBackCandidate()
        {
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 10;
            save.state.education.stage = "primary_school";
            service.Start(save);

            var originalRevision = service.ActiveSave.revision;
            var originalSmarts = service.ActiveSave.state.character.smarts;

            Assert.IsFalse(service.TryPerformEducationAction(
                StimEducationActionType.Read, out var summary));
            Assert.That(summary, Is.EqualTo("failed"));
            Assert.That(service.ActiveSave, Is.SameAs(save));
            Assert.That(service.ActiveSave.revision, Is.EqualTo(originalRevision));
            Assert.That(service.ActiveSave.state.character.smarts, Is.EqualTo(originalSmarts));
            Assert.That(service.ActiveSave.state.statuses.Exists(
                status => status.statusId == StimEducationActionService.MonthlyCooldownStatusId), Is.False);
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void EducationDisciplineCatalog_MapsThreeOriginalPathsToDistinctCareerConsequences()
        {
            var disciplines = StimEducationDisciplineCatalog.GetAll();

            Assert.That(disciplines, Has.Count.EqualTo(3));
            Assert.That(disciplines.Select(item => item.disciplineId).Distinct().Count(), Is.EqualTo(3));
            Assert.That(disciplines.Select(item => item.studyTrack).Distinct().Count(), Is.EqualTo(3));
            Assert.That(disciplines, Has.Some.Matches<StimEducationDisciplineDefinition>(item =>
                item.displayName == "Applied Finance" && item.consequenceSummary.Contains("Finance")));
            Assert.That(disciplines, Has.Some.Matches<StimEducationDisciplineDefinition>(item =>
                item.displayName == "Community Health" && item.consequenceSummary.Contains("Healthcare")));
            Assert.That(disciplines, Has.Some.Matches<StimEducationDisciplineDefinition>(item =>
                item.displayName == "Sustainable Trades" && item.consequenceSummary.Contains("Skilled Trades")));
        }

        [Test]
        public void ChooseStudyTrack_DeductsAuthoredCostPersistsAndWritesFeed()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.education = new StimEducationState { stage = "high_school" };
            save.state.finances.cashMinorUnits = 10000;
            service.Start(save);

            Assert.IsTrue(service.TryChooseStudyTrack(StimStudyTrack.Vocational, out var summary), summary);

            Assert.That(service.ActiveSave.state.education.studyTrack, Is.EqualTo("vocational"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(2500));
            Assert.That(service.ActiveSave.state.lifeFeed, Has.Some.Matches<StimLifeFeedEntry>(entry =>
                entry.category == "education" && entry.text.Contains("Vocational study track selected")));
            Assert.That(repository.CommitCount, Is.EqualTo(1));

            var reloaded = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            Assert.IsTrue(reloaded.TryLoadLatest(out var loadSummary), loadSummary);
            Assert.That(reloaded.ActiveSave.state.education.studyTrack, Is.EqualTo("vocational"));
            Assert.That(reloaded.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(2500));
        }

        [Test]
        public void ChooseStudyTrack_RejectsAgeFundsAndDuplicateWithoutMutation()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 13;
            save.state.education = new StimEducationState { stage = "middle_school" };
            save.state.finances.cashMinorUnits = 1000;
            service.Start(save);

            Assert.IsFalse(service.TryChooseStudyTrack(StimStudyTrack.General, out var ageSummary));
            Assert.That(ageSummary, Does.Contain("ages 14 through 17"));

            service.ActiveSave.state.character.age = 15;
            Assert.IsFalse(service.TryChooseStudyTrack(StimStudyTrack.Academic, out var fundsSummary));
            Assert.That(fundsSummary, Does.Contain("Not enough cash"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(1000));

            Assert.IsTrue(service.TryChooseStudyTrack(StimStudyTrack.General, out var firstSummary), firstSummary);
            Assert.IsFalse(service.TryChooseStudyTrack(StimStudyTrack.Vocational, out var duplicateSummary));
            Assert.That(duplicateSummary, Does.Contain("already been selected"));
            Assert.That(service.ActiveSave.state.education.studyTrack, Is.EqualTo("general"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(1000));
        }

        [Test]
        public void ChooseStudyTrack_WhenAutosaveFails_RollsBackCostAndSelection()
        {
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.education = new StimEducationState { stage = "high_school" };
            save.state.finances.cashMinorUnits = 10000;
            service.Start(save);

            Assert.IsFalse(service.TryChooseStudyTrack(StimStudyTrack.Academic, out var summary));

            Assert.That(summary, Is.EqualTo("failed"));
            Assert.That(service.ActiveSave, Is.SameAs(save));
            Assert.That(service.ActiveSave.state.education.studyTrack, Is.Null.Or.Empty);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(10000));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void StudySession_AppliesDifficultyTradeoffQualificationTierAndCooldown()
        {
            var repository = new RecordingSaveRepository();
            var now = DateTimeOffset.Parse("2026-07-15T12:00:00Z");
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), repository, utcNow: () => now);
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.character.smarts = 60;
            save.state.character.happiness = 70;
            save.state.education = new StimEducationState
            {
                stage = "high_school",
                studyTrack = "academic",
                qualificationExperience = 30
            };
            service.Start(save);

            Assert.IsTrue(service.TryStartStudySession(
                StimStudyDifficulty.Hard, "hard-study-1", out var started), started);
            now = now.AddSeconds(181);
            Assert.IsTrue(service.TryClaimStudySession("hard-study-1", out var summary), summary);

            Assert.That(service.ActiveSave.state.education.qualificationExperience, Is.EqualTo(65));
            Assert.That(service.ActiveSave.state.character.smarts, Is.EqualTo(62));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(67));
            Assert.That(summary, Does.Contain("Qualification XP +35")
                .And.Contain("Certificate Qualification"));
            Assert.That(service.ActiveSave.state.statuses, Has.Some.Matches<StimStatusState>(status =>
                status.statusId == StimEducationActionService.MonthlyCooldownStatusId));
            Assert.IsFalse(service.TryStartStudySession(
                StimStudyDifficulty.Easy, "easy-study-1", out var cooldown));
            Assert.That(cooldown, Does.Contain("already completed this month"));
        }

        [Test]
        public void StudySession_DefinitionsPreviewTradeoffsAndLockHardBelowSmartsRequirement()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.character.smarts = 59;
            save.state.education = new StimEducationState
            {
                stage = "high_school",
                studyTrack = "vocational"
            };
            service.Start(save);

            var definitions = service.GetStudySessionDefinitions();

            Assert.That(definitions, Has.Count.EqualTo(3));
            Assert.That(definitions, Has.Some.Matches<StimActionDefinition>(definition =>
                definition.id == "education.study.medium" &&
                definition.state == StimActionState.Ready &&
                definition.previews.Exists(delta => delta.targetId == "Qualification XP" && delta.amount == 20) &&
                definition.previews.Exists(delta => delta.targetId == "Happiness" && delta.amount == -1)));
            Assert.That(definitions, Has.Some.Matches<StimActionDefinition>(definition =>
                definition.id == "education.study.hard" &&
                definition.state == StimActionState.Locked &&
                definition.lockedReason.Contains("60 Smarts")));
        }

        [TestCase(0, "Foundation Qualification", 50)]
        [TestCase(49, "Foundation Qualification", 50)]
        [TestCase(50, "Certificate Qualification", 125)]
        [TestCase(124, "Certificate Qualification", 125)]
        [TestCase(125, "Diploma Qualification", 250)]
        [TestCase(249, "Diploma Qualification", 250)]
        [TestCase(250, "Advanced Qualification", 250)]
        public void QualificationTiers_UseDocumentedExactBoundaries(
            int experience, string expectedTier, int expectedNextTier)
        {
            Assert.That(StimEducationActionService.GetQualificationTier(experience),
                Is.EqualTo(expectedTier));
            Assert.That(StimEducationActionService.GetNextQualificationTierAt(experience),
                Is.EqualTo(expectedNextTier));
        }

        [Test]
        public void EasyStudyRoute_ReachesAdvancedQualificationWithinTeenStudyWindow()
        {
            var now = DateTimeOffset.Parse("2026-07-15T12:00:00Z");
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository(), utcNow: () => now);
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.character.lifeStage = "teen";
            save.state.calendar.monthOfYear = 1;
            save.state.education = new StimEducationState
            {
                stage = "high_school",
                studyTrack = "general"
            };
            service.Start(save);

            for (var month = 0; month < 25; month++)
            {
                var instanceId = $"easy-study-{month}";
                Assert.IsTrue(service.TryStartStudySession(
                    StimStudyDifficulty.Easy, instanceId, out var startSummary), startSummary);
                now = now.AddSeconds(61);
                Assert.IsTrue(service.TryClaimStudySession(
                    instanceId, out var studySummary), studySummary);
                if (month < 24)
                {
                    Assert.IsTrue(service.TryAdvanceMonth(out var evt, out var advanceSummary), advanceSummary);
                    Assert.That(evt, Is.Null);
                }
            }

            Assert.That(service.ActiveSave.state.education.qualificationExperience,
                Is.EqualTo(StimEducationActionService.AdvancedQualificationExperience));
            Assert.That(StimEducationActionService.GetQualificationTier(
                service.ActiveSave.state.education.qualificationExperience),
                Is.EqualTo("Advanced Qualification"));
            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(17));
        }

        [Test]
        public void EducationActionRequest_IsIdempotentAcrossRepeatedSubmission()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 10;
            save.state.education.stage = "primary_school";
            service.Start(save);
            var request = new StimActionRequest(
                StimEducationActionService.GetActionId(StimEducationActionType.Read),
                "education-request-1");

            Assert.IsTrue(service.TryPerformEducationAction(
                StimEducationActionType.Read, request, out var firstSummary), firstSummary);
            var experience = StimGameSessionService.GetSkillExperience(
                service.ActiveSave.state.skills, "learning");
            Assert.That(service.ActiveSave.state.actionProgress, Has.Count.EqualTo(1));

            var reloadedService = new StimGameSessionService(
                new InMemoryStimEventCatalog(), repository);
            Assert.IsTrue(reloadedService.TryLoadLatest(out var loadSummary), loadSummary);
            Assert.IsFalse(reloadedService.TryPerformEducationAction(
                StimEducationActionType.Read, request, out var repeatedSummary));
            Assert.That(repeatedSummary, Is.EqualTo(firstSummary));
            Assert.That(StimGameSessionService.GetSkillExperience(
                reloadedService.ActiveSave.state.skills, "learning"), Is.EqualTo(experience));
            Assert.That(reloadedService.ActiveSave.state.actionProgress, Has.Count.EqualTo(1));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void TimedAction_StartsInProgressAndCannotBeClaimedEarly()
        {
            var repository = new RecordingSaveRepository();
            var now = DateTimeOffset.Parse("2026-07-14T20:00:00Z");
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), repository, utcNow: () => now);
            service.Start(CreateValidSave());
            var definition = new StimActionDefinition
            {
                id = "education.timed-study",
                title = "Timed Study",
                state = StimActionState.Ready,
                durationSeconds = 60,
                progressRequired = 1
            };

            Assert.IsTrue(service.TryStartAction(
                definition, new StimActionRequest(definition.id, "timed-1"), out var startSummary), startSummary);
            Assert.That(service.ActiveSave.state.actionProgress[0].state, Is.EqualTo("InProgress"));
            Assert.That(service.ActiveSave.state.actionProgress[0].completesAtUtc, Is.Not.Empty);
            Assert.IsFalse(service.TryClaimAction("timed-1", out var earlySummary));
            Assert.That(earlySummary, Does.Contain("not ready"));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void TimedStudySession_GrantsRewardOnceOnlyAfterCompletion()
        {
            var now = DateTimeOffset.Parse("2026-07-15T12:00:00Z");
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository(), utcNow: () => now);
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.character.lifeStage = "teen";
            save.state.character.smarts = 60;
            save.state.education = new StimEducationState
            {
                stage = "high_school",
                studyTrack = "academic",
                qualificationExperience = 30
            };
            service.Start(save);

            Assert.IsTrue(service.TryStartStudySession(
                StimStudyDifficulty.Medium, "focused-study-1", out var started), started);
            Assert.That(service.ActiveSave.state.education.qualificationExperience, Is.EqualTo(30));
            Assert.That(service.ActiveSave.state.actionProgress[0].state, Is.EqualTo("InProgress"));
            Assert.IsFalse(service.IsActionReadyToClaim(service.ActiveSave.state.actionProgress[0]));
            Assert.IsFalse(service.TryClaimStudySession("focused-study-1", out var early));
            Assert.That(early, Does.Contain("not ready"));

            now = now.AddSeconds(121);
            Assert.IsTrue(service.IsActionReadyToClaim(service.ActiveSave.state.actionProgress[0]));
            Assert.IsTrue(service.TryClaimStudySession("focused-study-1", out var claimed), claimed);
            Assert.That(service.ActiveSave.state.education.qualificationExperience, Is.EqualTo(50));
            Assert.That(service.ActiveSave.state.actionProgress[0].state, Is.EqualTo("Complete"));
            Assert.That(service.ActiveSave.state.actionProgress[0].resultSummary,
                Does.Contain("Certificate Qualification"));

            Assert.IsFalse(service.TryClaimStudySession("focused-study-1", out var duplicate));
            Assert.That(duplicate, Does.Contain("already claimed"));
            Assert.That(service.ActiveSave.state.education.qualificationExperience, Is.EqualTo(50));
        }

        [Test]
        public void TimedAction_ReconcilesAfterReloadAndCanOnlyBeClaimedOnce()
        {
            var repository = new RecordingSaveRepository();
            var now = DateTimeOffset.Parse("2026-07-14T20:00:00Z");
            var firstSession = new StimGameSessionService(
                new InMemoryStimEventCatalog(), repository, utcNow: () => now);
            firstSession.Start(CreateValidSave());
            var definition = new StimActionDefinition
            {
                id = "education.timed-study",
                title = "Timed Study",
                state = StimActionState.Ready,
                durationSeconds = 60
            };
            Assert.IsTrue(firstSession.TryStartAction(
                definition, new StimActionRequest(definition.id, "timed-2"), out var startSummary), startSummary);

            now = now.AddSeconds(61);
            var reloadedSession = new StimGameSessionService(
                new InMemoryStimEventCatalog(), repository, utcNow: () => now);
            Assert.IsTrue(reloadedSession.TryLoadLatest(out var loadSummary), loadSummary);
            Assert.IsTrue(reloadedSession.TryReconcileActionProgress(out var reconcileSummary), reconcileSummary);
            Assert.That(reloadedSession.ActiveSave.state.actionProgress[0].state, Is.EqualTo("Claimable"));
            Assert.IsTrue(reloadedSession.TryClaimAction("timed-2", out var claimSummary), claimSummary);
            Assert.That(reloadedSession.ActiveSave.state.actionProgress[0].state, Is.EqualTo("Complete"));
            Assert.IsFalse(reloadedSession.TryClaimAction("timed-2", out var duplicateSummary));
            Assert.That(duplicateSummary, Does.Contain("already claimed"));
            Assert.That(repository.CommitCount, Is.EqualTo(3));
        }

        [Test]
        public void CareerApplication_UnlocksNextMonthInterviewAndEntryRole()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.career = new StimCareerState();
            service.Start(save);

            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.Apply, out var applySummary), applySummary);
            Assert.That(applySummary, Does.Contain("Interview unlocked next month"));
            Assert.IsFalse(service.TryPerformCareerAction(
                StimCareerActionType.Interview, out var cooldownSummary));
            Assert.That(cooldownSummary, Does.Contain("already completed a career action"));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var advanceSummary), advanceSummary);
            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.Interview, out var interviewSummary), interviewSummary);
            Assert.That(service.ActiveSave.state.career.roleTitle, Is.EqualTo("Junior Associate"));
            Assert.That(service.ActiveSave.state.career.annualSalaryMinorUnits, Is.EqualTo(4000000));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry => entry.category == "career"), Is.True);
            Assert.That(repository.CommitCount, Is.EqualTo(3));
        }

        [Test]
        public void CareerApplication_UsesSelectedTrackQualificationAsVisiblePrerequisite()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.career = new StimCareerState();
            save.state.education = new StimEducationState
            {
                stage = "completed_secondary",
                studyTrack = "academic",
                qualificationExperience = 49
            };
            service.Start(save);

            Assert.IsFalse(StimGameSessionService.TryGetCareerActionRequirement(
                service.ActiveSave.state, StimCareerActionType.Apply, out var requirement));
            Assert.That(requirement, Does.Contain("Certificate qualification (50 XP)"));
            Assert.IsFalse(service.TryPerformCareerAction(StimCareerActionType.Apply, out var lockedSummary));
            Assert.That(lockedSummary, Is.EqualTo(requirement));

            service.ActiveSave.state.education.qualificationExperience = 50;
            Assert.IsTrue(service.TryPerformCareerAction(StimCareerActionType.Apply, out var summary), summary);
        }

        [Test]
        public void CareerIndustries_ExposeDistinctStableLaddersAndRequirements()
        {
            var industries = StimCareerCatalog.GetIndustries();

            Assert.That(industries, Has.Count.EqualTo(3));
            Assert.That(industries.Select(industry => industry.industryId), Is.EquivalentTo(new[]
            {
                StimCareerCatalog.FinanceIndustryId,
                StimCareerCatalog.HealthcareIndustryId,
                StimCareerCatalog.SkilledTradesIndustryId
            }));
            Assert.That(industries.All(industry => industry.roles.Count == 4), Is.True);
            Assert.That(industries.Select(industry => industry.roles[0].title).Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void HealthcareApplication_RequiresAcademicQualificationAndLearningThenPersistsIndustry()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.career = new StimCareerState();
            save.state.education = new StimEducationState
            {
                stage = "completed_secondary", studyTrack = "academic", qualificationExperience = 124
            };
            save.state.skills.Add(new StimSkillState { skillId = "learning", experience = 50 });
            service.Start(save);

            Assert.IsFalse(service.TryApplyForCareer(
                StimCareerCatalog.HealthcareIndustryId, out var locked));
            Assert.That(locked, Does.Contain("125 qualification XP"));
            service.ActiveSave.state.education.qualificationExperience = 125;
            Assert.IsTrue(service.TryApplyForCareer(
                StimCareerCatalog.HealthcareIndustryId, out var applied), applied);
            Assert.That(service.ActiveSave.state.career.pendingIndustryId,
                Is.EqualTo(StimCareerCatalog.HealthcareIndustryId));
            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.Interview, out var interview), interview);
            Assert.That(service.ActiveSave.state.career.industryId,
                Is.EqualTo(StimCareerCatalog.HealthcareIndustryId));
            Assert.That(service.ActiveSave.state.career.roleTitle, Is.EqualTo("Care Assistant"));
            Assert.That(service.ActiveSave.state.career.employerId, Is.EqualTo("harbor_health_network"));
            Assert.That(JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave)
                .state.career.industryId, Is.EqualTo(StimCareerCatalog.HealthcareIndustryId));
        }

        [Test]
        public void SkilledTradesPromotion_UsesIndustrySpecificThresholdAndRole()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.career = new StimCareerState
            {
                industryId = StimCareerCatalog.SkilledTradesIndustryId,
                employerId = "community_build_works",
                roleTitle = "Apprentice Technician",
                annualSalaryMinorUnits = 3600000,
                careerProgress = 20
            };
            service.Start(save);

            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.AskForPromotion, out var summary), summary);
            Assert.That(service.ActiveSave.state.career.roleTitle, Is.EqualTo("Technician"));
            Assert.That(service.ActiveSave.state.career.annualSalaryMinorUnits, Is.EqualTo(5200000));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.Zero);
        }

        [Test]
        public void InterviewRejection_IsDeterministicAndUnlocksTransactionalRetraining()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.rng.seed = 2;
            save.state.career = new StimCareerState();
            service.Start(save);

            Assert.IsTrue(service.TryApplyForCareer(
                StimCareerCatalog.FinanceIndustryId, out var application), application);
            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.Interview, out var interview), interview);
            Assert.That(interview, Does.Contain("Not selected this time"));
            Assert.That(service.ActiveSave.state.career.employmentStatus, Is.EqualTo("unemployed"));
            Assert.That(string.IsNullOrEmpty(service.ActiveSave.state.career.roleTitle), Is.True);
            Assert.That(service.ActiveSave.state.career.pendingIndustryId, Is.Empty);

            AdvanceAndAssert(service);
            var previousRevision = service.ActiveSave.revision;
            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.Retrain, out var retraining), retraining);
            Assert.That(service.ActiveSave.state.skills.Find(skill => skill.skillId == "professional")
                .experience, Is.EqualTo(15));
            Assert.That(service.ActiveSave.revision, Is.EqualTo(previousRevision + 1));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].text, Does.Contain("Professional XP +15"));
            Assert.That(JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave)
                .state.skills.Find(skill => skill.skillId == "professional").experience, Is.EqualTo(15));
        }

        [Test]
        public void InterviewChance_IsBoundedAndImprovesWithSmartsSkillsAndQualificationSurplus()
        {
            Assert.IsTrue(StimCareerCatalog.TryGetIndustry(
                StimCareerCatalog.HealthcareIndustryId, out var industry));
            var baseline = CreateValidSave().state;
            baseline.education.studyTrack = "academic";
            baseline.education.qualificationExperience = 125;
            baseline.character.smarts = 40;
            var developed = CreateValidSave().state;
            developed.education.studyTrack = "academic";
            developed.education.qualificationExperience = 225;
            developed.character.smarts = 90;
            developed.skills.Add(new StimSkillState { skillId = "professional", experience = 150 });

            var baselineChance = StimGameSessionService.CalculateInterviewSuccessChance(baseline, industry);
            var developedChance = StimGameSessionService.CalculateInterviewSuccessChance(developed, industry);

            Assert.That(baselineChance, Is.InRange(0f, 0.90f));
            Assert.That(developedChance, Is.InRange(0f, 0.90f));
            Assert.That(developedChance, Is.GreaterThan(baselineChance));
        }

        [Test]
        public void Retraining_IsUnavailableWhileEmployed()
        {
            var state = CreateValidSave().state;

            Assert.IsFalse(StimGameSessionService.TryGetCareerActionRequirement(
                state, StimCareerActionType.Retrain, out var requirement));
            Assert.That(requirement, Does.Contain("while unemployed"));
        }

        [Test]
        public void RepeatedLowPerformanceWarning_CanCauseDeterministicFiringAndUnemployment()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.rng.seed = 1;
            save.state.calendar.monthOfYear = 12;
            save.state.career = new StimCareerState
            {
                industryId = StimCareerCatalog.FinanceIndustryId,
                employerId = "stim_financial_group",
                roleTitle = "Junior Associate",
                annualSalaryMinorUnits = 4000000,
                careerProgress = 0,
                employmentStatus = "employed",
                performanceWarnings = 1
            };
            service.Start(save);

            AdvanceAndAssert(service);

            Assert.That(service.ActiveSave.state.career.employmentStatus, Is.EqualTo("unemployed"));
            Assert.That(string.IsNullOrEmpty(service.ActiveSave.state.career.roleTitle), Is.True);
            Assert.That(service.ActiveSave.state.career.annualSalaryMinorUnits, Is.Zero);
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(66));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "career" && entry.text.Contains("dismissed")), Is.True);
        }

        [Test]
        public void LocalServicesBusiness_StartUpgradeAndSaleAreTransactional()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 1000000;
            save.state.skills.Add(new StimSkillState { skillId = "professional", experience = 50 });
            service.Start(save);

            Assert.IsTrue(service.TryPerformBusinessAction(StimBusinessActionType.Start, out var started), started);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(900000));
            AdvanceAndAssert(service);
            Assert.IsTrue(service.TryPerformBusinessAction(StimBusinessActionType.Work, out var worked), worked);
            Assert.That(service.ActiveSave.state.business.operatingProgress, Is.EqualTo(20));
            AdvanceAndAssert(service);
            service.ActiveSave.state.business.operatingProgress =
                StimProgressionStandards.GetBusinessUpgradeProgressRequired(1) - 1;
            Assert.IsFalse(service.TryPerformBusinessAction(
                StimBusinessActionType.Upgrade, out var progressLocked));
            Assert.That(progressLocked, Does.Contain(
                $"{StimProgressionStandards.GetBusinessUpgradeProgressRequired(1)} operating progress"));
            service.ActiveSave.state.business.operatingProgress =
                StimProgressionStandards.GetBusinessUpgradeProgressRequired(1);
            Assert.IsTrue(service.TryPerformBusinessAction(
                StimBusinessActionType.Upgrade, out var upgraded), upgraded);
            Assert.That(service.ActiveSave.state.business.level, Is.EqualTo(2));
            AdvanceAndAssert(service);
            var cashBeforeSale = service.ActiveSave.state.finances.cashMinorUnits;
            Assert.IsTrue(service.TryPerformBusinessAction(StimBusinessActionType.Sell, out var sold), sold);
            Assert.That(service.ActiveSave.state.business.status, Is.EqualTo("sold"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits,
                Is.EqualTo(cashBeforeSale + service.ActiveSave.state.business.valuationMinorUnits));
            Assert.That(JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave)
                .state.business.status, Is.EqualTo("sold"));
        }

        [Test]
        public void BusinessStartup_EnforcesSkillFundsAndPersistenceRollback()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            service.Start(save);
            Assert.IsFalse(service.TryPerformBusinessAction(StimBusinessActionType.Start, out var skillLocked));
            Assert.That(skillLocked, Does.Contain("Professional Level 2"));
            service.ActiveSave.state.skills.Add(new StimSkillState { skillId = "professional", experience = 50 });
            service.ActiveSave.state.finances.cashMinorUnits = 99999;
            Assert.IsFalse(service.TryPerformBusinessAction(StimBusinessActionType.Start, out var fundsLocked));
            Assert.That(fundsLocked, Does.Contain("costs $1,000"));
            service.ActiveSave.state.finances.cashMinorUnits = 100000;
            repository.ShouldCommit = false;
            Assert.IsFalse(service.TryPerformBusinessAction(StimBusinessActionType.Start, out _));
            Assert.That(service.ActiveSave.state.business.status, Is.EqualTo("none"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(100000));
        }

        [Test]
        public void Business_ClosesAfterThirdLossAndBoundsLedger()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.rng.seed = 1;
            save.state.business = new StimBusinessState
            {
                businessId = "business_test", businessType = "local_services",
                displayName = "Local Services Co.", status = "operating", level = 1,
                locationLevel = 1, actionPoints = 3, maxActionPoints = 3,
                consecutiveLossMonths = 2, valuationMinorUnits = 100000
            };
            for (var index = 0; index < 60; index++)
                save.state.business.ledger.Add(new StimBusinessLedgerEntry
                {
                    entryId = $"old_{index}", type = "monthly_result", age = 24,
                    monthOfYear = 1, revision = index + 1, timestampUtc = "2026-07-13T17:00:00Z"
                });
            service.Start(save);

            AdvanceAndAssert(service);

            Assert.That(service.ActiveSave.state.business.status, Is.EqualTo("failed"));
            Assert.That(service.ActiveSave.state.business.valuationMinorUnits, Is.Zero);
            Assert.That(service.ActiveSave.state.business.ledger, Has.Count.EqualTo(60));
            Assert.That(service.ActiveSave.state.business.ledger[0].entryId, Is.EqualTo("old_1"));
        }

        [Test]
        public void BusinessValuation_UsesLevelAndPositiveLifetimeProfit()
        {
            var business = new StimBusinessState
                { status = "operating", level = 2, lifetimeProfitMinorUnits = 75000 };

            Assert.That(StimGameSessionService.CalculateBusinessValuation(business), Is.EqualTo(450000));
        }

        [Test]
        public void BusinessStaffing_AddsCapacityRevenueAndPayrollWithinActionPointLimit()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 1000000;
            save.state.skills.Add(new StimSkillState { skillId = "professional", experience = 50 });
            service.Start(save);
            Assert.IsTrue(service.TryPerformBusinessAction(StimBusinessActionType.Start, out _));
            Assert.IsTrue(service.TryPerformBusinessAction(StimBusinessActionType.HireStaff, out _));
            Assert.That(service.ActiveSave.state.business.staffCount, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.business.maxActionPoints, Is.EqualTo(4));
            Assert.That(service.ActiveSave.state.business.actionPoints, Is.EqualTo(2));
            Assert.IsTrue(service.TryPerformBusinessAction(StimBusinessActionType.Work, out _));
            Assert.IsTrue(service.TryPerformBusinessAction(StimBusinessActionType.Work, out _));
            Assert.IsFalse(service.TryPerformBusinessAction(StimBusinessActionType.Work, out var exhausted));
            Assert.That(exhausted, Does.Contain("No business action points"));

            AdvanceAndAssert(service);

            Assert.That(service.ActiveSave.state.business.actionPoints, Is.EqualTo(4));
            Assert.That(service.ActiveSave.state.business.lastExpensesMinorUnits, Is.EqualTo(105000));
        }

        [Test]
        public void BusinessLocationExpansionAndSeededDisruptionAffectMonthlyResult()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.rng.seed = 8;
            save.state.finances.cashMinorUnits = 1000000;
            save.state.business = new StimBusinessState
            {
                businessId = "business_test", businessType = "local_services",
                displayName = "Local Services Co.", status = "operating", level = 2,
                locationLevel = 1, actionPoints = 3, maxActionPoints = 3,
                operatingProgress = 50, valuationMinorUnits = 400000
            };
            service.Start(save);
            Assert.IsTrue(service.TryPerformBusinessAction(
                StimBusinessActionType.ExpandLocation, out var expanded), expanded);
            Assert.That(service.ActiveSave.state.business.locationLevel, Is.EqualTo(2));

            AdvanceAndAssert(service);

            Assert.That(service.ActiveSave.state.business.riskEventsExperienced, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.business.ledger[^1].type, Is.EqualTo("monthly_disruption"));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "business" && entry.text.Contains("operational disruption")), Is.True);
        }

        [TestCase(17)]
        [TestCase(71)]
        [TestCase(211)]
        public void SeededTenYearBusinessSimulation_IsDeterministicAndBalanceBounded(int seed)
        {
            var first = SimulateBusiness(seed, 120);
            var repeated = SimulateBusiness(seed, 120);

            Assert.That(repeated, Is.EqualTo(first));
            Assert.That(first, Is.InRange(-15000000L, 50000000L));
        }

        [Test]
        public void DailyGoal_ProgressesFromRealActivityAndClaimsRewardExactlyOnceAcrossReload()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            service.Start(save);
            Assert.IsTrue(service.TryPerformActivity(StimActivityType.Rest, out var activity), activity);
            var daily = service.ActiveSave.state.goals.Find(goal => goal.category == "daily");
            Assert.That(daily.status, Is.EqualTo("claimable"));
            var cashBefore = service.ActiveSave.state.finances.cashMinorUnits;

            Assert.IsTrue(service.TryClaimGoalReward(daily.goalId, out var claimed), claimed);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore + 1000));
            Assert.IsFalse(service.TryClaimGoalReward(daily.goalId, out var duplicate));
            Assert.That(duplicate, Does.Contain("already claimed"));

            var reloaded = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            reloaded.Start(JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave));
            Assert.IsFalse(reloaded.TryClaimGoalReward(daily.goalId, out duplicate));
            Assert.That(reloaded.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore + 1000));
        }

        [Test]
        public void MainAndLifeGoals_UseCareerAndNetWorthProgressWithDestinationMetadata()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 10000000;
            service.Start(save);
            Assert.IsTrue(service.TryPerformCareerAction(StimCareerActionType.WorkHard, out var worked), worked);

            var main = service.ActiveSave.state.goals.Find(goal => goal.goalId == "main_first_career");
            var life = service.ActiveSave.state.goals.Find(goal => goal.goalId == "life_net_worth_100k");
            Assert.That(main.status, Is.EqualTo("claimable"));
            Assert.That(main.destination, Is.EqualTo("career"));
            Assert.That(life.status, Is.EqualTo("claimable"));
            Assert.That(life.destination, Is.EqualTo("money"));
        }

        [Test]
        public void DailyGoals_ExpireAndRemainBoundedAcrossLongLives()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            service.Start(save);
            for (var month = 0; month < 30; month++)
            {
                service.ActiveSave.state.character.age = 24 + month / 12;
                service.ActiveSave.state.calendar.monthOfYear = month % 12 + 1;
                service.ActiveSave.state.statuses.RemoveAll(status => status.statusId == "monthly_focus_used");
                Assert.IsTrue(service.TryPerformActivity(StimActivityType.Rest, out var activity), activity);
            }

            Assert.That(service.ActiveSave.state.goals.Count, Is.LessThanOrEqualTo(20));
            Assert.That(service.ActiveSave.state.goals.Count(goal => goal.category == "daily" &&
                goal.status != "expired"), Is.EqualTo(1));
        }

        [Test]
        public void AchievementPrize_ClaimsExactlyOnceAndSurvivesReload()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.achievements.Add(new StimAchievementState
            {
                achievementId = "first_job", unlockedAtAge = 18, revision = 1,
                timestampUtc = "2026-07-13T17:00:00Z"
            });
            service.Start(save);
            var cashBefore = service.ActiveSave.state.finances.cashMinorUnits;

            Assert.IsTrue(service.TryClaimAchievementReward("first_job", out var claimed), claimed);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore + 50000));
            Assert.That(service.ActiveSave.state.achievements[0].rewardClaimed, Is.True);
            Assert.IsFalse(service.TryClaimAchievementReward("first_job", out var duplicate));
            Assert.That(duplicate, Does.Contain("already claimed"));

            var reloaded = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            reloaded.Start(JsonUtility.FromJson<StimSaveEnvelope>(repository.LastCommittedSave));
            Assert.IsFalse(reloaded.TryClaimAchievementReward("first_job", out duplicate));
            Assert.That(reloaded.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore + 50000));
        }

        [Test]
        public void AchievementPrize_RollsBackWhenAutosaveFails()
        {
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.achievements.Add(new StimAchievementState
            {
                achievementId = "first_year", unlockedAtAge = 1, revision = 1,
                timestampUtc = "2026-07-13T17:00:00Z"
            });
            service.Start(save);
            var cashBefore = service.ActiveSave.state.finances.cashMinorUnits;

            Assert.IsFalse(service.TryClaimAchievementReward("first_year", out _));

            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore));
            Assert.That(service.ActiveSave.state.achievements[0].rewardClaimed, Is.False);
        }

        [Test]
        public void AchievementPrizes_AreDefinedForEveryAuthoredAchievement()
        {
            var ids = new[]
            {
                "first_year", "school_days", "learning_level_2", "family_bond", "first_job",
                "moving_up", "six_figures", "first_choice", "retirement", "life_complete"
            };

            Assert.That(ids.All(id => StimGameSessionService.GetAchievementRewardMinorUnits(id) > 0), Is.True);
        }

        [Test]
        public void NewLifeTransition_PersistsAndCanBeAcknowledgedOnlyOnce()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();

            Assert.IsTrue(service.TryStartNewLife(save, out var started), started);
            var transition = service.GetPendingTransition();
            Assert.That(transition, Is.Not.Null);
            Assert.That(transition.transitionType, Is.EqualTo("new_life"));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "milestone" && entry.text.Contains("A new life begins")), Is.True);

            var reloaded = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            Assert.IsTrue(reloaded.TryLoadLatest(out var loaded), loaded);
            Assert.That(reloaded.GetPendingTransition()?.transitionId, Is.EqualTo(transition.transitionId));
            Assert.IsTrue(reloaded.TryAcknowledgeTransition(transition.transitionId, out var acknowledged), acknowledged);
            Assert.That(reloaded.GetPendingTransition(), Is.Null);
            Assert.IsFalse(reloaded.TryAcknowledgeTransition(transition.transitionId, out var duplicate));
            Assert.That(duplicate, Does.Contain("already acknowledged"));
        }

        [Test]
        public void FirstLifeOrientation_CompletesOnceAndSurvivesReload()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository,
                utcNow: () => DateTimeOffset.Parse("2026-07-14T12:00:00Z"));
            var save = StimNewLifeFactory.Create(new StimNewLifeRequest(), "0.1.0",
                DateTimeOffset.Parse("2026-07-14T11:00:00Z"), 42);
            Assert.IsTrue(service.TryStartNewLife(save, out var started), started);
            Assert.That(service.ShouldPresentFirstLifeOrientation(), Is.True);

            Assert.IsTrue(service.TryCompleteFirstLifeOrientation(out var completed), completed);
            Assert.That(service.ShouldPresentFirstLifeOrientation(), Is.False);
            Assert.IsFalse(service.TryCompleteFirstLifeOrientation(out var duplicate));
            Assert.That(duplicate, Does.Contain("already completed"));

            var reloaded = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            Assert.IsTrue(reloaded.TryLoadLatest(out var loaded), loaded);
            Assert.That(reloaded.ShouldPresentFirstLifeOrientation(), Is.False);
            Assert.That(reloaded.ActiveSave.state.orientation.completedRevision, Is.GreaterThan(0));
        }

        [Test]
        public void SecondaryGraduation_QueuesFocusedPersistedTransition()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 17;
            save.state.character.lifeStage = "teen";
            save.state.calendar.monthOfYear = 12;
            save.state.education.stage = "high_school";
            save.state.education.schoolPath = "academic_track";
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var summary), summary);

            Assert.That(service.GetPendingTransition()?.transitionType, Is.EqualTo("graduation"));
            Assert.That(service.GetPendingTransition()?.summary, Does.Contain("completed secondary school"));
        }

        [Test]
        public void AuthoredEventRequirements_CanGateByStudyTrackAndQualificationExperience()
        {
            var gatedEvent = RepresentativeStimEvents.CreateSalaryNegotiation();
            gatedEvent.requirementsJson =
                "{\"studyTrack\":\"academic\",\"minimumQualificationExperience\":50}";
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(gatedEvent);

            var lockedService = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var lockedSave = CreateValidSave();
            lockedSave.state.calendar.monthOfYear = 12;
            lockedSave.state.education = new StimEducationState
            {
                studyTrack = "academic",
                qualificationExperience = 49
            };
            lockedService.Start(lockedSave);
            Assert.IsTrue(lockedService.TryAdvanceMonth(out var lockedEvent, out var lockedSummary), lockedSummary);
            Assert.That(lockedEvent, Is.Null);

            var wrongTrackService = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var wrongTrackSave = CreateValidSave();
            wrongTrackSave.state.character.age = 15;
            wrongTrackSave.state.character.lifeStage = "teen";
            wrongTrackSave.state.calendar.monthOfYear = 12;
            wrongTrackSave.state.education = new StimEducationState
            {
                studyTrack = "undecided",
                qualificationExperience = 10
            };
            wrongTrackService.Start(wrongTrackSave);
            Assert.IsTrue(wrongTrackService.TryAdvanceMonth(
                out var wrongTrackEvent, out var wrongTrackSummary), wrongTrackSummary);
            Assert.That(wrongTrackEvent, Is.Null);

            var eligibleService = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var eligibleSave = CreateValidSave();
            eligibleSave.state.calendar.monthOfYear = 12;
            eligibleSave.state.education = new StimEducationState
            {
                studyTrack = "academic",
                qualificationExperience = 50
            };
            eligibleService.Start(eligibleSave);
            Assert.IsTrue(eligibleService.TryAdvanceMonth(
                out var eligibleEvent, out var eligibleSummary), eligibleSummary);
            Assert.That(eligibleEvent?.id, Is.EqualTo(RepresentativeStimEvents.SalaryNegotiationId));
        }

        [TestCase("general", RepresentativeStimEvents.AppliedFinanceChallengeId)]
        [TestCase("academic", RepresentativeStimEvents.CommunityHealthChallengeId)]
        [TestCase("vocational", RepresentativeStimEvents.SustainableTradesChallengeId)]
        public void DisciplineChallenge_RequiresMatchingTrackAndExperience(
            string studyTrack, string expectedEventId)
        {
            var catalog = new InMemoryStimEventCatalog();
            foreach (var evt in RepresentativeStimEvents.CreateLaunchAlphaCatalog())
            {
                if (evt.id == expectedEventId)
                {
                    catalog.Upsert(evt);
                }
            }

            var lockedService = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var lockedSave = CreateValidSave();
            lockedSave.state.character.age = 15;
            lockedSave.state.character.lifeStage = "teen";
            lockedSave.state.calendar.monthOfYear = 12;
            lockedSave.state.education = new StimEducationState
            {
                studyTrack = studyTrack,
                qualificationExperience = 9
            };
            lockedService.Start(lockedSave);
            Assert.IsTrue(lockedService.TryAdvanceMonth(out var lockedEvent, out var lockedSummary), lockedSummary);
            Assert.That(lockedEvent, Is.Null);

            var eligibleService = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var eligibleSave = CreateValidSave();
            eligibleSave.state.character.age = 15;
            eligibleSave.state.character.lifeStage = "teen";
            eligibleSave.state.calendar.monthOfYear = 12;
            eligibleSave.state.education = new StimEducationState
            {
                studyTrack = studyTrack,
                qualificationExperience = 10
            };
            eligibleService.Start(eligibleSave);
            Assert.IsTrue(eligibleService.TryAdvanceMonth(
                out var eligibleEvent, out var eligibleSummary), eligibleSummary);
            Assert.That(eligibleEvent?.id, Is.EqualTo(expectedEventId));
        }

        [TestCase("Junior Associate", "Associate", 5500000, 25)]
        [TestCase("Associate", "Senior Associate", 7500000, 50)]
        [TestCase("Senior Associate", "Manager", 10000000, 75)]
        public void CareerLadder_DefinesPromotionThresholds(
            string role,
            string expectedNextRole,
            long expectedSalary,
            int expectedProgress)
        {
            Assert.IsTrue(StimGameSessionService.TryGetNextCareerStep(
                role, out var nextRole, out var salary, out var progress));
            Assert.That(nextRole, Is.EqualTo(expectedNextRole));
            Assert.That(salary, Is.EqualTo(expectedSalary));
            Assert.That(progress, Is.EqualTo(expectedProgress));
        }

        [Test]
        public void FinanceCareerLadder_IsReachableWithinTwoYearsOfMonthlyActions()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.career = new StimCareerState
            {
                industryId = StimCareerCatalog.FinanceIndustryId,
                employerId = "stim_financial_group",
                roleTitle = "Junior Associate",
                annualSalaryMinorUnits = 4000000,
                employmentStatus = "employed"
            };
            service.Start(save);

            var expectedRoles = new[] { "Associate", "Senior Associate", "Manager" };
            var months = 0;
            foreach (var expectedRole in expectedRoles)
            {
                Assert.IsTrue(StimGameSessionService.TryGetNextCareerStep(
                    service.ActiveSave.state.career.roleTitle, out _, out _, out var requiredProgress));
                while (service.ActiveSave.state.career.careerProgress < requiredProgress)
                {
                    Assert.IsTrue(service.TryPerformCareerAction(
                        StimCareerActionType.WorkHard, out var workSummary), workSummary);
                    AdvanceAndAssert(service);
                    months++;
                }
                Assert.IsTrue(service.TryPerformCareerAction(
                    StimCareerActionType.AskForPromotion, out var promotionSummary), promotionSummary);
                Assert.That(service.ActiveSave.state.career.roleTitle, Is.EqualTo(expectedRole));
                if (expectedRole == "Manager") continue;
                AdvanceAndAssert(service);
                months++;
            }

            Assert.That(months, Is.LessThanOrEqualTo(24));
        }

        [Test]
        public void CareerPromotion_RequiresProgressAndResetsItAfterPromotion()
        {
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.career.roleTitle = "Junior Associate";
            save.state.career.annualSalaryMinorUnits = 4000000;
            save.state.career.careerProgress = 24;
            service.Start(save);

            Assert.IsFalse(service.TryPerformCareerAction(
                StimCareerActionType.AskForPromotion, out var lockedSummary));
            Assert.That(lockedSummary, Does.Contain("Requires 25 career progress"));
            service.ActiveSave.state.career.careerProgress = 25;
            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.AskForPromotion, out var promotionSummary), promotionSummary);
            Assert.That(service.ActiveSave.state.career.roleTitle, Is.EqualTo("Associate"));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.Zero);
            Assert.That(promotionSummary, Does.Contain("Salary +$15,000"));
        }

        [Test]
        public void CareerRetirement_IsAgeGatedAndStopsSalaryProgression()
        {
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 64;
            service.Start(save);

            Assert.IsFalse(service.TryPerformCareerAction(
                StimCareerActionType.Retire, out var lockedSummary));
            Assert.That(lockedSummary, Does.Contain("age 65"));
            service.ActiveSave.state.character.age = 65;
            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.Retire, out var retirementSummary), retirementSummary);
            Assert.That(service.ActiveSave.state.career.roleTitle, Is.EqualTo("Retired"));
            Assert.That(service.ActiveSave.state.career.annualSalaryMinorUnits, Is.Zero);
            Assert.That(retirementSummary, Does.Contain("Retired from Analyst"));
            Assert.That(service.ActiveSave.state.character.lifeStatus, Is.EqualTo("retired"));
            Assert.That(service.ActiveSave.state.character.endedAtAge, Is.EqualTo(65));
            Assert.That(service.GetPendingTransition()?.transitionType, Is.EqualTo("retirement"));
            Assert.IsFalse(service.TryAdvanceMonth(out _, out var endedSummary));
            Assert.That(endedSummary, Does.Contain("life has ended"));
        }

        [TestCase(0, 0)]
        [TestCase(4000000, 1923)]
        [TestCase(5500000, 2644)]
        [TestCase(10000000, 4808)]
        public void ManualWorkTap_UsesAnnualSalaryHourlyRate(long annualSalary, long expectedHourlyRate)
        {
            Assert.That(
                StimGameSessionService.CalculateHourlyRateMinorUnits(annualSalary),
                Is.EqualTo(expectedHourlyRate));
        }

        [Test]
        public void ManualWorkTap_AddsOneHourlyRateAndAutosaves()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.career.roleTitle = "Junior Associate";
            save.state.career.annualSalaryMinorUnits = 4000000;
            var cashBefore = save.state.finances.cashMinorUnits;
            service.Start(save);

            Assert.IsTrue(service.TryPerformManualWorkTap(out var earnings, out var summary), summary);

            Assert.That(earnings, Is.EqualTo(1923));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore + 1923));
            Assert.That(service.ActiveSave.revision, Is.EqualTo(2));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
            Assert.That(summary, Does.Contain("Cash +$19.23"));
        }

        [Test]
        public void ManualWorkTap_RejectsUnemployedAndRollsBackFailedCommit()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.career = new StimCareerState();
            service.Start(save);

            Assert.IsFalse(service.TryPerformManualWorkTap(out var unemployedEarnings, out var unemployedSummary));
            Assert.That(unemployedEarnings, Is.Zero);
            Assert.That(unemployedSummary, Does.Contain("salaried job"));

            save.state.career = new StimCareerState
            {
                roleTitle = "Associate",
                annualSalaryMinorUnits = 5500000
            };
            service.Start(save);
            repository.ShouldCommit = false;
            var cashBefore = service.ActiveSave.state.finances.cashMinorUnits;
            Assert.IsFalse(service.TryPerformManualWorkTap(out var failedEarnings, out _));
            Assert.That(failedEarnings, Is.Zero);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashBefore));
        }

        [TestCase(49, 0)]
        [TestCase(50, 1)]
        [TestCase(64, 1)]
        [TestCase(65, 2)]
        [TestCase(80, 4)]
        public void AnnualHealthDecline_IncreasesWithAge(int age, int expectedDecline)
        {
            Assert.That(StimGameSessionService.GetAnnualHealthDecline(age), Is.EqualTo(expectedDecline));
        }

        [Test]
        public void AnnualHealthDecline_CanEndLifeAndBlocksFurtherActions()
        {
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 79;
            save.state.character.health = 4;
            save.state.character.lifeStage = StimGameSessionService.GetLifeStage(79);
            save.state.calendar.monthOfYear = 12;
            save.state.career = new StimCareerState();
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);

            Assert.That(nextEvent, Is.Null);
            Assert.That(service.ActiveSave.state.character.health, Is.Zero);
            Assert.That(service.ActiveSave.state.character.lifeStatus, Is.EqualTo("deceased"));
            Assert.That(service.ActiveSave.state.character.endedAtAge, Is.EqualTo(80));
            Assert.That(summary, Does.Contain("death at age 80"));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].category, Is.EqualTo("milestone"));
            Assert.IsFalse(service.TryPerformActivity(StimActivityType.Rest, out var actionSummary));
            Assert.That(actionSummary, Does.Contain("life has ended"));
        }

        [Test]
        public void Achievements_UnlockPersistAndDoNotDuplicate()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 10;
            save.state.education.stage = "primary_school";
            save.state.career = new StimCareerState();
            save.state.skills.Add(new StimSkillState { skillId = "learning", experience = 45 });
            service.Start(save);

            Assert.IsTrue(service.TryPerformEducationAction(
                StimEducationActionType.Read, out var actionSummary), actionSummary);

            Assert.That(service.ActiveSave.state.achievements.Exists(
                achievement => achievement.achievementId == "first_year"), Is.True);
            Assert.That(service.ActiveSave.state.achievements.Exists(
                achievement => achievement.achievementId == "school_days"), Is.True);
            Assert.That(service.ActiveSave.state.achievements.Exists(
                achievement => achievement.achievementId == "learning_level_2"), Is.True);
            var countAfterUnlock = service.ActiveSave.state.achievements.Count;

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var advanceSummary), advanceSummary);
            Assert.That(service.ActiveSave.state.achievements, Has.Count.EqualTo(countAfterUnlock));
            Assert.That(service.ActiveSave.state.lifeFeed.Count(entry =>
                entry.category == "achievement" && entry.text.Contains("Curious Mind")), Is.EqualTo(1));
        }

        [Test]
        [Category("SlowSimulation")]
        public void SeededLife_ProgressesFromBirthToDeathWithoutDeveloperIntervention()
        {
            var catalog = CreateRepresentativeCatalog();
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(
                catalog,
                repository,
                utcNow: () => DateTimeOffset.Parse("2026-07-13T20:00:00Z"));
            var save = StimNewLifeFactory.Create(
                new StimNewLifeRequest
                {
                    firstName = "Ari",
                    lastName = "Morgan",
                    country = "Jamaica",
                    backgroundId = StimNewLifeFactory.WorkingClassBackground
                },
                "0.1.0",
                DateTimeOffset.Parse("2026-07-13T19:00:00Z"),
                24680);
            Assert.IsTrue(service.TryStartNewLife(save, out var startSummary), startSummary);

            var stopwatch = Stopwatch.StartNew();
            var months = 0;
            const int maximumMonths = 1800;
            while (service.ActiveSave.state.character.lifeStatus == "active" && months < maximumMonths)
            {
                if (!string.IsNullOrEmpty(service.ActiveSave.state.education.awaitingDecisionId))
                {
                    var schoolChoice = service.ActiveSave.state.education.awaitingDecisionId == "education_high_transition"
                        ? StimSchoolPathChoice.AcademicTrack
                        : StimSchoolPathChoice.PublicSchool;
                    Assert.IsTrue(service.TryChooseSchoolPath(schoolChoice, out var schoolSummary), schoolSummary);
                }
                Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var advanceSummary), advanceSummary);
                months++;
                if (nextEvent == null) continue;
                Assert.That(nextEvent.choices, Is.Not.Empty, $"Event {nextEvent.id} has no playable choice.");
                var selectedChoice = nextEvent.choices.Find(choice =>
                    StimGameSessionService.CalculateChoicePotentialCost(
                        nextEvent, choice, service.ActiveSave.state) == 0);
                if (selectedChoice == null)
                {
                    selectedChoice = nextEvent.choices.Find(choice =>
                        StimGameSessionService.CalculateChoicePotentialCost(
                            nextEvent, choice, service.ActiveSave.state) <=
                        service.ActiveSave.state.finances.cashMinorUnits);
                }
                if (selectedChoice == null)
                {
                    var availableCredit = Math.Max(0,
                        StimGameSessionService.CalculateHouseholdCreditLimit(service.ActiveSave.state) -
                        service.ActiveSave.state.finances.debtMinorUnits);
                    selectedChoice = nextEvent.choices.Find(choice =>
                        StimGameSessionService.CalculateChoicePotentialCost(
                            nextEvent, choice, service.ActiveSave.state) <= availableCredit);
                }
                Assert.That(selectedChoice, Is.Not.Null,
                    $"Event {nextEvent.id} has no choice payable with current cash or available credit.");
                var potentialCost = StimGameSessionService.CalculateChoicePotentialCost(
                    nextEvent, selectedChoice, service.ActiveSave.state);
                var paymentMethod = potentialCost <= service.ActiveSave.state.finances.cashMinorUnits
                    ? StimPaymentMethod.Cash
                    : StimPaymentMethod.Credit;
                Assert.IsTrue(service.TryResolveChoice(
                    nextEvent.id,
                    selectedChoice.id,
                    paymentMethod,
                    out var resolutionSummary), resolutionSummary);
            }

            Assert.That(months, Is.LessThan(maximumMonths), "The seeded life did not reach an ending.");
            Assert.That(service.ActiveSave.state.character.lifeStatus, Is.EqualTo("deceased"));
            Assert.That(service.ActiveSave.state.character.endedAtAge, Is.GreaterThan(0));
            Assert.That(service.ActiveSave.state.pendingEventId, Is.Null.Or.Empty);
            Assert.That(service.ActiveSave.state.achievements.Exists(
                achievement => achievement.achievementId == "life_complete"), Is.True);
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "milestone" && entry.text.Contains("ended in death")), Is.True);
            Assert.That(repository.CommitCount, Is.GreaterThan(months));
            stopwatch.Stop();
            TestContext.Progress.WriteLine(
                $"SlowSimulation: {months} months, {repository.CommitCount} commits, " +
                $"{repository.MaxSerializedLength} max JSON chars, " +
                $"{service.ActiveSave.state.lifeFeed.Count} feed entries, " +
                $"{stopwatch.ElapsedMilliseconds} ms.");
        }

        [Test]
        public void SchoolTransition_RequiresAChoiceAndPersistsTheSelectedBranch()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                repository,
                utcNow: () => DateTimeOffset.Parse("2026-07-13T20:00:00Z"));
            var save = CreateValidSave();
            save.state.character.age = 5;
            save.state.calendar.monthOfYear = 12;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var transitionSummary), transitionSummary);
            Assert.That(service.ActiveSave.state.education.awaitingDecisionId,
                Is.EqualTo("education_primary_enrollment"));
            Assert.IsFalse(service.TryAdvanceMonth(out _, out var blockedSummary));
            Assert.That(blockedSummary, Does.Contain("Choose a school path"));

            Assert.IsTrue(service.TryChooseSchoolPath(
                StimSchoolPathChoice.Homeschool,
                out var choiceSummary), choiceSummary);

            Assert.That(service.ActiveSave.state.education.schoolPath, Is.EqualTo("homeschool"));
            Assert.That(service.ActiveSave.state.education.awaitingDecisionId, Is.Empty);
            Assert.That(service.ActiveSave.state.lifeDecisions, Has.Count.EqualTo(1));
            Assert.That(service.ActiveSave.state.lifeDecisions[0].decisionId,
                Is.EqualTo("education_primary_enrollment"));
            Assert.That(service.ActiveSave.state.lifeDecisions[0].choiceId, Is.EqualTo("homeschool"));
            var peer = service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "school_peer_primary");
            Assert.That(peer, Is.Not.Null);
            Assert.That(peer.relationshipType, Is.EqualTo("friend"));
            Assert.That(peer.origin, Is.EqualTo("neighborhood"));
            Assert.That(peer.introducedAtAge, Is.EqualTo(6));
            Assert.That(repository.LastCommittedSave, Does.Contain("education_primary_enrollment"));
        }

        [Test]
        public void SchoolPathChoice_RollsBackWhenAutosaveFails()
        {
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.education.stage = "high_school";
            save.state.education.awaitingDecisionId = "education_high_transition";
            service.Start(save);

            Assert.IsFalse(service.TryChooseSchoolPath(
                StimSchoolPathChoice.VocationalTrack,
                out _));

            Assert.That(service.ActiveSave.state.education.schoolPath, Is.Null.Or.Empty);
            Assert.That(service.ActiveSave.state.education.awaitingDecisionId,
                Is.EqualTo("education_high_transition"));
            Assert.That(service.ActiveSave.state.lifeDecisions, Is.Empty);
        }

        [Test]
        public void ContextActivity_OvertimeRequiresEmploymentAndCommitsPayProgressAndHealth()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.character.age = 30;
            save.state.career.roleTitle = string.Empty;
            service.Start(save);

            Assert.IsFalse(service.TryPerformActivity(StimActivityType.Overtime, out var lockedSummary));
            Assert.That(lockedSummary, Does.Contain("active job"));

            service.ActiveSave.state.career.roleTitle = "Junior Associate";
            service.ActiveSave.state.career.annualSalaryMinorUnits = 4000000;
            var cashBefore = service.ActiveSave.state.finances.cashMinorUnits;
            var healthBefore = service.ActiveSave.state.character.health;

            Assert.IsTrue(service.TryPerformActivity(StimActivityType.Overtime, out var summary), summary);

            Assert.That(service.ActiveSave.state.finances.cashMinorUnits,
                Is.EqualTo(cashBefore + StimGameSessionService.CalculateHourlyRateMinorUnits(4000000) * 4));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.EqualTo(6));
            Assert.That(service.ActiveSave.state.character.health, Is.EqualTo(healthBefore - 2));
            var overtimeFeedEntry = service.ActiveSave.state.lifeFeed.FindLast(entry =>
                entry != null && entry.category == "activity" && entry.text.Contains("Worked overtime"));
            Assert.That(overtimeFeedEntry, Is.Not.Null);
        }

        [Test]
        public void FitnessLevel_ReducesOvertimeHealthStrain()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 30;
            save.state.career.roleTitle = "Junior Associate";
            save.state.career.annualSalaryMinorUnits = 4000000;
            save.state.skills.Add(new StimSkillState { skillId = "fitness", experience = 50 });
            service.Start(save);
            var healthBefore = service.ActiveSave.state.character.health;

            Assert.IsTrue(service.TryPerformActivity(StimActivityType.Overtime, out var summary), summary);

            Assert.That(service.ActiveSave.state.character.health, Is.EqualTo(healthBefore - 1));
            Assert.That(summary, Does.Contain("Health −1").And.Contain("Fitness reduced strain"));
        }

        [Test]
        public void ProfessionalLevel_IncreasesCareerProgressFromWorkingHard()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.career.roleTitle = "Junior Associate";
            save.state.career.careerProgress = 0;
            save.state.skills.Add(new StimSkillState { skillId = "professional", experience = 50 });
            service.Start(save);

            Assert.IsTrue(service.TryPerformCareerAction(
                StimCareerActionType.WorkHard, out var summary), summary);

            Assert.That(service.ActiveSave.state.career.careerProgress, Is.EqualTo(12));
            Assert.That(summary, Does.Contain("Career +12")
                .And.Contain("Professional Level 2 bonus"));
        }

        [Test]
        public void PeerRelationship_CanBecomeARivalAndReconcileLater()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 15;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                displayName = "Maya",
                relationshipType = "friend",
                origin = "primary_school",
                introducedAtAge = 6,
                value = 27
            });
            service.Start(save);

            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "school_peer_primary", StimRelationshipInteractionType.Argue, out var argueSummary), argueSummary);
            Assert.That(service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "school_peer_primary").relationshipType, Is.EqualTo("rival"));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var advanceSummary), advanceSummary);
            Assert.IsTrue(service.TryPerformRelationshipInteraction(
                "school_peer_primary", StimRelationshipInteractionType.Reconcile, out var reconcileSummary), reconcileSummary);
            Assert.That(service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "school_peer_primary").relationshipType, Is.EqualTo("friend"));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "relationship" && entry.text.Contains("Reconcile with Maya")), Is.True);
        }

        [Test]
        public void Friendship_NeglectCanDowngradeBestFriendAndRecordsTheChange()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "old_friend",
                displayName = "Alex",
                relationshipType = "best_friend",
                origin = "primary_school",
                introducedAtAge = 6,
                value = 75,
                monthsSinceInteraction = 3
            });
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var summary), summary);

            var friendship = service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "old_friend");
            Assert.That(friendship.value, Is.EqualTo(74));
            Assert.That(friendship.relationshipType, Is.EqualTo("friend"));
            Assert.That(friendship.monthsSinceInteraction, Is.EqualTo(4));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "relationship" && entry.text.Contains("best friend to friend")), Is.True);
        }

        [Test]
        public void ScheduledFollowUp_TriggersAtAnnualRolloverAndIsConsumed()
        {
            var followUp = RepresentativeStimEvents.CreateSalaryNegotiation();
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(followUp);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.calendar.monthOfYear = 12;
            save.state.scheduledEvents.Add(new StimScheduledEventRecord
            {
                eventId = followUp.id,
                earliestTriggerAge = 25,
                latestTriggerAge = 26,
                chance = 1f,
                sourceEventId = "relationship_drama_setup"
            });
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);

            Assert.That(nextEvent, Is.Not.Null);
            Assert.That(nextEvent.id, Is.EqualTo(followUp.id));
            Assert.That(service.ActiveSave.state.pendingEventId, Is.EqualTo(followUp.id));
            Assert.That(service.ActiveSave.state.scheduledEvents, Is.Empty);
        }

        [Test]
        public void PeerDrama_BetrayalSchedulesAndResolvesLaterConsequence()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreatePeerTrustConflict());
            catalog.Upsert(RepresentativeStimEvents.CreatePeerTrustAftermath());
            catalog.Upsert(RepresentativeStimEvents.CreatePeerJealousy());
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 14;
            save.state.calendar.monthOfYear = 6;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary",
                displayName = "Maya",
                relationshipType = "friend",
                origin = "primary_school",
                introducedAtAge = 6,
                value = 55
            });
            service.Start(save);

            Assert.IsTrue(service.TryResolveChoice(
                RepresentativeStimEvents.PeerTrustConflictId,
                "share_secret",
                out var betrayalSummary), betrayalSummary);
            Assert.That(service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "school_peer_primary").value, Is.EqualTo(37));
            Assert.That(service.ActiveSave.state.scheduledEvents, Has.Count.EqualTo(1));
            Assert.That(service.ActiveSave.state.scheduledEvents[0].eventId,
                Is.EqualTo(RepresentativeStimEvents.PeerTrustAftermathId));

            service.ActiveSave.state.calendar.monthOfYear = 12;
            Assert.IsTrue(service.TryAdvanceMonth(out var followUp, out var advanceSummary), advanceSummary);
            Assert.That(followUp?.id, Is.EqualTo(RepresentativeStimEvents.PeerTrustAftermathId));
            Assert.IsTrue(service.TryResolveChoice(
                followUp.id,
                "talk_honestly",
                out var repairSummary), repairSummary);

            Assert.That(service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "school_peer_primary").value, Is.EqualTo(47));
            Assert.That(service.ActiveSave.state.eventHistory.Exists(entry =>
                entry.eventId == RepresentativeStimEvents.PeerTrustAftermathId), Is.True);
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "event" && entry.text.Contains("honest conversation")), Is.True);
        }

        [Test]
        public void PeerJealousy_RequiresBothPeersAndBranchesBothRelationships()
        {
            var jealousy = RepresentativeStimEvents.CreatePeerJealousy();
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(jealousy);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 14;
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_primary", displayName = "Maya",
                relationshipType = "friend", value = 50
            });
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var unavailable, out var quietSummary), quietSummary);
            Assert.That(unavailable, Is.Null);

            service.ActiveSave.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = "school_peer_middle", displayName = "Alex",
                relationshipType = "friend", value = 50
            });
            Assert.IsTrue(service.TryAdvanceMonth(out var eventWithBothPeers, out var eventSummary), eventSummary);
            Assert.That(eventWithBothPeers?.id, Is.EqualTo(RepresentativeStimEvents.PeerJealousyId));
            Assert.IsTrue(service.TryResolveChoice(
                eventWithBothPeers.id,
                "choose_new_friend",
                out var resolutionSummary), resolutionSummary);

            Assert.That(service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "school_peer_primary").value, Is.EqualTo(38));
            Assert.That(service.ActiveSave.state.relationships.Find(relationship =>
                relationship.relationshipId == "school_peer_middle").value, Is.EqualTo(60));
            Assert.That(service.ActiveSave.state.lifeFeed.Exists(entry =>
                entry.category == "event" && entry.text.Contains("newer bond grows stronger")), Is.True);
        }

        [Test]
        public void LuckWeightsRandomGainsUpAndRandomLossesDown()
        {
            var gain = RepresentativeStimEvents.CreateRandomGain();
            var loss = RepresentativeStimEvents.CreateRandomLoss();

            Assert.That(StimGameSessionService.GetLuckEventWeight(gain, 90),
                Is.GreaterThan(StimGameSessionService.GetLuckEventWeight(gain, 10)));
            Assert.That(StimGameSessionService.GetLuckEventWeight(loss, 90),
                Is.LessThan(StimGameSessionService.GetLuckEventWeight(loss, 10)));
        }

        [Test]
        public void PerformActivity_RejectsSecondFocusUntilMonthAdvances()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                repository);
            service.Start(CreateValidSave());

            Assert.IsTrue(service.TryPerformActivity(StimActivityType.Study, out var firstSummary), firstSummary);
            var secondPerformed = service.TryPerformActivity(StimActivityType.Workout, out var secondSummary);

            Assert.IsFalse(secondPerformed);
            Assert.That(secondSummary, Does.Contain("already used"));
            Assert.That(service.ActiveSave.state.character.health, Is.EqualTo(80));
            Assert.That(service.ActiveSave.revision, Is.EqualTo(2));
            Assert.That(repository.CommitCount, Is.EqualTo(1));

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var advanceSummary), advanceSummary);
            Assert.IsTrue(service.TryPerformActivity(StimActivityType.Workout, out var thirdSummary), thirdSummary);
            Assert.That(service.ActiveSave.state.character.health, Is.EqualTo(82));
        }

        [Test]
        public void AdvanceMonth_CanTriggerOrdinaryEventBeforeAnnualRollover()
        {
            var evt = RepresentativeStimEvents.CreateHealthBurnout();
            evt.monthlyTriggerChance = 1f;
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(evt);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            service.Start(CreateValidSave());

            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);

            Assert.That(nextEvent?.id, Is.EqualTo(evt.id));
            Assert.That(service.ActiveSave.state.character.age, Is.EqualTo(24));
            Assert.That(service.ActiveSave.state.calendar.monthOfYear, Is.EqualTo(2));
            Assert.That(service.ActiveSave.state.pendingEventId, Is.EqualTo(evt.id));
        }

        [Test]
        public void AdvanceMonth_RespectsSpecificMonthTiming()
        {
            var evt = RepresentativeStimEvents.CreateHealthBurnout();
            evt.timingPolicy = EventTimingPolicy.SpecificMonth;
            evt.requiredMonth = 2;
            evt.monthlyTriggerChance = 1f;
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(evt);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            service.Start(CreateValidSave());

            Assert.IsTrue(service.TryAdvanceMonth(out var firstEvent, out var firstSummary), firstSummary);
            Assert.That(firstEvent, Is.Null);

            Assert.IsTrue(service.TryAdvanceMonth(out var secondEvent, out var secondSummary), secondSummary);
            Assert.That(secondEvent?.id, Is.EqualTo(evt.id));
        }

        [Test]
        public void AdvanceMonth_AppliesTaxesExpensesAndPositiveNetStatFeedback()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.monthlyLivingExpensesMinorUnits = 300000;
            save.state.finances.taxRateBasisPoints = 2000;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var summary), summary);

            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(133334));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(0));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(71));
            Assert.That(summary, Contains.Substring("taxes"));
            Assert.That(summary, Contains.Substring("expenses"));
        }

        [Test]
        public void AdvanceMonth_ConvertsUncoveredNegativeCashFlowIntoDebt()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.career.annualSalaryMinorUnits = 0;
            save.state.finances.monthlyLivingExpensesMinorUnits = 200000;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var summary), summary);

            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(0));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(100000));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(68));
            Assert.That(summary, Contains.Substring("net -"));
        }

        [Test]
        public void AdvanceMonth_SelectsEligibleEventEvenWithLowAuthoredWeight()
        {
            var evt = RepresentativeStimEvents.CreateHealthBurnout();
            evt.monthlyTriggerChance = 0.01f;
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(evt);
            var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.calendar.quietMonthsSinceEvent = 4;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);

            Assert.That(nextEvent?.id, Is.EqualTo(evt.id));
            Assert.That(service.ActiveSave.state.calendar.quietMonthsSinceEvent, Is.EqualTo(0));
        }

        [Test]
        public void AdvanceMonth_IncrementsQuietCounterWhenNoEventTriggers()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(),
                new RecordingSaveRepository());
            service.Start(CreateValidSave());

            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);

            Assert.That(nextEvent, Is.Null);
            Assert.That(service.ActiveSave.state.calendar.quietMonthsSinceEvent, Is.EqualTo(1));
        }

        [Test]
        public void MonthlyEventSelection_ReturnsAnEligibleEventAcrossSeededLives()
        {
            var evt = RepresentativeStimEvents.CreateHealthBurnout();
            evt.monthlyTriggerChance = 0.25f;
            var triggered = 0;

            for (var seed = 0; seed < 1000; seed++)
            {
                var catalog = new InMemoryStimEventCatalog();
                catalog.Upsert(evt);
                var service = new StimGameSessionService(catalog, new RecordingSaveRepository());
                var save = CreateValidSave();
                save.rng.seed = seed;
                service.Start(save);

                Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);
                if (nextEvent != null)
                {
                    triggered++;
                }
            }

            Assert.That(triggered, Is.EqualTo(1000));
        }

        [Test]
        public void PlayFlow_ResolvesEventReloadsSaveAndContinuesNextMonth()
        {
            var evt = RepresentativeStimEvents.CreateHealthBurnout();
            evt.monthlyTriggerChance = 1f;
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(evt);
            var repository = new RecordingSaveRepository();
            var firstSession = new StimGameSessionService(catalog, repository);
            firstSession.Start(CreateValidSave());

            Assert.IsTrue(firstSession.TryAdvanceMonth(out var nextEvent, out var advanceSummary), advanceSummary);
            Assert.That(nextEvent?.id, Is.EqualTo(evt.id));
            Assert.IsTrue(firstSession.TryResolveChoice(evt.id, "take_a_break", out var resolutionSummary), resolutionSummary);
            var revisionAfterResolution = firstSession.ActiveSave.revision;
            var cashAfterResolution = firstSession.ActiveSave.state.finances.cashMinorUnits;

            var resumedSession = new StimGameSessionService(catalog, repository);
            Assert.IsTrue(resumedSession.TryLoadLatest(out var loadSummary), loadSummary);
            Assert.That(resumedSession.ActiveSave.revision, Is.EqualTo(revisionAfterResolution));
            Assert.That(resumedSession.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(cashAfterResolution));
            Assert.That(resumedSession.ActiveSave.state.eventHistory, Has.Count.EqualTo(1));

            Assert.IsTrue(resumedSession.TryAdvanceMonth(out var followingEvent, out var followingSummary), followingSummary);
            Assert.That(followingEvent, Is.Null);
            Assert.That(resumedSession.ActiveSave.revision, Is.EqualTo(revisionAfterResolution + 1));
            Assert.That(resumedSession.ActiveSave.state.calendar.monthOfYear, Is.EqualTo(3));
        }

        [Test]
        public void SavingsTransfers_AreAtomicAndWriteAuditableHistory()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            service.Start(save);
            var lifeFeedCountBefore = service.ActiveSave.state.lifeFeed.Count;

            Assert.IsTrue(service.TryTransferSavings(
                StimSavingsTransferType.Deposit, 25000, out var depositSummary), depositSummary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(75000));
            Assert.That(service.ActiveSave.state.finances.savingsMinorUnits, Is.EqualTo(25000));
            Assert.IsTrue(service.TryTransferSavingsPercentage(
                StimSavingsTransferType.Withdrawal, 50, out var withdrawalSummary), withdrawalSummary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(87500));
            Assert.That(service.ActiveSave.state.finances.savingsMinorUnits, Is.EqualTo(12500));
            Assert.That(service.ActiveSave.state.moneyTransactions, Has.Count.EqualTo(2));
            Assert.That(service.ActiveSave.state.moneyTransactions[0].type, Is.EqualTo("savings_deposit"));
            Assert.That(service.ActiveSave.state.moneyTransactions[1].type, Is.EqualTo("savings_withdrawal"));
            Assert.That(service.ActiveSave.state.lifeFeed, Has.Count.EqualTo(lifeFeedCountBefore + 2));
            Assert.That(repository.CommitCount, Is.EqualTo(2));
        }

        [Test]
        public void SavingsTransfer_PersistenceFailureRollsBackBalancesAndHistory()
        {
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            service.Start(CreateValidSave());

            Assert.IsFalse(service.TryTransferSavings(
                StimSavingsTransferType.Deposit, 10000, out _));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(100000));
            Assert.That(service.ActiveSave.state.finances.savingsMinorUnits, Is.Zero);
            Assert.That(service.ActiveSave.state.moneyTransactions, Is.Empty);
            Assert.That(service.ActiveSave.revision, Is.EqualTo(1));
        }

        [Test]
        public void SavingsHistory_RetainsNewestOneHundredEntries()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 1000000;
            service.Start(save);

            for (var index = 0; index < 101; index++)
                Assert.IsTrue(service.TryTransferSavings(
                    StimSavingsTransferType.Deposit, 100, out var summary), summary);

            Assert.That(service.ActiveSave.state.moneyTransactions, Has.Count.EqualTo(100));
            Assert.That(service.ActiveSave.state.moneyTransactions[0].revision, Is.EqualTo(3));
            Assert.That(StimSaveValidator.ValidateSave(service.ActiveSave).isValid, Is.True);
        }

        [Test]
        public void AdvanceMonth_AccruesGroundedSavingsInterestAndPersistsCashFlowDetail()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.savingsMinorUnits = 120000;
            save.state.finances.savingsApyBasisPoints = 350;
            save.state.finances.taxRateBasisPoints = 2000;
            save.state.finances.monthlyLivingExpensesMinorUnits = 200000;
            service.Start(save);

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var summary), summary);

            var finances = service.ActiveSave.state.finances;
            Assert.That(finances.savingsMinorUnits, Is.EqualTo(120350));
            Assert.That(finances.lastSavingsInterestMinorUnits, Is.EqualTo(350));
            Assert.That(finances.lastGrossIncomeMinorUnits, Is.EqualTo(416667));
            Assert.That(finances.lastTaxesMinorUnits, Is.EqualTo(83333));
            Assert.That(finances.lastExpensesMinorUnits, Is.EqualTo(200000));
            Assert.That(finances.lastNetCashFlowMinorUnits, Is.EqualTo(133684));
            Assert.That(service.ActiveSave.state.moneyTransactions[^1].type, Is.EqualTo("savings_interest"));
            Assert.That(StimGameSessionService.CalculateProjectedAnnualSavingsInterest(finances),
                Is.EqualTo(4212));
        }

        [Test]
        public void CreditRepayment_IsAtomicAndReducesRevolvingAndTotalDebt()
        {
            var repository = new RecordingSaveRepository();
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.finances.debtMinorUnits = 80000;
            save.state.finances.householdCreditBalanceMinorUnits = 60000;
            save.state.finances.householdCreditAprBasisPoints = 2000;
            service.Start(save);

            Assert.IsTrue(service.TryRepayHouseholdCredit(25000, out var summary), summary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(75000));
            Assert.That(service.ActiveSave.state.finances.householdCreditBalanceMinorUnits, Is.EqualTo(35000));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(55000));
            Assert.That(service.ActiveSave.state.moneyTransactions[^1].type, Is.EqualTo("credit_repayment"));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].text, Does.Contain("Repaid $250"));
        }

        [Test]
        public void CreditRepayment_RejectsOverpaymentWithoutMutation()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.debtMinorUnits = 10000;
            save.state.finances.householdCreditBalanceMinorUnits = 10000;
            save.state.finances.householdCreditAprBasisPoints = 1800;
            service.Start(save);

            Assert.IsFalse(service.TryRepayHouseholdCredit(10001, out var summary));
            Assert.That(summary, Does.Contain("exceeds the revolving credit balance"));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(100000));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(10000));
            Assert.That(service.ActiveSave.state.moneyTransactions, Is.Empty);
        }

        [Test]
        public void CreditRepaymentPercentage_UsesAffordableBalanceAndClearsAprWhenPaidOff()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.finances.cashMinorUnits = 40000;
            save.state.finances.debtMinorUnits = 40000;
            save.state.finances.householdCreditBalanceMinorUnits = 40000;
            save.state.finances.householdCreditAprBasisPoints = 1800;
            service.Start(save);

            Assert.IsTrue(service.TryRepayHouseholdCreditPercentage(100, out var summary), summary);

            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.Zero);
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.Zero);
            Assert.That(service.ActiveSave.state.finances.householdCreditBalanceMinorUnits, Is.Zero);
            Assert.That(service.ActiveSave.state.finances.householdCreditAprBasisPoints, Is.Zero);
            Assert.That(service.ActiveSave.state.moneyTransactions[^1].amountMinorUnits,
                Is.EqualTo(40000));
        }

        [Test]
        public void CreditRepayment_PersistenceFailureRollsBackDebtCashAndHistory()
        {
            var repository = new RecordingSaveRepository { ShouldCommit = false };
            var service = new StimGameSessionService(new InMemoryStimEventCatalog(), repository);
            var save = CreateValidSave();
            save.state.finances.debtMinorUnits = 50000;
            save.state.finances.householdCreditBalanceMinorUnits = 50000;
            save.state.finances.householdCreditAprBasisPoints = 1800;
            service.Start(save);

            Assert.IsFalse(service.TryRepayHouseholdCredit(25000, out _));

            Assert.That(service.ActiveSave, Is.SameAs(save));
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(100000));
            Assert.That(service.ActiveSave.state.finances.debtMinorUnits, Is.EqualTo(50000));
            Assert.That(service.ActiveSave.state.finances.householdCreditBalanceMinorUnits,
                Is.EqualTo(50000));
            Assert.That(service.ActiveSave.state.finances.householdCreditAprBasisPoints,
                Is.EqualTo(1800));
            Assert.That(service.ActiveSave.state.moneyTransactions, Is.Empty);
        }

        [Test]
        public void IndexInvestment_EnforcesEducationKnowledgeAndEmergencySavingsGates()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.smarts = 39;
            save.state.education.graduatedSecondary = true;
            save.state.finances.savingsMinorUnits = 50000;
            service.Start(save);

            Assert.IsFalse(service.TryInvestInIndexFund(1000, out var knowledgeSummary));
            Assert.That(knowledgeSummary, Does.Contain("40 Smarts"));
            service.ActiveSave.state.character.smarts = 60;
            service.ActiveSave.state.education.graduatedSecondary = false;
            Assert.IsFalse(service.TryInvestInIndexFund(1000, out var educationSummary));
            Assert.That(educationSummary, Does.Contain("secondary school"));
            service.ActiveSave.state.education.graduatedSecondary = true;
            service.ActiveSave.state.finances.savingsMinorUnits = 49999;
            Assert.IsFalse(service.TryInvestInIndexFund(1000, out var savingsSummary));
            Assert.That(savingsSummary, Does.Contain("emergency savings"));
        }

        [Test]
        public void IndexInvestment_UnlocksAtDocumentedExactBoundaries()
        {
            var state = CreateValidSave().state;
            state.character.age = StimGameSessionService.IndexInvestmentMinimumAge;
            state.character.smarts = StimGameSessionService.IndexInvestmentMinimumSmarts;
            state.education.graduatedSecondary = false;
            state.education.qualificationExperience =
                StimEducationActionService.CertificateQualificationExperience;
            state.finances.monthlyLivingExpensesMinorUnits = 0;
            state.finances.savingsMinorUnits =
                StimGameSessionService.IndexInvestmentMinimumEmergencySavingsMinorUnits;
            state.finances.cashMinorUnits = 1000;

            Assert.IsTrue(StimGameSessionService.TryGetIndexInvestmentRequirement(
                state, out var requirement), requirement);

            state.character.age--;
            Assert.IsFalse(StimGameSessionService.TryGetIndexInvestmentRequirement(state, out _));
            state.character.age++;
            state.character.smarts--;
            Assert.IsFalse(StimGameSessionService.TryGetIndexInvestmentRequirement(state, out _));
            state.character.smarts++;
            state.education.qualificationExperience--;
            Assert.IsFalse(StimGameSessionService.TryGetIndexInvestmentRequirement(state, out _));
            state.education.qualificationExperience++;
            state.finances.savingsMinorUnits--;
            Assert.IsFalse(StimGameSessionService.TryGetIndexInvestmentRequirement(state, out _));
        }

        [Test]
        public void IndexInvestment_IsAtomicAndIncludedInNetWorthLedger()
        {
            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.smarts = 60;
            save.state.education.graduatedSecondary = true;
            save.state.finances.savingsMinorUnits = 50000;
            service.Start(save);

            Assert.IsTrue(service.TryInvestInIndexFund(25000, out var summary), summary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(75000));
            Assert.That(service.ActiveSave.state.finances.indexFundMinorUnits, Is.EqualTo(25000));
            Assert.That(service.ActiveSave.state.finances.indexFundContributionsMinorUnits, Is.EqualTo(25000));
            Assert.That(service.ActiveSave.state.moneyTransactions[^1].type, Is.EqualTo("index_investment"));
            Assert.That(service.ActiveSave.state.lifeFeed[^1].text, Does.Contain("not guaranteed"));
        }

        [Test]
        public void AnnualIndexReturn_IsDeterministicBoundedAndPersisted()
        {
            var first = StimGameSessionService.CalculateAnnualIndexReturnBasisPoints(42, 30);
            var second = StimGameSessionService.CalculateAnnualIndexReturnBasisPoints(42, 30);
            Assert.That(second, Is.EqualTo(first));
            Assert.That(first, Is.InRange(-1200, 1800));

            var service = new StimGameSessionService(
                new InMemoryStimEventCatalog(), new RecordingSaveRepository());
            var save = CreateValidSave();
            save.state.character.age = 29;
            save.state.calendar.monthOfYear = 12;
            save.state.finances.indexFundMinorUnits = 100000;
            save.state.finances.indexFundContributionsMinorUnits = 100000;
            service.Start(save);
            Assert.IsTrue(service.TryAdvanceMonth(out _, out var summary), summary);
            var expectedChange = (long)Math.Round(100000 * (first / 10000m), MidpointRounding.AwayFromZero);
            Assert.That(service.ActiveSave.state.finances.indexFundMinorUnits,
                Is.EqualTo(100000 + expectedChange));
            Assert.That(service.ActiveSave.state.finances.indexFundContributionsMinorUnits,
                Is.EqualTo(100000));
            Assert.That(service.ActiveSave.state.moneyTransactions[^1].type,
                Is.EqualTo(expectedChange >= 0 ? "index_gain" : "index_loss"));
        }

        [TestCase(101, 2400000L, 180000L, 10000L, 0L)]
        [TestCase(202, 5000000L, 280000L, 100000L, 50000L)]
        [TestCase(303, 12000000L, 550000L, 500000L, 250000L)]
        public void SeededEconomySimulation_StaysWithinTwentyYearBalanceBudgets(
            int seed,
            long annualSalary,
            long monthlyExpenses,
            long startingSavings,
            long startingIndex)
        {
            decimal cash = 100000;
            decimal savings = startingSavings;
            decimal index = startingIndex;
            decimal debt = 0;
            for (var month = 0; month < 240; month++)
            {
                var income = annualSalary / 12m * 0.8m;
                var surplus = income - monthlyExpenses;
                cash += surplus;
                if (cash < 0)
                {
                    debt += -cash;
                    cash = 0;
                }
                debt *= 1m + 0.08m / 12m;
                savings *= 1m + 0.035m / 12m;
                if (surplus > 0 && cash > surplus)
                {
                    var savingsContribution = surplus * 0.10m;
                    var indexContribution = surplus * 0.05m;
                    cash -= savingsContribution + indexContribution;
                    savings += savingsContribution;
                    index += indexContribution;
                }
                if ((month + 1) % 12 == 0 && index > 0)
                {
                    var age = 18 + (month + 1) / 12;
                    index *= 1m + StimGameSessionService.CalculateAnnualIndexReturnBasisPoints(seed, age) / 10000m;
                }
            }
            var netWorth = cash + savings + index - debt;
            Assert.That(netWorth, Is.GreaterThan(-annualSalary * 6m));
            Assert.That(netWorth, Is.LessThan(annualSalary * 25m));
            Assert.That(savings, Is.GreaterThanOrEqualTo(0));
            Assert.That(index, Is.GreaterThanOrEqualTo(0));
        }

        private static StimSaveEnvelope CreateValidSave()
        {
            return new StimSaveEnvelope
            {
                gameBuildVersion = "0.1.0",
                contentVersion = "1",
                saveId = "save_001",
                playerAccountId = "local-player",
                lifeId = "life_001",
                createdAtUtc = "2026-07-13T17:00:00Z",
                updatedAtUtc = "2026-07-13T17:00:00Z",
                revision = 1,
                deviceIdHash = "device-hash",
                rng = new StimRngState { seed = 42, step = 0 },
                integrity = new StimSaveIntegrity { payloadHash = "pending" },
                state = new StimGameState
                {
                    character = new StimCharacterState
                    {
                        age = 24,
                        health = 80,
                        happiness = 70,
                        smarts = 60
                    },
                    finances = new StimFinancesState { cashMinorUnits = 100000 },
                    calendar = new StimCalendarState { monthOfYear = 1 },
                    career = new StimCareerState
                    {
                        employerId = "test_employer",
                        roleTitle = "Analyst",
                        annualSalaryMinorUnits = 5000000
                    },
                    eventHistory = new List<StimEventHistoryEntry>(),
                    scheduledEvents = new List<StimScheduledEventRecord>()
                }
            };
        }

        private static void AdvanceAndAssert(StimGameSessionService service)
        {
            Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var summary), summary);
            Assert.That(nextEvent, Is.Null);
        }

        private static long SimulateBusiness(int seed, int months)
        {
            var business = new StimBusinessState
            {
                status = "operating",
                level = 2,
                locationLevel = 1,
                staffCount = 1,
                operatingProgress = 50,
                actionPoints = 4,
                maxActionPoints = 4
            };
            long total = 0;
            for (var month = 0; month < months; month++)
            {
                var profit = StimGameSessionService.CalculateBusinessMonthlyProfit(
                    seed, month, business, out _, out _, out _);
                total += profit;
                business.lifetimeProfitMinorUnits += profit;
                business.consecutiveLossMonths = profit < 0
                    ? business.consecutiveLossMonths + 1
                    : 0;
                if (business.consecutiveLossMonths >= 3) break;
            }
            return total;
        }

        private static void AddTestChild(
            StimSaveEnvelope save,
            string childId,
            int age,
            string partnerRelationshipId)
        {
            save.state.family.children.Add(new StimChildState
            {
                childId = childId,
                displayName = "Ari",
                path = "adoption",
                parentRelationshipId = partnerRelationshipId,
                joinedAtParentAge = 24,
                birthMonth = 1,
                age = age,
                wellbeing = 60,
                custodyStatus = age >= 18 ? "independent" : "household"
            });
            save.state.relationships.Add(new StimRelationshipState
            {
                relationshipId = childId,
                identityId = $"identity_{childId}",
                displayName = "Ari",
                relationshipType = age >= 18 ? "adult_child" : "child",
                relationshipStage = age >= 18 ? "adult_child" : "dependent_child",
                origin = "adoption",
                introducedAtAge = 24,
                value = 70,
                warmth = 70
            });
        }

        private static InMemoryStimEventCatalog CreateRepresentativeCatalog()
        {
            var catalog = new InMemoryStimEventCatalog();
            catalog.Upsert(RepresentativeStimEvents.CreateSalaryNegotiation());
            catalog.Upsert(RepresentativeStimEvents.CreateHealthBurnout());
            catalog.Upsert(RepresentativeStimEvents.CreateMoneyFastReturn());
            catalog.Upsert(RepresentativeStimEvents.CreateSchoolGroupProject());
            catalog.Upsert(RepresentativeStimEvents.CreateChildhoodGrownFolksTable());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomGain());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomLoss());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomGainRefund());
            catalog.Upsert(RepresentativeStimEvents.CreateRandomLossRepair());
            catalog.Upsert(RepresentativeStimEvents.CreateLuckCrossroads());
            catalog.Upsert(RepresentativeStimEvents.CreateChildhoodDiscovery());
            catalog.Upsert(RepresentativeStimEvents.CreateChildhoodComfort());
            catalog.Upsert(RepresentativeStimEvents.CreatePeerTrustConflict());
            catalog.Upsert(RepresentativeStimEvents.CreatePeerTrustAftermath());
            catalog.Upsert(RepresentativeStimEvents.CreateComingOfAgeGender());
            catalog.Upsert(RepresentativeStimEvents.CreateComingOfAgeOrientation());
            catalog.Upsert(RepresentativeStimEvents.CreatePromInvitation());
            catalog.Upsert(RepresentativeStimEvents.CreateFirstKiss());
            catalog.Upsert(RepresentativeStimEvents.CreateProposal());
            catalog.Upsert(RepresentativeStimEvents.CreateWedding());
            catalog.Upsert(RepresentativeStimEvents.CreateMarriageCrossroads());
            catalog.Upsert(RepresentativeStimEvents.CreateHomeDeferredMaintenance());
            return catalog;
        }

        private sealed class RecordingSaveRepository : IStimSaveRepository
        {
            public bool ShouldCommit { get; set; } = true;
            public int CommitCount { get; private set; }
            public string LastCommittedSave { get; private set; }
            public int MaxSerializedLength { get; private set; }

            public bool TryCommitAutosave(string serializedSave, out string persistenceSummary)
            {
                CommitCount++;
                LastCommittedSave = serializedSave;
                MaxSerializedLength = System.Math.Max(
                    MaxSerializedLength,
                    serializedSave?.Length ?? 0);
                persistenceSummary = ShouldCommit ? "saved" : "failed";
                return ShouldCommit;
            }

            public bool TryLoadLatestSave(out string serializedSave)
            {
                serializedSave = LastCommittedSave;
                return !string.IsNullOrEmpty(serializedSave);
            }

            public bool TryValidateSave(string serializedSave, out string validationSummary)
            {
                var save = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);
                var result = StimSaveValidator.ValidateSave(save);
                validationSummary = StimSaveValidator.GetValidationSummary(result, save?.saveId);
                return result.isValid;
            }
        }
    }
}
