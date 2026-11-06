# ArabFaker.NET

A .NET library for generating **fake / synthetic test data**, with first-class support
for Arabic names and Egyptian administrative locations. You keep your own classes;
ArabFaker only gives you reproducible building blocks to fill them.

> **Status: under development (pre-release).** Parts 1–10 of the plan are implemented
> (randomization engine, seeds/streams, providers, `Faker` + `FakerContext`).
> `Define<T>() / RuleFor()` (Part 11) is implemented; `Generate()` (Part 12) is not available yet.
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
