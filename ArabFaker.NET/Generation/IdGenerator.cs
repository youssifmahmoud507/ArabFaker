using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Produces digit strings and GUIDs from the
    /// item's random stream, so the same seed always gives the same identifiers.
    /// </summary>
    internal sealed class IdGenerator
    {
        private readonly IRandomSource _randomSource;

        internal IdGenerator(IRandomSource randomSource)
        {
            _randomSource = randomSource;
        }

        /// <summary>
        /// Builds a string of <paramref name="length"/> random decimal digits (leading zeros allowed).
        /// </summary>
        internal string NextDigits(int length)
        {
            if (length <= 0)
                return string.Empty;

            Span<byte> buffer = stackalloc byte[32];
            Span<char> digits = length <= 64 ? stackalloc char[length] : new char[length];

            int filled = 0;

            while (filled < digits.Length)
            {
                _randomSource.NextBytes(buffer);

                foreach (byte value in buffer)
                {
                    // 250 is the largest multiple of 10 that fits in a byte, so every digit is equally likely.
                    if (value >= 250)
                        continue;

                    digits[filled++] = (char)('0' + (value % 10));

                    if (filled == digits.Length)
                        break;
                }
            }

            return new string(digits);
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
