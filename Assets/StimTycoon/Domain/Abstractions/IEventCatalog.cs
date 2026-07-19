using System.Collections.Generic;
using StimTycoon.Events;

namespace StimTycoon.Abstractions
{
    /// <summary>
    /// First-party catalog boundary for authored Stim events.
    /// </summary>
    public interface IEventCatalog
    {
        int Count { get; }

        bool TryGetEvent(string eventId, out Event evt);

        IReadOnlyList<Event> GetAllEvents();

        void Upsert(Event evt);
    }
}
