using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using System;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(ContactsDialog))]
    public static class ContactsDialog_Patch
    {
        // Patch the message colors
        [HarmonyPatch("get_SENDING_MESSAGE_COLOR")]
        [HarmonyPostfix]
        public static void SendingMessageColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkOrange, out colorX color))
                {
                    __result = color;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in SendingMessageColor_Postfix: {ex.Message}");
            }
        }

        [HarmonyPatch("get_SENT_MESSAGE_COLOR")]
        [HarmonyPostfix]
        public static void SentMessageColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkYellow, out colorX color))
                {
                    __result = color;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in SentMessageColor_Postfix: {ex.Message}");
            }
        }

        [HarmonyPatch("get_READ_MESSAGE_COLOR")]
        [HarmonyPostfix]
        public static void ReadMessageColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkGreen, out colorX color))
                {
                    __result = color;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in ReadMessageColor_Postfix: {ex.Message}");
            }
        }

        [HarmonyPatch("get_ERROR_SENT_COLOR")]
        [HarmonyPostfix]
        public static void ErrorSentColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkRed, out colorX color))
                {
                    __result = color;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in ErrorSentColor_Postfix: {ex.Message}");
            }
        }

        [HarmonyPatch("get_RECEIVED_MESSAGE_COLOR")]
        [HarmonyPostfix]
        public static void ReceivedMessageColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkCyan, out colorX color))
                {
                    __result = color;
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in ReceivedMessageColor_Postfix: {ex.Message}");
            }
        }

        [HarmonyPatch("get_BLOCK_ON_COLOR")]
        [HarmonyPostfix]
        public static void BlockOnColor_Postfix(ref colorX __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch() ||
                    !CustomPlatformColors.Config.GetValue(CustomPlatformColors.patchUserspaceScreensManager))
                    return;

                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkRed, out colorX color))
                {
                    // Use a brighter version for the block color
                    __result = color.MulRGB(1.5f);
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in BlockOnColor_Postfix: {ex.Message}");
            }
        }
    }
} 
