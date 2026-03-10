using System;
using Microsoft.Xna.Framework;
using StardewValley;
using EasyWarps.Core;
using EasyWarps.Models;
using EasyWarps.Utilities;

namespace EasyWarps.Services
{
    public class WarpService
    {
        private const int MaxRingRadius = 5;

        public void ExecuteWarp(WarpPoint destination, Vector2 currentSignTile, Farmer player, WarpPointStore store)
        {
            var targetLocation = Game1.getLocationFromName(destination.LocationName);
            if (targetLocation == null)
            {
                Game1.addHUDMessage(new HUDMessage(TranslationCache.WarpDestinationMissing, HUDMessage.error_type));
                return;
            }

            var relativeOffset = new Vector2(player.TilePoint.X, player.TilePoint.Y) - currentSignTile;
            var destTile = new Vector2(destination.TileX, destination.TileY);

            const CollisionMask warpMask = CollisionMask.All & ~CollisionMask.Characters & ~CollisionMask.Farmers;
            const CollisionMask warpIgnorePassables = CollisionMask.Objects | CollisionMask.Flooring | CollisionMask.TerrainFeatures;
            var warpTarget = FindWarpTarget(
                destination.TileX, destination.TileY, relativeOffset,
                tile => targetLocation.isTileOnMap(tile) &&
                    !targetLocation.IsTileBlockedBy(tile, warpMask, warpIgnorePassables));

            if (warpTarget == null)
            {
                Game1.addHUDMessage(new HUDMessage(TranslationCache.WarpNoSpace, HUDMessage.error_type));
                return;
            }

            destination.LastUsedDay = Game1.stats.DaysPlayed;
            destination.LastUsedTick = Game1.ticks;
            store.Update(destination);

            var target = warpTarget.Value;

            if (ModEntry.Config.EnableWarpAnimation)
                PlayWarpAnimation(player, destination.LocationName, (int)target.X, (int)target.Y);
            else
                PlaySimpleFade(player, destination.LocationName, (int)target.X, (int)target.Y);
        }

        internal static Vector2? FindWarpTarget(int destTileX, int destTileY, Vector2 relativeOffset, Func<Vector2, bool> isPassable)
        {
            var candidate = new Vector2(destTileX + relativeOffset.X, destTileY + relativeOffset.Y);
            if (isPassable(candidate))
                return candidate;

            var center = new Vector2(destTileX, destTileY);

            Vector2 below = center + new Vector2(0, 1);
            if (isPassable(below)) return below;
            Vector2 left = center + new Vector2(-1, 0);
            if (isPassable(left)) return left;
            Vector2 right = center + new Vector2(1, 0);
            if (isPassable(right)) return right;
            Vector2 up = center + new Vector2(0, -1);
            if (isPassable(up)) return up;

            for (int ring = 1; ring <= MaxRingRadius; ring++)
            {
                for (int x = -ring; x <= ring; x++)
                {
                    for (int y = -ring; y <= ring; y++)
                    {
                        if (Math.Abs(x) != ring && Math.Abs(y) != ring) continue;
                        // Ring 1 cardinals already checked above
                        if (ring == 1 && (x == 0 || y == 0)) continue;

                        var tile = center + new Vector2(x, y);
                        if (isPassable(tile))
                            return tile;
                    }
                }
            }

            return null;
        }

        private static void PlayWarpAnimation(Farmer player, string locationName, int tileX, int tileY)
        {
            var location = player.currentLocation;

            for (int i = 0; i < 12; i++)
            {
                location.temporarySprites.Add(
                    new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1,
                        new Vector2(
                            Game1.random.Next((int)player.Position.X - 256, (int)player.Position.X + 192),
                            Game1.random.Next((int)player.Position.Y - 256, (int)player.Position.Y + 192)),
                        flicker: false, Game1.random.Next(2) == 0));
            }

            player.playNearbySoundAll("wand");
            Game1.displayFarmer = false;
            Game1.player.temporarilyInvincible = true;
            Game1.player.temporaryInvincibilityTimer = -2000;
            Game1.player.freezePause = 1000;
            Game1.flashAlpha = 1f;

            DelayedAction.fadeAfterDelay(() =>
            {
                Game1.warpFarmer(locationName, tileX, tileY, flip: false);
                Game1.fadeToBlackAlpha = 0.99f;
                Game1.screenGlow = false;
                Game1.player.temporarilyInvincible = false;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.displayFarmer = true;
            }, 1000);

            int j = 0;
            Point playerTile = player.TilePoint;
            for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
            {
                location.temporarySprites.Add(
                    new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
                    {
                        layerDepth = 1f,
                        delayBeforeAnimationStart = j * 25,
                        motion = new Vector2(-0.25f, 0f)
                    });
                j++;
            }
        }

        private static void PlaySimpleFade(Farmer player, string locationName, int tileX, int tileY)
        {
            Game1.warpFarmer(locationName, tileX, tileY, flip: false);
        }
    }
}
