using System;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;

namespace Dreamy.LuckyWheel
{
    public static class LuckyWheelInstaller
    {
        public const string DefaultConfigKey = "luckyWheel";
        public const string DefaultSaveKey = "lucky-wheel";

        public static void RegisterConfig(IDataConfigService dataConfigService)
        {
            if (dataConfigService == null)
            {
                throw new ArgumentNullException(nameof(dataConfigService));
            }

            dataConfigService.Register<LuckyWheelConfig>(DefaultConfigKey);
        }

        public static ILuckyWheelService Install(string saveKey = DefaultSaveKey)
        {
            IDataConfigService configService = ServiceLocator.Get<IDataConfigService>();
            IDatasaveService datasaveService = ServiceLocator.Get<IDatasaveService>();
            IResourceWallet resourceWallet = ServiceLocator.Get<IResourceWallet>();
            ILuckyWheelClock clock = ServiceLocator.TryGet<ILuckyWheelClock>(out ILuckyWheelClock registeredClock)
                ? registeredClock
                : new SystemLuckyWheelClock();
            ILuckyWheelRandom random = ServiceLocator.TryGet<ILuckyWheelRandom>(out ILuckyWheelRandom registeredRandom)
                ? registeredRandom
                : new SystemLuckyWheelRandom();

            return Install(
                configService.GetTable<LuckyWheelConfig>(),
                datasaveService,
                resourceWallet,
                clock,
                random,
                saveKey);
        }

        public static ILuckyWheelService Install(
            LuckyWheelConfig config,
            IDatasaveService datasaveService,
            IResourceWallet resourceWallet,
            ILuckyWheelClock clock,
            ILuckyWheelRandom random,
            string saveKey = DefaultSaveKey)
        {
            LuckyWheelModel service = new(
                config,
                datasaveService,
                resourceWallet,
                clock,
                random,
                saveKey);
            ServiceLocator.Register<ILuckyWheelService>(service);
            return service;
        }
    }
}
