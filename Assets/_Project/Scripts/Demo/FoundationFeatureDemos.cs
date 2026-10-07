using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Audio;
using Dreamy.Feedback;
using Dreamy.Feedback.Samples.Economy;
using Dreamy.Economy;
using Dreamy.Feature.Economy.Integration;
using Dreamy.Feature.LuckyWheel.Integration;
using Dreamy.Feature.Missions.Integration;
using Dreamy.Feature.Progression.Integration;
using Dreamy.LuckyWheel;
using Dreamy.Missions;
using Dreamy.Progression;
using Dreamy.UI;
using UnityEngine;

namespace Dreamy.Template.Demo
{
    /// <summary>Project composition only: sample panels use the bootstrap-owned services.</summary>
    public sealed class FoundationFeatureDemos : MonoBehaviour
    {
        [SerializeField] private FoundationDemoRoot demoRoot;
        [SerializeField] private MissionPanel missionPrefab;
        [SerializeField] private ProgressionDemoPanel progressionPrefab;
        [SerializeField] private LuckyWheelPanel luckyWheelPrefab;
        [SerializeField] private EconomyDemoPanel economyPrefab;
        [SerializeField] private FeedbackEconomyPanel feedbackPrefab;
        private FoundationDemoPanel boundPanel;
        private UIPanel activePanel;
        private CancellationTokenSource lifetime;
        private bool opening;
        public bool IsBusy => opening || activePanel != null;

        private void OnEnable()
        {
            lifetime = new CancellationTokenSource();
            demoRoot.DemoPanelReady += Bind;
            if (demoRoot.CurrentPanel != null) Bind(demoRoot.CurrentPanel);
        }
        private void Bind(FoundationDemoPanel panel)
        {
            Unbind();
            boundPanel = panel;
            panel.OpenMissionsRequested += OpenMissions;
            panel.OpenProgressionRequested += OpenProgression;
            panel.OpenLuckyWheelRequested += OpenLuckyWheel;
            panel.OpenEconomyRequested += OpenEconomy;
            panel.OpenFeedbackRequested += OpenFeedback;
            panel.Destroyed += Unbind;
        }
        private void Unbind()
        {
            if (!ReferenceEquals(boundPanel, null))
            {
                boundPanel.OpenMissionsRequested -= OpenMissions;
                boundPanel.OpenProgressionRequested -= OpenProgression;
                boundPanel.OpenLuckyWheelRequested -= OpenLuckyWheel;
                boundPanel.OpenEconomyRequested -= OpenEconomy;
                boundPanel.OpenFeedbackRequested -= OpenFeedback;
                boundPanel.Destroyed -= Unbind;
            }
            boundPanel = null;
        }
        private void OpenMissions() => Open(missionPrefab, panel =>
        {
            var service = ServiceLocator.Get<IMissionService>();
            panel.Configure(service);
            panel.GetComponent<MissionDemoControls>().Configure(service);
        });
        private void OpenProgression() => Open(progressionPrefab, panel => panel.Configure(ServiceLocator.Get<IProgressionService>()));
        private void OpenLuckyWheel() => Open(luckyWheelPrefab, panel => panel.Configure(ServiceLocator.Get<ILuckyWheelService>()));
        private void OpenFeedback() => Open(feedbackPrefab, panel => panel.Configure(
            ServiceLocator.Get<IResourceWallet>(), (IResourceBalanceSource)ServiceLocator.Get<IResourceBalanceProvider>(),
            ServiceLocator.Get<IAudioService>(), GameInstaller.Feedback));
        private void OpenEconomy() => Open(economyPrefab, panel => panel.Configure(
            ServiceLocator.Get<IResourceWallet>(), ServiceLocator.Get<IResourceBalanceProvider>()));

        private void Open<T>(T prefab, Action<T> configure) where T : UIPanel
        {
            if (IsBusy || demoRoot.HasFeatureDemo || demoRoot.IsTransitioning || GameInstaller.State != BootstrapState.Ready) return;
            if (prefab == null) { Debug.LogError($"Missing {typeof(T).Name} sample prefab.", this); return; }
            opening = true;
            OpenAsync(prefab, configure, lifetime.Token).Forget();
        }
        private async UniTaskVoid OpenAsync<T>(T prefab, Action<T> configure, CancellationToken token) where T : UIPanel
        {
            T created = null;
            try
            {
                created = Instantiate(prefab, PanelManager.Instance.transform);
                activePanel = created;
                await created.Init();
                token.ThrowIfCancellationRequested();
                await created.PostInit();
                token.ThrowIfCancellationRequested();
                configure(created);
                await created.Show();
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) { if (created != null) Destroy(created.gameObject); }
            catch (Exception exception)
            {
                if (created != null) Destroy(created.gameObject);
                Debug.LogException(exception, this);
            }
            finally { opening = false; }
        }
        private void OnDisable()
        {
            demoRoot.DemoPanelReady -= Bind;
            Unbind();
            lifetime?.Cancel();
            lifetime?.Dispose();
            lifetime = null;
            if (activePanel != null) Destroy(activePanel.gameObject);
            activePanel = null;
        }
    }
}
