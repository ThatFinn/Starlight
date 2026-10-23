using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;
using Starlight.UI.Blueprints;
using Starlight.Storage;
using Starlight.UI;
using UnityEngine.UI;

namespace Starlight.EmberMode.Components;

[InjectIntoIL]
internal class EmberModeTransformInspector : MonoBehaviour
{
    public static EmberModeTransformInspector Instance;

    private GameObject _windowRoot;
    public GameObject WindowRoot => _windowRoot;
    private RectTransform _windowRect;

    private GameObject _lastSelected;
    private TMP_InputField _nameInput;

    private class Vec3Inputs
    {
        public TMP_InputField X, Y, Z;
    }

    private Vec3Inputs _posInputs;
    private Vec3Inputs _lPosInputs;
    private Vec3Inputs _rotInputs;
    private Vec3Inputs _lRotInputs;
    private Vec3Inputs _scaleInputs;
    
    private Button _btnMove;
    private Button _btnRotate;
    private Button _btnScale;
    
    private Image _imgMove;
    private Image _imgRotate;
    private Image _imgScale;

    private bool _ignoreChanges = false;
    private FontTheme _fontTheme;

    void Awake()
    {
        Instance = this;
        BuildUI();
    }

    private void BuildUI()
    {
        _fontTheme = new FontTheme();

        var theme = EmberModeUITheme.Create();

        var windowBlueprint = new WindowUIBlueprintV01()
        {
            Title = "Transform Inspector",
            CustomBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
            CustomTitleBarColor = new Color(0.2f, 0.2f, 0.2f, 1f),
            TitleHeight = 30f,
            Draggable = true,
            Resizable = true,
            MinSize = new Vector2(300, 250),
            Size = new Vector2(380, 450),
            Position = new Vector2(20, -100),
            Pivot = new Vector2(0, 1),
            Anchors = new Vector4(0, 1, 0, 1),
        };

        _windowRect = windowBlueprint.Render(theme, _fontTheme, transform);
        _windowRoot = _windowRect.gameObject;
        _windowRoot.name = "EmberModeTransformInspectorUI";

        
        var contentRect = _windowRoot.transform.Find("Content").GetComponent<RectTransform>();
        var contentObj = contentRect.gameObject;

        var vLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 5;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.childForceExpandWidth = true;
        vLayout.childForceExpandHeight = false;

        
        var toolbarRow = CreateRow(contentRect, "ToolsRow", 30);
        _btnMove = CreateButton(toolbarRow, "Move",
            () => EmberModeTransformGizmo.CurrentTool = EmberModeTransformGizmo.TransformTool.Move, out _imgMove);
        _btnRotate = CreateButton(toolbarRow, "Rotate",
            () => EmberModeTransformGizmo.CurrentTool = EmberModeTransformGizmo.TransformTool.Rotate, out _imgRotate);
        _btnScale = CreateButton(toolbarRow, "Scale",
            () => EmberModeTransformGizmo.CurrentTool = EmberModeTransformGizmo.TransformTool.Scale, out _imgScale);

        CreateSpacer(contentRect);

        
        var nameRow = CreateRow(contentRect, "NameRow", 25);
        CreateLabel(nameRow, "Name", 70);
        _nameInput = CreateInputField(nameRow, "NameInput");
        _nameInput.onEndEdit.AddListener((System.Action<string>)(val =>
        {
            if (_lastSelected && _lastSelected.name != val) _lastSelected.name = val;
        }));

        
        _posInputs = CreateVectorRow(contentRect, "Position", val =>
        {
            if (_lastSelected) _lastSelected.transform.position = val;
        });
        _lPosInputs = CreateVectorRow(contentRect, "Local Pos", val =>
        {
            if (_lastSelected) _lastSelected.transform.localPosition = val;
        });
        _rotInputs = CreateVectorRow(contentRect, "Rotation", val =>
        {
            if (_lastSelected) _lastSelected.transform.eulerAngles = val;
        });
        _lRotInputs = CreateVectorRow(contentRect, "Local Rot", val =>
        {
            if (_lastSelected) _lastSelected.transform.localEulerAngles = val;
        });
        _scaleInputs = CreateVectorRow(contentRect, "Scale", val =>
        {
            if (_lastSelected) _lastSelected.transform.localScale = val;
        });

        _windowRoot.SetActive(false);
    }

    private RectTransform CreateRow(RectTransform parent, string name, float height)
    {
        var rowObj = new GameObject(name);
        var rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.SetParent(parent, false);
        var layoutElement = rowObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = height;
        layoutElement.preferredHeight = height;

        var hLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 5;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        return rowRect;
    }

    private void CreateSpacer(RectTransform parent)
    {
        var spacer = new GameObject("Spacer");
        var rect = spacer.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var layoutElement = spacer.AddComponent<LayoutElement>();
        layoutElement.minHeight = 10;
    }

    private void CreateLabel(RectTransform parent, string text, float width)
    {
        var obj = new GameObject("Label");
        var rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        var layoutElement = obj.AddComponent<LayoutElement>();
        layoutElement.minWidth = width;
        layoutElement.preferredWidth = width;
        layoutElement.flexibleWidth = 0;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = _fontTheme.DefaultFont;
        tmp.text = text;
        tmp.fontSize = 14f;
        tmp.alignment = TextAlignmentOptions.Left;
    }

    [HideFromIl2Cpp] private Button CreateButton(RectTransform parent, string text, System.Action onClick, out Image bgImage)
    {
        var obj = new GameObject("Button_" + text);
        var rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        bgImage = obj.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = bgImage;
        btn.onClick.AddListener(onClick);

        var txtObj = new GameObject("Text");
        var txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.SetParent(rect, false);
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.font = _fontTheme.DefaultFont;
        tmp.text = text;
        tmp.fontSize = 14f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btn;
    }

    private TMP_InputField CreateInputField(RectTransform parent, string name)
    {
        var obj = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
        obj.name = name;
        var rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var input = obj.GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.Standard;
        input.lineType = TMP_InputField.LineType.SingleLine;

        var tmp = input.textComponent;
        tmp.font = _fontTheme.DefaultFont;
        tmp.fontSize = 14f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;

        var placeholder = input.placeholder.GetComponent<TextMeshProUGUI>();
        placeholder.font = _fontTheme.DefaultFont;
        placeholder.fontSize = 14f;
        placeholder.text = "";

        var bgImage = obj.GetComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        return input;
    }

    [HideFromIl2Cpp] private Vec3Inputs CreateVectorRow(RectTransform parent, string label, System.Action<Vector3> onUpdate)
    {
        var row = CreateRow(parent, label + "Row", 25);
        CreateLabel(row, label, 70);

        var inputs = new Vec3Inputs();
        inputs.X = CreateInputField(row, "X");
        inputs.Y = CreateInputField(row, "Y");
        inputs.Z = CreateInputField(row, "Z");

        System.Action<string> handler = val =>
        {
            if (_ignoreChanges) return;
            if (float.TryParse(inputs.X.text, out float x) &&
                float.TryParse(inputs.Y.text, out float y) &&
                float.TryParse(inputs.Z.text, out float z))
            {
                onUpdate(new Vector3(x, y, z));
            }
        };

        inputs.X.onEndEdit.AddListener(handler);
        inputs.Y.onEndEdit.AddListener(handler);
        inputs.Z.onEndEdit.AddListener(handler);

        return inputs;
    }

    void Update()
    {
        UpdateVisibilityAndSelection();
        if (!_windowRoot || !_windowRoot.activeSelf) return;

        UpdateTransformValues();
        UpdateToolbarVisuals();
    }

    private void UpdateVisibilityAndSelection()
    {
        if (_windowRoot && _windowRoot.activeSelf)
        {
            var obj = EmberModeTransformGizmo.SelectedObject;
            if (obj != _lastSelected)
            {
                _lastSelected = obj;
                _ignoreChanges = true;
                if (obj) _nameInput.text = obj.name;
                _ignoreChanges = false;
            }
        }
    }

    private void UpdateTransformValues()
    {
        if (!_lastSelected) return;
        var obj = _lastSelected;

        _ignoreChanges = true;

        SyncVectorIfUnfocused(_posInputs, obj.transform.position);
        SyncVectorIfUnfocused(_lPosInputs, obj.transform.localPosition);
        SyncVectorIfUnfocused(_rotInputs, obj.transform.eulerAngles);
        SyncVectorIfUnfocused(_lRotInputs, obj.transform.localEulerAngles);
        SyncVectorIfUnfocused(_scaleInputs, obj.transform.localScale);

        _ignoreChanges = false;
    }

    [HideFromIl2Cpp]
    private void SyncVectorIfUnfocused(Vec3Inputs inputs, Vector3 val)
    {
        if (!inputs.X.isFocused) inputs.X.text = val.x.ToString("F3");
        if (!inputs.Y.isFocused) inputs.Y.text = val.y.ToString("F3");
        if (!inputs.Z.isFocused) inputs.Z.text = val.z.ToString("F3");
    }

    private void UpdateToolbarVisuals()
    {
        var tool = EmberModeTransformGizmo.CurrentTool;
        var activeColor = new Color(0.2f, 0.6f, 1f, 1f);
        var inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        _imgMove.color = tool == EmberModeTransformGizmo.TransformTool.Move ? activeColor : inactiveColor;
        _imgRotate.color = tool == EmberModeTransformGizmo.TransformTool.Rotate ? activeColor : inactiveColor;
        _imgScale.color = tool == EmberModeTransformGizmo.TransformTool.Scale ? activeColor : inactiveColor;
    }
}