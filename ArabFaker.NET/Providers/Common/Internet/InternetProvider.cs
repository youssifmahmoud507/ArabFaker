using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Providers.Common.Internet
{
    /// <summary>
    /// Generates test-only internet values: user names and email addresses (on a reserved test domain by default).
    /// </summary>
    internal sealed class InternetProvider
    {
        private const string DefaultTestDomain = "example.test";

        private const string UsernameCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

        private const string UsernamePrefix = "user";

        private const int RandomUsernameLength = 8;

        private readonly RandomGenerator _random;

        internal InternetProvider(RandomGenerator random)
        {
            _random = random;
        }

        internal bool TryUsername(out string username)
        {
            username = string.Empty;

            Span<char> buffer = stackalloc char[UsernamePrefix.Length + RandomUsernameLength];

            UsernamePrefix.AsSpan().CopyTo(buffer);

            for (int i = 0; i < RandomUsernameLength; i++)
            {
                bool success = _random.TryNextInt(0, UsernameCharacters.Length, out int index);

                if (!success)
                    return false;

                buffer[UsernamePrefix.Length + i] = UsernameCharacters[index];
            }

            username = new string(buffer);

            return true;
        }

        internal bool TryEmail(out string email, string? domain = DefaultTestDomain)
        {
            email = string.Empty;

            if (!IsValidTestDomain(domain))
                return false;

            if (!TryUsername(out string username))
                return false;

            email = string.Concat(username, "@", domain);

            return true;
        }

        private static bool IsValidTestDomain(string? domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            if (domain.Length > 253)
                return false;

            if (!domain.EndsWith(".test", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            ReadOnlySpan<char> remaining = domain.AsSpan();

            while (!remaining.IsEmpty)
            {
                int separatorIndex = remaining.IndexOf('.');

                ReadOnlySpan<char> label = separatorIndex >= 0 ? remaining[..separatorIndex] : remaining;

                if (label.Length is 0 or > 63)
                    return false;

                if (label[0] == '-' || label[^1] == '-')
                    return false;

                foreach (char character in label)
                {
                    bool isValid = char.IsAsciiLetterOrDigit(character) || character == '-';

                    if (!isValid)
                        return false;
                }

                if (separatorIndex < 0)
                    break;

                remaining = remaining[(separatorIndex + 1)..];
            }

            return true;
        }
    }
}
