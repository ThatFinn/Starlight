using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Storage;
using Starlight.UI;
using Starlight.UI.Blueprints;

namespace Starlight.Menus.Development;

internal class StarlightTestDevMenu : StarlightMenu
{
    public new static MenuIdentifier GetMenuIdentifier() => new ("starlighttestdevemenu",StarlightMenuFont.Native,StarlightMenuTheme.Starlight, "TestDevMenu",true,true);

    protected override bool createCommands => true;
    protected override bool inGameOnly => false;
    protected override void OnAwake()
    {
        GetComponent<RectTransform>().localPosition = Vector2.zero;
        requiredFeatures = [DevTestMenu];
        openActions = [MenuActions.PauseGameFalse];
        closeActions = [MenuActions.UnPauseGameFalse, MenuActions.EnableInput];
    }

    [HideFromIl2Cpp] private PanelUIBlueprintV01 blueprint => new ()
    {
        Color = UIColor.Primary, CornerRadius = 90, Size = new Vector2(1330, 840),
        Children =
        [
            new PanelUIBlueprintV01 { Color = UIColor.Accent, CornerRadius = 0, Size = new Vector2(1330,10), Position = new Vector2(0,-265f) },
            new PanelUIBlueprintV01 { Color = UIColor.Accent, CornerRadius = 0, Size = new Vector2(1330,10), Position = new Vector2(0,265f) },
            new NavigationUIBlueprintV01
            {
                CornerRadius = 90, Size = new Vector2(1330, 840),
                TabSize = new Vector2(1330,520),
                Tabs = [
                    new PanelUIBlueprintV01
                    {
                        Name = "Tab1",
                        Children = [
                            new VScrollUIBlueprintV01
                            {
                                Size = new Vector2(500,500), CornerRadius = 10,
                                Children = [
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 1", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 2", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 3", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 4", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 5", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 5", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 6", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 7", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 8", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 10", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 11", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 12", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                ]
                            }
                        ]
                    },
                    new PanelUIBlueprintV01
                    {
                        Name = "Tab2",
                        Children = [
                            new PanelUIBlueprintV01 { Color = UIColor.Accent, CornerRadius = 0, Size = new Vector2(10,520) },
                            new VScrollUIBlueprintV01
                            {
                                Size = new Vector2(660,520), CornerRadius = 10, Position = new Vector2(-332.5f,0),
                                Children = [
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 1", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(500,70), CornerRadius = 30, Children = [new TextUIBlueprintV01 { TextContent = "Button 2", Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1), }] },
                                ]
                            },
                            new TextUIBlueprintV01
                            {
                                Size = new Vector2(660,520), CornerRadius = 10, Position = new Vector2(332.5f,0),
                                TextContent = "Youre on Panel 2", Alignment = TextAlignmentOptions.TopLeft,
                                Margins= new Vector4(10,10,10,10),
                            }
                        ]
                    },
                    new PanelUIBlueprintV01
                    {
                        Name = "Tab3",
                        Children = [
                            new SliderUIBlueprintV01()
                            {
                                Size =  new Vector2(400,40),
                                DefaultValue = 20,
                                MinValue = 0,
                                MaxValue = 100,
                            },
                            
                            new SliderUIBlueprintV01()
                            {
                                Size =  new Vector2(400,40), Position = new Vector2(0,-200),
                                DefaultValue = 20,
                                MinValue = 0,
                                MaxValue = 100,
                                OutputPow = 3.51f,OutputMaxValue = 999999f
                            }
                        ]
                    },
                    new PanelUIBlueprintV01
                    {
                        Name = "Test",
                        Children = [
                            new VScrollUIBlueprintV01
                            {
                                Size = new Vector2(500,500), CornerRadius = 10,
                                Children = [
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, OnClick = () => { Starlight.Popups.StarlightTextViewerPopUp.Open("This is a text viewer popup test"); }, Children = [new TextUIBlueprintV01 { TextContent = "Text Viewer", DisableAutoTranslation = true, Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1) }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, OnClick = () => { Starlight.Popups.StarlightConfirmationViewerPopUp.Open("This is a confirmation test", () => { }, () => { }); }, Children = [new TextUIBlueprintV01 { TextContent = "Confirmation", DisableAutoTranslation = true, Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1) }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, OnClick = () => { Starlight.Popups.StarlightGridMenuListPopUp.Open(new System.Collections.Generic.Dictionary<string, (string, UnityEngine.Sprite)>() { {"test1", ("Grid 1", null)}, {"test2", ("Grid 2", null)}, {"test3", ("Grid 3", null)}, {"test4", ("Grid 4", null)} }, (s) => { }); }, Children = [new TextUIBlueprintV01 { TextContent = "Grid List", DisableAutoTranslation = true, Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1) }] },
                                    new ButtonUIBlueprintV01 { Size = new Vector2(600,70), CornerRadius = 30, OnClick = () => { Starlight.Popups.StarlightVMenuListPopUp.Open(new System.Collections.Generic.Dictionary<string, (string, UnityEngine.Sprite)>() { {"test1", ("VList 1", null)}, {"test2", ("VList 2", null)} }, (s) => { }); }, Children = [new TextUIBlueprintV01 { TextContent = "V Menu List", DisableAutoTranslation = true, Alignment = TextAlignmentOptions.Center, FontSize = 30, Color = UIColor.TextButton, Anchors = new Vector4(0,0,1,1) }] },
                                ]
                            }
                        ]
                    }
                ]
            },
            new PanelUIBlueprintV01
            {
                Color = UIColor.Badge, CornerRadius = 50, Size = new Vector2(530,90), Position = new Vector2(0,-420f),
                Children = [
                    new TextUIBlueprintV01 { TextContent = "Test Menu", Size = new Vector2(530,90), FontSize = 50,  Alignment = TextAlignmentOptions.Center, Color = UIColor.TextButton }
                ]
                
            },
        ]
    };
    
    public new static GameObject GetMenuRootObject()
    {
        var obj = new GameObject("StarlightTestDevMenu");
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero; 
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        return obj;
    }

    private RectTransform _openThing;
    protected override void OnOpen()
    {
        _openThing = blueprint.Render(currentTheme, currentFontTheme, transform);
    }

    protected override void OnClose()
    {
        Destroy(_openThing.gameObject);
    }
    
    public override void OnCloseUIPressed()
    {
        if (MenuEUtil.isAnyPopUpOpen) return;
        Close();
    }
}