using System;
using System.Collections.Generic;
using Dreamy.Datasave;
using Dreamy.Economy.Rewards;
using Newtonsoft.Json;
namespace Dreamy.BattlePass
{
    [Serializable] public sealed class BattlePassSaveData : SaveData
    {
        public int Schema=1;
        public string ActiveOccurrenceId;
        public long LastObservedUtcTicks;
        public List<BattlePassSeasonSave> Seasons=new();
        public BattlePassSaveData Clone() => JsonConvert.DeserializeObject<BattlePassSaveData>(JsonConvert.SerializeObject(this));
        public override void Migrate(int fromVersion) { throw new InvalidOperationException("Unsupported Battle Pass save version; preserve file for migration."); }
    }
    [Serializable] public sealed class BattlePassSeasonSave
    {
        public BattlePassSeasonDefinition Snapshot;
        public long EnrolledUtcTicks;
        public long EndUtcTicks;
        public long ClaimDeadlineUtcTicks;
        public long Xp;
        public Dictionary<string,string> ProgressReceipts=new(StringComparer.Ordinal);
        public Dictionary<string,RewardJournal> Claims=new(StringComparer.Ordinal);
        public BattlePassClaimBatch Batch;
    }
    [Serializable] public sealed class BattlePassClaimBatch
    {
        public long CreatedUtcTicks;
        public List<string> Slots=new();
        public int Cursor;
    }
}
