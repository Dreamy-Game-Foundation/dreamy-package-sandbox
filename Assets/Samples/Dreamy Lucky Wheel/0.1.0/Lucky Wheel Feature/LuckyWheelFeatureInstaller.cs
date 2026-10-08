using System;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.LuckyWheel;
using Dreamy.UI;

namespace Dreamy.Feature.LuckyWheel.Integration
{
    public static class LuckyWheelFeatureInstaller
    {
        public static void RegisterConfig(IDataConfigService config) => LuckyWheelInstaller.RegisterConfig(config);

        public static ILuckyWheelService Install(PanelPresenterFactory factory, LuckyWheelConfig config, IDatasaveService save, IResourceWallet wallet, ILuckyWheelClock clock, ILuckyWheelRandom random, string saveKey = LuckyWheelInstaller.DefaultSaveKey)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            return Install(factory, LuckyWheelInstaller.Install(config, save, wallet, clock, random, saveKey));
        }

        public static ILuckyWheelService Install(PanelPresenterFactory factory, ILuckyWheelService service)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (service == null) throw new ArgumentNullException(nameof(service));
            factory.Register<LuckyWheelPanel>(view => new LuckyWheelPresenter(service, view));
            return service;
        }
    }
}
