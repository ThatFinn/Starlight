using Starlight.EmberMode.Components;

namespace Starlight.EmberMode;

internal static class EmberModeUI
{
    private static bool _initialized = false;
    internal static bool IsActive { get; private set; } = false;

    internal static GameObject EmberModeRoot;
    internal static GameObject EmberModeToolbar;
    
    internal static void Initialize(SystemContext systemContext)
    {
        if (_initialized) return;
        if (!EnableEmberMode.HasFlag()) return;

        EmberModeRoot = new GameObject("EmberModeRoot");
        Object.DontDestroyOnLoad(EmberModeRoot);
        var canvas = EmberModeRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        var scaler = EmberModeRoot.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        EmberModeRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        EmberModeToolbar = new GameObject("EmberModeToolbar");
        EmberModeToolbar.transform.SetParent(EmberModeRoot.transform, false);
        EmberModeToolbar.AddComponent<EmberModeToolbar>();

        var emberModeInfo = new GameObject("EmberModeInfo");
        emberModeInfo.transform.SetParent(EmberModeRoot.transform, false);
        emberModeInfo.AddComponent<EmberModeUIInfo>();

        var mouseIndicator = new GameObject("EmberModeMouseIndicator");
        mouseIndicator.transform.SetParent(EmberModeRoot.transform, false);
        mouseIndicator.AddComponent<EmberModeMouseIndicator>();

        EmberModeRoot.AddComponent<EmberModeTransformGizmo>();
        EmberModeRoot.AddComponent<EmberModeTransformInspector>();
        
        
        EmberModeRoot.SetActive(false);
        IsActive = false;
        
        _initialized = true;
    }

    internal static void Enable()
    {
        if (!_initialized) return;
        if (EmberModeRoot) EmberModeRoot.SetActive(true);
        IsActive = true;
    }

    internal static void Disable()
    {
        if (!_initialized) return;
        if (EmberModeRoot) EmberModeRoot.SetActive(false);
        IsActive = false;
    }

    internal static void OnGUI()
    {
        if (!_initialized) return;
        if (!IsActive) return;
    }
}