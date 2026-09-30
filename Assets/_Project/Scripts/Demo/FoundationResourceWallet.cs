using System;
using System.Collections.Generic;
using Dreamy.Economy;

namespace Dreamy.Template.Demo
{
    public sealed class FoundationResourceWallet : IResourceWallet, IResourceBalanceSource
    {
        private readonly Dictionary<ResourceId, long> balances = new();
        private readonly HashSet<string> transactionIds = new();

        public event Action<ResourceBalanceChanged> BalanceChanged;

        public FoundationResourceWallet(long initialCoins)
        {
            balances[new ResourceId("currency.coin")] = initialCoins;
        }

        public bool TryGrant(ResourceGrantRequest request)
        {
            if (!transactionIds.Add(request.TransactionId))
            {
                return true;
            }

            balances[request.Resource.ResourceId] =
                GetBalance(request.Resource.ResourceId) + request.Resource.Amount;
            NotifyBalanceChanged(request.Resource.ResourceId);
            return true;
        }

        public bool TryExchange(ResourceExchangeRequest request)
        {
            if (transactionIds.Contains(request.TransactionId))
            {
                return true;
            }

            if (GetBalance(request.Cost.ResourceId) < request.Cost.Amount)
            {
                return false;
            }

            balances[request.Cost.ResourceId] =
                GetBalance(request.Cost.ResourceId) - request.Cost.Amount;
            NotifyBalanceChanged(request.Cost.ResourceId);
            foreach (ResourceAmount reward in request.Rewards)
            {
                balances[reward.ResourceId] =
                    GetBalance(reward.ResourceId) + reward.Amount;
                NotifyBalanceChanged(reward.ResourceId);
            }

            transactionIds.Add(request.TransactionId);
            return true;
        }

        public long GetBalance(ResourceId resourceId) =>
            balances.TryGetValue(resourceId, out long value) ? value : 0;

        private void NotifyBalanceChanged(ResourceId resourceId) =>
            BalanceChanged?.Invoke(new ResourceBalanceChanged(resourceId, GetBalance(resourceId)));
    }
}
