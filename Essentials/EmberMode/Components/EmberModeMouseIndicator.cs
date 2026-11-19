using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Storage;
using Starlight.UI;

namespace Starlight.EmberMode.Components;

[InjectIntoIL]
internal class EmberModeMouseIndicator : MonoBehaviour
{
    private TextMeshProUGUI _text;
    internal enum OverrideState { Default, ForceUnlocked, ForceLocked }
    private static OverrideState _overrideState = OverrideState.Default;
    private float _nextUpdate = 0f;

    internal static OverrideState CurrentOverrideState => _overrideState;

    void Start()
    {
        var rect = gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-8, -22);
        rect.sizeDelta = new Vector2(250, 20);

        _text = gameObject.AddComponent<TextMeshProUGUI>();
        _text.fontSize = 11f;
        _text.color = EmberModeUITheme.TextColor;
        _text.alignment = TextAlignmentOptions.TopRight;
        _text.raycastTarget = false;
        _text.richText = true;
        _text.overflowMode = TextOverflowModes.Overflow;
        _text.enableWordWrapping = false;

        var fontTheme = new FontTheme();
        if (fontTheme.DefaultFont)
            _text.font = fontTheme.DefaultFont;
    }

    void Update()
    {
        if (!_text) return;

        if (LKey.Q.OnKeyDown())
        {
            _overrideState = (OverrideState)(((int)_overrideState + 1) % 3);
            if(DebugLogging.HasFlag()) Log($"[EmberMode/Mouse] State: {_overrideState}");
            _nextUpdate = 0f;
        }

        if (_overrideState == OverrideState.ForceUnlocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (_overrideState == OverrideState.ForceLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Time.time < _nextUpdate) return;
        _nextUpdate = Time.time + 0.2f;

        string state;
        Color color;

        if (_overrideState == OverrideState.ForceUnlocked)
        {
            state = "Force-Unlocked";
            color = EmberModeUITheme.AccentAltColor;
        }
        else if (_overrideState == OverrideState.ForceLocked)
        {
            state = "Force-Locked";
            color = EmberModeUITheme.AccentAltColor;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            state = "Unlocked";
            color = EmberModeUITheme.TextColor;
        }
        else
        {
            state = "Locked";
            color = EmberModeUITheme.TextDimColor;
        }

        _text.text = $"Mouse: {state}";
        _text.color = color;
    }

    void OnDestroy()
    {
        if (_overrideState != OverrideState.Default)
        {
            _overrideState = OverrideState.Default;
        }
    }
}