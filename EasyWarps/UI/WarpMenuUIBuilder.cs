using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using EasyWarps.Models;
using EasyWarps.Services;
using EasyWarps.Utilities;
using static EasyWarps.WarpLayoutConstants;

namespace EasyWarps.UI
{
    public class WarpMenuUIBuilder
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        // Content box (inside background texture box border)
        public Rectangle ContentBox { get; private set; }
        public Rectangle TitleBoxBounds { get; private set; }

        public ClickableComponent TabAll { get; private set; } = null!;
        public ClickableComponent TabFarm { get; private set; } = null!;
        public ClickableComponent TabWorld { get; private set; } = null!;

        // Search row: scope dropdown + search bar + clear
        public ClickableComponent SearchScopeDropdown { get; private set; } = null!;
        public ClickableComponent SearchBar { get; private set; } = null!;
        public ClickableComponent SearchClearButton { get; private set; } = null!;

        // Sort/filter row: sort dropdown + favorite checkbox
        public ClickableComponent SortDropdown { get; private set; } = null!;
        public ClickableComponent FavoritesCheckbox { get; private set; } = null!;

        public List<ClickableComponent> SearchScopeOptions { get; private set; } = new();
        public List<ClickableComponent> SortOptions { get; private set; } = new();

        public List<ClickableComponent> ListItems { get; private set; } = new();
        public Rectangle ListBox { get; private set; }
        public int DividerY { get; private set; }

        // Per-row sub-regions (parallel arrays to ListItems)
        public List<Rectangle> FavStarBounds { get; private set; } = new();
        public List<Rectangle> NameBounds { get; private set; } = new();
        public List<Rectangle> EditBounds { get; private set; } = new();
        public List<Rectangle> DeleteBounds { get; private set; } = new();

        // Scroll arrows (right side of list)
        public ClickableTextureComponent ScrollUpArrow { get; private set; } = null!;
        public ClickableTextureComponent ScrollDownArrow { get; private set; } = null!;

        public ClickableTextureComponent CloseButton { get; private set; } = null!;

        // Config gear (floating, right of menu)
        public ClickableTextureComponent ConfigGearButton { get; private set; } = null!;

        private int searchScopeWidth;
        private int sortDropdownWidth;
        private int favLabelWidth;

        // Cached per-row text widths
        public int NameMaxWidth { get; private set; }
        public int LocationMaxWidth { get; private set; }
        public int SeparatorWidth { get; private set; }

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

            int contentHeight = tabRowHeight + TabToFilterGap
                + searchRowHeight + FilterRowGap
                + sortFilterRowHeight + FilterToDividerGap
                + DividerHeight + DividerToListGap
                + listHeight;

            Width = MenuWidth;
            Height = contentHeight + BorderTopPadding + BorderPadding;

            Vector2 titleSize = Game1.dialogueFont.MeasureString(TranslationCache.MenuTitle);
            int titleBoxWidth = (int)titleSize.X + ConfigTitleSidePadding * 2;
            int titleBoxHeight = ConfigTitleTopPadding + (int)titleSize.Y + ConfigTitleBottomPadding;

            int totalHeight = titleBoxHeight + Height;
            X = (Game1.uiViewport.Width - Width) / 2;
            Y = (Game1.uiViewport.Height - totalHeight) / 2 + titleBoxHeight;

            int titleBoxX = X + (Width - titleBoxWidth) / 2;
            TitleBoxBounds = new Rectangle(titleBoxX, Y - titleBoxHeight, titleBoxWidth, titleBoxHeight);

            ContentBox = new Rectangle(X + BorderPadding, Y + BorderTopPadding, contentWidth, contentHeight);

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

            favLabelWidth = (int)Game1.smallFont.MeasureString(TranslationCache.FilterFavorite).X;
        }

        private void CalculateLayout()
        {
            int cx = ContentBox.X;
            int cy = ContentBox.Y;
            int cw = ContentBox.Width;
            int curY = cy;

            int totalTabsW = TabAndButtonWidth * 3 + TabGap * 2;
            int tabStartX = cx + (cw - totalTabsW) / 2;
            TabAll = new ClickableComponent(new Rectangle(tabStartX, curY, TabAndButtonWidth, TabAndButtonHeight), "tabAll");
            TabFarm = new ClickableComponent(new Rectangle(tabStartX + TabAndButtonWidth + TabGap, curY, TabAndButtonWidth, TabAndButtonHeight), "tabFarm");
            TabWorld = new ClickableComponent(new Rectangle(tabStartX + TabAndButtonWidth * 2 + TabGap * 2, curY, TabAndButtonWidth, TabAndButtonHeight), "tabWorld");
            curY += TabAndButtonHeight + TabToFilterGap;

            int searchBarX = cx + searchScopeWidth + FilterRowGap;
            int searchBarW = cw - searchScopeWidth - FilterRowGap;
            SearchScopeDropdown = new ClickableComponent(new Rectangle(cx, curY, searchScopeWidth, TabAndButtonHeight), "searchScope");
            SearchBar = new ClickableComponent(new Rectangle(searchBarX, curY, searchBarW, TabAndButtonHeight), "searchBar");

            int clearX = searchBarX + searchBarW - ClearButtonRightMargin - ClearButtonSize;
            int clearY = curY + (TabAndButtonHeight - ClearButtonSize) / 2;
            SearchClearButton = new ClickableComponent(new Rectangle(clearX, clearY, ClearButtonSize, ClearButtonSize), "searchClear");

            BuildSearchScopeOptions();
            curY += TabAndButtonHeight + FilterRowGap;

            SortDropdown = new ClickableComponent(new Rectangle(cx, curY, sortDropdownWidth, TabAndButtonHeight), "sortDropdown");

            int checkboxX = cx + sortDropdownWidth + 25;
            int checkboxY = curY + (TabAndButtonHeight - CheckboxSize) / 2;
            int favTotalWidth = CheckboxSize + 8 + favLabelWidth;
            FavoritesCheckbox = new ClickableComponent(new Rectangle(checkboxX, checkboxY, favTotalWidth, CheckboxSize), "favoritesCheckbox");

            BuildSortOptions();
            curY += TabAndButtonHeight + FilterToDividerGap;

            DividerY = curY;
            curY += DividerHeight + DividerToListGap;

            int listW = cw;
            int listH = MaxVisibleRows * ListItemHeight;
            ListBox = new Rectangle(cx, curY, listW, listH);

            BuildListItems(curY, listW);
            CalculateRowSubRegions();

            // Scroll arrows (in right border padding, outside content box)
            int arrowCenterX = ContentBox.Right + BorderPadding / 2;

            ScrollUpArrow = new ClickableTextureComponent(
                new Rectangle(arrowCenterX - ScrollArrowButtonSize / 2, curY + 4, ScrollArrowButtonSize, ScrollArrowButtonSize),
                Game1.mouseCursors, UIHelpers.UpScrollArrowSourceRect, ListScrollArrowScale);
            ScrollDownArrow = new ClickableTextureComponent(
                new Rectangle(arrowCenterX - ScrollArrowButtonSize / 2, curY + listH - ScrollArrowButtonSize + 4, ScrollArrowButtonSize, ScrollArrowButtonSize),
                Game1.mouseCursors, UIHelpers.DownScrollArrowSourceRect, ListScrollArrowScale);

            // --- Config gear (floating, right of menu) ---
            int gearBtnX = X + Width + ConfigGearFloatingGap;
            int gearBtnY = Y + (Height - ConfigGearButtonSize) / 2 - 130;
            ConfigGearButton = new ClickableTextureComponent(
                new Rectangle(gearBtnX, gearBtnY, ConfigGearButtonSize, ConfigGearButtonSize),
                Game1.mouseCursors,
                new Rectangle(30, 428, 10, 10),
                ConfigGearButtonSize / 10f);

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
            int optY = SearchScopeDropdown.bounds.Bottom + DropdownPanelPaddingV;
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
            int optY = SortDropdown.bounds.Bottom + DropdownPanelPaddingV;
            for (int i = 0; i < labels.Length; i++)
            {
                SortOptions.Add(new ClickableComponent(
                    new Rectangle(SortDropdown.bounds.X, optY + i * DropdownOptionHeight, SortDropdown.bounds.Width, DropdownOptionHeight),
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

                int starX = rx + ListItemLeftMargin;
                int starY = ry + (rh - FavoriteStarSize) / 2;
                FavStarBounds.Add(new Rectangle(starX, starY, FavoriteStarSize, FavoriteStarSize));

                int deleteX = rx + rw - ListItemLeftMargin - RowDeleteButtonSize;
                int deleteY = ry + (rh - RowDeleteButtonSize) / 2;
                DeleteBounds.Add(new Rectangle(deleteX, deleteY, RowDeleteButtonSize, RowDeleteButtonSize));

                int editX = deleteX - RowActionButtonGap - RowEditButtonSize;
                int editY = ry + (rh - RowEditButtonSize) / 2;
                EditBounds.Add(new Rectangle(editX, editY, RowEditButtonSize, RowEditButtonSize));

                int nameX = starX + FavoriteStarSize + ListIconToTextGap;
                int nameW = editX - RowTextToButtonGap - nameX;
                NameBounds.Add(new Rectangle(nameX, ry, nameW, rh));
            }

            SeparatorWidth = (int)Game1.smallFont.MeasureString(" | ").X;
            if (NameBounds.Count > 0)
            {
                int textAreaWidth = NameBounds[0].Width - SeparatorWidth;
                NameMaxWidth = textAreaWidth / 2;
                LocationMaxWidth = textAreaWidth - NameMaxWidth;
            }
        }

        // --- Drawing helpers ---

        public void DrawBackground(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * BackgroundOverlayOpacity);

            UIHelpers.DrawTextureBox(b, X, Y, Width, Height, Color.White);

            UIHelpers.DrawTextureBox(b, TitleBoxBounds.X, TitleBoxBounds.Y,
                TitleBoxBounds.Width, TitleBoxBounds.Height, Color.White);

            Vector2 titleSize = Game1.dialogueFont.MeasureString(TranslationCache.MenuTitle);
            Utility.drawTextWithShadow(b, TranslationCache.MenuTitle, Game1.dialogueFont,
                new Vector2(TitleBoxBounds.X + (TitleBoxBounds.Width - titleSize.X) / 2,
                    TitleBoxBounds.Y + ConfigTitleTopPadding),
                Game1.textColor);
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

        public void DrawSortFilterRow(SpriteBatch b, WarpSortMode sortMode, bool favoritesOnly, bool sortOpen)
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

            var cb = FavoritesCheckbox.bounds;
            Rectangle sourceRect = favoritesOnly ? UIHelpers.CheckedSourceRect : UIHelpers.UncheckedSourceRect;
            b.Draw(Game1.mouseCursors, new Vector2(cb.X, cb.Y), sourceRect,
                Color.White, 0f, Vector2.Zero, CheckboxScale, SpriteEffects.None, 1f);

            float textHeight = Game1.smallFont.MeasureString(TranslationCache.FilterFavorite).Y;
            int labelX = cb.X + CheckboxSize + 8;
            int labelY = cb.Y + (CheckboxSize - (int)textHeight) / 2;
            Utility.drawTextWithShadow(b, TranslationCache.FilterFavorite, Game1.smallFont,
                new Vector2(labelX, labelY), Game1.textColor);
        }

        public void DrawDivider(SpriteBatch b)
        {
            b.Draw(Game1.staminaRect,
                new Rectangle(ContentBox.X, DividerY, ContentBox.Width, DividerHeight),
                Game1.textColor * 0.3f);
        }

        public void DrawList(SpriteBatch b, List<WarpPoint> points, int scrollOffset, bool anyDropdownOpen, string? hoveredPointId)
        {
            for (int i = 0; i < MaxVisibleRows; i++)
            {
                int dataIndex = scrollOffset + i;
                if (dataIndex >= points.Count)
                    break;

                var point = points[dataIndex];
                var rowBounds = ListItems[i].bounds;

                if (!anyDropdownOpen && !UIHelpers.SuppressHover && hoveredPointId == point.Id)
                {
                    b.Draw(Game1.staminaRect, rowBounds, Color.Wheat * 0.6f);
                }

                var starRect = FavStarBounds[i];
                var starSourceRect = point.IsFavorite
                    ? new Rectangle(346, 392, 8, 8)
                    : new Rectangle(338, 400, 8, 8);
                b.Draw(Game1.mouseCursors, new Vector2(starRect.X, starRect.Y), starSourceRect,
                    Color.White, 0f, Vector2.Zero, FavoriteStarSize / 8f, SpriteEffects.None, 1f);

                var nameBounds = NameBounds[i];
                bool hasName = !string.IsNullOrEmpty(point.Name);
                float textH = Game1.smallFont.MeasureString("A").Y;
                float nameY = nameBounds.Y + (nameBounds.Height - textH) / 2;

                string locDisplayName = LocationClassifier.GetDisplayName(point.LocationName);

                if (hasName)
                {
                    int totalAvail = NameMaxWidth + SeparatorWidth + LocationMaxWidth;
                    var (displayName, displayLoc) = UIHelpers.TruncateNameAndLocation(
                        point.Name, locDisplayName, totalAvail, SeparatorWidth);

                    Utility.drawTextWithShadow(b, displayName, Game1.smallFont,
                        new Vector2(nameBounds.X, nameY), Game1.textColor);

                    float nameW = Game1.smallFont.MeasureString(displayName).X;
                    float sepX = nameBounds.X + nameW;
                    Utility.drawTextWithShadow(b, " | ", Game1.smallFont,
                        new Vector2(sepX, nameY), Game1.textColor * 0.4f);
                    float locX = sepX + SeparatorWidth;
                    Utility.drawTextWithShadow(b, displayLoc, Game1.smallFont,
                        new Vector2(locX, nameY), Game1.textColor * 0.6f);
                }
                else
                {
                    int fullWidth = NameMaxWidth + SeparatorWidth + LocationMaxWidth;
                    string truncLoc = UIHelpers.TruncateText(locDisplayName, fullWidth);
                    Utility.drawTextWithShadow(b, truncLoc, Game1.smallFont,
                        new Vector2(nameBounds.X, nameY), Game1.textColor);
                }

                DrawRowEditButton(b, EditBounds[i], anyDropdownOpen);
                DrawRowDeleteButton(b, DeleteBounds[i], anyDropdownOpen);
            }
        }

        private void DrawRowEditButton(SpriteBatch b, Rectangle btn, bool suppressHover)
        {
            bool hovered = !suppressHover && !UIHelpers.SuppressHover && btn.Contains(Game1.getMouseX(), Game1.getMouseY());
            float scale = hovered ? RowEditHoverScale : RowEditBaseScale;
            Vector2 center = new Vector2(btn.X + btn.Width / 2f, btn.Y + btn.Height / 2f);
            b.Draw(Game1.mouseCursors, center, new Rectangle(30, 428, 10, 10),
                Color.White, 0f, new Vector2(5, 5), scale, SpriteEffects.None, 1f);
        }

        private void DrawRowDeleteButton(SpriteBatch b, Rectangle btn, bool suppressHover)
        {
            bool hovered = !suppressHover && !UIHelpers.SuppressHover && btn.Contains(Game1.getMouseX(), Game1.getMouseY());
            float scale = hovered ? RowDeleteHoverScale : RowDeleteBaseScale;
            Vector2 center = new Vector2(btn.X + btn.Width / 2f, btn.Y + btn.Height / 2f);
            b.Draw(Game1.mouseCursors, center, new Rectangle(337, 494, 12, 12),
                Color.White, 0f, new Vector2(6, 6), scale, SpriteEffects.None, 1f);
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
            bool isHovered = !UIHelpers.SuppressHover && ConfigGearButton.containsPoint(Game1.getMouseX(), Game1.getMouseY());
            float buttonScale = isHovered ? ButtonHoveringScale : 1f;

            int bgSize = (int)(ConfigGearButton.bounds.Width * buttonScale);
            int bgX = ConfigGearButton.bounds.X + (ConfigGearButton.bounds.Width - bgSize) / 2;
            int bgY = ConfigGearButton.bounds.Y + (ConfigGearButton.bounds.Height - bgSize) / 2;

            UIHelpers.DrawTextureBox(b, bgX, bgY, bgSize, bgSize, Color.White, 1f, 4, 0.6f);

            Vector2 iconCenter = new Vector2(
                ConfigGearButton.bounds.X + ConfigGearButton.bounds.Width / 2,
                ConfigGearButton.bounds.Y + ConfigGearButton.bounds.Height / 2);
            Rectangle gearSource = new Rectangle(30, 428, 10, 10);
            float iconScale = (bgSize / 10f) * 0.6f;
            Vector2 origin = new Vector2(5, 5);
            b.Draw(Game1.mouseCursors, iconCenter, gearSource, Color.White, 0f,
                origin, iconScale, SpriteEffects.None, 1f);
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
