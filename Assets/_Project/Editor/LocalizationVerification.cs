using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
namespace Dreamy.Template.Editor
{
    // Native TestRunner API fallback when the bridge loses a job across domain reload.
    [InitializeOnLoad]
    public static class LocalizationVerification
    {
        private const string Active = "Dreamy.Localization.NativeTestRun";
        static LocalizationVerification()
        {
            TestRunnerApi.RegisterTestCallback(new Results());
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Active+".Restore",false)) return;
                EditorApplication.delayCall += () => EditorApplication.delayCall += () =>
                {
                    EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Active+".Options",0);
                    EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Active+".Enabled",true);
                    SessionState.SetBool(Active+".Restore",false);
                };
            };
        }
        public static string RunPlayMode()
        {
            SessionState.SetInt(Active+".Options",(int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(Active+".Enabled",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetBool(Active+".Restore",true);
            EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.None;
            EditorSettings.enterPlayModeOptionsEnabled=false;
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=null;
            SessionState.SetString(Active+".Mode","playmode");
            SessionState.SetBool(Active,true);
            var api=ScriptableObject.CreateInstance<TestRunnerApi>();
            var id=api.Execute(new ExecutionSettings(new Filter {testMode=TestMode.PlayMode,assemblyNames=new[]{"Dreamy.Template.Localization.Tests"}}));
            SessionState.SetString(Active+".Id",id); return id;
        }
        public static string RunEditMode()
        {
            SessionState.SetString(Active+".Mode","editmode");
            SessionState.SetBool(Active,true);
            var api=ScriptableObject.CreateInstance<TestRunnerApi>();
            return api.Execute(new ExecutionSettings(new Filter {testMode=TestMode.EditMode,assemblyNames=new[]{"Dreamy.Localization.Tests.EditMode","Dreamy.Template.Localization.Editor.Tests"}}));
        }
        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                if(!SessionState.GetBool(Active,false)) return;
                Directory.CreateDirectory("docs/evidence/localization/phase-07");
                TestRunnerApi.SaveResultToFile(result,"docs/evidence/localization/phase-07/"+SessionState.GetString(Active+".Mode","playmode")+".xml");
                SessionState.SetBool(Active,false);
                Debug.Log("Localization native tests finished: "+result.TestStatus);
            }
        }
    }
}
