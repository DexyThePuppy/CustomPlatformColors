using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using CustomPlatformColors;
using System;
using ResoniteModLoader;
using Renderite.Shared;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(WorldPresets))]
    public static class WorldPresets_Patch
    {
        // Patch the Grid method to customize grid colors
        [HarmonyPatch("Grid")]
        [HarmonyPostfix]
        public static void Grid_Postfix(World w)
        {
            if (CustomPlatformColors.Config == null || 
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled) ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchWorldPresets))
                return;

            try
            {
                // Check if the world is owned by the local user
                if (!IsLocalUserWorld(w))
                    return;

                // Find the GridTexture component in the world
                GridTexture gridTexture = w.RootSlot.GetComponentInChildren<GridTexture>();
                if (gridTexture == null)
                    return;

                // Apply custom grid background color
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.gridBackgroundColor, out colorX bgColor))
                    gridTexture.BackgroundColor.Value = bgColor;

                // Apply custom grid line colors - first grid is minor lines, second is major lines
                if (gridTexture.Grids.Count >= 2)
                {
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.gridMinorLinesColor, out colorX minorColor))
                        gridTexture.Grids[0].LineColor.Value = minorColor;

                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.gridMajorLinesColor, out colorX majorColor))
                        gridTexture.Grids[1].LineColor.Value = majorColor;
                }
            }
            catch (Exception e)
            {
                UniLog.Error($"Error in WorldPresets_Patch.Grid_Postfix: {e}");
            }
        }

        // Patch the SimplePlatform method to customize platform color
        [HarmonyPatch("SimplePlatform")]
        [HarmonyPostfix]
        public static void SimplePlatform_Postfix(World w)
        {
            if (CustomPlatformColors.Config == null || 
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled) ||
                !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchWorldPresets))
                return;

            try
            {
                // Check if the world is owned by the local user
                if (!IsLocalUserWorld(w))
                    return;

                // Find the Ground slot with the cylinder
                Slot groundSlot = w.RootSlot.FindChild("Ground");
                if (groundSlot == null)
                    return;

                // Get the material on the cylinder
                var meshRenderer = groundSlot.GetComponent<MeshRenderer>();
                if (meshRenderer?.Material?.Target == null)
                    return;
                
                PBS_Metallic? material = meshRenderer.Material.Target as PBS_Metallic;
                if (material == null)
                    return;

                // Apply custom platform color
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.platformColor, out colorX platformColor))
                {
                    material.AlbedoColor.Value = platformColor;
                }
            }
            catch (Exception e)
            {
                UniLog.Error($"Error in WorldPresets_Patch.SimplePlatform_Postfix: {e}");
            }
        }

        // Helper method to check if a world is owned by the local user
        private static bool IsLocalUserWorld(World world)
        {
            if (world == null)
                return false;

            // We only want to modify the world if it's owned by the local user
            if (world.IsUserspace())
                return true;

            try
            {
                // Check if the world is owned by local user
                return world.LocalUser.IsHost;
            }
            catch
            {
                return false;
            }
        }
    }
} 
