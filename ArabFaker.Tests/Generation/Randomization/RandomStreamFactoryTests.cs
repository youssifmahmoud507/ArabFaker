using ArabFaker.NET.Generation.Randomization;

namespace ArabFaker.Tests.Generation.Randomization
{
    public class RandomStreamFactoryTests
    {
        [Fact]
        public void SameSeedAndKeyAndIndex_ShouldProduceSameSequence()
        {
            // Arrange
            var firstFactory = new RandomStreamFactory(2026);
            var secondFactory = new RandomStreamFactory(2026);

            var firstStream = firstFactory.Create(
                "employee.email", 0);

            var secondStream = secondFactory.Create(
                "employee.email", 0);

            // Act
            var firstSequence = ReadSequence(firstStream, 10);
            var secondSequence = ReadSequence(secondStream, 10);

            // Assert
            Assert.Equal(firstSequence, secondSequence);
        }

        [Fact]
        public void DifferentStreamKeys_ShouldProduceDifferentSequences()
        {
            // Arrange
            var factory = new RandomStreamFactory(2026);

            var nameStream = factory.Create(
                "employee.name", 0);

            var emailStream = factory.Create(
                "employee.email", 0);

            // Act
            var nameSequence = ReadSequence(nameStream, 10);
            var emailSequence = ReadSequence(emailStream, 10);

            // Assert
            Assert.False(nameSequence.SequenceEqual(emailSequence));
        }

        [Fact]
        public void DifferentItemIndexes_ShouldProduceDifferentSequences()
        {
            // Arrange
            var factory = new RandomStreamFactory(2026);

            var firstStream = factory.Create(
                "employee.name", 0);

            var secondStream = factory.Create(
                "employee.name", 1);

            // Act
            var firstSequence = ReadSequence(firstStream, 10);
            var secondSequence = ReadSequence(secondStream, 10);

            // Assert
            Assert.False(firstSequence.SequenceEqual(secondSequence));
        }

        [Fact]
        public void DifferentRootSeeds_ShouldProduceDifferentSequences()
        {
            // Arrange
            var firstFactory = new RandomStreamFactory(2026);
            var secondFactory = new RandomStreamFactory(2027);

            var firstStream = firstFactory.Create("employee.name", 0);
            var secondStream = secondFactory.Create("employee.name", 0);

            // Act
            var firstSequence = ReadSequence(firstStream, 10);
            var secondSequence = ReadSequence(secondStream, 10);

            // Assert
            Assert.False(firstSequence.SequenceEqual(secondSequence));
        }

        [Fact]
        public void CreatingStreamsInDifferentOrder_ShouldNotAffectResults()
        {
            // Arrange
            var firstFactory = new RandomStreamFactory(2026);
            var secondFactory = new RandomStreamFactory(2026);

            // First factory: Name then Email
            var firstName = firstFactory
                .Create("employee.name", 0);

            var firstNameValue = firstName.NextUInt64();

            var firstEmail = firstFactory
                .Create("employee.email", 0);

            var firstEmailValue = firstEmail.NextUInt64();

            // Second factory: Email then Name
            var secondEmail = secondFactory
                .Create("employee.email", 0);

            var secondEmailValue = secondEmail.NextUInt64();

            var secondName = secondFactory
                .Create("employee.name", 0);

            var secondNameValue = secondName.NextUInt64();

            // Assert
            Assert.Equal(firstNameValue, secondNameValue);
            Assert.Equal(firstEmailValue, secondEmailValue);
        }

        [Fact]
        public void CreatingSameStreamAgain_ShouldStartFromInitialState()
        {
            // Arrange
            var factory = new RandomStreamFactory(2026);

            var firstStream = factory.Create("employee.name", 0);

            // Consume some values
            _ = ReadSequence(firstStream, 20);

            // Act
            var secondStream = factory.Create("employee.name", 0);

            var expectedStream = factory.Create("employee.name", 0);

            var actualSequence = ReadSequence(secondStream, 10);
            var expectedSequence = ReadSequence(expectedStream, 10);

            // Assert
            Assert.Equal(expectedSequence, actualSequence);
        }

        private static ulong[] ReadSequence(IRandomSource source, int count)
        {
            return [.. Enumerable.Range(0, count).Select(_ => source.NextUInt64())];
        }


    }
}
