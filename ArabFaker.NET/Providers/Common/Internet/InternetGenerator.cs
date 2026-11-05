using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Providers.Common.Internet
{
    /// <summary>
    /// Provides methods for generating fake internet-related data.
    /// </summary>
    public sealed class InternetGenerator
    {
        private readonly InternetProvider _provider;

        internal InternetGenerator(InternetProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Generates a fake username.
        /// </summary>
        public bool TryUsername(out string username)
        {
            return _provider.TryUsername(out username);
        }

        /// <summary>
        /// Generates a fake email address.
        /// </summary>
        /// <param name="email">The generated email address.</param>
        /// <param name="domain">
        /// The test domain. Defaults to example.test.
        /// </param>
        public bool TryEmail(out string email, string domain = "example.test")
        {
            return _provider.TryEmail(out email, domain);
        }
    }
}
