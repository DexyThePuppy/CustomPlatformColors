using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using CustomPlatformColors;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;



namespace CustomPlatformColors.Patches
{
    [HarmonyPatch]
    public static class ColorReplacer
    {
        [HarmonyPatch(typeof(RadiantUI_Constants), "GetTintedButton")]
        [HarmonyPrefix]
        public static bool GetTintedButton_Prefix(ref colorX __result, colorX tint)
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return true;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonHoverColor, out colorX hoverColor))
            {
                __result = hoverColor;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "get_BUTTON_COLOR")]
        [HarmonyPostfix]
        public static void ButtonColor_Postfix(ref colorX __result)
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonNormalColor, out colorX buttonColor))
            {
                __result = buttonColor;
            }
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "get_TEXT_COLOR")]
        [HarmonyPostfix]
        public static void TextColor_Postfix(ref colorX __result)
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return;

            if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.buttonTextColor, out colorX textColor))
            {
                __result = textColor;
            }
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupBaseStyle")]
        [HarmonyPostfix]
        public static void SetupBaseStyle_Postfix(UIBuilder ui)
        {
            UpdateAllColors();
        }

        [HarmonyPatch(typeof(RadiantUI_Constants), "SetupDefaultStyle")]
        [HarmonyPrefix]
        public static void SetupDefaultStyle_Prefix()
        {
            try
            {
                if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                    return;

                var flags = BindingFlags.Public | BindingFlags.Static;
                var neutralsType = typeof(RadiantUI_Constants.Neutrals);
                var heroType = typeof(RadiantUI_Constants.Hero);
                var subType = typeof(RadiantUI_Constants.Sub);
                var darkType = typeof(RadiantUI_Constants.Dark);

                // Update neutral colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX darkColor))
                    neutralsType.GetField("DARK", flags).SetValue(null, darkColor);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralMid, out colorX midColor))
                    neutralsType.GetField("MID", flags).SetValue(null, midColor);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralLight, out colorX lightColor))
                    neutralsType.GetField("LIGHT", flags).SetValue(null, lightColor);

                // Update hero colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroYellow, out colorX heroYellow))
                    heroType.GetField("YELLOW", flags).SetValue(null, heroYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroGreen, out colorX heroGreen))
                    heroType.GetField("GREEN", flags).SetValue(null, heroGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroRed, out colorX heroRed))
                    heroType.GetField("RED", flags).SetValue(null, heroRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroPurple, out colorX heroPurple))
                    heroType.GetField("PURPLE", flags).SetValue(null, heroPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroCyan, out colorX heroCyan))
                    heroType.GetField("CYAN", flags).SetValue(null, heroCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroOrange, out colorX heroOrange))
                    heroType.GetField("ORANGE", flags).SetValue(null, heroOrange);

                // Update sub colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subYellow, out colorX subYellow))
                    subType.GetField("YELLOW", flags).SetValue(null, subYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subGreen, out colorX subGreen))
                    subType.GetField("GREEN", flags).SetValue(null, subGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subRed, out colorX subRed))
                    subType.GetField("RED", flags).SetValue(null, subRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subPurple, out colorX subPurple))
                    subType.GetField("PURPLE", flags).SetValue(null, subPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subCyan, out colorX subCyan))
                    subType.GetField("CYAN", flags).SetValue(null, subCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subOrange, out colorX subOrange))
                    subType.GetField("ORANGE", flags).SetValue(null, subOrange);

                // Update dark colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkYellow, out colorX darkYellow))
                    darkType.GetField("YELLOW", flags).SetValue(null, darkYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkGreen, out colorX darkGreen))
                    darkType.GetField("GREEN", flags).SetValue(null, darkGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkRed, out colorX darkRed))
                    darkType.GetField("RED", flags).SetValue(null, darkRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkPurple, out colorX darkPurple))
                    darkType.GetField("PURPLE", flags).SetValue(null, darkPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkCyan, out colorX darkCyan))
                    darkType.GetField("CYAN", flags).SetValue(null, darkCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkOrange, out colorX darkOrange))
                    darkType.GetField("ORANGE", flags).SetValue(null, darkOrange);
            }
            catch (Exception e)
            {
                UniLog.Error($"Error updating RadiantUI colors: {e}");
            }
        }

        private static void UpdateAllColors()
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return;

            try
            {
                // Update neutral colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralDark, out colorX darkColor))
                    typeof(RadiantUI_Constants.Neutrals).GetField("DARK").SetValue(null, darkColor);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralMid, out colorX midColor))
                    typeof(RadiantUI_Constants.Neutrals).GetField("MID").SetValue(null, midColor);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.neutralLight, out colorX lightColor))
                    typeof(RadiantUI_Constants.Neutrals).GetField("LIGHT").SetValue(null, lightColor);

                // Update hero colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroYellow, out colorX heroYellow))
                    typeof(RadiantUI_Constants.Hero).GetField("YELLOW").SetValue(null, heroYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroGreen, out colorX heroGreen))
                    typeof(RadiantUI_Constants.Hero).GetField("GREEN").SetValue(null, heroGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroRed, out colorX heroRed))
                    typeof(RadiantUI_Constants.Hero).GetField("RED").SetValue(null, heroRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroPurple, out colorX heroPurple))
                    typeof(RadiantUI_Constants.Hero).GetField("PURPLE").SetValue(null, heroPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroCyan, out colorX heroCyan))
                    typeof(RadiantUI_Constants.Hero).GetField("CYAN").SetValue(null, heroCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.heroOrange, out colorX heroOrange))
                    typeof(RadiantUI_Constants.Hero).GetField("ORANGE").SetValue(null, heroOrange);

                // Update sub colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subYellow, out colorX subYellow))
                    typeof(RadiantUI_Constants.Sub).GetField("YELLOW").SetValue(null, subYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subGreen, out colorX subGreen))
                    typeof(RadiantUI_Constants.Sub).GetField("GREEN").SetValue(null, subGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subRed, out colorX subRed))
                    typeof(RadiantUI_Constants.Sub).GetField("RED").SetValue(null, subRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subPurple, out colorX subPurple))
                    typeof(RadiantUI_Constants.Sub).GetField("PURPLE").SetValue(null, subPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subCyan, out colorX subCyan))
                    typeof(RadiantUI_Constants.Sub).GetField("CYAN").SetValue(null, subCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.subOrange, out colorX subOrange))
                    typeof(RadiantUI_Constants.Sub).GetField("ORANGE").SetValue(null, subOrange);

                // Update dark colors
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkYellow, out colorX darkYellow))
                    typeof(RadiantUI_Constants.Dark).GetField("YELLOW").SetValue(null, darkYellow);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkGreen, out colorX darkGreen))
                    typeof(RadiantUI_Constants.Dark).GetField("GREEN").SetValue(null, darkGreen);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkRed, out colorX darkRed))
                    typeof(RadiantUI_Constants.Dark).GetField("RED").SetValue(null, darkRed);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkPurple, out colorX darkPurple))
                    typeof(RadiantUI_Constants.Dark).GetField("PURPLE").SetValue(null, darkPurple);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkCyan, out colorX darkCyan))
                    typeof(RadiantUI_Constants.Dark).GetField("CYAN").SetValue(null, darkCyan);
                if (CustomPlatformColors.Config.TryGetValue(CustomPlatformColors.darkOrange, out colorX darkOrange))
                    typeof(RadiantUI_Constants.Dark).GetField("ORANGE").SetValue(null, darkOrange);
            }
            catch (Exception e)
            {
                UniLog.Error($"Error updating RadiantUI colors: {e}");
            }
        }
    }
}