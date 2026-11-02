#if !UNITY_EDITOR
using System.Collections;
using UnityEngine;
using Dreamy.Feedback;
using Dreamy.Feedback.Samples;
public sealed class FeedbackPlayerSmoke:MonoBehaviour
{
 private bool failed;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install(){new GameObject("FeedbackPlayerSmoke").AddComponent<FeedbackPlayerSmoke>();}
 void Awake(){Application.logMessageReceived+=Log;}
 void Log(string msg,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)failed=true;}
 IEnumerator Start(){yield return null;yield return null;var host=Object.FindFirstObjectByType<FeedbackHost>();var controls=Object.FindFirstObjectByType<FeedbackDemoControls>();if(!host||!controls||!host.IsInitialized){Debug.LogError("Missing initialized host/sample");Application.Quit(1);yield break;}for(int i=0;i<controls.ChannelCount;i++)controls.PlayChannel(i);yield return new WaitForSecondsRealtime(.2f);controls.CompleteAll();controls.StopAll();yield return null;Debug.Log(failed?"FEEDBACK_PLAYER_SMOKE_FAIL":"FEEDBACK_PLAYER_SMOKE_PASS");Application.Quit(failed?1:0);}
 void OnDestroy(){Application.logMessageReceived-=Log;}
}

#endif
