using Dreamy.Core;
using Dreamy.Settings;

namespace Dreamy.Feature.Settings.Integration
{
    internal static class SettingsSampleInstaller
    {
        public static ISettingsService EnsureInstalled()
        {
            if (ServiceLocator.TryGet<ISettingsService>(out ISettingsService settingsService))
            {
                return settingsService;
            }

            if (!ServiceLocator.TryGet<ISettingsPlatformGateway>(out ISettingsPlatformGateway platformGateway))
            {
                platformGateway = new SimulatedSettingsPlatformGateway();
                ServiceLocator.Register<ISettingsPlatformGateway>(platformGateway);
            }

            return SettingsInstaller.Install();
        }
    }
}
