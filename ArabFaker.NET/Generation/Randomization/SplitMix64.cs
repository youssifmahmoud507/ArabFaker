namespace ArabFaker.NET.Generation.Randomization
{
    internal static class SplitMix64
    {
        private const ulong GoldenRatio = 0x9E3779B97F4A7C15UL;
        private const ulong MixConstant1 = 0xBF58476D1CE4E5B9UL;
        private const ulong MixConstant2 = 0x94D049BB133111EBUL;
        internal static ulong Next(ref ulong state)
        {
            state = unchecked(state + GoldenRatio);
            ulong value = state;
            value = unchecked((value ^ (value >> 30)) * MixConstant1);
            value = unchecked((value ^ (value >> 27)) * MixConstant2);
            return value ^ (value >> 31);
        }
    }
}
