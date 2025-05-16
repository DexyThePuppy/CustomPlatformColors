using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(InspectorPanel))]
    public static class InspectorPanel_Patch
    {
        [HarmonyPatch("Setup")]
        [HarmonyPrefix]
        public static void Setup_Prefix(ref colorX hierarchy, ref colorX workers)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInspectorPanel))
                return;

            // Modify the input parameters to change the panel colors
            if (ColorReplacer.IsLocalUserUIContext())
            {
                // Use hero colors for better visibility
                hierarchy = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralDark);
                workers = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralMid);
            }
        }

        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(InspectorPanel __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchInspectorPanel))
                return;

            // Check if the inspector is owned by the local user
            if (__instance?.Slot?.World == null)
                return;

            var slot = __instance.Slot;
            var world = slot.World;

            // Get the allocating user
            slot.ReferenceID.ExtractIDs(out var position, out var user);
            User userByAllocationID = world.GetUserByAllocationID(user);

            if (userByAllocationID == null || userByAllocationID != world.LocalUser || position < userByAllocationID.AllocationIDStart)
                return;

            // We'll attempt to find and color any panels that may have already been created
            // This is a backup in case the prefix patch doesn't catch them
            var images = slot.GetComponentsInChildren<Image>();
            if (images == null)
                return;

            foreach (var image in images)
            {
                if (image == null) continue;

                var color = image.Tint.Value;
                
                // Identify and modify panel colors
                if (color.a == 0.9f)
                {
                    // Most panels in inspectors have 0.9 alpha
                    // We'll check for panels close to neutral colors
                    if (ColorIsSimilar(color, RadiantUI_Constants.Neutrals.DARK))
                    {
                        image.Tint.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralDark).SetA(0.9f);
                    }
                    else if (ColorIsSimilar(color, RadiantUI_Constants.Neutrals.MID))
                    {
                        image.Tint.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralMid).SetA(0.9f);
                    }
                }
            }
        }

        // Helper method to check if colors are similar
        private static bool ColorIsSimilar(colorX a, colorX b, float threshold = 0.15f)
        {
            return MathX.Abs(a.r - b.r) < threshold && 
                   MathX.Abs(a.g - b.g) < threshold && 
                   MathX.Abs(a.b - b.b) < threshold;
        }
    }
} 

