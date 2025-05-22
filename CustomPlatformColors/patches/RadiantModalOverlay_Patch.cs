using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using System;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(RadiantModalOverlay))]
    public static class RadiantModalOverlay_Patch
    {
        // Patch the OnAttach method to customize the background color
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
                
                Image backgroundImage = maskRootSlot.GetComponent<Image>();
                if (backgroundImage != null && backgroundImage.Tint.Value == RadiantUI_Constants.BG_COLOR)
                {
                    // Replace with custom color
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX color))
                    {
                        backgroundImage.Tint.Value = color;
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantModalOverlay OnAttach_Postfix: {ex.Message}");
            }
        }
    }
} 
