using System.ComponentModel;
using Dreamy.Debugger.Options;
using Dreamy.Debugger.Presentation;
using UnityEngine;

namespace MyGame.Debugging
{
    public sealed partial class GameCheatOptions
    {
        [Category("Gameplay"), DisplayName("Time scale"), CheatId("mygame.time-scale")]
        [NumberRange(0.1, 3), Increment(0.1), Sort(0)]
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        [Category("Gameplay"), DisplayName("Reset time scale"), Sort(1)]
        public void ResetTimeScale() => Time.timeScale = 1;
    }
}
