using System.Collections.Generic;
using Dreamy.Economy.Rewards;
namespace Dreamy.BattlePass
{
    public enum BattlePassPhase { NotEnrolled, Active, Grace, Expired }
    public sealed class BattlePassViewState
    {
        public string OccurrenceId { get; internal set; }
        public string Title { get; internal set; }
        public long Xp { get; internal set; }
        public long MaxXp { get; internal set; }
        public long EndUtcTicks { get; internal set; }
        public long ClaimDeadlineUtcTicks { get; internal set; }
        public long NowUtcTicks { get; internal set; }
        public BattlePassPhase Phase { get; internal set; }
        public bool IsPremium { get; internal set; }
        public bool HasPending { get; internal set; }
        public bool RequiresReload { get; internal set; }
        public int ClaimableCount { get; internal set; }
        public IReadOnlyList<BattlePassSlotViewState> Slots { get; internal set; } = new List<BattlePassSlotViewState>().AsReadOnly();
    }
    public sealed class BattlePassSlotViewState
    {
        public string Id { get; internal set; }
        public string TierId { get; internal set; }
        public bool Premium { get; internal set; }
        public long RequiredXp { get; internal set; }
        public BattlePassStatus Status { get; internal set; }
        public ResolvedRewardBundle Bundle { get; internal set; }
    }
}
