using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.Components;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.Interaction.Focus;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Starlight.Utils;

public static class NativeEUtil
{
    private static EventSystem _nativeEventSystem;
    private static BaseInputModule _nativeInputModule;
    private static GameObject _starlightEventSystemObj;
    internal static bool ue = false;

    private static BaseUI _hiddenGameUI;
    private static bool _hudHiddenByStarlight;
    private static bool _hudWasVisible;
    private static bool _inputDisabledByStarlight;
    private static bool _mainGameMapWasEnabled;
    private static bool _pausedMapWasEnabled;
    private static bool _pausedByStarlight;
    
    public static void OverrideSR2Input()
    {
        // Fixes compatibility with UE
        if (ue)
        {
            var method = AccessTools.Method("UniverseLib.Input.EventSystemHelper:EnableEventSystem");

            if (method != null)
            {
                try
                {
                    method.Invoke(null,null);
                }
                catch (Exception e) { LogError(e); }
                return;
            }
        }
        try
        {
            if (_starlightEventSystemObj) return;
            var natives = GetAllInScene<FocusManagerBehaviour>();
            EventSystem native = null;
            foreach (var fmb in natives)
            {
                if (!fmb.gameObject.activeSelf) continue;
                if (!fmb.gameObject.active) continue;
                native = fmb._eventSystem;
                break;
            }
            if (!native) return;
            _nativeEventSystem = native;
            _nativeInputModule = _nativeEventSystem.currentInputModule;
            _nativeEventSystem.enabled = false;
            _starlightEventSystemObj = new GameObject("StarlightEventSystem");
            Object.DontDestroyOnLoad(_starlightEventSystemObj);
            _starlightEventSystemObj.AddComponent<EventSystem>();
            _starlightEventSystemObj.AddComponent<InputSystemUIInputModule>();
            EventSystem.current = _starlightEventSystemObj.GetComponent<EventSystem>();
        }
        catch (Exception e) { LogError(e); }
    }

    public static void DeOverrideSR2Input()
    {
        // Fixes compatibility with UE
        if (ue)
        {
            var method = AccessTools.Method("UniverseLib.Input.EventSystemHelper:ReleaseEventSystem");

            if (method != null)
            {
                try
                {
                    method.Invoke(null,null);
                }
                catch (Exception e) { LogError(e); }
                return;
            }
        }
        try
        {
            if (_starlightEventSystemObj) Object.Destroy(_starlightEventSystemObj);
            if (_nativeEventSystem)
            {
                _nativeEventSystem.enabled = true;
                if (_nativeInputModule)
                    _nativeInputModule.ActivateModule();
            }
        }
        catch (Exception e) { LogError(e); }
    }
    
    public static void TryHideMenus()
    {
        if (StarlightEntryPoint.MainMenuLoaded && !inGame)
        {
            try
            {
                var ui = GetAnyInScene<MainMenuLandingRootUI>();
                if (ui)
                {
                    ui.gameObject.SetActive(false);
                    ui.enabled = false;
                    ui.Close(true);
                }
            }
            catch (Exception e) { LogError(e); }
        }

        if (inGame)
        {
            try
            {
                var pauseMenu = GetAnyInScene<PauseMenuRoot>();
                if (pauseMenu)
                {
                    pauseMenu.HideUI();
                    _hiddenGameUI = pauseMenu;
                }
            } catch { }
        }
    }

    public static void TryPauseAndHide()
    {
        if (inGame&&Object.FindObjectOfType<PauseMenuRoot>())
        {
            TryHideMenus();
            TryPauseGame(false);
            if (inGame) HideHud();
        }
        else
        {
            TryPauseGame();
            TryHideMenus();
        }
    }

    public static void TryPauseGame(bool usePauseMenu = true)
    {
        if (StarlightEntryPoint.MainMenuLoaded)
        {
            Time.timeScale = 0;
            _pausedByStarlight = true;
        }
        
        try { systemContext.SceneLoader.TryPauseGame(); } catch { }

        if (usePauseMenu)
            try { sceneContext.PauseMenuDirector.PauseGame(); } catch { }
    }

    public static void TryUnPauseGame(bool usePauseMenu = true)
    {

        if (StarlightEntryPoint.MainMenuLoaded)
            Time.timeScale = 1;
        _pausedByStarlight = false;
        
        try { systemContext.SceneLoader.UnpauseGame(); } catch { }

        if (usePauseMenu)
            try { sceneContext.PauseMenuDirector.UnPauseGame(); } catch { }
    }

    public static void TryUnHideMenus()
    {
        try
        {
            if (StarlightEntryPoint.MainMenuLoaded && !inGame)
            {
                try
                {
                    foreach (var container in GetAllInScene<UIDisplayContainer>())
                        if (container.TargetContainer.name == "MainMenuRoot" && container.name == "MainMenuRoot")
                        {
                            try
                            {

                                container.OnEnable();
                                break;
                            } catch { }
                        }
                } catch { }
            }
            if (_hiddenGameUI)
            {
                try { _hiddenGameUI.ShowUI(); } catch { }
                _hiddenGameUI = null;
            }
            if (inGame) ShowHud();
        } catch { }
    }

    static void HideHud()
    {
        try
        {
            var hud = HudUI.Instance.transform.GetChild(0).gameObject;
            if (!_hudHiddenByStarlight)
            {
                _hudWasVisible = hud.activeSelf;
                _hudHiddenByStarlight = true;
            }
            hud.SetActive(false);
        } catch { }
    }

    static void ShowHud()
    {
        try
        {
            var hud = HudUI.Instance.transform.GetChild(0).gameObject;
            hud.SetActive(!_hudHiddenByStarlight || _hudWasVisible);
            _hudHiddenByStarlight = false;
        } catch { }
    }

    private static float _customTimeScale = 1f;

    public static float CustomTimeScale
    {
        get => _customTimeScale;
        set
        {
            _customTimeScale = value;
            if (value < 0.01f) _customTimeScale = 0.01f;
            if (value > 2000f) _customTimeScale = 2000f;
            StarlightEntryPoint.CheckForTime();
        }
    }

    public static void TryDisableSR2Input()
    {
        try
        {
            var paused = gameContext.InputDirector._paused.Map;
            var mainGame = gameContext.InputDirector._mainGame.Map;
            if (!_inputDisabledByStarlight)
            {
                _pausedMapWasEnabled = paused.enabled;
                _mainGameMapWasEnabled = mainGame.enabled;
                _inputDisabledByStarlight = true;
            }
            paused.Disable();
            mainGame.Disable();
        } catch { }
    }

    public static void TryEnableSR2Input()
    {
        if (!_inputDisabledByStarlight) return;

        try
        {
            var paused = gameContext.InputDirector._paused.Map;
            var mainGame = gameContext.InputDirector._mainGame.Map;

            if (_pausedMapWasEnabled) paused.Enable();
            if (_mainGameMapWasEnabled) mainGame.Enable();
        } catch { }

        _inputDisabledByStarlight = false;
    }
    
    internal static void ReleaseForSceneLoad()
    {
        if (MenuEUtil.isAnyMenuOpen) return;

        if (_inputDisabledByStarlight)
        {
            _inputDisabledByStarlight = false;
            try { gameContext.InputDirector._mainGame.Map.Enable(); } catch { }
        }

        if (!_pausedByStarlight) return;

        _pausedByStarlight = false;
        Time.timeScale = CustomTimeScale;
        try { systemContext.SceneLoader.UnpauseGame(); } catch { }
    }
}