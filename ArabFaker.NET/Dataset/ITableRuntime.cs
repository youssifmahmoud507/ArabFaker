using ArabFaker.NET.Generation;

namespace ArabFaker.NET.Dataset
{
    /// <summary>
    /// Type-erased view of a dataset table, used by the builder.
    /// </summary>
    internal interface ITableRuntime
    {
        int Ordinal { get; }

        int RowCount { get; }

        string RowTypeName { get; }

        IReadOnlyList<ForeignKeyPlan> GetForeignKeys();

        IReadOnlyList<UniqueGroup> GetUniqueGroups();

        /// <summary>Generates the rows and checks the keys. Returns false with errors on failure.</summary>
        bool TryGenerate(out List<GenerationError> errors);

        object GetKeyAt(int rowIndex);

        object GetRowAt(int rowIndex);

        /// <summary>The generated rows as <c>IReadOnlyList&lt;T&gt;</c>.</summary>
        object GetRowsAsList();
    }

    internal sealed class ForeignKeyPlan
    {
        internal ForeignKeyPlan(
            string propertyName,
            ITableRuntime parent,
            bool isSelfReference,
            int roots,
            double nullChance,
            Action<object, object?> setter)
        {
            PropertyName = propertyName;
            Parent = parent;
            IsSelfReference = isSelfReference;
            Roots = roots;
            NullChance = nullChance;
            Setter = setter;
        }

        internal string PropertyName { get; }

        internal ITableRuntime Parent { get; }

        internal bool IsSelfReference { get; }

        internal int Roots { get; }

        internal double NullChance { get; }

        internal Action<object, object?> Setter { get; }
    }

    internal sealed class UniqueGroup
    {
        internal UniqueGroup(IReadOnlyList<ForeignKeyPlan> members)
        {
            Members = members;
        }

        internal IReadOnlyList<ForeignKeyPlan> Members { get; }
    }
}
