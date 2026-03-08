using EasyWarps.Models;

namespace EasyWarps.Services
{
    public static class WarpPointFiltering
    {
        public static IEnumerable<WarpPoint> ApplySearchFilter(
            IEnumerable<WarpPoint> points,
            string searchText,
            WarpSearchScope scope,
            IReadOnlyDictionary<string, string> precomputedSearchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return points;

            return scope switch
            {
                WarpSearchScope.Name => points.Where(p =>
                    p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)),
                WarpSearchScope.Location => points.Where(p =>
                    p.LocationName.Contains(searchText, StringComparison.OrdinalIgnoreCase)),
                _ => points.Where(p =>
                    precomputedSearchText.TryGetValue(p.Id, out var text) &&
                    text.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            };
        }

        public static IEnumerable<WarpPoint> ApplyCategoryFilter(
            IEnumerable<WarpPoint> points,
            WarpCategory category,
            Func<string, WarpCategory> classifier)
        {
            if (category == WarpCategory.All)
                return points;

            return points.Where(p => classifier(p.LocationName) == category);
        }

        public static IEnumerable<WarpPoint> ApplyFavoriteFilter(
            IEnumerable<WarpPoint> points,
            IReadOnlySet<string> favoriteIds)
        {
            return points.Where(p => favoriteIds.Contains(p.Id));
        }

        public static IOrderedEnumerable<WarpPoint> ApplySort(
            IEnumerable<WarpPoint> points,
            WarpSortMode sortMode)
        {
            return sortMode switch
            {
                WarpSortMode.ZToA => points.OrderByDescending(p => p.Name, StringComparer.OrdinalIgnoreCase),
                WarpSortMode.Newest => points
                    .OrderByDescending(p => p.CreatedDay)
                    .ThenByDescending(p => p.CreatedTick),
                WarpSortMode.Oldest => points
                    .OrderBy(p => p.CreatedDay)
                    .ThenBy(p => p.CreatedTick),
                WarpSortMode.LastUsed => points
                    .OrderByDescending(p => p.LastUsedDay > 0 ? 1 : 0)
                    .ThenByDescending(p => p.LastUsedDay)
                    .ThenByDescending(p => p.LastUsedTick),
                _ => points.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            };
        }
    }
}
