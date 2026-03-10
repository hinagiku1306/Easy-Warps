using System.Collections.Generic;
using StardewValley;
using EasyWarps.Models;

namespace EasyWarps.Services
{
    public static class LocationClassifier
    {
        private static readonly Dictionary<string, string> displayNameCache = new();

        public static WarpCategory Classify(string locationName)
        {
            var location = Game1.getLocationFromName(locationName);
            if (location == null)
                return WarpCategory.World;

            if (location.IsFarm || location.IsGreenhouse)
                return WarpCategory.Farm;

            if (location.GetParentLocation()?.IsFarm == true)
                return WarpCategory.Farm;

            return WarpCategory.World;
        }

        public static string GetDisplayName(string internalName)
        {
            if (string.IsNullOrEmpty(internalName))
                return internalName;

            if (displayNameCache.TryGetValue(internalName, out var cached))
                return cached;

            var location = Game1.getLocationFromName(internalName);
            var displayName = location?.DisplayName ?? internalName;
            displayNameCache[internalName] = displayName;
            return displayName;
        }

        public static void ClearDisplayNameCache()
        {
            displayNameCache.Clear();
        }
    }
}
