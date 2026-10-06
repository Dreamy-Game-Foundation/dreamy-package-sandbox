using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Settings;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    public sealed class SettingsController : MonoBehaviour
    {
        [SerializeField] private SettingsPanel panel;
        [SerializeField] private RateUsPanel rateUsPanel;
        private SettingsPresenter presenter;

        private void Awake()
        {
            presenter = new SettingsPresenter(ServiceLocator.Get<ISettingsService>(), panel);
            panel.OpenRateUsRequested += OpenRateUs;
        }

        private void OnEnable() => presenter.Show();

        private void OnDestroy()
        {
            if (panel != null) panel.OpenRateUsRequested -= OpenRateUs;
            presenter?.Dispose();
        }

        private void OpenRateUs()
        {
            if (rateUsPanel != null) rateUsPanel.Show().Forget();
        }
    }
}
