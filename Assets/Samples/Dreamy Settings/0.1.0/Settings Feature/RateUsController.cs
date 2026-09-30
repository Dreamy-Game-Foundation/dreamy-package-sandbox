using System;
using Dreamy.Core;
using Dreamy.Economy;
using Dreamy.Settings;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    public sealed class RateUsController : MonoBehaviour
    {
        [SerializeField] private RateUsPanel panel;
        [SerializeField] private string rewardResourceId = "currency.gem";
        [SerializeField, Min(1)] private long rewardAmount = 10;

        private RateUsPresenter presenter;

        private void Awake()
        {
            presenter = new RateUsPresenter(ServiceLocator.Get<ISettingsService>(), panel);
            presenter.PositiveRatingSubmitted += GrantReward;
        }

        private void OnEnable() => presenter.Show();

        private void OnDestroy()
        {
            if (presenter != null)
            {
                presenter.PositiveRatingSubmitted -= GrantReward;
                presenter.Dispose();
            }
        }

        private void GrantReward(int rating)
        {
            if (!ServiceLocator.TryGet<IResourceWallet>(out IResourceWallet wallet) ||
                !ResourceId.TryParse(rewardResourceId, out ResourceId resourceId))
            {
                return;
            }

            wallet.TryGrant(new ResourceGrantRequest(
                $"rate-us:{resourceId.Value}:{rating}:{Guid.NewGuid():N}",
                new ResourceAmount(resourceId, rewardAmount)));
        }
    }
}
