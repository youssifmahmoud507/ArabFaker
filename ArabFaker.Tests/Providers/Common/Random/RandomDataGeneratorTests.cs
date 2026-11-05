using ArabFaker.NET;
using ArabFaker.NET.Generation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Providers.Common.Random
{
    public class RandomDataGeneratorTests
    {
        private static FakerContext CreateContext(long seed = 2026)
        {
            var faker = new Faker(seed);

            return faker.CreatePublicContext(
                "random-data-tests",
                0UL);
        }

        [Fact]
        public void TryInt_ShouldGenerateValueWithinRange()
        {
            var context = CreateContext();

            for (int i = 0; i < 1000; i++)
            {
                bool success = context.RandomData.TryInt(
                    -100,
                    100,
                    out int value);

                Assert.True(success);
                Assert.InRange(value, -100, 99);
            }
        }

        [Fact]
        public void TryInt_ShouldRejectInvalidRange()
        {
            var context = CreateContext();

            bool success = context.RandomData.TryInt(
                100,
                100,
                out int value);

            Assert.False(success);
            Assert.Equal(default, value);
        }

        [Fact]
        public void TryInt64_ShouldGenerateValueWithinRange()
        {
            var context = CreateContext();

            bool success = context.RandomData.TryInt64(
                long.MinValue,
                long.MaxValue,
                out long value);

            Assert.True(success);

            Assert.InRange(
                value,
                long.MinValue,
                long.MaxValue - 1);
        }

        [Fact]
        public void TryDecimal_ShouldGenerateValueWithinRange()
        {
            var context = CreateContext();

            bool success = context.RandomData.TryDecimal(
                10.25m,
                100.75m,
                2,
                out decimal value);

            Assert.True(success);
            Assert.InRange(value, 10.25m, 100.75m);

            decimal rounded = decimal.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);

            Assert.Equal(rounded, value);
        }

        [Fact]
        public void NextDouble_ShouldReturnValueBetweenZeroAndOne()
        {
            var context = CreateContext();

            for (int i = 0; i < 1000; i++)
            {
                double value = context.RandomData.NextDouble();

                Assert.True(value >= 0);
                Assert.True(value < 1);
            }
        }

        [Theory]
        [InlineData(0.0, false)]
        [InlineData(1.0, true)]
        public void TryBoolean_BoundaryProbabilities_ShouldReturnExpectedValue(double probability, bool expected)
        {
            var context = CreateContext();

            bool success = context.RandomData.TryBoolean(
                probability,
                out bool value);

            Assert.True(success);
            Assert.Equal(expected, value);
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void TryBoolean_InvalidProbability_ShouldReturnFalse(double probability)
        {
            var context = CreateContext();

            bool success = context.RandomData.TryBoolean(
                probability,
                out bool value);

            Assert.False(success);
            Assert.False(value);
        }

        [Fact]
        public void TryPick_ShouldSelectItemFromCollection()
        {
            var context = CreateContext();

            int[] source = [10, 20, 30, 40, 50];

            bool success = context.RandomData.TryPick(
                source,
                out int value);

            Assert.True(success);
            Assert.Contains(value, source);
        }

        [Fact]
        public void TryPick_ShouldRejectEmptyCollection()
        {
            var context = CreateContext();

            int[] source = [];

            bool success = context.RandomData.TryPick(
                source,
                out int value);

            Assert.False(success);
            Assert.Equal(default, value);
        }

        [Fact]
        public void TryPickMany_WithoutDuplicates_ShouldReturnUniqueItems()
        {
            var context = CreateContext();

            int[] source = [10, 20, 30, 40, 50];

            bool success = context.RandomData.TryPickMany(
                source,
                3,
                false,
                out int[] values);

            Assert.True(success);
            Assert.Equal(3, values.Length);
            Assert.Equal(3, values.Distinct().Count());

            Assert.All(values, value =>
            {
                Assert.Contains(value, source);
            });
        }

        [Fact]
        public void TryPickMany_WithDuplicates_ShouldAllowRepeatedItems()
        {
            var context = CreateContext();

            int[] source = [10, 20, 30];

            bool success = context.RandomData.TryPickMany(
                source,
                10,
                true,
                out int[] values);

            Assert.True(success);
            Assert.Equal(10, values.Length);

            Assert.All(values, value =>
            {
                Assert.Contains(value, source);
            });
        }

        [Fact]
        public void TryPickMany_WithoutDuplicates_ShouldRejectExcessiveCount()
        {
            var context = CreateContext();

            int[] source = [10, 20, 30];

            bool success = context.RandomData.TryPickMany(
                source,
                4,
                false,
                out int[] values);

            Assert.False(success);
            Assert.Empty(values);
        }

        [Fact]
        public void TryPickMany_NegativeCount_ShouldReturnFalse()
        {
            var context = CreateContext();

            int[] source = [10, 20, 30];

            bool success = context.RandomData.TryPickMany(
                source,
                -1,
                true,
                out int[] values);

            Assert.False(success);
            Assert.Empty(values);
        }

        [Fact]
        public void SameSeed_ShouldGenerateSameInteger()
        {
            var firstContext = CreateContext(2026);
            var secondContext = CreateContext(2026);

            bool firstSuccess = firstContext.RandomData.TryInt(
                1,
                1000,
                out int first);

            bool secondSuccess = secondContext.RandomData.TryInt(
                1,
                1000,
                out int second);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(first, second);
        }

        [Fact]
        public void TryPickMany_ShouldNotModifySourceCollection()
        {
            var context = CreateContext();

            int[] source = [1, 2, 3, 4, 5];

            int[] original = [.. source];

            bool success = context.RandomData.TryPickMany(source, 3, false, out _);

            Assert.True(success);
            Assert.Equal(original, source);
        }
    }
}
