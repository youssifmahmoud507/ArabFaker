namespace ArabFaker.NET.Providers.Common.Dates
{
    /// <summary>
    /// Provides methods for generating fake dates and date of birth values.
    /// </summary>
    public sealed class DatesGenerator
    {
        private readonly DatesProvider _provider;

        internal DatesGenerator(DatesProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Generates a random date within an inclusive date range.
        /// </summary>
        public bool TryDateOnlyBetween(DateOnly startInclusive, DateOnly endInclusive, out DateOnly value)
        {
            return _provider.TryDateOnlyBetween(startInclusive, endInclusive, out value);
        }

        /// <summary>
        /// Generates a random date and time within an inclusive range.
        /// Both DateTime values must have the same Kind.
        /// </summary>
        public bool TryDateTimeBetween(DateTime startInclusive, DateTime endInclusive, out DateTime value)
        {
            return _provider.TryDateTimeBetween(startInclusive, endInclusive, out value);
        }

        /// <summary>
        /// Generates a random birth date within the specified age range.
        /// </summary>
        public bool TryDateOfBirth(DateOnly asOfDate, int minAge, int maxAge, out DateOnly dateOfBirth)
        {
            return _provider.TryDateOfBirth(asOfDate, minAge, maxAge, out dateOfBirth);
        }
    }

}
