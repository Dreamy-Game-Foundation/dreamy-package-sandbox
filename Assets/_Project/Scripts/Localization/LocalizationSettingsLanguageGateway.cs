using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Localization;
using Dreamy.Settings;
namespace Dreamy.Template.Localization
{
    public sealed class LocalizationSettingsLanguageGateway : ISettingsLanguageGateway, IDisposable
    {
        private readonly ILocalizationService service;
        private readonly List<SettingsLanguageOption> options = new();
        private bool disposed;
        public IReadOnlyList<SettingsLanguageOption> Options => options;
        public string CurrentCode => service.CurrentLocaleCode;
        public event Action LanguageChanged;
        public LocalizationSettingsLanguageGateway(ILocalizationService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            if (!service.IsInitialized) throw new InvalidOperationException("Initialize localization before installing Settings.");
            foreach (var code in service.AvailableLocaleCodes)
                options.Add(new SettingsLanguageOption(code, CultureInfo.GetCultureInfo(code).NativeName));
            service.LocaleChanged += Changed;
        }
        private void Changed(string _) => LanguageChanged?.Invoke();
        public UniTask<bool> SetLanguageAsync(string code, CancellationToken token = default)
        {
            if (disposed) throw new ObjectDisposedException(nameof(LocalizationSettingsLanguageGateway));
            return service.SetLocaleAsync(code, token);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; service.LocaleChanged -= Changed; LanguageChanged = null;
        }
    }
}
