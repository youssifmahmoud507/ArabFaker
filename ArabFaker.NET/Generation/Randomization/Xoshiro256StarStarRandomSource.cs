using System.Buffers.Binary;

namespace ArabFaker.NET.Generation.Randomization
{
    internal sealed class Xoshiro256StarStarRandomSource : IRandomSource
    {
        private ulong _s0;
        private ulong _s1;
        private ulong _s2;
        private ulong _s3;

        public Xoshiro256StarStarRandomSource(ulong seed)
        {
            _s0 = SplitMix64.Next(ref seed);
            _s1 = SplitMix64.Next(ref seed);
            _s2 = SplitMix64.Next(ref seed);
            _s3 = SplitMix64.Next(ref seed);

            // Xoshiro256** cannot operate with an all zero state.
            if ((_s0 | _s1 | _s2 | _s3) == 0)
            {
                _s0 = 0x9E3779B97F4A7C15UL;
            }
        }

        public ulong NextUInt64()
        {
            ulong result = unchecked(RotateLeft(unchecked(_s1 * 5), 7) * 9);

            ulong temporary = _s1 << 17;

            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;

            _s2 ^= temporary;
            _s3 = RotateLeft(_s3, 45);

            return result;
        }

        public bool TryNextInt32(int minInclusive, int maxExclusive, out int value)
        {
            value = default;

            if (minInclusive >= maxExclusive)
                return false;

            ulong range = (ulong)((long)maxExclusive - minInclusive);

            ulong threshold = unchecked(0UL - range) % range;

            ulong randomValue;

            do
            {
                randomValue = NextUInt64();
            }
            while (randomValue < threshold);

            ulong offset = randomValue % range;

            value = (int)((long)minInclusive + (long)offset);

            return true;
        }

        public double NextDouble()
        {
            const double Scale = 1.0 / 9007199254740992.0;

            return (NextUInt64() >> 11) * Scale;
        }

        public void NextBytes(Span<byte> destination)
        {
            while (destination.Length >= sizeof(ulong))
            {
                BinaryPrimitives.WriteUInt64LittleEndian(destination, NextUInt64());

                destination = destination[sizeof(ulong)..];
            }

            if (destination.IsEmpty)
                return;

            Span<byte> remaining = stackalloc byte[sizeof(ulong)];

            BinaryPrimitives.WriteUInt64LittleEndian(remaining, NextUInt64());

            remaining[..destination.Length].CopyTo(destination);
        }

        private static ulong RotateLeft(ulong value, int count)
        {
            return (value << count) | (value >> (64 - count));
        }
    }

}
