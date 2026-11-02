using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dreamy.Audio;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Localization;
using Dreamy.Localization.Editor.Json;
using Dreamy.Localization.UI;
using Dreamy.Template.Localization;
using Dreamy.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Dreamy.Template.Editor
{
    [InitializeOnLoad]
    public static class LocalizationTestSceneBuilder
    {
        static LocalizationTestSceneBuilder()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool("Dreamy.Localization.TestLaunch.Restore",false)) return;
                EditorApplication.delayCall += () =>
                {
                    var path=SessionState.GetString("Dreamy.Localization.TestLaunch.Start", "");
                    EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(path)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                    EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt("Dreamy.Localization.TestLaunch.Options",0);
                    EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool("Dreamy.Localization.TestLaunch.Enabled",true);
                    SessionState.SetBool("Dreamy.Localization.TestLaunch.Restore",false);
                };
            };
        }
        [MenuItem("Dreamy/Localization/Play Settings Test")]
        public static void PlayTest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save scenes before launching the localization test.");
            var previous=EditorSceneManager.playModeStartScene;
            SessionState.SetString("Dreamy.Localization.TestLaunch.Start",previous==null?"":AssetDatabase.GetAssetPath(previous));
            SessionState.SetInt("Dreamy.Localization.TestLaunch.Options",(int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool("Dreamy.Localization.TestLaunch.Enabled",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetBool("Dreamy.Localization.TestLaunch.Restore",true);
            EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.None;
            EditorSettings.enterPlayModeOptionsEnabled=false;
            EditorApplication.isPlaying=true;
        }
        [MenuItem("Dreamy/Localization/Configure Main Bootstrap")]
        public static void ConfigureMainBootstrap()
        {
            const string path = "Assets/_Project/Scenes/BootstrapScene.unity";
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            if (scene.isDirty) throw new InvalidOperationException("Save BootstrapScene changes before configuration.");
            try
            {
                var installer = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<GameInstaller>(true)).Single();
                Assign(installer,"localizationProfile",AssetDatabase.LoadAssetAtPath<DreamyLocalizationProfile>(Base + "/Profiles/Runtime.asset"));
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene,true); }
        }
        [MenuItem("Dreamy/Localization/Configure Main Labels")]
        public static void ConfigureMainLabels()
        {
            const string path="Assets/_Project/Prefabs/Panel/FoundationDemoPanel.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Base+"/Generated/LocalizationFont.asset");
                foreach(var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    string key=text.text=="Dreamy Foundation Demo"?"main.title":text.text=="Open Settings"?"demo.open_settings":null;
                    if(key==null) continue;
                    text.font=font; Bind(text,key);
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally {PrefabUtility.UnloadPrefabContents(root);}
        }
        public const string Base = "Assets/_Project/Localization";
        public const string Table = "DreamyGame";
        public const string ScenePath = "Assets/_Project/Scenes/LocalizationSettingsTest.unity";
        public const string SettingsPath = "Assets/Samples/Dreamy Settings/0.2.0/Settings Feature/Prefabs/SettingsPanel.prefab";
        [MenuItem("Dreamy/Localization/Build Settings Test Scene")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play Mode before building the scene.");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save dirty scenes before generating the localization scene.");
            Directory.CreateDirectory(Base + "/Generated"); Directory.CreateDirectory(Base + "/Profiles");
            AssetDatabase.Refresh();
            var import = AssetDatabase.LoadAssetAtPath<LocalizationJsonImportProfile>(Base + "/Profiles/JsonImport.asset");
            if (import == null)
            {
                import = ScriptableObject.CreateInstance<LocalizationJsonImportProfile>();
                AssetDatabase.CreateAsset(import, Base + "/Profiles/JsonImport.asset");
            }
            import.SourceLocale = "en"; import.FallbackLocale = "en"; import.Table = Table;
            import.OutputDirectory = Base + "/Generated";
            import.LocaleFiles = new List<TextAsset> { AssetDatabase.LoadAssetAtPath<TextAsset>(Base + "/Source/en.json"), AssetDatabase.LoadAssetAtPath<TextAsset>(Base + "/Source/vi.json") };
            import.SmartKeys = new List<string> { "demo.reward" };
            EditorUtility.SetDirty(import); AssetDatabase.SaveAssetIfDirty(import);
            LocalizationJsonImporter.Apply(LocalizationJsonImporter.Preview(import));
            var profile = AssetDatabase.LoadAssetAtPath<DreamyLocalizationProfile>(Base + "/Profiles/Runtime.asset");
            if (profile == null) { profile = ScriptableObject.CreateInstance<DreamyLocalizationProfile>(); AssetDatabase.CreateAsset(profile, Base + "/Profiles/Runtime.asset"); }
            var data = new SerializedObject(profile);
            data.FindProperty("useDefaultSmartFormatterAtRuntime").boolValue = true;
            data.FindProperty("defaultLocaleCode").stringValue = "en";
            data.FindProperty("fallbackLocaleCode").stringValue = "en";
            data.FindProperty("preferenceKey").stringValue = "dreamy.localization.locale";
            var tables = data.FindProperty("tables"); tables.arraySize = 1;
            tables.GetArrayElementAtIndex(0).FindPropertyRelative("table").stringValue = Table;
            tables.GetArrayElementAtIndex(0).FindPropertyRelative("policy").enumValueIndex = 1;
            data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(profile);
            var font = CreateFont();
            CreateStandaloneSettingsPrefab();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); camera.tag = "MainCamera";
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = new Color(.035f,.055f,.10f);
            new GameObject("Directional Light", typeof(Light)).GetComponent<Light>().type = LightType.Directional;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var canvas = new GameObject("Localization Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = .5f;
            var safe = Rect("Safe Area", canvas.transform, Vector2.zero, Vector2.zero); Stretch(safe); safe.gameObject.AddComponent<SafeArea>();
            var content = Rect("Demo Content", safe, Vector2.zero, Vector2.zero); Stretch(content);
            Text("Title", content, new Vector2(0,660), new Vector2(900,130), font, "demo.title", 54);
            Text("Greeting", content, new Vector2(0,490), new Vector2(900,100), font, "demo.greeting", 38);
            var reward = Text("Reward", content, new Vector2(0,330), new Vector2(900,100), font, "demo.reward", 40).GetComponent<LocalizedTmpText>();
            var open = Button("Open Settings", content, new Vector2(0,80), font, "demo.open_settings");
            var increment = Button("Increment Amount", content, new Vector2(0,-70), font, "demo.increment");
            var reset = Button("Reset Saved Language", content, new Vector2(0,-220), font, "demo.reset");
            var code = Text("Current Locale", content, new Vector2(0,-420), new Vector2(900,70), font, null, 32);
            var overlay = Rect("Settings Overlay", safe, Vector2.zero, Vector2.zero); Stretch(overlay);
            var root = new GameObject("LocalizationTestRoot").AddComponent<LocalizationTestRoot>();
            Assign(root,"profile",profile); Assign(root,"settingsPrefab",AssetDatabase.LoadAssetAtPath<GameObject>(StandaloneSettingsPath));
            Assign(root,"settingsParent",overlay);
            Assign(root,"content",content.gameObject); Assign(root,"openSettings",open); Assign(root,"incrementAmount",increment);
            Assign(root,"resetPreference",reset); Assign(root,"reward",reward); Assign(root,"localeLabel",code);
            var audio = AssetDatabase.LoadAssetAtPath<DreamyAudioProfile>(Base + "/Profiles/TestAudio.asset");
            if (audio == null)
            {
                audio = ScriptableObject.CreateInstance<DreamyAudioProfile>();
                var audioData = new SerializedObject(audio); audioData.FindProperty("dontDestroyOnLoad").boolValue=false;
                audioData.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(audio, Base + "/Profiles/TestAudio.asset");
            }
            Assign(root,"audioProfile",audio);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Localization scene ready: " + ScenePath);
        }

        public const string StandaloneSettingsPath = Base + "/Generated/StandaloneSettingsView.prefab";
        private static void CreateStandaloneSettingsPrefab()
        {
            var contents = PrefabUtility.LoadPrefabContents(SettingsPath);
            try
            {
                contents.name = "Standalone Settings View";
                var panel = contents.GetComponent<SettingsPanel>();
                var source = new SerializedObject(panel);
                var view = contents.AddComponent<StandaloneSettingsView>();
                var target = new SerializedObject(view);
                foreach (var field in new[] { "musicToggle", "sfxToggle", "hapticsToggle", "gdprButton", "restorePurchasesButton", "openRateUsButton", "closeButton", "languageDropdown", "languageError" })
                    target.FindProperty(field).objectReferenceValue = source.FindProperty(field).objectReferenceValue;
                target.ApplyModifiedPropertiesWithoutUndo();
                UnityEngine.Object.DestroyImmediate(panel);
                contents.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(contents, StandaloneSettingsPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }
        [MenuItem("Dreamy/Localization/Separate Test Scene From Panel Manager")]
        public static void SeparateTestScene()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(ScenePath);
            if (scene.isDirty) throw new InvalidOperationException("Save the test scene before migration.");
            CreateStandaloneSettingsPrefab();
            var root = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LocalizationTestRoot>(true)).Single();
            RectTransform overlay = null;
            foreach (var manager in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PanelManager>(true)).ToArray())
            {
                overlay = manager.GetComponent<RectTransform>();
                manager.gameObject.name = "Settings Overlay";
                UnityEngine.Object.DestroyImmediate(manager);
            }
            if (overlay == null)
            {
                var data = new SerializedObject(root);
                var content = (GameObject)data.FindProperty("content").objectReferenceValue;
                overlay = Rect("Settings Overlay", content.transform.parent, Vector2.zero, Vector2.zero);
                Stretch(overlay);
            }
            Assign(root,"settingsPrefab",AssetDatabase.LoadAssetAtPath<GameObject>(StandaloneSettingsPath));
            Assign(root,"settingsParent",overlay);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static TMP_FontAsset CreateFont()
        {
            var path = Base + "/Generated/LocalizationFont.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null)
            {
                font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf"));
                font.name = "LocalizationFont"; AssetDatabase.CreateAsset(font,path);
                AssetDatabase.AddObjectToAsset(font.material,font);
                foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas,font);
            }
            var chars = "EnglishTiếng ViệtCài đặtNgôn ngữÂĂĐÊÔƠƯâăđêôơư" + File.ReadAllText(Base + "/Source/vi.json");
            if (!font.TryAddCharacters(chars,out string missing)) throw new InvalidOperationException("Missing font glyphs: " + missing);
            EditorUtility.SetDirty(font); AssetDatabase.SaveAssetIfDirty(font); return font;
        }
        public static void ConfigureSettingsPrefab(TMP_FontAsset font)
        {
            var root = PrefabUtility.LoadPrefabContents(SettingsPath);
            try
            {
                var panel = root.GetComponent<SettingsPanel>();
                var card = root.GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Card");
                card.sizeDelta = new Vector2(700,1080);
                card.GetComponent<Image>().color = new Color(.10f,.11f,.18f,1f);
                if (root.GetComponent<SafeArea>() == null) root.AddComponent<SafeArea>();
                var labels = new Dictionary<string,string> { ["Settings"]="settings.title",["Music"]="settings.music",["SFX"]="settings.sfx",["Haptics"]="settings.haptics",["Privacy & GDPR"]="settings.privacy",["Restore Purchases"]="settings.restore",["Rate Us"]="settings.rate",["Close"]="settings.close" };
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.font=font;
                    if (labels.TryGetValue(text.text,out var key)) Bind(text,key);
                }
                card.Find("Close Button").GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-440);
                var dropdown = card.Find("Language Selector")?.GetComponent<TMP_Dropdown>();
                if (dropdown == null)
                {
                    var go = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
                    go.name="Language Selector"; go.transform.SetParent(card,false); dropdown=go.GetComponent<TMP_Dropdown>();
                    var rect = (RectTransform)go.transform; rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
                    rect.anchoredPosition=new Vector2(0,-310); rect.sizeDelta=new Vector2(420,70);
                }
                foreach (var text in dropdown.GetComponentsInChildren<TMP_Text>(true)) { text.font=font; text.color=Color.black; text.fontSize=30; }
                if (card.Find("Language Label") == null) Text("Language Label",card,new Vector2(0,-225),new Vector2(420,60),font,"settings.language",32);
                var error=card.Find("Language Error")?.GetComponent<TMP_Text>();
                if (error == null) error=Text("Language Error",card,new Vector2(0,-375),new Vector2(600,60),font,"settings.language_failed",24);
                error.color=new Color(1,.45f,.4f); error.gameObject.SetActive(false);
                Assign(panel,"languageDropdown",dropdown); Assign(panel,"languageError",error);
                PrefabUtility.SaveAsPrefabAsset(root,SettingsPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        public static void Assign(UnityEngine.Object target,string field,UnityEngine.Object value)
        { var so=new SerializedObject(target); so.FindProperty(field).objectReferenceValue=value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform; rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f); rect.anchoredPosition=position; rect.sizeDelta=size; return rect;
        }
        private static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero; }
        private static TMP_Text Text(string name,Transform parent,Vector2 position,Vector2 size,TMP_FontAsset font,string key,float fontSize)
        {
            var go=Rect(name,parent,position,size).gameObject;
            var text=go.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=fontSize;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            if(key!=null) Bind(text,key); return text;
        }
        private static void Bind(TMP_Text text,string key)
        {
            var binding=text.GetComponent<LocalizedTmpText>()??text.gameObject.AddComponent<LocalizedTmpText>();
            binding.enabled=false; Assign(binding,"target",text); binding.SetReference(Table,key); binding.enabled=true;
        }
        private static Button Button(string name,Transform parent,Vector2 position,TMP_FontAsset font,string key)
        {
            var rect=Rect(name,parent,position,new Vector2(740,110)); var image=rect.gameObject.AddComponent<Image>();image.color=new Color(.13f,.30f,.53f);
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var label=Text("Label",rect,Vector2.zero,new Vector2(700,100),font,key,34);return button;
        }
    }
}
