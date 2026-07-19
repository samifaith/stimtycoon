using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace StimTycoon.Editor
{
    public static class ProjectReadinessCheck
    {
        private const string EasySaveDefine = "STIM_EASY_SAVE_3";

        [MenuItem("Tools/Stim Tycoon/Run Setup Check")]
        public static void Run()
        {
            var failures = new List<string>();
            var warnings = new List<string>();
            var passes = new List<string>();
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;

            CheckPath(EditorApplication.applicationPath, "Unity Editor application", failures, passes);
            Check(Path.Combine(projectRoot ?? string.Empty, "Packages", "manifest.json"), "Packages/manifest.json", failures, passes);
            Check(Path.Combine(projectRoot ?? string.Empty, "ProjectSettings", "ProjectVersion.txt"), "ProjectVersion.txt", failures, passes);
            Check(Path.Combine(projectRoot ?? string.Empty, "ProjectSettings", "ProjectSettings.asset"), "ProjectSettings.asset", failures, passes);
            CheckProductionScene(failures, passes);

            var editorVersion = Application.unityVersion;
            if (editorVersion.StartsWith("6000.3.", StringComparison.Ordinal))
            {
                passes.Add($"Editor version is {editorVersion}.");
            }
            else
            {
                warnings.Add($"Expected Unity 6000.3.x; running {editorVersion}.");
            }

            var easySaveInstalled = IsTypeAvailable("ES3");
            var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.iOS);
            var easySaveEnabled = HasDefine(defines, EasySaveDefine);

            if (easySaveInstalled && easySaveEnabled)
            {
                passes.Add("Easy Save 3 is installed and its Stim adapter is enabled for iOS.");
            }
            else if (easySaveInstalled)
            {
                warnings.Add($"Easy Save 3 is installed; add {EasySaveDefine} to the iOS scripting defines when its import compiles cleanly.");
            }
            else if (easySaveEnabled)
            {
                failures.Add($"{EasySaveDefine} is enabled, but the ES3 type is unavailable. Remove the define or finish importing Easy Save 3.");
            }
            else
            {
                passes.Add("Easy Save 3 is not enabled yet (correct for the clean baseline).");
            }

            WriteResults(passes, warnings, failures);
        }

        private static void CheckProductionScene(ICollection<string> failures, ICollection<string> passes)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(StimTycoon.Runtime.StimProjectPaths.VerticalSliceScene);
            if (sceneAsset == null)
            {
                failures.Add($"Missing production scene: {StimTycoon.Runtime.StimProjectPaths.VerticalSliceScene}");
                return;
            }

            passes.Add($"Found production scene at {StimTycoon.Runtime.StimProjectPaths.VerticalSliceScene}.");
            var enabledInBuild = Array.Exists(EditorBuildSettings.scenes,
                scene => scene.enabled && scene.path == StimTycoon.Runtime.StimProjectPaths.VerticalSliceScene);
            if (enabledInBuild)
            {
                passes.Add("Production scene is enabled in Build Settings.");
            }
            else
            {
                failures.Add("Production scene is not enabled in Build Settings.");
            }
        }

        private static void Check(string path, string label, ICollection<string> failures, ICollection<string> passes)
        {
            if (File.Exists(path))
            {
                passes.Add($"Found {label}.");
            }
            else
            {
                failures.Add($"Missing {label}: {path}");
            }
        }

        private static void CheckPath(string path, string label, ICollection<string> failures, ICollection<string> passes)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                passes.Add($"Found {label}.");
            }
            else
            {
                failures.Add($"Missing {label}: {path}");
            }
        }

        private static bool IsTypeAvailable(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(fullName, false) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDefine(string defines, string expected)
        {
            var symbols = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var symbol in symbols)
            {
                if (string.Equals(symbol.Trim(), expected, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void WriteResults(
            IReadOnlyCollection<string> passes,
            IReadOnlyCollection<string> warnings,
            IReadOnlyCollection<string> failures)
        {
            foreach (var pass in passes)
            {
                Debug.Log($"[Stim Setup] PASS: {pass}");
            }

            foreach (var warning in warnings)
            {
                Debug.LogWarning($"[Stim Setup] WARNING: {warning}");
            }

            foreach (var failure in failures)
            {
                Debug.LogError($"[Stim Setup] FAIL: {failure}");
            }

            var summary = $"Stim setup check: {passes.Count} passed, {warnings.Count} warnings, {failures.Count} failed.";
            if (failures.Count == 0)
            {
                Debug.Log($"[Stim Setup] {summary}");
            }
            else
            {
                Debug.LogError($"[Stim Setup] {summary}");
            }
        }
    }
}
