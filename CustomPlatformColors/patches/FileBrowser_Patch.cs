using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Reflection;
using System.Threading.Tasks;

namespace CustomPlatformColors.Patches
{
    /// <summary>
    /// FileBrowser-specific patches. Common button and background panel customization 
    /// is now handled by BrowserDialog_Patch.cs to avoid code duplication.
    /// This patch handles only FileBrowser-specific functionality:
    /// - SELECTED_COLOR property override
    /// - FileSystemItem color customization in OnChanges
    /// </summary>
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
