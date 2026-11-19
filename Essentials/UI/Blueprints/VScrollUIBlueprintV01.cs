using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Components.AssetBundle;
using Starlight.Patches.Context;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

internal class VScrollUIBlueprintV01 : UIBlueprint
{
    public float Spacing = 5f;
    public bool ChildControlWidth = true;
    public bool ChildControlHeight = false;
    public bool ChildExpandWidth = true;
    public bool ChildExpandHeight = true;
    public bool UseContentSizeFitter = true;
    public TextAnchor ChildAlignment = TextAnchor.UpperLeft;
    public ContentSizeFitter.FitMode VerticalFit = ContentSizeFitter.FitMode.MinSize;
    public ContentSizeFitter.FitMode HorizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    public bool UseScrollBar = true;
    public ScrollRect.MovementType MovementType = ScrollRect.MovementType.Clamped;
    public bool ScrollByMenuKeys = true;
    public float ScrollByMenuKeysPerFrame = 9f;
    public UIColor BackgroundColor = UIColor.Transparent;
    public Color? CustomBackgroundColor = null;
    public string? ContentName = "Content";
    public string? ScrollBarVerticalName = "Scrollbar Vertical";
    public string? ViewportName = "Viewport";
    
    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        IgnoreCorners = true;
        var image = obj.AddComponent<Image>();
        image.color = CustomBackgroundColor ?? theme.GetColor(BackgroundColor);
        var scrollRect = obj.AddComponent<ScrollRect>();
        
        var viewPortObj = new GameObject("Viewport");
        var viewPortRect = viewPortObj.AddComponent<RectTransform>();
        viewPortObj.transform.SetParent(obj, false);
        viewPortRect.anchorMin = new Vector2(0, 0);
        viewPortRect.anchorMax = new Vector2(1, 1);
        viewPortRect.sizeDelta = Vector2.zero;
        viewPortRect.anchoredPosition = Vector2.zero;
        viewPortRect.offsetMin = new Vector2(0, 0);
        viewPortRect.offsetMax = new Vector2(UseScrollBar?-25*ScaleFactorX:0, 0);
        var viewPortImage = viewPortObj.AddComponent<Image>();
        var whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, new Color(0,0,0,1));
        whiteTexture.Apply();
        viewPortImage.sprite = whiteTexture.Texture2DToSprite();
        var viewPortMask = viewPortObj.AddComponent<Mask>();
        viewPortMask.showMaskGraphic = false;
        
        var contentObj = new GameObject(ContentName??"Content");
        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.anchoredPosition = Vector2.zero;
        CustomChildHolder = contentRect;
        contentRect.SetParent(viewPortObj.transform, false);
        scrollRect.content = contentRect;

        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        scrollRect.movementType = MovementType;
        if (UseScrollBar)
        {
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            var scrollBar = new VScrollbarUIBlueprintV01()
            {
                Name=ScrollBarVerticalName??"Scrollbar Vertical",
                Size = new (25, Size.y),
                Anchors = new Vector4(1,0,1,1),
            }.Render(theme,fontTheme,obj);
            scrollBar.offsetMin = new Vector2(-25*ScaleFactorX, 0);
            scrollBar.offsetMax = new Vector2(0, 0);
            scrollRect.verticalScrollbar = scrollBar.GetComponent<Scrollbar>();
        }
        var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5, 5, 5, 5);
        vlg.spacing = Spacing;
        vlg.childControlHeight = ChildControlHeight;
        vlg.childControlWidth = ChildControlWidth;
        vlg.childForceExpandHeight = ChildExpandHeight;
        vlg.childForceExpandWidth = ChildExpandWidth;
        vlg.childAlignment = ChildAlignment;
        if (UseContentSizeFitter)
        {
            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = VerticalFit;
            csf.horizontalFit = HorizontalFit;
        }

        viewPortRect.sizeDelta /= 1.95f;
        contentRect.sizeDelta = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        if (ScrollByMenuKeys&&GameContextPatch.InputDown&&GameContextPatch.InputUp)
        {
            var comp = obj.AddComponent<ScrollByMenuKeys>();
            comp._scrollDownInput = GameContextPatch.InputDown;
            comp._scrollUpInput = GameContextPatch.InputUp;
            comp._scrollPerFrame = ScrollByMenuKeysPerFrame;
        }
    }

    protected override void AfterRenderChildren(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        obj.GetComponent<ScrollRect>().verticalNormalizedPosition=1f;
        if(CornerRadius>0)
        {
            var viewport = obj.transform.GetChild(0).gameObject;
            var sortGroup = viewport.AddComponent<SortingGroup>();
            sortGroup.enabled = false;
            sortGroup.sortingOrder = Mathf.FloorToInt(CornerRadius * ScaleFactor);
            viewport.AddComponent<RoundedUIImage>().CornerRadius = CornerRadius * ScaleFactor;
        }
    }
}