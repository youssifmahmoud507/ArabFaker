using ArabFaker.NET.Providers.Arab.Reference;

namespace ArabFaker.Tests.Providers.Arab.Reference
{
    public class ReferenceCatalogTests
    {
        public static TheoryData<string> EntryLists => new()
        {
            "Faculties", "Universities", "EducationLevels", "AcademicYears", "Industries",
            "ProductCategories", "PaymentMethods", "Banks", "MedicalSpecialties", "MaritalStatuses",
            "Countries", "Languages", "Colors", "Weekdays", "Months"
        };

        private static ReferenceEntry[] GetList(string name) => name switch
        {
            "Faculties" => ReferenceCatalog.Faculties,
            "Universities" => ReferenceCatalog.Universities,
            "EducationLevels" => ReferenceCatalog.EducationLevels,
            "AcademicYears" => ReferenceCatalog.AcademicYears,
            "Industries" => ReferenceCatalog.Industries,
            "ProductCategories" => ReferenceCatalog.ProductCategories,
            "PaymentMethods" => ReferenceCatalog.PaymentMethods,
            "Banks" => ReferenceCatalog.Banks,
            "MedicalSpecialties" => ReferenceCatalog.MedicalSpecialties,
            "MaritalStatuses" => ReferenceCatalog.MaritalStatuses,
            "Countries" => ReferenceCatalog.Countries,
            "Languages" => ReferenceCatalog.Languages,
            "Colors" => ReferenceCatalog.Colors,
            "Weekdays" => ReferenceCatalog.Weekdays,
            "Months" => ReferenceCatalog.Months,
            _ => throw new ArgumentException(name)
        };

        private static bool IsArabicText(string text) =>
            text.Length > 0 && text.All(c => c == ' ' || (c >= '؀' && c <= 'ۿ'));

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

        [Theory]
        [MemberData(nameof(EntryLists))]
        public void List_ShouldBeUniqueAndWellFormed(string list)
        {
            var entries = GetList(list);

            Assert.NotEmpty(entries);
            AssertValid(entries);
            AssertUnique(entries);
        }

        [Fact]
        public void Lists_ShouldBeBigEnoughToAvoidObviousRepetition()
        {
            Assert.True(ReferenceCatalog.Universities.Length >= 30);
            Assert.True(ReferenceCatalog.Faculties.Length >= 20);
            Assert.True(ReferenceCatalog.Banks.Length >= 15);
            Assert.True(ReferenceCatalog.MedicalSpecialties.Length >= 30);
            Assert.True(ReferenceCatalog.ProductCategories.Length >= 30);
            Assert.True(ReferenceCatalog.Countries.Length >= 50);
            Assert.Equal(7, ReferenceCatalog.Weekdays.Length);
            Assert.Equal(12, ReferenceCatalog.Months.Length);
        }

        [Theory]
        [InlineData("Academic")]
        [InlineData("Work")]
        public void Groups_ShouldHaveUniqueNamesAndNoSharedItems(string kind)
        {
            var groups = kind == "Academic" ? ReferenceCatalog.AcademicDepartments : ReferenceCatalog.WorkDepartments;

            Assert.True(groups.Length >= 15);

            AssertUnique(groups.Select(x => x.Group).ToArray());

            var items = groups.SelectMany(x => x.Items).ToArray();

            AssertValid(groups.Select(x => x.Group));
            AssertValid(items);
            AssertUnique(items);

            Assert.All(groups, group => Assert.True(group.Items.Length >= 7, $"{group.Group.English} has too few items"));
        }

        [Fact]
        public void Catalog_ShouldHaveManyCoursesAndJobTitles()
        {
            Assert.True(ReferenceCatalog.AcademicDepartments.Sum(x => x.Items.Length) >= 150);
            Assert.True(ReferenceCatalog.WorkDepartments.Sum(x => x.Items.Length) >= 100);
        }

        [Fact]
        public void CodeLists_ShouldBeUniqueAndNotEmpty()
        {
            Assert.Equal(8, ReferenceCatalog.BloodTypes.Distinct().Count());
            Assert.Equal(ReferenceCatalog.CurrencyCodes.Length, ReferenceCatalog.CurrencyCodes.Distinct().Count());
            Assert.All(ReferenceCatalog.CurrencyCodes, code => Assert.Matches("^[A-Z]{3}$", code));
            Assert.Contains("EGP", ReferenceCatalog.CurrencyCodes);
        }
    }
}
