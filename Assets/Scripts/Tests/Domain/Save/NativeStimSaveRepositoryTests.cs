using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using StimTycoon.Runtime;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Tests.Domain.Save
{
    public sealed class NativeStimSaveRepositoryTests
    {
        private string persistenceRoot;
        private NativeStimSaveRepository repository;

        [SetUp]
        public void SetUp()
        {
            persistenceRoot = Path.Combine(
                Application.temporaryCachePath,
                "stim-save-tests",
                Guid.NewGuid().ToString("N"));
            repository = new NativeStimSaveRepository(persistenceRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(persistenceRoot))
            {
                Directory.Delete(persistenceRoot, true);
            }
        }

        [Test]
        public void CommitAndLoad_RoundTripsValidatedEnvelope()
        {
            var save = CreateValidSave(1, 250000);

            var committed = repository.TryCommitAutosave(JsonUtility.ToJson(save), out var summary);
            var loaded = repository.TryLoadLatestSave(out var serializedSave);
            var loadedSave = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);

            Assert.IsTrue(committed, summary);
            Assert.IsTrue(loaded);
            Assert.That(loadedSave.revision, Is.EqualTo(1));
            Assert.That(loadedSave.state.finances.cashMinorUnits, Is.EqualTo(250000));
            Assert.That(loadedSave.integrity.payloadHash, Does.StartWith("sha256:"));
        }

        [Test]
        public void Commit_RejectsInvalidEnvelopeWithoutReplacingCurrentSave()
        {
            Assert.IsTrue(repository.TryCommitAutosave(JsonUtility.ToJson(CreateValidSave(1, 100)), out _));
            var invalidSave = CreateValidSave(2, 200);
            invalidSave.state.character.health = 101;

            var committed = repository.TryCommitAutosave(JsonUtility.ToJson(invalidSave), out var summary);
            Assert.IsTrue(repository.TryLoadLatestSave(out var serializedSave));
            var loadedSave = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);

            Assert.IsFalse(committed);
            Assert.That(summary, Does.Contain("health"));
            Assert.That(loadedSave.revision, Is.EqualTo(1));
        }

        [Test]
        public void Load_RecoversPreviousRevisionWhenPrimaryIsCorrupt()
        {
            Assert.IsTrue(repository.TryCommitAutosave(JsonUtility.ToJson(CreateValidSave(1, 100)), out _));
            Assert.IsTrue(repository.TryCommitAutosave(JsonUtility.ToJson(CreateValidSave(2, 200)), out _));
            var autosavePath = Path.Combine(
                persistenceRoot,
                NativeStimSaveRepository.SaveDirectoryName,
                NativeStimSaveRepository.AutosaveFileName);
            File.WriteAllText(autosavePath, "{ corrupt");

            var loaded = repository.TryLoadLatestSave(out var serializedSave);
            var recoveredSave = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);

            Assert.IsTrue(loaded);
            Assert.That(recoveredSave.revision, Is.EqualTo(1));
            Assert.That(recoveredSave.state.finances.cashMinorUnits, Is.EqualTo(100));
        }

        [Test]
        public void Validate_RejectsTamperedHashedEnvelope()
        {
            Assert.IsTrue(repository.TryCommitAutosave(JsonUtility.ToJson(CreateValidSave(1, 100)), out _));
            Assert.IsTrue(repository.TryLoadLatestSave(out var serializedSave));
            var tamperedSave = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);
            tamperedSave.state.finances.cashMinorUnits = 999999;

            var isValid = repository.TryValidateSave(JsonUtility.ToJson(tamperedSave), out var summary);

            Assert.IsFalse(isValid);
            Assert.That(summary, Does.Contain("integrity"));
        }

        private static StimSaveEnvelope CreateValidSave(int revision, long cashMinorUnits)
        {
            return new StimSaveEnvelope
            {
                gameBuildVersion = "0.1.0",
                contentVersion = "1",
                saveId = "save_001",
                playerAccountId = "local-player",
                lifeId = "life_001",
                createdAtUtc = "2026-07-13T12:00:00Z",
                updatedAtUtc = "2026-07-13T12:00:00Z",
                revision = revision,
                deviceIdHash = "device-hash",
                rng = new StimRngState { seed = 42, step = 0 },
                integrity = new StimSaveIntegrity { payloadHash = "pending" },
                state = new StimGameState
                {
                    character = new StimCharacterState
                    {
                        age = 18,
                        health = 80,
                        happiness = 70,
                        smarts = 60
                    },
                    finances = new StimFinancesState
                    {
                        cashMinorUnits = cashMinorUnits,
                        debtMinorUnits = 0
                    },
                    eventHistory = new List<StimEventHistoryEntry>(),
                    scheduledEvents = new List<StimScheduledEventRecord>()
                }
            };
        }
    }
}
