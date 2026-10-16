using System;
using System.Linq;
using Dreamy.Datasave;
using Dreamy.Economy;
using System.Collections.Generic;
using Dreamy.Economy.Rewards;
using Dreamy.Progression;

namespace Dreamy.Template.Demo
{
    // Sandbox composition and fault injection; package Runtime has no game dependency.
    public sealed class FoundationProgressionRewards
    {
        public const string SaveKey = "foundation-progression-multi-reward-v2";
        private readonly DatasaveResourceWallet wallet;
        private readonly FaultStore store;
        public ProgressionModel Model { get; }
        public long CardCount => wallet.GetBalance(new ResourceId("card.knight"));
        public long ChestCount => wallet.GetBalance(new ResourceId("chest.silver"));
        public long CosmeticCount => wallet.GetBalance(new ResourceId("skin.hero.red"));
        private bool failCard;
        public static IReadOnlyList<ResourceRewardDefinition> RewardDefinitions => new[]
        {
            new ResourceRewardDefinition("card", new ResourceId("card.knight")),
            new ResourceRewardDefinition("item", new ResourceId("item.demo-card")),
            new ResourceRewardDefinition("chest", new ResourceId("chest.silver")),
            new ResourceRewardDefinition("cosmetic", new ResourceId("skin.hero.red"), unique: true)
        };
        public string LastGrantId { get; private set; }
        public long LastGrantAmount { get; private set; }
        public FoundationProgressionRewards(ProgressionCatalogConfig catalog, IDatasaveService datasave, DatasaveResourceWallet wallet)
        {
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            ImportLegacyInventories(datasave, wallet);
            store = new FaultStore(datasave);
            var gateway = new RewardDeliveryGateway(new[]
            {
                wallet.CreateRewardDestination("currency"),
                wallet.CreateRewardDestination("card"),
                wallet.CreateRewardDestination("chest"),
                wallet.CreateRewardDestination("cosmetic")
            });
            Model = new ProgressionModel(catalog, store, new FaultGateway(this, gateway), SaveKey);
            Model.RetryPendingRewards();
        }
        public ProgressionAdvanceResult Grant(long amount)
        {
            LastGrantId = "foundation-progression/" + Guid.NewGuid().ToString("N");
            LastGrantAmount = amount;
            return Model.GrantExperience(LastGrantId, amount);
        }
        public ProgressionAdvanceResult RetryLastGrant() => Model.GrantExperience(LastGrantId, LastGrantAmount);
        public void FailCard() => failCard = true;
        public void FailAcknowledgement() => store.FailAck = true;
        public void FailCompletion() => store.FailComplete = true;
        public void Recover()
        {
            wallet.Reload();
            Model.Reload();
            Model.RetryPendingRewards();
        }

        public static void ImportLegacyInventories(IDatasaveService datasave, DatasaveResourceWallet wallet)
        {
            const string dailyKey = "daily-demo-items";
            if (datasave.Exists(dailyKey))
            {
                var legacyDaily = datasave.Load<Dreamy.Feature.DailyReward.Integration.DemoItemSaveData>(dailyKey);
                if (legacyDaily.Count < 0 || legacyDaily.Receipts == null) throw new InvalidOperationException("Invalid legacy Daily inventory.");
                var amounts = legacyDaily.Count > 0
                    ? new[] { new ResourceAmount(new ResourceId("item.demo-card"), legacyDaily.Count) } : Array.Empty<ResourceAmount>();
                var receipts = legacyDaily.Receipts.Select(r => new RewardReceiptSaveEntry { TransactionId = r.Key, Fingerprint = r.Value }).ToArray();
                var imported = wallet.ImportInventory("foundation-daily-items-v1", amounts, receipts);
                if (!imported.IsSuccess) throw new InvalidOperationException("Daily inventory migration needs recovery: " + imported.Reason);
            }
            foreach (var pair in new[] { (kind: "card", target: "card.knight"), (kind: "chest", target: "chest.silver"), (kind: "cosmetic", target: "skin.hero.red") })
            {
                string key = "foundation-progression-v2-" + pair.kind;
                if (!datasave.Exists(key)) continue;
                var legacy = datasave.Load<FoundationProgressionInventorySave>(key);
                if (legacy.Count < 0 || legacy.Receipts == null) throw new InvalidOperationException("Invalid legacy inventory.");
                var amounts = new List<ResourceAmount>();
                if (legacy.Count > 0) amounts.Add(new ResourceAmount(new ResourceId(pair.target), legacy.Count));
                var receipts = legacy.Receipts.Select(r => new RewardReceiptSaveEntry { TransactionId = r.Key, Fingerprint = r.Value }).ToArray();
                var imported = wallet.ImportInventory("foundation-inventory-v2/" + pair.kind, amounts, receipts);
                if (!imported.IsSuccess) throw new InvalidOperationException("Inventory migration needs recovery: " + imported.Reason);
            }
        }

        private sealed class FaultGateway : IRewardDeliveryGateway
        {
            private readonly FoundationProgressionRewards demo;
            private readonly IRewardDeliveryGateway inner;
            public FaultGateway(FoundationProgressionRewards demo, IRewardDeliveryGateway inner) { this.demo = demo; this.inner = inner; }
            public RewardDeliveryResult Validate(string id, string fingerprint, ResolvedRewardBundle bundle) => inner.Validate(id, fingerprint, bundle);
            public RewardDeliveryResult Deliver(RewardDeliveryRequest request)
            {
                if (demo.failCard && request.Entry.KindId == "card")
                { demo.failCard = false; return RewardDeliveryResult.Pending(RewardDeliveryReason.DestinationUnavailable); }
                return inner.Deliver(request);
            }
        }

        private sealed class FaultStore : IDatasaveService
        {
            private readonly IDatasaveService inner;
            private ProgressionSaveData last;
            public bool FailAck, FailComplete;
            public FaultStore(IDatasaveService inner) => this.inner = inner;
            public T Load<T>(string key = null) where T : SaveData, new()
            {
                var value = inner.Load<T>(key);
                if (value is ProgressionSaveData progress) last = progress.Clone();
                return value;
            }
            public void Save<T>(T value, string key = null) where T : SaveData
            {
                if (value is ProgressionSaveData progress)
                {
                    if (FailAck && progress.MilestoneRewards.Any(r => r.Journal.Entries.Count(e => e.Acknowledged) >
                        (last?.MilestoneRewards.FirstOrDefault(old => old.MilestoneId == r.MilestoneId)?.Journal.Entries.Count(e => e.Acknowledged) ?? 0)))
                    { FailAck = false; throw new System.IO.IOException("Foundation demo: journal acknowledgement failure."); }
                    if (FailComplete && progress.MilestoneRewards.Any(r => r.Journal.State == RewardJournalState.Completed &&
                        last?.MilestoneRewards.FirstOrDefault(old => old.MilestoneId == r.MilestoneId)?.Journal.State != RewardJournalState.Completed))
                    { FailComplete = false; throw new System.IO.IOException("Foundation demo: completion failure."); }
                }
                inner.Save(value, key);
                if (value is ProgressionSaveData committed) last = committed.Clone();
            }
            public void SaveAll() => inner.SaveAll();
            public bool Exists(string key) => inner.Exists(key);
            public void Delete(string key) => inner.Delete(key);
            public void DeleteAll() => inner.DeleteAll();
        }
    }
}
