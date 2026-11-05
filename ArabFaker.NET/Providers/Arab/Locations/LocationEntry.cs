namespace ArabFaker.NET.Providers.Arab.Locations
{
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
