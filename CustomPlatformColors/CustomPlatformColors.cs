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
        public const string VERSION_CONSTANT = "1.0.5";
        public override string Link => "https://github.com/DexyThePuppy/CustomPlatformColors";

        //The following
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> enabled = new("enabled", "Should the mod be enabled", () => true);

        // Neutral colors
       [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<dummy> spacerNeutrals = new("spacerNeutrals", "--- Neutral Colors ---");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> neutralDark = new("neutralDark", "Dark neutral color", () => RadiantUI_Constants.Neutrals.DARK);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> neutralMid = new("neutralMid", "Mid neutral color", () => RadiantUI_Constants.Neutrals.MID);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> neutralMidLight = new("neutralMidLight", "Mid-Light neutral color", () => RadiantUI_Constants.Neutrals.MIDLIGHT);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> neutralLight = new("neutralLight", "Light neutral color", () => RadiantUI_Constants.Neutrals.LIGHT);

        // Hero colors (Primary configurable colors)
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<dummy> spacerHero = new("spacerHero", "--- Hero Colors (Primary Colors) ---");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroYellow = new("heroYellow", "Hero yellow color (Mid/Sub/Dark auto-generated)", () => RadiantUI_Constants.Hero.YELLOW);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroGreen = new("heroGreen", "Hero green color (Mid/Sub/Dark auto-generated)", () => RadiantUI_Constants.Hero.GREEN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroRed = new("heroRed", "Hero red color (Mid/Sub/Dark auto-generated)", () => RadiantUI_Constants.Hero.RED);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroPurple = new("heroPurple", "Hero purple color (Mid/Sub/Dark auto-generated)", () => RadiantUI_Constants.Hero.PURPLE);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroCyan = new("heroCyan", "Hero cyan color (Mid/Sub/Dark auto-generated)", () => RadiantUI_Constants.Hero.CYAN);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> heroOrange = new("heroOrange", "Hero orange color (Mid/Sub/Dark auto-generated)", () => RadiantUI_Constants.Hero.ORANGE);

        // Configurable color generation factors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<dummy> spacerColorGeneration = new("spacerColorGeneration", "--- Color Generation Settings ---");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> midSaturationFactor = new("midSaturationFactor", "Mid saturation factor", () => 0.75f);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> midBrightnessFactor = new("midBrightnessFactor", "Mid color brightness factor (0.0-1.0)", () => 0.247f);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> subSaturationFactor = new("subSaturationFactor", "Sub saturation factor", () => 0.45f);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> subBrightnessFactor = new("subBrightnessFactor", "Sub color brightness factor (0.0-1.0)", () => 0.501f);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> darkBrightnessFactor = new("darkBrightnessFactor", "Dark color brightness factor (0.0-1.0)", () => 0.165f);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float> darkSaturationFactor = new("darkSaturationFactor", "Dark saturation factor", () => 0.25f);

        // Helper method to get float configuration values
        private static float GetValue(ModConfigurationKey<float> key)
        {
            if (Config == null) 
                return 0.5f; // Safe default brightness factor
            return Config.GetValue(key);
        }

        // Helper method to generate universal color variants that preserve hue while adjusting brightness/saturation
        // Creates a proper color curve for any base color, not just browns
        private static colorX GenerateDarkerShade(colorX baseColor, ModConfigurationKey<float> brightnessFactorKey)
        {
            float targetBrightness = GetValue(brightnessFactorKey);
            bool isDarkColor = brightnessFactorKey == darkBrightnessFactor;
            bool isSubColor = brightnessFactorKey == subBrightnessFactor;
            bool isMidColor = brightnessFactorKey == midBrightnessFactor;
            
            // Convert to HSV for proper hue preservation
            ColorHSV hsv = new ColorHSV(baseColor.baseColor);
            
            // Calculate current luminance for brightness scaling
            float currentLuminance = (baseColor.r * 0.299f + baseColor.g * 0.587f + baseColor.b * 0.114f);
            
            // Apply brightness scaling based on level
            float brightnessScale = targetBrightness / currentLuminance;
            hsv.v *= brightnessScale;
            
            // Apply saturation reduction based on level - preserves hue but reduces intensity
            if (isDarkColor)
            {
                // Dark colors get maximum saturation reduction (0.25) for very muted appearance
                float saturationReduction = GetValue(darkSaturationFactor);
                hsv.s *= saturationReduction;
            }
            else if (isSubColor)
            {
                // Sub colors get moderate saturation reduction (0.45) for slightly muted appearance
                float saturationReduction = GetValue(subSaturationFactor);
                hsv.s *= saturationReduction;
            }
            else if (isMidColor)
            {
                // Mid colors get light saturation reduction (0.75) for more vibrant appearance
                float saturationReduction = GetValue(midSaturationFactor);
                hsv.s *= saturationReduction;
            }
            
            // Convert back to RGB, preserving the original color profile
            color result = hsv.ToRGB();
            return new colorX(result, baseColor.profile);
        }

        // Helper method to update a color group with generated colors
        private void UpdateColorGroup(PlatformColorPalette palette, colorX yellow, colorX green, colorX red, colorX purple, colorX cyan, colorX orange)
        {
            if (palette == null) return;
            
            // Update Hero colors (original colors)
            palette.Hero.Yellow.Value = yellow;
            palette.Hero.Green.Value = green;
            palette.Hero.Red.Value = red;
            palette.Hero.Purple.Value = purple;
            palette.Hero.Cyan.Value = cyan;
            palette.Hero.Orange.Value = orange;
            
            // Generate Mid colors (0.501 - brighter MidLight variants)
            palette.Mid.Yellow.Value = GenerateDarkerShade(yellow, midBrightnessFactor);
            palette.Mid.Green.Value = GenerateDarkerShade(green, midBrightnessFactor);
            palette.Mid.Red.Value = GenerateDarkerShade(red, midBrightnessFactor);
            palette.Mid.Purple.Value = GenerateDarkerShade(purple, midBrightnessFactor);
            palette.Mid.Cyan.Value = GenerateDarkerShade(cyan, midBrightnessFactor);
            palette.Mid.Orange.Value = GenerateDarkerShade(orange, midBrightnessFactor);
            
            // Generate Sub colors (0.247 - darker variants)
            palette.Sub.Yellow.Value = GenerateDarkerShade(yellow, subBrightnessFactor);
            palette.Sub.Green.Value = GenerateDarkerShade(green, subBrightnessFactor);
            palette.Sub.Red.Value = GenerateDarkerShade(red, subBrightnessFactor);
            palette.Sub.Purple.Value = GenerateDarkerShade(purple, subBrightnessFactor);
            palette.Sub.Cyan.Value = GenerateDarkerShade(cyan, subBrightnessFactor);
            palette.Sub.Orange.Value = GenerateDarkerShade(orange, subBrightnessFactor);
            
            // Generate Dark colors (0.2 - darkest variants)
            palette.Dark.Yellow.Value = GenerateDarkerShade(yellow, darkBrightnessFactor);
            palette.Dark.Green.Value = GenerateDarkerShade(green, darkBrightnessFactor);
            palette.Dark.Red.Value = GenerateDarkerShade(red, darkBrightnessFactor);
            palette.Dark.Purple.Value = GenerateDarkerShade(purple, darkBrightnessFactor);
            palette.Dark.Cyan.Value = GenerateDarkerShade(cyan, darkBrightnessFactor);
            palette.Dark.Orange.Value = GenerateDarkerShade(orange, darkBrightnessFactor);
            
            // Update hex values for all colors
            UpdateHexValues(palette.Hero);
            UpdateHexValues(palette.Mid);
            UpdateHexValues(palette.Sub);
            UpdateHexValues(palette.Dark);
        }
        
        // Helper method to update hex values for a color group
        private void UpdateHexValues(PlatformColorPalette.Colors group)
        {
            group.YellowHex.Value = group.Yellow.Value.ToHexString();
            group.GreenHex.Value = group.Green.Value.ToHexString();
            group.RedHex.Value = group.Red.Value.ToHexString();
            group.PurpleHex.Value = group.Purple.Value.ToHexString();
            group.CyanHex.Value = group.Cyan.Value.ToHexString();
            group.OrangeHex.Value = group.Orange.Value.ToHexString();
        }

        // UI Button Colors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<dummy> spacerUI = new("spacerUI", "--- UI Button Colors ---");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> browserBackgroundColor = new("browserBackgroundColor", "Browser background color", () => RadiantUI_Constants.BG_COLOR);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonTextColor = new("buttonTextColor", "Button text color", () => RadiantUI_Constants.TEXT_COLOR);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonNormalColor = new("buttonNormalColor", "Button normal color", () => RadiantUI_Constants.BUTTON_COLOR);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonHoverColor = new("buttonHoverColor", "Button hover color", () => RadiantUI_Constants.GetTintedButton(RadiantUI_Constants.BUTTON_COLOR));
		[AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonPressColor = new("buttonPressColor", "Button press color", () => RadiantUI_Constants.GetTintedButton(RadiantUI_Constants.BUTTON_COLOR));
		[AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> buttonDisabledColor = new("buttonDisabledColor", "Button disabled color", () => RadiantUI_Constants.BUTTON_DISABLED_COLOR);

        // Dashboard Colors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<dummy> spacerDash = new("spacerDash", "--- Dashboard Colors ---");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> patchRadiantDashButton = new("patchRadiantDashButton", "Patch dashboard buttons", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> patchRadiantDashScreen = new("patchRadiantDashScreen", "Patch dashboard screens", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> patchUserspaceRadiantDash = new("patchUserspaceRadiantDash", "Patch userspace dashboard", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> patchUserspaceScreensManager = new("patchUserspaceScreensManager", "Patch screen manager", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashAccentColor = new("dashAccentColor", "Dashboard accent color", () => new colorX(0.5f, 0.5f, 1.0f, 1.0f));
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashBorderColor = new("dashBorderColor", "Dashboard border color", () => colorX.White * 1.5f);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> dashTransparentBackground = new("dashTransparentBackground", "Use transparent dashboard background", () => false);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashBackgroundColor = new("dashBackgroundColor", "Dashboard background color", () => UserspaceRadiantDash.DEFAULT_BACKGROUND);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashScreenBackgroundColor = new("dashScreenBackgroundColor", "Dashboard screen background color", () => RadiantUI_Constants.BG_COLOR.SetA(0.965f));
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashButtonColor = new("dashButtonColor", "Dashboard button color", () => RadiantDashButton.DEFAULT_COLOR);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashButtonHoverColor = new("dashButtonHoverColor", "Dashboard button hover color", () => RadiantDashButton.DEFAULT_COLOR * 0.9f);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashButtonDisabledColor = new("dashButtonDisabledColor", "Dashboard button disabled color", () => RadiantDashButton.DISABLED_COLOR);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashButtonTextColor = new("dashButtonTextColor", "Dashboard button text color", () => colorX.White);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> dashScreenColor = new("dashScreenColor", "Dashboard screen color", () => new colorX(0.3f, 0.6f, 1.0f));
        
        // World Preset Colors
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<dummy> spacerWorldPresets = new("spacerWorldPresets", "--- World Preset Colors ---");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> patchWorldPresets = new("patchWorldPresets", "Patch world grid/platform colors", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> gridBackgroundColor = new("gridBackgroundColor", "Grid background color", () => new colorX(0.07f, 0.08f, 0.11f));
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> gridMinorLinesColor = new("gridMinorLinesColor", "Grid minor lines color", () => new colorX(0.17f, 0.18f, 0.21f));
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> gridMajorLinesColor = new("gridMajorLinesColor", "Grid major lines color", () => new colorX(0.88f));
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<colorX> platformColor = new("platformColor", "Platform color", () => new colorX(0.5f, 0.5f, 0.5f));
        
        // Inventory Colors - Using existing hero colors for consistency
		[AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<dummy> spacerInventory = new("spacerInventory", "--- Inventory Colors (Auto-generated from Hero Colors) ---");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> patchInventoryBrowser = new("patchInventoryBrowser", "Patch inventory browser", () => true);
		[AutoRegisterConfigKey] 
        public static readonly ModConfigurationKey<bool> inventoryTransparentDeselected = new("inventoryTransparentDeselected", "Use transparent background for deselected items", () => false);

        // Helper methods to get inventory colors using existing color system
        public static colorX GetInventoryDeselectedColor() => GetValue(inventoryTransparentDeselected) ? colorX.Clear : GetValue(neutralMid);
        public static colorX GetInventorySelectedColor() => GenerateDarkerShade(GetValue(heroGreen), subBrightnessFactor);
        public static colorX GetInventorySelectedTextColor() => GetValue(heroGreen);
        public static colorX GetInventoryFolderColor() => GenerateDarkerShade(GetValue(heroYellow), subBrightnessFactor);
        public static colorX GetInventoryFolderTextColor() => GetValue(heroYellow);
        public static colorX GetInventoryLinkColor() => GenerateDarkerShade(GetValue(heroCyan), subBrightnessFactor);
        public static colorX GetInventoryLinkTextColor() => GetValue(heroCyan);
        public static colorX GetInventoryFavouriteColor() => GenerateDarkerShade(GetValue(heroPurple), subBrightnessFactor);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<Uri> customDashBackgroundTexture = new("customDashBackgroundTexture", "Custom dashboard background texture URL", () => null);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> useCustomDashBackground = new("useCustomDashBackground", "Use custom background texture", () => false);

		public bool Enabled => Config?.GetValue(enabled) ?? false;

		/// <summary>
		/// Checks if the mod is enabled and should apply its patches
		/// </summary>
		/// <returns>True if patches should be applied, false otherwise</returns>
		public static bool ShouldApplyPatch()
		{
			return Config != null && Config.GetValue(enabled);
		}

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

            Harmony harmony = new("com.Dexy.CustomPlatformColors");
            harmony.PatchAll();
		}

		[HarmonyPatch(typeof(PlatformColorPalette))]
        public class PlatformColorPalette_Patch
        {
            [HarmonyPatch("OnStart")]
            [HarmonyPostfix]
            public static void Postfix(PlatformColorPalette __instance)
            {
                try
                {
                    if (!ShouldApplyPatch() || Config == null)
                        return;

                    Instance?.UpdateColors(__instance);
                }
                catch (Exception ex)
                {
                    UniLog.Error($"[CustomPlatformColors] Error in PlatformColorPalette_Patch.Postfix: {ex.Message}");
                }
            }
        }
		
        // Helper method to get configuration values
        private static colorX GetValue(ModConfigurationKey<colorX> key)
        {
            if (Config == null) 
                return new colorX(1, 1, 1, 1); // Default white color
            return Config.GetValue(key);
        }

        // Helper method to get boolean configuration values
        private static bool GetValue(ModConfigurationKey<bool> key)
        {
            if (Config == null) 
                return false;
            return Config.GetValue(key);
        }

        // Helper methods to get auto-generated dark colors for use in patch files
        public static colorX GetDarkOrange() => GenerateDarkerShade(GetValue(heroOrange), darkBrightnessFactor);
        public static colorX GetDarkYellow() => GenerateDarkerShade(GetValue(heroYellow), darkBrightnessFactor);
        public static colorX GetDarkGreen() => GenerateDarkerShade(GetValue(heroGreen), darkBrightnessFactor);
        public static colorX GetDarkRed() => GenerateDarkerShade(GetValue(heroRed), darkBrightnessFactor);
        public static colorX GetDarkCyan() => GenerateDarkerShade(GetValue(heroCyan), darkBrightnessFactor);
        public static colorX GetDarkPurple() => GenerateDarkerShade(GetValue(heroPurple), darkBrightnessFactor);

        public void UpdateColors(PlatformColorPalette palette)
        {
            if (palette == null) return;

            // Update Neutral Colors
            palette.Neutrals.Dark.Value = GetValue(neutralDark);
            palette.Neutrals.DarkHex.Value = palette.Neutrals.Dark.Value.ToHexString();
            palette.Neutrals.Mid.Value = GetValue(neutralMid);
            palette.Neutrals.MidHex.Value = palette.Neutrals.Mid.Value.ToHexString();
            palette.Neutrals.MidLight.Value = GetValue(neutralMidLight);
            palette.Neutrals.MidLightHex.Value = palette.Neutrals.MidLight.Value.ToHexString();
            palette.Neutrals.Light.Value = GetValue(neutralLight);
            palette.Neutrals.LightHex.Value = palette.Neutrals.Light.Value.ToHexString();

            // Update Hero Colors and auto-generate Mid/Sub/Dark variants
            UpdateColorGroup(
                palette,
                GetValue(heroYellow),
                GetValue(heroGreen),
                GetValue(heroRed),
                GetValue(heroPurple),
                GetValue(heroCyan),
                GetValue(heroOrange)
            );
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
                            item.NormalColor.Value = GetInventoryFavouriteColor();
                            item.SelectedColor.Value = GetInventoryFavouriteColor().MulRGB(2f);
							return;
						}
					}
				}

				FieldInfo directoryField = item.GetType().GetField("Directory", BindingFlags.NonPublic | BindingFlags.Instance);
				if (directoryField != null) {
					RecordDirectory? directoryValue = directoryField.GetValue(item) as RecordDirectory;

					if (directoryValue != null) {
                        item.NormalColor.Value = directoryValue.IsLink ? GetInventoryLinkColor() : GetInventoryFolderColor();
                        item.NormalText.Value = directoryValue.IsLink ? GetInventoryLinkTextColor() : GetInventoryFolderTextColor();
                        item.SelectedColor.Value = GetInventorySelectedColor();
                        item.SelectedText.Value = GetInventorySelectedTextColor();
					} else {
                        item.NormalColor.Value = GetInventoryDeselectedColor();
                        item.SelectedColor.Value = GetInventoryDeselectedColor();
					}
				}
			}
		}
	}
}

// LeCloutPanda was here
