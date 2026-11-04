using System;
using System.IO;
using System.Linq;
using Dreamy.Debugger.Options;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Template.Demo;
using NUnit.Framework;

namespace Dreamy.Template.Debugging.Tests
{
    public sealed class SandboxCheatTests
    {
        private string directory;
        private DatasaveService saves;
        private DatasaveResourceWallet wallet;
        private CheatRegistry registry;
        private IDisposable lease;
        private bool unlocked;

        [SetUp] public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "DreamySandboxCheats", Guid.NewGuid().ToString("N"));
            saves = CreateSaves();
            wallet = CreateWallet(saves);
            unlocked = true;
            registry = new CheatRegistry(() => unlocked);
            lease = registry.Register(new SandboxGameCheats(wallet, wallet).CreateOptions());
        }

        [TearDown] public void TearDown()
        {
            lease.Dispose();
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test] public void CoinAndGemGrantsPersistAndPublishBalanceChanges()
        {
            var changes = 0;
            wallet.BalanceChanged += _ => changes++;
            registry.Set("sandbox.currency.coin.amount", 250L);
            registry.Execute("sandbox.currency.coin.grant", true);
            registry.Execute("sandbox.currency.coin.grant", true);
            registry.Execute("sandbox.currency.gem.grant", true);
            var restored = CreateWallet(CreateSaves());
            Assert.That(restored.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(500));
            Assert.That(restored.GetBalance(new ResourceId("currency.gem")), Is.EqualTo(100));
            Assert.That(changes, Is.EqualTo(3));
            var transactions = CreateSaves().Load<EconomySaveData>("cheat-test").TransactionIds;
            Assert.That(transactions.Distinct().Count(), Is.EqualTo(3));
            Assert.That(transactions.All(id => id.StartsWith("debugger/sandbox/")), Is.True);
        }

        [Test] public void NoAdsPersistsAndExistingEntitlementEffectRunsOnlyOnce()
        {
            var effects = 0;
            using var effect = new EntitlementEffectBinding(wallet, EntitlementIds.RemoveAds, () => effects++, e => throw e);
            registry.Execute("sandbox.entitlement.remove-ads.unlock", true);
            registry.Execute("sandbox.entitlement.remove-ads.unlock", true);
            Assert.That(effects, Is.EqualTo(1));
            Assert.That(wallet.GetBalance(EntitlementIds.RemoveAds), Is.EqualTo(1));
            Assert.That(CreateWallet(CreateSaves()).GetBalance(EntitlementIds.RemoveAds), Is.EqualTo(1));
            Assert.That(registry.Read("sandbox.entitlement.remove-ads.owned"), Is.True);
        }

        [Test] public void InventoryGrantsUseSharedPersistentInventory()
        {
            registry.Execute("sandbox.card.knight.grant", true);
            registry.Execute("sandbox.chest.silver.grant", true);
            registry.Execute("sandbox.item.demo-card.grant", true);
            registry.Execute("sandbox.skin.hero.red.unlock", true);
            registry.Execute("sandbox.skin.hero.red.unlock", true);
            var restored = CreateWallet(CreateSaves());
            Assert.That(restored.GetBalance(new ResourceId("card.knight")), Is.EqualTo(10));
            Assert.That(restored.GetBalance(new ResourceId("chest.silver")), Is.EqualTo(1));
            Assert.That(restored.GetBalance(new ResourceId("item.demo-card")), Is.EqualTo(10));
            Assert.That(restored.GetBalance(new ResourceId("skin.hero.red")), Is.EqualTo(1));
        }

        [Test] public void LockedUnconfirmedAndInvalidAmountsNeverGrant()
        {
            Assert.Throws<InvalidOperationException>(() => registry.Execute("sandbox.currency.coin.grant"));
            Assert.Throws<ArgumentOutOfRangeException>(() => registry.Set("sandbox.currency.coin.amount", 0L));
            Assert.Throws<ArgumentOutOfRangeException>(() => registry.Set("sandbox.currency.coin.amount", 1000001L));
            unlocked = false;
            Assert.Throws<UnauthorizedAccessException>(() => registry.Execute("sandbox.currency.coin.grant", true));
            Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.Zero);
        }

        [Test] public void DisposingGameRegistrationRemovesAllGameOptions()
        {
            lease.Dispose();
            Assert.That(registry.Snapshot(), Is.Empty);
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => registry.Execute("sandbox.currency.coin.grant", true));
        }

        [Test] public void WalletRejectionIsVisibleWithoutClaimingSuccess()
        {
            var model = new SandboxGameCheats(new RejectingWallet(), wallet);
            var other = new CheatRegistry(() => true);
            using var otherLease = other.Register(model.CreateOptions());
            other.Execute("sandbox.currency.coin.grant", true);
            Assert.That(model.LastResult, Does.StartWith("Not granted:"));
            Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.Zero);
        }

        private DatasaveService CreateSaves() => new DatasaveService(new DatasaveOptions { DirectoryName = directory, CreateFileOnFirstLoad = false });
        private static DatasaveResourceWallet CreateWallet(IDatasaveService service) => new DatasaveResourceWallet(service,
            "cheat-test", rewardDefinitions: FoundationProgressionRewards.RewardDefinitions);
        private sealed class RejectingWallet : IResourceWallet
        {
            public bool TryGrant(ResourceGrantRequest request) => false;
            public bool TryExchange(ResourceExchangeRequest request) => false;
        }
    }
}
