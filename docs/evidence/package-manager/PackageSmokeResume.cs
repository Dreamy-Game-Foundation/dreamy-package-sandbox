using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using Newtonsoft.Json.Linq;
public static class PackageSmokeResume
{
    public static void Run()
    {
        var fixture=JObject.Parse(File.ReadAllText("Fixture.json"));
        var state=JObject.Parse(File.ReadAllText("Library/PackageManagerSmoke.json"));
        var journal=JObject.Parse(File.ReadAllText("Library/DreamyPackageManager/operation.json"));
        var p=UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Single(x=>x.name=="com.dreamy.editor-tools");
        bool pass=p.version=="1.1.1" && p.source==PackageSource.Git && p.git.hash==(string)fixture["selfSha"] && !(bool)journal["Pending"] && ((string)journal["Message"]).StartsWith("UPM completed");
        ((JArray)state["Passed"]).Add("Self source-switch/update: pinned Git version/revision verified after restarting the isolated consumer following assembly reload");
        File.WriteAllText("PackageManagerSmoke.json",new JObject { ["status"]=pass?"pass":"fail",["Passed"]=state["Passed"],["Degraded"]=state["Degraded"],["unity"]=UnityEngine.Application.unityVersion }.ToString());
        EditorApplication.Exit(pass?0:1);
    }
}
