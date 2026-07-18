using System;
using System.Collections.Generic;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimGoalsBinder
    {
        private readonly Label achievementsCount;
        private readonly VisualElement achievementsList;
        private readonly VisualTreeAsset achievementRowTemplate;

        public StimGoalsBinder(VisualElement root, VisualTreeAsset rowTemplate)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            achievementsCount = root.Q<Label>("achievements-count");
            achievementsList = root.Q<VisualElement>("achievements-list");
            achievementRowTemplate = rowTemplate;
        }

        public bool IsValid => achievementsCount != null && achievementsList != null;

        public void Render(
            IReadOnlyList<StimAchievementState> achievements,
            IReadOnlyList<StimGoalState> goals,
            Func<long, string> formatMoney,
            Func<long, long, string> formatProgress,
            Func<string, string> formatCategory,
            Action<StimGoalState> handleGoal,
            Action<string> claimAchievement)
        {
            if (!IsValid) return;
            achievementsList.Clear();
            achievementsCount.text = $"{achievements?.Count ?? 0} unlocked";

            if (goals != null)
            {
                foreach (var goal in goals)
                {
                    if (goal == null || goal.status == "expired") continue;
                    var capturedGoal = goal;
                    var actionText = goal.status == "claimable" ? "CLAIM" :
                        goal.status == "claimed" ? "DONE" : "GO";
                    var row = StimUiComponentFactory.CreateAchievementRow(
                        goal.goalId,
                        goal.category == "daily" ? "📅" : goal.category == "main" ? "🎯" : "🏆",
                        goal.title,
                        formatCategory(goal.category),
                        formatProgress(goal.progress, goal.progressRequired),
                        formatMoney(goal.rewardMinorUnits),
                        actionText,
                        goal.status != "claimed",
                        () => handleGoal(capturedGoal),
                        accessibleProgress: $"{goal.progress:N0} / {goal.progressRequired:N0}",
                        template: achievementRowTemplate);
                    row.Q<Button>().name = $"goal-action-{goal.goalId}";
                    achievementsList.Add(row);
                }
            }

            if ((achievements == null || achievements.Count == 0) && (goals == null || goals.Count == 0))
            {
                var empty = new Label("Your goals and milestones will appear here as this life unfolds.");
                empty.AddToClassList("st-feed-empty");
                achievementsList.Add(empty);
            }

            if (achievements == null) return;
            for (var index = achievements.Count - 1; index >= 0; index--)
            {
                var achievement = achievements[index];
                if (achievement == null) continue;
                var capturedAchievementId = achievement.achievementId;
                var reward = StimGameSessionService.GetAchievementRewardMinorUnits(achievement.achievementId);
                var row = StimUiComponentFactory.CreateAchievementRow(
                    achievement.achievementId,
                    "🏆",
                    StimGameSessionService.GetAchievementDisplayName(achievement.achievementId),
                    "Achievement",
                    $"Age {achievement.unlockedAtAge}",
                    formatMoney(reward),
                    achievement.rewardClaimed ? "DONE" : "CLAIM",
                    !achievement.rewardClaimed && reward > 0,
                    () => claimAchievement(capturedAchievementId),
                    template: achievementRowTemplate);
                row.Q<Button>().name = $"achievement-claim-{achievement.achievementId}";
                achievementsList.Add(row);
            }
        }
    }
}
