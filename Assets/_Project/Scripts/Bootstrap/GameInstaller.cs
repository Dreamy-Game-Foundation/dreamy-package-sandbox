using System;
using Dreamy.Economy.UI;
using Dreamy.Shop.UI;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Core;
using Dreamy.Core.Pooling;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.DailyReward;
using Dreamy.Missions;
using Dreamy.Progression;
using Dreamy.LuckyWheel;
using Dreamy.Tutorial;
using Dreamy.Economy;
using Dreamy.Economy.Rewards;
using Dreamy.Feedback;
using Dreamy.Template.Demo;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Feature.DailyReward.Integration;
using Dreamy.Feature.Missions.Integration;
using Dreamy.Feature.Progression.Integration;
using Dreamy.Feature.LuckyWheel.Integration;
using Dreamy.Tutorial.Integration;
using Dreamy.Feature.Tutorial.Integration;
using Dreamy.Settings;
using Dreamy.UI;
using Dreamy.Shop;
using Dreamy.Template.Demo;
using Dreamy.Template.Pooling;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamy.Template
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] private bool prettySaveInEditor = true;
        [SerializeField] private string saveDirectory = "DreamySaves";
        [SerializeField] private DreamyAudioProfile audioProfile;
        [SerializeField] private string storeListingUrl;
        [SerializeField] private FeedbackHost feedbackPrefab;
        [SerializeField, Tooltip("Camera used by Feedback camera shake and world-to-UI text. Falls back to MainCamera.")]
        private Camera feedbackCamera;
        private IInsufficientResourcesPresentation shopResourceRecovery;

        private IShopPurchaseGateway demoShopPurchaseGateway;
        private FeedbackHost feedbackInstance;
        private IDisposable feedbackRegistration;
        public static FeedbackHost Feedback { get; private set; }

        private IDatasaveService datasave;
        private IPoolService poolService;
        private IResourceWallet resourceWallet;
        public ResourceAcquisitionCoordinator ResourceAcquisition { get; private set; }
        private PanelPresenterFactory presenterFactory;
        private readonly List<PresenterViewHost> viewHosts = new();
        private bool ownsSettingsService;
        private readonly List<EntitlementEffectBinding> entitlementBindings = new();

        public static BootstrapState State { get; private set; }
        public static Exception InitializationException { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            State = BootstrapState.Initializing;
            try
            {
                var cancellationToken = this.GetCancellationTokenOnDestroy();

                // Register the pool before any consumer is installed.
                InstallPoolService();

                // 1. Install Datasave Service
                InstallDatasaveService();

                // 2. Install Pool and audio services
                InstallOtherServices();
                InstallAudioService();
                InstallFeedbackService();


                // 3. Install DataConfig Service (called last due to async loading)
                await InstallDataConfigServiceAsync(cancellationToken);
                InstallPanelPresenters();

                State = BootstrapState.Ready;
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                State = BootstrapState.Failed;
                ReleaseFeedbackAndPool();
                Debug.LogException(exception, this);
            }
        }

        private void InstallPanelPresenters()
        {
            presenterFactory = new PanelPresenterFactory();
            InstallShopPurchaseGateway();
            ServiceLocator.TryGet<IShopPurchaseGateway>(out var purchases);
            var shopCatalog = ServiceLocator.Get<IDataConfigService>().GetTable<ShopCatalogConfig>();
            var destinations = new ShopResourceDestinationMap(new Dictionary<ResourceId, string>
            {
                [new ResourceId("currency.coin")] = "currency",
                [new ResourceId("currency.gem")] = "currency"
            });
            var navigator = new ShopPanelResourceNavigator(async token =>
            {
                token.ThrowIfCancellationRequested();
                if (!PanelManager.Instance.TryGet<ShopPanel>(out var panel)) return null;
                await panel.Show();
                token.ThrowIfCancellationRequested();
                return panel;
            });
            var shopAcquisition = new ResourceAcquisitionCoordinator(new ShopResourceAcquisitionGateway(
                shopCatalog, destinations, navigator, (IResourceBalanceProvider)resourceWallet));
            var recoveryRoutes = new Dictionary<ResourceId, IInsufficientResourcesPresentation>();
            recoveryRoutes.Add(new ResourceId("currency.coin"), ResourceSuggestionInstaller.Install<CoinSuggestionPanel>(
                presenterFactory, new ResourceId("currency.coin"), Address.CoinSuggestionPanel,
                (IResourceBalanceSource)resourceWallet, shopAcquisition));
            recoveryRoutes.Add(new ResourceId("currency.gem"), ResourceSuggestionInstaller.Install<GemSuggestionPanel>(
                presenterFactory, new ResourceId("currency.gem"), Address.GemSuggestionPanel,
                (IResourceBalanceSource)resourceWallet, shopAcquisition));
            shopResourceRecovery = new ResourceRecoveryPresentationRouter(
                (IResourceBalanceProvider)resourceWallet, recoveryRoutes);
            ShopFeatureInstaller.Install(presenterFactory, shopCatalog, resourceWallet,
                purchases, (IResourceBalanceProvider)resourceWallet, feedbackInstance, RecoverShopResourcesAsync);

            ownsSettingsService = !ServiceLocator.TryGet<ISettingsService>(out var settings);
            if (ownsSettingsService)
            {
                ServiceLocator.TryGet<ISettingsPlatformGateway>(out var platform);
                ServiceLocator.TryGet<ISettingsHapticsGateway>(out var haptics);
                ServiceLocator.TryGet<ISettingsReviewGateway>(out var review);
                if (haptics == null && platform is not ISettingsHapticsGateway)
                {
                    var hapticService = feedbackInstance.Haptic;
                    haptics = new PlayerPrefsSettingsHapticsGateway(
                        enabled => hapticService.Enabled = enabled,
                        () => hapticService != null &&
                              (Application.platform == RuntimePlatform.Android ||
                               Application.platform == RuntimePlatform.IPhonePlayer));
                }
                SettingsFeatureInstaller.Install(presenterFactory, ServiceLocator.Get<IAudioService>(), OpenRateUs,
                    platform ?? new SettingsPlatformGateway(storeListingUrl), haptics,
                    review ?? new PlayerPrefsSettingsReviewGateway(resourceWallet));
            }
            else SettingsFeatureInstaller.Install(presenterFactory, settings, OpenRateUs);
            var config = ServiceLocator.Get<IDataConfigService>();
            var rewardGateway = new RewardDeliveryGateway(new IRewardDestination[]
            {
                (IRewardDestination)resourceWallet,
                ((DatasaveResourceWallet)resourceWallet).CreateRewardDestination("item")
            });
            ServiceLocator.TryGet<IRewardClock>(out var rewardClock);
            DailyRewardFeatureInstaller.Install(presenterFactory, config.GetTable<DailyRewardScheduleConfig>(),
                datasave, rewardGateway, rewardClock ?? new SystemRewardClock(), feedbackHost: feedbackInstance);
            MissionFeatureInstaller.Install(presenterFactory, config.GetTable<MissionCatalogConfig>(), datasave, rewardGateway);
            ServiceLocator.TryGet<ILuckyWheelClock>(out var wheelClock);
            ServiceLocator.TryGet<ILuckyWheelRandom>(out var wheelRandom);
            LuckyWheelFeatureInstaller.Install(presenterFactory, config.GetTable<LuckyWheelConfig>(), datasave,
                resourceWallet, wheelClock ?? new SystemLuckyWheelClock(), wheelRandom ?? new SystemLuckyWheelRandom());
            var progressionDemo = new FoundationProgressionRewards(
                config.GetTable<ProgressionCatalogConfig>(), datasave, (DatasaveResourceWallet)resourceWallet);
            ServiceLocator.Register<FoundationProgressionRewards>(progressionDemo);
            ServiceLocator.Register<IProgressionService>(progressionDemo.Model);
            ServiceLocator.Register<IProgressionRewardService>(progressionDemo.Model);
            ProgressionFeatureInstaller.Install(presenterFactory, progressionDemo.Model);
            TutorialFeatureInstaller.Install(presenterFactory, config.GetTable<TutorialCatalogConfig>(), datasave,
                view => view.GetComponent<TutorialTargetRegistry>(), "foundation-tutorials");
            AttachPanelPresenters();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var camera = feedbackCamera ? feedbackCamera : Camera.main;
            if (feedbackInstance && camera && feedbackInstance.WorldCamera != camera)
                BindFeedbackCamera(camera);
            AttachPanelPresenters();
        }

        private void AttachPanelPresenters()
        {
            if (presenterFactory == null) return;
            if (PanelManager.HasInstance) PanelManager.Instance.PresenterFactory = presenterFactory;
            viewHosts.RemoveAll(host => host == null);
            foreach (var view in UnityEngine.Object.FindObjectsByType<ProgressionHud>(
                FindObjectsInactive.Include))
            {
                var host = ProgressionFeatureInstaller.Attach(presenterFactory, view);
                if (!viewHosts.Contains(host)) viewHosts.Add(host);
            }
        }

        private void OpenRateUs()
        {
            if (PanelManager.HasInstance)
                PanelManager.Instance.Show<RateUsPanel>(Address.RateUsPanel).Forget();
        }

        private void InstallShopPurchaseGateway()
        {
            if (ServiceLocator.IsRegistered<IShopPurchaseGateway>()) return;

            demoShopPurchaseGateway = new ShopPurchaseGateway((request, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return UniTask.FromResult(ShopGatewayPurchaseResult.Cancelled());

                string transactionId = $"demo:{Guid.NewGuid():N}";
                Debug.Log($"[Shop Demo] Simulated IAP purchase: {request.StoreProductId} " +
                          $"(offer={request.OfferId}, transaction={transactionId}).", this);
                return UniTask.FromResult(ShopGatewayPurchaseResult.Purchased(transactionId));
            });
            ServiceLocator.Register<IShopPurchaseGateway>(demoShopPurchaseGateway);
        }

        private void ReleaseShopPurchaseGateway()
        {
            if (demoShopPurchaseGateway != null &&
                ServiceLocator.TryGet<IShopPurchaseGateway>(out var current) &&
                ReferenceEquals(current, demoShopPurchaseGateway))
                ServiceLocator.Unregister<IShopPurchaseGateway>();
            demoShopPurchaseGateway = null;
        }

        private void InstallDatasaveService()
        {
            ISaveCodec codec;
#if UNITY_EDITOR
            codec = new PlainTextSaveCodec();
#else
            codec = new XorSaveCodec("Dreamy123@");
            // codec = new AesSaveCodec("Dreamy123@");
#endif

            datasave = new DatasaveService(new DatasaveOptions
            {
                DirectoryName = saveDirectory,
                PrettyPrint = Application.isEditor && prettySaveInEditor,
                Codec = codec
            });
            ServiceLocator.Register<IDatasaveService>(datasave);
        }

        private void InstallPoolService()
        {
            if (poolService != null || ServiceLocator.IsRegistered<IPoolService>())
                throw new InvalidOperationException("A pool service is already installed. Only one bootstrap may own the registration.");
            poolService = new LeanPoolService();
            ServiceLocator.Register<IPoolService>(poolService);
        }

        private void ReleaseFeedbackAndPool()
        {
            feedbackRegistration?.Dispose(); feedbackRegistration = null;
            if (feedbackInstance != null) feedbackInstance.Shutdown();
            if (ReferenceEquals(Feedback, feedbackInstance)) Feedback = null;
            if (feedbackInstance != null) Destroy(feedbackInstance.gameObject);
            feedbackInstance = null;
            if (poolService == null) return;
            if (ServiceLocator.TryGet<IPoolService>(out var registered) && ReferenceEquals(registered, poolService))
                ServiceLocator.Unregister<IPoolService>();
            if (poolService is IDisposable disposable) disposable.Dispose();
            poolService = null;
        }

        private async UniTask RecoverShopResourcesAsync(ShopPurchaseResult result, CancellationToken token)
        {
            if (result.Offer == null || result.Status != ShopPurchaseStatus.InsufficientResources) return;
            await shopResourceRecovery.ShowAsync(new[] { result.Offer.Cost }, "shop:" + result.Offer.Id, token);
        }

        private void InstallOtherServices()
        {
            resourceWallet = new DatasaveResourceWallet(
                datasave,
                initialBalances: new[] { new ResourceAmount(new ResourceId("currency.coin"), 100) },
                rewardDefinitions: FoundationProgressionRewards.RewardDefinitions);
            FoundationProgressionRewards.ImportLegacyInventories(datasave, (DatasaveResourceWallet)resourceWallet);
            ServiceLocator.Register<IResourceWallet>(resourceWallet);
            ServiceLocator.Register<IResourceBalanceProvider>((IResourceBalanceProvider)resourceWallet);
            ResourceAcquisition = new ResourceAcquisitionCoordinator(new Demo.CustomShopResourceAcquisitionGateway(async token =>
            {
                using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy());
                var panel = await PanelManager.Instance.Create<Demo.UIShopPanel>(Address.ShopPanel);
                lifetime.Token.ThrowIfCancellationRequested();
                await panel.Show();
                lifetime.Token.ThrowIfCancellationRequested();
                return panel;
            }, this.GetCancellationTokenOnDestroy()));
            InstallEntitlementEffects((IResourceBalanceSource)resourceWallet);
        }

        private void InstallEntitlementEffects(IResourceBalanceSource balanceSource)
        {
            // Register once, after wallet load and SDK/service readiness.
            // Production: replace these demo effects with SDK/domain service callbacks.
            BindEntitlement(balanceSource, EntitlementIds.RemoveAds, ApplyNoAdsDemo);
            BindEntitlement(balanceSource, EntitlementIds.LifetimeVip, ApplyLifetimeVipDemo);
            BindEntitlement(balanceSource, EntitlementIds.StarterFund, UnlockStarterFundDemo);
            BindEntitlement(balanceSource, EntitlementIds.Season1PremiumPass, UnlockSeason1PremiumPassDemo);
        }

        private void BindEntitlement(IResourceBalanceSource source, ResourceId id, Action effect)
        {
            entitlementBindings.Add(new EntitlementEffectBinding(
                source, id, effect, exception => Debug.LogException(exception, this)));
        }

        private void ApplyNoAdsDemo() =>
            Debug.Log("[Shop Demo] No Ads entitlement is owned. Production adapter calls DreamySDK.SetNoAds().", this);

        private void ApplyLifetimeVipDemo() =>
            Debug.Log("[Shop Demo] Lifetime VIP owned. Production: enable permanent VIP benefits.", this);

        private void UnlockStarterFundDemo() =>
            Debug.Log("[Shop Demo] Starter Fund owned. Production: unlock its milestone claim service.", this);

        private void UnlockSeason1PremiumPassDemo() =>
            Debug.Log("[Shop Demo] Season 1 premium pass owned. Production: unlock that season's premium track.", this);

        private void InstallAudioService()
        {
            if (audioProfile == null)
            {
                throw new InvalidOperationException("GameInstaller requires a DreamyAudioProfile for Settings.");
            }

            DreamyAudio.Initialize(audioProfile);
            ServiceLocator.Register<IAudioService>(DreamyAudio.Service);
        }

        private void InstallFeedbackService()
        {
            if (feedbackPrefab == null) throw new InvalidOperationException("GameInstaller needs the FeedbackRig sample prefab.");
            feedbackInstance = Instantiate(feedbackPrefab, transform);
            Feedback = feedbackInstance;
            BindFeedbackCamera(feedbackCamera ? feedbackCamera : Camera.main);
        }

        private void BindFeedbackCamera(Camera camera)
        {
            feedbackRegistration?.Dispose();
            feedbackInstance.Initialize(camera);
            feedbackRegistration = FeedbackServiceRegistry.RegisterOwned(feedbackInstance.Vfx, feedbackInstance.Haptic, feedbackInstance.FloatingText,
                feedbackInstance.IconFly, feedbackInstance.Screen, feedbackInstance.CameraShake, feedbackInstance.Feedback, feedbackInstance.Ui);
        }

        private async UniTask InstallDataConfigServiceAsync(CancellationToken cancellationToken)
        {
            IRemoteConfigProvider remoteConfigProvider = new RemoteDataConfigProvider();
            IDataConfigSource configSource = new CompositeConfigSource(new IDataConfigSource[]
            {
                new RemoteConfigSource(remoteConfigProvider),
                new ResourcesJsonConfigSource()
            });

            var dataConfig = new DataConfigService(configSource);
            dataConfig.Register<TemplateConfig>("templateConfig");

            // Example of registering a Table Config
            dataConfig.Register<DataConfigTable<TestConfig>>("testConfigs");
            dataConfig.Register<OfferConfigTable>("offerConfigs");
            ShopFeatureInstaller.RegisterConfig(dataConfig);
            DailyRewardFeatureInstaller.RegisterConfig(dataConfig);
            MissionFeatureInstaller.RegisterConfig(dataConfig);
            LuckyWheelFeatureInstaller.RegisterConfig(dataConfig);
            ProgressionFeatureInstaller.RegisterConfig(dataConfig);
            TutorialFeatureInstaller.RegisterConfig(dataConfig);

            await dataConfig.InitializeAsync(cancellationToken);
            ServiceLocator.Register<IDataConfigService>(dataConfig);

            // Example of retrieving and printing rows from the Table Config
            var testTable = dataConfig.GetTable<DataConfigTable<TestConfig>>();
            foreach (var row in testTable.GetAll())
            {
                Debug.Log($"[TestConfig] Id: {row.Id}, Name: {row.Name}, Value: {row.Value}");
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) datasave?.SaveAll();
        }

        private void OnApplicationQuit() => datasave?.SaveAll();

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (PanelManager.HasInstance && ReferenceEquals(PanelManager.Instance.PresenterFactory, presenterFactory))
                PanelManager.Instance.PresenterFactory = null;
            foreach (var host in viewHosts) if (host != null) host.Release();
            viewHosts.Clear();
            presenterFactory = null;
            if (ownsSettingsService) ServiceLocator.Unregister<ISettingsService>();
            foreach (EntitlementEffectBinding binding in entitlementBindings) binding.Dispose();
            entitlementBindings.Clear();
            ReleaseFeedbackAndPool();
            ResourceAcquisition = null;
            ServiceLocator.Unregister<IResourceWallet>();
            ServiceLocator.Unregister<IResourceBalanceProvider>();
            ServiceLocator.Unregister<IAudioService>();
            ServiceLocator.Unregister<IShopService>();
            ReleaseShopPurchaseGateway();
            ServiceLocator.Unregister<IDailyRewardService>();
            ServiceLocator.Unregister<IMissionService>();
            ServiceLocator.Unregister<ILuckyWheelService>();
            ServiceLocator.Unregister<IProgressionService>();
            ServiceLocator.Unregister<IProgressionRewardService>();
            ServiceLocator.Unregister<FoundationProgressionRewards>();
            ServiceLocator.Unregister<ITutorialService>();
        }
    }
}
