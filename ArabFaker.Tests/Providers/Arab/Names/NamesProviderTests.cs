using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using ArabFaker.NET.Providers.Arab.Names;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Providers.Arab.Names
{
    public class NamesProviderTests
    {
        [Fact]
        public void TryFirstName_Arabic_ShouldReturnValidName()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Names.TryFirstName(out var name, NameLanguage.Arabic);

            // Assert
            Assert.True(success);
            Assert.Contains(name, NameCatalog.AllFirstNames.Select(x => x.Arabic));
        }

        [Fact]
        public void TryFirstName_Male_ShouldReturnMaleName()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Names.TryFirstName(out var name, NameLanguage.Arabic, NameGender.Male);

            // Assert
            Assert.True(success);

            Assert.Contains(name, NameCatalog.MaleFirstNames.Select(x => x.Arabic));
        }

        [Fact]
        public void TryFirstName_Female_ShouldReturnFemaleName()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Names.TryFirstName(out var name, NameLanguage.Arabic, NameGender.Female);

            // Assert
            Assert.True(success);

            Assert.Contains(name, NameCatalog.FemaleFirstNames.Select(x => x.Arabic));
        }

        [Fact]
        public void TryFirstName_Latin_ShouldReturnLatinName()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Names.TryFirstName(out var name, NameLanguage.Latin);

            // Assert
            Assert.True(success);

            Assert.Contains(name, NameCatalog.AllFirstNames.Select(x => x.Latin));
        }

        [Fact]
        public void TryLastName_ShouldReturnValidFamilyName()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Names.TryLastName(out var name, NameLanguage.Arabic);

            // Assert
            Assert.True(success);

            Assert.Contains(name, NameCatalog.FamilyNames.Select(x => x.Arabic));
        }

        [Fact]
        public void TryFullName_Arabic_ShouldReturnThreeNameParts()
        {
            AssertFullName(NameLanguage.Arabic);
        }

        [Fact]
        public void TryFullName_Latin_ShouldReturnThreeNameParts()
        {
            AssertFullName(NameLanguage.Latin);
        }

        private static void AssertFullName(NameLanguage language)
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Names.TryFullName(out var fullName, language);

            // Assert
            Assert.True(success);
            Assert.Equal(3, fullName.Split(' ').Length);
        }

        [Fact]
        public void SameSeed_ShouldGenerateSameFullName()
        {
            // Arrange
            var firstContext = CreateContext();
            var secondContext = CreateContext();

            // Act
            firstContext.Names.TryFullName(out var firstName);

            secondContext.Names.TryFullName(out var secondName);

            // Assert
            Assert.Equal(firstName, secondName);
        }

        [Fact]
        public void InvalidLanguage_ShouldReturnFalse()
        {
            // Arrange
            var context = CreateContext();

            // Act
            bool success = context.Names.TryFirstName(
                out var name,
                (NameLanguage)999);

            // Assert
            Assert.False(success);
            Assert.Empty(name);
        }

        private static GenerationContext CreateContext()
        {
            var factory = new RandomStreamFactory(2026);

            return new GenerationContext(factory, "names-provider-tests", 0);
        }
    }
}
