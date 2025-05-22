using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Reflection;
using System.Threading.Tasks;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(FileBrowser))]
    public static class FileBrowser_Patch
    {
        // Patch the SELECTED_COLOR property
        [HarmonyPatch("get_SELECTED_COLOR")]
        [HarmonyPostfix]
        public static void SELECTED_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedColor, out colorX color))
                __result = color;
        }

        // Patch the OnAttach method to customize button colors
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(FileBrowser __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            try
            {
                // Get all buttons in the browser
                var buttons = __instance.Slot.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    if (button?.ColorDrivers == null || button.ColorDrivers.Count == 0)
                        continue;

                    var driver = button.ColorDrivers[0];
                    if (driver == null)
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

                // Update background color for the browser
                var panels = __instance.Slot.GetComponentsInChildren<Image>();
                foreach (var panel in panels)
                {
                    if (panel.Tint.Value == RadiantUI_Constants.BG_COLOR && 
                        CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.browserBackgroundColor, out colorX bgColor))
                    {
                        panel.Tint.Value = bgColor;
                    }
                }
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in FileBrowser OnAttach_Postfix: {ex.Message}");
            }
        }

        // Patch OnChanges to customize directory and file colors
        [HarmonyPatch("OnChanges")]
        [HarmonyPostfix]
        public static void OnChanges_Postfix(FileBrowser __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            try
            {
                // Find all FileSystemItems and update their colors
                foreach (var item in __instance.Slot.GetComponentsInChildren<FileSystemItem>())
                {
                    if (item.Type.Value == FileSystemItem.EntryType.Directory)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderColor, out colorX folderColor))
                            item.NormalColor.Value = folderColor;
                        
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderTextColor, out colorX folderTextColor))
                            item.NormalText.Value = folderTextColor;
                    }
                    else
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryDeselectedColor, out colorX fileColor))
                            item.NormalColor.Value = fileColor;
                    }

                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedColor, out colorX selectedColor))
                        item.SelectedColor.Value = selectedColor;

                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedTextColor, out colorX selectedTextColor))
                        item.SelectedText.Value = selectedTextColor;
                }
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in FileBrowser OnChanges_Postfix: {ex.Message}");
            }
        }
    }
} 
