using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using CustomPlatformColors;
using System;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(RadiantDashButton))]
    public static class RadiantDashButton_Patch
    {
        // Patch the static color properties
        [HarmonyPatch("get_DEFAULT_COLOR")]
        [HarmonyPostfix]
        public static void DefaultColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!ShouldApplyCustomColors())
                    return;

                if (CustomPlatformColors.Config != null && 
                    CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashButtonColor, out colorX buttonColor))
                {
                    __result = buttonColor;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in DefaultColor_Postfix: {ex.Message}");
            }
        }

        [HarmonyPatch("get_DISABLED_COLOR")]
        [HarmonyPostfix]
        public static void DisabledColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!ShouldApplyCustomColors())
                    return;

                if (CustomPlatformColors.Config != null && 
                    CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashButtonDisabledColor, out colorX disabledColor))
                {
                    __result = disabledColor;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in DisabledColor_Postfix: {ex.Message}");
            }
        }

        // Patch the UpdateColors method to customize button colors
        [HarmonyPatch("UpdateColors")]
        [HarmonyPrefix]
        public static bool UpdateColors_Prefix(
            RadiantDashButton __instance,
            RadiantDashScreen screen, 
            colorX screenColor)
        {
            try
            {
                if (__instance == null || !ShouldApplyCustomColors() || CustomPlatformColors.Config == null)
                    return true;

                // Check if any custom colors are defined
                bool hasCustomButtonColor = CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashButtonColor, out colorX buttonColor);
                bool hasCustomHighlightColor = CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashButtonHoverColor, out colorX highlightColor);
                bool hasCustomTextColor = CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashButtonTextColor, out colorX textColor);
                
                if (!hasCustomButtonColor && !hasCustomHighlightColor && !hasCustomTextColor)
                    return true;

                // Get private fields via reflection
                var buttonField = __instance.GetType().GetField("_button", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var textField = __instance.GetType().GetField("_text", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var textBgField = __instance.GetType().GetField("_textBg", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var iconField = __instance.GetType().GetField("_icon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var currentScreenField = __instance.GetType().GetField("_currentScreen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (buttonField == null || textField == null || textBgField == null)
                    return true;

                var button = (buttonField.GetValue(__instance) as SyncRef<Button>)?.Target;
                var text = (textField.GetValue(__instance) as SyncRef<Text>)?.Target;
                var textBg = (textBgField.GetValue(__instance) as SyncRef<Image>)?.Target;
                var currentScreen = (currentScreenField?.GetValue(__instance) as RelayRef<SyncRef<RadiantDashScreen>>)?.Target?.Target;

                if (button == null || text == null || textBg == null)
                    return true;

                // Apply the custom colors
                if (hasCustomButtonColor)
                {
                    // Set text background tint if available
                    if (textBg.Tint != null && !textBg.Tint.IsDriven)
                        textBg.Tint.Value = buttonColor.SetA(0.25f);

                    // Check if ColorDrivers is not null before iterating
                    if (button.ColorDrivers != null)
                    {
                        foreach (var colorDriver in button.ColorDrivers)
                        {
                            if (colorDriver == null || colorDriver.DisabledColor == null || colorDriver.DisabledColor.IsDriven)
                                continue;

                            if (colorDriver.NormalColor == null || colorDriver.HighlightColor == null || colorDriver.PressColor == null)
                                continue;

                            if (currentScreen == screen)
                            {
                                colorDriver.NormalColor.Value = screenColor;
                                colorDriver.HighlightColor.Value = hasCustomHighlightColor ? highlightColor : screenColor * 0.9f;
                            }
                            else
                            {
                                colorDriver.NormalColor.Value = buttonColor;
                                colorDriver.HighlightColor.Value = hasCustomHighlightColor ? highlightColor : MathX.Lerp(buttonColor, screenColor, 0.5f);
                            }
                            
                            colorDriver.PressColor.Value = screenColor + 0.2f;
                        }
                    }
                }
                
                // Let the original method continue to apply other non-color settings
                return true;
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in UpdateColors_Prefix: {ex.Message}");
                return true; // Let the original method run in case of error
            }
        }

        // Helper method to check if custom colors should be applied
        private static bool ShouldApplyCustomColors()
        {
            try
            {
                return CustomPlatformColors.Config != null && 
                       CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled) &&
                       CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantDashButton);
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in ShouldApplyCustomColors: {ex.Message}");
                return false;
            }
        }
    }
} 
