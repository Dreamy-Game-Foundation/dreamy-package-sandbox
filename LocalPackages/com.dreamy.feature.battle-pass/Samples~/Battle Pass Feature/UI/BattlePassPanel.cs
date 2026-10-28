using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dreamy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassPanel : UIPanel,IBattlePassView
    {
        [SerializeField] private TMP_Text titleText,xpText,timeText,statusText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button closeButton,claimAllButton,premiumButton,recoverButton;
        [SerializeField] private Transform content;
        [SerializeField] private BattlePassTierItem tierPrefab;
        private readonly List<BattlePassTierItem> rows=new();
        public override bool CanBack=>true;
        public override bool CanCache=>true;
        public override UILayer Layer=>UILayer.Popup;
        public event Action<string> ClaimRequested;
        public event Action ClaimAllRequested;
        public event Action RecoverRequested;
        public event Action PremiumRequested;
        public int RowCount=>rows.Count;
        private void OnEnable()
        { closeButton.onClick.AddListener(Close);claimAllButton.onClick.AddListener(All);premiumButton.onClick.AddListener(Premium);recoverButton.onClick.AddListener(Recover); }
        protected override void OnDisable()
        { closeButton.onClick.RemoveListener(Close);claimAllButton.onClick.RemoveListener(All);premiumButton.onClick.RemoveListener(Premium);recoverButton.onClick.RemoveListener(Recover);base.OnDisable(); }
        private void Close()=>Hide().Forget(Debug.LogException);
        private void All()=>ClaimAllRequested?.Invoke();private void Premium()=>PremiumRequested?.Invoke();private void Recover()=>RecoverRequested?.Invoke();
        private void Select(string slot)=>ClaimRequested?.Invoke(slot);
        public void Render(BattlePassViewState state)
        {
            titleText.text=state.Title ?? "BATTLE PASS";xpText.text=state.Xp+" / "+state.MaxXp+" XP";
            float progress=state.MaxXp > 0 ? (float)((double)state.Xp/state.MaxXp) : 0;
            progressFill.rectTransform.anchorMax=new Vector2(progress,1);progressFill.rectTransform.offsetMin=Vector2.zero;progressFill.rectTransform.offsetMax=Vector2.zero;
            long remaining=Math.Max(0,(state.Phase == BattlePassPhase.Active ? state.EndUtcTicks : state.ClaimDeadlineUtcTicks)-state.NowUtcTicks);
            timeText.text=state.Phase+"  "+TimeSpan.FromTicks(remaining).ToString(@"dd\d\ hh\h\ mm\m")+(state.IsPremium ? "   PREMIUM ACTIVE" : "   FREE TRACK");
            statusText.text=state.RequiresReload ? "SAVE UNCERTAIN: reload all durable owners." : state.HasPending ? "Pending rewards: press Recover." : state.Phase == BattlePassPhase.Grace ? "Season XP ended. Claim unlocked rewards before grace ends." : state.Phase == BattlePassPhase.Expired ? "Season closed. Pending rewards remain recoverable." : "Play puzzle runs to unlock rewards.";
            var tiers=state.Slots.GroupBy(x=>x.TierId).ToList();
            while(rows.Count < tiers.Count) { var row=Instantiate(tierPrefab,content);row.Selected+=Select;rows.Add(row); }
            for(int i=0;i<rows.Count;i++) { rows[i].gameObject.SetActive(i<tiers.Count);if(i<tiers.Count)rows[i].Bind(tiers[i].FirstOrDefault(x=>!x.Premium),tiers[i].FirstOrDefault(x=>x.Premium)); }
            claimAllButton.interactable=!state.RequiresReload && (state.HasPending || state.ClaimableCount>0);
            recoverButton.interactable=!state.RequiresReload && state.HasPending;
            premiumButton.interactable=!state.RequiresReload && !state.IsPremium && state.Phase == BattlePassPhase.Active;
        }
        public void ShowResult(BattlePassResult result)=>statusText.text=result.Status+(result.Reason == null ? "" : ": "+result.Reason);
        protected override void OnDestroy() { foreach(var row in rows)if(row != null)row.Selected-=Select;base.OnDestroy(); }
    }
}
