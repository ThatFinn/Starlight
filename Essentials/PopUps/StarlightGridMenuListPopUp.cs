using System;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Enums.Sounds;
using Starlight.Storage;
using Starlight.UI;
using Starlight.UI.Blueprints;
using Il2CppInterop.Runtime.Attributes;
using Starlight.Managers;

namespace Starlight.Popups;

[InjectIntoIL]
public class StarlightGridMenuListPopUp : StarlightPopUp
{
    private Dictionary<string,(string, Sprite)> _entries;
    private Action<string> _onSelect;
    public void OnPress(string key)
    {
        _onSelect.Invoke(key);
        Close();
    }
    public new static void PreAwake(GameObject obj, List<object> objects)
    {
        var comp = obj.AddComponent<StarlightGridMenuListPopUp>();
        comp._entries = (Dictionary<string,(string, Sprite)>) objects[0];
        comp._onSelect = (Action<string>) objects[1];
        comp.ReloadFont();
    }
    
    [HideFromIl2Cpp] private PanelUIBlueprintV01 blueprint 
    {
        get
        {
            var bp = new PanelUIBlueprintV01
            {
                Color = UIColor.Primary, CornerRadius = 60, Size = new Vector2(1330, 840),
                Children =
                [
                    new PanelUIBlueprintV01
                    {
                        Color = UIColor.Secondary, CornerRadius = 30, Size = new Vector2(1290, 800),
                        Children = [
                            new GridScrollUIBlueprintV01
                            {
                                Size = new Vector2(1270, 780), CornerRadius = 30,
                                CellSize = new Vector2(165, 165),
                                Spacing = new Vector2(13, 15),
                                Children = []
                            }
                        ]
                    }
                ]
            };
            var scroll = (GridScrollUIBlueprintV01)((PanelUIBlueprintV01)bp.Children[0]).Children[0];
            foreach (var entry in _entries)
            {
                var key = entry.Key;
                var value = entry.Value;
                scroll.Children.Add(new ButtonUIBlueprintV01
                {
                    Size = new Vector2(165, 165), CornerRadius = 30,
                    OnClick = () => { AudioEUtil.PlaySound(MenuSound.Click); OnPress(key); },
                    Children = [
                        new PanelUIBlueprintV01 { Size = new Vector2(85, 85), Position = new Vector2(0, 30), Color = UIColor.None, Sprite = value.Item2 },
                        new TextUIBlueprintV01 { TextContent = value.Item1, Alignment = TextAlignmentOptions.Center, FontSize = 25, FontAutoSizeMax = 25, FontAutoSizeMin = 5, EnableAutoSizing = true, Position = new Vector2(0, -45), Size = new Vector2(155, 60) }
                    ]
                });
            }
            return bp;
        }
    }
    
    private RectTransform _openThing;
    
    protected override void OnOpen()
    {
        var fontEnum = StarlightMenuFont.Native;
        try { fontEnum = StarlightSaveManager.data.fonts[MenuEUtil.GetOpenMenu().GetMenuIdentifier().saveKey]; } catch { }
        _openThing = blueprint.Render(_theme.GetTheme(), fontEnum.GetFontTheme(), transform);
    }
    public static void Open(Dictionary<string, (string, Sprite)> entries,Action<string> onSelect)
    {
        if (!MenuEUtil.isAnyMenuOpen)
        {
            OpenSelf(typeof(StarlightGridMenuListPopUp),StarlightMenuTheme.Starlight,new List<object>(){entries,onSelect});
            return;
        }
        OpenSelf(typeof(StarlightGridMenuListPopUp),MenuEUtil.GetOpenMenu().GetTheme(),new List<object>(){entries,onSelect});
    }
    public static void Open(Dictionary<string, (string, Sprite)> entries,Action<string> onSelect, StarlightMenuTheme theme)
    {
        OpenSelf(typeof(StarlightGridMenuListPopUp),theme,new List<object>(){entries,onSelect});
    }
    protected override void OnUpdate()
    {
        if (LKey.Escape.OnKeyDown())
            Close();
    }
}