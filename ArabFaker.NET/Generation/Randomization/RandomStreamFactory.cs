using ArabFaker.NET.Generation.Seed;

namespace ArabFaker.NET.Generation.Randomization
{
    /// <summary>
    /// Creates independent, reproducible random streams, one per provider and item, from a single seed.
    /// </summary>
    internal sealed class RandomStreamFactory
    {
        private readonly ulong _rootSeed;

        internal RandomStreamFactory(ulong rootSeed)
        {
            _rootSeed = rootSeed;
        }

        internal IRandomSource Create(ReadOnlySpan<char> streamKey, ulong itemIndex)
        {
            ulong derivedSeed = SeedDeriver.Derive(_rootSeed, streamKey, itemIndex);

            return new Xoshiro256StarStarRandomSource(derivedSeed);
        }
    }
}
