using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using EasyWarps.Models;
using EasyWarps.Utilities;
using static EasyWarps.WarpLayoutConstants;

namespace EasyWarps.UI
{
    public class WarpMenuUIBuilder
    {
        // Outer bounds
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        // Content box (inside background texture box border)
        public Rectangle ContentBox { get; private set; }

        // Tabs
        public ClickableComponent TabAll { get; private set; } = null!;
        public ClickableComponent TabFarm { get; private set; } = null!;
        public ClickableComponent TabWorld { get; private set; } = null!;

        // Search row: scope dropdown + search bar + clear
        public ClickableComponent SearchScopeDropdown { get; private set; } = null!;
        public ClickableComponent SearchBar { get; private set; } = null!;
        public ClickableComponent SearchClearButton { get; private set; } = null!;

        // Sort/filter row: sort dropdown + filter dropdown + clear
        public ClickableComponent SortDropdown { get; private set; } = null!;
        public ClickableComponent FilterDropdown { get; private set; } = null!;
        public ClickableComponent FilterClearButton { get; private set; } = null!;

        // Dropdown options
        public List<ClickableComponent> SearchScopeOptions { get; private set; } = new();
        public List<ClickableComponent> SortOptions { get; private set; } = new();
        public List<ClickableComponent> FilterOptions { get; private set; } = new();

        // List
        public List<ClickableComponent> ListItems { get; private set; } = new();
        public Rectangle ListBox { get; private set; }

        // Per-row sub-regions (parallel arrays to ListItems)
        public List<Rectangle> FavStarBounds { get; private set; } = new();
        public List<Rectangle> NameBounds { get; private set; } = new();
        public List<Rectangle> EditBounds { get; private set; } = new();
        public List<Rectangle> DeleteBounds { get; private set; } = new();

        // Scroll arrows (right side of list)
        public ClickableTextureComponent ScrollUpArrow { get; private set; } = null!;
        public ClickableTextureComponent ScrollDownArrow { get; private set; } = null!;

        // Close button
        public ClickableTextureComponent CloseButton { get; private set; } = null!;

        // Config gear
        public ClickableComponent ConfigGearButton { get; private set; } = null!;

        // Cached widths
        private int searchScopeWidth;
        private int sortDropdownWidth;
        private int filterDropdownWidth;

        // Cached per-row text widths
        public int NameMaxWidth { get; private set; }
        public int LocationMaxWidth { get; private set; }

        public WarpMenuUIBuilder()
        {
            Recalculate();
        }

        public void Recalculate()
        {
            CalculateComponentWidths();

            int contentWidth = MenuWidth - BorderPadding * 2;
            int tabRowHeight = TabAndButtonHeight;
            int searchRowHeight = TabAndButtonHeight;
            int sortFilterRowHeight = TabAndButtonHeight;
            int listHeight = MaxVisibleRows * ListItemHeight;
            int gearRowHeight = ConfigGearButtonSize + ConfigGearButtonGap;

            int contentHeight = tabRowHeight + FilterRowGap
                + searchRowHeight + FilterRowGap
                + sortFilterRowHeight + FilterRowGap
                + listHeight + FilterRowGap
                + gearRowHeight;

            Width = MenuWidth;
            Height = contentHeight + BorderPadding * 2;

            X = (Game1.uiViewport.Width - Width) / 2;
            Y = (Game1.uiViewport.Height - Height) / 2;

            ContentBox = new Rectangle(X + BorderPadding, Y + BorderPadding, contentWidth, contentHeight);

            CalculateLayout();
        }

        private void CalculateComponentWidths()
        {
            int dropdownPadding = 40;
            string[] scopeLabels = { TranslationCache.SearchScopeAll, TranslationCache.SearchScopeName, TranslationCache.SearchScopeLocation };
            int maxScopeWidth = 0;
            foreach (var label in scopeLabels)
            {
                int w = (int)Game1.smallFont.MeasureString(label).X + dropdownPadding;
                maxScopeWidth = Math.Max(maxScopeWidth, w);
            }
            searchScopeWidth = Math.Max(maxScopeWidth, TabAndButtonWidth);

            string[] sortLabels = { TranslationCache.SortAToZ, TranslationCache.SortZToA, TranslationCache.SortNewest, TranslationCache.SortOldest, TranslationCache.SortLastUsed };
            int maxSortWidth = 0;
            foreach (var label in sortLabels)
            {
                int w = (int)Game1.smallFont.MeasureString(label).X + dropdownPadding;
                maxSortWidth = Math.Max(maxSortWidth, w);
            }
            sortDropdownWidth = Math.Max(maxSortWidth, TabAndButtonWidth);

            string[] filterLabels = { TranslationCache.FilterFavorite };
            int maxFilterWidth = 0;
            foreach (var label in filterLabels)
            {
                int w = (int)Game1.smallFont.MeasureString(label).X + dropdownPadding;
                maxFilterWidth = Math.Max(maxFilterWidth, w);
            }
            filterDropdownWidth = Math.Max(maxFilterWidth, TabAndButtonWidth);
        }

        private void CalculateLayout()
        {
            int cx = ContentBox.X;
            int cy = ContentBox.Y;
            int cw = ContentBox.Width;
            int curY = cy;

            // --- Tabs row ---
            int tabW = (cw - TabGap * 2) / 3;
            TabAll = new ClickableComponent(new Rectangle(cx, curY, tabW, TabAndButtonHeight), "tabAll");
            TabFarm = new ClickableComponent(new Rectangle(cx + tabW + TabGap, curY, tabW, TabAndButtonHeight), "tabFarm");
            TabWorld = new ClickableComponent(new Rectangle(cx + tabW * 2 + TabGap * 2, curY, cw - tabW * 2 - TabGap * 2, TabAndButtonHeight), "tabWorld");
            curY += TabAndButtonHeight + FilterRowGap;

            // --- Search row: scope dropdown + search bar + clear ---
            int searchBarX = cx + searchScopeWidth + FilterRowGap;
            int searchBarW = cw - searchScopeWidth - FilterRowGap;
            SearchScopeDropdown = new ClickableComponent(new Rectangle(cx, curY, searchScopeWidth, TabAndButtonHeight), "searchScope");
            SearchBar = new ClickableComponent(new Rectangle(searchBarX, curY, searchBarW, TabAndButtonHeight), "searchBar");

            int clearX = searchBarX + searchBarW - ClearButtonRightMargin - ClearButtonSize;
            int clearY = curY + (TabAndButtonHeight - ClearButtonSize) / 2;
            SearchClearButton = new ClickableComponent(new Rectangle(clearX, clearY, ClearButtonSize, ClearButtonSize), "searchClear");

            BuildSearchScopeOptions();
            curY += TabAndButtonHeight + FilterRowGap;

            // --- Sort/filter row ---
            int filterX = cx + cw - filterDropdownWidth - ClearButtonSize - FilterRowGap;
            int filterClearX = cx + cw - ClearButtonSize;
            int filterClearY = curY + (TabAndButtonHeight - ClearButtonSize) / 2;
            SortDropdown = new ClickableComponent(new Rectangle(cx, curY, sortDropdownWidth, TabAndButtonHeight), "sortDropdown");
            FilterDropdown = new ClickableComponent(new Rectangle(filterX, curY, filterDropdownWidth, TabAndButtonHeight), "filterDropdown");
            FilterClearButton = new ClickableComponent(new Rectangle(filterClearX, filterClearY, ClearButtonSize, ClearButtonSize), "filterClear");

            BuildSortOptions();
            BuildFilterOptions();
            curY += TabAndButtonHeight + FilterRowGap;

            // --- List box ---
            int listW = cw;
            int listH = MaxVisibleRows * ListItemHeight;
            ListBox = new Rectangle(cx, curY, listW, listH);

            BuildListItems(curY, listW);
            CalculateRowSubRegions();

            // Scroll arrows (right side, vertically centered on list)
            int arrowW = (int)(UIHelpers.UpScrollArrowSourceRect.Width * ListScrollArrowScale);
            int arrowH = (int)(UIHelpers.UpScrollArrowSourceRect.Height * ListScrollArrowScale);
            int arrowX = cx + cw - arrowW - ScrollArrowRightPadding;
            int listMidY = curY + listH / 2;

            ScrollUpArrow = new ClickableTextureComponent(
                new Rectangle(arrowX, listMidY - arrowH - 4, ScrollArrowButtonSize, ScrollArrowButtonSize),
                Game1.mouseCursors, UIHelpers.UpScrollArrowSourceRect, ListScrollArrowScale);
            ScrollDownArrow = new ClickableTextureComponent(
                new Rectangle(arrowX, listMidY + 4, ScrollArrowButtonSize, ScrollArrowButtonSize),
                Game1.mouseCursors, UIHelpers.DownScrollArrowSourceRect, ListScrollArrowScale);

            curY += listH + FilterRowGap;

            // --- Config gear (bottom-right) ---
            int gearX = cx + cw - ConfigGearButtonSize;
            ConfigGearButton = new ClickableComponent(new Rectangle(gearX, curY, ConfigGearButtonSize, ConfigGearButtonSize), "configGear");

            // --- Close button (top-right corner of menu) ---
            int closeX = X + Width - CloseButtonSize - CloseButtonEdgeMargin;
            int closeY = Y + CloseButtonEdgeMargin;
            CloseButton = new ClickableTextureComponent(
                new Rectangle(closeX, closeY, CloseButtonSize, CloseButtonSize),
                Game1.mouseCursors, new Rectangle(337, 494, 12, 12), CloseButtonSize / 12f);
        }

        private void BuildSearchScopeOptions()
        {
            SearchScopeOptions.Clear();
            string[] labels = { TranslationCache.SearchScopeAll, TranslationCache.SearchScopeName, TranslationCache.SearchScopeLocation };
            int optY = SearchScopeDropdown.bounds.Bottom;
            for (int i = 0; i < labels.Length; i++)
            {
                SearchScopeOptions.Add(new ClickableComponent(
                    new Rectangle(SearchScopeDropdown.bounds.X, optY + i * DropdownOptionHeight, SearchScopeDropdown.bounds.Width, DropdownOptionHeight),
                    labels[i]));
            }
        }

        private void BuildSortOptions()
        {
            SortOptions.Clear();
            string[] labels = { TranslationCache.SortAToZ, TranslationCache.SortZToA, TranslationCache.SortNewest, TranslationCache.SortOldest, TranslationCache.SortLastUsed };
            int optY = SortDropdown.bounds.Bottom;
            for (int i = 0; i < labels.Length; i++)
            {
                SortOptions.Add(new ClickableComponent(
                    new Rectangle(SortDropdown.bounds.X, optY + i * DropdownOptionHeight, SortDropdown.bounds.Width, DropdownOptionHeight),
                    labels[i]));
            }
        }

        private void BuildFilterOptions()
        {
            FilterOptions.Clear();
            string[] labels = { TranslationCache.FilterFavorite };
            int optY = FilterDropdown.bounds.Bottom;
            for (int i = 0; i < labels.Length; i++)
            {
                FilterOptions.Add(new ClickableComponent(
                    new Rectangle(FilterDropdown.bounds.X, optY + i * DropdownOptionHeight, FilterDropdown.bounds.Width, DropdownOptionHeight),
                    labels[i]));
            }
        }

        private void BuildListItems(int listY, int listW)
        {
            ListItems.Clear();
            for (int i = 0; i < MaxVisibleRows; i++)
            {
                ListItems.Add(new ClickableComponent(
                    new Rectangle(ContentBox.X, listY + i * ListItemHeight, listW, ListItemHeight),
                    $"listItem_{i}"));
            }
        }

        private void CalculateRowSubRegions()
        {
            FavStarBounds.Clear();
            NameBounds.Clear();
            EditBounds.Clear();
            DeleteBounds.Clear();

            foreach (var item in ListItems)
            {
                int rx = item.bounds.X;
                int ry = item.bounds.Y;
                int rw = item.bounds.Width;
                int rh = item.bounds.Height;

                // Fav star: left margin, vertically centered
                int starX = rx + ListItemLeftMargin;
                int starY = ry + (rh - FavoriteStarSize) / 2;
                FavStarBounds.Add(new Rectangle(starX, starY, FavoriteStarSize, FavoriteStarSize));

                // Delete X: right end
                int deleteX = rx + rw - ListItemLeftMargin - RowActionButtonSize;
                int deleteY = ry + (rh - RowActionButtonSize) / 2;
                DeleteBounds.Add(new Rectangle(deleteX, deleteY, RowActionButtonSize, RowActionButtonSize));

                // Edit gear: left of delete
                int editX = deleteX - RowActionButtonGap - RowActionButtonSize;
                int editY = deleteY;
                EditBounds.Add(new Rectangle(editX, editY, RowActionButtonSize, RowActionButtonSize));

                // Name area: between star and edit
                int nameX = starX + FavoriteStarSize + ListIconToTextGap;
                int nameW = editX - RowActionButtonGap - nameX;
                NameBounds.Add(new Rectangle(nameX, ry, nameW, rh));
            }

            // Calculate max text widths for name and location (name gets ~60% of name area, location gets rest)
            if (NameBounds.Count > 0)
            {
                int totalNameAreaWidth = NameBounds[0].Width;
                NameMaxWidth = (int)(totalNameAreaWidth * 0.55f);
                LocationMaxWidth = totalNameAreaWidth - NameMaxWidth - ListIconGap;
            }
        }

        // --- Drawing helpers ---

        public void DrawBackground(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * BackgroundOverlayOpacity);

            UIHelpers.DrawTextureBox(b, X, Y, Width, Height, Color.White);
        }

        public void DrawTabs(SpriteBatch b, WarpCategory activeCategory)
        {
            UIHelpers.DrawTabWithText(b, TabAll, TranslationCache.TabAll, activeCategory == WarpCategory.All);
            UIHelpers.DrawTabWithText(b, TabFarm, TranslationCache.TabFarm, activeCategory == WarpCategory.Farm);
            UIHelpers.DrawTabWithText(b, TabWorld, TranslationCache.TabWorld, activeCategory == WarpCategory.World);
        }

        public void DrawSearchRow(SpriteBatch b, string searchText, bool searchFocused, WarpSearchScope scope, bool scopeDropdownOpen)
        {
            string scopeLabel = scope switch
            {
                WarpSearchScope.All => TranslationCache.SearchScopeAll,
                WarpSearchScope.Name => TranslationCache.SearchScopeName,
                WarpSearchScope.Location => TranslationCache.SearchScopeLocation,
                _ => TranslationCache.SearchScopeAll
            };

            UIHelpers.DrawDropdownButton(b, SearchScopeDropdown.bounds, scopeLabel, scopeDropdownOpen);
            UIHelpers.DrawInputBar(b, SearchBar.bounds, searchText, searchFocused,
                placeholder: TranslationCache.Search, clearButton: SearchClearButton);
        }

        public void DrawSortFilterRow(SpriteBatch b, WarpSortMode sortMode, bool favoritesOnly, bool sortOpen, bool filterOpen)
        {
            string sortLabel = sortMode switch
            {
                WarpSortMode.AToZ => TranslationCache.SortAToZ,
                WarpSortMode.ZToA => TranslationCache.SortZToA,
                WarpSortMode.Newest => TranslationCache.SortNewest,
                WarpSortMode.Oldest => TranslationCache.SortOldest,
                WarpSortMode.LastUsed => TranslationCache.SortLastUsed,
                _ => TranslationCache.SortAToZ
            };

            UIHelpers.DrawDropdownButton(b, SortDropdown.bounds, sortLabel, sortOpen,
                label: TranslationCache.Sort, labelX: SortDropdown.bounds.X, labelY: null);
            UIHelpers.DrawDropdownButton(b, FilterDropdown.bounds, favoritesOnly ? TranslationCache.FilterFavorite : "",
                filterOpen, label: TranslationCache.Filter, labelX: FilterDropdown.bounds.X, labelY: null,
                clearButton: FilterClearButton, hasValue: favoritesOnly, placeholder: "\u2014");
        }

        public void DrawList(SpriteBatch b, List<WarpPoint> points, int scrollOffset, bool anyDropdownOpen, string? hoveredPointId)
        {
            // Draw list box background
            UIHelpers.DrawTextureBoxNoShadow(b, ListBox.X, ListBox.Y, ListBox.Width, ListBox.Height, Color.White * 0.3f);

            for (int i = 0; i < MaxVisibleRows; i++)
            {
                int dataIndex = scrollOffset + i;
                if (dataIndex >= points.Count)
                    break;

                var point = points[dataIndex];
                var rowBounds = ListItems[i].bounds;

                // Hover highlight
                if (!anyDropdownOpen && !UIHelpers.SuppressHover && hoveredPointId == point.Id)
                {
                    b.Draw(Game1.staminaRect, rowBounds, Color.Wheat * 0.6f);
                }

                // Fav star
                var starRect = FavStarBounds[i];
                var starSourceRect = point.IsFavorite
                    ? new Rectangle(346, 392, 8, 8)
                    : new Rectangle(338, 400, 8, 8);
                b.Draw(Game1.mouseCursors, new Vector2(starRect.X, starRect.Y), starSourceRect,
                    Color.White, 0f, Vector2.Zero, FavoriteStarSize / 8f, SpriteEffects.None, 1f);

                // Name + location text
                var nameBounds = NameBounds[i];
                string truncName = UIHelpers.TruncateText(point.Name, NameMaxWidth);
                string truncLoc = UIHelpers.TruncateText(point.LocationName, LocationMaxWidth);

                float textH = Game1.smallFont.MeasureString("A").Y;
                float nameY = nameBounds.Y + (nameBounds.Height - textH) / 2;
                Utility.drawTextWithShadow(b, truncName, Game1.smallFont,
                    new Vector2(nameBounds.X, nameY), Game1.textColor);

                float nameW = Game1.smallFont.MeasureString(truncName).X;
                float locX = nameBounds.X + nameW + ListIconGap;
                Utility.drawTextWithShadow(b, truncLoc, Game1.smallFont,
                    new Vector2(locX, nameY), Game1.textColor * 0.6f);

                // Edit gear icon
                var editRect = EditBounds[i];
                var gearSourceRect = new Rectangle(402, 361, 10, 10);
                float gearScale = RowActionButtonSize / 10f;
                b.Draw(Game1.mouseCursors, new Vector2(editRect.X, editRect.Y), gearSourceRect,
                    Color.White, 0f, Vector2.Zero, gearScale, SpriteEffects.None, 1f);

                // Delete X icon
                var deleteRect = DeleteBounds[i];
                var xSourceRect = new Rectangle(337, 494, 12, 12);
                float xScale = RowActionButtonSize / 12f;
                b.Draw(Game1.mouseCursors, new Vector2(deleteRect.X, deleteRect.Y), xSourceRect,
                    Color.White, 0f, Vector2.Zero, xScale, SpriteEffects.None, 1f);
            }
        }

        public void DrawScrollArrows(SpriteBatch b, int scrollOffset, int totalItems)
        {
            bool canScrollUp = scrollOffset > 0;
            bool canScrollDown = scrollOffset + MaxVisibleRows < totalItems;

            if (canScrollUp)
            {
                ScrollUpArrow.draw(b);
            }
            if (canScrollDown)
            {
                ScrollDownArrow.draw(b);
            }
        }

        public void DrawConfigGear(SpriteBatch b)
        {
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            bool hovered = !UIHelpers.SuppressHover && ConfigGearButton.containsPoint(mouseX, mouseY);

            var gearSourceRect = new Rectangle(402, 361, 10, 10);
            float scale = ConfigGearButtonSize / 10f;
            float drawScale = hovered ? scale * 1.1f : scale;
            int drawX = ConfigGearButton.bounds.X + (ConfigGearButtonSize - (int)(10 * drawScale)) / 2;
            int drawY = ConfigGearButton.bounds.Y + (ConfigGearButtonSize - (int)(10 * drawScale)) / 2;

            b.Draw(Game1.mouseCursors, new Vector2(drawX, drawY), gearSourceRect,
                Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 1f);
        }

        public void DrawEmptyState(SpriteBatch b, string message)
        {
            Vector2 textSize = Game1.smallFont.MeasureString(message);
            Vector2 pos = new Vector2(
                ListBox.X + (ListBox.Width - textSize.X) / 2,
                ListBox.Y + (ListBox.Height - textSize.Y) / 2);
            Utility.drawTextWithShadow(b, message, Game1.smallFont, pos, Game1.textColor * 0.6f);
        }

        public void DrawCloseButton(SpriteBatch b)
        {
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            float scale = CloseButton.containsPoint(mouseX, mouseY) ? CloseButtonSize / 12f * 1.1f : CloseButtonSize / 12f;
            var sourceRect = new Rectangle(337, 494, 12, 12);
            Vector2 center = new Vector2(
                CloseButton.bounds.X + CloseButton.bounds.Width / 2,
                CloseButton.bounds.Y + CloseButton.bounds.Height / 2);
            b.Draw(Game1.mouseCursors, center, sourceRect, Color.White, 0f, new Vector2(6, 6), scale, SpriteEffects.None, 1f);
        }
    }
}
