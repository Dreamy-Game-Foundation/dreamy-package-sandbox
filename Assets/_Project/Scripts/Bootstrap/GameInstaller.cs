using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.DailyReward;
using Dreamy.Missions;
using Dreamy.Progression;
using Dreamy.LuckyWheel;
using Dreamy.Tutorial;
using Dreamy.Economy;
using Dreamy.Feedback;
using Dreamy.Feedback.Samples.Economy;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Shop;
using Dreamy.Template.Demo;
using Dreamy.Template.Pooling;
using Newtonsoft.Json;
using UnityEngine;

namespace Dreamy.Template
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] private bool prettySaveInEditor = true;
        [SerializeField] private string saveDirectory = "DreamySaves";
        [SerializeField] private DreamyAudioProfile audioProfile;
        [SerializeField] private FeedbackHost feedbackPrefab;
        private FeedbackRewardPresenter feedbackPresenter;
        private FeedbackHost feedbackInstance;
        private IDisposable feedbackRegistration;
        public static FeedbackHost Feedback { get; private set; }

        private IDatasaveService datasave;
        private IPoolService poolService;
        private IResourceWallet resourceWallet;

        public static BootstrapState State { get; private set; }
        public static Exception InitializationException { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            State = BootstrapState.Initializing;
            try
            {
                var cancellationToken = this.GetCancellationTokenOnDestroy();

                // 1. Install Datasave Service
                InstallDatasaveService();

                // 2. Install Pool and audio services
                InstallOtherServices();
                InstallAudioService();
                InstallFeedbackService();


                // 3. Install DataConfig Service (called last due to async loading)
                await InstallDataConfigServiceAsync(cancellationToken);
                ShopInstaller.Install();
                DailyRewardInstaller.Install();
                MissionInstaller.Install();
                ProgressionInstaller.Install();
                LuckyWheelInstaller.Install();

                State = BootstrapState.Ready;
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                State = BootstrapState.Failed;
                Debug.LogException(exception, this);
            }
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

        private void InstallOtherServices()
        {
            // Install Pool Service
            poolService = new LeanPoolService();
            ServiceLocator.Register<IPoolService>(poolService);

            resourceWallet = new DatasaveResourceWallet(
                datasave,
                initialBalances: new[] { new ResourceAmount(new ResourceId("currency.coin"), 100) });
            ServiceLocator.Register<IResourceWallet>(resourceWallet);
            ServiceLocator.Register<IResourceBalanceProvider>((IResourceBalanceProvider)resourceWallet);
            ServiceLocator.Register<IShopPurchaseGateway>(new SimulatedShopPurchaseGateway());
        }

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
            feedbackInstance.Initialize();
            feedbackRegistration = FeedbackServiceRegistry.RegisterOwned(Feedback.Vfx, Feedback.Haptic, Feedback.FloatingText,
                Feedback.IconFly, Feedback.Screen, Feedback.CameraShake, Feedback.Sequence);
            feedbackPresenter = new FeedbackRewardPresenter((IResourceBalanceSource)resourceWallet,
                new ResourceId("currency.coin"), (_, delta) =>
                {
                    if (delta <= 0 || feedbackInstance == null || !feedbackInstance.isActiveAndEnabled) return;
                    feedbackInstance.PlayReward(delta, feedbackInstance.transform.Find("WorldTarget").position);
                    DreamyAudio.Service.Play("ui.click");
                });
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
            ShopInstaller.RegisterConfig(dataConfig);
            DailyRewardInstaller.RegisterConfig(dataConfig);
            TutorialInstaller.RegisterConfig(dataConfig);
            MissionInstaller.RegisterConfig(dataConfig);
            ProgressionInstaller.RegisterConfig(dataConfig);
            LuckyWheelInstaller.RegisterConfig(dataConfig);

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
            feedbackPresenter?.Dispose(); feedbackPresenter = null;
            feedbackRegistration?.Dispose(); feedbackRegistration = null;
            if (feedbackInstance != null) feedbackInstance.Shutdown();
            if (ReferenceEquals(Feedback, feedbackInstance)) Feedback = null;
            feedbackInstance = null;
            if (poolService is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ServiceLocator.Unregister<IPoolService>();
            ServiceLocator.Unregister<IResourceWallet>();
            ServiceLocator.Unregister<IResourceBalanceProvider>();
            ServiceLocator.Unregister<IShopPurchaseGateway>();
            ServiceLocator.Unregister<IAudioService>();
            ServiceLocator.Unregister<IShopService>();
            ServiceLocator.Unregister<IDailyRewardService>();
            ServiceLocator.Unregister<IMissionService>();
            ServiceLocator.Unregister<IProgressionService>();
            ServiceLocator.Unregister<ILuckyWheelService>();
        }
    }
}
