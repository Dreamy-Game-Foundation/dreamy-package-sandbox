using System;
using System.IO;
using System.Linq;
using Dreamy.Feedback;
using Dreamy.Feedback.Samples.Editor;
using Dreamy.Feedback.Samples.Economy;
using Dreamy.Feedback.Samples.Economy.Editor;
using Dreamy.Template.Demo;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamy.Template.Editor
{
    public static class FoundationFeedbackBuilder
    {
        public const string Basic = "Assets/Samples/Dreamy Feedback/0.2.0/Basic Feedback/Generated";
        public const string Economy = "Assets/Samples/Dreamy Feedback/0.2.0/Feedback Economy/Generated";
        [MenuItem("Dreamy/Sandbox/Build Feedback Samples and Integration")]
        public static void Build()
        {
            FeedbackSampleBuilder.RequireEditMode();
            if (Application.isBatchMode && string.IsNullOrEmpty(SceneManager.GetActiveScene().path)) EditorSceneManager.OpenScene("Assets/_Project/Scenes/BootstrapScene.unity", OpenSceneMode.Single);
            if (!File.Exists(Basic + "/FeedbackDemo.unity")) FeedbackSampleBuilder.BuildAt(Basic);
            if (!File.Exists(Economy + "/FeedbackEconomyDemo.unity")) FeedbackEconomySampleBuilder.BuildAt(Economy, Basic);
            NormalizeSamples(); UpdateFoundation(); UpdateHost(); UpdateBootstrap();
            AssetDatabase.SaveAssets();
            Debug.Log("[Feedback] Samples and Foundation integration built.");
        }
        private static void NormalizeSamples()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Basic + "/DemoFont.asset");
            font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            font.TryAddCharacters(new string(Enumerable.Range(32, 95).Select(i => (char)i).ToArray()));
            font.atlasPopulationMode = AtlasPopulationMode.Static; EditorUtility.SetDirty(font);
            font.material.shader = Shader.Find("Dreamy/Feedback/SampleText"); EditorUtility.SetDirty(font.material);
            var material = AssetDatabase.LoadAssetAtPath<Material>(Basic + "/RewardParticles.mat");
            material.shader = Shader.Find("Dreamy/Feedback/SampleParticles"); EditorUtility.SetDirty(material);
            var rig = PrefabUtility.LoadPrefabContents(Basic + "/FeedbackRig.prefab");
            try
            {
                foreach (var listener in rig.GetComponentsInChildren<AudioListener>(true)) UnityEngine.Object.DestroyImmediate(listener);
                PrefabUtility.SaveAsPrefabAsset(rig, Basic + "/FeedbackRig.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(rig); }
            var panel = PrefabUtility.LoadPrefabContents(Economy + "/FeedbackEconomyPanel.prefab");
            try
            {
                FeedbackSampleBuilder.Stretch((RectTransform)panel.transform);
                MakeScrollable((RectTransform)panel.transform.Find("SafeArea/Card"));
                PrefabUtility.SaveAsPrefabAsset(panel, Economy + "/FeedbackEconomyPanel.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(panel); }
            var basicScene = EditorSceneManager.OpenScene(Basic + "/FeedbackDemo.unity", OpenSceneMode.Additive);
            try
            {
                var canvas = basicScene.GetRootGameObjects().Single(g => g.name == "DemoCanvas");
                MakeScrollable((RectTransform)canvas.transform.Find("SafeArea/Controls"));
                NormalizeInput(basicScene);
                EditorSceneManager.MarkSceneDirty(basicScene); EditorSceneManager.SaveScene(basicScene);
            }
            finally { EditorSceneManager.CloseScene(basicScene, true); }
            var scene = EditorSceneManager.OpenScene(Economy + "/FeedbackEconomyDemo.unity", OpenSceneMode.Additive);
            try
            {
                var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>(true)).Single();
                if (camera.GetComponent<AudioListener>() == null) camera.gameObject.AddComponent<AudioListener>();
                NormalizeInput(scene);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
        private static void NormalizeInput(Scene scene)
        {
            var system = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true)).Single();
            foreach (var module in system.GetComponents<UnityEngine.EventSystems.BaseInputModule>())
                if (!(module is UnityEngine.EventSystems.StandaloneInputModule)) UnityEngine.Object.DestroyImmediate(module);
            if (system.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null) system.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        private static void MakeScrollable(RectTransform card)
        {
            var existing = card.GetComponent<UnityEngine.UI.ScrollRect>();
            if (existing != null)
            {
                var vertical = existing.content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                vertical.childControlWidth = true; vertical.childControlHeight = true; vertical.childForceExpandWidth = true;
                foreach (var toggle in existing.content.GetComponentsInChildren<UnityEngine.UI.Toggle>(true))
                {
                    toggle.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().childForceExpandWidth = false;
                    toggle.GetComponentInChildren<TMP_Text>().GetComponent<UnityEngine.UI.LayoutElement>().flexibleWidth = 1;
                }
                return;
            }
            var children = card.Cast<Transform>().ToArray();
            var old = card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            var padding = old.padding; float spacing = old.spacing;
            UnityEngine.Object.DestroyImmediate(old);
            var content = FeedbackSampleBuilder.ScrollContent(card);
            var layout = content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding = padding; layout.spacing = spacing; layout.childForceExpandHeight = false; layout.childControlWidth = true; layout.childControlHeight = true; layout.childForceExpandWidth = true;
            foreach (var child in children) child.SetParent(content, false);
        }
        private static void UpdateFoundation()
        {
            var root = PrefabUtility.LoadPrefabContents(FoundationFeatureDemoBuilder.FoundationPath);
            try
            {
                var panel = root.GetComponent<FoundationDemoPanel>();
                var data = new SerializedObject(panel);
                if (data.FindProperty("openFeedbackButton").objectReferenceValue != null) return;
                var group = root.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "BtnGroup");
                var font = root.GetComponentInChildren<TMP_Text>(true).font;
                var row = FeedbackSampleBuilder.Row(group); row.name = "FeedbackRow";
                var button = FeedbackSampleBuilder.Button("Feedback", row, font);
                FeedbackSampleBuilder.Set(panel, "openFeedbackButton", button);
                PrefabUtility.SaveAsPrefabAsset(root, FoundationFeatureDemoBuilder.FoundationPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void UpdateHost()
        {
            var root = PrefabUtility.LoadPrefabContents(FoundationFeatureDemoBuilder.HostPath);
            try
            {
                FeedbackSampleBuilder.Set(root.GetComponent<FoundationFeatureDemos>(), "feedbackPrefab",
                    AssetDatabase.LoadAssetAtPath<GameObject>(Economy + "/FeedbackEconomyPanel.prefab").GetComponent<FeedbackEconomyPanel>());
                PrefabUtility.SaveAsPrefabAsset(root, FoundationFeatureDemoBuilder.HostPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void UpdateBootstrap()
        {
            const string path = "Assets/_Project/Scenes/BootstrapScene.unity";
            var loaded = SceneManager.GetSceneByPath(path);
            bool opened = !loaded.IsValid() || !loaded.isLoaded;
            if (!opened && loaded.isDirty) throw new InvalidOperationException("BootstrapScene is dirty. Save it before building integration.");
            Scene scene = opened ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) : loaded;
            try
            {
                var installer = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<GameInstaller>(true)).Single();
                FeedbackSampleBuilder.Set(installer, "feedbackPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Basic + "/FeedbackRig.prefab").GetComponent<FeedbackHost>());
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
