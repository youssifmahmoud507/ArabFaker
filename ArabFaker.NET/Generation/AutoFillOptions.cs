using ArabFaker.NET.Providers.Arab.Names;

namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Settings for <see cref="FakerDefinition{T}.AutoFill(AutoFillOptions)"/>.
    /// </summary>
    /// <remarks>
    /// Dates never depend on the real clock, so results stay reproducible. Instead they are
    /// computed relative to <see cref="ReferenceDate"/>.
    /// </remarks>
    public sealed class AutoFillOptions
    {
        /// <summary>
        /// Gets the fixed "today" used for dates of birth and other dates. Defaults to 2026-01-01.
        /// The year must be 200 or later.
        /// </summary>
        public DateOnly ReferenceDate { get; init; } = new DateOnly(2026, 1, 1);

        /// <summary>
        /// Gets the minimum age, in years, used for ages and dates of birth. Defaults to 18.
        /// </summary>
        public int MinAge { get; init; } = 18;

        /// <summary>
        /// Gets the maximum age, in years (at most 150), used for ages and dates of birth. Defaults to 60.
        /// </summary>
        public int MaxAge { get; init; } = 60;

        /// <summary>
        /// Gets the language used for names and locations. Defaults to Arabic.
        /// </summary>
        public NameLanguage Language { get; init; } = NameLanguage.Arabic;

        internal bool TryValidate(out string message)
        {
            if (MinAge < 0)
            {
                message = "MinAge cannot be negative.";

                return false;
            }

            if (MaxAge < MinAge)
            {
                message = "MaxAge cannot be smaller than MinAge.";

                return false;
            }

            if (MaxAge > 150)
            {
                message = "MaxAge cannot be greater than 150.";

                return false;
            }

            if (ReferenceDate.Year < 200)
            {
                message = "ReferenceDate must be in the year 200 or later.";

                return false;
            }

            if (!Enum.IsDefined(Language))
            {
                message = "Language is not a defined value.";

                return false;
            }

            message = string.Empty;

            return true;
        }
    }
}
