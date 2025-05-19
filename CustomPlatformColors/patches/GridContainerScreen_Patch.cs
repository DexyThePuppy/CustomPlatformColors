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
        // Patch the OnAttach method to customize the active color and background
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(GridContainerScreen __instance)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen))
                    return;

                // Apply custom accent color instead of random hue
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashAccentColor, out colorX accentColor))
                {
                    __instance.ActiveColor.Value = new colorX?(accentColor);
                }

                // Apply our custom background customization right after the RadiantDashScreen's OnAttach is done
                // This will ensure proper background and texture customization
                ApplyBackgroundCustomization(__instance);
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in GridContainerScreen OnAttach_Postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Customizes the background for GridContainerScreen without directly patching the BuildBackground method
        /// </summary>
        private static void ApplyBackgroundCustomization(GridContainerScreen screen)
        {
            try
            {
                if (screen == null || screen.GridContainer == null || screen.GridContainer.BackgroundRoot == null)
                    return;

                Slot backgroundSlot = screen.GridContainer.BackgroundRoot.Slot;

                // Check for existing TiledRawImage component (pattern background)
                TiledRawImage tiledImage = backgroundSlot.GetComponent<TiledRawImage>();
                if (tiledImage != null)
                {
                    // Apply custom background color
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                    {
                        tiledImage.Tint.Value = bgColor;
                    }

                    // Apply custom texture if enabled
                    if (CustomPlatformColors.Config.GetValue(CustomPlatformColors.useCustomDashBackground))
                    {
                        Uri customTexture = CustomPlatformColors.Config.GetValue(CustomPlatformColors.customDashBackgroundTexture);
                        if (customTexture != null)
                        {
                            tiledImage.Texture.Target = (IAssetProvider<ITexture2D>)backgroundSlot.AttachTexture(customTexture);
                        }
                    }
                }
                else
                {
                    // Check for existing Image component (solid background)
                    Image backgroundImage = backgroundSlot.GetComponent<Image>();
                    if (backgroundImage != null)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                        {
                            backgroundImage.Tint.Value = bgColor;
                        }
                    }
                    
                    // If no image component exists but we want a custom texture, create one now
                    if (CustomPlatformColors.Config.GetValue(CustomPlatformColors.useCustomDashBackground))
                    {
                        Uri customTexture = CustomPlatformColors.Config.GetValue(CustomPlatformColors.customDashBackgroundTexture);
                        if (customTexture != null && backgroundImage != null)
                        {
                            // Remove the existing solid color image
                            backgroundImage.Destroy();
                            
                            // Create a tiled image with our custom texture
                            TiledRawImage newTiledImage = backgroundSlot.AttachComponent<TiledRawImage>();
                            newTiledImage.TileSize.Value = RadiantDashScreen.BackgroundTiling;
                            newTiledImage.Texture.Target = (IAssetProvider<ITexture2D>)backgroundSlot.AttachTexture(customTexture);
                            
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                            {
                                newTiledImage.Tint.Value = bgColor;
                            }
                            else
                            {
                                newTiledImage.Tint.Value = UserspaceRadiantDash.DEFAULT_BACKGROUND;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error applying background customization: {ex.Message}");
            }
        }

        // Patch the OnLoading method to handle background color for loaded screens
        [HarmonyPatch("OnLoading")]
        [HarmonyPrefix]
        public static void OnLoading_Prefix(GridContainerScreen __instance, DataTreeNode node, LoadControl control)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashScreen))
                    return;

                // We need to modify the loading using a callback since we can't access protected fields
                control.OnLoaded(__instance, () => {
                    try
                    {
                        // Apply background customization after the screen is loaded
                        ApplyBackgroundCustomization(__instance);
                    }
                    catch (Exception ex)
                    {
                        UniLog.Error($"[CustomPlatformColors] Error in GridContainerScreen OnLoaded callback: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in GridContainerScreen OnLoading_Prefix: {ex.Message}");
            }
        }
    }
} 
