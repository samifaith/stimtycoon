using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimSocialBinder
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

        public StimSocialBinder(VisualElement root)
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
            RelationshipActions = root.Q<VisualElement>("relationship-actions");
        }

        public Button DiscoverCompatiblePerson { get; }
        public Button RelationshipBack { get; }
        public VisualElement RelationshipActions { get; }
        public bool IsValid => relationshipListView != null && relationshipList != null &&
            DiscoverCompatiblePerson != null && relationshipDetailView != null && RelationshipBack != null &&
            relationshipAvatar != null && relationshipName != null && relationshipType != null &&
            relationshipStrength != null && relationshipFill != null && relationshipGenetics != null &&
            RelationshipActions != null;

        public void ShowList() { relationshipDetailView.AddToClassList("hidden"); relationshipListView.RemoveFromClassList("hidden"); }

        public void RenderList(StimGameState state, Action<string> showDetail)
        {
            relationshipList.Clear();
            var adult = state.character.age >= 18;
            var discoveryUsed = state.statuses?.Exists(status => status.statusId == "relationship_discovery_used") == true;
            DiscoverCompatiblePerson.EnableInClassList("hidden", !adult);
            DiscoverCompatiblePerson.SetEnabled(adult && !discoveryUsed && string.IsNullOrEmpty(state.pendingEventId));
            if (state.relationships == null || state.relationships.Count == 0)
            {
                var empty = new Label("Important people will appear here as your life grows.");
                empty.AddToClassList("st-feed-empty"); relationshipList.Add(empty); return;
            }
            foreach (var relationship in state.relationships)
            {
                if (relationship == null) continue;
                var id = relationship.relationshipId;
                relationshipList.Add(StimUiComponentFactory.CreateRelationshipRow(id, relationship.displayName,
                    relationship.relationshipType, relationship.value, () => showDetail(id)));
            }
        }

        public void ShowDetail(StimRelationshipState relationship)
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
        }

        private static string Display(string id) => string.IsNullOrEmpty(id) ? "" : char.ToUpperInvariant(id[0]) + id.Substring(1).Replace('_', ' ');
    }
}
