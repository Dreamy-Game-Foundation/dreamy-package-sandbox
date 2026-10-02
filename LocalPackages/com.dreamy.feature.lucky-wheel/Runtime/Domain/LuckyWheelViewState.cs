using System;
using System.Collections.Generic;

namespace Dreamy.LuckyWheel
{
    public sealed class LuckyWheelViewState
    {
        public LuckyWheelViewState(
            IReadOnlyList<LuckyWheelSegment> segments,
            LuckyWheelAvailability availability,
            DateTime nextSpinUtc,
            LuckyWheelSpinResult? pendingResult)
        {
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
            Availability = availability;
            NextSpinUtc = nextSpinUtc;
            PendingResult = pendingResult;
        }

        public IReadOnlyList<LuckyWheelSegment> Segments { get; }
        public LuckyWheelAvailability Availability { get; }
        public DateTime NextSpinUtc { get; }
        public LuckyWheelSpinResult? PendingResult { get; }
        public bool CanSpin => Availability == LuckyWheelAvailability.Ready;
        public bool CanRetry => Availability == LuckyWheelAvailability.PendingGrant;
    }

    public enum LuckyWheelAvailability
    {
        Ready,
        Cooldown,
        PendingGrant,
        AwaitingReveal
    }
}
