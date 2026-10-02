using System;

namespace Dreamy.LuckyWheel
{
    public interface ILuckyWheelClock
    {
        DateTime UtcNow { get; }
    }
}
