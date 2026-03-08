namespace EasyWarps.Models
{
    public class WarpFilterState
    {
        public string SearchText { get; set; } = "";
        public WarpSearchScope SearchScope { get; set; } = WarpSearchScope.All;
        public WarpCategory Category { get; set; } = WarpCategory.All;
        public WarpSortMode SortMode { get; set; } = WarpSortMode.AToZ;
        public bool FavoritesOnly { get; set; }

        public bool HasActiveFilters =>
            !string.IsNullOrEmpty(SearchText) ||
            Category != WarpCategory.All ||
            FavoritesOnly;

        public string ToCacheKey()
        {
            return $"{(int)SearchScope}|{SearchText}|{(int)Category}|{(int)SortMode}|{FavoritesOnly}";
        }

        public void Reset()
        {
            SearchText = "";
            SearchScope = WarpSearchScope.All;
            Category = WarpCategory.All;
            SortMode = WarpSortMode.AToZ;
            FavoritesOnly = false;
        }
    }
}
