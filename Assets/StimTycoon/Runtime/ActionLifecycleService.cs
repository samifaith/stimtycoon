using System;
using StimTycoon.Saves;

namespace StimTycoon.Runtime
{
    /// <summary>Owns persisted timed-action state transitions without granting rewards.</summary>
    public sealed class ActionLifecycleService
    {
        public TransactionMutationResult Start(
            SaveEnvelope save,
            ActionDefinition definition,
            ActionRequest request,
            DateTimeOffset now)
        {
            if (save?.state == null || definition == null ||
                string.IsNullOrWhiteSpace(request.InstanceId) || request.ActionId != definition.id)
                return TransactionMutationResult.Failure("A valid action definition and instance are required.");
            save.state.actionProgress ??= new System.Collections.Generic.List<ActionProgressState>();
            PruneTerminalActions(save.state.actionProgress);
            if (save.state.actionProgress.Count >= ActionProgressState.MaxEntries)
                return TransactionMutationResult.Failure("Too many active actions are stored. Complete or recover an existing action first.");
            if (save.state.actionProgress.Exists(action => action?.instanceId == request.InstanceId))
                return TransactionMutationResult.Failure("This action instance already exists.");
            if (definition.state != ActionState.Ready)
                return TransactionMutationResult.Failure(
                    string.IsNullOrEmpty(definition.lockedReason) ? "This action is not ready." : definition.lockedReason);

            var utc = now.ToUniversalTime();
            var timed = definition.durationSeconds > 0;
            save.state.actionProgress.Add(new ActionProgressState
            {
                instanceId = request.InstanceId,
                actionId = definition.id,
                state = timed ? ActionState.InProgress.ToString() : ActionState.Claimable.ToString(),
                progress = timed ? 0 : 1,
                progressRequired = Math.Max(1, definition.progressRequired),
                durationSeconds = Math.Max(0, definition.durationSeconds),
                claimWindowSeconds = Math.Max(0, definition.claimWindowSeconds),
                revision = save.revision,
                startedAtUtc = utc.ToString("O"),
                completesAtUtc = timed ? utc.AddSeconds(definition.durationSeconds).ToString("O") : utc.ToString("O")
            });
            return TransactionMutationResult.Success(timed ? "Action started." : "Action ready to claim.");
        }

        public int Reconcile(SaveEnvelope save, DateTimeOffset now)
        {
            if (save?.state?.actionProgress == null) return 0;
            var changed = 0;
            foreach (var action in save.state.actionProgress)
            {
                if (action == null || action.state != ActionState.InProgress.ToString() ||
                    !DateTimeOffset.TryParse(action.completesAtUtc, out var completesAt) || now < completesAt)
                    continue;
                action.state = ActionState.Claimable.ToString();
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
                if (action == null || action.state != ActionState.Claimable.ToString() ||
                    !DateTimeOffset.TryParse(action.expiresAtUtc, out var expiresAt) || now < expiresAt)
                    continue;
                action.state = ActionState.Expired.ToString();
                action.revision = save.revision;
                changed++;
            }
            return changed;
        }

        public TransactionMutationResult Pause(SaveEnvelope save, string instanceId, DateTimeOffset now)
        {
            var action = Find(save, instanceId);
            if (action == null) return TransactionMutationResult.Failure("Action instance was not found.");
            if (action.state != ActionState.InProgress.ToString())
                return TransactionMutationResult.Failure("Only an active action can be paused.");
            if (!DateTimeOffset.TryParse(action.completesAtUtc, out var completesAt))
                return TransactionMutationResult.Failure("The action timer is invalid.");
            if (now >= completesAt)
                return TransactionMutationResult.Failure("This action has finished and is ready to reconcile.");
            action.remainingSeconds = Math.Max(1, (int)Math.Ceiling((completesAt - now).TotalSeconds));
            action.pausedAtUtc = now.ToUniversalTime().ToString("O");
            action.state = ActionState.Paused.ToString();
            action.revision = save.revision;
            return TransactionMutationResult.Success("Action paused.");
        }

        public TransactionMutationResult Resume(SaveEnvelope save, string instanceId, DateTimeOffset now)
        {
            var action = Find(save, instanceId);
            if (action == null) return TransactionMutationResult.Failure("Action instance was not found.");
            if (action.state != ActionState.Paused.ToString() || action.remainingSeconds < 1)
                return TransactionMutationResult.Failure("This action is not paused.");
            action.completesAtUtc = now.AddSeconds(action.remainingSeconds).ToUniversalTime().ToString("O");
            action.pausedAtUtc = string.Empty;
            action.remainingSeconds = 0;
            action.state = ActionState.InProgress.ToString();
            action.revision = save.revision;
            return TransactionMutationResult.Success("Action resumed.");
        }

        public TransactionMutationResult Claim(SaveEnvelope save, string instanceId, DateTimeOffset now)
        {
            var action = save?.state?.actionProgress?.Find(candidate => candidate?.instanceId == instanceId);
            if (action == null) return TransactionMutationResult.Failure("Action instance was not found.");
            if (action.state == ActionState.Claimable.ToString() &&
                DateTimeOffset.TryParse(action.expiresAtUtc, out var expiresAt) && now >= expiresAt)
                return TransactionMutationResult.Failure("This action expired before it was claimed.");
            if (action.state != ActionState.Claimable.ToString())
                return TransactionMutationResult.Failure(
                    action.state == ActionState.Complete.ToString()
                        ? "This action was already claimed."
                        : action.state == ActionState.Expired.ToString()
                            ? "This action expired before it was claimed."
                        : "This action is not ready to claim.");
            action.state = ActionState.Complete.ToString();
            action.progress = action.progressRequired;
            action.claimedAtUtc = now.ToUniversalTime().ToString("O");
            action.revision = save.revision;
            return TransactionMutationResult.Success("Action claimed.");
        }

        private static ActionProgressState Find(SaveEnvelope save, string instanceId) =>
            save?.state?.actionProgress?.Find(candidate => candidate?.instanceId == instanceId);

        private static void PruneTerminalActions(System.Collections.Generic.List<ActionProgressState> actions)
        {
            while (actions.Count >= ActionProgressState.MaxEntries)
            {
                var index = actions.FindIndex(action => action != null &&
                    (action.state == ActionState.Complete.ToString() || action.state == ActionState.Expired.ToString()));
                if (index < 0) return;
                actions.RemoveAt(index);
            }
        }
    }
}
