using Dreamy.Economy;

namespace Dreamy.LuckyWheel
{
    public readonly struct LuckyWheelSpinResult
    {
        private LuckyWheelSpinResult(
            LuckyWheelSpinStatus status,
            string transactionId,
            string segmentId,
            int segmentIndex,
            ResourceAmount? reward)
        {
            Status = status;
            TransactionId = transactionId;
            SegmentId = segmentId;
            SegmentIndex = segmentIndex;
            Reward = reward;
        }

        public LuckyWheelSpinStatus Status { get; }
        public string TransactionId { get; }
        public string SegmentId { get; }
        public int SegmentIndex { get; }
        public ResourceAmount? Reward { get; }
        public bool HasOutcome => Reward.HasValue && !string.IsNullOrEmpty(TransactionId);

        public static LuckyWheelSpinResult Succeeded(
            string transactionId,
            string segmentId,
            int segmentIndex,
            ResourceAmount reward) =>
            new(LuckyWheelSpinStatus.Succeeded, transactionId, segmentId, segmentIndex, reward);

        public static LuckyWheelSpinResult AwaitingReveal(
            string transactionId,
            string segmentId,
            int segmentIndex,
            ResourceAmount reward) =>
            new(LuckyWheelSpinStatus.AwaitingReveal, transactionId, segmentId, segmentIndex, reward);

        public static LuckyWheelSpinResult Failed(LuckyWheelSpinStatus status) =>
            new(status, null, null, -1, null);
    }

    public enum LuckyWheelSpinStatus
    {
        Succeeded,
        Cooldown,
        GrantFailed,
        NoPendingGrant,
        AwaitingReveal,
        InvalidReveal
    }
}
