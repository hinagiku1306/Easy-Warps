using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class SignEditMenu : TitleTextInputMenu
    {
        private readonly SObject signObj;
        private readonly WarpPointStore store;
        private readonly WarpPoint? existingWarp;
        private readonly bool editNameOnly;

        private bool isWarpChecked;
        private ClickableComponent checkboxComponent = null!;

        public SignEditMenu(SObject signObj, WarpPointStore store, WarpPoint? existingWarp, bool editNameOnly = false)
            : base(GetTitle(signObj, editNameOnly), null,
                  editNameOnly && existingWarp != null ? existingWarp.Name : signObj.signText.Value ?? "")
        {
            this.signObj = signObj;
            this.store = store;
            this.existingWarp = existingWarp;
            this.editNameOnly = editNameOnly;

            isWarpChecked = existingWarp != null
                || (ModEntry.Config.AlwaysRegisterAsWarpPoint && string.IsNullOrEmpty(signObj.signText.Value));
            pasteButton.visible = false;

            doneNaming = OnDoneNaming;
            textBox.textLimit = SignTextMaxLength;

            RecalculateCheckboxLayout();
        }

        private static string GetTitle(SObject signObj, bool editNameOnly)
        {
            if (editNameOnly)
                return TranslationCache.SignEditTitle;
            return string.IsNullOrEmpty(signObj.signText.Value)
                ? TranslationCache.SignEditTitleNew
                : TranslationCache.SignEditTitle;
        }

        private void RecalculateCheckboxLayout()
        {
            if (editNameOnly) return;

            int checkboxY = textBox.Y + SignEditCheckboxBelowTextBox;
            int checkboxX = textBox.X;

            float labelWidth = Game1.smallFont.MeasureString(TranslationCache.SignEditCheckbox).X;
            int totalWidth = CheckboxSize + SignEditCheckboxLabelGap + (int)labelWidth;

            checkboxComponent = new ClickableComponent(
                new Rectangle(checkboxX, checkboxY, totalWidth, CheckboxSize),
                "warpCheckbox");
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!editNameOnly && checkboxComponent.containsPoint(x, y))
            {
                isWarpChecked = !isWarpChecked;
                Game1.playSound("drumkit6");
                return;
            }

            base.receiveLeftClick(x, y, playSound);
        }

        private void OnDoneNaming(string text)
        {
            string trimmed = text.Trim();

            if (!editNameOnly)
            {
                signObj.signText.Value = trimmed;
                signObj.showNextIndex.Value = string.IsNullOrEmpty(trimmed);
            }

            var locationName = signObj.Location?.NameOrUniqueName ?? "";
            var tileX = (int)signObj.TileLocation.X;
            var tileY = (int)signObj.TileLocation.Y;

            if (isWarpChecked)
            {
                if (existingWarp != null)
                {
                    existingWarp.Name = trimmed;
                    store.Update(existingWarp);
                }
                else
                {
                    store.Add(new WarpPoint
                    {
                        Name = trimmed,
                        LocationName = locationName,
                        TileX = tileX,
                        TileY = tileY,
                        CreatedDay = (uint)Game1.Date.TotalDays,
                        CreatedTick = DateTime.UtcNow.Ticks
                    });
                }
            }
            else if (!isWarpChecked && existingWarp != null)
            {
                store.Delete(existingWarp.Id);
            }

            if (editNameOnly)
            {
                Game1.activeClickableMenu = new WarpMenu(store, signObj, signObj.TileLocation);
            }
            else
            {
                Game1.player.freezePause = 300;
                exitThisMenu();
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            if (!editNameOnly)
            {
                var sourceRect = isWarpChecked ? UIHelpers.CheckedSourceRect : UIHelpers.UncheckedSourceRect;
                int cbX = checkboxComponent.bounds.X;
                int cbY = checkboxComponent.bounds.Y;
                b.Draw(Game1.mouseCursors, new Vector2(cbX, cbY), sourceRect, Color.White,
                    0f, Vector2.Zero, CheckboxScale, SpriteEffects.None, 1f);

                float textHeight = Game1.smallFont.MeasureString(TranslationCache.SignEditCheckbox).Y;
                int labelX = cbX + CheckboxSize + SignEditCheckboxLabelGap;
                int labelY = cbY + (CheckboxSize - (int)textHeight) / 2;
                Utility.drawTextWithShadow(b, TranslationCache.SignEditCheckbox, Game1.smallFont,
                    new Vector2(labelX, labelY), Game1.textColor);
            }

            drawMouse(b);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            int vW = Game1.uiViewport.Width;
            int vH = Game1.uiViewport.Height;

            xPositionOnScreen = 0;
            yPositionOnScreen = 0;
            width = vW;
            height = vH;

            textBox.X = vW / 2 - SignEditTextBoxOffsetX;
            textBox.Y = vH / 2;

            doneNamingButton.bounds.X = vW / 2 + SignEditDoneButtonOffsetX;
            doneNamingButton.bounds.Y = vH / 2 - SignEditButtonOffsetY;

            textBoxCC.bounds.X = textBox.X;
            textBoxCC.bounds.Y = textBox.Y;

            RecalculateCheckboxLayout();
        }
    }
}
