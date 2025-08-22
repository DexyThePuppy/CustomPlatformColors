using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using CustomPlatformColors;
using System;
using Renderite.Shared;

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

            // Use single color for all screen types
            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashScreenColor, out colorX screenColor))
                __result = screenColor;
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
                
                // Let our implementation run but with color override
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
        
        // Also patch the UIBuilder overload of BuildBackground to ensure consistent behavior
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
                if (backgroundTexture != null)
                {
                    IAssetProvider<ITexture2D> texture = ui.Root.AttachTexture(backgroundTexture);
                    if (texture != null)
                    {
                        // Apply custom color if available
                        colorX uiTint = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                        if (CustomPlatformColors.Config?.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX uiCustomColor) ?? false)
                        {
                            uiTint = uiCustomColor;
                        }
                        
                        ui.TiledRawImage(texture, uiTint).TileSize.Value = RadiantDashScreen.BackgroundTiling;
                    }
                    else
                    {
                        // Fallback to plain image if texture fails to load
                        colorX uiTint = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                        if (CustomPlatformColors.Config?.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX uiCustomColor) ?? false)
                        {
                            uiTint = uiCustomColor;
                        }
                        ui.Image(uiTint);
                    }
                }
                else
                {
                    colorX uiTint = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                    if (CustomPlatformColors.Config?.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX uiCustomColor) ?? false)
                    {
                        uiTint = uiCustomColor;
                    }
                    ui.Image(uiTint);
                }
                    
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
