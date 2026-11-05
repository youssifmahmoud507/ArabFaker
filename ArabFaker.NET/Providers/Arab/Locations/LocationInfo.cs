namespace ArabFaker.NET.Providers.Arab.Locations
{
    /// <summary>
    /// Represents a generated geographical location.
    /// </summary>
    public sealed record LocationInfo
    {
        internal LocationInfo(LocationEntry entry)
        {
            Id = entry.Id;
            Level = entry.Level;
            ArabicName = entry.ArabicName;
            EnglishName = entry.EnglishName;
            ParentId = entry.ParentId;
            Latitude = entry.Latitude;
            Longitude = entry.Longitude;
        }

        /// <summary>
        /// Gets the unique identifier of the location.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the administrative level.
        /// </summary>
        public LocationLevel Level { get; }

        /// <summary>
        /// Gets the Arabic name.
        /// </summary>
        public string ArabicName { get; }

        /// <summary>
        /// Gets the English name.
        /// </summary>
        public string EnglishName { get; }

        /// <summary>
        /// Gets the parent location identifier, if available.
        /// </summary>
        public string? ParentId { get; }

        /// <summary>
        /// Gets the latitude, if available.
        /// </summary>
        public double? Latitude { get; }

        /// <summary>
        /// Gets the longitude, if available.
        /// </summary>
        public double? Longitude { get; }
    }

}
