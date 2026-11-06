using System.ComponentModel;
using Dreamy.Debugger.Options;
using Dreamy.Feature.Shop.Integration;

namespace Dreamy.Template.Debugging
{
    public sealed partial class GameCheatOptions
    {
        private const string NoAdsCheatId = "sandbox.entitlement.remove-ads";

        [Category("03 Entitlements"), OwnershipCheat(NoAdsCheatId, "No Ads", CheatCardRole.Owned)]
        public bool OwnsNoAds => IsOwned(EntitlementIds.RemoveAds);

        [Category("03 Entitlements"), OwnershipCheat(NoAdsCheatId, "No Ads", CheatCardRole.Action)]
        public void UnlockNoAds() => GrantOwnership(EntitlementIds.RemoveAds);

        private const string LifetimeVipCheatId = "sandbox.entitlement.vip.lifetime";

        [Category("03 Entitlements"), OwnershipCheat(LifetimeVipCheatId, "Lifetime VIP", CheatCardRole.Owned)]
        public bool OwnsLifetimeVip => IsOwned(EntitlementIds.LifetimeVip);

        [Category("03 Entitlements"), OwnershipCheat(LifetimeVipCheatId, "Lifetime VIP", CheatCardRole.Action)]
        public void UnlockLifetimeVip() => GrantOwnership(EntitlementIds.LifetimeVip);

        private const string StarterFundCheatId = "sandbox.entitlement.fund.starter";

        [Category("03 Entitlements"), OwnershipCheat(StarterFundCheatId, "Starter Fund", CheatCardRole.Owned)]
        public bool OwnsStarterFund => IsOwned(EntitlementIds.StarterFund);

        [Category("03 Entitlements"), OwnershipCheat(StarterFundCheatId, "Starter Fund", CheatCardRole.Action)]
        public void UnlockStarterFund() => GrantOwnership(EntitlementIds.StarterFund);

        private const string SeasonPremiumCheatId = "sandbox.entitlement.battle-pass.season-1.premium";

        [Category("03 Entitlements"), OwnershipCheat(SeasonPremiumCheatId, "Season 1 Premium Pass", CheatCardRole.Owned)]
        public bool OwnsSeasonPremium => IsOwned(EntitlementIds.Season1PremiumPass);

        [Category("03 Entitlements"), OwnershipCheat(SeasonPremiumCheatId, "Season 1 Premium Pass", CheatCardRole.Action)]
        public void UnlockSeasonPremium() => GrantOwnership(EntitlementIds.Season1PremiumPass);
    }
}
