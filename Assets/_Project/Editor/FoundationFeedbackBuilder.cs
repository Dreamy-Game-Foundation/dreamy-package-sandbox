using System;
using Dreamy.Feedback;
using Dreamy.Feedback.Samples.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Dreamy.Template.Editor
{
    public static class FoundationFeedbackBuilder
    {
        public const string Basic = "Assets/Samples/Dreamy Feedback/0.3.0/Basic Feedback/Generated";
        [MenuItem("Dreamy/Sandbox/Wire Feedback Rig")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before wiring the rig.");
            const string path = "Assets/_Project/Scenes/BootstrapScene.unity";
            var loaded = SceneManager.GetSceneByPath(path);
            bool opened = !loaded.IsValid() || !loaded.isLoaded;
            if (!opened && loaded.isDirty) throw new InvalidOperationException("Save BootstrapScene before wiring the rig.");
            var scene = opened ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) : loaded;
            try
            {
                var installer = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<GameInstaller>(true)).Single();
                var data = new SerializedObject(installer);
                data.FindProperty("feedbackPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Basic + "/FeedbackRig.prefab").GetComponent<FeedbackHost>();
                data.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
