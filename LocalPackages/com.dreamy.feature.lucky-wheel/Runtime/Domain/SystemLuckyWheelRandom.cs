using System;

namespace Dreamy.LuckyWheel
{
    public sealed class SystemLuckyWheelRandom : ILuckyWheelRandom
    {
        private readonly Random random;

        public SystemLuckyWheelRandom()
            : this(new Random())
        {
        }

        public SystemLuckyWheelRandom(int seed)
            : this(new Random(seed))
        {
        }

        private SystemLuckyWheelRandom(Random random)
        {
            this.random = random;
        }

        public int Next(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            return random.Next(exclusiveMax);
        }
    }
}
