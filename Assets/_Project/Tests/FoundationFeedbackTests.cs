using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.IO;
using System.Linq;
using System.Reflection;
using Dreamy.Audio;
using Dreamy.Core;
using Dreamy.Economy;
using Dreamy.Feedback;
using Dreamy.Feedback.Samples.Tests;
using Dreamy.Template.Demo;
using Dreamy.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace Dreamy.Template.Tests
{
    public sealed class FoundationFeedbackTests
    {
        private static void Set(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        [UnityTest] public IEnumerator BootstrapAndFoundationUseFeedbackWithoutEconomyPanel()
        {
#if UNITY_EDITOR
            var priorRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Where(go => go.activeSelf && (go.name == "FeedbackRig" || go.name == "DemoCanvas" || go.name == "EventSystem")).ToArray();
            foreach (var prior in priorRoots) prior.SetActive(false);
            // Use a fresh save directory; the player's existing save is never read or overwritten.
            var obj = new GameObject("FeedbackBootstrapFixture"); obj.SetActive(false);
            var installer = obj.AddComponent<GameInstaller>();
            string savePath = Path.Combine(Path.GetTempPath(), "DreamyFeedbackSmoke", Guid.NewGuid().ToString("N"));
            Set(installer, "saveDirectory", savePath);
            string profilePath = "Assets/_Project/Audio/Feedback/AudioProfile.asset";
            Set(installer, "audioProfile", AssetDatabase.LoadAssetAtPath<DreamyAudioProfile>(profilePath));
            Set(installer, "feedbackPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Dreamy Feedback/0.3.0/Basic Feedback/Generated/FeedbackRig.prefab").GetComponent<FeedbackHost>());
            GameObject panelRoot = null; GameObject demo = null; GameObject eventRoot = null;
            try
            {
                obj.SetActive(true);
                float deadline = Time.realtimeSinceStartup + 30;
                while (GameInstaller.State != BootstrapState.Ready && GameInstaller.State != BootstrapState.Failed && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(GameInstaller.State, Is.EqualTo(BootstrapState.Ready), GameInstaller.InitializationException?.ToString());
                Assert.That(GameInstaller.Feedback.IsInitialized, Is.True);
                Assert.That(ServiceLocator.Get<IVfxService>(), Is.SameAs(GameInstaller.Feedback.Vfx));
                if (EventSystem.current == null) eventRoot = new GameObject("FeedbackEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                panelRoot = new GameObject("FeedbackPanelManager", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PanelManager));
                panelRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = panelRoot.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
                typeof(GameInstaller).GetMethod("AttachPanelPresenters", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(installer, null);
                yield return null;
                var holder = new GameObject("DemoHolder"); holder.transform.SetParent(panelRoot.transform, false); holder.SetActive(false);
                demo = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Demo/FoundationDemo.prefab"), holder.transform);
                var tutorial = demo.GetComponent<FoundationTutorialAdapter>(); if (tutorial != null) tutorial.enabled = false;
                holder.SetActive(true);
                var root = demo.GetComponent<FoundationDemoRoot>();
                deadline = Time.realtimeSinceStartup + 30;
                while ((root.CurrentPanel == null || root.IsTransitioning) && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(root.CurrentPanel, Is.Not.Null);
                yield return new WaitForSeconds(.6f);
                var button = root.CurrentPanel.GetComponentsInChildren<Button>(true).Single(b => b.name == "FeedbackButton");
                Click(button);
                yield return new WaitForSeconds(.6f);
                Assert.That(GameInstaller.Feedback.IsInitialized, Is.True);
                var balances = ServiceLocator.Get<IResourceBalanceProvider>();
                long balance = balances.GetBalance(new ResourceId("currency.coin"));
                Click(button); yield return null;
                Assert.That(balances.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(balance), "Feedback must not grant currency.");
                int scoreClicks = 0; root.CurrentPanel.AddScoreRequested += () => scoreClicks++;
                var scoreButton = root.CurrentPanel.GetComponentsInChildren<Button>(true).Single(b => b.name == "AddScoreButton");
                Click(scoreButton); Assert.That(scoreClicks, Is.EqualTo(1));
                foreach (var name in new[] { "Economy", "Missions", "Progression", "LuckyWheel" })
                {
                    Click(root.CurrentPanel.GetComponentsInChildren<Button>(true).Single(b => b.name == name + "Button"));
                    yield return new WaitForSeconds(.6f);
                    var feature = PanelManager.Instance.LastPanel; Assert.That(feature, Is.Not.Null); Assert.That(feature, Is.Not.SameAs(root.CurrentPanel), name);
                    var hide = feature.Hide(); while (!hide.GetAwaiter().IsCompleted) yield return null; hide.GetAwaiter().GetResult();
                    yield return null;
                }
                Click(button); yield return new WaitForSeconds(.6f);
                Assert.That(GameInstaller.Feedback.IsInitialized, Is.True);
            }
            finally
            {
                if (demo) Object.Destroy(demo); if (panelRoot) Object.Destroy(panelRoot); if (obj) Object.Destroy(obj); if (eventRoot) Object.Destroy(eventRoot);
                foreach (var prior in priorRoots) if (prior) prior.SetActive(true);
            }
            yield return null;
            Assert.That(ServiceLocator.IsRegistered<IVfxService>(), Is.False);
#else
            Assert.Ignore("Foundation asset fixture requires the Editor."); yield return null;
#endif
        }
        private static void Click(Button button)
        {
            Canvas.ForceUpdateCanvases();
            var scroll = button.GetComponentInParent<ScrollRect>();
            if (scroll && scroll.content && scroll.viewport)
            {
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content,button.transform);
                float excess = scroll.content.rect.height - scroll.viewport.rect.height;
                if(excess>0) scroll.verticalNormalizedPosition = Mathf.Clamp01((bounds.center.y - scroll.content.rect.yMin - scroll.viewport.rect.height * .5f) / excess);
                Canvas.ForceUpdateCanvases();
            }
            Assert.That(button.IsInteractable(), Is.True, button.name);
            var canvas = button.GetComponentInParent<Canvas>().rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var data = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(camera, ((RectTransform)button.transform).TransformPoint(((RectTransform)button.transform).rect.center)) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data, hits);
            Assert.That(hits.Count, Is.GreaterThan(0), button.name + " ancestry=" + string.Join(" / ", button.GetComponentsInParent<Transform>().Select(t => t.name+":"+t.localScale+":"+t.position)) + " has no raycast hit: active=" + button.gameObject.activeInHierarchy + " position=" + data.position + " screen=" + Screen.width + "x" + Screen.height + " raycasters=" + Object.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None).Length);
            var hitButton = hits[0].gameObject.GetComponentInParent<Button>();
            Assert.That(hitButton, Is.SameAs(button), button.name + " at " + data.position + " blocked by " + hits[0].gameObject.name + " canvas=" + button.GetComponentInParent<Canvas>().name + "/" + button.GetComponentInParent<Canvas>().sortingOrder + "/" + button.GetComponentInParent<Canvas>().overrideSorting + " hits=" + string.Join(",", hits.Select(h => h.gameObject.name + ":" + h.sortingOrder)));
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, data, ExecuteEvents.pointerClickHandler);
        }
    }
}
