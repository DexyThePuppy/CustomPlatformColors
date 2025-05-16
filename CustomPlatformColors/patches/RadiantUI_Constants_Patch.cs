using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using CustomPlatformColors;
using ResoniteModLoader;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch]
    public static class ColorReplacer
    {
        private static readonly AsyncLocal<bool> IsLocalUserContext = new AsyncLocal<bool>();
        private static readonly BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.Static;
        private static readonly Stack<Dictionary<FieldInfo, object>> _colorBackupStack = new();

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupBaseStyle")]
        [HarmonyPrefix]
        public static void SetupBaseStyle_Prefix(UIBuilder ui)
        {
            if (ui?.Current == null)
                return;

            // Determine ownership context
            bool isLocal = CheckLocalUserOwnership(ui.Current);
            IsLocalUserContext.Value = isLocal;

            if (!ShouldApplyCustomColors())
                return;

            // Backup & apply overrides
            var backup = new Dictionary<FieldInfo, object>();
            _colorBackupStack.Push(backup);
            ApplyCustomColors(backup);
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupBaseStyle")]
        [HarmonyPostfix]
        public static void SetupBaseStyle_Postfix(UIBuilder ui)
        {
            if (!ShouldApplyCustomColors())
            {
                IsLocalUserContext.Value = false;
                return;
            }

            if (_colorBackupStack.Count > 0)
            {
                var backup = _colorBackupStack.Pop();
                RestoreColors(backup);
            }

            IsLocalUserContext.Value = false;
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupDefaultStyle")]
        [HarmonyPrefix]
        public static void SetupDefaultStyle_Prefix()
        {
            IsLocalUserContext.Value = IsLocalUserUIContext();
            if (!ShouldApplyCustomColors())
                return;
            var backup = new Dictionary<FieldInfo, object>();
            _colorBackupStack.Push(backup);
            ApplyCustomColors(backup);
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupDefaultStyle")]
        [HarmonyPostfix]
        public static void SetupDefaultStyle_Postfix()
        {
            if (!ShouldApplyCustomColors())
            {
                IsLocalUserContext.Value = false;
                return;
            }
            if (_colorBackupStack.Count > 0)
            {
                var backup = _colorBackupStack.Pop();
                RestoreColors(backup);
            }
            IsLocalUserContext.Value = false;
        }

        // Helper method to check if custom colors should be applied
        private static bool ShouldApplyCustomColors()
        {
            return CustomPlatformColors.Config != null &&
                   CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled) &&
                   CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchRadiantUIConstants) &&
                   IsLocalUserContext.Value;
        }

        public static bool IsLocalUserUIContext()
        {
            try
            {
                var world = Engine.Current?.WorldManager?.FocusedWorld;
                if (world == null)
                    return false;

                // Get the current UI context from the call stack
                var stackTrace = new System.Diagnostics.StackTrace();
                foreach (var frame in stackTrace.GetFrames() ?? Array.Empty<System.Diagnostics.StackFrame>())
                {
                    if (frame == null) continue;
                    
                    var method = frame.GetMethod();
                    if (method?.DeclaringType == null) continue;

                    // Check if we're in an inspector or developer UI context
                    if (typeof(InspectorPanel).IsAssignableFrom(method.DeclaringType))
                    {
                        var slot = GetSlotFromStackFrame(frame);
                        if (slot != null)
                            return CheckLocalUserOwnership(slot);
                    }
                }

                // If we can't determine the context, assume it's safe to apply colors
                return true;
            }
            catch (Exception e)
            {
                UniLog.Error($"Error in IsLocalUserUIContext: {e}");
                return false;
            }
        }

        private static Slot? GetSlotFromStackFrame(System.Diagnostics.StackFrame frame)
        {
            if (frame == null) return null;
            
            try
            {
                var method = frame.GetMethod();
                if (method?.GetParameters().FirstOrDefault()?.Name == "__instance")
                {
                    var instance = method.Invoke(null, new object[] { frame });
                    if (instance is InspectorPanel inspector)
                        return inspector.Slot;
                }
            }
            catch (Exception) { /* Ignore errors in reflection */ }
            return null;
        }

        private static bool CheckLocalUserOwnership(Slot slot)
        {
            if (slot == null)
                return false;

            // Fast flag for local-only elements (unsynced)
            if (slot.IsLocalElement)
                return true;

            if (slot.World == null)
                return false;

            // Compare allocation user byte with the local user
            slot.ReferenceID.ExtractIDs(out var position, out byte userByte);
            User? owner = slot.World.GetUserByAllocationID(userByte);
            if (owner == null)
                return false;

            // Ensure element belongs to the local user and the position lies within their allocation range
            return owner == slot.World.LocalUser && position >= owner.AllocationIDStart;
        }

        private static void ApplyCustomColors(Dictionary<FieldInfo, object> backup)
        {
            if (CustomPlatformColors.Config == null)
                return;

            void OverrideColor(System.Type type, string fieldName, ModConfigurationKey<colorX> cfgKey)
            {
                if (!CustomPlatformColors.Config.TryGetValue(cfgKey, out colorX newCol))
                    return;
                FieldInfo fi = type.GetField(fieldName, StaticFlags);
                if (fi == null)
                    return;
                backup[fi] = fi.GetValue(null);
                fi.SetValue(null, newCol);
            }

            // Neutrals
            OverrideColor(typeof(RadiantUI_Constants.Neutrals), "DARK", CustomPlatformColors.neutralDark);
            OverrideColor(typeof(RadiantUI_Constants.Neutrals), "MID", CustomPlatformColors.neutralMid);
            OverrideColor(typeof(RadiantUI_Constants.Neutrals), "LIGHT", CustomPlatformColors.neutralLight);
            // Hero
            OverrideColor(typeof(RadiantUI_Constants.Hero), "YELLOW", CustomPlatformColors.heroYellow);
            OverrideColor(typeof(RadiantUI_Constants.Hero), "GREEN", CustomPlatformColors.heroGreen);
            OverrideColor(typeof(RadiantUI_Constants.Hero), "RED", CustomPlatformColors.heroRed);
            OverrideColor(typeof(RadiantUI_Constants.Hero), "PURPLE", CustomPlatformColors.heroPurple);
            OverrideColor(typeof(RadiantUI_Constants.Hero), "CYAN", CustomPlatformColors.heroCyan);
            OverrideColor(typeof(RadiantUI_Constants.Hero), "ORANGE", CustomPlatformColors.heroOrange);
            // Sub
            OverrideColor(typeof(RadiantUI_Constants.Sub), "YELLOW", CustomPlatformColors.subYellow);
            OverrideColor(typeof(RadiantUI_Constants.Sub), "GREEN", CustomPlatformColors.subGreen);
            OverrideColor(typeof(RadiantUI_Constants.Sub), "RED", CustomPlatformColors.subRed);
            OverrideColor(typeof(RadiantUI_Constants.Sub), "PURPLE", CustomPlatformColors.subPurple);
            OverrideColor(typeof(RadiantUI_Constants.Sub), "CYAN", CustomPlatformColors.subCyan);
            OverrideColor(typeof(RadiantUI_Constants.Sub), "ORANGE", CustomPlatformColors.subOrange);
            // Dark
            OverrideColor(typeof(RadiantUI_Constants.Dark), "YELLOW", CustomPlatformColors.darkYellow);
            OverrideColor(typeof(RadiantUI_Constants.Dark), "GREEN", CustomPlatformColors.darkGreen);
            OverrideColor(typeof(RadiantUI_Constants.Dark), "RED", CustomPlatformColors.darkRed);
            OverrideColor(typeof(RadiantUI_Constants.Dark), "PURPLE", CustomPlatformColors.darkPurple);
            OverrideColor(typeof(RadiantUI_Constants.Dark), "CYAN", CustomPlatformColors.darkCyan);
            OverrideColor(typeof(RadiantUI_Constants.Dark), "ORANGE", CustomPlatformColors.darkOrange);
        }

        private static void RestoreColors(Dictionary<FieldInfo, object> backup)
        {
            foreach (var kvp in backup)
            {
                kvp.Key.SetValue(null, kvp.Value);
            }
        }
    }
}
