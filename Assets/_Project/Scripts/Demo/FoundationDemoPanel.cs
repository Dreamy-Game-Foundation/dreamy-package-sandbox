using System;
using Dreamy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class FoundationDemoPanel : UIPanel
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text healthText;

        [Header("Buttons")]
        [SerializeField] private Button addScoreButton;
        [SerializeField] private Button damageButton;
        [SerializeField] private Button healButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button openShopButton;
        [SerializeField] private Button openDailyRewardButton;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button openRateUsButton;

        [SerializeField] private Button openMissionsButton;
        [SerializeField] private Button openProgressionButton;
        [SerializeField] private Button openLuckyWheelButton;
        [SerializeField] private Button openTutorialButton;
        [SerializeField] private Button openEconomyButton;
        [SerializeField] private Button openFeedbackButton;

        public override bool CanBack => true;
        public Button TutorialAddScoreButton => addScoreButton;
        public Button TutorialOpenShopButton => openShopButton;

        public event Action AddScoreRequested;
        public event Action DamageRequested;
        public event Action HealRequested;
        public event Action SaveRequested;
        public event Action LoadRequested;
        public event Action OpenShopRequested;
        public event Action OpenDailyRewardRequested;
        public event Action OpenSettingsRequested;
        public event Action OpenRateUsRequested;
        public event Action OpenMissionsRequested;
        public event Action OpenProgressionRequested;
        public event Action OpenLuckyWheelRequested;
        public event Action OpenTutorialRequested;
        public event Action OpenEconomyRequested;
        public event Action OpenFeedbackRequested;
        public event Action Destroyed;

        private void OnEnable()
        {
            if (openFeedbackButton != null) openFeedbackButton.onClick.AddListener(OnOpenFeedback);
            if (openMissionsButton != null) openMissionsButton.onClick.AddListener(OnOpenMissions);
            if (openProgressionButton != null) openProgressionButton.onClick.AddListener(OnOpenProgression);
            if (openLuckyWheelButton != null) openLuckyWheelButton.onClick.AddListener(OnOpenLuckyWheel);
            if (openTutorialButton != null) openTutorialButton.onClick.AddListener(OnOpenTutorial);
            if (openEconomyButton != null) openEconomyButton.onClick.AddListener(OnOpenEconomy);
            addScoreButton.onClick.AddListener(OnAddScore);
            damageButton.onClick.AddListener(OnDamage);
            healButton.onClick.AddListener(OnHeal);
            saveButton.onClick.AddListener(OnSave);
            loadButton.onClick.AddListener(OnLoad);
            if (openShopButton != null) openShopButton.onClick.AddListener(OnOpenShop);
            if (openDailyRewardButton != null) openDailyRewardButton.onClick.AddListener(OnOpenDailyReward);
            if (openSettingsButton != null) openSettingsButton.onClick.AddListener(OnOpenSettings);
            if (openRateUsButton != null) openRateUsButton.onClick.AddListener(OnOpenRateUs);
        }

        protected override void OnDisable()
        {
            if (openFeedbackButton != null) openFeedbackButton.onClick.RemoveListener(OnOpenFeedback);
            if (openMissionsButton != null) openMissionsButton.onClick.RemoveListener(OnOpenMissions);
            if (openProgressionButton != null) openProgressionButton.onClick.RemoveListener(OnOpenProgression);
            if (openLuckyWheelButton != null) openLuckyWheelButton.onClick.RemoveListener(OnOpenLuckyWheel);
            if (openTutorialButton != null) openTutorialButton.onClick.RemoveListener(OnOpenTutorial);
            if (openEconomyButton != null) openEconomyButton.onClick.RemoveListener(OnOpenEconomy);
            addScoreButton.onClick.RemoveListener(OnAddScore);
            damageButton.onClick.RemoveListener(OnDamage);
            healButton.onClick.RemoveListener(OnHeal);
            saveButton.onClick.RemoveListener(OnSave);
            loadButton.onClick.RemoveListener(OnLoad);
            if (openShopButton != null) openShopButton.onClick.RemoveListener(OnOpenShop);
            if (openDailyRewardButton != null) openDailyRewardButton.onClick.RemoveListener(OnOpenDailyReward);
            if (openSettingsButton != null) openSettingsButton.onClick.RemoveListener(OnOpenSettings);
            if (openRateUsButton != null) openRateUsButton.onClick.RemoveListener(OnOpenRateUs);
            base.OnDisable();
        }

        public void SetStatus(string value) => statusText.text = value;
        public void SetScore(int value) => scoreText.text = $"Score: {value}";
        public void SetHealth(float value) => healthText.text = $"Health: {value:0}";

        private void OnAddScore() => AddScoreRequested?.Invoke();
        private void OnDamage() => DamageRequested?.Invoke();
        private void OnHeal() => HealRequested?.Invoke();
        private void OnSave() => SaveRequested?.Invoke();
        private void OnLoad() => LoadRequested?.Invoke();
        private void OnOpenShop() => OpenShopRequested?.Invoke();
        private void OnOpenDailyReward() => OpenDailyRewardRequested?.Invoke();
        private void OnOpenSettings() => OpenSettingsRequested?.Invoke();
        private void OnOpenRateUs() => OpenRateUsRequested?.Invoke();

        private void OnOpenMissions() => OpenMissionsRequested?.Invoke();

        private void OnOpenProgression() => OpenProgressionRequested?.Invoke();

        private void OnOpenLuckyWheel() => OpenLuckyWheelRequested?.Invoke();

        private void OnOpenTutorial() => OpenTutorialRequested?.Invoke();

        private void OnOpenFeedback() => OpenFeedbackRequested?.Invoke();

        private void OnOpenEconomy() => OpenEconomyRequested?.Invoke();

        protected override void OnDestroy()
        {
            Destroyed?.Invoke();
            base.OnDestroy();
        }
    }
}
