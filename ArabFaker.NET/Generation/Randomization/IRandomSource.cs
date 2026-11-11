using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Generation.Randomization
{
    /// <summary>
    /// A source of raw random numbers. Kept behind an interface so the engine can be swapped or tested.
    /// </summary>
    internal interface IRandomSource
    {
        ulong NextUInt64();

        bool TryNextInt32(int minInclusive, int maxExclusive, out int value);

        double NextDouble();

        void NextBytes(Span<byte> destination);
    }
}
