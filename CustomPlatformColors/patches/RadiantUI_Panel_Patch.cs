using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;
using System.Linq;

namespace CustomPlatformColors.Patches
{
    [HarmonyPatch(typeof(RadiantUI_Panel))]
    public static class RadiantUI_Panel_Patch
    {
        [HarmonyPatch("SetupPanel")]
        [HarmonyPrefix]
        public static bool SetupPanel_Prefix(
            Slot root,
            LocaleString label,
            float2 size,
            bool pinButton,
            bool closeButton,
            ref UIBuilder __result)
        {
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
                return true;

            GenericUIContainer genericUiContainer = root.AttachComponent<GenericUIContainer>();
            genericUiContainer.CloseDestroyRoot.Target = root;
            Canvas canvas = root.AttachComponent<Canvas>();
            canvas.Size.Value = size;
            root.AttachComponent<Grabbable>().Scalable.Value = true;
            UIBuilder ui = new UIBuilder(canvas);
            RadiantUI_Constants.SetupDefaultStyle(ui);
            ui.PushStyle();
            SpriteProvider sprite = root.AttachSprite(OfficialAssets.Graphics.UI.Circle.Light_Border.Circle_Phi2);
            sprite.Borders.Value = float4.One * 0.5f;
            sprite.FixedSize.Value = 80.33f;

            // Use custom background color
            colorX bgColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.neutralDark);
            ui.Panel(bgColor, sprite, ui.Style.NineSliceSizing, true);

            RectTransform header;
            RectTransform content;
            ui.HorizontalHeader(80f, out header, out content);
            ui.NestInto(header);
            ui.HorizontalLayout(8f, 16f, 32f, 16f, 32f);
            ui.Style.FlexibleWidth = 1f;
            Text text = ui.Text(in label, alignment: new Alignment?(Alignment.MiddleLeft));
            ui.Style.FlexibleWidth = -1f;
            ui.Style.MinWidth = 64f;
            ui.Style.ButtonIconPadding = 8f;
            ui.Style.ButtonSprite = ui.CircleSprite;
            genericUiContainer.Title.Target = text.Content;

            // Use custom pin button color
            if (pinButton && !root.World.IsUserspace())
            {
                colorX pinColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.subOrange);
                ui.Button(OfficialAssets.Graphics.Icons.Inspector.Pin, new colorX?(pinColor), colorX.White)
                    .Slot.AttachComponent<ButtonParentUnderUser>().Root.Target = root;
            }

            // Use custom close button colors
            if (closeButton)
            {
                colorX closeHeroColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroRed);
                colorX closeSubColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.subRed);
                ui.Button(OfficialAssets.Graphics.Icons.Inspector.Close, new colorX?(closeHeroColor), closeSubColor)
                    .Slot.AttachComponent<ButtonDestroy>().Target.Target = root;
            }

            ui.Style.MinWidth = -1f;
            ui.NestOutFrom(header);
            ui.NestInto(content);
            content.AddFixedPadding(8f, 16f, 16f, 16f);
            ui.PopStyle();

            __result = ui;
            return false;
        }
    }
}