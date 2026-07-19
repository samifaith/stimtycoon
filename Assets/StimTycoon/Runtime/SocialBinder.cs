using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class SocialBinder
    {
        private readonly VisualElement relationshipListView;
        private readonly VisualElement relationshipList;
        private readonly VisualElement relationshipDetailView;
        private readonly Label relationshipAvatar;
        private readonly Label relationshipName;
        private readonly Label relationshipType;
        private readonly Label relationshipStrength;
        private readonly VisualElement relationshipFill;
        private readonly Label relationshipGenetics;
        private readonly Label relationshipStatus;
        private readonly Label relationshipHistory;
        private readonly Label familySummary;

        public SocialBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            relationshipListView = root.Q<VisualElement>("relationship-list-view");
            relationshipList = root.Q<VisualElement>("relationship-list");
            DiscoverCompatiblePerson = root.Q<Button>("discover-compatible-person");
            relationshipDetailView = root.Q<VisualElement>("relationship-detail-view");
            RelationshipBack = root.Q<Button>("relationship-back");
            relationshipAvatar = root.Q<Label>("relationship-avatar");
            relationshipName = root.Q<Label>("relationship-name");
            relationshipType = root.Q<Label>("relationship-type");
            relationshipStrength = root.Q<Label>("relationship-strength");
            relationshipFill = root.Q<VisualElement>("relationship-fill");
            relationshipGenetics = root.Q<Label>("relationship-genetics");
            relationshipStatus = root.Q<Label>("relationship-status");
            relationshipHistory = root.Q<Label>("relationship-history");
            familySummary = root.Q<Label>("family-summary");
            FilterAll = root.Q<Button>("social-filter-all");
            FilterFamily = root.Q<Button>("social-filter-family");
            FilterFriends = root.Q<Button>("social-filter-friends");
            FilterRomance = root.Q<Button>("social-filter-romance");
            RelationshipActions = root.Q<VisualElement>("relationship-actions");
        }

        public Button DiscoverCompatiblePerson { get; }
        public Button RelationshipBack { get; }
        public VisualElement RelationshipActions { get; }
        public Button FilterAll { get; }
        public Button FilterFamily { get; }
        public Button FilterFriends { get; }
        public Button FilterRomance { get; }
        public bool IsValid => relationshipListView != null && relationshipList != null &&
            DiscoverCompatiblePerson != null && relationshipDetailView != null && RelationshipBack != null &&
            relationshipAvatar != null && relationshipName != null && relationshipType != null &&
            relationshipStrength != null && relationshipFill != null && relationshipGenetics != null &&
            relationshipStatus != null && relationshipHistory != null && familySummary != null &&
            FilterAll != null && FilterFamily != null && FilterFriends != null && FilterRomance != null &&
            RelationshipActions != null;

        public void ShowList() { relationshipDetailView.AddToClassList("hidden"); relationshipListView.RemoveFromClassList("hidden"); }

        public void RenderList(GameState state, string filter, Action<string> showDetail)
        {
            relationshipList.Clear();
            var adult = state.character.age >= 18;
            var discoveryUsed = state.statuses?.Exists(status => status.statusId == "relationship_discovery_used") == true;
            DiscoverCompatiblePerson.EnableInClassList("hidden", !adult);
            DiscoverCompatiblePerson.SetEnabled(adult && !discoveryUsed && string.IsNullOrEmpty(state.pendingEventId));
            var dependentCount = state.family?.children?.FindAll(child => child != null && child.age < 18).Count ?? 0;
            familySummary.text = $"Household happiness {state.household?.happiness ?? 50}/100 · Cohesion {state.household?.cohesion ?? 50}/100 · " +
                                 $"Dependents {dependentCount} · Monthly care ${dependentCount * 250:N0}";
            ApplyFilterState(FilterAll, filter == "all");
            ApplyFilterState(FilterFamily, filter == "family");
            ApplyFilterState(FilterFriends, filter == "friends");
            ApplyFilterState(FilterRomance, filter == "romance");
            if (state.relationships == null || state.relationships.Count == 0)
            {
                var empty = new Label("Important people will appear here as your life grows.");
                empty.AddToClassList("st-feed-empty"); relationshipList.Add(empty); return;
            }
            foreach (var relationship in state.relationships)
            {
                if (relationship == null) continue;
                if (!MatchesFilter(relationship, filter)) continue;
                var id = relationship.relationshipId;
                relationshipList.Add(UiComponentFactory.CreateRelationshipRow(id, relationship.displayName,
                    relationship.relationshipType, relationship.value, () => showDetail(id)));
            }
        }

        public void ShowDetail(RelationshipState relationship)
        {
            relationshipListView.AddToClassList("hidden"); relationshipDetailView.RemoveFromClassList("hidden");
            relationshipName.text = string.IsNullOrEmpty(relationship.displayName) ? Display(relationship.relationshipId) : relationship.displayName;
            relationshipAvatar.text = relationshipName.text.Substring(0, 1).ToUpperInvariant();
            relationshipType.text = Display(string.IsNullOrEmpty(relationship.relationshipStage) ? relationship.relationshipType : relationship.relationshipStage).ToUpperInvariant();
            relationshipStrength.text = $"Relationship {relationship.value} / 100 · Warmth {relationship.warmth} / 100";
            relationshipFill.style.width = Length.Percent(Math.Max(0, Math.Min(100, relationship.value)));
            relationshipGenetics.text = relationship.isGeneticParent
                ? $"Inherited profile · Health {relationship.geneticHealth} · Looks {relationship.geneticLooks} · Smarts {relationship.geneticSmarts}"
                : string.IsNullOrEmpty(relationship.origin) ? "This relationship is part of your growing social story."
                : $"{(string.IsNullOrEmpty(relationship.pronouns) ? string.Empty : relationship.pronouns + " · ")}Met through {Display(string.IsNullOrEmpty(relationship.introductionContext) ? relationship.origin : relationship.introductionContext)} at age {relationship.introducedAtAge} · " +
                  (relationship.monthsSinceInteraction == 0 ? "Connected this month." : $"{relationship.monthsSinceInteraction} months since focused time together.");
            var unavailable = relationship.relationshipType == "deceased" || relationship.relationshipType == "unavailable";
            relationshipStatus.text = unavailable ? "Unavailable · interactions closed" :
                $"Consent: {(IsRomantic(relationship.relationshipType) ? "mutual relationship" : "ordinary social contact")} · " +
                (relationship.monthsSinceInteraction > 0 ? $"Cooldown clear · {relationship.monthsSinceInteraction} months since interaction" : "Interacted this month");
            if (relationship.relationshipHistory == null || relationship.relationshipHistory.Count == 0)
                relationshipHistory.text = "History · No recorded moments yet.";
            else
            {
                var start = Math.Max(0, relationship.relationshipHistory.Count - 5);
                var lines = new System.Text.StringBuilder("History");
                for (var index = start; index < relationship.relationshipHistory.Count; index++)
                    lines.Append($"\n• {relationship.relationshipHistory[index].summary}");
                relationshipHistory.text = lines.ToString();
            }
        }

        private static void ApplyFilterState(Button button, bool selected) =>
            PresentationStateStyler.Apply(button, selected ? PresentationState.Selected : PresentationState.Available);

        private static bool MatchesFilter(RelationshipState relationship, string filter)
        {
            if (string.IsNullOrEmpty(filter) || filter == "all") return true;
            var type = relationship.relationshipType ?? string.Empty;
            if (filter == "family") return relationship.isGeneticParent || type == "parent" || type == "child" || type == "adult_child" || type == "married";
            if (filter == "friends") return type == "friend" || type == "best_friend" || type == "rival";
            return IsRomantic(type);
        }

        private static bool IsRomantic(string type) =>
            type == "dating" || type == "partner" || type == "engaged" || type == "married" || type == "ex_partner";

        private static string Display(string id) => string.IsNullOrEmpty(id) ? "" : char.ToUpperInvariant(id[0]) + id.Substring(1).Replace('_', ' ');
    }
}
