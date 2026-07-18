using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimStudyBinder
    {
        private readonly VisualElement educationCatalog;
        private readonly Label educationCatalogStatus;
        private readonly VisualElement educationCatalogList;
        private readonly Label sessionTitle;
        private readonly Label sessionDescription;
        private readonly Label sessionEffects;
        private readonly Label sessionTiming;
        private readonly Label sessionRequirement;

        public StimStudyBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            educationCatalog = root.Q<VisualElement>("education-catalog");
            educationCatalogStatus = root.Q<Label>("education-catalog-status");
            educationCatalogList = root.Q<VisualElement>("education-catalog-list");
            sessionTitle = root.Q<Label>("study-session-title");
            sessionDescription = root.Q<Label>("study-session-description");
            sessionEffects = root.Q<Label>("study-session-effects");
            sessionTiming = root.Q<Label>("study-session-timing");
            sessionRequirement = root.Q<Label>("study-session-requirement");
            Cancel = root.Q<Button>("study-session-cancel");
            Confirm = root.Q<Button>("study-session-confirm");
        }

        public Button Cancel { get; }
        public Button Confirm { get; }
        public bool IsValid => educationCatalog != null && educationCatalogStatus != null &&
                               educationCatalogList != null && sessionTitle != null &&
                               sessionDescription != null && sessionEffects != null && sessionTiming != null &&
                               sessionRequirement != null && Cancel != null && Confirm != null;

        public void SetCatalogVisible(bool visible) =>
            educationCatalog?.EnableInClassList("hidden", !visible);

        public void RenderCatalog(StimGameState state, VisualElement educationActions, Func<long, string> formatMoney)
        {
            educationCatalogList.Clear();
            if (state?.character == null || state.education == null ||
                state.character.age < 14 || state.character.age >= 18) return;

            var currentTrack = state.education.studyTrack ?? string.Empty;
            var qualificationXp = Math.Max(0, state.education.qualificationExperience);
            educationCatalogStatus.text = string.IsNullOrEmpty(currentTrack)
                ? "Choose one path below. The choice persists and shapes later qualifications."
                : $"Current: {ToDisplayName(currentTrack)} · " +
                  $"{StimEducationActionService.GetQualificationTier(qualificationXp)} · {qualificationXp} XP";

            foreach (var discipline in StimEducationDisciplineCatalog.GetAll())
            {
                var cost = discipline.studyTrack == StimStudyTrack.Academic ? 5000L :
                    discipline.studyTrack == StimStudyTrack.Vocational ? 7500L : 0L;
                AddCatalogRow(state, educationActions, formatMoney, discipline, cost, currentTrack);
            }
        }

        public void RenderSession(StimActionDefinition definition)
        {
            if (definition == null) return;
            sessionTitle.text = definition.title;
            sessionDescription.text = definition.description;
            sessionEffects.text = definition.previews == null || definition.previews.Count == 0
                ? "No numeric changes"
                : string.Join(" · ", definition.previews.ConvertAll(delta =>
                    $"{delta.targetId} {(delta.amount >= 0 ? "+" : "−")}{Math.Abs(delta.amount)}"));
            sessionTiming.text = definition.cooldownMonths > 0
                ? $"{definition.durationSeconds} seconds · Uses this month's school action · Available again after advancing a month"
                : "No monthly cooldown";
            StimFeedbackPresenter.Show(
                sessionRequirement,
                string.IsNullOrEmpty(definition.lockedReason) ? "Ready to begin" : definition.lockedReason,
                definition.state == StimActionState.Ready
                    ? StimFeedbackKind.Confirmation
                    : StimFeedbackKind.Error);
            Confirm.SetEnabled(definition.state == StimActionState.Ready);
        }

        public void ShowPersistenceError(string message) =>
            StimFeedbackPresenter.Show(sessionRequirement, message, StimFeedbackKind.Error);

        private void AddCatalogRow(
            StimGameState state,
            VisualElement educationActions,
            Func<long, string> formatMoney,
            StimEducationDisciplineDefinition discipline,
            long costMinorUnits,
            string currentTrack)
        {
            var trackId = discipline.studyTrack.ToString().ToLowerInvariant();
            var selected = string.Equals(currentTrack, trackId, StringComparison.OrdinalIgnoreCase);
            var choiceOpen = string.IsNullOrEmpty(currentTrack) &&
                             string.IsNullOrEmpty(state.education.awaitingDecisionId);
            var affordable = state.finances.cashMinorUnits >= costMinorUnits;
            var materialText = costMinorUnits == 0 ? "Free materials" : $"{formatMoney(costMinorUnits)} materials";
            var trailing = selected ? "CURRENT" : choiceOpen && affordable ? "GO" : materialText;
            Action onOpen = null;
            if (choiceOpen && affordable)
                onOpen = () => educationActions?.Q<Button>($"study-track-{trackId}")?.Focus();

            var row = StimUiComponentFactory.CreatePathRow(
                $"study-{trackId}", "🎓", discipline.displayName,
                $"{discipline.studyTrack} Track · {discipline.consequenceSummary} · {materialText}",
                trailing, affordable || selected, onOpen);
            row.AddToClassList("st-education-catalog-row");
            if (selected) StimPresentationStateStyler.Apply(row, StimPresentationState.Selected);
            educationCatalogList.Add(row);
        }

        private static string ToDisplayName(string id) =>
            string.IsNullOrEmpty(id) ? "" : char.ToUpperInvariant(id[0]) + id.Substring(1).Replace('_', ' ');
    }
}
