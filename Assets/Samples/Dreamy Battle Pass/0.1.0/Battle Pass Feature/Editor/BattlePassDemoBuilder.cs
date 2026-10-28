
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Dreamy.Economy.UI;
using Dreamy.UI;
using Dreamy.BattlePass.Sample.Rewards;
namespace Dreamy.BattlePass.Sample.Editor
{
    public static class BattlePassDemoBuilder
    {
        public static string Root
        {
            get
            {
                foreach(var guid in AssetDatabase.FindAssets("BattlePassDemoBuilder t:MonoScript"))
                { string path=AssetDatabase.GUIDToAssetPath(guid);if(path.EndsWith("/Editor/BattlePassDemoBuilder.cs",StringComparison.Ordinal))return path.Substring(0,path.Length-"/Editor/BattlePassDemoBuilder.cs".Length); }
                throw new InvalidOperationException("Import Battle Pass Feature sample first.");
            }
        }
        private static readonly Color Navy=new(0.025f,0.04f,0.08f),Card=new(0.07f,0.10f,0.17f),Gold=new(1,0.76f,0.29f);
        [MenuItem("Dreamy/Battle Pass/Samples/Build Demo")]
        public static void Build()
        {
            if(SceneManager.GetSceneByPath(Root+"/Scenes/BattlePassDemo.unity").isLoaded)throw new InvalidOperationException("Close Battle Pass demo scene before regenerating.");
            RequireBase(PanelBase);RequireBase(ItemBase);
            Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory(Root+"/Scenes");Directory.CreateDirectory(Root+"/Resources/BattlePassPresentation");AssetDatabase.Refresh();
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            try
            {
                var camera=new GameObject("Main Camera",typeof(Camera));camera.tag="MainCamera";camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().backgroundColor=Navy;
                var events=new GameObject("EventSystem",typeof(EventSystem));var input=Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");if(input!=null)events.AddComponent(input);else events.AddComponent<StandaloneInputModule>();
                var canvas=new GameObject("Battle Pass Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(PanelManager));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
                var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=0.5f;
                var screenLayer=Rect("Layer Screen",canvas.transform);Stretch(screenLayer);screenLayer.gameObject.AddComponent<UILayerRoot>().SetLayer(UILayer.Screen);
                var background=Box("Background",screenLayer,Navy);Stretch(background);var safe=Rect("Safe Area",screenLayer);Stretch(safe);safe.gameObject.AddComponent<SafeArea>();
                var heading=Text("Title",safe,"BATTLE PASS LAB",58,Color.white);Place(heading.rectTransform,.08f,.88f,.92f,.97f);
                var subtitle=Text("Subtitle",safe,"OFFLINE PUZZLE  /  FREE + PREMIUM",23,Gold);Place(subtitle.rectTransform,.08f,.83f,.92f,.88f);
                var balance=Text("Balance",safe,"Loading shared Economy...",26,Gold);Place(balance.rectTransform,.08f,.75f,.92f,.82f);
                string[] names={"ENROLL IN SEASON","COMPLETE RUN  +20 XP","REPLAY LAST CALLBACK","ADVANCE DEMO CLOCK  +1 DAY","RELOAD DURABLE OWNERS"};var buttons=new Button[5];
                for(int i=0;i<5;i++){buttons[i]=Button("Action"+i,safe,names[i],Card);Place((RectTransform)buttons[i].transform,.08f,.65f-i*.095f,.92f,.725f-i*.095f);}
                var toggleRect=Box("Confirm Toggle",safe,Card);Place(toggleRect,.08f,.18f,.92f,.24f);var toggle=toggleRect.gameObject.AddComponent<Toggle>();toggle.targetGraphic=toggleRect.GetComponent<Image>();
                var check=Box("Check",toggleRect,Gold);Place(check,.03f,.2f,.08f,.8f);toggle.graphic=check.GetComponent<Image>();toggle.isOn=true;var toggleLabel=Text("Label",toggleRect,"SHOW CLAIM CONFIRMATION PANEL",22,Color.white);Place(toggleLabel.rectTransform,.11f,0,.98f,1);
                var status=Text("Status",safe,"Loading...",22,Color.white);Place(status.rectTransform,.08f,.09f,.92f,.17f);
                var entryRect=FeatureItem("Battle Pass Entry",safe,Gold,true);Place(entryRect,.08f,.015f,.92f,.08f);var entryButton=entryRect.GetComponent<Button>();var entry=entryRect.gameObject.AddComponent<BattlePassEntryView>();
                var entryLabel=Text("Title",entryRect,"OPEN BATTLE PASS",25,Navy);Place(entryLabel.rectTransform,.04f,0,.7f,1);var badge=Text("Badge",entryRect,"0 READY",20,Navy);Place(badge.rectTransform,.72f,0,.98f,1);Set(entry,"button",entryButton);Set(entry,"badge",badge);SaveVariant(entryRect.gameObject,Root+"/Prefabs/BattlePassEntry.prefab");
                // Build concrete reward/tier prefabs through Editor APIs.
                var rewardItem=FeatureItem("Battle Pass Reward",canvas.transform,Card,true);rewardItem.sizeDelta=new Vector2(330,160);var rewardButton=rewardItem.GetComponent<Button>();var rewardView=rewardItem.gameObject.AddComponent<BattlePassRewardItem>();
                var rewardLabel=Text("Label",rewardItem,"FREE\n100 coin\nLOCKED",24,Gold);Stretch(rewardLabel.rectTransform);rewardLabel.alignment=TextAlignmentOptions.Center;Set(rewardView,"label",rewardLabel);Set(rewardView,"button",rewardButton);
                var rewardPrefab=SaveVariant(rewardItem.gameObject,Root+"/Prefabs/BattlePassRewardItem.prefab");UnityEngine.Object.DestroyImmediate(rewardItem.gameObject);
                var tier=FeatureItem("Battle Pass Tier",canvas.transform,new Color(.10f,.14f,.22f),false);tier.sizeDelta=new Vector2(900,180);tier.GetComponent<LayoutElement>().preferredHeight=180;var tierView=tier.gameObject.AddComponent<BattlePassTierItem>();
                var tierLabel=Text("Tier",tier,"TIER 1\n20 XP",24,Color.white);Place(tierLabel.rectTransform,.025f,.05f,.22f,.95f);
                var free=(GameObject)PrefabUtility.InstantiatePrefab(rewardPrefab,tier);Place((RectTransform)free.transform,.24f,.06f,.60f,.94f);
                var premium=(GameObject)PrefabUtility.InstantiatePrefab(rewardPrefab,tier);Place((RectTransform)premium.transform,.62f,.06f,.98f,.94f);
                Set(tierView,"tierLabel",tierLabel);Set(tierView,"freeReward",free.GetComponent<BattlePassRewardItem>());Set(tierView,"premiumReward",premium.GetComponent<BattlePassRewardItem>());
                var tierPrefab=SaveVariant(tier.gameObject,Root+"/Prefabs/BattlePassTierItem.prefab");UnityEngine.Object.DestroyImmediate(tier.gameObject);
                var main=FeaturePanel("Battle Pass Panel",canvas.transform,new Color(0,0,0,.9f));Stretch(main);var mainPanel=main.gameObject.AddComponent<BattlePassPanel>();
                var panelSafe=(RectTransform)main.Find("SafeArea/Content");var mainCard=Box("Card",panelSafe,Card);Place(mainCard,.035f,.04f,.965f,.96f);
                var title=Text("Title",mainCard,"PUZZLE EXPEDITION",40,Color.white);Place(title.rectTransform,.04f,.91f,.76f,.985f);var close=Button("Close",mainCard,"CLOSE",Card);Place((RectTransform)close.transform,.78f,.92f,.97f,.98f);
                var xp=Text("XP",mainCard,"0 / 200 XP",27,Gold);Place(xp.rectTransform,.04f,.855f,.96f,.915f);
                var bar=Box("Progress Background",mainCard,new Color(.15f,.19f,.25f));Place(bar,.04f,.83f,.96f,.845f);var fill=Box("Fill",bar,Gold);Stretch(fill);var fillImage=fill.GetComponent<Image>();fillImage.type=Image.Type.Filled;fillImage.fillMethod=Image.FillMethod.Horizontal;
                var time=Text("Time",mainCard,"ACTIVE",22,Color.white);Place(time.rectTransform,.04f,.78f,.96f,.825f);
                var scrollRect=Rect("Scroll",mainCard);Place(scrollRect,.04f,.19f,.96f,.77f);var scroll=scrollRect.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;
                var viewport=Box("Viewport",scrollRect,new Color(1,1,1,.01f));Stretch(viewport);viewport.gameObject.AddComponent<RectMask2D>();
                var content=Rect("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;
                var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=12;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;
                content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.viewport=viewport;scroll.content=content;
                var mainStatus=Text("Status",mainCard,"Loading...",22,Color.white);Place(mainStatus.rectTransform,.04f,.11f,.96f,.185f);
                var all=Button("Claim All",mainCard,"CLAIM ALL",Gold);Place((RectTransform)all.transform,.04f,.025f,.41f,.10f);
                var unlock=Button("Premium",mainCard,"PREMIUM",new Color(.28f,.22f,.12f));Place((RectTransform)unlock.transform,.43f,.025f,.70f,.10f);
                var recover=Button("Recover",mainCard,"RECOVER",new Color(.12f,.20f,.27f));Place((RectTransform)recover.transform,.72f,.025f,.96f,.10f);
                Set(mainPanel,"titleText",title);Set(mainPanel,"xpText",xp);Set(mainPanel,"timeText",time);Set(mainPanel,"statusText",mainStatus);Set(mainPanel,"progressFill",fillImage);
                Set(mainPanel,"closeButton",close);Set(mainPanel,"claimAllButton",all);Set(mainPanel,"premiumButton",unlock);Set(mainPanel,"recoverButton",recover);Set(mainPanel,"content",content);Set(mainPanel,"tierPrefab",tierPrefab.GetComponent<BattlePassTierItem>());Set(mainPanel,"canvasGroup",main.GetComponent<CanvasGroup>());Set(mainPanel,"tweenPlayer",main.GetComponent<UITweenPlayer>());
                SaveVariant(main.gameObject,Root+"/Prefabs/BattlePassPanel.prefab");
                var popup=FeaturePanel("Premium Popup",canvas.transform,new Color(0,0,0,.92f));Stretch(popup);var premiumPopup=popup.gameObject.AddComponent<BattlePassPremiumPopup>();var premiumSafe=(RectTransform)popup.Find("SafeArea/Content");
                var premiumCard=Box("Card",premiumSafe,Card);Place(premiumCard,.08f,.28f,.92f,.72f);var premiumTitle=Text("Title",premiumCard,"UNLOCK PREMIUM",42,Gold);Place(premiumTitle.rectTransform,.07f,.76f,.93f,.95f);
                var premiumStatus=Text("Status",premiumCard,"200 gems",28,Color.white);Place(premiumStatus.rectTransform,.07f,.34f,.93f,.72f);
                var buy=Button("Buy",premiumCard,"UNLOCK  200 GEMS",Gold);Place((RectTransform)buy.transform,.07f,.17f,.93f,.31f);var premiumClose=Button("Close",premiumCard,"CLOSE",Card);Place((RectTransform)premiumClose.transform,.07f,.025f,.93f,.14f);
                Set(premiumPopup,"buyButton",buy);Set(premiumPopup,"closeButton",premiumClose);Set(premiumPopup,"statusText",premiumStatus);Set(premiumPopup,"canvasGroup",popup.GetComponent<CanvasGroup>());Set(premiumPopup,"tweenPlayer",popup.GetComponent<UITweenPlayer>());SaveVariant(popup.gameObject,Root+"/Prefabs/BattlePassPremiumPopup.prefab");
                RewardPanel receivedPanel;
                {
                var catalog=AssetDatabase.LoadAssetAtPath<ResourceDisplayCatalog>(Root+"/Resources/BattlePassPresentation/DisplayCatalog.asset");
                if(catalog==null){catalog=ScriptableObject.CreateInstance<ResourceDisplayCatalog>();AssetDatabase.CreateAsset(catalog,Root+"/Resources/BattlePassPresentation/DisplayCatalog.asset");}
                var catalogSo=new SerializedObject(catalog);var resources=catalogSo.FindProperty("resources");resources.arraySize=3;
                string[] ids={"currency.coin","item.booster.hammer","currency.gem"},resourceNames={"Coins","Hammer booster","Gems"};
                for(int i=0;i<3;i++){var catalogEntry=resources.GetArrayElementAtIndex(i);catalogEntry.FindPropertyRelative("resourceId").stringValue=ids[i];catalogEntry.FindPropertyRelative("displayName").stringValue=resourceNames[i];}
                catalogSo.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(catalog);
                var receivedItem=FeatureItem("Reward Item",canvas.transform,new Color(0.12f,0.16f,0.23f),false);receivedItem.sizeDelta=new Vector2(600,110);
                receivedItem.GetComponent<LayoutElement>().preferredHeight=110;
                var itemView=receivedItem.gameObject.AddComponent<RewardItemView>();
                var rewardIcon=(RectTransform)receivedItem.Find("Icon");rewardIcon.gameObject.SetActive(true);rewardIcon.GetComponent<Image>().color=Gold;Place(rewardIcon,0.05f,0.22f,0.16f,0.78f);
                var rewardName=Text("Name",receivedItem,"Coins",27,Color.white);Place(rewardName.rectTransform,0.20f,0.1f,0.73f,0.9f);
                var rewardAmount=Text("Amount",receivedItem,"100",32,Gold);Place(rewardAmount.rectTransform,0.73f,0.1f,0.95f,0.9f);rewardAmount.alignment=TextAlignmentOptions.Right;
                Set(itemView,"nameText",rewardName);Set(itemView,"amountText",rewardAmount);Set(itemView,"icon",rewardIcon.GetComponent<Image>());
                var itemAsset=SaveVariant(receivedItem.gameObject,Root+"/Prefabs/RewardItem.prefab");UnityEngine.Object.DestroyImmediate(receivedItem.gameObject);
                var overlay=FeaturePanel("Reward Panel",canvas.transform,new Color(0,0,0,0.82f));Stretch(overlay);
                var rewardPanel=overlay.gameObject.AddComponent<RewardPanel>();
                var rewardPanelSafe=(RectTransform)overlay.Find("SafeArea/Content");
                var rewardCard=Box("Card",rewardPanelSafe,Card);Place(rewardCard,0.12f,0.25f,0.88f,0.75f);
                var rewardStripe=Box("Accent",rewardCard,Gold);Place(rewardStripe,0,0.984f,1,1);
                var panelTitle=Text("Title",rewardCard,"REWARDS",42,Color.white);Place(panelTitle.rectTransform,0.08f,0.84f,0.80f,0.95f);
                var rewardClose=Button("Close",rewardCard,"CLOSE",new Color(0.16f,0.2f,0.29f));Place((RectTransform)rewardClose.transform,0.73f,0.855f,0.94f,0.93f);
                var scrollBox=Rect("Scroll",rewardCard);Place(scrollBox,0.07f,0.31f,0.93f,0.79f);
                var rewardScroll=scrollBox.gameObject.AddComponent<ScrollRect>();rewardScroll.horizontal=false;
                var rewardViewport=Box("Viewport",scrollBox,new Color(1,1,1,0.01f));Stretch(rewardViewport);rewardViewport.gameObject.AddComponent<RectMask2D>();
                var rewardContent=Rect("Content",rewardViewport);rewardContent.anchorMin=new Vector2(0,1);rewardContent.anchorMax=Vector2.one;rewardContent.pivot=new Vector2(0.5f,1);rewardContent.sizeDelta=Vector2.zero;
                var rewardLayout=rewardContent.gameObject.AddComponent<VerticalLayoutGroup>();rewardLayout.spacing=15;rewardLayout.childControlHeight=true;rewardLayout.childForceExpandHeight=false;rewardLayout.childControlWidth=true;
                rewardContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;rewardScroll.viewport=rewardViewport;rewardScroll.content=rewardContent;
                var panelStatus=Text("Status",rewardCard,"Tap Claim to receive these rewards.",22,new Color(0.65f,0.73f,0.85f));Place(panelStatus.rectTransform,0.08f,0.20f,0.92f,0.29f);
                var claim=Button("Claim",rewardCard,"CLAIM REWARDS",Gold);Place((RectTransform)claim.transform,0.08f,0.06f,0.92f,0.17f);
                Set(rewardPanel,"titleText",panelTitle);Set(rewardPanel,"statusText",panelStatus);Set(rewardPanel,"claimButton",claim);Set(rewardPanel,"claimLabel",claim.GetComponentInChildren<TMP_Text>());
                Set(rewardPanel,"closeButton",rewardClose);Set(rewardPanel,"itemContainer",rewardContent);Set(rewardPanel,"itemPrefab",itemAsset.GetComponent<RewardItemView>());Set(rewardPanel,"displayCatalog",catalog);
                Set(rewardPanel,"canvasGroup",overlay.GetComponent<CanvasGroup>());Set(rewardPanel,"tweenPlayer",overlay.GetComponent<UITweenPlayer>());
                SaveVariant(overlay.gameObject,Root+"/Prefabs/RewardPanel.prefab");
                receivedPanel=rewardPanel;
                }

                var demo=canvas.AddComponent<BattlePassDemo>();Set(demo,"panel",mainPanel);Set(demo,"premiumPopup",premiumPopup);Set(demo,"rewardPanel",receivedPanel);Set(demo,"entry",entry);Set(demo,"balanceText",balance);Set(demo,"statusText",status);Set(demo,"confirmToggle",toggle);
                string[] fields={"enrollButton","winButton","replayButton","advanceButton","reloadButton"};for(int i=0;i<5;i++)Set(demo,fields[i],buttons[i]);
                if(!EditorSceneManager.SaveScene(scene,Root+"/Scenes/BattlePassDemo.unity"))throw new InvalidOperationException("Scene save failed.");AssetDatabase.SaveAssets();BattlePassPrefabValidator.Validate();
            }
            finally {SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);}
        }
        private const string PanelBase="Packages/com.dreamy.feature/Runtime/Prefabs/BaseFeaturePanel.prefab";
        private const string ItemBase="Packages/com.dreamy.feature/Runtime/Prefabs/BaseFeatureItem.prefab";
        private static GameObject RequireBase(string path)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(asset == null)throw new InvalidOperationException("Missing required Dreamy base: "+path);
            foreach(var child in asset.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject)>0)throw new InvalidOperationException("Missing script in Dreamy base: "+path);
            if(path == PanelBase && (asset.GetComponent<CanvasGroup>() == null || asset.GetComponent<UITweenPlayer>() == null || asset.transform.Find("SafeArea/Content") == null || asset.GetComponentInChildren<SafeArea>(true) == null || asset.transform.Find("Backdrop") == null))
                throw new InvalidOperationException("Incompatible BaseFeaturePanel hierarchy/components.");
            if(path == ItemBase && (asset.GetComponent<Image>() == null || asset.GetComponent<Button>() == null || asset.GetComponent<LayoutElement>() == null || asset.transform.Find("Icon") == null || asset.transform.Find("Content") == null))
                throw new InvalidOperationException("Incompatible BaseFeatureItem hierarchy/components.");
            return asset;
        }
        private static RectTransform FeaturePanel(string name,Transform parent,Color backdrop)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(RequireBase(PanelBase),parent);go.name=name;
            go.transform.Find("Backdrop").GetComponent<Image>().color=backdrop;return (RectTransform)go.transform;
        }
        private static RectTransform FeatureItem(string name,Transform parent,Color color,bool interactive)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(RequireBase(ItemBase),parent);go.name=name;
            go.GetComponent<Image>().color=color;go.GetComponent<Button>().enabled=interactive;
            go.GetComponent<Image>().raycastTarget=interactive;go.transform.Find("Icon").gameObject.SetActive(false);
            return (RectTransform)go.transform;
        }
        private static GameObject SaveVariant(GameObject instance,string path)
        {
            string guid=AssetDatabase.AssetPathToGUID(path);
            var result=PrefabUtility.SaveAsPrefabAssetAndConnect(instance,path,InteractionMode.AutomatedAction);
            if(result == null || PrefabUtility.GetPrefabAssetType(result) != PrefabAssetType.Variant)throw new InvalidOperationException("Expected Dreamy prefab variant: "+path);
            if(!string.IsNullOrEmpty(guid) && guid != AssetDatabase.AssetPathToGUID(path))throw new InvalidOperationException("Prefab GUID changed: "+path);
            return result;
        }
        private static RectTransform Rect(string name,Transform parent)
        {var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform;}
        private static RectTransform Box(string name,Transform parent,Color color)
        {var rect=Rect(name,parent);rect.gameObject.AddComponent<Image>().color=color;return rect;}
        private static void Stretch(RectTransform rect)=>Place(rect,0,0,1,1);
        private static void Anchor(RectTransform rect,Vector2 min,Vector2 max){rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;}
        private static void Place(RectTransform rect,float x,float y,float x2,float y2)=>Anchor(rect,new Vector2(x,y),new Vector2(x2,y2));
        private static TMP_Text Text(string name,Transform parent,string value,float size,Color color)
        {
            var rect=Rect(name,parent);var text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.text=value;text.fontSize=size;text.color=color;
            text.font=TMP_Settings.defaultFontAsset;text.raycastTarget=false;text.alignment=TextAlignmentOptions.MidlineLeft;return text;
        }
        private static Button Button(string name,Transform parent,string value,Color color)
        {
            var rect=Box(name,parent,color);var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=rect.GetComponent<Image>();
            var label=Text("Label",rect,value,23,color==Gold?Navy:Color.white);Stretch(label.rectTransform);label.alignment=TextAlignmentOptions.Center;return button;
        }
        private static void Set(UnityEngine.Object target,string field,UnityEngine.Object value)
        {var so=new SerializedObject(target);var property=so.FindProperty(field);if(property==null)throw new InvalidOperationException(field);property.objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
