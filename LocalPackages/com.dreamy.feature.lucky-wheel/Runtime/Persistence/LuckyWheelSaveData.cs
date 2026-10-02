using Dreamy.Datasave;
using Newtonsoft.Json;

namespace Dreamy.LuckyWheel
{
    public sealed class LuckyWheelSaveData : SaveData
    {
        [JsonProperty("wheelId")]
        public string WheelId { get; set; }

        [JsonProperty("lastSpinUtcTicks")]
        public long LastSpinUtcTicks { get; set; }

        [JsonProperty("lastObservedUtcTicks")]
        public long LastObservedUtcTicks { get; set; }

        [JsonProperty("nextSpinSequence")]
        public long NextSpinSequence { get; set; }

        [JsonProperty("pendingSpin")]
        public PendingLuckyWheelSpin PendingSpin { get; set; }

        public override int Version => 1;
    }

    public sealed class PendingLuckyWheelSpin
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("wheelId")]
        public string WheelId { get; set; }

        [JsonProperty("configRevision")]
        public int ConfigRevision { get; set; }

        [JsonProperty("segmentId")]
        public string SegmentId { get; set; }

        [JsonProperty("segmentIndex")]
        public int SegmentIndex { get; set; }

        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("selectedAtUtcTicks")]
        public long SelectedAtUtcTicks { get; set; }

        [JsonProperty("status")]
        public PendingLuckyWheelSpinStatus Status { get; set; }
    }

    public enum PendingLuckyWheelSpinStatus
    {
        PendingGrant,
        GrantedAwaitingReveal
    }
}
