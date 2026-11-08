namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Stable error codes reported in <see cref="GenerationResult{T}.Errors"/>.
    /// </summary>
    public static class GenerationErrorCodes
    {
        /// <summary>
        /// The definition has configuration errors. One <see cref="GenerationError"/> is reported
        /// per configuration error and its message starts with the original configuration code.
        /// </summary>
        public const string InvalidDefinition = "Faker.InvalidDefinition";

        /// <summary>The factory returned null instead of an instance.</summary>
        public const string FactoryReturnedNull = "Faker.FactoryReturnedNull";

        /// <summary>The requested count is negative.</summary>
        public const string InvalidCount = "Faker.InvalidCount";

        /// <summary>The definition has no item indices left to hand out.</summary>
        public const string ItemIndexExhausted = "Faker.ItemIndexExhausted";

        /// <summary>A dataset table's key is null.</summary>
        public const string NullKey = "Faker.NullKey";

        /// <summary>Two rows of a dataset table have the same key.</summary>
        public const string DuplicateKey = "Faker.DuplicateKey";

        /// <summary>A table has rows but the table it references has none.</summary>
        public const string EmptyParentTable = "Faker.EmptyParentTable";

        /// <summary>There are more rows than distinct combinations a unique group allows.</summary>
        public const string UniqueConstraintUnsatisfiable = "Faker.UniqueConstraintUnsatisfiable";

        /// <summary>The rows passed to an exporter are null.</summary>
        public const string NullRows = "Faker.NullRows";

        /// <summary>One of the rows passed to an exporter is null.</summary>
        public const string NullRow = "Faker.NullRow";

        /// <summary>The row type has no public read/write scalar property to export.</summary>
        public const string NoExportableColumns = "Faker.NoExportableColumns";

        /// <summary>The export options are invalid.</summary>
        public const string InvalidExportOptions = "Faker.InvalidExportOptions";

        /// <summary>Two tables of a dataset would be exported under the same name.</summary>
        public const string DuplicateTableName = "Faker.DuplicateTableName";
    }
}
