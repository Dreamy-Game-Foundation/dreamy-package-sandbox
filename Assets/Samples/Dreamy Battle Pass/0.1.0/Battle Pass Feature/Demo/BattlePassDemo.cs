using System;
using Cysharp.Threading.Tasks;
using Dreamy.BattlePass.Presentation;
using Dreamy.BattlePass.Sample.Rewards;
using Dreamy.Economy;
using Dreamy.Economy.UI;
using Dreamy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassDemo : MonoBehaviour
    {
        [SerializeField] private BattlePassPanel panel;
        [SerializeField] private BattlePassPremiumPopup premiumPopup;
        [SerializeField] private RewardPanel rewardPanel;
        [SerializeField] private BattlePassEntryView entry;
        [SerializeField] private Button enrollButton,winButton,replayButton,advanceButton,reloadButton;
        [SerializeField] private Toggle confirmToggle;
        [SerializeField] private TMP_Text balanceText,statusText;
        private BattlePassFeatureInstaller root;private RewardPresentationQueue queue;private BattlePassClaimSource source;
        private bool ready;private float nextRefresh;
        public BattlePassFeatureInstaller Root=>root;
        public BattlePassPanel Panel=>panel;
        public RewardPanel RewardPanel=>rewardPanel;
        private void Start()=>Initialize().Forget(Debug.LogException);
        private async UniTask Initialize()
        {
            ready=false;root=new BattlePassFeatureInstaller();await root.Initialize(this.GetCancellationTokenOnDestroy());if(this == null)return;
            source=new BattlePassClaimSource("battle-pass-demo",root.Model);queue=RewardPresentationInstaller.Install("battle-pass-demo",rewardPanel);queue.Diagnostic+=Debug.LogException;
            var factory=new PanelPresenterFactory();factory.Register<BattlePassPanel>(view=>new BattlePassPanelPresenter(new BattlePassPresenter(root.Model,view,OpenPremium,Claim)));PanelManager.Instance.PresenterFactory=factory;
            root.Model.StateChanged+=Refresh;root.Wallet.BalanceChanged+=BalanceChanged;
            enrollButton.onClick.AddListener(Enroll);winButton.onClick.AddListener(Win);replayButton.onClick.AddListener(Replay);advanceButton.onClick.AddListener(Advance);reloadButton.onClick.AddListener(Reload);
            premiumPopup.BuyRequested+=Buy;entry.OpenRequested+=Open;ready=true;Refresh();statusText.text="Enroll explicitly, play puzzle runs, then open the pass.";
        }
        private void Update(){if(ready && Time.unscaledTime >= nextRefresh){nextRefresh=Time.unscaledTime+1;if(panel.isActiveAndEnabled)panel.Render(root.Model.GetState());Refresh();}}
        private void OnApplicationPause(bool paused){if(paused && ready)root.Model.Refresh();}
        private void BalanceChanged(ResourceBalanceChanged _) => Refresh();
        private void Refresh()
        {
            if(!ready)return;var state=root.Model.GetState();entry.Render(state);
            balanceText.text="COINS "+root.Wallet.GetBalance(new ResourceId("currency.coin"))+"   GEMS "+root.Wallet.GetBalance(new ResourceId("currency.gem"))+"   HAMMERS "+root.Wallet.GetBalance(new ResourceId("item.booster.hammer"));
            enrollButton.interactable=state.Phase == BattlePassPhase.NotEnrolled;winButton.interactable=state.Phase == BattlePassPhase.Active && !state.RequiresReload;
        }
        private void Enroll(){statusText.text=root.Model.Enroll(BattlePassFeatureInstaller.Occurrence).Status.ToString();Refresh();}
        private void Win()
        { var state=root.Model.GetState();if(state.OccurrenceId == null)return;root.Outbox.Record(state.OccurrenceId,Guid.NewGuid().ToString("N"),20);statusText.text=root.Outbox.Drain(root.Model).Status.ToString(); }
        private void Replay()
        { if(root.Outbox.LatestRunId == null)return;statusText.text=root.Model.ReportProgress(new(root.Model.GetState().OccurrenceId,root.Outbox.LatestRunId,20)).Status.ToString(); }
        private void Advance(){root.Clock.AdvanceDay();root.Model.Refresh();}
        private void Open()=>panel.Show().Forget(Debug.LogException);
        private void OpenPremium(){premiumPopup.Render("Unlock every Premium tier for this occurrence.\nCost: 200 gems. Previous unlocked tiers remain claimable.",true);premiumPopup.Show().Forget(Debug.LogException);}
        private void Buy(){premiumPopup.Render(root.BuyPremium(),!root.Model.GetState().IsPremium);Refresh();}
        private void Claim(string occurrence,string slot)=>ClaimAsync(occurrence,slot).Forget(Debug.LogException);
        private async UniTask ClaimAsync(string occurrence,string slot)
        {
            var reference=source.Reference(occurrence,slot);
            if(confirmToggle.isOn)await queue.OpenAsync(RewardPresentationRequest.Confirm(reference),source,this.GetCancellationTokenOnDestroy());
            else await RewardClaimFlow.ClaimAsync(reference,source,uiCancellationToken:this.GetCancellationTokenOnDestroy());
            if(this != null){panel.Render(root.Model.GetState());Refresh();}
        }
        private void Reload()=>ReloadAsync().Forget(Debug.LogException);
        private async UniTask ReloadAsync(){if(!ready)return;ready=false;await panel.Hide();await premiumPopup.Hide();Unbind();await Initialize();}
        private void Unbind()
        {
            queue?.Dispose();if(root?.Model != null)root.Model.StateChanged-=Refresh;if(root?.Wallet != null)root.Wallet.BalanceChanged-=BalanceChanged;
            enrollButton.onClick.RemoveListener(Enroll);winButton.onClick.RemoveListener(Win);replayButton.onClick.RemoveListener(Replay);advanceButton.onClick.RemoveListener(Advance);reloadButton.onClick.RemoveListener(Reload);
            premiumPopup.BuyRequested-=Buy;entry.OpenRequested-=Open;
        }
        private void OnDestroy(){ready=false;Unbind();if(PanelManager.HasInstance)PanelManager.Instance.PresenterFactory=null;}
    }
}
