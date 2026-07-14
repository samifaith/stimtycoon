using System;
using StimTycoon.Abstractions;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Runtime
{
    public readonly struct StimTransactionMutationResult
    {
        public bool Succeeded { get; }
        public string Summary { get; }

        private StimTransactionMutationResult(bool succeeded, string summary)
        {
            Succeeded = succeeded;
            Summary = summary ?? string.Empty;
        }

        public static StimTransactionMutationResult Success(string summary) =>
            new StimTransactionMutationResult(true, summary);

        public static StimTransactionMutationResult Failure(string summary) =>
            new StimTransactionMutationResult(false, summary);
    }

    /// <summary>
    /// Runs save mutations against a clone and exposes the candidate only after persistence succeeds.
    /// </summary>
    public sealed class StimSaveTransactionRunner
    {
        private readonly IStimSaveRepository saveRepository;
        private readonly Func<DateTimeOffset> utcNow;

        public StimSaveTransactionRunner(
            IStimSaveRepository saveRepository,
            Func<DateTimeOffset> utcNow = null)
        {
            this.saveRepository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        public bool TryExecute(
            StimSaveEnvelope activeSave,
            Func<StimSaveEnvelope, StimTransactionMutationResult> mutation,
            out StimSaveEnvelope committedSave,
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

        private static StimSaveEnvelope Clone(StimSaveEnvelope save)
        {
            return JsonUtility.FromJson<StimSaveEnvelope>(JsonUtility.ToJson(save));
        }
    }
}
