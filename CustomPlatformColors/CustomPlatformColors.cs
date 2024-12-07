using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using System;
using System.Reflection;
using FrooxEngine.Store;
using EnumsNET;
using SkyFrost.Base;
using FrooxEngine.UIX;

namespace CustomPlatformColors
{

    public class CustomPlatformColors : ResoniteMod
    {
        public static CustomPlatformColors? Instance { get; private set; }
        private static ModConfiguration? _config;
        public static ModConfiguration? Config => _config;

        public override string Name => "CustomPlatformColors";
        public override string Author => "Dexy";
        public override string Version => VERSION_CONSTANT;
        public const string VERSION_CONSTANT = "1.0.1";
        public override string Link => "https://github.com/DexyThePuppy/CustomPlatformColors";

        //The following
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> enabled = new("enabled", "Should the mod be enabled", () => true);

        // Neutral colors
        [AutoRegisterConfigKey] 
        public static readonly ModConfigurationKey<colorX> neutralDark = new("neutralDark", "Dark neutral color", () => RadiantUI_Constants.Neutrals.DARK);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> neutralMid = new("neutralMid", "Mid neutral color", () => RadiantUI_Constants.Neutrals.MID);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> neutralsMid = new("neutralsMid", "Color for regular items", () => RadiantUI_Constants.Neutrals.MID);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> neutralLight = new("neutralLight", "Light neutral color", () => RadiantUI_Constants.Neutrals.LIGHT);

        // Hero colors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroYellow = new("heroYellow", "Hero yellow color", () => RadiantUI_Constants.Hero.YELLOW);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroGreen = new("heroGreen", "Hero green color", () => RadiantUI_Constants.Hero.GREEN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroRed = new("heroRed", "Hero red color", () => RadiantUI_Constants.Hero.RED);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroPurple = new("heroPurple", "Hero purple color", () => RadiantUI_Constants.Hero.PURPLE);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroCyan = new("heroCyan", "Hero cyan color", () => RadiantUI_Constants.Hero.CYAN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroOrange = new("heroOrange", "Hero orange color", () => RadiantUI_Constants.Hero.ORANGE);

        // Sub colors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> subYellow = new("subYellow", "Sub yellow color", () => RadiantUI_Constants.Sub.YELLOW);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> subGreen = new("subGreen", "Sub green color", () => RadiantUI_Constants.Sub.GREEN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> subRed = new("subRed", "Sub red color", () => RadiantUI_Constants.Sub.RED);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> subPurple = new("subPurple", "Sub purple color", () => RadiantUI_Constants.Sub.PURPLE);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> subCyan = new("subCyan", "Sub cyan color", () => RadiantUI_Constants.Sub.CYAN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> subOrange = new("subOrange", "Sub orange color", () => RadiantUI_Constants.Sub.ORANGE);

        // Dark colors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> darkYellow = new("darkYellow", "Dark yellow color", () => RadiantUI_Constants.Dark.YELLOW);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> darkGreen = new("darkGreen", "Dark green color", () => RadiantUI_Constants.Dark.GREEN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> darkRed = new("darkRed", "Dark red color", () => RadiantUI_Constants.Dark.RED);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> darkPurple = new("darkPurple", "Dark purple color", () => RadiantUI_Constants.Dark.PURPLE);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> darkCyan = new("darkCyan", "Dark cyan color", () => RadiantUI_Constants.Dark.CYAN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> darkOrange = new("darkOrange", "Dark orange color", () => RadiantUI_Constants.Dark.ORANGE);

        // UI Button Colors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonTextColor = new("buttonTextColor", "Button text color", () => RadiantUI_Constants.TEXT_COLOR);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonNormalColor = new("buttonNormalColor", "Button normal color", () => RadiantUI_Constants.BUTTON_COLOR);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonHoverColor = new("buttonHoverColor", "Button hover color", () => RadiantUI_Constants.GetTintedButton(RadiantUI_Constants.BUTTON_COLOR));
		// Spacer
		[AutoRegisterConfigKey]
		public static readonly ModConfigurationKey<dummy> spacer1 = new("", "");
		// Inventory Browser Folder Colours
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryDeselectedColor = new("inventoryDeselectedColor", "", () => RadiantUI_Constants.Neutrals.MID);
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventorySelectedColor = new("inventorySelectedColor", "", () => RadiantUI_Constants.Sub.GREEN);
        [AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventorySelectedTextColor = new("inventorySelectedTextColor", "", () => RadiantUI_Constants.Hero.GREEN);
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryFolderColor = new("inventoryFolderColor", "", () => RadiantUI_Constants.Sub.YELLOW);
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryFolderTextColor = new("inventoryFolderTextColor", "", () => RadiantUI_Constants.Hero.YELLOW);
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryLinkColor = new("inventoryLinkColor", "", () => RadiantUI_Constants.Sub.CYAN);
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryLinkTextColor = new("inventoryLinkTextColor", "", () => RadiantUI_Constants.Hero.CYAN);
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryFavouriteColor = new("inventoryFavouriteColor", "", () => RadiantUI_Constants.Sub.PURPLE);
		// This is hacky don't ever hard code these values if you can avoid it but for the sake of customisation I will leave it
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryButtonsNormalColor = new("inventoryButtonsNormalColor", "", () => RadiantUI_Constants.Neutrals.MID);
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryButtonsHighlightedColor = new("inventoryButtonsHighlightedColor", "", () => new colorX(0.37f, 0.41f, 0.46f, 1.0f, ColorProfile.sRGB));
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryButtonsPressColor = new("inventoryButtonsPressColor", "", () => new colorX(0.43f, 0.54f, 0.71f, 1.0f, ColorProfile.sRGB));
		[AutoRegisterConfigKey] 
		public static readonly ModConfigurationKey<colorX> inventoryButtonsDisabledColor = new("inventoryButtonsDisabledColor", "", () => RadiantUI_Constants.Neutrals.DARK);

		public bool Enabled => Config?.GetValue(enabled) ?? false;

		public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
        {
            builder
                .Version(new Version(1, 0, 0))
                .AutoSave(true);
        }

        public override void OnEngineInit()
        {
            Instance = this;
            _config = GetConfiguration();
            if (_config == null)
            {
                Error("Failed to get mod configuration!");
                return;
            }

            _config.Save(true);

            Harmony harmony = new("net.dexy.CustomPlatformColors");
            harmony.PatchAll();
		}

		[HarmonyPatch(typeof(PlatformColorPalette), "OnStart")]
        class PlatformColorPalette_OnStart_Patch
        {
            static void Postfix(PlatformColorPalette __instance)
            {
                if (Instance != null)
                {
                    Instance.UpdateColors(__instance);
                }
            }
        }
		
		// Fixed
        public void UpdateColors(PlatformColorPalette palette, bool forceUpdate = false)
        {
            if (Config == null || !Config.GetValue(enabled) || palette == null) 
            {
                Warn("Config is null or mod is disabled or palette is null!");
                return;
            }

            // Get the AllocUser
            palette.Slot.ReferenceID.ExtractIDs(out ulong position, out byte user);
            FrooxEngine.User allocUser = palette.World.GetUserByAllocationID(user);

            // Don't run if the AllocUser isn't the LocalUser
            if (allocUser == null || position < allocUser.AllocationIDStart) {
                palette.ReferenceID.ExtractIDs(out ulong position1, out byte user1);
                FrooxEngine.User instanceAllocUser = palette.World.GetUserByAllocationID(user1);
                
                // Don't run if the AllocUser is null or Invalid, and the Instance AllocUser is Null or invalid or isn't the LocalUser
                if (instanceAllocUser == null || position1 < instanceAllocUser.AllocationIDStart || instanceAllocUser != palette.LocalUser) return;
            }
            else if (allocUser != palette.LocalUser) return;

            try
            {
                // Update Neutral colors
                if (Config.TryGetValue(neutralDark, out colorX darkColor))
                {
                    palette.Neutrals.Dark.Value = darkColor;
                    palette.Neutrals.DarkHex.Value =darkColor.ToHexString();
                }
                if (Config.TryGetValue(neutralMid, out colorX midColor))
                {
                    palette.Neutrals.Mid.Value = midColor;
                    palette.Neutrals.MidHex.Value = midColor.ToHexString();
                }
                if (Config.TryGetValue(neutralLight, out colorX lightColor))
                {
                    palette.Neutrals.Light.Value = lightColor;
                    palette.Neutrals.LightHex.Value = lightColor.ToHexString();
                }

                // Update Hero colors
                if (palette.Hero != null)
                {
                    if (Config.TryGetValue(heroYellow, out colorX yellowColor))
                    {
                        palette.Hero.Yellow.Value = yellowColor;
                    }
                    if (Config.TryGetValue(heroGreen, out colorX greenColor))
                    {
                        palette.Hero.Green.Value = greenColor;
                    }
                    if (Config.TryGetValue(heroRed, out colorX redColor))
                    {
                        palette.Hero.Red.Value = redColor;
                    }
                    if (Config.TryGetValue(heroPurple, out colorX purpleColor))
                    {
                        palette.Hero.Purple.Value = purpleColor;
                    }
                    if (Config.TryGetValue(heroCyan, out colorX cyanColor))
                    {
                        palette.Hero.Cyan.Value = cyanColor;
                    }
                    if (Config.TryGetValue(heroOrange, out colorX orangeColor))
                    {
                        palette.Hero.Orange.Value = orangeColor;
                    }
                }

                // Update Sub colors
                if (palette.Sub != null)
                {
                    if (Config.TryGetValue(subYellow, out colorX yellowColor))
                    {
                        palette.Sub.Yellow.Value = yellowColor;
                    }
                    if (Config.TryGetValue(subGreen, out colorX greenColor))
                    {
                        palette.Sub.Green.Value = greenColor;
                    }
                    if (Config.TryGetValue(subRed, out colorX redColor))
                    {
                        palette.Sub.Red.Value = redColor;
                    }
                    if (Config.TryGetValue(subPurple, out colorX purpleColor))
                    {
                        palette.Sub.Purple.Value = purpleColor;
                    }
                    if (Config.TryGetValue(subCyan, out colorX cyanColor))
                    {
                        palette.Sub.Cyan.Value = cyanColor;
                    }
                    if (Config.TryGetValue(subOrange, out colorX orangeColor))
                    {
                        palette.Sub.Orange.Value = orangeColor;
                    }
                }

                // Update Dark colors
                if (palette.Dark != null)
                {
                    if (Config.TryGetValue(darkYellow, out colorX darkYellowColor) && darkYellowColor != null && palette.Dark?.Yellow != null)
                        palette.Dark.Yellow.Value = darkYellowColor;
                    if (Config.TryGetValue(darkGreen, out colorX darkGreenColor) && darkGreenColor != null && palette.Dark?.Green != null)
                        palette.Dark.Green.Value = darkGreenColor;
                    if (Config.TryGetValue(darkRed, out colorX darkRedColor) && darkRedColor != null && palette.Dark?.Red != null)
                        palette.Dark.Red.Value = darkRedColor;
                    if (Config.TryGetValue(darkPurple, out colorX darkPurpleColor) && darkPurpleColor != null && palette.Dark?.Purple != null)
                        palette.Dark.Purple.Value = darkPurpleColor;
                    if (Config.TryGetValue(darkCyan, out colorX darkCyanColor) && darkCyanColor != null && palette.Dark?.Cyan != null)
                        palette.Dark.Cyan.Value = darkCyanColor;
                    if (Config.TryGetValue(darkOrange, out colorX darkOrangeColor) && darkOrangeColor != null && palette.Dark?.Orange != null)
                        palette.Dark.Orange.Value = darkOrangeColor;
                }
            }
            catch (Exception e)
            {
                Error($"Error updating colors: {e}");
            }
        }

		[HarmonyPatch(typeof(InventoryBrowser))]
		class InventoryBrowser_Patch {

			[HarmonyPatch("ProcessItem")]
			static void Postfix(InventoryItemUI item) {
				if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled) || item == null) return;

				FieldInfo recordField = item.GetType().GetField("Item", BindingFlags.NonPublic | BindingFlags.Instance);
				if (recordField == null) return;

				var recordValue = recordField.GetValue(item) as FrooxEngine.Store.Record;
				if (recordValue == null || item.Cloud?.Platform == null) return;

				Uri? uri = recordValue.GetUrl(item.Cloud.Platform);
				if (uri != null) {
					foreach (FavoriteEntity value in Enums.GetValues<FavoriteEntity>()) {
						if (uri == item.Engine.Cloud.Profile.GetCurrentFavorite(value)) {
							item.NormalColor.Value = Config.GetValue(inventoryFavouriteColor);
							item.SelectedColor.Value = Config.GetValue(inventoryFavouriteColor).MulRGB(2f);
							return;
						}
					}
				}

				FieldInfo directoryField = item.GetType().GetField("Directory", BindingFlags.NonPublic | BindingFlags.Instance);
				if (directoryField != null) {
					RecordDirectory? directoryValue = directoryField.GetValue(item) as RecordDirectory;

					if (directoryValue != null) {
						item.NormalColor.Value = directoryValue.IsLink ? Config.GetValue(inventoryLinkColor) : Config.GetValue(inventoryFolderColor);
						item.NormalText.Value = directoryValue.IsLink ? Config.GetValue(inventoryLinkTextColor) : Config.GetValue(inventoryFolderTextColor);
						item.SelectedColor.Value = Config.GetValue(inventorySelectedColor);
						item.SelectedText.Value = Config.GetValue(inventorySelectedTextColor);
					} else {
						item.NormalColor.Value = Config.GetValue(inventoryDeselectedColor);
						item.SelectedColor.Value = Config.GetValue(inventoryDeselectedColor);
					}
				}
			}

			
			[HarmonyPatch("OnItemSelected")]
			static void Postfix(InventoryBrowser __instance, SyncRef<Button> ____inventoriesButton) 
			{
				if (Config == null || ____inventoriesButton.Target?.Slot?.Parent == null) return;
				
				foreach (Slot child in ____inventoriesButton.Target.Slot.Parent.Children) {
					Button buttonComp = child.GetComponent<Button>();
					if (buttonComp?.ColorDrivers == null || buttonComp.ColorDrivers.Count == 0) continue;

					var driver = buttonComp.ColorDrivers[0];
					if (driver == null) continue;

					driver.NormalColor.Value = Config.GetValue(inventoryButtonsNormalColor);
					driver.HighlightColor.Value = Config.GetValue(inventoryButtonsHighlightedColor);
					driver.PressColor.Value = Config.GetValue(inventoryButtonsPressColor);
					driver.DisabledColor.Value = Config.GetValue(inventoryButtonsDisabledColor);
				}
			}
		}
	}
}

// LeCloutPanda was here
