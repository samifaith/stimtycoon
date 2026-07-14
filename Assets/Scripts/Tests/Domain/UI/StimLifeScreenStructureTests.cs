using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class StimLifeScreenStructureTests
    {
        private const string PlayableLifePath = "Assets/UI/StimVerticalSlice.uxml";
        private const string HeaderPath = "Assets/StimTycoon/UI/Components/AppHeader/AppHeader.uxml";
        private const string NavigationPath = "Assets/StimTycoon/UI/Components/BottomNavigation/BottomNavigation.uxml";

        [Test]
        public void PlayableLifeScreen_ContainsEveryControllerBinding()
        {
            var root = Clone(PlayableLifePath);
            var requiredNames = new[]
            {
                "cash-value", "life-summary", "avatar-glyph", "event-category", "event-title", "event-body",
                "result-text", "result-effects", "life-feed-card", "life-feed-scroll", "life-feed-list", "overview-career", "overview-calendar",
                "health-value", "happiness-value", "smarts-value", "looks-value", "luck-value",
                "career-progress-value", "monthly-paycheck-value", "annual-salary-value", "net-worth-value",
                "choices", "result-card", "player-overview", "skills-card", "skills-list", "career-progress-fill",
                "event-sheet", "health-fill", "happiness-fill", "smarts-fill", "looks-fill", "luck-fill",
                "advance-month", "advance-year", "toggle-overview", "event-continue", "focus-study", "focus-workout",
                "focus-study-title", "focus-study-effect", "focus-workout-title", "focus-workout-effect",
                "context-activities",
                "open-new-life", "new-life-setup", "cancel-new-life", "continue-current-life",
                "create-new-life", "new-life-error", "social-view", "time-dock",
                "relationship-list-view", "relationship-list",
                "relationship-detail-view", "relationship-back", "relationship-avatar", "relationship-name",
                "relationship-type", "relationship-strength", "relationship-fill", "relationship-genetics",
                "relationship-actions", "education-card", "education-stage", "learning-level", "learning-fill",
                "learning-progress", "education-actions", "career-card", "career-role", "career-salary",
                "career-next-step", "career-action-fill", "career-action-progress", "career-actions",
                "final-life-summary", "ending-name", "ending-status", "ending-summary", "ending-new-life",
                "achievements-card", "achievements-count", "achievements-list", "money-view", "net-worth-card",
                "manual-work-role", "manual-work-rate", "money-cash-value",
                "manual-work-tap", "manual-work-feedback"
            };

            foreach (var elementName in requiredNames)
            {
                Assert.That(root.Q(elementName), Is.Not.Null, $"Playable Life UXML is missing '{elementName}'.");
            }
        }

        [Test]
        public void PlayableLifeScreen_PutsFeedFirstAndNetWorthInMoney()
        {
            var root = Clone(PlayableLifePath);
            var lifeContent = root.Q(className: "st-life-content");
            var moneyContent = root.Q(className: "st-money-content");
            var feedCard = root.Q("life-feed-card");
            var netWorthCard = root.Q("net-worth-card");

            Assert.That(lifeContent.ElementAt(0), Is.SameAs(feedCard));
            Assert.IsTrue(lifeContent.Contains(feedCard));
            Assert.IsFalse(lifeContent.Contains(netWorthCard));
            Assert.IsTrue(moneyContent.Contains(netWorthCard));
            Assert.That(root.Q("net-worth-trend"), Is.Null, "Net Worth must not show an uncomputed trend.");
            Assert.That(root.Q(className: "st-worth-chart"), Is.Null, "Net Worth must not show a decorative chart without history data.");
        }

        [TestCase(320f, true)]
        [TestCase(360f, true)]
        [TestCase(390f, false)]
        [TestCase(430f, false)]
        [TestCase(768f, false)]
        public void ResponsiveLayout_UsesCompactRulesAtNarrowWidths(float width, bool expectedCompact)
        {
            var root = new VisualElement();

            StimVerticalSliceController.ApplyResponsiveLayout(root, width);

            Assert.That(root.ClassListContains("st-compact-width"), Is.EqualTo(expectedCompact));
        }

        [TestCase(1f, false)]
        [TestCase(1.29f, false)]
        [TestCase(1.3f, true)]
        public void AccessibilityLayout_ReflowsAtSupportedLargeTextScale(
            float textScale,
            bool expectedLargeText)
        {
            var root = new VisualElement();

            StimVerticalSliceController.ApplyAccessibilityTextLayout(root, textScale);

            Assert.That(root.ClassListContains("st-large-text"), Is.EqualTo(expectedLargeText));
        }

        [Test]
        public void SharedShell_ProvidesFourNamedDestinationsAndHeaderState()
        {
            var header = Clone(HeaderPath);
            var navigation = Clone(NavigationPath);

            Assert.That(header.Q<Label>("life-summary"), Is.Not.Null);
            Assert.That(header.Q<Label>("career-progress-value"), Is.Not.Null);
            Assert.That(header.Q<Label>("cash-value"), Is.Not.Null);
            Assert.That(navigation.Query<Button>().ToList(), Has.Count.EqualTo(4));
            Assert.That(navigation.Q<Button>("nav-life"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-money"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-social"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-business"), Is.Not.Null);
        }

        [Test]
        public void PlayableLifeScreen_DefaultsToDashboardWithEventSheetHidden()
        {
            var root = Clone(PlayableLifePath);
            var eventSheet = root.Q<VisualElement>("event-sheet");
            var scroll = root.Q<ScrollView>("life-scroll");

            Assert.That(scroll, Is.Not.Null);
            Assert.That(eventSheet, Is.Not.Null);
            Assert.IsTrue(eventSheet.ClassListContains("hidden"));
            Assert.That(root.Q<Button>("advance-month"), Is.Not.Null);
            Assert.That(root.Q<Button>("advance-year"), Is.Not.Null);
            Assert.IsTrue(root.Q("social-view").ClassListContains("hidden"));
            Assert.IsFalse(root.Q("life-scroll").ClassListContains("hidden"));
            Assert.IsTrue(root.Q("final-life-summary").ClassListContains("hidden"));
            Assert.That(root.Q("age-fill"), Is.Null, "Age is shown in the header and should not use a stat progress bar.");
        }

        [Test]
        public void PlayableLifeScreen_KeepsAdvanceMonthOutsideScrollableContent()
        {
            var root = Clone(PlayableLifePath);
            var scroll = root.Q<ScrollView>("life-scroll");
            var advanceMonth = root.Q<Button>("advance-month");

            Assert.That(advanceMonth, Is.Not.Null);
            Assert.IsFalse(scroll.Contains(advanceMonth), "Advance Month must remain visible outside the Life ScrollView.");
        }

        [TestCase(-25f, 0f)]
        [TestCase(42f, 42f)]
        [TestCase(125f, 100f)]
        public void StatFillPercent_IsClampedToVisibleRange(float value, float expected)
        {
            Assert.That(StimVerticalSliceController.ClampFillPercent(value), Is.EqualTo(expected));
        }

        [Test]
        public void NewLifeSetup_OffersOnlyRandomizedStartAndDefaultsHidden()
        {
            var root = Clone(PlayableLifePath);
            var setup = root.Q<VisualElement>("new-life-setup");

            Assert.That(setup, Is.Not.Null);
            Assert.IsTrue(setup.ClassListContains("hidden"));
            Assert.That(setup.Query<TextField>().ToList(), Is.Empty);
            Assert.That(root.Q("country-usa"), Is.Null);
            Assert.That(root.Q("country-jamaica"), Is.Null);
            Assert.That(root.Q("background-working"), Is.Null);
            Assert.That(root.Q("background-middle"), Is.Null);
            Assert.That(root.Q("background-wealthy"), Is.Null);
            Assert.That(root.Q<Button>("continue-current-life").ClassListContains("hidden"), Is.True);
            Assert.That(root.Q<Button>("create-new-life").text, Does.Contain("START NEW GAME"));
        }

        private static TemplateContainer Clone(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Assert.That(asset, Is.Not.Null, $"Could not load UI asset at {path}.");
            return asset.CloneTree();
        }
    }
}
