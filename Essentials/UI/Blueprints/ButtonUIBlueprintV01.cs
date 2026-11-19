using Starlight.Enums.Sounds;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class ButtonUIBlueprintV01 : UIBlueprint
{
    public Sprite Sprite;
    public UIColor Color = UIColor.None;
    public UIColorBlock ButtonColors = UIColorBlock.Buttons;
    public SystemAction OnClick = null;
    public System.Action<Button> OnClickButton = null;
    public bool Interactable = true;
    public bool UseClickSound = true;
    public MenuSound ClickSound = MenuSound.Click;

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var image = obj.AddComponent<Image>();
        image.sprite = Sprite;
        image.color = theme.GetColor(Color);
        var button = obj.AddComponent<Button>();
        button.interactable = Interactable;
        button.colors = theme.GetColorBlock(ButtonColors);
        button.targetGraphic = image;
        button.onClick = new Button.ButtonClickedEvent();
        if (OnClick != null)
            button.onClick.AddListener(OnClick);
        if (OnClickButton != null)
            button.onClick.AddListener((SystemAction)(() => { OnClickButton.Invoke(button); }));
        if (UseClickSound)
            button.onClick.AddListener((SystemAction)(() => AudioEUtil.PlaySound(ClickSound)));
    }

    protected override void AfterRenderChildren(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        foreach (var child in obj.GetAllChildren())
            if (child.GetComponent<Graphic>())
                child.GetComponent<Graphic>().raycastTarget = false;
    }
}