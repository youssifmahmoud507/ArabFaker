using ArabFaker.NET.Generation.Randomization;

namespace ArabFaker.NET.Providers.Arab.Names
{
    internal sealed class NamesProvider
    {
        private readonly RandomGenerator _random;

        internal NamesProvider(RandomGenerator random)
        {
            _random = random;
        }

        internal bool TryFirstName(out string name, NameLanguage language = NameLanguage.Arabic, NameGender gender = NameGender.Any)
        {
            name = string.Empty;

            var names = gender switch
            {
                NameGender.Male => NameCatalog.MaleFirstNames,

                NameGender.Female => NameCatalog.FemaleFirstNames,

                NameGender.Any => NameCatalog.AllFirstNames,

                _ => null
            };

            if (names is null)
                return false;


            if (language is not NameLanguage.Arabic and not NameLanguage.Latin)
            {
                return false;
            }

            if (!_random.TryPick(names, out var selectedName))
                return false;


            return selectedName.TryGetValue(language, out name);
        }

        internal bool TryMiddleName(out string name, NameLanguage language = NameLanguage.Arabic, NameGender gender = NameGender.Any)
        {

            return TryFirstName(out name, language, gender);
        }

        internal bool TryLastName(out string name, NameLanguage language = NameLanguage.Arabic)
        {
            name = string.Empty;

            if (language is not NameLanguage.Arabic and not NameLanguage.Latin)
            {
                return false;
            }

            if (!_random.TryPick(NameCatalog.FamilyNames, out var selectedName))
            {
                return false;
            }

            return selectedName.TryGetValue(language, out name);
        }

        internal bool TryFullName(out string fullName, NameLanguage language = NameLanguage.Arabic, NameGender gender = NameGender.Any)
        {
            fullName = string.Empty;

            if (!TryFirstName(out var firstName, language, gender))
                return false;

            if (!TryMiddleName(out var middleName, language, gender))
                return false;

            if (!TryLastName(out var lastName, language))
                return false;

            fullName = $"{firstName} {middleName} {lastName}";

            return true;
        }
    }

}
