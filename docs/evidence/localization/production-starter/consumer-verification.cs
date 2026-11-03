using System;
using System.Linq;
using System.IO;
using Cysharp.Threading.Tasks;
using Dreamy.Localization;
using Dreamy.Localization.Editor.Json;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class StarterVerification
{
    private const string Active="Dreamy.ProductionConsumer.Active";
    static StarterVerification()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if(!SessionState.GetBool(Active,false)) return;
            if(s==PlayModeStateChange.EnteredPlayMode) {SessionState.SetFloat(Active+".Deadline",(float)EditorApplication.timeSinceStartup+90);EditorApplication.update+=Poll;}
            if(s==PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Active+".Finished",false)) EditorApplication.delayCall+=()=>EditorApplication.Exit(SessionState.GetInt(Active+".Exit",1));
        };
    }
    public static void Run()
    {
        try
        {
            var dir="Assets/Game/Localization";
            var profile=AssetDatabase.LoadAssetAtPath<LocalizationJsonImportProfile>(dir+"/Profiles/JsonImport.asset");
            var plan=LocalizationJsonImporter.Preview(profile);
            if(plan.Locales.Count!=12 || plan.EntryCount!=18)throw new Exception("Invalid sample coverage");
            LocalizationJsonImporter.Apply(plan);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));new GameObject("Light",typeof(Light)).GetComponent<Light>().type=LightType.Directional;
            var canvas=new GameObject("Consumer Canvas",typeof(RectTransform),typeof(Canvas));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(dir+"/Prefabs/LocalizationBootstrap.prefab"));
            var label=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(dir+"/Prefabs/LocalizedLabel.prefab"));label.transform.SetParent(canvas.transform,false);
            EditorSceneManager.SaveScene(scene,"Assets/Consumer.unity");
            PlayerPrefs.SetString("dreamy.localization.locale","en");
            SessionState.SetBool(Active,true);SessionState.SetInt(Active+".Step",0);EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.playModeStartScene=null;EditorApplication.isPlaying=true;
        }
        catch(Exception e){File.WriteAllText("receipt.json",JsonUtility.ToJson(new Receipt{passed=false,detail=e.ToString()},true));Debug.LogException(e);EditorApplication.Exit(1);}
    }
    private static void Poll()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Active+".Deadline",0))throw new Exception("Consumer timed out");
            if(!DreamyLocalization.IsInitialized)return;
            var label=UnityEngine.Object.FindFirstObjectByType<TMP_Text>();if(label==null)return;
            var step=SessionState.GetInt(Active+".Step",0);
            if(step==0 && label.text=="Settings"){SessionState.SetInt(Active+".Step",1);DreamyLocalization.SetLocaleAsync("fr").Forget();}
            else if(step==1 && label.text=="Paramètres"){SessionState.SetInt(Active+".Step",2);DreamyLocalization.SetLocaleAsync("ja").Forget();}
            else if(step==2 && label.text=="設定")
            {
                foreach(char c in label.text)if(!label.font.HasCharacter(c,true,true))throw new Exception("Missing glyph "+c);
                File.WriteAllText("receipt.json",JsonUtility.ToJson(new Receipt{passed=true,version="0.2.0",locales=DreamyLocalization.AvailableLocaleCodes.Count,detail="Fresh consumer: import 12x18; sample bootstrap/label applied en -> fr -> ja; no Settings package installed."},true));
                Finish(0);
            }
        }
        catch(Exception e){File.WriteAllText("receipt.json",JsonUtility.ToJson(new Receipt{passed=false,detail=e.ToString()},true));Debug.LogException(e);Finish(1);}
    }
    private static void Finish(int exit){EditorApplication.update-=Poll;SessionState.SetInt(Active+".Exit",exit);SessionState.SetBool(Active+".Finished",true);EditorApplication.isPlaying=false;}
    [Serializable]private sealed class Receipt{public bool passed;public string version;public int locales;public string detail;}
}
