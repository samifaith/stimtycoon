using System.IO;
using StimTycoon.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace StimTycoon.Editor
{
    public static class StimVerticalSliceSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/StimVerticalSlice.unity";
        private const string UxmlPath = "Assets/UI/StimVerticalSlice.uxml";
        private const string PanelSettingsPath = "Assets/UI/StimPanelSettings.asset";

        [MenuItem("Tools/Stim Tycoon/Create Vertical Slice Scene")]
        public static void CreateScene()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
            {
                Debug.LogError($"Missing vertical slice UI at {UxmlPath}.");
                return;
            }

            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.name = "StimPanelSettings";
                AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(PanelSettingsPath, ImportAssetOptions.ForceUpdate);
                panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            }

            if (panelSettings == null)
            {
                Debug.LogError($"Could not create panel settings at {PanelSettingsPath}.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var gameObject = new GameObject("Stim Vertical Slice");
            gameObject.SetActive(false);
            gameObject.AddComponent<UIDocument>();
            var controller = gameObject.AddComponent<StimVerticalSliceController>();
            controller.Configure(panelSettings, visualTree);
            gameObject.SetActive(true);
            EditorUtility.SetDirty(controller);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.071f, 0.094f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = gameObject;
            Debug.Log($"Created playable vertical slice scene at {ScenePath}.");
        }
    }
}
