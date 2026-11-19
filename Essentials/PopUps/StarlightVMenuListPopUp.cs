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
public class StarlightVMenuListPopUp : StarlightPopUp
{
    private Dictionary<string,(string, Sprite)> _entries;
    private Action<string> _onSelect;
    private RectTransform _openThing;
    public void OnPress(string key)
    {
        _onSelect.Invoke(key);
        Close();
    }
    public new static void PreAwake(GameObject obj, List<object> objects)
    {
        var comp = obj.AddComponent<StarlightVMenuListPopUp>();
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
                            new VScrollUIBlueprintV01
                            {
                                Size = new Vector2(1270, 780), CornerRadius = 30,
                                Children = []
                            }
                        ]
                    }
                ]
            };
            var scroll = (VScrollUIBlueprintV01)((PanelUIBlueprintV01)bp.Children[0]).Children[0];
            foreach (var entry in _entries)
            {
                var key = entry.Key;
                var value = entry.Value;
                scroll.Children.Add(new ButtonUIBlueprintV01
                {
                    Size = new Vector2(1250, 80), CornerRadius = 30,
                    OnClick = () => { AudioEUtil.PlaySound(MenuSound.Click); OnPress(key); },
                    Children = [
                        new PanelUIBlueprintV01 { Size = new Vector2(60, 60), Position = new Vector2(-575, 0), Color = UIColor.None, Sprite = value.Item2 },
                        new TextUIBlueprintV01 { TextContent = value.Item1, Alignment = TextAlignmentOptions.Left, FontSize = 30, Position = new Vector2(60, 0), Size = new Vector2(1130, 80) }
                    ]
                });
            }
            return bp;
        }
    }
    
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
            OpenSelf(typeof(StarlightVMenuListPopUp),StarlightMenuTheme.Starlight,new List<object>(){entries,onSelect});
            return;
        }
        OpenSelf(typeof(StarlightVMenuListPopUp),MenuEUtil.GetOpenMenu().GetTheme(),new List<object>(){entries,onSelect});
    }
    public static void Open(Dictionary<string, (string, Sprite)> entries,Action<string> onSelect, StarlightMenuTheme theme)
    {
        OpenSelf(typeof(StarlightVMenuListPopUp),theme,new List<object>(){entries,onSelect});
    }
    protected override void OnUpdate()
    {
        if (LKey.Escape.OnKeyDown())
            Close();
    }
}