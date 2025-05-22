using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System;
using System.Reflection;
using FrooxEngine.Store;
using EnumsNET;
using SkyFrost.Base;
using System.Threading.Tasks;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(InventoryBrowser))]
    public static class InventoryBrowser_Patch
    {
        // Cache reflection fields for better performance
        private static FieldInfo itemField = typeof(InventoryItemUI).GetField("Item", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo directoryField = typeof(InventoryItemUI).GetField("Directory", BindingFlags.NonPublic | BindingFlags.Instance);

        // Helper methods to access private fields
        private static FrooxEngine.Store.Record GetItem(InventoryItemUI ui)
        {
            return itemField?.GetValue(ui) as FrooxEngine.Store.Record;
        }

        private static RecordDirectory GetDirectory(InventoryItemUI ui)
        {
            return directoryField?.GetValue(ui) as RecordDirectory;
        }

        // Patch static color properties
        [HarmonyPatch("get_DESELECTED_COLOR")]
        [HarmonyPostfix]
        public static void DESELECTED_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryDeselectedColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_SELECTED_COLOR")]
        [HarmonyPostfix]
        public static void SELECTED_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_SELECTED_TEXT")]
        [HarmonyPostfix]
        public static void SELECTED_TEXT_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedTextColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_FOLDER_COLOR")]
        [HarmonyPostfix]
        public static void FOLDER_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_FOLDER_TEXT")]
        [HarmonyPostfix]
        public static void FOLDER_TEXT_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderTextColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_LINK_COLOR")]
        [HarmonyPostfix]
        public static void LINK_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_LINK_TEXT")]
        [HarmonyPostfix]
        public static void LINK_TEXT_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkTextColor, out colorX color))
                __result = color;
        }

        // Patch OnAttach to customize all buttons
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(InventoryBrowser __instance)
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
                UniLog.Error($"[CustomPlatformColors] Error in InventoryBrowser OnAttach_Postfix: {ex.Message}");
            }
        }

        // Patch OnChanges to customize inventory items and their colors
        [HarmonyPatch("OnChanges")]
        [HarmonyPostfix]
        public static void OnChanges_Postfix(InventoryBrowser __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            try
            {
                // Find all InventoryItemUIs and update their colors
                foreach (var item in __instance.Slot.GetComponentsInChildren<InventoryItemUI>())
                {
                    // Check if it's a favorite item first
                    bool isFavorite = false;
                    var record = GetItem(item);
                    if (record != null)
                    {
                        var url = record.GetUrl(__instance.Cloud.Platform);
                        if (url != null)
                        {
                            foreach (FavoriteEntity entity in System.Enum.GetValues(typeof(FavoriteEntity)))
                            {
                                if (url == __instance.Engine.Cloud.Profile.GetCurrentFavorite(entity))
                                {
                                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFavouriteColor, out colorX favColor))
                                    {
                                        item.NormalColor.Value = favColor;
                                        item.SelectedColor.Value = favColor.MulRGB(2f);
                                    }
                                    isFavorite = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!isFavorite)
                    {
                        var directory = GetDirectory(item);
                        if (directory != null)
                        {
                            if (directory.IsLink)
                            {
                                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkColor, out colorX linkColor))
                                    item.NormalColor.Value = linkColor;
                                
                                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkTextColor, out colorX linkTextColor))
                                    item.NormalText.Value = linkTextColor;
                            }
                            else
                            {
                                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderColor, out colorX folderColor))
                                    item.NormalColor.Value = folderColor;
                                
                                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderTextColor, out colorX folderTextColor))
                                    item.NormalText.Value = folderTextColor;
                            }

                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedColor, out colorX selectedColor))
                                item.SelectedColor.Value = selectedColor;

                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedTextColor, out colorX selectedTextColor))
                                item.SelectedText.Value = selectedTextColor;
                        }
                        else
                        {
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryDeselectedColor, out colorX deselectedColor))
                                item.NormalColor.Value = deselectedColor;

                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedColor, out colorX selectedColor))
                                item.SelectedColor.Value = selectedColor;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in InventoryBrowser OnChanges_Postfix: {ex.Message}");
            }
        }
    }
} 

