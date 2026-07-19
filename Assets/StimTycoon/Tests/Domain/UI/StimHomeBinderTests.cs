using System;
using NUnit.Framework;
using StimTycoon.Runtime;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class StimHomeBinderTests
    {
        [Test]
        public void Constructor_RequiresCompleteNamedContract()
        {
            Assert.Throws<ArgumentNullException>(() => new StimHomeBinder(null));
            Assert.That(new StimHomeBinder(new VisualElement()).IsValid, Is.False);
            Assert.That(new StimHomeBinder(CreateRoot()).IsValid, Is.True);
        }

        [Test]
        public void Render_AdultHomeBuildsActionsAndEligibleUpgrade()
        {
            var root = CreateRoot();
            var binder = new StimHomeBinder(root);
            var state = CreateState(25);
            state.finances.cashMinorUnits = 1000000;
            state.home.improvementProgress = 100;

            Render(binder, state);

            Assert.That(root.Q<Label>("home-condition").text, Does.StartWith("Starter Home · Condition 80 / 100"));
            Assert.That(root.Q<VisualElement>("home-object-list").childCount, Is.EqualTo(5));
            Assert.That(root.Q<VisualElement>("home-actions").childCount, Is.EqualTo(2));
            Assert.That(root.Q<Button>("home-upgrade"), Is.Not.Null);
            Assert.That(root.Q<Button>("home-upgrade").enabledSelf, Is.True);
        }

        [Test]
        public void Render_ChildUsesCaregiverMaintenanceAndOmitsUpgrade()
        {
            var root = CreateRoot();
            var binder = new StimHomeBinder(root);
            var state = CreateState(12);

            Render(binder, state, "toolbox");

            Assert.That(root.Q<Button>("home-upgrade"), Is.Null);
            Assert.That(root.Q<Button>("home-action-maintain").text,
                Does.StartWith("Ask caregiver to maintain\nFree"));
        }

        [Test]
        public void ShowTransactionResult_UsesSharedRecoverableFeedbackContract()
        {
            var root = CreateRoot();
            var binder = new StimHomeBinder(root);

            binder.ShowTransactionResult(false, "Save unavailable");

            var feedback = root.Q<Label>("home-upgrade-feedback");
            Assert.That(feedback.text, Is.EqualTo("Save unavailable Try again."));
            Assert.That(feedback.ClassListContains("is-error"), Is.True);
        }

        private static StimGameState CreateState(int age)
        {
            var state = new StimGameState();
            state.character.age = age;
            return state;
        }

        private static void Render(StimHomeBinder binder, StimGameState state, string selectedObjectId = "bookshelf")
        {
            binder.Render(
                state,
                StimHomeContentCatalog.Get("starter_home"),
                StimGameSessionService.GetHomeUpgradeRequiredProgress(state.home.upgradeLevel),
                value => $"${value / 100m:0.00}",
                value => value.Replace('_', ' '),
                selectedObjectId,
                _ => { },
                _ => { },
                () => { });
        }

        private static VisualElement CreateRoot()
        {
            var root = new VisualElement();
            root.Add(new Label { name = "home-condition" });
            root.Add(new Label { name = "home-progress" });
            root.Add(new VisualElement { name = "home-object-list" });
            root.Add(new VisualElement { name = "home-actions" });
            root.Add(new Label { name = "home-upgrade-feedback" });
            root.Add(new Button { name = "home-action-retry" });
            return root;
        }
    }
}
