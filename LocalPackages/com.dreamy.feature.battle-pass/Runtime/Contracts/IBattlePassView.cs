using System;
using Dreamy.Feature;
namespace Dreamy.BattlePass
{
    public interface IBattlePassView : IFeatureView<BattlePassViewState>
    {
        event Action<string> ClaimRequested;
        event Action ClaimAllRequested;
        event Action RecoverRequested;
        event Action PremiumRequested;
        void ShowResult(BattlePassResult result);
    }
}
