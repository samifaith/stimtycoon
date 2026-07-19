namespace StimTycoon.Abstractions
{
    /// <summary>
    /// Wraps Unity Cloud Save and sync conflict handling behind a narrow interface.
    /// </summary>
    public interface ICloudSaveService
    {
        void QueueSync();
        bool TryPushPendingSync();
        bool TryResolveConflict(out string resolutionSummary);
    }
}