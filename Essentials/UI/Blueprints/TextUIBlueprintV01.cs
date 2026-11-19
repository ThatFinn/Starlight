using Il2CppTMPro;
using Starlight.Components;

namespace Starlight.UI.Blueprints;

public class TextUIBlueprintV01 : UIBlueprint
{
    public UIColor Color = UIColor.TextGeneral;
    public Color? CustomColor = null;
    public string TextContent = "";
    public bool DisableAutoTranslation = false;
    public TextAlignmentOptions Alignment = TextAlignmentOptions.TopLeft;
    public TMP_FontAsset CustomFont;
    public FontStyles FontStyle;
    public float FontSize = 20f;
    public float FontAutoSizeMin = 1f;
    public float FontAutoSizeMax = 32f;
    public bool EnableAutoSizing;
    public bool EnableAutoSizeTextContainer;
    public bool ClickableLinks = false;
    public Vector4 Margins = Vector4.zero;

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var txt = obj.AddComponent<TextMeshProUGUI>();

        txt.alignment = Alignment;
        txt.text = DisableAutoTranslation?TextContent:Tr(TextContent);
        txt.font = CustomFont ?? fontTheme.DefaultFont;
        txt.color = CustomColor ?? theme.GetColor(Color);
        txt.fontSize=FontSize*ScaleFactor;
        txt.fontSizeMin=FontAutoSizeMin*ScaleFactor;
        txt.fontSizeMax=FontAutoSizeMax*ScaleFactor;
        txt.enableAutoSizing=EnableAutoSizing;
        txt.autoSizeTextContainer = EnableAutoSizeTextContainer;
        txt.fontStyle = FontStyle;
        txt.margin = Margins * ScaleFactor;
        if (ClickableLinks)
            obj.AddComponent<ClickableTextLink>();

    }
}