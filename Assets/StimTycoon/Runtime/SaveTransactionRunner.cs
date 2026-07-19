using System;
using StimTycoon.Abstractions;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Runtime
{
    public readonly struct TransactionMutationResult
    {
        public bool Succeeded { get; }
        public string Summary { get; }

        private TransactionMutationResult(bool succeeded, string summary)
        {
            Succeeded = succeeded;
            Summary = summary ?? string.Empty;
        }

        public static TransactionMutationResult Success(string summary) =>
            new TransactionMutationResult(true, summary);

        public static TransactionMutationResult Failure(string summary) =>
            new TransactionMutationResult(false, summary);
    }

    /// <summary>
    /// Runs save mutations against a clone and exposes the candidate only after persistence succeeds.
    /// </summary>
    public sealed class SaveTransactionRunner
    {
        private readonly ISaveRepository saveRepository;
        private readonly Func<DateTimeOffset> utcNow;

        public SaveTransactionRunner(
            ISaveRepository saveRepository,
            Func<DateTimeOffset> utcNow = null)
        {
            this.saveRepository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        public bool TryExecute(
            SaveEnvelope activeSave,
            Func<SaveEnvelope, TransactionMutationResult> mutation,
            out SaveEnvelope committedSave,
            out string summary)
        {
            committedSave = null;
            if (activeSave == null)
            {
                summary = "No active save is loaded.";
                return false;
            }
            if (mutation == null)
            {
                throw new ArgumentNullException(nameof(mutation));
            }

            var candidateSave = Clone(activeSave);
            candidateSave.revision++;
            candidateSave.updatedAtUtc = utcNow().ToUniversalTime().ToString("O");

            var result = mutation(candidateSave);
            summary = result.Summary;
            if (!result.Succeeded)
            {
                return false;
            }

            var serializedSave = JsonUtility.ToJson(candidateSave);
            if (!saveRepository.TryCommitAutosave(serializedSave, out var persistenceSummary))
            {
                summary = persistenceSummary;
                return false;
            }

            committedSave = candidateSave;
            return true;
        }

        private static SaveEnvelope Clone(SaveEnvelope save)
        {
            return JsonUtility.FromJson<SaveEnvelope>(JsonUtility.ToJson(save));
        }
    }
}
