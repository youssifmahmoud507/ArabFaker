using ArabFaker.NET.Export;
using ArabFaker.NET.Generation;
using System.Text;
using System.Text.Json;

namespace ArabFaker.NET.Dataset
{
    /// <summary>
    /// The rows produced by <see cref="DatasetBuilder.Build"/>, one list per table.
    /// </summary>
    public sealed class GeneratedDataset
    {
        private readonly Dictionary<object, object> _rowsByTable;

        private readonly ITableRuntime[] _tables;

        internal GeneratedDataset(Dictionary<object, object> rowsByTable, ITableRuntime[] tables)
        {
            _rowsByTable = rowsByTable;
            _tables = tables;
        }

        /// <summary>
        /// Gets the generated rows of a table. Foreign keys in these rows point to rows of the
        /// referenced tables that exist in this same dataset.
        /// </summary>
        /// <typeparam name="T">The row type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="table">The table handle returned by <see cref="DatasetBuilder.Table{T, TKey}"/>.</param>
        /// <returns>The rows, or an empty list if the table does not belong to this dataset.</returns>
        public IReadOnlyList<T> Rows<T, TKey>(DatasetTable<T, TKey> table)
            where T : class
            where TKey : notnull
        {
            if (table is not null && _rowsByTable.TryGetValue(table, out object? rows))
                return (IReadOnlyList<T>)rows;

            return Array.Empty<T>();
        }

        /// <summary>
        /// Writes every table as INSERT statements, referenced tables first, so the script can be
        /// run on an empty database without breaking foreign keys.
        /// </summary>
        /// <remarks>
        /// Each table is named after its row type, so every table needs a different row type. Tables
        /// that reference each other cannot be ordered and are written in registration order; load them
        /// with constraints deferred. Navigation objects and collections are not written.
        /// </remarks>
        /// <param name="options">The SQL settings, or null for the defaults. <c>TableName</c> is ignored here.</param>
        /// <returns>The SQL script, or errors.</returns>
        public GenerationResult<string> ToSql(SqlExportOptions? options = null)
        {
            options ??= new SqlExportOptions();

            if (!options.TryValidate(out string message))
            {
                return GenerationResult<string>.Failure(new GenerationError(
                    GenerationErrorCodes.InvalidExportOptions,
                    message));
            }

            if (!TryGetColumns(out Dictionary<ITableRuntime, ExportColumn[]> columns, out GenerationError? error))
                return GenerationResult<string>.Failure(error);

            var builder = new StringBuilder();

            foreach (ITableRuntime table in TableOrdering.ParentsFirst(_tables))
            {
                RowExporter.AppendInserts(builder, table.RowType.Name, columns[table], ItemsOf(table), options);
            }

            return GenerationResult<string>.Success(builder.ToString());
        }

        /// <summary>
        /// Writes every table as one JSON object: each property is a table (named after its row type)
        /// holding an array of rows. Tables appear in registration order.
        /// </summary>
        /// <param name="options">The JSON settings, or null for the defaults.</param>
        /// <returns>The JSON text, or errors.</returns>
        public GenerationResult<string> ToJson(JsonExportOptions? options = null)
        {
            options ??= new JsonExportOptions();

            if (!TryGetColumns(out Dictionary<ITableRuntime, ExportColumn[]> columns, out GenerationError? error))
                return GenerationResult<string>.Failure(error);

            using var stream = new MemoryStream();

            using (var writer = new Utf8JsonWriter(stream, RowExporter.CreateWriterOptions(options)))
            {
                writer.WriteStartObject();

                foreach (ITableRuntime table in _tables)
                {
                    string name = table.RowType.Name;

                    writer.WritePropertyName(options.CamelCase && name.Length > 0 ? char.ToLowerInvariant(name[0]) + name[1..] : name);

                    RowExporter.WriteRows(writer, columns[table], ItemsOf(table), options);
                }

                writer.WriteEndObject();
            }

            return GenerationResult<string>.Success(Encoding.UTF8.GetString(stream.ToArray()));
        }

        private bool TryGetColumns(
            out Dictionary<ITableRuntime, ExportColumn[]> columns,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out GenerationError? error)
        {
            columns = [];

            var names = new HashSet<string>(StringComparer.Ordinal);

            foreach (ITableRuntime table in _tables)
            {
                if (!names.Add(table.RowType.Name))
                {
                    error = new GenerationError(
                        GenerationErrorCodes.DuplicateTableName,
                        $"More than one table uses the row type '{table.RowType.Name}', so the export names would clash.");

                    return false;
                }

                if (!RowExporter.TryGetColumns(table.RowType, out ExportColumn[] tableColumns, out error))
                    return false;

                columns[table] = tableColumns;
            }

            error = null;

            return true;
        }

        private static List<object> ItemsOf(ITableRuntime table)
        {
            var items = new List<object>(table.RowCount);

            for (int i = 0; i < table.RowCount; i++)
            {
                items.Add(table.GetRowAt(i));
            }

            return items;
        }
    }
}
