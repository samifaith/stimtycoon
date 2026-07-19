using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StimTycoon.Editor
{
    [InitializeOnLoad]
    public static class IPhone17DeviceProfiles
    {
        private const string DevicePackage = "Packages/com.unity.device-simulator.devices/Editor/Devices";
        private const string OutputDirectory = "Assets/DeviceSimulatorDevices";
        private const string OutputOverlayDirectory = OutputDirectory + "/DeviceOverlays";

        private readonly struct Profile
        {
            public readonly string name;
            public readonly string template;
            public readonly int width;
            public readonly int height;
            public readonly int safeBottom;
            public readonly int safeTop;
            public readonly int landscapeInset;
            public readonly string model;
            public readonly string chip;
            public readonly int memoryMb;

            public Profile(string name, string template, int width, int height, int safeBottom,
                int safeTop, int landscapeInset, string model, string chip, int memoryMb)
            {
                this.name = name;
                this.template = template;
                this.width = width;
                this.height = height;
                this.safeBottom = safeBottom;
                this.safeTop = safeTop;
                this.landscapeInset = landscapeInset;
                this.model = model;
                this.chip = chip;
                this.memoryMb = memoryMb;
            }
        }

        private static readonly Profile[] Profiles =
        {
            new Profile("Apple iPhone 17", "Apple iPhone 13 Pro.device", 1206, 2622,
                102, 186, 186, "iPhone18,3", "Apple A19 GPU", 8192),
            new Profile("Apple iPhone 17 Pro", "Apple iPhone 13 Pro.device", 1206, 2622,
                102, 186, 186, "iPhone18,1", "Apple A19 Pro GPU", 12288),
            new Profile("Apple iPhone 17 Pro Max", "Apple iPhone 13 Pro Max.device", 1320, 2868,
                102, 186, 186, "iPhone18,2", "Apple A19 Pro GPU", 12288)
        };

        static IPhone17DeviceProfiles()
        {
            EditorApplication.delayCall += InstallIfNeeded;
        }

        [MenuItem("Tools/Stim Tycoon/Install iPhone 17 Simulator Profiles")]
        public static void Install()
        {
            if (InstallProfiles())
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("Stim Tycoon iPhone 17 Device Simulator profiles are installed. Reopen Device Simulator if its device list was already open.");
        }

        private static void InstallIfNeeded()
        {
            if (InstallProfiles()) AssetDatabase.Refresh();
        }

        private static bool InstallProfiles()
        {
            Directory.CreateDirectory(OutputDirectory);
            Directory.CreateDirectory(OutputOverlayDirectory);
            var changed = false;
            changed |= CopyOverlay("Apple iPhone 13 Pro_Overlay.png");
            changed |= CopyOverlay("Apple iPhone 13 Pro Max_Overlay.png");
            foreach (var profile in Profiles)
            {
                var templatePath = Path.Combine(DevicePackage, profile.template);
                if (!File.Exists(templatePath))
                {
                    Debug.LogWarning($"Cannot create {profile.name}: missing Device Simulator template {templatePath}.");
                    continue;
                }

                var outputPath = Path.Combine(OutputDirectory, profile.name + ".device");
                var generated = BuildDefinition(File.ReadAllText(templatePath), profile);
                if (File.Exists(outputPath) && File.ReadAllText(outputPath) == generated) continue;
                File.WriteAllText(outputPath, generated);
                changed = true;
            }
            return changed;
        }

        private static bool CopyOverlay(string fileName)
        {
            var source = Path.Combine(DevicePackage, "DeviceOverlays", fileName);
            var destination = Path.Combine(OutputOverlayDirectory, fileName);
            if (!File.Exists(source))
            {
                Debug.LogWarning($"Cannot install simulator overlay: missing {source}.");
                return false;
            }
            if (File.Exists(destination) && FilesEqual(source, destination)) return false;
            File.Copy(source, destination, true);
            return true;
        }

        private static bool FilesEqual(string left, string right)
        {
            var leftInfo = new FileInfo(left);
            var rightInfo = new FileInfo(right);
            if (leftInfo.Length != rightInfo.Length) return false;
            var leftBytes = File.ReadAllBytes(left);
            var rightBytes = File.ReadAllBytes(right);
            for (var index = 0; index < leftBytes.Length; index++)
                if (leftBytes[index] != rightBytes[index]) return false;
            return true;
        }

        private static string BuildDefinition(string template, Profile profile)
        {
            var safeHeight = profile.height - profile.safeBottom - profile.safeTop;
            var landscapeWidth = profile.height - profile.landscapeInset * 2;
            var landscapeHeight = profile.width - 126;

            var json = ReplaceOnce(template, "\"friendlyName\": \"Apple iPhone 13 Pro Max\"", $"\"friendlyName\": \"{profile.name}\"");
            json = ReplaceOnce(json, "\"friendlyName\": \"Apple iPhone 13 Pro\"", $"\"friendlyName\": \"{profile.name}\"");
            json = ReplaceOnce(json, "\"width\": 1284", $"\"width\": {profile.width}");
            json = ReplaceOnce(json, "\"height\": 2778", $"\"height\": {profile.height}");
            json = ReplaceOnce(json, "\"width\": 1170", $"\"width\": {profile.width}");
            json = ReplaceOnce(json, "\"height\": 2532", $"\"height\": {profile.height}");

            json = ReplaceAll(json, "\"y\": 102.0", $"\"y\": {profile.safeBottom}.0");
            json = ReplaceAll(json, "\"y\": 141.0", $"\"y\": {profile.safeTop}.0");
            json = ReplaceAll(json, "\"height\": 2535.0", $"\"height\": {safeHeight}.0");
            json = ReplaceAll(json, "\"height\": 2289.0", $"\"height\": {safeHeight}.0");
            json = ReplaceAll(json, "\"width\": 1284.0", $"\"width\": {profile.width}.0");
            json = ReplaceAll(json, "\"width\": 1170.0", $"\"width\": {profile.width}.0");
            json = ReplaceAll(json, "\"x\": 141.0", $"\"x\": {profile.landscapeInset}.0");
            json = ReplaceAll(json, "\"width\": 2496.0", $"\"width\": {landscapeWidth}.0");
            json = ReplaceAll(json, "\"width\": 2250.0", $"\"width\": {landscapeWidth}.0");
            json = ReplaceAll(json, "\"height\": 1221.0", $"\"height\": {landscapeHeight}.0");
            json = ReplaceAll(json, "\"height\": 1107.0", $"\"height\": {landscapeHeight}.0");

            json = ReplaceOnce(json, "\"deviceModel\": \"iPhone14,3\"", $"\"deviceModel\": \"{profile.model}\"");
            json = ReplaceOnce(json, "\"deviceModel\": \"iPhone14,2\"", $"\"deviceModel\": \"{profile.model}\"");
            json = ReplaceOnce(json, "\"operatingSystem\": \"iOS 15.1\"", "\"operatingSystem\": \"iOS 26\"");
            json = ReplaceOnce(json, "\"graphicsDeviceName\": \"Apple A15 GPU\"", $"\"graphicsDeviceName\": \"{profile.chip}\"");
            json = ReplaceOnce(json, "\"systemMemorySize\": 5693", $"\"systemMemorySize\": {profile.memoryMb}");
            json = ReplaceOnce(json, "\"systemMemorySize\": 5666", $"\"systemMemorySize\": {profile.memoryMb}");
            return json;
        }

        private static string ReplaceOnce(string value, string oldValue, string newValue)
        {
            var index = value.IndexOf(oldValue, StringComparison.Ordinal);
            return index < 0 ? value : value.Substring(0, index) + newValue + value.Substring(index + oldValue.Length);
        }

        private static string ReplaceAll(string value, string oldValue, string newValue)
        {
            return value.Replace(oldValue, newValue);
        }
    }
}
