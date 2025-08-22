using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using SkyFrost.Base;
using FrooxEngine.UIX;
using Renderite.Shared;

namespace CustomPlatformColors
{
    [HarmonyPatch(typeof(LegacyWorldThumbnailItem))]
    public static class LegacyWorldThumbnailItem_Patch
    {
        private static AccessTools.FieldRef<LegacyWorldThumbnailItem, SyncRef<Image>> _borderOverlayRef = 
            AccessTools.FieldRefAccess<LegacyWorldThumbnailItem, SyncRef<Image>>("_borderOverlay");
            
        private static AccessTools.FieldRef<LegacyWorldThumbnailItem, Sync<colorX>> _borderColorRef = 
            AccessTools.FieldRefAccess<LegacyWorldThumbnailItem, Sync<colorX>>("_borderColor");

        private static AccessTools.FieldRef<LegacyWorldThumbnailItem, SyncRef<RectTransform>> _visitedRootRef =
            AccessTools.FieldRefAccess<LegacyWorldThumbnailItem, SyncRef<RectTransform>>("_visitedRoot");

        private static AccessTools.FieldRef<LegacyWorldThumbnailItem, SyncRef<RectTransform>> _closeButtonRef =
            AccessTools.FieldRefAccess<LegacyWorldThumbnailItem, SyncRef<RectTransform>>("_closeButton");

        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(LegacyWorldThumbnailItem __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() || CustomPlatformColors.Config == null)
                return;

            var borderOverlay = _borderOverlayRef(__instance)?.Target;
            if (borderOverlay == null)
                return;

            // Replace the default blue-ish tint (0.8f, 1.1f, 1.25f) with our neutral light color
            borderOverlay.Tint.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralLight);

            // Find and update the close button colors
            var closeButtonRect = _closeButtonRef(__instance)?.Target;
            if (closeButtonRect != null)
            {
                var parentRect = closeButtonRect.Parent as RectTransform;
                if (parentRect != null)
                {
                    var closeImage = parentRect.Slot.GetComponent<Image>();
                    var closeButton = parentRect.Slot.GetComponent<Button>();
                    
                    if (closeButton != null && closeImage != null)
                    {
                        var heroRed = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroRed);
                        var baseColor = heroRed * 0.8f; // Slightly dimmer for normal state
                        
                        // Set up the button colors using the color driver system
                        closeButton.SetupBackgroundColor(closeImage.Tint);
                        
                        // Configure the color driver
                        var colorDriver = closeButton.ColorDrivers[0];
                        colorDriver.NormalColor.Value = baseColor;
                        colorDriver.HighlightColor.Value = heroRed * 1.2f; // Brighter for hover
                        colorDriver.PressColor.Value = heroRed * 0.6f; // Darker for press
                        colorDriver.DisabledColor.Value = heroRed * 0.3f; // Very dim for disabled
                    }
                }
            }
        }

        [HarmonyPatch("SetupVisual")]
        [HarmonyPrefix]
        public static void SetupVisual_Prefix(ref colorX tint)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() || CustomPlatformColors.Config == null)
                return;

            // Replace black tint with neutral dark color
            if (tint == colorX.Black.SetA(0.9f))
            {
                tint = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralDark).SetA(0.9f);
            }
            // Replace dark gray tint with neutral mid color
            else if (tint == colorX.DarkGray.SetA(0.9f))
            {
                tint = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralMid).SetA(0.9f);
            }
            // Replace red tint (close button) with warning red color
            else if (tint == colorX.Red.SetA(0.9f))
            {
                tint = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroRed).SetA(0.9f);
            }
        }

        [HarmonyPatch("UpdateInfo")]
        [HarmonyPostfix]
        public static void UpdateInfo_Postfix(LegacyWorldThumbnailItem __instance, FrooxEngine.Store.Record record, System.Collections.Generic.IReadOnlyList<SessionInfo> sessions, System.Collections.Generic.IReadOnlyList<World> openedWorlds)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() || CustomPlatformColors.Config == null)
                return;

            // Get the border color field
            var borderColorField = _borderColorRef(__instance);
            if (borderColorField == null)
                return;

            bool isAuthority = openedWorlds?.Count > 0 && openedWorlds[0]?.IsAuthority == true;

            // Replace the hardcoded colors with our custom colors based on the same conditions
            if (isAuthority)
            {
                // For hosting worlds - use hero yellow
                borderColorField.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroYellow);
            }
            else if (openedWorlds?.Count > 0)
            {
                // For joined worlds - use hero orange
                borderColorField.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroOrange);
            }
            else if (sessions?.Count > 0)
            {
                // For available sessions - use hero purple
                borderColorField.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroPurple);
            }
            else
            {
                // For regular worlds - use hero cyan
                borderColorField.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroCyan);
            }

            // Call UpdateBorderColor to apply the hover/press state
            var button = __instance.Slot.GetComponent<Button>();
            var borderOverlay = _borderOverlayRef(__instance)?.Target;
            if (button != null && borderOverlay != null)
            {
                bool isHovering = button.IsHovering.Value;
                bool isPressed = button.IsPressed.Value;

                colorX baseColor = borderColorField.Value;
                colorX finalColor;

                // Apply hover and press effects
                if (isHovering)
                {
                    finalColor = isPressed ? baseColor * 2f : baseColor;
                    finalColor = finalColor.SetA(1f);
                }
                else
                {
                    // When not hovering, use a dimmed version of the base color
                    finalColor = baseColor * 0.25f;
                    finalColor = finalColor.SetA(0.75f);
                }

                borderOverlay.Tint.Value = finalColor;
            }
        }

        [HarmonyPatch("UpdateBorderColor")]
        [HarmonyPrefix]
        public static bool UpdateBorderColor_Prefix(LegacyWorldThumbnailItem __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() || CustomPlatformColors.Config == null)
                return true;

            var button = __instance.Slot.GetComponent<Button>();
            var borderOverlay = _borderOverlayRef(__instance)?.Target;
            var borderColor = _borderColorRef(__instance);

            if (button == null || borderOverlay == null || borderColor == null)
                return true;

            bool isHovering = button.IsHovering.Value;
            bool isPressed = button.IsPressed.Value;

            colorX baseColor = borderColor.Value;
            colorX finalColor;

            // Apply hover and press effects
            if (isHovering)
            {
                finalColor = isPressed ? baseColor * 2f : baseColor;
                finalColor = finalColor.SetA(1f);
            }
            else
            {
                // When not hovering, use a dimmed version of the base color
                finalColor = baseColor * 0.25f;
                finalColor = finalColor.SetA(0.75f);
            }

            borderOverlay.Tint.Value = finalColor;

            // Return false to skip original method
            return false;
        }
    }
} 
