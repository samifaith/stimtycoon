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
        public void SkillLevels_FollowCumulativeXpThresholds(int experience, int expectedLevel)
        {
            Assert.That(StimGameSessionService.GetSkillLevel(experience), Is.EqualTo(expectedLevel));
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
