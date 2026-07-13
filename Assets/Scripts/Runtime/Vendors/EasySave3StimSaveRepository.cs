#if STIM_EASY_SAVE_3
using System;
using StimTycoon.Abstractions;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// Easy Save 3 implementation of Stim's local-save boundary.
    /// Enable only after importing Easy Save 3 by adding STIM_EASY_SAVE_3 to
    /// Project Settings > Player > Scripting Define Symbols.
    /// </summary>
    public sealed class EasySave3StimSaveRepository : IStimSaveRepository
    {
        public const string DefaultAutosaveKey = "stim.autosave.latest.v1";

        private readonly string autosaveKey;

        public EasySave3StimSaveRepository(string autosaveKey = DefaultAutosaveKey)
        {
            if (string.IsNullOrWhiteSpace(autosaveKey))
            {
                throw new ArgumentException("An autosave key is required.", nameof(autosaveKey));
            }

            this.autosaveKey = autosaveKey;
        }

        public bool TryCommitAutosave(string serializedSave, out string persistenceSummary)
        {
            if (!TryValidateSave(serializedSave, out persistenceSummary))
            {
                return false;
            }

            try
            {
                // Store the locked JSON envelope as one value. Cloud Save can use the
                // same payload later without depending on Easy Save's object format.
                ES3.Save(autosaveKey, serializedSave);
                persistenceSummary = $"Saved autosave to Easy Save key {autosaveKey}.";
                return true;
            }
            catch (Exception exception)
            {
                persistenceSummary = $"Easy Save failed to write the autosave: {exception.Message}";
                return false;
            }
        }

        public bool TryLoadLatestSave(out string serializedSave)
        {
            serializedSave = null;

            try
            {
                if (!ES3.KeyExists(autosaveKey))
                {
                    return false;
                }

                serializedSave = ES3.Load<string>(autosaveKey);
                return !string.IsNullOrWhiteSpace(serializedSave);
            }
            catch (Exception)
            {
                serializedSave = null;
                return false;
            }
        }

        public bool TryValidateSave(string serializedSave, out string validationSummary)
        {
            if (string.IsNullOrWhiteSpace(serializedSave))
            {
                validationSummary = "Serialized save is required.";
                return false;
            }

            try
            {
                var save = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);
                var result = StimSaveValidator.ValidateSave(save);
                validationSummary = StimSaveValidator.GetValidationSummary(
                    result,
                    string.IsNullOrWhiteSpace(save?.saveId) ? "unknown-save" : save.saveId);
                return result.isValid;
            }
            catch (Exception exception)
            {
                validationSummary = $"Serialized save is not valid JSON: {exception.Message}";
                return false;
            }
        }
    }
}
#endif
