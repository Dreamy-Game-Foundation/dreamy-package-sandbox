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
                var profile=AssetDatabase.LoadAssetAtPath<DreamyLocalizationProfile>(ProductionBase + "/Profiles/Runtime.asset")
                    ?? AssetDatabase.LoadAssetAtPath<DreamyLocalizationProfile>(Base + "/Profiles/Runtime.asset");
                Assign(installer,"localizationProfile",profile);
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
                var font=CreateFont();
                var keys=new Dictionary<string,string>
                {
                    ["Title"]="main.title", ["StatusText"]="main.waiting", ["ScoreText"]="main.score", ["HealthText"]="main.health",
                    ["ToggleButton"]="main.toggle", ["AddScoreButton"]="main.add_score", ["DamageButton"]="main.damage",
                    ["HealButton"]="main.heal", ["SaveButton"]="main.save", ["LoadButton"]="main.load", ["ShopButton"]="main.shop",
                    ["DailyRewardButton"]="main.daily_reward", ["SoundButton"]="main.sound", ["SettingsButton"]="demo.open_settings",
                    ["RateUsButton"]="main.rate", ["MissionsButton"]="main.missions", ["ProgressionButton"]="main.progression",
                    ["LuckyWheelButton"]="main.lucky_wheel", ["TutorialButton"]="main.tutorial", ["EconomyButton"]="main.economy",
                    ["FeedbackButton"]="main.feedback", ["OnlineRewardButton"]="main.online_reward"
                };
                foreach(var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if(!keys.TryGetValue(text.name,out var key) && !keys.TryGetValue(text.transform.parent.name,out key))
                        throw new InvalidOperationException("Unmapped Foundation label: "+text.name);
                    text.font=font; text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=Mathf.Max(18,text.fontSize);
                    Bind(text,key);
                    // Arguments are runtime state, not serialized prefab data. The presenter configures these
                    // bindings atomically with their values before starting a Smart String lookup.
                    if(key=="main.score" || key=="main.health")text.GetComponent<LocalizedTmpText>().ClearReference();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally {PrefabUtility.UnloadPrefabContents(root);}
        }
        public const string Base = "Assets/_Project/Localization";
        public const string ProductionBase = Base + "/Production";
        public static string SourceDirectory => Directory.Exists(ProductionBase+"/Source") ? ProductionBase+"/Source" : Base+"/Source";
        public static string ImportProfilePath => AssetDatabase.LoadAssetAtPath<LocalizationJsonImportProfile>(ProductionBase+"/Profiles/JsonImport.asset") != null
            ? ProductionBase+"/Profiles/JsonImport.asset" : Base+"/Profiles/JsonImport.asset";
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
            var import = AssetDatabase.LoadAssetAtPath<LocalizationJsonImportProfile>(ImportProfilePath);
            if (import == null)
            {
                import = ScriptableObject.CreateInstance<LocalizationJsonImportProfile>();
                AssetDatabase.CreateAsset(import, Base + "/Profiles/JsonImport.asset");
            }
            import.SourceLocale = "en"; import.FallbackLocale = "en"; import.Table = Table;
            import.OutputDirectory = Base + "/Generated";
            import.LocaleFiles = FindLocaleFiles();
            import.SmartKeys = FoundationSmartKeys();
            EditorUtility.SetDirty(import); AssetDatabase.SaveAssetIfDirty(import);
            LocalizationJsonImporter.Apply(LocalizationJsonImporter.Preview(import));
            var profile = AssetDatabase.LoadAssetAtPath<DreamyLocalizationProfile>(ProductionBase+"/Profiles/Runtime.asset")
                ?? AssetDatabase.LoadAssetAtPath<DreamyLocalizationProfile>(Base + "/Profiles/Runtime.asset");
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
            ConfigureSettingsPrefab(font);
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
            // Update an existing view in place: cloning over it can remap duplicate child names/fileIDs.
            if (AssetDatabase.LoadAssetAtPath<GameObject>(StandaloneSettingsPath) != null)
            {
                ConfigureSettingsPrefab(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Base + "/Generated/LocalizationFont.asset"), StandaloneSettingsPath);
                return;
            }
            var contents = PrefabUtility.LoadPrefabContents(SettingsPath);
            try
            {
                contents.name = "Standalone Settings View";
                var panel = contents.GetComponent<SettingsPanel>();
                var source = new SerializedObject(panel);
                var view = contents.AddComponent<StandaloneSettingsView>();
                var target = new SerializedObject(view);
                foreach (var field in new[] { "musicToggle", "sfxToggle", "hapticsToggle", "gdprButton", "restorePurchasesButton", "openRateUsButton", "closeButton", "languageDropdown", "languageError", "languageLabel", "languageIcons" })
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

        private static List<TextAsset> FindLocaleFiles() => Directory.GetFiles(SourceDirectory, "*.json")
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => AssetDatabase.LoadAssetAtPath<TextAsset>(path.Replace('\\', '/'))).ToList();

        [MenuItem("Dreamy/Localization/Refresh Languages And Dropdown")]
        public static void RefreshLanguagesAndDropdown()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            var profile = AssetDatabase.LoadAssetAtPath<LocalizationJsonImportProfile>(ImportProfilePath);
            if (profile == null) throw new InvalidOperationException("Build the localization test scene first.");
            profile.LocaleFiles = FindLocaleFiles();
            profile.SmartKeys = FoundationSmartKeys();
            EditorUtility.SetDirty(profile); AssetDatabase.SaveAssetIfDirty(profile);
            LocalizationJsonImporter.Apply(LocalizationJsonImporter.Preview(profile));
            ConfigureSettingsPrefab(CreateFont());
            CreateStandaloneSettingsPrefab();
            Debug.Log("Localization languages and both Settings dropdowns refreshed without rebuilding the scene.");
        }

        private static TMP_FontAsset CreateFont()
        {
            var production = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ProductionBase + "/Fonts/TMP/LocalizationFont.asset");
            var path = Base + "/Generated/LocalizationFont.asset";
            var font = production ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null)
            {
                font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf"));
                font.name = "LocalizationFont"; AssetDatabase.CreateAsset(font,path);
                AssetDatabase.AddObjectToAsset(font.material,font);
                foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas,font);
            }
            var cjk = production != null ? production.fallbackFontAssetTable[0] : CreateFallbackFont("LocalizationCJK", Base + "/Fonts/LocalizationCJKTest.otf");
            var thai = production != null ? production.fallbackFontAssetTable[1] : CreateFallbackFont("LocalizationThai", Base + "/Fonts/NotoSansThai-Regular.ttf");
            font.fallbackFontAssetTable = new List<TMP_FontAsset> { cjk, thai };
            var chars = string.Concat(FindLocaleFiles().Select(file => file.text + System.Globalization.CultureInfo
                .GetCultureInfo(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(file))).NativeName));
            chars = new string(chars.Where(c => !char.IsControl(c)).Distinct().ToArray());
            var fonts = new[] { font, cjk, thai };
            foreach (var asset in fonts)
            {
                var pending = new string(chars.Where(c => !fonts.Any(f => f.HasCharacter(c))).ToArray());
                if (!string.IsNullOrEmpty(pending)) asset.TryAddCharacters(pending, true);
            }
            // TMP 5 can return the entire input as "missing" when all glyphs already exist.
            // Verify the actual character tables so repeating this command stays idempotent.
            var missing = new string(chars.Where(c => !fonts.Any(f => f.HasCharacter(c))).ToArray());
            if (!string.IsNullOrEmpty(missing)) throw new InvalidOperationException("Missing font glyphs: " + missing);
            foreach (var asset in new[] { cjk, thai })
            { EditorUtility.SetDirty(asset); AssetDatabase.SaveAssetIfDirty(asset); }
            EditorUtility.SetDirty(font); AssetDatabase.SaveAssetIfDirty(font); return font;
        }
        private static List<string> FoundationSmartKeys() => new() { "demo.reward", "main.score", "main.health", "main.ready", "main.score_changed", "main.saved", "main.loaded" };
        private static TMP_FontAsset CreateFallbackFont(string name, string sourcePath)
        {
            var path = Base + "/Generated/" + name + ".asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null) return font;
            var source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
            if (source == null) throw new InvalidOperationException("Font source missing: " + sourcePath);
            font = TMP_FontAsset.CreateFontAsset(source);
            font.name = name; AssetDatabase.CreateAsset(font, path);
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
            return font;
        }
        public static void ConfigureSettingsPrefab(TMP_FontAsset font) => ConfigureSettingsPrefab(font, SettingsPath);
        private static void ConfigureSettingsPrefab(TMP_FontAsset font, string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                UnityEngine.Object panel = root.GetComponent<SettingsPanel>();
                if (panel == null) panel = root.GetComponent<StandaloneSettingsView>();
                var card = root.GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Card");
                card.sizeDelta = new Vector2(700,1080);
                card.GetComponent<Image>().color = new Color(.10f,.11f,.18f,1f);
                if (root.GetComponent<SafeArea>() == null) root.AddComponent<SafeArea>();
                var labels = new Dictionary<string,string> { ["Settings"]="settings.title",["Music"]="settings.music",["SFX"]="settings.sfx",["Haptics"]="settings.haptics",["Privacy & GDPR"]="settings.privacy",["Restore Purchases"]="settings.restore",["Rate Us"]="settings.rate",["Close"]="settings.close" };
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.font=font;
                    text.enableAutoSizing=true; text.fontSizeMin=22; text.fontSizeMax=Mathf.Max(22,text.fontSize);
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
                ConfigureLanguageDropdown(dropdown, font);
                if (card.Find("Language Label") == null) Text("Language Label",card,new Vector2(0,-225),new Vector2(420,60),font,"settings.language",32);
                var error=card.Find("Language Error")?.GetComponent<TMP_Text>();
                if (error == null) error=Text("Language Error",card,new Vector2(0,-375),new Vector2(600,60),font,"settings.language_failed",24);
                error.color=new Color(1,.45f,.4f); error.gameObject.SetActive(false);
                Assign(panel,"languageDropdown",dropdown); Assign(panel,"languageError",error);
                Assign(panel,"languageLabel",card.Find("Language Label").GetComponent<TMP_Text>());
                var icons=AssetDatabase.LoadAssetAtPath<SettingsLanguageIcons>(Base+"/Profiles/LanguageIcons.asset");
                if(icons!=null)
                {
                    Assign(panel,"languageIcons",icons);
                    Dreamy.Localization.SettingsIntegration.Editor.SettingsLocalizationSetup.ConfigureDropdownIcons(dropdown);
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void ConfigureLanguageDropdown(TMP_Dropdown dropdown, TMP_FontAsset font)
        {
            // TMP positions cloned options using the item rect's height, not the text's preferred height.
            // The default 20px item overlaps a 30px label. Reserve 64px for every row and scroll the rest.
            var template = dropdown.template;
            template.sizeDelta = new Vector2(template.sizeDelta.x, 328);
            var content = (RectTransform)template.Find("Viewport/Content");
            content.sizeDelta = new Vector2(content.sizeDelta.x, 72);
            var item = (RectTransform)content.Find("Item");
            item.sizeDelta = new Vector2(item.sizeDelta.x, 64);
            var label = dropdown.itemText.rectTransform;
            label.offsetMin = new Vector2(40, 6); label.offsetMax = new Vector2(-16, -6);
            var scroll = template.GetComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            foreach (var text in dropdown.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font=font; text.color=Color.black; text.fontSize=30;
                text.enableAutoSizing=true; text.fontSizeMin=24; text.fontSizeMax=30;
                text.textWrappingMode=TextWrappingModes.NoWrap; text.overflowMode=TextOverflowModes.Ellipsis;
            }
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
