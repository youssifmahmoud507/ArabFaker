using ArabFaker.NET.Generation.Randomization;

namespace ArabFaker.NET.Providers.Common.Text
{
    /// <summary>
    /// Generates random strings of a given length from a chosen character set (digits, letters, hexadecimal, ...).
    /// </summary>
    internal sealed class TextProvider
    {
        private static readonly char[] LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        private static readonly char[] UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private static readonly char[] DigitCharacters = "0123456789".ToCharArray();

        private static readonly char[] LetterCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] LowercaseAlphaNumericCharacters = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        private static readonly char[] AlphaNumericCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static readonly char[] HexadecimalCharacters = "0123456789abcdef".ToCharArray();

        private readonly RandomGenerator _random;

        internal TextProvider(RandomGenerator random)
        {
            _random = random;
        }

        internal bool TryRandomString(int length, TextCharacterSet characterSet, out string value)
        {
            value = string.Empty;

            if (length < 0)
                return false;

            if (!TryGetCharacters(characterSet, out char[] characters))
                return false;

            if (length == 0)
                return true;

            var buffer = new char[length];

            for (int i = 0; i < length; i++)
            {
                if (!_random.TryPick(characters, out char selected))
                    return false;

                buffer[i] = selected;
            }

            value = new string(buffer);

            return true;
        }

        internal bool TryDigits(int length, out string value)
        {
            return TryRandomString(
                length,
                TextCharacterSet.Digits,
                out value);
        }

        internal bool TryLowercaseLetters(int length, out string value)
        {
            return TryRandomString(
                length,
                TextCharacterSet.LowercaseLetters,
                out value);
        }

        internal bool TryUppercaseLetters(int length, out string value)
        {
            return TryRandomString(
                length,
                TextCharacterSet.UppercaseLetters,
                out value);
        }

        internal bool TryLetters(int length, out string value)
        {
            return TryRandomString(length, TextCharacterSet.Letters, out value);
        }

        internal bool TryAlphaNumeric(int length, out string value)
        {
            return TryRandomString(length, TextCharacterSet.AlphaNumeric, out value);
        }

        internal bool TryHexadecimal(int length, out string value)
        {
            return TryRandomString(length, TextCharacterSet.Hexadecimal, out value);
        }

        private static bool TryGetCharacters(TextCharacterSet characterSet, out char[] characters)
        {
            characters = characterSet switch
            {
                TextCharacterSet.LowercaseLetters => LowercaseCharacters,

                TextCharacterSet.UppercaseLetters => UppercaseCharacters,

                TextCharacterSet.Digits => DigitCharacters,

                TextCharacterSet.Letters => LetterCharacters,

                TextCharacterSet.AlphaNumericLowercase => LowercaseAlphaNumericCharacters,

                TextCharacterSet.AlphaNumeric => AlphaNumericCharacters,

                TextCharacterSet.Hexadecimal => HexadecimalCharacters,

                _ => []
            };

            return characters.Length > 0;
        }
    }
}
