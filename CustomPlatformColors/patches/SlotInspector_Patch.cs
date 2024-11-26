using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Linq;
using CustomPlatformColors;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(SlotInspector))]
    public static class SlotInspector_Patch
    {
        [HarmonyPatch("UpdateText")]
        [HarmonyPrefix]
        public static bool UpdateText_Prefix(SlotInspector __instance)
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return true;

            var setupRoot = AccessTools.Field(typeof(SlotInspector), "_setupRoot").GetValue(__instance) as Slot;
            var slotNameText = AccessTools.Field(typeof(SlotInspector), "_slotNameText").GetValue(__instance) as SyncRef<Text>;
            var selectionReference = AccessTools.Field(typeof(SlotInspector), "_selectionReference").GetValue(__instance) as RelayRef<SyncRef<Slot>>;

            if (setupRoot == null || slotNameText?.Target == null) return false;

            __instance.Slot.OrderOffset = setupRoot.OrderOffset;
            slotNameText.Target.Content.Value = setupRoot.Name;

            // Default text color from config
            colorX textColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralLight);

            // Non-persistent items use orange colors
            if (!setupRoot.IsPersistent)
            {
                textColor = setupRoot.PersistentSelf ? 
                    CustomPlatformColors.Config.GetValue(CustomPlatformColors.subOrange) : 
                    CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroOrange);
            }

            // Selected items use yellow
            if (selectionReference?.Target?.Target == setupRoot)
            {
                textColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroYellow);
            }

            // Inactive items are more transparent
            if (!setupRoot.IsActive)
            {
                textColor = setupRoot.ActiveSelf ? textColor.SetA(0.5f) : textColor.SetA(0.3f);
            }

            slotNameText.Target.Color.Value = textColor;

            return false;
        }
    }
}