using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Reflection;
using Renderite.Shared;

namespace CustomPlatformColors.Patches
{
    /// <summary>
    /// SessionPermissionController patches. Handles role button customization
    /// for individual permission controllers in the Permissions tab.
    /// These are the buttons that set user roles (Anonymous, Visitor, Contact, Host, etc.)
    /// - Patches SetColors calls to apply custom colors while maintaining functionality
    /// - Maintains visual distinction between selected and unselected roles
    /// </summary>
    [HarmonyPatch(typeof(SessionPermissionController))]
    public static class SessionPermissionController_Patch
    {
        // Cache reflection fields for better performance
        private static FieldInfo getReferenceField = typeof(SessionPermissionController).GetField("GetReference", BindingFlags.Public | BindingFlags.Instance);
        private static FieldInfo rolesButtonsField = typeof(SessionPermissionController).GetField("_rolesButtons", BindingFlags.NonPublic | BindingFlags.Instance);

        // Patch OnCommonUpdate to detect when colors should be applied
        [HarmonyPatch("OnCommonUpdate")]
        [HarmonyPostfix]
        public static void OnCommonUpdate_Postfix(SessionPermissionController __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch())
                return;

            try
            {
                // Apply our custom colors after the original method runs
                ApplyRoleButtonCustomizations(__instance);
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in SessionPermissionController OnCommonUpdate_Postfix: {ex.Message}");
            }
        }

        // Apply custom colors to role buttons while respecting selected state
        private static void ApplyRoleButtonCustomizations(SessionPermissionController instance)
        {
            try
            {
                // Get the reference function and current role
                var getReference = getReferenceField?.GetValue(instance) as System.Func<ReadOnlyRef<PermissionSet>>;
                if (getReference == null) return;

                var readOnlyRef = getReference();
                var world = readOnlyRef?.World;
                if (world == null) return;

                // Get the roles buttons list
                var rolesButtons = rolesButtonsField?.GetValue(instance) as SyncRefList<Button>;
                if (rolesButtons == null) return;

                // Determine visibility permissions
                bool canSeeOtherRoles = world.LocalUser.CanSeeOtherRoles();
                if (instance.AssociatedUser != null && instance.AssociatedUser.IsLocalUser)
                    canSeeOtherRoles = true;

                // Apply custom colors to each role button
                for (int index = 0; index < rolesButtons.Count && index < world.Permissions.Roles.Count; ++index)
                {
                    var button = rolesButtons[index];
                    if (button?.ColorDrivers == null || button.ColorDrivers.Count == 0)
                        continue;

                    var driver = button.ColorDrivers[0];
                    if (driver == null)
                        continue;

                    // Check if this role is currently selected
                    bool isSelected = world.Permissions.Roles[index] == readOnlyRef.Target && canSeeOtherRoles;

                    // Apply our custom colors ONLY, overriding whatever was set before
                    ApplyCustomButtonColors(button, isSelected);
                }
            }
            catch (System.Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in ApplyRoleButtonCustomizations: {ex.Message}");
            }
        }

        // Apply custom colors to a button based on selection state
        private static void ApplyCustomButtonColors(Button button, bool isSelected)
        {
            if (button?.ColorDrivers == null || button.ColorDrivers.Count == 0)
                return;

            var driver = button.ColorDrivers[0];
            if (driver == null) return;

            // Apply colors based on selection state
            if (isSelected)
            {
                // Selected role - use neutralLight or brighter version of normal color
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralLight, out colorX selectedColor))
                    driver.NormalColor.Value = selectedColor;
                else if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX normalColor))
                    driver.NormalColor.Value = normalColor.MulRGB(1.5f);
                else
                    driver.NormalColor.Value = RadiantUI_Constants.TAB_ACTIVE_BACKGROUND_COLOR; // Fallback to original
            }
            else
            {
                // Unselected role - use normal custom color
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX normalColor))
                    driver.NormalColor.Value = normalColor;
                else
                    driver.NormalColor.Value = RadiantUI_Constants.TAB_INACTIVE_BACKGROUND_COLOR; // Fallback to original
            }

            // Apply other button state colors
            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hoverColor))
                driver.HighlightColor.Value = hoverColor;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonPressColor, out colorX pressColor))
                driver.PressColor.Value = pressColor;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonDisabledColor, out colorX disabledColor))
                driver.DisabledColor.Value = disabledColor;

            // Update button text color with proper contrast
            var text = button.Slot.GetComponentInChildren<Text>();
            if (text != null)
            {
                if (isSelected && CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX selectedTextColor))
                {
                    // Use neutralDark for selected text to contrast with neutralLight background
                    text.Color.Value = selectedTextColor;
                }
                else if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
                {
                    text.Color.Value = textColor;
                }
            }
        }
    }
} 
