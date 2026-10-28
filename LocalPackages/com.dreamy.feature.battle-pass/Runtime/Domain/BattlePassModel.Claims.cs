using System;
using System.Linq;
using Dreamy.Economy.Rewards;
namespace Dreamy.BattlePass
{
    public sealed partial class BattlePassModel
    {
        public BattlePassResult Claim(string occurrenceId,string slotId,string expectedFingerprint=null) => Mutate(()=>
        {
            var s=data.Seasons.Find(x=>x.Snapshot.OccurrenceId == occurrenceId);if(s == null)return new(BattlePassStatus.NotEnrolled);
            var offer=GetClaimOffer(occurrenceId,slotId);if(offer == null)return new(BattlePassStatus.Unavailable);
            if(expectedFingerprint != null && expectedFingerprint != offer.Fingerprint)return new(BattlePassStatus.PayloadMismatch);
            if(offer.Status == BattlePassStatus.AlreadyApplied)return new(BattlePassStatus.AlreadyApplied,bundle:offer.Bundle);
            if(s.Batch != null)return new(BattlePassStatus.Pending,"Use RecoverPending for the frozen batch.");
            if(Pending && !s.Claims.ContainsKey(slotId))return new(BattlePassStatus.Pending,"Another claim is pending.");
            if(offer.Status != BattlePassStatus.Success && offer.Status != BattlePassStatus.Pending)return new(offer.Status);
            return Deliver(s.Snapshot.OccurrenceId,slotId);
        });
        private BattlePassResult Deliver(string occurrenceId,string slotId)
        {
            var s=data.Seasons.Find(x=>x.Snapshot.OccurrenceId == occurrenceId);
            var slot=Slots(s).First(x=>x.Id == slotId);var bundle=slot.Reward.ToBundle();string fingerprint=Fingerprint(s,slotId,bundle);
            s.Claims.TryGetValue(slotId,out var journal);var coordinator=new RewardDeliveryCoordinator(gateway,journal);
            Action<RewardJournal> persist=j=>
            { var next=data.Clone();next.Seasons.Find(x=>x.Snapshot.OccurrenceId == occurrenceId).Claims[slotId]=j.Clone();Commit(next); };
            var result=coordinator.Begin(RequestId(s,slotId),fingerprint,persist);
            if(result.IsSuccess)result=coordinator.Resolve(journal != null && journal.State != RewardJournalState.IntentPersisted ? journal.GetResolvedBundle() : bundle,persist);
            if(result.IsSuccess)result=coordinator.Deliver(persist);
            if(result.IsSuccess)result=coordinator.Complete(j=>
            {
                var next=data.Clone();var updated=next.Seasons.Find(x=>x.Snapshot.OccurrenceId == occurrenceId);updated.Claims[slotId]=j.Clone();
                if(updated.Batch != null)
                {
                    if(updated.Batch.Slots[updated.Batch.Cursor] != slotId)throw new InvalidOperationException("Batch cursor mismatch.");
                    updated.Batch.Cursor++;if(updated.Batch.Cursor == updated.Batch.Slots.Count)updated.Batch=null;
                }
                Commit(next);
            });
            if(result.Reason == RewardDeliveryReason.PersistenceFailed)requiresReload=true;
            return new(result.IsSuccess ? BattlePassStatus.Success : requiresReload ? BattlePassStatus.RequiresReload : BattlePassStatus.Pending,result.Reason.ToString(),bundle);
        }
        public BattlePassResult ClaimAll() => Mutate(()=>
        {
            if(Pending)return RecoverCore();var s=Active;if(s == null)return new(BattlePassStatus.NotEnrolled);
            var ids=Slots(s).Where(slot=>Eligibility(s,slot.Id,premium.HasAccess(s.Snapshot.PremiumEntitlementId),Now) == BattlePassStatus.Success).Select(slot=>slot.Id).ToList();
            if(ids.Count == 0)return new(Now >= s.ClaimDeadlineUtcTicks ? BattlePassStatus.Expired : BattlePassStatus.Locked);
            var next=data.Clone();next.Seasons.Find(x=>x.Snapshot.OccurrenceId == s.Snapshot.OccurrenceId).Batch=new BattlePassClaimBatch{CreatedUtcTicks=Now,Slots=ids};Commit(next);
            return RecoverCore();
        });
        public BattlePassResult RecoverPending() => Mutate(RecoverCore,false);
        private BattlePassResult RecoverCore()
        {
            foreach(var original in data.Seasons.ToArray())
            {
                string id=original.Snapshot.OccurrenceId;
                var pending=original.Claims.FirstOrDefault(p=>p.Value.State != RewardJournalState.Completed);
                if(pending.Value != null) { var result=Deliver(id,pending.Key);if(!result.IsSuccess)return result; }
                while(true)
                {
                    var season=data.Seasons.Find(s=>s.Snapshot.OccurrenceId == id);if(season.Batch == null)break;
                    var result=Deliver(id,season.Batch.Slots[season.Batch.Cursor]);if(!result.IsSuccess)return result;
                }
            }
            return new(BattlePassStatus.Success);
        }
    }
}
