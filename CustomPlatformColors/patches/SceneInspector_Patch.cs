using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Linq;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(SceneInspector))]
    public static class SceneInspector_Patch 
    {
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(SceneInspector __instance)
        {
            if (!CustomPlatformColors.ShouldApplyPatch() || 
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchSceneInspector))
                return;

            // Check if the inspector is owned by the local user
            if (__instance?.Slot?.World == null)
            {
                UniLog.Log("[CustomPlatformColors] Inspector, Slot or World is null, skipping patch");
                return;
            }

            __instance.Slot.ReferenceID.ExtractIDs(out var position, out var user);
            User userByAllocationID = __instance.Slot.World.GetUserByAllocationID(user);

            if (userByAllocationID == null)
            {
                UniLog.Log("[CustomPlatformColors] Could not find user by allocation ID, skipping patch");
                return;
            }

            // Filter out users who left and then someone joined with their id
            if (position < userByAllocationID.AllocationIDStart)
            {
                UniLog.Log("[CustomPlatformColors] Inspector not owned by local user, skipping patch");
                return;
            }

            // Check if the element is owned by local user
            if (userByAllocationID != __instance.Slot.World.LocalUser)
            {
                UniLog.Log("[CustomPlatformColors] Inspector not owned by local user, skipping patch");
                return;
            }

            // Find all Image components in children that use the default colors
            foreach (var image in __instance.Slot.GetComponentsInChildren<Image>())
            {
                var color = image.Tint.Value;
                
                // Check if it's using the default cyan color
                if (color == RadiantUI_Constants.Dark.CYAN)
                {
                    image.Tint.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroCyan);
                }
                // Check if it's using the default background color
                else if (color == RadiantUI_Constants.BG_COLOR)
                {
                    image.Tint.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralDark);
                }
                // Check if it's the panel background (dark blue)
                else if (color == new colorX(0.067f, 0.082f, 0.114f, 1f))
                {
                    image.Tint.Value = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralDark).SetA(0.95f);
                }
            }
        }
    }
}

