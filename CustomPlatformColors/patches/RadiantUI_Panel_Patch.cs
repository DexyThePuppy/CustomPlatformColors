using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

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
            UniLog.Log($"[CustomPlatformColors] SetupPanel called for: {label}");
            
            if (CustomPlatformColors.Config == null || !CustomPlatformColors.Config.GetValue(CustomPlatformColors.enabled))
            {
                UniLog.Log("[CustomPlatformColors] Config is null or disabled");
                return true;
            }

            // Check if the root element is owned by the local user
            if (root?.World == null)
            {
                UniLog.Log("[CustomPlatformColors] Root or World is null, skipping patch");
                return true;
            }

            root.ReferenceID.ExtractIDs(out var position, out var user);
            User userByAllocationID = root.World.GetUserByAllocationID(user);

            if (userByAllocationID == null)
            {
                UniLog.Log("[CustomPlatformColors] Could not find user by allocation ID, skipping patch");
                return true;
            }

            // Filter out users who left and then someone joined with their id
            if (position < userByAllocationID.AllocationIDStart)
            {
                UniLog.Log("[CustomPlatformColors] Element not owned by local user, skipping patch");
                return true;
            }

            // Check if the element is owned by local user
            if (userByAllocationID != root.World.LocalUser)
            {
                UniLog.Log("[CustomPlatformColors] Element not owned by local user, skipping patch");
                return true;
            }

            UniLog.Log("[CustomPlatformColors] Applying custom colors to panel");

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
            UniLog.Log($"[CustomPlatformColors] Setting background color: {bgColor}");
            var panel = ui.Panel(bgColor, sprite, ui.Style.NineSliceSizing, true);

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
                UniLog.Log($"[CustomPlatformColors] Setting pin button color: {pinColor}");
                var pinBtn = ui.Button(OfficialAssets.Graphics.Icons.Inspector.Pin, new colorX?(pinColor), colorX.White);
                pinBtn.Slot.AttachComponent<ButtonParentUnderUser>().Root.Target = root;
            }

            // Use custom close button colors
            if (closeButton)
            {
                colorX closeHeroColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.heroRed);
                colorX closeSubColor = CustomPlatformColors.Config.GetValue(CustomPlatformColors.subRed);
                UniLog.Log($"[CustomPlatformColors] Setting close button colors - hero: {closeHeroColor}, sub: {closeSubColor}");
                var closeBtn = ui.Button(OfficialAssets.Graphics.Icons.Inspector.Close, new colorX?(closeHeroColor), closeSubColor);
                closeBtn.Slot.AttachComponent<ButtonDestroy>().Target.Target = root;
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