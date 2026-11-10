﻿﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Generation.Seed
{
    internal static class StableStringHash64
    {
        private const ulong OffsetBasis = 14695981039346656037UL;

        private const ulong Prime = 1099511628211UL;

        internal static ulong Compute(ReadOnlySpan<char> value)
        {
            ulong hash = OffsetBasis;

            foreach (char character in value)
            {
                // Process the low byte.
                hash ^= (byte)character;
                hash = unchecked(hash * Prime);

                // Process the high byte.
                hash ^= (byte)(character >> 8);
                hash = unchecked(hash * Prime);
            }

            return hash;
        }
    }
}
