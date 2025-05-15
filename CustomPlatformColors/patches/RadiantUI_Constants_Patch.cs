using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using CustomPlatformColors;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch]
    public static class ColorReplacer
    {
        [HarmonyPatch(typeof(RadiantUI_Constants), "GetTintedButton")]
        [HarmonyPrefix]
        public static bool GetTintedButton_Prefix(ref colorX __result, colorX tint)
        {
           // UniLog.Log($"[CustomPlatformColors] GetTintedButton called with tint: {tint}");
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
            {
                UniLog.Log("[CustomPlatformColors] Config is null or disabled, skipping GetTintedButton patch");
                return true;
            }

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hoverColor))
            {
               // UniLog.Log($"[CustomPlatformColors] Applying custom hover color: {hoverColor}");
                __result = hoverColor;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "get_BUTTON_COLOR")]
        [HarmonyPostfix]
        public static void ButtonColor_Postfix(ref colorX __result)
        {
           // UniLog.Log("[CustomPlatformColors] BUTTON_COLOR getter called");
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
            {
                UniLog.Log("[CustomPlatformColors] Config is null or disabled, skipping BUTTON_COLOR patch");
                return;
            }

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX buttonColor))
            {
               // UniLog.Log($"[CustomPlatformColors] Applying custom button color: {buttonColor}");
                __result = buttonColor;
            }
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "get_TEXT_COLOR")]
        [HarmonyPostfix]
        public static void TextColor_Postfix(ref colorX __result)
        {
           // UniLog.Log("[CustomPlatformColors] TEXT_COLOR getter called");
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
            {
                UniLog.Log("[CustomPlatformColors] Config is null or disabled, skipping TEXT_COLOR patch");
                return;
            }

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
            {
              //  UniLog.Log($"[CustomPlatformColors] Applying custom text color: {textColor}");
                __result = textColor;
            }
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupBaseStyle")]
        [HarmonyPostfix]
        public static void SetupBaseStyle_Postfix(UIBuilder ui)
        {
          //  UniLog.Log($"[CustomPlatformColors] SetupBaseStyle called for UI builder: {ui?.Current?.Name ?? "null"}");
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
            {
                UniLog.Log("[CustomPlatformColors] Config is null or disabled, skipping SetupBaseStyle patch");
                return;
            }

            // Check if the UI element is owned by the local user
            if (ui?.Current == null || !CheckLocalUserOwnership(ui.Current))
            {
             //   UniLog.Log($"[CustomPlatformColors] UI element not owned by local user: {ui?.Current?.Name ?? "null"}");
                return;
            }

           // UniLog.Log("[CustomPlatformColors] Updating all colors in SetupBaseStyle");
            UpdateAllColors();
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupDefaultStyle")]
        [HarmonyPrefix]
        public static void SetupDefaultStyle_Prefix()
        {
           // UniLog.Log("[CustomPlatformColors] SetupDefaultStyle called");
            try
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                {
                    UniLog.Log("[CustomPlatformColors] Config is null or disabled, skipping SetupDefaultStyle patch");
                    return;
                }

                // Check if we're in an inspector context by looking at the current stack trace
                var stackTrace = new System.Diagnostics.StackTrace();
                bool isInspectorContext = false;
                foreach (var frame in stackTrace.GetFrames())
                {
                    var method = frame.GetMethod();
                    if (method?.DeclaringType != null)
                    {
                        var type = method.DeclaringType;
                        if (typeof(InspectorPanel).IsAssignableFrom(type))
                        {
                            isInspectorContext = true;
                            break;
                        }
                    }
                }

                // If we're in an inspector context, check if it belongs to the local user
                if (isInspectorContext)
                {
                    UniLog.Log("[CustomPlatformColors] Inspector context detected, checking ownership");
                    var world = Engine.Current.WorldManager.FocusedWorld;
                    if (world == null)
                    {
                        UniLog.Log("[CustomPlatformColors] World is null, skipping color updates");
                        return;
                    }

                    // Get the current inspector from the stack trace
                    InspectorPanel? currentInspector = null;
                    foreach (var frame in stackTrace.GetFrames())
                    {
                        var method = frame.GetMethod();
                        if (method?.DeclaringType != null && typeof(InspectorPanel).IsAssignableFrom(method.DeclaringType))
                        {
                            // Try to get the inspector instance from the stack frame
                            try
                            {
                                if (frame.GetMethod()?.GetParameters().FirstOrDefault()?.Name == "__instance")
                                {
                                    var instance = frame.GetMethod()?.Invoke(null, new object[] { frame });
                                    if (instance is InspectorPanel inspector)
                                    {
                                        currentInspector = inspector;
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                UniLog.Warning($"[CustomPlatformColors] Error getting inspector instance: {ex.Message}");
                            }
                        }
                    }

                    if (currentInspector?.Slot == null)
                    {
                        UniLog.Log("[CustomPlatformColors] Could not find current inspector, skipping color updates");
                        return;
                    }

                    // Check if the inspector belongs to the local user
                    currentInspector.Slot.ReferenceID.ExtractIDs(out var position, out var user);
                    User userByAllocationID = world.GetUserByAllocationID(user);

                    if (userByAllocationID == null || position < userByAllocationID.AllocationIDStart || userByAllocationID != world.LocalUser)
                    {
                        UniLog.Log("[CustomPlatformColors] Inspector not owned by local user, skipping color updates");
                        return;
                    }
                }

               // UniLog.Log("[CustomPlatformColors] Updating colors in SetupDefaultStyle");
                UpdateAllColors();
            }
            catch (Exception e)
            {
                UniLog.Error($"Error updating RadiantUI colors in SetupDefaultStyle: {e}");
            }
        }

        private static void UpdateAllColors()
        {
           // UniLog.Log("[CustomPlatformColors] UpdateAllColors called");
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
            {
                UniLog.Log("[CustomPlatformColors] Config is null or disabled, skipping UpdateAllColors");
                return;
            }

            try
            {
                // Check if we're in an inspector context by looking at the current stack trace
                var stackTrace = new System.Diagnostics.StackTrace();
                bool isInspectorContext = false;
                foreach (var frame in stackTrace.GetFrames())
                {
                    var method = frame.GetMethod();
                    if (method?.DeclaringType != null)
                    {
                        var type = method.DeclaringType;
                        if (typeof(InspectorPanel).IsAssignableFrom(type))
                        {
                            isInspectorContext = true;
                            break;
                        }
                    }
                }

                // If we're in an inspector context, check if it belongs to the local user
                if (isInspectorContext)
                {
                    UniLog.Log("[CustomPlatformColors] Inspector context detected, checking ownership");
                    var world = Engine.Current.WorldManager.FocusedWorld;
                    if (world == null)
                    {
                        UniLog.Log("[CustomPlatformColors] World is null, skipping color updates");
                        return;
                    }

                    // Get the current inspector from the stack trace
                    InspectorPanel? currentInspector = null;
                    foreach (var frame in stackTrace.GetFrames())
                    {
                        var method = frame.GetMethod();
                        if (method?.DeclaringType != null && typeof(InspectorPanel).IsAssignableFrom(method.DeclaringType))
                        {
                            // Try to get the inspector instance from the stack frame
                            try
                            {
                                if (frame.GetMethod()?.GetParameters().FirstOrDefault()?.Name == "__instance")
                                {
                                    var instance = frame.GetMethod()?.Invoke(null, new object[] { frame });
                                    if (instance is InspectorPanel inspector)
                                    {
                                        currentInspector = inspector;
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                UniLog.Warning($"[CustomPlatformColors] Error getting inspector instance: {ex.Message}");
                            }
                        }
                    }

                    if (currentInspector?.Slot == null)
                    {
                        UniLog.Log("[CustomPlatformColors] Could not find current inspector, skipping color updates");
                        return;
                    }

                    // Check if the inspector belongs to the local user
                    currentInspector.Slot.ReferenceID.ExtractIDs(out var position, out var user);
                    User userByAllocationID = world.GetUserByAllocationID(user);

                    if (userByAllocationID == null || position < userByAllocationID.AllocationIDStart || userByAllocationID != world.LocalUser)
                    {
                        UniLog.Log("[CustomPlatformColors] Inspector not owned by local user, skipping color updates");
                        return;
                    }
                }

               // UniLog.Log("[CustomPlatformColors] Starting color updates");
                var flags = BindingFlags.Public | BindingFlags.Static;

                // Update neutral colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX darkColor))
                    typeof(RadiantUI_Constants.Neutrals).GetField("DARK", flags).SetValue(null, darkColor);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralMid, out colorX midColor))
                    typeof(RadiantUI_Constants.Neutrals).GetField("MID", flags).SetValue(null, midColor);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralLight, out colorX lightColor))
                    typeof(RadiantUI_Constants.Neutrals).GetField("LIGHT", flags).SetValue(null, lightColor);

                // Update hero colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroYellow, out colorX heroYellow))
                    typeof(RadiantUI_Constants.Hero).GetField("YELLOW", flags).SetValue(null, heroYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroGreen, out colorX heroGreen))
                    typeof(RadiantUI_Constants.Hero).GetField("GREEN", flags).SetValue(null, heroGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroRed, out colorX heroRed))
                    typeof(RadiantUI_Constants.Hero).GetField("RED", flags).SetValue(null, heroRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroPurple, out colorX heroPurple))
                    typeof(RadiantUI_Constants.Hero).GetField("PURPLE", flags).SetValue(null, heroPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroCyan, out colorX heroCyan))
                    typeof(RadiantUI_Constants.Hero).GetField("CYAN", flags).SetValue(null, heroCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroOrange, out colorX heroOrange))
                    typeof(RadiantUI_Constants.Hero).GetField("ORANGE", flags).SetValue(null, heroOrange);

                // Update sub colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subYellow, out colorX subYellow))
                    typeof(RadiantUI_Constants.Sub).GetField("YELLOW", flags).SetValue(null, subYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subGreen, out colorX subGreen))
                    typeof(RadiantUI_Constants.Sub).GetField("GREEN", flags).SetValue(null, subGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subRed, out colorX subRed))
                    typeof(RadiantUI_Constants.Sub).GetField("RED", flags).SetValue(null, subRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subPurple, out colorX subPurple))
                    typeof(RadiantUI_Constants.Sub).GetField("PURPLE", flags).SetValue(null, subPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subCyan, out colorX subCyan))
                    typeof(RadiantUI_Constants.Sub).GetField("CYAN", flags).SetValue(null, subCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subOrange, out colorX subOrange))
                    typeof(RadiantUI_Constants.Sub).GetField("ORANGE", flags).SetValue(null, subOrange);

                // Update dark colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkYellow, out colorX darkYellow))
                    typeof(RadiantUI_Constants.Dark).GetField("YELLOW", flags).SetValue(null, darkYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkGreen, out colorX darkGreen))
                    typeof(RadiantUI_Constants.Dark).GetField("GREEN", flags).SetValue(null, darkGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkRed, out colorX darkRed))
                    typeof(RadiantUI_Constants.Dark).GetField("RED", flags).SetValue(null, darkRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkPurple, out colorX darkPurple))
                    typeof(RadiantUI_Constants.Dark).GetField("PURPLE", flags).SetValue(null, darkPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkCyan, out colorX darkCyan))
                    typeof(RadiantUI_Constants.Dark).GetField("CYAN", flags).SetValue(null, darkCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkOrange, out colorX darkOrange))
                    typeof(RadiantUI_Constants.Dark).GetField("ORANGE", flags).SetValue(null, darkOrange);
            }
            catch (Exception e)
            {
                UniLog.Error($"Error updating RadiantUI colors in UpdateAllColors: {e}");
            }
        }

        private static bool CheckLocalUserOwnership(Slot slot)
        {
           // UniLog.Log($"[CustomPlatformColors] Checking ownership for: {slot?.Name ?? "null"}");
            if (slot?.World == null)
            {
                UniLog.Log("[CustomPlatformColors] Slot or World is null");
                return true;
            }

            // Check if this is an inspector element
            bool isInspector = slot.GetComponentInParents<InspectorPanel>() != null || slot.Tag == "Developer";
            if (!isInspector)
            {
              //  UniLog.Log("[CustomPlatformColors] Not an inspector element, skipping ownership check");
                return true;
            }

            slot.ReferenceID.ExtractIDs(out var position, out var user);
            User userByAllocationID = slot.World.GetUserByAllocationID(user);

            if (userByAllocationID == null)
            {
                UniLog.Log("[CustomPlatformColors] Could not find user by allocation ID");
                return true;
            }

            // Filter out users who left and then someone joined with their id
            if (position < userByAllocationID.AllocationIDStart)
            {
                UniLog.Log("[CustomPlatformColors] Element not owned by local user (position check)");
                return false;
            }

            // Check if the element is owned by local user
            if (userByAllocationID != slot.World.LocalUser)
            {
                UniLog.Log("[CustomPlatformColors] Element not owned by local user");
                return false;
            }

            UniLog.Log("[CustomPlatformColors] Element is owned by local user");
            return true;
        }
    }
}
