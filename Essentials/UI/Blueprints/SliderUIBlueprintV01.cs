using System;
using Il2CppTMPro;
using Starlight.Components.AssetBundle;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class SliderUIBlueprintV01 : UIBlueprint
{
    public UIColor BackgroundColor = UIColor.Accent;
    public Color? CustomBackgroundColor = null;
    public UIColor HandleColor = UIColor.Badge;
    public Color? CustomHandleColor = null;
    public TMP_FontAsset CustomHandleFont;
    
    public UIColor HandleTextColor = UIColor.TextGeneral;
    public Color? CustomHandleTextColor = null;
    public float MinValue = 0;
    public float MaxValue = 100;
    public float DefaultValue = 0;
    public bool WholeNumbers = true;
    public bool AllowDisplayingDecimalNumbers = true;
    public float HandleWidth = 60f;
    public float OutputPow = 1f;
    public float OutputMaxValue = float.MaxValue;
    private float _oldValue = 0f;
    public System.Action<float> onValueChanged = null;

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var slider = obj.AddComponent<Slider>();

        slider.minValue = MinValue;
        slider.maxValue = MaxValue;
        slider.value = Mathf.Clamp(DefaultValue,MinValue,MaxValue);
        _oldValue = slider.value;
        slider.wholeNumbers = WholeNumbers;
        
        var bColor = CustomBackgroundColor ?? theme.GetColor(BackgroundColor);
        var background = new GameObject("Background");
        var backgroundRect = background.AddComponent<RectTransform>();
        background.transform.SetParent(obj, false);
        backgroundRect.sizeDelta = new Vector2(Size.x*ScaleFactorX, Size.y*ScaleFactorY / 2.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        var backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = bColor;
        var backgroundSortGroup = background.AddComponent<SortingGroup>();
        backgroundSortGroup.enabled = false;
        backgroundSortGroup.sortingOrder = Mathf.FloorToInt(5 * ScaleFactor);
        backgroundSortGroup.AddComponent<RoundedUIImage>().CornerRadius = 5 * ScaleFactor;
        
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.sizeDelta = backgroundRect.sizeDelta;
        fillAreaRect.anchorMin = new Vector2(1, .75f);
        fillAreaRect.anchorMax = new Vector2(0, .25f);
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        var fill = new GameObject("Fill");
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.sizeDelta = backgroundRect.sizeDelta;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.anchoredPosition = Vector2.zero;
        fill.transform.SetParent(fillArea.transform, false);
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = bColor*new Color(0.75f,0.75f,0.75f,1f);
        slider.fillRect = fillRect;

        var handleSlideArea = new GameObject("Handle Slide Area");
        var handleSlideAreaRect = handleSlideArea.AddComponent<RectTransform>();
        handleSlideAreaRect.anchorMin = Vector2.zero;
        handleSlideAreaRect.anchorMax = new Vector2(1, 1);
        handleSlideAreaRect.offsetMin = new Vector2(0, 0);
        handleSlideAreaRect.offsetMax = new Vector2(0, 0);
        handleSlideArea.transform.SetParent(obj, false);
        handleSlideAreaRect.anchoredPosition = new Vector2(-(HandleWidth / 4f)*ScaleFactorX,0);
        handleSlideAreaRect.sizeDelta = new Vector2((-HandleWidth - (HandleWidth / 2f))*ScaleFactorX, 0);
        
        var handle = new GameObject("Handle");
        var handleRect = handle.AddComponent<RectTransform>();
        handle.transform.SetParent(handleSlideArea.transform, false);
        handleRect.offsetMin = new Vector2(-(HandleWidth/2f)*ScaleFactorX, -10*ScaleFactorY);
        handleRect.offsetMax = new Vector2(HandleWidth*ScaleFactorX, 10*ScaleFactorY);
        var handleGroup = handle.AddComponent<SortingGroup>();
        handleGroup.enabled = false;
        handleGroup.sortingOrder = Mathf.FloorToInt(5 * ScaleFactor);
        handleGroup.AddComponent<RoundedUIImage>().CornerRadius = 5 * ScaleFactor;
        
        
        var handleImage = handle.AddComponent<Image>();
        handleImage.color = CustomHandleColor ?? theme.GetColor(HandleColor);
        slider.targetGraphic = handleImage;
        slider.handleRect = handleRect;
        
        
        var text = new GameObject("Text");
        text.transform.SetParent(handle.transform, false);
        var textRect = text.AddComponent<RectTransform>();
        textRect.sizeDelta = Vector2.zero;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = new Vector2(1, 1);
        textRect.anchoredPosition = Vector2.zero;
        
        var txt = text.AddComponent<TextMeshProUGUI>();
        txt.margin = new Vector4(3, 6, 3, 6) * ScaleFactor;
        txt.font = CustomHandleFont ?? fontTheme.DefaultFont;
        txt.enableAutoSizing = true;
        txt.fontSizeMax = 999f;
        txt.fontSizeMin = 1f;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = CustomHandleTextColor ?? theme.GetColor(HandleTextColor);
        txt.text = (slider.value.ToString());
        var call = (System.Action<float>)((output) =>
        {
            var newValue = Mathf.Clamp((float)Math.Pow(output, OutputPow), MinValue, OutputMaxValue);
            if (WholeNumbers) newValue = Mathf.FloorToInt(newValue);
            var newText = newValue.ToString();
            if (newText.EndsWith(".0")||!AllowDisplayingDecimalNumbers) newText = Mathf.FloorToInt(newValue).ToString();
            txt.text = (newText);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(onValueChanged!=null&&_oldValue!=newValue)
            {
                _oldValue = newValue;
                onValueChanged.Invoke(newValue);
            }
        });
        call.Invoke(slider.value);
        slider.onValueChanged.AddListener(call);
    }
}