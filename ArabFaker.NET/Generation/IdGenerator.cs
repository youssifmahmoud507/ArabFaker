using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Generation
{
    internal sealed class IdGenerator
    {
        private readonly IRandomSource _randomSource;

        internal IdGenerator(IRandomSource randomSource)
        {
            _randomSource = randomSource;
        }

        internal Guid NextGuid()
        {
            Span<byte> bytes = stackalloc byte[16];

            _randomSource.NextBytes(bytes);

            // Set UUID version 4 bits.
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40);

            // Set RFC-compatible variant bits.
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

            return new Guid(bytes, bigEndian: true);
        }
    }
}
