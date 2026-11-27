using System.ComponentModel;
using ThunderPunch.Debugger.Options;
using ThunderPunch.Debugger.Presentation;

namespace ThunderPunch.Debugger.Sample
{
    public partial class GameDebugOptions
    {
        private const string CoinsCard = "game.coins";

        [Category("Economy"), ResourceCheat(CoinsCard, "Coins", CheatCardRole.Balance), CheatId("game.Coins")]
        public long Coins => readCoins();

        [Category("Economy"), ResourceCheat(CoinsCard, "Coins", CheatCardRole.Amount), CheatId("game.GrantAmount")]
        [NumberRange(1, 1000000), Increment(100)]
        public long GrantAmount { get; set; } = 100;

        [Category("Economy"), ResourceCheat(CoinsCard, "Coins", CheatCardRole.Action), CheatId("game.AddCoins")]
        public void AddCoins() => grantCoins(GrantAmount);

        [Browsable(false)] public string HiddenDiagnostic => "Not rendered";
    }
}
