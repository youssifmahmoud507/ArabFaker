using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;

namespace ArabFaker.Tests.Generation.Randomization
{
    public class GenerationContextTests
    {
        [Fact]
        public void SameSeedAndScopeAndIndex_ShouldGenerateSameGuid()
        {
            // Arrange
            var first = CreateContext(2026, "employee", 5);
            var second = CreateContext(2026, "employee", 5);

            // Act
            Guid firstId = first.Ids.NextGuid();
            Guid secondId = second.Ids.NextGuid();

            // Assert
            Assert.Equal(firstId, secondId);
        }

        [Fact]
        public void DifferentItemIndexes_ShouldGenerateDifferentGuids()
        {
            // Arrange
            var first = CreateContext(2026, "employee", 0);
            var second = CreateContext(2026, "employee", 1);

            // Act
            Guid firstId = first.Ids.NextGuid();
            Guid secondId = second.Ids.NextGuid();

            // Assert
            Assert.NotEqual(firstId, secondId);
        }

        [Fact]
        public void NextGuid_ShouldFollowUuidV4Format()
        {
            // Arrange
            var context = CreateContext(2026, "employee", 0);

            // Act
            Guid id = context.Ids.NextGuid();
            string value = id.ToString("N");

            // Assert
            Assert.Equal('4', value[12]);

            Assert.True("89ab".Contains(value[16]));
        }

        [Fact]
        public void NextGuid_ShouldGenerateUniqueIds()
        {
            // Arrange
            var context = CreateContext(2026, "employee", 0);

            var ids = new HashSet<Guid>();

            // Act
            for (int i = 0; i < 10_000; i++)
            {
                ids.Add(context.Ids.NextGuid());
            }

            // Assert
            Assert.Equal(10_000, ids.Count);
        }

        [Fact]
        public void ConsumingRandomValues_ShouldNotAffectIdSequence()
        {
            // Arrange
            var first = CreateContext(2026, "employee", 0);
            var second = CreateContext(2026, "employee", 0);

            // Act
            for (int i = 0; i < 100; i++)
            {
                _ = first.Random.NextDouble();
            }

            Guid firstId = first.Ids.NextGuid();
            Guid secondId = second.Ids.NextGuid();

            // Assert
            Assert.Equal(firstId, secondId);
        }

        [Fact]
        public void ChildContexts_ShouldHaveIndependentStreams()
        {
            // Arrange
            var parent1 = CreateContext(2026, "employee", 5);
            var parent2 = CreateContext(2026, "employee", 5);

            var firstNameContext = parent1.CreateChild("FirstName");
            var lastNameContext = parent1.CreateChild("LastName");

            var expectedNameContext = parent2.CreateChild("FirstName");

            // Act
            double firstNameValue = firstNameContext.Random.NextDouble();
            double lastNameValue = lastNameContext.Random.NextDouble();

            double expectedFirstNameValue =
                expectedNameContext.Random.NextDouble();

            // Assert
            Assert.Equal(
                expectedFirstNameValue,
                firstNameValue);

            Assert.NotEqual(
                firstNameValue,
                lastNameValue);
        }

        [Fact]
        public void RandomGenerator_ShouldReturnValueWithinRange()
        {
            // Arrange
            var context = CreateContext(2026, "employee", 0);

            // Act & Assert
            for (int i = 0; i < 1000; i++)
            {
                bool success = context.Random.TryNextInt(
                    18,
                    60,
                    out int age);

                Assert.True(success);
                Assert.InRange(age, 18, 59);
            }
        }

        [Fact]
        public void RandomGenerator_InvalidRange_ShouldReturnFalse()
        {
            // Arrange
            var context = CreateContext(2026, "employee", 0);

            // Act
            bool success = context.Random.TryNextInt(
                60,
                18,
                out int value);

            // Assert
            Assert.False(success);
            Assert.Equal(0, value);
        }

        [Fact]
        public void TryNextBoolean_ShouldRespectBoundaryProbabilities()
        {
            // Arrange
            var context = CreateContext(2026, "employee", 0);

            // Act
            bool falseSuccess = context.Random.TryNextBoolean(
                0,
                out bool falseValue);

            bool trueSuccess = context.Random.TryNextBoolean(
                1,
                out bool trueValue);

            // Assert
            Assert.True(falseSuccess);
            Assert.True(trueSuccess);

            Assert.False(falseValue);
            Assert.True(trueValue);
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        public void TryNextBoolean_InvalidProbability_ShouldReturnFalse(double probability)
        {
            // Arrange
            var context = CreateContext(2026, "employee", 0);

            // Act
            bool success = context.Random.TryNextBoolean(
                probability,
                out bool value);

            // Assert
            Assert.False(success);
            Assert.False(value);
        }

        private static GenerationContext CreateContext(ulong seed, string scope, ulong index)
        {
            var factory = new RandomStreamFactory(seed);

            return new GenerationContext(factory, scope, index);
        }
    }
}
