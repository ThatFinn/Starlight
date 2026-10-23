using Il2CppTMPro;
using Starlight.Components;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class WindowUIBlueprintV01 : UIBlueprint
{
    public string Title = "Window";
    public UIColor BackgroundColor = UIColor.Secondary;
    public Color? CustomBackgroundColor = null;
    public UIColor TitleBarColor = UIColor.Primary;
    public Color? CustomTitleBarColor = null;
    public float TitleHeight = 30f;
    public bool Draggable = true;
    public bool Resizable = true;
    public Vector2 MinSize = new Vector2(200, 150);

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var bgImage = obj.gameObject.AddComponent<Image>();
        bgImage.color = CustomBackgroundColor ?? theme.GetColor(BackgroundColor);

        var titleBarObj = new GameObject("TitleBar");
        var titleBarRect = titleBarObj.AddComponent<RectTransform>();
        titleBarRect.SetParent(obj, false);
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = new Vector2(1, 1);
        titleBarRect.pivot = new Vector2(0, 1);
        titleBarRect.anchoredPosition = Vector2.zero;
        titleBarRect.sizeDelta = new Vector2(0, TitleHeight * ScaleFactor);

        var titleBg = titleBarObj.AddComponent<Image>();
        titleBg.color = CustomTitleBarColor ?? theme.GetColor(TitleBarColor);

        var titleTextObj = new GameObject("TitleText");
        var titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleTextRect.SetParent(titleBarRect, false);
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.offsetMin = new Vector2(10 * ScaleFactor, 0);
        titleTextRect.offsetMax = Vector2.zero;

        var titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleText.text = Title;
        titleText.font = fontTheme.DefaultFont;
        titleText.fontSize = 16f * ScaleFactor;
        titleText.alignment = TextAlignmentOptions.Left;

        RectTransform resizeRect = null;
        if (Resizable)
        {
            var resizeObj = new GameObject("ResizeHandle");
            resizeRect = resizeObj.AddComponent<RectTransform>();
            resizeRect.SetParent(obj, false);
            resizeRect.anchorMin = new Vector2(1, 0);
            resizeRect.anchorMax = new Vector2(1, 0);
            resizeRect.pivot = new Vector2(1, 0);
            resizeRect.anchoredPosition = Vector2.zero;
            resizeRect.sizeDelta = new Vector2(20 * ScaleFactor, 20 * ScaleFactor);
            
            var resizeImg = resizeObj.AddComponent<Image>();
            resizeImg.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        CustomChildHolder = new GameObject("Content").AddComponent<RectTransform>();
        CustomChildHolder.SetParent(obj, false);
        CustomChildHolder.anchorMin = new Vector2(0, 0);
        CustomChildHolder.anchorMax = new Vector2(1, 1);
        CustomChildHolder.offsetMin = new Vector2(10 * ScaleFactor, 10 * ScaleFactor);
        CustomChildHolder.offsetMax = new Vector2(-10 * ScaleFactor, -TitleHeight * ScaleFactor - 5 * ScaleFactor);

        var interaction = obj.gameObject.AddComponent<WindowInteractionHandler>();
        interaction.titleBar = titleBarRect;
        interaction.resizeHandle = resizeRect;
        interaction.windowRect = obj;
        interaction.draggable = Draggable;
        interaction.resizable = Resizable;
        interaction.minSize = MinSize * ScaleFactor;
    }
}