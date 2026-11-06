using System;
using System.Collections.Generic;
using Dreamy.Debugger;
using Dreamy.Debugger.Options;
using Dreamy.Economy;

namespace Dreamy.Template.Debugging
{
    /// <summary>One game-owned container; add options in GameCheatOptions.* partial files.</summary>
    public sealed partial class GameCheatOptions
    {
        private readonly IResourceWallet wallet;
        private readonly IResourceBalanceProvider balances;

        public GameCheatOptions(IResourceWallet wallet, IResourceBalanceProvider balances)
        {
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            this.balances = balances ?? throw new ArgumentNullException(nameof(balances));
        }

        public IEnumerable<CheatOption> CreateOptions() =>
            AttributedCheatOptions.From(this, "sandbox.game");
    }
}
