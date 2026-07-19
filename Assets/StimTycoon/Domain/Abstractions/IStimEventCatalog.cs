using System.Collections.Generic;
using StimTycoon.Events;

namespace StimTycoon.Abstractions
{
    /// <summary>
    /// First-party catalog boundary for authored Stim events.
    /// </summary>
    public interface IStimEventCatalog
    {
        int Count { get; }

        bool TryGetEvent(string eventId, out StimEvent evt);

        IReadOnlyList<StimEvent> GetAllEvents();

        void Upsert(StimEvent evt);
    }
}
