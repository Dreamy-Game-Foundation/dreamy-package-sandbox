using System;
using System.Text;
using Dreamy.Core;
using Dreamy.Economy;
using Dreamy.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class FoundationProgressionPanelControls : MonoBehaviour
    {
        [SerializeField] private TMP_Text rewardStatus;
        [SerializeField] private TMP_Text actionStatus;
        [SerializeField] private Button grant300, retryGrant, retryRewards, failCard, failAck, failComplete, recover;
        private FoundationProgressionRewards demo;
        private bool bound;
        public void Configure(FoundationProgressionRewards context)
        {
            Unbind(); demo = context ?? throw new ArgumentNullException(nameof(context));
            if (isActiveAndEnabled) Bind();
            Render();
        }
        private void OnEnable() => Bind();
        private void Bind()
        {
            if (bound || demo == null) return;
            bound = true;
            demo.Model.RewardsChanged += Render;
            grant300.onClick.AddListener(Grant);
            retryGrant.onClick.AddListener(RetryGrant);
            retryRewards.onClick.AddListener(RetryRewards);
            failCard.onClick.AddListener(FailCard);
            failAck.onClick.AddListener(FailAck);
            failComplete.onClick.AddListener(FailComplete);
            recover.onClick.AddListener(Recover);
        }
        private void Unbind()
        {
            if (!bound) return;
            demo.Model.RewardsChanged -= Render;
            grant300.onClick.RemoveListener(Grant); retryGrant.onClick.RemoveListener(RetryGrant);
            retryRewards.onClick.RemoveListener(RetryRewards); failCard.onClick.RemoveListener(FailCard);
            failAck.onClick.RemoveListener(FailAck); failComplete.onClick.RemoveListener(FailComplete);
            recover.onClick.RemoveListener(Recover);
            bound = false;
        }
        private void Grant() => Execute(() => demo.Grant(300).Status.ToString());
        private void RetryGrant() => Execute(() => demo.RetryLastGrant().Status.ToString());
        private void RetryRewards() => Execute(() => { var results=demo.Model.RetryPendingRewards(); return $"Retried {results.Count} pending milestones"; });
        private void FailCard() => Execute(() => { demo.FailCard(); return "Next Card delivery will fail once"; });
        private void FailAck() => Execute(() => { demo.FailAcknowledgement(); return "Next acknowledgement save will fail once"; });
        private void FailComplete() => Execute(() => { demo.FailCompletion(); return "Next Completed save will fail once"; });
        private void Recover() => Execute(() => { demo.Recover(); return "Reloaded save and recovered pending rewards"; });
        private void Execute(Func<string> action)
        {
            try { actionStatus.text = action(); }
            catch (Exception exception) { actionStatus.text = exception.Message; Debug.LogException(exception, this); }
            Render();
        }
        private void Render()
        {
            if (demo == null || rewardStatus == null) return;
            var text = new StringBuilder();
            if (ServiceLocator.TryGet<IResourceBalanceProvider>(out var wallet))
                text.AppendLine($"Coin {wallet.GetBalance(new ResourceId("currency.coin"))}  |  Gem {wallet.GetBalance(new ResourceId("currency.gem"))}");
            text.AppendLine($"Card {demo.CardCount}  |  Chest {demo.ChestCount}  |  Cosmetic {demo.CosmeticCount}");
            text.AppendLine(demo.Model.RequiresReload ? "SAVE UNCERTAIN: use Reload + Recover" : "Reward delivery / per-entry status");
            foreach (var row in demo.Model.GetRewardStates())
            {
                text.AppendLine($"Level {row.Level}: {row.Status} ({row.Reason})");
                foreach (var entry in row.Entries)
                    text.AppendLine($"  {entry.Reward.TargetId} x{entry.Reward.Amount}: {(entry.Delivered ? "Delivered" : "Pending")}");
            }
            rewardStatus.text = text.ToString();
            grant300.interactable = !demo.Model.RequiresReload && !demo.Model.GetState().IsMaxLevel;
            retryGrant.interactable = !demo.Model.RequiresReload && !string.IsNullOrEmpty(demo.LastGrantId);
            retryRewards.interactable = !demo.Model.RequiresReload;
        }
        private void OnDisable() => Unbind();
        private void OnDestroy() => Unbind();
    }
}
