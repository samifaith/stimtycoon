using System;
using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class EventSheetBinderTests
    {
        [Test]
        public void Constructor_RequiresCompleteNamedContract()
        {
            Assert.Throws<ArgumentNullException>(() => new EventSheetBinder(null));
            Assert.That(new EventSheetBinder(new VisualElement()).IsValid, Is.False);
            Assert.That(new EventSheetBinder(CreateRoot()).IsValid, Is.True);
        }

        [Test]
        public void Properties_UpdateCopyAndSemanticVisibility()
        {
            var root = CreateRoot();
            var binder = new EventSheetBinder(root)
            {
                CategoryText = "CAREER UPDATE",
                TitleText = "Promotion",
                BodyText = "Your work paid off.",
                ResultText = "Promoted",
                EffectsText = "Salary +$500",
                ContinueText = "Continue",
                ChoicesVisible = false,
                ResultVisible = true,
                EffectsVisible = true,
                ContinueVisible = true
            };

            Assert.That(root.Q<Label>("event-category").text, Is.EqualTo("CAREER UPDATE"));
            Assert.That(root.Q<Label>("event-title").text, Is.EqualTo("Promotion"));
            Assert.That(root.Q<Label>("event-body").text, Is.EqualTo("Your work paid off."));
            Assert.That(root.Q<Label>("result-text").text, Is.EqualTo("Promoted"));
            Assert.That(root.Q<Label>("result-effects").text, Is.EqualTo("Salary +$500"));
            Assert.That(root.Q("choices").ClassListContains("hidden"), Is.True);
            Assert.That(root.Q("result-card").ClassListContains("hidden"), Is.False);
            Assert.That(root.Q("result-effects").ClassListContains("hidden"), Is.False);
            Assert.That(binder.Continue.ClassListContains("hidden"), Is.False);
        }

        [Test]
        public void ChoiceHost_ClearsAndAcceptsControllerAuthoredActions()
        {
            var root = CreateRoot();
            var binder = new EventSheetBinder(root);
            var first = new Button { name = "first-choice" };
            var second = new Button { name = "second-choice" };

            binder.AddChoice(first);
            binder.ClearChoices();
            binder.AddChoice(second);

            var choices = root.Q<VisualElement>("choices");
            Assert.That(choices.childCount, Is.EqualTo(1));
            Assert.That(choices[0], Is.SameAs(second));
        }

        private static VisualElement CreateRoot()
        {
            var root = new VisualElement();
            root.Add(new Label { name = "event-category" });
            root.Add(new Label { name = "event-title" });
            root.Add(new Label { name = "event-body" });
            root.Add(new Label { name = "result-text" });
            root.Add(new Label { name = "result-effects" });
            root.Add(new VisualElement { name = "choices" });
            root.Add(new VisualElement { name = "result-card" });
            root.Add(new Button { name = "event-continue" });
            return root;
        }
    }
}
