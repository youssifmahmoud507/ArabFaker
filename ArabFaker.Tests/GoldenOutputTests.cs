using ArabFaker.NET;
using ArabFaker.NET.Export;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;

namespace ArabFaker.Tests
{
    /// <summary>
    /// The library promises that the same seed gives the same data. These tests pin that promise to real output:
    /// if a catalog, a convention or the engine changes what a seed produces, the diff shows up here in the pull request.
    /// An intended change is accepted by regenerating the files (see <see cref="GoldenFile"/>) and committing them.
    /// </summary>
    public class GoldenOutputTests
    {
        public sealed class GoldenPerson
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public DateOnly BirthDate { get; set; }
        }

        public sealed class GoldenEmployee
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string NationalId { get; set; } = string.Empty;

            public string Department { get; set; } = string.Empty;

            public string JobTitle { get; set; } = string.Empty;

            public string BloodType { get; set; } = string.Empty;

            public string Bank { get; set; } = string.Empty;
        }

        public sealed class GoldenStudent
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;
        }

        public sealed class GoldenCourse
        {
            public int Id { get; set; }

            public string Title { get; set; } = string.Empty;
        }

        public sealed class GoldenEnrollment
        {
            public int Id { get; set; }

            public int StudentId { get; set; }

            public int CourseId { get; set; }
        }

        private const long Seed = 2026;

        [Theory]
        [InlineData("people-arabic.txt", NameLanguage.Arabic)]
        [InlineData("people-latin.txt", NameLanguage.Latin)]
        public void People_MatchTheGoldenFile(string fileName, NameLanguage language)
        {
            var people = new Faker(Seed).Define<GoldenPerson>().AutoFill(new AutoFillOptions { Language = language }).Generate(10);

            Assert.True(people.IsSuccess);

            GoldenFile.Verify(fileName,
                "=== csv ===\n" + RowExporter.ToCsv(people.Value).Value + "\n=== json ===\n" + RowExporter.ToJson(people.Value).Value);
        }

        [Theory]
        [InlineData("employees-arabic.txt", NameLanguage.Arabic)]
        [InlineData("employees-latin.txt", NameLanguage.Latin)]
        public void Employees_WithReferenceData_MatchTheGoldenFile(string fileName, NameLanguage language)
        {
            var rows = new Faker(Seed).Define<GoldenEmployee>().AutoFill(new AutoFillOptions { Language = language }).Generate(12);

            Assert.True(rows.IsSuccess);

            GoldenFile.Verify(fileName, RowExporter.ToJson(rows.Value).Value!);
        }

        [Theory]
        [InlineData("university-arabic-sqlserver.txt", NameLanguage.Arabic, SqlDialect.SqlServer)]
        [InlineData("university-latin-mysql.txt", NameLanguage.Latin, SqlDialect.MySql)]
        public void RelatedTables_MatchTheGoldenFile(string fileName, NameLanguage language, SqlDialect dialect)
        {
            var options = new AutoFillOptions { Language = language };
            var faker = new Faker(Seed);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<GoldenStudent>().AutoFill(options), 8, x => x.Id);

            var courses = dataset.Table(
                faker.Define<GoldenCourse>()
                    .AutoFill(options)
                    .RuleFor(x => x.Title, c => c.Reference.TryCourse(out string title, language) ? title : "Course"),
                5,
                x => x.Id);

            dataset.Table(faker.Define<GoldenEnrollment>().AutoFill(options), 20, x => x.Id)
                .ForeignKey(x => x.StudentId, students)
                .ForeignKey(x => x.CourseId, courses)
                .UniqueTogether(x => x.StudentId, x => x.CourseId);

            var built = dataset.Build();

            Assert.True(built.IsSuccess);

            GoldenFile.Verify(fileName,
                "=== json ===\n" + built.Value.ToJson().Value + "\n=== sql ===\n" + built.Value.ToSql(new SqlExportOptions { Dialect = dialect }).Value);
        }
    }
}
