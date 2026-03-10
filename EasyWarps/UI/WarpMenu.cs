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

        private bool searchScopeOpen;
        private bool sortDropdownOpen;

        private TextBox? searchTextBox;
        private bool searchFocused = true;
        private string lastSearchText = "";

        private bool showDeleteConfirmation;
        private string? pendingDeleteId;
        private Rectangle deleteDialogBounds;
        private ClickableComponent? deleteYesButton;
        private ClickableComponent? deleteNoButton;

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

            filterState.SearchScope = ModEntry.Config.DefaultSearchScope;
            if (ModEntry.Config.RememberSortOption)
                filterState.SortMode = ModEntry.Config.LastSortMode;
            if (ModEntry.Config.RememberFavoriteOption)
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

        private bool AnyDropdownOpen => searchScopeOpen || sortDropdownOpen;

        private void CloseAllDropdowns()
        {
            searchScopeOpen = false;
            sortDropdownOpen = false;
        }

        private void PersistFilterState()
        {
            if (ModEntry.Config.RememberSortOption)
                ModEntry.Config.LastSortMode = filterState.SortMode;
            if (ModEntry.Config.RememberFavoriteOption)
                ModEntry.Config.LastFilterFavorite = filterState.FavoritesOnly;
        }

        // --- Input ---

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            bool clickOnGear = uiBuilder.ConfigGearButton.containsPoint(x, y);
            if (!isWithinBounds(x, y) && !clickOnGear && !showDeleteConfirmation && !AnyDropdownOpen)
                return;

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

            if (uiBuilder.CloseButton.containsPoint(x, y))
            {
                PersistFilterState();
                exitThisMenu();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }

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

            if (uiBuilder.SearchScopeDropdown.containsPoint(x, y))
            {
                searchScopeOpen = true;
                searchFocused = false;
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            if (uiBuilder.SearchClearButton.containsPoint(x, y) && searchTextBox != null && !string.IsNullOrEmpty(searchTextBox.Text))
            {
                searchTextBox.Text = "";
                lastSearchText = "";
                filterState.SearchText = "";
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            if (uiBuilder.SearchBar.containsPoint(x, y))
            {
                searchFocused = true;
                return;
            }

            if (uiBuilder.SortDropdown.containsPoint(x, y))
            {
                sortDropdownOpen = true;
                searchFocused = false;
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            if (uiBuilder.FavoritesCheckbox.containsPoint(x, y))
            {
                filterState.FavoritesOnly = !filterState.FavoritesOnly;
                RefreshDisplayedPoints();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

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

            if (uiBuilder.ConfigGearButton.containsPoint(x, y))
            {
                PersistFilterState();
                Game1.activeClickableMenu = new ConfigOverlay(this);
                if (playSound) Game1.playSound("bigSelect");
                return;
            }

            for (int i = 0; i < uiBuilder.ListItems.Count; i++)
            {
                int dataIndex = scrollOffset + i;
                if (dataIndex >= displayedPoints.Count)
                    break;

                if (!uiBuilder.ListItems[i].containsPoint(x, y))
                    continue;

                var point = displayedPoints[dataIndex];

                if (uiBuilder.FavStarBounds[i].Contains(x, y))
                {
                    store.ToggleFavorite(point.Id);
                    RefreshDisplayedPoints();
                    if (playSound) Game1.playSound("smallSelect");
                    return;
                }

                if (uiBuilder.EditBounds[i].Contains(x, y))
                {
                    OpenEditName(point);
                    if (playSound) Game1.playSound("bigSelect");
                    return;
                }

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

            // Try dropdown option clicks first (before bar checks, since options can overlap bars)
            if (TryHandleDropdownOptionClick(x, y, playSound))
                return;

            // Same dropdown bar = toggle off
            if ((searchScopeOpen && uiBuilder.SearchScopeDropdown.containsPoint(x, y)) ||
                (sortDropdownOpen && uiBuilder.SortDropdown.containsPoint(x, y)))
            {
                CloseAllDropdowns();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            // Different dropdown bar = switch
            if (uiBuilder.SearchScopeDropdown.containsPoint(x, y) ||
                uiBuilder.SortDropdown.containsPoint(x, y) ||
                uiBuilder.SearchBar.containsPoint(x, y))
            {
                CloseAllDropdowns();
                if (uiBuilder.SearchScopeDropdown.containsPoint(x, y))
                    searchScopeOpen = true;
                else if (uiBuilder.SortDropdown.containsPoint(x, y))
                    sortDropdownOpen = true;
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
                    PersistFilterState();
                    exitThisMenu();
                    Game1.playSound("bigDeSelect");
                    return;
                }
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
            uiBuilder.DrawSortFilterRow(b, filterState.SortMode, filterState.FavoritesOnly, sortDropdownOpen);
            uiBuilder.DrawDivider(b);

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
                    isSelected: opt => opt.name == selectedLabel,
                    panelPaddingV: WarpLayoutConstants.DropdownPanelPaddingV);
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
                    isSelected: opt => opt.name == selectedSort,
                    panelPaddingV: WarpLayoutConstants.DropdownPanelPaddingV);
            }
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

                bool hasName = !string.IsNullOrEmpty(point.Name);
                int totalAvail = uiBuilder.NameMaxWidth + uiBuilder.SeparatorWidth + uiBuilder.LocationMaxWidth;
                string locDisplayName = LocationClassifier.GetDisplayName(point.LocationName);

                if (hasName)
                {
                    float rawNameW = Game1.smallFont.MeasureString(point.Name).X;
                    float rawLocW = Game1.smallFont.MeasureString(locDisplayName).X;
                    float rawTotal = rawNameW + uiBuilder.SeparatorWidth + rawLocW;

                    if (rawTotal > totalAvail)
                        UIHelpers.DrawWrappedTooltip(Game1.spriteBatch, $"{point.Name} | {locDisplayName}", 450);
                }
                else
                {
                    bool locTruncated = Game1.smallFont.MeasureString(locDisplayName).X > totalAvail;
                    if (locTruncated)
                        UIHelpers.DrawWrappedTooltip(Game1.spriteBatch, locDisplayName, 450);
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
