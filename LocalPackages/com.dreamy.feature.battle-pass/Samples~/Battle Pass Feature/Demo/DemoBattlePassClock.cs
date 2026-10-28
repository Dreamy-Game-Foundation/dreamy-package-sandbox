using System;
using Dreamy.Datasave;
namespace Dreamy.BattlePass.Sample
{
    public sealed class DemoBattlePassClockSave : SaveData { public long OffsetTicks; }
    public sealed class DemoBattlePassClock : IBattlePassClock
    {
        private readonly IDatasaveService save;private DemoBattlePassClockSave data;
        public DemoBattlePassClock(IDatasaveService save){this.save=save;data=save.Load<DemoBattlePassClockSave>("demo-clock");}
        public DateTime UtcNow=>DateTime.UtcNow.AddTicks(data.OffsetTicks);
        public void AdvanceDay(){var next=new DemoBattlePassClockSave{OffsetTicks=checked(data.OffsetTicks+TimeSpan.TicksPerDay)};save.Save(next,"demo-clock");data=next;}
    }
}
