using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Providers.Common.Dates
{
    /// <summary>
    /// Generates dates, date-times and dates of birth inside a range, always from the seeded stream.
    /// </summary>
    internal sealed class DatesProvider
    {
        private readonly RandomGenerator _random;

        internal DatesProvider(RandomGenerator random)
        {
            _random = random;
        }

        internal bool TryDateOnlyBetween(DateOnly startInclusive, DateOnly endInclusive, out DateOnly value)
        {
            value = default;

            if (startInclusive > endInclusive)
                return false;

            int minDay = startInclusive.DayNumber;

            int maxDayExclusive = endInclusive.DayNumber + 1;

            if (!_random.TryNextInt(minDay, maxDayExclusive, out int selectedDay))
            {
                return false;
            }

            value = DateOnly.FromDayNumber(selectedDay);

            return true;
        }

        internal bool TryDateTimeBetween(DateTime startInclusive, DateTime endInclusive, out DateTime value)
        {
            value = default;

            if (startInclusive.Kind != endInclusive.Kind)
                return false;

            if (startInclusive > endInclusive)
                return false;

            long minTicks = startInclusive.Ticks;

            long maxTicksExclusive = endInclusive.Ticks + 1;

            if (!_random.TryNextInt64(minTicks, maxTicksExclusive, out long selectedTicks))
            {
                return false;
            }

            value = new DateTime(selectedTicks, startInclusive.Kind);

            return true;
        }

        internal bool TryDateOfBirth(DateOnly asOfDate, int minAge, int maxAge, out DateOnly dateOfBirth)
        {
            dateOfBirth = default;

            if (minAge < 0 || maxAge < minAge)
                return false;

            // Calculate the earliest possible birth date.
            bool hasEarliestBoundary = TrySubtractYears(asOfDate, (long)maxAge + 1, out DateOnly lowerBoundary);

            DateOnly earliestDate;

            if (!hasEarliestBoundary)
            {
                earliestDate = DateOnly.MinValue;
            }
            else
            {
                earliestDate = lowerBoundary.AddDays(1);
            }

            // Calculate the latest possible birth date.
            if (!TrySubtractYears(asOfDate, minAge, out DateOnly latestDate))
            {
                return false;
            }

            if (earliestDate > latestDate)
                return false;

            return TryDateOnlyBetween(earliestDate, latestDate, out dateOfBirth);
        }

        private static bool TrySubtractYears(DateOnly date, long years, out DateOnly result)
        {
            result = default;

            long targetYear = date.Year - years;

            if (targetYear < 1 || targetYear > 9999)
                return false;

            int year = (int)targetYear;

            int day = Math.Min(date.Day, DateTime.DaysInMonth(year, date.Month));

            result = new DateOnly(year, date.Month, day);

            return true;
        }
    }

}
