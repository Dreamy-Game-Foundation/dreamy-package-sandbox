using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class ConsumerValidation
{
    public static void ImportBasic() { Import("Basic Feedback"); }
    public static void ImportEconomy() { Import("Feedback Economy"); }
    private static void Import(string name)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("UnityEditor.PackageManager.UI.Sample")).First(t => t != null);
        var method = type.GetMethod("FindByPackage", BindingFlags.Public | BindingFlags.Static);
        if (method == null) throw new Exception("UPM sample discovery API missing: " + string.Join(",", type.GetMethods().Select(m => m.Name)));
        var samples = (IEnumerable)method.Invoke(null, new object[] { "com.dreamy.feedback", "0.2.0" });
        foreach (var sample in samples)
        {
            if ((string)type.GetProperty("displayName").GetValue(sample) != name) continue;
            var import = type.GetMethods().First(m => m.Name == "Import" && m.GetParameters().Length == 1);
            var options = Enum.Parse(import.GetParameters()[0].ParameterType, "OverridePreviousImports");
            bool result = (bool)import.Invoke(sample, new[] { options });
            if (!result) throw new Exception("UPM import returned false: " + name);
            Debug.Log("[Feedback Consumer] UPM imported " + name); return;
        }
        throw new Exception("Declared sample missing: " + name);
    }
    public static void ImportTMP()
    {
        AssetDatabase.importPackageCompleted += name => { Debug.Log("[Feedback Consumer] TMP import completed: " + name); EditorApplication.delayCall += () => EditorApplication.Exit(Resources.Load<TMPro.TMP_Settings>("TMP Settings") ? 0 : 1); };
        AssetDatabase.importPackageFailed += (name, error) => { Debug.LogError(error); EditorApplication.Exit(1); };
        var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TMP_Settings).Assembly);
        AssetDatabase.ImportPackage(package.resolvedPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
    }
    public static void BuildPlayer()
    {
        if (!Resources.Load<TMPro.TMP_Settings>("TMP Settings")) throw new Exception("TMP Essential Resources missing");
        string scene = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("FeedbackDemo t:Scene").First());
        string folder = Path.Combine(Path.GetTempPath(), "dreamy-feedback-player-20261007"); Directory.CreateDirectory(folder);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { scene }, locationPathName = folder + "/FeedbackDemo.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Player build failed: " + report.summary.result);
        Debug.Log("[Feedback Consumer] Player build passed: " + report.summary.totalSize + " bytes");
    }
}
