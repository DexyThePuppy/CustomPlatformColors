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

            __result = CustomPlatformColors.GetInventorySelectedColor();
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
                        // Directories use yellow colors (matching RadiantUI_Constants.Sub.YELLOW and Hero.YELLOW)
                        item.NormalColor.Value = CustomPlatformColors.GetInventoryFolderColor();
                        item.NormalText.Value = CustomPlatformColors.GetInventoryFolderTextColor();
                    }
                    else
                    {
                        // Files use cyan colors (matching RadiantUI_Constants.Sub.CYAN and Hero.CYAN)
                        item.NormalColor.Value = CustomPlatformColors.GetInventoryLinkColor();
                        item.NormalText.Value = CustomPlatformColors.GetInventoryLinkTextColor();
                    }

                    // Both files and directories use green for selection
                    item.SelectedColor.Value = CustomPlatformColors.GetInventorySelectedColor();
                    item.SelectedText.Value = CustomPlatformColors.GetInventorySelectedTextColor();
                }
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in FileBrowser OnChanges_Postfix: {ex.Message}");
            }
        }
    }
} 
