using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Dreamy.DesignPreview
{
    public static class UiCandidateSpecimen
    {
        const string AssetsPath = "Assets/UiCandidateV3/";
        const string Output = "/tmp/dreamy-ui-material-preview";
        static Font regular, bold, mono;
        static Sprite fill, outline, circle;
        static Canvas canvas;
        static Camera camera;
        static RenderTexture target;
        static int frames;
        static readonly Color Bg = Hex("101216"), Surface = Hex("1C1F25"), Control = Hex("272C34"), Border = Hex("363E49"), Ink = Hex("EEF0F4"), Muted = Hex("99A3B2"), Red = Hex("CB465D");
        static Color Hex(string hex) { Color result; ColorUtility.TryParseHtmlString("#" + hex, out result); return result; }
        static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h); return rect;
        }
        static Image Box(Transform parent, string name, float x, float y, float w, float h, Color color, bool stroke = false)
        {
            var r = Rect(parent, name, x, y, w, h); var image = r.gameObject.AddComponent<Image>(); image.sprite = stroke ? outline : fill;
            image.type = Image.Type.Sliced; image.color = color; image.raycastTarget = false; return image;
        }
        static void Card(Transform parent, string name, float x, float y, float w, float h, Color color)
        { Box(parent, name, x, y, w, h, color); Box(parent, name + " edge", x, y, w, h, Border, true); }
        static Text Label(Transform parent, string name, string text, float x, float y, float w, float h, int size = 18, Color? color = null, bool strong = false, bool numeric = false)
        {
            var r = Rect(parent, name, x, y, w, h); var label = r.gameObject.AddComponent<Text>(); label.font = numeric ? mono : strong ? bold : regular;
            label.text = text; label.fontSize = size; label.color = color ?? Ink; label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate; label.raycastTarget = false; return label;
        }
        static void Icon(Transform parent, string name, float x, float y, float size = 24, Color? color = null)
        {
            var r = Rect(parent, name, x, y, size, size); var image = r.gameObject.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetsPath + name + ".png");
            image.preserveAspect = true; image.color = color ?? Muted; image.raycastTarget = false;
        }
        static Button Button(Transform parent, string name, string text, float x, float y, float w, float h, bool primary = false)
        {
            var image = Box(parent, name, x, y, w, h, primary ? Red : Control); image.raycastTarget = true;
            if (!primary) Box(image.transform, "edge", 0, 0, w, h, Border, true);
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var label = Label(image.transform, "Caption", text, 6, 0, w - 12, h, 18, Ink, true); label.alignment = TextAnchor.MiddleCenter; return button;
        }
        static void Dot(Transform parent, float x, float y, Color color, float size = 8)
        { var r = Rect(parent, "Severity", x, y, size, size); var image = r.gameObject.AddComponent<Image>(); image.sprite = circle; image.preserveAspect = true; image.color = color; image.raycastTarget = false; }
        static void Graph(Transform parent, float x, float y, float width, float height)
        {
            for (int i = 0; i < 50; i++)
            {
                float value = .55f + .16f * Mathf.Sin(i * .61f) + (i % 13 == 0 ? .18f : 0);
                var r = Rect(parent, "Sample " + i, x + i * width / 50, y + height * (1 - value), width / 50 - 2, height * value);
                var image = r.gameObject.AddComponent<RawImage>(); image.texture = Texture2D.whiteTexture;
                image.color = i % 13 == 0 ? Hex("D5B757") : Hex("90C777"); image.raycastTarget = false;
            }
        }
        public static void Build()
        {
            foreach (var path in Directory.GetFiles(AssetsPath, "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single; importer.spritePixelsPerUnit = 100;
                importer.spriteBorder = Path.GetFileName(path).StartsWith("rounded") ? new Vector4(8,8,8,8) : Vector4.zero;
                importer.mipmapEnabled = false; importer.isReadable = false; importer.maxTextureSize = 32; importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            }
            regular = AssetDatabase.LoadAssetAtPath<Font>(AssetsPath + "Barlow-Regular.ttf"); bold = AssetDatabase.LoadAssetAtPath<Font>(AssetsPath + "Barlow-SemiBold.ttf"); mono = AssetDatabase.LoadAssetAtPath<Font>(AssetsPath + "JetBrainsMono-Regular.ttf");
            fill = AssetDatabase.LoadAssetAtPath<Sprite>(AssetsPath + "rounded-fill.png"); outline = AssetDatabase.LoadAssetAtPath<Sprite>(AssetsPath + "rounded-outline.png"); circle = AssetDatabase.LoadAssetAtPath<Sprite>(AssetsPath + "circle.png");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraGo = new GameObject("Specimen camera"); camera = cameraGo.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 600;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Bg; camera.transform.position = new Vector3(0,0,-10);
            target = new RenderTexture(1920,1200,24); camera.targetTexture = target;
            var root = new GameObject("Real resource specimen", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.scaleFactor = 1;
            Label(root.transform, "Board title", "DREAMY  /  MATERIAL COMPACT", 40, 25, 1200, 45, 32, Ink, true);
            Label(root.transform, "Proof", "REAL UNITY RENDER  /  SYNTHETIC DATA", 1310, 31, 580, 32, 15, Muted, false, true);
            Label(root.transform, "Assets", "Barlow + JetBrains Mono   |   Material Icons   |   32 px sliced surfaces", 42, 75, 1500, 30, 17, Muted);
            DrawRuntime(root.transform); DrawEditor(root.transform); DrawOverlays(root.transform); DrawKeypad(root.transform);
            Label(root.transform, "Coverage", "Tài nguyên • Cài đặt • Hiệu năng • Bộ nhớ • Nhật ký     0123456789  -42  16.7 ms", 42, 1130, 1800, 38, 20, Muted);
            frames = 0; EditorApplication.update += Finish;
        }
        static void DrawRuntime(Transform p)
        {
            Card(p,"Runtime",40,130,1180,580,Surface);
            Label(p,"Brand","DREAMY",62,143,130,42,25,Ink,true); Icon(p,"menu",220,152);
            Label(p,"Page","Cheats",264,141,500,44,25,Ink,true); Icon(p,"close",1174,152);
            Box(p,"Rail boundary",202,130,1,580,Border); Box(p,"Header boundary",40,190,1180,1,Border);
            string[] names={"Cheats","Console","Performance","System"};string[] icons={"settings","console","performance","settings"};
            for(int i=0;i<4;i++){if(i==0)Box(p,"Active",48,211,146,44,Hex("39212C"));Icon(p,icons[i],62,224+i*59,22,i==0?Ink:Muted);Label(p,"Navigation",names[i],94,213+i*59,106,42,17,i==0?Ink:Muted,i==0);}
            Card(p,"Search",226,211,968,42,Control);Icon(p,"search",238,220,22);Label(p,"Search label","Search options",272,212,600,40,17,Muted);
            Button(p,"Filter","All categories",226,265,690,38);Button(p,"Pinned","Only pinned",928,265,148,38);Label(p,"Count","34 options",1090,263,110,40,14,Muted,false,true);
            Label(p,"Category","Economy",230,322,800,38,23,Ink,true);Icon(p,"pin",1157,328,20);
            Resource(p,"Coins","3M","100",226,375); Resource(p,"Gems","816","10",718,375);
            Label(p,"Category 2","Gameplay",230,541,800,34,23,Ink,true);Icon(p,"pin",1157,548,20);
            Card(p,"Time scale",226,590,470,70,Control);Label(p,"Time scale title","Time scale",244,601,220,44,19);Button(p,"Minus","−",474,604,44,40);Label(p,"Scale","1.0",532,603,68,42,18,Ink,false,true);Button(p,"Plus","+",628,604,44,40);
            Button(p,"Reset scale","Reset time scale",718,604,302,40);
        }
        static void Resource(Transform p,string name,string balance,string amount,float x,float y)
        {
            Card(p,name,x,y,476,145,Control);Label(p,name+" name",name,x+18,y+10,250,32,21,Ink,true);
            Label(p,name+" balance","Balance",x+18,y+45,90,24,16,Muted);Label(p,name+" value",balance,x+107,y+44,200,27,20,Ink,false,true);
            Label(p,name+" amount","Amount",x+18,y+91,95,34,17,Muted);Button(p,name+" input",amount,x+118,y+88,160,38);Button(p,name+" grant","Grant",x+292,y+88,166,38,true);
        }
        static void DrawEditor(Transform p)
        {
            Card(p,"Editor",1250,130,630,580,Surface);Label(p,"Editor title","Dreamy Debugger · Settings",1270,145,560,40,23,Ink,true);
            Label(p,"Editor tabs","General    Access    Layout    Performance",1270,200,570,36,18,Muted);Box(p,"Tab active",1489,238,70,2,Red);
            Label(p,"Config label","Project Configuration",1270,265,250,30,17,Muted);Button(p,"Config","DebuggerConfig",1522,263,330,38);
            Label(p,"Section","Performance",1270,323,550,34,24,Ink,true);
            string[] modes={"Off","FPS","Compact","Detail"};for(int i=0;i<4;i++)Button(p,"Mode "+i,modes[i],1270+i*147,373,137,38,i==2);
            Label(p,"Setting 1","Tap FPS to cycle",1272,438,460,32,19);Box(p,"Toggle 1",1790,439,50,26,Red);Dot(p,1817,443,Ink,18);
            Label(p,"Setting 2","Show close button",1272,489,460,32,19);Box(p,"Toggle 2",1790,490,50,26,Red);Dot(p,1817,494,Ink,18);
            Label(p,"Setting 3","Free drag",1272,540,460,32,19);Box(p,"Toggle 3",1790,541,50,26,Red);Dot(p,1817,545,Ink,18);
            Label(p,"HUD safe","HUD safe area: Off   ·   Panel safe area: On",1272,602,550,36,17,Muted);
            Button(p,"Preview","Reset preview",1270,652,196,36);
        }
        static void DrawOverlays(Transform p)
        {
            Label(p,"HUD title","HUD / CATEGORY CHIP",40,751,600,34,20,Ink,true);
            Card(p,"HUD",40,800,300,135,Hex("191C22"));Icon(p,"drag",50,810,20);Label(p,"FPS","60",79,802,74,40,26,Hex("90C777"),false,true);Label(p,"FPS suffix","FPS",153,811,42,25,14,Muted);Label(p,"MS","16.7 ms",204,811,108,25,14,Ink,false,true);Icon(p,"close",313,809,17);
            Graph(p,54,852,272,37);Label(p,"RAM","RAM 412 MB",54,897,262,24,14,Muted,false,true);
            Card(p,"Pinned chip",40,955,300,42,Control);Icon(p,"chevron",53,966,18);Label(p,"Chip","Economy · Coin 3M · Gem 816",80,957,235,36,16);Icon(p,"pin",312,967,16);
            Label(p,"Dock title","PINNED CONSOLE",382,751,650,34,20,Ink,true);
            Card(p,"Dock",382,800,720,190,Surface);Icon(p,"drag",394,811,20);Icon(p,"chevron",425,812,18);
            Dot(p,469,818,Red);Label(p,"errors","2",486,806,43,30,15,Ink,false,true);Dot(p,543,818,Hex("D5B757"));Label(p,"warnings","7",560,806,43,30,15,Ink,false,true);Dot(p,618,818,Hex("86BCD9"));Label(p,"logs","18",635,806,60,30,15,Ink,false,true);Icon(p,"pin",1033,811,18);Icon(p,"close",1066,811,18);
            Box(p,"Dock divider",395,845,694,1,Border);Dot(p,398,866,Hex("86BCD9"));Label(p,"Log1","[INFO]  Scene loaded: MainScene",418,855,655,34,15,Ink,false,true);
            Dot(p,398,910,Hex("D5B757"));Label(p,"Log2","[WARN]  Reward icon missing",418,900,655,34,15,Ink,false,true);
            Label(p,"Dock hint","Collapsed by default · Shared console history",395,950,688,28,15,Muted);
        }
        static void DrawKeypad(Transform p)
        {
            Label(p,"Keypad external label","TITLELESS KEYPAD",1160,751,700,34,20,Ink,true);
            Card(p,"Keypad",1160,800,450,300,Surface);
            for(int i=0;i<4;i++)Dot(p,1324+i*24,818,Ink,10);
            for(int i=1;i<=9;i++)Button(p,"Key "+i,i.ToString(),1180+((i-1)%3)*139,848+((i-1)/3)*56,129,46);
            var cancel=Button(p,"Cancel","",1180,1016,129,46);Icon(cancel.transform,"close",52,11,24);
            Button(p,"Zero","0",1319,1016,129,46);
            var back=Button(p,"Backspace","",1458,1016,129,46);Icon(back.transform,"backspace",52,11,24);
        }
        static void Finish()
        {
            if(++frames<40)return;EditorApplication.update-=Finish;Directory.CreateDirectory(Output);
            Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(Output+"/unity-resource-specimen.png",image.EncodeToPNG());RenderTexture.active=previous;
            var report=new System.Text.StringBuilder();report.AppendLine("Actual Unity render; synthetic values. Fonts and sprites are real imported assets.");
            foreach(var path in Directory.GetFiles(AssetsPath,"*.png")){var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);report.AppendLine(Path.GetFileName(path)+" "+t.width+"x"+t.height+" "+t.format+" "+UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t)+" bytes");}
            report.AppendLine("Fonts: "+regular.name+", "+bold.name+", "+mono.name);File.WriteAllText(Output+"/resource-receipt.txt",report.ToString());
            camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);EditorApplication.Exit(0);
        }
    }
}
