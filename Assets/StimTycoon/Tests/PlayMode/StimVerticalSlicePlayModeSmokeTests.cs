using System.Collections;
using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.PlayMode
{
    [Category("PlayModeSmoke")]
    public sealed class StimVerticalSlicePlayModeSmokeTests
    {
        private const string SceneName = "StimVerticalSlice";

        [UnitySetUp]
        public IEnumerator LoadProductionScene()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, $"{SceneName} must remain enabled in Build Settings.");
            yield return load;
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProductionScene_BootsWithOneUiDocumentAndOneInputSystemEventSystem()
        {
            var documents = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            var inputModules = Object.FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None);

            Assert.That(documents, Has.Length.EqualTo(1));
            Assert.That(documents[0].panelSettings, Is.Not.Null);
            Assert.That(documents[0].visualTreeAsset, Is.Not.Null);
            Assert.That(eventSystems, Has.Length.EqualTo(1));
            Assert.That(inputModules, Has.Length.EqualTo(1));
            Assert.That(inputModules[0].gameObject, Is.SameAs(eventSystems[0].gameObject));
            Assert.That(Object.FindFirstObjectByType<StimVerticalSliceController>(), Is.Not.Null);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ProductionScene_ExposesTheCompleteNavigationAndOverlayContract()
        {
            var document = Object.FindFirstObjectByType<UIDocument>();
            Assert.That(document, Is.Not.Null);

            var root = document.rootVisualElement;
            Assert.That(root, Is.Not.Null);
            AssertNamed<Button>(root, "nav-life");
            AssertNamed<Button>(root, "nav-education");
            AssertNamed<Button>(root, "nav-career");
            AssertNamed<Button>(root, "nav-money");
            AssertNamed<Button>(root, "nav-social");
            AssertNamed<Button>(root, "nav-goals");
            AssertNamed<Button>(root, "advance-month");
            AssertNamed<Button>(root, "advance-year");
            AssertNamed<VisualElement>(root, "life-scroll");
            AssertNamed<VisualElement>(root, "education-view");
            AssertNamed<VisualElement>(root, "career-view");
            AssertNamed<VisualElement>(root, "money-view");
            AssertNamed<VisualElement>(root, "social-view");
            AssertNamed<VisualElement>(root, "goals-view");
            AssertNamed<VisualElement>(root, "event-sheet");
            AssertNamed<VisualElement>(root, "new-life-setup");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Controller_DisableEnableCycleRestoresLiveBindingsWithoutErrors()
        {
            var controller = Object.FindFirstObjectByType<StimVerticalSliceController>();
            Assert.That(controller, Is.Not.Null);

            controller.enabled = false;
            yield return null;
            controller.enabled = true;
            yield return null;

            Assert.That(controller.isActiveAndEnabled, Is.True);
            var document = controller.GetComponent<UIDocument>();
            Assert.That(document.rootVisualElement.Q<Button>("advance-month"), Is.Not.Null);
            Assert.That(document.rootVisualElement.Q<Button>("event-continue"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator CommercePresentationSlots_RemainDisabledAndOutsideFocusOrder()
        {
            var document = Object.FindFirstObjectByType<UIDocument>();
            Assert.That(document, Is.Not.Null);
            var root = document.rootVisualElement;
            var slotIds = new[]
            {
                "com.header.money_entry",
                "com.study.premium_module",
                "com.work.rewarded_module",
                "com.bank.premium_tools",
                "com.bank.rewarded_module",
                "com.social.premium_module",
                "com.goals.sponsored_challenge",
                "com.goals.season_preview",
                "com.goals.bonus_game_preview"
            };

            foreach (var slotId in slotIds)
            {
                var slot = root.Q<VisualElement>(slotId);
                Assert.That(slot, Is.Not.Null, $"Production UI is missing {slotId}.");
                Assert.That(slot.enabledSelf, Is.False, $"{slotId} must remain unavailable.");
                Assert.That(slot.focusable, Is.False, $"{slotId} must not enter focus order.");
                Assert.That(slot.ClassListContains("st-commerce-unavailable"), Is.True);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ProductionScene_ReflowsAcrossSupportedWidthAndTextScaleMatrix()
        {
            var document = Object.FindFirstObjectByType<UIDocument>();
            var controller = Object.FindFirstObjectByType<StimVerticalSliceController>();
            Assert.That(document, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);
            var root = document.rootVisualElement;
            var screen = root.Q<VisualElement>("screen");
            Assert.That(screen, Is.Not.Null);
            var widths = new[] { 320f, 390f, 430f, 768f };
            var textScales = new[] { 1f, 1.3f };
            var persistentTargets = new[]
            {
                "nav-life", "nav-education", "nav-career",
                "nav-money", "nav-social", "nav-goals"
            };

            foreach (var width in widths)
            foreach (var textScale in textScales)
            {
                root.style.width = width;
                StimVerticalSliceController.ApplyResponsiveLayout(root, width);
                controller.SetAccessibilityTextScale(textScale);
                yield return null;
                yield return null;

                Assert.That(root.ClassListContains("st-compact-width"), Is.EqualTo(width <= 360f),
                    $"Compact-width state was wrong at {width} points / {textScale:P0} text.");
                Assert.That(root.ClassListContains("st-large-text"), Is.EqualTo(textScale >= 1.3f),
                    $"Large-text state was wrong at {width} points / {textScale:P0} text.");

                var safeBounds = screen.worldBound;
                Assert.That(safeBounds.width, Is.GreaterThan(0f));
                foreach (var targetName in persistentTargets)
                {
                    var target = root.Q<Button>(targetName);
                    Assert.That(target, Is.Not.Null);
                    AssertTargetWithin(target, safeBounds, width, textScale);
                }

                AssertTargetWithin(root.Q<Button>("advance-month"), safeBounds, width, textScale);
                AssertTargetWithin(root.Q<Button>("advance-year"), safeBounds, width, textScale);

                var studySheet = root.Q<VisualElement>("study-session-sheet");
                var studyWasHidden = studySheet.ClassListContains("hidden");
                studySheet.RemoveFromClassList("hidden");
                yield return null;
                AssertTargetWithin(root.Q<Button>("study-session-cancel"), safeBounds, width, textScale);
                AssertTargetWithin(root.Q<Button>("study-session-confirm"), safeBounds, width, textScale);
                studySheet.EnableInClassList("hidden", studyWasHidden);

                var eventSheet = root.Q<VisualElement>("event-sheet");
                var eventWasHidden = eventSheet.ClassListContains("hidden");
                var eventContinue = root.Q<Button>("event-continue");
                var continueWasHidden = eventContinue.ClassListContains("hidden");
                eventSheet.RemoveFromClassList("hidden");
                eventContinue.RemoveFromClassList("hidden");
                yield return null;
                AssertTargetWithin(eventContinue, safeBounds, width, textScale);
                eventContinue.EnableInClassList("hidden", continueWasHidden);
                eventSheet.EnableInClassList("hidden", eventWasHidden);

                var newLifeSetup = root.Q<VisualElement>("new-life-setup");
                var newLifeWasHidden = newLifeSetup.ClassListContains("hidden");
                var cancelNewLife = root.Q<Button>("cancel-new-life");
                var cancelWasHidden = cancelNewLife.ClassListContains("hidden");
                newLifeSetup.RemoveFromClassList("hidden");
                cancelNewLife.RemoveFromClassList("hidden");
                yield return null;
                AssertTargetWithin(cancelNewLife, safeBounds, width, textScale);
                AssertTargetWithin(root.Q<Button>("create-new-life"), safeBounds, width, textScale);
                cancelNewLife.EnableInClassList("hidden", cancelWasHidden);
                newLifeSetup.EnableInClassList("hidden", newLifeWasHidden);
            }

            root.style.width = StyleKeyword.Auto;
            controller.SetAccessibilityTextScale(1f);
        }

        private static void AssertTargetWithin(
            Button target, Rect safeBounds, float width, float textScale)
        {
            Assert.That(target, Is.Not.Null);
            Assert.That(target.worldBound.height, Is.GreaterThanOrEqualTo(44f),
                $"{target.name} fell below 44 points at {width} / {textScale:P0} text.");
            Assert.That(target.worldBound.xMin, Is.GreaterThanOrEqualTo(safeBounds.xMin - 0.5f),
                $"{target.name} escaped the left safe bound at {width} / {textScale:P0} text.");
            Assert.That(target.worldBound.xMax, Is.LessThanOrEqualTo(safeBounds.xMax + 0.5f),
                $"{target.name} escaped the right safe bound at {width} / {textScale:P0} text.");
        }

        private static void AssertNamed<TElement>(VisualElement root, string name)
            where TElement : VisualElement
        {
            Assert.That(root.Q<TElement>(name), Is.Not.Null,
                $"Production UXML is missing required {typeof(TElement).Name} '{name}'.");
        }
    }
}
