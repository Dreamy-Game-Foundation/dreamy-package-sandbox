using System;
using System.Threading;
using Dreamy.Economy;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Assets;
using Dreamy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class UIShopPanel : UIPanel
    {
        [Header("Controls")] [SerializeField] private Button closeButton;

        [Header("Holders")] [SerializeField] private Transform gemOfferHolder;
        [SerializeField] private Transform goldOfferHolder;

        private GameObject uiGemOfferPrefab;
        private GameObject uiGoldOfferPrefab;

        public override bool CanBack => true;

        public event Action Destroyed;

        private readonly List<GameObject> spawnedItems = new();

        public override async UniTask Init()
        {
            await base.Init();

            await LoadOffers();

            if (tweenPlayer != null)
            {
                await tweenPlayer.Init();
            }

            var delayControl = this.GetOrAddComponent<TweenDelayControl>();
            if (delayControl != null)
            {
                delayControl.ApplyDelays();
            }
        }


        private void Awake()
        {
            closeButton.onClick.AddListener(OnClose);
        }

        private async UniTask LoadOffers()
        {
            ClearSpawnedItems();

            var dataConfigService = ServiceLocator.Get<IDataConfigService>();
            if (dataConfigService == null) return;

            var offerTable = dataConfigService.GetTable<OfferConfigTable>();
            if (offerTable == null) return;

            var gemOffers = offerTable.GetOffersByType(EResourceOffer.Gem);
            var goldOffers = offerTable.GetOffersByType(EResourceOffer.Gold);

            uiGemOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gem);
            uiGoldOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gold);

            foreach (var offer in gemOffers)
            {
                if (uiGemOfferPrefab != null && gemOfferHolder != null)
                {
                    var itemGo = Instantiate(uiGemOfferPrefab, gemOfferHolder, false);
                    var uiGemOffer = itemGo.GetComponent<UIResourceOfferItem>();
                    if (uiGemOffer != null)
                    {
                        uiGemOffer.Setup(offer);
                    }

                    spawnedItems.Add(itemGo);
                }
                else
                {
                    Debug.Log($"[UIShopPanel] Gem Offer (No Prefab/Holder): {offer.Name} | Price: {offer.Price}");
                }
            }

            foreach (var offer in goldOffers)
            {
                if (uiGoldOfferPrefab != null && goldOfferHolder != null)
                {
                    var itemGo = Instantiate(uiGoldOfferPrefab, goldOfferHolder, false);
                    var uiGoldOffer = itemGo.GetComponent<UIResourceOfferItem>();
                    if (uiGoldOffer != null)
                    {
                        uiGoldOffer.Setup(offer);
                    }

                    spawnedItems.Add(itemGo);
                }
                else
                {
                    Debug.Log($"[UIShopPanel] Gold Offer (No Prefab/Holder): {offer.Name} | Price: {offer.Price}");
                }
            }

            Canvas.ForceUpdateCanvases();
        }


        private void ClearSpawnedItems()
        {
            gemOfferHolder.DestroyChildrenImmediate();
            goldOfferHolder.DestroyChildrenImmediate();
            foreach (var item in spawnedItems)
            {
                if (item != null) DestroyImmediate(item);
            }

            spawnedItems.Clear();
        }

        public async UniTask<bool> FocusResourceAsync(ResourceId resource, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Transform holder = resource == new ResourceId("currency.gem") ? gemOfferHolder
                : resource == new ResourceId("currency.coin") ? goldOfferHolder : null;
            if (holder == null || holder.childCount == 0) return false;
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
            token.ThrowIfCancellationRequested();
            Canvas.ForceUpdateCanvases();
            var scroll = holder.GetComponentInParent<ScrollRect>();
            if (scroll != null && scroll.content != null && scroll.viewport != null)
            {
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, holder);
                float height = scroll.content.rect.height - scroll.viewport.rect.height;
                if (height > 0) scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(-bounds.center.y / height);
            }
            holder.GetComponentInChildren<Selectable>()?.Select();
            return true;
        }

        private void OnClose() => Hide().Forget();

        protected override void OnDestroy()
        {
            Destroyed?.Invoke();
            closeButton.onClick.RemoveListener(OnClose);
            base.OnDestroy();
        }
    }
}