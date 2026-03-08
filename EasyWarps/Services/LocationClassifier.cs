using StardewValley;
using EasyWarps.Models;

namespace EasyWarps.Services
{
    public static class LocationClassifier
    {
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
    }
}
