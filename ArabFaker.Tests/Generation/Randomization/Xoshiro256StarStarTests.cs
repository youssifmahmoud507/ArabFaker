using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Generation.Randomization
{
    public class Xoshiro256StarStarTests
    {
        [Fact]
        public void SameSeed_ShouldProduceSameSequence()
        {
            // Arrange
            var first = new Xoshiro256StarStarRandomSource(2026);
            var second = new Xoshiro256StarStarRandomSource(2026);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(first.NextUInt64(), second.NextUInt64());
            }
        }

        [Fact]
        public void DifferentSeeds_ShouldProduceDifferentSequences()
        {
            // Arrange
            var first = new Xoshiro256StarStarRandomSource(2026);
            var second = new Xoshiro256StarStarRandomSource(2027);

            // Act
            var firstValue = first.NextUInt64();
            var secondValue = second.NextUInt64();

            // Assert
            Assert.NotEqual(firstValue, secondValue);
        }

        [Fact]
        public void SameSeed_ShouldMatchKnownTestVector()
        {
            // Arrange
            var random = new Xoshiro256StarStarRandomSource(2026);

            ulong[] expected = [10583478199052185109UL, 5232962402658359512UL, 14988153452874227418UL, 16485387573092771586UL, 14979407190658529994UL];

            // Act
            ulong[] actual = [.. Enumerable.Range(0, expected.Length).Select(_ => random.NextUInt64())];

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryNextInt32_ShouldReturnValueWithinRange()
        {
            // Arrange
            var random = new Xoshiro256StarStarRandomSource(2026);

            // Act & Assert
            for (int i = 0; i < 10_000; i++)
            {
                bool success = random.TryNextInt32(10, 100, out int value);

                Assert.True(success);
                Assert.InRange(value, 10, 99);
            }
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(100, 10)]
        public void TryNextInt32_InvalidRange_ShouldReturnFalse(int min, int max)
        {
            // Arrange
            var random = new Xoshiro256StarStarRandomSource(2026);

            // Act
            bool success = random.TryNextInt32(min, max, out int value);

            // Assert
            Assert.False(success);
            Assert.Equal(0, value);
        }

        [Fact]
        public void NextDouble_ShouldReturnValueBetweenZeroAndOne()
        {
            // Arrange
            var random = new Xoshiro256StarStarRandomSource(2026);

            // Act & Assert
            for (int i = 0; i < 10_000; i++)
            {
                double value = random.NextDouble();

                Assert.InRange(value, 0.0, 1.0);

                Assert.True(value < 1.0);
            }
        }

        [Fact]
        public void NextBytes_SameSeed_ShouldProduceSameBytes()
        {
            // Arrange
            var first = new Xoshiro256StarStarRandomSource(2026);
            var second = new Xoshiro256StarStarRandomSource(2026);

            byte[] firstBytes = new byte[17];
            byte[] secondBytes = new byte[17];

            // Act
            first.NextBytes(firstBytes);
            second.NextBytes(secondBytes);

            // Assert
            Assert.Equal(firstBytes, secondBytes);
        }
    }
}
