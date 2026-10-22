using Starlight.Components.AssetBundle;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class CheckboxUIBlueprintV01 : UIBlueprint
{
    public Sprite CustomCheckSprite;
    public UIColor BackgroundColor = UIColor.Primary;
    public Color? CustomBackgroundColor = null;
    public UIColor CheckColor = UIColor.Accent;
    public Color? CustomCheckColor = null;
    public bool DefaultValue;
    public System.Action<bool> OnValueChanged = null;

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        IgnoreCorners = true;
        var toggle = obj.AddComponent<Toggle>();
        toggle.isOn = DefaultValue;

        var backgroundObj = new GameObject("Background");
        var backgroundRect = backgroundObj.AddComponent<RectTransform>();
        backgroundObj.transform.SetParent(obj, false);
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = new Vector2(1,1);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        var backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = CustomBackgroundColor ?? theme.GetColor(BackgroundColor);
        if(CornerRadius>0)
        {
            var sortGroup = backgroundObj.AddComponent<SortingGroup>();
            sortGroup.enabled = false;
            sortGroup.sortingOrder = Mathf.FloorToInt(CornerRadius * ScaleFactor);
            backgroundObj.AddComponent<RoundedUIImage>().CornerRadius = CornerRadius * ScaleFactor;
        }
        
        var checkmarkObj = new GameObject("Checkmark");
        var checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
        checkmarkObj.transform.SetParent(backgroundObj.transform, false);
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = new Vector2(1,1);
        checkmarkRect.offsetMin = Vector2.zero;
        checkmarkRect.offsetMax = Vector2.zero;
        checkmarkRect.anchoredPosition = Vector2.zero;
        var checkmarkImage = checkmarkObj.AddComponent<Image>();
        checkmarkImage.color = CustomCheckColor ?? theme.GetColor(CheckColor);
        checkmarkImage.sprite = CustomCheckSprite ?? EmbeddedResourceEUtil.LoadSprite("Assets.check.png");
        if(CornerRadius>0)
        {
            var sortGroup = checkmarkObj.AddComponent<SortingGroup>();
            sortGroup.enabled = false;
            sortGroup.sortingOrder = Mathf.FloorToInt(CornerRadius * ScaleFactor);
            checkmarkObj.AddComponent<RoundedUIImage>().CornerRadius = CornerRadius * ScaleFactor;
        }
        toggle.graphic = checkmarkImage;
        toggle.targetGraphic = backgroundImage;
        if(OnValueChanged!=null)
            toggle.onValueChanged.AddListener(OnValueChanged);
    }
}