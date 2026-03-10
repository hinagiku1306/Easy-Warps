using EasyWarps.Models;
using EasyWarps.Services;
using Xunit;

namespace EasyWarps.Tests.Services
{
    public class WarpPointFilteringTests
    {
        private static WarpPoint MakePoint(string name, string location, bool favorite = false,
            uint createdDay = 1, long createdTick = 100, uint lastUsedDay = 0, long lastUsedTick = 0)
        {
            return new WarpPoint
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                LocationName = location,
                IsFavorite = favorite,
                CreatedDay = createdDay,
                CreatedTick = createdTick,
                LastUsedDay = lastUsedDay,
                LastUsedTick = lastUsedTick
            };
        }

        private static Dictionary<string, string> BuildSearchText(IEnumerable<WarpPoint> points)
        {
            return points.ToDictionary(p => p.Id, p => $"{p.Name} {p.LocationName}");
        }

        #region Search Filter

        // Expected: Scope All matches either name or location
        [Fact]
        public void ApplySearchFilter_ScopeAll_MatchesNameOrLocation()
        {
            var points = new[]
            {
                MakePoint("Farm Gate", "Farm"),
                MakePoint("Mine Entrance", "Mine"),
                MakePoint("Beach House", "Beach")
            };
            var searchText = BuildSearchText(points);

            var results = WarpPointFiltering.ApplySearchFilter(points, "Mine", WarpSearchScope.All, searchText).ToList();

            Assert.Single(results);
            Assert.Equal("Mine Entrance", results[0].Name);
        }

        // Expected: Scope All matches location even when name doesn't contain search text
        [Fact]
        public void ApplySearchFilter_ScopeAll_MatchesLocationOnly()
        {
            var points = new[] { MakePoint("Home Base", "Farm") };
            var searchText = BuildSearchText(points);

            var results = WarpPointFiltering.ApplySearchFilter(points, "Farm", WarpSearchScope.All, searchText).ToList();

            Assert.Single(results);
        }

        // Expected: Scope Name matches only on name field
        [Fact]
        public void ApplySearchFilter_ScopeName_MatchesOnlyName()
        {
            var points = new[]
            {
                MakePoint("Farm Gate", "Town"),
                MakePoint("Town Square", "Farm")
            };
            var searchText = BuildSearchText(points);

            var results = WarpPointFiltering.ApplySearchFilter(points, "Farm", WarpSearchScope.Name, searchText).ToList();

            Assert.Single(results);
            Assert.Equal("Farm Gate", results[0].Name);
        }

        // Expected: Search is case-insensitive
        [Fact]
        public void ApplySearchFilter_CaseInsensitive()
        {
            var points = new[] { MakePoint("FARM gate", "farm") };
            var searchText = BuildSearchText(points);

            var byName = WarpPointFiltering.ApplySearchFilter(points, "farm", WarpSearchScope.Name, searchText).ToList();
            var byAll = WarpPointFiltering.ApplySearchFilter(points, "fArM", WarpSearchScope.All, searchText).ToList();

            Assert.Single(byName);
            Assert.Single(byAll);
        }

        // Expected: Empty search text returns all points
        [Fact]
        public void ApplySearchFilter_EmptyText_ReturnsAll()
        {
            var points = new[] { MakePoint("A", "B"), MakePoint("C", "D") };
            var searchText = BuildSearchText(points);

            var results = WarpPointFiltering.ApplySearchFilter(points, "", WarpSearchScope.All, searchText).ToList();

            Assert.Equal(2, results.Count);
        }

        // Expected: Empty-name point is included in scope All search by location
        [Fact]
        public void ApplySearchFilter_EmptyName_ScopeAll_MatchesByLocation()
        {
            var points = new[] { MakePoint("", "BathHouse_WomensLocker") };
            var searchText = BuildSearchText(points);

            var results = WarpPointFiltering.ApplySearchFilter(points, "Bath", WarpSearchScope.All, searchText).ToList();

            Assert.Single(results);
        }

        // Expected: Empty-name point returns no results for scope Name search
        [Fact]
        public void ApplySearchFilter_EmptyName_ScopeName_NoMatch()
        {
            var points = new[] { MakePoint("", "BathHouse_WomensLocker") };
            var searchText = BuildSearchText(points);

            var results = WarpPointFiltering.ApplySearchFilter(points, "Bath", WarpSearchScope.Name, searchText).ToList();

            Assert.Empty(results);
        }

        #endregion

        #region Category Filter

        // Expected: Category All returns all points
        [Fact]
        public void ApplyCategoryFilter_All_ReturnsAll()
        {
            var points = new[] { MakePoint("A", "Farm"), MakePoint("B", "Town") };

            var results = WarpPointFiltering.ApplyCategoryFilter(points, WarpCategory.All, _ => WarpCategory.World).ToList();

            Assert.Equal(2, results.Count);
        }

        // Expected: Category Farm returns only farm-classified points
        [Fact]
        public void ApplyCategoryFilter_Farm_ReturnsFarmOnly()
        {
            var points = new[]
            {
                MakePoint("A", "Farm"),
                MakePoint("B", "Town"),
                MakePoint("C", "Greenhouse")
            };
            WarpCategory Classify(string loc) => loc is "Farm" or "Greenhouse" ? WarpCategory.Farm : WarpCategory.World;

            var results = WarpPointFiltering.ApplyCategoryFilter(points, WarpCategory.Farm, Classify).ToList();

            Assert.Equal(2, results.Count);
            Assert.All(results, p => Assert.True(p.LocationName is "Farm" or "Greenhouse"));
        }

        // Expected: Category World returns only world-classified points
        [Fact]
        public void ApplyCategoryFilter_World_ReturnsWorldOnly()
        {
            var points = new[]
            {
                MakePoint("A", "Farm"),
                MakePoint("B", "Town"),
                MakePoint("C", "Mine")
            };
            WarpCategory Classify(string loc) => loc == "Farm" ? WarpCategory.Farm : WarpCategory.World;

            var results = WarpPointFiltering.ApplyCategoryFilter(points, WarpCategory.World, Classify).ToList();

            Assert.Equal(2, results.Count);
            Assert.DoesNotContain(results, p => p.LocationName == "Farm");
        }

        #endregion

        #region Favorite Filter

        // Expected: Favorite filter returns only favorites
        [Fact]
        public void ApplyFavoriteFilter_ReturnsFavoritesOnly()
        {
            var favPoint = MakePoint("Fav", "Farm", favorite: true);
            var normalPoint = MakePoint("Normal", "Town");
            var points = new[] { favPoint, normalPoint };
            var favoriteIds = new HashSet<string> { favPoint.Id };

            var results = WarpPointFiltering.ApplyFavoriteFilter(points, favoriteIds).ToList();

            Assert.Single(results);
            Assert.Same(favPoint, results[0]);
        }

        #endregion

        #region Sort

        // Expected: Newest sorts by CreatedDay desc then CreatedTick desc
        [Fact]
        public void ApplySort_Newest_ByCreatedDayThenTick()
        {
            var points = new[]
            {
                MakePoint("Old", "A", createdDay: 1, createdTick: 50),
                MakePoint("New", "B", createdDay: 3, createdTick: 10),
                MakePoint("Mid", "C", createdDay: 1, createdTick: 200)
            };

            var results = WarpPointFiltering.ApplySort(points, WarpSortMode.Newest).ToList();

            Assert.Equal("New", results[0].Name);
            Assert.Equal("Mid", results[1].Name);
            Assert.Equal("Old", results[2].Name);
        }

        // Expected: Oldest sorts by CreatedDay asc then CreatedTick asc
        [Fact]
        public void ApplySort_Oldest_ByCreatedDayThenTick()
        {
            var points = new[]
            {
                MakePoint("New", "A", createdDay: 3, createdTick: 10),
                MakePoint("Old", "B", createdDay: 1, createdTick: 50),
                MakePoint("Mid", "C", createdDay: 1, createdTick: 200)
            };

            var results = WarpPointFiltering.ApplySort(points, WarpSortMode.Oldest).ToList();

            Assert.Equal("Old", results[0].Name);
            Assert.Equal("Mid", results[1].Name);
            Assert.Equal("New", results[2].Name);
        }

        // Expected: LastUsed sorts by LastUsedDay desc then LastUsedTick desc, never-used at bottom
        [Fact]
        public void ApplySort_LastUsed_NeverUsedAtBottom()
        {
            var points = new[]
            {
                MakePoint("Never", "A"),
                MakePoint("Recent", "B", lastUsedDay: 5, lastUsedTick: 300),
                MakePoint("Earlier", "C", lastUsedDay: 3, lastUsedTick: 100)
            };

            var results = WarpPointFiltering.ApplySort(points, WarpSortMode.LastUsed).ToList();

            Assert.Equal("Recent", results[0].Name);
            Assert.Equal("Earlier", results[1].Name);
            Assert.Equal("Never", results[2].Name);
        }

        // Expected: LastUsed tiebreaks on tick within same day
        [Fact]
        public void ApplySort_LastUsed_TiebreaksOnTick()
        {
            var points = new[]
            {
                MakePoint("EarlierTick", "A", lastUsedDay: 5, lastUsedTick: 100),
                MakePoint("LaterTick", "B", lastUsedDay: 5, lastUsedTick: 500)
            };

            var results = WarpPointFiltering.ApplySort(points, WarpSortMode.LastUsed).ToList();

            Assert.Equal("LaterTick", results[0].Name);
            Assert.Equal("EarlierTick", results[1].Name);
        }

        #endregion

        #region Combined Filters

        // Expected: Combined search + category + sort works correctly
        [Fact]
        public void CombinedFilters_WorkCorrectly()
        {
            var points = new[]
            {
                MakePoint("Farm Gate", "Farm", createdDay: 2),
                MakePoint("Farm Barn", "Farm", createdDay: 5),
                MakePoint("Mine Exit", "Mine", createdDay: 3),
                MakePoint("Town Farm Shop", "Town", createdDay: 4)
            };
            var searchText = BuildSearchText(points);
            WarpCategory Classify(string loc) => loc == "Farm" ? WarpCategory.Farm : WarpCategory.World;

            var filtered = WarpPointFiltering.ApplySearchFilter(points, "Farm", WarpSearchScope.Name, searchText);
            filtered = WarpPointFiltering.ApplyCategoryFilter(filtered, WarpCategory.Farm, Classify);
            var results = WarpPointFiltering.ApplySort(filtered, WarpSortMode.Newest).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal("Farm Barn", results[0].Name);
            Assert.Equal("Farm Gate", results[1].Name);
        }

        #endregion
    }
}
