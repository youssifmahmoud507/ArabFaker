using ArabFaker.NET.Dataset;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using System.Globalization;

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

        private readonly object _definitionLock = new();

        private ulong _nextDefinitionId;

        private ulong _nextDatasetId;

        private bool _definitionIdsExhausted;

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

        /// <summary>
        /// Starts a definition for <typeparamref name="T"/> using its public parameterless constructor.
        /// </summary>
        /// <remarks>
        /// Every definition gets its own random scope, allocated in call order. Reproducibility
        /// therefore depends on defining types in the same order for the same seed.
        /// </remarks>
        /// <typeparam name="T">The type to generate.</typeparam>
        /// <returns>A definition ready to receive rules.</returns>
        public FakerDefinition<T> Define<T>() where T : class, new()
        {
            return CreateDefinition<T>(static () => new T(), []);
        }

        /// <summary>
        /// Starts a definition for <typeparamref name="T"/> using a custom factory,
        /// which allows types without a parameterless constructor.
        /// </summary>
        /// <remarks>
        /// The factory is not invoked here. A null factory is reported in
        /// <see cref="FakerDefinition{T}.ConfigurationErrors"/>.
        /// </remarks>
        /// <typeparam name="T">The type to generate.</typeparam>
        /// <param name="factory">Creates a new instance each time one is needed.</param>
        /// <returns>A definition ready to receive rules.</returns>
        public FakerDefinition<T> Define<T>(Func<T> factory) where T : class
        {
            if (factory is null)
            {
                return CreateDefinition<T>(null,[new ConfigurationError(ConfigurationErrorCodes.NullFactory, "The factory cannot be null.")]);
            }

            return CreateDefinition(factory, []);
        }

        /// <summary>
        /// Starts a dataset: several related tables generated together so every foreign key
        /// points to a row that exists.
        /// </summary>
        /// <remarks>
        /// Like definitions, datasets get a random scope in call order, so create them in the
        /// same order for the same seed.
        /// </remarks>
        /// <returns>An empty dataset builder.</returns>
        public DatasetBuilder CreateDataset()
        {
            ulong id = Interlocked.Increment(ref _nextDatasetId) - 1;

            return new DatasetBuilder(this, id);
        }

        internal RandomGenerator CreateRandom(string streamKey, ulong itemIndex)
        {
            return new RandomGenerator(_streamFactory.Create(streamKey, itemIndex));
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

        private FakerDefinition<T> CreateDefinition<T>(Func<T>? factory, ConfigurationError[] initialErrors) where T : class
        {
            var errors = new List<ConfigurationError>(initialErrors);

            string scopeKey = string.Empty;

            if (TryAllocateDefinitionId(out ulong definitionId))
            {
                scopeKey = string.Concat("definition/", definitionId.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.DefinitionIdExhausted,
                    "No more definition scopes are available for this Faker."));
            }

            return new FakerDefinition<T>(this, scopeKey, factory, errors);
        }

        private bool TryAllocateDefinitionId(out ulong definitionId)
        {
            lock (_definitionLock)
            {
                if (_definitionIdsExhausted)
                {
                    definitionId = 0;
                    return false;
                }

                definitionId = _nextDefinitionId;

                if (definitionId == ulong.MaxValue)
                {
                    _definitionIdsExhausted = true;
                }
                else
                {
                    _nextDefinitionId++;
                }

                return true;
            }
        }
    }
}
