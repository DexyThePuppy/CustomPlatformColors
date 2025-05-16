using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System;
using System.Reflection;
using FrooxEngine.Store;
using EnumsNET;
using SkyFrost.Base;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(InventoryBrowser))]
    public static class InventoryBrowser_Patch
    {
        // Patch all static color properties
        [HarmonyPatch("get_DESELECTED_COLOR")]
        [HarmonyPostfix]
        public static void DeselectedColor_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryDeselectedColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_SELECTED_COLOR")]
        [HarmonyPostfix]
        public static void SelectedColor_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_SELECTED_TEXT")]
        [HarmonyPostfix]
        public static void SelectedText_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventorySelectedTextColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_FOLDER_COLOR")]
        [HarmonyPostfix]
        public static void FolderColor_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_FOLDER_TEXT")]
        [HarmonyPostfix]
        public static void FolderText_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFolderTextColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_LINK_COLOR")]
        [HarmonyPostfix]
        public static void LinkColor_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_LINK_TEXT")]
        [HarmonyPostfix]
        public static void LinkText_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkTextColor, out colorX color))
                __result = color;
        }

        [HarmonyPatch("get_FAVORITE_COLOR")]
        [HarmonyPostfix]
        public static void FavoriteColor_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFavouriteColor, out colorX color))
                __result = color;
        }

        // Patch the OnItemSelected method to customize button colors in the inventory view
        [HarmonyPatch("OnItemSelected")]
        [HarmonyPostfix]
        public static void OnItemSelected_Postfix(InventoryBrowser __instance, SyncRef<Button> ____inventoriesButton) 
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser) ||
                ____inventoriesButton.Target?.Slot?.Parent == null) 
                return;
            
            try
            {
                foreach (Slot child in ____inventoriesButton.Target.Slot.Parent.Children) {
                    Button buttonComp = child.GetComponent<Button>();
                    if (buttonComp?.ColorDrivers == null || buttonComp.ColorDrivers.Count == 0) 
                        continue;

                    var driver = buttonComp.ColorDrivers[0];
                    if (driver == null) 
                        continue;

                    // Apply custom button colors
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryButtonsNormalColor, out colorX normalColor))
                        driver.NormalColor.Value = normalColor;
                    
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryButtonsHighlightedColor, out colorX highlightColor))
                        driver.HighlightColor.Value = highlightColor;
                    
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryButtonsPressColor, out colorX pressColor))
                        driver.PressColor.Value = pressColor;
                    
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryButtonsDisabledColor, out colorX disabledColor))
                        driver.DisabledColor.Value = disabledColor;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in InventoryBrowser OnItemSelected_Postfix: {ex.Message}");
            }
        }

        // Patch ProcessItem to ensure coloring is consistent even for dynamic items
        [HarmonyPatch("ProcessItem")]
        [HarmonyPostfix]
        public static void ProcessItem_Postfix(InventoryItemUI item) 
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInventoryBrowser) ||
                item == null) 
                return;

            try {
                // Get the record from the private field
                FieldInfo recordField = item.GetType().GetField("Item", BindingFlags.NonPublic | BindingFlags.Instance);
                if (recordField == null) 
                    return;

                var recordValue = recordField.GetValue(item) as FrooxEngine.Store.Record;
                if (recordValue == null || item.Cloud?.Platform == null) 
                    return;

                // Check if it's a favorite
                Uri? uri = recordValue.GetUrl(item.Cloud.Platform);
                if (uri != null) {
                    foreach (FavoriteEntity value in Enums.GetValues<FavoriteEntity>()) {
                        if (uri == item.Engine.Cloud.Profile.GetCurrentFavorite(value)) {
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryFavouriteColor, out colorX favColor)) {
                                item.NormalColor.Value = favColor;
                                item.SelectedColor.Value = favColor.MulRGB(2f);
                            }
                            return;
                        }
                    }
                }

                // Check if it's a directory
                FieldInfo directoryField = item.GetType().GetField("Directory", BindingFlags.NonPublic | BindingFlags.Instance);
                if (directoryField != null) {
                    RecordDirectory? directoryValue = directoryField.GetValue(item) as RecordDirectory;

                    if (directoryValue != null) {
                        if (directoryValue.IsLink) {
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkColor, out colorX linkColor))
                                item.NormalColor.Value = linkColor;
                            
                            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryLinkTextColor, out colorX linkTextColor))
                                item.NormalText.Value = linkTextColor;
                        } 
                        else {
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
                    else {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.inventoryDeselectedColor, out colorX deselectedColor)) {
                            item.NormalColor.Value = deselectedColor;
                            item.SelectedColor.Value = deselectedColor;
                        }
                    }
                }
            }
            catch (Exception ex) {
                UniLog.Error($"[CustomPlatformColors] Error in ProcessItem_Postfix: {ex.Message}");
            }
        }
    }
} 

