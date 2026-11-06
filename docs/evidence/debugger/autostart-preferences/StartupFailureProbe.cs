using System;
using System.IO;
using System.Linq;
using Dreamy.Debugger.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
public sealed class StartupFailureProbe : IPreprocessBuildWithReport
{
    private static bool armed;private static int ticks;private static BuildReport failed;
    public int callbackOrder=>100;
    public void OnPreprocessBuild(BuildReport report){if(armed)throw new BuildFailedException("[FAKE] Intentional failure after debugger startup staging");}
    public static void Run()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.SaveScene(scene,"Assets/ForcedFailureScene.unity");
        armed=true;failed=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ForcedFailureScene.unity"},target=BuildTarget.StandaloneLinux64,options=BuildOptions.Development,locationPathName="/tmp/dreamy-startup-failed-player/Game.x86_64"});armed=false;
        EditorApplication.update+=CheckRecovery;
    }
    private static void CheckRecovery()
    {
        if(++ticks<3)return;EditorApplication.update-=CheckRecovery;
        var reason=failed.steps.SelectMany(s=>s.messages).Any(m=>m.content.Contains("Intentional failure"));
        var clean=!Directory.Exists(DebuggerStartupBuild.Stage);var passed=failed.summary.result==BuildResult.Failed && reason && clean;
        File.WriteAllText("/tmp/dreamy-startup-forced-failure.json",JsonUtility.ToJson(new Receipt{result=failed.summary.result.ToString(),intentionalFailure=reason,stagingCleaned=clean,passed=passed},true));
        EditorApplication.Exit(passed?0:2);
    }
    [Serializable]private sealed class Receipt{public string result;public bool intentionalFailure,stagingCleaned,passed;}
}
