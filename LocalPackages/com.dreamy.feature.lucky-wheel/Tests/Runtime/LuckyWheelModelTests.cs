using System;
using System.Collections.Generic;
using System.Reflection;
using Dreamy.Datasave;
using Dreamy.Economy;
using NUnit.Framework;

namespace Dreamy.LuckyWheel.Tests
{
    public sealed class LuckyWheelModelTests
    {
        [Test]
        public void Config_WithSixValidRewards_CalculatesTotalWeight()
        {
            LuckyWheelConfig config = CreateConfig();

            Assert.That(config.Segments, Has.Count.EqualTo(6));
            Assert.That(config.TotalWeight, Is.EqualTo(6));
        }

        [Test]
        public void TrySpin_SelectsConfiguredSegmentAndGrantsBeforeReveal()
        {
            var wallet = new FakeWallet();
            LuckyWheelModel model = CreateModel(new FakeRandom(5), wallet, out _, cooldownSeconds: 60);

            LuckyWheelSpinResult result = model.TrySpin();

            Assert.That(result.Status, Is.EqualTo(LuckyWheelSpinStatus.Succeeded));
            Assert.That(result.SegmentIndex, Is.EqualTo(5));
            Assert.That(result.SegmentId, Is.EqualTo("reward-6"));
            Assert.That(wallet.Requests, Has.Count.EqualTo(1));
            Assert.That(wallet.Requests[0].TransactionId, Is.EqualTo("lucky-wheel:test-wheel:0"));
            Assert.That(model.GetState().Availability, Is.EqualTo(LuckyWheelAvailability.AwaitingReveal));
        }

        [Test]
        public void RetryPendingGrant_ReusesTransactionAndDoesNotReroll()
        {
            var wallet = new FakeWallet { ShouldSucceed = false };
            var random = new FakeRandom(2);
            LuckyWheelModel model = CreateModel(random, wallet, out _);
            LuckyWheelSpinResult first = model.TrySpin();
            wallet.ShouldSucceed = true;

            LuckyWheelSpinResult retry = model.RetryPendingGrant();

            Assert.That(first.Status, Is.EqualTo(LuckyWheelSpinStatus.GrantFailed));
            Assert.That(retry.Status, Is.EqualTo(LuckyWheelSpinStatus.Succeeded));
            Assert.That(retry.SegmentIndex, Is.EqualTo(2));
            Assert.That(random.CallCount, Is.EqualTo(1));
            Assert.That(wallet.Requests[0].TransactionId, Is.EqualTo(wallet.Requests[1].TransactionId));
        }

        [Test]
        public void Restart_WithGrantedPendingSpin_RevealsSameOutcomeWithoutGrantingAgain()
        {
            var wallet = new FakeWallet();
            LuckyWheelModel firstModel = CreateModel(new FakeRandom(3), wallet, out InMemoryDatasave datasave);
            LuckyWheelSpinResult firstResult = firstModel.TrySpin();

            LuckyWheelModel restored = CreateModel(
                CreateConfig(),
                datasave,
                wallet,
                new FakeClock(new DateTime(2026, 10, 2, 1, 0, 0, DateTimeKind.Utc)),
                new FakeRandom(0));
            LuckyWheelViewState restoredState = restored.GetState();

            Assert.That(restoredState.Availability, Is.EqualTo(LuckyWheelAvailability.AwaitingReveal));
            Assert.That(restoredState.PendingResult.Value.TransactionId, Is.EqualTo(firstResult.TransactionId));
            Assert.That(restoredState.PendingResult.Value.SegmentIndex, Is.EqualTo(3));
            Assert.That(wallet.Requests, Has.Count.EqualTo(1));
        }

        [Test]
        public void AcknowledgeReveal_ClearsPendingAndStartsCooldown()
        {
            var wallet = new FakeWallet();
            LuckyWheelModel model = CreateModel(new FakeRandom(0), wallet, out _, cooldownSeconds: 60);
            LuckyWheelSpinResult result = model.TrySpin();

            bool acknowledged = model.AcknowledgeReveal(result.TransactionId);

            Assert.That(acknowledged, Is.True);
            Assert.That(model.GetState().Availability, Is.EqualTo(LuckyWheelAvailability.Cooldown));
            Assert.That(model.TrySpin().Status, Is.EqualTo(LuckyWheelSpinStatus.Cooldown));
            Assert.That(wallet.Requests, Has.Count.EqualTo(1));
        }

        private static LuckyWheelModel CreateModel(
            FakeRandom random,
            FakeWallet wallet,
            out InMemoryDatasave datasave,
            int cooldownSeconds = 0)
        {
            datasave = new InMemoryDatasave();
            return CreateModel(
                CreateConfig(cooldownSeconds),
                datasave,
                wallet,
                new FakeClock(new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc)),
                random);
        }

        private static LuckyWheelModel CreateModel(
            LuckyWheelConfig config,
            InMemoryDatasave datasave,
            FakeWallet wallet,
            FakeClock clock,
            FakeRandom random) =>
            new(config, datasave, wallet, clock, random, "test-save");

        private static LuckyWheelConfig CreateConfig(int cooldownSeconds = 0)
        {
            var segments = new List<LuckyWheelSegment>
            {
                CreateSegment("reward-1", "currency.coin", 10),
                CreateSegment("reward-2", "currency.coin", 20),
                CreateSegment("reward-3", "currency.coin", 30),
                CreateSegment("reward-4", "currency.gem", 4),
                CreateSegment("reward-5", "currency.gem", 5),
                CreateSegment("reward-6", "item.chest", 1)
            };
            var config = new LuckyWheelConfig();
            SetField(config, "wheelId", "test-wheel");
            SetField(config, "revision", 1);
            SetField(config, "cooldownSeconds", cooldownSeconds);
            SetField(config, "segments", segments);
            config.Initialize("test");
            return config;
        }

        private static LuckyWheelSegment CreateSegment(string id, string resourceId, long amount)
        {
            var segment = new LuckyWheelSegment();
            SetField(segment, "id", id);
            SetField(segment, "weight", 1);
            SetField(segment, "resourceId", resourceId);
            SetField(segment, "amount", amount);
            return segment;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private sealed class FakeClock : ILuckyWheelClock
        {
            public FakeClock(DateTime utcNow)
            {
                UtcNow = utcNow;
            }

            public DateTime UtcNow { get; set; }
        }

        private sealed class FakeRandom : ILuckyWheelRandom
        {
            private readonly int value;

            public FakeRandom(int value)
            {
                this.value = value;
            }

            public int CallCount { get; private set; }

            public int Next(int exclusiveMax)
            {
                CallCount++;
                Assert.That(value, Is.GreaterThanOrEqualTo(0).And.LessThan(exclusiveMax));
                return value;
            }
        }

        private sealed class FakeWallet : IResourceWallet
        {
            public List<ResourceGrantRequest> Requests { get; } = new();
            public bool ShouldSucceed { get; set; } = true;

            public bool TryGrant(ResourceGrantRequest request)
            {
                Requests.Add(request);
                return ShouldSucceed;
            }

            public bool TryExchange(ResourceExchangeRequest request) => false;
        }

        private sealed class InMemoryDatasave : IDatasaveService
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
