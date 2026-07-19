using System;
using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class FinalLifeBinderTests
    {
        [Test]
        public void Constructor_RequiresCompleteNamedContract()
        {
            Assert.Throws<ArgumentNullException>(() => new FinalLifeBinder(null));
            Assert.That(new FinalLifeBinder(new VisualElement()).IsValid, Is.False);
            Assert.That(new FinalLifeBinder(CreateRoot()).IsValid, Is.True);
        }

        [Test]
        public void Render_BindsControllerAuthoredEndingCopy()
        {
            var root = CreateRoot();
            var binder = new FinalLifeBinder(root);

            binder.Render("Avery Morgan", "Retired at age 68", "A meaningful life remembered.");

            Assert.That(root.Q<Label>("ending-name").text, Is.EqualTo("Avery Morgan"));
            Assert.That(root.Q<Label>("ending-status").text, Is.EqualTo("Retired at age 68"));
            Assert.That(root.Q<Label>("ending-summary").text, Is.EqualTo("A meaningful life remembered."));
            Assert.That(binder.StartNewLife.name, Is.EqualTo("ending-new-life"));
        }

        private static VisualElement CreateRoot()
        {
            var root = new VisualElement();
            root.Add(new Label { name = "ending-name" });
            root.Add(new Label { name = "ending-status" });
            root.Add(new Label { name = "ending-summary" });
            root.Add(new Button { name = "ending-new-life" });
            return root;
        }
    }
}
