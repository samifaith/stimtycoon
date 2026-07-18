using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimHomeBinder
    {
        private readonly Label condition;
        private readonly Label progress;
        private readonly VisualElement actions;
        private readonly Label feedback;

        public StimHomeBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            condition = root.Q<Label>("home-condition");
            progress = root.Q<Label>("home-progress");
            actions = root.Q<VisualElement>("home-actions");
            feedback = root.Q<Label>("home-upgrade-feedback");
            Retry = root.Q<Button>("home-action-retry");
        }

        public Button Retry { get; }
        public bool IsValid => condition != null && progress != null && actions != null && feedback != null && Retry != null;

        public void Render(
            StimGameState state,
            StimHomeDefinition definition,
            int requiredProgress,
            Func<long, string> formatMoney,
            Func<string, string> formatDisplayName,
            Action<StimHomeActionType> performAction,
            Action performUpgrade)
        {
            var home = state.home ?? new StimHomeState();
            condition.text = $"{definition.displayName} · Condition {home.condition} / 100";
            progress.text = home.upgradeLevel >= definition.maxUpgradeLevel
                ? $"Level {definition.maxUpgradeLevel} · Fully upgraded · Reading stock {home.readingMaterialStock}/{home.readingMaterialCapacity} · Equipment {home.trainingEquipmentCondition}%"
                : $"Level {home.upgradeLevel} · Improvement {home.improvementProgress}/{requiredProgress} · Reading stock {home.readingMaterialStock}/{home.readingMaterialCapacity} · Equipment {home.trainingEquipmentCondition}%";
            actions.Clear();
            foreach (var action in definition.actions)
                AddAction(state, action, formatMoney, formatDisplayName, performAction);

            if (home.upgradeLevel >= definition.maxUpgradeLevel || state.character.age < 18) return;
            var cost = StimGameSessionService.GetHomeUpgradeCost(home.upgradeLevel);
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
            StimFeedbackPresenter.ShowTransactionResult(feedback, succeeded, summary);
        }

        private void AddAction(
            StimGameState state,
            StimHomeActionDefinition definition,
            Func<long, string> formatMoney,
            Func<string, string> formatDisplayName,
            Action<StimHomeActionType> performAction)
        {
            var actionType = definition.actionType;
            var caregiverHandlesMaintenance = actionType == StimHomeActionType.Maintain && state.character.age < 18;
            var cost = caregiverHandlesMaintenance ? 0 : definition.costMinorUnits;
            var cooldownId = $"home_{actionType.ToString().ToLowerInvariant()}_used";
            var coolingDown = state.statuses.Exists(status => status.statusId == cooldownId);
            var hasCapacity = actionType != StimHomeActionType.Read || state.home.readingMaterialStock > 0;
            hasCapacity &= actionType != StimHomeActionType.Train || state.home.trainingEquipmentCondition >= 10;
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
