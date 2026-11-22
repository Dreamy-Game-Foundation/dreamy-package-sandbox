using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dreamy.Debugger;
using Dreamy.Debugger.SceneInspection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public sealed class InspectionPlayerSmoke : MonoBehaviour
{
    [Serializable, Preserve] public struct Leaf { public long Number; }
    [Serializable, Preserve] public struct Nested { public Leaf Leaf; }
    public List<Nested> Items = new List<Nested> { new Nested { Leaf = new Leaf { Number = 7 } } };
    private IEnumerator Start()
    {
        yield return null; yield return null;
        string error = null;
        try
        {
            var runtime = DebuggerRuntime.Instance;
            if (runtime == null) throw new Exception("Runtime did not initialize");
            runtime.Access.RequestPrompt(); runtime.Access.SubmitPin("0000"); runtime.Open(Dreamy.Debugger.Presentation.DefaultTabs.Scene);
            var binding = InspectionMember.Field(this, typeof(InspectionPlayerSmoke).GetField("Items"))
                .Element(0, typeof(Nested)).Child(typeof(Nested).GetField("Leaf")).Child(typeof(Leaf).GetField("Number"));
            if (!binding.TryCommit(7L, 9007199254740993L, () => runtime.CanEditScene, out error)) throw new Exception(error);
            if (Items[0].Leaf.Number != 9007199254740993L) throw new Exception("Struct write-back lost precision");
            if (InspectionValues.TryParse("Infinity", typeof(double), out _, out _)) throw new Exception("Accepted non-finite number");
            if (binding.TryCommit(7L, 5L, () => true, out _)) throw new Exception("Accepted stale baseline");
            runtime.Relock();
            if (binding.TryCommit(9007199254740993L, 5L, () => runtime.CanEditScene, out _)) throw new Exception("Accepted locked write");
        }
        catch (Exception e) { error = e.ToString(); }
        var args = Environment.GetCommandLineArgs(); var receipt = Path.Combine(Application.persistentDataPath, "inspection-smoke.json");
        for (var i = 0; i + 1 < args.Length; i++) if (args[i] == "-inspectionReceipt") receipt = args[i + 1];
        var result = new Receipt { passed = error == null, error = error, unity = Application.unityVersion, platform = Application.platform.ToString() };
        File.WriteAllText(receipt, JsonUtility.ToJson(result, true)); Debug.Log(JsonUtility.ToJson(result)); Application.Quit(error == null ? 0 : 1);
    }
    [Serializable] private sealed class Receipt { public bool passed; public string error, unity, platform; }
}
