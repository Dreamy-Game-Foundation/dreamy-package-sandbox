using System;
using System.IO;
using System.Linq;
using Dreamy.Audio;
using Dreamy.Feedback.Samples.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Dreamy.Feedback.Samples.Economy.Editor
{
    public static class FeedbackEconomySampleBuilder
    {
        [MenuItem("Dreamy/Feedback/Build Economy Demo")]
        public static void Build()
        {
            FeedbackSampleBuilder.RequireEditMode();
            string[] guids = AssetDatabase.FindAssets("FeedbackRig t:Prefab");
            if (guids.Length == 0) throw new InvalidOperationException("Import/build Basic Feedback first.");
            BuildAt(AssetDatabase.GenerateUniqueAssetPath("Assets/DreamyFeedbackEconomyDemo"), Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guids[0])).Replace('\\', '/'));
        }
        public static void BuildAt(string folder, string basic)
        {
            FeedbackSampleBuilder.RequireEditMode();
            if (Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any()) throw new IOException("Sample output exists: " + folder);
            Directory.CreateDirectory(folder); AssetDatabase.Refresh();
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(basic + "/DemoFont.asset");
                var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basic + "/FeedbackRig.prefab");
                var canvas = FeedbackSampleBuilder.Canvas("FeedbackEconomyPanel", 100);
                FeedbackSampleBuilder.Stretch((RectTransform)canvas.transform);
                canvas.gameObject.AddComponent<CanvasGroup>();
                var panel = canvas.gameObject.AddComponent<FeedbackEconomyPanel>();
                var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab, scene); rig.transform.SetParent(canvas.transform, false);
                var safe = FeedbackSampleBuilder.Rect("SafeArea", canvas.transform); FeedbackSampleBuilder.Stretch(safe); safe.gameObject.AddComponent<FeedbackSampleSafeArea>();
                var card = FeedbackSampleBuilder.Rect("Card", safe); card.anchorMin = new Vector2(.05f, .04f); card.anchorMax = new Vector2(.95f, .58f); card.offsetMin = card.offsetMax = Vector2.zero;
                card.gameObject.AddComponent<Image>().color = new Color(.035f, .06f, .1f, .98f);
                var content = FeedbackSampleBuilder.ScrollContent(card);
                var layout = content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(30, 30, 20, 20); layout.spacing = 20; layout.childForceExpandHeight = false; layout.childControlWidth = true; layout.childControlHeight = true; layout.childForceExpandWidth = true;
                FeedbackSampleBuilder.Label("Heading", content, "FEEDBACK + ECONOMY", font, 42, 90);
                FeedbackSampleBuilder.Label("Description", content, "Wallet commits first. VFX, text, icon fly and audio follow. Retry stays idempotent.", font, 28, 150);
                var balance = FeedbackSampleBuilder.Label("Balance", content, "Coins: 100", font, 42, 95);
                var grant = FeedbackSampleBuilder.Button("Grant 100 Coins", FeedbackSampleBuilder.Row(content), font);
                var retry = FeedbackSampleBuilder.Button("Retry Last Grant", FeedbackSampleBuilder.Row(content), font);
                var status = FeedbackSampleBuilder.Label("Status", content, "Standalone demo uses an in-memory wallet.", font, 26, 140);
                var close = FeedbackSampleBuilder.Button("Back / Close", FeedbackSampleBuilder.Row(content), font);
                var start = FeedbackSampleBuilder.Rect("IconStart", safe); FeedbackSampleBuilder.Anchor(start, new Vector2(.2f, .68f));
                var end = FeedbackSampleBuilder.Rect("CoinHUD", safe); FeedbackSampleBuilder.Anchor(end, new Vector2(.8f, .9f));
                var icon = AssetDatabase.LoadAssetAtPath<Sprite>(basic + "/Coin.png"); end.gameObject.AddComponent<Image>().sprite = icon;
                FeedbackSampleBuilder.Set(panel, "host", rig.GetComponent<FeedbackHost>()); FeedbackSampleBuilder.Set(panel, "worldTarget", rig.transform.Find("WorldTarget"));
                FeedbackSampleBuilder.Set(panel, "iconStart", start); FeedbackSampleBuilder.Set(panel, "iconEnd", end); FeedbackSampleBuilder.Set(panel, "coinIcon", icon);
                FeedbackSampleBuilder.Set(panel, "grantButton", grant); FeedbackSampleBuilder.Set(panel, "retryButton", retry); FeedbackSampleBuilder.Set(panel, "closeButton", close);
                FeedbackSampleBuilder.Set(panel, "balanceText", balance); FeedbackSampleBuilder.Set(panel, "statusText", status);
                var prefab = PrefabUtility.SaveAsPrefabAsset(canvas.gameObject, folder + "/FeedbackEconomyPanel.prefab"); Object.DestroyImmediate(canvas.gameObject);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.GetComponentInChildren<Camera>().gameObject.AddComponent<AudioListener>();
                var demo = new GameObject("EconomyDemoInstaller").AddComponent<FeedbackEconomyDemo>();
                FeedbackSampleBuilder.Set(demo, "panel", instance.GetComponent<FeedbackEconomyPanel>());
                FeedbackSampleBuilder.Set(demo, "audioProfile", CreateAudio(folder));
                FeedbackSampleBuilder.CreateEventSystem();
                EditorSceneManager.SaveScene(scene, folder + "/FeedbackEconomyDemo.unity"); AssetDatabase.SaveAssets();
                Debug.Log("[Feedback] Built economy sample: " + folder);
            }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        private static DreamyAudioProfile CreateAudio(string folder)
        {
            // Author a tiny original tone; sample does not depend on sandbox audio assets.
            const int rate = 44100; const int frames = 8820;
            using (var stream = new FileStream(folder + "/Reward.wav", FileMode.CreateNew))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + frames * 2); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(frames * 2);
                for (int i = 0; i < frames; i++) writer.Write((short)(Math.Sin(2 * Math.PI * 880 * i / rate) * 8000 * (1f - (float)i / frames)));
            }
            AssetDatabase.ImportAsset(folder + "/Reward.wav");
            var sound = ScriptableObject.CreateInstance<SoundAudioFile>(); AssetDatabase.CreateAsset(sound, folder + "/RewardSound.asset");
            var s = new SerializedObject(sound); s.FindProperty("key").stringValue = "ui.click"; s.FindProperty("bus").stringValue = "ui";
            var clips = s.FindProperty("clips"); clips.arraySize = 1; clips.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "/Reward.wav"); s.ApplyModifiedPropertiesWithoutUndo();
            var library = ScriptableObject.CreateInstance<AudioLibrary>(); AssetDatabase.CreateAsset(library, folder + "/AudioLibrary.asset");
            var l = new SerializedObject(library); var sounds = l.FindProperty("sounds"); sounds.arraySize = 1; sounds.GetArrayElementAtIndex(0).objectReferenceValue = sound; l.ApplyModifiedPropertiesWithoutUndo();
            var profile = ScriptableObject.CreateInstance<DreamyAudioProfile>(); AssetDatabase.CreateAsset(profile, folder + "/AudioProfile.asset");
            var p = new SerializedObject(profile); p.FindProperty("dontDestroyOnLoad").boolValue = false; var libs = p.FindProperty("libraries"); libs.arraySize = 1; libs.GetArrayElementAtIndex(0).objectReferenceValue = library; p.ApplyModifiedPropertiesWithoutUndo(); return profile;
        }
    }
}
