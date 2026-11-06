# Third-Party Notices

ArabFaker.NET's source code is licensed under the MIT License (see `LICENSE.txt`).
The embedded datasets listed below are **not** covered by that license; they keep
their own terms.

## Egypt Administrative Divisions Dataset

- Source: https://github.com/open-admin-data/egypt-administrative-divisions
- Project site: https://openadmindata.org/eg/
- License: Creative Commons Attribution 4.0 International (CC BY 4.0)
- License URL: https://creativecommons.org/licenses/by/4.0/

Embedded files (`ArabFaker.NET/Data/Locations/Egypt/`):

- `all-governorate.json` (27 governorates)
- `all-district.json` (365 districts)
- `all-shiyakha.json` (5,716 shiyakhas)

Changes made by ArabFaker.NET: none to the files themselves. They are embedded as-is
and read at runtime; only a subset of the fields (id, level, names, parent, geo) is used.

The dataset provider does not endorse or sponsor ArabFaker.NET.
Re-verify the source license and record counts before every release.
