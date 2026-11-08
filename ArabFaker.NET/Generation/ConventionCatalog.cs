using ArabFaker.NET.Providers.Arab.Locations;
using ArabFaker.NET.Providers.Arab.Names;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Chooses a value factory for a property from its name and type.
    /// </summary>
    /// <remarks>
    /// Anything ambiguous or relational is deliberately left unmapped rather than filled with
    /// meaningless data: a bare <c>Name</c>, foreign keys such as <c>CustomerId</c> and strings
    /// with no recognised name. The caller reports those so they can get an explicit rule.
    /// A bare <c>Department</c> is filled with an academic department when the class name looks
    /// academic (for example <c>Course</c> or <c>Student</c>), and with a company department otherwise.
    /// </remarks>
    internal static class ConventionCatalog
    {
        internal static bool TryCreateFactory(
            PropertyInfo property,
            AutoFillOptions options,
            [NotNullWhen(true)] out Func<FakerContext, object?>? factory)
        {
            Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            string name = property.Name;
            string key = Normalize(name);
            bool isKeyColumn = IsKeyName(name, key);
            bool isPrimaryKey = key == "id";

            factory = null;

            if (type == typeof(string))
            {
                factory = StringFactory(property, key, isPrimaryKey, isKeyColumn, options);
            }
            else if (type == typeof(Guid))
            {
                if (!isKeyColumn || isPrimaryKey)
                    factory = context => context.Identifiers.GenerateGuid();
            }
            else if (type == typeof(int))
            {
                factory = IntFactory(key, isPrimaryKey, isKeyColumn, options);
            }
            else if (type == typeof(long))
            {
                factory = LongFactory(isPrimaryKey, isKeyColumn);
            }
            else if (type == typeof(decimal))
            {
                factory = DecimalFactory(key);
            }
            else if (type == typeof(double))
            {
                factory = context => Math.Round(context.RandomData.NextDouble() * 1000d, 2);
            }
            else if (type == typeof(float))
            {
                factory = context => (float)Math.Round(context.RandomData.NextDouble() * 1000d, 2);
            }
            else if (type == typeof(bool))
            {
                factory = context => context.RandomData.TryBoolean(0.5, out bool value) && value;
            }
            else if (type == typeof(DateOnly))
            {
                factory = DateOnlyFactory(key, options);
            }
            else if (type == typeof(DateTime))
            {
                factory = DateTimeFactory(options);
            }
            else if (type.IsEnum)
            {
                factory = EnumFactory(type);
            }

            return factory is not null;
        }

        private static Func<FakerContext, object?>? StringFactory(
            PropertyInfo property,
            string key,
            bool isPrimaryKey,
            bool isKeyColumn,
            AutoFillOptions options)
        {
            NameLanguage language = options.Language;

            LocationLanguage locationLanguage = language == NameLanguage.Latin
                ? LocationLanguage.English
                : LocationLanguage.Arabic;

            if (isPrimaryKey)
                return context => context.Identifiers.GenerateGuidString();

            if (isKeyColumn)
                return null;

            switch (key)
            {
                case "firstname":
                case "givenname":
                case "forename":
                    return context => context.Names.TryFirstName(out string value, language) ? value : string.Empty;

                case "middlename":
                    return context => context.Names.TryMiddleName(out string value, language) ? value : string.Empty;

                case "lastname":
                case "surname":
                case "familyname":
                    return context => context.Names.TryLastName(out string value, language) ? value : string.Empty;

                case "fullname":
                case "displayname":
                    return context => context.Names.TryFullName(out string value, language) ? value : string.Empty;

                case "email":
                case "emailaddress":
                    return context => context.Internet.TryEmail(out string value) ? value : string.Empty;

                case "username":
                case "login":
                    return context => context.Internet.TryUsername(out string value) ? value : string.Empty;

                case "governorate":
                case "governoratename":
                    return LocationName(LocationLevel.Governorate, locationLanguage);

                case "city":
                case "district":
                case "districtname":
                    return LocationName(LocationLevel.District, locationLanguage);

                case "shiyakha":
                case "shiyakhaname":
                    return LocationName(LocationLevel.Shiyakha, locationLanguage);

                default:
                    return ReferenceFactory(property, key, language);
            }
        }

        // Words in a class name that make a bare "Department" mean an academic department.
        private static readonly string[] AcademicTypeHints =
        [
            "course", "student", "faculty", "university", "college", "academic",
            "lecturer", "instructor", "professor", "teacher", "enrollment", "school"
        ];

        private static Func<FakerContext, object?>? ReferenceFactory(PropertyInfo property, string key, NameLanguage language)
        {
            switch (key)
            {
                case "course":
                case "coursename":
                case "coursetitle":
                    return context => context.Reference.TryCourse(out string value, language) ? value : string.Empty;

                case "academicdepartment":
                case "academicdepartmentname":
                    return context => context.Reference.TryAcademicDepartment(out string value, language) ? value : string.Empty;

                case "department":
                case "departmentname":
                    if (IsAcademicType(property.DeclaringType))
                        return context => context.Reference.TryAcademicDepartment(out string value, language) ? value : string.Empty;

                    return context => context.Reference.TryDepartment(out string value, language) ? value : string.Empty;

                case "jobtitle":
                case "occupation":
                    return context => context.Reference.TryJobTitle(out string value, language) ? value : string.Empty;

                case "faculty":
                case "facultyname":
                    return context => context.Reference.TryFaculty(out string value, language) ? value : string.Empty;

                case "university":
                case "universityname":
                    return context => context.Reference.TryUniversity(out string value, language) ? value : string.Empty;

                case "educationlevel":
                    return context => context.Reference.TryEducationLevel(out string value, language) ? value : string.Empty;

                case "industry":
                    return context => context.Reference.TryIndustry(out string value, language) ? value : string.Empty;

                case "productcategory":
                    return context => context.Reference.TryProductCategory(out string value, language) ? value : string.Empty;

                case "paymentmethod":
                    return context => context.Reference.TryPaymentMethod(out string value, language) ? value : string.Empty;

                case "bank":
                case "bankname":
                    return context => context.Reference.TryBank(out string value, language) ? value : string.Empty;

                case "specialty":
                case "specialization":
                case "medicalspecialty":
                    return context => context.Reference.TryMedicalSpecialty(out string value, language) ? value : string.Empty;

                case "bloodtype":
                case "bloodgroup":
                    return context => context.Reference.TryBloodType(out string value) ? value : string.Empty;

                case "maritalstatus":
                    return context => context.Reference.TryMaritalStatus(out string value, language) ? value : string.Empty;

                case "country":
                case "countryname":
                    return context => context.Reference.TryCountry(out string value, language) ? value : string.Empty;

                case "language":
                    return context => context.Reference.TryLanguage(out string value, language) ? value : string.Empty;

                case "color":
                case "colour":
                    return context => context.Reference.TryColor(out string value, language) ? value : string.Empty;

                case "currency":
                case "currencycode":
                    return context => context.Reference.TryCurrencyCode(out string value) ? value : string.Empty;

                default:
                    return null;
            }
        }

        private static bool IsAcademicType(Type? type)
        {
            if (type is null)
                return false;

            string typeName = Normalize(type.Name);

            foreach (string hint in AcademicTypeHints)
            {
                if (typeName.Contains(hint, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static Func<FakerContext, object?> LocationName(LocationLevel level, LocationLanguage language)
        {
            return context => context.Locations.TryPickName(level, language, out string value) ? value : string.Empty;
        }

        private static Func<FakerContext, object?>? IntFactory(
            string key,
            bool isPrimaryKey,
            bool isKeyColumn,
            AutoFillOptions options)
        {
            if (isPrimaryKey)
                return context => ToKey(context.ItemIndex, int.MaxValue);

            if (isKeyColumn)
                return null;

            if (key == "age")
            {
                int min = options.MinAge;
                int maxExclusive = options.MaxAge + 1;

                return context => context.RandomData.TryInt(min, maxExclusive, out int value) ? value : min;
            }

            if (key is "quantity" or "qty" or "count")
                return context => context.RandomData.TryInt(1, 101, out int value) ? value : 1;

            return context => context.RandomData.TryInt(0, 1000, out int value) ? value : 0;
        }

        private static Func<FakerContext, object?>? LongFactory(bool isPrimaryKey, bool isKeyColumn)
        {
            if (isPrimaryKey)
                return context => ToKey(context.ItemIndex, long.MaxValue);

            if (isKeyColumn)
                return null;

            return context => context.RandomData.TryInt64(0, 1_000_000, out long value) ? value : 0L;
        }

        private static Func<FakerContext, object?> DecimalFactory(string key)
        {
            if (key is "price" or "amount" or "salary" or "total" or "cost" or "balance" or "fee")
            {
                return context => context.RandomData.TryDecimal(1m, 10_000m, 2, out decimal value) ? value : 1m;
            }

            return context => context.RandomData.TryDecimal(0m, 1000m, 2, out decimal value) ? value : 0m;
        }

        private static Func<FakerContext, object?> DateOnlyFactory(string key, AutoFillOptions options)
        {
            DateOnly reference = options.ReferenceDate;

            if (key.Contains("birth", StringComparison.Ordinal) || key == "dob")
            {
                int minAge = options.MinAge;
                int maxAge = options.MaxAge;

                return context => context.Dates.TryDateOfBirth(reference, minAge, maxAge, out DateOnly value)
                    ? value
                    : reference;
            }

            DateOnly start = reference.AddYears(-5);

            return context => context.Dates.TryDateOnlyBetween(start, reference, out DateOnly value)
                ? value
                : reference;
        }

        private static Func<FakerContext, object?> DateTimeFactory(AutoFillOptions options)
        {
            DateTime end = options.ReferenceDate.ToDateTime(TimeOnly.MinValue);
            DateTime start = end.AddYears(-5);

            return context => context.Dates.TryDateTimeBetween(start, end, out DateTime value) ? value : end;
        }

        private static Func<FakerContext, object?>? EnumFactory(Type enumType)
        {
            Array values = Enum.GetValues(enumType);

            if (values.Length == 0)
                return null;

            object[] items = new object[values.Length];

            for (int i = 0; i < items.Length; i++)
            {
                items[i] = values.GetValue(i)!;
            }

            return context => context.RandomData.TryPick(items, out object value) ? value : items[0];
        }

        // Index 0 becomes key 1. The clamp only matters after billions of items.
        private static object ToKey(ulong itemIndex, long maxValue)
        {
            ulong key = itemIndex >= (ulong)maxValue ? (ulong)maxValue : itemIndex + 1;

            return maxValue == int.MaxValue ? (object)(int)key : (long)key;
        }

        private static string Normalize(string name)
        {
            var builder = new System.Text.StringBuilder(name.Length);

            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString();
        }

        // "Id", "ID", "id" or a camel/Pascal-case suffix such as "CustomerId" / "CustomerID".
        // "Paid" and "Valid" do not match because the suffix must start with a capital I.
        private static bool IsKeyName(string name, string key)
        {
            return key == "id"
                || name.EndsWith("Id", StringComparison.Ordinal)
                || name.EndsWith("ID", StringComparison.Ordinal);
        }
    }
}
