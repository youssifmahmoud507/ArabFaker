namespace ArabFaker.NET.Dataset
{
    /// <summary>
    /// The rows produced by <see cref="DatasetBuilder.Build"/>, one list per table.
    /// </summary>
    public sealed class GeneratedDataset
    {
        private readonly Dictionary<object, object> _rowsByTable;

        internal GeneratedDataset(Dictionary<object, object> rowsByTable)
        {
            _rowsByTable = rowsByTable;
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
    }
}
