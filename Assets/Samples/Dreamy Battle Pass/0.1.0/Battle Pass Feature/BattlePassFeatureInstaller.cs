using System;
using Cysharp.Threading.Tasks;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Economy.Rewards;
using Dreamy.Shop;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassFeatureInstaller
    {
        public const string Occurrence="puzzle-demo-2026-01";
        public const string Entitlement="entitlement.battlepass.puzzle-demo-2026-01.premium";
        public BattlePassModel Model {get;private set;}
        public DatasaveResourceWallet Wallet {get;private set;}
        public DemoBattlePassClock Clock {get;private set;}
        public BattlePassCompletionOutbox Outbox {get;private set;}
        public ShopModel Shop {get;private set;}
        public async UniTask Initialize(System.Threading.CancellationToken token)
        {
            var config=new DataConfigService(new ResourcesJsonConfigSource());BattlePassInstaller.RegisterConfig(config);
            config.Register<ShopCatalogConfig>("battlePassDemoShop");await config.InitializeAsync(token);token.ThrowIfCancellationRequested();
            // Independent demo profile. Production receives the existing shared save/wallet/gateway.
            var save=new DatasaveService(new DatasaveOptions{DirectoryName="DreamyBattlePassDemo"});
            Wallet=new DatasaveResourceWallet(save,rewardDefinitions:new[]{new ResourceRewardDefinition("item",new ResourceId("item.booster.hammer")),new ResourceRewardDefinition("entitlement",new ResourceId(Entitlement),true)});
            Clock=new DemoBattlePassClock(save);Outbox=new BattlePassCompletionOutbox(save);
            Model=new BattlePassModel(config.GetTable<BattlePassCatalogConfig>(),save,new RewardDeliveryGateway(new[]{Wallet.CreateRewardDestination("currency"),Wallet.CreateRewardDestination("item")}),Clock,new EconomyBattlePassPremiumAccess(Wallet));
            Shop=new ShopModel(config.GetTable<ShopCatalogConfig>(),Wallet,balances:Wallet);
            // Demo-only seed has a durable receipt. It never runs in package domain.
            if(!Wallet.TryGrant(new ResourceGrantRequest("battle-pass-demo-seed-v1",new ResourceAmount(new ResourceId("currency.gem"),500))))throw new InvalidOperationException("Demo seed failed.");
            Model.RecoverPending();Outbox.Drain(Model);
        }
        public string BuyPremium()
        {
            Model.Refresh();var state=Model.GetState();
            if(state.RequiresReload || state.OccurrenceId != Occurrence || state.Phase != BattlePassPhase.Active || state.HasPending)return "Premium purchase unavailable for this occurrence.";
            var result=Shop.Purchase("battle-pass-premium");Model.Refresh();return result.Status.ToString();
        }
    }
}
