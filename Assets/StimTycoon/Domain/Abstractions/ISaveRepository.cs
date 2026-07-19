namespace StimTycoon.Abstractions
{
    /// <summary>
    /// Wraps local save persistence behind a replaceable repository boundary.
    /// </summary>
    public interface ISaveRepository
    {
        bool TryCommitAutosave(string serializedSave, out string persistenceSummary);
        bool TryLoadLatestSave(out string serializedSave);
        bool TryValidateSave(string serializedSave, out string validationSummary);
    }
}
