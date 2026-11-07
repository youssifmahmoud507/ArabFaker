# ArabFaker.NET

A .NET library for generating **fake / synthetic test data**, with first-class support
for Arabic names and Egyptian administrative locations. You keep your own classes;
ArabFaker only gives you reproducible building blocks to fill them.

> **Status: under development (pre-release).** Parts 1–10 of the plan are implemented
> (randomization engine, seeds/streams, providers, `Faker` + `FakerContext`).
> `Define<T>() / RuleFor()` (Part 11) `Generate()` / `Generate(count)` / `GenerateLazy()` (Part 12) and `AutoFill()` (Part 13) are implemented.
> No NuGet package has been published.

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
