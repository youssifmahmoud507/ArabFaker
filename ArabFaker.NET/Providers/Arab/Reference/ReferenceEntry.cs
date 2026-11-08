using ArabFaker.NET.Providers.Arab.Names;

namespace ArabFaker.NET.Providers.Arab.Reference
{
    /// <summary>A value with its Arabic and English spelling.</summary>
    internal readonly record struct ReferenceEntry(string Arabic, string English)
    {
        internal string GetValue(NameLanguage language)
        {
            return language == NameLanguage.Latin ? English : Arabic;
        }

        internal bool Matches(string text)
        {
            return string.Equals(Arabic, text, StringComparison.Ordinal)
                || string.Equals(English, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>A named group of entries, such as a department and the courses it teaches.</summary>
    internal readonly record struct ReferenceGroup(ReferenceEntry Group, ReferenceEntry[] Items);
}
