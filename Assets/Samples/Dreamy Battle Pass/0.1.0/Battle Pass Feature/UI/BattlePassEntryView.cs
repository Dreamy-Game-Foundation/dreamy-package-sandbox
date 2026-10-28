using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassEntryView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text badge;
        public event Action OpenRequested;
        private void OnEnable()=>button.onClick.AddListener(Open);
        private void OnDisable()=>button.onClick.RemoveListener(Open);
        private void Open()=>OpenRequested?.Invoke();
        public void Render(BattlePassViewState state)=>badge.text=state.HasPending ? "RECOVER" : state.ClaimableCount+" READY";
    }
}
