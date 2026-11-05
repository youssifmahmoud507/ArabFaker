using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using ArabFaker.NET.Providers.Common.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Providers.Common.Text
{
    public class TextProviderTests
    {
        private static GenerationContext CreateContext()
        {
            var factory = new RandomStreamFactory(2026);

            return new GenerationContext(
                factory,
                "text-provider-tests",
                0);
        }

        [Fact]
        public void TryDigits_ShouldGenerateOnlyDigits()
        {
            var context = CreateContext();

            bool success = context.Text.TryDigits(
                20,
                out string result);

            Assert.True(success);
            Assert.Equal(20, result.Length);

            Assert.All(result, character =>
            {
                Assert.InRange(character, '0', '9');
            });
        }

        [Fact]
        public void TryLowercaseLetters_ShouldGenerateOnlyLowercase()
        {
            var context = CreateContext();

            bool success = context.Text.TryLowercaseLetters(
                30,
                out string result);

            Assert.True(success);
            Assert.Equal(30, result.Length);

            Assert.All(result, character =>
            {
                Assert.InRange(character, 'a', 'z');
            });
        }

        [Fact]
        public void TryUppercaseLetters_ShouldGenerateOnlyUppercase()
        {
            var context = CreateContext();

            bool success = context.Text.TryUppercaseLetters(
                30,
                out string result);

            Assert.True(success);
            Assert.Equal(30, result.Length);

            Assert.All(result, character =>
            {
                Assert.InRange(character, 'A', 'Z');
            });
        }

        [Fact]
        public void TryLetters_ShouldGenerateOnlyLetters()
        {
            var context = CreateContext();

            bool success = context.Text.TryLetters(
                100,
                out string result);

            Assert.True(success);
            Assert.Equal(100, result.Length);

            Assert.All(result, character =>
            {
                bool isLowercase = character >= 'a' && character <= 'z';
                bool isUppercase = character >= 'A' && character <= 'Z';

                Assert.True(isLowercase || isUppercase);
            });
        }

        [Fact]
        public void TryAlphaNumeric_ShouldGenerateOnlyLettersAndDigits()
        {
            var context = CreateContext();

            bool success = context.Text.TryAlphaNumeric(
                100,
                out string result);

            Assert.True(success);
            Assert.Equal(100, result.Length);

            Assert.All(result, character =>
            {
                bool isLowercase = character >= 'a' && character <= 'z';
                bool isUppercase = character >= 'A' && character <= 'Z';
                bool isDigit = character >= '0' && character <= '9';

                Assert.True(
                    isLowercase || isUppercase || isDigit);
            });
        }

        [Fact]
        public void TryHexadecimal_ShouldGenerateOnlyHexCharacters()
        {
            var context = CreateContext();

            bool success = context.Text.TryHexadecimal(
                32,
                out string result);

            Assert.True(success);
            Assert.Equal(32, result.Length);

            Assert.All(result, character =>
            {
                bool isDigit = character >= '0' && character <= '9';
                bool isHexLetter = character >= 'a' && character <= 'f';

                Assert.True(isDigit || isHexLetter);
            });
        }

        [Fact]
        public void TryRandomString_ZeroLength_ShouldReturnEmptyString()
        {
            var context = CreateContext();

            bool success = context.Text.TryRandomString(
                0,
                TextCharacterSet.AlphaNumeric,
                out string result);

            Assert.True(success);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void TryRandomString_NegativeLength_ShouldReturnFalse()
        {
            var context = CreateContext();

            bool success = context.Text.TryRandomString(
                -1,
                TextCharacterSet.AlphaNumeric,
                out string result);

            Assert.False(success);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void TryRandomString_InvalidCharacterSet_ShouldReturnFalse()
        {
            var context = CreateContext();

            bool success = context.Text.TryRandomString(
                10,
                (TextCharacterSet)999,
                out string result);

            Assert.False(success);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void TryRandomString_SameSeed_ShouldReturnSameResult()
        {
            var firstContext = CreateContext();
            var secondContext = CreateContext();

            bool firstSuccess = firstContext.Text.TryAlphaNumeric(
                32,
                out string firstResult);

            bool secondSuccess = secondContext.Text.TryAlphaNumeric(
                32,
                out string secondResult);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(firstResult, secondResult);
        }

        [Fact]
        public void TryRandomString_AlphaNumericLowercase_ShouldNotContainUppercase()
        {
            var context = CreateContext();

            bool success = context.Text.TryRandomString(
                100,
                TextCharacterSet.AlphaNumericLowercase,
                out string result);

            Assert.True(success);

            Assert.All(result, character =>
            {
                bool isLowercase = character >= 'a' && character <= 'z';
                bool isDigit = character >= '0' && character <= '9';

                Assert.True(isLowercase || isDigit);
            });
        }
    }
}
