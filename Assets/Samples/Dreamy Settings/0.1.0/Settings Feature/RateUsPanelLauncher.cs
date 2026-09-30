using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Settings;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    [RequireComponent(typeof(RateUsPanel))]
    public sealed class RateUsPanelLauncher : MonoBehaviour
    {
        private RateUsPanel panel;
        private RateUsPresenter presenter;

private void Awake()
        {
            panel = GetComponent<RateUsPanel>();
            panel.CloseRequested += OnCloseRequested;
            presenter = new RateUsPresenter(SettingsSampleInstaller.EnsureInstalled(), panel);
            ShowAsync().Forget();
        }

        private async UniTaskVoid ShowAsync()
        {
            await panel.Init();
            await panel.PostInit();
            presenter.Show();
            await panel.Show();
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
            if (panel != null) panel.CloseRequested -= OnCloseRequested;
            presenter?.Dispose();
        }
    }
}