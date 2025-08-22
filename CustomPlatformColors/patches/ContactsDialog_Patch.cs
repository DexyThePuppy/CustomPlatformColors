using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using System;
using SkyFrost.Base;
using Renderite.Shared;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(ContactsDialog))]
    public static class ContactsDialog_Patch
    {
        // Patch to remove timestamps
        [HarmonyPatch("AddMessage")]
        [HarmonyPrefix]
        public static void AddMessage_Prefix(ContactsDialog __instance, SkyFrost.Base.Message message, ref bool __state)
        {
            try
            {
                // Store the current value of _lastTimestampMessage in __state to restore it later
                var field = typeof(ContactsDialog).GetField("_lastTimestampMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    __state = true;
                    // Set _lastTimestampMessage to the current message to trick the system into thinking
                    // we just displayed a timestamp for this message
                    field.SetValue(__instance, message);
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in AddMessage_Prefix: {ex.Message}");
            }
        }

        // Patch the panel creation colors
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void OnAttach_Postfix(ContactsDialog __instance)
        {
            try
            {
                // Find all panel images that use BG_COLOR
                Slot rootSlot = __instance.Slot;
                
                // Replace background colors
                foreach (Image img in rootSlot.GetComponentsInChildren<Image>())
                {
                    // Background panels
                    if (img.Tint.Value == RadiantUI_Constants.BG_COLOR)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX color))
                        {
                            img.Tint.Value = color;
                        }
                    }
                    
                    // Button colors
                    if (img.Tint.Value == RadiantUI_Constants.BUTTON_COLOR)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX color))
                        {
                            img.Tint.Value = color;
                        }
                    }
                    
                    // Tab colors
                    if (img.Tint.Value == RadiantUI_Constants.TAB_ACTIVE_BACKGROUND_COLOR)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX color))
                        {
                            img.Tint.Value = color;
                        }
                    }
                    
                    if (img.Tint.Value == RadiantUI_Constants.TAB_INACTIVE_BACKGROUND_COLOR)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX color))
                        {
                            img.Tint.Value = color;
                        }
                    }
                    
                    // Neutral colors
                    if (img.Tint.Value == RadiantUI_Constants.Neutrals.MID)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralMid, out colorX color))
                        {
                            img.Tint.Value = color;
                        }
                    }
                    
                    if (img.Tint.Value == RadiantUI_Constants.Neutrals.DARK)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX color))
                        {
                            img.Tint.Value = color;
                        }
                    }
                    
                    if (img.Tint.Value == RadiantUI_Constants.Neutrals.LIGHT)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralLight, out colorX color))
                        {
                            img.Tint.Value = color;
                        }
                    }
                }
                
                // Replace text colors
                foreach (Text text in rootSlot.GetComponentsInChildren<Text>())
                {
                    if (text.Color.Value == RadiantUI_Constants.TEXT_COLOR)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX color))
                        {
                            text.Color.Value = color;
                        }
                    }
                }
                
                // Replace button color drivers
                foreach (Button button in rootSlot.GetComponentsInChildren<Button>())
                {
                    if (button.ColorDrivers.Count > 0)
                    {
                        var driver = button.ColorDrivers[0];
                        
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX normalColor))
                        {
                            driver.NormalColor.Value = normalColor;
                        }
                        
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hoverColor))
                        {
                            driver.HighlightColor.Value = hoverColor;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in OnAttach_Postfix: {ex.Message}");
            }
        }

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

                colorX color = CustomPlatformColors.GetDarkOrange();
                if (CustomPlatformColors.Config != null)
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

                colorX color = CustomPlatformColors.GetDarkYellow();
                if (CustomPlatformColors.Config != null)
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

                colorX color = CustomPlatformColors.GetDarkGreen();
                if (CustomPlatformColors.Config != null)
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

                colorX color = CustomPlatformColors.GetDarkRed();
                if (CustomPlatformColors.Config != null)
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

                colorX color = CustomPlatformColors.GetDarkCyan();
                if (CustomPlatformColors.Config != null)
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

                colorX color = CustomPlatformColors.GetDarkRed();
                if (CustomPlatformColors.Config != null)
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

        // Remove the existing AddMessage_Postfix method and replace with this one:
        [HarmonyPatch("AddMessage")]
        [HarmonyPostfix]
        public static void AddMessage_Postfix(ContactsDialog __instance, SkyFrost.Base.Message message, bool __state)
        {
            try
            {
                // Find and destroy any timestamp panels
                Slot rootSlot = __instance.Slot;
                if (rootSlot != null)
                {
                    // Get the messagesRoot field
                    var messagesRootField = typeof(ContactsDialog).GetField("_messagesRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (messagesRootField != null)
                    {
                        var messagesRoot = messagesRootField.GetValue(__instance) as SyncRef<Slot>;
                        if (messagesRoot?.Target != null)
                        {
                            // Look for timestamp panels in the messages root
                            foreach (Slot child in messagesRoot.Target.Children)
                            {
                                if (child == null) continue;
                                
                                // Look for timestamp elements - they have specific characteristics
                                var images = child.GetComponentsInChildren<Image>();
                                if (images == null) continue;
                                
                                foreach (Image img in images)
                                {
                                    if (img == null) continue;
                                    
                                    if (img.Tint.Value == RadiantUI_Constants.Neutrals.MID)
                                    {
                                        Text text = img.Slot?.GetComponentInChildren<Text>();
                                        if (text != null && text.Size.Value == 12f)
                                        {
                                            // This is a timestamp panel - destroy it
                                            img.Slot?.Destroy();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in AddMessage_Postfix: {ex.Message}");
            }
        }

        // Add this new patch to handle AudioWaveformTexture colors
        [HarmonyPatch("AddMessage")]
        [HarmonyPrefix]
        public static void AddMessage_AudioWaveform_Prefix(ContactsDialog __instance, SkyFrost.Base.Message message)
        {
            try
            {
                if (message.MessageType == SkyFrost.Base.MessageType.Sound)
                {
                    // Get the messagesRoot field
                    var messagesRootField = typeof(ContactsDialog).GetField("_messagesRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (messagesRootField != null)
                    {
                        var messagesRoot = messagesRootField.GetValue(__instance) as SyncRef<Slot>;
                        if (messagesRoot?.Target != null)
                        {
                            // Find any AudioWaveformTexture components and update their colors
                            foreach (AudioWaveformTexture texture in messagesRoot.Target.GetComponentsInChildren<AudioWaveformTexture>())
                            {
                                if (message.IsSent)
                                {
                                    if (CustomPlatformColors.Config?.TryGetValue(CustomPlatformColors.heroGreen, out colorX heroColor) ?? false)
                                    {
                                        texture.ForegroundColor.Value = heroColor;
                                    }
                                    colorX darkColor = CustomPlatformColors.GetDarkGreen();
                                    texture.BackgroundColor.Value = darkColor;
                                }
                                else
                                {
                                    if (CustomPlatformColors.Config?.TryGetValue(CustomPlatformColors.heroCyan, out colorX heroColor) ?? false)
                                    {
                                        texture.ForegroundColor.Value = heroColor;
                                    }
                                    colorX darkColor = CustomPlatformColors.GetDarkCyan();
                                    texture.BackgroundColor.Value = darkColor;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in AddMessage_AudioWaveform_Prefix: {ex.Message}");
            }
        }
    }
} 
