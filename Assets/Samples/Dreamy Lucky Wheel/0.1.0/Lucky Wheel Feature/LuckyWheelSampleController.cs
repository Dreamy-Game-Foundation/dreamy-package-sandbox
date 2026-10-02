using System;
using System.Collections.Generic;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.LuckyWheel;
using Newtonsoft.Json;
using UnityEngine;

namespace Dreamy.Feature.LuckyWheel.Integration
{
    public sealed class LuckyWheelSampleController : MonoBehaviour
    {
        [SerializeField] private LuckyWheelPanel panel;
        [SerializeField] private string configResourcePath = "DataConfig/luckyWheel";
        [SerializeField] private int randomSeed = 20261002;

        private LuckyWheelPresenter presenter;

        private void Start()
        {
            TextAsset configJson = Resources.Load<TextAsset>(configResourcePath);
            if (configJson == null)
            {
                throw new InvalidOperationException(
                    $"Lucky Wheel config was not found at Resources/{configResourcePath}.json.");
            }

            LuckyWheelConfig config = JsonConvert.DeserializeObject<LuckyWheelConfig>(configJson.text);
            config.Initialize(configResourcePath);
            var service = new LuckyWheelModel(
                config,
                new InMemoryDatasaveService(),
                new InMemoryResourceWallet(),
                new SystemLuckyWheelClock(),
                new SystemLuckyWheelRandom(randomSeed),
                "lucky-wheel-sample");
            presenter = new LuckyWheelPresenter(service, panel);
            presenter.Show();
        }

        private void OnDestroy() => presenter?.Dispose();

        private sealed class InMemoryResourceWallet : IResourceWallet
        {
            private readonly HashSet<string> transactionIds = new(StringComparer.Ordinal);
            private readonly Dictionary<ResourceId, long> balances = new();

            public bool TryGrant(ResourceGrantRequest request)
            {
                if (!transactionIds.Add(request.TransactionId))
                {
                    return true;
                }

                balances.TryGetValue(request.Resource.ResourceId, out long balance);
                balances[request.Resource.ResourceId] = balance + request.Resource.Amount;
                return true;
            }

            public bool TryExchange(ResourceExchangeRequest request) => false;
        }

        private sealed class InMemoryDatasaveService : IDatasaveService
        {
            private readonly Dictionary<string, SaveData> data = new();

            public T Load<T>(string key = null) where T : SaveData, new()
            {
                if (data.TryGetValue(key, out SaveData saved))
                {
                    return (T)saved;
                }

                T created = new();
                data[key] = created;
                return created;
            }

            public void Save<T>(T saved, string key = null) where T : SaveData => data[key] = saved;
            public void SaveAll() { }
            public bool Exists(string key) => data.ContainsKey(key);
            public void Delete(string key) => data.Remove(key);
            public void DeleteAll() => data.Clear();
        }
    }
}
