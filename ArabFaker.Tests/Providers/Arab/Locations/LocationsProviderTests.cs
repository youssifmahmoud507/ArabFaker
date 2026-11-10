﻿using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using ArabFaker.NET.Providers.Arab.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Providers.Arab.Locations
{
    public partial class LocationsProviderTests
    {
        private static GenerationContext CreateContext(long seed = 2026)
        {
            var factory = new RandomStreamFactory((ulong)seed);

            return new GenerationContext(factory, "locations-provider-tests", 0);
        }

        [Fact]
        public void LocationFiles_ShouldBeEmbeddedResources()
        {
            var assembly = typeof(LocationCatalog).Assembly;

            var resources = assembly.GetManifestResourceNames();

            string[] expectedResources = ["ArabFaker.Data.Locations.Egypt.all-governorate.json", "ArabFaker.Data.Locations.Egypt.all-district.json", "ArabFaker.Data.Locations.Egypt.all-shiyakha.json"
            ];

            var missingResources = expectedResources.Where(expected => !resources.Contains(expected)).ToArray();

            Assert.True(missingResources.Length == 0, $"Missing embedded resources: {string.Join(", ", missingResources)}" + Environment.NewLine + "Available resources:" + Environment.NewLine + string.Join(Environment.NewLine, resources));
        }


        [Fact]
        public void TryGovernorate_ShouldReturnValidLocation()
        {
            var context = CreateContext();

            bool success = context.Locations.TryGovernorate(
                out var governorate);

            Assert.True(success);

            Assert.Equal(
                LocationLevel.Governorate,
                governorate.Level);

            Assert.False(string.IsNullOrWhiteSpace(governorate.Id));
            Assert.False(string.IsNullOrWhiteSpace(governorate.ArabicName));
            Assert.False(string.IsNullOrWhiteSpace(governorate.EnglishName));

            Assert.Null(governorate.ParentId);
        }

        [Fact]
        public void TryDistrict_ShouldReturnChildOfRequestedGovernorate()
        {
            var context = CreateContext();

            const string governorateId = "EG14";

            for (int i = 0; i < 100; i++)
            {
                bool success = context.Locations.TryDistrict(
                    governorateId,
                    out var district);

                Assert.True(success);

                Assert.Equal(
                    LocationLevel.District,
                    district.Level);

                Assert.Equal(
                    governorateId,
                    district.ParentId);
            }
        }

        [Fact]
        public void TryShiyakha_ShouldReturnChildOfRequestedDistrict()
        {
            var context = CreateContext();

            const string districtId = "EG1413";

            for (int i = 0; i < 100; i++)
            {
                bool success = context.Locations.TryShiyakha(
                    districtId,
                    out var shiyakha);

                Assert.True(success);

                Assert.Equal(
                    LocationLevel.Shiyakha,
                    shiyakha.Level);

                Assert.Equal(
                    districtId,
                    shiyakha.ParentId);
            }
        }

        [Fact]
        public void TryPick_ShouldRejectInvalidLevel()
        {
            var context = CreateContext();

            bool success = context.Locations.TryPick(
                (LocationLevel)999,
                out var location);

            Assert.False(success);
            Assert.Equal(default, location);
        }

        [Fact]
        public void TryDistrict_ShouldRejectUnknownGovernorate()
        {
            var context = CreateContext();

            bool success = context.Locations.TryDistrict(
                "INVALID-ID",
                out var location);

            Assert.False(success);
            Assert.Equal(default, location);
        }

        [Fact]
        public void TryShiyakha_ShouldRejectUnknownDistrict()
        {
            var context = CreateContext();

            bool success = context.Locations.TryShiyakha(
                "INVALID-ID",
                out var location);

            Assert.False(success);
            Assert.Equal(default, location);
        }

        [Fact]
        public void TryPickName_ShouldReturnArabicName()
        {
            var context = CreateContext();

            bool success = context.Locations.TryPickName(
                LocationLevel.Governorate,
                LocationLanguage.Arabic,
                out var name);

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
                out var name);

            Assert.True(success);
            Assert.False(string.IsNullOrWhiteSpace(name));
        }

        [Fact]
        public void TryPickName_ShouldRejectInvalidLanguage()
        {
            var context = CreateContext();

            bool success = context.Locations.TryPickName(
                LocationLevel.Governorate,
                (LocationLanguage)999,
                out var name);

            Assert.False(success);
            Assert.Equal(string.Empty, name);
        }

        [Fact]
        public void SameSeed_ShouldProduceSameGovernorate()
        {
            var firstContext = CreateContext();
            var secondContext = CreateContext();

            bool firstSuccess =
                firstContext.Locations.TryGovernorate(
                    out var first);

            bool secondSuccess = secondContext.Locations.TryGovernorate(out var second);

            Assert.True(firstSuccess);
            Assert.True(secondSuccess);

            Assert.Equal(first.Id, second.Id);
            Assert.Equal(first.ArabicName, second.ArabicName);
            Assert.Equal(first.EnglishName, second.EnglishName);
        }

        [Fact]
        public void Catalog_ShouldContainAllThreeLevels()
        {
            bool success = LocationCatalog.TryGet(out var catalog);

            Assert.True(success);

            Assert.True(catalog.GetCount(LocationLevel.Governorate) >= 27);

            Assert.True(catalog.GetCount(LocationLevel.District) >= 365);

            Assert.True(catalog.GetCount(LocationLevel.Shiyakha) >= 5716);
        }
    }
}
