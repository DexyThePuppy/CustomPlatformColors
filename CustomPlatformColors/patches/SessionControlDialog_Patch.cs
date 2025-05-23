using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Threading.Tasks;

namespace CustomPlatformColors.Patches
{
    /// <summary>
    /// SessionControlDialog patches. Handles button and background panel customization
    /// for the session control dialog that appears when managing world settings.
    /// - Button color customization in OnAttach (with delay for UI construction)
    /// - Reapplies button colors in OnCommonUpdate when buttons get enabled/disabled
    /// - Background panel customization
    /// </summary>
    [HarmonyPatch(typeof(SessionControlDialog))]
    public static class SessionControlDialog_Patch
    {
        // Hook OnAttach to apply initial customizations
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(SessionControlDialog __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            // Apply customizations with a delay to ensure UI is fully constructed
            __instance.StartTask(async () =>
            {
                await new ToWorld();
                await Task.Delay(150); // Slightly longer delay since SessionControlDialog has complex UI generation
                
                try
                {
                    ApplyCommonCustomizations(__instance);
                }
                catch (System.Exception ex)
                {
                    UniLog.Error($"[CustomPlatformColors] Error in SessionControlDialog OnAttach_Postfix: {ex.Message}");
                }
            });
        }

        // Hook OnCommonUpdate to reapply button colors when buttons get enabled/disabled
        [HarmonyPatch("OnCommonUpdate")]
        [HarmonyPostfix]
        public static void OnCommonUpdate_Postfix(SessionControlDialog __instance)
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
                UniLog.Error($"[CustomPlatformColors] Error in SessionControlDialog OnCommonUpdate_Postfix: {ex.Message}");
            }
        }

        // Common customization method
        public static void ApplyCommonCustomizations(SessionControlDialog instance)
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
                UniLog.Error($"[CustomPlatformColors] Error in SessionControlDialog ApplyCommonCustomizations: {ex.Message}");
            }
        }

        // Apply button color customizations
        private static void ApplyButtonCustomizations(SessionControlDialog instance)
        {
            // Get all buttons in the session control dialog
            var buttons = instance.Slot.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                if (button?.ColorDrivers == null || button.ColorDrivers.Count == 0)
                    continue;

                var driver = button.ColorDrivers[0];
                if (driver == null)
                    continue;

                // Skip buttons that belong to SessionPermissionController to avoid conflicts
                var permissionController = button.Slot.GetComponentInParents<SessionPermissionController>();
                if (permissionController != null)
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

            // Also handle Radio buttons (access level radios)
            var radios = instance.Slot.GetComponentsInChildren<Radio>();
            foreach (var radio in radios)
            {
                var radioButton = radio.Slot.GetComponent<Button>();
                if (radioButton?.ColorDrivers == null || radioButton.ColorDrivers.Count == 0)
                    continue;

                var driver = radioButton.ColorDrivers[0];
                if (driver == null)
                    continue;

                // Skip radio buttons that belong to SessionPermissionController to avoid conflicts
                var permissionController = radioButton.Slot.GetComponentInParents<SessionPermissionController>();
                if (permissionController != null)
                    continue;

                // Apply radio button colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX normalColor))
                    driver.NormalColor.Value = normalColor;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hoverColor))
                    driver.HighlightColor.Value = hoverColor;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonPressColor, out colorX pressColor))
                    driver.PressColor.Value = pressColor;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonDisabledColor, out colorX disabledColor))
                    driver.DisabledColor.Value = disabledColor;

                // Update radio button text color
                var text = radioButton.Slot.GetComponentInChildren<Text>();
                if (text != null && CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
                {
                    text.Color.Value = textColor;
                }
            }
        }

        // Apply background panel customizations
        private static void ApplyBackgroundCustomizations(SessionControlDialog instance)
        {
            // Update background color for panels
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
