using System;
using NUnit.Framework;
using StimTycoon.Abstractions;
using StimTycoon.Runtime;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class StimSaveTransactionRunnerTests
    {
        [Test]
        public void Execute_CommitsStampedCloneWithoutMutatingActiveSave()
        {
            var repository = new RecordingRepository();
            var runner = new StimSaveTransactionRunner(
                repository,
                () => DateTimeOffset.Parse("2026-07-14T18:30:00Z"));
            var activeSave = CreateSave();

            var succeeded = runner.TryExecute(
                activeSave,
                candidate =>
                {
                    candidate.state.character.smarts += 2;
                    return StimTransactionMutationResult.Success("Studied");
                },
                out var committedSave,
                out var summary);

            Assert.IsTrue(succeeded, summary);
            Assert.That(summary, Is.EqualTo("Studied"));
            Assert.That(activeSave.revision, Is.EqualTo(1));
            Assert.That(activeSave.state.character.smarts, Is.EqualTo(60));
            Assert.That(committedSave, Is.Not.SameAs(activeSave));
            Assert.That(committedSave.revision, Is.EqualTo(2));
            Assert.That(committedSave.updatedAtUtc, Is.EqualTo("2026-07-14T18:30:00.0000000+00:00"));
            Assert.That(committedSave.state.character.smarts, Is.EqualTo(62));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void Execute_WhenPersistenceFails_DoesNotExposeCandidate()
        {
            var repository = new RecordingRepository { ShouldCommit = false };
            var runner = new StimSaveTransactionRunner(repository);
            var activeSave = CreateSave();

            var succeeded = runner.TryExecute(
                activeSave,
                candidate =>
                {
                    candidate.state.character.smarts = 99;
                    return StimTransactionMutationResult.Success("Studied");
                },
                out var committedSave,
                out var summary);

            Assert.IsFalse(succeeded);
            Assert.That(summary, Is.EqualTo("persistence failed"));
            Assert.That(committedSave, Is.Null);
            Assert.That(activeSave.revision, Is.EqualTo(1));
            Assert.That(activeSave.state.character.smarts, Is.EqualTo(60));
            Assert.That(repository.CommitCount, Is.EqualTo(1));
        }

        [Test]
        public void Execute_WhenMutationRejects_DoesNotAttemptPersistence()
        {
            var repository = new RecordingRepository();
            var runner = new StimSaveTransactionRunner(repository);

            var succeeded = runner.TryExecute(
                CreateSave(),
                _ => StimTransactionMutationResult.Failure("Locked"),
                out var committedSave,
                out var summary);

            Assert.IsFalse(succeeded);
            Assert.That(summary, Is.EqualTo("Locked"));
            Assert.That(committedSave, Is.Null);
            Assert.That(repository.CommitCount, Is.Zero);
        }

        private static StimSaveEnvelope CreateSave()
        {
            return new StimSaveEnvelope
            {
                gameBuildVersion = "0.1.0",
                contentVersion = "1",
                saveId = "transaction-test",
                playerAccountId = "local-player",
                lifeId = "life-1",
                createdAtUtc = "2026-07-14T18:00:00Z",
                updatedAtUtc = "2026-07-14T18:00:00Z",
                revision = 1,
                deviceIdHash = "device-hash",
                rng = new StimRngState { seed = 42 },
                integrity = new StimSaveIntegrity { payloadHash = "pending" },
                state = new StimGameState
                {
                    character = new StimCharacterState
                    {
                        lifeStatus = "active",
                        age = 10,
                        health = 70,
                        happiness = 70,
                        smarts = 60
                    }
                }
            };
        }

        private sealed class RecordingRepository : IStimSaveRepository
        {
            public bool ShouldCommit { get; set; } = true;
            public int CommitCount { get; private set; }

            public bool TryCommitAutosave(string serializedSave, out string persistenceSummary)
            {
                CommitCount++;
                persistenceSummary = ShouldCommit ? "saved" : "persistence failed";
                return ShouldCommit;
            }

            public bool TryLoadLatestSave(out string serializedSave)
            {
                serializedSave = null;
                return false;
            }

            public bool TryValidateSave(string serializedSave, out string validationSummary)
            {
                validationSummary = string.Empty;
                return JsonUtility.FromJson<StimSaveEnvelope>(serializedSave) != null;
            }
        }
    }
}
