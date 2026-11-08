namespace ArabFaker.NET.Export
{
    /// <summary>
    /// Settings for JSON export.
    /// </summary>
    public sealed class JsonExportOptions
    {
        /// <summary>
        /// Gets a value indicating whether the output is indented. Defaults to true.
        /// </summary>
        public bool Indented { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether property names start with a lower-case letter
        /// (<c>firstName</c> instead of <c>FirstName</c>). Defaults to false.
        /// </summary>
        public bool CamelCase { get; init; }
    }
}
