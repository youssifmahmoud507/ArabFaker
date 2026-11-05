using ArabFaker.NET;
using ArabFaker.NET.Generation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Providers.Common.Identifiers
{
    public class IdentifierGeneratorTests
    {
        private static FakerContext CreateContext(long seed = 2026)
        {
            var faker = new Faker(seed);

            return faker.CreatePublicContext("identifier-tests", 0UL);
        }

        [Fact]
        public void GenerateGuid_ShouldReturnValidGuid()
        {
            var context = CreateContext();

            Guid value = context.Identifiers.GenerateGuid();

            Assert.NotEqual(Guid.Empty, value);
        }

        [Fact]
        public void GenerateGuid_ShouldGenerateUuidVersionFour()
        {
            var context = CreateContext();

            Guid value = context.Identifiers.GenerateGuid();

            byte[] bytes = value.ToByteArray(bigEndian: true);

            Assert.Equal(4, bytes[6] >> 4);
            Assert.Equal(2, bytes[8] >> 6);
        }

        [Fact]
        public void GenerateGuidString_ShouldReturnValidGuidString()
        {
            var context = CreateContext();

            string value = context.Identifiers.GenerateGuidString();

            Assert.True(Guid.TryParseExact(value, "D", out _));
            Assert.Equal(36, value.Length);
        }

        [Fact]
        public void GenerateGuidString_ShouldUseStandardFormat()
        {
            var context = CreateContext();

            string value = context.Identifiers.GenerateGuidString();

            Assert.Equal(value, value.ToLowerInvariant());

            Assert.Equal('-', value[8]);
            Assert.Equal('-', value[13]);
            Assert.Equal('-', value[18]);
            Assert.Equal('-', value[23]);
        }

        [Fact]
        public void SameSeed_ShouldProduceSameGuid()
        {
            var firstContext = CreateContext(2026);
            var secondContext = CreateContext(2026);

            Guid first = firstContext.Identifiers.GenerateGuid();
            Guid second = secondContext.Identifiers.GenerateGuid();

            Assert.Equal(first, second);
        }

        [Fact]
        public void DifferentSeeds_ShouldProduceDifferentGuids()
        {
            var firstContext = CreateContext(2026);
            var secondContext = CreateContext(2027);

            Guid first = firstContext.Identifiers.GenerateGuid();
            Guid second = secondContext.Identifiers.GenerateGuid();

            Assert.NotEqual(first, second);
        }
    }
}
