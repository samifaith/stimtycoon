using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using StimTycoon.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace StimTycoon.Tests.PlayMode
{
    [Category("VisualCapture")]
    public sealed class StimM13VisualCaptureTests
    {
        private const string SceneName = "StimVerticalSlice";

        private static readonly CaptureProfile[] Profiles =
        {
            new CaptureProfile(320, 693),
            new CaptureProfile(390, 844),
            new CaptureProfile(430, 932),
            new CaptureProfile(768, 1024)
        };

        private static readonly DestinationCapture[] Destinations =
        {
            new DestinationCapture("life", "nav-life", "life-scroll"),
            new DestinationCapture("study", "nav-education", "education-view"),
            new DestinationCapture("work", "nav-career", "career-view"),
            new DestinationCapture("bank", "nav-money", "money-view"),
            new DestinationCapture("social", "nav-social", "social-view"),
            new DestinationCapture("goals", "nav-goals", "goals-view")
        };

        [UnityTest]
        public IEnumerator CaptureSixDestinationsAcrossM13Matrix()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, $"{SceneName} must remain enabled in Build Settings.");
            yield return load;
            yield return null;

            var document = Object.FindFirstObjectByType<UIDocument>();
            var controller = Object.FindFirstObjectByType<StimVerticalSliceController>();
            Assert.That(document, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);

            var root = document.rootVisualElement;
            var panelSettings = document.panelSettings;
            Assert.That(root, Is.Not.Null);
            Assert.That(panelSettings, Is.Not.Null);

            var outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "Artifacts", "M13Visual"));
            Directory.CreateDirectory(outputDirectory);
            var evidence = new List<string>();
            var originalTarget = panelSettings.targetTexture;
            var originalScaleMode = panelSettings.scaleMode;
            var originalScale = panelSettings.scale;

            try
            {
                panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
                panelSettings.scale = 1f;
                foreach (var profile in Profiles)
                foreach (var textScale in new[] { 1f, 1.3f })
                {
                    var renderTexture = new RenderTexture(
                        profile.Width, profile.Height, 24, RenderTextureFormat.ARGB32)
                    {
                        name = $"M13-{profile.Width}x{profile.Height}-{TextScaleLabel(textScale)}",
                        antiAliasing = 1
                    };
                    Assert.That(renderTexture.Create(), Is.True,
                        "M13 visual capture requires a graphics device; do not run the visual mode with -nographics.");

                    panelSettings.targetTexture = renderTexture;
                    var reload = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
                    Assert.That(reload, Is.Not.Null);
                    yield return reload;
                    yield return null;
                    yield return null;
                    document = Object.FindFirstObjectByType<UIDocument>();
                    controller = Object.FindFirstObjectByType<StimVerticalSliceController>();
                    Assert.That(document, Is.Not.Null);
                    Assert.That(controller, Is.Not.Null);
                    root = document.rootVisualElement;
                    Assert.That(root, Is.Not.Null);
                    StimVerticalSliceController.ApplyResponsiveLayout(root, profile.Width);
                    controller.SetAccessibilityTextScale(textScale);
                    for (var frame = 0; frame < 5; frame++) yield return null;
                    root.MarkDirtyRepaint();
                    yield return null;

                    foreach (var destination in Destinations)
                    {
                        HideBlockingOverlays(root);
                        ShowDestination(root, destination);
                        yield return null;
                        HideBlockingOverlays(root);
                        ClearRenderTexture(renderTexture);
                        root.MarkDirtyRepaint();
                        yield return null;

                        Assert.That(root.Q<Button>(destination.NavigationName).ClassListContains("active"), Is.True,
                            $"{destination.Name} navigation did not activate for capture.");
                        Assert.That(root.Q<VisualElement>(destination.ViewName).ClassListContains("hidden"), Is.False,
                            $"{destination.Name} view was hidden during capture.");

                        var fileName = $"{profile.Width}x{profile.Height}-{TextScaleLabel(textScale)}-{destination.Name}.png";
                        var path = Path.Combine(outputDirectory, fileName);
                        WriteRenderTexturePng(renderTexture, path);
                        evidence.Add(fileName);
                    }

                    panelSettings.targetTexture = originalTarget;
                    yield return null;
                    yield return null;
                    renderTexture.Release();
                    Object.Destroy(renderTexture);
                }

                WriteReviewManifest(outputDirectory, evidence);
                Assert.That(evidence, Has.Count.EqualTo(Profiles.Length * 2 * Destinations.Length));
            }
            finally
            {
                panelSettings.targetTexture = originalTarget;
                panelSettings.scaleMode = originalScaleMode;
                panelSettings.scale = originalScale;
                document = Object.FindFirstObjectByType<UIDocument>();
                controller = Object.FindFirstObjectByType<StimVerticalSliceController>();
                root = document?.rootVisualElement;
                controller?.SetAccessibilityTextScale(1f);
            }
        }

        private static void HideBlockingOverlays(VisualElement root)
        {
            foreach (var name in new[]
                     {
                         "event-sheet", "study-session-sheet", "new-life-setup",
                         "final-life-summary", "first-life-orientation"
                     })
            {
                var overlay = root.Q<VisualElement>(name);
                if (overlay == null) continue;
                overlay.AddToClassList("hidden");
                overlay.style.display = DisplayStyle.None;
            }
        }

        private static void ShowDestination(VisualElement root, DestinationCapture selected)
        {
            foreach (var destination in Destinations)
            {
                root.Q<Button>(destination.NavigationName)
                    .EnableInClassList("active", destination.Name == selected.Name);
                var view = root.Q<VisualElement>(destination.ViewName);
                var isSelected = destination.Name == selected.Name;
                view.EnableInClassList("hidden", !isSelected);
                view.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static void WriteRenderTexturePng(RenderTexture source, string path)
        {
            var previous = RenderTexture.active;
            var texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                texture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(texture);
            }
        }

        private static void ClearRenderTexture(RenderTexture target)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = previous;
        }

        private static void WriteReviewManifest(string outputDirectory, IReadOnlyList<string> evidence)
        {
            var lines = new List<string>
            {
                "# M13 Visual Review",
                string.Empty,
                $"Generated from commit `{GetCommitLabel()}` with Unity `{Application.unityVersion}`.",
                string.Empty,
                "Review each image for safe-area composition, clipping, overlap, text wrapping, visible scroll affordances, 44-point targets, hierarchy, and aspect-safe artwork.",
                string.Empty,
                "| Screenshot | Safe area | Text/overflow | Scrolling | Hierarchy/art | Result |",
                "|---|---|---|---|---|---|"
            };
            foreach (var fileName in evidence)
                lines.Add($"| [{fileName}]({fileName}) | ☐ | ☐ | ☐ | ☐ | ☐ Pending |");
            lines.Add(string.Empty);
            File.WriteAllLines(Path.Combine(outputDirectory, "REVIEW.md"), lines);
        }

        private static string GetCommitLabel()
        {
            var commit = System.Environment.GetEnvironmentVariable("GITHUB_SHA");
            return string.IsNullOrWhiteSpace(commit) ? "local-worktree" : commit;
        }

        private static string TextScaleLabel(float textScale)
        {
            return textScale >= 1.3f ? "130pct" : "100pct";
        }

        private readonly struct CaptureProfile
        {
            public CaptureProfile(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public int Width { get; }
            public int Height { get; }
        }

        private readonly struct DestinationCapture
        {
            public DestinationCapture(string name, string navigationName, string viewName)
            {
                Name = name;
                NavigationName = navigationName;
                ViewName = viewName;
            }

            public string Name { get; }
            public string NavigationName { get; }
            public string ViewName { get; }
        }
    }
}
