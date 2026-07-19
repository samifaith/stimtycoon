using System;
using System.Collections.Generic;
using NUnit.Framework;
using StimTycoon.Runtime;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class GoalsBinderTests
    {
        [Test]
        public void Constructor_RequiresRootAndReportsMissingContract()
        {
            Assert.Throws<ArgumentNullException>(() => new GoalsBinder(null, null));
            Assert.That(new GoalsBinder(new VisualElement(), null).IsValid, Is.False);
        }

        [Test]
        public void Render_EmptyStateUsesCanonicalCopyAndUnlockedCount()
        {
            var root = CreateRoot();
            var binder = new GoalsBinder(root, null);

            Render(binder, new List<AchievementState>(), new List<GoalState>());

            Assert.That(root.Q<Label>("achievements-count").text, Is.EqualTo("0 unlocked"));
            var list = root.Q<VisualElement>("achievements-list");
            Assert.That(list.childCount, Is.EqualTo(1));
            Assert.That(((Label)list[0]).text,
                Is.EqualTo("Your goals and milestones will appear here as this life unfolds."));
            Assert.That(list[0].ClassListContains("st-feed-empty"), Is.True);
        }

        [Test]
        public void Render_FiltersExpiredGoalsAndBuildsStableActions()
        {
            var root = CreateRoot();
            var binder = new GoalsBinder(root, null);
            var achievements = new List<AchievementState>
            {
                new AchievementState { achievementId = "first_job", unlockedAtAge = 18 }
            };
            var goals = new List<GoalState>
            {
                new GoalState
                {
                    goalId = "main-path", category = "main", title = "Choose a path",
                    progress = 1, progressRequired = 3, rewardMinorUnits = 5000, status = "active"
                },
                new GoalState { goalId = "old-daily", status = "expired" }
            };

            Render(binder, achievements, goals);

            var list = root.Q<VisualElement>("achievements-list");
            Assert.That(root.Q<Label>("achievements-count").text, Is.EqualTo("1 unlocked"));
            Assert.That(list.childCount, Is.EqualTo(1));
            Assert.That(list.Q<Button>("goal-action-main-path"), Is.Not.Null);
            Assert.That(list.Q<Button>("goal-pin-main-path"), Is.Not.Null);
            Assert.That(list.Q<Button>("goal-action-old-daily"), Is.Null);
            StringAssert.Contains("1 / 3", list[0].tooltip);
            Render(binder, achievements, goals, "achievements");
            Assert.That(list.Q<Button>("achievement-claim-first_job"), Is.Not.Null);
        }

        private static VisualElement CreateRoot()
        {
            var root = new VisualElement();
            root.Add(new Label { name = "achievements-count" });
            root.Add(new VisualElement { name = "achievements-list" });
            root.Add(new Label { name = "pinned-goal-summary" });
            root.Add(new Button { name = "goals-tab-main" });
            root.Add(new Button { name = "goals-tab-daily" });
            root.Add(new Button { name = "goals-tab-life" });
            root.Add(new Button { name = "goals-tab-achievements" });
            return root;
        }

        private static void Render(
            GoalsBinder binder,
            IReadOnlyList<AchievementState> achievements,
            IReadOnlyList<GoalState> goals,
            string board = "main")
        {
            binder.Render(
                achievements,
                goals,
                value => $"${value / 100m:0.00}",
                (value, maximum) => $"{value} / {maximum}",
                value => value.ToUpperInvariant(),
                board,
                _ => { },
                _ => { },
                _ => { });
        }
    }
}
