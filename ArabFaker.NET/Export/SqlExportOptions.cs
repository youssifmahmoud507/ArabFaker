namespace ArabFaker.NET.Export
{
    /// <summary>
    /// Settings for SQL INSERT export.
    /// </summary>
    public sealed class SqlExportOptions
    {
        /// <summary>
        /// Gets the SQL flavour. Defaults to <see cref="SqlDialect.Standard"/>.
        /// </summary>
        public SqlDialect Dialect { get; init; } = SqlDialect.Standard;

        /// <summary>
        /// Gets the table name for a single sequence of rows. Defaults to the row type name.
        /// Ignored when exporting a whole dataset, where each table uses its row type name.
        /// </summary>
        public string? TableName { get; init; }

        /// <summary>
        /// Gets a value indicating whether enum values are written as their names (default)
        /// or as their underlying numbers.
        /// </summary>
        public bool EnumsAsText { get; init; } = true;

        internal bool TryValidate(out string message)
        {
            if (!Enum.IsDefined(Dialect))
            {
                message = "Dialect is not a defined value.";

                return false;
            }

            if (TableName is not null && string.IsNullOrWhiteSpace(TableName))
            {
                message = "TableName cannot be empty or white space.";

                return false;
            }

            message = string.Empty;

            return true;
        }
    }
}
