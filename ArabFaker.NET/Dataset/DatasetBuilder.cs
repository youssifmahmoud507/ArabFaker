using ArabFaker.NET.Generation;
using ArabFaker.NET.Generation.Randomization;
using System.Linq.Expressions;
using System.Reflection;

namespace ArabFaker.NET.Dataset
{
    /// <summary>
    /// Describes several related tables and generates them together so that every foreign key
    /// points to a row that exists. Create one with <see cref="Faker.CreateDataset"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configuration problems never throw. They are collected in <see cref="ConfigurationErrors"/>
    /// and make <see cref="Build"/> fail without generating anything.
    /// </para>
    /// <para>
    /// A dataset needs no particular table order: rows of every table are generated first, then
    /// the relations are filled in, so tables may even reference each other.
    /// </para>
    /// <para>
    /// Everything is deterministic for the same seed, definitions, rules and dataset order. Each
    /// call to <see cref="Build"/> generates fresh rows (the definitions keep counting), while the
    /// relation choices depend only on the row positions.
    /// </para>
    /// </remarks>
    public sealed class DatasetBuilder
    {
        private const int MaxRandomAttempts = 32;

        private const int AttemptStride = 64;

        private readonly Faker _faker;

        private readonly string _scope;

        private readonly List<ITableRuntime> _tables = [];

        private readonly List<ConfigurationError> _errors = [];

        private readonly HashSet<object> _definitions = new(ReferenceEqualityComparer.Instance);

        internal DatasetBuilder(Faker faker, ulong id)
        {
            _faker = faker;
            _scope = string.Concat("dataset/", id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Gets the configuration errors recorded so far. The returned list is a snapshot.
        /// </summary>
        public IReadOnlyList<ConfigurationError> ConfigurationErrors
        {
            get
            {
                lock (SyncRoot)
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
                lock (SyncRoot)
                {
                    return _errors.Count == 0;
                }
            }
        }

        internal object SyncRoot { get; } = new();

        /// <summary>
        /// Adds a table made of <paramref name="rowCount"/> rows produced by <paramref name="definition"/>.
        /// </summary>
        /// <typeparam name="T">The row type.</typeparam>
        /// <typeparam name="TKey">The key type (for example <see cref="int"/>, <see cref="long"/>, <see cref="Guid"/> or <see cref="string"/>).</typeparam>
        /// <param name="definition">How to create and fill rows. Use <c>AutoFill()</c> to get unique <c>Id</c> values.</param>
        /// <param name="rowCount">How many rows to generate. Zero is allowed.</param>
        /// <param name="keySelector">A direct, writable property that identifies a row, such as <c>x =&gt; x.Id</c>.</param>
        /// <returns>The table handle, used to declare relations and to read the rows back.</returns>
        public DatasetTable<T, TKey> Table<T, TKey>(
            FakerDefinition<T> definition,
            int rowCount,
            Expression<Func<T, TKey>> keySelector)
            where T : class
            where TKey : notnull
        {
            var errors = new List<ConfigurationError>();

            if (definition is null)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.NullDefinition,
                    "The definition cannot be null."));
            }

            if (rowCount < 0)
            {
                errors.Add(new ConfigurationError(
                    ConfigurationErrorCodes.InvalidRowCount,
                    $"The row count must be zero or greater, but was {rowCount}."));
            }

            PropertyInfo? keyProperty = null;

            if (!PropertySelectorAnalyzer.TryAnalyze(keySelector, out keyProperty, out ConfigurationError? keyError))
            {
                errors.Add(keyError);
            }

            lock (SyncRoot)
            {
                if (definition is not null && !_definitions.Add(definition))
                {
                    errors.Add(new ConfigurationError(
                        ConfigurationErrorCodes.DuplicateTable,
                        $"The definition for '{typeof(T).Name}' was already added to this dataset. "
                        + "Create a second definition for a second table."));
                }

                if (errors.Count > 0 || definition is null || keyProperty is null)
                {
                    _errors.AddRange(errors);

                    return new DatasetTable<T, TKey>(this, -1, null, 0, null, null);
                }

                var table = new DatasetTable<T, TKey>(
                    this,
                    _tables.Count,
                    definition,
                    rowCount,
                    keyProperty,
                    keySelector.Compile());

                _tables.Add(table);

                return table;
            }
        }

        /// <summary>
        /// Generates every table and fills the relations.
        /// </summary>
        /// <remarks>
        /// The call is all or nothing: on failure no rows are returned. Validation that does not need
        /// the rows (invalid configuration, empty parent tables, unique groups with too few
        /// combinations) fails before any row is generated.
        /// </remarks>
        /// <returns>A successful result with the dataset, or a failed result with errors.</returns>
        public GenerationResult<GeneratedDataset> Build()
        {
            ITableRuntime[] tables;

            lock (SyncRoot)
            {
                if (_errors.Count > 0)
                {
                    return GenerationResult<GeneratedDataset>.Failure(_errors.Select(error => new GenerationError(
                        GenerationErrorCodes.InvalidDefinition,
                        $"{error.Code}: {error.Message}")));
                }

                tables = _tables.ToArray();
            }

            List<GenerationError> preflight = Preflight(tables);

            if (preflight.Count > 0)
                return GenerationResult<GeneratedDataset>.Failure(preflight);

            foreach (ITableRuntime table in tables)
            {
                if (!table.TryGenerate(out List<GenerationError> errors))
                    return GenerationResult<GeneratedDataset>.Failure(errors);
            }

            var failures = new List<GenerationError>();

            foreach (ITableRuntime table in tables)
            {
                AssignRelations(table, failures);
            }

            if (failures.Count > 0)
                return GenerationResult<GeneratedDataset>.Failure(failures);

            var rows = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

            foreach (ITableRuntime table in tables)
            {
                rows[table] = table.GetRowsAsList();
            }

            return GenerationResult<GeneratedDataset>.Success(new GeneratedDataset(rows));
        }

        internal void AddErrors(IEnumerable<ConfigurationError> errors)
        {
            lock (SyncRoot)
            {
                _errors.AddRange(errors);
            }
        }

        // ---------- validation before generation ----------

        private static List<GenerationError> Preflight(ITableRuntime[] tables)
        {
            var errors = new List<GenerationError>();

            foreach (ITableRuntime table in tables)
            {
                if (table.RowCount == 0)
                    continue;

                foreach (ForeignKeyPlan plan in table.GetForeignKeys())
                {
                    if (!plan.IsSelfReference && plan.Parent.RowCount == 0 && plan.NullChance < 1)
                    {
                        errors.Add(new GenerationError(
                            GenerationErrorCodes.EmptyParentTable,
                            $"'{table.RowTypeName}.{plan.PropertyName}' references '{plan.Parent.RowTypeName}', "
                            + "which has no rows. Give the parent at least one row."));
                    }
                }

                foreach (UniqueGroup group in table.GetUniqueGroups())
                {
                    UInt128 combinations = UInt128.One;

                    foreach (ForeignKeyPlan member in group.Members)
                    {
                        combinations *= (UInt128)member.Parent.RowCount;
                    }

                    if ((UInt128)table.RowCount > combinations)
                    {
                        string names = string.Join(", ", group.Members.Select(member => member.PropertyName));

                        errors.Add(new GenerationError(
                            GenerationErrorCodes.UniqueConstraintUnsatisfiable,
                            $"'{table.RowTypeName}' has {table.RowCount} rows but ({names}) allows only "
                            + $"{combinations} distinct combinations."));
                    }
                }
            }

            return errors;
        }

        // ---------- filling relations ----------

        private void AssignRelations(ITableRuntime table, List<GenerationError> failures)
        {
            IReadOnlyList<ForeignKeyPlan> plans = table.GetForeignKeys();

            IReadOnlyList<UniqueGroup> groups = table.GetUniqueGroups();

            var grouped = new HashSet<ForeignKeyPlan>(groups.SelectMany(group => group.Members));

            foreach (ForeignKeyPlan plan in plans)
            {
                if (grouped.Contains(plan))
                    continue;

                for (int row = 0; row < table.RowCount; row++)
                {
                    plan.Setter(table.GetRowAt(row), PickReference(table, plan, row));
                }
            }

            foreach (UniqueGroup group in groups)
            {
                AssignUniqueGroup(table, group, failures);
            }
        }

        private object? PickReference(ITableRuntime table, ForeignKeyPlan plan, int row)
        {
            if (plan.NullChance > 0 && IsNull(table, plan, row))
                return null;

            if (plan.IsSelfReference)
            {
                if (row < plan.Roots)
                    return null;

                // Only rows before this one, so parents always exist and cycles are impossible.
                return table.GetKeyAt(NextIndex(table, plan, row, 0, row, 0));
            }

            return plan.Parent.GetKeyAt(NextIndex(table, plan, row, 0, plan.Parent.RowCount, 0));
        }

        private bool IsNull(ITableRuntime table, ForeignKeyPlan plan, int row)
        {
            RandomGenerator random = _faker.CreateRandom(StreamKey(table, plan) + "/null", (ulong)row);

            return random.TryNextBoolean(plan.NullChance, out bool isNull) && isNull;
        }

        private void AssignUniqueGroup(ITableRuntime table, UniqueGroup group, List<GenerationError> failures)
        {
            IReadOnlyList<ForeignKeyPlan> members = group.Members;

            int[] counts = members.Select(member => member.Parent.RowCount).ToArray();

            var used = new HashSet<(int, int, int)>();

            int[] chosen = new int[members.Count];

            for (int row = 0; row < table.RowCount; row++)
            {
                bool found = false;

                for (int attempt = 0; attempt < MaxRandomAttempts && !found; attempt++)
                {
                    for (int m = 0; m < members.Count; m++)
                    {
                        chosen[m] = NextIndex(table, members[m], row, 0, counts[m], attempt);
                    }

                    found = used.Add(ToTuple(chosen));
                }

                if (!found && !TryScanForFreeCombination(table, members, counts, row, used, chosen))
                {
                    failures.Add(new GenerationError(
                        GenerationErrorCodes.UniqueConstraintUnsatisfiable,
                        $"Could not find an unused combination for row {row} of '{table.RowTypeName}'."));

                    return;
                }

                object rowObject = table.GetRowAt(row);

                for (int m = 0; m < members.Count; m++)
                {
                    members[m].Setter(rowObject, members[m].Parent.GetKeyAt(chosen[m]));
                }
            }
        }

        // Dense case: walk the combinations from a random start until one is free.
        private bool TryScanForFreeCombination(
            ITableRuntime table,
            IReadOnlyList<ForeignKeyPlan> members,
            int[] counts,
            int row,
            HashSet<(int, int, int)> used,
            int[] chosen)
        {
            UInt128 total = UInt128.One;

            foreach (int count in counts)
            {
                total *= (UInt128)count;
            }

            UInt128 start = UInt128.Zero;

            for (int m = 0; m < members.Count; m++)
            {
                start = (start * (UInt128)counts[m]) + (UInt128)NextIndex(table, members[m], row, 0, counts[m], MaxRandomAttempts);
            }

            for (UInt128 step = UInt128.Zero; step < total; step++)
            {
                UInt128 position = (start + step) % total;

                for (int m = members.Count - 1; m >= 0; m--)
                {
                    chosen[m] = (int)(position % (UInt128)counts[m]);
                    position /= (UInt128)counts[m];
                }

                if (used.Add(ToTuple(chosen)))
                    return true;
            }

            return false;
        }

        private int NextIndex(ITableRuntime table, ForeignKeyPlan plan, int row, int minInclusive, int maxExclusive, int attempt)
        {
            RandomGenerator random = _faker.CreateRandom(
                StreamKey(table, plan),
                ((ulong)row * AttemptStride) + (ulong)attempt);

            return random.TryNextInt(minInclusive, maxExclusive, out int value) ? value : minInclusive;
        }

        private string StreamKey(ITableRuntime table, ForeignKeyPlan plan)
        {
            return string.Concat(_scope, "/t", table.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture), "/", plan.PropertyName);
        }

        private static (int, int, int) ToTuple(int[] values)
        {
            return (values[0], values[1], values.Length > 2 ? values[2] : 0);
        }
    }
}
