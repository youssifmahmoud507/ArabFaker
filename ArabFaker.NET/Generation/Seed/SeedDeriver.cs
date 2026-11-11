﻿using ArabFaker.NET.Generation.Randomization;

namespace ArabFaker.NET.Generation.Seed
{
    /// <summary>
    /// Derives a stable sub-seed from the main seed, a scope key and an item index, so providers
    /// never share a stream and adding one does not change the output of another.
    /// </summary>
    internal static class SeedDeriver
    {
        private const ulong StreamDomain = 0x4152414246414B45UL;

        private const ulong IndexMultiplier = 0x9E3779B97F4A7C15UL;

        internal static ulong Derive(ulong rootSeed, ReadOnlySpan<char> streamKey, ulong itemIndex)
        {
            ulong streamHash = StableStringHash64.Compute(streamKey);

            ulong state = rootSeed ^ streamHash ^ StreamDomain;

            state = unchecked(state + (itemIndex * IndexMultiplier));

            return SplitMix64.Next(ref state);
        }
    }
}
