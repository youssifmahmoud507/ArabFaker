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
- Larger name catalog (123 male, 122 female and 139 family names, each in Arabic and Latin spelling).
- The quick-start sample writes UTF-8 files (`people.csv`, `people.json`, `dataset.json`, `dataset.sql`) and accepts `--latin`, `--dialect` and `--out`.

### Notes

- Changing the name catalog changes the names produced for a given seed.
- Nothing has been published to NuGet. Check that the package ID `ArabFaker.NET` is available
  before publishing.
- The embedded Egypt administrative divisions data is licensed separately (CC BY 4.0); see
  `THIRD-PARTY-NOTICES.md`.
