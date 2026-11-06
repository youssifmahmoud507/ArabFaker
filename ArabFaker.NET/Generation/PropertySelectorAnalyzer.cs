using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Validates the property selector expressions accepted by <c>RuleFor</c>.
    /// Only direct, public, writable (non init-only) instance properties are supported.
    /// </summary>
    internal static class PropertySelectorAnalyzer
    {
        internal static bool TryAnalyze<T, TProperty>(
            Expression<Func<T, TProperty>>? selector,
            [NotNullWhen(true)] out PropertyInfo? property,
            [NotNullWhen(false)] out ConfigurationError? error)
        {
            property = null;
            error = null;

            if (selector is null)
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.NullPropertySelector,
                    "The property selector cannot be null.");

                return false;
            }

            Expression body = selector.Body;

            if (body is IndexExpression ||
                (body is MethodCallExpression indexerCall &&
                 indexerCall.Method.IsSpecialName &&
                 indexerCall.Arguments.Count > 0))
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.IndexerNotSupported,
                    "Indexers are not supported. Select a direct property of the object.");

                return false;
            }

            if (body is not MemberExpression member)
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.UnsupportedSelector,
                    "The selector must be a direct property access such as x => x.Name. "
                    + "Method calls, conversions and other expressions are not supported.");

                return false;
            }

            if (member.Expression is null)
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.StaticMemberNotSupported,
                    $"The static member '{member.Member.Name}' is not supported. Select an instance property.");

                return false;
            }

            if (!ReferenceEquals(member.Expression, selector.Parameters[0]))
            {
                if (member.Expression is MemberExpression)
                {
                    error = new ConfigurationError(
                        ConfigurationErrorCodes.NestedPropertyNotSupported,
                        $"The nested or computed member '{member.Member.Name}' is not supported. "
                        + "Select a property directly on the object.");
                }
                else
                {
                    error = new ConfigurationError(
                        ConfigurationErrorCodes.UnsupportedSelector,
                        "The selector must access the property directly on the lambda parameter.");
                }

                return false;
            }

            if (member.Member is FieldInfo)
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.FieldNotSupported,
                    $"'{member.Member.Name}' is a field. Only properties are supported.");

                return false;
            }

            if (member.Member is not PropertyInfo candidate)
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.UnsupportedSelector,
                    $"'{member.Member.Name}' is not a property.");

                return false;
            }

            if (candidate.GetIndexParameters().Length > 0)
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.IndexerNotSupported,
                    "Indexers are not supported. Select a direct property of the object.");

                return false;
            }

            MethodInfo? setter = candidate.GetSetMethod(nonPublic: false);

            if (setter is null)
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.PropertyNotWritable,
                    $"The property '{candidate.Name}' has no public setter.");

                return false;
            }

            if (IsInitOnly(setter))
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.InitOnlyPropertyNotSupported,
                    $"The property '{candidate.Name}' is init-only, which is not supported.");

                return false;
            }

            if (candidate.PropertyType != typeof(TProperty))
            {
                error = new ConfigurationError(
                    ConfigurationErrorCodes.PropertyTypeMismatch,
                    $"The property '{candidate.Name}' has type '{candidate.PropertyType}' "
                    + $"but the rule produces '{typeof(TProperty)}'.");

                return false;
            }

            property = candidate;

            return true;
        }

        private static bool IsInitOnly(MethodInfo setter)
        {
            foreach (Type modifier in setter.ReturnParameter.GetRequiredCustomModifiers())
            {
                if (modifier == typeof(IsExternalInit))
                    return true;
            }

            return false;
        }
    }
}
