using System;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using EasyWarps.Models;
using EasyWarps.Services;
using EasyWarps.Utilities;

namespace EasyWarps.Core
{
    public class ModEntry : Mod
    {
        private const string WizardMailId = "hinagiku1306.EasyWarps.WizardLetter";
        private const string WizardEventFlag = "canReadJunimoText";

        private ModConfig config = null!;

        internal static ModEntry Instance { get; private set; } = null!;
        public static ModConfig Config { get; private set; } = null!;
        public static WarpPointStore Store { get; private set; } = new();

        public static bool IsFeatureUnlocked()
        {
            return Config.DisableModRequirement
                || Game1.player.mailReceived.Contains(WizardMailId);
        }

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            config = helper.ReadConfig<ModConfig>();
            Config = config;

            DebugLogger.Initialize(Monitor, config);
            TranslationCache.Initialize(helper.Translation);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Content.LocaleChanged += OnLocaleChanged;
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            RegisterGmcm();
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Store.Load(Helper);
        }

        private void OnSaving(object? sender, SavingEventArgs e)
        {
            Store.Save(Helper);
        }

        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            Store.Clear();
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (Store.Count == 0)
                return;

            string locationName = e.Location.NameOrUniqueName;
            foreach (var pair in e.Removed)
            {
                var obj = pair.Value;
                if (!obj.IsTextSign())
                    continue;

                Vector2 tile = pair.Key;
                string locationKey = WarpPointStore.MakeLocationKey(locationName, (int)tile.X, (int)tile.Y);
                if (Store.RemoveByLocationKey(locationKey))
                {
                    DebugLogger.Trace($"Removed warp point at {locationKey} (sign removed)");
                    Store.Save(Helper);
                }
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (Config.DisableModRequirement)
                return;

            if (Game1.player.mailReceived.Contains(WizardMailId))
                return;

            if (!Game1.player.mailReceived.Contains(WizardEventFlag))
                return;

            if (!Game1.player.mailbox.Contains(WizardMailId))
                Game1.player.mailbox.Add(WizardMailId);
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>();
                    data.Data[WizardMailId] = TranslationCache.MailWizardLetter;
                });
            }
        }

        private void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
        {
            TranslationCache.Initialize(Helper.Translation);
            LocationClassifier.ClearDisplayNameCache();
            Helper.GameContent.InvalidateCache("Data/Mail");
        }

        private void RegisterGmcm()
        {
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm == null)
                return;

            gmcm.Register(
                ModManifest,
                () => { config = new ModConfig(); Config = config; },
                () => { Helper.WriteConfig(config); }
            );

            gmcm.AddBoolOption(
                ModManifest,
                () => config.DisableModRequirement,
                value => config.DisableModRequirement = value,
                () => TranslationCache.ConfigDisableModRequirementName,
                () => TranslationCache.ConfigDisableModRequirementTooltip
            );

            gmcm.AddBoolOption(
                ModManifest,
                () => config.AlwaysRegisterAsWarpPoint,
                value => config.AlwaysRegisterAsWarpPoint = value,
                () => TranslationCache.ConfigAlwaysRegisterName,
                () => TranslationCache.ConfigAlwaysRegisterTooltip
            );

            gmcm.AddBoolOption(
                ModManifest,
                () => config.EnableWarpAnimation,
                value => config.EnableWarpAnimation = value,
                () => TranslationCache.ConfigEnableAnimationName,
                () => TranslationCache.ConfigEnableAnimationTooltip
            );

            gmcm.AddBoolOption(
                ModManifest,
                () => config.DisableSignHoveringText,
                value => config.DisableSignHoveringText = value,
                () => TranslationCache.ConfigDisableHoverTextName,
                () => TranslationCache.ConfigDisableHoverTextTooltip
            );

            gmcm.AddBoolOption(
                ModManifest,
                () => config.RememberSortOption,
                value => config.RememberSortOption = value,
                () => TranslationCache.ConfigRememberSortName,
                () => TranslationCache.ConfigRememberSortTooltip
            );

            gmcm.AddBoolOption(
                ModManifest,
                () => config.RememberFavoriteOption,
                value => config.RememberFavoriteOption = value,
                () => TranslationCache.ConfigRememberFavoriteName,
                () => TranslationCache.ConfigRememberFavoriteTooltip
            );

            gmcm.AddTextOption(
                ModManifest,
                () => config.DefaultSearchScope.ToString(),
                value => config.DefaultSearchScope = Enum.Parse<WarpSearchScope>(value),
                () => TranslationCache.ConfigDefaultSearchScopeName,
                () => TranslationCache.ConfigDefaultSearchScopeTooltip,
                Enum.GetNames<WarpSearchScope>(),
                value => value switch
                {
                    nameof(WarpSearchScope.All) => TranslationCache.SearchScopeAll,
                    nameof(WarpSearchScope.Name) => TranslationCache.SearchScopeName,
                    nameof(WarpSearchScope.Location) => TranslationCache.SearchScopeLocation,
                    _ => value
                }
            );
        }

        internal ModConfig GetConfig() => config;

        internal void SaveConfig() => Helper.WriteConfig(config);
    }
}
