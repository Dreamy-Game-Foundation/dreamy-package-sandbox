using System.ComponentModel;
using Dreamy.Debugger.Options;
using Dreamy.Debugger.Presentation;
using Dreamy.Economy;

namespace Dreamy.Template.Debugging
{
    public sealed partial class GameCheatOptions
    {
        private const string CoinsCheatId = "sandbox.currency.coin";

        [Category("01 Economy"), ResourceCheat(CoinsCheatId, "Coins", CheatCardRole.Balance)]
        public long CoinsBalance => Balance("currency.coin");

        [Category("01 Economy"), ResourceCheat(CoinsCheatId, "Coins", CheatCardRole.Amount)]
        [NumberRange(1, 1000000), Increment(1)]
        public long CoinsGrantAmount { get; set; } = 1000;

        [Category("01 Economy"), ResourceCheat(CoinsCheatId, "Coins", CheatCardRole.Action)]
        public void AddCoins() => Grant(new ResourceId("currency.coin"), CoinsGrantAmount);

        private const string GemsCheatId = "sandbox.currency.gem";

        [Category("01 Economy"), ResourceCheat(GemsCheatId, "Gems", CheatCardRole.Balance)]
        public long GemsBalance => Balance("currency.gem");

        [Category("01 Economy"), ResourceCheat(GemsCheatId, "Gems", CheatCardRole.Amount)]
        [NumberRange(1, 1000000), Increment(1)]
        public long GemsGrantAmount { get; set; } = 100;

        [Category("01 Economy"), ResourceCheat(GemsCheatId, "Gems", CheatCardRole.Action)]
        public void AddGems() => Grant(new ResourceId("currency.gem"), GemsGrantAmount);
    }
}
