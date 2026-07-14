using System;
using System.Collections.Generic;
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

            Assert.IsTrue(service.TryAdvanceMonth(out _, out var paycheckSummary), paycheckSummary);
            Assert.That(service.ActiveSave.state.finances.cashMinorUnits, Is.EqualTo(558334));
            Assert.That(service.ActiveSave.state.career.careerProgress, Is.EqualTo(1));
            Assert.That(service.ActiveSave.state.character.happiness, Is.EqualTo(76));
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

            var months = 0;
            const int maximumMonths = 1800;
            while (service.ActiveSave.state.character.lifeStatus == "active" && months < maximumMonths)
            {
                Assert.IsTrue(service.TryAdvanceMonth(out var nextEvent, out var advanceSummary), advanceSummary);
                months++;
                if (nextEvent == null) continue;
                Assert.That(nextEvent.choices, Is.Not.Empty, $"Event {nextEvent.id} has no playable choice.");
                Assert.IsTrue(service.TryResolveChoice(
                    nextEvent.id,
                    nextEvent.choices[0].id,
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
            return catalog;
        }

        private sealed class RecordingSaveRepository : IStimSaveRepository
        {
            public bool ShouldCommit { get; set; } = true;
            public int CommitCount { get; private set; }
            public string LastCommittedSave { get; private set; }

            public bool TryCommitAutosave(string serializedSave, out string persistenceSummary)
            {
                CommitCount++;
                LastCommittedSave = serializedSave;
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
