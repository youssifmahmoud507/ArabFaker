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
- Security and review workflows: CodeQL (security-extended), vulnerable-package check, secret scan, dependency review,
  OpenSSF Scorecard, workflow audit (zizmor), fuzzing of the exporters, reproducible-build check, compatibility matrix
  (time zones / globalization), mutation testing, link check and a code-quality report. Releases carry build provenance and an SBOM.
- Export security tests: hostile text round-trips through a real SQLite database, JSON and CSV.

### Testing

- Golden-file tests pin the output of seed 2026 (people, employees with reference data, related tables in SQL Server and
  MySQL dialects) and the full public API list. See `docs/GOLDEN-FILES.md`.
- `Update golden files` workflow regenerates them on a feature branch.
- `benchmarks/ArabFaker.Benchmarks` (BenchmarkDotNet): generation and export of 1,000 and 100,000 rows.

### Documentation

- XML `summary` comments on every internal type and on the remaining public members (random engine, seed derivation,
  providers, reference catalogs, locations catalog), written from the code so they describe what each piece does.
- Package description and tags now mention reference data, SQL dialects and determinism.

### Fixed

- Every GitHub Action in the workflows is pinned to a full commit SHA (with the version in a comment), so a moved tag cannot change what runs in CI. Dependabot keeps the pins current.
- OpenSSF Scorecard uses action v2.4.4, whose container comes from ghcr.io (v2.4.0 pulled from gcr.io, which now asks for billing).
- Culture test no longer fails when globalization-invariant mode is on (it builds a German-style culture from the invariant one).
- `.editorconfig` records why four analyzer rules are switched off for this library; `TryAnalyze` is static.

- SQL export wrote numbers inside quotes after the "never write unknown types raw" hardening; numeric types are written bare again.
- Formatting and analyzer findings from the quality report (whitespace, `Enum.IsDefined<T>`, xUnit2012).
- Compatibility workflow: Windows' `sha256sum` marker no longer makes identical files look different.
- Scorecard and Dependency review no longer turn builds red for reasons outside the code (gcr.io billing, Dependency graph setting).

### Security

- `CsvExportOptions.NeutralizeFormulas` (opt-in): text cells starting with `=`, `+`, `-`, `@`, tab or CR get a leading apostrophe.
- SQL export never writes a value of an unexpected type unquoted.
- CSV note: cells that start with `=`, `+`, `-` or `@` are written as they are; spreadsheet programs may treat them as formulas.
  Do not open generated CSV from untrusted text sources without checking.
- Reference data (`c.Reference`): 380 courses grouped under 38 academic departments, 214 job titles under 23 company departments, plus about 195 countries, 50 languages, 65 currency codes, nationalities, school subjects and stages, grade ratings, faculties, universities, industries, product categories, payment methods, banks, medical specialties, sports, hobbies, animals, fruits, vegetables, Egyptian dishes, family relations, car brands, Hijri and Coptic months, tourist attractions and more, in Arabic and English. Closed lists only; open-ended records such as orders stay user-defined.
- `c.Identifiers.GenerateNationalId()`: 14 random digits (no structure, test data only). `AutoFill` fills `NationalId` with it. No phone number generator is provided on purpose.
- `RuleFor(x => x.P, (x, c) => ...)`: a dependent rule that runs after all other rules and can read the other properties of the same object, so a job title or course can match the department written on the same row.
- `samples/ArabFaker.DatabaseDemo`: loads generated data into a real SQLite database with PRIMARY KEY, FOREIGN KEY, UNIQUE and CHECK constraints and verifies it with queries. CI runs it.
- `samples/ArabFaker.SqlServerDemo`: the same real-database check against SQL Server (Windows authentication by default, `--server`/`--connection` to change). It drops and recreates a database whose name starts with `ArabFaker`, loads the SQL Server dialect script (`N'..'` Arabic text) and checks constraints with SQL Server itself. Not run in CI because it needs a SQL Server instance.
- GitHub workflows: CI (build and test on Linux and Windows, coverage summary, samples, package check, and a consumer smoke test that installs the .nupkg into a fresh project), CodeQL, weekly NuGet vulnerability check, Dependabot, and a tag-triggered Release workflow (GitHub Release always; NuGet push only when `NUGET_API_KEY` exists and the `nuget` environment approves).
- `AutoFill` recognises `Course`, `Department`, `JobTitle`, `University`, `Faculty`, `Bank`, `Country`, `Currency`, `Specialty` and similar property names.
- Larger name catalog (123 male, 122 female and 139 family names, each in Arabic and Latin spelling).
- The quick-start sample writes UTF-8 files (`people.csv`, `people.json`, `dataset.json`, `dataset.sql`) and accepts `--latin`, `--dialect` and `--out`.

### Notes

- Changing the name catalog changes the names produced for a given seed.
- Nothing has been published to NuGet. Check that the package ID `ArabFaker.NET` is available
  before publishing.
- The embedded Egypt administrative divisions data is licensed separately (CC BY 4.0); see
  `THIRD-PARTY-NOTICES.md`.
