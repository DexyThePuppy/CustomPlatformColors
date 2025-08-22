using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using System;
using Renderite.Shared;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(RadiantModalOverlay))]
    public static class RadiantModalOverlay_Patch
    {
        // Patch the OnAttach method to customize all colors
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(RadiantModalOverlay __instance)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch())
                    return;

                // Find the background image component that uses RadiantUI_Constants.BG_COLOR
                Slot maskRoot = __instance.Slot.FindChild(slot => 
                    slot.Name == "Background" && 
                    slot.FindChild(s => s.Name == "SizeRoot") != null);
                
                if (maskRoot == null)
                    return;
                
                Slot sizeRoot = maskRoot.FindChild(s => s.Name == "SizeRoot");
                if (sizeRoot == null)
                    return;
                
                Slot maskRootSlot = sizeRoot.FindChild(s => s.Name == "MaskRoot");
                if (maskRootSlot == null)
                    return;
                
                // Update background color
                Image backgroundImage = maskRootSlot.GetComponent<Image>();
                if (backgroundImage != null && backgroundImage.Tint.Value == RadiantUI_Constants.BG_COLOR)
                {
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX bgColor))
                    {
                        backgroundImage.Tint.Value = bgColor;
                    }
                }

                // Find and update header elements
                Slot headerSlot = maskRootSlot.FindChild(s => s.Name == "Header");
                if (headerSlot != null)
                {
                    // Update title text color
                    Slot titleSlot = headerSlot.FindChild(s => s.Name == "Title");
                    if (titleSlot != null)
                    {
                        Text titleText = titleSlot.GetComponent<Text>();
                        if (titleText != null && titleText.Color.Value == RadiantUI_Constants.TEXT_COLOR)
                        {
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
                            {
                                titleText.Color.Value = textColor;
                            }
                        }
                    }

                    // Update close button colors
                    Slot closeSlot = headerSlot.FindChild(s => s.Name == "Close");
                    if (closeSlot != null)
                    {
                        Slot closePaddingSlot = closeSlot.FindChild(s => s.Name == "ClosePadding");
                        if (closePaddingSlot != null)
                        {
                            // Update close button background color
                            Image closeButtonBg = closePaddingSlot.GetComponent<Image>();
                            if (closeButtonBg != null && closeButtonBg.Tint.Value == RadiantUI_Constants.Hero.RED)
                            {
                                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroRed, out colorX closeColor))
                                {
                                    closeButtonBg.Tint.Value = closeColor;
                                }
                            }

                            // Update close button icon color
                            Slot closeImageSlot = closePaddingSlot.FindChild(s => s.Name == "Image");
                            if (closeImageSlot != null)
                            {
                                Image closeIcon = closeImageSlot.GetComponent<Image>();
                                if (closeIcon != null && closeIcon.Tint.Value == RadiantUI_Constants.Sub.RED)
                                {
                                    colorX subRed = CustomPlatformColors.GenerateDarkerShade(
                                        CustomPlatformColors.GetValue(CustomPlatformColors.heroRed),
                                        CustomPlatformColors.subBrightnessFactor
                                    );
                                    closeIcon.Tint.Value = subRed;
                                }
                            }
                        }
                    }
                }

                // Update blur material settings
                BlurMaterial blurMaterial = __instance.Slot.GetComponent<BlurMaterial>();
                if (blurMaterial != null)
                {
                    // Adjust blur material properties if needed
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX blurColor))
                    {
                        // The blur material's color is controlled by the BackgroundColor sync field
                        // We don't need to modify it here as it's handled by the component itself
                        __instance.BackgroundColor.Value = blurColor.MulA(0.75f);
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantModalOverlay OnAttach_Postfix: {ex.Message}");
            }
        }

        // Patch UpdateVisuals to ensure our colors persist through updates
        [HarmonyPatch("UpdateVisuals")]
        [HarmonyPostfix]
        public static void UpdateVisuals_Postfix(RadiantModalOverlay __instance)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch())
                    return;

                // Re-apply our color customizations after the base UpdateVisuals runs
                OnAttach_Postfix(__instance);
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantModalOverlay UpdateVisuals_Postfix: {ex.Message}");
            }
        }
    }
} 
