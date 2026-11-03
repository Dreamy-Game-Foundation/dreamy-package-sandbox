using System;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Localization;
using Dreamy.Localization.SettingsIntegration;
using Dreamy.Localization.UI;
using Dreamy.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.Template.Localization
{
    [DefaultExecutionOrder(-11000)]
    public sealed class LocalizationTestRoot : MonoBehaviour
    {
        [SerializeField] private DreamyLocalizationProfile profile;
        [SerializeField] private DreamyAudioProfile audioProfile;
        [SerializeField] private GameObject settingsPrefab;
        [SerializeField] private Transform settingsParent;
        [SerializeField] private GameObject content;
        [SerializeField] private Button openSettings;
        [SerializeField] private Button incrementAmount;
        [SerializeField] private Button resetPreference;
        [SerializeField] private LocalizedTmpText reward;
        [SerializeField] private TMP_Text localeLabel;
        private LocalizationService service;
        private LocalizationSettingsLanguageGateway gateway;
        private StandaloneSettingsView settingsView;
        private SettingsPresenter settingsPresenter;
        private SettingsModel settings;
        private int amount = 42;
        public bool IsReady { get; private set; }
        public ILocalizationService Service => service;
        public SettingsModel Settings => settings;
        public StandaloneSettingsView SettingsView => settingsView;
        private void Awake() { content.SetActive(false); }
        private void Start() => Initialize().Forget();
        private async UniTaskVoid Initialize()
        {
            content.SetActive(false);
            try
            {
                service = new LocalizationService(profile);
                await service.InitializeAsync(this.GetCancellationTokenOnDestroy());
                gateway = new LocalizationSettingsLanguageGateway(service);
                if (GameObject.Find("DreamyAudio") != null) throw new InvalidOperationException("Run the localization test scene independently.");
                var audio = new AudioService(); audio.Initialize(audioProfile);
                var audioHost = GameObject.Find("DreamyAudio");
                if (audioHost != null) audioHost.transform.SetParent(transform);
                settings = new SettingsModel(audio, languageGateway: gateway);
                service.LocaleChanged += UpdateLocale;
                openSettings.onClick.AddListener(OpenSettings);
                incrementAmount.onClick.AddListener(Increment);
                resetPreference.onClick.AddListener(ResetPreference);
                reward.SetArguments(new { amount });
                UpdateLocale(service.CurrentLocaleCode);
                content.SetActive(true); IsReady = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e, this); }
        }
        public void OpenSettings()
        {
            if (!IsReady) return;
            if (settingsView == null)
            {
                var instance = Instantiate(settingsPrefab, settingsParent);
                settingsView = instance.GetComponent<StandaloneSettingsView>();
                if (settingsView == null) throw new InvalidOperationException("The test prefab must contain StandaloneSettingsView.");
                settingsPresenter = new SettingsPresenter(settings, settingsView);
                settingsView.Disabled += ReleaseSettingsPresenter;
            }
            settingsView.gameObject.SetActive(true);
            settingsPresenter.Show();
        }
        private void ReleaseSettingsPresenter() => settingsPresenter?.Dispose();
        private void Increment() { amount++; reward.SetArguments(new { amount }); }
        private void ResetPreference() { new PlayerPrefsLocalePreferenceStore(profile.PreferenceKey).Clear(); }
        private void UpdateLocale(string code) { if (localeLabel != null) localeLabel.text = code; }
        private void OnDestroy()
        {
            IsReady = false;
            openSettings.onClick.RemoveListener(OpenSettings);
            incrementAmount.onClick.RemoveListener(Increment);
            resetPreference.onClick.RemoveListener(ResetPreference);
            ReleaseSettingsPresenter();
            if (settingsView != null)
            {
                settingsView.Disabled -= ReleaseSettingsPresenter;
                Destroy(settingsView.gameObject);
            }
            gateway?.Dispose(); service?.Dispose();
        }
    }
}
