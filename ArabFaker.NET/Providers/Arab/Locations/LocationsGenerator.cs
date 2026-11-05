namespace ArabFaker.NET.Providers.Arab.Locations
{
    /// <summary>
    /// Provides methods for generating geographical locations.
    /// </summary>
    public sealed class LocationsGenerator
    {
        private readonly LocationsProvider _provider;

        internal LocationsGenerator(LocationsProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Generates a random location from the specified administrative level.
        /// </summary>
        public bool TryPick(LocationLevel level, out LocationInfo location)
        {
            location = null!;

            if (!_provider.TryPick(level, out LocationEntry entry))
                return false;

            location = new LocationInfo(entry);

            return true;
        }

        /// <summary>
        /// Generates a random governorate.
        /// </summary>
        public bool TryGovernorate(out LocationInfo location)
        {
            return TryPick(LocationLevel.Governorate, out location);
        }

        /// <summary>
        /// Generates a random district inside the specified governorate.
        /// </summary>
        public bool TryDistrict(string governorateId, out LocationInfo location)
        {
            location = null!;

            if (!_provider.TryDistrict(
                governorateId,
                out LocationEntry entry))
            {
                return false;
            }

            location = new LocationInfo(entry);

            return true;
        }

        /// <summary>
        /// Generates a random shiyakha inside the specified district.
        /// </summary>
        public bool TryShiyakha(string districtId, out LocationInfo location)
        {
            location = null!;

            if (!_provider.TryShiyakha(
                districtId,
                out LocationEntry entry))
            {
                return false;
            }

            location = new LocationInfo(entry);

            return true;
        }

        /// <summary>
        /// Generates a random location name using the specified language.
        /// </summary>
        public bool TryPickName(LocationLevel level, LocationLanguage language, out string name)
        {
            return _provider.TryPickName(
                level,
                language,
                out name);
        }
    }

}
