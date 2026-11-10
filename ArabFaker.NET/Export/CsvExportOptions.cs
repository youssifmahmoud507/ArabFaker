namespace ArabFaker.NET.Export
{
    /// <summary>
    /// Settings for CSV export.
    /// </summary>
    public sealed class CsvExportOptions
    {
        /// <summary>
        /// Gets a value indicating whether text cells that start with <c>=</c>, <c>+</c>, <c>-</c>, <c>@</c>, a tab or a
        /// carriage return get a leading apostrophe, so Excel and similar programs show them as text instead of running
        /// them as a formula ("CSV injection"). Only text values are changed; numbers keep their sign. Defaults to false,
        /// because the apostrophe changes the cell content.
        /// </summary>
        public bool NeutralizeFormulas { get; init; }
    }
}
