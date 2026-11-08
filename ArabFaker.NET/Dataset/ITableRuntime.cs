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

        Type RowType { get; }

        IReadOnlyList<ForeignKeyPlan> GetForeignKeys();

        IReadOnlyList<UniqueGroup> GetUniqueGroups();

        IReadOnlyList<NavigationPlan> GetNavigations();

        IReadOnlyList<CollectionPlan> GetCollections();

        /// <summary>Finds a generated row by its key. Only valid after <see cref="TryGenerate"/> succeeded.</summary>
        bool TryGetRowByKey(object key, out object row);

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
            Type parentRowType,
            Action<object, object?> setter,
            Func<object, object?> getter)
        {
            PropertyName = propertyName;
            Parent = parent;
            IsSelfReference = isSelfReference;
            Roots = roots;
            NullChance = nullChance;
            ParentRowType = parentRowType;
            Setter = setter;
            Getter = getter;
        }

        internal string PropertyName { get; }

        internal ITableRuntime Parent { get; }

        internal bool IsSelfReference { get; }

        internal int Roots { get; }

        internal double NullChance { get; }

        internal Type ParentRowType { get; }

        internal Action<object, object?> Setter { get; }

        /// <summary>Reads the property of a row as a boxed value, or null.</summary>
        internal Func<object, object?> Getter { get; }
    }

    /// <summary>A reference navigation: sets the parent row object on the child row.</summary>
    internal sealed class NavigationPlan
    {
        internal NavigationPlan(ForeignKeyPlan foreignKey, string propertyName, Action<object, object?> setter)
        {
            ForeignKey = foreignKey;
            PropertyName = propertyName;
            Setter = setter;
        }

        internal ForeignKeyPlan ForeignKey { get; }

        internal string PropertyName { get; }

        internal Action<object, object?> Setter { get; }
    }

    /// <summary>A collection navigation: sets the list of child rows on the parent row.</summary>
    internal sealed class CollectionPlan
    {
        internal CollectionPlan(
            ITableRuntime child,
            ForeignKeyPlan childForeignKey,
            string propertyName,
            Func<object> createList,
            Action<object, object> addToList,
            Action<object, object?> setter)
        {
            Child = child;
            ChildForeignKey = childForeignKey;
            PropertyName = propertyName;
            CreateList = createList;
            AddToList = addToList;
            Setter = setter;
        }

        internal ITableRuntime Child { get; }

        internal ForeignKeyPlan ChildForeignKey { get; }

        internal string PropertyName { get; }

        internal Func<object> CreateList { get; }

        internal Action<object, object> AddToList { get; }

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
