using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Describes how to create and fill fake instances of <typeparamref name="T"/>.
    /// Create one with <see cref="Faker.Define{T}()"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configuration errors never throw. They are collected in <see cref="ConfigurationErrors"/>,
    /// and an invalid rule is ignored so the fluent chain keeps working.
    /// </para>
    /// <para>
    /// Rules run in the order they were first registered. Registering a rule for a property that
    /// already has one replaces the earlier rule (the last rule wins) and keeps its position.
    /// </para>
    /// <para>
    /// Exceptions thrown by user supplied factories and rules are not caught by the library.
    /// </para>
    /// <para>
    /// Configuration methods are thread-safe. Call <see cref="Freeze"/> once configuration is
    /// complete; later <c>RuleFor</c> calls are ignored and reported as errors. The first
    /// successful call to <see cref="Generate()"/>, <see cref="Generate(int)"/> or
    /// <see cref="GenerateLazy"/> freezes the definition automatically.
    /// </para>
    /// <para>
    /// Every generated object gets the next item index of this definition (0, 1, 2, ...), and the
    /// index alone decides its random values. The same <see cref="Faker"/> seed, definition order,
    /// rules and call order therefore always produce the same objects. Concurrent calls are safe
    /// and never share an index, but which thread receives which index follows scheduling.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of object this definition produces.</typeparam>
    public sealed class FakerDefinition<T> where T : class
    {
        private static readonly UInt128 MaxIndexExclusive = (UInt128)ulong.MaxValue + 1UL;

        private readonly object _syncRoot = new();

        private readonly Faker _faker;

        private readonly Func<T>? _factory;

        private readonly List<PropertyRule> _rules = [];

        private readonly Dictionary<string, int> _ruleIndexes = new(StringComparer.Ordinal);

        private readonly List<ConfigurationError> _errors = [];

        private readonly object _indexLock = new();

        private PropertyRule[]? _frozenRules;

        // UInt128 so that "all 2^64 indices used" is representable without overflow tricks.
        private UInt128 _nextIndex;

        internal FakerDefinition(
            Faker faker,
            string scopeKey,
            Func<T>? factory,
            IEnumerable<ConfigurationError> initialErrors)
        {
            _faker = faker;
            ScopeKey = scopeKey;
            _factory = factory;
            _errors.AddRange(initialErrors);
        }

        /// <summary>
        /// Gets the configuration errors recorded so far. The returned list is a snapshot.
        /// </summary>
        public IReadOnlyList<ConfigurationError> ConfigurationErrors
        {
            get
            {
                lock (_syncRoot)
                {
                    return _errors.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether no configuration error has been recorded.
        /// </summary>
        public bool IsValid
        {
            get
            {
                lock (_syncRoot)
                {
                    return _errors.Count == 0;
                }
            }
        }

        /// <summary>
        /// Gets the number of registered rules (one per property).
        /// </summary>
        public int RuleCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _rules.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the definition is frozen and no longer accepts rules.
        /// </summary>
        public bool IsFrozen => Volatile.Read(ref _frozenRules) is not null;

        internal string ScopeKey { get; }

        /// <summary>
        /// Registers how to produce the value of one property.
        /// </summary>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="propertySelector">
        /// A direct property selector such as <c>x =&gt; x.Name</c>. The property must be a public,
        /// writable instance property (not init-only, nested, static, indexer or field).
        /// </param>
        /// <param name="valueFactory">
        /// Produces the value. It is not invoked when the rule is registered, only when an
        /// object is generated.
        /// </param>
        /// <returns>This definition, to allow fluent chaining.</returns>
        public FakerDefinition<T> RuleFor<TProperty>(
            Expression<Func<T, TProperty>> propertySelector,
            Func<FakerContext, TProperty> valueFactory)
        {
            var errors = new List<ConfigurationError>();

            if (valueFactory is null)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.NullValueFactory,
                    "The value factory cannot be null."));
            }

            PropertyInfo? property = null;

            if (!PropertySelectorAnalyzer.TryAnalyze(propertySelector, out property, out ConfigurationError? selectorError))
            {
                errors.Add(selectorError);
            }

            if (errors.Count > 0 || property is null)
            {
                AddErrors(errors);

                return this;
            }

            Action<T, TProperty> setter = CompileSetter<TProperty>(property);

            Action<T, FakerContext> apply = (instance, context) => setter(instance, valueFactory!(context));

            var rule = new PropertyRule(property.Name, apply);

            lock (_syncRoot)
            {
                if (_frozenRules is not null)
                {
                    _errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.DefinitionFrozen,
                        $"The rule for '{property.Name}' was ignored because the definition is frozen."));

                    return this;
                }

                if (_ruleIndexes.TryGetValue(property.Name, out int existingIndex))
                {
                    // Last rule wins; keep the original position.
                    _rules[existingIndex] = rule;
                }
                else
                {
                    _ruleIndexes[property.Name] = _rules.Count;
                    _rules.Add(rule);
                }
            }

            return this;
        }

        /// <summary>
        /// Freezes the definition so it can be used safely from several threads.
        /// Calling it more than once has no further effect.
        /// </summary>
        /// <returns>This definition.</returns>
        public FakerDefinition<T> Freeze()
        {
            lock (_syncRoot)
            {
                _frozenRules ??= _rules.ToArray();
            }

            return this;
        }

        /// <summary>
        /// Generates one object.
        /// </summary>
        /// <remarks>
        /// On failure no object is returned, so a half-filled instance never leaks out.
        /// An item index is consumed even if the factory then returns null.
        /// </remarks>
        /// <returns>A successful result with the object, or a failed result with errors.</returns>
        public GenerationResult<T> Generate()
        {
            if (!TryPrepare(out GenerationError[] errors))
                return GenerationResult<T>.Failure(errors);

            if (!TryReserveIndices(1, out ulong index))
                return GenerationResult<T>.Failure(ExhaustedError());

            return GenerateAt(index);
        }

        /// <summary>
        /// Generates <paramref name="count"/> objects, all or nothing.
        /// </summary>
        /// <remarks>
        /// A negative count or an invalid definition fails before any item index is used.
        /// If one item fails, the whole call fails and returns no objects; the indices it
        /// reserved stay used. A count of zero succeeds with an empty list.
        /// </remarks>
        /// <param name="count">How many objects to generate.</param>
        /// <returns>A successful result with a read-only list, or a failed result with errors.</returns>
        public GenerationResult<IReadOnlyList<T>> Generate(int count)
        {
            if (count < 0)
                return GenerationResult<IReadOnlyList<T>>.Failure(InvalidCountError(count));

            if (!TryPrepare(out GenerationError[] errors))
                return GenerationResult<IReadOnlyList<T>>.Failure(errors);

            if (count == 0)
                return GenerationResult<IReadOnlyList<T>>.Success(Array.Empty<T>());

            if (!TryReserveIndices((ulong)count, out ulong firstIndex))
                return GenerationResult<IReadOnlyList<T>>.Failure(ExhaustedError());

            var items = new T[count];

            for (int i = 0; i < count; i++)
            {
                GenerationResult<T> item = GenerateAt(firstIndex + (ulong)i);

                if (!item.IsSuccess)
                    return GenerationResult<IReadOnlyList<T>>.Failure(item.Errors);

                items[i] = item.Value;
            }

            return GenerationResult<IReadOnlyList<T>>.Success(items);
        }

        /// <summary>
        /// Generates <paramref name="count"/> objects one at a time while the sequence is enumerated.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Nothing is generated until enumeration starts. Each element is an independent result, so a
        /// failed item does not stop the sequence. Enumerating the sequence again continues with fresh
        /// item indices and therefore yields different objects.
        /// </para>
        /// <para>
        /// A negative count, an invalid definition or exhausted indices yield a single failed
        /// result and end the sequence.
        /// </para>
        /// </remarks>
        /// <param name="count">How many objects to generate.</param>
        /// <returns>A lazy sequence of results.</returns>
        public IEnumerable<GenerationResult<T>> GenerateLazy(int count)
        {
            if (count < 0)
                return [GenerationResult<T>.Failure(InvalidCountError(count))];

            return GenerateLazyCore(count);
        }

        internal bool TryCreateInstance([NotNullWhen(true)] out T? instance)
        {
            instance = null;

            if (_factory is null)
                return false;

            instance = _factory();

            return instance is not null;
        }

        // Lets tests reach the end of the index space without generating 2^64 objects.
        internal void SetNextIndexForTesting(UInt128 nextIndex)
        {
            lock (_indexLock)
            {
                _nextIndex = nextIndex;
            }
        }

        internal FakerContext CreateContext(ulong itemIndex)
        {
            return _faker.CreatePublicContext(ScopeKey, itemIndex);
        }

        internal void ApplyRules(T instance, FakerContext context)
        {
            foreach (PropertyRule rule in GetRules())
            {
                rule.Apply(instance, context);
            }
        }

        private IEnumerable<GenerationResult<T>> GenerateLazyCore(int count)
        {
            if (!TryPrepare(out GenerationError[] errors))
            {
                yield return GenerationResult<T>.Failure(errors);

                yield break;
            }

            for (int i = 0; i < count; i++)
            {
                if (!TryReserveIndices(1, out ulong index))
                {
                    yield return GenerationResult<T>.Failure(ExhaustedError());

                    yield break;
                }

                yield return GenerateAt(index);
            }
        }

        private GenerationResult<T> GenerateAt(ulong itemIndex)
        {
            if (!TryCreateInstance(out T? instance))
            {
                return GenerationResult<T>.Failure(new GenerationError(
                    GenerationErrorCodes.FactoryReturnedNull,
                    $"The factory for '{typeof(T).Name}' returned null."));
            }

            ApplyRules(instance, CreateContext(itemIndex));

            return GenerationResult<T>.Success(instance);
        }

        private bool TryPrepare(out GenerationError[] errors)
        {
            ConfigurationError[] configurationErrors;

            lock (_syncRoot)
            {
                configurationErrors = _errors.ToArray();
            }

            if (configurationErrors.Length > 0)
            {
                errors = configurationErrors
                    .Select(error => new GenerationError(
                        GenerationErrorCodes.InvalidDefinition,
                        $"{error.Code}: {error.Message}"))
                    .ToArray();

                return false;
            }

            Freeze();

            errors = [];

            return true;
        }

        private bool TryReserveIndices(ulong count, out ulong firstIndex)
        {
            lock (_indexLock)
            {
                UInt128 end = _nextIndex + count;

                if (end > MaxIndexExclusive)
                {
                    firstIndex = 0;

                    return false;
                }

                firstIndex = (ulong)_nextIndex;
                _nextIndex = end;

                return true;
            }
        }

        private static GenerationError ExhaustedError()
        {
            return new GenerationError(
                GenerationErrorCodes.ItemIndexExhausted,
                "This definition has no item indices left.");
        }

        private static GenerationError InvalidCountError(int count)
        {
            return new GenerationError(
                GenerationErrorCodes.InvalidCount,
                $"The count must be zero or greater, but was {count}.");
        }

        private PropertyRule[] GetRules()
        {
            PropertyRule[]? frozen = Volatile.Read(ref _frozenRules);

            if (frozen is not null)
                return frozen;

            lock (_syncRoot)
            {
                return _frozenRules ?? _rules.ToArray();
            }
        }

        private void AddErrors(List<ConfigurationError> errors)
        {
            lock (_syncRoot)
            {
                _errors.AddRange(errors);
            }
        }

        private static Action<T, TProperty> CompileSetter<TProperty>(PropertyInfo property)
        {
            ParameterExpression instance = Expression.Parameter(typeof(T), "instance");

            ParameterExpression value = Expression.Parameter(typeof(TProperty), "value");

            BinaryExpression assignment = Expression.Assign(Expression.Property(instance, property), value);

            return Expression.Lambda<Action<T, TProperty>>(assignment, instance, value).Compile();
        }

        private sealed class PropertyRule
        {
            internal PropertyRule(string propertyName, Action<T, FakerContext> apply)
            {
                PropertyName = propertyName;
                Apply = apply;
            }

            internal string PropertyName { get; }

            internal Action<T, FakerContext> Apply { get; }
        }
    }
}
