using EasyWarps.Models;
using Xunit;

namespace EasyWarps.Tests.Models
{
    public class WarpFilterStateTests
    {
        // Expected: Different SearchScope values produce different cache keys
        [Fact]
        public void ToCacheKey_DifferentSearchScope_ProducesDifferentKeys()
        {
            var a = new WarpFilterState { SearchScope = WarpSearchScope.All };
            var b = new WarpFilterState { SearchScope = WarpSearchScope.Name };
            var c = new WarpFilterState { SearchScope = WarpSearchScope.Location };

            Assert.NotEqual(a.ToCacheKey(), b.ToCacheKey());
            Assert.NotEqual(b.ToCacheKey(), c.ToCacheKey());
            Assert.NotEqual(a.ToCacheKey(), c.ToCacheKey());
        }

        // Expected: Different SearchText values produce different cache keys
        [Fact]
        public void ToCacheKey_DifferentSearchText_ProducesDifferentKeys()
        {
            var a = new WarpFilterState { SearchText = "farm" };
            var b = new WarpFilterState { SearchText = "mine" };

            Assert.NotEqual(a.ToCacheKey(), b.ToCacheKey());
        }

        // Expected: Different Category values produce different cache keys
        [Fact]
        public void ToCacheKey_DifferentCategory_ProducesDifferentKeys()
        {
            var a = new WarpFilterState { Category = WarpCategory.All };
            var b = new WarpFilterState { Category = WarpCategory.Farm };
            var c = new WarpFilterState { Category = WarpCategory.World };

            Assert.NotEqual(a.ToCacheKey(), b.ToCacheKey());
            Assert.NotEqual(b.ToCacheKey(), c.ToCacheKey());
        }

        // Expected: All five sort modes produce distinct cache keys
        [Fact]
        public void ToCacheKey_AllSortModes_ProduceDistinctKeys()
        {
            var modes = new[] { WarpSortMode.AToZ, WarpSortMode.ZToA, WarpSortMode.Newest, WarpSortMode.Oldest, WarpSortMode.LastUsed };
            var keys = new HashSet<string>();

            foreach (var mode in modes)
            {
                var state = new WarpFilterState { SortMode = mode };
                Assert.True(keys.Add(state.ToCacheKey()), $"Duplicate key for {mode}");
            }
        }

        // Expected: Different FavoritesOnly values produce different cache keys
        [Fact]
        public void ToCacheKey_DifferentFavoritesOnly_ProducesDifferentKeys()
        {
            var a = new WarpFilterState { FavoritesOnly = false };
            var b = new WarpFilterState { FavoritesOnly = true };

            Assert.NotEqual(a.ToCacheKey(), b.ToCacheKey());
        }

        // Expected: Two default states produce the same cache key
        [Fact]
        public void ToCacheKey_EquivalentStates_ProduceSameKey()
        {
            var a = new WarpFilterState();
            var b = new WarpFilterState();

            Assert.Equal(a.ToCacheKey(), b.ToCacheKey());
        }

        // Expected: States with same non-default values produce the same cache key
        [Fact]
        public void ToCacheKey_SameNonDefaultValues_ProduceSameKey()
        {
            var a = new WarpFilterState
            {
                SearchText = "test",
                SearchScope = WarpSearchScope.Name,
                Category = WarpCategory.Farm,
                SortMode = WarpSortMode.LastUsed,
                FavoritesOnly = true
            };
            var b = new WarpFilterState
            {
                SearchText = "test",
                SearchScope = WarpSearchScope.Name,
                Category = WarpCategory.Farm,
                SortMode = WarpSortMode.LastUsed,
                FavoritesOnly = true
            };

            Assert.Equal(a.ToCacheKey(), b.ToCacheKey());
        }

        // Expected: HasActiveFilters returns false for default state
        [Fact]
        public void HasActiveFilters_DefaultState_ReturnsFalse()
        {
            var state = new WarpFilterState();

            Assert.False(state.HasActiveFilters);
        }

        // Expected: HasActiveFilters returns true when SearchText is set
        [Fact]
        public void HasActiveFilters_WithSearchText_ReturnsTrue()
        {
            var state = new WarpFilterState { SearchText = "farm" };

            Assert.True(state.HasActiveFilters);
        }

        // Expected: HasActiveFilters returns true when Category is not All
        [Fact]
        public void HasActiveFilters_WithCategory_ReturnsTrue()
        {
            var state = new WarpFilterState { Category = WarpCategory.Farm };

            Assert.True(state.HasActiveFilters);
        }

        // Expected: HasActiveFilters returns true when FavoritesOnly is true
        [Fact]
        public void HasActiveFilters_WithFavoritesOnly_ReturnsTrue()
        {
            var state = new WarpFilterState { FavoritesOnly = true };

            Assert.True(state.HasActiveFilters);
        }

        // Expected: Reset restores all properties to defaults
        [Fact]
        public void Reset_RestoresDefaults()
        {
            var state = new WarpFilterState
            {
                SearchText = "test",
                SearchScope = WarpSearchScope.Location,
                Category = WarpCategory.World,
                SortMode = WarpSortMode.LastUsed,
                FavoritesOnly = true
            };

            state.Reset();

            Assert.Equal("", state.SearchText);
            Assert.Equal(WarpSearchScope.All, state.SearchScope);
            Assert.Equal(WarpCategory.All, state.Category);
            Assert.Equal(WarpSortMode.AToZ, state.SortMode);
            Assert.False(state.FavoritesOnly);
            Assert.False(state.HasActiveFilters);
        }
    }
}
