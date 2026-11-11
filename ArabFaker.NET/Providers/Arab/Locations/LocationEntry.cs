namespace ArabFaker.NET.Providers.Arab.Locations
{
    /// <summary>
    /// One place in the locations catalog (governorate, district or shiyakha) with its Arabic and
    /// English names, parent id and coordinates.
    /// </summary>
    internal readonly record struct LocationEntry(string Id, LocationLevel Level, string ArabicName, string EnglishName, string? ParentId, double? Latitude, double? Longitude)
    {
        internal bool TryGetName(LocationLanguage language, out string name)
        {
            name = string.Empty;

            switch (language)
            {
                case LocationLanguage.Arabic:
                    name = ArabicName;
                    return true;

                case LocationLanguage.English:
                    name = EnglishName;
                    return true;

                default:
                    return false;
            }
        }
    }

}
