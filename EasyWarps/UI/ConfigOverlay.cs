using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using EasyWarps.Core;
using EasyWarps.Models;
using EasyWarps.Utilities;
using static EasyWarps.WarpLayoutConstants;

namespace EasyWarps.UI
{
    public class ConfigOverlay : IClickableMenu
    {
        private readonly IClickableMenu backMenu;
        private readonly ConfigUIBuilder uiBuilder;

        private bool disableModRequirement;
        private bool alwaysRegister;
        private bool enableAnimation;
        private bool disableHoverText;
        private bool rememberSort;
        private bool rememberFavorite;
        private WarpSearchScope defaultSearchScope;

        private bool searchScopeDropdownOpen;

        public ConfigOverlay(IClickableMenu backMenu)
        {
            this.backMenu = backMenu;
            uiBuilder = new ConfigUIBuilder();

            disableModRequirement = ModEntry.Config.DisableModRequirement;
            alwaysRegister = ModEntry.Config.AlwaysRegisterAsWarpPoint;
            enableAnimation = ModEntry.Config.EnableWarpAnimation;
            disableHoverText = ModEntry.Config.DisableSignHoveringText;
            rememberSort = ModEntry.Config.RememberSortOption;
            rememberFavorite = ModEntry.Config.RememberFavoriteOption;
            defaultSearchScope = ModEntry.Config.DefaultSearchScope;

            Game1.keyboardDispatcher.Subscriber = null;

            UpdateDropdownPanelPosition();

            width = uiBuilder.Width;
            height = uiBuilder.Height;
            xPositionOnScreen = uiBuilder.X;
            yPositionOnScreen = uiBuilder.Y;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (uiBuilder.CloseButton.containsPoint(x, y))
            {
                CloseOverlay();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }

            if (!isWithinBounds(x, y))
            {
                bool clickedDropdownPanel = searchScopeDropdownOpen
                    && uiBuilder.SearchScopeOptions.Any(o => o.containsPoint(x, y));

                if (!clickedDropdownPanel)
                {
                    CloseOverlay();
                    if (playSound) Game1.playSound("bigDeSelect");
                    return;
                }
            }

            if (searchScopeDropdownOpen)
            {
                if (TryHandleDropdownClick(x, y, playSound))
                {
                    UpdateDropdownPanelPosition();
                    return;
                }
            }

            if (uiBuilder.SaveButton.containsPoint(x, y))
            {
                HandleSave();
                if (playSound) Game1.playSound("coin");
                return;
            }
            if (uiBuilder.CloseMenuButton.containsPoint(x, y))
            {
                CloseOverlay();
                if (playSound) Game1.playSound("bigDeSelect");
                return;
            }

            if (TryToggleCheckbox(uiBuilder.DisableModRequirementCheckbox, x, y, ref disableModRequirement, playSound)) return;
            if (TryToggleCheckbox(uiBuilder.AlwaysRegisterCheckbox, x, y, ref alwaysRegister, playSound)) return;
            if (TryToggleCheckbox(uiBuilder.EnableAnimationCheckbox, x, y, ref enableAnimation, playSound)) return;
            if (TryToggleCheckbox(uiBuilder.DisableHoverTextCheckbox, x, y, ref disableHoverText, playSound)) return;
            if (TryToggleCheckbox(uiBuilder.RememberSortCheckbox, x, y, ref rememberSort, playSound)) return;
            if (TryToggleCheckbox(uiBuilder.RememberFavoriteCheckbox, x, y, ref rememberFavorite, playSound)) return;

            if (IsInRowOf(uiBuilder.SearchScopeDropdown, x, y))
            {
                searchScopeDropdownOpen = !searchScopeDropdownOpen;
                if (playSound) Game1.playSound("smallSelect");
                return;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                CloseOverlay();
                Game1.playSound("bigDeSelect");
                return;
            }

            if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
                return;
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * BackgroundOverlayOpacity);

            UIHelpers.DrawTextureBox(b, uiBuilder.TitleBoxBounds.X, uiBuilder.TitleBoxBounds.Y,
                uiBuilder.TitleBoxBounds.Width, uiBuilder.TitleBoxBounds.Height, Color.White);

            UIHelpers.DrawTextureBox(b, uiBuilder.ContentBoxBounds.X, uiBuilder.ContentBoxBounds.Y,
                uiBuilder.ContentBoxBounds.Width, uiBuilder.ContentBoxBounds.Height, Color.White);

            uiBuilder.DrawTitle(b);

            uiBuilder.DrawCheckboxRow(b, TranslationCache.ConfigDisableModRequirementName, disableModRequirement, uiBuilder.DisableModRequirementCheckbox);
            uiBuilder.DrawCheckboxRow(b, TranslationCache.ConfigAlwaysRegisterName, alwaysRegister, uiBuilder.AlwaysRegisterCheckbox);
            uiBuilder.DrawCheckboxRow(b, TranslationCache.ConfigEnableAnimationName, enableAnimation, uiBuilder.EnableAnimationCheckbox);
            uiBuilder.DrawCheckboxRow(b, TranslationCache.ConfigDisableHoverTextName, disableHoverText, uiBuilder.DisableHoverTextCheckbox);
            uiBuilder.DrawCheckboxRow(b, TranslationCache.ConfigRememberSortName, rememberSort, uiBuilder.RememberSortCheckbox);
            uiBuilder.DrawCheckboxRow(b, TranslationCache.ConfigRememberFavoriteName, rememberFavorite, uiBuilder.RememberFavoriteCheckbox);

            string scopeDisplay = ConfigUIBuilder.FormatSearchScope(defaultSearchScope);
            uiBuilder.DrawSearchScopeRow(b, TranslationCache.ConfigDefaultSearchScopeName, scopeDisplay, searchScopeDropdownOpen);

            uiBuilder.DrawButtons(b);
            uiBuilder.DrawCloseButton(b);

            if (searchScopeDropdownOpen)
            {
                uiBuilder.DrawSearchScopeDropdownOptions(b, scopeDisplay);
            }

            if (!searchScopeDropdownOpen)
            {
                int mouseX = Game1.getMouseX();
                int mouseY = Game1.getMouseY();
                string? tooltip = uiBuilder.GetHoveredTooltip(mouseX, mouseY);
                if (tooltip != null)
                {
                    string wrappedTooltip = Game1.parseText(tooltip, Game1.smallFont, 450);
                    IClickableMenu.drawHoverText(b, wrappedTooltip, Game1.smallFont);
                }
            }

            drawMouse(b);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            uiBuilder.Recalculate();
            UpdateDropdownPanelPosition();
            width = uiBuilder.Width;
            height = uiBuilder.Height;
            xPositionOnScreen = uiBuilder.X;
            yPositionOnScreen = uiBuilder.Y;
        }

        private void UpdateDropdownPanelPosition()
        {
            uiBuilder.UpdateSearchScopePanelPosition(ConfigUIBuilder.FormatSearchScope(defaultSearchScope));
        }

        private bool IsInRowOf(ClickableComponent control, int x, int y)
        {
            int rowY = control.bounds.Height < ConfigRowHeight
                ? control.bounds.Y - (ConfigRowHeight - control.bounds.Height) / 2
                : control.bounds.Y;
            return x >= uiBuilder.ContentBoxBounds.X + ConfigBorderPadding
                && x < uiBuilder.ContentBoxBounds.Right - ConfigBorderPadding
                && y >= rowY
                && y < rowY + ConfigRowHeight;
        }

        private bool TryHandleDropdownClick(int x, int y, bool playSound)
        {
            foreach (var option in uiBuilder.SearchScopeOptions)
            {
                if (option.containsPoint(x, y))
                {
                    if (option.name == TranslationCache.SearchScopeAll)
                        defaultSearchScope = WarpSearchScope.All;
                    else if (option.name == TranslationCache.SearchScopeName)
                        defaultSearchScope = WarpSearchScope.Name;
                    else if (option.name == TranslationCache.SearchScopeLocation)
                        defaultSearchScope = WarpSearchScope.Location;

                    searchScopeDropdownOpen = false;
                    if (playSound) Game1.playSound("smallSelect");
                    return true;
                }
            }

            if (IsInRowOf(uiBuilder.SearchScopeDropdown, x, y))
            {
                searchScopeDropdownOpen = false;
                if (playSound) Game1.playSound("smallSelect");
                return true;
            }

            searchScopeDropdownOpen = false;
            return true;
        }

        private bool TryToggleCheckbox(ClickableComponent checkbox, int x, int y, ref bool value, bool playSound)
        {
            if (!IsInRowOf(checkbox, x, y))
                return false;

            value = !value;
            if (playSound) Game1.playSound("drumkit6");
            return true;
        }

        private void HandleSave()
        {
            ModEntry.Config.DisableModRequirement = disableModRequirement;
            ModEntry.Config.AlwaysRegisterAsWarpPoint = alwaysRegister;
            ModEntry.Config.EnableWarpAnimation = enableAnimation;
            ModEntry.Config.DisableSignHoveringText = disableHoverText;
            ModEntry.Config.RememberSortOption = rememberSort;
            ModEntry.Config.RememberFavoriteOption = rememberFavorite;
            ModEntry.Config.DefaultSearchScope = defaultSearchScope;

            ModEntry.Instance.SaveConfig();

            Game1.activeClickableMenu = backMenu;
        }

        private void CloseOverlay()
        {
            Game1.activeClickableMenu = backMenu;
        }
    }
}
