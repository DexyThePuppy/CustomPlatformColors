using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using System;
using System.Reflection;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(UserspaceScreensManager))]
    public static class UserspaceScreensManager_Patch
    {
        // Patch the AddContactsScreen method to customize the background
        [HarmonyPatch("AddContactsScreen")]
        [HarmonyPostfix]
        public static void AddContactsScreen_Postfix(RadiantDash dash)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                // Find the most recently added screen which should be the contacts screen
                var contactsScreen = dash.GetScreen<LegacyRadiantScreenWrapper<ContactsDialog>>();
                if (contactsScreen != null)
                {
                    SetCustomBackground(contactsScreen);
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in AddContactsScreen_Postfix: {ex.Message}");
            }
        }

        // Patch the AddInventoryScreen method to customize the background
        [HarmonyPatch("AddInventoryScreen")]
        [HarmonyPostfix]
        public static void AddInventoryScreen_Postfix(RadiantDash dash)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                // Find the most recently added screen which should be the inventory screen
                var inventoryScreen = dash.GetScreen<LegacyRadiantScreenWrapper<InventoryBrowser>>();
                if (inventoryScreen != null)
                {
                    SetCustomBackground(inventoryScreen);
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in AddInventoryScreen_Postfix: {ex.Message}");
            }
        }

        // Patch the AddFileBrowserScreen method to customize the background
        [HarmonyPatch("AddFileBrowserScreen")]
        [HarmonyPostfix]
        public static void AddFileBrowserScreen_Postfix(RadiantDash dash)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                // Find the most recently added screen which should be the file browser screen
                var fileBrowserScreen = dash.GetScreen<LegacyRadiantScreenWrapper<FileBrowser>>();
                if (fileBrowserScreen != null)
                {
                    SetCustomBackground(fileBrowserScreen);
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in AddFileBrowserScreen_Postfix: {ex.Message}");
            }
        }

        // Helper method to apply custom background to a screen
        private static void SetCustomBackground<T>(LegacyRadiantScreenWrapper<T> screen) where T : Component, new()
        {
            if (CustomPlatformColors.Config == null)
                return;
                
            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.dashScreenBackgroundColor, out colorX bgColor))
            {
                // Apply custom background color
                screen.Background.Value = bgColor.SetA(0.965f); // Keep same alpha as default
            }
        }
    }
} 

