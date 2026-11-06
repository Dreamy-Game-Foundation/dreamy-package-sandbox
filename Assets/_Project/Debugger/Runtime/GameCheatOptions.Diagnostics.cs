using System.ComponentModel;
using Dreamy.Debugger.Options;

namespace Dreamy.Template.Debugging
{
    public sealed partial class GameCheatOptions
    {
        [Category("04 Diagnostics"), DisplayName("Last cheat result"), CheatId("sandbox.status")]
        public string LastResult { get; private set; } = "Ready. Grants save through the game wallet.";

        [Category("04 Diagnostics"), DisplayName("Ads integration"), CheatId("sandbox.ads-adapter")]
        public string AdsIntegration => "Entitlement is persisted; this sandbox has a demo effect, no ads SDK.";
    }
}
