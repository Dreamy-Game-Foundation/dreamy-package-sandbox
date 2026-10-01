using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Tutorial;
using Dreamy.Tutorial.Integration;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.Template.Demo
{
    public sealed class FoundationTutorialAdapter : MonoBehaviour
    {
        [SerializeField] private FoundationDemoRoot demoRoot;
        [SerializeField] private TutorialOverlay overlayPrefab;
        private TutorialOverlay overlay;
        private TutorialTargetRegistry registry;
        private TutorialController controller;
        private ITutorialService service;
        private ShopPanel boundShop;
        private Guid navigationToken;
        private bool initialized;
        private bool initializationStarted;
        private bool initializeBusy;
        private bool restoringShop;
        private void Start() { initializationStarted = true; InitializeAsync().Forget(); }
        private async UniTask InitializeAsync()
        {
            if (initialized || initializeBusy) return;
            initializeBusy = true;
            try
            {
                await UniTask.WaitUntil(() => GameInstaller.State == BootstrapState.Ready || GameInstaller.State == BootstrapState.Failed,
                    cancellationToken: this.GetCancellationTokenOnDestroy());
                if (!isActiveAndEnabled || GameInstaller.State != BootstrapState.Ready || demoRoot == null || overlayPrefab == null) return;
                service = new TutorialModel(ServiceLocator.Get<IDataConfigService>().GetTable<TutorialCatalogConfig>(),
                    ServiceLocator.Get<IDatasaveService>(), "foundation-tutorials");
                registry = gameObject.AddComponent<TutorialTargetRegistry>();
                overlay = Instantiate(overlayPrefab);
                controller = gameObject.AddComponent<TutorialController>(); controller.Initialize(service, overlay, registry);
                initialized = true;
                Bind();
                if (demoRoot.CurrentPanel != null) PanelReady(demoRoot.CurrentPanel);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception) { Debug.LogException(exception, this); }
            finally { initializeBusy = false; }
        }
        private void OnEnable()
        {
            if (!initialized) { if (initializationStarted) InitializeAsync().Forget(); return; }
            controller.enabled = true; Bind();
            if (demoRoot.CurrentPanel != null) PanelReady(demoRoot.CurrentPanel);
            if (demoRoot.CurrentShop != null) ShopReady(demoRoot.CurrentShop);
        }
        private void Bind()
        {
            demoRoot.DemoPanelReady += PanelReady; demoRoot.ScoreAdded += ScoreAdded;
            demoRoot.ShopOpening += ShopOpening; demoRoot.ShopReady += ShopReady;
        }
        private void PanelReady(FoundationDemoPanel panel)
        {
            Register(panel.TutorialAddScoreButton, "foundation.add-score");
            Register(panel.TutorialOpenShopButton, "foundation.open-shop");
            if (service.GetState().Step?.SignalKey == "foundation.shop-closed")
                controller.ReportSignal("foundation.shop-closed", navigationToken);
            else if (service.GetState().Status == TutorialStatus.Idle)
            {
                service.TryStart("foundation");
                if (service.GetState().Step?.Id == "close-shop") RestoreShopAsync().Forget();
            }
        }
        private async UniTask RestoreShopAsync()
        {
            if (restoringShop) return;
            restoringShop = true;
            Guid captured = service.GetState().StepToken;
            try
            {
                await UniTask.WaitUntil(() => !demoRoot.IsTransitioning, cancellationToken: this.GetCancellationTokenOnDestroy());
                if (isActiveAndEnabled && service.GetState().StepToken == captured && service.GetState().Step?.Id == "close-shop" && demoRoot.CurrentShop == null)
                    demoRoot.OpenShopForTutorial();
            }
            catch (OperationCanceledException) { }
            catch (Exception exception) { Debug.LogException(exception, this); }
            finally { restoringShop = false; }
        }
        private void ScoreAdded()
        {
            Guid captured = service.GetState().StepToken;
            controller.ReportSignal("foundation.score-added", captured);
        }
        private void ShopOpening() => navigationToken = service.GetState().StepToken;
        private void ShopReady(ShopPanel panel)
        {
            UnbindShop(); boundShop = panel; boundShop.CloseRequested += ShopClosing;
            // This name is an inspected convention of the Foundation Shop sample, not a package lookup rule.
            Button close = panel.GetComponentsInChildren<Button>(true).SingleOrDefault(b => b.name == "CloseButton");
            if (close == null) Debug.LogError("Foundation tutorial requires a unique Shop CloseButton.", panel);
            else Register(close, "foundation.close-shop");
            controller.ReportSignal("foundation.shop-opened", navigationToken);
        }
        private void ShopClosing() => navigationToken = service.GetState().StepToken;
        private void Register(Button button, string id)
        {
            if (button == null) return;
            var target = button.GetComponent<TutorialUITarget>();
            if (target == null) target = button.gameObject.AddComponent<TutorialUITarget>();
            target.Configure(id, registry, button);
        }
        private void UnbindShop() { if (boundShop != null) boundShop.CloseRequested -= ShopClosing; boundShop = null; }
        private void OnDisable()
        {
            if (!initialized) return;
            demoRoot.DemoPanelReady -= PanelReady; demoRoot.ScoreAdded -= ScoreAdded;
            demoRoot.ShopOpening -= ShopOpening; demoRoot.ShopReady -= ShopReady;
            UnbindShop(); if (controller != null) controller.enabled = false;
        }
        private void OnDestroy() { UnbindShop(); if (overlay != null) Destroy(overlay.gameObject); }
    }
}
