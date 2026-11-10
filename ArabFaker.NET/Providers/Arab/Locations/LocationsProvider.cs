﻿﻿﻿using ArabFaker.NET.Generation.Randomization;

namespace ArabFaker.NET.Providers.Arab.Locations
{
    internal sealed class LocationsProvider
    {
        private readonly RandomGenerator _random;

        internal LocationsProvider(RandomGenerator random)
        {
            _random = random;
        }

        internal bool TryPick(LocationLevel level, out LocationEntry location)
        {
            location = default;

            if (!Enum.IsDefined(level))
                return false;

            if (!LocationCatalog.TryGet(out var catalog))
                return false;

            if (!catalog.TryGetByLevel(level, out var locations))
                return false;

            return _random.TryPick(locations, out location);
        }

        internal bool TryGovernorate(out LocationEntry location)
        {
            return TryPick(LocationLevel.Governorate, out location);
        }

        internal bool TryDistrict(string governorateId, out LocationEntry location)
        {
            return TryPickChild(governorateId, LocationLevel.District, out location);
        }

        internal bool TryShiyakha(string districtId, out LocationEntry location)
        {
            return TryPickChild(districtId, LocationLevel.Shiyakha, out location);
        }

        internal bool TryPickName(LocationLevel level, LocationLanguage language, out string name)
        {
            name = string.Empty;

            if (!Enum.IsDefined(language))
                return false;

            if (!TryPick(level, out LocationEntry location))
                return false;

            return location.TryGetName(language, out name);
        }

        private bool TryPickChild(string parentId, LocationLevel childLevel, out LocationEntry location)
        {
            location = default;

            if (string.IsNullOrWhiteSpace(parentId))
                return false;

            if (!Enum.IsDefined(childLevel))
                return false;

            if (!LocationCatalog.TryGet(out var catalog))
                return false;

            if (!catalog.TryGetChildren(parentId, childLevel, out var children))
            {
                return false;
            }

            return _random.TryPick(children, out location);
        }
    }

}
