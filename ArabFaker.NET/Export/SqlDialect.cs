namespace ArabFaker.NET.Export
{
    /// <summary>
    /// The SQL flavour used when exporting INSERT statements.
    /// </summary>
    public enum SqlDialect
    {
        /// <summary>
        /// ANSI style: "identifiers" in double quotes and TRUE/FALSE. Works with PostgreSQL and SQLite.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// SQL Server: [identifiers], N'unicode text' (needed for Arabic) and 1/0 for booleans.
        /// </summary>
        SqlServer = 1,

        /// <summary>
        /// MySQL / MariaDB: `identifiers` and backslash escaping inside text.
        /// </summary>
        MySql = 2
    }
}
