using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Reflection;
using System.Threading.Tasks;
using Renderite.Shared;

namespace CustomPlatformColors.Patches
{
    /// <summary>
    /// BrowserDialog base class patches. This handles common functionality shared between 
    /// FileBrowser and InventoryBrowser to avoid code duplication:
    /// - Background panel customization in GenerateContent
    /// - Common button and panel customization in OnAttach (with delay for UI construction)
    /// - Utility method ApplyCommonCustomizations for manual triggering if needed
    /// </summary>
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

        // Common method to customize buttons and background panels - used by both FileBrowser and InventoryBrowser
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(BrowserDialog __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            // Apply common customizations with a delay to ensure UI is fully constructed
            __instance.StartTask(async () =>
            {
                await new ToWorld();
                await Task.Delay(100); // Small delay to ensure all UI elements are created
                
                try
                {
                    ApplyCommonCustomizations(__instance);
                }
                catch (System.Exception ex)
                {
                    UniLog.Error($"[CustomPlatformColors] Error in BrowserDialog OnAttach_Postfix: {ex.Message}");
                }
            });
        }

        // Hook OnChanges to reapply button colors when buttons get enabled/disabled
        [HarmonyPatch("OnChanges")]
        [HarmonyPostfix]
        public static void OnChanges_Postfix(BrowserDialog __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            try
            {
                // Only reapply button colors, not background panels (those don't change)
                ApplyButtonCustomizations(__instance);
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in BrowserDialog OnChanges_Postfix: {ex.Message}");
            }
        }

        // Common customization method that can be called from derived class patches if needed
        public static void ApplyCommonCustomizations(BrowserDialog instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            try
            {
                ApplyButtonCustomizations(instance);
                ApplyBackgroundCustomizations(instance);
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in ApplyCommonCustomizations: {ex.Message}");
            }
        }

        // Apply button color customizations
        private static void ApplyButtonCustomizations(BrowserDialog instance)
        {
            // Get all buttons in the browser
            var buttons = instance.Slot.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                if (button?.ColorDrivers == null || button.ColorDrivers.Count == 0)
                    continue;

                var driver = button.ColorDrivers[0];
                if (driver == null)
                    continue;

                // Skip buttons that belong to BrowserItem (InventoryItemUI, etc.) to avoid conflicts
                var browserItem = button.Slot.GetComponentInParents<BrowserItem>();
                if (browserItem != null)
                    continue;

                // Apply button colors based on type
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX normalColor))
                    driver.NormalColor.Value = normalColor;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hoverColor))
                    driver.HighlightColor.Value = hoverColor;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonPressColor, out colorX pressColor))
                    driver.PressColor.Value = pressColor;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonDisabledColor, out colorX disabledColor))
                    driver.DisabledColor.Value = disabledColor;

                // Update button text color
                var text = button.Slot.GetComponentInChildren<Text>();
                if (text != null && CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
                {
                    text.Color.Value = textColor;
                }
            }
        }

        // Apply background panel customizations
        private static void ApplyBackgroundCustomizations(BrowserDialog instance)
        {
            // Update background color for the browser
            var panels = instance.Slot.GetComponentsInChildren<Image>();
            foreach (var panel in panels)
            {
                if (panel.Tint.Value == RadiantUI_Constants.BG_COLOR && 
                    CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.browserBackgroundColor, out colorX bgColor))
                {
                    panel.Tint.Value = bgColor;
                }
            }
        }
    }
} 
