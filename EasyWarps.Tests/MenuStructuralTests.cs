using System.Reflection;
using Xunit;

namespace EasyWarps.Tests
{
    public class MenuStructuralTests
    {
        private static readonly Type[] AllMenuTypes = new[]
        {
            typeof(EasyWarps.UI.WarpMenu),
            typeof(EasyWarps.UI.ConfigOverlay),
            typeof(EasyWarps.UI.SignEditMenu),
        };

        // Expected: All menus override gameWindowSizeChanged for resize anchoring
        [Theory]
        [MemberData(nameof(GetMenuTypes))]
        public void Menu_OverridesGameWindowSizeChanged(Type menuType)
        {
            var method = menuType.GetMethod("gameWindowSizeChanged",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                null, new[] { typeof(Microsoft.Xna.Framework.Rectangle), typeof(Microsoft.Xna.Framework.Rectangle) }, null);

            Assert.NotNull(method);
        }

        // Expected: WarpMenu has Recalculate called inside gameWindowSizeChanged
        [Fact]
        public void WarpMenu_GameWindowSizeChanged_CallsRecalculate()
        {
            var source = ReadSourceFile("UI/WarpMenu.cs");

            Assert.Contains("gameWindowSizeChanged", source);
            Assert.Contains("Recalculate", source);
        }

        // Expected: ConfigOverlay has Recalculate called inside gameWindowSizeChanged
        [Fact]
        public void ConfigOverlay_GameWindowSizeChanged_CallsRecalculate()
        {
            var source = ReadSourceFile("UI/ConfigOverlay.cs");

            Assert.Contains("gameWindowSizeChanged", source);
            Assert.Contains("Recalculate", source);
        }

        // Expected: SignEditMenu recalculates checkbox layout on resize
        [Fact]
        public void SignEditMenu_GameWindowSizeChanged_RecalculatesCheckbox()
        {
            var source = ReadSourceFile("UI/SignEditMenu.cs");

            Assert.Contains("RecalculateCheckboxLayout", source);
            Assert.Contains("gameWindowSizeChanged", source);
        }

        // Expected: WarpMenu recalculates delete dialog on resize when showing
        [Fact]
        public void WarpMenu_GameWindowSizeChanged_RecalculatesDeleteDialog()
        {
            var source = ReadSourceFile("UI/WarpMenu.cs");
            var resizeSection = ExtractMethod(source, "gameWindowSizeChanged");

            Assert.Contains("RecalculateDeleteDialog", resizeSection);
        }

        // Expected: WarpMenu handles close-on-click-outside
        [Fact]
        public void WarpMenu_ReceiveLeftClick_ClosesOnOutsideBounds()
        {
            var source = ReadSourceFile("UI/WarpMenu.cs");
            var clickSection = ExtractMethod(source, "receiveLeftClick");

            Assert.Contains("isWithinBounds", clickSection);
        }

        // Expected: ConfigOverlay handles close-on-click-outside
        [Fact]
        public void ConfigOverlay_ReceiveLeftClick_ClosesOnOutsideBounds()
        {
            var source = ReadSourceFile("UI/ConfigOverlay.cs");
            var clickSection = ExtractMethod(source, "receiveLeftClick");

            Assert.Contains("isWithinBounds", clickSection);
        }

        // Expected: WarpMenu overrides isWithinBounds
        [Fact]
        public void WarpMenu_OverridesIsWithinBounds()
        {
            var method = typeof(EasyWarps.UI.WarpMenu).GetMethod("isWithinBounds",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.NotNull(method);
        }

        // Expected: WarpMenu overrides readyToClose
        [Fact]
        public void WarpMenu_OverridesReadyToClose()
        {
            var method = typeof(EasyWarps.UI.WarpMenu).GetMethod("readyToClose",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.NotNull(method);
        }

        // Expected: WarpMenu has receiveScrollWheelAction for dropdown/list scrolling
        [Fact]
        public void WarpMenu_OverridesReceiveScrollWheelAction()
        {
            var method = typeof(EasyWarps.UI.WarpMenu).GetMethod("receiveScrollWheelAction",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.NotNull(method);
        }

        // --- Bug regression: search clear button must be checked before search bar ---

        // Expected: In normal click flow, SearchClearButton check comes before SearchBar check
        [Fact]
        public void WarpMenu_ReceiveLeftClick_ClearButtonBeforeSearchBar()
        {
            var source = ReadSourceFile("UI/WarpMenu.cs");
            var clickMethod = ExtractMethod(source, "receiveLeftClick");

            int clearIdx = clickMethod.IndexOf("SearchClearButton.containsPoint", StringComparison.Ordinal);
            int barIdx = clickMethod.IndexOf("SearchBar.containsPoint", StringComparison.Ordinal);

            Assert.True(clearIdx >= 0, "SearchClearButton check not found in receiveLeftClick");
            Assert.True(barIdx >= 0, "SearchBar check not found in receiveLeftClick");
            Assert.True(clearIdx < barIdx, "SearchClearButton must be checked before SearchBar to prevent click interception");
        }

        // Expected: In dropdown-open click flow, SearchClearButton check comes before SearchBar check
        [Fact]
        public void WarpMenu_HandleDropdownOpenClick_ClearButtonBeforeSearchBar()
        {
            var source = ReadSourceFile("UI/WarpMenu.cs");
            var method = ExtractMethod(source, "void HandleDropdownOpenClick");

            int clearIdx = method.IndexOf("SearchClearButton.containsPoint", StringComparison.Ordinal);
            int barIdx = method.IndexOf("SearchBar.containsPoint", StringComparison.Ordinal);

            Assert.True(clearIdx >= 0, "SearchClearButton check not found in HandleDropdownOpenClick");
            Assert.True(barIdx >= 0, "SearchBar check not found in HandleDropdownOpenClick");
            Assert.True(clearIdx < barIdx, "SearchClearButton must be checked before SearchBar in dropdown-open path");
        }

        // --- Bug regression: editNameOnly should return to WarpMenu, not exit ---

        // Expected: SignEditMenu.OnDoneNaming creates WarpMenu when editNameOnly is true
        [Fact]
        public void SignEditMenu_OnDoneNaming_ReturnsToWarpMenuInEditNameOnly()
        {
            var source = ReadSourceFile("UI/SignEditMenu.cs");
            var method = ExtractMethod(source, "void OnDoneNaming");

            Assert.Contains("editNameOnly", method);
            Assert.Contains("new WarpMenu", method);
        }

        // Expected: SignEditMenu skips signObj.signText update in editNameOnly mode
        [Fact]
        public void SignEditMenu_OnDoneNaming_SkipsSignTextUpdateInEditNameOnly()
        {
            var source = ReadSourceFile("UI/SignEditMenu.cs");
            var method = ExtractMethod(source, "void OnDoneNaming");

            Assert.Contains("!editNameOnly", method);
            Assert.Contains("signObj.signText.Value", method);
        }

        // --- Bug regression: edit button must use warp point name, not sign text ---

        // Expected: SignEditMenu constructor uses existingWarp.Name for editNameOnly mode
        [Fact]
        public void SignEditMenu_Constructor_UsesExistingWarpNameForEditNameOnly()
        {
            var source = ReadSourceFile("UI/SignEditMenu.cs");

            Assert.Contains("editNameOnly && existingWarp != null ? existingWarp.Name", source);
        }

        // --- Bug regression: deleted warp should not auto-check the checkbox ---

        // Expected: isWarpChecked only auto-checks when sign text is empty (truly new signs)
        [Fact]
        public void SignEditMenu_Constructor_OnlyAutoChecksForEmptySigns()
        {
            var source = ReadSourceFile("UI/SignEditMenu.cs");

            Assert.Contains("string.IsNullOrEmpty(signObj.signText.Value)", source);
            Assert.Contains("AlwaysRegisterAsWarpPoint", source);
        }

        // --- Bug regression: empty warp name allowed ---

        // Expected: SignEditMenu allows warp creation with empty name (no minLength guard)
        [Fact]
        public void SignEditMenu_NoMinLengthConstraint()
        {
            var source = ReadSourceFile("UI/SignEditMenu.cs");

            Assert.DoesNotContain("minLength", source);
        }

        // Expected: SignEditMenu creates warp point without requiring non-empty text
        [Fact]
        public void SignEditMenu_OnDoneNaming_AllowsEmptyNameWarp()
        {
            var source = ReadSourceFile("UI/SignEditMenu.cs");
            var method = ExtractMethod(source, "void OnDoneNaming");

            // The warp creation guard should check only isWarpChecked, not string content
            Assert.Contains("if (isWarpChecked)", method);
            Assert.DoesNotContain("isWarpChecked && !string.IsNullOrEmpty", method);
        }

        // --- Bug regression: adaptive truncation in list rows ---

        // Expected: WarpMenuUIBuilder uses TruncateNameAndLocation for adaptive truncation
        [Fact]
        public void WarpMenuUIBuilder_DrawList_UsesAdaptiveTruncation()
        {
            var source = ReadSourceFile("UI/WarpMenuUIBuilder.cs");
            var method = ExtractMethod(source, "void DrawList");

            Assert.Contains("TruncateNameAndLocation", method);
        }

        // --- Bug regression: scroll arrows outside content box ---

        // Expected: Scroll arrows positioned using ContentBox.Right (outside content area)
        [Fact]
        public void WarpMenuUIBuilder_ScrollArrows_OutsideContentBox()
        {
            var source = ReadSourceFile("UI/WarpMenuUIBuilder.cs");

            Assert.Contains("ContentBox.Right", source);
            Assert.DoesNotContain("cx + cw - ListArrowColumnWidth", source);
        }

        // --- Bug regression: tab width uses fixed TabAndButtonWidth ---

        // Expected: Tab width uses TabAndButtonWidth constant, not dynamic MeasureString
        [Fact]
        public void WarpMenuUIBuilder_TabWidth_UsesFixedConstant()
        {
            var source = ReadSourceFile("UI/WarpMenuUIBuilder.cs");
            var method = ExtractMethod(source, "void CalculateLayout");

            Assert.Contains("TabAndButtonWidth", method);
            Assert.DoesNotContain("MeasureString(TranslationCache.TabWorld)", method);
        }

        // --- Bug regression: WarpMenu uses FavoritesCheckbox, not filter dropdown ---

        // Expected: WarpMenu has no filter dropdown state field
        [Fact]
        public void WarpMenu_NoFilterDropdownState()
        {
            var source = ReadSourceFile("UI/WarpMenu.cs");

            Assert.DoesNotContain("filterDropdownOpen", source);
        }

        // Expected: WarpMenuUIBuilder exposes FavoritesCheckbox, not FilterDropdown
        [Fact]
        public void WarpMenuUIBuilder_HasFavoritesCheckbox()
        {
            var type = typeof(EasyWarps.UI.WarpMenuUIBuilder);
            var favProp = type.GetProperty("FavoritesCheckbox", BindingFlags.Public | BindingFlags.Instance);
            var filterProp = type.GetProperty("FilterDropdown", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(favProp);
            Assert.Null(filterProp);
        }

        public static IEnumerable<object[]> GetMenuTypes()
        {
            foreach (var type in AllMenuTypes)
                yield return new object[] { type };
        }

        private static string ReadSourceFile(string relativePath)
        {
            var projectDir = FindProjectRoot();
            var fullPath = Path.Combine(projectDir, "EasyWarps", relativePath);
            return File.ReadAllText(fullPath);
        }

        private static string FindProjectRoot()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir, "EasyWarps", "UI")))
                    return dir;
                dir = Directory.GetParent(dir)?.FullName;
            }
            throw new InvalidOperationException("Could not find project root containing EasyWarps/UI");
        }

        private static string ExtractMethod(string source, string methodName)
        {
            int idx = source.IndexOf(methodName, StringComparison.Ordinal);
            if (idx < 0) return "";
            int braceCount = 0;
            bool started = false;
            int start = idx;
            for (int i = idx; i < source.Length; i++)
            {
                if (source[i] == '{') { braceCount++; started = true; }
                else if (source[i] == '}') { braceCount--; }
                if (started && braceCount == 0)
                    return source.Substring(start, i - start + 1);
            }
            return source.Substring(start);
        }
    }
}
