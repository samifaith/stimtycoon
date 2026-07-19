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
        private readonly Label pinnedGoalSummary;

        public StimGoalsBinder(VisualElement root, VisualTreeAsset rowTemplate)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            achievementsCount = root.Q<Label>("achievements-count");
            achievementsList = root.Q<VisualElement>("achievements-list");
            achievementRowTemplate = rowTemplate;
            pinnedGoalSummary = root.Q<Label>("pinned-goal-summary");
            TabMain = root.Q<Button>("goals-tab-main");
            TabDaily = root.Q<Button>("goals-tab-daily");
            TabLife = root.Q<Button>("goals-tab-life");
            TabAchievements = root.Q<Button>("goals-tab-achievements");
        }

        public Button TabMain { get; }
        public Button TabDaily { get; }
        public Button TabLife { get; }
        public Button TabAchievements { get; }
        public bool IsValid => achievementsCount != null && achievementsList != null && pinnedGoalSummary != null &&
                               TabMain != null && TabDaily != null && TabLife != null && TabAchievements != null;

        public void Render(
            IReadOnlyList<StimAchievementState> achievements,
            IReadOnlyList<StimGoalState> goals,
            Func<long, string> formatMoney,
            Func<long, long, string> formatProgress,
            Func<string, string> formatCategory,
            string board,
            Action<StimGoalState> handleGoal,
            Action<StimGoalState> togglePin,
            Action<string> claimAchievement)
        {
            if (!IsValid) return;
            achievementsList.Clear();
            achievementsCount.text = $"{achievements?.Count ?? 0} unlocked";
            ApplyTab(TabMain, board == "main");
            ApplyTab(TabDaily, board == "daily");
            ApplyTab(TabLife, board == "life");
            ApplyTab(TabAchievements, board == "achievements");
            StimGoalState pinned = null;
            if (goals != null)
                foreach (var goal in goals) if (goal?.pinned == true) { pinned = goal; break; }
            pinnedGoalSummary.text = pinned == null ? "No goal pinned · Pin one goal for quick focus." :
                $"Pinned · {pinned.title} · {pinned.progress}/{pinned.progressRequired} · {pinned.description}";

            if (goals != null && board != "achievements")
            {
                foreach (var goal in goals)
                {
                    if (goal == null || goal.status == "expired" || goal.category != board) continue;
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
                    var pin = new Button(() => togglePin(capturedGoal))
                    {
                        name = $"goal-pin-{goal.goalId}", text = goal.pinned ? "UNPIN" : "PIN",
                        tooltip = goal.pinned ? "Remove this goal from quick focus" : "Pin this goal for quick focus"
                    };
                    pin.AddToClassList("st-action-requirement-chip");
                    row.Add(pin);
                    achievementsList.Add(row);
                }
            }

            if (board == "achievements" && achievements != null)
            {
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
            if (achievementsList.childCount == 0)
            {
                var noGoalsOrAchievements = (achievements == null || achievements.Count == 0) &&
                                            (goals == null || goals.Count == 0);
                var empty = new Label(noGoalsOrAchievements
                    ? "Your goals and milestones will appear here as this life unfolds."
                    : board == "achievements"
                    ? "No achievements unlocked yet."
                    : $"No {board} goals are active right now.");
                empty.AddToClassList("st-feed-empty");
                achievementsList.Add(empty);
            }
        }

        private static void ApplyTab(Button tab, bool selected) =>
            StimPresentationStateStyler.Apply(tab, selected ? StimPresentationState.Selected : StimPresentationState.Available);
    }
}
