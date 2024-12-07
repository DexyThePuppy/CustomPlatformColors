using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(RadiantDash))]
    public static class RadiantDash_Patch
    {
        [HarmonyPatch("OnAwake")]
        [HarmonyPostfix]
        public static void OnAwake_Postfix(RadiantDash __instance)
        {
            UniLog.Log("[CustomPlatformColors] RadiantDash OnAwake called");
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
            {
                UniLog.Log("[CustomPlatformColors] Config is null or disabled, skipping RadiantDash patch");
                return;
            }

            // Check if the dash is owned by the local user
            if (__instance?.Slot?.World == null)
            {
                UniLog.Log("[CustomPlatformColors] Dash, Slot or World is null, skipping patch");
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
                UniLog.Log("[CustomPlatformColors] Dash not owned by local user (position check), skipping patch");
                return;
            }

            // Check if the element is owned by local user
            if (userByAllocationID != __instance.Slot.World.LocalUser)
            {
                UniLog.Log("[CustomPlatformColors] Dash not owned by local user, skipping patch");
                return;
            }

            UniLog.Log("[CustomPlatformColors] Setting initial dash state");
            __instance.Open.Value = false; // Set the dash to be initially closed
        }
    }
} 