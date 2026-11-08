using ArabFaker.NET.Providers.Arab.Names;

namespace ArabFaker.Tests.Providers.Arab.Names
{
    public class NameCatalogTests
    {
        public static TheoryData<string> ListNames => new() { "Male", "Female", "Family" };

        private static NameEntry[] GetList(string name) => name switch
        {
            "Male" => NameCatalog.MaleFirstNames,
            "Female" => NameCatalog.FemaleFirstNames,
            _ => NameCatalog.FamilyNames
        };

        private static bool IsArabicLetter(char c) => c >= '؀' && c <= 'ۿ';

        [Theory]
        [MemberData(nameof(ListNames))]
        public void List_ShouldBeLargeEnoughToAvoidObviousRepetition(string list)
        {
            Assert.True(GetList(list).Length >= 100);
        }

        [Theory]
        [MemberData(nameof(ListNames))]
        public void List_ShouldNotContainDuplicateArabicOrLatinSpellings(string list)
        {
            var entries = GetList(list);

            Assert.Equal(entries.Length, entries.Select(x => x.Arabic).Distinct().Count());
            Assert.Equal(entries.Length, entries.Select(x => x.Latin).Distinct().Count());
        }

        [Theory]
        [MemberData(nameof(ListNames))]
        public void Entries_ShouldHaveArabicLettersAndPlainLatinText(string list)
        {
            Assert.All(GetList(list), entry =>
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.Arabic));
                Assert.All(entry.Arabic, c => Assert.True(IsArabicLetter(c), $"'{entry.Arabic}' has a non-Arabic character"));

                Assert.False(string.IsNullOrWhiteSpace(entry.Latin));
                Assert.All(entry.Latin, c => Assert.True(c is >= 'A' and <= 'Z' or >= 'a' and <= 'z', $"'{entry.Latin}' has a non-Latin character"));
                Assert.True(char.IsUpper(entry.Latin[0]), $"'{entry.Latin}' should start with a capital letter");
            });
        }

        [Fact]
        public void MaleAndFemaleFirstNames_ShouldNotOverlap()
        {
            Assert.Empty(NameCatalog.MaleFirstNames.Select(x => x.Arabic).Intersect(NameCatalog.FemaleFirstNames.Select(x => x.Arabic)));
            Assert.Empty(NameCatalog.MaleFirstNames.Select(x => x.Latin).Intersect(NameCatalog.FemaleFirstNames.Select(x => x.Latin)));
        }

        [Fact]
        public void AllFirstNames_ShouldBeMaleThenFemale()
        {
            Assert.Equal(
                NameCatalog.MaleFirstNames.Concat(NameCatalog.FemaleFirstNames),
                NameCatalog.AllFirstNames);
        }
    }
}
