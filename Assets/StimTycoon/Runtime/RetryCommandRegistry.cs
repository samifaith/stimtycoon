using System;
using System.Collections.Generic;

namespace StimTycoon.Runtime
{
    public sealed class RetryCommandRegistry
    {
        private readonly Dictionary<string, Action> commands = new Dictionary<string, Action>();
        private readonly HashSet<string> executing = new HashSet<string>();

        public bool IsAvailable(string commandId) =>
            !string.IsNullOrWhiteSpace(commandId) && commands.ContainsKey(commandId);

        public void Register(string commandId, Action command)
        {
            if (string.IsNullOrWhiteSpace(commandId))
                throw new ArgumentException("A stable retry command ID is required.", nameof(commandId));
            if (command == null) throw new ArgumentNullException(nameof(command));
            commands[commandId] = command;
        }

        public bool TryExecute(string commandId)
        {
            if (!IsAvailable(commandId) || executing.Contains(commandId)) return false;
            var command = commands[commandId];
            executing.Add(commandId);
            try
            {
                command();
                return true;
            }
            finally
            {
                executing.Remove(commandId);
            }
        }

        public void Clear(string commandId)
        {
            if (!string.IsNullOrWhiteSpace(commandId)) commands.Remove(commandId);
        }

        public void ClearAll()
        {
            commands.Clear();
            executing.Clear();
        }
    }
}
