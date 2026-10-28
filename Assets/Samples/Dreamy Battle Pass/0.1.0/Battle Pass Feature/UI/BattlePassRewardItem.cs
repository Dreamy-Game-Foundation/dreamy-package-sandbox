using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassRewardItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;
        private string slot;
        public event Action<string> Selected;
        private void OnEnable()=>button.onClick.AddListener(Select);
        private void OnDisable()=>button.onClick.RemoveListener(Select);
        private void Select()=>Selected?.Invoke(slot);
        public void Bind(BattlePassSlotViewState state)
        {
            slot=state.Id;label.text=(state.Premium ? "PREMIUM" : "FREE")+"\n"+
                string.Join(" + ",state.Bundle.Entries.Select(e=>e.Amount+" "+e.TargetId.Split('.').Last()))+"\n"+
                (state.Status == BattlePassStatus.Success ? "CLAIM" : state.Status == BattlePassStatus.AlreadyApplied ? "RECEIVED" : state.Status.ToString().ToUpperInvariant());
            button.interactable=state.Status == BattlePassStatus.Success || state.Status == BattlePassStatus.Pending;
        }
    }
}
