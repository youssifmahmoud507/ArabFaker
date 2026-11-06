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

        /// <summary>No more definition scopes can be allocated.</summary>
        public const string DefinitionIdExhausted = "Faker.DefinitionIdExhausted";
    }
}
