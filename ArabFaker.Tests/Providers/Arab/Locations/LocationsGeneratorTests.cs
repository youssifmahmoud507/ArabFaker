using ArabFaker.NET;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Locations;

namespace ArabFaker.Tests.Providers.Arab.Locations
{
    public partial class LocationsProviderTests
    {
        public class LocationsGeneratorTests
        {
            private static FakerContext CreateContext(long seed = 2026)
            {
                var faker = new Faker(seed);

                return faker.CreatePublicContext(
                    "public-locations-tests",
                    0UL);
            }

            [Fact]
            public void TryGovernorate_ShouldReturnValidLocation()
            {
                var context = CreateContext();

                bool success = context.Locations.TryGovernorate(
                    out var governorate);

                Assert.True(success);
                Assert.NotNull(governorate);

                Assert.Equal(
                    LocationLevel.Governorate,
                    governorate.Level);

                Assert.False(string.IsNullOrWhiteSpace(governorate.Id));
                Assert.False(string.IsNullOrWhiteSpace(governorate.ArabicName));
                Assert.False(string.IsNullOrWhiteSpace(governorate.EnglishName));

                Assert.Null(governorate.ParentId);
            }

            [Fact]
            public void TryPick_ShouldReturnRequestedLevel()
            {
                var context = CreateContext();

                bool success = context.Locations.TryPick(
                    LocationLevel.Governorate,
                    out var location);

                Assert.True(success);
                Assert.Equal(LocationLevel.Governorate, location.Level);
            }

            [Fact]
            public void TryDistrict_ShouldReturnChildOfSelectedGovernorate()
            {
                var context = CreateContext();

                bool governorateSuccess =
                    context.Locations.TryGovernorate(out var governorate);

                Assert.True(governorateSuccess);

                bool districtSuccess =
                    context.Locations.TryDistrict(
                        governorate.Id,
                        out var district);

                Assert.True(districtSuccess);

                Assert.Equal(LocationLevel.District, district.Level);
                Assert.Equal(governorate.Id, district.ParentId);
            }

            [Fact]
            public void TryShiyakha_ShouldReturnChildOfSelectedDistrict()
            {
                var context = CreateContext();

                bool governorateSuccess =
                    context.Locations.TryGovernorate(out var governorate);

                Assert.True(governorateSuccess);

                bool districtSuccess =
                    context.Locations.TryDistrict(
                        governorate.Id,
                        out var district);

                Assert.True(districtSuccess);

                bool shiyakhaSuccess =
                    context.Locations.TryShiyakha(
                        district.Id,
                        out var shiyakha);

                Assert.True(shiyakhaSuccess);

                Assert.Equal(LocationLevel.Shiyakha, shiyakha.Level);
                Assert.Equal(district.Id, shiyakha.ParentId);
            }

            [Fact]
            public void TryPickName_ShouldReturnArabicName()
            {
                var context = CreateContext();

                bool success = context.Locations.TryPickName(
                    LocationLevel.Governorate,
                    LocationLanguage.Arabic,
                    out string name);

                Assert.True(success);
                Assert.False(string.IsNullOrWhiteSpace(name));
            }

            [Fact]
            public void TryPickName_ShouldReturnEnglishName()
            {
                var context = CreateContext();

                bool success = context.Locations.TryPickName(
                    LocationLevel.Governorate,
                    LocationLanguage.English,
                    out string name);

                Assert.True(success);
                Assert.False(string.IsNullOrWhiteSpace(name));
            }

            [Fact]
            public void TryPick_ShouldRejectInvalidLevel()
            {
                var context = CreateContext();

                bool success = context.Locations.TryPick(
                    (LocationLevel)999,
                    out var location);

                Assert.False(success);
                Assert.Null(location);
            }

            [Fact]
            public void TryDistrict_ShouldRejectUnknownGovernorate()
            {
                var context = CreateContext();

                bool success = context.Locations.TryDistrict(
                    "INVALID-ID",
                    out var location);

                Assert.False(success);
                Assert.Null(location);
            }

            [Fact]
            public void TryShiyakha_ShouldRejectUnknownDistrict()
            {
                var context = CreateContext();

                bool success = context.Locations.TryShiyakha(
                    "INVALID-ID",
                    out var location);

                Assert.False(success);
                Assert.Null(location);
            }

            [Fact]
            public void TryPickName_ShouldRejectInvalidLanguage()
            {
                var context = CreateContext();

                bool success = context.Locations.TryPickName(
                    LocationLevel.Governorate,
                    (LocationLanguage)999,
                    out string name);

                Assert.False(success);
                Assert.Equal(string.Empty, name);
            }

            [Fact]
            public void SameSeed_ShouldProduceSameGovernorate()
            {
                var firstContext = CreateContext(2026);
                var secondContext = CreateContext(2026);

                bool firstSuccess =
                    firstContext.Locations.TryGovernorate(out var first);

                bool secondSuccess =
                    secondContext.Locations.TryGovernorate(out var second);

                Assert.True(firstSuccess);
                Assert.True(secondSuccess);

                Assert.Equal(first.Id, second.Id);
                Assert.Equal(first.ArabicName, second.ArabicName);
                Assert.Equal(first.EnglishName, second.EnglishName);
            }
        }
    }
}
