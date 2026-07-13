namespace StimTycoon.Abstractions
{
    /// <summary>
    /// Wraps Dialogue System / authored content execution behind Stim-owned code.
    /// </summary>
    public interface IStimDialogueBridge
    {
        bool CanRunEvent(string eventId);
        bool TryStartEvent(string eventId);
        string GetRiskLabel(string eventId, string choiceId);
        bool ResolveChoice(string eventId, string choiceId);
        void ApplyResolvedOutcome();
        void ScheduleFollowUps();
    }
}
