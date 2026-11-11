﻿namespace ArabFaker.NET.Generation.Randomization
{
    /// <summary>
    /// Turns a raw random source into the values the providers need (ranges, picks, probabilities).
    /// Every method reports bad arguments by returning false instead of throwing.
    /// </summary>
    internal sealed class RandomGenerator
    {
        private readonly IRandomSource _randomSource;

        internal RandomGenerator(IRandomSource randomSource)
        {
            _randomSource = randomSource;
        }

        /// <summary>
        /// Picks an integer in [minInclusive, maxExclusive). Returns false for an empty or inverted range.
        /// </summary>
        public bool TryNextInt(int minInclusive, int maxExclusive, out int value)
        {
            return _randomSource.TryNextInt32(minInclusive, maxExclusive, out value);
        }

        /// <summary>
        /// Picks a decimal in [minInclusive, maxInclusive] rounded to the given decimal places (0 to 28). Returns false for an inverted range or bad decimal places.
        /// </summary>
        public bool TryNextDecimal(decimal minInclusive, decimal maxInclusive, out decimal value, int decimalPlaces = 2)
        {
            value = default;

            if (minInclusive > maxInclusive)
                return false;

            if (decimalPlaces is < 0 or > 28)
                return false;

            if (minInclusive == maxInclusive)
            {
                value = minInclusive;
                return true;
            }

            decimal fraction = (decimal)NextDouble();

            // Interpolate without ever producing an intermediate value outside the
            // decimal range (which would raise OverflowException):
            //  - same sign: min + (max - min) * fraction. The span (max - min) is
            //    never larger than the larger magnitude, so it cannot overflow.
            //  - opposite signs: min * (1 - fraction) + max * fraction. Both terms
            //    are bounded by their operand and have opposite signs, so the sum
            //    cannot overflow either.
            bool sameSign = (minInclusive >= 0) == (maxInclusive >= 0);

            decimal generatedValue = sameSign
                ? minInclusive + ((maxInclusive - minInclusive) * fraction)
                : (minInclusive * (1m - fraction)) + (maxInclusive * fraction);

            generatedValue = decimal.Round(generatedValue, decimalPlaces, MidpointRounding.AwayFromZero);

            // Keep the result inside the requested range.
            if (generatedValue < minInclusive)
                generatedValue = minInclusive;

            if (generatedValue > maxInclusive)
                generatedValue = maxInclusive;

            value = generatedValue;

            return true;
        }

        /// <summary>
        /// Returns a double in [0, 1).
        /// </summary>
        public double NextDouble()
        {
            return _randomSource.NextDouble();
        }

        /// <summary>
        /// Returns true with the given probability (0 to 1). Returns false when the probability is outside that range.
        /// </summary>
        public bool TryNextBoolean(double trueProbability, out bool value)
        {
            value = false;

            if (double.IsNaN(trueProbability) || trueProbability < 0 || trueProbability > 1)
            {
                return false;
            }

            value = NextDouble() < trueProbability;

            return true;
        }

        /// <summary>
        /// Picks one item. Returns false when the list is null or empty.
        /// </summary>
        public bool TryPick<T>(IReadOnlyList<T>? items, out T value)
        {
            value = default!;

            if (items is null || items.Count == 0)
                return false;

            if (!_randomSource.TryNextInt32(0, items.Count, out int index))
            {
                return false;
            }

            value = items[index];

            return true;
        }

        /// <summary>
        /// Picks several items, with or without duplicates. Returns false for a null list, a negative count, an empty list with a positive count,
        /// or a count larger than the list when duplicates are not allowed.
        /// </summary>
        public bool TryPickMany<T>(IReadOnlyList<T>? items, int count, bool allowDuplicates, out T[] selected)
        {
            selected = [];

            if (items is null || count < 0)
                return false;

            if (count == 0)
                return true;

            if (items.Count == 0)
                return false;

            if (!allowDuplicates && count > items.Count)
                return false;

            selected = new T[count];

            if (allowDuplicates)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!TryPick(items, out selected[i]))
                    {
                        selected = [];
                        return false;
                    }
                }

                return true;
            }

            // Partial Fisher-Yates shuffle.
            T[] values = new T[items.Count];

            for (int i = 0; i < items.Count; i++)
            {
                values[i] = items[i];
            }

            for (int i = 0; i < count; i++)
            {
                bool success = _randomSource.TryNextInt32(i, values.Length, out int randomIndex);

                if (!success)
                {
                    selected = [];
                    return false;
                }

                (values[i], values[randomIndex]) = (values[randomIndex], values[i]);

                selected[i] = values[i];
            }

            return true;
        }

        internal bool TryNextInt64(long minInclusive, long maxExclusive, out long value)
        {
            value = default;

            if (minInclusive >= maxExclusive)
                return false;

            ulong range = unchecked((ulong)maxExclusive - (ulong)minInclusive);

            ulong threshold = unchecked(0UL - range) % range;

            ulong randomValue;

            do
            {
                randomValue = _randomSource.NextUInt64();
            }
            while (randomValue < threshold);

            ulong offset = randomValue % range;

            ulong result = unchecked((ulong)minInclusive + offset);

            value = unchecked((long)result);

            return true;
        }

    }


}
