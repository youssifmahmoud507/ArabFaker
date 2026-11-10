using System.Globalization;
using System.Text.Json;

namespace ArabFaker.NET.Export
{
    /// <summary>
    /// Turns scalar values into JSON, CSV text and SQL literals. Always uses the invariant culture.
    /// </summary>
    internal static class ValueFormatter
    {
        private const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF";

        private const string SqlDateTimeFormat = "yyyy-MM-dd HH:mm:ss.FFFFFFF";

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>The text form used in JSON strings and CSV cells.</summary>
        internal static string ToText(object value)
        {
            return value switch
            {
                string text => text,
                bool flag => flag ? "true" : "false",
                float number => number.ToString("R", Invariant),
                double number => number.ToString("R", Invariant),
                DateTime dateTime => dateTime.ToString(DateTimeFormat, Invariant),
                DateTimeOffset offset => offset.ToString(DateTimeFormat + "zzz", Invariant),
                DateOnly date => date.ToString("yyyy-MM-dd", Invariant),
                TimeOnly time => time.ToString("HH:mm:ss.FFFFFFF", Invariant),
                TimeSpan span => span.ToString("c", Invariant),
                Guid guid => guid.ToString("D"),
                Enum item => item.ToString(),
                IFormattable formattable => formattable.ToString(null, Invariant),
                _ => value.ToString() ?? string.Empty
            };
        }

        internal static void WriteJson(Utf8JsonWriter writer, object? value)
        {
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case bool flag:
                    writer.WriteBooleanValue(flag);
                    break;
                case byte number:
                    writer.WriteNumberValue(number);
                    break;
                case sbyte number:
                    writer.WriteNumberValue(number);
                    break;
                case short number:
                    writer.WriteNumberValue(number);
                    break;
                case ushort number:
                    writer.WriteNumberValue(number);
                    break;
                case int number:
                    writer.WriteNumberValue(number);
                    break;
                case uint number:
                    writer.WriteNumberValue(number);
                    break;
                case long number:
                    writer.WriteNumberValue(number);
                    break;
                case ulong number:
                    writer.WriteNumberValue(number);
                    break;
                case decimal number:
                    writer.WriteNumberValue(number);
                    break;
                case float number:
                    if (float.IsFinite(number))
                        writer.WriteNumberValue(number);
                    else
                        writer.WriteNullValue();
                    break;
                case double number:
                    if (double.IsFinite(number))
                        writer.WriteNumberValue(number);
                    else
                        writer.WriteNullValue();
                    break;
                default:
                    writer.WriteStringValue(ToText(value));
                    break;
            }
        }

        /// <summary>A SQL literal for the value, or NULL.</summary>
        internal static string ToSqlLiteral(object? value, SqlExportOptions options)
        {
            switch (value)
            {
                case null:
                    return "NULL";

                case string text:
                    return QuoteText(text, options.Dialect);

                case bool flag:
                    return options.Dialect == SqlDialect.SqlServer
                        ? (flag ? "1" : "0")
                        : (flag ? "TRUE" : "FALSE");

                case float single when !float.IsFinite(single):
                    return "NULL";

                case double real when !double.IsFinite(real):
                    return "NULL";

                case DateTime dateTime:
                    return "'" + dateTime.ToString(SqlDateTimeFormat, Invariant) + "'";

                case DateTimeOffset offset:
                    return "'" + offset.ToString(SqlDateTimeFormat + "zzz", Invariant) + "'";

                case DateOnly or TimeOnly or TimeSpan or Guid:
                    return "'" + ToText(value) + "'";

                case Enum item:
                    return options.EnumsAsText
                        ? QuoteText(item.ToString(), options.Dialect)
                        : ((IFormattable)Convert.ChangeType(item, Enum.GetUnderlyingType(item.GetType()), Invariant))
                            .ToString(null, Invariant);

                case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                    return ToText(value);

                default:
                    // Unknown types are never written raw: quoting keeps a stray value from becoming SQL.
                    return QuoteText(ToText(value), options.Dialect);
            }
        }

        /// <summary>Prefixes text that a spreadsheet would read as a formula with an apostrophe.</summary>
        internal static string NeutralizeFormula(string text)
        {
            return text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r'
                ? "'" + text
                : text;
        }

        internal static string QuoteIdentifier(string name, SqlDialect dialect)
        {
            return dialect switch
            {
                SqlDialect.SqlServer => "[" + name.Replace("]", "]]", StringComparison.Ordinal) + "]",
                SqlDialect.MySql => "`" + name.Replace("`", "``", StringComparison.Ordinal) + "`",
                _ => "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
            };
        }

        private static string QuoteText(string text, SqlDialect dialect)
        {
            string escaped = text.Replace("'", "''", StringComparison.Ordinal);

            if (dialect == SqlDialect.MySql)
                escaped = escaped.Replace("\\", "\\\\", StringComparison.Ordinal);

            return (dialect == SqlDialect.SqlServer ? "N'" : "'") + escaped + "'";
        }
    }
}
