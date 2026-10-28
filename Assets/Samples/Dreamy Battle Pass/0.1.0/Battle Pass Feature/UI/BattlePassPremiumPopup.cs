using System;
using Cysharp.Threading.Tasks;
using Dreamy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassPremiumPopup : UIPanel
    {
        [SerializeField] private Button buyButton,closeButton;
        [SerializeField] private TMP_Text statusText;
        public override bool CanBack=>true;public override bool CanCache=>true;public override UILayer Layer=>UILayer.Popup;
        public event Action BuyRequested;
        private void OnEnable(){buyButton.onClick.AddListener(Buy);closeButton.onClick.AddListener(Close);}
        protected override void OnDisable(){buyButton.onClick.RemoveListener(Buy);closeButton.onClick.RemoveListener(Close);base.OnDisable();}
        private void Buy()=>BuyRequested?.Invoke();private void Close()=>Hide().Forget(Debug.LogException);
        public void Render(string message,bool canBuy) { statusText.text=message;buyButton.interactable=canBuy; }
    }
}
