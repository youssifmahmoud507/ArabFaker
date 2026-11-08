using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ArabFaker.NET.Export
{
    /// <summary>
    /// A public read/write (set or init) scalar property that can be written to a file.
    /// </summary>
    internal sealed class ExportColumn
    {
        private static readonly ConcurrentDictionary<Type, ExportColumn[]> Cache = new();

        private ExportColumn(string name, Func<object, object?> getter)
        {
            Name = name;
            Getter = getter;
        }

        internal string Name { get; }

        internal Func<object, object?> Getter { get; }

        /// <summary>
        /// Returns the exportable columns of a type in a stable order: base type properties first,
        /// then declaration order. Navigations, collections and other non-scalar properties are left out.
        /// </summary>
        internal static ExportColumn[] For(Type rowType)
        {
            return Cache.GetOrAdd(rowType, Build);
        }

        private static ExportColumn[] Build(Type rowType)
        {
            return rowType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetGetMethod(nonPublic: false) is not null
                    && property.GetSetMethod(nonPublic: false) is not null
                    && property.GetIndexParameters().Length == 0
                    && IsScalar(property.PropertyType))
                .OrderBy(property => Depth(property.DeclaringType))
                .ThenBy(property => property.MetadataToken)
                .DistinctBy(property => property.Name)
                .Select(property => new ExportColumn(property.Name, CompileGetter(rowType, property)))
                .ToArray();
        }

        private static bool IsScalar(Type type)
        {
            Type underlying = Nullable.GetUnderlyingType(type) ?? type;

            return underlying == typeof(string)
                || underlying == typeof(bool)
                || underlying == typeof(byte)
                || underlying == typeof(sbyte)
                || underlying == typeof(short)
                || underlying == typeof(ushort)
                || underlying == typeof(int)
                || underlying == typeof(uint)
                || underlying == typeof(long)
                || underlying == typeof(ulong)
                || underlying == typeof(float)
                || underlying == typeof(double)
                || underlying == typeof(decimal)
                || underlying == typeof(Guid)
                || underlying == typeof(DateOnly)
                || underlying == typeof(TimeOnly)
                || underlying == typeof(DateTime)
                || underlying == typeof(DateTimeOffset)
                || underlying == typeof(TimeSpan)
                || underlying.IsEnum;
        }

        private static int Depth(Type? type)
        {
            int depth = 0;

            for (Type? current = type?.BaseType; current is not null; current = current.BaseType)
            {
                depth++;
            }

            return depth;
        }

        private static Func<object, object?> CompileGetter(Type rowType, PropertyInfo property)
        {
            ParameterExpression row = Expression.Parameter(typeof(object), "row");

            UnaryExpression value = Expression.Convert(
                Expression.Property(Expression.Convert(row, rowType), property),
                typeof(object));

            return Expression.Lambda<Func<object, object?>>(value, row).Compile();
        }
    }
}
