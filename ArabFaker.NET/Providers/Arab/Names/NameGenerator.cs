namespace ArabFaker.NET.Providers.Arab.Names
{
    /// <summary>
    /// Provides methods for generating Arabic and Latin names.
    /// </summary>
    public sealed class NameGenerator
    {
        private readonly NamesProvider _provider;

        internal NameGenerator(NamesProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Generates a first name.
        /// </summary>
        public bool TryFirstName(
            out string name,
            NameLanguage language = NameLanguage.Arabic,
            NameGender gender = NameGender.Any)
        {
            return _provider.TryFirstName(
                out name,
                language,
                gender);
        }

        /// <summary>
        /// Generates a middle name.
        /// </summary>
        public bool TryMiddleName(
            out string name,
            NameLanguage language = NameLanguage.Arabic,
            NameGender gender = NameGender.Any)
        {
            return _provider.TryMiddleName(
                out name,
                language,
                gender);
        }

        /// <summary>
        /// Generates a last name.
        /// </summary>
        public bool TryLastName(
            out string name,
            NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryLastName(
                out name,
                language);
        }

        /// <summary>
        /// Generates a full name.
        /// </summary>
        public bool TryFullName(out string name, NameLanguage language = NameLanguage.Arabic, NameGender gender = NameGender.Any)
        {
            return _provider.TryFullName(
                out name,
                language,
                gender);
        }
    }

}
