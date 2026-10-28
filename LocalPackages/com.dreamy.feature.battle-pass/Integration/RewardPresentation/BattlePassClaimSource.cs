using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Economy.Rewards;
using Dreamy.Economy.UI;
namespace Dreamy.BattlePass.Presentation
{
    public sealed class BattlePassClaimSource : IRewardClaimSource
    {
        private readonly IBattlePassService service;
        private readonly string scope;
        // References include an opaque occurrence+slot key. No reliance on the currently displayed season.
        public BattlePassClaimSource(string scope,IBattlePassService service) { this.scope=scope ?? throw new ArgumentNullException(nameof(scope));this.service=service ?? throw new ArgumentNullException(nameof(service)); }
        public RewardClaimReference Reference(string occurrence,string slot) => new(scope,"battle-pass",RewardIdentity.Encode(occurrence,slot));
        private BattlePassClaimOffer Find(RewardClaimReference reference)
        {
            if(reference == null || reference.Scope != scope || reference.SourceId != "battle-pass")return null;
            // Parse canonical two-field identity, then verify by re-encoding.
            string text=reference.ClaimId;const string prefix="reward-v1:";if(!text.StartsWith(prefix,StringComparison.Ordinal))return null;
            int pos=prefix.Length;string[] fields=new string[2];
            for(int i=0;i<2;i++) { int colon=text.IndexOf(':',pos);if(colon < 0 || !int.TryParse(text.Substring(pos,colon-pos),out int length) || length < 0 || length > text.Length-colon-1)return null;pos=colon+1;fields[i]=text.Substring(pos,length);pos+=length; }
            if(pos != text.Length || Reference(fields[0],fields[1]).ClaimId != text)return null;
            return service.GetClaimOffer(fields[0],fields[1]);
        }
        private static RewardClaimStatus Map(BattlePassStatus status) => status switch
        {
            BattlePassStatus.Success=>RewardClaimStatus.Available,BattlePassStatus.AlreadyApplied=>RewardClaimStatus.AlreadyClaimed,
            BattlePassStatus.Pending=>RewardClaimStatus.Pending,BattlePassStatus.RequiresReload=>RewardClaimStatus.RequiresReload,
            BattlePassStatus.Expired=>RewardClaimStatus.Expired,BattlePassStatus.Busy=>RewardClaimStatus.Busy,_=>RewardClaimStatus.Blocked
        };
        private RewardClaimSnapshot Snapshot(RewardClaimReference reference)
        { var offer=Find(reference);return offer == null ? new(reference,"missing",null,RewardClaimStatus.Unavailable) : new(reference,offer.Fingerprint,offer.Bundle,Map(offer.Status),offer.Status == BattlePassStatus.Success ? null : offer.Status.ToString(),offer.Status == BattlePassStatus.Pending); }
        public UniTask<RewardClaimSnapshot> QueryAsync(RewardClaimReference reference,CancellationToken token)
        { token.ThrowIfCancellationRequested();return UniTask.FromResult(Snapshot(reference)); }
        public UniTask<RewardClaimSnapshot> ClaimAsync(RewardClaimCommand command)
        {
            var before=Snapshot(command.Reference);if(before.Fingerprint != command.ExpectedFingerprint)return UniTask.FromResult(new RewardClaimSnapshot(command.Reference,command.ExpectedFingerprint,before.Bundle,RewardClaimStatus.Blocked,"PayloadMismatch"));
            var offer=Find(command.Reference);if(offer == null || before.IsCompleted)return UniTask.FromResult(before);
            var result=service.Claim(offer.OccurrenceId,offer.SlotId,command.ExpectedFingerprint);
            return UniTask.FromResult(new RewardClaimSnapshot(command.Reference,offer.Fingerprint,result.Bundle ?? offer.Bundle,
                result.Status == BattlePassStatus.Success ? RewardClaimStatus.Claimed : Map(result.Status),result.Reason,result.Status == BattlePassStatus.Pending));
        }
    }
}
