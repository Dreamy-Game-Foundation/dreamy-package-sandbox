using System;
using Dreamy.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feature.Shop.Integration
{
    public sealed class ShopOfferItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Button purchaseButton;
        private string offerId;

        public event Action<string> PurchaseRequested;

        private void OnEnable() => purchaseButton.onClick.AddListener(RequestPurchase);
        private void OnDisable() => purchaseButton.onClick.RemoveListener(RequestPurchase);

        public void Render(ShopOfferViewState state)
        {
            ShopOfferConfig offer = state.Offer;
            offerId = offer.Id;
            titleText.text = offer.TitleKey;
            costText.text = $"{offer.Cost.Amount} {offer.Cost.ResourceId}";
            rewardText.text = $"{offer.Rewards[0].Resource.Amount} {offer.Rewards[0].Resource.ResourceId}";
        }

        private void RequestPurchase() => PurchaseRequested?.Invoke(offerId);
    }
}
