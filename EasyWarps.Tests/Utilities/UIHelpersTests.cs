using Microsoft.Xna.Framework;
using EasyWarps;
using Xunit;

namespace EasyWarps.Tests.Utilities
{
    public class UIHelpersTests
    {
        private static readonly Func<string, float> FixedCharWidth = s => s.Length * 10f;

        #region TruncateText

        // Expected: Text that fits within maxWidth is returned unchanged
        [Fact]
        public void TruncateText_FitsWithinMax_ReturnsUnchanged()
        {
            var result = UIHelpers.TruncateText("Hello", 100, FixedCharWidth);

            Assert.Equal("Hello", result);
        }

        // Expected: Text exceeding maxWidth is truncated with "..." suffix
        [Fact]
        public void TruncateText_ExceedsMax_AddEllipsis()
        {
            var result = UIHelpers.TruncateText("Hello World!", 80, FixedCharWidth);

            Assert.EndsWith("...", result);
            Assert.True(FixedCharWidth(result) <= 80);
        }

        // Expected: Text exactly at maxWidth is returned unchanged
        [Fact]
        public void TruncateText_ExactlyAtMax_ReturnsUnchanged()
        {
            var result = UIHelpers.TruncateText("12345", 50, FixedCharWidth);

            Assert.Equal("12345", result);
        }

        // Expected: Empty string returns empty string
        [Fact]
        public void TruncateText_EmptyString_ReturnsEmpty()
        {
            var result = UIHelpers.TruncateText("", 100, FixedCharWidth);

            Assert.Equal("", result);
        }

        // Expected: Very narrow maxWidth still produces "..." at minimum
        [Fact]
        public void TruncateText_VeryNarrowMax_ReturnsEllipsisOnly()
        {
            var result = UIHelpers.TruncateText("Hello World", 30, FixedCharWidth);

            Assert.Equal("...", result);
        }

        #endregion

        #region TrimTextFromStart

        // Expected: Text that fits returns unchanged
        [Fact]
        public void TrimTextFromStart_Fits_ReturnsUnchanged()
        {
            var result = UIHelpers.TrimTextFromStart("Hello", 100, FixedCharWidth);

            Assert.Equal("Hello", result);
        }

        // Expected: Text exceeding max trims characters from start
        [Fact]
        public void TrimTextFromStart_ExceedsMax_TrimsFromStart()
        {
            var result = UIHelpers.TrimTextFromStart("Hello World", 50, FixedCharWidth);

            Assert.Equal("World", result);
            Assert.True(FixedCharWidth(result) <= 50);
        }

        // Expected: Null input returns empty string
        [Fact]
        public void TrimTextFromStart_Null_ReturnsEmpty()
        {
            var result = UIHelpers.TrimTextFromStart(null!, 100, FixedCharWidth);

            Assert.Equal("", result);
        }

        // Expected: Empty string returns empty string
        [Fact]
        public void TrimTextFromStart_Empty_ReturnsEmpty()
        {
            var result = UIHelpers.TrimTextFromStart("", 100, FixedCharWidth);

            Assert.Equal("", result);
        }

        #endregion

        #region CalculateInputBarMaxTextWidth

        // Expected: Without clear button, reserves InputBarTextStartX on both sides
        [Fact]
        public void CalculateInputBarMaxTextWidth_NoClearButton()
        {
            int result = UIHelpers.CalculateInputBarMaxTextWidth(300, hasClearButton: false);

            int expected = 300 - WarpLayoutConstants.InputBarTextStartX - WarpLayoutConstants.InputBarTextStartX;
            Assert.Equal(expected, result);
        }

        // Expected: With clear button, reserves ClearButtonSize + margin on right
        [Fact]
        public void CalculateInputBarMaxTextWidth_WithClearButton()
        {
            int result = UIHelpers.CalculateInputBarMaxTextWidth(300, hasClearButton: true);

            int rightReserved = WarpLayoutConstants.ClearButtonSize + WarpLayoutConstants.ClearButtonRightMargin;
            int expected = 300 - WarpLayoutConstants.InputBarTextStartX - rightReserved;
            Assert.Equal(expected, result);
        }

        #endregion

        #region CalculateDropdownArrowX

        // Expected: Arrow X is positioned left of anchor right edge with padding
        [Fact]
        public void CalculateDropdownArrowX_PositionsLeftOfAnchor()
        {
            int anchorRight = 500;

            int result = UIHelpers.CalculateDropdownArrowX(anchorRight);

            int arrowW = (int)(UIHelpers.UpScrollArrowSourceRect.Width * WarpLayoutConstants.DropdownArrowScale);
            int expected = anchorRight - arrowW - WarpLayoutConstants.DropdownArrowPad + 2;
            Assert.Equal(expected, result);
        }

        // Expected: arrowXNudge shifts arrow left
        [Fact]
        public void CalculateDropdownArrowX_WithNudge_ShiftsLeft()
        {
            int withoutNudge = UIHelpers.CalculateDropdownArrowX(500, 0);
            int withNudge = UIHelpers.CalculateDropdownArrowX(500, 10);

            Assert.Equal(withoutNudge - 10, withNudge);
        }

        #endregion

        #region CalculateDropdownUpArrowY

        // Expected: Up arrow Y is positioned below anchor bottom with padding
        [Fact]
        public void CalculateDropdownUpArrowY_PositionsBelowAnchor()
        {
            int result = UIHelpers.CalculateDropdownUpArrowY(200);

            int expected = 200 + WarpLayoutConstants.DropdownArrowPad - WarpLayoutConstants.DropdownArrowNudge + 4;
            Assert.Equal(expected, result);
        }

        // Expected: panelPaddingV shifts arrow down
        [Fact]
        public void CalculateDropdownUpArrowY_WithPanelPadding()
        {
            int without = UIHelpers.CalculateDropdownUpArrowY(200, 0);
            int with = UIHelpers.CalculateDropdownUpArrowY(200, 10);

            Assert.Equal(without + 10, with);
        }

        #endregion

        #region CalculateDropdownDownArrowY

        // Expected: Down arrow Y is positioned near bottom of dropdown panel
        [Fact]
        public void CalculateDropdownDownArrowY_PositionsNearBottom()
        {
            int result = UIHelpers.CalculateDropdownDownArrowY(200, 160);

            int arrowH = (int)(UIHelpers.UpScrollArrowSourceRect.Height * WarpLayoutConstants.DropdownArrowScale);
            int expected = 200 + 160 - arrowH - WarpLayoutConstants.DropdownArrowPad + WarpLayoutConstants.DropdownArrowNudge - 4;
            Assert.Equal(expected, result);
        }

        // Expected: Larger dropdown height pushes arrow further down
        [Fact]
        public void CalculateDropdownDownArrowY_LargerHeight_PushesDown()
        {
            int small = UIHelpers.CalculateDropdownDownArrowY(200, 100);
            int large = UIHelpers.CalculateDropdownDownArrowY(200, 200);

            Assert.True(large > small);
            Assert.Equal(100, large - small);
        }

        #endregion

        #region CalculateDropdownButtonMaxTextWidth

        // Expected: Without clear button, reserves 20px on each side
        [Fact]
        public void CalculateDropdownButtonMaxTextWidth_NoClearButton()
        {
            int result = UIHelpers.CalculateDropdownButtonMaxTextWidth(300, hasClearButton: false);

            Assert.Equal(300 - 20 - 20, result);
        }

        // Expected: With clear button, reserves ClearButtonSize + margin on right
        [Fact]
        public void CalculateDropdownButtonMaxTextWidth_WithClearButton()
        {
            int result = UIHelpers.CalculateDropdownButtonMaxTextWidth(300, hasClearButton: true);

            int reservedRight = WarpLayoutConstants.ClearButtonSize + WarpLayoutConstants.ClearButtonRightMargin;
            Assert.Equal(300 - reservedRight - 20, result);
        }

        #endregion

        #region ClampScrollOffset

        // Expected: Negative offset clamps to 0
        [Fact]
        public void ClampScrollOffset_Negative_ClampsToZero()
        {
            Assert.Equal(0, UIHelpers.ClampScrollOffset(-5, 10, 6));
        }

        // Expected: Offset beyond max clamps to totalItems - visibleItems
        [Fact]
        public void ClampScrollOffset_BeyondMax_ClampsToMax()
        {
            Assert.Equal(4, UIHelpers.ClampScrollOffset(10, 10, 6));
        }

        // Expected: Valid offset within range returns unchanged
        [Fact]
        public void ClampScrollOffset_WithinRange_ReturnsUnchanged()
        {
            Assert.Equal(3, UIHelpers.ClampScrollOffset(3, 10, 6));
        }

        // Expected: When totalItems <= visibleItems, clamps to 0
        [Fact]
        public void ClampScrollOffset_NoScrollNeeded_ReturnsZero()
        {
            Assert.Equal(0, UIHelpers.ClampScrollOffset(5, 4, 6));
        }

        // Expected: Zero items returns 0
        [Fact]
        public void ClampScrollOffset_ZeroItems_ReturnsZero()
        {
            Assert.Equal(0, UIHelpers.ClampScrollOffset(0, 0, 6));
        }

        #endregion

        #region GetVisualCenter

        // Expected: Centers content within bounds with vertical offset
        [Fact]
        public void GetVisualCenter_CentersWithOffset()
        {
            var bounds = new Rectangle(100, 200, 200, 100);
            var contentSize = new Vector2(80, 40);

            var result = UIHelpers.GetVisualCenter(bounds, contentSize);

            Assert.Equal(100 + (200 - 80) / 2, result.X);
            Assert.Equal(200 + (100 - 40) / 2 + WarpLayoutConstants.TextureBoxVisualOffsetY, result.Y);
        }

        // Expected: Content same size as bounds returns top-left with offset
        [Fact]
        public void GetVisualCenter_ContentSameAsBounds()
        {
            var bounds = new Rectangle(50, 50, 100, 100);
            var contentSize = new Vector2(100, 100);

            var result = UIHelpers.GetVisualCenter(bounds, contentSize);

            Assert.Equal(50f, result.X);
            Assert.Equal(50 + WarpLayoutConstants.TextureBoxVisualOffsetY, result.Y);
        }

        #endregion

        #region CalculateButtonWidth

        // Expected: Short text uses minimum TabAndButtonWidth
        [Fact]
        public void CalculateButtonWidth_ShortText_UsesMinimum()
        {
            var result = UIHelpers.CalculateButtonWidth("OK", int.MaxValue, FixedCharWidth);

            Assert.Equal(WarpLayoutConstants.TabAndButtonWidth, result);
        }

        // Expected: Long text expands beyond minimum
        [Fact]
        public void CalculateButtonWidth_LongText_ExpandsBeyondMin()
        {
            var result = UIHelpers.CalculateButtonWidth("Very Long Button Label", int.MaxValue, FixedCharWidth);

            int expected = (int)FixedCharWidth("Very Long Button Label") + WarpLayoutConstants.TextPadding * 2;
            Assert.Equal(expected, result);
        }

        // Expected: maxWidth caps the button width
        [Fact]
        public void CalculateButtonWidth_MaxWidthCaps()
        {
            var result = UIHelpers.CalculateButtonWidth("Very Long Button Label", 150, FixedCharWidth);

            Assert.Equal(150, result);
        }

        // Expected: maxWidth below minimum still returns minimum
        [Fact]
        public void CalculateButtonWidth_MaxWidthBelowMin_ReturnsMin()
        {
            var result = UIHelpers.CalculateButtonWidth("OK", 50, FixedCharWidth);

            Assert.Equal(50, result);
        }

        #endregion

        #region CalculateDeleteDialogLayout

        // Expected: Dialog is centered in viewport
        [Fact]
        public void CalculateDeleteDialogLayout_CenteredInViewport()
        {
            var (dialog, yes, no) = UIHelpers.CalculateDeleteDialogLayout(
                "Delete?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);

            Assert.Equal((800 - dialog.Width) / 2, dialog.X);
            Assert.Equal((600 - dialog.Height) / 2, dialog.Y);
        }

        // Expected: Yes button is left of No button
        [Fact]
        public void CalculateDeleteDialogLayout_YesLeftOfNo()
        {
            var (_, yes, no) = UIHelpers.CalculateDeleteDialogLayout(
                "Delete?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);

            Assert.True(yes.X < no.X);
        }

        // Expected: Buttons are same height (TabAndButtonHeight)
        [Fact]
        public void CalculateDeleteDialogLayout_ButtonsSameHeight()
        {
            var (_, yes, no) = UIHelpers.CalculateDeleteDialogLayout(
                "Delete?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);

            Assert.Equal(WarpLayoutConstants.TabAndButtonHeight, yes.Height);
            Assert.Equal(WarpLayoutConstants.TabAndButtonHeight, no.Height);
        }

        // Expected: Buttons are at same Y position
        [Fact]
        public void CalculateDeleteDialogLayout_ButtonsSameY()
        {
            var (_, yes, no) = UIHelpers.CalculateDeleteDialogLayout(
                "Delete?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);

            Assert.Equal(yes.Y, no.Y);
        }

        // Expected: Buttons are within dialog bounds
        [Fact]
        public void CalculateDeleteDialogLayout_ButtonsWithinDialog()
        {
            var (dialog, yes, no) = UIHelpers.CalculateDeleteDialogLayout(
                "Delete?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);

            Assert.True(yes.X >= dialog.X);
            Assert.True(no.Right <= dialog.Right);
            Assert.True(yes.Y >= dialog.Y);
            Assert.True(no.Bottom <= dialog.Bottom);
        }

        // Expected: Wider question text widens dialog
        [Fact]
        public void CalculateDeleteDialogLayout_WiderQuestion_WidensDialog()
        {
            var (narrow, _, _) = UIHelpers.CalculateDeleteDialogLayout(
                "OK?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);
            var (wide, _, _) = UIHelpers.CalculateDeleteDialogLayout(
                "Are you sure you want to delete this very long item?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);

            Assert.True(wide.Width >= narrow.Width);
        }

        // Expected: Dialog re-centers when viewport changes
        [Fact]
        public void CalculateDeleteDialogLayout_DifferentViewport_ReCenters()
        {
            var (small, _, _) = UIHelpers.CalculateDeleteDialogLayout(
                "Delete?", "Yes", "No", 800, 600,
                FixedCharWidth, FixedCharWidth);
            var (large, _, _) = UIHelpers.CalculateDeleteDialogLayout(
                "Delete?", "Yes", "No", 1920, 1080,
                FixedCharWidth, FixedCharWidth);

            Assert.Equal((800 - small.Width) / 2, small.X);
            Assert.Equal((1920 - large.Width) / 2, large.X);
            Assert.Equal(small.Width, large.Width);
        }

        #endregion

        #region TruncateNameAndLocation

        // Expected: Both name and location fit — returned unchanged
        [Fact]
        public void TruncateNameAndLocation_BothFit_ReturnsUnchanged()
        {
            var (name, loc) = UIHelpers.TruncateNameAndLocation("Home", "Farm", 200, 20, FixedCharWidth);

            Assert.Equal("Home", name);
            Assert.Equal("Farm", loc);
        }

        // Expected: Both exceed 50% — each truncated to half
        [Fact]
        public void TruncateNameAndLocation_BothExceed50Pct_TruncatesBoth()
        {
            // totalAvail=100, sep=20, textAvail=80, half=40
            // "LongNameHere"=120px, "LongLocationHere"=160px — both > 40
            var (name, loc) = UIHelpers.TruncateNameAndLocation(
                "LongNameHere", "LongLocationHere", 100, 20, FixedCharWidth);

            Assert.EndsWith("...", name);
            Assert.EndsWith("...", loc);
            Assert.True(FixedCharWidth(name) <= 40);
            Assert.True(FixedCharWidth(loc) <= 40);
        }

        // Expected: Name short, location long — only location truncated to remaining space
        [Fact]
        public void TruncateNameAndLocation_NameShort_OnlyLocTruncated()
        {
            // totalAvail=200, sep=20, textAvail=180, half=90
            // "Hi"=20px (< 90), "VeryLongLocationNameHere"=240px (> 90)
            var (name, loc) = UIHelpers.TruncateNameAndLocation(
                "Hi", "VeryLongLocationNameHere", 200, 20, FixedCharWidth);

            Assert.Equal("Hi", name);
            Assert.EndsWith("...", loc);
            Assert.True(FixedCharWidth(loc) <= 160); // textAvail - rawNameW = 180 - 20
        }

        // Expected: Location short, name long — only name truncated to remaining space
        [Fact]
        public void TruncateNameAndLocation_LocShort_OnlyNameTruncated()
        {
            // totalAvail=200, sep=20, textAvail=180, half=90
            // "VeryLongWarpPointNameHere"=250px (> 90), "OK"=20px (< 90)
            var (name, loc) = UIHelpers.TruncateNameAndLocation(
                "VeryLongWarpPointNameHere", "OK", 200, 20, FixedCharWidth);

            Assert.EndsWith("...", name);
            Assert.Equal("OK", loc);
            Assert.True(FixedCharWidth(name) <= 160); // textAvail - rawLocW = 180 - 20
        }

        // Expected: Both exactly at 50% — returned unchanged (total fits)
        [Fact]
        public void TruncateNameAndLocation_BothExactlyAtHalf_ReturnsUnchanged()
        {
            // totalAvail=120, sep=20, textAvail=100
            // "Hello"=50px, "World"=50px, total=50+20+50=120 <= 120
            var (name, loc) = UIHelpers.TruncateNameAndLocation(
                "Hello", "World", 120, 20, FixedCharWidth);

            Assert.Equal("Hello", name);
            Assert.Equal("World", loc);
        }

        #endregion
    }
}
