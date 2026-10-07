using System;
using System.Linq;
using Dreamy.Template.Demo;
using Dreamy.UI;
using Dreamy.Feature.Economy.Integration;
using Dreamy.Feature.Missions.Integration;
using Dreamy.Feature.Progression.Integration;
using Dreamy.Feature.LuckyWheel.Integration;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Editor
{
    /// <summary>Updates only the foundation and feature sample assets named below.</summary>
    public static class FoundationFeatureDemoBuilder
    {
        public const string FoundationPath = "Assets/_Project/Prefabs/Panel/FoundationDemoPanel.prefab";
        public const string HostPath = "Assets/_Project/Prefabs/Demo/FoundationDemo.prefab";
        public const string MissionPath = "Assets/Samples/Dreamy Missions/0.1.0/Mission Feature/Prefabs/MissionDemoPanel.prefab";
        public const string ProgressionPath = "Assets/Samples/Dreamy Progression/0.1.1/Basic Progression/UI/ProgressionDemoPanel.prefab";
        public const string EconomyPath = "Assets/Samples/Dreamy Economy Contracts/0.2.0/Wallet Demo/EconomyDemoPanel.prefab";
        public const string WheelPath = "Assets/Samples/Dreamy Lucky Wheel/0.1.0/Lucky Wheel Feature/Prefabs/LuckyWheelPanel.prefab";
        private static TMP_FontAsset font;

        [MenuItem("Dreamy/Sandbox/Build Foundation Feature Demos")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null) throw new InvalidOperationException("Close Prefab Stage before building demo assets.");
            font = AssetDatabase.LoadAssetAtPath<GameObject>(FoundationPath).GetComponentInChildren<TMP_Text>(true).font;
            BuildMission();
            BuildProgression();
            BuildEconomy();
            UpdateFoundation();
            UpdateHost();
            AssetDatabase.SaveAssets();
            Debug.Log("[FoundationDemo] Sample prefabs and Foundation buttons built.");
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj.GetComponent<RectTransform>();
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }
        private static LayoutElement Height(GameObject obj, float height)
        {
            var layout = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            layout.minHeight = height; layout.preferredHeight = height; layout.flexibleHeight = 0;
            return layout;
        }
        private static TMP_Text Label(string name, Transform parent, string text, float height = 72)
        {
            var rect = Rect(name, parent);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.text = text; label.fontSize = 36;
            label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = 36;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white; label.raycastTarget = false;
            Height(rect.gameObject, height);
            return label;
        }
        private static Button Button(string name, Transform parent, string text)
        {
            var rect = Rect(name, parent);
            rect.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.35f, 0.55f, 1f);
            var button = rect.gameObject.AddComponent<Button>();
            var label = Label("Label", rect, text);
            Stretch((RectTransform)label.transform);
            Height(rect.gameObject, 100);
            return button;
        }
        private static VerticalLayoutGroup Vertical(GameObject obj, int padding = 16)
        {
            var layout = obj.GetComponent<VerticalLayoutGroup>() ?? obj.AddComponent<VerticalLayoutGroup>();
            layout.enabled = true;
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = 16; layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            return layout;
        }
        private static void Set(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            var field = so.FindProperty(property);
            if (field == null) throw new InvalidOperationException(target.GetType().Name + "." + property);
            field.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static GameObject Panel<T>(string name, out T panel, out RectTransform content) where T : UIPanel
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            Stretch(root.GetComponent<RectTransform>());
            root.AddComponent<Image>().color = new Color(0.025f, 0.045f, 0.075f, 0.98f);
            panel = root.AddComponent<T>();
            Set(panel, "canvasGroup", root.GetComponent<CanvasGroup>());
            var safe = Rect("SafeArea", root.transform); Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();
            content = Rect("Content", safe); Stretch(content);
            content.offsetMin = new Vector2(40, 40); content.offsetMax = new Vector2(-40, -40);
            Vertical(content.gameObject, 24);
            return root;
        }
        private static void Save(GameObject root, string path)
        {
            try { PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static void BuildEconomy()
        {
            EconomyDemoPanel panel; RectTransform content;
            var root = Panel("EconomyDemoPanel", out panel, out content);
            Label("Title", content, "Economy / Shared Wallet");
            Set(panel, "balancesText", Label("Balances", content, "Shared resource balances", 240));
            Set(panel, "statusText", Label("Status", content, "Grant coins, then retry the same transaction", 100));
            Set(panel, "grantButton", Button("GrantButton", content, "Grant 100 Coins"));
            Set(panel, "retryButton", Button("RetryButton", content, "Retry Last Grant"));
            Set(panel, "closeButton", Button("CloseButton", content, "Back"));
            Save(root, EconomyPath);
        }
        private static void BuildProgression()
        {
            ProgressionDemoPanel panel; RectTransform content;
            var root = Panel("ProgressionDemoPanel", out panel, out content);
            Label("Title", content, "Progression");
            var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Dreamy Progression/0.1.1/Basic Progression/UI/ProgressionHud.prefab");
            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, content);
            Height(hud, 200);
            Set(panel, "hud", hud.GetComponent<ProgressionHud>());
            Set(panel, "statusText", Label("Status", content, "Complete a stage to advance", 100));
            Set(panel, "advanceButton", Button("AdvanceButton", content, "Complete Stage"));
            Set(panel, "closeButton", Button("CloseButton", content, "Back"));
            Save(root, ProgressionPath);
        }
        private static void BuildMission()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Dreamy Missions/0.1.0/Mission Feature/Prefabs/MissionPanel.prefab");
            var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            root.name = "MissionDemoPanel";
            var content = root.transform.Find("SafeArea/Content");
            var layout = Vertical(content.gameObject);
            var contentRect = (RectTransform)content;
            Stretch(contentRect);
            contentRect.offsetMin = new Vector2(40, 40); contentRect.offsetMax = new Vector2(-40, -40);
            root.transform.Find("Backdrop").GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.075f, 0.98f);
            Height(content.Find("Heading").gameObject, 72);
            Height(content.Find("Close").gameObject, 100);
            content.Find("Close").GetComponentInChildren<TMP_Text>().text = "Back";
            Height(content.Find("Status").gameObject, 80);
            var missionScroll = content.Find("MissionScroll").GetComponent<ScrollRect>();
            var scrollSize = Height(missionScroll.gameObject, 300);
            scrollSize.preferredHeight = 500; scrollSize.flexibleHeight = 1;
            missionScroll.scrollSensitivity = 60;
            var controls = root.AddComponent<MissionDemoControls>();
            var win = Button("WinStageButton", content, "Simulate Stage Win"); win.transform.SetSiblingIndex(3);
            var collect = Button("CollectCoinsButton", content, "Simulate Collect 100 Coins"); collect.transform.SetSiblingIndex(4);
            Set(controls, "winStageButton", win); Set(controls, "collectCoinsButton", collect);
            Set(controls, "statusText", content.Find("Status").GetComponent<TMP_Text>());
            Save(root, MissionPath);
        }
        private static void UpdateFoundation()
        {
            var root = PrefabUtility.LoadPrefabContents(FoundationPath);
            try
            {
                var panel = root.GetComponent<FoundationDemoPanel>();
                var closeRect = (RectTransform)root.transform.Find("SafeArea/CloseButton");
                closeRect.anchorMin = Vector2.one; closeRect.anchorMax = Vector2.one;
                closeRect.pivot = new Vector2(0.5f, 0.5f);
                closeRect.anchoredPosition = new Vector2(-50, -50);
                closeRect.sizeDelta = new Vector2(80, 80);
                var card = (RectTransform)root.transform.Find("SafeArea/Card");
                Stretch(card); card.offsetMin = new Vector2(40, 100); card.offsetMax = new Vector2(-40, -80);
                Vertical(card.gameObject, 24);
                // Layout owns positions/sizes; retain fade/scale effects without competing move/size tweens.
                foreach (var tween in card.GetComponentsInChildren<UITweenBase>(true))
                    if (tween is UITweenMove || tween is UITweenSize || tween is UITweenRotate)
                        UnityEngine.Object.DestroyImmediate(tween);
                var group = card.GetComponentsInChildren<RectTransform>(true).Single(t => t.name == "BtnGroup");
                var buttons = group.GetComponentsInChildren<Button>(true).ToList();
                string[] names = { "Missions", "Progression", "LuckyWheel", "Tutorial", "Economy", "Feedback" };
                string[] captions = { "Missions", "Progression", "Lucky Wheel", "Tutorial Demo", "Economy", "Feedback" };
                for (int i = 0; i < names.Length; i++)
                {
                    var existing = group.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == names[i] + "Button");
                    var button = existing ?? Button(names[i] + "Button", group, captions[i]);
                    Set(panel, "open" + names[i] + "Button", button);
                    if (!buttons.Contains(button)) buttons.Add(button);
                }
                foreach (var button in buttons)
                {
                    button.transform.SetParent(group, false);
                    var label = button.GetComponentInChildren<TMP_Text>();
                    if (label != null) { label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = 36; }
                    Height(button.gameObject, 100);
                }
                // Rebuild only layout containers; retain all existing button objects and tutorial target references.
                foreach (Transform child in group.Cast<Transform>().ToArray())
                    if (child.GetComponent<Button>() == null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                foreach (var old in group.GetComponents<LayoutGroup>()) UnityEngine.Object.DestroyImmediate(old);
                foreach (var old in group.GetComponents<ContentSizeFitter>()) UnityEngine.Object.DestroyImmediate(old);
                var scroll = (RectTransform)card.Find("FeatureScroll");
                if (scroll == null) scroll = Rect("FeatureScroll", card);
                var scrollHeight = Height(scroll.gameObject, 300); scrollHeight.flexibleHeight = 1;
                var viewport = (RectTransform)scroll.Find("Viewport");
                if (viewport == null) viewport = Rect("Viewport", scroll);
                Stretch(viewport);
                if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();
                var image = scroll.GetComponent<Image>() ?? scroll.gameObject.AddComponent<Image>(); image.color = new Color(0, 0, 0, 0.01f);
                group.SetParent(viewport, false);
                group.anchorMin = new Vector2(0, 1); group.anchorMax = Vector2.one; group.pivot = new Vector2(0.5f, 1);
                group.anchoredPosition = Vector2.zero; group.sizeDelta = Vector2.zero;
                var vertical = Vertical(group.gameObject, 0);
                var fitter = group.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                for (int i = 0; i < buttons.Count; i += 2)
                {
                    var row = Rect("ButtonRow" + i / 2, group);
                    var horizontal = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                    horizontal.spacing = 16; horizontal.childControlWidth = true; horizontal.childControlHeight = true;
                    horizontal.childForceExpandWidth = true; horizontal.childForceExpandHeight = true;
                    Height(row.gameObject, 110);
                    for (int j = i; j < Math.Min(i + 2, buttons.Count); j++)
                    {
                        buttons[j].transform.SetParent(row, false);
                        var e = Height(buttons[j].gameObject, 100); e.minWidth = 0; e.preferredWidth = 0; e.flexibleWidth = 1;
                    }
                }
                var rect = scroll.GetComponent<ScrollRect>() ?? scroll.gameObject.AddComponent<ScrollRect>();
                rect.content = group; rect.viewport = viewport; rect.horizontal = false; rect.vertical = true;
                rect.movementType = ScrollRect.MovementType.Clamped;
                rect.scrollSensitivity = 60;
                var barRect = (RectTransform)scroll.Find("Scrollbar");
                if (barRect == null)
                {
                    barRect = Rect("Scrollbar", scroll);
                    barRect.anchorMin = new Vector2(1, 0); barRect.anchorMax = Vector2.one;
                    barRect.pivot = new Vector2(1, 0.5f); barRect.sizeDelta = new Vector2(14, 0);
                    barRect.anchoredPosition = Vector2.zero;
                    barRect.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.15f, 0.2f);
                    var area = Rect("SlidingArea", barRect); Stretch(area);
                    var handle = Rect("Handle", area); Stretch(handle);
                    var handleImage = handle.gameObject.AddComponent<Image>(); handleImage.color = new Color(0.35f, 0.65f, 0.8f);
                    var bar = barRect.gameObject.AddComponent<Scrollbar>();
                    bar.handleRect = handle; bar.targetGraphic = handleImage; bar.direction = Scrollbar.Direction.BottomToTop;
                }
                rect.verticalScrollbar = barRect.GetComponent<Scrollbar>();
                rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                viewport.offsetMax = new Vector2(-24, 0);
                foreach (string labelName in new[] { "Title", "StatusText", "ScoreText", "HealthText" })
                {
                    var label = card.Find(labelName).GetComponent<TMP_Text>();
                    label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = 36;
                    Height(label.gameObject, labelName == "StatusText" ? 100 : 64);
                }
                PrefabUtility.SaveAsPrefabAsset(root, FoundationPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void UpdateHost()
        {
            var root = PrefabUtility.LoadPrefabContents(HostPath);
            try
            {
                var host = root.GetComponent<FoundationFeatureDemos>() ?? root.AddComponent<FoundationFeatureDemos>();
                Set(host, "demoRoot", root.GetComponent<FoundationDemoRoot>());
                Set(host, "missionPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(MissionPath).GetComponent<MissionPanel>());
                Set(host, "progressionPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ProgressionPath).GetComponent<ProgressionDemoPanel>());
                Set(host, "economyPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EconomyPath).GetComponent<EconomyDemoPanel>());
                Set(host, "luckyWheelPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(WheelPath).GetComponent<LuckyWheelPanel>());
                PrefabUtility.SaveAsPrefabAsset(root, HostPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
