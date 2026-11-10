using System.Text;
using System.Text.Json;
using ArabFaker.NET.Export;
using Microsoft.Data.Sqlite;

namespace ArabFaker.Tests.Security
{
    /// <summary>
    /// Hostile text must stay data: it may never end a SQL string, a CSV cell or a JSON string early.
    /// The SQL checks run against a real SQLite database, so a wrong escape would show up as a failed query.
    /// </summary>
    [Trait("Category", "Fuzz")]
    public class ExportSecurityTests
    {
        public sealed class Row
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;
        }

        private static readonly string[] Payloads =
        {
            "'; DROP TABLE People; --",
            "' OR '1'='1",
            "O'Brien",
            "''''",
            "back\\slash\\",
            "\\'; DROP TABLE People; --",
            "line1\r\nline2\nline3",
            "a,b,\"c\"",
            "\"quoted\"",
            "=cmd|'/c calc'!A1",
            "]; DROP TABLE People; --",
            "`backtick`",
            "محمد ' أحمد",
            "😀 emoji",
            "; SELECT 1; --",
            "/* comment */",
            "-- comment",
            string.Empty,
            "   "
        };

        private static SqliteConnection OpenDatabase()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            using SqliteCommand create = connection.CreateCommand();
            create.CommandText = "CREATE TABLE \"People\" (\"Id\" INTEGER NOT NULL, \"Name\" TEXT NOT NULL);";
            create.ExecuteNonQuery();

            return connection;
        }

        private static void Execute(SqliteConnection connection, string script)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = script;
            command.ExecuteNonQuery();
        }

        private static List<(long Id, string Name)> ReadAll(SqliteConnection connection)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT \"Id\", \"Name\" FROM \"People\" ORDER BY \"Id\";";

            var rows = new List<(long, string)>();
            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
                rows.Add((reader.GetInt64(0), reader.GetString(1)));

            return rows;
        }

        private static string RandomText(Random random)
        {
            const string alphabet = "abcXYZ019 '\"\\;,-/*=`[]()\r\n\t\u0645\u062D\u0645\u062F";
            int length = random.Next(0, 40);
            var builder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                builder.Append(alphabet[random.Next(alphabet.Length)]);

            return builder.ToString();
        }

        [Fact]
        public void Sql_HostileText_RoundTripsThroughARealDatabase()
        {
            using SqliteConnection connection = OpenDatabase();
            var rows = Payloads.Select((text, index) => new Row { Id = index, Name = text }).ToList();

            var result = RowExporter.ToSql(rows, new SqlExportOptions { TableName = "People" });

            Assert.True(result.IsSuccess);
            Execute(connection, result.Value!);

            var stored = ReadAll(connection);

            Assert.Equal(rows.Count, stored.Count);
            for (int i = 0; i < rows.Count; i++)
                Assert.Equal(rows[i].Name, stored[i].Name);
        }

        [Fact]
        public void Sql_RandomText_RoundTripsThroughARealDatabase()
        {
            using SqliteConnection connection = OpenDatabase();
            var random = new Random(20260610);
            var rows = Enumerable.Range(0, 500).Select(i => new Row { Id = i, Name = RandomText(random) }).ToList();

            var result = RowExporter.ToSql(rows, new SqlExportOptions { TableName = "People" });

            Assert.True(result.IsSuccess);
            Execute(connection, result.Value!);

            var stored = ReadAll(connection);

            Assert.Equal(rows.Count, stored.Count);
            for (int i = 0; i < rows.Count; i++)
                Assert.Equal(rows[i].Name, stored[i].Name);
        }

        [Fact]
        public void Sql_InjectionPayload_DoesNotDropTheTable()
        {
            using SqliteConnection connection = OpenDatabase();
            var rows = new[] { new Row { Id = 1, Name = "x'); DROP TABLE \"People\"; --" } };

            var result = RowExporter.ToSql(rows, new SqlExportOptions { TableName = "People" });

            Assert.True(result.IsSuccess);
            Execute(connection, result.Value!);

            using SqliteCommand count = connection.CreateCommand();
            count.CommandText = "SELECT COUNT(*) FROM \"People\";";
            Assert.Equal(1L, (long)count.ExecuteScalar()!);
        }

        [Fact]
        public void Sql_HostileTableName_StaysOneIdentifier()
        {
            const string evil = "Pe\"ople\"; DROP TABLE \"People\"; --";
            using SqliteConnection connection = OpenDatabase();

            Execute(connection, "CREATE TABLE \"Pe\"\"ople\"\"; DROP TABLE \"\"People\"\"; --\" (\"Id\" INTEGER, \"Name\" TEXT);");

            var result = RowExporter.ToSql(new[] { new Row { Id = 1, Name = "a" } }, new SqlExportOptions { TableName = evil });

            Assert.True(result.IsSuccess);
            Execute(connection, result.Value!);

            using SqliteCommand count = connection.CreateCommand();
            count.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name = 'People';";
            Assert.Equal(1L, (long)count.ExecuteScalar()!);
        }

        [Fact]
        public void Sql_SqlServerDialect_EscapesQuotesAndUsesUnicodePrefix()
        {
            var result = RowExporter.ToSql(
                new[] { new Row { Id = 1, Name = "O'Brien'; DROP TABLE x; --" } },
                new SqlExportOptions { Dialect = SqlDialect.SqlServer, TableName = "People" });

            Assert.True(result.IsSuccess);
            Assert.Contains("N'O''Brien''; DROP TABLE x; --'", result.Value);
        }

        [Fact]
        public void Sql_MySqlDialect_EscapesBackslashBeforeQuote()
        {
            var result = RowExporter.ToSql(
                new[] { new Row { Id = 1, Name = "\\' OR 1=1 --" } },
                new SqlExportOptions { Dialect = SqlDialect.MySql, TableName = "People" });

            Assert.True(result.IsSuccess);
            Assert.Contains("'\\\\'' OR 1=1 --'", result.Value);
        }

        [Theory]
        [InlineData(SqlDialect.Standard, "\"a\"\"b\"")]
        [InlineData(SqlDialect.SqlServer, "[a]]b]")]
        [InlineData(SqlDialect.MySql, "`a``b`")]
        public void Sql_IdentifierQuoting_DoublesTheClosingCharacter(SqlDialect dialect, string expected)
        {
            string quoted = ValueFormatter.QuoteIdentifier(dialect == SqlDialect.Standard ? "a\"b" : dialect == SqlDialect.SqlServer ? "a]b" : "a`b", dialect);

            Assert.Equal(expected, quoted);
        }

        [Fact]
        public void Json_HostileText_RoundTrips()
        {
            var rows = Payloads.Select((text, index) => new Row { Id = index, Name = text }).ToList();

            var result = RowExporter.ToJson(rows);

            Assert.True(result.IsSuccess);

            using JsonDocument document = JsonDocument.Parse(result.Value!);
            var names = document.RootElement.EnumerateArray()
                .Select(element => element.EnumerateObject().First(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)).Value.GetString())
                .ToList();

            Assert.Equal(rows.Select(r => r.Name), names);
        }

        [Fact]
        public void Csv_HostileText_RoundTrips()
        {
            var random = new Random(7);
            var rows = Payloads.Select((text, index) => new Row { Id = index, Name = text })
                .Concat(Enumerable.Range(100, 200).Select(i => new Row { Id = i, Name = RandomText(random) }))
                .ToList();

            var result = RowExporter.ToCsv(rows);

            Assert.True(result.IsSuccess);

            var parsed = ParseCsv(result.Value!);

            Assert.Equal(rows.Count + 1, parsed.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                Assert.Equal(2, parsed[i + 1].Count);
                Assert.Equal(rows[i].Name, parsed[i + 1][1]);
            }
        }

        [Theory]
        [InlineData("=1+1", "'=1+1")]
        [InlineData("+SUM(A1)", "'+SUM(A1)")]
        [InlineData("-2+3", "'-2+3")]
        [InlineData("@cmd", "'@cmd")]
        [InlineData("\tTabbed", "'\tTabbed")]
        [InlineData("Ali", "Ali")]
        [InlineData("a=b", "a=b")]
        [InlineData("", "")]
        public void Csv_NeutralizeFormulas_PrefixesOnlyDangerousText(string input, string expected)
        {
            var result = RowExporter.ToCsv(new[] { new Row { Id = 1, Name = input } }, new CsvExportOptions { NeutralizeFormulas = true });

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ParseCsv(result.Value!)[1][1]);
        }

        [Fact]
        public void Csv_NeutralizeFormulas_KeepsNegativeNumbers()
        {
            var result = RowExporter.ToCsv(new[] { new Row { Id = -5, Name = "x" } }, new CsvExportOptions { NeutralizeFormulas = true });

            Assert.Equal("-5", ParseCsv(result.Value!)[1][0]);
        }

        [Fact]
        public void Csv_Default_LeavesTextUntouched()
        {
            var result = RowExporter.ToCsv(new[] { new Row { Id = 1, Name = "=1+1" } });

            Assert.Equal("=1+1", ParseCsv(result.Value!)[1][1]);
        }

        // A minimal RFC 4180 reader, so the test checks the file a spreadsheet would actually read.
        private static List<List<string>> ParseCsv(string text)
        {
            var records = new List<List<string>>();
            var record = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (quoted)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                    else if (c == '"') quoted = false;
                    else cell.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { record.Add(cell.ToString()); cell.Clear(); }
                else if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    record.Add(cell.ToString()); cell.Clear();
                    records.Add(record); record = new List<string>();
                    i++;
                }
                else cell.Append(c);
            }

            if (cell.Length > 0 || record.Count > 0)
            {
                record.Add(cell.ToString());
                records.Add(record);
            }

            return records;
        }
    }
}
