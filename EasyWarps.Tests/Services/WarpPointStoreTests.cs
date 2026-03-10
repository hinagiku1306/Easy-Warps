using EasyWarps.Models;
using EasyWarps.Services;
using Xunit;

namespace EasyWarps.Tests.Services
{
    public class WarpPointStoreTests
    {
        // Expected: MakeLocationKey produces expected format
        [Fact]
        public void MakeLocationKey_ProducesExpectedFormat()
        {
            var key = WarpPointStore.MakeLocationKey("Farm", 10, 20);

            Assert.Equal("Farm:10,20", key);
        }

        // Expected: Delete with non-existent Id does nothing
        [Fact]
        public void Delete_NonExistentId_NoOp()
        {
            var store = new WarpPointStore();
            store.Delete("nonexistent");

            Assert.Equal(0, store.Count);
        }

        // Expected: RemoveByLocationKey with no match returns false
        [Fact]
        public void RemoveByLocationKey_NoMatch_ReturnsFalse()
        {
            var store = new WarpPointStore();

            var result = store.RemoveByLocationKey("Nowhere:0,0");

            Assert.False(result);
        }

        // Expected: GetByLocationKey returns null for non-existent key
        [Fact]
        public void GetByLocationKey_NonExistent_ReturnsNull()
        {
            var store = new WarpPointStore();

            Assert.Null(store.GetByLocationKey("Farm", 99, 99));
        }

        // Expected: Update with non-existent Id does nothing
        [Fact]
        public void Update_NonExistentId_NoOp()
        {
            var store = new WarpPointStore();
            var point = new WarpPoint
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                LocationName = "Farm",
                TileX = 10,
                TileY = 10
            };

            store.Update(point);

            Assert.Equal(0, store.Count);
        }

        // Expected: GetFilteredPoints on empty store returns empty list
        [Fact]
        public void GetFilteredPoints_EmptyStore_ReturnsEmpty()
        {
            var store = new WarpPointStore();
            var filter = new WarpFilterState { SearchText = "anything", SortMode = WarpSortMode.AToZ };

            var results = store.GetFilteredPoints(filter, _ => WarpCategory.World);

            Assert.Empty(results);
        }
    }
}
