using Il2CppTMPro;
using Starlight.Components.AssetBundle;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class InputUIBlueprintV01 : UIBlueprint
{
    public UIColor TextColor = UIColor.TextGeneral;
    public Color? CustomTextColor = null;
    public UIColor BackgroundColor = UIColor.Primary;
    public Color? CustomBackgroundColor = null;
    public Sprite CustomCheckSprite;
    public TMP_InputField.ContentType ContentType = TMP_InputField.ContentType.Standard;
    public TMP_InputField.LineType LineType = TMP_InputField.LineType.SingleLine;
    public string PlaceHolderContent = "";
    public Color? CustomPlaceHolderColor = null;
    public bool DisablePlaceHolderAutoTranslation = false;
    public string DefaultValue = "";
    public float FontSize = 20f;
    public Vector4? Margins = null;
    public bool RestoreOriginalTextOnEscape = true;
    public TMP_FontAsset CustomFont;
    public System.Action<string> OnValueChanged = null;
    public System.Action<string> OnEndEdit = null;
    public System.Action<string> OnSelect = null;
    public System.Action<string> OnDeselect = null;
    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        IgnoreCorners = true;
        var inputObject = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
        inputObject.transform.SetParent(obj, false);
        if(CornerRadius>0)
        {
            var sortGroup = inputObject.AddComponent<SortingGroup>();
            sortGroup.enabled = false;
            sortGroup.sortingOrder = Mathf.FloorToInt(CornerRadius * ScaleFactor);
            inputObject.AddComponent<RoundedUIImage>().CornerRadius = CornerRadius * ScaleFactor;
        }
        var inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = new Vector2(1, 1);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;
        inputRect.anchoredPosition = Vector2.zero;
        
        var textArea = inputObject.transform.Find("Text Area") as RectTransform;
        if (textArea != null && Margins.HasValue)
        {
            textArea.offsetMin = new Vector2(Margins.Value.x, Margins.Value.w);
            textArea.offsetMax = new Vector2(-Margins.Value.z, -Margins.Value.y);
        }
        
        var inputField = inputObject.GetComponent<TMP_InputField>();
        inputField.contentType = ContentType;
        inputField.text = DefaultValue;
        inputField.lineType = LineType;
        inputField.restoreOriginalTextOnEscape = RestoreOriginalTextOnEscape;
        
        var txt = inputField.textComponent;
        txt.fontSize = FontSize*ScaleFactor;
        txt.font = CustomFont ?? fontTheme.DefaultFont;
        txt.color = CustomTextColor ?? theme.GetColor(TextColor);
        
        var placeholder = inputField.placeholder.GetComponent<TextMeshProUGUI>();
        placeholder.text = DisablePlaceHolderAutoTranslation?PlaceHolderContent:Tr(PlaceHolderContent);
        placeholder.fontSize = FontSize*ScaleFactor;
        placeholder.font = CustomFont ?? fontTheme.DefaultFont;
        placeholder.color = CustomPlaceHolderColor ?? (CustomTextColor ?? theme.GetColor(TextColor))*new Color(0.2f,0.2f,0.2f,.5f);
        
        var backgroundImage = inputObject.GetComponent<Image>();
        backgroundImage.color = CustomBackgroundColor ?? theme.GetColor(BackgroundColor);
        backgroundImage.sprite = CustomCheckSprite;
        
        if(OnValueChanged!=null) inputField.onValueChanged.AddListener(OnValueChanged);
        if(OnEndEdit!=null) inputField.onEndEdit.AddListener(OnEndEdit);
        if(OnSelect!=null) inputField.onSelect.AddListener(OnSelect);
        if(OnDeselect!=null) inputField.onDeselect.AddListener(OnDeselect);
    }
}