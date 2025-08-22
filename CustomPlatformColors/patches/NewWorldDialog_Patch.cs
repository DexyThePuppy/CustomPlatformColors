using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using System;
using Renderite.Shared;
using SkyFrost.Base;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(NewWorldDialog))]
    public static class NewWorldDialog_Patch
    {
        // Patch the OpenDialogWindow method to customize the panel colors
        [HarmonyPatch("OpenDialogWindow")]
        [HarmonyPostfix]
        public static void OpenDialogWindow_Postfix(Slot root, ref NewWorldDialog __result)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch())
                    return;

                // Find and update the panel background
                Image panelBackground = root.GetComponentInChildren<Image>();
                if (panelBackground != null && panelBackground.Tint.Value == RadiantUI_Constants.BG_COLOR)
                {
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX bgColor))
                    {
                        panelBackground.Tint.Value = bgColor;
                    }
                }

                // Update text colors
                foreach (Text text in root.GetComponentsInChildren<Text>())
                {
                    if (text.Color.Value == RadiantUI_Constants.TEXT_COLOR)
                    {
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
                        {
                            text.Color.Value = textColor;
                        }
                    }
                }

                // Update button colors
                foreach (Button button in root.GetComponentsInChildren<Button>())
                {
                    if (button.ColorDrivers.Count > 0)
                    {
                        InteractionElement.ColorDriver colorDriver = button.ColorDrivers[0];
                        
                        // Normal color
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX normalColor))
                        {
                            colorDriver.NormalColor.Value = normalColor;
                        }
                        
                        // Hover color
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hoverColor))
                        {
                            colorDriver.HighlightColor.Value = hoverColor;
                        }
                        
                        // Pressed color
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonPressColor, out colorX pressColor))
                        {
                            colorDriver.PressColor.Value = pressColor;
                        }
                        
                        // Disabled color
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonDisabledColor, out colorX disabledColor))
                        {
                            colorDriver.DisabledColor.Value = disabledColor;
                        }
                    }
                }

                // Update checkbox colors by finding their associated buttons
                foreach (Checkbox checkbox in root.GetComponentsInChildren<Checkbox>())
                {
                    Button checkboxButton = checkbox.Slot.GetComponent<Button>();
                    if (checkboxButton?.ColorDrivers.Count > 0)
                    {
                        InteractionElement.ColorDriver colorDriver = checkboxButton.ColorDrivers[0];
                        
                        // Use hero colors for checkboxes to make them stand out
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroCyan, out colorX checkboxColor))
                        {
                            colorDriver.NormalColor.Value = checkboxColor;
                            colorDriver.HighlightColor.Value = CustomPlatformColors.GenerateDarkerShade(checkboxColor, CustomPlatformColors.midBrightnessFactor);
                            colorDriver.PressColor.Value = CustomPlatformColors.GenerateDarkerShade(checkboxColor, CustomPlatformColors.subBrightnessFactor);
                        }
                        
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonDisabledColor, out colorX disabledColor))
                        {
                            colorDriver.DisabledColor.Value = disabledColor;
                        }
                    }
                }

                // Update radio button colors by finding their associated buttons
                foreach (ValueRadio<SessionAccessLevel> radio in root.GetComponentsInChildren<ValueRadio<SessionAccessLevel>>())
                {
                    Button radioButton = radio.Slot.GetComponent<Button>();
                    if (radioButton?.ColorDrivers.Count > 0)
                    {
                        InteractionElement.ColorDriver colorDriver = radioButton.ColorDrivers[0];
                        
                        // Use hero colors for radio buttons
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroPurple, out colorX radioColor))
                        {
                            colorDriver.NormalColor.Value = radioColor;
                            colorDriver.HighlightColor.Value = CustomPlatformColors.GenerateDarkerShade(radioColor, CustomPlatformColors.midBrightnessFactor);
                            colorDriver.PressColor.Value = CustomPlatformColors.GenerateDarkerShade(radioColor, CustomPlatformColors.subBrightnessFactor);
                        }
                        
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonDisabledColor, out colorX disabledColor))
                        {
                            colorDriver.DisabledColor.Value = disabledColor;
                        }
                    }
                }

                // Update text field colors by finding their associated buttons
                foreach (TextField textField in root.GetComponentsInChildren<TextField>())
                {
                    Button textFieldButton = textField.Slot.GetComponent<Button>();
                    if (textFieldButton?.ColorDrivers.Count > 0)
                    {
                        InteractionElement.ColorDriver colorDriver = textFieldButton.ColorDrivers[0];
                        
                        // Use neutral colors for text fields
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralMid, out colorX fieldColor))
                        {
                            colorDriver.NormalColor.Value = fieldColor;
                            colorDriver.HighlightColor.Value = CustomPlatformColors.GenerateDarkerShade(fieldColor, CustomPlatformColors.midBrightnessFactor);
                            colorDriver.PressColor.Value = CustomPlatformColors.GenerateDarkerShade(fieldColor, CustomPlatformColors.subBrightnessFactor);
                        }
                        
                        if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonDisabledColor, out colorX disabledColor))
                        {
                            colorDriver.DisabledColor.Value = disabledColor;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in NewWorldDialog OpenDialogWindow_Postfix: {ex.Message}");
            }
        }

        // Patch OnStartSession to customize error feedback color
        [HarmonyPatch("OnStartSession")]
        [HarmonyPrefix]
        public static bool OnStartSession_Prefix(NewWorldDialog __instance, IButton button, ButtonEventData eventData)
        {
            try
            {
                if (!CustomPlatformColors.ShouldApplyPatch())
                    return true;

                ushort result = 0;
                // Use reflection to access the private _port field
                var portField = typeof(NewWorldDialog).GetField("_port", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var portRef = portField?.GetValue(__instance) as SyncRef<TextField>;
                
                if (!__instance.AutoPort && __instance.PortSelectionEnabled && portRef?.Target != null && !ushort.TryParse(portRef.Target.TargetString, out result))
                {
                    // Use custom error color feedback
                    if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroRed, out colorX errorColor))
                    {
                        var portButton = portRef.Target.Slot.GetComponentInChildren<Button>();
                        if (portButton?.ColorDrivers.Count > 0)
                        {
                            portButton.ColorDrivers[0].NormalColor.TweenFromTo<colorX>(errorColor, CustomPlatformColors.GetValue(CustomPlatformColors.buttonNormalColor), 1f);
                            return false; // Skip original method
                        }
                    }
                }
                return true; // Continue with original method
            }
            catch (Exception ex)
            {
                UniLog.Error($"[CustomPlatformColors] Error in NewWorldDialog OnStartSession_Prefix: {ex.Message}");
                return true;
            }
        }
    }
} 
