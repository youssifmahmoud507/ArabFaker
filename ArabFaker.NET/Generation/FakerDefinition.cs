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
    /// complete; later <c>RuleFor</c> calls are ignored and reported as errors.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of object this definition produces.</typeparam>
    public sealed class FakerDefinition<T> where T : class
    {
        private readonly object _syncRoot = new();

        private readonly Faker _faker;

        private readonly Func<T>? _factory;

        private readonly List<PropertyRule> _rules = [];

        private readonly Dictionary<string, int> _ruleIndexes = new(StringComparer.Ordinal);

        private readonly List<ConfigurationError> _errors = [];

        private PropertyRule[]? _frozenRules;

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

        internal bool TryCreateInstance([NotNullWhen(true)] out T? instance)
        {
            instance = null;

            if (_factory is null)
                return false;

            instance = _factory();

            return instance is not null;
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
