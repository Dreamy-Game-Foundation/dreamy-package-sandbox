using System;
using System.Collections.Generic;
using System.Linq;
using Dreamy.Economy.Rewards;
namespace Dreamy.BattlePass
{
    public sealed partial class BattlePassModel
    {
        private void ValidateSave()
        {
            if(data.Schema != 1 || data.Seasons == null || data.LastObservedUtcTicks < 0 || data.LastObservedUtcTicks > DateTime.MaxValue.Ticks)throw new InvalidOperationException("Unsupported/corrupt Battle Pass save; preserve it.");
            var ids=new HashSet<string>(StringComparer.Ordinal);int pending=0,batches=0;
            foreach(var s in data.Seasons)
            {
                if(s?.Snapshot == null || !ids.Add(s.Snapshot.OccurrenceId))throw new InvalidOperationException("Invalid occurrence snapshot.");s.Snapshot.Validate();
                if(s.EnrolledUtcTicks <= 0 || s.EndUtcTicks <= s.EnrolledUtcTicks || s.ClaimDeadlineUtcTicks < s.EndUtcTicks || s.ClaimDeadlineUtcTicks > DateTime.MaxValue.Ticks ||
                    s.Xp < 0 || s.Xp > s.Snapshot.Tiers.Last().RequiredXp || s.ProgressReceipts == null || s.Claims == null || data.LastObservedUtcTicks < s.EnrolledUtcTicks)
                    throw new InvalidOperationException("Invalid season state.");
                foreach(var receipt in s.ProgressReceipts)if(string.IsNullOrWhiteSpace(receipt.Key) || string.IsNullOrWhiteSpace(receipt.Value))throw new InvalidOperationException("Invalid progress receipt.");
                foreach(var pair in s.Claims)
                {
                    var slot=Slots(s).FirstOrDefault(x=>x.Id == pair.Key);var j=pair.Value;
                    if(slot.Reward == null || j == null || !j.IsValid() || j.RequestId != RequestId(s,pair.Key) || j.RequestFingerprint != Fingerprint(s,pair.Key,slot.Reward.ToBundle()) ||
                        (j.State != RewardJournalState.IntentPersisted && j.ResolvedFingerprint != slot.Reward.ToBundle().Fingerprint))throw new InvalidOperationException("Invalid claim journal context.");
                    if(j.State != RewardJournalState.Completed)pending++;
                }
                var b=s.Batch;if(b == null)continue;batches++;
                if(b.Slots == null || b.Slots.Count == 0 || b.Cursor < 0 || b.Cursor >= b.Slots.Count || b.Slots.Distinct().Count() != b.Slots.Count ||
                    b.CreatedUtcTicks < s.EnrolledUtcTicks || b.CreatedUtcTicks >= s.ClaimDeadlineUtcTicks || b.CreatedUtcTicks > data.LastObservedUtcTicks)
                    throw new InvalidOperationException("Invalid frozen batch.");
                for(int i=0;i<b.Slots.Count;i++)
                {
                    var slot=Slots(s).FirstOrDefault(x=>x.Id == b.Slots[i]);
                    if(slot.Reward == null || slot.Tier.RequiredXp > s.Xp)throw new InvalidOperationException("Invalid batch slot.");
                    bool completed=s.Claims.TryGetValue(b.Slots[i],out var j) && j.State == RewardJournalState.Completed;
                    if(completed != (i < b.Cursor))throw new InvalidOperationException("Invalid batch cursor.");
                }
                foreach(var pair in s.Claims.Where(p=>p.Value.State != RewardJournalState.Completed))
                    if(pair.Key != b.Slots[b.Cursor])throw new InvalidOperationException("Pending claim differs from batch cursor.");
            }
            if(pending > 1 || batches > 1 || (data.Seasons.Count > 0 && data.ActiveOccurrenceId == null) || (data.ActiveOccurrenceId != null && !ids.Contains(data.ActiveOccurrenceId)) ||
                (batches > 0 && data.Seasons.Any(s=>s.Batch == null && s.Claims.Values.Any(j=>j.State != RewardJournalState.Completed))))throw new InvalidOperationException("Invalid active/pending ownership.");
        }
    }
}
