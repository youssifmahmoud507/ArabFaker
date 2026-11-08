# Public API surface (ArabFaker.NET 0.1.0-preview.1)

Generated from the source on 8 November 2026 by a simple text scan, so formatting is approximate and
generic constraints are omitted. Regenerate it before every release and compare it with the previous
version to spot accidental breaking changes. Internal types and members are not listed.

## DatasetBuilder  (`ArabFaker.NET.Dataset`)

```csharp
public sealed class DatasetBuilder
public IReadOnlyList<ConfigurationError> ConfigurationErrors
public bool IsValid
public DatasetTable<T, TKey> Table<T, TKey>(
public GenerationResult<GeneratedDataset> Build()
```

## DatasetTable  (`ArabFaker.NET.Dataset`)

```csharp
public sealed class DatasetTable<T, TKey> : ITableRuntime
public int RowCount { get; }
public DatasetTable<T, TKey> ForeignKey<TProperty, TParent, TParentKey>(
public DatasetTable<T, TKey> SelfReference<TProperty>(
public DatasetTable<T, TKey> UniqueTogether<TFirst, TSecond>(
public DatasetTable<T, TKey> UniqueTogether<TFirst, TSecond, TThird>(
public DatasetTable<T, TKey> Navigation<TForeignKey, TNavigation>(
public DatasetTable<T, TKey> Collection<TCollection, TChild, TChildKey, TForeignKey>(
```

## GeneratedDataset  (`ArabFaker.NET.Dataset`)

```csharp
public sealed class GeneratedDataset
public IReadOnlyList<T> Rows<T, TKey>(DatasetTable<T, TKey> table)
public GenerationResult<string> ToSql(SqlExportOptions? options = null)
public GenerationResult<string> ToJson(JsonExportOptions? options = null)
```

## JsonExportOptions  (`ArabFaker.NET.Export`)

```csharp
public sealed class JsonExportOptions
public bool Indented { get; }
public bool CamelCase { get; }
```

## RowExporter  (`ArabFaker.NET.Export`)

```csharp
public static class RowExporter
public static GenerationResult<string> ToJson<T>(IEnumerable<T>? rows, JsonExportOptions? options = null)
public static GenerationResult<string> ToCsv<T>(IEnumerable<T>? rows)
public static GenerationResult<string> ToSql<T>(IEnumerable<T>? rows, SqlExportOptions? options = null)
```

## SqlDialect  (`ArabFaker.NET.Export`)

```csharp
public enum SqlDialect
```

## SqlExportOptions  (`ArabFaker.NET.Export`)

```csharp
public sealed class SqlExportOptions
public SqlDialect Dialect { get; }
public string? TableName { get; }
public bool EnumsAsText { get; }
```

## Faker  (`ArabFaker.NET`)

```csharp
public sealed class Faker(long seed)
public long Seed { get; }
public Faker() : this(Random.Shared.NextInt64())
public FakerDefinition<T> Define<T>() where T : class, new()
public FakerDefinition<T> Define<T>(Func<T> factory) where T : class
public DatasetBuilder CreateDataset()
```

## AutoFillOptions  (`ArabFaker.NET.Generation`)

```csharp
public sealed class AutoFillOptions
public DateOnly ReferenceDate { get; }
public int MinAge { get; }
public int MaxAge { get; }
public NameLanguage Language { get; }
```

## ConfigurationError  (`ArabFaker.NET.Generation`)

```csharp
public sealed record ConfigurationError(string Code, string Message);
```

## ConfigurationErrorCodes  (`ArabFaker.NET.Generation`)

```csharp
public static class ConfigurationErrorCodes
public const string NullFactory = "Faker.NullFactory";
public const string NullPropertySelector = "Faker.NullPropertySelector";
public const string NullValueFactory = "Faker.NullValueFactory";
public const string UnsupportedSelector = "Faker.UnsupportedSelector";
public const string NestedPropertyNotSupported = "Faker.NestedPropertyNotSupported";
public const string StaticMemberNotSupported = "Faker.StaticMemberNotSupported";
public const string FieldNotSupported = "Faker.FieldNotSupported";
public const string IndexerNotSupported = "Faker.IndexerNotSupported";
public const string PropertyNotWritable = "Faker.PropertyNotWritable";
public const string InitOnlyPropertyNotSupported = "Faker.InitOnlyPropertyNotSupported";
public const string PropertyTypeMismatch = "Faker.PropertyTypeMismatch";
public const string DefinitionFrozen = "Faker.DefinitionFrozen";
public const string InvalidAutoFillOptions = "Faker.InvalidAutoFillOptions";
public const string NullDefinition = "Faker.NullDefinition";
public const string DuplicateTable = "Faker.DuplicateTable";
public const string InvalidRowCount = "Faker.InvalidRowCount";
public const string ForeignKeyTargetInvalid = "Faker.ForeignKeyTargetInvalid";
public const string ForeignKeyTypeMismatch = "Faker.ForeignKeyTypeMismatch";
public const string ForeignKeyPropertyConflict = "Faker.ForeignKeyPropertyConflict";
public const string ForeignKeyNotNullable = "Faker.ForeignKeyNotNullable";
public const string InvalidNullChance = "Faker.InvalidNullChance";
public const string InvalidRoots = "Faker.InvalidRoots";
public const string InvalidUniqueGroup = "Faker.InvalidUniqueGroup";
public const string NavigationForeignKeyMissing = "Faker.NavigationForeignKeyMissing";
public const string NavigationTypeMismatch = "Faker.NavigationTypeMismatch";
public const string NavigationPropertyConflict = "Faker.NavigationPropertyConflict";
public const string NavigationTargetInvalid = "Faker.NavigationTargetInvalid";
public const string DefinitionIdExhausted = "Faker.DefinitionIdExhausted";
```

## FakerContext  (`ArabFaker.NET.Generation`)

```csharp
public sealed class FakerContext
public TextGenerator Text { get; }
public NameGenerator Names { get; }
public InternetGenerator Internet { get; }
public DatesGenerator Dates { get; }
public LocationsGenerator Locations { get; }
public RandomDataGenerator RandomData { get; }
public IdentifierGenerator Identifiers { get; }
public ulong ItemIndex { get; }
```

## FakerDefinition  (`ArabFaker.NET.Generation`)

```csharp
public sealed class FakerDefinition<T> where T : class
public IReadOnlyList<ConfigurationError> ConfigurationErrors
public bool IsValid
public int RuleCount
public bool IsFrozen
public IReadOnlyList<string> UnmappedProperties
public FakerDefinition<T> RuleFor<TProperty>(
public FakerDefinition<T> AutoFill()
public FakerDefinition<T> AutoFill(AutoFillOptions options)
public FakerDefinition<T> Freeze()
public GenerationResult<T> Generate()
public GenerationResult<IReadOnlyList<T>> Generate(int count)
public IEnumerable<GenerationResult<T>> GenerateLazy(int count)
```

## GenerationError  (`ArabFaker.NET.Generation`)

```csharp
public sealed record GenerationError(string Code, string Message);
```

## GenerationErrorCodes  (`ArabFaker.NET.Generation`)

```csharp
public static class GenerationErrorCodes
public const string InvalidDefinition = "Faker.InvalidDefinition";
public const string FactoryReturnedNull = "Faker.FactoryReturnedNull";
public const string InvalidCount = "Faker.InvalidCount";
public const string ItemIndexExhausted = "Faker.ItemIndexExhausted";
public const string NullKey = "Faker.NullKey";
public const string DuplicateKey = "Faker.DuplicateKey";
public const string EmptyParentTable = "Faker.EmptyParentTable";
public const string UniqueConstraintUnsatisfiable = "Faker.UniqueConstraintUnsatisfiable";
public const string NullRows = "Faker.NullRows";
public const string NullRow = "Faker.NullRow";
public const string NoExportableColumns = "Faker.NoExportableColumns";
public const string InvalidExportOptions = "Faker.InvalidExportOptions";
public const string DuplicateTableName = "Faker.DuplicateTableName";
```

## GenerationResult  (`ArabFaker.NET.Generation`)

```csharp
public sealed class GenerationResult<T>
public bool IsSuccess
public T? Value { get; }
public IReadOnlyList<GenerationError> Errors { get; }
```

## LocationInfo  (`ArabFaker.NET.Providers.Arab.Locations`)

```csharp
public sealed record LocationInfo
public string Id { get; }
public LocationLevel Level { get; }
public string ArabicName { get; }
public string EnglishName { get; }
public string? ParentId { get; }
public double? Latitude { get; }
public double? Longitude { get; }
```

## LocationLanguage  (`ArabFaker.NET.Providers.Arab.Locations`)

```csharp
public enum LocationLanguage
```

## LocationLevel  (`ArabFaker.NET.Providers.Arab.Locations`)

```csharp
public enum LocationLevel
```

## LocationsGenerator  (`ArabFaker.NET.Providers.Arab.Locations`)

```csharp
public sealed class LocationsGenerator
public bool TryPick(LocationLevel level, out LocationInfo location)
public bool TryGovernorate(out LocationInfo location)
public bool TryDistrict(string governorateId, out LocationInfo location)
public bool TryShiyakha(string districtId, out LocationInfo location)
public bool TryPickName(LocationLevel level, LocationLanguage language, out string name)
```

## NameGender  (`ArabFaker.NET.Providers.Arab.Names`)

```csharp
public enum NameGender
```

## NameGenerator  (`ArabFaker.NET.Providers.Arab.Names`)

```csharp
public sealed class NameGenerator
public bool TryFirstName(
public bool TryMiddleName(
public bool TryLastName(
public bool TryFullName(out string name, NameLanguage language = NameLanguage.Arabic, NameGender gender = NameGender.Any)
```

## NameLanguage  (`ArabFaker.NET.Providers.Arab.Names`)

```csharp
public enum NameLanguage
```

## DatesGenerator  (`ArabFaker.NET.Providers.Common.Dates`)

```csharp
public sealed class DatesGenerator
public bool TryDateOnlyBetween(DateOnly startInclusive, DateOnly endInclusive, out DateOnly value)
public bool TryDateTimeBetween(DateTime startInclusive, DateTime endInclusive, out DateTime value)
public bool TryDateOfBirth(DateOnly asOfDate, int minAge, int maxAge, out DateOnly dateOfBirth)
```

## IdentifierGenerator  (`ArabFaker.NET.Providers.Common.Identifiers`)

```csharp
public sealed class IdentifierGenerator
public Guid GenerateGuid()
public string GenerateGuidString()
```

## InternetGenerator  (`ArabFaker.NET.Providers.Common.Internet`)

```csharp
public sealed class InternetGenerator
public bool TryUsername(out string username)
public bool TryEmail(out string email, string domain = "example.test")
```

## RandomDataGenerator  (`ArabFaker.NET.Providers.Common.Random`)

```csharp
public sealed class RandomDataGenerator
public bool TryInt(int minInclusive, int maxExclusive, out int value)
public bool TryInt64(long minInclusive, long maxExclusive, out long value)
public bool TryDecimal(decimal minInclusive, decimal maxInclusive, int decimalPlaces, out decimal value)
public double NextDouble()
public bool TryBoolean(double trueProbability, out bool value)
public bool TryPick<T>(IReadOnlyList<T>? items, out T value)
public bool TryPickMany<T>(IReadOnlyList<T>? items, int count, bool allowDuplicates, out T[] values)
```

## TextGenerator  (`ArabFaker.NET.Providers.Common.Text`)

```csharp
public sealed class TextGenerator
public bool TryDigits(int length, out string value)
public bool TryLowercaseLetters(int length, out string value)
public bool TryUppercaseLetters(int length, out string value)
public bool TryLetters(int length, out string value)
public bool TryAlphaNumeric(int length, out string value)
public bool TryHexadecimal(int length, out string value)
```
