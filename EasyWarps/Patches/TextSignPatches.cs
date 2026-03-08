using HarmonyLib;
using StardewValley;
using EasyWarps.Core;
using EasyWarps.UI;
using EasyWarps.Utilities;
using SObject = StardewValley.Object;

namespace EasyWarps.Patches
{
    [HarmonyPatch(typeof(SObject), "CheckForActionOnTextSign")]
    internal static class TextSignPatches
    {
        private static bool Prefix(SObject __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            if (!ModEntry.IsFeatureUnlocked())
                return true;

            if (justCheckingForActivity)
            {
                __result = true;
                return false;
            }

            if (Game1.activeClickableMenu != null)
            {
                __result = false;
                return false;
            }

            var store = ModEntry.Store;
            if (store == null)
            {
                return true;
            }

            var locationName = __instance.Location?.NameOrUniqueName ?? "";
            var tileX = (int)__instance.TileLocation.X;
            var tileY = (int)__instance.TileLocation.Y;
            var existingWarp = store.GetByLocationKey(locationName, tileX, tileY);

            if (existingWarp != null)
            {
                DebugLogger.Trace($"Opening WarpMenu for warp point '{existingWarp.Name}' at {locationName}:{tileX},{tileY}");
                Game1.activeClickableMenu = new WarpMenu(store, __instance, __instance.TileLocation);
            }
            else
            {
                Game1.activeClickableMenu = new SignEditMenu(__instance, store, null);
            }

            __result = true;
            return false;
        }
    }
}
