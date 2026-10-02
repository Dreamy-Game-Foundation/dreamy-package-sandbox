using System;

namespace Dreamy.LuckyWheel
{
    public sealed class SystemLuckyWheelClock : ILuckyWheelClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
