using System.ComponentModel;
using Dreamy.Debugger.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamy.Template.Debugging
{
    public enum FrameRatePreset
    {
        PlatformDefault = -1,
        Fps30 = 30,
        Fps60 = 60,
        Fps90 = 90,
        Fps120 = 120
    }

    public sealed partial class GameCheatOptions
    {
        [Category("06 Application"), DisplayName("Target frame rate"), Sort(0)]
        public FrameRatePreset TargetFrameRate
        {
            get => (FrameRatePreset)Application.targetFrameRate;
            set => Application.targetFrameRate = (int)value;
        }

        [Category("06 Application"), DisplayName("VSync count (read only)"), Sort(1)]
        public int VSyncCount => QualitySettings.vSyncCount;

        [Category("06 Application"), DisplayName("Active scene"), Sort(2)]
        public string ActiveScene => SceneManager.GetActiveScene().name;
    }
}
