using Dreamy.Core;
using Dreamy.Settings;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    public sealed class SettingsController : MonoBehaviour
    {
        [SerializeField] private SettingsPanel panel;
        private SettingsPresenter presenter;

        private void Awake() => presenter = new SettingsPresenter(ServiceLocator.Get<ISettingsService>(), panel);
        private void OnEnable() => presenter.Show();
        private void OnDestroy() => presenter?.Dispose();
    }
}
