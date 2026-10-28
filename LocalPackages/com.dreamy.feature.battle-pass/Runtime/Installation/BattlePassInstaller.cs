using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Economy.Rewards;
namespace Dreamy.BattlePass
{
    public static class BattlePassInstaller
    {
        public static void RegisterConfig(IDataConfigService config) => config.Register<BattlePassCatalogConfig>("battlePassCatalog");
        public static BattlePassModel Install(BattlePassCatalogConfig catalog,IDatasaveService save,IRewardDeliveryGateway gateway,IBattlePassPremiumAccess premium,IBattlePassClock clock=null,string saveKey="battle-pass")
        { var model=new BattlePassModel(catalog,save,gateway,clock ?? new SystemBattlePassClock(),premium,saveKey);ServiceLocator.Register<IBattlePassService>(model);return model; }
    }
    public sealed class EconomyBattlePassPremiumAccess : IBattlePassPremiumAccess
    {
        private readonly IResourceBalanceProvider wallet;
        public EconomyBattlePassPremiumAccess(IResourceBalanceProvider wallet) { this.wallet=wallet ?? throw new System.ArgumentNullException(nameof(wallet)); }
        public bool HasAccess(string entitlementId) => wallet.GetBalance(new ResourceId(entitlementId)) > 0;
    }
}
