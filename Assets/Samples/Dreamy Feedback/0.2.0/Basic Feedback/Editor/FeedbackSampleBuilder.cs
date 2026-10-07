using System;
using System.IO;
using System.Linq;
using Dreamy.Feedback.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Dreamy.Feedback.Samples.Editor
{
    /// <summary>Creates only a new sample folder; never replaces an existing user scene/prefab.</summary>
    public static class FeedbackSampleBuilder
    {
        [MenuItem("Dreamy/Feedback/Build Basic Demo")]
        public static void Build()
        {
            RequireEditMode();
            string folder = AssetDatabase.GenerateUniqueAssetPath("Assets/DreamyFeedbackDemo");
            BuildAt(folder);
        }
        public static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Exit Play Mode and close Prefab Stage before building sample assets.");
        }
        public static void BuildAt(string folder)
        {
            RequireEditMode();
            if (Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any()) throw new IOException("Sample output exists: " + folder);
            Directory.CreateDirectory(folder); AssetDatabase.Refresh();
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                TMP_FontAsset font = CreateFont(folder);
                Sprite coin = CreateCoin(folder);
                var rig = CreateRig(folder, font, coin);
                var prefab = PrefabUtility.SaveAsPrefabAsset(rig, folder + "/FeedbackRig.prefab");
                Object.DestroyImmediate(rig);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                var canvas = Canvas("DemoCanvas", 100);
                var safe = Rect("SafeArea", canvas.transform); Stretch(safe);
                safe.gameObject.AddComponent<FeedbackSampleSafeArea>();
                var card = Rect("Controls", safe); card.anchorMin = new Vector2(.04f, .03f); card.anchorMax = new Vector2(.96f, .59f); card.offsetMin = card.offsetMax = Vector2.zero;
                card.gameObject.AddComponent<Image>().color = new Color(.035f, .06f, .1f, .97f);
                var content = ScrollContent(card);
                var vertical = content.gameObject.AddComponent<VerticalLayoutGroup>(); vertical.padding = new RectOffset(24, 24, 16, 16); vertical.spacing = 10;
                vertical.childControlWidth = true; vertical.childForceExpandWidth = true; vertical.childControlHeight = true; vertical.childForceExpandHeight = false;
                Label("Heading", content, "DREAMY FEEDBACK", font, 36, 60);
                Label("Description", content, "Seven channels. One scene owner. Stop safely at any time.", font, 24, 65);
                var targetLabel = Label("WorldTarget", safe, "WORLD REWARD TARGET", font, 30, 60);
                var tr = targetLabel.rectTransform; tr.anchorMin = new Vector2(.15f, .75f); tr.anchorMax = new Vector2(.85f, .85f); tr.offsetMin = tr.offsetMax = Vector2.zero;
                var controls = canvas.gameObject.AddComponent<FeedbackDemoControls>();
                string[] names = { "VFX", "Haptic", "Floating Text", "Icon Fly", "Screen Flash", "Camera Shake", "Sequence" };
                var buttons = new Button[names.Length];
                for (int i = 0; i < names.Length; i += 2)
                {
                    var row = Row(content);
                    for (int j = i; j < Math.Min(i + 2, names.Length); j++) buttons[j] = Button(names[j], row, font);
                }
                var lastRow = Row(content); var playAll = Button("Play All", lastRow, font); var stop = Button("Stop / Clear", lastRow, font);
                var toggleRow = Row(content);
                toggleRow.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
                var toggle = toggleRow.gameObject.AddComponent<Toggle>(); toggle.isOn = true;
                var check = Rect("Check", toggleRow); check.gameObject.AddComponent<LayoutElement>().preferredWidth = 45;
                var image = check.gameObject.AddComponent<Image>(); image.color = new Color(.25f, .8f, .7f); toggle.targetGraphic = image; toggle.graphic = image;
                Label("Haptics", toggleRow, "Enable mobile haptics", font, 24, 50).GetComponent<LayoutElement>().flexibleWidth = 1;
                var status = Label("Status", content, "Choose a channel", font, 24, 90);
                var start = Rect("IconStart", safe); Anchor(start, new Vector2(.2f, .68f));
                var end = Rect("IconEnd", safe); Anchor(end, new Vector2(.8f, .9f));
                var destination = end.gameObject.AddComponent<Image>(); destination.sprite = coin; destination.raycastTarget = false;
                Set(controls, "host", instance.GetComponent<FeedbackHost>()); Set(controls, "target", instance.transform.Find("WorldTarget"));
                Set(controls, "iconStart", start); Set(controls, "iconEnd", end); Set(controls, "icon", coin);
                Set(controls, "playAllButton", playAll); Set(controls, "stopButton", stop); Set(controls, "hapticToggle", toggle); Set(controls, "statusText", status);
                var data = new SerializedObject(controls); var array = data.FindProperty("channelButtons"); array.arraySize = buttons.Length;
                for (int i = 0; i < buttons.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i]; data.ApplyModifiedPropertiesWithoutUndo();
                CreateEventSystem();
                if (!EditorSceneManager.SaveScene(scene, folder + "/FeedbackDemo.unity")) throw new IOException("Could not save sample scene.");
                AssetDatabase.SaveAssets(); Validate(folder);
                Debug.Log("[Feedback] Built basic sample: " + folder);
            }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        private static GameObject CreateRig(string folder, TMP_FontAsset font, Sprite coin)
        {
            var rig = new GameObject("FeedbackRig");
            var root = rig.AddComponent<FeedbackRoot>(); var host = rig.AddComponent<FeedbackHost>();
            var world = new GameObject("WorldVfxRoot"); world.transform.SetParent(rig.transform, false);
            var cameraObj = new GameObject("FeedbackCamera", typeof(Camera)); cameraObj.transform.SetParent(rig.transform, false);
            var camera = cameraObj.GetComponent<Camera>(); camera.transform.localPosition = new Vector3(0, 0, -10); camera.orthographic = true; camera.orthographicSize = 4;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.02f, .04f, .07f); cameraObj.tag = "MainCamera";
            var light = new GameObject("FeedbackLight", typeof(Light)); light.transform.SetParent(rig.transform, false); light.GetComponent<Light>().type = LightType.Directional;
            var target = new GameObject("WorldTarget"); target.transform.SetParent(rig.transform, false); target.transform.localPosition = new Vector3(0, 2, 0);
            var effects = Canvas("FeedbackEffects", 300); effects.transform.SetParent(rig.transform, false); Object.DestroyImmediate(effects.GetComponent<GraphicRaycaster>());
            var textRoot = Rect("FloatingTextRoot", effects.transform); Stretch(textRoot);
            var iconRoot = Rect("IconFlyRoot", effects.transform); Stretch(iconRoot);
            var screen = new GameObject("ScreenFeedbackRoot"); screen.transform.SetParent(rig.transform, false);
            Set(root, "worldVfxRoot", world.transform); Set(root, "floatingTextRoot", textRoot); Set(root, "iconFlyRoot", iconRoot); Set(root, "screenFeedbackRoot", screen.transform); Set(root, "cameraRoot", cameraObj.transform);
            var particleObj = new GameObject("RewardParticles", typeof(ParticleSystem), typeof(ParticleVfxInstance));
            var particles = particleObj.GetComponent<ParticleSystem>(); particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main; main.loop = false; main.duration = .3f; main.startLifetime = .7f; main.startSpeed = 2; main.startSize = .12f; main.startColor = new Color(1, .78f, .15f);
            var emission = particles.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, 24) });
            var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Dreamy/Feedback/SampleParticles");
            if (!shader) throw new InvalidOperationException("Import the sample FeedbackParticles.shader first.");
            var material = new Material(shader); AssetDatabase.CreateAsset(material, folder + "/RewardParticles.mat"); renderer.sharedMaterial = material;
            var vfxPrefab = PrefabUtility.SaveAsPrefabAsset(particleObj, folder + "/RewardParticles.prefab"); Object.DestroyImmediate(particleObj);
            var textObj = Rect("RewardText", null); textObj.sizeDelta = new Vector2(300, 100);
            var text = textObj.gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.fontSize = 54; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            var floating = textObj.gameObject.AddComponent<FloatingTextInstance>(); textObj.gameObject.AddComponent<CanvasGroup>(); Set(floating, "text", text);
            var textPrefab = PrefabUtility.SaveAsPrefabAsset(textObj.gameObject, folder + "/RewardText.prefab"); Object.DestroyImmediate(textObj.gameObject);
            var vfx = ScriptableObject.CreateInstance<VfxDatabase>(); AssetDatabase.CreateAsset(vfx, folder + "/VfxDatabase.asset");
            var vs = new SerializedObject(vfx); var ve = vs.FindProperty("entries"); ve.arraySize = 1;
            var v = ve.GetArrayElementAtIndex(0); v.FindPropertyRelative("id").stringValue = "reward"; v.FindPropertyRelative("prefab").objectReferenceValue = vfxPrefab;
            v.FindPropertyRelative("despawnMode").enumValueIndex = (int)VfxDespawnMode.FixedLifetime; v.FindPropertyRelative("fixedLifetime").floatValue = 1; v.FindPropertyRelative("prewarmCount").intValue = 2; vs.ApplyModifiedPropertiesWithoutUndo();
            var db = ScriptableObject.CreateInstance<FloatingTextDatabase>(); AssetDatabase.CreateAsset(db, folder + "/FloatingTextDatabase.asset");
            var ts = new SerializedObject(db); var te = ts.FindProperty("entries"); te.arraySize = 1; var t = te.GetArrayElementAtIndex(0);
            t.FindPropertyRelative("id").stringValue = "default"; t.FindPropertyRelative("prefab").objectReferenceValue = textPrefab.GetComponent<FloatingTextInstance>();
            t.FindPropertyRelative("color").colorValue = new Color(1, .85f, .2f); t.FindPropertyRelative("duration").floatValue = .9f;
            t.FindPropertyRelative("moveOffset").vector3Value = new Vector3(0, 90, 0); t.FindPropertyRelative("startScale").floatValue = 1; t.FindPropertyRelative("endScale").floatValue = 1.15f; t.FindPropertyRelative("prewarmCount").intValue = 2; ts.ApplyModifiedPropertiesWithoutUndo();
            var shake = ScriptableObject.CreateInstance<CameraShakeDatabase>(); AssetDatabase.CreateAsset(shake, folder + "/CameraShakeDatabase.asset");
            var cs = new SerializedObject(shake); var presets = cs.FindProperty("presets"); presets.arraySize = 1; var preset = presets.GetArrayElementAtIndex(0);
            preset.FindPropertyRelative("id").stringValue = "small";
            var opts = preset.FindPropertyRelative("options"); opts.FindPropertyRelative("Duration").floatValue = .2f; opts.FindPropertyRelative("Amplitude").floatValue = .08f; opts.FindPropertyRelative("Frequency").floatValue = 30; cs.ApplyModifiedPropertiesWithoutUndo();
            var sequence = ScriptableObject.CreateInstance<FeedbackSequenceDatabase>(); AssetDatabase.CreateAsset(sequence, folder + "/FeedbackSequenceDatabase.asset");
            var ss = new SerializedObject(sequence); var entries = ss.FindProperty("entries"); entries.arraySize = 1; var entry = entries.GetArrayElementAtIndex(0); entry.FindPropertyRelative("id").stringValue = "reward";
            var actions = entry.FindPropertyRelative("actions"); actions.arraySize = 3;
            var types = new[] { FeedbackSequenceActionType.Vfx, FeedbackSequenceActionType.Haptic, FeedbackSequenceActionType.CameraShake };
            for (int i = 0; i < 3; i++) { var a = actions.GetArrayElementAtIndex(i); a.FindPropertyRelative("type").enumValueIndex = (int)types[i]; a.FindPropertyRelative("id").stringValue = i == 2 ? "small" : "reward"; a.FindPropertyRelative("hapticType").enumValueIndex = (int)HapticType.Light; }
            ss.ApplyModifiedPropertiesWithoutUndo();
            Set(host, "rewardIcon", coin); Set(host, "root", root); Set(host, "vfxDatabase", vfx); Set(host, "floatingTextDatabase", db); Set(host, "cameraShakeDatabase", shake); Set(host, "sequenceDatabase", sequence); Set(host, "worldCamera", camera);
            return rig;
        }
        public static void Validate(string folder)
        {
            var issues = VfxDatabaseValidator.Validate(AssetDatabase.LoadAssetAtPath<VfxDatabase>(folder + "/VfxDatabase.asset"));
            issues.AddRange(FloatingTextDatabaseValidator.Validate(AssetDatabase.LoadAssetAtPath<FloatingTextDatabase>(folder + "/FloatingTextDatabase.asset")));
            issues.AddRange(CameraShakeDatabaseValidator.Validate(AssetDatabase.LoadAssetAtPath<CameraShakeDatabase>(folder + "/CameraShakeDatabase.asset")));
            issues.AddRange(FeedbackSequenceDatabaseValidator.Validate(AssetDatabase.LoadAssetAtPath<FeedbackSequenceDatabase>(folder + "/FeedbackSequenceDatabase.asset"), AssetDatabase.LoadAssetAtPath<VfxDatabase>(folder + "/VfxDatabase.asset"), AssetDatabase.LoadAssetAtPath<FloatingTextDatabase>(folder + "/FloatingTextDatabase.asset"), AssetDatabase.LoadAssetAtPath<CameraShakeDatabase>(folder + "/CameraShakeDatabase.asset")));
            if (issues.Count > 0) throw new InvalidOperationException(string.Join("\n", issues.Select(i => i.Message)));
        }
        public static Canvas Canvas(string name, int order)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = order;
            var scaler = obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
            return canvas;
        }
        public static RectTransform Rect(string name, Transform parent)
        { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); return r; }
        public static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        public static void Anchor(RectTransform r, Vector2 anchor) { r.anchorMin = r.anchorMax = anchor; r.anchoredPosition = Vector2.zero; r.sizeDelta = new Vector2(64, 64); }
        public static TMP_Text Label(string name, Transform parent, string value, TMP_FontAsset font, int size, float height)
        {
            var rect = Rect(name, parent); var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.text = value; text.fontSize = size;
            text.enableAutoSizing = true; text.fontSizeMin = 18; text.fontSizeMax = size; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = height; return text;
        }
        public static RectTransform ScrollContent(RectTransform card)
        {
            var viewport = Rect("Viewport", card); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport); content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero; content.sizeDelta = Vector2.zero;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = card.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 60;
            return content;
        }
        public static RectTransform Row(Transform parent)
        {
            var row = Rect("ButtonRow", parent); var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 12;
            layout.childControlWidth = true; layout.childForceExpandWidth = true; layout.childControlHeight = true; row.gameObject.AddComponent<LayoutElement>().preferredHeight = 85; return row;
        }
        public static Button Button(string caption, Transform parent, TMP_FontAsset font)
        {
            var rect = Rect(caption.Replace(" ", "") + "Button", parent); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.09f, .27f, .39f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            rect.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var label = Label("Label", rect, caption, font, 30, 75); Stretch(label.rectTransform); return button;
        }
        public static void Set(Object obj, string field, Object value)
        { var data = new SerializedObject(obj); var prop = data.FindProperty(field); if (prop == null) throw new ArgumentException(field); prop.objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        public static void CreateEventSystem()
        {
            var obj = new GameObject("EventSystem", typeof(EventSystem));
            obj.AddComponent<StandaloneInputModule>();
        }
        private static Sprite CreateCoin(string folder)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            { float distance = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)); texture.SetPixel(x, y, distance < 29 ? (distance > 24 ? new Color(.8f, .47f, .04f) : new Color(1, .8f, .18f)) : Color.clear); }
            texture.Apply(); string path = folder + "/Coin.png"; File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 100; importer.alphaIsTransparency = true; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static TMP_FontAsset CreateFont(string folder)
        {
            var defaults = TMP_Settings.defaultFontAsset;
            if (!defaults) throw new InvalidOperationException("Import TMP Essential Resources before building the demo.");
            string source = AssetDatabase.GetAssetPath(defaults.sourceFontFile);
            if (string.IsNullOrEmpty(source)) source = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";
            string fontPath = folder + "/DemoFont.ttf";
            if (!AssetDatabase.CopyAsset(source, fontPath)) throw new IOException("Unable to copy sample font: " + source);
            var font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(fontPath)); font.name = "DemoFont SDF";
            font.TryAddCharacters(new string(Enumerable.Range(32, 95).Select(i => (char)i).ToArray()));
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            font.material.shader = Shader.Find("Dreamy/Feedback/SampleText");
            AssetDatabase.CreateAsset(font, folder + "/DemoFont.asset");
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
            AssetDatabase.SaveAssets(); return font;
        }
    }
}
