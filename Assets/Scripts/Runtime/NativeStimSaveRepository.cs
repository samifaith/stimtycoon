using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using StimTycoon.Abstractions;
using StimTycoon.Saves;
using UnityEngine;

namespace StimTycoon.Runtime
{
    /// <summary>
    /// Stores the portable Stim JSON envelope using atomic same-volume file replacement.
    /// </summary>
    public sealed class NativeStimSaveRepository : IStimSaveRepository
    {
        public const string SaveDirectoryName = "stim";
        public const string AutosaveFileName = "autosave.json";
        public const string BackupFileName = "autosave.backup.json";
        public const string TemporaryFileName = "autosave.tmp";

        private const string Sha256Prefix = "sha256:";

        private readonly string saveDirectory;
        private readonly string autosavePath;
        private readonly string backupPath;
        private readonly string temporaryPath;

        public NativeStimSaveRepository(string persistenceRoot = null)
        {
            var root = string.IsNullOrWhiteSpace(persistenceRoot)
                ? Application.persistentDataPath
                : persistenceRoot;

            saveDirectory = Path.Combine(root, SaveDirectoryName);
            autosavePath = Path.Combine(saveDirectory, AutosaveFileName);
            backupPath = Path.Combine(saveDirectory, BackupFileName);
            temporaryPath = Path.Combine(saveDirectory, TemporaryFileName);
        }

        public bool TryCommitAutosave(string serializedSave, out string persistenceSummary)
        {
            if (!TryPrepareForPersistence(serializedSave, out var normalizedSave, out persistenceSummary))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(saveDirectory);
                WriteDurably(temporaryPath, normalizedSave);

                if (!TryReadValidated(temporaryPath, out _, out var verificationSummary))
                {
                    TryDeleteTemporaryFile();
                    persistenceSummary = $"Temporary save verification failed: {verificationSummary}";
                    return false;
                }

                if (File.Exists(autosavePath))
                {
                    File.Replace(temporaryPath, autosavePath, backupPath);
                }
                else
                {
                    File.Move(temporaryPath, autosavePath);
                }

                persistenceSummary = $"Saved autosave to {autosavePath}.";
                return true;
            }
            catch (Exception exception)
            {
                TryDeleteTemporaryFile();
                persistenceSummary = $"Native save failed: {exception.Message}";
                return false;
            }
        }

        public bool TryLoadLatestSave(out string serializedSave)
        {
            if (TryReadValidated(autosavePath, out serializedSave, out _))
            {
                return true;
            }

            if (!TryReadValidated(backupPath, out serializedSave, out _))
            {
                serializedSave = null;
                return false;
            }

            // Recovery is best-effort. Returning the valid backup is more important
            // than repairing the primary file if storage is temporarily unavailable.
            try
            {
                Directory.CreateDirectory(saveDirectory);
                WriteDurably(temporaryPath, serializedSave);

                if (File.Exists(autosavePath))
                {
                    File.Delete(autosavePath);
                }

                File.Move(temporaryPath, autosavePath);
            }
            catch (Exception)
            {
                TryDeleteTemporaryFile();
            }

            return true;
        }

        public bool TryValidateSave(string serializedSave, out string validationSummary)
        {
            if (!TryDeserialize(serializedSave, out var save, out validationSummary))
            {
                return false;
            }

            var result = StimSaveValidator.ValidateSave(save);
            validationSummary = StimSaveValidator.GetValidationSummary(
                result,
                string.IsNullOrWhiteSpace(save.saveId) ? "unknown-save" : save.saveId);

            if (!result.isValid)
            {
                return false;
            }

            if (!HasVerifiableHash(save))
            {
                return true;
            }

            var expectedHash = save.integrity.payloadHash;
            var actualHash = CalculatePayloadHash(save);
            if (string.Equals(expectedHash, actualHash, StringComparison.Ordinal))
            {
                return true;
            }

            validationSummary = $"Save {save.saveId} failed its SHA-256 integrity check.";
            return false;
        }

        private bool TryPrepareForPersistence(
            string serializedSave,
            out string normalizedSave,
            out string validationSummary)
        {
            normalizedSave = null;

            if (!TryDeserialize(serializedSave, out var save, out validationSummary))
            {
                return false;
            }

            var result = StimSaveValidator.ValidateSave(save);
            validationSummary = StimSaveValidator.GetValidationSummary(
                result,
                string.IsNullOrWhiteSpace(save.saveId) ? "unknown-save" : save.saveId);
            if (!result.isValid)
            {
                return false;
            }

            save.integrity.payloadHash = CalculatePayloadHash(save);
            normalizedSave = JsonUtility.ToJson(save, true);
            return true;
        }

        private bool TryReadValidated(string path, out string serializedSave, out string validationSummary)
        {
            serializedSave = null;
            validationSummary = $"No save exists at {path}.";

            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                serializedSave = File.ReadAllText(path, Encoding.UTF8);
                if (TryValidateSave(serializedSave, out validationSummary))
                {
                    return true;
                }

                serializedSave = null;
                return false;
            }
            catch (Exception exception)
            {
                serializedSave = null;
                validationSummary = $"Could not read {path}: {exception.Message}";
                return false;
            }
        }

        private static bool TryDeserialize(
            string serializedSave,
            out StimSaveEnvelope save,
            out string validationSummary)
        {
            save = null;

            if (string.IsNullOrWhiteSpace(serializedSave))
            {
                validationSummary = "Serialized save is required.";
                return false;
            }

            try
            {
                save = JsonUtility.FromJson<StimSaveEnvelope>(serializedSave);
                if (save != null)
                {
                    validationSummary = string.Empty;
                    return true;
                }

                validationSummary = "Serialized save produced a null envelope.";
                return false;
            }
            catch (Exception exception)
            {
                validationSummary = $"Serialized save is not valid JSON: {exception.Message}";
                return false;
            }
        }

        private static bool HasVerifiableHash(StimSaveEnvelope save)
        {
            return save?.integrity?.payloadHash != null &&
                   save.integrity.payloadHash.StartsWith(Sha256Prefix, StringComparison.Ordinal);
        }

        private static string CalculatePayloadHash(StimSaveEnvelope save)
        {
            var originalHash = save.integrity.payloadHash;
            save.integrity.payloadHash = string.Empty;
            var canonicalPayload = JsonUtility.ToJson(save, false);
            save.integrity.payloadHash = originalHash;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(canonicalPayload);
                var hash = sha256.ComputeHash(bytes);
                var builder = new StringBuilder(Sha256Prefix, Sha256Prefix.Length + hash.Length * 2);
                foreach (var value in hash)
                {
                    builder.Append(value.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static void WriteDurably(string path, string contents)
        {
            var bytes = new UTF8Encoding(false).GetBytes(contents);
            using (var stream = new FileStream(
                       path,
                       FileMode.Create,
                       FileAccess.Write,
                       FileShare.None,
                       4096,
                       FileOptions.WriteThrough))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        private void TryDeleteTemporaryFile()
        {
            try
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
