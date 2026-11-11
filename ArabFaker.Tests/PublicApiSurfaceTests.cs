using System.Globalization;
using System.Reflection;
using ArabFaker.NET;

namespace ArabFaker.Tests
{
    /// <summary>
    /// Lists every public type and member of ArabFaker.NET and compares the list with <c>Golden/PublicApi.txt</c>.
    /// Any change to the public API (a new method, a renamed type, a changed parameter) makes this test fail until
    /// the file is regenerated and committed, so API changes are always visible and deliberate in a pull request.
    /// </summary>
    public class PublicApiSurfaceTests
    {
        [Fact]
        public void PublicApi_MatchesTheGoldenFile()
        {
            GoldenFile.Verify("PublicApi.txt", Describe(typeof(Faker).Assembly));
        }

        private static string Describe(Assembly assembly)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            var lines = new List<string>();

            foreach (Type type in assembly.GetExportedTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                lines.Add($"type {Name(type)}{Base(type)}");

                var members = new List<string>();

                foreach (ConstructorInfo ctor in type.GetConstructors(flags).Where(c => c.IsPublic || c.IsFamily))
                    members.Add($"  ctor({Parameters(ctor.GetParameters())})");

                foreach (MethodInfo method in type.GetMethods(flags).Where(m => (m.IsPublic || m.IsFamily) && !m.IsSpecialName))
                    members.Add($"  {(method.IsStatic ? "static " : string.Empty)}{Name(method.ReturnType)} {method.Name}{Generic(method)}({Parameters(method.GetParameters())})");

                foreach (PropertyInfo property in type.GetProperties(flags))
                {
                    MethodInfo? getter = property.GetMethod;
                    MethodInfo? setter = property.SetMethod;
                    bool visible = (getter is { IsPublic: true } or { IsFamily: true }) || (setter is { IsPublic: true } or { IsFamily: true });

                    if (!visible)
                        continue;

                    string access = (getter is { IsPublic: true } or { IsFamily: true } ? "get; " : string.Empty)
                                    + (setter is { IsPublic: true } or { IsFamily: true } ? "set; " : string.Empty);

                    members.Add($"  {Name(property.PropertyType)} {property.Name} {{ {access.TrimEnd()} }}");
                }

                foreach (FieldInfo field in type.GetFields(flags).Where(f => f.IsPublic || f.IsFamily))
                    members.Add($"  {(field.IsLiteral ? "const " : field.IsStatic ? "static " : string.Empty)}{Name(field.FieldType)} {field.Name}");

                lines.AddRange(members.OrderBy(m => m, StringComparer.Ordinal));
            }

            return string.Join("\n", lines);
        }

        private static string Base(Type type)
        {
            var parts = new List<string>();

            if (type.BaseType is not null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType) && type.BaseType != typeof(Enum))
                parts.Add(Name(type.BaseType));

            parts.AddRange(type.GetInterfaces().Select(Name).OrderBy(n => n, StringComparer.Ordinal));

            return parts.Count == 0 ? string.Empty : " : " + string.Join(", ", parts);
        }

        private static string Generic(MethodInfo method)
        {
            return method.IsGenericMethod ? "<" + string.Join(", ", method.GetGenericArguments().Select(Name)) + ">" : string.Empty;
        }

        private static string Parameters(ParameterInfo[] parameters)
        {
            return string.Join(", ", parameters.Select(p =>
            {
                string prefix = p.IsOut ? "out " : p.ParameterType.IsByRef ? "ref " : string.Empty;
                string suffix = p.HasDefaultValue ? " = " + Convert.ToString(p.DefaultValue, CultureInfo.InvariantCulture) ?? "null" : string.Empty;

                return $"{prefix}{Name(p.ParameterType)} {p.Name}{suffix}";
            }));
        }

        private static string Name(Type type)
        {
            if (type.IsByRef)
                return Name(type.GetElementType()!);

            if (type.IsArray)
                return Name(type.GetElementType()!) + "[]";

            if (type.IsGenericParameter)
                return type.Name;

            if (type.DeclaringType is not null && !type.IsGenericParameter)
            {
                string outer = Name(type.DeclaringType);
                string inner = type.Name;
                int innerTick = inner.IndexOf('`', StringComparison.Ordinal);

                return outer + "." + (innerTick >= 0 ? inner[..innerTick] : inner);
            }

            string name = type.Namespace is null ? type.Name : $"{type.Namespace}.{type.Name}";
            int tick = name.IndexOf('`', StringComparison.Ordinal);

            if (tick >= 0)
                name = name[..tick];

            if (type.IsGenericType)
                name += "<" + string.Join(", ", type.GetGenericArguments().Select(Name)) + ">";

            return name.Replace("+", ".", StringComparison.Ordinal);
        }
    }
}
