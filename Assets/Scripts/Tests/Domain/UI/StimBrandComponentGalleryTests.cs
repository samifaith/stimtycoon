using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.Domain.UI
{
    public sealed class StimBrandComponentGalleryTests
    {
        private const string GalleryPath = "Assets/StimTycoon/UI/Screens/ComponentGallery/ComponentGallery.uxml";
        private const string GalleryStylesPath = "Assets/StimTycoon/UI/Screens/ComponentGallery/ComponentGallery.uss";
        private const string ThemePath = "Assets/UI/Styles/StimTheme.uss";

        private static readonly string[] CanonicalComponentPaths =
        {
            "Assets/StimTycoon/UI/Components/SectionHeader/SectionHeader.uxml",
            "Assets/StimTycoon/UI/Components/FeedRow/FeedRow.uxml",
            "Assets/StimTycoon/UI/Components/StatTile/StatTile.uxml",
            "Assets/StimTycoon/UI/Components/AchievementRow/AchievementRow.uxml",
            "Assets/StimTycoon/UI/Components/ActionCard/ActionCard.uxml",
            "Assets/StimTycoon/UI/Components/InfoBanner/InfoBanner.uxml"
        };

        [Test]
        public void Gallery_ExposesAReviewSectionForEveryPack()
        {
            var root = CloneGallery();

            Assert.That(root.Q("skyden-section"), Is.Not.Null);
            Assert.That(root.Q("space-section"), Is.Not.Null);
            Assert.That(root.Q("jelly-section"), Is.Not.Null);
            Assert.That(root.Q("mixed-section"), Is.Not.Null);
        }

        [Test]
        public void Gallery_ExercisesInteractiveAndDisabledBrandStates()
        {
            var root = CloneGallery();
            var skydenPrimary = root.Q<Button>("skyden-primary");
            var skydenDisabled = root.Q<Button>("skyden-disabled");
            var jellyClaim = root.Q<Button>("jelly-claim");

            Assert.That(skydenPrimary.ClassListContains("st-brand-skyden-button"), Is.True);
            Assert.That(skydenPrimary.enabledSelf, Is.True);
            Assert.That(skydenDisabled.ClassListContains("st-brand-skyden-button"), Is.True);
            Assert.That(skydenDisabled.enabledSelf, Is.False);
            Assert.That(jellyClaim.ClassListContains("st-brand-jelly-reward"), Is.True);
            Assert.That(jellyClaim.ClassListContains("stim-pack-interaction-pop"), Is.True);
        }

        [Test]
        public void Gallery_UsesOnlyResponsiveSafeVendorBindings()
        {
            var theme = File.ReadAllText(ThemePath);
            var galleryStyles = File.ReadAllText(GalleryStylesPath);

            StringAssert.Contains("Free_Casual_GUI/Resource/Free_Casual_GUI/Other/Progress_Bar.svg.svg", theme);
            StringAssert.Contains("Free_Casual_GUI/Resource/Free_Casual_GUI/Buttons/button_plain_blugreen.svg", theme);
            StringAssert.Contains("-unity-slice-left: 55", theme);
            StringAssert.Contains("-unity-slice-scale: 0.4px", theme);
            StringAssert.Contains("Jelly_UI_Pack/Sprites/Icons/star_icon.png", theme);
            StringAssert.Contains("Space_Exploration_GUI_Kit/Picto_Icons/Dark_Purple", theme);
            StringAssert.DoesNotContain("Space_Exploration_GUI_Kit/Containers", theme);
            StringAssert.DoesNotContain("Jelly_UI_Pack/Sprites/Button", theme);
            StringAssert.Contains(".st-gallery.st-compact-width", galleryStyles);
        }

        [Test]
        public void PhaseTwoComponents_ExposeStableReusableContracts()
        {
            var requiredRoots = new[]
            {
                "section-header", "feed-row", "stat-tile", "achievement-row", "action-card", "info-banner"
            };

            for (var index = 0; index < CanonicalComponentPaths.Length; index++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CanonicalComponentPaths[index]);
                Assert.That(asset, Is.Not.Null, CanonicalComponentPaths[index]);
                var root = asset.CloneTree();
                Assert.That(root.Q(requiredRoots[index]), Is.Not.Null,
                    $"{CanonicalComponentPaths[index]} must retain the stable '{requiredRoots[index]}' root name.");
            }

            var actionCard = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CanonicalComponentPaths[4]).CloneTree();
            Assert.That(actionCard.Q<Button>("action-card-commit").ClassListContains("stim-pack-interaction-pop"), Is.True);
            Assert.That(actionCard.Q("action-card").ClassListContains("st-brand-skyden-panel"), Is.True);
            Assert.That(actionCard.Q<Button>("action-card-commit")
                .ClassListContains("st-brand-skyden-primary"), Is.True);
            var achievement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CanonicalComponentPaths[3]).CloneTree();
            Assert.That(achievement.Q<Button>("achievement-row-action").enabledSelf, Is.True);
            Assert.That(achievement.Q<Button>("achievement-row-action")
                .ClassListContains("st-brand-jelly-claim"), Is.True);
            var info = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CanonicalComponentPaths[5]).CloneTree();
            Assert.That(info.Q("info-banner-icon").ClassListContains("st-brand-space-icon"), Is.True);
        }

        [Test]
        public void CanonicalComponents_DeclareProductionAssetKitContracts()
        {
            var requiredBindings = new[]
            {
                ("Assets/StimTycoon/UI/Components/BaseCard/BaseCard.uxml", "st-brand-skyden-panel"),
                ("Assets/StimTycoon/UI/Components/FeedRow/FeedRow.uxml", "st-brand-skyden-list"),
                ("Assets/StimTycoon/UI/Components/StatTile/StatTile.uxml", "st-brand-skyden-progress"),
                ("Assets/StimTycoon/UI/Components/ActionCard/ActionCard.uxml", "st-brand-skyden-primary"),
                ("Assets/StimTycoon/UI/Components/InfoBanner/InfoBanner.uxml", "st-brand-space-icon"),
                ("Assets/StimTycoon/UI/Components/AchievementRow/AchievementRow.uxml", "st-brand-jelly-claim"),
                ("Assets/StimTycoon/UI/Components/BottomNavigation/BottomNavigation.uxml", "st-brand-space-icon")
            };

            foreach (var binding in requiredBindings)
                StringAssert.Contains(binding.Item2, File.ReadAllText(binding.Item1), binding.Item1);
        }

        private static TemplateContainer CloneGallery()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GalleryPath);
            Assert.That(asset, Is.Not.Null, $"Could not load component gallery at {GalleryPath}.");
            return asset.CloneTree();
        }
    }
}
