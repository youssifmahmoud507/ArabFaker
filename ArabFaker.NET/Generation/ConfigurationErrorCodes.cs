namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Stable error codes reported in <see cref="FakerDefinition{T}.ConfigurationErrors"/>.
    /// </summary>
    public static class ConfigurationErrorCodes
    {
        /// <summary>The factory passed to <c>Define</c> was null.</summary>
        public const string NullFactory = "Faker.NullFactory";

        /// <summary>The property selector passed to <c>RuleFor</c> was null.</summary>
        public const string NullPropertySelector = "Faker.NullPropertySelector";

        /// <summary>The value factory passed to <c>RuleFor</c> was null.</summary>
        public const string NullValueFactory = "Faker.NullValueFactory";

        /// <summary>The selector is not a direct property access (for example a method call or a conversion).</summary>
        public const string UnsupportedSelector = "Faker.UnsupportedSelector";

        /// <summary>The selector points to a nested property such as <c>x.Address.City</c>.</summary>
        public const string NestedPropertyNotSupported = "Faker.NestedPropertyNotSupported";

        /// <summary>The selector points to a static member.</summary>
        public const string StaticMemberNotSupported = "Faker.StaticMemberNotSupported";

        /// <summary>The selector points to a field instead of a property.</summary>
        public const string FieldNotSupported = "Faker.FieldNotSupported";

        /// <summary>The selector points to an indexer.</summary>
        public const string IndexerNotSupported = "Faker.IndexerNotSupported";

        /// <summary>The property has no public setter.</summary>
        public const string PropertyNotWritable = "Faker.PropertyNotWritable";

        /// <summary>The property is init-only, which is not supported.</summary>
        public const string InitOnlyPropertyNotSupported = "Faker.InitOnlyPropertyNotSupported";

        /// <summary>The property type does not match the type produced by the rule.</summary>
        public const string PropertyTypeMismatch = "Faker.PropertyTypeMismatch";

        /// <summary>A rule was added after the definition was frozen.</summary>
        public const string DefinitionFrozen = "Faker.DefinitionFrozen";

        /// <summary>The options passed to <c>AutoFill</c> are null or contain an invalid value.</summary>
        public const string InvalidAutoFillOptions = "Faker.InvalidAutoFillOptions";

        /// <summary>A dataset table was registered with a null definition.</summary>
        public const string NullDefinition = "Faker.NullDefinition";

        /// <summary>The same definition was added to a dataset more than once.</summary>
        public const string DuplicateTable = "Faker.DuplicateTable";

        /// <summary>A dataset table has a negative row count.</summary>
        public const string InvalidRowCount = "Faker.InvalidRowCount";

        /// <summary>The foreign key points to a table that is null, from another dataset, or the table itself.</summary>
        public const string ForeignKeyTargetInvalid = "Faker.ForeignKeyTargetInvalid";

        /// <summary>The foreign key property type does not match the referenced key type.</summary>
        public const string ForeignKeyTypeMismatch = "Faker.ForeignKeyTypeMismatch";

        /// <summary>The property is already a foreign key or is the table's own key.</summary>
        public const string ForeignKeyPropertyConflict = "Faker.ForeignKeyPropertyConflict";

        /// <summary>The property cannot hold null but the relation needs null values.</summary>
        public const string ForeignKeyNotNullable = "Faker.ForeignKeyNotNullable";

        /// <summary>The null chance is not a number between 0 and 1.</summary>
        public const string InvalidNullChance = "Faker.InvalidNullChance";

        /// <summary>The number of root rows of a self reference is less than 1.</summary>
        public const string InvalidRoots = "Faker.InvalidRoots";

        /// <summary>The unique group is not made of distinct, registered, non-null foreign keys.</summary>
        public const string InvalidUniqueGroup = "Faker.InvalidUniqueGroup";

        /// <summary>No more definition scopes can be allocated.</summary>
        public const string DefinitionIdExhausted = "Faker.DefinitionIdExhausted";
    }
}
