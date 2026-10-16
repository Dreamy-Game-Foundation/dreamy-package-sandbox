using System;
using Dreamy.Economy.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamy.Template.Editor
{
    public static class FoundationResourceSuggestionSetup
    {
        private const string Samples = "Assets/Samples/Dreamy Economy Contracts/0.4.0/Resource Recovery/ResourceSuggestions";
        private const string Output = "Assets/_Project/Prefabs/UI/ResourceSuggestions";
        [MenuItem("Tools/Dreamy/Sandbox/Install Resource Suggestion Panels")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before installing UI.");
            const string path = "Assets/_Project/Scenes/BootstrapScene.unity";
            var scene = SceneManager.GetSceneByPath(path);
            if (scene.IsValid() && scene.isDirty) throw new InvalidOperationException("BootstrapScene has unsaved edits; save them before installing suggestion bindings.");
            EnsureFolder(Output);
            var coin = Variant("Coin"); var gem = Variant("Gem");
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                GameInstaller installer = null;
                foreach (var root in scene.GetRootGameObjects())
                    if ((installer = root.GetComponentInChildren<GameInstaller>(true)) != null) break;
                if (installer == null) throw new InvalidOperationException("Bootstrap GameInstaller is missing.");
                var serialized = new SerializedObject(installer);
                var prefabs = serialized.FindProperty("resourceSuggestionPrefabs"); prefabs.arraySize = 2;
                prefabs.GetArrayElementAtIndex(0).objectReferenceValue = coin;
                prefabs.GetArrayElementAtIndex(1).objectReferenceValue = gem;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
            Debug.Log("PASS_FOUNDATION_RESOURCE_SUGGESTION_SETUP: Coin/Gem variants and Bootstrap bindings");
        }
        private static ResourceSuggestionPanel Variant(string name)
        {
            string path = Output + "/" + name + "SuggestionPanel.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<ResourceSuggestionPanel>(path);
            if (existing != null) return existing;
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Samples + "/" + name + "SuggestionPanel.prefab");
            if (source == null) throw new InvalidOperationException("Import Resource Recovery sample first: " + Samples);
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, preview);
                instance.name = name + "SuggestionPanel";
                return PrefabUtility.SaveAsPrefabAsset(instance, path).GetComponent<ResourceSuggestionPanel>();
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
