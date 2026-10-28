using System;
using Dreamy.Economy.Rewards;
namespace Dreamy.BattlePass
{
    public interface IBattlePassClock { DateTime UtcNow { get; } }
    public sealed class SystemBattlePassClock : IBattlePassClock { public DateTime UtcNow => DateTime.UtcNow; }
    public interface IBattlePassPremiumAccess { bool HasAccess(string entitlementId); }
    public interface IBattlePassService
    {
        event Action StateChanged;
        BattlePassViewState GetState();
        BattlePassResult Enroll(string occurrenceId);
        BattlePassResult ReportProgress(BattlePassProgressRequest request);
        BattlePassResult Claim(string occurrenceId,string slotId,string expectedFingerprint=null);
        BattlePassResult ClaimAll();
        BattlePassResult RecoverPending();
        BattlePassResult Refresh();
        BattlePassClaimOffer GetClaimOffer(string occurrenceId,string slotId);
    }
    public enum BattlePassStatus { Success, AlreadyApplied, NotEnrolled, Locked, Expired, Busy, Pending, RequiresReload, InvalidRequest, PayloadMismatch, Unavailable }
    public readonly struct BattlePassResult
    {
        public BattlePassResult(BattlePassStatus status,string reason=null,ResolvedRewardBundle bundle=null) { Status=status;Reason=reason;Bundle=bundle; }
        public BattlePassStatus Status { get; }
        public string Reason { get; }
        public ResolvedRewardBundle Bundle { get; }
        public bool IsSuccess => Status == BattlePassStatus.Success || Status == BattlePassStatus.AlreadyApplied;
    }
    public sealed class BattlePassProgressRequest
    {
        public BattlePassProgressRequest(string occurrenceId,string eventId,long xp)
        { OccurrenceId=occurrenceId;EventId=eventId;Xp=xp; }
        public string OccurrenceId { get; }
        public string EventId { get; }
        public long Xp { get; }
    }
    public sealed class BattlePassClaimOffer
    {
        public string OccurrenceId { get; internal set; }
        public string SlotId { get; internal set; }
        public string Fingerprint { get; internal set; }
        public ResolvedRewardBundle Bundle { get; internal set; }
        public BattlePassStatus Status { get; internal set; }
    }
}
