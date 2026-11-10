using ArabFaker.NET.Providers.Arab.Reference;
using System.Reflection;

namespace ArabFaker.Tests.Providers.Arab.Reference
{
    public class ReferenceCatalogTests
    {
        private static IEnumerable<FieldInfo> CatalogFields() =>
            typeof(ReferenceCatalog).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        public static TheoryData<string> EntryLists => ToTheoryData(NamesOf(typeof(ReferenceEntry[])));

        public static TheoryData<string> GroupLists => ToTheoryData(NamesOf(typeof(ReferenceGroup[])));

        public static TheoryData<string> CodeLists => ToTheoryData(NamesOf(typeof(string[])));

        private static string[] NamesOf(Type type) =>
            CatalogFields().Where(x => x.FieldType == type).Select(x => x.Name).ToArray();

        private static TheoryData<string> ToTheoryData(string[] names)
        {
            var data = new TheoryData<string>();

            foreach (string name in names)
                data.Add(name);

            return data;
        }

        private static T Get<T>(string name) =>
            (T)typeof(ReferenceCatalog).GetField(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(null)!;

        private static bool IsArabicText(string text) =>
            text.Length > 0 && text.All(c => c == ' ' || (c >= '\u0600' && c <= '\u06FF'));

        private static bool IsEnglishText(string text) =>
            text.Length > 0 && char.IsUpper(text[0]) && text.All(c => char.IsLetter(c) || c is ' ' or '\'' or '-' or '&' or '+');

        private static void AssertValid(IEnumerable<ReferenceEntry> entries)
        {
            Assert.All(entries, entry =>
            {
                Assert.True(IsArabicText(entry.Arabic), $"'{entry.Arabic}' is not plain Arabic text");
                Assert.True(IsEnglishText(entry.English), $"'{entry.English}' is not plain English text");
            });
        }

        private static void AssertUnique(IReadOnlyCollection<ReferenceEntry> entries)
        {
            Assert.Equal(entries.Count, entries.Select(x => x.Arabic).Distinct().Count());
            Assert.Equal(entries.Count, entries.Select(x => x.English).Distinct().Count());
        }

        [Fact]
        public void Catalog_ShouldExposeTheExpectedNumberOfLists()
        {
            Assert.True(NamesOf(typeof(ReferenceEntry[])).Length >= 35);
            Assert.Equal(2, NamesOf(typeof(ReferenceGroup[])).Length);
            Assert.True(NamesOf(typeof(string[])).Length >= 5);
        }

        [Theory]
        [MemberData(nameof(EntryLists))]
        public void EntryList_ShouldBeUniqueAndWellFormed(string list)
        {
            var entries = Get<ReferenceEntry[]>(list);

            Assert.NotEmpty(entries);
            AssertValid(entries);
            AssertUnique(entries);
        }

        [Theory]
        [MemberData(nameof(GroupLists))]
        public void GroupList_ShouldHaveUniqueNamesAndNoSharedItems(string list)
        {
            var groups = Get<ReferenceGroup[]>(list);

            Assert.True(groups.Length >= 20);

            AssertValid(groups.Select(x => x.Group));
            AssertUnique(groups.Select(x => x.Group).ToArray());

            var items = groups.SelectMany(x => x.Items).ToArray();

            AssertValid(items);
            AssertUnique(items);

            Assert.All(groups, group => Assert.True(group.Items.Length >= 7, $"{group.Group.English} has too few items"));
        }

        [Theory]
        [MemberData(nameof(CodeLists))]
        public void CodeList_ShouldBeUniqueAndNotEmpty(string list)
        {
            var codes = Get<string[]>(list);

            Assert.NotEmpty(codes);
            Assert.Equal(codes.Length, codes.Distinct().Count());
            Assert.All(codes, code => Assert.False(string.IsNullOrWhiteSpace(code)));
        }

        [Fact]
        public void Lists_ShouldBeBigEnoughToAvoidObviousRepetition()
        {
            Assert.True(ReferenceCatalog.Universities.Length >= 40);
            Assert.True(ReferenceCatalog.Faculties.Length >= 30);
            Assert.True(ReferenceCatalog.Banks.Length >= 25);
            Assert.True(ReferenceCatalog.MedicalSpecialties.Length >= 45);
            Assert.True(ReferenceCatalog.ProductCategories.Length >= 50);
            Assert.True(ReferenceCatalog.Countries.Length >= 190);
            Assert.True(ReferenceCatalog.Languages.Length >= 45);
            Assert.True(ReferenceCatalog.CurrencyCodes.Length >= 60);
            Assert.Equal(7, ReferenceCatalog.Weekdays.Length);
            Assert.Equal(12, ReferenceCatalog.Months.Length);
            Assert.Equal(12, ReferenceCatalog.HijriMonths.Length);
            Assert.Equal(12, ReferenceCatalog.CopticMonths.Length);
        }

        [Fact]
        public void Catalog_ShouldHaveManyCoursesAndJobTitles()
        {
            Assert.True(ReferenceCatalog.AcademicDepartments.Sum(x => x.Items.Length) >= 350);
            Assert.True(ReferenceCatalog.WorkDepartments.Sum(x => x.Items.Length) >= 200);
        }

        [Fact]
        public void CurrencyCodes_ShouldBeThreeCapitalLetters()
        {
            Assert.All(ReferenceCatalog.CurrencyCodes, code => Assert.Matches("^[A-Z]{3}$", code));
            Assert.Contains("EGP", ReferenceCatalog.CurrencyCodes);
        }

        [Fact]
        public void BloodTypes_ShouldBeTheEightGroups()
        {
            Assert.Equal(8, ReferenceCatalog.BloodTypes.Length);
        }
    }
}
