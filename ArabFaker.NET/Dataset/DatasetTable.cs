using ArabFaker.NET.Generation;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace ArabFaker.NET.Dataset
{
    /// <summary>
    /// One table of a dataset: a definition, a row count and a key, plus its relations.
    /// Create it with <see cref="DatasetBuilder.Table{T, TKey}"/>.
    /// </summary>
    /// <remarks>
    /// Foreign key properties are assigned by the dataset and replace whatever the definition
    /// produced for them. Every key must be unique within its table.
    /// </remarks>
    /// <typeparam name="T">The row type.</typeparam>
    /// <typeparam name="TKey">The type of the key property.</typeparam>
    public sealed class DatasetTable<T, TKey> : ITableRuntime
        where T : class
        where TKey : notnull
    {
        private readonly DatasetBuilder _builder;

        private readonly FakerDefinition<T>? _definition;

        private readonly PropertyInfo? _keyProperty;

        private readonly Func<T, TKey>? _keyGetter;

        private readonly List<ForeignKeyPlan> _foreignKeys = [];

        private readonly List<UniqueGroup> _uniqueGroups = [];

        private readonly List<NavigationPlan> _navigations = [];

        private readonly List<CollectionPlan> _collections = [];

        private T[] _rows = [];

        private object[] _keys = [];

        private Dictionary<object, int> _rowIndexByKey = new();

        internal DatasetTable(
            DatasetBuilder builder,
            int ordinal,
            FakerDefinition<T>? definition,
            int rowCount,
            PropertyInfo? keyProperty,
            Func<T, TKey>? keyGetter)
        {
            _builder = builder;
            Ordinal = ordinal;
            _definition = definition;
            RowCount = rowCount;
            _keyProperty = keyProperty;
            _keyGetter = keyGetter;
        }

        /// <summary>
        /// Gets the number of rows this table will have.
        /// </summary>
        public int RowCount { get; }

        internal int Ordinal { get; }

        internal bool IsRegistered => Ordinal >= 0;

        internal DatasetBuilder Builder => _builder;

        int ITableRuntime.Ordinal => Ordinal;

        string ITableRuntime.RowTypeName => typeof(T).Name;

        /// <summary>
        /// Makes a property of this table reference a row of another table.
        /// </summary>
        /// <typeparam name="TProperty">The property type: the parent key type, or its nullable form.</typeparam>
        /// <typeparam name="TParent">The parent row type.</typeparam>
        /// <typeparam name="TParentKey">The parent key type.</typeparam>
        /// <param name="propertySelector">A direct, writable property such as <c>x =&gt; x.StudentId</c>.</param>
        /// <param name="parent">The referenced table, from the same dataset.</param>
        /// <param name="nullChance">
        /// The probability (0 to 1) that the value is null instead of a reference. Above 0 the property
        /// must accept null. Defaults to 0: every row references an existing parent.
        /// </param>
        /// <returns>This table.</returns>
        public DatasetTable<T, TKey> ForeignKey<TProperty, TParent, TParentKey>(
            Expression<Func<T, TProperty>> propertySelector,
            DatasetTable<TParent, TParentKey> parent,
            double nullChance = 0)
            where TParent : class
            where TParentKey : notnull
        {
            if (!IsRegistered)
                return this;

            var errors = new List<ConfigurationError>();

            if (parent is null)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.ForeignKeyTargetInvalid,
                    "The referenced table cannot be null."));
            }
            else if (!parent.IsRegistered)
            {
                // The reason was already reported when that table was added.
                return this;
            }
            else if (!ReferenceEquals(parent.Builder, _builder))
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.ForeignKeyTargetInvalid,
                    "The referenced table belongs to a different dataset."));
            }
            else if (ReferenceEquals(parent, this))
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.ForeignKeyTargetInvalid,
                    "A table cannot reference itself with ForeignKey. Use SelfReference."));
            }

            if (!TryAnalyze(propertySelector, out PropertyInfo? property, errors))
            {
                _builder.AddErrors(errors);

                return this;
            }

            ValidateReferenceType(property, typeof(TProperty), typeof(TParentKey), errors);

            if (double.IsNaN(nullChance) || nullChance < 0 || nullChance > 1)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.InvalidNullChance,
                    "The null chance must be a number between 0 and 1."));
            }
            else if (nullChance > 0 && !CanHoldNull(typeof(TProperty)))
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.ForeignKeyNotNullable,
                    $"'{property.Name}' cannot hold null, so a null chance above 0 is not allowed."));
            }

            if (errors.Count > 0 || parent is null)
            {
                _builder.AddErrors(errors);

                return this;
            }

            AddPlan(new ForeignKeyPlan(
                property.Name,
                parent,
                isSelfReference: false,
                roots: 0,
                nullChance,
                typeof(TParent),
                CompileSetter(property),
                CompileGetter(property)));

            return this;
        }

        /// <summary>
        /// Makes a property reference another row of the same table (a hierarchy, such as an
        /// employee's manager).
        /// </summary>
        /// <remarks>
        /// The first <paramref name="roots"/> rows get null; every later row references a row that
        /// comes before it, so the result is always a forest with no cycles and no missing parents.
        /// </remarks>
        /// <typeparam name="TProperty">The property type: the nullable form of the key type, or a reference type.</typeparam>
        /// <param name="propertySelector">A direct, writable, nullable property such as <c>x =&gt; x.ManagerId</c>.</param>
        /// <param name="roots">How many top-level rows have no parent. Must be at least 1.</param>
        /// <returns>This table.</returns>
        public DatasetTable<T, TKey> SelfReference<TProperty>(
            Expression<Func<T, TProperty>> propertySelector,
            int roots = 1)
        {
            if (!IsRegistered)
                return this;

            var errors = new List<ConfigurationError>();

            if (!TryAnalyze(propertySelector, out PropertyInfo? property, errors))
            {
                _builder.AddErrors(errors);

                return this;
            }

            ValidateReferenceType(property, typeof(TProperty), typeof(TKey), errors);

            if (!CanHoldNull(typeof(TProperty)))
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.ForeignKeyNotNullable,
                    $"'{property.Name}' must accept null because top-level rows have no parent."));
            }

            if (roots < 1)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.InvalidRoots,
                    "The number of root rows must be at least 1."));
            }

            if (errors.Count > 0)
            {
                _builder.AddErrors(errors);

                return this;
            }

            AddPlan(new ForeignKeyPlan(
                property.Name,
                this,
                isSelfReference: true,
                roots,
                nullChance: 0,
                typeof(T),
                CompileSetter(property),
                CompileGetter(property)));

            return this;
        }

        /// <summary>
        /// Requires the combination of two foreign keys to be unique across rows, for example
        /// one enrollment per student and course.
        /// </summary>
        /// <remarks>
        /// Both properties must already be registered with <c>ForeignKey</c> and have no null chance.
        /// If there are more rows than distinct combinations, <see cref="DatasetBuilder.Build"/> fails.
        /// </remarks>
        /// <returns>This table.</returns>
        public DatasetTable<T, TKey> UniqueTogether<TFirst, TSecond>(
            Expression<Func<T, TFirst>> first,
            Expression<Func<T, TSecond>> second)
        {
            return AddUniqueGroup(
                TryGetName(first, out string? a) ? a : null,
                TryGetName(second, out string? b) ? b : null);
        }

        /// <summary>
        /// Requires the combination of three foreign keys to be unique across rows.
        /// </summary>
        /// <returns>This table.</returns>
        public DatasetTable<T, TKey> UniqueTogether<TFirst, TSecond, TThird>(
            Expression<Func<T, TFirst>> first,
            Expression<Func<T, TSecond>> second,
            Expression<Func<T, TThird>> third)
        {
            return AddUniqueGroup(
                TryGetName(first, out string? a) ? a : null,
                TryGetName(second, out string? b) ? b : null,
                TryGetName(third, out string? c) ? c : null);
        }

        /// <summary>
        /// Fills a navigation property on this table's rows with the parent row object that its
        /// foreign key points to (for example <c>enrollment.Student</c>).
        /// </summary>
        /// <remarks>
        /// The foreign key must already be registered with <c>ForeignKey</c> or <c>SelfReference</c>.
        /// The property type must accept the parent row type. A null foreign key gives a null navigation.
        /// Navigations never change which parent a row references.
        /// </remarks>
        /// <typeparam name="TForeignKey">The foreign key property type.</typeparam>
        /// <typeparam name="TNavigation">The navigation property type (the parent row type or a base type).</typeparam>
        /// <param name="foreignKey">The foreign key property, such as <c>x =&gt; x.StudentId</c>.</param>
        /// <param name="navigation">The writable navigation property, such as <c>x =&gt; x.Student</c>.</param>
        /// <returns>This table.</returns>
        public DatasetTable<T, TKey> Navigation<TForeignKey, TNavigation>(
            Expression<Func<T, TForeignKey>> foreignKey,
            Expression<Func<T, TNavigation>> navigation)
        {
            if (!IsRegistered)
                return this;

            var errors = new List<ConfigurationError>();

            bool hasForeignKey = TryAnalyze(foreignKey, out PropertyInfo? fkProperty, errors);
            bool hasNavigation = TryAnalyze(navigation, out PropertyInfo? navProperty, errors);

            if (!hasForeignKey || !hasNavigation)
            {
                _builder.AddErrors(errors);

                return this;
            }

            lock (_builder.SyncRoot)
            {
                ForeignKeyPlan? plan = _foreignKeys.FirstOrDefault(candidate => candidate.PropertyName == fkProperty!.Name);

                if (plan is null)
                {
                    errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.NavigationForeignKeyMissing,
                        $"'{fkProperty!.Name}' has no relation yet. Register ForeignKey or SelfReference before Navigation."));
                }
                else if (!typeof(TNavigation).IsAssignableFrom(plan.ParentRowType))
                {
                    errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.NavigationTypeMismatch,
                        $"'{navProperty!.Name}' has type '{typeof(TNavigation)}' but the related rows are '{plan.ParentRowType}'."));
                }
                else if (_navigations.Any(candidate => candidate.ForeignKey == plan))
                {
                    errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.NavigationPropertyConflict,
                        $"'{fkProperty!.Name}' already has a navigation."));
                }

                if (IsPropertyNameTaken(navProperty!.Name))
                {
                    errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.NavigationPropertyConflict,
                        $"'{navProperty!.Name}' is already used as a key, relation, navigation or collection."));
                }

                if (errors.Count == 0 && plan is not null)
                {
                    _navigations.Add(new NavigationPlan(plan, navProperty!.Name, CompileSetter(navProperty)));

                    return this;
                }
            }

            _builder.AddErrors(errors);

            return this;
        }

        /// <summary>
        /// Fills a collection property on this table's rows with the child rows that reference
        /// them (for example <c>student.Enrollments</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The child's foreign key must already be registered and must point to this table. Every row
        /// gets a list, empty when nothing references it. The property type must be assignable from
        /// <see cref="List{T}"/> (for example <c>List&lt;T&gt;</c>, <c>ICollection&lt;T&gt;</c>,
        /// <c>IList&lt;T&gt;</c>, <c>IEnumerable&lt;T&gt;</c>, <c>IReadOnlyList&lt;T&gt;</c>).
        /// </para>
        /// <para>
        /// For a hierarchy use the same table as <paramref name="child"/>, for example
        /// <c>employees.Collection(x =&gt; x.Reports, employees, x =&gt; x.ManagerId)</c>.
        /// </para>
        /// </remarks>
        /// <param name="collectionSelector">The writable collection property, such as <c>x =&gt; x.Enrollments</c>.</param>
        /// <param name="child">The table whose rows go into the collection, from the same dataset.</param>
        /// <param name="childForeignKey">The child's foreign key that points to this table.</param>
        /// <returns>This table.</returns>
        public DatasetTable<T, TKey> Collection<TCollection, TChild, TChildKey, TForeignKey>(
            Expression<Func<T, TCollection>> collectionSelector,
            DatasetTable<TChild, TChildKey> child,
            Expression<Func<TChild, TForeignKey>> childForeignKey)
            where TChild : class
            where TChildKey : notnull
        {
            if (!IsRegistered)
                return this;

            var errors = new List<ConfigurationError>();

            if (child is null)
            {
                _builder.AddErrors([new ConfigurationError(
                    ConfigurationErrorCodes.NavigationTargetInvalid,
                    "The child table cannot be null.")]);

                return this;
            }

            if (!child.IsRegistered)
                return this; // The reason was already reported when that table was added.

            if (!PropertySelectorAnalyzer.TryAnalyze(collectionSelector, out PropertyInfo? collectionProperty, out ConfigurationError? collectionError))
                errors.Add(collectionError);

            if (!PropertySelectorAnalyzer.TryAnalyze(childForeignKey, out PropertyInfo? childFkProperty, out ConfigurationError? fkError))
                errors.Add(fkError);

            if (collectionProperty is null || childFkProperty is null)
            {
                _builder.AddErrors(errors);

                return this;
            }

            if (!ReferenceEquals(child.Builder, _builder))
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.NavigationTargetInvalid,
                    "The child table belongs to a different dataset."));
            }

            if (!typeof(TCollection).IsAssignableFrom(typeof(List<TChild>)))
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.NavigationTypeMismatch,
                    $"'{collectionProperty.Name}' has type '{typeof(TCollection)}', which cannot hold a List<{typeof(TChild).Name}>. "
                    + "Use List<T>, ICollection<T>, IList<T>, IEnumerable<T> or IReadOnlyList<T>."));
            }

            ForeignKeyPlan? childPlan = ((ITableRuntime)child)
                .GetForeignKeys()
                .FirstOrDefault(candidate => candidate.PropertyName == childFkProperty.Name);

            if (childPlan is null)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.NavigationForeignKeyMissing,
                    $"'{childFkProperty.Name}' of '{typeof(TChild).Name}' has no relation yet."));
            }
            else if (!ReferenceEquals(childPlan.Parent, this))
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.NavigationTargetInvalid,
                    $"'{typeof(TChild).Name}.{childFkProperty.Name}' does not reference '{typeof(T).Name}'."));
            }

            lock (_builder.SyncRoot)
            {
                if (IsPropertyNameTaken(collectionProperty.Name))
                {
                    errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.NavigationPropertyConflict,
                        $"'{collectionProperty.Name}' is already used as a key, relation, navigation or collection."));
                }

                if (errors.Count == 0 && childPlan is not null)
                {
                    _collections.Add(new CollectionPlan(
                        child,
                        childPlan,
                        collectionProperty.Name,
                        static () => new List<TChild>(),
                        static (list, row) => ((List<TChild>)list).Add((TChild)row),
                        CompileSetter(collectionProperty)));

                    return this;
                }
            }

            _builder.AddErrors(errors);

            return this;
        }

        // ---------- ITableRuntime ----------

        int ITableRuntime.RowCount => RowCount;

        IReadOnlyList<ForeignKeyPlan> ITableRuntime.GetForeignKeys()
        {
            lock (_builder.SyncRoot)
            {
                return _foreignKeys.ToArray();
            }
        }

        IReadOnlyList<UniqueGroup> ITableRuntime.GetUniqueGroups()
        {
            lock (_builder.SyncRoot)
            {
                return _uniqueGroups.ToArray();
            }
        }

        IReadOnlyList<NavigationPlan> ITableRuntime.GetNavigations()
        {
            lock (_builder.SyncRoot)
            {
                return _navigations.ToArray();
            }
        }

        IReadOnlyList<CollectionPlan> ITableRuntime.GetCollections()
        {
            lock (_builder.SyncRoot)
            {
                return _collections.ToArray();
            }
        }

        bool ITableRuntime.TryGetRowByKey(object key, out object row)
        {
            if (_rowIndexByKey.TryGetValue(key, out int index))
            {
                row = _rows[index];

                return true;
            }

            row = null!;

            return false;
        }

        bool ITableRuntime.TryGenerate(out List<GenerationError> errors)
        {
            errors = [];

            GenerationResult<IReadOnlyList<T>> result = _definition!.Generate(RowCount);

            if (!result.IsSuccess)
            {
                errors.AddRange(result.Errors);

                return false;
            }

            T[] rows = result.Value.ToArray();
            object[] keys = new object[rows.Length];
            var indexByKey = new Dictionary<object, int>();

            for (int i = 0; i < rows.Length; i++)
            {
                object? key = _keyGetter!(rows[i]);

                if (key is null)
                {
                    errors.Add(new GenerationError(
                        GenerationErrorCodes.NullKey,
                        $"Row {i} of '{typeof(T).Name}' has a null key in '{_keyProperty!.Name}'."));

                    return false;
                }

                if (!indexByKey.TryAdd(key, i))
                {
                    errors.Add(new GenerationError(
                        GenerationErrorCodes.DuplicateKey,
                        $"Key '{key}' of '{typeof(T).Name}.{_keyProperty!.Name}' is used by more than one row. "
                        + "Keys must be unique; give the definition a unique key rule or use AutoFill for 'Id'."));

                    return false;
                }

                keys[i] = key;
            }

            _rows = rows;
            _keys = keys;
            _rowIndexByKey = indexByKey;

            return true;
        }

        object ITableRuntime.GetKeyAt(int rowIndex) => _keys[rowIndex];

        object ITableRuntime.GetRowAt(int rowIndex) => _rows[rowIndex];

        object ITableRuntime.GetRowsAsList() => _rows;

        // ---------- helpers ----------

        private bool TryAnalyze<TProperty>(
            Expression<Func<T, TProperty>> selector,
            [NotNullWhen(true)] out PropertyInfo? property,
            List<ConfigurationError> errors)
        {
            if (!PropertySelectorAnalyzer.TryAnalyze(selector, out property, out ConfigurationError? error))
            {
                errors.Add(error);

                return false;
            }

            return true;
        }

        private bool TryGetName<TProperty>(Expression<Func<T, TProperty>> selector, out string? name)
        {
            name = null;

            if (!IsRegistered)
                return false;

            var errors = new List<ConfigurationError>();

            if (!TryAnalyze(selector, out PropertyInfo? property, errors))
            {
                _builder.AddErrors(errors);

                return false;
            }

            name = property.Name;

            return true;
        }

        private void ValidateReferenceType(
            PropertyInfo property,
            Type propertyType,
            Type keyType,
            List<ConfigurationError> errors)
        {
            Type underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (underlying != keyType)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.ForeignKeyTypeMismatch,
                    $"The property '{property.Name}' has type '{propertyType}' but the referenced key has type '{keyType}'."));
            }

            if (_keyProperty is not null && property.Name == _keyProperty.Name)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.ForeignKeyPropertyConflict,
                    $"'{property.Name}' is the key of this table and cannot also be a foreign key."));
            }

            lock (_builder.SyncRoot)
            {
                if (_foreignKeys.Any(plan => plan.PropertyName == property.Name))
                {
                    errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.ForeignKeyPropertyConflict,
                        $"'{property.Name}' already has a relation."));
                }
            }
        }

        private static bool CanHoldNull(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
        }

        private void AddPlan(ForeignKeyPlan plan)
        {
            lock (_builder.SyncRoot)
            {
                _foreignKeys.Add(plan);
            }
        }

        private DatasetTable<T, TKey> AddUniqueGroup(params string?[] names)
        {
            if (!IsRegistered)
                return this;

            if (names.Any(name => name is null))
                return this; // The selector error was already reported.

            lock (_builder.SyncRoot)
            {
                var members = new List<ForeignKeyPlan>();

                foreach (string? name in names)
                {
                    ForeignKeyPlan? plan = _foreignKeys.FirstOrDefault(candidate => candidate.PropertyName == name);

                    string? problem = null;

                    if (plan is null)
                        problem = $"'{name}' has no ForeignKey registered. Register it before UniqueTogether.";
                    else if (plan.IsSelfReference)
                        problem = $"'{name}' is a self reference, which cannot be part of a unique group.";
                    else if (plan.NullChance > 0)
                        problem = $"'{name}' can be null, which cannot be part of a unique group.";
                    else if (members.Contains(plan))
                        problem = $"'{name}' is listed twice.";
                    else if (_uniqueGroups.Any(group => group.Members.Contains(plan)))
                        problem = $"'{name}' already belongs to another unique group.";

                    if (problem is not null)
                    {
                        _builder.AddErrors([new ConfigurationError(ConfigurationErrorCodes.InvalidUniqueGroup, problem)]);

                        return this;
                    }

                    members.Add(plan!);
                }

                _uniqueGroups.Add(new UniqueGroup(members));
            }

            return this;
        }

        private bool IsPropertyNameTaken(string name)
        {
            return (_keyProperty is not null && _keyProperty.Name == name)
                || _foreignKeys.Any(plan => plan.PropertyName == name)
                || _navigations.Any(plan => plan.PropertyName == name)
                || _collections.Any(plan => plan.PropertyName == name);
        }

        private static Func<object, object?> CompileGetter(PropertyInfo property)
        {
            ParameterExpression row = Expression.Parameter(typeof(object), "row");

            UnaryExpression value = Expression.Convert(
                Expression.Property(Expression.Convert(row, typeof(T)), property),
                typeof(object));

            return Expression.Lambda<Func<object, object?>>(value, row).Compile();
        }

        private static Action<object, object?> CompileSetter(PropertyInfo property)
        {
            ParameterExpression row = Expression.Parameter(typeof(object), "row");

            ParameterExpression value = Expression.Parameter(typeof(object), "value");

            BinaryExpression assignment = Expression.Assign(
                Expression.Property(Expression.Convert(row, typeof(T)), property),
                Expression.Convert(value, property.PropertyType));

            return Expression.Lambda<Action<object, object?>>(assignment, row, value).Compile();
        }
    }
}
