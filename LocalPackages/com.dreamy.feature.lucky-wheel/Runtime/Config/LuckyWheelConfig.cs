using System;
using System.Collections.Generic;
using Dreamy.DataConfig;
using Dreamy.Economy;
using Newtonsoft.Json;

namespace Dreamy.LuckyWheel
{
    public sealed class LuckyWheelConfig : ConfigBase
    {
        [JsonProperty("wheelId", Required = Required.Always)]
        private string wheelId;

        [JsonProperty("revision", Required = Required.Always)]
        private int revision;

        [JsonProperty("cooldownSeconds", Required = Required.Always)]
        private int cooldownSeconds;

        [JsonProperty("segments", Required = Required.Always)]
        private List<LuckyWheelSegment> segments = new();

        [JsonIgnore]
        public string WheelId => wheelId;

        [JsonIgnore]
        public int Revision => revision;

        [JsonIgnore]
        public int CooldownSeconds => cooldownSeconds;

        [JsonIgnore]
        public IReadOnlyList<LuckyWheelSegment> Segments => segments;

        [JsonIgnore]
        public int TotalWeight { get; private set; }

        public override void Initialize(string documentName)
        {
            if (string.IsNullOrWhiteSpace(wheelId))
            {
                throw new DataConfigException(documentName, "wheelId cannot be empty.");
            }

            if (revision <= 0)
            {
                throw new DataConfigException(documentName, "revision must be greater than zero.");
            }

            if (cooldownSeconds < 0)
            {
                throw new DataConfigException(documentName, "cooldownSeconds cannot be negative.");
            }

            if (segments == null || segments.Count < 2)
            {
                throw new DataConfigException(documentName, "segments must contain at least two entries.");
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            long totalWeight = 0;
            for (int index = 0; index < segments.Count; index++)
            {
                LuckyWheelSegment segment = segments[index];
                if (segment == null)
                {
                    throw new DataConfigException(documentName, $"Segment at index {index} is null.");
                }

                segment.Validate(documentName, index, ids);
                totalWeight += segment.Weight;
                if (totalWeight > int.MaxValue)
                {
                    throw new DataConfigException(documentName, "Total segment weight exceeds Int32.MaxValue.");
                }
            }

            TotalWeight = (int)totalWeight;
        }
    }

    [Serializable]
    public sealed class LuckyWheelSegment
    {
        [JsonProperty("id", Required = Required.Always)]
        private string id;

        [JsonProperty("weight", Required = Required.Always)]
        private int weight;

        [JsonProperty("resourceId", Required = Required.Always)]
        private string resourceId;

        [JsonProperty("amount", Required = Required.Always)]
        private long amount;

        [JsonIgnore]
        public string Id => id;

        [JsonIgnore]
        public int Weight => weight;

        [JsonIgnore]
        public ResourceAmount Reward => new(new ResourceId(resourceId), amount);

        internal void Validate(string documentName, int index, ISet<string> ids)
        {
            if (string.IsNullOrWhiteSpace(id) || !ids.Add(id))
            {
                throw new DataConfigException(documentName, $"Segment at index {index} has a duplicate or empty id.");
            }

            if (weight <= 0)
            {
                throw new DataConfigException(documentName, $"Segment '{id}' must have positive weight.");
            }

            if (!ResourceId.TryParse(resourceId, out _) || amount <= 0)
            {
                throw new DataConfigException(documentName, $"Segment '{id}' has invalid reward data.");
            }
        }
    }
}
