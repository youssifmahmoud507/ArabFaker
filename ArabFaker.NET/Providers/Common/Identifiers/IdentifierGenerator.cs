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
        /// Generates a UUID version 4 identifier as a string.
        /// </summary>
        public string GenerateGuidString()
        {
            return _provider.NextGuid().ToString("D");
        }
    }
}
