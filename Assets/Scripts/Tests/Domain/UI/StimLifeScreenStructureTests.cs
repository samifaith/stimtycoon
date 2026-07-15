using System.IO;
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
        private const string ThemePath = "Assets/UI/Styles/StimTheme.uss";
        private const string ComponentsPath = "Assets/UI/Styles/Components.uss";

        [Test]
        public void PlayableRoot_UsesOnlyCanonicalStylesheetEntryPoints()
        {
            var source = File.ReadAllText(PlayableLifePath);

            StringAssert.Contains("<Style src=\"Styles/StimTheme.uss\" />", source);
            StringAssert.Contains("<Style src=\"Styles/Components.uss\" />", source);
            StringAssert.DoesNotContain("StimTycoonTheme.uss", source);
            StringAssert.DoesNotContain("StimVerticalSliceCozyCorporate.uss", source);
            Assert.That(CountOccurrences(source, "<Style src="), Is.EqualTo(2));

            var componentSource = File.ReadAllText(ComponentsPath);
            StringAssert.Contains("@import url(\"../StimVerticalSliceCozyCorporate.uss\")", componentSource);
        }

        [Test]
        public void PlayableShell_AppliesBoundedThemeAdapterSurfaces()
        {
            var root = Clone(PlayableLifePath);
            var header = Clone(HeaderPath);

            Assert.That(root.Q("life-feed-card").ClassListContains("stim-pack-panel"), Is.True);
            Assert.That(root.Q<Button>("advance-month").ClassListContains("stim-pack-foundation-button"), Is.True);
            Assert.That(root.Q<Button>("advance-year").ClassListContains("stim-pack-secondary-button"), Is.True);
            Assert.That(root.Q<Button>("manual-work-tap").ClassListContains("stim-pack-foundation-button"), Is.True);
            Assert.That(header.Q("app-header").ClassListContains("stim-pack-status-cluster"), Is.True);
            Assert.That(header.Q<Button>("add-cash").ClassListContains("stim-pack-secondary-button"), Is.True);

            var themeSource = File.ReadAllText(ThemePath);
            StringAssert.Contains("Assets/Jelly_UI_Pack", themeSource);
        }

        [Test]
        public void BottomNavigation_UsesLicensedFunctionalSvgIcons()
        {
            var iconNames = new[]
            {
                "house.svg", "graduation-cap.svg", "briefcase-business.svg",
                "landmark.svg", "users.svg", "star.svg"
            };

            foreach (var iconName in iconNames)
                Assert.That(File.Exists($"Assets/UI/Icons/Lucide/{iconName}"), Is.True, iconName);

            Assert.That(File.Exists("Assets/UI/Icons/Lucide/LICENSE.txt"), Is.True);
            var components = File.ReadAllText(ComponentsPath);
            foreach (var iconName in iconNames)
                StringAssert.Contains($"Assets/UI/Icons/Lucide/{iconName}", components);
        }

        [Test]
        public void FeedRowFactory_CreatesCompactSemanticRow()
        {
            var entry = new StimTycoon.Saves.StimLifeFeedEntry
            {
                category = "education",
                text = "Studied hard. Smarts +8.",
                age = 16,
                monthOfYear = 5
            };

            var row = StimUiComponentFactory.CreateFeedRow(entry, 0, 4);

            Assert.That(row.ClassListContains("st-feed-entry"), Is.True);
            Assert.That(row.Q(className: "st-feed-dot"), Is.Not.Null);
            Assert.That(row.Q<Label>(className: "st-feed-title").text, Is.EqualTo("Studied hard."));
            Assert.That(row.Q<Label>(className: "st-feed-icon").text, Is.EqualTo("📚"));
            Assert.That(row.Q<Label>(className: "st-feed-result-chip").text, Is.EqualTo("Education"));
            Assert.That(row.tooltip, Does.Contain("Item 1 of 4").And.Contain("Age 16"));
        }

        [Test]
        public void AchievementRowFactory_CreatesWireframeDensityAndAction()
        {
            var row = StimUiComponentFactory.CreateAchievementRow(
                "first-paycheck", "★", "First Paycheck", "Career", "0 / 1", "$250", "GO", true,
                () => { });

            Assert.That(row.ClassListContains("st-achievement-row"), Is.True);
            Assert.That(row.Q<Label>(className: "st-achievement-name").text, Is.EqualTo("First Paycheck"));
            Assert.That(row.Q<Label>(className: "st-achievement-category").text, Is.EqualTo("Career"));
            Assert.That(row.Q<Label>(className: "st-achievement-reward").text, Is.EqualTo("$250"));
            Assert.That(row.Q<Button>().enabledSelf, Is.True);
        }

        [Test]
        public void VisualPlaceholder_RequiresStableAccessibleMetadataAndRendersFallback()
        {
            Assert.Throws<System.ArgumentException>(() => StimVisualPlaceholderFactory.Create(
                new StimVisualPlaceholderDefinition { visualId = "event.school.exam", decorative = false }));

            var definition = new StimVisualPlaceholderDefinition
            {
                visualId = "event.school.exam",
                role = StimVisualRole.Hero,
                aspectRatio = "16:9",
                accessibilityLabelKey = "visual.event.school.exam",
                fallbackGlyph = "A+",
                themeToken = "education"
            };
            var visual = StimVisualPlaceholderFactory.Create(definition);

            Assert.That(visual.name, Is.EqualTo("visual-event-school-exam"));
            Assert.That(visual.ClassListContains("st-visual-placeholder"), Is.True);
            Assert.That(visual.ClassListContains("role-hero"), Is.True);
            Assert.That(visual.tooltip, Is.EqualTo("visual.event.school.exam"));
            Assert.That(visual.Q<Label>(className: "st-visual-placeholder-glyph").text, Is.EqualTo("A+"));
            Assert.That(visual.userData, Is.SameAs(definition));
        }

        [Test]
        public void PlayableLifeScreen_ContainsEveryControllerBinding()
        {
            var root = Clone(PlayableLifePath);
            var requiredNames = new[]
            {
                "cash-value", "life-summary", "calendar-summary", "header-net-worth-value", "avatar-glyph",
                "age-progression", "age-stage-summary", "age-stage-0", "age-stage-1", "age-stage-2", "age-stage-3",
                "event-category", "event-title", "event-body",
                "result-text", "result-effects", "life-feed-card", "life-feed-scroll", "life-feed-list", "overview-career", "overview-calendar",
                "health-value", "happiness-value", "smarts-value", "looks-value", "luck-value",
                "career-progress-value", "monthly-paycheck-value", "annual-salary-value", "net-worth-value",
                "choices", "result-card", "player-overview", "skills-card", "skills-list", "career-progress-fill",
                "event-sheet", "health-fill", "happiness-fill", "smarts-fill", "looks-fill", "luck-fill",
                "advance-month", "advance-year", "toggle-overview", "event-continue", "focus-study", "focus-workout",
                "focus-study-title", "focus-study-effect", "focus-workout-title", "focus-workout-effect",
                "context-activities",
                "event-visual-slot", "home-visual-slot", "education-visual-slot", "relationship-visual-slot",
                "open-new-life", "new-life-setup", "cancel-new-life", "continue-current-life",
                "create-new-life", "new-life-error", "social-view", "time-dock",
                "education-view", "career-view", "goals-view", "education-empty-state", "career-empty-state",
                "education-destination-content", "career-destination-content", "goals-destination-content",
                "relationship-list-view", "relationship-list", "discover-compatible-person", "relationship-discovery-feedback",
                "relationship-detail-view", "relationship-back", "relationship-avatar", "relationship-name",
                "relationship-type", "relationship-strength", "relationship-fill", "relationship-genetics",
                "relationship-actions", "education-card", "education-stage", "learning-level", "learning-fill",
                "learning-progress", "education-actions", "career-card", "career-role", "career-salary",
                "career-next-step", "career-action-fill", "career-action-progress", "career-actions",
                "final-life-summary", "ending-name", "ending-status", "ending-summary", "ending-new-life",
                "achievements-card", "achievements-count", "achievements-list", "money-view", "net-worth-card",
                "manual-work-role", "manual-work-rate", "money-cash-value",
                "manual-work-tap", "manual-work-feedback", "savings-card", "savings-balance-value",
                "savings-available-value", "savings-deposit-mode", "savings-withdraw-mode",
                "savings-amount-input", "savings-transfer-feedback", "money-history-card",
                "money-transaction-history", "cash-flow-card", "cash-flow-gross", "cash-flow-taxes",
                "cash-flow-expenses", "cash-flow-credit-interest", "cash-flow-savings-interest",
                "cash-flow-net", "savings-projection", "credit-repayment-card", "credit-balance-value",
                "credit-detail-value", "available-credit-value", "credit-repayment-input",
                "credit-repayment-feedback", "index-investment-card", "index-fund-value",
                "index-investment-requirement", "index-investment-input", "index-investment-feedback",
                "home-card", "home-condition", "home-progress", "home-actions", "home-upgrade-feedback"
            };

            foreach (var elementName in requiredNames)
            {
                Assert.That(root.Q(elementName), Is.Not.Null, $"Playable Life UXML is missing '{elementName}'.");
            }
        }

        [Test]
        public void PlayableLifeScreen_PutsAgeProgressionThenFeedAndNetWorthInMoney()
        {
            var root = Clone(PlayableLifePath);
            var lifeContent = root.Q(className: "st-life-content");
            var moneyContent = root.Q(className: "st-money-content");
            var ageProgression = root.Q("age-progression");
            var feedCard = root.Q("life-feed-card");
            var netWorthCard = root.Q("net-worth-card");

            Assert.That(lifeContent.ElementAt(0), Is.SameAs(ageProgression));
            Assert.That(lifeContent.ElementAt(1), Is.SameAs(feedCard));
            Assert.IsTrue(lifeContent.Contains(ageProgression));
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
        [TestCase(402f, false)]
        [TestCase(440f, false)]
        public void ResponsiveLayout_UsesCompactRulesAtNarrowWidths(float width, bool expectedCompact)
        {
            var root = new VisualElement();

            StimVerticalSliceController.ApplyResponsiveLayout(root, width);

            Assert.That(root.ClassListContains("st-compact-width"), Is.EqualTo(expectedCompact));
            var insets = StimVerticalSliceController.CalculateSafeAreaInsets(
                390f, 844f, new UnityEngine.Rect(0f, 34f, 390f, 763f), 390f, 844f);
            Assert.That(insets, Is.EqualTo(new UnityEngine.Vector4(0f, 47f, 0f, 34f)));
        }

        [TestCase(1206f, 2622f, 402f, 874f)]
        [TestCase(1320f, 2868f, 440f, 956f)]
        public void IPhone17Profiles_UseConservativeDynamicIslandSafeArea(
            float physicalWidth, float physicalHeight, float panelWidth, float panelHeight)
        {
            var safeArea = new UnityEngine.Rect(0f, 102f, physicalWidth, physicalHeight - 288f);

            var insets = StimVerticalSliceController.CalculateSafeAreaInsets(
                physicalWidth, physicalHeight, safeArea, panelWidth, panelHeight);

            Assert.That(insets.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(insets.y, Is.EqualTo(62f).Within(0.01f));
            Assert.That(insets.z, Is.EqualTo(0f).Within(0.01f));
            Assert.That(insets.w, Is.EqualTo(34f).Within(0.01f));
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
        public void SharedShell_ProvidesSixNamedDestinationsAndHeaderState()
        {
            var header = Clone(HeaderPath);
            var navigation = Clone(NavigationPath);
            var theme = AssetDatabase.LoadAssetAtPath<StyleSheet>(ThemePath);
            var components = AssetDatabase.LoadAssetAtPath<StyleSheet>(ComponentsPath);

            Assert.That(header.Q<Label>("life-summary"), Is.Not.Null);
            Assert.That(header.Q<Label>("calendar-summary"), Is.Not.Null);
            Assert.That(header.Q<Label>("career-progress-value"), Is.Not.Null);
            Assert.That(header.Q<Label>("cash-value"), Is.Not.Null);
            Assert.That(header.Q<Label>("header-net-worth-value"), Is.Not.Null);
            Assert.That(theme, Is.Not.Null, "The Stim-owned vendor adapter theme must remain importable.");
            Assert.That(components, Is.Not.Null, "Shared Stim UI component styling must remain importable.");
            Assert.That(navigation.Query<Button>().ToList(), Has.Count.EqualTo(6));
            Assert.That(navigation.Q<Button>("nav-life"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-education"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-career"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-money"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-social"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-goals"), Is.Not.Null);
            foreach (var button in navigation.Query<Button>().ToList())
            {
                Assert.That(button.ClassListContains("stim-pack-interaction-pop"), Is.True,
                    $"{button.name} must retain shared pressed interaction feedback.");
                Assert.That(button.Q<VisualElement>(className: "st-nav-icon"), Is.Not.Null,
                    $"{button.name} must use the compact icon-over-label navigation pattern.");
                Assert.That(button.Q<Label>(className: "st-nav-label"), Is.Not.Null,
                    $"{button.name} must expose a readable navigation label.");
            }
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

        private static int CountOccurrences(string value, string token)
        {
            var count = 0;
            var offset = 0;
            while ((offset = value.IndexOf(token, offset, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }

            return count;
        }
    }
}
