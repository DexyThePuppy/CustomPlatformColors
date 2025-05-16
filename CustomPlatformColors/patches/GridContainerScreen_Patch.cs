using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using System;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(GridContainerScreen))]
    public static class GridContainerScreen_Patch
    {
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(GridContainerScreen __instance)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() || 
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen))
                    return;

                // Apply custom accent color for different screen types
                bool isGridHomeScreen = __instance.HasPreset(typeof(HomeScreenInitializer));
                if (isGridHomeScreen && CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashHomeScreenColor, out colorX gridHomeColor))
                {
                    __instance.ActiveColor.Value = new colorX?(gridHomeColor);
                }
                else if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashAccentColor, out colorX accentColor))
                {
                    __instance.ActiveColor.Value = new colorX?(accentColor);
                }
                
                // Apply background color based on screen type
                if (__instance.GridContainer?.BackgroundRoot?.Slot != null)
                {
                    Image bgImage = __instance.GridContainer.BackgroundRoot.Slot.GetComponent<Image>();
                    if (bgImage != null)
                    {
                        if (isGridHomeScreen && CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashHomeScreenColor, out colorX homeScreenColor))
                        {
                            bgImage.Tint.Value = homeScreenColor.MulRGB(0.6f); // Darker version for background
                        }
                        else if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                        {
                            bgImage.Tint.Value = bgColor;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in GridContainerScreen OnAttach_Postfix: {ex.Message}");
            }
        }
    }
    
    // This patch specifically targets the home screen initialization
    [HarmonyPatch(typeof(UserspaceScreensManager))]
    public static class UserspaceScreensManager_HomeScreen_Patch
    {
        [HarmonyPatch("AddHomeScreen")]
        [HarmonyPostfix]
        public static void AddHomeScreen_Postfix(GridContainerScreen __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() || 
                    __result == null ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen))
                    return;
                
                // Update the accent color
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashHomeScreenColor, out colorX accentColor))
                {
                    __result.ActiveColor.Value = new colorX?(accentColor);
                }
                
                // Apply initial background
                ApplyHomeScreenBackground(__result);
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in UserspaceScreensManager_HomeScreen_Patch.AddHomeScreen_Postfix: {ex.Message}");
            }
        }
        
        // Helper method to apply background to home screen
        public static void ApplyHomeScreenBackground(GridContainerScreen screen)
        {
            if (screen == null || screen.GridContainer?.BackgroundRoot?.Slot == null)
                return;
                
            // Handle custom background if enabled
            if (CustomPlatformColors.Config.GetValue(CustomPlatformColors.useCustomDashBackground))
            {
                Uri customTexture = CustomPlatformColors.Config.GetValue(CustomPlatformColors.customDashBackgroundTexture);
                if (customTexture != null)
                {
                    // First remove existing background components
                    foreach (Image img in screen.GridContainer.BackgroundRoot.Slot.GetComponents<Image>())
                    {
                        img.Destroy();
                    }
                    
                    foreach (TiledRawImage img in screen.GridContainer.BackgroundRoot.Slot.GetComponents<TiledRawImage>())
                    {
                        img.Destroy();
                    }
                    
                    // Create our custom background
                    Slot bgSlot = screen.GridContainer.BackgroundRoot.Slot;
                    TiledRawImage tiledRawImage = bgSlot.AttachComponent<TiledRawImage>();
                    tiledRawImage.TileSize.Value = RadiantDashScreen.BackgroundTiling;
                    tiledRawImage.Texture.Target = (IAssetProvider<ITexture2D>)bgSlot.AttachTexture(customTexture);
                    
                    // Apply home screen specific color
                    colorX bgColor = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashHomeScreenColor, out colorX screenColor))
                    {
                        bgColor = screenColor.MulRGB(0.6f); // Darker version for background
                    }
                    tiledRawImage.Tint.Value = bgColor;
                }
            }
            // Regular color override (no custom texture)
            else
            {
                Image bgImage = screen.GridContainer.BackgroundRoot.Slot.GetComponent<Image>();
                if (bgImage != null && CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashHomeScreenColor, out colorX homeScreenColor))
                {
                    bgImage.Tint.Value = homeScreenColor.MulRGB(0.6f); // Darker version for background
                }
            }
        }
    }
} 
