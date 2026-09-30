using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.DailyReward;
using Dreamy.Economy;
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
        [SerializeField] private DreamyAudioProfile audioProfile;

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


                // 3. Install DataConfig Service (called last due to async loading)
                await InstallDataConfigServiceAsync(cancellationToken);
                ShopInstaller.Install();
                DailyRewardInstaller.Install();

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
        }
    }
}
