using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using CustomPlatformColors;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(UIStyle))]
    public static class UIStyle_Patch
    {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPostfix]
        public static void Constructor_Postfix(UIStyle __instance)
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled)) 
                return;

            // Update text colors
            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
            {
                __instance.TextColor = textColor;
            }

            // Update button colors
            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX buttonColor))
            {
                __instance.ButtonColor = buttonColor;
            }

            // Update highlight color
            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX highlightColor))
            {
                __instance.HighlightColor = highlightColor;
            }

            // Update slider fill color
            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subCyan, out colorX sliderColor))
            {
                __instance.SliderFillColor = sliderColor;
            }
        }
    }
}