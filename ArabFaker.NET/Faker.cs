using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET
{
    /// <summary>
    /// Main entry point for configuring fake data generation.
    /// </summary>
    /// <remarks>
    /// Creates a Faker instance with a specific seed.
    /// </remarks>
    /// <param name="seed">Seed used for reproducible generation.</param>
    public sealed class Faker(long seed)
    {
        private readonly RandomStreamFactory _streamFactory = new(unchecked((ulong)seed));

        /// <summary>
        /// Gets the seed used by this Faker instance.
        /// </summary>
        public long Seed { get; } = seed;

        /// <summary>
        /// Creates a Faker instance with an automatically generated seed.
        /// </summary>
        public Faker() : this(Random.Shared.NextInt64())
        {
        }

        internal GenerationContext CreateContext(string scopeKey, ulong itemIndex)
        {
            return new GenerationContext(_streamFactory, scopeKey, itemIndex);
        }

        internal FakerContext CreatePublicContext(string scopeKey, ulong itemIndex)
        {
            return new FakerContext(
                CreateContext(scopeKey, itemIndex));
        }

    }

}
