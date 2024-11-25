using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using System;
using System.Collections.Generic;
using Elements.Assets;
using FrooxEngine.UIX;
using System.Reflection;
using System.Linq;

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
        public const string VERSION_CONSTANT = "1.0.0";
        public override string Link => "https://github.com/Dexy/CustomPlatformColors";

        //The following
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enabled = new("enabled", "Should the mod be enabled", () => true);

        // Neutral colors
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> neutralDark = new("neutralDark", "Dark neutral color", () => new colorX(0.067f, 0.082f, 0.114f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> neutralMid = new("neutralMid", "Mid neutral color", () => new colorX(0.102f, 0.122f, 0.157f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> neutralsMid = new("neutralsMid", "Color for regular items", () => RadiantUI_Constants.Neutrals.MID);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> neutralLight = new("neutralLight", "Light neutral color", () => new colorX(0.165f, 0.192f, 0.251f, 1f));

        // Hero colors
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> heroYellow = new("heroYellow", "Hero yellow color", () => new colorX(1f, 0.843f, 0f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> heroGreen = new("heroGreen", "Hero green color", () => new colorX(0f, 1f, 0f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> heroRed = new("heroRed", "Hero red color", () => new colorX(1f, 0.462f, 0.462f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> heroPurple = new("heroPurple", "Hero purple color", () => new colorX(0.727f, 0.391f, 0.949f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> heroCyan = new("heroCyan", "Hero cyan color", () => new colorX(0.382f, 0.816f, 0.98f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> heroOrange = new("heroOrange", "Hero orange color", () => new colorX(0.902f, 0.594f, 0.312f, 1f));

        // Sub colors
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> subYellow = new("subYellow", "Sub yellow color", () => new colorX(0.281f, 0.291f, 0.17f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> subGreen = new("subGreen", "Sub green color", () => new colorX(0.141f, 0.316f, 0.17f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> subRed = new("subRed", "Sub red color", () => new colorX(0.362f, 0.196f, 0.225f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> subPurple = new("subPurple", "Sub purple color", () => new colorX(0.281f, 0.181f, 0.391f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> subCyan = new("subCyan", "Sub cyan color", () => new colorX(0.165f, 0.297f, 0.362f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> subOrange = new("subOrange", "Sub orange color", () => new colorX(0.281f, 0.235f, 0.165f, 1f));

        // Dark colors
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> darkYellow = new("darkYellow", "Dark yellow color", () => new colorX(0.165f, 0.175f, 0.157f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> darkGreen = new("darkGreen", "Dark green color", () => new colorX(0.102f, 0.175f, 0.141f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> darkRed = new("darkRed", "Dark red color", () => new colorX(0.102f, 0.078f, 0.114f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> darkPurple = new("darkPurple", "Dark purple color", () => new colorX(0.141f, 0.114f, 0.211f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> darkCyan = new("darkCyan", "Dark cyan color", () => new colorX(0.102f, 0.165f, 0.211f, 1f));
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> darkOrange = new("darkOrange", "Dark orange color", () => new colorX(0.165f, 0.141f, 0.137f, 1f));

        // Button colors
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> buttonNormalColor = new("buttonNormalColor", "Color for normal button state", () => RadiantUI_Constants.Neutrals.MID);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> buttonHoverColor = new("buttonHoverColor", "Color for button hover state", () => RadiantUI_Constants.Sub.GREEN);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> buttonTextColor = new("buttonTextColor", "Color for button text", () => RadiantUI_Constants.Neutrals.LIGHT);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> browserBackgroundColor = new("browserBackgroundColor", "Color for inventory browser background", () => RadiantUI_Constants.Neutrals.DARK);

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

            // Save default config values
            _config.Save(true);

            Harmony harmony = new("net.dexy.CustomPlatformColors");
            harmony.PatchAll();

            // Add RunPostInit to ensure UI is ready
            Engine.Current.RunPostInit(() => {
                Msg("Applying custom platform colors...");
                
                // Force refresh all UI colors
                if (Engine.Current.WorldManager.FocusedWorld != null)
                {
                    var palettes = Engine.Current.WorldManager.FocusedWorld.RootSlot.GetComponentsInChildren<PlatformColorPalette>();
                    foreach (var palette in palettes)
                    {
                        Msg($"Updating palette: {palette.Slot.Name}");
                        // Force update the palette
                        if (_config.GetValue(enabled))
                        {
                            if (_config.TryGetValue(subYellow, out colorX subYellowColor))
                            {
                                palette.Sub.Yellow.Value = subYellowColor;
                                Msg($"Set Sub.Yellow to {subYellowColor}");
                            }
                            // ... similar for other colors ...
                        }
                    }
                }
            });

            Msg("CustomPlatformColors initialized! *wags tail*");
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

        public void UpdateColors(PlatformColorPalette palette, bool forceUpdate = false)
        {
            if (Config == null || !Config.GetValue(enabled) || palette == null) 
            {
                Warn("Config is null or mod is disabled or palette is null!");
                return;
            }

            try
            {
                // Update Neutral colors
                if (Config.TryGetValue(neutralDark, out colorX darkColor))
                {
                    Msg($"Setting Neutral Dark to {darkColor}");
                    palette.Neutrals.Dark.Value = darkColor;
                    palette.Neutrals.DarkHex.Value = "#" + darkColor.ToHexString();
                }
                if (Config.TryGetValue(neutralMid, out colorX midColor))
                {
                    Msg($"Setting Neutral Mid to {midColor}");
                    palette.Neutrals.Mid.Value = midColor;
                    palette.Neutrals.MidHex.Value = "#" + midColor.ToHexString();
                }
                if (Config.TryGetValue(neutralLight, out colorX lightColor))
                {
                    Msg($"Setting Neutral Light to {lightColor}");
                    palette.Neutrals.Light.Value = lightColor;
                    palette.Neutrals.LightHex.Value = "#" + lightColor.ToHexString();
                }

                // Update Hero colors
                if (palette.Hero != null)
                {
                    if (Config.TryGetValue(heroYellow, out colorX yellowColor))
                    {
                        Msg($"Setting Hero Yellow to {yellowColor}");
                        palette.Hero.Yellow.Value = yellowColor;
                    }
                    if (Config.TryGetValue(heroGreen, out colorX greenColor))
                    {
                        Msg($"Setting Hero Green to {greenColor}");
                        palette.Hero.Green.Value = greenColor;
                    }
                    if (Config.TryGetValue(heroRed, out colorX redColor))
                    {
                        Msg($"Setting Hero Red to {redColor}");
                        palette.Hero.Red.Value = redColor;
                    }
                    if (Config.TryGetValue(heroPurple, out colorX purpleColor))
                    {
                        Msg($"Setting Hero Purple to {purpleColor}");
                        palette.Hero.Purple.Value = purpleColor;
                    }
                    if (Config.TryGetValue(heroCyan, out colorX cyanColor))
                    {
                        Msg($"Setting Hero Cyan to {cyanColor}");
                        palette.Hero.Cyan.Value = cyanColor;
                    }
                    if (Config.TryGetValue(heroOrange, out colorX orangeColor))
                    {
                        Msg($"Setting Hero Orange to {orangeColor}");
                        palette.Hero.Orange.Value = orangeColor;
                    }
                }

                // Update Sub colors
                if (palette.Sub != null)
                {
                    if (Config.TryGetValue(subYellow, out colorX yellowColor))
                    {
                        Msg($"Setting Sub Yellow to {yellowColor}");
                        palette.Sub.Yellow.Value = yellowColor;
                    }
                    if (Config.TryGetValue(subGreen, out colorX greenColor))
                    {
                        Msg($"Setting Sub Green to {greenColor}");
                        palette.Sub.Green.Value = greenColor;
                    }
                    if (Config.TryGetValue(subRed, out colorX redColor))
                    {
                        Msg($"Setting Sub Red to {redColor}");
                        palette.Sub.Red.Value = redColor;
                    }
                    if (Config.TryGetValue(subPurple, out colorX purpleColor))
                    {
                        Msg($"Setting Sub Purple to {purpleColor}");
                        palette.Sub.Purple.Value = purpleColor;
                    }
                    if (Config.TryGetValue(subCyan, out colorX cyanColor))
                    {
                        Msg($"Setting Sub Cyan to {cyanColor}");
                        palette.Sub.Cyan.Value = cyanColor;
                    }
                    if (Config.TryGetValue(subOrange, out colorX orangeColor))
                    {
                        Msg($"Setting Sub Orange to {orangeColor}");
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
        class InventoryBrowser_Colors_Patch
        {
            [HarmonyPatch("ProcessItem")]
            static void Postfix(InventoryItemUI item)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return;

                // Check if it's a favorite item (purple)
                if (item.NormalColor.Value == RadiantUI_Constants.Sub.PURPLE)
                {
                    if (CustomPlatformColors.Config.TryGetValue(subPurple, out colorX favoriteColor))
                    {
                        item.NormalColor.Value = favoriteColor;
                        item.SelectedColor.Value = favoriteColor.MulRGB(2f);
                        if (CustomPlatformColors.Config.TryGetValue(heroPurple, out colorX favoriteTextColor))
                        {
                            item.NormalText.Value = favoriteTextColor;
                        }
                    }
                    return;
                }

                // Check if it's a folder or link
                if (item.NormalColor.Value == RadiantUI_Constants.Sub.YELLOW || 
                    item.NormalColor.Value == RadiantUI_Constants.Sub.CYAN ||
                    item.NormalColor.Value == RadiantUI_Constants.Sub.ORANGE ||
                    item.NormalColor.Value == RadiantUI_Constants.Sub.GREEN ||
                    item.NormalColor.Value == RadiantUI_Constants.Sub.RED ||
                    item.NormalColor.Value == RadiantUI_Constants.Sub.PURPLE)
                {
                    if (CustomPlatformColors.Config.TryGetValue(subOrange, out colorX folderColor))
                    {
                        item.NormalColor.Value = folderColor;
                        if (CustomPlatformColors.Config.TryGetValue(heroOrange, out colorX folderTextColor))
                        {
                            item.NormalText.Value = folderTextColor;
                        }
                    }
                    return;
                }

                // Regular item colors (not a folder/link/favorite)
                if (CustomPlatformColors.Config.TryGetValue(neutralsMid, out colorX normalColor))
                {
                    item.NormalColor.Value = normalColor;
                }
                if (CustomPlatformColors.Config.TryGetValue(subGreen, out colorX selectedColor))
                {
                    item.SelectedColor.Value = selectedColor;
                    if (CustomPlatformColors.Config.TryGetValue(heroGreen, out colorX selectedTextColor))
                    {
                        item.SelectedText.Value = selectedTextColor;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RadiantUI_Constants.Sub))]
        class RadiantUI_Constants_Sub_Patch
        {
            [HarmonyPatch("get_YELLOW")]
            static bool Prefix_Yellow(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(subYellow, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_GREEN")]
            static bool Prefix_Green(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(subGreen, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_RED")]
            static bool Prefix_Red(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(subRed, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_PURPLE")]
            static bool Prefix_Purple(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(subPurple, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_CYAN")]
            static bool Prefix_Cyan(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(subCyan, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_ORANGE")]
            static bool Prefix_Orange(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(subOrange, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(RadiantUI_Constants.Hero))]
        class RadiantUI_Constants_Hero_Patch
        {
            [HarmonyPatch("get_YELLOW")]
            static bool Prefix_Yellow(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(heroYellow, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_GREEN")]
            static bool Prefix_Green(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(heroGreen, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_RED")]
            static bool Prefix_Red(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(heroRed, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_PURPLE")]
            static bool Prefix_Purple(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(heroPurple, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_CYAN")]
            static bool Prefix_Cyan(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(heroCyan, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_ORANGE")]
            static bool Prefix_Orange(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(heroOrange, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(RadiantUI_Constants.Dark))]
        class RadiantUI_Constants_Dark_Patch
        {
            // Similar pattern for Dark colors (YELLOW, GREEN, RED, PURPLE, CYAN, ORANGE)
            // ... add all Dark color patches ...
        }

        [HarmonyPatch(typeof(RadiantUI_Constants.Neutrals))]
        class RadiantUI_Constants_Neutrals_Patch
        {
            [HarmonyPatch("get_DARK")]
            static bool Prefix_Dark(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(neutralDark, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_MID")]
            static bool Prefix_Mid(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(neutralMid, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }

            [HarmonyPatch("get_LIGHT")]
            static bool Prefix_Light(ref colorX __result)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return true;
                if (CustomPlatformColors.Config.TryGetValue(neutralLight, out colorX color))
                {
                    __result = color;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlatformColorPalette))]
        class PlatformColorPalette_Patch
        {
            [HarmonyPatch("OnAwake")]
            static void Postfix(PlatformColorPalette __instance)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return;

                // Hero Colors
                if (CustomPlatformColors.Config.TryGetValue(heroYellow, out colorX heroYellowColor))
                    __instance.Hero.Yellow.Value = heroYellowColor;
                if (CustomPlatformColors.Config.TryGetValue(heroGreen, out colorX heroGreenColor))
                    __instance.Hero.Green.Value = heroGreenColor;
                if (CustomPlatformColors.Config.TryGetValue(heroPurple, out colorX heroPurpleColor))
                    __instance.Hero.Purple.Value = heroPurpleColor;
                if (CustomPlatformColors.Config.TryGetValue(heroCyan, out colorX heroCyanColor))
                    __instance.Hero.Cyan.Value = heroCyanColor;

                // Sub Colors
                if (CustomPlatformColors.Config.TryGetValue(subYellow, out colorX subYellowColor))
                    __instance.Sub.Yellow.Value = subYellowColor;
                if (CustomPlatformColors.Config.TryGetValue(subGreen, out colorX subGreenColor))
                    __instance.Sub.Green.Value = subGreenColor;
                if (CustomPlatformColors.Config.TryGetValue(subPurple, out colorX subPurpleColor))
                    __instance.Sub.Purple.Value = subPurpleColor;
                if (CustomPlatformColors.Config.TryGetValue(subCyan, out colorX subCyanColor))
                    __instance.Sub.Cyan.Value = subCyanColor;

                // Neutral Colors
                if (CustomPlatformColors.Config.TryGetValue(neutralDark, out colorX neutralDarkColor))
                    __instance.Neutrals.Dark.Value = neutralDarkColor;
                if (CustomPlatformColors.Config.TryGetValue(neutralMid, out colorX neutralMidColor))
                    __instance.Neutrals.Mid.Value = neutralMidColor;
                if (CustomPlatformColors.Config.TryGetValue(neutralLight, out colorX neutralLightColor))
                    __instance.Neutrals.Light.Value = neutralLightColor;
            }
        }

        [HarmonyPatch(typeof(InventoryBrowser))]
        class InventoryBrowser_Buttons_Patch
        {
            [HarmonyPatch("OnAwake")]
            static void Postfix(InventoryBrowser __instance)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return;

                Debug("Starting button color patch"); // Add debug logging

                var fields = typeof(InventoryBrowser).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.FieldType == typeof(SyncRef<Button>))
                    .ToArray();

                foreach (var field in fields)
                {
                    var buttonRef = field.GetValue(__instance) as SyncRef<Button>;
                    var button = buttonRef?.Target;
                    
                    if (button == null) continue;

                    Debug($"Processing button: {field.Name}"); // Add debug logging

                    if (CustomPlatformColors.Config.TryGetValue(buttonNormalColor, out colorX normalColor))
                    {
                        var image = button.Slot.GetComponent<Image>();
                        if (image != null)
                        {
                            image.Tint.Value = normalColor;

                            // Handle hover through button events
                            if (CustomPlatformColors.Config.TryGetValue(buttonHoverColor, out colorX hoverColor))
                            {
                                button.LocalPressed += (button, data) => 
                                {
                                    if (image != null && image.IsValid)
                                    {
                                        image.Tint.Value = hoverColor;
                                    }
                                };
                                
                                button.LocalReleased += (button, data) => 
                                {
                                    if (image != null && image.IsValid)
                                    {
                                        image.Tint.Value = normalColor;
                                    }
                                };
                            }
                        }
                    }

                    var text = button.Slot.GetComponent<Text>();
                    if (text != null && CustomPlatformColors.Config.TryGetValue(buttonTextColor, out colorX textColor))
                    {
                        text.Color.Value = textColor;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BrowserDialog))]
        class BrowserDialog_Background_Patch
        {
            [HarmonyPatch("OnAwake")]
            static void Postfix(BrowserDialog __instance)
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(enabled)) return;

                if (CustomPlatformColors.Config.TryGetValue(browserBackgroundColor, out colorX bgColor))
                {
                    var background = __instance.Slot.GetComponentInChildren<RectTransform>()?.Slot.GetComponent<Image>();
                    if (background != null)
                    {
                        background.Tint.Value = bgColor;
                    }
                }
            }
        }
    }
}