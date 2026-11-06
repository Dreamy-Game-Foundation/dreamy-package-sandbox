using System;
using System.Collections;
using System.IO;
using Dreamy.Debugger;
using UnityEngine;
public sealed class StartupPlayerProbe : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot(){if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DREAMY_STARTUP_PROBE")))new GameObject("Consumer startup probe").AddComponent<StartupPlayerProbe>();}
    private IEnumerator Start()
    {
        yield return null;var runtime=DebuggerRuntime.Instance;
        while(runtime!=null && !runtime.Preferences.Ready.IsCompleted)yield return null;
        var result=new Receipt{development=Debug.isDebugBuild,runtime=runtime!=null};
        if(runtime!=null){
            result.catalog=runtime.Catalog!=null;result.initialHud=runtime.Hud!=null?runtime.Hud.Mode.ToString():"Off";
            runtime.Access.RequestPrompt();runtime.Access.SubmitPin("0000");runtime.Open();yield return null;
            result.unlocked=runtime.Access.IsUnlocked;result.panel=DreamyDebug.Instance.IsDebugPanelVisible;
            runtime.SetHud(HudMode.Compact);runtime.SetHudPosition(HudPosition.BottomLeft);
            var flush=runtime.FlushPreferencesAsync();while(!flush.IsCompleted)yield return null;
            result.persisted=runtime.Preferences.LastError==null;
        }
        var pass=Debug.isDebugBuild?result.runtime && result.catalog && result.unlocked && result.panel && result.persisted:!result.runtime;
        result.passed=pass;File.WriteAllText(Environment.GetEnvironmentVariable("DREAMY_STARTUP_PROBE"),JsonUtility.ToJson(result,true));Application.Quit(pass?0:2);
    }
    [Serializable]private sealed class Receipt{public bool development,runtime,catalog,unlocked,panel,persisted,passed;public string initialHud;}
}
