using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using EasyWarps.Core;
using SObject = StardewValley.Object;

namespace EasyWarps.Patches
{
    [HarmonyPatch(typeof(SObject), nameof(SObject.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
    internal static class SignHoverPatch
    {
        private static void Prefix(SObject __instance)
        {
            if (!__instance.hovering || !__instance.IsTextSign())
                return;

            if (!ModEntry.IsFeatureUnlocked())
                return;

            if (!ModEntry.Config.DisableSignHoveringText)
                return;

            var store = ModEntry.Store;
            if (store == null)
                return;

            var locationName = __instance.Location?.NameOrUniqueName ?? "";
            var tileX = (int)__instance.TileLocation.X;
            var tileY = (int)__instance.TileLocation.Y;

            if (store.GetByLocationKey(locationName, tileX, tileY) != null)
                __instance.hovering = false;
        }
    }
}
