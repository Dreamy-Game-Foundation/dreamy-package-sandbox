using System;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;
using Dreamy.Settings;
using Dreamy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feature.Settings.Integration
{
    public sealed class SettingsPanel : UIPanel, ISettingsView, ISettingsLanguageView
    {
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Button gdprButton;
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private Button openRateUsButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private TMP_Text languageError;
        private readonly List<string> languageCodes = new();
        public event Action<string> LanguageRequested;
        public void RenderLanguage(SettingsLanguageState state)
        {
            if (languageDropdown == null) return;
            languageDropdown.gameObject.SetActive(state.IsAvailable);
            if (languageError != null) languageError.gameObject.SetActive(!string.IsNullOrEmpty(state.ErrorKey));
            if (!state.IsAvailable) return;
            languageCodes.Clear();
            var labels = new List<string>();
            int selected = 0;
            foreach (var option in state.Options)
            {
                if (option.Code == state.CurrentCode) selected = languageCodes.Count;
                languageCodes.Add(option.Code); labels.Add(option.DisplayName);
            }
            languageDropdown.ClearOptions(); languageDropdown.AddOptions(labels);
            languageDropdown.SetValueWithoutNotify(selected);
            languageDropdown.RefreshShownValue();
            languageDropdown.interactable = !state.IsBusy;
        }
        private void RequestLanguage(int index)
        { if (index >= 0 && index < languageCodes.Count) LanguageRequested?.Invoke(languageCodes[index]); }


        public override bool CanBack => true;

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action<bool> HapticsEnabledChanged;
        public event Action GdprRequested;
        public event Action RestorePurchasesRequested;
        public event Action OpenRateUsRequested;
        public event Action CloseRequested;

        private void OnEnable()
        {
            if (languageDropdown != null) languageDropdown.onValueChanged.AddListener(RequestLanguage);
            musicToggle.onValueChanged.AddListener(RequestMusicToggle);
            sfxToggle.onValueChanged.AddListener(RequestSfxToggle);
            if (hapticsToggle != null) hapticsToggle.onValueChanged.AddListener(RequestHapticsToggle);
            gdprButton.onClick.AddListener(RequestGdpr);
            restorePurchasesButton.onClick.AddListener(RequestRestorePurchases);
            openRateUsButton.onClick.AddListener(RequestOpenRateUs);
            closeButton.onClick.AddListener(RequestClose);
        }

        protected override void OnDisable()
        {
            if (languageDropdown != null) languageDropdown.onValueChanged.RemoveListener(RequestLanguage);
            musicToggle.onValueChanged.RemoveListener(RequestMusicToggle);
            sfxToggle.onValueChanged.RemoveListener(RequestSfxToggle);
            if (hapticsToggle != null) hapticsToggle.onValueChanged.RemoveListener(RequestHapticsToggle);
            gdprButton.onClick.RemoveListener(RequestGdpr);
            restorePurchasesButton.onClick.RemoveListener(RequestRestorePurchases);
            openRateUsButton.onClick.RemoveListener(RequestOpenRateUs);
            closeButton.onClick.RemoveListener(RequestClose);
            base.OnDisable();
        }

        public void Render(SettingsViewState state)
        {
            musicToggle.SetIsOnWithoutNotify(state.MusicVolume > 0f);
            sfxToggle.SetIsOnWithoutNotify(state.SfxVolume > 0f);
            if (hapticsToggle != null)
            {
                hapticsToggle.SetIsOnWithoutNotify(state.HapticsEnabled);
                hapticsToggle.interactable = state.CanSetHaptics;
            }
            gdprButton.gameObject.SetActive(state.CanShowGdprConsent);
            restorePurchasesButton.gameObject.SetActive(state.CanRestorePurchases);
            openRateUsButton.gameObject.SetActive(state.CanRequestReview);
        }

        public void SetPlatformActionsInteractable(bool interactable)
        {
            gdprButton.interactable = interactable;
            restorePurchasesButton.interactable = interactable;
        }

        public void Close() => Hide().Forget();

        private void RequestMusicToggle(bool isOn) => MusicVolumeChanged?.Invoke(isOn ? 1f : 0f);
        private void RequestSfxToggle(bool isOn) => SfxVolumeChanged?.Invoke(isOn ? 1f : 0f);
        private void RequestHapticsToggle(bool isOn) => HapticsEnabledChanged?.Invoke(isOn);
        private void RequestGdpr() => GdprRequested?.Invoke();
        private void RequestRestorePurchases() => RestorePurchasesRequested?.Invoke();
        private void RequestOpenRateUs() => OpenRateUsRequested?.Invoke();
        private void RequestClose() => CloseRequested?.Invoke();
    }
}
