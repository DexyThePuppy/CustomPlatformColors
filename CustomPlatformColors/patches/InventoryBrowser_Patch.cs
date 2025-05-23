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
    /// <summary>
    /// InventoryBrowser-specific patches. Common button and background panel customization 
    /// is now handled by BrowserDialog_Patch.cs to avoid code duplication.
    /// This patch handles only InventoryBrowser-specific functionality:
    /// - Static color property overrides (DESELECTED_COLOR, SELECTED_COLOR, etc.)
    /// - InventoryItemUI color customization in OnChanges with favorites/links logic
    /// </summary>
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

