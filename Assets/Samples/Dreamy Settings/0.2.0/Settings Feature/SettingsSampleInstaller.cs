using Dreamy.Core;
using Dreamy.Audio;
using Dreamy.Settings;

namespace Dreamy.Feature.Settings.Integration
{
    public static class SettingsSampleInstaller
    {
        public static ISettingsService EnsureInstalled()
        {
            if (ServiceLocator.TryGet<ISettingsService>(out ISettingsService settingsService))
            {
                return settingsService;
            }

            return SettingsInstaller.Install(
                ServiceLocator.Get<IAudioService>(),
                new SimulatedSettingsPlatformGateway());
        }
    }
}
