// Consumer-only harness. Copy into the package's Tests/Editor directory in an isolated project.
// Uses friend access to exercise the same backend as the GUI; not shipped with the package.
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dreamy.EditorTools.Package;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Dreamy.EditorTools.Tests
{
    [InitializeOnLoad]
    public static class PackageManagerConsumerSmoke
    {
        private const string StatePath = "Library/PackageManagerSmoke.json";
        private const string ResultPath = "PackageManagerSmoke.json";
        private static bool ticking;
        private static double nextTick;
        private sealed class State { public int Phase; public string Started; public List<string> Passed = new(); public List<string> Degraded = new(); }
        static PackageManagerConsumerSmoke() { EditorApplication.update += Tick; }
        public static void Start()
        {
            File.WriteAllText(StatePath, JsonConvert.SerializeObject(new State { Started = DateTime.UtcNow.ToString("O") }));
        }
        private static async void Tick()
        {
            if (ticking || !File.Exists(StatePath) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < nextTick) return;
            ticking = true;
            try
            {
                var state = JsonConvert.DeserializeObject<State>(File.ReadAllText(StatePath));
                if ((DateTime.UtcNow - DateTime.Parse(state.Started).ToUniversalTime()).TotalMinutes > 6) throw new Exception("Smoke timed out.");
                if (PackageOperations.Busy) return;
                var fixture = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText("Fixture.json"));
                switch (state.Phase)
                {
                    case 0:
                        using (var provider = new PackageReleases())
                        {
                            var result = await provider.Check(new CatalogEntry { Id = "com.dreamy.core", Repository = "https://github.com/Dreamy-Game-Foundation/com.dreamy.core" }, true);
                            if (result.Latest(false) == null || result.Error != null) state.Degraded.Add("Live GitHub: " + result.Error);
                            else state.Passed.Add("Live GitHub core tag/version/metadata: " + result.Latest(false).Version);
                        }
                        using (var provider = new PackageReleases())
                        {
                            var result = await provider.Check(new CatalogEntry { Id = "com.unity.ugui", Source = "registry" }, true);
                            if (result.Latest(false) == null || result.Error != null) state.Degraded.Add("Live registry: " + result.Error);
                            else state.Passed.Add("Live registry exact-version metadata: " + result.Latest(false).Version);
                        }
                        Save(state, 1);
                        break;
                    case 1:
                        Save(state, 2);
                        Apply("com.dreamy.manager-fixture", "1.0.0", fixture["v1"], fixture["url"]);
                        break;
                    case 2:
                        Verify("com.dreamy.manager-fixture", "1.0.0", fixture["v1"]);
                        state.Passed.Add("Real Git install, source/version/hash verified across compilation/reload");
                        Save(state, 3);
                        Apply("com.dreamy.manager-fixture", "1.1.0", fixture["v2"], fixture["url"]);
                        break;
                    case 3:
                        Verify("com.dreamy.manager-fixture", "1.1.0", fixture["v2"]);
                        state.Passed.Add("Real Git update, source/version/hash verified across compilation/reload");
                        if (!PackageOperations.CanRestore) throw new Exception("Restore snapshot not offered.");
                        Save(state, 4);
                        PackageOperations.Restore();
                        break;
                    case 4:
                        Verify("com.dreamy.manager-fixture", "1.0.0", fixture["v1"]);
                        state.Passed.Add("Guarded restore resolves previous pinned Git version");
                        Save(state, 5);
                        PackageOperations.Remove("com.dreamy.manager-fixture");
                        break;
                    case 5:
                        if (InstalledPackage.Read().ContainsKey("com.dreamy.manager-fixture")) throw new Exception("Remove left package installed.");
                        state.Passed.Add("Real UPM remove verified");
                        Save(state, 6);
                        Apply("com.dreamy.editor-tools", "1.1.1", fixture["selfSha"], fixture["selfUrl"]);
                        break;
                    case 6:
                        Verify("com.dreamy.editor-tools", "1.1.1", fixture["selfSha"]);
                        state.Passed.Add("Self source-switch/update survives its own assembly reload and verifies pinned Git commit");
                        File.WriteAllText(ResultPath, JsonConvert.SerializeObject(new { status = "pass", state.Passed, state.Degraded, unity = Application.unityVersion }, Formatting.Indented));
                        File.Delete(StatePath);
                        EditorApplication.Exit(0);
                        break;
                }
            }
            catch (Exception e)
            {
                File.WriteAllText(ResultPath, JsonConvert.SerializeObject(new { status = "fail", error = e.ToString(), backend = PackageOperations.Status }, Formatting.Indented));
                File.Delete(StatePath);
                EditorApplication.Exit(1);
            }
            finally { ticking = false; nextTick = EditorApplication.timeSinceStartup + 1; }
        }
        private static void Save(State state, int phase) { state.Phase = phase; File.WriteAllText(StatePath, JsonConvert.SerializeObject(state)); }
        private static void Apply(string id, string version, string sha, string url)
        {
            var plan = new PackageChangePlan
            {
                ManifestBefore = File.ReadAllText("Packages/manifest.json"),
                LockBefore = File.Exists("Packages/packages-lock.json") ? File.ReadAllText("Packages/packages-lock.json") : null,
                Changes = new List<PackageChange> { new() { Id = id, Version = version, Revision = sha, Value = url + "#" + sha, Source = "git", Reason = "Isolated consumer smoke fixture" } }
            };
            PackageOperations.Apply(plan);
        }
        private static void Verify(string id, string version, string sha)
        {
            var p = InstalledPackage.Read()[id];
            if (p.Version != version || p.Source != "Git" || p.Revision != sha || !PackageOperations.Status.StartsWith("UPM completed"))
                throw new Exception(id + " verification failed: " + p.Version + " " + p.Source + " " + p.Revision + "; " + PackageOperations.Status);
        }
    }
}
