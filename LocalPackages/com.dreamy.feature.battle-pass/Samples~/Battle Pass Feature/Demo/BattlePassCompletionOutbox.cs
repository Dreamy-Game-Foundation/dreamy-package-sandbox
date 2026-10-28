using System;
using System.Collections.Generic;
using System.Linq;
using Dreamy.Datasave;
namespace Dreamy.BattlePass.Sample
{
    public sealed class BattlePassCompletionOutboxSave : SaveData
    { public List<BattlePassCompletion> Completions=new(); }
    public sealed class BattlePassCompletion
    { public string OccurrenceId;public string RunId;public long Xp;public bool Acknowledged; }
    // Host-owned example. In a real game, write completion + each consumer's outbox
    // into the SAME gameplay save candidate before showing the win screen.
    public sealed class BattlePassCompletionOutbox
    {
        private readonly IDatasaveService save;private BattlePassCompletionOutboxSave data;
        public BattlePassCompletionOutbox(IDatasaveService save){this.save=save;data=save.Load<BattlePassCompletionOutboxSave>("demo-runs");}
        public string LatestRunId=>data.Completions.LastOrDefault()?.RunId;
        public void Record(string occurrence,string run,long xp)
        {
            var old=data.Completions.Find(x=>x.RunId == run);
            if(old != null){if(old.OccurrenceId != occurrence || old.Xp != xp)throw new InvalidOperationException("Run payload mismatch.");return;}
            var next=Clone();next.Completions.Add(new BattlePassCompletion{OccurrenceId=occurrence,RunId=run,Xp=xp});Commit(next);
        }
        public BattlePassResult Drain(IBattlePassService service)
        {
            foreach(var item in data.Completions.Where(x=>!x.Acknowledged).ToArray())
            {
                var result=service.ReportProgress(new(item.OccurrenceId,item.RunId,item.Xp));if(!result.IsSuccess)return result;
                var next=Clone();next.Completions.Find(x=>x.RunId == item.RunId).Acknowledged=true;Commit(next);
            }
            return new(BattlePassStatus.Success);
        }
        private BattlePassCompletionOutboxSave Clone()=>Newtonsoft.Json.JsonConvert.DeserializeObject<BattlePassCompletionOutboxSave>(Newtonsoft.Json.JsonConvert.SerializeObject(data));
        private void Commit(BattlePassCompletionOutboxSave next){save.Save(next,"demo-runs");data=next;}
    }
}
