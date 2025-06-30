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

            __result = CustomPlatformColors.GetInventoryDeselectedColor();
        }

        [HarmonyPatch("get_SELECTED_COLOR")]
        [HarmonyPostfix]
        public static void SELECTED_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            __result = CustomPlatformColors.GetInventorySelectedColor();
        }

        [HarmonyPatch("get_SELECTED_TEXT")]
        [HarmonyPostfix]
        public static void SELECTED_TEXT_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            __result = CustomPlatformColors.GetInventorySelectedTextColor();
        }

        [HarmonyPatch("get_FOLDER_COLOR")]
        [HarmonyPostfix]
        public static void FOLDER_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            __result = CustomPlatformColors.GetInventoryFolderColor();
        }

        [HarmonyPatch("get_FOLDER_TEXT")]
        [HarmonyPostfix]
        public static void FOLDER_TEXT_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            __result = CustomPlatformColors.GetInventoryFolderTextColor();
        }

        [HarmonyPatch("get_LINK_COLOR")]
        [HarmonyPostfix]
        public static void LINK_COLOR_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            __result = CustomPlatformColors.GetInventoryLinkColor();
        }

        [HarmonyPatch("get_LINK_TEXT")]
        [HarmonyPostfix]
        public static void LINK_TEXT_Postfix(ref colorX __result)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            __result = CustomPlatformColors.GetInventoryLinkTextColor();
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
                                    item.NormalColor.Value = CustomPlatformColors.GetInventoryFavouriteColor();
                                    item.SelectedColor.Value = CustomPlatformColors.GetInventoryFavouriteColor().MulRGB(2f);
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
                                item.NormalColor.Value = CustomPlatformColors.GetInventoryLinkColor();
                                item.NormalText.Value = CustomPlatformColors.GetInventoryLinkTextColor();
                            }
                            else
                            {
                                item.NormalColor.Value = CustomPlatformColors.GetInventoryFolderColor();
                                item.NormalText.Value = CustomPlatformColors.GetInventoryFolderTextColor();
                            }

                            item.SelectedColor.Value = CustomPlatformColors.GetInventorySelectedColor();
                            item.SelectedText.Value = CustomPlatformColors.GetInventorySelectedTextColor();
                        }
                        else
                        {
                            item.NormalColor.Value = CustomPlatformColors.GetInventoryDeselectedColor();
                            item.SelectedColor.Value = CustomPlatformColors.GetInventorySelectedColor();
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

