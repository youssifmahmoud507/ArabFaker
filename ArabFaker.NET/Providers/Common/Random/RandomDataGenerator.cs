using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Providers.Common.Random
{
    /// <summary>
    /// Provides methods for generating general random data.
    /// </summary>
    public sealed class RandomDataGenerator
    {
        private readonly RandomGenerator _random;

        internal RandomDataGenerator(RandomGenerator random)
        {
            _random = random;
        }

        /// <summary>
        /// Generates a random integer within an inclusive/exclusive range.
        /// </summary>
        public bool TryInt(int minInclusive, int maxExclusive, out int value)
        {
            return _random.TryNextInt(minInclusive, maxExclusive, out value);
        }

        /// <summary>
        /// Generates a random 64-bit integer within an inclusive/exclusive range.
        /// </summary>
        public bool TryInt64(long minInclusive, long maxExclusive, out long value)
        {
            return _random.TryNextInt64(minInclusive, maxExclusive, out value);
        }

        /// <summary>
        /// Generates a random decimal within an inclusive range.
        /// </summary>
        public bool TryDecimal(decimal minInclusive, decimal maxInclusive, int decimalPlaces, out decimal value)
        {
            return _random.TryNextDecimal(minInclusive, maxInclusive, out value, decimalPlaces);
        }

        /// <summary>
        /// Generates a random double between 0.0 inclusive and 1.0 exclusive.
        /// </summary>
        public double NextDouble()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// Generates a random boolean using the specified true probability.
        /// </summary>
        /// <param name="trueProbability">A probability between 0.0 and 1.0.</param>
        /// <param name="value">The generated boolean value.</param>
        public bool TryBoolean(double trueProbability, out bool value)
        {
            return _random.TryNextBoolean(trueProbability, out value);
        }

        /// <summary>
        /// Randomly selects one item from a collection.
        /// </summary>
        public bool TryPick<T>(IReadOnlyList<T>? items, out T value)
        {
            return _random.TryPick(items, out value);
        }

        /// <summary>
        /// Randomly selects multiple items from a collection.
        /// </summary>
        /// <param name="items">The source collection.</param>
        /// <param name="count">The number of items to select.</param>
        /// <param name="allowDuplicates">
        /// Whether the same item can be selected more than once.
        /// </param>
        /// <param name="values">The selected items.</param>
        public bool TryPickMany<T>(IReadOnlyList<T>? items, int count, bool allowDuplicates, out T[] values)
        {
            return _random.TryPickMany(items, count, allowDuplicates, out values);
        }
    }
}
