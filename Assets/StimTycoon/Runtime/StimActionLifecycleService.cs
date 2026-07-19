using System;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    /// <summary>Owns persisted timed-action state transitions without granting rewards.</summary>
    public sealed class StimActionLifecycleService
    {
        public StimTransactionMutationResult Start(
            StimSaveEnvelope save,
            StimActionDefinition definition,
            StimActionRequest request,
            DateTimeOffset now)
        {
            if (save?.state == null || definition == null ||
                string.IsNullOrWhiteSpace(request.InstanceId) || request.ActionId != definition.id)
                return StimTransactionMutationResult.Failure("A valid action definition and instance are required.");
            save.state.actionProgress ??= new System.Collections.Generic.List<StimActionProgressState>();
            if (save.state.actionProgress.Exists(action => action?.instanceId == request.InstanceId))
                return StimTransactionMutationResult.Failure("This action instance already exists.");
            if (definition.state != StimActionState.Ready)
                return StimTransactionMutationResult.Failure(
                    string.IsNullOrEmpty(definition.lockedReason) ? "This action is not ready." : definition.lockedReason);

            var utc = now.ToUniversalTime();
            var timed = definition.durationSeconds > 0;
            save.state.actionProgress.Add(new StimActionProgressState
            {
                instanceId = request.InstanceId,
                actionId = definition.id,
                state = timed ? StimActionState.InProgress.ToString() : StimActionState.Claimable.ToString(),
                progress = timed ? 0 : 1,
                progressRequired = Math.Max(1, definition.progressRequired),
                durationSeconds = Math.Max(0, definition.durationSeconds),
                revision = save.revision,
                startedAtUtc = utc.ToString("O"),
                completesAtUtc = timed ? utc.AddSeconds(definition.durationSeconds).ToString("O") : utc.ToString("O")
            });
            return StimTransactionMutationResult.Success(timed ? "Action started." : "Action ready to claim.");
        }

        public int Reconcile(StimSaveEnvelope save, DateTimeOffset now)
        {
            if (save?.state?.actionProgress == null) return 0;
            var changed = 0;
            foreach (var action in save.state.actionProgress)
            {
                if (action == null || action.state != StimActionState.InProgress.ToString() ||
                    !DateTimeOffset.TryParse(action.completesAtUtc, out var completesAt) || now < completesAt)
                    continue;
                action.state = StimActionState.Claimable.ToString();
                action.progress = action.progressRequired;
                action.completedAtUtc = completesAt.ToUniversalTime().ToString("O");
                action.revision = save.revision;
                changed++;
            }
            return changed;
        }

        public StimTransactionMutationResult Claim(StimSaveEnvelope save, string instanceId, DateTimeOffset now)
        {
            var action = save?.state?.actionProgress?.Find(candidate => candidate?.instanceId == instanceId);
            if (action == null) return StimTransactionMutationResult.Failure("Action instance was not found.");
            if (action.state != StimActionState.Claimable.ToString())
                return StimTransactionMutationResult.Failure(
                    action.state == StimActionState.Complete.ToString()
                        ? "This action was already claimed."
                        : "This action is not ready to claim.");
            action.state = StimActionState.Complete.ToString();
            action.progress = action.progressRequired;
            action.claimedAtUtc = now.ToUniversalTime().ToString("O");
            action.revision = save.revision;
            return StimTransactionMutationResult.Success("Action claimed.");
        }
    }
}
