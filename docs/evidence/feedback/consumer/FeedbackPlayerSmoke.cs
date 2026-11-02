using System.Collections;
using System.Linq;
using Dreamy.Feedback;
using Dreamy.Feedback.Samples;
using UnityEngine;
public sealed class FeedbackPlayerSmoke : MonoBehaviour
{
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void StartSmoke() { if (System.Environment.GetCommandLineArgs().Contains("--feedback-smoke")) new GameObject("Smoke").AddComponent<FeedbackPlayerSmoke>(); }
 bool errors;
 void Awake() { Application.logMessageReceived += OnLog; }
 void OnLog(string message,string stack,LogType type) { if(type==LogType.Exception || type==LogType.Error || type==LogType.Assert) errors=true; }
 IEnumerator Start() {
  yield return null;
  var controls=Object.FindAnyObjectByType<FeedbackDemoControls>(); var host=Object.FindAnyObjectByType<FeedbackHost>();
  if (!controls || !host || !host.IsInitialized) { Debug.LogError("FEEDBACK_SMOKE_FAILED: initialization"); Application.Quit(1); yield break; }
  controls.PlayAll(); yield return new WaitForSeconds(2); controls.StopAll(); yield return null;
  var root=host.GetComponent<FeedbackRoot>();
  if(root.WorldVfxRoot.childCount+root.FloatingTextRoot.childCount+root.IconFlyRoot.childCount != 0) { Debug.LogError("FEEDBACK_SMOKE_FAILED: cleanup"); Application.Quit(1); yield break; }
  if(errors) { Debug.LogError("FEEDBACK_SMOKE_FAILED: logged errors"); Application.Quit(1); yield break; }
  Debug.Log("FEEDBACK_SMOKE_PASSED"); Application.Quit(0);
 }
}
