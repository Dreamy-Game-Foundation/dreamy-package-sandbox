using Cysharp.Threading.Tasks;
using Dreamy.Core;

using Dreamy.UI;
using Dreamy.Settings;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    [RequireComponent(typeof(SettingsPanel))]
    public sealed class SettingsPanelLauncher : MonoBehaviour
    {
        
        [SerializeField] private GameObject rateUsPanelPrefab;
private SettingsPanel panel;
        private SettingsPresenter presenter;

private void Awake()
        {
            panel = GetComponent<SettingsPanel>();
            
            panel.OpenRateUsRequested += OpenRateUs;
panel.CloseRequested += OnCloseRequested;
            presenter = new SettingsPresenter(SettingsSampleInstaller.EnsureInstalled(), panel);
            ShowAsync().Forget();
        }

        private async UniTaskVoid ShowAsync()
        {
            await panel.Init();
            await panel.PostInit();
            presenter.Show();
            await panel.Show();
        }

        private void OpenRateUs()
        {
            if (rateUsPanelPrefab != null) Instantiate(rateUsPanelPrefab, PanelManager.Instance.transform);
        }

private void OnCloseRequested()
        {
            panel.CloseRequested -= OnCloseRequested;
            presenter?.Dispose();
            presenter = null;
            panel.Close();
        }

        private void OnDestroy()
        {
            
            if (panel != null) panel.OpenRateUsRequested -= OpenRateUs;
if (panel != null) panel.CloseRequested -= OnCloseRequested;
            presenter?.Dispose();
        }
    }
}