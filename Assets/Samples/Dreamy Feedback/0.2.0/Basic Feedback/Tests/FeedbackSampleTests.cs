using System.Collections;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace Dreamy.Feedback.Samples.Tests
{
    public sealed class FeedbackSampleTests
    {
        [UnityTest] public IEnumerator BasicScenePlaysAllChannelsAndClears()
        {
#if UNITY_EDITOR
            var guid = System.Array.Find(AssetDatabase.FindAssets("FeedbackDemo t:Scene"), id => AssetDatabase.GUIDToAssetPath(id).Contains("Basic Feedback"));
            Assert.That(guid, Is.Not.Null);
            EditorSceneManager.LoadSceneInPlayMode(AssetDatabase.GUIDToAssetPath(guid), new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;
            var controls = Object.FindFirstObjectByType<FeedbackDemoControls>(); Assert.That(controls, Is.Not.Null);
            var host = Object.FindFirstObjectByType<FeedbackHost>(); Assert.That(host.IsInitialized, Is.True);
            foreach (var gameObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var child in gameObject.GetComponentsInChildren<Transform>(true)) Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject), Is.Zero, child.name);
            Assert.That(Resources.Load<TMP_Settings>("TMP Settings"), Is.Not.Null, "Import TMP Essential Resources once before running the samples.");
            Canvas.ForceUpdateCanvases();
            foreach (var button in controls.GetComponentsInChildren<UnityEngine.UI.Button>()) Assert.That(((RectTransform)button.transform).rect.width, Is.GreaterThan(150), button.name);
            for (int i = 0; i < 7; i++) controls.PlayChannel(i);
            controls.PlayAll(); yield return null;
            controls.StopAll(); yield return null;
            var root = host.GetComponent<FeedbackRoot>();
            Assert.That(root.WorldVfxRoot.childCount, Is.Zero); Assert.That(root.FloatingTextRoot.childCount, Is.Zero); Assert.That(root.IconFlyRoot.childCount, Is.Zero);
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) { Assert.That(text.font, Is.Not.Null, text.name); Assert.That(text.font.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static), text.name); }
            Assert.That(AssetDatabase.LoadAssetAtPath<Material>(Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guid)) + "/RewardParticles.mat").shader.name, Is.EqualTo("Dreamy/Feedback/SampleParticles"));
            yield return Capture("basic-portrait", 1080, 1920);
            yield return Capture("basic-landscape", 1920, 1080);
#else
            Assert.Ignore("Scene import check uses Editor scene loading."); yield return null;
#endif
        }
        public static IEnumerator Capture(string name, int width, int height)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            var camera = Object.FindFirstObjectByType<Camera>();
            var target = new RenderTexture(width, height, 24); target.Create();
            var originalTarget = camera.targetTexture; camera.targetTexture = target;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var modes = new RenderMode[canvases.Length]; var cameras = new Camera[canvases.Length]; var distances = new float[canvases.Length];
            var scalers = Object.FindObjectsByType<UnityEngine.UI.CanvasScaler>(FindObjectsSortMode.None);
            var scaleModes = new UnityEngine.UI.CanvasScaler.ScaleMode[scalers.Length]; var scales = new float[scalers.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                modes[i] = canvases[i].renderMode; cameras[i] = canvases[i].worldCamera; distances[i] = canvases[i].planeDistance;
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera; canvases[i].worldCamera = camera; canvases[i].planeDistance = 1;
            }
            for (int i = 0; i < scalers.Length; i++)
            {
                scaleModes[i] = scalers[i].uiScaleMode; scales[i] = scalers[i].scaleFactor;
                scalers[i].uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
                scalers[i].scaleFactor = Mathf.Sqrt((float)width / 1080 * height / 1920);
            }
            try
            {
                for (int i = 0; i < 3; i++) yield return null;
                Canvas.ForceUpdateCanvases();
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                    UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, new UnityEngine.Rendering.RenderPipeline.StandardRequest { destination = target });
                else camera.Render();
                var previous = RenderTexture.active; RenderTexture.active = target;
                var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply(); RenderTexture.active = previous;
                string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../.omo/evidence/feedback")); Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG()); Object.Destroy(texture);
            }
            finally
            {
                camera.targetTexture = originalTarget;
                for (int i = 0; i < canvases.Length; i++) { canvases[i].renderMode = modes[i]; canvases[i].worldCamera = cameras[i]; canvases[i].planeDistance = distances[i]; }
                for (int i = 0; i < scalers.Length; i++) { scalers[i].uiScaleMode = scaleModes[i]; scalers[i].scaleFactor = scales[i]; }
                target.Release(); Object.Destroy(target);
            }
        }
    }
}
