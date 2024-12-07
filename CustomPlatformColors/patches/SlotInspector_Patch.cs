using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(SlotInspector))]
    public static class SlotInspector_Patch
    {
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(SlotInspector __instance)
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return;

            // Check if the inspector is owned by the local user
            if (__instance?.Slot?.World == null)
            {
                UniLog.Log("[CustomPlatformColors] Inspector, Slot or World is null, skipping patch");
                return;
            }

            var slot = __instance.Slot;
            var world = slot.World;

            // Get the allocating user
            slot.ReferenceID.ExtractIDs(out var position, out var user);
            User userByAllocationID = world.GetUserByAllocationID(user);

            if (userByAllocationID == null)
            {
                UniLog.Log("[CustomPlatformColors] Could not find user by allocation ID, skipping patch");
                return;
            }

            // Filter out users who left and then someone joined with their id
            if (position < userByAllocationID.AllocationIDStart)
            {
                UniLog.Log("[CustomPlatformColors] Inspector not owned by local user (position check), skipping patch");
                return;
            }

            // Check if the element is owned by local user
            if (userByAllocationID != world.LocalUser)
            {
                UniLog.Log("[CustomPlatformColors] Inspector not owned by local user, skipping patch");
                return;
            }

            // Find all Image components in children that use the default colors
            var images = slot.GetComponentsInChildren<Image>();
            if (images == null)
            {
                UniLog.Log("[CustomPlatformColors] No images found to modify");
                return;
            }

            foreach (var image in images)
            {
                if (image == null) continue;

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