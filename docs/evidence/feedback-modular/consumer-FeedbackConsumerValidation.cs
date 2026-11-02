using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FeedbackConsumerValidation
{
 public static void Validate()
 {
  var path="Assets/Samples/Dreamy Feedback/0.3.0/Basic Feedback/Generated/FeedbackDemo.unity";
  var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
  foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script: "+t.name);
  foreach(var root in scene.GetRootGameObjects()) foreach(var r in root.GetComponentsInChildren<Renderer>(true)) if(!r.sharedMaterial||!r.sharedMaterial.shader) throw new Exception("Missing renderer material/shader: "+r.name);
  foreach(var guid in AssetDatabase.FindAssets("t:FeedbackDefinition")) { var asset=AssetDatabase.LoadAssetAtPath<Dreamy.Feedback.FeedbackDefinition>(AssetDatabase.GUIDToAssetPath(guid)); var error=Dreamy.Feedback.FeedbackGraphValidation.Validate(asset); if(error!=null) throw new Exception(error); }
  if(Type.GetType("Dreamy.Feedback.Dotween.DotweenFeedbackProvider, Dreamy.Feedback.Dotween.Runtime")!=null) throw new Exception("Unexpected DOTween dependency");
  if(Type.GetType("Dreamy.Core.ServiceLocator, Dreamy.Core.Runtime")!=null) throw new Exception("Unexpected Core dependency");
  Debug.Log("FEEDBACK_CONSUMER_VALIDATION_PASS");
 }
 public static void Build()
 {
  Validate();
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/Samples/Dreamy Feedback/0.3.0/Basic Feedback/Generated/FeedbackDemo.unity"},locationPathName="Build/FeedbackConsumer.x86_64",target=BuildTarget.StandaloneLinux64,options=BuildOptions.Development});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception(report.summary.result.ToString());
  Debug.Log("FEEDBACK_CONSUMER_BUILD_PASS");
 }
}
