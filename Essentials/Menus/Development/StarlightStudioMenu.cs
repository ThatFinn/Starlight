using Starlight.Components;
using Starlight.Enums;
using Starlight.Managers;
using Starlight.Prism.Data.Native;
using Starlight.Storage;
using Starlight.UI;
using Starlight.UI.Blueprints;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Starlight.Menus.Development;

internal class StarlightStudioMenu : StarlightMenu
{
    public new static MenuIdentifier GetMenuIdentifier() => new ("starlightstudiomenu",StarlightMenuFont.Native,StarlightMenuTheme.Starlight, "StarlightStudioMenu",true,true);

    private RectTransform _openThing;
    protected override bool createCommands => false;
    protected override bool inGameOnly => false;
    
    private StarlightSlimeRenderer _slimeRenderer;
    private Camera _previewCamera;
    private RenderTexture _renderTexture;
    private Volume _volume;
    private readonly Vector3 _viewAngle = new(3, -8, 0);
    private readonly float _distance = 2.75f;
    private readonly int _targetLayer = 1 << 28;
    protected override void OnAwake()
    {
        GetComponent<RectTransform>().localPosition = Vector2.zero;
        requiredFeatures = [EnableStudioMenu];
        openActions = [MenuActions.PauseGameFalse];
        closeActions = [MenuActions.UnPauseGameFalse, MenuActions.EnableInput];
    }

    protected override void OnLateAwake()
    {
        var rendererObj = new GameObject("StudioSlimeRenderer");
        DontDestroyOnLoad(rendererObj);
        _slimeRenderer = rendererObj.AddComponent<StarlightSlimeRenderer>();
        _slimeRenderer.unscaledTime = true;
        
        _renderTexture = new RenderTexture(1024, 1024, 24);
        _renderTexture.name = "StudioSlimeRendererTexture";
        
        var camObj = new GameObject("StudioSlimeRendererCamera");
        DontDestroyOnLoad(camObj);
        _previewCamera = camObj.AddComponent<Camera>();
        _volume = camObj.AddComponent<Volume>();

        _volume.isGlobal = false;
        _volume.priority = 1000000;
        var collider = camObj.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(20f,20f,20f);
        
        _previewCamera.farClipPlane = 10f;
        _previewCamera.targetTexture = _renderTexture;
        _previewCamera.clearFlags = CameraClearFlags.SolidColor;
        _previewCamera.backgroundColor = new Color(0, 0, 0, 0);

        _previewCamera.cullingMask = 1 << _targetLayer;
        var hdData = camObj.AddComponent<HDAdditionalCameraData>();
        hdData.volumeAnchorOverride = _previewCamera.transform;
        hdData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
        hdData.backgroundColorHDR = new Color(0, 0, 0, 0);

        var rotation = Quaternion.Euler(_viewAngle);
        var positionOffset = rotation * Vector3.back * _distance;
        
        _previewCamera.transform.position = _slimeRenderer.transform.position + positionOffset;
        _previewCamera.transform.LookAt(_slimeRenderer.transform.position);

        _previewCamera.transform.position -= new Vector3(10000f, 9999.2f, 10000f);
        _slimeRenderer.transform.position -= new Vector3(10000f, 10000f, 10000f);
        _slimeRenderer.transform.rotation = Quaternion.Euler(0f,20f,0f);
        SetLayerRecursive(rendererObj, _targetLayer);
        SetLayerRecursive(camObj, _targetLayer);
        
        rendererObj.SetActive(false);
        camObj.SetActive(false);
    }
    void SetLayerRecursive(GameObject obj, int newLayer)
    {
        if (null == obj) return;
        obj.layer = newLayer;
        foreach (var child in obj.transform.GetChildren())
        {
            if (null == child) continue;
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }
    public new static GameObject GetMenuRootObject()
    {
        var obj = new GameObject("StarlightStudioMenu");
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        return obj;
    }

    protected override void OnOpen()
    {
        _slimeRenderer.gameObject.SetActive(true);
        _previewCamera.gameObject.SetActive(true);
        try
        {
            _volume.profile = Instantiate(StarlightVolumeProfileManager.Presets["POTATO"]);
            //if (_volume.profile.TryGet<VisualEnvironment>(out var visualEnv ))
        } catch { }
        var angler = PrismNativeBaseSlime.Angler.Find();
        if (angler != null) _slimeRenderer.RenderAppearance(angler, 1);
        _openThing = new PanelUIBlueprintV01()
        {
            Color = UIColor.Primary, CornerRadius = 90, Size = new Vector2(1330, 840),
            Children =
            [
                new PanelUIBlueprintV01() { Color = UIColor.Accent, Size = new Vector2(1330,10), Position = new Vector2(0,-265f) },
                new PanelUIBlueprintV01() { Color = UIColor.Accent, Size = new Vector2(1330,10), Position = new Vector2(0,265f) },
                new NavigationUIBlueprintV01()
                {
                    CornerRadius = 90, Size = new Vector2(1330, 840),
                    TabSize = new Vector2(1330,520),
                    Tabs = [
                        new PanelUIBlueprintV01()
                        {
                            Name = "Pink",
                            Children = [
                                new PanelUIBlueprintV01()
                                {
                                    Color = UIColor.Space3DBackground,
                                    Position = new Vector2(530,0),
                                    Size = new Vector2(270,520),
                                },
                                new PanelUIBlueprintV01()
                                {
                                    Color = UIColor.None, Sprite = EmbeddedResourceEUtil.LoadSprite("Assets.podest.png"),
                                    Position = new Vector2(530,-180),
                                    Size = new Vector2(250,250),
                                },
                                new TextureUIBlueprintV01()
                                {
                                    Position = new Vector2(540,0),
                                    Size = new Vector2(505,520),
                                    Texture = _renderTexture
                                },
                                new PanelUIBlueprintV01()
                                {
                                    Color = UIColor.None, Sprite = EmbeddedResourceEUtil.LoadSprite("Assets.light.png"),
                                    Position = new Vector2(530,0),
                                    Size = new Vector2(250,520),
                                },
                            ]
                        }
                    ]
                }
            ]
        }.Render(currentTheme, currentFontTheme, transform);
        foreach (var lit in GetAllInScene<Light>())
        {
            if (!lit) continue;
            lit.cullingMask &= ~_targetLayer;
        }
    }

    protected override void OnClose()
    {
        _slimeRenderer.DestroyRender();
        _slimeRenderer.gameObject.SetActive(false);
        _previewCamera.gameObject.SetActive(false);
        Destroy(_openThing.gameObject);
    }
    
    public override void OnCloseUIPressed()
    {
        if (MenuEUtil.isAnyPopUpOpen) return;
        Close();
    }
}