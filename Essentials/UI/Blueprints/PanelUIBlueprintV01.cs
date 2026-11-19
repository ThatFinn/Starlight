using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class PanelUIBlueprintV01 : UIBlueprint
{
    public Sprite Sprite;
    public UIColor Color = UIColor.Secondary;
    public Color? CustomColor = null;
    public PanelUIBlueprintV01()
    {
    }
    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var image = obj.AddComponent<Image>();
        image.color = CustomColor ?? theme.GetColor(Color);
        image.sprite = Sprite;
    }
}