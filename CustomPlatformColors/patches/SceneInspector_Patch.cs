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
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return;

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