using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using EasyWarps.Core;
using EasyWarps.Models;
using EasyWarps.Services;
using EasyWarps.Utilities;
using static EasyWarps.WarpLayoutConstants;
using SObject = StardewValley.Object;

namespace EasyWarps.UI
{
    public class WarpMenu : IClickableMenu
    {
        private readonly WarpMenuUIBuilder uiBuilder;
        private readonly WarpPointStore store;
        private readonly WarpService warpService;
        private readonly SObject interactedSign;
        private readonly Vector2 interactedSignTile;

        private readonly WarpFilterState filterState = new();
        private List<WarpPoint> displayedPoints = new();
        private int scrollOffset;

        // Dropdown state
        private bool searchScopeOpen;
        private bool sortDropdownOpen;
        private bool filterDropdownOpen;

        // Search
        private TextBox? searchTextBox;
        private bool searchFocused = true;
        private string lastSearchText = "";

        // Delete confirmation
        private bool showDeleteConfirmation;
        private string? pendingDeleteId;
        private Rectangle deleteDialogBounds;
        private ClickableComponent? deleteYesButton;
        private ClickableComponent? deleteNoButton;

        // Hover tracking
        private string? hoveredPointId;

        public WarpMenu(WarpPointStore store, SObject interactedSign, Vector2 interactedSignTile)
        {
            this.store = store;
            this.interactedSign = interactedSign;
            this.interactedSignTile = interactedSignTile;
            this.warpService = new WarpService();

            uiBuilder = new WarpMenuUIBuilder();
            width = uiBuilder.Width;
            height = uiBuilder.Height;
            xPositionOnScreen = uiBuilder.X;
            yPositionOnScreen = uiBuilder.Y;

            // Initialize filter from config
            filterState.SearchScope = ModEntry.Config.DefaultSearchScope;
            if (ModEntry.Config.RememberSortOption)
                filterState.SortMode = ModEntry.Config.LastSortMode;
            if (ModEntry.Config.RememberFilterOption)
                filterState.FavoritesOnly = ModEntry.Config.LastFilterFavorite;

            CreateSearchTextBox();
            Game1.keyboardDispatcher.Subscriber = searchTextBox;
            RefreshDisplayedPoints();
        }

        private void CreateSearchTextBox()
        {
            searchTextBox = new TextBox(
                Game1.content.Load<Texture2D>("LooseSprites\\textBox"),
                null, Game1.smallFont, Game1.textColor)
            {
                Text = "",
                Selected = true
            };
            UpdateSearchTextBoxBounds();
        }

        private void UpdateSearchTextBoxBounds()
        {
            if (searchTextBox == null) return;
            var bounds = uiBuilder.SearchBar.bounds;
            searchTextBox.X = bounds.X + 16;
            searchTextBox.Y = bounds.Y + ((bounds.Height - 48) / 2);
            searchTextBox.Width = bounds.Width - 32;
        }

        private void RefreshDisplayedPoints()
        {
            displayedPoints = store.GetFilteredPoints(filterState, LocationClassifier.Classify);
            ClampScroll();
        }

        private void ClampScroll()
        {
            int maxVisible = MaxVisibleRows;
            scrollOffset = UIHelpers.ClampScrollOffset(scrollOffset, displayedPoints.Count, maxVisible);
        }

        private bool AnyDropdownOpen => searchScopeOpen || sortDropdownOpen || filterDropdownOpen;

        private void CloseAllDropdowns()
        {
            searchScopeOpen = false;
            sortDropdownOpen = false;
            filterDropdownOpen = false;
        }

        private void PersistFilterState()
        {
            if (ModEntry.Config.RememberSortOption)
                ModEntry.Config.LastSortMode = filterState.SortMode;
            if (ModEntry.Config.RememberFilterOption)
                ModEntry.Config.LastFilterFavorite = filterState.FavoritesOnly;
        }

        // --- Input ---

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!isWithinBounds(x, y) && !showDeleteConfirmation && !AnyDropdownOpen)
            {
                PersistFilterState();
                exitThisMenu();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }

            if (showDeleteConfirmation)
            {
                HandleDeleteConfirmationClick(x, y, playSound);
                return;
            }

            if (AnyDropdownOpen)
            {
                HandleDropdownOpenClick(x, y, playSound);
                return;
            }

            // Close button
            if (uiBuilder.CloseButton.containsPoint(x, y))
            {
                PersistFilterState();
                exitThisMenu();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }

            // Tabs
            if (uiBuilder.TabAll.containsPoint(x, y))
            {
                filterState.Category = WarpCategory.All;
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }
            if (uiBuilder.TabFarm.containsPoint(x, y))
            {
                filterState.Category = WarpCategory.Farm;
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }
            if (uiBuilder.TabWorld.containsPoint(x, y))
            {
                filterState.Category = WarpCategory.World;
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Search scope dropdown bar
            if (uiBuilder.SearchScopeDropdown.containsPoint(x, y))
            {
                searchScopeOpen = true;
                searchFocused = false;
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Search bar
            if (uiBuilder.SearchBar.containsPoint(x, y))
            {
                searchFocused = true;
                return;
            }

            // Search clear
            if (uiBuilder.SearchClearButton.containsPoint(x, y) && searchTextBox != null && !string.IsNullOrEmpty(searchTextBox.Text))
            {
                searchTextBox.Text = "";
                lastSearchText = "";
                filterState.SearchText = "";
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Sort dropdown bar
            if (uiBuilder.SortDropdown.containsPoint(x, y))
            {
                sortDropdownOpen = true;
                searchFocused = false;
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Filter dropdown bar
            if (uiBuilder.FilterDropdown.containsPoint(x, y))
            {
                filterDropdownOpen = true;
                searchFocused = false;
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Filter clear
            if (uiBuilder.FilterClearButton.containsPoint(x, y) && filterState.FavoritesOnly)
            {
                filterState.FavoritesOnly = false;
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Scroll arrows
            if (uiBuilder.ScrollUpArrow.containsPoint(x, y) && scrollOffset > 0)
            {
                scrollOffset--;
                if (playSound) Game1.playSound("shiny4");
                return;
            }
            if (uiBuilder.ScrollDownArrow.containsPoint(x, y) && scrollOffset + MaxVisibleRows < displayedPoints.Count)
            {
                scrollOffset++;
                if (playSound) Game1.playSound("shiny4");
                return;
            }

            // Config gear
            if (uiBuilder.ConfigGearButton.containsPoint(x, y))
            {
                PersistFilterState();
                Game1.activeClickableMenu = new ConfigOverlay(this);
                if (playSound) Game1.playSound("bigSelect");
                return;
            }

            // List row clicks
            for (int i = 0; i < uiBuilder.ListItems.Count; i++)
            {
                int dataIndex = scrollOffset + i;
                if (dataIndex >= displayedPoints.Count)
                    break;

                if (!uiBuilder.ListItems[i].containsPoint(x, y))
                    continue;

                var point = displayedPoints[dataIndex];

                // Fav star
                if (uiBuilder.FavStarBounds[i].Contains(x, y))
                {
                    store.ToggleFavorite(point.Id);
                    RefreshDisplayedPoints();
                    if (playSound) Game1.playSound("smallSelect");
                    return;
                }

                // Edit gear
                if (uiBuilder.EditBounds[i].Contains(x, y))
                {
                    OpenEditName(point);
                    if (playSound) Game1.playSound("bigSelect");
                    return;
                }

                // Delete X
                if (uiBuilder.DeleteBounds[i].Contains(x, y))
                {
                    ShowDeleteConfirmation(point.Id);
                    if (playSound) Game1.playSound("smallSelect");
                    return;
                }

                // Row click = warp
                ExecuteWarp(point);
                return;
            }
        }

        private void HandleDropdownOpenClick(int x, int y, bool playSound)
        {
            // Close button always works
            if (uiBuilder.CloseButton.containsPoint(x, y))
            {
                CloseAllDropdowns();
                PersistFilterState();
                exitThisMenu();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }

            // Search clear always works
            if (uiBuilder.SearchClearButton.containsPoint(x, y) && searchTextBox != null && !string.IsNullOrEmpty(searchTextBox.Text))
            {
                CloseAllDropdowns();
                searchTextBox.Text = "";
                lastSearchText = "";
                filterState.SearchText = "";
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Filter clear always works
            if (uiBuilder.FilterClearButton.containsPoint(x, y) && filterState.FavoritesOnly)
            {
                CloseAllDropdowns();
                filterState.FavoritesOnly = false;
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Try dropdown option clicks first (before bar checks, since options can overlap bars)
            if (TryHandleDropdownOptionClick(x, y, playSound))
                return;

            // Same dropdown bar = toggle off
            if ((searchScopeOpen && uiBuilder.SearchScopeDropdown.containsPoint(x, y)) ||
                (sortDropdownOpen && uiBuilder.SortDropdown.containsPoint(x, y)) ||
                (filterDropdownOpen && uiBuilder.FilterDropdown.containsPoint(x, y)))
            {
                CloseAllDropdowns();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Different dropdown bar = switch
            if (uiBuilder.SearchScopeDropdown.containsPoint(x, y) ||
                uiBuilder.SortDropdown.containsPoint(x, y) ||
                uiBuilder.FilterDropdown.containsPoint(x, y) ||
                uiBuilder.SearchBar.containsPoint(x, y))
            {
                CloseAllDropdowns();
                if (uiBuilder.SearchScopeDropdown.containsPoint(x, y))
                    searchScopeOpen = true;
                else if (uiBuilder.SortDropdown.containsPoint(x, y))
                    sortDropdownOpen = true;
                else if (uiBuilder.FilterDropdown.containsPoint(x, y))
                    filterDropdownOpen = true;
                else if (uiBuilder.SearchBar.containsPoint(x, y))
                    searchFocused = true;
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Click elsewhere closes dropdown
            CloseAllDropdowns();
        }

        private bool TryHandleDropdownOptionClick(int x, int y, bool playSound)
        {
            if (searchScopeOpen)
            {
                WarpSearchScope[] scopes = { WarpSearchScope.All, WarpSearchScope.Name, WarpSearchScope.Location };
                for (int i = 0; i < uiBuilder.SearchScopeOptions.Count; i++)
                {
                    if (uiBuilder.SearchScopeOptions[i].containsPoint(x, y))
                    {
                        filterState.SearchScope = scopes[i];
                        CloseAllDropdowns();
                        RefreshDisplayedPoints();
                        if (playSound) Game1.playSound("smallSelect");
                        return true;
                    }
                }
            }

            if (sortDropdownOpen)
            {
                WarpSortMode[] modes = { WarpSortMode.AToZ, WarpSortMode.ZToA, WarpSortMode.Newest, WarpSortMode.Oldest, WarpSortMode.LastUsed };
                for (int i = 0; i < uiBuilder.SortOptions.Count; i++)
                {
                    if (uiBuilder.SortOptions[i].containsPoint(x, y))
                    {
                        filterState.SortMode = modes[i];
                        CloseAllDropdowns();
                        RefreshDisplayedPoints();
                        if (playSound) Game1.playSound("smallSelect");
                        return true;
                    }
                }
            }

            if (filterDropdownOpen)
            {
                for (int i = 0; i < uiBuilder.FilterOptions.Count; i++)
                {
                    if (uiBuilder.FilterOptions[i].containsPoint(x, y))
                    {
                        // Only one filter option: Favorite toggle
                        filterState.FavoritesOnly = !filterState.FavoritesOnly;
                        CloseAllDropdowns();
                        RefreshDisplayedPoints();
                        if (playSound) Game1.playSound("smallSelect");
                        return true;
                    }
                }
            }

            return false;
        }

        private void HandleDeleteConfirmationClick(int x, int y, bool playSound)
        {
            if (deleteYesButton != null && deleteYesButton.containsPoint(x, y))
            {
                ConfirmDelete();
                return;
            }

            if (deleteNoButton != null && deleteNoButton.containsPoint(x, y))
            {
                HideDeleteConfirmation();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (showDeleteConfirmation)
            {
                if (key == Keys.Escape)
                {
                    HideDeleteConfirmation();
                    Game1.playSound("bigDeSelect");
                }
                else if (key == Keys.Enter)
                {
                    ConfirmDelete();
                }
                return;
            }

            if (AnyDropdownOpen)
            {
                if (key == Keys.Escape)
                {
                    CloseAllDropdowns();
                    Game1.playSound("bigDeSelect");
                }
                return;
            }

            if (searchFocused)
            {
                if (key == Keys.Escape)
                {
                    searchFocused = false;
                    Game1.playSound("bigDeSelect");
                    return;
                }
                if (key == Keys.Tab)
                {
                    searchFocused = false;
                    return;
                }
                // Let text input handle other keys
                return;
            }

            int maxScroll = Math.Max(0, displayedPoints.Count - MaxVisibleRows);

            if (key == Keys.Up && scrollOffset > 0)
            {
                scrollOffset--;
                Game1.playSound("shiny4");
                return;
            }
            if (key == Keys.Down && scrollOffset < maxScroll)
            {
                scrollOffset++;
                Game1.playSound("shiny4");
                return;
            }
            if (key == Keys.PageUp && scrollOffset > 0)
            {
                scrollOffset = Math.Max(0, scrollOffset - MaxVisibleRows);
                Game1.playSound("shiny4");
                return;
            }
            if (key == Keys.PageDown && scrollOffset < maxScroll)
            {
                scrollOffset = Math.Min(maxScroll, scrollOffset + MaxVisibleRows);
                Game1.playSound("shiny4");
                return;
            }

            if (key == Keys.Escape)
            {
                PersistFilterState();
                exitThisMenu();
                Game1.playSound("bigDeSelect");
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (showDeleteConfirmation)
                return;

            int maxScroll = Math.Max(0, displayedPoints.Count - MaxVisibleRows);

            if (direction > 0 && scrollOffset > 0)
            {
                scrollOffset--;
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && scrollOffset < maxScroll)
            {
                scrollOffset++;
                Game1.playSound("shiny4");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            hoveredPointId = null;
            if (AnyDropdownOpen || showDeleteConfirmation)
                return;

            for (int i = 0; i < uiBuilder.ListItems.Count; i++)
            {
                int dataIndex = scrollOffset + i;
                if (dataIndex >= displayedPoints.Count)
                    break;
                if (uiBuilder.ListItems[i].containsPoint(x, y))
                {
                    hoveredPointId = displayedPoints[dataIndex].Id;
                    break;
                }
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if (searchTextBox != null)
            {
                searchTextBox.Update();
                searchTextBox.Selected = searchFocused;

                // Detect text changes from TextBox
                string currentText = searchTextBox.Text ?? "";
                if (currentText != lastSearchText)
                {
                    lastSearchText = currentText;
                    filterState.SearchText = currentText;
                    RefreshDisplayedPoints();
                }
            }
        }

        // --- Actions ---

        private void ExecuteWarp(WarpPoint point)
        {
            PersistFilterState();
            Game1.activeClickableMenu = null;
            warpService.ExecuteWarp(point, interactedSignTile, Game1.player, store);
        }

        private void OpenEditName(WarpPoint point)
        {
            PersistFilterState();
            var existingWarp = store.GetById(point.Id);
            if (existingWarp == null) return;

            Game1.activeClickableMenu = new SignEditMenu(interactedSign, store, existingWarp, editNameOnly: true);
        }

        private void ShowDeleteConfirmation(string pointId)
        {
            pendingDeleteId = pointId;
            showDeleteConfirmation = true;
            RecalculateDeleteDialog();
        }

        private void RecalculateDeleteDialog()
        {
            var (bounds, yes, no) = UIHelpers.CalculateDeleteDialogLayout(
                TranslationCache.DeleteConfirmQuestion,
                TranslationCache.CommonYes,
                TranslationCache.CommonNo);
            deleteDialogBounds = bounds;
            deleteYesButton = yes;
            deleteNoButton = no;
        }

        private void ConfirmDelete()
        {
            if (pendingDeleteId != null)
            {
                store.Delete(pendingDeleteId);
                RefreshDisplayedPoints();
                Game1.playSound("trashcan");
            }
            HideDeleteConfirmation();
        }

        private void HideDeleteConfirmation()
        {
            showDeleteConfirmation = false;
            pendingDeleteId = null;
            deleteYesButton = null;
            deleteNoButton = null;
        }

        // --- Drawing ---

        public override void draw(SpriteBatch b)
        {
            uiBuilder.DrawBackground(b);
            uiBuilder.DrawTabs(b, filterState.Category);
            uiBuilder.DrawSearchRow(b, searchTextBox?.Text ?? "", searchFocused, filterState.SearchScope, searchScopeOpen);
            uiBuilder.DrawSortFilterRow(b, filterState.SortMode, filterState.FavoritesOnly, sortDropdownOpen, filterDropdownOpen);

            if (displayedPoints.Count == 0)
            {
                string msg = store.Count == 0 ? TranslationCache.EmptyNoWarpPoints : TranslationCache.EmptyNoMatches;
                uiBuilder.DrawEmptyState(b, msg);
            }
            else
            {
                uiBuilder.DrawList(b, displayedPoints, scrollOffset, AnyDropdownOpen, hoveredPointId);
                uiBuilder.DrawScrollArrows(b, scrollOffset, displayedPoints.Count);
            }

            uiBuilder.DrawConfigGear(b);
            uiBuilder.DrawCloseButton(b);

            // Dropdowns on top (z-order)
            if (searchScopeOpen)
            {
                string selectedLabel = filterState.SearchScope switch
                {
                    WarpSearchScope.All => TranslationCache.SearchScopeAll,
                    WarpSearchScope.Name => TranslationCache.SearchScopeName,
                    WarpSearchScope.Location => TranslationCache.SearchScopeLocation,
                    _ => TranslationCache.SearchScopeAll
                };
                UIHelpers.DrawDropdownOptions(b, uiBuilder.SearchScopeDropdown.bounds,
                    uiBuilder.SearchScopeOptions, 0, uiBuilder.SearchScopeOptions.Count,
                    isSelected: opt => opt.name == selectedLabel);
            }
            else if (sortDropdownOpen)
            {
                string selectedSort = filterState.SortMode switch
                {
                    WarpSortMode.AToZ => TranslationCache.SortAToZ,
                    WarpSortMode.ZToA => TranslationCache.SortZToA,
                    WarpSortMode.Newest => TranslationCache.SortNewest,
                    WarpSortMode.Oldest => TranslationCache.SortOldest,
                    WarpSortMode.LastUsed => TranslationCache.SortLastUsed,
                    _ => TranslationCache.SortAToZ
                };
                UIHelpers.DrawDropdownOptions(b, uiBuilder.SortDropdown.bounds,
                    uiBuilder.SortOptions, 0, uiBuilder.SortOptions.Count,
                    isSelected: opt => opt.name == selectedSort);
            }
            else if (filterDropdownOpen)
            {
                UIHelpers.DrawDropdownOptions(b, uiBuilder.FilterDropdown.bounds,
                    uiBuilder.FilterOptions, 0, uiBuilder.FilterOptions.Count,
                    isSelected: opt => opt.name == TranslationCache.FilterFavorite && filterState.FavoritesOnly);
            }

            // Tooltips (when no dropdown/dialog is showing)
            if (!AnyDropdownOpen && !showDeleteConfirmation)
            {
                DrawTooltips();
            }

            // Delete confirmation on top of everything
            if (showDeleteConfirmation)
            {
                UIHelpers.DrawDeleteConfirmationDialog(b, deleteDialogBounds,
                    TranslationCache.DeleteConfirmQuestion,
                    deleteYesButton!, TranslationCache.CommonYes,
                    deleteNoButton!, TranslationCache.CommonNo);
            }

            drawMouse(b);
        }

        private void DrawTooltips()
        {
            if (hoveredPointId == null)
                return;

            // Find the hovered point to check if name or location is truncated
            var point = store.GetById(hoveredPointId);
            if (point == null) return;

            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            for (int i = 0; i < uiBuilder.ListItems.Count; i++)
            {
                int dataIndex = scrollOffset + i;
                if (dataIndex >= displayedPoints.Count) break;
                if (displayedPoints[dataIndex].Id != hoveredPointId) continue;

                var nameBounds = uiBuilder.NameBounds[i];
                if (!nameBounds.Contains(mouseX, mouseY)) break;

                bool nameTruncated = Game1.smallFont.MeasureString(point.Name).X > uiBuilder.NameMaxWidth;
                bool locTruncated = Game1.smallFont.MeasureString(point.LocationName).X > uiBuilder.LocationMaxWidth;

                if (nameTruncated || locTruncated)
                {
                    string tooltip = nameTruncated && locTruncated
                        ? $"{point.Name} ({point.LocationName})"
                        : nameTruncated ? point.Name : point.LocationName;
                    UIHelpers.DrawWrappedTooltip(Game1.spriteBatch, tooltip);
                }
                break;
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            uiBuilder.Recalculate();
            width = uiBuilder.Width;
            height = uiBuilder.Height;
            xPositionOnScreen = uiBuilder.X;
            yPositionOnScreen = uiBuilder.Y;

            UpdateSearchTextBoxBounds();

            if (showDeleteConfirmation)
                RecalculateDeleteDialog();
        }

        public override bool isWithinBounds(int x, int y)
        {
            return x >= xPositionOnScreen && x < xPositionOnScreen + width
                && y >= yPositionOnScreen && y < yPositionOnScreen + height;
        }

        public override bool readyToClose()
        {
            return true;
        }
    }
}
