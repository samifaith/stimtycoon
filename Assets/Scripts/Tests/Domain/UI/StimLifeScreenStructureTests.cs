using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class StimLifeScreenStructureTests
    {
        private const string PlayableLifePath = "Assets/UI/StimVerticalSlice.uxml";
        private const string PlayableScenePath = "Assets/Scenes/StimVerticalSlice.unity";
        private const string HeaderPath = "Assets/StimTycoon/UI/Components/AppHeader/AppHeader.uxml";
        private const string NavigationPath = "Assets/StimTycoon/UI/Components/BottomNavigation/BottomNavigation.uxml";
        private const string FeedRowPath = "Assets/StimTycoon/UI/Components/FeedRow/FeedRow.uxml";
        private const string AchievementRowPath = "Assets/StimTycoon/UI/Components/AchievementRow/AchievementRow.uxml";
        private const string ActionCardPath = "Assets/StimTycoon/UI/Components/ActionCard/ActionCard.uxml";
        private const string ThemePath = "Assets/UI/Styles/StimTheme.uss";
        private const string ShellPath = "Assets/UI/Styles/Shell.uss";
        private const string ComponentsPath = "Assets/UI/Styles/Components.uss";
        private const string DestinationsPath = "Assets/UI/Styles/Destinations.uss";
        private const string FrontendCanvasPath = "Assets/UI/Styles/FrontendCanvas.uss";
        private const string ControllerPath = "Assets/Scripts/Runtime/StimVerticalSliceController.cs";
        private const string ShellBinderPath = "Assets/Scripts/Runtime/StimShellBinder.cs";

        [Test]
        public void PlayableRoot_UsesOnlyCanonicalStylesheetEntryPoints()
        {
            var source = File.ReadAllText(PlayableLifePath);

            StringAssert.Contains("<Style src=\"Styles/StimTheme.uss\" />", source);
            StringAssert.Contains("<Style src=\"Styles/Shell.uss\" />", source);
            StringAssert.Contains("<Style src=\"Styles/Components.uss\" />", source);
            StringAssert.Contains("<Style src=\"Styles/Destinations.uss\" />", source);
            StringAssert.Contains("<Style src=\"Styles/FrontendCanvas.uss\" />", source);
            StringAssert.DoesNotContain("StimTycoonTheme.uss", source);
            StringAssert.DoesNotContain("StimVerticalSliceCozyCorporate.uss", source);
            Assert.That(CountOccurrences(source, "<Style src="), Is.EqualTo(5));
            Assert.That(File.Exists("Assets/UI/StimVerticalSlice.uss"), Is.False);
            Assert.That(File.Exists("Assets/UI/StimVerticalSliceCozyCorporate.uss"), Is.False);
            Assert.That(!Directory.Exists("Assets/StimTycoon/UI/Styles") ||
                        Directory.GetFiles("Assets/StimTycoon/UI/Styles").Length == 0, Is.True,
                "The unused prototype style system must not remain in the production migration path.");

            foreach (var stylesheetPath in new[] { ThemePath, ShellPath, ComponentsPath, DestinationsPath, FrontendCanvasPath })
            {
                var stylesheet = File.ReadAllText(stylesheetPath);
                StringAssert.DoesNotContain("@import", stylesheet,
                    $"{stylesheetPath} must be referenced directly and cannot restore an overlapping cascade.");
                StringAssert.DoesNotContain("StimVerticalSliceCozyCorporate.uss", stylesheet);
            }

            var shellSource = File.ReadAllText(ShellPath);
            Assert.That(CountSelectorOccurrences(shellSource, ".st-life-header"), Is.GreaterThanOrEqualTo(1));
            Assert.That(CountSelectorOccurrences(shellSource, ".st-time-dock"), Is.GreaterThanOrEqualTo(1));
            Assert.That(CountSelectorOccurrences(shellSource, ".st-life-bottom-nav"), Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void PlayableShell_FullyIntegratesAllThreeAssetKits()
        {
            var root = Clone(PlayableLifePath);
            var header = Clone(HeaderPath);

            Assert.That(root.Q("screen").ClassListContains("st-asset-kits-integrated"), Is.True);
            Assert.That(root.Q("life-feed-card").ClassListContains("stim-pack-panel"), Is.True);
            Assert.That(root.Q<Button>("advance-month").ClassListContains("stim-pack-foundation-button"), Is.True);
            Assert.That(root.Q<Button>("advance-year").ClassListContains("stim-pack-secondary-button"), Is.True);
            Assert.That(root.Q<Button>("manual-work-tap").ClassListContains("stim-pack-foundation-button"), Is.True);
            Assert.That(header.Q("app-header").ClassListContains("stim-pack-status-cluster"), Is.True);
            Assert.That(header.Q<Button>("add-cash").ClassListContains("stim-pack-secondary-button"), Is.True);

            var themeSource = File.ReadAllText(ThemePath);
            var shellSource = File.ReadAllText(ShellPath);
            var componentsSource = File.ReadAllText(ComponentsPath);
            var destinationsSource = File.ReadAllText(DestinationsPath);
            var productionStyles = themeSource + shellSource + componentsSource + destinationsSource;
            StringAssert.Contains("Assets/Skyden_Games/Free_Casual_GUI", productionStyles);
            StringAssert.Contains("Assets/Space_Exploration_GUI_Kit", productionStyles);
            StringAssert.Contains("Assets/Jelly_UI_Pack", productionStyles);
            StringAssert.DoesNotContain("/Pannel/", productionStyles,
                "Native-ratio panel artwork cannot be used as a flexible responsive surface.");
            StringAssert.Contains("Progress_Bar.svg.svg", productionStyles);
            StringAssert.Contains("Jelly_UI_Pack/Sprites/Icons/star_icon.png", productionStyles);
            StringAssert.Contains("progress_bg.png", themeSource);
            StringAssert.DoesNotContain("panel_status_score", productionStyles);
            StringAssert.DoesNotContain("panel_list_", productionStyles);
            StringAssert.Contains(".st-asset-kits-integrated .st-account-row", componentsSource);
            StringAssert.Contains(".st-asset-kits-integrated .st-stat-tile", componentsSource);
            StringAssert.Contains(".st-asset-kits-integrated .st-skill-row", destinationsSource);
            StringAssert.Contains(".st-asset-kits-integrated .st-summary-detail-row", destinationsSource);
            Assert.That(root.Query(className: "st-brand-space-icon").ToList(),
                Has.Count.GreaterThanOrEqualTo(8));
            Assert.That(root.Query(className: "st-brand-space-container").ToList(),
                Has.Count.EqualTo(6));
            Assert.That(root.Query(className: "st-jelly-result-mark").ToList(), Has.Count.EqualTo(2));
        }

        [Test]
        public void LiveBenchmark_UsesProductionComponentContracts()
        {
            var root = Clone(PlayableLifePath);
            var header = Clone(HeaderPath);
            var navigation = Clone(NavigationPath);

            Assert.That(header.Q("app-header").ClassListContains("st-component-app-header"), Is.True);
            Assert.That(navigation.Q("bottom-navigation").ClassListContains("st-component-bottom-nav"), Is.True);
            Assert.That(root.Q("age-progression").ClassListContains("st-component-card"), Is.True);
            Assert.That(root.Q("life-feed-card").ClassListContains("st-component-card"), Is.True);
            Assert.That(root.Q("player-overview").ClassListContains("st-component-card"), Is.True);
            Assert.That(root.Query(className: "st-component-page-heading").ToList(), Has.Count.EqualTo(6));

            var source = File.ReadAllText(PlayableLifePath);
            StringAssert.Contains("template=\"AppHeader\" class=\"st-shell-header-slot\"", source);
            StringAssert.Contains("template=\"BottomNavigation\" class=\"st-shell-nav-slot\"", source);
            StringAssert.DoesNotContain("\\nMoney", source);
            StringAssert.DoesNotContain("\\nAge", source);
        }

        [Test]
        public void BottomNavigation_UsesSpaceKitDestinationIcons()
        {
            var iconNames = new[]
            {
                "home-64.png", "book-64.png", "rocket-64.png",
                "buy-64.png", "heart-64.png", "trophy-64.png"
            };

            foreach (var iconName in iconNames)
                Assert.That(File.Exists(
                    $"Assets/Space_Exploration_GUI_Kit/Picto_Icons/Dark_Purple/{iconName}"),
                    Is.True, iconName);

            var components = File.ReadAllText(ShellPath);
            foreach (var iconName in iconNames)
                StringAssert.Contains(
                    $"Assets/Space_Exploration_GUI_Kit/Picto_Icons/Dark_Purple/{iconName}", components);
        }

        [Test]
        public void Navigation_PersistsDestinationScrollStateAcrossViewsAndLifeSummary()
        {
            var controller = File.ReadAllText(ControllerPath);

            StringAssert.Contains("Dictionary<StimDestination, Vector2> destinationScrollOffsets", controller);
            StringAssert.Contains("destinationScrollOffsets[activeDestination] = previousView.scrollOffset", controller);
            StringAssert.Contains("destinationScrollOffsets.TryGetValue(destination, out var savedOffset)", controller);
            StringAssert.Contains("selectedScroll.schedule.Execute(() => selectedScroll.scrollOffset = targetOffset)", controller);
            StringAssert.Contains("NavigateTo(activeDestination);", controller);
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
            Assert.That(row.Q<Label>(className: "st-feed-result-chip").text, Is.EqualTo("Smarts +8"));
            Assert.That(row.Q<Label>(className: "st-feed-timestamp").text, Is.EqualTo("Month 5"));
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
        public void RuntimeFactories_CloneUiBuilderAuthoredTemplates()
        {
            var feedTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FeedRowPath);
            var achievementTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AchievementRowPath);
            var actionTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ActionCardPath);
            Assert.That(feedTemplate, Is.Not.Null);
            Assert.That(achievementTemplate, Is.Not.Null);
            Assert.That(actionTemplate, Is.Not.Null);

            var feed = StimUiComponentFactory.CreateFeedRow(
                new StimTycoon.Saves.StimLifeFeedEntry
                {
                    category = "money", text = "Saved money. Cash +10.", age = 18, monthOfYear = 2
                },
                0,
                1,
                feedTemplate);
            var achievement = StimUiComponentFactory.CreateAchievementRow(
                "template-check", "★", "Template Check", "QA", "1 / 1", "$1", "CLAIM", true,
                () => { },
                template: achievementTemplate);
            var action = StimActionCardFactory.Create(
                new StimActionDefinition
                {
                    id = "education.template",
                    title = "Template Action",
                    description = "Authored in UI Builder",
                    state = StimActionState.Ready
                },
                () => { },
                actionTemplate);

            Assert.That(feed.Q<Label>("feed-row-title").text, Is.EqualTo("Saved money."));
            Assert.That(achievement.Q<Label>("achievement-row-title").text, Is.EqualTo("Template Check"));
            Assert.That(action.Q<Label>("action-card-title").text, Is.EqualTo("Template Action"));
        }

        [Test]
        public void UiBuilderHierarchy_MatchesRuntimeDestinationsAndPreservesAuthoredVisualSlots()
        {
            var root = Clone(PlayableLifePath);
            Assert.That(root.Q("education-card").parent.name, Is.EqualTo("education-destination-content"));
            Assert.That(root.Q("career-card").parent.name, Is.EqualTo("career-destination-content"));
            Assert.That(root.Q("achievements-card").parent.name, Is.EqualTo("goals-destination-content"));

            var controller = File.ReadAllText(ControllerPath);
            StringAssert.DoesNotContain("ConfigureDestinationContent", controller);
            StringAssert.Contains("if (slot.childCount > 0) return;", controller);
            StringAssert.DoesNotContain("slot.Clear();", controller);
        }

        [Test]
        public void PlayableScene_UsesInspectorAuthoredUiDocumentAndSingleInputSystemEventSystem()
        {
            var scene = SceneManager.GetSceneByPath(PlayableScenePath);
            var openedForTest = !scene.IsValid() || !scene.isLoaded;
            if (openedForTest) scene = EditorSceneManager.OpenScene(PlayableScenePath, OpenSceneMode.Additive);
            try
            {
                UIDocument document = null;
                StimVerticalSliceController controller = null;
                var eventSystemCount = 0;
                InputSystemUIInputModule inputModule = null;
                StandaloneInputModule legacyInputModule = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    document = document ?? root.GetComponentInChildren<UIDocument>(true);
                    controller = controller ?? root.GetComponentInChildren<StimVerticalSliceController>(true);
                    eventSystemCount += root.GetComponentsInChildren<EventSystem>(true).Length;
                    inputModule = inputModule ?? root.GetComponentInChildren<InputSystemUIInputModule>(true);
                    legacyInputModule = legacyInputModule ?? root.GetComponentInChildren<StandaloneInputModule>(true);
                }

                Assert.That(document, Is.Not.Null);
                Assert.That(document.panelSettings, Is.EqualTo(AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/StimPanelSettings.asset")));
                Assert.That(document.visualTreeAsset, Is.EqualTo(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PlayableLifePath)));
                Assert.That(eventSystemCount, Is.EqualTo(1));
                Assert.That(inputModule, Is.Not.Null);
                Assert.That(legacyInputModule, Is.Null);
                var serializedController = new SerializedObject(controller);
                Assert.That(serializedController.FindProperty("feedRowTemplate").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedController.FindProperty("achievementRowTemplate").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedController.FindProperty("actionCardTemplate").objectReferenceValue, Is.Not.Null);
            }
            finally
            {
                if (openedForTest) EditorSceneManager.CloseScene(scene, true);
            }
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
                "open-life-summary", "close-life-summary", "life-summary-view",
                "summary-stage-detail", "summary-calendar-detail", "summary-career-detail",
                "summary-health-value", "summary-happiness-value", "summary-smarts-value",
                "summary-looks-value", "summary-luck-value", "summary-health-fill",
                "summary-happiness-fill", "summary-smarts-fill", "summary-looks-fill", "summary-luck-fill",
                "age-progression", "age-stage-summary", "age-stage-0", "age-stage-1", "age-stage-2", "age-stage-3",
                "event-category", "event-title", "event-body",
                "study-session-sheet", "study-session-title", "study-session-description",
                "study-session-effects", "study-session-timing", "study-session-requirement",
                "study-session-cancel", "study-session-confirm",
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
                "education-catalog", "education-catalog-status", "education-catalog-list",
                "education-unavailable-copy", "career-context-copy", "career-path-preview",
                "education-destination-content", "career-destination-content", "goals-destination-content",
                "relationship-list-view", "relationship-list", "discover-compatible-person", "relationship-discovery-feedback",
                "relationship-detail-view", "relationship-back", "relationship-avatar", "relationship-name",
                "relationship-type", "relationship-strength", "relationship-fill", "relationship-genetics",
                "relationship-actions", "education-card", "education-stage", "learning-level", "learning-fill",
                "learning-progress", "education-actions", "career-card", "career-role", "career-salary",
                "career-next-step", "career-action-fill", "career-action-progress", "career-actions", "career-actions-card",
                "final-life-summary", "ending-name", "ending-status", "ending-summary", "ending-new-life",
                "achievements-card", "achievements-count", "achievements-list", "money-view", "net-worth-card",
                "manual-work-card", "manual-work-role", "manual-work-rate", "money-cash-value",
                "bank-tabs", "bank-tab-savings", "bank-tab-credit", "bank-tab-investing",
                "bank-panel-savings", "bank-panel-credit", "bank-panel-investing",
                "manual-work-tap", "manual-work-feedback", "savings-card", "savings-balance-value",
                "savings-available-value", "savings-deposit-mode", "savings-withdraw-mode",
                "savings-amount-input", "savings-transfer-feedback", "money-history-card",
                "money-transaction-history", "money-accounts-list", "cash-flow-card", "cash-flow-gross", "cash-flow-taxes",
                "cash-flow-expenses", "cash-flow-credit-interest", "cash-flow-savings-interest",
                "cash-flow-net", "savings-projection", "credit-repayment-card", "credit-balance-value",
                "credit-detail-value", "available-credit-value", "credit-repayment-input",
                "credit-repayment-feedback", "index-investment-card", "index-fund-value",
                "index-fund-contributions", "index-fund-performance",
                "index-investment-requirement", "index-investment-input", "index-investment-feedback",
                "home-card", "home-condition", "home-progress", "home-actions", "home-upgrade-feedback"
            };

            foreach (var elementName in requiredNames)
            {
                Assert.That(root.Q(elementName), Is.Not.Null, $"Playable Life UXML is missing '{elementName}'.");
            }
        }

        [Test]
        public void BindingManifest_GroupsEveryRequiredNameWithExclusiveOwnership()
        {
            var root = Clone(PlayableLifePath);
            var owners = new Dictionary<string, StimUiBindingOwner>();

            Assert.That(StimUiBindingManifest.Bindings.Keys, Is.EquivalentTo(new[]
            {
                StimUiBindingOwner.Shell,
                StimUiBindingOwner.Life,
                StimUiBindingOwner.Study,
                StimUiBindingOwner.Work,
                StimUiBindingOwner.Bank,
                StimUiBindingOwner.Social,
                StimUiBindingOwner.Goals,
                StimUiBindingOwner.Modal
            }));
            foreach (var group in StimUiBindingManifest.Bindings)
            {
                Assert.That(group.Value, Is.Not.Empty, $"{group.Key} must own at least one binding.");
                foreach (var name in group.Value)
                {
                    Assert.That(owners.ContainsKey(name), Is.False,
                        $"'{name}' is owned by both {owners.GetValueOrDefault(name)} and {group.Key}.");
                    owners[name] = group.Key;
                    Assert.That(root.Q(name), Is.Not.Null, $"{group.Key}/{name} is missing from playable UXML.");
                }
            }

            Assert.That(StimUiBindingManifest.TryValidate(root, out var error), Is.True, error);
            Assert.That(owners.Count, Is.GreaterThan(140));
        }

        [Test]
        public void PlayableLifeScreen_LeadsWithFeedAndKeepsAgeProgressionInSummary()
        {
            var root = Clone(PlayableLifePath);
            var lifeContent = root.Q(className: "st-life-content");
            var moneyContent = root.Q(className: "st-money-content");
            var ageProgression = root.Q("age-progression");
            var feedCard = root.Q("life-feed-card");
            var netWorthCard = root.Q("net-worth-card");

            var summaryContent = root.Q("life-summary-view").Q(className: "st-life-summary-content");

            Assert.That(lifeContent.ElementAt(0), Is.SameAs(feedCard));
            Assert.IsFalse(lifeContent.Contains(ageProgression));
            Assert.IsTrue(lifeContent.Contains(feedCard));
            Assert.IsTrue(summaryContent.Contains(ageProgression));
            Assert.IsFalse(lifeContent.Contains(netWorthCard));
            Assert.IsTrue(moneyContent.Contains(netWorthCard));
            Assert.That(root.Q("net-worth-trend"), Is.Null, "Net Worth must not show an uncomputed trend.");
            Assert.That(root.Q(className: "st-worth-chart"), Is.Null, "Net Worth must not show a decorative chart without history data.");
        }

        [Test]
        public void LifeFeed_UsesThePageScrollerInsteadOfACompetingNestedScroller()
        {
            var root = Clone(PlayableLifePath);
            var pageScroll = root.Q<ScrollView>("life-scroll");
            var feedViewport = root.Q("life-feed-scroll");

            Assert.That(pageScroll, Is.Not.Null);
            Assert.That(feedViewport, Is.Not.Null);
            Assert.That(feedViewport, Is.Not.InstanceOf<ScrollView>(),
                "The feed must flow inside the page scroller so mobile does not render two competing scrollbars.");
            Assert.That(pageScroll.Contains(feedViewport), Is.True);
        }

        [Test]
        public void CoreDestinations_FollowReferenceModuleOrder()
        {
            var source = File.ReadAllText(PlayableLifePath);
            AssertTokensInOrder(source,
                "name=\"life-feed-card\"", "name=\"player-overview\"", "name=\"focus-study\"");
            AssertTokensInOrder(source,
                "text=\"Learn and qualify\"", "name=\"education-empty-state\"", "name=\"education-destination-content\"");
            AssertTokensInOrder(source,
                "text=\"Build your working life\"", "name=\"career-context-copy\"",
                "name=\"career-destination-content\"", "name=\"career-path-preview\"", "name=\"career-actions-card\"");
            AssertTokensInOrder(source,
                "text=\"Earn and manage\"", "name=\"net-worth-card\"", "name=\"savings-card\"",
                "name=\"money-accounts-list\"", "name=\"money-history-card\"",
                "name=\"credit-repayment-card\"", "name=\"index-investment-card\"");
            AssertTokensInOrder(source,
                "text=\"Your people\"", "name=\"discover-compatible-person\"", "name=\"relationship-list\"");
            AssertTokensInOrder(source,
                "text=\"Shape this life\"", "name=\"goals-destination-content\"");
        }

        [Test]
        public void ProductionStylesheets_HaveExclusiveExactSelectorOwnership()
        {
            var ownerBySelector = new Dictionary<string, string>();
            foreach (var path in new[] { ThemePath, ShellPath, ComponentsPath, DestinationsPath, FrontendCanvasPath })
            {
                var source = Regex.Replace(File.ReadAllText(path), @"/\*[\s\S]*?\*/", string.Empty);
                foreach (Match match in Regex.Matches(source, @"([^{}]+)\{"))
                {
                    var selector = match.Groups[1].Value.Trim();
                    if (string.IsNullOrEmpty(selector)) continue;
                    Assert.That(ownerBySelector.ContainsKey(selector), Is.False,
                        $"Exact selector '{selector}' is owned by both {ownerBySelector.GetValueOrDefault(selector)} and {path}.");
                    ownerBySelector[selector] = path;
                }
            }

            Assert.That(ownerBySelector.Count, Is.GreaterThan(100));
        }

        [Test]
        public void PlayableController_UnbindsEveryPersistentButtonCallbackWhenDisabled()
        {
            var source = File.ReadAllText(ControllerPath);
            var shellBinderSource = File.ReadAllText(ShellBinderPath);
            var bindBody = ExtractMethodBody(source, "private void BindPersistentCallbacks()");
            var bindButtonBody = ExtractMethodBody(source, "private void BindPersistentButton(Button button, Action callback)");
            var unbindBody = ExtractMethodBody(source, "private void UnbindPersistentCallbacks()");
            var enableBody = ExtractMethodBody(source, "private void OnEnable()");
            var disableBody = ExtractMethodBody(source, "private void OnDisable()");

            Assert.That(CountOccurrences(bindBody, "BindPersistentButton("), Is.EqualTo(20));
            StringAssert.Contains("UnbindPersistentCallbacks();", bindBody);
            StringAssert.Contains("shellBinder?.BindActions(", bindBody);
            AssertTokensInOrder(bindButtonBody,
                "button.clicked -= callback;",
                "button.clicked += callback;",
                "persistentButtonBindings.Add");
            StringAssert.Contains("binding.button.clicked -= binding.callback;", unbindBody);
            StringAssert.Contains("persistentButtonBindings.Clear();", unbindBody);
            StringAssert.Contains("BindPersistentCallbacks();", enableBody);
            StringAssert.Contains("UnbindPersistentCallbacks();", disableBody);
            StringAssert.Contains("shellBinder?.Dispose();", disableBody);
            StringAssert.Contains("button.clicked -= callback;", shellBinderSource);
            StringAssert.Contains("button.clicked += callback;", shellBinderSource);
            StringAssert.Contains("binding.Button.clicked -= binding.Callback;", shellBinderSource);
            StringAssert.Contains("root.UnregisterCallback(geometryChanged);", shellBinderSource);
        }

        [Test]
        public void GameplayActionHandlers_PresentPendingEventsBeforeMutation()
        {
            var source = File.ReadAllText(ControllerPath);
            var guardedHandlers = new[]
            {
                "AdvanceMonth", "AdvanceYear", "PerformHomeAction", "PerformHomeUpgrade",
                "PerformSavingsTransfer", "PerformCreditRepayment", "PerformIndexInvestment",
                "StartTimedStudySession", "ClaimTimedStudySession", "PerformStudyTrackChoice",
                "PerformSchoolPathChoice", "PerformEducationAction", "PerformCareerAction",
                "PerformBusinessAction", "PerformActivity", "PerformManualWorkTap",
                "DiscoverCompatiblePerson", "PerformParentingAction", "PerformFamilyPlanning",
                "PerformRelationshipInteraction"
            };

            foreach (var handler in guardedHandlers)
            {
                Assert.That(Regex.IsMatch(source,
                        $@"private void {handler}\([^)]*\)\s*\{{\s*if \(PresentPendingEventIfAvailable\(\)\) return;"),
                    Is.True, $"{handler} must present a pending event before attempting mutation.");
            }
        }

        [Test]
        public void PlayableController_RegistersTheCanonicalLaunchCatalogInsteadOfADuplicateList()
        {
            var source = File.ReadAllText(ControllerPath);

            StringAssert.Contains("RepresentativeStimEvents.CreateLaunchAlphaCatalog()", source);
            StringAssert.DoesNotContain("catalog.Upsert(RepresentativeStimEvents.Create", source);
        }

        [Test]
        public void ProductionStylesheets_UseAspectFitOrCompleteNineSliceForVendorArtwork()
        {
            var checkedBindings = 0;
            var checkedSlicedBindings = 0;
            foreach (var path in new[] { ThemePath, ShellPath, ComponentsPath, DestinationsPath })
            {
                var source = File.ReadAllText(path);

                foreach (Match rule in Regex.Matches(source, @"([^{}]+)\{([^{}]*)\}",
                             RegexOptions.Singleline))
                {
                    var selector = rule.Groups[1].Value;
                    var declarations = rule.Groups[2].Value;
                    if (!declarations.Contains("background-image:") ||
                        !Regex.IsMatch(declarations,
                            @"Assets/(Skyden_Games|Space_Exploration_GUI_Kit|Jelly_UI_Pack)"))
                        continue;

                    checkedBindings++;
                    if (declarations.Contains("-unity-background-scale-mode: stretch-to-fill"))
                    {
                        checkedSlicedBindings++;
                        StringAssert.Contains("Free_Casual_GUI/Resource/Free_Casual_GUI/Buttons", declarations,
                            $"Only the Skyden vector controls may use calibrated nine-slicing: {selector.Trim()}.");
                        Assert.That(Regex.IsMatch(selector,
                                @"\.st-brand-skyden-(button|primary|secondary|neutral)"), Is.True,
                            $"Sliced artwork must be isolated behind an explicit Skyden control adapter: {selector.Trim()}.");
                        foreach (var property in new[]
                                 {
                                     "-unity-slice-left:", "-unity-slice-right:", "-unity-slice-top:",
                                     "-unity-slice-bottom:", "-unity-slice-scale:"
                                 })
                            StringAssert.Contains(property, declarations,
                                $"Nine-sliced binding '{selector.Trim()}' is missing {property}.");
                    }
                    else
                    {
                        StringAssert.Contains("-unity-background-scale-mode: scale-to-fit", declarations,
                            $"Unsliced vendor artwork in {path} must explicitly preserve its aspect ratio.");
                        Assert.That(Regex.IsMatch(selector,
                                @"(\.st-brand-space-icon|\.st-nav-icon|\.st-destination-icon-frame|" +
                                @"\.stim-pack-reward-icon|\.st-info-icon|\.st-jelly-result-mark|" +
                                @"\.st-brand-(skyden|jelly)-(progress|add)|" +
                                @"\.st-(stat|education|career|relationship|relationship-card|skill)-track|" +
                                @"\.st-xp-pill)"), Is.True,
                            $"Unsliced vendor artwork may only occupy an aspect-safe icon/progress slot; found '{selector.Trim()}'.");
                    }
                }
            }

            Assert.That(checkedBindings, Is.GreaterThan(20));
            Assert.That(checkedSlicedBindings, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void Typography_UsesBalooOnlyOnOptInDisplayAndBrandSelectors()
        {
            var theme = File.ReadAllText(ThemePath);
            var rootRule = Regex.Match(theme, @":root\s*\{([^}]*)\}", RegexOptions.Singleline).Groups[1].Value;
            StringAssert.DoesNotContain("-unity-font-definition", rootRule);

            var allStyles = string.Join("\n", new[]
            {
                File.ReadAllText(ShellPath), File.ReadAllText(ComponentsPath), File.ReadAllText(DestinationsPath)
            });
            StringAssert.Contains(".st-section-heading-title", allStyles);
            StringAssert.Contains(".st-component-page-heading .st-social-title", allStyles);
            StringAssert.DoesNotContain(".screen.st-life-screen {\n    -unity-font-definition", allStyles);
        }

        [Test]
        public void RelationshipAndAccountFactories_HandleLongAndMissingMetadata()
        {
            var relationship = StimUiComponentFactory.CreateRelationshipRow(
                "very-long-person-id", "Alexandria A Very Long Dynamic Name", "best_friend", 125, () => { });
            Assert.That(relationship.Q<Label>(className: "st-relationship-card-meta").text,
                Is.EqualTo("Best friend · Relationship 100 / 100"));
            Assert.That(relationship.Q(className: "st-relationship-card-track"), Is.Not.Null);
            Assert.That(relationship.tooltip, Does.Contain("100 out of 100"));

            var account = StimUiComponentFactory.CreateAccountRow(
                "cash", "", "Cash Wallet", "$1,234,567,890", null);
            Assert.That(account.name, Is.EqualTo("account-row-cash"));
            Assert.That(account.Q<Label>(className: "st-account-row-value").text, Is.EqualTo("$1,234,567,890"));
            Assert.That(account.tooltip, Does.Contain("Cash Wallet"));
        }

        [Test]
        public void PathAndStatFactories_ExposeRequirementsAndClampProgress()
        {
            var path = StimUiComponentFactory.CreatePathRow(
                "business", "B", "Start a Business", "Build a supported company.",
                "Professional 2", false);
            Assert.That(path.ClassListContains("locked"), Is.True);
            Assert.That(path.Q<Label>(className: "st-path-lock").text, Is.EqualTo("Professional 2"));

            var stat = StimUiComponentFactory.CreateStatRow("health", "H", "Health", 140);
            Assert.That(stat.Q<Label>(className: "st-stat-number").text, Is.EqualTo("100 / 100"));
            Assert.That(stat.tooltip, Does.Contain("100 out of 100"));
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
            Assert.That(header.Q<Button>("open-life-summary"), Is.Not.Null);
            Assert.That(header.Q<Button>("add-cash"), Is.Not.Null);
            Assert.That(theme, Is.Not.Null, "The Stim-owned vendor adapter theme must remain importable.");
            Assert.That(components, Is.Not.Null, "Shared Stim UI component styling must remain importable.");
            Assert.That(navigation.Query<Button>().ToList(), Has.Count.EqualTo(6));
            Assert.That(navigation.Q<Button>("nav-life"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-education"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-career"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-money"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-social"), Is.Not.Null);
            Assert.That(navigation.Q<Button>("nav-goals"), Is.Not.Null);
            var controllerSource = File.ReadAllText(ControllerPath);
            StringAssert.Contains("shellBinder?.BindActions(", controllerSource);
            var shellBinderSource = File.ReadAllText(ShellBinderPath);
            StringAssert.Contains("Bind(OpenLifeSummary, openLifeSummary);", shellBinderSource);
            StringAssert.Contains("Bind(AddCash, openMoney);", shellBinderSource);
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
        public void Header_UsesContainedFlexColumnsWithoutOverlayPositioning()
        {
            var shell = File.ReadAllText(ShellPath);

            StringAssert.Contains(".st-balance-pill {", shell);
            StringAssert.Contains("width: 196px;", shell);
            StringAssert.Contains("flex-shrink: 0;", shell);
            StringAssert.Contains("height: 18px;", shell);
            StringAssert.Contains("overflow: hidden;", shell);
            StringAssert.Contains(".st-balance-value {", shell);
            StringAssert.Contains(".st-balance-copy {", shell);
            StringAssert.Contains("text-overflow: ellipsis;", shell);
            StringAssert.DoesNotContain("overflow: visible;", ExtractRule(shell, ".st-balance-value"));
            StringAssert.DoesNotContain("overflow: visible;", ExtractRule(shell, ".st-header-net-worth"));
            StringAssert.DoesNotContain("position: absolute;\n    right: 10px;\n    top: 8px;", shell);
            StringAssert.DoesNotContain("padding-right: 158px;", shell);
            StringAssert.Contains(".st-compact-width .st-balance-pill", shell);
            StringAssert.Contains("width: 154px;", shell);
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

        private static int CountSelectorOccurrences(string stylesheet, string selector)
        {
            var count = 0;
            foreach (var line in stylesheet.Split('\n'))
            {
                if (line.Trim() == selector + " {") count++;
            }

            return count;
        }

        private static string ExtractRule(string stylesheet, string selector)
        {
            var start = stylesheet.IndexOf(selector + " {", System.StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            var end = stylesheet.IndexOf('}', start);
            return end < 0 ? stylesheet.Substring(start) : stylesheet.Substring(start, end - start + 1);
        }

        private static string ExtractMethodBody(string source, string signature)
        {
            var signatureIndex = source.IndexOf(signature, System.StringComparison.Ordinal);
            Assert.That(signatureIndex, Is.GreaterThanOrEqualTo(0), $"Could not find {signature}.");
            var openingBrace = source.IndexOf('{', signatureIndex);
            Assert.That(openingBrace, Is.GreaterThanOrEqualTo(0));
            var depth = 0;
            for (var index = openingBrace; index < source.Length; index++)
            {
                if (source[index] == '{') depth++;
                else if (source[index] == '}' && --depth == 0)
                    return source.Substring(openingBrace + 1, index - openingBrace - 1);
            }

            Assert.Fail($"Method body for {signature} was not balanced.");
            return string.Empty;
        }

        private static void AssertTokensInOrder(string source, params string[] tokens)
        {
            var previous = -1;
            foreach (var token in tokens)
            {
                var current = source.IndexOf(token, System.StringComparison.Ordinal);
                Assert.That(current, Is.GreaterThan(previous), $"Expected '{token}' after the prior module token.");
                previous = current;
            }
        }
    }
}
