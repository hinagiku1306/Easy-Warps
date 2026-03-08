using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using EasyWarps.Utilities;
using static EasyWarps.WarpLayoutConstants;

namespace EasyWarps.UI
{
    public class ConfigUIBuilder
    {
        public ClickableTextureComponent CloseButton { get; private set; } = null!;

        public ClickableComponent DisableModRequirementCheckbox { get; private set; } = null!;
        public ClickableComponent AlwaysRegisterCheckbox { get; private set; } = null!;
        public ClickableComponent EnableAnimationCheckbox { get; private set; } = null!;
        public ClickableComponent DisableHoverTextCheckbox { get; private set; } = null!;
        public ClickableComponent RememberSortCheckbox { get; private set; } = null!;
        public ClickableComponent RememberFilterCheckbox { get; private set; } = null!;

        public ClickableComponent SearchScopeDropdown { get; private set; } = null!;
        public List<ClickableComponent> SearchScopeOptions { get; private set; } = new();
        private Rectangle searchScopePanelAnchor;
        private int searchScopePanelW;

        public ClickableComponent SaveButton { get; private set; } = null!;
        public ClickableComponent CloseMenuButton { get; private set; } = null!;

        private int[] rowYPositions = Array.Empty<int>();
        private string[] rowLabels = Array.Empty<string>();
        private string[] rowTooltips = Array.Empty<string>();

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Rectangle TitleBoxBounds { get; private set; }
        public Rectangle ContentBoxBounds { get; private set; }
        public Rectangle ButtonBoxBounds { get; private set; }

        private int contentX;
        private int contentWidth;
        private int controlX;

        public ConfigUIBuilder()
        {
            Width = ConfigOverlayWidth;
            Recalculate();
        }

        public void Recalculate()
        {
            int totalRows = 7;

            int contentNatural = ConfigBorderPadding * 2 + ConfigRowHeight * totalRows;

            Vector2 titleSize = Game1.dialogueFont.MeasureString(TranslationCache.ConfigTitle);
            int titleBoxHeight = ConfigTitleTopPadding + (int)titleSize.Y + ConfigTitleBottomPadding;

            Height = ConfigBorderPadding + titleBoxHeight + contentNatural + ConfigBoxGap + ConfigButtonBoxHeight;

            X = (Game1.uiViewport.Width - Width) / 2;
            Y = (Game1.uiViewport.Height - Height) / 2;

            int currentY = Y + ConfigBorderPadding;

            int titleBoxWidth = (int)titleSize.X + ConfigTitleSidePadding * 2;
            int titleBoxX = X + (Width - titleBoxWidth) / 2;
            TitleBoxBounds = new Rectangle(titleBoxX, currentY, titleBoxWidth, titleBoxHeight);
            currentY += titleBoxHeight;

            ContentBoxBounds = new Rectangle(X, currentY, Width, contentNatural);
            currentY += contentNatural + ConfigBoxGap;

            ButtonBoxBounds = new Rectangle(X, currentY, Width, ConfigButtonBoxHeight);

            contentX = X + ConfigBorderPadding;
            contentWidth = Width - ConfigBorderPadding * 2;
            controlX = contentX + contentWidth - ConfigControlRightPadding;

            CalculateLayout();
        }

        private void CalculateLayout()
        {
            var rowYList = new List<int>();
            var labelsList = new List<string>();
            var tooltipsList = new List<string>();

            int currentY = ContentBoxBounds.Y + ConfigBorderPadding;

            rowYList.Add(currentY);
            labelsList.Add(TranslationCache.ConfigDisableModRequirementName);
            tooltipsList.Add(TranslationCache.ConfigDisableModRequirementTooltip);
            DisableModRequirementCheckbox = CreateCheckbox(currentY, "DisableModRequirement");
            currentY += ConfigRowHeight;

            rowYList.Add(currentY);
            labelsList.Add(TranslationCache.ConfigAlwaysRegisterName);
            tooltipsList.Add(TranslationCache.ConfigAlwaysRegisterTooltip);
            AlwaysRegisterCheckbox = CreateCheckbox(currentY, "AlwaysRegister");
            currentY += ConfigRowHeight;

            rowYList.Add(currentY);
            labelsList.Add(TranslationCache.ConfigEnableAnimationName);
            tooltipsList.Add(TranslationCache.ConfigEnableAnimationTooltip);
            EnableAnimationCheckbox = CreateCheckbox(currentY, "EnableAnimation");
            currentY += ConfigRowHeight;

            rowYList.Add(currentY);
            labelsList.Add(TranslationCache.ConfigDisableHoverTextName);
            tooltipsList.Add(TranslationCache.ConfigDisableHoverTextTooltip);
            DisableHoverTextCheckbox = CreateCheckbox(currentY, "DisableHoverText");
            currentY += ConfigRowHeight;

            rowYList.Add(currentY);
            labelsList.Add(TranslationCache.ConfigRememberSortName);
            tooltipsList.Add(TranslationCache.ConfigRememberSortTooltip);
            RememberSortCheckbox = CreateCheckbox(currentY, "RememberSort");
            currentY += ConfigRowHeight;

            rowYList.Add(currentY);
            labelsList.Add(TranslationCache.ConfigRememberFilterName);
            tooltipsList.Add(TranslationCache.ConfigRememberFilterTooltip);
            RememberFilterCheckbox = CreateCheckbox(currentY, "RememberFilter");
            currentY += ConfigRowHeight;

            rowYList.Add(currentY);
            labelsList.Add(TranslationCache.ConfigDefaultSearchScopeName);
            tooltipsList.Add(TranslationCache.ConfigDefaultSearchScopeTooltip);
            int dropdownX = controlX - ConfigDropdownMinWidth;
            SearchScopeDropdown = new ClickableComponent(
                new Rectangle(dropdownX, currentY, ConfigDropdownMinWidth, ConfigRowHeight),
                "SearchScopeDropdown"
            );

            string[] scopeLabels = {
                TranslationCache.SearchScopeAll,
                TranslationCache.SearchScopeName,
                TranslationCache.SearchScopeLocation
            };
            int maxLabelWidth = 0;
            foreach (var lbl in scopeLabels)
                maxLabelWidth = Math.Max(maxLabelWidth, (int)Game1.smallFont.MeasureString(lbl).X);
            searchScopePanelW = Math.Min(ConfigDropdownMaxWidth, maxLabelWidth + 32);
            searchScopePanelAnchor = new Rectangle(controlX - searchScopePanelW, currentY, searchScopePanelW, ConfigRowHeight);
            BuildSearchScopeOptions();

            rowYPositions = rowYList.ToArray();
            rowLabels = labelsList.ToArray();
            rowTooltips = tooltipsList.ToArray();

            // Buttons
            int buttonY = ButtonBoxBounds.Y + (ConfigButtonBoxHeight - TabAndButtonHeight) / 2;
            int maxButtonWidth = (Width - ConfigBottomButtonGap) / 2;
            int saveWidth = UIHelpers.CalculateButtonWidth(TranslationCache.ConfigSave, maxButtonWidth);
            int closeWidth = UIHelpers.CalculateButtonWidth(TranslationCache.ConfigClose, maxButtonWidth);
            int totalBtnWidth = saveWidth + closeWidth + ConfigBottomButtonGap;
            int btnStartX = X + (Width - totalBtnWidth) / 2;

            SaveButton = new ClickableComponent(
                new Rectangle(btnStartX, buttonY, saveWidth, TabAndButtonHeight),
                "save"
            );
            CloseMenuButton = new ClickableComponent(
                new Rectangle(btnStartX + saveWidth + ConfigBottomButtonGap, buttonY, closeWidth, TabAndButtonHeight),
                "close"
            );

            // Close X button
            CloseButton = new ClickableTextureComponent(
                new Rectangle(X + Width - CloseButtonSize - CloseButtonEdgeMargin,
                    Y + CloseButtonEdgeMargin, CloseButtonSize, CloseButtonSize),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );
        }

        private void BuildSearchScopeOptions()
        {
            SearchScopeOptions.Clear();
            string[] labels = {
                TranslationCache.SearchScopeAll,
                TranslationCache.SearchScopeName,
                TranslationCache.SearchScopeLocation
            };

            int baseY = searchScopePanelAnchor.Bottom + ConfigDropdownTopPadding;
            for (int i = 0; i < labels.Length; i++)
            {
                SearchScopeOptions.Add(new ClickableComponent(
                    new Rectangle(searchScopePanelAnchor.X, baseY + i * ConfigRowHeight,
                        searchScopePanelAnchor.Width, ConfigRowHeight),
                    labels[i]
                ) { label = labels[i] });
            }
        }

        public void UpdateSearchScopePanelPosition(string displayValue)
        {
            int valueW = (int)Game1.smallFont.MeasureString(displayValue).X;
            int panelX = controlX - valueW / 2 - searchScopePanelW / 2;
            searchScopePanelAnchor = new Rectangle(panelX, searchScopePanelAnchor.Y, searchScopePanelW, ConfigRowHeight);
            BuildSearchScopeOptions();
        }

        private ClickableComponent CreateCheckbox(int rowY, string name)
        {
            int checkY = rowY + (ConfigRowHeight - ConfigCheckboxSize) / 2;
            return new ClickableComponent(
                new Rectangle(controlX - ConfigCheckboxSize, checkY, ConfigCheckboxSize, ConfigCheckboxSize),
                name
            );
        }

        // --- Drawing ---

        public void DrawTitle(SpriteBatch b)
        {
            string title = TranslationCache.ConfigTitle;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont,
                new Vector2(TitleBoxBounds.X + (TitleBoxBounds.Width - titleSize.X) / 2,
                    TitleBoxBounds.Y + ConfigTitleTopPadding),
                Game1.textColor);
        }

        public void DrawCheckboxRow(SpriteBatch b, string label, bool isChecked, ClickableComponent checkbox)
        {
            int rowY = checkbox.bounds.Y - (ConfigRowHeight - ConfigCheckboxSize) / 2;
            DrawRowLabel(b, label, rowY);

            Rectangle sourceRect = isChecked ? UIHelpers.CheckedSourceRect : UIHelpers.UncheckedSourceRect;
            b.Draw(Game1.mouseCursors,
                new Vector2(checkbox.bounds.X, checkbox.bounds.Y),
                sourceRect, Color.White, 0f, Vector2.Zero, ConfigCheckboxScale, SpriteEffects.None, 1f);
        }

        public void DrawSearchScopeRow(SpriteBatch b, string label, string displayValue, bool isOpen)
        {
            int rowY = SearchScopeDropdown.bounds.Y;
            DrawRowLabel(b, label, rowY);

            Vector2 textSize = Game1.smallFont.MeasureString(displayValue);
            float textX = controlX - textSize.X;
            float textY = rowY + (ConfigRowHeight - textSize.Y) / 2;
            Vector2 textPos = new Vector2(textX, textY);

            bool isHovered = !isOpen && SearchScopeDropdown.containsPoint(Game1.getMouseX(), Game1.getMouseY());

            if (isHovered)
            {
                Utility.drawTextWithShadow(b, displayValue, Game1.smallFont, textPos + new Vector2(-1, 0), Game1.textColor * 0.8f);
                Utility.drawTextWithShadow(b, displayValue, Game1.smallFont, textPos, Game1.textColor);
            }
            else
            {
                Utility.drawTextWithShadow(b, displayValue, Game1.smallFont, textPos, Game1.textColor);
            }
        }

        public void DrawSearchScopeDropdownOptions(SpriteBatch b, string selectedLabel)
        {
            UIHelpers.DrawDropdownOptions(
                b,
                searchScopePanelAnchor,
                SearchScopeOptions,
                firstVisibleIndex: 0,
                maxVisibleItems: ConfigDropdownMaxVisible,
                isSelected: option => option.name == selectedLabel,
                enableTruncation: true,
                panelPaddingV: ConfigDropdownTopPadding
            );
        }

        public void DrawButtons(SpriteBatch b)
        {
            UIHelpers.DrawTextButton(b, SaveButton, TranslationCache.ConfigSave);
            UIHelpers.DrawTextButton(b, CloseMenuButton, TranslationCache.ConfigClose);
        }

        public void DrawCloseButton(SpriteBatch b)
        {
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            float baseScale = CloseButtonSize / 12f;
            float scale = CloseButton.containsPoint(mouseX, mouseY) ? baseScale * 1.1f : baseScale;
            var sourceRect = new Rectangle(337, 494, 12, 12);
            Vector2 center = new Vector2(
                CloseButton.bounds.X + CloseButton.bounds.Width / 2,
                CloseButton.bounds.Y + CloseButton.bounds.Height / 2);
            b.Draw(Game1.mouseCursors, center, sourceRect, Color.White, 0f, new Vector2(6, 6), scale, SpriteEffects.None, 1f);
        }

        public string? GetHoveredTooltip(int mouseX, int mouseY)
        {
            int labelX = contentX + ConfigRowIndent;
            for (int i = 0; i < rowYPositions.Length; i++)
            {
                int labelWidth = (int)Game1.smallFont.MeasureString(rowLabels[i]).X;
                Rectangle labelBounds = new Rectangle(labelX, rowYPositions[i], labelWidth, ConfigRowHeight);
                if (labelBounds.Contains(mouseX, mouseY))
                    return rowTooltips[i];
            }
            return null;
        }

        private void DrawRowLabel(SpriteBatch b, string label, int rowY)
        {
            float textHeight = Game1.smallFont.MeasureString(label).Y;
            int textY = rowY + (int)((ConfigRowHeight - textHeight) / 2);
            Utility.drawTextWithShadow(b, label, Game1.smallFont,
                new Vector2(contentX + ConfigRowIndent, textY), Game1.textColor);
        }

        public static string FormatSearchScope(Models.WarpSearchScope scope)
        {
            return scope switch
            {
                Models.WarpSearchScope.All => TranslationCache.SearchScopeAll,
                Models.WarpSearchScope.Name => TranslationCache.SearchScopeName,
                Models.WarpSearchScope.Location => TranslationCache.SearchScopeLocation,
                _ => TranslationCache.SearchScopeAll
            };
        }
    }
}
