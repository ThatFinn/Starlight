using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class TextureUIBlueprintV01 : UIBlueprint
{
    public Texture Texture;
    public UIColor Color = UIColor.None;
    public Color? CustomColor = null;
    public TextureUIBlueprintV01()
    {
    }
    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var image = obj.AddComponent<RawImage>();
        image.color = CustomColor ?? theme.GetColor(Color);
        image.texture = Texture;
    }
}