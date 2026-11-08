using ArabFaker.NET.Export;
using ArabFaker.NET.Generation;
using System.Globalization;
using System.Text.Json;

namespace ArabFaker.Tests.Export
{
    public class RowExporterTests
    {
        private static readonly Guid Token = Guid.Parse("11111111-2222-3333-4444-555555555555");

        private static ExpRow Row(string name = "Ali", int id = 1) => new()
        {
            BaseId = id,
            Name = name,
            Active = true,
            Price = 12.50m,
            Ratio = 0.5,
            Token = Token,
            Day = new DateOnly(2026, 1, 2),
            Stamp = new DateTime(2026, 1, 2, 3, 4, 5),
            Color = ExpColor.Green,
            Maybe = null,
            Notes = null
        };

        private const string Header = "BaseId,Name,Active,Price,Ratio,Token,Day,Stamp,Color,Maybe,Notes";

        // ---------- CSV ----------

        [Fact]
        public void ToCsv_ShouldWriteHeaderThenRowsWithBaseColumnsFirst()
        {
            var result = RowExporter.ToCsv(new[] { Row() });

            Assert.True(result.IsSuccess);
            Assert.Equal(
                Header + "\r\n1,Ali,true,12.50,0.5,11111111-2222-3333-4444-555555555555,2026-01-02,2026-01-02T03:04:05,Green,,\r\n",
                result.Value);
        }

        [Fact]
        public void ToCsv_ShouldQuoteCommasQuotesAndNewLines()
        {
            var result = RowExporter.ToCsv(new[] { Row("Ali, \"A\""), Row("two\nlines", 2) });

            var lines = result.Value!.Split("\r\n");

            Assert.Equal("1,\"Ali, \"\"A\"\"\",true,12.50,0.5,11111111-2222-3333-4444-555555555555,2026-01-02,2026-01-02T03:04:05,Green,,", lines[1]);
            Assert.StartsWith("2,\"two\nlines\",true", lines[2]);
        }

        [Fact]
        public void ToCsv_ShouldNotDependOnTheCurrentCulture()
        {
            var original = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var csv = RowExporter.ToCsv(new[] { Row() }).Value!;

                Assert.Contains(",12.50,0.5,", csv);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void ToCsv_NoRows_ShouldWriteOnlyTheHeader()
        {
            Assert.Equal(Header + "\r\n", RowExporter.ToCsv(Array.Empty<ExpRow>()).Value);
        }

        // ---------- JSON ----------

        [Fact]
        public void ToJson_ShouldWriteTypedValues()
        {
            var result = RowExporter.ToJson(new[] { Row() });

            using var document = JsonDocument.Parse(result.Value!);

            var item = document.RootElement[0];

            Assert.Equal(1, item.GetProperty("BaseId").GetInt32());
            Assert.Equal("Ali", item.GetProperty("Name").GetString());
            Assert.True(item.GetProperty("Active").GetBoolean());
            Assert.Equal(12.5m, item.GetProperty("Price").GetDecimal());
            Assert.Equal(0.5, item.GetProperty("Ratio").GetDouble());
            Assert.Equal(Token, item.GetProperty("Token").GetGuid());
            Assert.Equal("2026-01-02", item.GetProperty("Day").GetString());
            Assert.Equal("2026-01-02T03:04:05", item.GetProperty("Stamp").GetString());
            Assert.Equal("Green", item.GetProperty("Color").GetString());
            Assert.Equal(JsonValueKind.Null, item.GetProperty("Maybe").ValueKind);
            Assert.Equal(JsonValueKind.Null, item.GetProperty("Notes").ValueKind);
        }

        [Fact]
        public void ToJson_ShouldLeaveOutReadOnlyNavigationAndCollectionProperties()
        {
            var parent = Row("parent");
            var child = Row("child", 2);

            parent.Children.Add(child);
            child.Parent = parent; // a cycle must not matter

            var json = RowExporter.ToJson(new[] { parent, child }).Value!;

            Assert.DoesNotContain("Computed", json);
            Assert.DoesNotContain("Parent", json);
            Assert.DoesNotContain("Children", json);
        }

        [Fact]
        public void ToJson_ShouldKeepArabicTextReadable()
        {
            var json = RowExporter.ToJson(new[] { Row("أحمد") }).Value!;

            Assert.Contains("أحمد", json);
        }

        [Fact]
        public void ToJson_CamelCaseAndCompact_ShouldApplyOptions()
        {
            var json = RowExporter.ToJson(new[] { Row() }, new JsonExportOptions { CamelCase = true, Indented = false }).Value!;

            Assert.DoesNotContain("\n", json);
            Assert.Contains("\"baseId\":1", json);
            Assert.Contains("\"name\":\"Ali\"", json);
        }

        [Fact]
        public void ToJson_Indented_ShouldUseLineFeedsOnly()
        {
            var json = RowExporter.ToJson(new[] { Row() }).Value!;

            Assert.Contains("\n", json);
            Assert.DoesNotContain("\r", json);
        }

        [Fact]
        public void ToJson_NonFiniteNumbers_ShouldBecomeNull()
        {
            var row = Row();

            row.Ratio = double.NaN;

            using var document = JsonDocument.Parse(RowExporter.ToJson(new[] { row }).Value!);

            Assert.Equal(JsonValueKind.Null, document.RootElement[0].GetProperty("Ratio").ValueKind);
        }

        [Fact]
        public void ToJson_NoRows_ShouldWriteEmptyArray()
        {
            using var document = JsonDocument.Parse(RowExporter.ToJson(Array.Empty<ExpRow>()).Value!);

            Assert.Equal(0, document.RootElement.GetArrayLength());
        }

        // ---------- SQL ----------

        private const string Columns = "BaseId\", \"Name\", \"Active\", \"Price\", \"Ratio\", \"Token\", \"Day\", \"Stamp\", \"Color\", \"Maybe\", \"Notes\"";

        [Fact]
        public void ToSql_Standard_ShouldWriteOneInsertPerRow()
        {
            var result = RowExporter.ToSql(new[] { Row(), Row("Sara", 2) });

            Assert.True(result.IsSuccess);

            var lines = result.Value!.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(2, lines.Length);
            Assert.Equal(
                "INSERT INTO \"ExpRow\" (\"" + Columns + ") VALUES (1, 'Ali', TRUE, 12.50, 0.5, "
                + "'11111111-2222-3333-4444-555555555555', '2026-01-02', '2026-01-02 03:04:05', 'Green', NULL, NULL);",
                lines[0]);
        }

        [Fact]
        public void ToSql_ShouldEscapeSingleQuotes()
        {
            var sql = RowExporter.ToSql(new[] { Row("O'Neil") }).Value!;

            Assert.Contains("'O''Neil'", sql);
        }

        [Fact]
        public void ToSql_SqlServer_ShouldUseBracketsUnicodePrefixAndNumericBooleans()
        {
            var sql = RowExporter.ToSql(new[] { Row("أحمد") }, new SqlExportOptions { Dialect = SqlDialect.SqlServer }).Value!;

            Assert.StartsWith("INSERT INTO [ExpRow] ([BaseId], [Name],", sql);
            Assert.Contains("N'أحمد', 1, 12.50", sql);
        }

        [Fact]
        public void ToSql_MySql_ShouldUseBackticksAndEscapeBackslashes()
        {
            var sql = RowExporter.ToSql(new[] { Row("a\\b") }, new SqlExportOptions { Dialect = SqlDialect.MySql }).Value!;

            Assert.StartsWith("INSERT INTO `ExpRow` (`BaseId`,", sql);
            Assert.Contains("'a\\\\b'", sql);
        }

        [Fact]
        public void ToSql_EnumsAsNumbers_ShouldWriteUnderlyingValue()
        {
            var sql = RowExporter.ToSql(new[] { Row() }, new SqlExportOptions { EnumsAsText = false }).Value!;

            Assert.Contains("'2026-01-02 03:04:05', 2, NULL, NULL", sql);
        }

        [Fact]
        public void ToSql_CustomTableName_ShouldBeQuotedAndEscaped()
        {
            var sql = RowExporter.ToSql(new[] { Row() }, new SqlExportOptions { TableName = "my\"table" }).Value!;

            Assert.StartsWith("INSERT INTO \"my\"\"table\" (", sql);
        }

        [Fact]
        public void ToSql_NoRows_ShouldBeEmpty()
        {
            var result = RowExporter.ToSql(Array.Empty<ExpRow>());

            Assert.True(result.IsSuccess);
            Assert.Equal(string.Empty, result.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ToSql_BlankTableName_ShouldFail(string name)
        {
            var result = RowExporter.ToSql(new[] { Row() }, new SqlExportOptions { TableName = name });

            Assert.Equal(GenerationErrorCodes.InvalidExportOptions, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void ToSql_UndefinedDialect_ShouldFail()
        {
            var result = RowExporter.ToSql(new[] { Row() }, new SqlExportOptions { Dialect = (SqlDialect)99 });

            Assert.Equal(GenerationErrorCodes.InvalidExportOptions, Assert.Single(result.Errors).Code);
        }

        // ---------- Shared failures ----------

        [Fact]
        public void Export_NullRows_ShouldFail()
        {
            Assert.Equal(GenerationErrorCodes.NullRows, Assert.Single(RowExporter.ToCsv<ExpRow>(null).Errors).Code);
            Assert.Equal(GenerationErrorCodes.NullRows, Assert.Single(RowExporter.ToJson<ExpRow>(null).Errors).Code);
            Assert.Equal(GenerationErrorCodes.NullRows, Assert.Single(RowExporter.ToSql<ExpRow>(null).Errors).Code);
        }

        [Fact]
        public void Export_NullRowInside_ShouldFailWithItsIndex()
        {
            var result = RowExporter.ToCsv(new ExpRow?[] { Row(), null }!);

            var error = Assert.Single(result.Errors);

            Assert.Equal(GenerationErrorCodes.NullRow, error.Code);
            Assert.Contains("1", error.Message);
        }

        [Fact]
        public void Export_TypeWithoutScalarColumns_ShouldFail()
        {
            var rows = new[] { new ExpNoColumns() };

            Assert.Equal(GenerationErrorCodes.NoExportableColumns, Assert.Single(RowExporter.ToCsv(rows).Errors).Code);
            Assert.Equal(GenerationErrorCodes.NoExportableColumns, Assert.Single(RowExporter.ToJson(rows).Errors).Code);
            Assert.Equal(GenerationErrorCodes.NoExportableColumns, Assert.Single(RowExporter.ToSql(rows).Errors).Code);
        }
    }
}
