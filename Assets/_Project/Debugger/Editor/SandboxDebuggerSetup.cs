using System;
using System.Linq;
using Dreamy.Debugger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamy.Template.Debugging.Editor
{
    public static class SandboxDebuggerSetup
    {
        public const string Root = "Assets/_Project/Debugger";
        public const string PrefabPath = Root + "/Prefabs/SandboxDebugger.prefab";
        public const string ConfigPath = Root + "/SandboxDebuggerConfiguration.asset";

        [MenuItem("Window/Dreamy Debugger/Set up sandbox game cheats", priority = 110)]
        public static void InstallIntoStartupScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Stop Play Mode and close Prefab Mode first.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/_Project/Scenes/BootstrapScene.unity")
                throw new InvalidOperationException("Open BootstrapScene to install this game's cheat host.");
            var hosts = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<DebuggerRuntime>(true)).ToArray();
            if (hosts.Length > 1) throw new InvalidOperationException("Multiple debugger hosts found. Inspect scene ownership first.");
            var config = AssetDatabase.LoadAssetAtPath<DebuggerConfiguration>(ConfigPath);
            if (config == null)
            {
                config = hosts.Length == 1 && hosts[0].ConfigurationAsset != null
                    ? UnityEngine.Object.Instantiate(hosts[0].ConfigurationAsset)
                    : ScriptableObject.CreateInstance<DebuggerConfiguration>();
                config.name = "SandboxDebuggerConfiguration";
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
            if (!AssetDatabase.IsValidFolder(Root + "/Prefabs")) AssetDatabase.CreateFolder(Root, "Prefabs");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.dreamy.debugger/Runtime/Prefabs/DebuggerBootstrap.prefab");
                var authoring = (GameObject)PrefabUtility.InstantiatePrefab(source);
                try
                {
                    authoring.name = "SandboxDebugger";
                    authoring.GetComponent<DebuggerRuntime>().ConfigurationAsset = config;
                    authoring.AddComponent<SandboxCheatHost>();
                    prefab = PrefabUtility.SaveAsPrefabAsset(authoring, PrefabPath);
                }
                finally { UnityEngine.Object.DestroyImmediate(authoring); }
            }
            if (hosts.Length == 1 && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(hosts[0]) == PrefabPath)
            {
                Selection.activeGameObject = hosts[0].gameObject;
                return;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Install sandbox game cheats");
            if (hosts.Length == 1) Undo.DestroyObjectImmediate(hosts[0].gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = instance;
        }
    }
}
