using System;
using Microsoft.Xna.Framework;
using EasyWarps.Services;
using Xunit;

namespace EasyWarps.Tests.Services
{
    public class WarpServiceTests
    {
        // Expected: When relative offset tile is passable, returns that tile directly
        [Fact]
        public void FindWarpTarget_RelativeOffsetPassable_ReturnsOffsetTile()
        {
            var offset = new Vector2(1, -1);
            Func<Vector2, bool> isPassable = _ => true;

            var result = WarpService.FindWarpTarget(10, 10, offset, isPassable);

            Assert.NotNull(result);
            Assert.Equal(new Vector2(11, 9), result!.Value);
        }

        // Expected: When relative offset is blocked but ring 1 has passable tile, returns below (cardinal priority)
        [Fact]
        public void FindWarpTarget_OffsetBlocked_ReturnsNearestInRing1()
        {
            var offset = new Vector2(0, 0);
            var blockedTiles = new HashSet<Vector2> { new Vector2(10, 10) };
            Func<Vector2, bool> isPassable = tile => !blockedTiles.Contains(tile);

            var result = WarpService.FindWarpTarget(10, 10, offset, isPassable);

            Assert.NotNull(result);
            // Cardinal priority: below > left > right > up
            Assert.Equal(new Vector2(10, 11), result!.Value);
        }

        // Expected: When ring 1 is all blocked, expands to ring 2 and returns first passable
        [Fact]
        public void FindWarpTarget_Ring1AllBlocked_ExpandsToRing2()
        {
            var offset = new Vector2(0, 0);
            var center = new Vector2(10, 10);

            // Block center + all ring 1 tiles
            var blockedTiles = new HashSet<Vector2> { center };
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    blockedTiles.Add(center + new Vector2(x, y));

            Func<Vector2, bool> isPassable = tile => !blockedTiles.Contains(tile);

            var result = WarpService.FindWarpTarget(10, 10, offset, isPassable);

            Assert.NotNull(result);
            // First tile in ring 2: top row starts at (-2, -2)
            Assert.Equal(new Vector2(8, 8), result!.Value);
        }

        // Expected: When all tiles within 5 rings are blocked, returns null
        [Fact]
        public void FindWarpTarget_AllBlockedWithin5Rings_ReturnsNull()
        {
            var offset = new Vector2(0, 0);
            Func<Vector2, bool> isPassable = _ => false;

            var result = WarpService.FindWarpTarget(10, 10, offset, isPassable);

            Assert.Null(result);
        }

        // Expected: Ring search checks tiles in expanding order (ring 1 before ring 2)
        [Fact]
        public void FindWarpTarget_RingSearchVisitsExpandingOrder()
        {
            var offset = new Vector2(0, 0);
            var center = new Vector2(10, 10);
            var visitOrder = new List<Vector2>();

            // Block everything except one tile in ring 2
            var passableTile = new Vector2(12, 10); // ring 2, right side
            Func<Vector2, bool> isPassable = tile =>
            {
                visitOrder.Add(tile);
                return tile == passableTile;
            };

            var result = WarpService.FindWarpTarget(10, 10, offset, isPassable);

            Assert.NotNull(result);
            Assert.Equal(passableTile, result!.Value);

            // Verify ring 1 tiles were all checked before ring 2 tiles
            var ring1Tiles = new HashSet<Vector2>();
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    if (x != 0 || y != 0)
                        ring1Tiles.Add(center + new Vector2(x, y));

            int lastRing1Index = -1;
            int firstRing2Index = int.MaxValue;
            for (int i = 0; i < visitOrder.Count; i++)
            {
                // Skip the first entry (the relative offset check)
                if (i == 0) continue;

                if (ring1Tiles.Contains(visitOrder[i]))
                    lastRing1Index = i;
                else if (i > 0 && !ring1Tiles.Contains(visitOrder[i]) && visitOrder[i] != center)
                    firstRing2Index = Math.Min(firstRing2Index, i);
            }

            Assert.True(lastRing1Index < firstRing2Index, "Ring 1 tiles should be checked before ring 2 tiles");
        }

        // Expected: Ring search does not go beyond ring 5 (max cap)
        [Fact]
        public void FindWarpTarget_RingSearchStopsAtCap()
        {
            var offset = new Vector2(0, 0);
            var visitedTiles = new HashSet<Vector2>();

            Func<Vector2, bool> isPassable = tile =>
            {
                visitedTiles.Add(tile);
                return false;
            };

            WarpService.FindWarpTarget(10, 10, offset, isPassable);

            // No tile should be beyond 5 rings from center (10,10)
            foreach (var tile in visitedTiles)
            {
                int dx = Math.Abs((int)tile.X - 10);
                int dy = Math.Abs((int)tile.Y - 10);
                int ringDistance = Math.Max(dx, dy);
                Assert.True(ringDistance <= 5, $"Tile {tile} is at ring {ringDistance}, exceeds max cap of 5");
            }
        }

        // Expected: Non-zero relative offset that is blocked still falls back to ring search around destination
        [Fact]
        public void FindWarpTarget_NonZeroOffsetBlocked_FallsBackToRingSearch()
        {
            var offset = new Vector2(3, 2);
            var center = new Vector2(10, 10);
            var blockedTile = center + offset; // (13, 12) is blocked
            Func<Vector2, bool> isPassable = tile => tile != blockedTile;

            var result = WarpService.FindWarpTarget(10, 10, offset, isPassable);

            Assert.NotNull(result);
            // Should fall back to ring search around (10,10), cardinal priority: below first
            Assert.Equal(new Vector2(10, 11), result!.Value);
        }

        // Expected: Cardinal directions checked in order: below > left > right > up before diagonals
        [Fact]
        public void FindWarpTarget_CardinalPriority_BelowLeftRightUp()
        {
            var offset = new Vector2(0, 0);
            var center = new Vector2(10, 10);

            // Block center and below — should pick left
            var blocked1 = new HashSet<Vector2> { center, center + new Vector2(0, 1) };
            var result1 = WarpService.FindWarpTarget(10, 10, offset, tile => !blocked1.Contains(tile));
            Assert.Equal(new Vector2(9, 10), result1!.Value);

            // Block center, below, left — should pick right
            var blocked2 = new HashSet<Vector2> { center, center + new Vector2(0, 1), center + new Vector2(-1, 0) };
            var result2 = WarpService.FindWarpTarget(10, 10, offset, tile => !blocked2.Contains(tile));
            Assert.Equal(new Vector2(11, 10), result2!.Value);

            // Block center, below, left, right — should pick up
            var blocked3 = new HashSet<Vector2> { center, center + new Vector2(0, 1), center + new Vector2(-1, 0), center + new Vector2(1, 0) };
            var result3 = WarpService.FindWarpTarget(10, 10, offset, tile => !blocked3.Contains(tile));
            Assert.Equal(new Vector2(10, 9), result3!.Value);

            // Block all cardinals — should pick a diagonal in ring 1
            var blocked4 = new HashSet<Vector2> { center, center + new Vector2(0, 1), center + new Vector2(-1, 0), center + new Vector2(1, 0), center + new Vector2(0, -1) };
            var result4 = WarpService.FindWarpTarget(10, 10, offset, tile => !blocked4.Contains(tile));
            Assert.NotNull(result4);
            int dx = Math.Abs((int)result4!.Value.X - 10);
            int dy = Math.Abs((int)result4.Value.Y - 10);
            Assert.Equal(1, Math.Max(dx, dy));
            Assert.True(dx == 1 && dy == 1, "Should be a diagonal tile");
        }

        // Expected: Exactly zero offset that is passable returns the destination tile itself
        [Fact]
        public void FindWarpTarget_ZeroOffset_Passable_ReturnsDestTile()
        {
            var offset = Vector2.Zero;
            Func<Vector2, bool> isPassable = _ => true;

            var result = WarpService.FindWarpTarget(5, 8, offset, isPassable);

            Assert.NotNull(result);
            Assert.Equal(new Vector2(5, 8), result!.Value);
        }
    }
}
