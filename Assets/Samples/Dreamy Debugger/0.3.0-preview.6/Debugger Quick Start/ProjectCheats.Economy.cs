using System;
using System.ComponentModel;
using ThunderPunch.Debugger.Options;
using ThunderPunch.Debugger.Presentation;

namespace ThunderPunch.Debugger.Sample
{
    public sealed partial class ProjectCheats
    {
        private int coins = 0;
        [Category("Economy"), CheatId("economy.coins"), DisplayName("Coins"), NumberRange(0, 1000000)]
        public int Coins { get => coins; set => SetValue(ref coins, value); }

        [Category("Economy"), CheatId("economy.grant"), DisplayName("Grant 100 coins")]
        [CheatConfirmation]
        public void GrantCoins() => Coins = Math.Min(1000000, Coins + 100);
    }
}
