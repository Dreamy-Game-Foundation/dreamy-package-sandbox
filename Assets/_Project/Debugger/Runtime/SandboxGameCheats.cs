using System;
using System.Collections.Generic;
using Dreamy.Debugger.Options;
using Dreamy.Debugger.Sample;
using Dreamy.Economy;
using Dreamy.Feature.Shop.Integration;

namespace Dreamy.Template.Debugging
{
    /// <summary>Game-specific catalog and mutations. Package Runtime never references this class.</summary>
    public sealed class SandboxGameCheats
    {
        private readonly IResourceWallet wallet;
        private readonly IResourceBalanceProvider balances;
        public string LastResult { get; private set; } = "Ready. Grants save through the game wallet.";

        public SandboxGameCheats(IResourceWallet wallet, IResourceBalanceProvider balances)
        {
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            this.balances = balances ?? throw new ArgumentNullException(nameof(balances));
        }

        public IEnumerable<CheatOption> CreateOptions()
        {
            foreach (var option in Resource("currency.coin", "01 Economy", "Coins", 1000)) yield return option;
            foreach (var option in Resource("currency.gem", "01 Economy", "Gems", 100)) yield return option;
            foreach (var option in Resource("card.knight", "02 Inventory", "Knight cards", 10)) yield return option;
            foreach (var option in Resource("chest.silver", "02 Inventory", "Silver chests", 1)) yield return option;
            foreach (var option in Resource("item.demo-card", "02 Inventory", "Daily reward cards", 10)) yield return option;
            foreach (var option in Ownership(new ResourceId("skin.hero.red"), "02 Inventory", "Red hero skin")) yield return option;
            foreach (var option in Ownership(EntitlementIds.RemoveAds, "03 Entitlements", "No Ads")) yield return option;
            foreach (var option in Ownership(EntitlementIds.LifetimeVip, "03 Entitlements", "Lifetime VIP")) yield return option;
            foreach (var option in Ownership(EntitlementIds.StarterFund, "03 Entitlements", "Starter Fund")) yield return option;
            foreach (var option in Ownership(EntitlementIds.Season1PremiumPass, "03 Entitlements", "Season 1 Premium Pass")) yield return option;
            yield return CheatOption.Value("sandbox.status", "04 Diagnostics", "Last cheat result", () => LastResult);
            yield return CheatOption.Value("sandbox.ads-adapter", "04 Diagnostics", "Ads integration",
                () => "Entitlement is persisted; this sandbox has a demo effect, no ads SDK.");
        }

        private IEnumerable<CheatOption> Resource(string resourceId, string category, string label, long initialAmount)
        {
            var id = new ResourceId(resourceId);
            return new ResourceCheatOptions("sandbox." + resourceId, category, label,
                () => balances.GetBalance(id), amount => Grant(id, amount), initialAmount).CreateOptions();
        }

        private IEnumerable<CheatOption> Ownership(ResourceId id, string category, string label) =>
            new OwnershipCheatOptions("sandbox." + id.Value, category, label,
                () => balances.GetBalance(id) > 0, () => Grant(id, 1)).CreateOptions();

        private void Grant(ResourceId id, long amount)
        {
            // A deliberate click is a new grant; persistence/rejection/events stay owned by the wallet.
            var request = new ResourceGrantRequest("debugger/sandbox/" + id.Value + "/" + Guid.NewGuid().ToString("N"),
                new ResourceAmount(id, amount));
            try
            {
                LastResult = wallet.TryGrant(request)
                    ? "Saved: +" + amount + " " + id.Value
                    : "Not granted: wallet rejected " + id.Value + ". Check save recovery/limits.";
            }
            catch (Exception exception)
            {
                LastResult = "Not confirmed: " + id.Value + " (" + exception.GetType().Name + "). Check save recovery before retrying.";
            }
        }
    }
}
