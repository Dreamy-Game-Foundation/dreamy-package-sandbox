
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
namespace Dreamy.BattlePass.Sample.Editor
{
    public static class BattlePassSmoke
    {
        public static void Start(int cycles=100)=>Run(cycles).Forget(e=>{SessionState.SetString("BP.Smoke",e.ToString());Debug.LogException(e);});
        private static int Listeners(object owner,string field)
        { var d=owner.GetType().GetField(field,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.GetValue(owner) as Delegate;return d?.GetInvocationList().Length ?? 0; }
        private static async UniTask Run(int cycles)
        {
            SessionState.SetString("BP.Smoke","running");var demo=UnityEngine.Object.FindFirstObjectByType<BattlePassDemo>();if(demo?.Root?.Model==null)throw new Exception("Play initialized sample first.");
            var panel=demo.Panel;await panel.Hide();int baseline=Listeners(demo.Root.Model,"StateChanged");
            for(int i=0;i<cycles;i++)
            {
                await panel.Show();await UniTask.NextFrame();
                if(panel.RowCount != 10 || Listeners(panel,"ClaimRequested") != 1 || Listeners(demo.Root.Model,"StateChanged") != baseline+1)throw new Exception("Open lifecycle leak at "+i);
                await panel.Hide();await UniTask.NextFrame();
                if(Listeners(panel,"ClaimRequested") != 0 || Listeners(demo.Root.Model,"StateChanged") != baseline)throw new Exception("Close lifecycle leak at "+i);
            }
            SessionState.SetString("BP.Smoke","PASS "+cycles+" cycles; rows=10; view listeners=0 closed/1 open; service baseline="+baseline);
        }
    }
}
