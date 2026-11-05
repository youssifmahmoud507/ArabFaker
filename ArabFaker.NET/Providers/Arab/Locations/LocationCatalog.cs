using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArabFaker.NET.Providers.Arab.Locations
{
    internal sealed class LocationCatalog
    {
        private const string ResourcePrefix = "ArabFaker.Data.Locations.Egypt.";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly Lazy<LocationCatalog?> CachedCatalog =
            new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly Dictionary<LocationLevel, LocationEntry[]> _byLevel;

        private readonly Dictionary<(string ParentId, LocationLevel Level), LocationEntry[]> _children;

        private LocationCatalog(LocationEntry[] entries)
        {
            _byLevel = Enum.GetValues<LocationLevel>().ToDictionary(level => level, level => entries.Where(entry => entry.Level == level).ToArray());

            _children = entries.Where(entry => entry.ParentId is not null).GroupBy(entry => (entry.ParentId!, entry.Level)).ToDictionary(group => group.Key, group => group.ToArray());
        }

        internal static bool TryGet(out LocationCatalog catalog)
        {
            LocationCatalog? cached = CachedCatalog.Value;

            if (cached is null)
            {
                catalog = null!;
                return false;
            }

            catalog = cached;
            return true;
        }

        internal bool TryGetByLevel(LocationLevel level, out IReadOnlyList<LocationEntry> entries)
        {
            if (_byLevel.TryGetValue(level, out var locations) &&
                locations.Length > 0)
            {
                entries = locations;
                return true;
            }

            entries = Array.Empty<LocationEntry>();
            return false;
        }

        internal bool TryGetChildren(string parentId, LocationLevel level, out IReadOnlyList<LocationEntry> children)
        {
            if (_children.TryGetValue((parentId, level), out var locations) &&
                locations.Length > 0)
            {
                children = locations;
                return true;
            }

            children = Array.Empty<LocationEntry>();
            return false;
        }

        internal int GetCount(LocationLevel level)
        {
            return _byLevel.TryGetValue(level, out var entries) ? entries.Length : 0;
        }

        private static LocationCatalog? Load()
        {
            try
            {
                Assembly assembly = typeof(LocationCatalog).Assembly;

                var entries = new List<LocationEntry>();

                if (!TryLoadFile(assembly, "all-governorate.json", LocationLevel.Governorate, entries))
                {
                    return null;
                }

                if (!TryLoadFile(assembly, "all-district.json", LocationLevel.District, entries))
                {
                    return null;
                }

                if (!TryLoadFile(assembly, "all-shiyakha.json", LocationLevel.Shiyakha, entries))
                {
                    return null;
                }

                if (!HasValidHierarchy(entries))
                    return null;

                var catalog = new LocationCatalog([.. entries]);

                foreach (LocationLevel level in Enum.GetValues<LocationLevel>())
                {
                    if (catalog.GetCount(level) == 0)
                        return null;
                }

                return catalog;
            }
            catch (JsonException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static bool TryLoadFile(Assembly assembly, string fileName, LocationLevel expectedLevel, List<LocationEntry> entries)
        {
            string resourceName = $"{ResourcePrefix}{fileName}";

            using Stream? stream =
                assembly.GetManifestResourceStream(resourceName);

            if (stream is null)
                return false;

            DatasetRecord[]? records = JsonSerializer.Deserialize<DatasetRecord[]>(stream, JsonOptions);

            if (records is null || records.Length == 0)
                return false;

            int previousCount = entries.Count;

            foreach (DatasetRecord record in records)
            {
                if (TryConvert(record, expectedLevel, out LocationEntry entry))
                {
                    entries.Add(entry);
                }
            }

            return entries.Count > previousCount;
        }

        private static bool TryConvert(DatasetRecord record, LocationLevel expectedLevel, out LocationEntry entry)
        {
            entry = default;

            if (string.IsNullOrWhiteSpace(record.Id))
                return false;

            if (record.Level != (int)expectedLevel)
                return false;

            if (record.Name is null || string.IsNullOrWhiteSpace(record.Name.Local) || string.IsNullOrWhiteSpace(record.Name.En))
            {
                return false;
            }

            string? parentId = record.Parent?.Id;

            if (expectedLevel != LocationLevel.Governorate && string.IsNullOrWhiteSpace(parentId))
            {
                return false;
            }

            double? latitude = TryParseCoordinate(record.Geo?.Lat, -90, 90);

            double? longitude = TryParseCoordinate(record.Geo?.Lon, -180, 180);

            entry = new LocationEntry(record.Id, expectedLevel, record.Name.Local, record.Name.En, parentId, latitude, longitude);

            return true;
        }

        private static double? TryParseCoordinate(string? value, double minimum, double maximum)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double coordinate))
            {
                return null;
            }

            if (!double.IsFinite(coordinate))
                return null;

            if (coordinate < minimum || coordinate > maximum)
                return null;

            return coordinate;
        }

        private static bool HasValidHierarchy(List<LocationEntry> entries)
        {
            var entriesById = new Dictionary<string, LocationEntry>(StringComparer.Ordinal);

            foreach (LocationEntry entry in entries)
            {
                if (!entriesById.TryAdd(entry.Id, entry))
                    return false;
            }

            foreach (LocationEntry entry in entries)
            {
                if (entry.Level == LocationLevel.Governorate)
                {
                    if (entry.ParentId is not null)
                        return false;

                    continue;
                }

                if (entry.ParentId is null || !entriesById.TryGetValue(entry.ParentId, out LocationEntry parent))
                {
                    return false;
                }

                LocationLevel expectedParentLevel = entry.Level == LocationLevel.District ? LocationLevel.Governorate : LocationLevel.District;

                if (parent.Level != expectedParentLevel)
                    return false;
            }

            return true;
        }

        private sealed class DatasetRecord
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("level")]
            public int Level { get; set; }

            [JsonPropertyName("name")]
            public DatasetName? Name { get; set; }

            [JsonPropertyName("parent")]
            public DatasetParent? Parent { get; set; }

            [JsonPropertyName("geo")]
            public DatasetGeo? Geo { get; set; }
        }

        private sealed class DatasetName
        {
            [JsonPropertyName("local")]
            public string? Local { get; set; }

            [JsonPropertyName("en")]
            public string? En { get; set; }
        }

        private sealed class DatasetParent
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }

        private sealed class DatasetGeo
        {
            [JsonPropertyName("lat")]
            public string? Lat { get; set; }

            [JsonPropertyName("lon")]
            public string? Lon { get; set; }
        }
    }

}
