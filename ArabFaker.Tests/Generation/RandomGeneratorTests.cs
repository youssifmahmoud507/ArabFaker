using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Generation
{
    public class RandomGeneratorTests
    {
        private static RandomGenerator CreateRandom()
        {
            var source = new Xoshiro256StarStarRandomSource(2026);

            return new RandomGenerator(source);
        }

        [Fact]
        public void TryNextInt_ShouldReturnValueWithinRange()
        {
            // Arrange
            var random = CreateRandom();

            // Act & Assert
            for (int i = 0; i < 10_000; i++)
            {
                bool success = random.TryNextInt(10, 100, out int value);

                Assert.True(success);
                Assert.InRange(value, 10, 99);
            }
        }

        [Fact]
        public void TryNextInt_InvalidRange_ShouldReturnFalse()
        {
            var random = CreateRandom();

            bool success = random.TryNextInt(100, 10, out int value);

            Assert.False(success);
            Assert.Equal(0, value);
        }

        [Fact]
        public void TryNextDecimal_ShouldReturnValueWithinRange()
        {
            var random = CreateRandom();

            for (int i = 0; i < 10_000; i++)
            {
                bool success = random.TryNextDecimal(100m, 5000m, out decimal value, 2);

                Assert.True(success);
                Assert.InRange(value, 100m, 5000m);
                Assert.Equal(2, GetDecimalPlaces(value));
            }
        }

        [Fact]
        public void TryNextDecimal_SameBounds_ShouldReturnExactValue()
        {
            var random = CreateRandom();

            bool success = random.TryNextDecimal(150.75m, 150.75m, out decimal value);

            Assert.True(success);
            Assert.Equal(150.75m, value);
        }

        [Fact]
        public void TryNextDecimal_InvalidRange_ShouldReturnFalse()
        {
            var random = CreateRandom();

            bool success = random.TryNextDecimal(500m, 100m, out decimal value);

            Assert.False(success);
            Assert.Equal(0m, value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(29)]
        public void TryNextDecimal_InvalidPrecision_ShouldReturnFalse(int decimalPlaces)
        {
            var random = CreateRandom();

            bool success = random.TryNextDecimal(100m, 500m, out decimal value, decimalPlaces);

            Assert.False(success);
            Assert.Equal(0m, value);
        }

        [Fact]
        public void TryNextBoolean_ZeroProbability_ShouldReturnFalse()
        {
            var random = CreateRandom();

            bool success = random.TryNextBoolean(0, out bool value);

            Assert.True(success);
            Assert.False(value);
        }

        [Fact]
        public void TryNextBoolean_OneProbability_ShouldReturnTrue()
        {
            var random = CreateRandom();

            bool success = random.TryNextBoolean(1, out bool value);

            Assert.True(success);
            Assert.True(value);
        }

        [Fact]
        public void TryNextBoolean_InvalidProbability_ShouldReturnFalse()
        {
            var random = CreateRandom();

            bool success = random.TryNextBoolean(1.5, out bool value);

            Assert.False(success);
            Assert.False(value);
        }

        [Fact]
        public void TryPick_ShouldReturnExistingItem()
        {
            // Arrange
            var random = CreateRandom();

            int[] items = [10, 20, 30, 40, 50];

            // Act
            bool success = random.TryPick(items, out int selected);

            // Assert
            Assert.True(success);
            Assert.Contains(selected, items);
        }

        [Fact]
        public void TryPick_EmptyCollection_ShouldReturnFalse()
        {
            var random = CreateRandom();

            bool success = random.TryPick([], out int selected);

            Assert.False(success);
            Assert.Equal(0, selected);
        }

        [Fact]
        public void TryPickMany_WithoutDuplicates_ShouldReturnUniqueItems()
        {
            // Arrange
            var random = CreateRandom();

            int[] items = [10, 20, 30, 40, 50];

            int[] originalItems = [.. items];

            // Act
            bool success = random.TryPickMany(items, 3, allowDuplicates: false, out int[] selected);

            // Assert
            Assert.True(success);
            Assert.Equal(3, selected.Length);

            Assert.Equal(selected.Length, selected.Distinct().Count());

            Assert.All(selected, item => Assert.Contains(item, items));

            Assert.Equal(originalItems, items);
        }

        [Fact]
        public void TryPickMany_WithDuplicates_ShouldAllowRepeatedItems()
        {
            // Arrange
            var random = CreateRandom();

            int[] items = [10];

            // Act
            bool success = random.TryPickMany(items, 5, allowDuplicates: true, out int[] selected);

            // Assert
            Assert.True(success);
            Assert.Equal(5, selected.Length);
            Assert.All(selected, item => Assert.Equal(10, item));
        }

        [Fact]
        public void TryPickMany_WithoutDuplicates_ExceedingCount_ShouldFail()
        {
            var random = CreateRandom();

            int[] items = [10, 20, 30];

            bool success = random.TryPickMany(items, 5, allowDuplicates: false, out int[] selected);

            Assert.False(success);
            Assert.Empty(selected);
        }

        [Fact]
        public void TryPickMany_NegativeCount_ShouldFail()
        {
            var random = CreateRandom();

            bool success = random.TryPickMany([10, 20, 30], -1, false, out int[] selected);

            Assert.False(success);
            Assert.Empty(selected);
        }

        [Fact]
        public void TryPickMany_ZeroCount_ShouldReturnEmptyCollection()
        {
            var random = CreateRandom();

            bool success = random.TryPickMany([10, 20, 30], 0, false, out int[] selected);

            Assert.True(success);
            Assert.Empty(selected);
        }

        [Fact]
        public void SameSeed_ShouldGenerateSameRandomValues()
        {
            var first = new RandomGenerator(new Xoshiro256StarStarRandomSource(2026));

            var second = new RandomGenerator(new Xoshiro256StarStarRandomSource(2026));

            for (int i = 0; i < 100; i++)
            {
                bool firstSuccess = first.TryNextInt(1, 1000, out int firstValue);

                bool secondSuccess = second.TryNextInt(1, 1000, out int secondValue);

                Assert.True(firstSuccess);
                Assert.True(secondSuccess);

                Assert.Equal(firstValue, secondValue);
            }
        }

        private static int GetDecimalPlaces(decimal value)
        {
            return (decimal.GetBits(value)[3] >> 16) & 0xFF;
        }
    }
}
