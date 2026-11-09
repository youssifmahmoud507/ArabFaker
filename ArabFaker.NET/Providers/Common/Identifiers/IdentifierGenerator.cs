using ArabFaker.NET.Generation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Providers.Common.Identifiers
{
    /// <summary>
    /// Provides methods for generating fake identifiers.
    /// </summary>

    public sealed class IdentifierGenerator
    {
        private readonly IdGenerator _provider;

        internal IdentifierGenerator(IdGenerator provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Generates a UUID version 4 identifier.
        /// </summary>
        public Guid GenerateGuid()
        {
            return _provider.NextGuid();
        }

        /// <summary>
        /// Generates a fake national ID number: 14 random digits.
        /// </summary>
        /// <remarks>
        /// The digits carry no meaning (no birth date, governorate or check digit), and the number is
        /// not tied to a real person. A generated value may happen to equal a real one, so use it
        /// only as test data.
        /// </remarks>
        /// <returns>A 14-character string made of digits.</returns>
        public string GenerateNationalId()
        {
            return _provider.NextDigits(14);
        }

        /// <summary>
        /// Generates a UUID version 4 identifier as a string.
        /// </summary>
        public string GenerateGuidString()
        {
            return _provider.NextGuid().ToString("D");
        }
    }
}
