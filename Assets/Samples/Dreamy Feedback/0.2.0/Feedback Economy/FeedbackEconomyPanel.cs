using System;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Economy;
using Dreamy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feedback.Samples.Economy
{
    public sealed class FeedbackEconomyPanel : UIPanel
    {
        [SerializeField] private FeedbackHost host;
        [SerializeField] private Transform worldTarget;
        [SerializeField] private RectTransform iconStart;
        [SerializeField] private RectTransform iconEnd;
        [SerializeField] private Sprite coinIcon;
        [SerializeField] private Button grantButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private string rewardAudioId = "ui.click";
        private IResourceWallet wallet;
        private IResourceBalanceSource balances;
        private IAudioService audioService;
        private FeedbackRewardPresenter presenter;
        private string lastTransaction;
        private bool ownsHost = true;
        private static readonly ResourceId Coins = new ResourceId("currency.coin");
        public int RewardFeedbackCount { get; private set; }
        public override bool CanBack => true;

        public override async UniTask Init()
        {
            await base.Init();
            // Root Canvas layout is driven by Unity; normalize after parenting into the UI layer.
            var rect = (RectTransform)transform;
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            // Set this after registration: Unity ignores sorting overrides on a root Canvas.
            GetComponent<Canvas>().overrideSorting = true;
        }

        public void Configure(IResourceWallet wallet, IResourceBalanceSource balances, IAudioService audioService = null, FeedbackHost sharedHost = null)
        {
            Unbind();
            if (sharedHost != null && sharedHost != host)
            {
                host.gameObject.SetActive(false); host = sharedHost; ownsHost = false;
                worldTarget = host.transform.Find("WorldTarget");
            }
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            this.balances = balances ?? throw new ArgumentNullException(nameof(balances)); this.audioService = audioService;
            if (isActiveAndEnabled) Bind();
            Render();
        }
        private void OnEnable()
        {
            host.Initialize();
            grantButton.onClick.AddListener(Grant); retryButton.onClick.AddListener(Retry); closeButton.onClick.AddListener(Close);
            Bind(); Render();
        }
        private void Bind() { if (balances != null && presenter == null) presenter = new FeedbackRewardPresenter(balances, Coins, BalanceChanged); }
        private void Unbind() { presenter?.Dispose(); presenter = null; }
        private void BalanceChanged(long balance, long delta)
        {
            balanceText.text = "Coins: " + balance;
            if (delta <= 0) return;
            RewardFeedbackCount++;
            if (!ownsHost) return; // Bootstrap owns shared-wallet feedback; this panel only renders its balance.
            host.Sequence.Play("reward", FeedbackContext.At(worldTarget.position));
            host.FloatingText.Play("+" + delta, worldTarget.position);
            var icons = IconFlyOptions.Create(coinIcon, iconStart.position, iconEnd.position); icons.Count = 8;
            host.IconFly.Fly(icons);
            if (audioService != null && audioService.IsInitialized && !string.IsNullOrWhiteSpace(rewardAudioId)) audioService.Play(rewardAudioId);
        }
        public void Grant() { if (wallet == null) return; lastTransaction = "feedback-demo/" + Guid.NewGuid().ToString("N"); Apply(); }
        public void Retry() { if (lastTransaction != null) Apply(); }
        private void Apply()
        {
            long before = balances.GetBalance(Coins);
            bool accepted = wallet.TryGrant(new ResourceGrantRequest(lastTransaction, new ResourceAmount(Coins, 100)));
            long after = balances.GetBalance(Coins);
            statusText.text = !accepted ? "Grant failed. Retry available." : after == before ? "Transaction already applied. No reward or feedback replay." : "100 coins committed. Feedback is presentation only.";
            Render();
        }
        private void Render()
        {
            grantButton.interactable = wallet != null; retryButton.interactable = wallet != null && lastTransaction != null;
            balanceText.text = balances == null ? "Waiting for shared wallet" : "Coins: " + balances.GetBalance(Coins);
        }
        private void Close()
        {
            if (PanelManager.HasInstance) Hide().Forget(); else gameObject.SetActive(false);
        }
        protected override void OnDisable()
        {
            grantButton.onClick.RemoveListener(Grant); retryButton.onClick.RemoveListener(Retry); closeButton.onClick.RemoveListener(Close);
            Unbind(); if (ownsHost) host.StopAll(); base.OnDisable();
        }
        protected override void OnDestroy() { Unbind(); base.OnDestroy(); }
    }
}
