using System;
using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class StimNewLifeBinderTests
    {
        [Test]
        public void Constructor_RequiresCompleteNamedContract()
        {
            Assert.Throws<ArgumentNullException>(() => new StimNewLifeBinder(null));
            Assert.That(new StimNewLifeBinder(new VisualElement()).IsValid, Is.False);
            Assert.That(new StimNewLifeBinder(CreateRoot()).IsValid, Is.True);
        }

        [Test]
        public void Configure_ControlsOptionalActionsAndCurrentLifeLabel()
        {
            var root = CreateRoot();
            var binder = new StimNewLifeBinder(root);

            binder.Configure(true, false, "Avery");

            Assert.That(binder.Continue.ClassListContains("hidden"), Is.False);
            Assert.That(binder.Continue.text, Is.EqualTo("CONTINUE AVERY'S LIFE  ›"));
            Assert.That(binder.Cancel.ClassListContains("hidden"), Is.True);

            binder.Configure(false, true, null);
            Assert.That(binder.Continue.ClassListContains("hidden"), Is.True);
            Assert.That(binder.Cancel.ClassListContains("hidden"), Is.False);
        }

        [Test]
        public void ConfigureAndShowError_ResetThenPresentFeedback()
        {
            var root = CreateRoot();
            var binder = new StimNewLifeBinder(root);
            var error = root.Q<Label>("new-life-error");

            binder.ShowError("Save unavailable");
            Assert.That(error.text, Is.EqualTo("Save unavailable Try again."));
            Assert.That(error.ClassListContains("hidden"), Is.False);

            binder.Configure(false, false, null);
            Assert.That(error.text, Is.Empty);
            Assert.That(error.ClassListContains("hidden"), Is.True);
        }

        private static VisualElement CreateRoot()
        {
            var root = new VisualElement();
            var error = new Label { name = "new-life-error" };
            error.AddToClassList("hidden");
            root.Add(error);
            root.Add(new Button { name = "cancel-new-life" });
            root.Add(new Button { name = "continue-current-life" });
            root.Add(new Button { name = "create-new-life" });
            return root;
        }
    }
}
