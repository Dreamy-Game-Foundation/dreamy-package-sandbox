using System;
using Dreamy.Datasave;
using Dreamy.Economy;

namespace Dreamy.LuckyWheel
{
    public sealed class LuckyWheelModel : ILuckyWheelService
    {
        private readonly LuckyWheelConfig config;
        private readonly IDatasaveService datasave;
        private readonly IResourceWallet resourceWallet;
        private readonly ILuckyWheelClock clock;
        private readonly ILuckyWheelRandom random;
        private readonly string saveKey;
        private readonly LuckyWheelSaveData save;

        public LuckyWheelModel(
            LuckyWheelConfig config,
            IDatasaveService datasave,
            IResourceWallet resourceWallet,
            ILuckyWheelClock clock,
            ILuckyWheelRandom random,
            string saveKey)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.datasave = datasave ?? throw new ArgumentNullException(nameof(datasave));
            this.resourceWallet = resourceWallet ?? throw new ArgumentNullException(nameof(resourceWallet));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.saveKey = string.IsNullOrWhiteSpace(saveKey)
                ? throw new ArgumentException("Save key cannot be empty.", nameof(saveKey))
                : saveKey;

            save = datasave.Load<LuckyWheelSaveData>(saveKey);
            ResetForChangedWheelWhenSafe();
        }

        public LuckyWheelViewState GetState()
        {
            if (save.PendingSpin != null)
            {
                if (save.PendingSpin.Status == PendingLuckyWheelSpinStatus.PendingGrant)
                {
                    return new LuckyWheelViewState(
                        config.Segments,
                        LuckyWheelAvailability.PendingGrant,
                        DateTime.MinValue,
                        null);
                }

                return new LuckyWheelViewState(
                    config.Segments,
                    LuckyWheelAvailability.AwaitingReveal,
                    DateTime.MinValue,
                    CreatePendingResult(LuckyWheelSpinStatus.AwaitingReveal));
            }

            DateTime now = GetEffectiveNow();
            DateTime nextSpinUtc = GetNextSpinUtc();
            bool canSpin = save.LastSpinUtcTicks <= 0 || now >= nextSpinUtc;
            return new LuckyWheelViewState(
                config.Segments,
                canSpin ? LuckyWheelAvailability.Ready : LuckyWheelAvailability.Cooldown,
                canSpin ? DateTime.MinValue : nextSpinUtc,
                null);
        }

        public LuckyWheelSpinResult TrySpin()
        {
            if (save.PendingSpin != null)
            {
                return save.PendingSpin.Status == PendingLuckyWheelSpinStatus.PendingGrant
                    ? CompletePendingGrant()
                    : CreatePendingResult(LuckyWheelSpinStatus.AwaitingReveal);
            }

            LuckyWheelViewState state = GetState();
            if (!state.CanSpin)
            {
                return LuckyWheelSpinResult.Failed(LuckyWheelSpinStatus.Cooldown);
            }

            int segmentIndex = SelectSegmentIndex();
            LuckyWheelSegment segment = config.Segments[segmentIndex];
            ResourceAmount reward = segment.Reward;
            DateTime selectedAtUtc = GetEffectiveNow();
            long sequence = save.NextSpinSequence;
            save.NextSpinSequence++;
            save.WheelId = config.WheelId;
            save.LastSpinUtcTicks = selectedAtUtc.Ticks;
            save.LastObservedUtcTicks = selectedAtUtc.Ticks;
            save.PendingSpin = new PendingLuckyWheelSpin
            {
                TransactionId = $"lucky-wheel:{config.WheelId}:{sequence}",
                WheelId = config.WheelId,
                ConfigRevision = config.Revision,
                SegmentId = segment.Id,
                SegmentIndex = segmentIndex,
                ResourceId = reward.ResourceId.Value,
                Amount = reward.Amount,
                SelectedAtUtcTicks = selectedAtUtc.Ticks,
                Status = PendingLuckyWheelSpinStatus.PendingGrant
            };
            Save();
            return CompletePendingGrant();
        }

        public LuckyWheelSpinResult RetryPendingGrant()
        {
            if (save.PendingSpin == null ||
                save.PendingSpin.Status != PendingLuckyWheelSpinStatus.PendingGrant)
            {
                return LuckyWheelSpinResult.Failed(LuckyWheelSpinStatus.NoPendingGrant);
            }

            return CompletePendingGrant();
        }

        public bool AcknowledgeReveal(string transactionId)
        {
            if (save.PendingSpin == null ||
                save.PendingSpin.Status != PendingLuckyWheelSpinStatus.GrantedAwaitingReveal ||
                !string.Equals(save.PendingSpin.TransactionId, transactionId, StringComparison.Ordinal))
            {
                return false;
            }

            save.PendingSpin = null;
            if (!string.Equals(save.WheelId, config.WheelId, StringComparison.Ordinal))
            {
                ResetForCurrentWheel();
            }

            Save();
            return true;
        }

        private LuckyWheelSpinResult CompletePendingGrant()
        {
            PendingLuckyWheelSpin pending = save.PendingSpin;
            ResourceAmount reward = new(new ResourceId(pending.ResourceId), pending.Amount);
            if (!resourceWallet.TryGrant(new ResourceGrantRequest(pending.TransactionId, reward)))
            {
                return LuckyWheelSpinResult.Failed(LuckyWheelSpinStatus.GrantFailed);
            }

            pending.Status = PendingLuckyWheelSpinStatus.GrantedAwaitingReveal;
            Save();
            return LuckyWheelSpinResult.Succeeded(
                pending.TransactionId,
                pending.SegmentId,
                pending.SegmentIndex,
                reward);
        }

        private LuckyWheelSpinResult CreatePendingResult(LuckyWheelSpinStatus status)
        {
            PendingLuckyWheelSpin pending = save.PendingSpin;
            ResourceAmount reward = new(new ResourceId(pending.ResourceId), pending.Amount);
            return status == LuckyWheelSpinStatus.Succeeded
                ? LuckyWheelSpinResult.Succeeded(
                    pending.TransactionId,
                    pending.SegmentId,
                    pending.SegmentIndex,
                    reward)
                : LuckyWheelSpinResult.AwaitingReveal(
                    pending.TransactionId,
                    pending.SegmentId,
                    pending.SegmentIndex,
                    reward);
        }

        private int SelectSegmentIndex()
        {
            int roll = random.Next(config.TotalWeight);
            int cumulative = 0;
            for (int index = 0; index < config.Segments.Count; index++)
            {
                cumulative += config.Segments[index].Weight;
                if (roll < cumulative)
                {
                    return index;
                }
            }

            throw new InvalidOperationException("Lucky wheel selection exceeded the validated weight range.");
        }

        private DateTime GetEffectiveNow()
        {
            DateTime now = clock.UtcNow.ToUniversalTime();
            if (save.LastObservedUtcTicks <= 0)
            {
                return now;
            }

            DateTime lastObserved = new(save.LastObservedUtcTicks, DateTimeKind.Utc);
            return now < lastObserved ? lastObserved : now;
        }

        private DateTime GetNextSpinUtc()
        {
            if (save.LastSpinUtcTicks <= 0)
            {
                return DateTime.MinValue;
            }

            return new DateTime(save.LastSpinUtcTicks, DateTimeKind.Utc)
                .AddSeconds(config.CooldownSeconds);
        }

        private void ResetForChangedWheelWhenSafe()
        {
            if (string.Equals(save.WheelId, config.WheelId, StringComparison.Ordinal) ||
                save.PendingSpin != null)
            {
                return;
            }

            ResetForCurrentWheel();
            Save();
        }

        private void ResetForCurrentWheel()
        {
            save.WheelId = config.WheelId;
            save.LastSpinUtcTicks = 0;
            save.LastObservedUtcTicks = 0;
            save.NextSpinSequence = 0;
            save.PendingSpin = null;
        }

        private void Save() => datasave.Save(save, saveKey);
    }
}
