using System;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;
using Starlight.Commands;
using Starlight.Enums;
using Starlight.Enums.Features;
using Starlight.Enums.Sounds;
using Starlight.Managers;
using Starlight.Storage;
using Starlight.UI;

namespace Starlight;

/// <summary>
/// Abstract menu class
/// </summary>
[InjectIntoIL]
public abstract class StarlightMenu : MonoBehaviour
{
    public bool IsPermanentlyDisabled() => _inActive;
    private bool _inActive;
    private bool _changedOpenState;

    public static GameObject GetMenuRootObject() => null;
    public static MenuIdentifier GetMenuIdentifier() => new();

    private StarlightMenu _menuToOpenOnClose;
    protected virtual bool createCommands => false;
    
    protected virtual bool inGameOnly => false;

    [HideFromIl2Cpp] public MenuActions[] openActions
    {
        get { try { return (StarlightEntryPoint.Menus[this][nameof(openActions)] as List<MenuActions>)?.ToArray(); } catch { return Array.Empty<MenuActions>(); } }
        set
        {
            StarlightEntryPoint.Menus ??= new Dictionary<StarlightMenu, Dictionary<string, object>>();
            if (!StarlightEntryPoint.Menus.ContainsKey(this)) StarlightEntryPoint.Menus[this] = new Dictionary<string, object>();
            StarlightEntryPoint.Menus[this][nameof(openActions)] = value.ToNetList();
        }
    }
    [HideFromIl2Cpp] public MenuActions[] closeActions
    {
        get { try { return (StarlightEntryPoint.Menus[this][nameof(closeActions)] as List<MenuActions>)?.ToArray(); } catch { return Array.Empty<MenuActions>(); } }
        set
        {
            StarlightEntryPoint.Menus ??= new Dictionary<StarlightMenu, Dictionary<string, object>>();
            if (!StarlightEntryPoint.Menus.ContainsKey(this)) StarlightEntryPoint.Menus[this] = new Dictionary<string, object>();
            StarlightEntryPoint.Menus[this][nameof(closeActions)] = value.ToNetList();
        }
    }
    [HideFromIl2Cpp] public FeatureFlag[] requiredFeatures
    {
        get { try { return (StarlightEntryPoint.Menus[this][nameof(requiredFeatures)] as List<FeatureFlag>)?.ToArray(); } catch { return Array.Empty<FeatureFlag>(); } }
        set
        {
            StarlightEntryPoint.Menus ??= new Dictionary<StarlightMenu, Dictionary<string, object>>();
            if (!StarlightEntryPoint.Menus.ContainsKey(this)) StarlightEntryPoint.Menus[this] = new Dictionary<string, object>();
            StarlightEntryPoint.Menus[this][nameof(requiredFeatures)] = value.ToNetList();
        }
    }
    protected virtual void OnClose()
    {
    }

    protected virtual void OnOpen()
    {
    }
    public virtual void OnCloseUIPressed()
    {
    }

    [HideFromIl2Cpp] protected FontTheme currentFontTheme => StarlightSaveManager.data.fonts[this.GetMenuIdentifier().saveKey].GetFontTheme(); 
    [HideFromIl2Cpp] protected UITheme currentTheme => StarlightSaveManager.data.themes[this.GetMenuIdentifier().saveKey].GetTheme(); 
    public virtual void ApplyFont(TMP_FontAsset font)
    {
        foreach (var text in gameObject.GetAllChildrenOfType<TMP_Text>())
            text.font = font;
    }

    public void Awake()
    {
        try {OnAwake();}
        catch (Exception e) { LogError(e); }
        StarlightEntryPoint.Menus.TryAdd(this, new Dictionary<string, object>()
        {
            { "requiredFeatures", new List<FeatureFlag>() },
            { "openActions", new List<MenuActions> { MenuActions.PauseGame, MenuActions.HideMenus } },
            { "closeActions", new List<MenuActions> { MenuActions.UnPauseGame, MenuActions.UnHideMenus, MenuActions.EnableInput } },
        });
        if (requiredFeatures.Any(featureFlag => !featureFlag.HasFlag()))
        {
            _inActive = true;
            return;
        }
        if (createCommands)
        {
            bool error = false;
            try
            {
                var identifier = this.GetMenuIdentifier();
                if (!string.IsNullOrEmpty(identifier.saveKey))
                {
                    try { StarlightCommandManager.RegisterCommand(new MenuVisibilityCommands.OpenCommand(identifier, this, inGameOnly)); }
                    catch (Exception e) { error=true; LogError(e); }

                    try { StarlightCommandManager.RegisterCommand(new MenuVisibilityCommands.ToggleCommand(identifier, this, inGameOnly)); }
                    catch (Exception e) { error=true; LogError(e); }

                    try { StarlightCommandManager.RegisterCommand(new MenuVisibilityCommands.CloseCommand(identifier, this, inGameOnly)); } 
                    catch (Exception e) { error=true; LogError(e); }
                }
            }
            catch
            {
                error = true;
            }
            if(error) LogError("There was an error creating menu commands");
        }

        if (MenuEUtil.MenuBlock == null)
            MenuEUtil.MenuBlock = transform.parent.GetObjectRecursively<GameObject>("blockRec");
        if (MenuEUtil.PopUpBlock == null)
            MenuEUtil.PopUpBlock = transform.parent.GetObjectRecursively<Transform>("blockPopUpRec");
        try {OnLateAwake();}
        catch (Exception e) { LogError(e); }
    }

    protected virtual void OnAwake()
    {
    }

    protected virtual void OnLateAwake()
    {
    }

    protected virtual void OnStart()
    {
    }

    protected void Start()
    {
        try {OnStart();}
        catch (Exception e) { LogError(e); }
        gameObject.SetActive(false);
    }

    protected internal void AlwaysUpdate()
    {
        _changedOpenState = false;
        try {OnAlwaysUpdate();}
        catch (Exception e) { LogError(e); }
    }

    protected virtual void OnAlwaysUpdate()
    {
    }

    protected void Update()
    {
        _changedOpenState = false;
        try {OnUpdate();}
        catch (Exception e) { LogError(e); }
    }

    protected virtual void OnUpdate()
    {
    }

    private bool _closing;
    public new void Close()
    {
        _closing = true;
        if (_changedOpenState) return;
        if (_inActive) return;
        if (!isOpen) return;
        MenuEUtil.MenuBlock.SetActive(false);
        gameObject.SetActive(false);
        _changedOpenState = true;
        foreach (StarlightPopUp popUp in MenuEUtil.OpenPopUps) popUp.Close();
        (StarlightEntryPoint.Menus[this]["closeActions"] as List<MenuActions>).DoMenuActions();
        try
        {
            OnClose();
        }
        catch (Exception e)
        {
            LogError(e);
        }

        _closing = false;
        if (_menuToOpenOnClose != null)
            ExecuteInTicks((() =>
            {
                _menuToOpenOnClose.TryCast<StarlightMenu>()?.Open();
                _menuToOpenOnClose = null;
            }), 2);
        AudioEUtil.PlaySound(MenuSound.CloseMenu);
    }

    [HideFromIl2Cpp]
    protected Action selectCategorySound
    {
        get
        {
            return (Action)(() =>
            {
                if(_closing) return;
                AudioEUtil.PlaySound(MenuSound.SelectCategory);
            });
        }
    }
    [HideFromIl2Cpp] public void OpenC(StarlightMenu menuToOpenOnClose)
    {
        _menuToOpenOnClose = menuToOpenOnClose;
        Open();
    }

    public new void Open()
    {
        if (_changedOpenState) return;
        if (_inActive) return;
        if (MenuEUtil.isAnyMenuOpen) return;
        if(inGameOnly) if (!inGame) return;
        if (StarlightWarpManager.WarpTo != null) return;
        foreach (var pair in StarlightEntryPoint.Menus)
            if(pair.Key!=this) pair.Key._menuToOpenOnClose = null;
        
        switch (systemContext.SceneLoader.CurrentSceneGroup.name)
        {
            case "StandaloneStart":
            case "CompanyLogo":
            case "LoadScene":
                return;
        }
        MenuEUtil.MenuBlock.SetActive(true);
        gameObject.SetActive(true);
        _changedOpenState = true;
        ExecuteInTicks((() => { gameObject.SetActive(true);}), 1);
        (StarlightEntryPoint.Menus[this]["openActions"] as List<MenuActions>).DoMenuActions();
        try { OnOpen(); }catch (Exception e) { LogError(e); }
        foreach (var pair in ToTranslate) pair.Key.text = (Tr(pair.Value));
        AudioEUtil.PlaySound(MenuSound.OpenMenu);
    }
    
    public new void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public bool isOpen => !_inActive && gameObject.activeSelf;

    protected readonly Dictionary<TextMeshProUGUI, string> ToTranslate = new ();

    protected Sprite whitePillBg => MenuEUtil.whitePillBg; 
    protected Texture2D whitePillBgTex => MenuEUtil.whitePillBgTex;

    public virtual void OnThemeChange()
    {
        
    }
    
    /// <summary>
    /// Gets executed once GameContext loads. In Postfix of the Start method
    /// </summary>
    public virtual void AfterGameContext(GameContext gameContext) { }
}

