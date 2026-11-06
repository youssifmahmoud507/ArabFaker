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

            if (!TryPickFirstEntry(language, gender, out NameEntry selectedName))
                return false;

            return selectedName.TryGetValue(language, out name);
        }

        /// <summary>
        /// Picks a middle name. In Arabic naming the middle name is the father's name,
        /// so it is always taken from the male first names, whatever the gender is.
        /// </summary>
        internal bool TryMiddleName(out string name, NameLanguage language = NameLanguage.Arabic, NameGender gender = NameGender.Any)
        {
            name = string.Empty;

            if (!IsValidLanguage(language) || !Enum.IsDefined(gender))
                return false;

            if (!TryPickMiddleEntry(null, out NameEntry selectedName))
                return false;

            return selectedName.TryGetValue(language, out name);
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

            if (!TryPickFirstEntry(language, gender, out NameEntry first))
                return false;

            // The middle name must differ from the first name.
            if (!TryPickMiddleEntry(first, out NameEntry middle))
                return false;

            if (!_random.TryPick(NameCatalog.FamilyNames, out NameEntry last))
                return false;

            if (!first.TryGetValue(language, out string firstName) ||
                !middle.TryGetValue(language, out string middleName) ||
                !last.TryGetValue(language, out string lastName))
            {
                return false;
            }

            fullName = $"{firstName} {middleName} {lastName}";

            return true;
        }

        private static bool IsValidLanguage(NameLanguage language)
        {
            return language is NameLanguage.Arabic or NameLanguage.Latin;
        }

        private bool TryPickFirstEntry(NameLanguage language, NameGender gender, out NameEntry entry)
        {
            entry = default;

            if (!IsValidLanguage(language))
                return false;

            NameEntry[]? names = gender switch
            {
                NameGender.Male => NameCatalog.MaleFirstNames,
                NameGender.Female => NameCatalog.FemaleFirstNames,
                NameGender.Any => NameCatalog.AllFirstNames,
                _ => null
            };

            if (names is null)
                return false;

            return _random.TryPick(names, out entry);
        }

        private bool TryPickMiddleEntry(NameEntry? exclude, out NameEntry entry)
        {
            entry = default;

            NameEntry[] fathers = NameCatalog.MaleFirstNames;

            int excludedIndex = exclude is null ? -1 : Array.IndexOf(fathers, exclude.Value);

            if (excludedIndex < 0)
                return _random.TryPick(fathers, out entry);

            if (fathers.Length < 2)
                return false;

            // Pick uniformly among the remaining names by skipping the excluded one.
            if (!_random.TryNextInt(0, fathers.Length - 1, out int index))
                return false;

            if (index >= excludedIndex)
                index++;

            entry = fathers[index];

            return true;
        }
    }

}
