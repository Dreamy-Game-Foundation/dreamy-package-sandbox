using System;
using System.IO;
using Dreamy.Debugger;
using Dreamy.Debugger.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class InspectionSmokeBuild
{
    public static void Windows()
    {
        Prepare();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/InspectionSmoke.unity" },
            locationPathName = "Builds/Windows/InspectionSmoke.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
        Finish(report);
    }
    public static void Android()
    {
        Prepare();
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.High);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/InspectionSmoke.unity" },
            locationPathName = "Builds/Android/InspectionSmoke.apk", target = BuildTarget.Android, options = BuildOptions.Development });
        Finish(report);
    }
    private static void Prepare()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera)); camera.tag = "MainCamera";
        new GameObject("Directional Light", typeof(Light)).GetComponent<Light>().type = LightType.Directional;
        var config = AssetDatabase.LoadAssetAtPath<DebuggerConfiguration>("Assets/InspectionConfiguration.asset");
        if (config == null) { config = ScriptableObject.CreateInstance<DebuggerConfiguration>(); AssetDatabase.CreateAsset(config, "Assets/InspectionConfiguration.asset"); }
        config.AllowSceneEditing = true; config.PersistRuntimePreferences = false; EditorUtility.SetDirty(config); AssetDatabase.SaveAssetIfDirty(config);
        var root = new GameObject("Inspection runtime"); root.SetActive(false);
        var runtime = root.AddComponent<DebuggerRuntime>(); runtime.ConfigurationAsset = config;
        runtime.Catalog = AssetDatabase.LoadAssetAtPath<DebuggerAssetCatalog>("Packages/com.dreamy.debugger/Runtime/Prefabs/DebuggerAssets.asset"); root.SetActive(true);
        new GameObject("Inspection smoke fixture", typeof(InspectionPlayerSmoke));
        EditorSceneManager.SaveScene(scene, "Assets/InspectionSmoke.unity");
    }
    private static void Finish(BuildReport report)
    {
        File.WriteAllText("build-result.json", JsonUtility.ToJson(new Receipt { result = report.summary.result.ToString(), bytes = report.summary.totalSize.ToString(), platform = report.summary.platform.ToString() }, true));
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + report.summary.result);
    }
    [Serializable] private sealed class Receipt { public string result, bytes, platform; }
}
