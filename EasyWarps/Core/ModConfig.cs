using EasyWarps.Models;

namespace EasyWarps.Core
{
    public class ModConfig
    {
        // User-facing options
        public bool DisableModRequirement { get; set; }
        public bool AlwaysRegisterAsWarpPoint { get; set; } = true;
        public bool EnableWarpAnimation { get; set; } = true;
        public bool DisableSignHoveringText { get; set; } = true;
        public bool RememberSortOption { get; set; } = true;
        public bool RememberFilterOption { get; set; } = true;
        public WarpSearchScope DefaultSearchScope { get; set; } = WarpSearchScope.All;

        // Internal persisted (not shown in GMCM)
        public WarpSortMode LastSortMode { get; set; } = WarpSortMode.AToZ;
        public bool LastFilterFavorite { get; set; }

        // Dev-only: Enable debug/trace logging (manually edit config.json)
        public bool EnableDebugLogging { get; set; }
    }
}
