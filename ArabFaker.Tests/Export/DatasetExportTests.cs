using ArabFaker.NET;
using ArabFaker.NET.Dataset;
using ArabFaker.NET.Export;
using ArabFaker.NET.Generation;
using ArabFaker.Tests.Dataset;
using System.Text.Json;

namespace ArabFaker.Tests.Export
{
    public class DatasetExportTests
    {
        private static GeneratedDataset BuildSchool(Faker faker, int students, int courses, int enrollments)
        {
            var dataset = faker.CreateDataset();

            // The child is registered first on purpose: the SQL must still list parents first.
            var enrollmentTable = dataset.Table(faker.Define<NvEnrollment>().AutoFill(), enrollments, x => x.Id);
            var studentTable = dataset.Table(faker.Define<NvStudent>().AutoFill(), students, x => x.Id);
            var courseTable = dataset.Table(faker.Define<NvCourse>().AutoFill(), courses, x => x.Id);

            enrollmentTable
                .ForeignKey(x => x.StudentId, studentTable)
                .ForeignKey(x => x.CourseId, courseTable)
                .Navigation(x => x.StudentId, x => x.Student)
                .Navigation(x => x.CourseId, x => x.Course);

            studentTable.Collection(x => x.Enrollments, enrollmentTable, e => e.StudentId);

            return dataset.Build().Value!;
        }

        [Fact]
        public void ToSql_ShouldListParentTablesBeforeTheTablesThatReferenceThem()
        {
            var data = BuildSchool(new Faker(2026), 5, 3, 20);

            var lines = data.ToSql().Value!.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5 + 3 + 20, lines.Length);

            int lastParent = Array.FindLastIndex(lines, l => l.StartsWith("INSERT INTO \"NvStudent\"") || l.StartsWith("INSERT INTO \"NvCourse\""));
            int firstChild = Array.FindIndex(lines, l => l.StartsWith("INSERT INTO \"NvEnrollment\""));

            Assert.True(lastParent < firstChild);
        }

        [Fact]
        public void ToSql_ShouldExportForeignKeysButNotNavigationObjects()
        {
            var data = BuildSchool(new Faker(2026), 3, 3, 5);

            var sql = data.ToSql().Value!;

            Assert.Contains("(\"Id\", \"StudentId\", \"CourseId\")", sql);
            Assert.DoesNotContain("\"Student\"", sql);
            Assert.DoesNotContain("\"Course\"", sql);
        }

        [Fact]
        public void ToSql_EverySqlForeignKeyValueShouldExistInTheReferencedTable()
        {
            var data = BuildSchool(new Faker(2026), 4, 3, 40);

            var lines = data.ToSql().Value!.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var studentIds = lines.Where(l => l.StartsWith("INSERT INTO \"NvStudent\""))
                .Select(l => l[(l.IndexOf("VALUES (", StringComparison.Ordinal) + 8)..].TrimEnd(';', ')'))
                .ToHashSet();

            var enrollmentStudentIds = lines.Where(l => l.StartsWith("INSERT INTO \"NvEnrollment\""))
                .Select(l => l[(l.IndexOf("VALUES (", StringComparison.Ordinal) + 8)..].TrimEnd(';', ')').Split(", ")[1]);

            Assert.All(enrollmentStudentIds, id => Assert.Contains(id, studentIds));
        }

        [Fact]
        public void ToSql_TablesThatReferenceEachOther_ShouldFallBackToRegistrationOrder()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var left = dataset.Table(faker.Define<DsLeft>().AutoFill(), 2, x => x.Id);
            var right = dataset.Table(faker.Define<DsRight>().AutoFill(), 2, x => x.Id);

            left.ForeignKey(x => x.RightId, right);
            right.ForeignKey(x => x.LeftId, left);

            var lines = dataset.Build().Value!.ToSql().Value!.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.StartsWith("INSERT INTO \"DsLeft\"", lines[0]);
            Assert.StartsWith("INSERT INTO \"DsRight\"", lines[2]);
        }

        [Fact]
        public void ToSql_SqlServerDialect_ShouldApplyToEveryTable()
        {
            var data = BuildSchool(new Faker(2026), 2, 2, 3);

            var sql = data.ToSql(new SqlExportOptions { Dialect = SqlDialect.SqlServer }).Value!;

            Assert.Contains("INSERT INTO [NvStudent]", sql);
            Assert.Contains("INSERT INTO [NvEnrollment]", sql);
            Assert.DoesNotContain("\"", sql);
        }

        [Fact]
        public void ToSql_InvalidOptions_ShouldFail()
        {
            var data = BuildSchool(new Faker(2026), 1, 1, 1);

            var result = data.ToSql(new SqlExportOptions { TableName = " " });

            Assert.Equal(GenerationErrorCodes.InvalidExportOptions, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void ToJson_ShouldWriteOneArrayPerTableInRegistrationOrder()
        {
            var data = BuildSchool(new Faker(2026), 5, 3, 20);

            using var document = JsonDocument.Parse(data.ToJson().Value!);

            var names = document.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

            Assert.Equal(new[] { "NvEnrollment", "NvStudent", "NvCourse" }, names);
            Assert.Equal(20, document.RootElement.GetProperty("NvEnrollment").GetArrayLength());
            Assert.Equal(5, document.RootElement.GetProperty("NvStudent").GetArrayLength());
        }

        [Fact]
        public void ToJson_CamelCase_ShouldApplyToTablesAndColumns()
        {
            var data = BuildSchool(new Faker(2026), 2, 2, 3);

            using var document = JsonDocument.Parse(data.ToJson(new JsonExportOptions { CamelCase = true }).Value!);

            var enrollment = document.RootElement.GetProperty("nvEnrollment")[0];

            Assert.True(enrollment.TryGetProperty("studentId", out _));
        }

        [Fact]
        public void ToJson_ShouldNotFailBecauseOfNavigationCycles()
        {
            var data = BuildSchool(new Faker(2026), 3, 3, 10);

            Assert.True(data.ToJson().IsSuccess);
        }

        [Fact]
        public void Export_TwoTablesWithTheSameRowType_ShouldReportNameClash()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsStudent>().AutoFill(), 2, x => x.Id);
            dataset.Table(faker.Define<DsStudent>().AutoFill(), 2, x => x.Id);

            var data = dataset.Build().Value!;

            Assert.Equal(GenerationErrorCodes.DuplicateTableName, Assert.Single(data.ToSql().Errors).Code);
            Assert.Equal(GenerationErrorCodes.DuplicateTableName, Assert.Single(data.ToJson().Errors).Code);
        }

        [Fact]
        public void Export_SingleSequenceFromDefinition_ShouldWorkWithoutADataset()
        {
            var rows = new Faker(2026).Define<DsStudent>().AutoFill().Generate(3).Value!;

            var csv = RowExporter.ToCsv(rows).Value!;

            Assert.StartsWith("Id,FirstName\r\n1,", csv);
        }
    }
}
