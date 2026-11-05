using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.Tests.Providers.Common.Dates
{
    public class DatesProviderTests
    {
        private static GenerationContext CreateContext()
        {
            var factory = new RandomStreamFactory(2026);

            return new GenerationContext(factory, "dates-provider-tests", 0);
        }

        [Fact]
        public void TryDateOnlyBetween_ShouldReturnDateWithinRange()
        {
            var context = CreateContext();

            var start = new DateOnly(2020, 1, 1);
            var end = new DateOnly(2025, 12, 31);

            for (int i = 0; i < 1000; i++)
            {
                bool success = context.Dates.TryDateOnlyBetween(start, end, out var result);

                Assert.True(success);
                Assert.InRange(result, start, end);
            }
        }

        [Fact]
        public void TryDateOnlyBetween_SameDates_ShouldReturnSameDate()
        {
            var context = CreateContext();

            var date = new DateOnly(2026, 11, 2);

            bool success = context.Dates.TryDateOnlyBetween(date, date, out var result);

            Assert.True(success);
            Assert.Equal(date, result);
        }

        [Fact]
        public void TryDateOnlyBetween_InvalidRange_ShouldReturnFalse()
        {
            var context = CreateContext();

            bool success = context.Dates.TryDateOnlyBetween(new DateOnly(2026, 1, 1), new DateOnly(2025, 1, 1), out var result);

            Assert.False(success);
            Assert.Equal(default, result);
        }

        [Fact]
        public void TryDateOnlyBetween_ShouldSupportBoundaries()
        {
            var context = CreateContext();

            bool success = context.Dates.TryDateOnlyBetween(DateOnly.MinValue, DateOnly.MaxValue, out var result);

            Assert.True(success);

            Assert.InRange(result.DayNumber, DateOnly.MinValue.DayNumber, DateOnly.MaxValue.DayNumber);
        }

        [Fact]
        public void TryDateTimeBetween_ShouldReturnDateTimeWithinRange()
        {
            var context = CreateContext();

            var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var end = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            for (int i = 0; i < 1000; i++)
            {
                bool success = context.Dates.TryDateTimeBetween(start, end, out var result);

                Assert.True(success);

                Assert.InRange(result.Ticks, start.Ticks, end.Ticks);

                Assert.Equal(DateTimeKind.Utc, result.Kind);
            }
        }

        [Fact]
        public void TryDateTimeBetween_MismatchedKinds_ShouldReturnFalse()
        {
            var context = CreateContext();

            var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var end = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

            bool success = context.Dates.TryDateTimeBetween(start, end, out var result);

            Assert.False(success);
            Assert.Equal(default, result);
        }

        [Fact]
        public void TryDateTimeBetween_ShouldSupportDateTimeBoundaries()
        {
            var context = CreateContext();

            bool success = context.Dates.TryDateTimeBetween(DateTime.MinValue, DateTime.MaxValue, out var result);

            Assert.True(success);

            Assert.InRange(result.Ticks, DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
        }

        [Fact]
        public void TryDateOfBirth_ShouldGenerateValidAge()
        {
            var context = CreateContext();

            var asOfDate = new DateOnly(2026, 11, 2);

            bool success = context.Dates.TryDateOfBirth(asOfDate, 18, 60, out var birthDate);

            Assert.True(success);

            int age = CalculateAge(birthDate, asOfDate);

            Assert.InRange(age, 18, 60);
            Assert.True(birthDate <= asOfDate);
        }

        [Theory]
        [InlineData(-1, 60)]
        [InlineData(60, 18)]
        public void TryDateOfBirth_InvalidAgeRange_ShouldReturnFalse(int minAge, int maxAge)
        {
            var context = CreateContext();

            bool success = context.Dates.TryDateOfBirth(new DateOnly(2026, 11, 2), minAge, maxAge, out var result);

            Assert.False(success);
            Assert.Equal(default, result);
        }

        [Fact]
        public void TryDateOfBirth_SameSeed_ShouldReturnSameDate()
        {
            var first = CreateContext();
            var second = CreateContext();

            var asOfDate = new DateOnly(2026, 11, 2);

            first.Dates.TryDateOfBirth(asOfDate, 18, 60, out var firstDate);

            second.Dates.TryDateOfBirth(asOfDate, 18, 60, out var secondDate);

            Assert.Equal(firstDate, secondDate);
        }

        [Fact]
        public void TryDateOfBirth_ShouldSupportEarlyCalendarDates()
        {
            var context = CreateContext();

            bool success = context.Dates.TryDateOfBirth(new DateOnly(100, 1, 1), 0, 150, out var result);

            Assert.True(success);
            Assert.True(result <= new DateOnly(100, 1, 1));
        }

        private static int CalculateAge(DateOnly birthDate, DateOnly asOfDate)
        {
            int age = asOfDate.Year - birthDate.Year;

            if (birthDate > asOfDate.AddYears(-age))
                age--;

            return age;
        }
    }
}
