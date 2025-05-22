using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Reflection;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(BrowserDialog))]
    public static class BrowserDialog_Patch
    {
        [HarmonyPatch("GenerateContent")]
        [HarmonyPostfix]
        public static void GenerateContent_Postfix(BrowserDialog __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            var swapperField = __instance.GetType().GetField("_swapper", BindingFlags.NonPublic | BindingFlags.Instance);
            if (swapperField == null)
                return;

            var swapperRef = swapperField.GetValue(__instance) as SyncRef<SlideSwapRegion>;
            if (swapperRef?.Target == null)
                return;

            // Find all panel images that use BG_COLOR in the swapper
            foreach (Image img in swapperRef.Target.Slot.GetComponentsInChildren<Image>())
            {
                // Background panels
                if (img.Tint.Value == RadiantUI_Constants.BG_COLOR)
                {
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.browserBackgroundColor, out colorX color))
                    {
                        img.Tint.Value = color;
                    }
                }
            }
        }
    }
} 
