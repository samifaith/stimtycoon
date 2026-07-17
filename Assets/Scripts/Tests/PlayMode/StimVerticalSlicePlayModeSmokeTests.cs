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

        private static void AssertNamed<TElement>(VisualElement root, string name)
            where TElement : VisualElement
        {
            Assert.That(root.Q<TElement>(name), Is.Not.Null,
                $"Production UXML is missing required {typeof(TElement).Name} '{name}'.");
        }
    }
}
