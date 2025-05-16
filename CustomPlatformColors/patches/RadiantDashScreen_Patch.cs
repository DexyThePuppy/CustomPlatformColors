using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using CustomPlatformColors;
using System;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(RadiantDashScreen))]
    public static class RadiantDashScreen_Patch
    {
        // Patch the CurrentColor method to apply custom screen colors
        [HarmonyPatch("get_CurrentColor")]
        [HarmonyPostfix]
        public static void CurrentColor_Postfix(RadiantDashScreen __instance, ref colorX __result)
        {
            if (!ShouldApplyCustomColors() || CustomPlatformColors.Config == null || __instance == null)
                return;

            // Get the screen type name to check for specific screen colors
            string screenType = __instance.GetType().Name ?? "";
            if (string.IsNullOrEmpty(screenType))
                return;

            // Apply specific color for each screen type if configured
            switch (screenType)
            {
                case "HomeScreen":
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashHomeScreenColor, out colorX homeColor))
                        __result = homeColor;
                    break;
                case "WorldsScreen":
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashWorldsScreenColor, out colorX worldsColor))
                        __result = worldsColor;
                    break;
                case "InventoryScreen":
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashInventoryScreenColor, out colorX inventoryColor))
                        __result = inventoryColor;
                    break;
                case "ContactsScreen":
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashContactsScreenColor, out colorX contactsColor))
                        __result = contactsColor;
                    break;
                case "SettingsScreen":
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashSettingsScreenColor, out colorX settingsColor))
                        __result = settingsColor;
                    break;
                case "ExitScreen":
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashExitScreenColor, out colorX exitColor))
                        __result = exitColor;
                    break;
                default:
                    // Apply generic screen color for other screen types
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashScreenColor, out colorX genericColor))
                        __result = genericColor;
                    break;
            }
        }

        // Patch the OnAttach method to apply custom accent colors immediately
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(RadiantDashScreen __instance)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() || 
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen))
                    return;

                // Apply custom color based on screen type
                string screenType = __instance.GetType().Name;
                
                // GridContainerScreen with HomeScreenInitializer
                if (__instance is GridContainerScreen gridScreen && gridScreen.HasPreset(typeof(HomeScreenInitializer)))
                {
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashHomeScreenColor, out colorX homeColor))
                    {
                        __instance.ActiveColor.Value = new colorX?(homeColor);
                    }
                }
                else if (__instance is GridContainerScreen worldsScreen && worldsScreen.HasPreset(typeof(WorldsScreenInitializer)))
                {
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashWorldsScreenColor, out colorX worldsColor))
                    {
                        __instance.ActiveColor.Value = new colorX?(worldsColor);
                    }
                }
                else if (__instance is GridContainerScreen settingsScreen && settingsScreen.HasPreset(typeof(SettingsScreenInitializer)))
                {
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashSettingsScreenColor, out colorX settingsColor))
                    {
                        __instance.ActiveColor.Value = new colorX?(settingsColor);
                    }
                }
                else
                {
                    switch (screenType)
                    {
                        case "InventoryScreen":
                        case "LegacyRadiantScreenWrapper`1":
                            // Check for inventory wrapper
                            if (__instance.Label.Value != null && __instance.Label.Value.Contains("Inventory"))
                            {
                                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashInventoryScreenColor, out colorX inventoryColor))
                                    __instance.ActiveColor.Value = new colorX?(inventoryColor);
                            }
                            // Check for contacts wrapper
                            else if (__instance.Label.Value != null && __instance.Label.Value.Contains("Contacts"))
                            {
                                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashContactsScreenColor, out colorX contactsColor))
                                    __instance.ActiveColor.Value = new colorX?(contactsColor);
                            }
                            break;
                        case "ExitScreen":
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashExitScreenColor, out colorX exitColor))
                                __instance.ActiveColor.Value = new colorX?(exitColor);
                            break;
                        default:
                            // Apply generic screen color for other screen types
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashScreenColor, out colorX genericColor))
                                __instance.ActiveColor.Value = new colorX?(genericColor);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantDashScreen OnAttach_Postfix: {ex.Message}");
            }
        }

        // Helper method to check if custom colors should be applied
        private static bool ShouldApplyCustomColors()
        {
            return CustomPlatformColors.Config != null && 
                   CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled) &&
                   CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen);
        }

        // Patch the BuildBackground method to use custom texture when enabled
        [HarmonyPatch("BuildBackground", new Type[] { typeof(Slot) })]
        [HarmonyPrefix]
        public static bool BuildBackground_Prefix(RadiantDashScreen __instance, Slot root)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen))
                    return true; // Continue to original method

                // Check if we should use custom background
                if (CustomPlatformColors.Config.GetValue(CustomPlatformColors.useCustomDashBackground))
                {
                    Uri customTexture = CustomPlatformColors.Config.GetValue(CustomPlatformColors.customDashBackgroundTexture);
                    // Only override if we have a valid custom texture URL
                    if (customTexture != null)
                    {
                        TiledRawImage tiledRawImage = root.AttachComponent<TiledRawImage>();
                        tiledRawImage.TileSize.Value = RadiantDashScreen.BackgroundTiling;
                        tiledRawImage.Texture.Target = (IAssetProvider<ITexture2D>)root.AttachTexture(customTexture);
                        
                        // Apply custom color if specified
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                        {
                            tiledRawImage.Tint.Value = bgColor;
                        }
                        else
                        {
                            tiledRawImage.Tint.Value = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                        }
                        
                        return false; // Skip original method
                    }
                }
                
                // Let the original method run but apply our color override
                Uri backgroundTexture = __instance.Engine.InUniverse ? null : RadiantDashScreen.BackgroundTexture;
                if (backgroundTexture != null)
                {
                    TiledRawImage tiledRawImage = root.AttachComponent<TiledRawImage>();
                    tiledRawImage.TileSize.Value = RadiantDashScreen.BackgroundTiling;
                    tiledRawImage.Texture.Target = (IAssetProvider<ITexture2D>)root.AttachTexture(backgroundTexture);
                    
                    // Apply custom background color
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                    {
                        tiledRawImage.Tint.Value = bgColor;
                    }
                    else
                    {
                        tiledRawImage.Tint.Value = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                    }
                }
                else
                {
                    Image image = root.AttachComponent<Image>();
                    
                    // Apply custom background color
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                    {
                        image.Tint.Value = bgColor;
                    }
                    else
                    {
                        image.Tint.Value = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                    }
                }
                
                return false; // Skip original method
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantDashScreen BuildBackground_Prefix: {ex.Message}");
                return true; // Continue to original method on error
            }
        }
        
        // Also patch the UIBuilder overload of BuildBackground
        [HarmonyPatch("BuildBackground", new Type[] { typeof(UIBuilder), typeof(bool) })]
        [HarmonyPrefix]
        public static bool BuildBackground_UIBuilder_Prefix(RadiantDashScreen __instance, UIBuilder ui, bool nest)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen))
                    return true; // Continue to original method

                // Check if we should use custom background
                if (CustomPlatformColors.Config.GetValue(CustomPlatformColors.useCustomDashBackground))
                {
                    Uri customTexture = CustomPlatformColors.Config.GetValue(CustomPlatformColors.customDashBackgroundTexture);
                    // Only override if we have a valid custom texture URL
                    if (customTexture != null)
                    {
                        IAssetProvider<ITexture2D> texture = (IAssetProvider<ITexture2D>)ui.Root.AttachTexture(customTexture);
                        
                        // Apply custom color if available
                        colorX uiBackgroundTint = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX uiBgColor))
                        {
                            uiBackgroundTint = uiBgColor;
                        }
                        
                        ui.TiledRawImage(texture, uiBackgroundTint).TileSize.Value = RadiantDashScreen.BackgroundTiling;
                        
                        if (nest)
                            ui.Nest();
                            
                        return false; // Skip original method
                    }
                }
                
                // Let our implementation run but with color override
                Uri backgroundTexture = __instance.Engine.InUniverse ? null : RadiantDashScreen.BackgroundTexture;
                
                // Apply custom color if available
                colorX uiTint = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX uiCustomColor))
                {
                    uiTint = uiCustomColor;
                }
                
                if (backgroundTexture != null)
                    ui.TiledRawImage((IAssetProvider<ITexture2D>)ui.Root.AttachTexture(backgroundTexture), uiTint).TileSize.Value = RadiantDashScreen.BackgroundTiling;
                else
                    ui.Image(uiTint);
                    
                if (nest)
                    ui.Nest();
                    
                return false; // Skip original method
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantDashScreen BuildBackground_UIBuilder_Prefix: {ex.Message}");
                return true; // Continue to original method on error
            }
        }
    }
} 
