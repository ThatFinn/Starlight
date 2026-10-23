using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Storage;
using Starlight.UI;
using Starlight.UI.Blueprints;
using Il2CppInterop.Runtime.Attributes;
using Starlight.Managers;

namespace Starlight.Popups;

[InjectIntoIL]
public class StarlightTextViewerPopUp : StarlightPopUp
{
    private string _text;
    private RectTransform _openThing;
    public new static void PreAwake(GameObject obj, List<object> objects)
    {
        var comp = obj.AddComponent<StarlightTextViewerPopUp>();
        comp._text = objects[0].ToString();
        comp.ReloadFont();
        
    }
    
    [HideFromIl2Cpp] private PanelUIBlueprintV01 blueprint => new ()
    {
        Color = UIColor.Primary, CornerRadius = 60, Size = new Vector2(1330, 840),
        Children =
        [
            new PanelUIBlueprintV01
            {
                Color = UIColor.Secondary, CornerRadius = 30, Size = new Vector2(1290, 800),
                Children = [
                    new TextUIBlueprintV01
                    {
                        Name = "TextViewerText",
                        TextContent = _text, Alignment = TextAlignmentOptions.TopLeft, FontSize = 30, Color = UIColor.TextCategory, 
                        Size = new Vector2(1250, 760), Margins = new Vector4(20,20,20,20)
                    }
                ]
            }
        ]
    };
    
    
    protected override void OnOpen()
    {
        var fontEnum = StarlightMenuFont.Native;
        try { fontEnum = StarlightSaveManager.data.fonts[MenuEUtil.GetOpenMenu().GetMenuIdentifier().saveKey]; } catch { }
        _openThing = blueprint.Render(_theme.GetTheme(), fontEnum.GetFontTheme(), transform);
    }
    
    public static void Open(string text)
    {
        if (!MenuEUtil.isAnyMenuOpen)
        {
            OpenSelf(typeof(StarlightTextViewerPopUp),StarlightMenuTheme.Starlight,new List<object>(){text});
            return;
        }
        OpenSelf(typeof(StarlightTextViewerPopUp),MenuEUtil.GetOpenMenu().GetTheme(),new List<object>(){text});
    }
    public static void Open(string text, StarlightMenuTheme theme)
    {
        OpenSelf(typeof(StarlightTextViewerPopUp),theme,new List<object>(){text});
    }
    protected override void OnUpdate()
    {
        if (LKey.Escape.OnKeyDown())
            Close();
    }
}