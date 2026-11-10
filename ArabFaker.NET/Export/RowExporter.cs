using ArabFaker.NET.Generation;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ArabFaker.NET.Export
{
    /// <summary>
    /// Writes generated rows as JSON, CSV or SQL INSERT statements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only public properties with a getter and a setter (or init) of a scalar type are written (computed read-only properties would break INSERT scripts): text, numbers, booleans, GUIDs, dates and
    /// times, and enums. Navigation objects and collections are left out, so cyclic graphs from
    /// <c>Navigation</c>/<c>Collection</c> export without problems. Columns follow a stable order (base
    /// type first, then declaration order).
    /// </para>
    /// <para>
    /// Output never depends on the machine culture. Problems are returned as errors, never thrown.
    /// The column set comes from the row type argument <c>T</c> of each method, not from the runtime type of each row.
    /// </para>
    /// </remarks>
    public static class RowExporter
    {
        /// <summary>
        /// Writes the rows as a JSON array of objects.
        /// </summary>
        /// <typeparam name="T">The row type.</typeparam>
        /// <param name="rows">The rows to write.</param>
        /// <param name="options">The JSON settings, or null for the defaults.</param>
        /// <returns>The JSON text, or errors.</returns>
        public static GenerationResult<string> ToJson<T>(IEnumerable<T>? rows, JsonExportOptions? options = null)
            where T : class
        {
            if (!TryPrepare(rows, out ExportColumn[] columns, out List<object> items, out GenerationError? error))
                return GenerationResult<string>.Failure(error);

            return GenerationResult<string>.Success(WriteJsonArray(columns, items, options ?? new JsonExportOptions()));
        }

        /// <summary>
        /// Writes the rows as CSV (RFC 4180): a header line, comma separators and CRLF line ends.
        /// </summary>
        /// <typeparam name="T">The row type.</typeparam>
        /// <param name="rows">The rows to write.</param>
        /// <param name="options">The CSV settings, or null for the defaults.</param>
        /// <returns>The CSV text, or errors.</returns>
        public static GenerationResult<string> ToCsv<T>(IEnumerable<T>? rows, CsvExportOptions? options = null)
            where T : class
        {
            if (!TryPrepare(rows, out ExportColumn[] columns, out List<object> items, out GenerationError? error))
                return GenerationResult<string>.Failure(error);

            bool neutralize = options?.NeutralizeFormulas == true;
            var builder = new StringBuilder();

            AppendCsvLine(builder, columns.Select(column => column.Name));

            foreach (object item in items)
            {
                AppendCsvLine(builder, columns.Select(column =>
                {
                    object? value = column.Getter(item);

                    if (value is null)
                        return string.Empty;

                    string text = ValueFormatter.ToText(value);

                    return neutralize && value is string ? ValueFormatter.NeutralizeFormula(text) : text;
                }));
            }

            return GenerationResult<string>.Success(builder.ToString());
        }

        /// <summary>
        /// Writes one INSERT statement per row, separated by new lines.
        /// </summary>
        /// <typeparam name="T">The row type.</typeparam>
        /// <param name="rows">The rows to write.</param>
        /// <param name="options">The SQL settings, or null for the defaults.</param>
        /// <returns>The SQL text (empty when there are no rows), or errors.</returns>
        public static GenerationResult<string> ToSql<T>(IEnumerable<T>? rows, SqlExportOptions? options = null)
            where T : class
        {
            options ??= new SqlExportOptions();

            if (!options.TryValidate(out string message))
            {
                return GenerationResult<string>.Failure(new GenerationError(
                    GenerationErrorCodes.InvalidExportOptions,
                    message));
            }

            if (!TryPrepare(rows, out ExportColumn[] columns, out List<object> items, out GenerationError? error))
                return GenerationResult<string>.Failure(error);

            var builder = new StringBuilder();

            AppendInserts(builder, options.TableName ?? typeof(T).Name, columns, items, options);

            return GenerationResult<string>.Success(builder.ToString());
        }

        // ---------- used by GeneratedDataset ----------

        internal static bool TryGetColumns(Type rowType, out ExportColumn[] columns, [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out GenerationError? error)
        {
            columns = ExportColumn.For(rowType);

            if (columns.Length == 0)
            {
                error = new GenerationError(
                    GenerationErrorCodes.NoExportableColumns,
                    $"'{rowType.Name}' has no public read/write scalar property to export.");

                return false;
            }

            error = null;

            return true;
        }

        internal static string WriteJsonArray(ExportColumn[] columns, IReadOnlyList<object> items, JsonExportOptions options)
        {
            using var stream = new MemoryStream();

            using (var writer = new Utf8JsonWriter(stream, CreateWriterOptions(options)))
            {
                WriteRows(writer, columns, items, options);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        internal static void WriteRows(Utf8JsonWriter writer, ExportColumn[] columns, IReadOnlyList<object> items, JsonExportOptions options)
        {
            string[] names = columns.Select(column => options.CamelCase ? ToCamelCase(column.Name) : column.Name).ToArray();

            writer.WriteStartArray();

            foreach (object item in items)
            {
                writer.WriteStartObject();

                for (int i = 0; i < columns.Length; i++)
                {
                    writer.WritePropertyName(names[i]);
                    ValueFormatter.WriteJson(writer, columns[i].Getter(item));
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        internal static JsonWriterOptions CreateWriterOptions(JsonExportOptions options)
        {
            return new JsonWriterOptions
            {
                Indented = options.Indented,
                NewLine = "\n",

                // Keeps Arabic readable instead of \uXXXX escapes. The output is data, not HTML.
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        internal static void AppendInserts(
            StringBuilder builder,
            string tableName,
            ExportColumn[] columns,
            IReadOnlyList<object> items,
            SqlExportOptions options)
        {
            string table = ValueFormatter.QuoteIdentifier(tableName, options.Dialect);

            string columnList = string.Join(", ", columns.Select(column => ValueFormatter.QuoteIdentifier(column.Name, options.Dialect)));

            foreach (object item in items)
            {
                string values = string.Join(", ", columns.Select(column => ValueFormatter.ToSqlLiteral(column.Getter(item), options)));

                builder.Append("INSERT INTO ").Append(table).Append(" (").Append(columnList).Append(") VALUES (").Append(values).Append(");").Append('\n');
            }
        }

        // ---------- helpers ----------

        private static bool TryPrepare<T>(
            IEnumerable<T>? rows,
            out ExportColumn[] columns,
            out List<object> items,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out GenerationError? error)
            where T : class
        {
            columns = [];
            items = [];

            if (rows is null)
            {
                error = new GenerationError(GenerationErrorCodes.NullRows, "The rows cannot be null.");

                return false;
            }

            if (!TryGetColumns(typeof(T), out columns, out error))
                return false;

            int index = 0;

            foreach (T? row in rows)
            {
                if (row is null)
                {
                    error = new GenerationError(GenerationErrorCodes.NullRow, $"Row {index} is null.");

                    return false;
                }

                items.Add(row);
                index++;
            }

            error = null;

            return true;
        }

        private static void AppendCsvLine(StringBuilder builder, IEnumerable<string> cells)
        {
            bool first = true;

            foreach (string cell in cells)
            {
                if (!first)
                    builder.Append(',');

                first = false;

                if (cell.IndexOfAny([',', '"', '\r', '\n']) >= 0)
                    builder.Append('"').Append(cell.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"');
                else
                    builder.Append(cell);
            }

            builder.Append("\r\n");
        }

        private static string ToCamelCase(string name)
        {
            return name.Length == 0 || char.IsLower(name[0])
                ? name
                : char.ToLowerInvariant(name[0]) + name[1..];
        }
    }
}
