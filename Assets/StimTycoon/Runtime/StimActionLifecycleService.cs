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
            PruneTerminalActions(save.state.actionProgress);
            if (save.state.actionProgress.Count >= StimActionProgressState.MaxEntries)
                return StimTransactionMutationResult.Failure("Too many active actions are stored. Complete or recover an existing action first.");
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
                claimWindowSeconds = Math.Max(0, definition.claimWindowSeconds),
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
                action.expiresAtUtc = action.claimWindowSeconds > 0
                    ? completesAt.AddSeconds(action.claimWindowSeconds).ToUniversalTime().ToString("O")
                    : string.Empty;
                action.revision = save.revision;
                changed++;
            }
            foreach (var action in save.state.actionProgress)
            {
                if (action == null || action.state != StimActionState.Claimable.ToString() ||
                    !DateTimeOffset.TryParse(action.expiresAtUtc, out var expiresAt) || now < expiresAt)
                    continue;
                action.state = StimActionState.Expired.ToString();
                action.revision = save.revision;
                changed++;
            }
            return changed;
        }

        public StimTransactionMutationResult Pause(StimSaveEnvelope save, string instanceId, DateTimeOffset now)
        {
            var action = Find(save, instanceId);
            if (action == null) return StimTransactionMutationResult.Failure("Action instance was not found.");
            if (action.state != StimActionState.InProgress.ToString())
                return StimTransactionMutationResult.Failure("Only an active action can be paused.");
            if (!DateTimeOffset.TryParse(action.completesAtUtc, out var completesAt))
                return StimTransactionMutationResult.Failure("The action timer is invalid.");
            if (now >= completesAt)
                return StimTransactionMutationResult.Failure("This action has finished and is ready to reconcile.");
            action.remainingSeconds = Math.Max(1, (int)Math.Ceiling((completesAt - now).TotalSeconds));
            action.pausedAtUtc = now.ToUniversalTime().ToString("O");
            action.state = StimActionState.Paused.ToString();
            action.revision = save.revision;
            return StimTransactionMutationResult.Success("Action paused.");
        }

        public StimTransactionMutationResult Resume(StimSaveEnvelope save, string instanceId, DateTimeOffset now)
        {
            var action = Find(save, instanceId);
            if (action == null) return StimTransactionMutationResult.Failure("Action instance was not found.");
            if (action.state != StimActionState.Paused.ToString() || action.remainingSeconds < 1)
                return StimTransactionMutationResult.Failure("This action is not paused.");
            action.completesAtUtc = now.AddSeconds(action.remainingSeconds).ToUniversalTime().ToString("O");
            action.pausedAtUtc = string.Empty;
            action.remainingSeconds = 0;
            action.state = StimActionState.InProgress.ToString();
            action.revision = save.revision;
            return StimTransactionMutationResult.Success("Action resumed.");
        }

        public StimTransactionMutationResult Claim(StimSaveEnvelope save, string instanceId, DateTimeOffset now)
        {
            var action = save?.state?.actionProgress?.Find(candidate => candidate?.instanceId == instanceId);
            if (action == null) return StimTransactionMutationResult.Failure("Action instance was not found.");
            if (action.state == StimActionState.Claimable.ToString() &&
                DateTimeOffset.TryParse(action.expiresAtUtc, out var expiresAt) && now >= expiresAt)
                return StimTransactionMutationResult.Failure("This action expired before it was claimed.");
            if (action.state != StimActionState.Claimable.ToString())
                return StimTransactionMutationResult.Failure(
                    action.state == StimActionState.Complete.ToString()
                        ? "This action was already claimed."
                        : action.state == StimActionState.Expired.ToString()
                            ? "This action expired before it was claimed."
                        : "This action is not ready to claim.");
            action.state = StimActionState.Complete.ToString();
            action.progress = action.progressRequired;
            action.claimedAtUtc = now.ToUniversalTime().ToString("O");
            action.revision = save.revision;
            return StimTransactionMutationResult.Success("Action claimed.");
        }

        private static StimActionProgressState Find(StimSaveEnvelope save, string instanceId) =>
            save?.state?.actionProgress?.Find(candidate => candidate?.instanceId == instanceId);

        private static void PruneTerminalActions(System.Collections.Generic.List<StimActionProgressState> actions)
        {
            while (actions.Count >= StimActionProgressState.MaxEntries)
            {
                var index = actions.FindIndex(action => action != null &&
                    (action.state == StimActionState.Complete.ToString() || action.state == StimActionState.Expired.ToString()));
                if (index < 0) return;
                actions.RemoveAt(index);
            }
        }
    }
}
