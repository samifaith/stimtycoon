using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class HomeBinder
    {
        private readonly Label condition;
        private readonly Label progress;
        private readonly VisualElement actions;
        private readonly VisualElement objectList;
        private readonly Label feedback;

        public HomeBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            condition = root.Q<Label>("home-condition");
            progress = root.Q<Label>("home-progress");
            actions = root.Q<VisualElement>("home-actions");
            objectList = root.Q<VisualElement>("home-object-list");
            feedback = root.Q<Label>("home-upgrade-feedback");
            Retry = root.Q<Button>("home-action-retry");
        }

        public Button Retry { get; }
        public bool IsValid => condition != null && progress != null && objectList != null && actions != null && feedback != null && Retry != null;

        public void Render(
            GameState state,
            HomeDefinition definition,
            int requiredProgress,
            Func<long, string> formatMoney,
            Func<string, string> formatDisplayName,
            string selectedObjectId,
            Action<string> selectObject,
            Action<HomeActionType> performAction,
            Action performUpgrade)
        {
            var home = state.home ?? new HomeState();
            var books = home.inventory?.Find(item => item != null && item.itemId == "starter_books");
            var equipment = home.inventory?.Find(item => item != null && item.itemId == "starter_training_kit");
            var inventorySummary = books != null && equipment != null
                ? $"Books {books.quantity}/{books.capacity} · Equipment {equipment.condition}% · Source {formatDisplayName(books.acquisitionSource)}"
                : $"Books {home.readingMaterialStock}/{home.readingMaterialCapacity} · Equipment {home.trainingEquipmentCondition}%";
            condition.text = $"{definition.displayName} · Condition {home.condition} / 100";
            progress.text = home.upgradeLevel >= definition.maxUpgradeLevel
                ? $"Level {definition.maxUpgradeLevel} · Fully upgraded · {inventorySummary}"
                : $"Level {home.upgradeLevel} · Improvement {home.improvementProgress}/{requiredProgress} · {inventorySummary}";
            objectList.Clear();
            var selectedExists = definition.actions.Exists(item => item.roomObjectId == selectedObjectId);
            if (!selectedExists) selectedObjectId = definition.actions.Count > 0 ? definition.actions[0].roomObjectId : string.Empty;
            foreach (var action in definition.actions)
            {
                var objectId = action.roomObjectId;
                var objectButton = new Button { name = $"home-object-{objectId}", text = formatDisplayName(objectId) };
                objectButton.AddToClassList("st-segmented-tab");
                PresentationStateStyler.Apply(objectButton, objectId == selectedObjectId
                    ? PresentationState.Selected : PresentationState.Available);
                objectButton.clicked += () => selectObject(objectId);
                objectList.Add(objectButton);
            }
            actions.Clear();
            foreach (var action in definition.actions)
                if (action.roomObjectId == selectedObjectId)
                    AddAction(state, action, formatMoney, formatDisplayName, performAction);

            if (home.upgradeLevel >= definition.maxUpgradeLevel || state.character.age < 18) return;
            var cost = GameSessionService.GetHomeUpgradeCost(home.upgradeLevel);
            var button = new Button
            {
                name = "home-upgrade",
                text = $"UPGRADE TO LEVEL {home.upgradeLevel + 1}\n{formatMoney(cost)} · Requires {requiredProgress} progress · Improves home benefits"
            };
            button.AddToClassList("st-home-action");
            button.AddToClassList("st-home-upgrade");
            button.SetEnabled(home.improvementProgress >= requiredProgress && state.finances.cashMinorUnits >= cost);
            button.clicked += performUpgrade;
            actions.Add(button);
        }

        public void ShowTransactionResult(bool succeeded, string summary)
        {
            FeedbackPresenter.ShowTransactionResult(feedback, succeeded, summary);
        }

        private void AddAction(
            GameState state,
            HomeActionDefinition definition,
            Func<long, string> formatMoney,
            Func<string, string> formatDisplayName,
            Action<HomeActionType> performAction)
        {
            var actionType = definition.actionType;
            var caregiverHandlesMaintenance = actionType == HomeActionType.Maintain && state.character.age < 18;
            var cost = caregiverHandlesMaintenance ? 0 : definition.costMinorUnits;
            var cooldownId = $"home_{actionType.ToString().ToLowerInvariant()}_used";
            var coolingDown = state.statuses.Exists(status => status.statusId == cooldownId);
            var hasCapacity = actionType != HomeActionType.Read || state.home.readingMaterialStock > 0;
            hasCapacity &= actionType != HomeActionType.Train || state.home.trainingEquipmentCondition >= 10;
            var button = new Button
            {
                name = $"home-action-{actionType.ToString().ToLowerInvariant()}",
                text = $"{(caregiverHandlesMaintenance ? "Ask caregiver to maintain" : definition.displayName)}\n" +
                       $"{(cost == 0 ? "Free" : formatMoney(cost))} · {definition.benefitPreview}" +
                       $" · {formatDisplayName(definition.roomObjectId)}" +
                       (coolingDown ? "\nAvailable next month" : string.Empty)
            };
            button.AddToClassList("st-home-action");
            button.SetEnabled(!coolingDown && hasCapacity && state.finances.cashMinorUnits >= cost);
            button.clicked += () => performAction(actionType);
            actions.Add(button);
        }
    }
}
