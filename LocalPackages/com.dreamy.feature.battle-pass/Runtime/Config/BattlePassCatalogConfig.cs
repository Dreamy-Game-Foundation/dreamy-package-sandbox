using System;
using System.Collections.Generic;
using Dreamy.DataConfig;
using Dreamy.Economy;
using Dreamy.Economy.Rewards;
using Newtonsoft.Json;
namespace Dreamy.BattlePass
{
    public sealed class BattlePassCatalogConfig : ConfigBase
    {
        [JsonProperty("seasons", Required=Required.Always)] public List<BattlePassSeasonDefinition> Seasons { get; private set; } = new();
        public override void Initialize(string documentName)
        {
            if(SchemaVersion != 1 || Seasons == null || Seasons.Count == 0) throw new ArgumentException("Unsupported/empty Battle Pass catalog.");
            var ids=new HashSet<string>(StringComparer.Ordinal);var entitlements=new HashSet<string>(StringComparer.Ordinal);
            foreach(var season in Seasons)
            { if(season == null || !ids.Add(season.OccurrenceId)) throw new ArgumentException("Duplicate occurrence.");season.Validate();if(!entitlements.Add(season.PremiumEntitlementId))throw new ArgumentException("Premium entitlements must be unique per occurrence."); }
        }
    }
    public enum BattlePassSeasonMode { EnrollmentDuration, FixedUtcWindow }
    public sealed class BattlePassSeasonDefinition
    {
        public string SeasonId;
        public string OccurrenceId;
        public string Revision;
        public string Title;
        public string PremiumEntitlementId;
        public BattlePassSeasonMode Mode;
        public int DurationDays=14;
        public int GraceHours=48;
        public long StartUtcTicks;
        public long EndUtcTicks;
        public List<BattlePassTierDefinition> Tiers=new();
        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(SeasonId) || string.IsNullOrWhiteSpace(OccurrenceId) || string.IsNullOrWhiteSpace(Revision) ||
                !ResourceId.TryParse(PremiumEntitlementId,out _) || !Enum.IsDefined(typeof(BattlePassSeasonMode),Mode) ||
                DurationDays < 1 || DurationDays > 366 || GraceHours < 0 || GraceHours > 8760 || Tiers == null || Tiers.Count == 0)
                throw new ArgumentException("Invalid season identity, timing or tiers.");
            if(Mode == BattlePassSeasonMode.FixedUtcWindow && (StartUtcTicks <= 0 || EndUtcTicks <= StartUtcTicks || EndUtcTicks > DateTime.MaxValue.Ticks-TimeSpan.FromHours(GraceHours).Ticks))
                throw new ArgumentException("Invalid fixed UTC window.");
            long previous=0;var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var tier in Tiers)
            {
                if(tier == null || string.IsNullOrWhiteSpace(tier.Id) || !ids.Add(tier.Id) || tier.RequiredXp <= previous)
                    throw new ArgumentException("Unique tier IDs and strictly increasing positive XP thresholds required.");
                previous=tier.RequiredXp;tier.Free?.ToBundle();tier.Premium?.ToBundle();
                if(tier.Free == null && tier.Premium == null)throw new ArgumentException("Tier needs a reward.");
            }
        }
        public BattlePassSeasonDefinition Clone() => JsonConvert.DeserializeObject<BattlePassSeasonDefinition>(JsonConvert.SerializeObject(this));
    }
    public sealed class BattlePassTierDefinition
    {
        public string Id;
        public long RequiredXp;
        public BattlePassRewardBundleConfig Free;
        public BattlePassRewardBundleConfig Premium;
    }
    public sealed class BattlePassRewardBundleConfig
    {
        public List<BattlePassRewardEntryConfig> Entries=new();
        public ResolvedRewardBundle ToBundle()
        {
            if(Entries == null)throw new ArgumentException("Missing reward entries.");
            var entries=new List<ResolvedRewardEntry>();
            foreach(var e in Entries)
            { if(e == null || !ResourceId.TryParse(e.TargetId,out _))throw new ArgumentException("Invalid reward target.");entries.Add(new ResolvedRewardEntry(e.Id,e.KindId,e.TargetId,e.Amount)); }
            return new ResolvedRewardBundle(1,"battle-pass-fixed-v1",entries);
        }
    }
    public sealed class BattlePassRewardEntryConfig
    { public string Id;public string KindId;public string TargetId;public long Amount; }
}
