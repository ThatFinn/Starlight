using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class VScrollbarUIBlueprintV01 : UIBlueprint
{
    public Sprite BackgroundSprite;
    public Sprite HandleSprite;
    public UIColor Color = UIColor.Accent;
    public Color? CustomColor = null;
    public bool InvertDirection = false;

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var image = obj.AddComponent<Image>();
        //image.sprite = backgroundSprite;
        image.color = theme.GetColor(UIColor.Transparent);
        var scrollbar = obj.AddComponent<Scrollbar>();
        scrollbar.direction = InvertDirection?Scrollbar.Direction.TopToBottom:Scrollbar.Direction.BottomToTop;
        var backgroundArea = new GameObject("Background");
        var backgroundAreaRect = backgroundArea.AddComponent<RectTransform>();
        backgroundArea.transform.SetParent(obj, false);
        backgroundAreaRect.sizeDelta = new Vector2(Size.x*ScaleFactorX / 5, 0);
        backgroundAreaRect.anchoredPosition = Vector2.zero;
        backgroundAreaRect.anchorMin = new Vector2(0.5f, 0);
        backgroundAreaRect.anchorMax = new Vector2(0.5f, 1);
        var backgroundImage = backgroundArea.AddComponent<Image>();
        backgroundImage.sprite = BackgroundSprite;
        backgroundImage.color = CustomColor ?? theme.GetColor(Color);
        
        var slidingArea = new GameObject("Sliding Area");
        var slidingAreaRect = slidingArea.AddComponent<RectTransform>();
        slidingArea.transform.SetParent(obj, false);
        slidingAreaRect.anchorMin = new Vector2(0, 0);
        slidingAreaRect.anchorMax = new Vector2(1, 1);
        slidingAreaRect.offsetMin = new Vector2(0, 0);
        slidingAreaRect.offsetMax = new Vector2(0, 0);
        
        var handle = new GameObject("Handle");
        var handleRect = handle.AddComponent<RectTransform>();
        handle.transform.SetParent(slidingArea.transform, false);
        handleRect.anchorMin = new Vector2(0, 0);
        handleRect.anchorMax = new Vector2(1, 1);
        handleRect.offsetMin = new Vector2(0, 0);
        handleRect.offsetMax = new Vector2(0, 0);
        handleRect.sizeDelta = new Vector2(-4*ScaleFactorX, 0);
        handle.AddComponent<Image>().sprite = HandleSprite ?? EmbeddedResourceEUtil.LoadSprite("Assets.scrollV.png");
        
        scrollbar.handleRect = handleRect;
    }
}