# Changelog

All notable changes are listed here. The project follows [Semantic Versioning](https://semver.org/);
versions below `1.0.0` may change the public API.

## [Unreleased] — 0.1.0-preview.1 (not published)

### Added

- Deterministic randomization engine with independent streams per provider (seeded, reproducible).
- Providers: names (Arabic/Latin), internet (test-only emails), dates, Egyptian locations, text,
  random values and identifiers.
- `Define<T>()` / `RuleFor()` generators with validation errors instead of exceptions.
- `Generate()`, `Generate(count)` and `GenerateLazy(count)` with `GenerationResult<T>`.
- `AutoFill()` conventions by property name and type, with `UnmappedProperties`.
- Datasets: foreign keys, optional foreign keys, self-referencing hierarchies and unique
  combinations, so generated tables never contain orphan references.
- Navigation properties and collections for EF-style classes.
- Export to JSON, CSV and SQL INSERT (Standard, SQL Server, MySQL dialects).
- CI workflow, NuGet package metadata and a quick-start sample.
- Reference data (`c.Reference`): 380 courses grouped under 38 academic departments, 214 job titles under 23 company departments, plus about 195 countries, 50 languages, 65 currency codes, nationalities, school subjects and stages, grade ratings, faculties, universities, industries, product categories, payment methods, banks, medical specialties, sports, hobbies, animals, fruits, vegetables, Egyptian dishes, family relations, car brands, Hijri and Coptic months, tourist attractions and more, in Arabic and English. Closed lists only; open-ended records such as orders stay user-defined.
- `c.Identifiers.GenerateNationalId()`: 14 random digits (no structure, test data only). `AutoFill` fills `NationalId` with it. No phone number generator is provided on purpose.
- `RuleFor(x => x.P, (x, c) => ...)`: a dependent rule that runs after all other rules and can read the other properties of the same object, so a job title or course can match the department written on the same row.
- `samples/ArabFaker.DatabaseDemo`: loads generated data into a real SQLite database with PRIMARY KEY, FOREIGN KEY, UNIQUE and CHECK constraints and verifies it with queries. CI runs it.
- `samples/ArabFaker.SqlServerDemo`: the same real-database check against SQL Server (Windows authentication by default, `--server`/`--connection` to change). It drops and recreates a database whose name starts with `ArabFaker`, loads the SQL Server dialect script (`N'..'` Arabic text) and checks constraints with SQL Server itself. Not run in CI because it needs a SQL Server instance.
- `AutoFill` recognises `Course`, `Department`, `JobTitle`, `University`, `Faculty`, `Bank`, `Country`, `Currency`, `Specialty` and similar property names.
- Larger name catalog (123 male, 122 female and 139 family names, each in Arabic and Latin spelling).
- The quick-start sample writes UTF-8 files (`people.csv`, `people.json`, `dataset.json`, `dataset.sql`) and accepts `--latin`, `--dialect` and `--out`.

### Notes

- Changing the name catalog changes the names produced for a given seed.
- Nothing has been published to NuGet. Check that the package ID `ArabFaker.NET` is available
  before publishing.
- The embedded Egypt administrative divisions data is licensed separately (CC BY 4.0); see
  `THIRD-PARTY-NOTICES.md`.
