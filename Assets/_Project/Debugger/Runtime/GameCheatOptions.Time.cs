using System.ComponentModel;
using Dreamy.Debugger.Presentation;
using UnityEngine;

namespace Dreamy.Template.Debugging
{
    public sealed partial class GameCheatOptions
    {
        [Category("05 Gameplay"), DisplayName("Time scale")]
        [NumberRange(0.1, 3), Increment(0.1), Sort(0)]
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        // Harmless actions run immediately. Use [CheatConfirmation] for destructive actions;
        // resource/ownership card grants always require confirmation.
        [Category("05 Gameplay"), DisplayName("Reset time scale"), Sort(1)]
        public void ResetTimeScale() => Time.timeScale = 1;
    }
}
