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
    }
}
