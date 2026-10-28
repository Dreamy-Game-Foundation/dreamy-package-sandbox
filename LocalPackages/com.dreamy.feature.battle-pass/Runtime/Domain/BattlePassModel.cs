using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dreamy.Datasave;
using Dreamy.Economy.Rewards;
namespace Dreamy.BattlePass
{
    // Single synchronous owner per profile/save key. UI never owns this lifetime.
    public sealed partial class BattlePassModel : IBattlePassService
    {
        private readonly BattlePassCatalogConfig catalog;
        private readonly IDatasaveService save;
        private readonly IRewardDeliveryGateway gateway;
        private readonly IBattlePassClock clock;
        private readonly IBattlePassPremiumAccess premium;
        private readonly string saveKey;
        private BattlePassSaveData data;
        private bool busy,requiresReload;
        public event Action StateChanged;
        public event Action<Exception> Diagnostic;
        public BattlePassModel(BattlePassCatalogConfig catalog,IDatasaveService save,IRewardDeliveryGateway gateway,IBattlePassClock clock,IBattlePassPremiumAccess premium,string saveKey="battle-pass")
        {
            this.catalog=catalog ?? throw new ArgumentNullException(nameof(catalog));catalog.Initialize("battlePassCatalog");
            this.save=save ?? throw new ArgumentNullException(nameof(save));this.gateway=gateway ?? throw new ArgumentNullException(nameof(gateway));
            this.clock=clock ?? throw new ArgumentNullException(nameof(clock));this.premium=premium ?? throw new ArgumentNullException(nameof(premium));
            this.saveKey=string.IsNullOrWhiteSpace(saveKey) ? throw new ArgumentException("Save key required.") : saveKey;
            data=save.Load<BattlePassSaveData>(saveKey).Clone();ValidateSave();
        }
        private BattlePassSeasonSave Active => data.Seasons.Find(s=>s.Snapshot.OccurrenceId == data.ActiveOccurrenceId);
        private long Now
        {
            get { var utc=clock.UtcNow;if(utc.Kind != DateTimeKind.Utc)throw new InvalidOperationException("Clock must return UTC.");return Math.Max(utc.Ticks,data.LastObservedUtcTicks); }
        }
        private static string Slot(string tier,bool premiumTrack) => RewardIdentity.Encode("battle-pass-slot",tier,premiumTrack ? "premium" : "free");
        private static IEnumerable<(string Id,BattlePassTierDefinition Tier,bool Premium,BattlePassRewardBundleConfig Reward)> Slots(BattlePassSeasonSave season)
        {
            foreach(var tier in season.Snapshot.Tiers)
            { if(tier.Free != null)yield return (Slot(tier.Id,false),tier,false,tier.Free);if(tier.Premium != null)yield return (Slot(tier.Id,true),tier,true,tier.Premium); }
        }
        private string RequestId(BattlePassSeasonSave s,string slot) => RewardIdentity.Request(saveKey,"battle-pass",s.Snapshot.SeasonId,s.Snapshot.OccurrenceId,slot);
        private string Fingerprint(BattlePassSeasonSave s,string slot,ResolvedRewardBundle bundle) => RewardIdentity.Fingerprint("battle-pass-claim-v1",RequestId(s,slot),s.Snapshot.Revision,bundle.Fingerprint);
        private bool Pending => data.Seasons.Any(s=>s.Claims.Values.Any(j=>j.State != RewardJournalState.Completed) || s.Batch != null);
        private void Commit(BattlePassSaveData candidate)
        {
            try { save.Save(candidate,saveKey); } catch { requiresReload=true;throw; }
            data=candidate;
        }
        private BattlePassResult Mutate(Func<BattlePassResult> command,bool observe=true)
        {
            if(requiresReload)return new(BattlePassStatus.RequiresReload);
            if(busy)return new(BattlePassStatus.Busy);
            busy=true;BattlePassResult result;
            try
            {
                if(observe) { long now=Now;if(now != data.LastObservedUtcTicks) { var next=data.Clone();next.LastObservedUtcTicks=now;Commit(next); } }
                result=command();
            }
            catch(Exception e) { requiresReload=true;Report(e);result=new(BattlePassStatus.RequiresReload,e.Message); }
            finally { busy=false; }
            // Observer exceptions must never change a committed domain result.
            if(StateChanged != null)foreach(Action listener in StateChanged.GetInvocationList())try { listener(); }catch(Exception e) { Report(e); }
            return result;
        }
        private void Report(Exception e) { if(Diagnostic != null)foreach(Action<Exception> listener in Diagnostic.GetInvocationList())try { listener(e); }catch {} }
        public BattlePassResult Refresh() => Mutate(()=>new(BattlePassStatus.Success));
        public BattlePassResult Enroll(string occurrenceId) => Mutate(()=>
        {
            if(Pending)return new(BattlePassStatus.Pending,"Recover previous claims/batch first.");
            if(data.Seasons.Any(s=>s.Snapshot.OccurrenceId == occurrenceId))return new(BattlePassStatus.AlreadyApplied);
            if(Active != null && Now < Active.ClaimDeadlineUtcTicks)return new(BattlePassStatus.Locked,"Previous occurrence has not closed.");
            var definition=catalog.Seasons.Find(s=>s.OccurrenceId == occurrenceId);
            if(definition == null)return new(BattlePassStatus.Unavailable);
            long now=Now;
            if(definition.Mode == BattlePassSeasonMode.FixedUtcWindow && (now < definition.StartUtcTicks || now >= definition.EndUtcTicks))return new(BattlePassStatus.Expired);
            long end=definition.Mode == BattlePassSeasonMode.EnrollmentDuration ? checked(now+TimeSpan.FromDays(definition.DurationDays).Ticks) : definition.EndUtcTicks;
            long deadline=checked(end+TimeSpan.FromHours(definition.GraceHours).Ticks);
            if(deadline > DateTime.MaxValue.Ticks)return new(BattlePassStatus.InvalidRequest);
            var next=data.Clone();next.Seasons.Add(new BattlePassSeasonSave{Snapshot=definition.Clone(),EnrolledUtcTicks=now,EndUtcTicks=end,ClaimDeadlineUtcTicks=deadline});next.ActiveOccurrenceId=occurrenceId;Commit(next);
            return new(BattlePassStatus.Success);
        });
        public BattlePassResult ReportProgress(BattlePassProgressRequest request)
        {
            if(request == null || string.IsNullOrWhiteSpace(request.EventId) || request.Xp <= 0)return new(BattlePassStatus.InvalidRequest);
            return Mutate(()=>
            {
                var season=data.Seasons.Find(s=>s.Snapshot.OccurrenceId == request.OccurrenceId);if(season == null)return new(BattlePassStatus.NotEnrolled);
                string fp=RewardIdentity.Fingerprint("battle-pass-xp-v1",request.OccurrenceId,request.EventId,request.Xp.ToString(CultureInfo.InvariantCulture));
                if(season.ProgressReceipts.TryGetValue(request.EventId,out var old))return new(old == fp ? BattlePassStatus.AlreadyApplied : BattlePassStatus.PayloadMismatch);
                if(season != Active || Now >= season.EndUtcTicks)return new(BattlePassStatus.Expired);
                if(Pending)return new(BattlePassStatus.Pending,"Recover before new progress.");
                var next=data.Clone();var updated=next.Seasons.Find(s=>s.Snapshot.OccurrenceId == request.OccurrenceId);
                long max=updated.Snapshot.Tiers.Last().RequiredXp;updated.Xp+=Math.Min(request.Xp,max-updated.Xp);updated.ProgressReceipts.Add(request.EventId,fp);Commit(next);
                return new(BattlePassStatus.Success);
            });
        }
        public BattlePassViewState GetState()
        {
            var s=Active;long now=Now;
            if(s == null)return new BattlePassViewState{Phase=BattlePassPhase.NotEnrolled,RequiresReload=requiresReload,NowUtcTicks=now,HasPending=Pending};
            bool access=premium.HasAccess(s.Snapshot.PremiumEntitlementId);var slots=new List<BattlePassSlotViewState>();
            foreach(var slot in Slots(s))slots.Add(new BattlePassSlotViewState{Id=slot.Id,TierId=slot.Tier.Id,Premium=slot.Premium,RequiredXp=slot.Tier.RequiredXp,
                Bundle=slot.Reward.ToBundle(),Status=Eligibility(s,slot.Id,access,now)});
            return new BattlePassViewState{OccurrenceId=s.Snapshot.OccurrenceId,Title=s.Snapshot.Title,Xp=s.Xp,MaxXp=s.Snapshot.Tiers.Last().RequiredXp,
                EndUtcTicks=s.EndUtcTicks,ClaimDeadlineUtcTicks=s.ClaimDeadlineUtcTicks,NowUtcTicks=now,IsPremium=access,RequiresReload=requiresReload,HasPending=Pending,
                Phase=now < s.EndUtcTicks ? BattlePassPhase.Active : now < s.ClaimDeadlineUtcTicks ? BattlePassPhase.Grace : BattlePassPhase.Expired,
                ClaimableCount=slots.Count(x=>x.Status == BattlePassStatus.Success),Slots=slots.AsReadOnly()};
        }
        private BattlePassStatus Eligibility(BattlePassSeasonSave s,string slotId,bool access,long now)
        {
            if(requiresReload)return BattlePassStatus.RequiresReload;
            if(s.Claims.TryGetValue(slotId,out var journal))return journal.State == RewardJournalState.Completed ? BattlePassStatus.AlreadyApplied : BattlePassStatus.Pending;
            var slot=Slots(s).FirstOrDefault(x=>x.Id == slotId);if(slot.Reward == null)return BattlePassStatus.Unavailable;
            if(s.Batch != null && s.Batch.Slots.Skip(s.Batch.Cursor).Contains(slotId))return BattlePassStatus.Pending;
            if(s != Active || now >= s.ClaimDeadlineUtcTicks)return BattlePassStatus.Expired;
            if(s.Xp < slot.Tier.RequiredXp || (slot.Premium && !access))return BattlePassStatus.Locked;
            return BattlePassStatus.Success;
        }
        public BattlePassClaimOffer GetClaimOffer(string occurrenceId,string slotId)
        {
            var s=data.Seasons.Find(x=>x.Snapshot.OccurrenceId == occurrenceId);if(s == null)return null;
            var slot=Slots(s).FirstOrDefault(x=>x.Id == slotId);if(slot.Reward == null)return null;
            var bundle=slot.Reward.ToBundle();
            return new BattlePassClaimOffer{OccurrenceId=occurrenceId,SlotId=slotId,Bundle=bundle,Fingerprint=Fingerprint(s,slotId,bundle),Status=Eligibility(s,slotId,premium.HasAccess(s.Snapshot.PremiumEntitlementId),Now)};
        }
    }
}
