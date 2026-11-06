using System.Collections.Generic;
using Dreamy.Debugger;
using Dreamy.Debugger.Options;

namespace MyGame.Debugging
{
    public sealed partial class GameCheatOptions
    {
        public IEnumerable<CheatOption> CreateOptions() =>
            AttributedCheatOptions.From(this, "mygame");
    }
}
