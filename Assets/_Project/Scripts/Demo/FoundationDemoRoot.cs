using System;
using System.Threading;
using Dreamy.Connectivity;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.DailyReward;
using Dreamy.Feature.DailyReward.Integration;
using Dreamy.Feature.Settings.Integration;

using Dreamy.Feature.Shop.Integration;
using Dreamy.Shop;
using Dreamy.Template.Pooling;
using Dreamy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    [DisallowMultipleComponent]
    public sealed class FoundationDemoRoot : MonoBehaviour
    {
        [SerializeField] private Button togglePanelButton;
        [SerializeField] private ShopPanel featureShopPrefab;
        [SerializeField] private DailyRewardPanel dailyRewardPrefab;
        [SerializeField] private GameObject settingsPrefab;

        public FoundationDemoPanel CurrentPanel => panel;
        public ShopPanel CurrentShop => shopPanel;
        public bool IsTransitioning => isTransitioning;
        public bool HasFeatureDemo => (GetComponent<FoundationFeatureDemos>()?.IsBusy ?? false)
            || onlineRewardSession != null || dailyRewardOpening || (dailyRewardPanel != null && dailyRewardPanel.gameObject.activeInHierarchy)
            || IsOpen<SettingsPanel>() || IsOpen<RateUsPanel>();
        private static bool IsOpen<T>() where T : UIPanel => PanelManager.HasInstance
            && PanelManager.Instance.TryGet<T>(out var view) && view.gameObject.activeInHierarchy;
        private bool dailyRewardOpening;
        private CancellationTokenSource onlineRewardSession;
        public void OpenShopForTutorial() => OpenShop();
        public event Action<FoundationDemoPanel> DemoPanelReady;
        public event Action ScoreAdded;
        public event Action ShopOpening;
        public event Action<ShopPanel> ShopReady;

        private IDatasaveService datasave;
        private TemplateSave saveData;
        private FoundationDemoPanel panel;
        private ShopPanel shopPanel;
        private DailyRewardPanel dailyRewardPanel;
        
        private int score;
        private float health = 100f;
        private bool isTransitioning;
        private bool isShuttingDown;

        private void Awake()
        {
            if (togglePanelButton == null)
            {
                Debug.LogError("[FoundationDemo] Toggle panel button is not assigned.", this);
                enabled = false;
                return;
            }

            if (featureShopPrefab == null)
            {
                Debug.LogError("[FoundationDemo] Feature Shop prefab is not assigned.", this);
                enabled = false;
                return;
            }

            if (settingsPrefab == null)
            {
                Debug.LogError("[FoundationDemo] Settings prefab must be assigned.", this);
                enabled = false;
                return;
            }

            togglePanelButton.onClick.AddListener(TogglePanel);
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                await UniTask.WaitUntil(
                    () => GameInstaller.State is BootstrapState.Ready or BootstrapState.Failed,
                    cancellationToken: this.GetCancellationTokenOnDestroy());

                if (GameInstaller.State == BootstrapState.Failed)
                {
                    Debug.LogError(
                        $"[FoundationDemo] Bootstrap failed: {GameInstaller.InitializationException}",
                        this);
                    return;
                }

                InitializeDemo();
                await SetPanelVisibleAsync(true);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void InitializeDemo()
        {
            datasave = ServiceLocator.Get<IDatasaveService>();
            saveData = datasave.Load<TemplateSave>();
            saveData.LaunchCount++;
            datasave.Save(saveData);
        }

        private async UniTask CreatePanelAsync()
        {
            FoundationDemoPanel created = await PanelManager.Instance.Show<FoundationDemoPanel>(
                Address.FoundationDemoPanel);
            if (isShuttingDown)
            {
                await created.Hide();
                return;
            }

            panel = created;
            BindPanel(created);
            RefreshPanel();

            TemplateConfig config = ServiceLocator.Get<IDataConfigService>()
                .GetTable<TemplateConfig>();
            created.SetStatus(
                $"PASS | launch={saveData.LaunchCount} | config coins={config.StartingCoins}");
            DemoPanelReady?.Invoke(created);
        }

        private void BindPanel(FoundationDemoPanel target)
        {
            target.OnlineRewardRequested += ClaimOnlineReward;
            target.AddScoreRequested += AddScore;
            target.DamageRequested += Damage;
            target.HealRequested += Heal;
            target.SaveRequested += Save;
            target.LoadRequested += Load;
            target.OpenShopRequested += OpenShop;
            target.OpenDailyRewardRequested += OpenDailyReward;
            target.OpenSettingsRequested += OpenSettings;
            target.OpenRateUsRequested += OpenRateUs;
            target.Destroyed += OnPanelDestroyed;
        }

        private void UnbindPanel(FoundationDemoPanel target)
        {
            if (ReferenceEquals(target, null)) return;
            onlineRewardSession?.Cancel();
            target.OnlineRewardRequested -= ClaimOnlineReward;
            target.AddScoreRequested -= AddScore;
            target.DamageRequested -= Damage;
            target.HealRequested -= Heal;
            target.SaveRequested -= Save;
            target.LoadRequested -= Load;
            target.OpenShopRequested -= OpenShop;
            target.OpenDailyRewardRequested -= OpenDailyReward;
            target.OpenSettingsRequested -= OpenSettings;
            target.OpenRateUsRequested -= OpenRateUs;
            target.Destroyed -= OnPanelDestroyed;
        }

        private void ClaimOnlineReward()
        {
            if (isTransitioning || isShuttingDown || HasFeatureDemo || panel == null) return;
            ClaimOnlineRewardAsync(panel).Forget();
        }

        private async UniTaskVoid ClaimOnlineRewardAsync(FoundationDemoPanel target)
        {
            var connectivity = FindFirstObjectByType<ConnectivityRoot>();
            if (connectivity == null || connectivity.Feature == null)
            {
                target.SetStatus("Online Reward unavailable: ConnectivityRoot is not ready.");
                return;
            }

            using var session = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy(), target.GetCancellationTokenOnDestroy());
            onlineRewardSession = session;
            target.SetOnlineRewardBusy(true);
            target.SetStatus("Online Reward: checking connection...");
            try
            {
                // Demo reward: only dispatch after the shared connectivity preflight succeeds.
                // Never automatically repeat a dispatched reward mutation.
                var result = await connectivity.ExecuteWhenConnectedAsync("foundation.online-reward",
                    token =>
                    {
                        token.ThrowIfCancellationRequested();
                        score += 10;
                        RefreshPanel();
                        return UniTask.FromResult(ConnectedOperationResult.Success());
                    }, new ConnectivityRetryPolicy(), session.Token);
                if (target != null && !isShuttingDown)
                    target.SetStatus(result.Status == ConnectivityActionStatus.Succeeded
                        ? "Online Reward received: +10 Score (demo)"
                        : $"Online Reward: {result.Status}. No reward granted.");
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                if (target != null) target.SetStatus("Online Reward failed. See Console.");
                Debug.LogException(exception, this);
            }
            finally
            {
                onlineRewardSession = null;
                if (target != null) target.SetOnlineRewardBusy(false);
            }
        }

        private void AddScore()
        {
            score += 10;
            RefreshPanel();
            panel?.SetStatus($"Score changed | score={score}");
            ScoreAdded?.Invoke();
        }

        private void Damage()
        {
            health = Mathf.Max(0f, health - 10f);
            RefreshPanel();
        }

        private void Heal()
        {
            health = Mathf.Min(100f, health + 10f);
            RefreshPanel();
        }

        private void Save()
        {
            saveData.Coins = score;
            saveData.Score = Mathf.Max(saveData.Score, score);
            datasave.Save(saveData);
            panel?.SetStatus($"Save PASS | coins={saveData.Coins}");
        }

        private void Load()
        {
            TemplateConfig config = ServiceLocator.Get<IDataConfigService>()
                .GetTable<TemplateConfig>();
            score = config.StartingCoins;
            RefreshPanel();
            panel?.SetStatus(
                $"Config Load PASS | coins={config.StartingCoins} ");
        }

        private void OpenShop()
        {
            if (isTransitioning || isShuttingDown || HasFeatureDemo) return;
            ShopOpening?.Invoke();
            OpenShopAsync().Forget();
        }

        private void OpenDailyReward()
        {
            if (isTransitioning || isShuttingDown || HasFeatureDemo || dailyRewardOpening || dailyRewardPrefab == null) return;
            dailyRewardOpening = true;
            OpenDailyRewardAsync().Forget();
        }

        private void OpenSettings()
        {
            if (isTransitioning || isShuttingDown || HasFeatureDemo || dailyRewardOpening) return;
            if (settingsPrefab != null) PanelManager.Instance.Show<SettingsPanel>(settingsPrefab).Forget();
        }

        private void OpenRateUs()
        {
            if (isTransitioning || isShuttingDown || HasFeatureDemo || dailyRewardOpening) return;
            PanelManager.Instance.Show<RateUsPanel>(Address.RateUsPanel).Forget();
        }

        private async UniTaskVoid OpenDailyRewardAsync()
        {
            DailyRewardPanel created = null;
            try
            {
                created = await PanelManager.Instance.Create<DailyRewardPanel>(dailyRewardPrefab.gameObject);
                if (isShuttingDown) return;
                dailyRewardPanel = created;
                dailyRewardPanel.Destroyed -= OnDailyRewardDestroyed;
                dailyRewardPanel.Destroyed += OnDailyRewardDestroyed;
                await created.Show();
            }
            catch (Exception exception)
            {
                if (created != null) Destroy(created.gameObject);
                OnDailyRewardDestroyed();
                Debug.LogException(exception, this);
            }
            finally { dailyRewardOpening = false; }
        }

        private void OnDailyRewardDestroyed()
        {
            if (dailyRewardPanel != null)
            {
                dailyRewardPanel.Destroyed -= OnDailyRewardDestroyed;
                dailyRewardPanel = null;
            }

        }

        private async UniTaskVoid OpenShopAsync()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            try
            {
                if (panel != null)
                {
                    FoundationDemoPanel hidingPanel = panel;
                    panel = null;
                    UnbindPanel(hidingPanel);
                    await hidingPanel.Hide();
                }

                ShopPanel created = await PanelManager.Instance.Create<ShopPanel>(featureShopPrefab.gameObject);
                if (isShuttingDown) return;
                shopPanel = created;
                shopPanel.Destroyed -= OnShopDestroyed;
                shopPanel.Destroyed += OnShopDestroyed;
                await shopPanel.Show();
                ShopReady?.Invoke(created);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private void OpenDemoFromShop()
        {
            OpenDemoFromShopAsync().Forget();
        }

        private async UniTaskVoid OpenDemoFromShopAsync()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            try
            {
                if (shopPanel != null)
                {
                    ShopPanel hidingShop = shopPanel;
                    shopPanel = null;
                    hidingShop.Destroyed -= OnShopDestroyed;
                    await hidingShop.Hide();
                }

                if (panel == null)
                {
                    await CreatePanelAsync();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private void OnShopDestroyed()
        {
            if (shopPanel != null)
            {
                shopPanel.Destroyed -= OnShopDestroyed;
                shopPanel = null;
            }

            if (!isShuttingDown)
            {
                OpenDemoFromShop();
            }
        }

        private void RefreshPanel()
        {
            if (panel == null) return;
            panel.SetScore(score);
            panel.SetHealth(health);
        }

        private void TogglePanel()
        {
            if (!isTransitioning && !HasFeatureDemo && !dailyRewardOpening)
            {
                bool showDemo = (panel == null && shopPanel == null);
                SetPanelVisibleAsync(showDemo).Forget();
            }
        }

        private async UniTask SetPanelVisibleAsync(bool visible)
        {
            isTransitioning = true;
            togglePanelButton.interactable = false;
            try
            {
                if (visible)
                {
                    if (panel == null && shopPanel == null) await CreatePanelAsync();
                }
                else
                {
                    if (panel != null)
                    {
                        FoundationDemoPanel hidingPanel = panel;
                        panel = null;
                        UnbindPanel(hidingPanel);
                        await hidingPanel.Hide();
                    }
                    if (shopPanel != null)
                    {
                        ShopPanel hidingShop = shopPanel;
                        shopPanel = null;
                        hidingShop.Destroyed -= OnShopDestroyed;
                        await hidingShop.Hide();
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                isTransitioning = false;
                if (!isShuttingDown && togglePanelButton != null)
                {
                    togglePanelButton.interactable = true;
                }
            }
        }

        private void OnPanelDestroyed()
        {
            FoundationDemoPanel destroyed = panel;
            panel = null;
            UnbindPanel(destroyed);
        }

        private void OnDestroy()
        {
            isShuttingDown = true;
            UnbindPanel(panel);
            panel = null;

            if (shopPanel != null)
            {
                shopPanel.Destroyed -= OnShopDestroyed;
                shopPanel = null;
            }

            if (dailyRewardPanel != null)
            {
                dailyRewardPanel.Destroyed -= OnDailyRewardDestroyed;
                dailyRewardPanel = null;
            }

            if (togglePanelButton != null)
            {
                togglePanelButton.onClick.RemoveListener(TogglePanel);
            }
        }
    }
}
