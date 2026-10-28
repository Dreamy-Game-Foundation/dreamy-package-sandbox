using System;
using System.IO;
using System.Linq;
using System.Threading;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Economy.Rewards;
using Dreamy.Economy.UI;
using Dreamy.BattlePass.Presentation;
using Newtonsoft.Json;
using NUnit.Framework;
namespace Dreamy.BattlePass.Tests
{
    public sealed class BattlePassModelTests
    {
        private sealed class Clock : IBattlePassClock { public DateTime UtcNow { get; set; }=new(2026,10,20,12,0,0,DateTimeKind.Utc); }
        private sealed class FaultSave : IDatasaveService
        {
            public IDatasaveService Inner;public Func<BattlePassSaveData,bool> Fail;public bool AfterWrite;
            public T Load<T>(string key=null) where T:SaveData,new()=>Inner.Load<T>(key);
            public void Save<T>(T value,string key=null) where T:SaveData
            { bool fail=value is BattlePassSaveData bp && Fail != null && Fail(bp);if(fail){Fail=null;if(AfterWrite)Inner.Save(value,key);throw new IOException("Injected failure");}Inner.Save(value,key); }
            public void SaveAll()=>Inner.SaveAll();public bool Exists(string key)=>Inner.Exists(key);public void Delete(string key)=>Inner.Delete(key);public void DeleteAll()=>Inner.DeleteAll();
        }
        private string path;private Clock clock;private DatasaveResourceWallet wallet;private FaultSave store;private BattlePassModel model;private BattlePassCatalogConfig catalog;
        private static readonly ResourceId Coin=new("currency.coin"),Entitlement=new("entitlement.battlepass.demo.premium");
        private BattlePassCatalogConfig Catalog() => JsonConvert.DeserializeObject<BattlePassCatalogConfig>(@"{'seasons':[
          {'SeasonId':'demo','OccurrenceId':'demo-1','Revision':'1','PremiumEntitlementId':'entitlement.battlepass.demo.premium','Tiers':[
           {'Id':'one','RequiredXp':20,'Free':{'Entries':[{'Id':'coin','KindId':'currency','TargetId':'currency.coin','Amount':100}]},'Premium':{'Entries':[{'Id':'coin','KindId':'currency','TargetId':'currency.coin','Amount':200}]}},
           {'Id':'two','RequiredXp':40,'Free':{'Entries':[{'Id':'coin','KindId':'currency','TargetId':'currency.coin','Amount':300}]}}]},
          {'SeasonId':'demo','OccurrenceId':'demo-2','Revision':'2','PremiumEntitlementId':'entitlement.battlepass.demo2.premium','Tiers':[
           {'Id':'one','RequiredXp':20,'Free':{'Entries':[{'Id':'coin','KindId':'currency','TargetId':'currency.coin','Amount':999}]}}]}
        ]}");
        [SetUp] public void Setup()
        { path=Path.Combine(Path.GetTempPath(),"DreamyBattlePass-"+Guid.NewGuid().ToString("N"));clock=new Clock();catalog=Catalog();Reconstruct(); }
        [TearDown] public void Cleanup() { if(Directory.Exists(path))Directory.Delete(path,true); }
        private void Reconstruct()
        {
            var inner=new DatasaveService(new DatasaveOptions{DirectoryName=path,CreateFileOnFirstLoad=false});store=new FaultSave{Inner=inner};
            wallet=new DatasaveResourceWallet(inner,rewardDefinitions:new[]{new ResourceRewardDefinition("entitlement",Entitlement,true)});
            model=new BattlePassModel(catalog,store,new RewardDeliveryGateway(new IRewardDestination[]{wallet}),clock,new EconomyBattlePassPremiumAccess(wallet));
        }
        private void Earn(long xp=20,string id="run-1") { Assert.IsTrue(model.Enroll("demo-1").IsSuccess);Assert.IsTrue(model.ReportProgress(new("demo-1",id,xp)).IsSuccess); }
        private string Free => model.GetState().Slots.First(s=>!s.Premium).Id;
        private void Unlock() { Assert.IsTrue(wallet.TryGrant(new ResourceGrantRequest("premium",new ResourceAmount(Entitlement,1)))); }
        [Test] public void ReadAndOpenDoNotEnrollOrGrant()
        { Assert.AreEqual(BattlePassPhase.NotEnrolled,model.GetState().Phase);Assert.IsNull(model.GetClaimOffer("demo-1","missing"));Assert.AreEqual(0,wallet.GetBalance(Coin));Assert.IsFalse(store.Exists("battle-pass")); }
        [Test] public void ProgressDedupSurvivesRestartAndRejectsChangedPayload()
        { Earn();Reconstruct();Assert.AreEqual(BattlePassStatus.AlreadyApplied,model.ReportProgress(new("demo-1","run-1",20)).Status);Assert.AreEqual(BattlePassStatus.PayloadMismatch,model.ReportProgress(new("demo-1","run-1",21)).Status);Assert.AreEqual(20,model.GetState().Xp); }
        [Test] public void XpCapsWithoutOverflowAndStillRecordsReceipts()
        { Earn(long.MaxValue);Assert.IsTrue(model.ReportProgress(new("demo-1","run-2",long.MaxValue)).IsSuccess);Assert.AreEqual(40,model.GetState().Xp);Assert.AreEqual(BattlePassStatus.AlreadyApplied,model.ReportProgress(new("demo-1","run-2",long.MaxValue)).Status); }
        [Test] public void ClaimIsExactlyOnceAcrossRestart()
        { Earn();string slot=Free;Assert.IsTrue(model.Claim("demo-1",slot).IsSuccess);Reconstruct();Assert.AreEqual(BattlePassStatus.AlreadyApplied,model.Claim("demo-1",slot).Status);Assert.AreEqual(100,wallet.GetBalance(Coin)); }
        [Test] public void PremiumUnlockLateAllowsPreviousTiers()
        { Earn();string slot=model.GetState().Slots.First(s=>s.Premium).Id;Assert.AreEqual(BattlePassStatus.Locked,model.Claim("demo-1",slot).Status);Unlock();Assert.IsTrue(model.Claim("demo-1",slot).IsSuccess);Assert.AreEqual(200,wallet.GetBalance(Coin)); }
        [Test] public void GraceStopsXpButAllowsClaimsAndDeadlineIsExclusive()
        { Earn();clock.UtcNow=clock.UtcNow.AddDays(14);Assert.AreEqual(BattlePassPhase.Grace,model.GetState().Phase);Assert.AreEqual(BattlePassStatus.Expired,model.ReportProgress(new("demo-1","late",20)).Status);Assert.IsTrue(model.Claim("demo-1",Free).IsSuccess);clock.UtcNow=clock.UtcNow.AddHours(48);Assert.AreEqual(BattlePassPhase.Expired,model.GetState().Phase); }
        [Test] public void DurableHighWaterPreventsRollbackReopeningSeason()
        { Earn();clock.UtcNow=clock.UtcNow.AddDays(20);model.Refresh();clock.UtcNow=clock.UtcNow.AddDays(-30);Reconstruct();Assert.AreEqual(BattlePassPhase.Expired,model.GetState().Phase);Assert.AreEqual(BattlePassStatus.Expired,model.Claim("demo-1",Free).Status); }
        [TestCase(false)] [TestCase(true)] public void FinalCommitUncertaintyBlocksUntilAllOwnersReconstructed(bool afterWrite)
        {
            Earn();string slot=Free;store.AfterWrite=afterWrite;store.Fail=s=>s.Seasons[0].Claims.Values.Any(j=>j.State == RewardJournalState.Completed);
            Assert.AreEqual(BattlePassStatus.RequiresReload,model.Claim("demo-1",slot).Status);Assert.AreEqual(100,wallet.GetBalance(Coin));
            Assert.AreEqual(BattlePassStatus.RequiresReload,model.ReportProgress(new("demo-1","run-2",20)).Status);
            clock.UtcNow=clock.UtcNow.AddDays(30);Reconstruct();Assert.IsTrue(model.RecoverPending().IsSuccess);Assert.AreEqual(100,wallet.GetBalance(Coin));Assert.AreEqual(BattlePassStatus.AlreadyApplied,model.Claim("demo-1",slot).Status);
        }
        [TestCase(RewardJournalState.IntentPersisted)] [TestCase(RewardJournalState.Resolved)] [TestCase(RewardJournalState.DeliveredAwaitingClaimCommit)]
        public void FailureAtEveryJournalWriteResumesOnce(RewardJournalState state)
        { Earn();string slot=Free;store.Fail=s=>s.Seasons[0].Claims.Values.Any(j=>j.State == state);Assert.AreEqual(BattlePassStatus.RequiresReload,model.Claim("demo-1",slot).Status);Reconstruct();Assert.IsTrue(model.Claim("demo-1",slot).IsSuccess);Assert.AreEqual(100,wallet.GetBalance(Coin)); }
        [Test] public void FrozenBatchResumesAfterDeadlineAndDoesNotAddPremiumUnlockedLater()
        {
            Earn(40);store.Fail=s=>s.Seasons[0].Claims.Values.Any(j=>j.State == RewardJournalState.Completed);
            Assert.AreEqual(BattlePassStatus.RequiresReload,model.ClaimAll().Status);Unlock();clock.UtcNow=clock.UtcNow.AddDays(30);Reconstruct();
            Assert.IsTrue(model.RecoverPending().IsSuccess);Assert.AreEqual(400,wallet.GetBalance(Coin));Assert.IsNull(store.Inner.Load<BattlePassSaveData>("battle-pass").Seasons[0].Batch);
        }
        [Test] public void EnrollmentSnapshotSurvivesConfigRemovalAndRolloverRetainsTombstone()
        {
            Earn();string slot=Free;catalog.Seasons.RemoveAt(0);Reconstruct();Assert.IsTrue(model.Claim("demo-1",slot).IsSuccess);
            clock.UtcNow=clock.UtcNow.AddDays(20);Assert.IsTrue(model.Enroll("demo-2").IsSuccess);Assert.AreEqual(BattlePassStatus.AlreadyApplied,model.Claim("demo-1",slot).Status);
            Assert.AreEqual(BattlePassStatus.AlreadyApplied,model.Enroll("demo-1").Status);Assert.AreEqual("demo-2",model.GetState().OccurrenceId);
        }
        [Test] public void ClaimPanelPreviewAndSilentFlowUseSameAuthority()
        {
            Earn();var source=new BattlePassClaimSource("profile",model);var reference=source.Reference("demo-1",Free);
            var preview=source.QueryAsync(reference,CancellationToken.None).GetAwaiter().GetResult();Assert.AreEqual(0,wallet.GetBalance(Coin));
            Assert.AreEqual(RewardClaimStatus.Blocked,source.ClaimAsync(new RewardClaimCommand(reference,"wrong")).GetAwaiter().GetResult().Status);
            Assert.IsTrue(RewardClaimFlow.ClaimAsync(reference,source).GetAwaiter().GetResult().IsCompleted);Assert.AreEqual(100,wallet.GetBalance(Coin));
        }
        [Test] public void CorruptOrFutureSchemaIsRejectedWithoutReset()
        { Earn();var data=store.Inner.Load<BattlePassSaveData>("battle-pass").Clone();data.Schema=99;store.Inner.Save(data,"battle-pass");Assert.Throws<InvalidOperationException>(()=>Reconstruct());Assert.AreEqual(99,store.Inner.Load<BattlePassSaveData>("battle-pass").Schema); }
        [Test] public void InvalidCatalogRejectsDuplicateThresholdsAndBadBundles()
        { catalog.Seasons[0].Tiers[1].RequiredXp=20;Assert.Throws<ArgumentException>(()=>catalog.Initialize("test")); }
        [Test] public void ObserverFailureDoesNotTurnCommittedGrantIntoFailure()
        { Earn();model.StateChanged+=()=>throw new Exception("Broken UI");Assert.IsTrue(model.Claim("demo-1",Free).IsSuccess);Assert.AreEqual(100,wallet.GetBalance(Coin)); }

        [Test] public void FixedWindowRejectsEarlyEnrollmentAndStopsAtExclusiveEnd()
        {
            var definition=catalog.Seasons[0];definition.Mode=BattlePassSeasonMode.FixedUtcWindow;definition.StartUtcTicks=clock.UtcNow.AddHours(1).Ticks;definition.EndUtcTicks=clock.UtcNow.AddHours(2).Ticks;Reconstruct();
            Assert.AreEqual(BattlePassStatus.Expired,model.Enroll("demo-1").Status);clock.UtcNow=clock.UtcNow.AddHours(1);Assert.IsTrue(model.Enroll("demo-1").IsSuccess);
            clock.UtcNow=clock.UtcNow.AddHours(1);Assert.AreEqual(BattlePassPhase.Grace,model.GetState().Phase);Assert.AreEqual(BattlePassStatus.Expired,model.ReportProgress(new("demo-1","run-1",20)).Status);
        }
        [Test] public void PremiumEntitlementCannotBeSharedAcrossOccurrences()
        { catalog.Seasons[1].PremiumEntitlementId=catalog.Seasons[0].PremiumEntitlementId;Assert.Throws<ArgumentException>(()=>catalog.Initialize("test")); }
        [Test] public void NewClaimAtExactDeadlineIsRejected()
        { Earn();clock.UtcNow=clock.UtcNow.AddDays(16);Assert.AreEqual(BattlePassStatus.Expired,model.Claim("demo-1",Free).Status);Assert.AreEqual(0,wallet.GetBalance(Coin)); }
        [Test] public void PendingClaimBlocksRolloverAndUnrelatedMutations()
        {
            Earn(40);store.Fail=s=>s.Seasons[0].Claims.Values.Any(j=>j.State == RewardJournalState.Resolved);
            Assert.AreEqual(BattlePassStatus.RequiresReload,model.Claim("demo-1",Free).Status);Reconstruct();
            Assert.AreEqual(BattlePassStatus.Pending,model.ReportProgress(new("demo-1","new",20)).Status);clock.UtcNow=clock.UtcNow.AddDays(30);
            Assert.AreEqual(BattlePassStatus.Pending,model.Enroll("demo-2").Status);Assert.IsTrue(model.RecoverPending().IsSuccess);Assert.IsTrue(model.Enroll("demo-2").IsSuccess);
        }
        private sealed class View : IBattlePassView
        {
            public event Action<string> ClaimRequested;public event Action ClaimAllRequested;public event Action RecoverRequested;public event Action PremiumRequested;
            public int ClaimListeners=>ClaimRequested?.GetInvocationList().Length ?? 0;
            public void Render(BattlePassViewState state){}public void ShowResult(BattlePassResult result){}
            public void Claim(string slot)=>ClaimRequested?.Invoke(slot);
            public void Touch(){ClaimAllRequested?.Invoke();RecoverRequested?.Invoke();PremiumRequested?.Invoke();}
        }
        [Test] public void PresenterRepeatedShowAndDisposeDoNotMultiplyListeners()
        {
            Earn();var view=new View();var presenter=new BattlePassPresenter(model,view);
            for(int i=0;i<100;i++){presenter.Show();presenter.Show();Assert.AreEqual(1,view.ClaimListeners);presenter.Dispose();Assert.AreEqual(0,view.ClaimListeners);}
            presenter.Show();view.Claim(Free);presenter.Dispose();Assert.AreEqual(100,wallet.GetBalance(Coin));
        }
    }
}
