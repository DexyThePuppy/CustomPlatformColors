using HarmonyLib;
using FrooxEngine.UIX;
using Elements.Core;
using CustomPlatformColors;
using FrooxEngine;
using System;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(Button))]
    public static class Button_Patch
    {
        // Patch the SetupBackgroundColor method which constructs the primary ColorDriver.
        [HarmonyPatch("SetupBackgroundColor")]
        [HarmonyPostfix]
        public static void SetupBackgroundColor_Postfix(Button __instance)
        {
            try
            {
                var cfg = CustomPlatformColors.Config;
                if (cfg == null ||
                    !cfg.GetValue(CustomPlatformColors.enabled) ||
                    !cfg.GetValue(CustomPlatformColors.patchUIButtonColors))
                    return;

                // Ownership check – only touch buttons we own / spawned
                if (!IsLocal(__instance))
                    return;

                if (__instance.ColorDrivers == null || __instance.ColorDrivers.Count == 0)
                    return;

                var driver = __instance.ColorDrivers[0];
                if (driver == null)
                    return;

                // Override colours locally via ValueUserOverride so the base sync value remains unchanged for others.
                if (cfg.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX normal))
                    ApplyLocalOverride(driver.NormalColor, normal, __instance);

                if (cfg.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hover))
                    ApplyLocalOverride(driver.HighlightColor, hover, __instance);

                // Generate press & disabled colours from provided ones (local-only as well)
                colorX press = cfg.TryGetValue(CustomPlatformColors.buttonHoverColor, out hover)
                    ? MathX.LerpUnclamped(hover, colorX.White, 0.4f)
                    : (colorX)driver.PressColor;
                ApplyLocalOverride(driver.PressColor, press, __instance);

                colorX disabledBase = cfg.TryGetValue(CustomPlatformColors.buttonNormalColor, out normal) ? normal : (colorX)driver.NormalColor;
                colorX disabled = new colorX(disabledBase.r, disabledBase.g, disabledBase.b, 0.25f, disabledBase.profile);
                ApplyLocalOverride(driver.DisabledColor, disabled, __instance);

                // Force immediate visual refresh for local client only (does not sync)
                driver.UpdateColor(__instance.BaseColor.Value, __instance.Enabled, false, false);
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Button patch error: {ex.Message}");
            }
        }

        private static bool IsLocal(Button btn)
        {
            Slot slot = btn.Slot;
            if (slot == null) return false;
            if (slot.IsLocalElement) return true;
            if (slot.World == null) return false;

            slot.ReferenceID.ExtractIDs(out ulong pos, out byte usr);
            var owner = slot.World.GetUserByAllocationID(usr);
            return owner != null && owner == slot.World.LocalUser && pos >= owner.AllocationIDStart;
        }

        private static void ApplyLocalOverride(Sync<colorX> field, colorX value, Button btn)
        {
            // Check if override already exists targeting this field
            foreach (var existing in btn.Slot.GetComponents<ValueUserOverride<colorX>>())
            {
                if (existing.Target.Target == field)
                {
                    existing.SetOverride(btn.LocalUser, value);
                    return;
                }
            }

            var ov = btn.Slot.AttachComponent<ValueUserOverride<colorX>>();
            ov.Target.Target = field;
            ov.PersistentOverrides.Value = false; // don't save
            // Preserve original colour for other users
            ov.Default.Value = field.Value;
            ov.SetOverride(btn.LocalUser, value);
        }
    }
} 
