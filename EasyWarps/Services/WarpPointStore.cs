using StardewModdingAPI;
using EasyWarps.Models;
using EasyWarps.Utilities;

namespace EasyWarps.Services
{
    public class WarpPointStore
    {
        private const string SaveDataKey = "WarpPoints";

        private readonly Dictionary<string, WarpPoint> byId = new();
        private readonly Dictionary<string, WarpPoint> byLocationKey = new();
        private readonly HashSet<string> favoriteIds = new();
        private readonly Dictionary<string, string> searchText = new(StringComparer.OrdinalIgnoreCase);

        private List<WarpPoint>? cachedFilteredPoints;
        private string? cachedFilterKey;

        public int Count => byId.Count;

        public static string MakeLocationKey(string locationName, int tileX, int tileY)
        {
            return $"{locationName}:{tileX},{tileY}";
        }

        public WarpPoint? GetById(string id)
        {
            return byId.TryGetValue(id, out var point) ? point : null;
        }

        public WarpPoint? GetByLocationKey(string locationKey)
        {
            return byLocationKey.TryGetValue(locationKey, out var point) ? point : null;
        }

        public WarpPoint? GetByLocationKey(string locationName, int tileX, int tileY)
        {
            return GetByLocationKey(MakeLocationKey(locationName, tileX, tileY));
        }

        public IReadOnlyList<WarpPoint> GetAllPoints()
        {
            return byId.Values.ToList();
        }

        public bool Add(WarpPoint point)
        {
            var locationKey = MakeLocationKey(point.LocationName, point.TileX, point.TileY);
            if (byLocationKey.ContainsKey(locationKey))
            {
                DebugLogger.Log($"Duplicate location key: {locationKey}", StardewModdingAPI.LogLevel.Warn);
                return false;
            }

            if (string.IsNullOrEmpty(point.Id))
                point.Id = Guid.NewGuid().ToString();

            byId[point.Id] = point;
            byLocationKey[locationKey] = point;
            if (point.IsFavorite)
                favoriteIds.Add(point.Id);
            BuildSearchTextForPoint(point);
            InvalidateFilterCache();
            return true;
        }

        public void Update(WarpPoint point)
        {
            if (!byId.ContainsKey(point.Id))
                return;

            RemoveFromIndexes(point.Id);
            byId[point.Id] = point;
            AddToIndexes(point);
            InvalidateFilterCache();
        }

        public void Delete(string id)
        {
            if (!byId.ContainsKey(id))
                return;

            RemoveFromIndexes(id);
            byId.Remove(id);
            InvalidateFilterCache();
        }

        public bool RemoveByLocationKey(string locationKey)
        {
            if (!byLocationKey.TryGetValue(locationKey, out var point))
                return false;

            Delete(point.Id);
            return true;
        }

        public void ToggleFavorite(string id)
        {
            if (!byId.TryGetValue(id, out var point))
                return;

            point.IsFavorite = !point.IsFavorite;
            if (point.IsFavorite)
                favoriteIds.Add(id);
            else
                favoriteIds.Remove(id);
            InvalidateFilterCache();
        }

        public List<WarpPoint> GetFilteredPoints(WarpFilterState filter, Func<string, WarpCategory> categoryClassifier)
        {
            var key = filter.ToCacheKey();
            if (cachedFilterKey == key && cachedFilteredPoints != null)
                return cachedFilteredPoints;

            var points = byId.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(filter.SearchText))
                points = WarpPointFiltering.ApplySearchFilter(points, filter.SearchText, filter.SearchScope, searchText);

            if (filter.Category != WarpCategory.All)
                points = WarpPointFiltering.ApplyCategoryFilter(points, filter.Category, categoryClassifier);

            if (filter.FavoritesOnly)
                points = WarpPointFiltering.ApplyFavoriteFilter(points, favoriteIds);

            cachedFilteredPoints = WarpPointFiltering.ApplySort(points, filter.SortMode).ToList();
            cachedFilterKey = key;
            return cachedFilteredPoints;
        }

        public void Load(IModHelper helper)
        {
            var data = helper.Data.ReadSaveData<WarpPointSaveData>(SaveDataKey);
            Clear();

            if (data?.Points == null)
                return;

            foreach (var point in data.Points)
            {
                if (string.IsNullOrEmpty(point.Id))
                    point.Id = Guid.NewGuid().ToString();
                byId[point.Id] = point;
            }

            RebuildIndexes();
            DebugLogger.Trace($"Loaded {byId.Count} warp points from save data");
        }

        public void Save(IModHelper helper)
        {
            var data = new WarpPointSaveData { Points = byId.Values.ToList() };
            helper.Data.WriteSaveData(SaveDataKey, data);
            DebugLogger.Trace($"Saved {data.Points.Count} warp points to save data");
        }

        public void Clear()
        {
            byId.Clear();
            byLocationKey.Clear();
            favoriteIds.Clear();
            searchText.Clear();
            InvalidateFilterCache();
        }

        internal void RebuildIndexes()
        {
            byLocationKey.Clear();
            favoriteIds.Clear();
            searchText.Clear();

            foreach (var point in byId.Values)
                AddToIndexes(point);

            InvalidateFilterCache();
        }

        private void AddToIndexes(WarpPoint point)
        {
            var locationKey = MakeLocationKey(point.LocationName, point.TileX, point.TileY);
            byLocationKey[locationKey] = point;
            if (point.IsFavorite)
                favoriteIds.Add(point.Id);
            BuildSearchTextForPoint(point);
        }

        private void RemoveFromIndexes(string id)
        {
            if (!byId.TryGetValue(id, out var point))
                return;

            var locationKey = MakeLocationKey(point.LocationName, point.TileX, point.TileY);
            byLocationKey.Remove(locationKey);
            favoriteIds.Remove(id);
            searchText.Remove(id);
        }

        private void BuildSearchTextForPoint(WarpPoint point)
        {
            searchText[point.Id] = $"{point.Name} {point.LocationName}";
        }

        private void InvalidateFilterCache()
        {
            cachedFilteredPoints = null;
            cachedFilterKey = null;
        }
    }

    public class WarpPointSaveData
    {
        public List<WarpPoint> Points { get; set; } = new();
    }
}
