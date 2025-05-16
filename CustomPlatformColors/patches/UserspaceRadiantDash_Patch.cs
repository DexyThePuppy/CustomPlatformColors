using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using System;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(UserspaceRadiantDash))]
    public static class UserspaceRadiantDash_Patch
    {
        // Patch the default background color
        [HarmonyPatch("get_DEFAULT_BACKGROUND")]
        [HarmonyPostfix]
        public static void DefaultBackground_Postfix(ref colorX __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceRadiantDash))
                    return;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashBackgroundColor, out colorX bgColor))
                {
                    // Keep the original alpha value from RadiantUI_Constants.BG_COLOR
                    __result = bgColor.SetA(__result.a);
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in UserspaceRadiantDash DefaultBackground_Postfix: {ex.Message}");
            }
        }
    }
} 

