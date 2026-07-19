using System.IO;
using StimTycoon.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace StimTycoon.Editor
{
    public static class VerticalSliceSceneBuilder
    {
        private const string ScenePath = "Assets/StimTycoon/Scenes/VerticalSlice.unity";
        private const string UxmlPath = "Assets/StimTycoon/UI/VerticalSlice.uxml";
        private const string PanelSettingsPath = "Assets/StimTycoon/UI/PanelSettings.asset";
        private const string FeedRowPath = "Assets/StimTycoon/UI/Components/FeedRow/FeedRow.uxml";
        private const string AchievementRowPath = "Assets/StimTycoon/UI/Components/AchievementRow/AchievementRow.uxml";
        private const string ActionCardPath = "Assets/StimTycoon/UI/Components/ActionCard/ActionCard.uxml";

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
                panelSettings.name = "PanelSettings";
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

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/StimTycoon/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var gameObject = new GameObject("Stim Vertical Slice");
            gameObject.SetActive(false);
            var document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTree;
            var controller = gameObject.AddComponent<VerticalSliceController>();
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("feedRowTemplate").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FeedRowPath);
            serializedController.FindProperty("achievementRowTemplate").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AchievementRowPath);
            serializedController.FindProperty("actionCardTemplate").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ActionCardPath);
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            gameObject.SetActive(true);
            EditorUtility.SetDirty(document);
            EditorUtility.SetDirty(controller);

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

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
