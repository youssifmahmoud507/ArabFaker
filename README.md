# ArabFaker.NET

A .NET library for generating **fake / synthetic test data**, with first-class support
for Arabic names and Egyptian administrative locations. You keep your own classes;
ArabFaker only gives you reproducible building blocks to fill them.

> **Status: pre-release (`0.1.0-preview.1`).** Generators, `Generate*`, `AutoFill`, datasets with
> relations, navigation properties and export are implemented and covered by tests. The public API
> may still change before `1.0.0`. No NuGet package has been published yet; see `CHANGELOG.md`.

## Installation

Not on NuGet yet. Until a package is published, add a project reference:

```
dotnet add reference path/to/ArabFaker.NET/ArabFaker.NET.csproj
```

A runnable example lives in `samples/ArabFaker.QuickStart` (`dotnet run --project samples/ArabFaker.QuickStart -- --latin`). It writes UTF-8 files to `quickstart-output/`; add `--latin` for English-letter names.

## What it is, and what it is not

- It **is** a test-data library: deterministic, offline, dependency-free, Arabic- and Egypt-aware.
- It is **not** a cryptographic or security tool: seeds make output predictable, identifiers are not
  secrets, and generated emails/names are fake and for testing only.
- It does not connect to databases or APIs. It produces objects, JSON, CSV and SQL scripts that you load yourself.

## Highlights

- Target framework: `net10.0`, no runtime dependencies, works offline.
- Seeded, reproducible output with independent random streams per provider.
- Providers: names (Arabic/Latin), internet (test-only emails), dates, locations (Egypt),
  text, random values, identifiers (UUID v4).
- No `throw`-based validation: providers use the `Try...` pattern.

## Defining a generator (Part 11)

```csharp
var faker = new Faker(seed: 2026);

var students = faker.Define<Student>()
    .RuleFor(x => x.FirstName, c => c.Names.TryFirstName(out var n) ? n : "Unknown")
    .RuleFor(x => x.Age, c => c.RandomData.TryInt(18, 40, out var a) ? a : 18);

// Invalid rules never throw; check students.IsValid / students.ConfigurationErrors.
```

## Generating objects (Part 12)

```csharp
GenerationResult<Student> one = students.Generate();

if (one.IsSuccess)
    Console.WriteLine(one.Value.FirstName);
else
    foreach (GenerationError error in one.Errors)
        Console.WriteLine($"{error.Code}: {error.Message}");

GenerationResult<IReadOnlyList<Student>> many = students.Generate(100); // all or nothing

foreach (GenerationResult<Student> item in students.GenerateLazy(1_000)) // generated while enumerating
{
    // each item has its own result
}
```

The same seed, definitions, rules and call order always produce the same objects.
The first `Generate*` call freezes the definition. Exceptions thrown by your own
factories and rules are not caught.

## Filling properties automatically (Part 13)

```csharp
var people = faker.Define<Person>()
    .AutoFill()                                   // fills what it can recognise
    .RuleFor(x => x.Name, c => "Custom");         // explicit rules always win

people.UnmappedProperties; // properties AutoFill did not touch (e.g. "CustomerId")
```

`AutoFill` fills properties by name and type: first/last/full names, e-mail, username,
governorate/city, `Age`, dates of birth, numbers, booleans, enums and GUIDs. A property named
`Id` gets 1, 2, 3, ... (or a GUID). It deliberately does **not** guess foreign keys such as
`CustomerId`, a bare `Name`, or unrecognised strings, so you never get meaningless data: those
are listed in `UnmappedProperties` and are meant to be handled by explicit rules (and, later,
by relations). Dates are computed from a fixed `AutoFillOptions.ReferenceDate`, never from the
real clock.

## Related tables without orphans (Part 14)

```csharp
var faker = new Faker(2026);
var dataset = faker.CreateDataset();

var students = dataset.Table(faker.Define<Student>().AutoFill(), 100, x => x.Id);
var courses  = dataset.Table(faker.Define<Course>().AutoFill(), 20, x => x.Id);
var enrollments = dataset.Table(faker.Define<Enrollment>().AutoFill(), 400, x => x.Id)
    .ForeignKey(x => x.StudentId, students)
    .ForeignKey(x => x.CourseId, courses)
    .UniqueTogether(x => x.StudentId, x => x.CourseId);   // one enrollment per pair

var employees = dataset.Table(faker.Define<Employee>().AutoFill(), 50, x => x.Id)
    .SelfReference(x => x.ManagerId);                      // hierarchy, no cycles

GenerationResult<GeneratedDataset> result = dataset.Build();

if (result.IsSuccess)
{
    IReadOnlyList<Enrollment> rows = result.Value.Rows(enrollments);
}
```

Every foreign key points to a row that exists in the same dataset. Tables can be listed in any
order and may even reference each other. Foreign keys can be optional (`nullChance`, on a
nullable property), and a mistake in the setup (wrong type, duplicate relation, too many rows for a
unique group, an empty parent table, duplicate keys) is reported as an error before or instead of
producing data. 
For EF-style classes, fill navigation properties too (register the foreign key first):

```csharp
enrollments.Navigation(x => x.StudentId, x => x.Student);                // enrollment.Student
students.Collection(x => x.Enrollments, enrollments, e => e.StudentId);  // student.Enrollments
employees.Collection(x => x.Reports, employees, x => x.ManagerId);       // hierarchy
```

Navigation objects point to the very same instances that are in the dataset, and every parent gets a
list (empty if nothing references it). Both directions refer to each other, so serialise with a
cycle-aware setting. A single class with no relations needs none of this: use `Define<T>().Generate()`.

## Exporting data (Part 16)

```csharp
var rows = faker.Define<Student>().AutoFill().Generate(100).Value!;

string json = RowExporter.ToJson(rows).Value!;
string csv  = RowExporter.ToCsv(rows).Value!;
string sql  = RowExporter.ToSql(rows, new SqlExportOptions { Dialect = SqlDialect.SqlServer }).Value!;

// A whole dataset: INSERT statements with referenced tables first, or one JSON object per table.
string script = generated.ToSql().Value!;
string all    = generated.ToJson().Value!;
```

Only public read/write scalar properties are written (text, numbers, booleans, GUIDs, dates, enums);
navigation objects and collections are skipped, so cyclic graphs export fine. Output never depends on
the machine culture, Arabic text stays readable in JSON, and SQL Server output uses `N'...'` literals.
Tables that reference each other cannot be ordered and are written in registration order.
Problems are returned as errors (`result.Errors`), never thrown.

## More documentation

- `docs/ArabFaker.NET-Project-Complete-Guide.md` — design, decisions and per-part implementation notes (Arabic).
- `docs/PublicAPI.md` — the public API surface.
- `docs/RELEASE-CHECKLIST.md` — what to verify before publishing.
- `CHANGELOG.md` and `THIRD-PARTY-NOTICES.md`.

## Not for security use

The generators use a seeded, non-cryptographic PRNG (Xoshiro256**). Do **not** use them
for passwords, tokens, API keys or any security-sensitive value. E-mails use the reserved
`.test` TLD and are never deliverable.

## Build & test

```bash
dotnet build --configuration Release
dotnet test
```

## License

Code: MIT (`LICENSE.txt`). Embedded data: see `THIRD-PARTY-NOTICES.md`
(Egypt administrative divisions — CC BY 4.0).
