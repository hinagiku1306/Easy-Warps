using Microsoft.Xna.Framework;

namespace EasyWarps
{
    public static class WarpLayoutConstants
    {
        // Menu Dimensions
        public const int MenuWidth = 650;
        public const int BorderPadding = 40;
        public const int ContentBoxPadding = 16;

        // Tabs
        public const int TabGap = 8;

        // Buttons & Tabs
        public const int TabAndButtonWidth = 110;
        public const int TabAndButtonHeight = 60;
        public const int TextPadding = 15;
        public const float TabOpacity = 0.2f;

        // List
        public const int MaxVisibleRows = 6;
        public const int ListItemHeight = 48;
        public const int ListItemLeftMargin = 8;
        public const int ListIconGap = 10;
        public const int ListIconToTextGap = 10;

        // Scroll Arrows (right side of list)
        public const int ScrollArrowButtonSize = 44;
        public const float ListScrollArrowScale = 2.5f;
        public const int ScrollArrowRightPadding = 10;

        // Search & Filter Row
        public const int FilterRowHeight = 44;
        public const int FilterRowGap = 8;
        public const int FilterDropdownWidth = 200;
        public const int FilterTextPadding = 12;

        // Input Bar
        public const int InputBarTextStartX = 20;
        public const int InputBarCaretWidth = 4;

        // Clear Button
        public const int ClearButtonSize = 24;
        public const int ClearButtonRightMargin = 12;

        // Dropdown Arrows
        public const int DropdownArrowPad = 8;
        public const float DropdownArrowScale = 1.5f;
        public const int DropdownArrowNudge = 4;
        public const int DropdownMaxVisible = 5;
        public const int DropdownOptionHeight = 40;
        public const int DropdownPanelPadding = 5;

        // Close Button
        public const int CloseButtonSize = 48;
        public const int CloseButtonEdgeMargin = 8;

        // Bottom Buttons
        public const int BottomButtonGap = 16;
        public const int ButtonBoxHeight = 70;

        // Config Gear
        public const int ConfigGearButtonSize = 40;
        public const int ConfigGearButtonGap = 5;

        // Hover & Effects
        public const float ButtonHoveringScale = 1.05f;
        public static readonly Color HoverEffectColor = Color.Wheat * 0.3f;
        public const float BackgroundOverlayOpacity = 0.6f;

        // Texture Box
        public const int TextureBoxVisualOffsetY = 2;
        public static readonly Rectangle MenuBoxSourceRect = new Rectangle(0, 256, 60, 60);

        // Checkbox
        public const float CheckboxScale = 3.2f;
        public const int CheckboxSize = 29;

        // Config Overlay
        public const int ConfigOverlayWidth = 650;
        public const int ConfigBorderPadding = 30;
        public const int ConfigRowHeight = 44;
        public const int ConfigSectionHeaderHeight = 44;
        public const int ConfigSectionGap = 20;
        public const int ConfigRowIndent = 20;
        public const int ConfigLabelWidth = 350;
        public const int ConfigControlRightPadding = 30;
        public const float ConfigCheckboxScale = 3.2f;
        public const int ConfigCheckboxSize = 29;
        public const int ConfigDropdownMinWidth = 85;
        public const int ConfigDropdownMaxWidth = 200;
        public const int ConfigDropdownTopPadding = 2;
        public const int ConfigDropdownHeight = 40;
        public const int ConfigDropdownMaxVisible = 3;
        public const int ConfigBottomButtonGap = 16;
        public const int ConfigMaxHeight = 700;
        public const float ConfigScrollArrowScale = 2f;
        public const int ConfigScrollArrowRightExtend = 5;
        public const int ConfigTitleTopPadding = 15;
        public const int ConfigTitleBottomPadding = 10;
        public const int ConfigTitleSidePadding = 30;
        public const int ConfigButtonBoxHeight = 70;
        public const int ConfigBoxGap = 10;

        // Title Box
        public const int TitleTopPadding = 15;
        public const int TitleBottomPadding = 10;
        public const int TitleSidePadding = 30;

        // Tooltip
        public const int TooltipPadding = 16;

        // Favorite Star
        public const int FavoriteStarSize = 28;

        // Row Action Buttons (edit gear, delete X)
        public const int RowActionButtonSize = 20;
        public const int RowActionButtonGap = 8;

        // Sign Edit Menu (vanilla TitleTextInputMenu layout reconstruction)
        public const int SignTextMaxLength = 60;
        public const int SignEditCheckboxBelowTextBox = 60;
        public const int SignEditCheckboxLabelGap = 10;
        public const int SignEditTextBoxOffsetX = 320;
        public const int SignEditDoneButtonOffsetX = 228;
        public const int SignEditPasteButtonOffsetX = 292;
        public const int SignEditButtonOffsetY = 8;
    }
}
