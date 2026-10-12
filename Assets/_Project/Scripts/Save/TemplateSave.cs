using System;
using Dreamy.Datasave;
using Newtonsoft.Json;

namespace Dreamy.Template
{
    [Serializable]
    public sealed class TemplateSave : SaveData
    {
        public override int Version => 3;

        public int LaunchCount;
        public int Coins;
        public int Score;
        public int BestScore;
        public bool IsInitialized;

        private int? legacyDemoOpenCount;
        private int? legacyCurrentScore;

        // Version 2 used different names. Read them without writing them back.
        [JsonProperty("DemoOpenCount")]
        private int LegacyDemoOpenCount { set => legacyDemoOpenCount = value; }

        [JsonProperty("CurrentScore")]
        private int LegacyCurrentScore { set => legacyCurrentScore = value; }

        public override void Migrate(int fromVersion)
        {
            if (fromVersion == 2)
            {
                if (legacyDemoOpenCount.HasValue) LaunchCount = legacyDemoOpenCount.Value;
                if (legacyCurrentScore.HasValue) Score = legacyCurrentScore.Value;
            }
        }
    }
}
