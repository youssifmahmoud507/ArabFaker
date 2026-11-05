using ArabFaker.NET;
using ArabFaker.NET.Providers.Arab.Names;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests
{
    public class FakerTests
    {
        [Fact]
        public void Constructor_WithSeed_ShouldPreserveSeed()
        {
            const long seed = 2026;

            var faker = new Faker(seed);

            Assert.Equal(seed, faker.Seed);
        }

        [Fact]
        public void Constructor_ShouldSupportNegativeSeed()
        {
            const long seed = -2026;

            var faker = new Faker(seed);

            Assert.Equal(seed, faker.Seed);
        }

        [Fact]
        public void Constructor_WithoutSeed_ShouldCreateInstance()
        {
            var faker = new Faker();

            Assert.NotNull(faker);
        }

        [Fact]
        public void SameSeedAndContext_ShouldGenerateSameText()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2026);

            var firstContext = firstFaker.CreateContext(
                "faker-tests",
                0);

            var secondContext = secondFaker.CreateContext(
                "faker-tests",
                0);

            bool firstSuccess = firstContext.Text.TryAlphaNumeric(
                64,
                out string firstResult);

            bool secondSuccess = secondContext.Text.TryAlphaNumeric(
                64,
                out string secondResult);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(firstResult, secondResult);
        }

        [Fact]
        public void DifferentSeeds_ShouldGenerateDifferentText()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2027);

            var firstContext = firstFaker.CreateContext(
                "faker-tests",
                0);

            var secondContext = secondFaker.CreateContext(
                "faker-tests",
                0);

            bool firstSuccess = firstContext.Text.TryAlphaNumeric(
                128,
                out string firstResult);

            bool secondSuccess = secondContext.Text.TryAlphaNumeric(
                128,
                out string secondResult);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.NotEqual(firstResult, secondResult);
        }

        [Fact]
        public void CreatePublicContext_ShouldExposeTextGenerator()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "public-context-tests",
                0);

            Assert.NotNull(context.Text);

            bool success = context.Text.TryAlphaNumeric(
                32,
                out string result);

            Assert.True(success);
            Assert.Equal(32, result.Length);
        }

        [Fact]
        public void PublicTextGenerator_ShouldPreserveSeedReproducibility()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2026);

            var firstContext = firstFaker.CreatePublicContext(
                "public-context-tests",
                0);

            var secondContext = secondFaker.CreatePublicContext(
                "public-context-tests",
                0);

            bool firstSuccess = firstContext.Text.TryDigits(
                20,
                out string firstResult);

            bool secondSuccess = secondContext.Text.TryDigits(
                20,
                out string secondResult);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(firstResult, secondResult);
        }

        [Fact]
        public void PublicTextGenerator_ShouldHandleInvalidLength()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "public-context-tests",
                0);

            bool success = context.Text.TryDigits(
                -1,
                out string result);

            Assert.False(success);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void PublicNamesGenerator_ShouldGenerateArabicFirstName()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "names-tests",
                0);

            bool success = context.Names.TryFirstName(
                out string name,
                NameLanguage.Arabic);

            Assert.True(success);
            Assert.False(string.IsNullOrWhiteSpace(name));
        }

        [Fact]
        public void PublicNamesGenerator_ShouldGenerateLatinFullName()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "names-tests",
                0);

            bool success = context.Names.TryFullName(
                out string name,
                NameLanguage.Latin,
                NameGender.Any);

            Assert.True(success);
            Assert.False(string.IsNullOrWhiteSpace(name));
        }

        [Fact]
        public void PublicNamesGenerator_SameSeed_ShouldProduceSameName()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2026);

            var firstContext = firstFaker.CreatePublicContext(
                "names-tests",
                0);

            var secondContext = secondFaker.CreatePublicContext(
                "names-tests",
                0);

            bool firstSuccess = firstContext.Names.TryFullName(
                out string firstName);

            bool secondSuccess = secondContext.Names.TryFullName(
                out string secondName);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(firstName, secondName);
        }

        [Fact]
        public void PublicNamesGenerator_ShouldRespectGender()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "names-tests",
                0);

            bool success = context.Names.TryFirstName(
                out string name,
                NameLanguage.Arabic,
                NameGender.Male);

            Assert.True(success);
            Assert.False(string.IsNullOrWhiteSpace(name));
        }

        [Fact]
        public void PublicInternetGenerator_ShouldGenerateUsername()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "internet-tests",
                0);

            bool success = context.Internet.TryUsername(
                out string username);

            Assert.True(success);
            Assert.StartsWith("user", username);
            Assert.Equal(12, username.Length);

            Assert.All(username.Skip(4), character =>
            {
                bool isLowercase = character >= 'a' && character <= 'z';
                bool isDigit = character >= '0' && character <= '9';

                Assert.True(isLowercase || isDigit);
            });
        }

        [Fact]
        public void PublicInternetGenerator_ShouldGenerateDefaultEmail()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "internet-tests",
                0);

            bool success = context.Internet.TryEmail(
                out string email);

            Assert.True(success);
            Assert.EndsWith("@example.test", email);
        }

        [Fact]
        public void PublicInternetGenerator_ShouldGenerateEmailWithCustomDomain()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "internet-tests",
                0);

            bool success = context.Internet.TryEmail(
                out string email,
                "my-app.example.test");

            Assert.True(success);
            Assert.EndsWith("@my-app.example.test", email);
        }

        [Fact]
        public void PublicInternetGenerator_ShouldRejectInvalidDomain()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "internet-tests",
                0);

            bool success = context.Internet.TryEmail(
                out string email,
                "gmail.com");

            Assert.False(success);
            Assert.Equal(string.Empty, email);
        }

        [Fact]
        public void PublicInternetGenerator_SameSeed_ShouldProduceSameUsername()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2026);

            var firstContext = firstFaker.CreatePublicContext(
                "internet-tests",
                0);

            var secondContext = secondFaker.CreatePublicContext(
                "internet-tests",
                0);

            bool firstSuccess = firstContext.Internet.TryUsername(
                out string firstUsername);

            bool secondSuccess = secondContext.Internet.TryUsername(
                out string secondUsername);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(firstUsername, secondUsername);
        }

        [Fact]
        public void PublicInternetGenerator_SameSeed_ShouldProduceSameEmail()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2026);

            var firstContext = firstFaker.CreatePublicContext("internet-tests", 0);

            var secondContext = secondFaker.CreatePublicContext("internet-tests", 0);

            bool firstSuccess = firstContext.Internet.TryEmail(out string firstEmail);

            bool secondSuccess = secondContext.Internet.TryEmail(out string secondEmail);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(firstEmail, secondEmail);
        }

        [Fact]
        public void PublicDatesGenerator_ShouldGenerateDateWithinRange()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "dates-tests",
                0);

            var start = new DateOnly(2020, 1, 1);
            var end = new DateOnly(2025, 12, 31);

            bool success = context.Dates.TryDateOnlyBetween(
                start,
                end,
                out var result);

            Assert.True(success);
            Assert.InRange(result, start, end);
        }

        [Fact]
        public void PublicDatesGenerator_ShouldGenerateDateTimeWithinRange()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "dates-tests",
                0);

            var start = new DateTime(
                2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var end = new DateTime(
                2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            bool success = context.Dates.TryDateTimeBetween(
                start,
                end,
                out var result);

            Assert.True(success);
            Assert.InRange(result.Ticks, start.Ticks, end.Ticks);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void PublicDatesGenerator_ShouldGenerateDateOfBirthWithinAgeRange()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "dates-tests",
                0);

            var asOfDate = new DateOnly(2026, 11, 2);

            bool success = context.Dates.TryDateOfBirth(
                asOfDate,
                18,
                60,
                out var birthDate);

            Assert.True(success);

            int age = CalculateAge(birthDate, asOfDate);

            Assert.InRange(age, 18, 60);
        }

        [Fact]
        public void PublicDatesGenerator_ShouldRejectInvalidDateRange()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "dates-tests",
                0);

            bool success = context.Dates.TryDateOnlyBetween(
                new DateOnly(2026, 1, 1),
                new DateOnly(2025, 1, 1),
                out var result);

            Assert.False(success);
            Assert.Equal(default, result);
        }

        [Fact]
        public void PublicDatesGenerator_ShouldRejectInvalidAgeRange()
        {
            var faker = new Faker(2026);

            var context = faker.CreatePublicContext(
                "dates-tests",
                0);

            bool success = context.Dates.TryDateOfBirth(
                new DateOnly(2026, 11, 2),
                60,
                18,
                out var result);

            Assert.False(success);
            Assert.Equal(default, result);
        }

        [Fact]
        public void PublicDatesGenerator_SameSeed_ShouldProduceSameDate()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2026);

            var firstContext = firstFaker.CreatePublicContext(
                "dates-tests",
                0);

            var secondContext = secondFaker.CreatePublicContext(
                "dates-tests",
                0);

            var start = new DateOnly(2000, 1, 1);
            var end = new DateOnly(2026, 12, 31);

            bool firstSuccess = firstContext.Dates.TryDateOnlyBetween(
                start,
                end,
                out var firstDate);

            bool secondSuccess = secondContext.Dates.TryDateOnlyBetween(
                start,
                end,
                out var secondDate);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(firstDate, secondDate);
        }

        private static int CalculateAge(DateOnly birthDate, DateOnly asOfDate)
        {
            int age = asOfDate.Year - birthDate.Year;

            if (birthDate > asOfDate.AddYears(-age))
                age--;

            return age;
        }
    }
}
