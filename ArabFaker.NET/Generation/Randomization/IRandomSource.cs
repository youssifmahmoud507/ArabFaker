using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Generation.Randomization
{
    internal interface IRandomSource
    {
        ulong NextUInt64();

        bool TryNextInt32(int minInclusive, int maxExclusive, out int value);

        double NextDouble();

        void NextBytes(Span<byte> destination);
    }
}
