using System;

namespace SnowFight.Utils
{
    public static class Extensions
    {
        public static float Range(this Random rng, float min, float max) => rng.NextSingle() * (max - min) + min;

        public static uint Range(this Random rng, uint min, uint max) => (uint)rng.NextInt64(min, max);
    }
} 