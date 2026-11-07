using System.Diagnostics.CodeAnalysis;

namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// The outcome of a generation call: either a value or a list of errors, never both.
    /// </summary>
    /// <remarks>
    /// A failed result never carries a partially filled object.
    /// </remarks>
    /// <typeparam name="T">The type of the generated value.</typeparam>
    public sealed class GenerationResult<T>
    {
        private static readonly IReadOnlyList<GenerationError> NoErrors = Array.Empty<GenerationError>();

        private GenerationResult(T? value, IReadOnlyList<GenerationError> errors)
        {
            Value = value;
            Errors = errors;
        }

        /// <summary>
        /// Gets a value indicating whether generation succeeded. When true, <see cref="Value"/> is not null.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Gets the generated value, or null when generation failed.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets the errors. The list is empty when generation succeeded and never changes afterwards.
        /// </summary>
        public IReadOnlyList<GenerationError> Errors { get; }

        internal static GenerationResult<T> Success(T value)
        {
            return new GenerationResult<T>(value, NoErrors);
        }

        internal static GenerationResult<T> Failure(IEnumerable<GenerationError> errors)
        {
            GenerationError[] snapshot = errors.ToArray();

            if (snapshot.Length == 0)
            {
                snapshot = [new GenerationError(
                    GenerationErrorCodes.InvalidDefinition,
                    "Generation failed without a reported reason.")];
            }

            return new GenerationResult<T>(default, snapshot);
        }

        internal static GenerationResult<T> Failure(GenerationError error)
        {
            return Failure([error]);
        }
    }
}
