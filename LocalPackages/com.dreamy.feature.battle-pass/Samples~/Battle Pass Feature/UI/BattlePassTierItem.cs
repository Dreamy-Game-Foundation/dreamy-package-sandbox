using System;
using TMPro;
using UnityEngine;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassTierItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text tierLabel;
        [SerializeField] private BattlePassRewardItem freeReward;
        [SerializeField] private BattlePassRewardItem premiumReward;
        public event Action<string> Selected;
        private void OnEnable() { freeReward.Selected+=Select;premiumReward.Selected+=Select; }
        private void OnDisable() { freeReward.Selected-=Select;premiumReward.Selected-=Select; }
        private void Select(string slot)=>Selected?.Invoke(slot);
        public void Bind(BattlePassSlotViewState free,BattlePassSlotViewState premium)
        {
            var state=free ?? premium;tierLabel.text=state.TierId+"\n"+state.RequiredXp+" XP";
            freeReward.gameObject.SetActive(free != null);premiumReward.gameObject.SetActive(premium != null);
            if(free != null)freeReward.Bind(free);if(premium != null)premiumReward.Bind(premium);
        }
    }
}
