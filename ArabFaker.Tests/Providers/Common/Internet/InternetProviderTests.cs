using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Providers.Common.Internet
{
    public class InternetProviderTests
    {
        [Fact]
        public void TryUsername_ShouldGenerateValidUsername()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Internet.TryUsername(out string username);

            // Assert
            Assert.True(success);
            Assert.Equal(12, username.Length);
            Assert.StartsWith("user", username);

            Assert.All(username.Skip(4), character =>
            {
                Assert.True(char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character));
            });
        }

        [Fact]
        public void TryEmail_ShouldUseDefaultTestDomain()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Internet.TryEmail(out string email);

            // Assert
            Assert.True(success);

            Assert.EndsWith("@example.test", email, StringComparison.Ordinal);
        }

        [Fact]
        public void TryEmail_ShouldSupportCustomTestDomain()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Internet.TryEmail(out string email, "qa.example.test");

            // Assert
            Assert.True(success);

            Assert.EndsWith("@qa.example.test", email, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("gmail.com")]
        [InlineData("yahoo.com")]
        [InlineData("example.com")]
        [InlineData("example.test.com")]
        [InlineData("example..test")]
        [InlineData("-invalid.test")]
        [InlineData("invalid-.test")]
        [InlineData("")]
        public void TryEmail_InvalidDomain_ShouldReturnFalse(string domain)
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Internet.TryEmail(out string email, domain);

            // Assert
            Assert.False(success);
            Assert.Empty(email);
        }

        [Fact]
        public void TryEmail_NullDomain_ShouldReturnFalse()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Internet.TryEmail(out string email, null);

            // Assert
            Assert.False(success);
            Assert.Empty(email);
        }

        [Fact]
        public void SameSeed_ShouldGenerateSameUsername()
        {
            // Arrange
            var first = CreateContext();
            var second = CreateContext();

            // Act
            first.Internet.TryUsername(out string firstUsername);

            second.Internet.TryUsername(out string secondUsername);

            // Assert
            Assert.Equal(firstUsername, secondUsername);
        }

        [Fact]
        public void InvalidEmailDomain_ShouldNotConsumeRandomStream()
        {
            // Arrange
            var first = CreateContext();
            var second = CreateContext();

            // Act
            first.Internet.TryEmail(out _, "gmail.com");

            first.Internet.TryUsername(out string firstUsername);

            second.Internet.TryUsername(out string secondUsername);

            // Assert
            Assert.Equal(firstUsername, secondUsername);
        }

        [Fact]
        public void InternetGeneration_ShouldNotAffectNamesGeneration()
        {
            // Arrange
            var first = CreateContext();
            var second = CreateContext();

            // Act
            for (int i = 0; i < 100; i++)
            {
                first.Internet.TryEmail(out _);
            }

            first.Names.TryFirstName(out string firstName);

            second.Names.TryFirstName(out string secondName);

            // Assert
            Assert.Equal(firstName, secondName);
        }

        private static GenerationContext CreateContext()
        {
            var factory = new RandomStreamFactory(2026);

            return new GenerationContext(factory, "internet-provider-tests", 0);
        }
    }
}
