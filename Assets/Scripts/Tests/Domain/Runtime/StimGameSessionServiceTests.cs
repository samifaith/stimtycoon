using System;
using System.Collections.Generic;
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
