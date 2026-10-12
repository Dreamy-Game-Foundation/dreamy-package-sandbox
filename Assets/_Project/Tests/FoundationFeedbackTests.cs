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
        [Test] public void ManualEconomyHolderUpdatesOnlyOnRefreshAndRebind()
        {
            var root = new GameObject("HolderFixture"); root.SetActive(false);
            var label = new GameObject("Amount", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            label.transform.SetParent(root.transform, false);
            try
            {
                var holder = root.AddComponent<Dreamy.Economy.UI.ResourceUIHolder>();
                var text = label.GetComponent<TMPro.TMP_Text>();
                Set(holder, "amountText", text);
                Set(holder, "autoRefresh", false);
                holder.SetResource("currency.coin");
                var wallet = new FoundationResourceWallet(100);
                holder.Bind(wallet);
                wallet.TryGrant(new ResourceGrantRequest("holder-fixture", new ResourceAmount(new ResourceId("currency.coin"), 50)));
                Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(150));
                Assert.That(text.text, Is.EqualTo("100"));
                holder.RefreshBalance();
                Assert.That(text.text, Is.EqualTo("150"));
                root.SetActive(true); root.SetActive(false); root.SetActive(true);
                holder.Bind(wallet);
                Assert.That(text.text, Is.EqualTo("150"), "Rebinding reads the latest committed balance.");
                Set(holder, "autoRefresh", true);
                holder.Bind(wallet);
                wallet.TryGrant(new ResourceGrantRequest("holder-auto-fixture", new ResourceAmount(new ResourceId("currency.coin"), 25)));
                Assert.That(text.text, Is.EqualTo("175"), "Automatic holders still follow wallet changes.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void Set(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        [UnityTest] public IEnumerator SceneCameraBindsAfterFeedbackBootstrapAndRefreshesRegistry()
        {
            var priorCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(camera => camera.enabled).ToArray();
            foreach (var camera in priorCameras) camera.enabled = false;
            var installerObject = new GameObject("InstallerFixture"); installerObject.SetActive(false);
            var rig = new GameObject("FeedbackFixture"); rig.SetActive(false);
            GameObject cameraObject = null;
            GameInstaller installer = null;
            FeedbackHost host = null;
            try
            {
                var root = rig.AddComponent<FeedbackRoot>();
                host = rig.AddComponent<FeedbackHost>(); Set(host, "root", root); Set(host, "useMainCamera", false);
                host.Initialize(); Assert.That(host.CameraShake, Is.Null);
                installer = installerObject.AddComponent<GameInstaller>(); Set(installer, "feedbackInstance", host);
                cameraObject = new GameObject("LateMainCamera", typeof(Camera)); cameraObject.tag = "MainCamera";
                var camera = cameraObject.GetComponent<Camera>();
                typeof(GameInstaller).GetMethod("OnSceneLoaded", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(installer,
                    new object[] { UnityEngine.SceneManagement.SceneManager.GetActiveScene(), UnityEngine.SceneManagement.LoadSceneMode.Single });
                Assert.That(host.WorldCamera, Is.SameAs(camera));
                Assert.That(ServiceLocator.Get<ICameraShakeService>(), Is.SameAs(host.CameraShake));
                Assert.That(ServiceLocator.Get<IFeedbackService>(), Is.SameAs(host.Feedback));
                var basis = camera.transform.localPosition;
                var handle = host.CameraShake.Shake(CameraShakeOptions.Small);
                yield return null;
                Assert.That(camera.transform.localPosition, Is.Not.EqualTo(basis));
                handle.Stop(); Assert.That(camera.transform.localPosition, Is.EqualTo(basis));
            }
            finally
            {
                if (installer != null)
                {
                    var registration = (IDisposable)typeof(GameInstaller).GetField("feedbackRegistration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(installer);
                    registration?.Dispose();
                }
                host?.Shutdown();
                Object.DestroyImmediate(installerObject); Object.DestroyImmediate(rig);
                if (cameraObject) Object.DestroyImmediate(cameraObject);
                foreach (var camera in priorCameras) if (camera) camera.enabled = true;
            }
        }

        [UnityTest] public IEnumerator ShopPrefabCommitsWalletThenUpdatesHolderAfterRealIconFly()
        {
#if UNITY_EDITOR
            var canvas = new GameObject("ShopFeedbackCanvas", typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var rig = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Dreamy Feedback/0.3.0/Basic Feedback/Generated/Rigs/GameFeedbackRig.prefab"));
            var viewRoot = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Dreamy Shop/0.1.1/Shop Feature/Prefabs/ShopPanel.prefab"), canvas.transform);
            Dreamy.Shop.ShopPresenter presenter = null;
            try
            {
                var wallet = new FoundationResourceWallet(100);
                wallet.TryGrant(new ResourceGrantRequest("shop-test-gems", new ResourceAmount(new ResourceId("currency.gem"), 100)));
                var reward = new Dreamy.Shop.ShopResourceConfig();
                Set(reward, "resourceId", "currency.coin"); Set(reward, "amount", 1000L);
                var offer = new Dreamy.Shop.ShopOfferConfig();
                Set(offer, "id", "test-offer"); Set(offer, "costResourceId", "currency.gem"); Set(offer, "costAmount", 15L);
                Set(offer, "rewards", new List<Dreamy.Shop.ShopResourceConfig> { reward });
                var catalog = new Dreamy.Shop.ShopCatalogConfig();
                Set(catalog, "catalogId", "feedback-test"); Set(catalog, "offers", new List<Dreamy.Shop.ShopOfferConfig> { offer });
                var panel = viewRoot.GetComponent<Dreamy.Feature.Shop.Integration.ShopPanel>();
                foreach (var holder in viewRoot.GetComponentsInChildren<Dreamy.Economy.UI.ResourceUIHolder>()) holder.Bind(wallet);
                var coinHolder = viewRoot.GetComponentsInChildren<Dreamy.Economy.UI.ResourceUIHolder>().Single(h => h.ResourceId.Value == "currency.coin");
                var text = (TMPro.TMP_Text)typeof(Dreamy.Economy.UI.ResourceUIHolder).GetField("amountText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(coinHolder);
                var gemHolder = viewRoot.GetComponentsInChildren<Dreamy.Economy.UI.ResourceUIHolder>().Single(h => h.ResourceId.Value == "currency.gem");
                var gemText = (TMPro.TMP_Text)typeof(Dreamy.Economy.UI.ResourceUIHolder).GetField("amountText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gemHolder);
                panel.BindFeedbackHost(rig.GetComponent<FeedbackHost>());
                presenter = new Dreamy.Shop.ShopPresenter(new Dreamy.Shop.ShopModel(catalog, wallet), panel, panel.PresentPurchaseAsync);
                presenter.Show(); Canvas.ForceUpdateCanvases();
                typeof(Dreamy.Feature.Shop.Integration.ShopPanel).GetMethod("RequestPurchase", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(panel, new object[] { "test-offer" });
                Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(1100));
                Assert.That(text.text, Is.EqualTo("100"), "Reward HUD waits for IconFly completion.");
                Assert.That(gemText.text, Is.EqualTo("85"), "Cost HUD updates while IconFly is still running.");
                var player = viewRoot.GetComponent<FeedbackPlayer>();
                Assert.That(player.LastHandle.IsRunning, Is.True, "The wired prefab must play an actual IconFly.");
                float deadline = Time.realtimeSinceStartup + 8;
                while (player.LastHandle.IsRunning && Time.realtimeSinceStartup < deadline) yield return null;
                yield return null;
                Assert.That(player.LastHandle.Status, Is.EqualTo(FeedbackStatus.Completed));
                Assert.That(text.text, Is.EqualTo("1.1K"));
                Assert.That(wallet.GetBalance(new ResourceId("currency.gem")), Is.EqualTo(85));
            }
            finally { presenter?.Dispose(); Object.DestroyImmediate(viewRoot); Object.DestroyImmediate(rig); Object.DestroyImmediate(canvas); }
#else
            Assert.Ignore("Prefab fixture requires the Editor."); yield return null;
#endif
        }

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
            Set(installer, "feedbackPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Dreamy Feedback/0.3.0/Basic Feedback/Generated/Rigs/FeedbackRig.prefab").GetComponent<FeedbackHost>());
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
