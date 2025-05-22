using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using System;
using FrooxEngine.UIX;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using ResoniteModLoader;
using System.Threading.Tasks;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(RadiantDash))]
    public static class RadiantDash_Patch
    {
        // Helper method to get the appropriate background color
        private static colorX GetBackgroundColor()
        {
            if (!CustomPlatformColors.ShouldApplyPatch() || CustomPlatformColors.Config == null)
                return UserspaceRadiantDash.DEFAULT_BACKGROUND;

            // If transparent background is enabled, return transparent
            if (CustomPlatformColors.Config.GetValue(CustomPlatformColors.dashTransparentBackground))
                return new colorX(0f, 0f, 0f, 0f);
            
            // Otherwise use the neutralDark config value
            return CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralDark);
        }

        // Patch OnAttach to modify the buttons background after creation
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(RadiantDash __instance)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch())
                    return;

                // Find the Render slot
                Slot renderSlot = __instance.Slot.FindChild("Render");
                if (renderSlot != null)
                {
                    // Patch the buttons background immediately
                    Slot buttonsSlot = renderSlot.FindChild("Buttons");
                    if (buttonsSlot != null)
                    {
                        Canvas buttonsCanvas = buttonsSlot.GetComponent<Canvas>();
                        if (buttonsCanvas != null)
                        {
                            // Find the background Image component and update its color
                            Image backgroundImage = buttonsCanvas.Slot.GetComponentInChildren<Image>();
                            if (backgroundImage != null)
                            {
                                backgroundImage.Tint.Value = GetBackgroundColor();
                            }
                        }
                    }

                    // Delay TopRoot patching until engine is ready and structure is built
                    __instance.StartTask(async () =>
                    {
                        // Wait for engine to be ready
                        while (!Engine.Current.IsReady)
                        {
                            await Task.Delay(100);
                        }
                        
                        // Wait a bit more for UI structure to be fully built
                        await Task.Delay(500);
                        
                        // Now try to patch TopRoot on the main thread
                        __instance.RunSynchronously(() =>
                        {
                            try
                            {
                                PatchTopRootBackground(__instance, renderSlot);
                            }
                            catch (Exception ex)
                            {
                                ResoniteMod.Error($"[CustomPlatformColors] Error patching TopRoot: {ex.Message}");
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantDash OnAttach_Postfix: {ex.Message}");
            }
        }

        private static void PatchTopRootBackground(RadiantDash instance, Slot renderSlot)
        {
            Slot topRootSlot = renderSlot.FindChild("TopRoot");
            if (topRootSlot != null)
            {
                Slot topSlot = topRootSlot.FindChild("Top");
                if (topSlot != null)
                {
                    Slot containerSlot = topSlot.FindChild("Container");
                    if (containerSlot != null)
                    {
                        // Find the Image component in the container and update its color
                        Image backgroundImage = containerSlot.GetComponentInChildren<Image>();
                        if (backgroundImage != null)
                        {
                            backgroundImage.Tint.Value = GetBackgroundColor();
                        }
                    }
                }
            }
        }

        // Patch OnChanges to handle UV_RectMaterial settings
        [HarmonyPatch("OnChanges")]
        [HarmonyPostfix]
        public static void OnChanges_Postfix(RadiantDash __instance)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch())
                    return;

                // Access the materials through reflection since they are protected
                var topBorderMaterial = AccessTools.Field(typeof(RadiantDash), "_topBorderMaterial").GetValue(__instance) as SyncRef<UV_RectMaterial>;
                var screenBorderMaterial = AccessTools.Field(typeof(RadiantDash), "_screenBorderMaterial").GetValue(__instance) as SyncRef<UV_RectMaterial>;
                var buttonsBorderMaterial = AccessTools.Field(typeof(RadiantDash), "_buttonsBorderMaterial").GetValue(__instance) as SyncRef<UV_RectMaterial>;

                // Configure UV_RectMaterial settings to make borders transparent
                if (topBorderMaterial?.Target != null)
                {
                    topBorderMaterial.Target.OuterColor.Value = new colorX(0f, 0f, 0f, 0f);
                    topBorderMaterial.Target.Sidedness.Value = Sidedness.Back;
                }
                
                if (screenBorderMaterial?.Target != null)
                {
                    screenBorderMaterial.Target.OuterColor.Value = new colorX(0f, 0f, 0f, 0f);
                    screenBorderMaterial.Target.Sidedness.Value = Sidedness.Back;
                }
                
                if (buttonsBorderMaterial?.Target != null)
                {
                    buttonsBorderMaterial.Target.OuterColor.Value = new colorX(0f, 0f, 0f, 0f);
                    buttonsBorderMaterial.Target.Sidedness.Value = Sidedness.Back;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in RadiantDash OnChanges_Postfix: {ex.Message}");
            }
        }
    }
} 

