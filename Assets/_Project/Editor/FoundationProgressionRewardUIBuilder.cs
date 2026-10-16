using System;
using System.Linq;
using Dreamy.Template.Demo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Editor
{
    public static class FoundationProgressionRewardUIBuilder
    {
        [MenuItem("Tools/Dreamy/Sandbox/Install Progression Reward Controls")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit PlayMode first.");
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Close Prefab Stage first.");
            const string path = FoundationFeatureDemoBuilder.ProgressionPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var content = (RectTransform)root.transform.Find("SafeArea/Content");
                if (content == null || root.transform.Find("SafeArea").GetComponent<Dreamy.UI.SafeArea>() == null)
                    throw new InvalidOperationException("Expected SafeArea ownership.");
                if (content.Find("RewardScroll") != null) throw new InvalidOperationException("Reward controls already installed.");
                var font = root.GetComponentInChildren<TMP_Text>(true).font;
                var controls = root.AddComponent<FoundationProgressionPanelControls>();
                var vertical = content.GetComponent<VerticalLayoutGroup>(); vertical.spacing = 8;
                foreach (var child in content.Cast<Transform>())
                {
                    var layout = child.GetComponent<LayoutElement>();
                    if (layout != null) { layout.minHeight = 50; layout.preferredHeight = child.name.Contains("Hud") ? 120 : 50; }
                    foreach (var childLabel in child.GetComponentsInChildren<TMP_Text>(true))
                    { childLabel.fontSizeMax = 30; childLabel.fontSizeMin = 16; }
                }
                var title = content.Find("Title").GetComponent<TMP_Text>(); title.text = "Progression / Multi Rewards";
                var advance = content.Find("AdvanceButton");
                Set(controls,"actionStatus", Label("RewardActionStatus", content, font,"Arm a failure, grant XP, then retry / reload",64));
                var row1 = Row("RewardActions",content);
                Set(controls,"grant300", Button("Grant300",row1,font,"+300 XP"));
                Set(controls,"retryGrant", Button("RetryXP",row1,font,"Retry last XP"));
                Set(controls,"retryRewards", Button("RetryRewards",row1,font,"Retry rewards"));
                var row2 = Row("RewardFailures",content);
                Set(controls,"failCard", Button("FailCard",row2,font,"Fail Card"));
                Set(controls,"failAck", Button("FailAck",row2,font,"Fail Ack Save"));
                Set(controls,"failComplete", Button("FailComplete",row2,font,"Fail Final Save"));
                Set(controls,"recover", Button("ReloadRecover",content,font,"Reload Save + Recover"));
                var scrollRect = Rect("RewardScroll",content);
                var scrollImage=scrollRect.gameObject.AddComponent<Image>(); scrollImage.color=new Color(.035f,.05f,.08f,.95f);
                var size=scrollRect.gameObject.AddComponent<LayoutElement>(); size.minHeight=160; size.preferredHeight=420; size.flexibleHeight=1;
                var scroll=scrollRect.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.scrollSensitivity=50;
                var viewport=Rect("Viewport",scrollRect); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
                var text=Label("RewardEntries",viewport,font,"Pending / Delivered entries",100);
                var textRect=(RectTransform)text.transform; textRect.anchorMin=new Vector2(0,1);textRect.anchorMax=new Vector2(1,1);textRect.pivot=new Vector2(.5f,1);textRect.offsetMin=new Vector2(12,0);textRect.offsetMax=new Vector2(-12,0);
                text.enableAutoSizing=false;text.fontSize=23;text.alignment=TextAlignmentOptions.TopLeft;
                text.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                scroll.viewport=viewport;scroll.content=textRect;scroll.movementType=ScrollRect.MovementType.Clamped;
                Set(controls,"rewardStatus",text);
                content.Find("CloseButton").SetAsLastSibling();
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("Foundation Progression reward UI installed.");
        }
        private static RectTransform Rect(string name,Transform parent)
        { var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform; }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero; }
        private static Transform Row(string name,Transform parent)
        {
            var row=Rect(name,parent);var layout=row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing=8;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;
            var size=row.gameObject.AddComponent<LayoutElement>();size.minHeight=60;size.preferredHeight=60;return row;
        }
        private static TMP_Text Label(string name,Transform parent,TMP_FontAsset font,string text,float height)
        {
            var rect=Rect(name,parent);var label=rect.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.text=text;
            label.fontSize=26;label.enableAutoSizing=true;label.fontSizeMin=14;label.fontSizeMax=26;
            label.alignment=TextAlignmentOptions.Center;label.color=Color.white;label.raycastTarget=false;
            var layout=rect.gameObject.AddComponent<LayoutElement>();layout.minHeight=height;layout.preferredHeight=height;return label;
        }
        private static Button Button(string name,Transform parent,TMP_FontAsset font,string title)
        {
            var rect=Rect(name,parent);var image=rect.gameObject.AddComponent<Image>();image.color=new Color(.16f,.35f,.55f);
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var label=Label("Label",rect,font,title,50);Stretch((RectTransform)label.transform);
            var size=rect.gameObject.AddComponent<LayoutElement>();size.minHeight=60;size.preferredHeight=60;size.flexibleWidth=1;
            return button;
        }
        private static void Set(UnityEngine.Object target,string property,UnityEngine.Object value)
        { var serialized=new SerializedObject(target);serialized.FindProperty(property).objectReferenceValue=value;serialized.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
