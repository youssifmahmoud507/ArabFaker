# Golden files

ArabFaker promises that the same seed gives the same data. Two test classes pin that promise to real output,
stored in `ArabFaker.Tests/Golden`:

| File | Test | What it protects |
|------|------|------------------|
| `people-*.txt`, `employees-*.txt`, `university-*.txt` | `GoldenOutputTests` | What seed 2026 produces: names, reference data, national ids, related tables, CSV/JSON/SQL text |
| `PublicApi.txt` | `PublicApiSurfaceTests` | Every public type and member of `ArabFaker.NET` |

A failing test is a message, not necessarily a bug: it says "this change alters what a seed produces" or
"this change alters the public API". If the change is intended:

1. Regenerate the files:
   - PowerShell: `$env:ARABFAKER_UPDATE_GOLDEN = "1"; dotnet test; Remove-Item Env:ARABFAKER_UPDATE_GOLDEN`
   - bash: `ARABFAKER_UPDATE_GOLDEN=1 dotnet test`
   - or run the **Update golden files** workflow on your branch (not on master).
2. Read the diff of `ArabFaker.Tests/Golden`. Every changed line is a changed output or API.
3. Commit the files with the change and mention it in `CHANGELOG.md`. A changed seed output is a breaking change for
   anyone who stored expected data from an older version.

Line endings are ignored when comparing, and the files are marked `-text` in `.gitattributes`.
