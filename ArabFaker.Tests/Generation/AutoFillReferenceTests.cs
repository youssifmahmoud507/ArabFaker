using ArabFaker.NET;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;
using ArabFaker.NET.Providers.Arab.Reference;

namespace ArabFaker.Tests.Generation
{
    internal sealed class RefEmployee
    {
        public int Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
    }

    internal sealed class RefStudent
    {
        public int Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string EducationLevel { get; set; } = string.Empty;
    }

    internal sealed class RefCourse
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string AcademicDepartment { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    internal sealed class RefClinic
    {
        public int Id { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ProductCategory { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class AutoFillReferenceTests
    {
        private static bool HasArabic(string text) => text.Any(c => c >= '؀' && c <= 'ۿ');

        private static bool Contains(ReferenceEntry[] entries, string value) =>
            entries.Any(x => x.Arabic == value || x.English == value);

        private static bool GroupNameContains(ReferenceGroup[] groups, string value) =>
            groups.Any(x => x.Group.Arabic == value || x.Group.English == value);

        [Theory]
        [InlineData(NameLanguage.Arabic)]
        [InlineData(NameLanguage.Latin)]
        public void AutoFill_ShouldFillWorkProperties(NameLanguage language)
        {
            var rows = new Faker(1).Define<RefEmployee>().AutoFill(new AutoFillOptions { Language = language }).Generate(40).Value!;

            Assert.All(rows, row =>
            {
                Assert.True(GroupNameContains(ReferenceCatalog.WorkDepartments, row.Department), row.Department);
                Assert.True(ReferenceCatalog.WorkDepartments.SelectMany(x => x.Items).Any(x => x.Arabic == row.JobTitle || x.English == row.JobTitle));
                Assert.True(Contains(ReferenceCatalog.Banks, row.Bank));
                Assert.Contains(row.BloodType, ReferenceCatalog.BloodTypes);
                Assert.True(Contains(ReferenceCatalog.Countries, row.Country));
                Assert.Contains(row.Currency, ReferenceCatalog.CurrencyCodes);
                Assert.True(Contains(ReferenceCatalog.MaritalStatuses, row.MaritalStatus));
                Assert.Equal(language == NameLanguage.Arabic, HasArabic(row.Bank));
            });
        }

        [Fact]
        public void AutoFill_DepartmentInAnAcademicClass_ShouldBeAnAcademicDepartment()
        {
            var students = new Faker(1).Define<RefStudent>().AutoFill().Generate(40).Value!;
            var courses = new Faker(1).Define<RefCourse>().AutoFill().Generate(40).Value!;

            Assert.All(students, row =>
            {
                Assert.True(GroupNameContains(ReferenceCatalog.AcademicDepartments, row.Department), row.Department);
                Assert.True(Contains(ReferenceCatalog.Universities, row.University));
                Assert.True(Contains(ReferenceCatalog.Faculties, row.Faculty));
                Assert.True(Contains(ReferenceCatalog.EducationLevels, row.EducationLevel));
            });

            Assert.All(courses, row =>
            {
                Assert.True(ReferenceCatalog.AcademicDepartments.SelectMany(x => x.Items).Any(x => x.Arabic == row.CourseName));
                Assert.True(GroupNameContains(ReferenceCatalog.AcademicDepartments, row.AcademicDepartment));
                Assert.True(GroupNameContains(ReferenceCatalog.AcademicDepartments, row.Department));
            });
        }

        [Fact]
        public void AutoFill_ShouldFillCommerceAndHealthProperties_AndLeaveAmbiguousOnesUnmapped()
        {
            var definition = new Faker(1).Define<RefClinic>().AutoFill();
            var rows = definition.Generate(40).Value!;

            Assert.All(rows, row =>
            {
                Assert.True(Contains(ReferenceCatalog.MedicalSpecialties, row.Specialty));
                Assert.True(Contains(ReferenceCatalog.Industries, row.Industry));
                Assert.True(Contains(ReferenceCatalog.ProductCategories, row.ProductCategory));
                Assert.True(Contains(ReferenceCatalog.PaymentMethods, row.PaymentMethod));
                Assert.True(Contains(ReferenceCatalog.Languages, row.Language));
                Assert.True(Contains(ReferenceCatalog.Colors, row.Color));
                Assert.Empty(row.Category);
                Assert.Empty(row.Title);
            });

            Assert.Equal(new[] { "Category", "Title" }, definition.UnmappedProperties.OrderBy(x => x));
        }
    }
}
