namespace ArabFaker.NET.Providers.Common.Text
{
    /// <summary>
    /// Provides methods for generating random text.
    /// </summary>
    public sealed class TextGenerator
    {
        private readonly TextProvider _provider;

        internal TextGenerator(TextProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Generates a numeric string.
        /// </summary>
        public bool TryDigits(int length, out string value)
        {
            return _provider.TryDigits(length, out value);
        }

        /// <summary>
        /// Generates a lowercase string.
        /// </summary>
        public bool TryLowercaseLetters(int length, out string value)
        {
            return _provider.TryLowercaseLetters(
                length, out value);
        }

        /// <summary>
        /// Generates an uppercase string.
        /// </summary>
        public bool TryUppercaseLetters(int length, out string value)
        {
            return _provider.TryUppercaseLetters(
                length,
                out value);
        }

        /// <summary>
        /// Generates a string containing letters.
        /// </summary>
        public bool TryLetters(int length, out string value)
        {
            return _provider.TryLetters(length, out value);
        }

        /// <summary>
        /// Generates an alphanumeric string.
        /// </summary>
        public bool TryAlphaNumeric(int length, out string value)
        {
            return _provider.TryAlphaNumeric(
                length,
                out value);
        }

        /// <summary>
        /// Generates a hexadecimal string.
        /// </summary>
        public bool TryHexadecimal(int length, out string value)
        {
            return _provider.TryHexadecimal(
                length,
                out value);
        }
    }
}
