using System;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Storage;
using Starlight.UI;
using Starlight.UI.Blueprints;
using Il2CppInterop.Runtime.Attributes;
using Starlight.Managers;

namespace Starlight.Popups;

[InjectIntoIL]
public class StarlightConfirmationViewerPopUp : StarlightPopUp
{
    private string _text;
    private int _variant;
    private Action _okAction = null;
    private Action _yesAction = null;
    private Action _noAction = null;
    private Action _escapeAction = null;
    private RectTransform _openThing;
    public new static void PreAwake(GameObject obj, List<object> objects)
    {
        var comp = obj.AddComponent<StarlightConfirmationViewerPopUp>();
        comp._text = objects[0].ToString();
        comp._variant = int.Parse(objects[1].ToString() ?? "0");
        
        if (comp._variant == 0)
        {
            if (objects.Count > 2) comp._okAction = (Action)objects[2];
            if (objects.Count > 3) comp._escapeAction = (Action)objects[3];
        }
        else
        {
            if (objects.Count > 2) comp._yesAction = (Action)objects[2];
            if (objects.Count > 3) comp._noAction = (Action)objects[3];
            if (objects.Count > 4) comp._escapeAction = (Action)objects[4];
        }
        
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
                        Size = new Vector2(1250, 600), Position = new Vector2(0, 70), Margins = new Vector4(20,20,20,20)
                    },
                    new PanelUIBlueprintV01
                    {
                        Name = "ButtonsPanel", Color = UIColor.Transparent, Size = new Vector2(1250, 80), Position = new Vector2(0, -320),
                        Children = _variant == 0 ? new List<UIBlueprint>
                        {
                            new ButtonUIBlueprintV01
                            {
                                Name = "OKButtonRec", Size = new Vector2(300, 80), CornerRadius = 30,
                                Children = [ new TextUIBlueprintV01 { TextContent = "OK", DisableAutoTranslation = true, Alignment = TextAlignmentOptions.Center, FontSize = 40 } ],
                                OnClick = () => { if(_okAction!=null) _okAction.Invoke(); Close(); }
                            }
                        } : new List<UIBlueprint>
                        {
                            new ButtonUIBlueprintV01
                            {
                                Name = "YesButtonRec", Size = new Vector2(300, 80), Position = new Vector2(-200, 0), CornerRadius = 30,
                                Children = [ new TextUIBlueprintV01 { TextContent = "Yes", DisableAutoTranslation = true, Alignment = TextAlignmentOptions.Center, FontSize = 40 } ],
                                OnClick = () => { if(_yesAction!=null) _yesAction.Invoke(); Close(); }
                            },
                            new ButtonUIBlueprintV01
                            {
                                Name = "NoButtonRec", Size = new Vector2(300, 80), Position = new Vector2(200, 0), CornerRadius = 30,
                                Children = [ new TextUIBlueprintV01 { TextContent = "No", DisableAutoTranslation = true, Alignment = TextAlignmentOptions.Center, FontSize = 40 } ],
                                OnClick = () => { if(_noAction!=null) _noAction.Invoke(); Close(); }
                            }
                        }
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
    
    public static void Open(string text, Action yesAction, Action noAction, Action escapeAction)
    {
        if (!MenuEUtil.isAnyMenuOpen)
        {
            OpenSelf(typeof(StarlightConfirmationViewerPopUp),StarlightMenuTheme.Starlight,new List<object>(){text,1,yesAction,noAction,escapeAction});
            return;
        }
        OpenSelf(typeof(StarlightConfirmationViewerPopUp),MenuEUtil.GetOpenMenu().GetTheme(),new List<object>(){text,1,yesAction,noAction,escapeAction});
    }
    public static void Open(string text, Action yesAction, Action noAction, Action escapeAction, StarlightMenuTheme theme)
    {
        OpenSelf(typeof(StarlightConfirmationViewerPopUp),theme,new List<object>(){text,1,yesAction,noAction,escapeAction});
    }
    
    public static void Open(string text, Action okAction, Action escapeAction)
    {
        if (!MenuEUtil.isAnyMenuOpen)
        {
            OpenSelf(typeof(StarlightConfirmationViewerPopUp),StarlightMenuTheme.Starlight,new List<object>(){text,0,okAction,escapeAction});
            return;
        }
        OpenSelf(typeof(StarlightConfirmationViewerPopUp),MenuEUtil.GetOpenMenu().GetTheme(),new List<object>(){text,0,okAction,escapeAction});
    }
    public static void Open(string text, Action okAction, Action escapeAction, StarlightMenuTheme theme)
    {
        OpenSelf(typeof(StarlightConfirmationViewerPopUp),theme,new List<object>(){text,0,okAction,escapeAction});
    }
    
    public static void OpenYesNo(string text, Action yesAction, Action noAction, Action escapeAction = null)
    {
        if (!MenuEUtil.isAnyMenuOpen)
        {
            OpenSelf(typeof(StarlightConfirmationViewerPopUp),StarlightMenuTheme.Starlight,new List<object>(){text,1,yesAction,noAction,escapeAction});
            return;
        }
        OpenSelf(typeof(StarlightConfirmationViewerPopUp),MenuEUtil.GetOpenMenu().GetTheme(),new List<object>(){text,1,yesAction,noAction,escapeAction});
    }
    
    protected override void OnUpdate()
    {
        if (LKey.Escape.OnKeyDown())
        {
            if(_escapeAction!=null) _escapeAction.Invoke();
            Close();
        }
    }
}