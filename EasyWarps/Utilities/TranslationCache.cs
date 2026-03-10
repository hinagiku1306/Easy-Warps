using StardewModdingAPI;

namespace EasyWarps.Utilities
{
    public static class TranslationCache
    {
        // Menu
        public static string MenuTitle { get; private set; } = "";
        public static string TabAll { get; private set; } = "";
        public static string TabFarm { get; private set; } = "";
        public static string TabWorld { get; private set; } = "";
        public static string Search { get; private set; } = "";
        public static string SearchScopeAll { get; private set; } = "";
        public static string SearchScopeName { get; private set; } = "";
        public static string SearchScopeLocation { get; private set; } = "";
        public static string SortAToZ { get; private set; } = "";
        public static string SortZToA { get; private set; } = "";
        public static string SortNewest { get; private set; } = "";
        public static string SortOldest { get; private set; } = "";
        public static string SortLastUsed { get; private set; } = "";
        public static string Sort { get; private set; } = "";
        public static string FilterFavorite { get; private set; } = "";
        public static string EmptyNoWarpPoints { get; private set; } = "";
        public static string EmptyNoMatches { get; private set; } = "";
        public static string WarpDestinationMissing { get; private set; } = "";
        public static string WarpNoSpace { get; private set; } = "";

        // Sign edit
        public static string SignEditTitle { get; private set; } = "";
        public static string SignEditTitleNew { get; private set; } = "";
        public static string SignEditCheckbox { get; private set; } = "";

        // Warp menu actions
        public static string DeleteConfirmQuestion { get; private set; } = "";
        public static string CommonYes { get; private set; } = "";
        public static string CommonNo { get; private set; } = "";

        // Config
        public static string ConfigAlwaysRegisterName { get; private set; } = "";
        public static string ConfigAlwaysRegisterTooltip { get; private set; } = "";
        public static string ConfigEnableAnimationName { get; private set; } = "";
        public static string ConfigEnableAnimationTooltip { get; private set; } = "";
        public static string ConfigDisableHoverTextName { get; private set; } = "";
        public static string ConfigDisableHoverTextTooltip { get; private set; } = "";
        public static string ConfigRememberSortName { get; private set; } = "";
        public static string ConfigRememberSortTooltip { get; private set; } = "";
        public static string ConfigRememberFavoriteName { get; private set; } = "";
        public static string ConfigRememberFavoriteTooltip { get; private set; } = "";
        public static string ConfigDefaultSearchScopeName { get; private set; } = "";
        public static string ConfigDefaultSearchScopeTooltip { get; private set; } = "";

        public static string ConfigDisableModRequirementName { get; private set; } = "";
        public static string ConfigDisableModRequirementTooltip { get; private set; } = "";

        // Mail
        public static string MailWizardLetter { get; private set; } = "";

        // Config overlay
        public static string ConfigTitle { get; private set; } = "";
        public static string ConfigSave { get; private set; } = "";
        public static string ConfigClose { get; private set; } = "";

        public static void Initialize(ITranslationHelper translation)
        {
            // Menu
            MenuTitle = translation.Get("menu.title");
            TabAll = translation.Get("menu.tab.all");
            TabFarm = translation.Get("menu.tab.farm");
            TabWorld = translation.Get("menu.tab.world");
            Search = translation.Get("menu.search");
            SearchScopeAll = translation.Get("menu.searchScope.all");
            SearchScopeName = translation.Get("menu.searchScope.name");
            SearchScopeLocation = translation.Get("menu.searchScope.location");
            SortAToZ = translation.Get("menu.sort.aToZ");
            SortZToA = translation.Get("menu.sort.zToA");
            SortNewest = translation.Get("menu.sort.newest");
            SortOldest = translation.Get("menu.sort.oldest");
            SortLastUsed = translation.Get("menu.sort.lastUsed");
            Sort = translation.Get("menu.sort");
            FilterFavorite = translation.Get("menu.filter.favorite");
            EmptyNoWarpPoints = translation.Get("menu.empty.noWarpPoints");
            EmptyNoMatches = translation.Get("menu.empty.noMatches");
            WarpDestinationMissing = translation.Get("menu.warpDestinationMissing");
            WarpNoSpace = translation.Get("menu.warpNoSpace");

            // Sign edit
            SignEditTitle = translation.Get("signEdit.title");
            SignEditTitleNew = translation.Get("signEdit.titleNew");
            SignEditCheckbox = translation.Get("signEdit.checkbox");

            // Warp menu actions
            DeleteConfirmQuestion = translation.Get("menu.deleteConfirm");
            CommonYes = translation.Get("common.yes");
            CommonNo = translation.Get("common.no");

            // Config
            ConfigAlwaysRegisterName = translation.Get("config.alwaysRegister.name");
            ConfigAlwaysRegisterTooltip = translation.Get("config.alwaysRegister.tooltip");
            ConfigEnableAnimationName = translation.Get("config.enableAnimation.name");
            ConfigEnableAnimationTooltip = translation.Get("config.enableAnimation.tooltip");
            ConfigDisableHoverTextName = translation.Get("config.disableHoverText.name");
            ConfigDisableHoverTextTooltip = translation.Get("config.disableHoverText.tooltip");
            ConfigRememberSortName = translation.Get("config.rememberSort.name");
            ConfigRememberSortTooltip = translation.Get("config.rememberSort.tooltip");
            ConfigRememberFavoriteName = translation.Get("config.rememberFavorite.name");
            ConfigRememberFavoriteTooltip = translation.Get("config.rememberFavorite.tooltip");
            ConfigDefaultSearchScopeName = translation.Get("config.defaultSearchScope.name");
            ConfigDefaultSearchScopeTooltip = translation.Get("config.defaultSearchScope.tooltip");

            ConfigDisableModRequirementName = translation.Get("config.disableModRequirement.name");
            ConfigDisableModRequirementTooltip = translation.Get("config.disableModRequirement.tooltip");

            // Mail
            MailWizardLetter = translation.Get("mail.wizardLetter");

            // Config overlay
            ConfigTitle = translation.Get("config.title");
            ConfigSave = translation.Get("config.save");
            ConfigClose = translation.Get("config.close");
        }
    }
}
