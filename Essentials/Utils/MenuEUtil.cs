using System;
using System.Linq;
using System.Reflection;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Managers;
using Starlight.Storage;
using Starlight.UI;

namespace Starlight.Utils;

public static class MenuEUtil
{
    internal static readonly Dictionary<string, List<StarlightMenuTheme>> ValidThemes = new ();
    internal static readonly List<StarlightPopUp> OpenPopUps = new ();
    internal static GameObject MenuBlock;
    internal static Transform PopUpBlock;

    internal static void OpenPopUpBlock(StarlightPopUp popUp)
    {
        if (PopUpBlock.transform.GetParent() != popUp.transform.GetParent()) return;
        var instance = Object.Instantiate(PopUpBlock, PopUpBlock.transform);
        instance.gameObject.SetActive(true);
        instance.SetSiblingIndex(popUp.transform.GetSiblingIndex()-1);
        popUp.block = instance;
    }
    internal static void ReloadFont(this StarlightPopUp popUp)
    {
        var dataFont = StarlightMenuFont.Native;
        try
        {
            var ident = GetOpenMenu().GetMenuIdentifier();
            if (string.IsNullOrEmpty(ident.saveKey)) return;
            if (StarlightSaveManager.data.fonts.TryAdd(ident.saveKey, ident.defaultFont)) StarlightSaveManager.Save();
            dataFont = StarlightSaveManager.data.fonts[ident.saveKey];
        } catch { }

        TMP_FontAsset fontAsset = null;
        switch (dataFont)
        {
            case StarlightMenuFont.Default: fontAsset = StarlightEntryPoint.NormalFont; break;
            case StarlightMenuFont.NotoSans: fontAsset = StarlightEntryPoint.NotoSansFont; break;
            case StarlightMenuFont.Bold: fontAsset = StarlightEntryPoint.BoldFont; break;
            case StarlightMenuFont.Native: fontAsset = StarlightEntryPoint.Sr2FontAsset; break;
        }

        if (fontAsset != null) popUp.ApplyFont(fontAsset);
    }
    internal static void ReloadFont(this StarlightMenu menu)
    {
        var ident = menu.GetMenuIdentifier();
        if (string.IsNullOrEmpty(ident.saveKey)) return;
        if (StarlightSaveManager.data.fonts.TryAdd(ident.saveKey, ident.defaultFont)) StarlightSaveManager.Save();
        var dataFont = StarlightSaveManager.data.fonts[ident.saveKey];
        TMP_FontAsset fontAsset = null;
        switch (dataFont)
        {
            case StarlightMenuFont.Default: fontAsset = StarlightEntryPoint.NormalFont; break;
            case StarlightMenuFont.NotoSans: fontAsset = StarlightEntryPoint.NotoSansFont; break;
            case StarlightMenuFont.Bold: fontAsset = StarlightEntryPoint.BoldFont; break;
            case StarlightMenuFont.Native: fontAsset = StarlightEntryPoint.Sr2FontAsset; break;
        }

        if (fontAsset != null) menu.ApplyFont(fontAsset);
    }

    internal static GameObject GetMenuRootObject(this Type type)
    {
        try
        {
            var methodInfo = type.GetMethod(nameof(StarlightMenu.GetMenuRootObject), BindingFlags.Static | BindingFlags.Public);
            if (methodInfo == null) return null;
            dynamic result = methodInfo.Invoke(null, null);
            if (result == null) return null;
            if (result is GameObject gameObject) return gameObject;
        }
        catch (Exception e) { LogError(e); }
        return null;
    }
    internal static MenuIdentifier GetMenuIdentifierByType(this Type type)
    {
        try
        {
            var methodInfo = type.GetMethod(nameof(StarlightMenu.GetMenuIdentifier), BindingFlags.Static | BindingFlags.Public);
            if (methodInfo != null)
            {
                var result = methodInfo.Invoke(null, null);
                if (result == null) return new MenuIdentifier();
                if (result is MenuIdentifier identifier) return identifier;
            }
        }
        catch (Exception e) { LogError(e); }
        return new MenuIdentifier();
    }

    public static MenuIdentifier GetMenuIdentifier(this StarlightMenu menu) => menu.GetType().GetMenuIdentifierByType();
    public static T GetMenu<T>() where T : StarlightMenu
    {
        foreach (var pair in StarlightEntryPoint.Menus)
            if (pair.Key is T key) return key;
        return null;
    }

    public static UITheme GetTheme(this StarlightMenuTheme theme) => UITheme.GetTheme(theme);
    public static FontTheme GetFontTheme(this StarlightMenuFont theme) => FontTheme.GetTheme(theme);
    public static StarlightMenu GetMenu(this MenuIdentifier identifier)
    {
        try
        {
            foreach (var pair in StarlightEntryPoint.Menus)
            {
                var ident = pair.Key.GetMenuIdentifier();
                if (ident.saveKey == identifier.saveKey) return pair.Key;
            }
        }
        catch (Exception e) { LogError(e); }
        return null;
    }
    public static StarlightMenuTheme GetTheme(this StarlightMenu menu)
    {
        try
        {
            var methodInfo = menu.GetType().GetMethod(nameof(StarlightMenu.GetMenuIdentifier), BindingFlags.Static | BindingFlags.Public);
            if (methodInfo != null)
            {
                var result = methodInfo.Invoke(null, null);
                if (result is MenuIdentifier identifier)
                {
                    StarlightSaveManager.data.themes.TryAdd(identifier.saveKey, identifier.defaultTheme);
                    var currentTheme = StarlightSaveManager.data.themes[identifier.saveKey];
                    var validThemes = GetValidThemes(identifier.saveKey);
                    if (validThemes.Count == 0) return StarlightMenuTheme.None;
                    if (!validThemes.Contains(currentTheme)) currentTheme = validThemes.First();
                    return currentTheme;
                }
            }
        } catch { }

        return StarlightMenuTheme.None;
    }
    
    internal static void DoMenuActions(this MenuActions[] actions) => DoMenuActions(actions.ToList());
    internal static void DoMenuActions(this List<MenuActions> actions)
    {
        if(actions.Contains(MenuActions.UnPauseGame)) NativeEUtil.TryUnPauseGame();
        if(actions.Contains(MenuActions.UnPauseGameFalse)) NativeEUtil.TryUnPauseGame(false);
        if(actions.Contains(MenuActions.PauseGameFalse)) NativeEUtil.TryPauseGame(false);
        if(actions.Contains(MenuActions.UnHideMenus)) NativeEUtil.TryUnHideMenus();
        if(actions.Contains(MenuActions.EnableInput)) NativeEUtil.TryEnableSR2Input();
        if(actions.Contains(MenuActions.DisableInput)) NativeEUtil.TryDisableSR2Input();
        if(actions.Contains(MenuActions.OverrideInput)) NativeEUtil.OverrideSR2Input();
        if(actions.Contains(MenuActions.DeOverrideInput)) NativeEUtil.DeOverrideSR2Input();
        if(actions.Contains(MenuActions.PauseGame)&&actions.Contains(MenuActions.HideMenus)) NativeEUtil.TryPauseAndHide();
        else
        {
            if(actions.Contains(MenuActions.HideMenus)) NativeEUtil.TryHideMenus();
            if(actions.Contains(MenuActions.PauseGame)) NativeEUtil.TryPauseGame();
        }

    }
    
    public static List<StarlightMenuTheme> GetValidThemes(string menuSaveKey)
    {
        if (ValidThemes.ContainsKey(menuSaveKey.ToLower()))
            return ValidThemes[menuSaveKey.ToLower()];
        return new List<StarlightMenuTheme>();
    }

    public static bool isAnyPopUpOpen => OpenPopUps.Count != 0;
    public static bool isAnyMenuOpen
    {
        get
        {
            try
            {
                foreach (var child in StarlightEntryPoint.StarlightStuff.GetChildren())
                    if (child.activeSelf)
                        if (child.HasComponent<StarlightMenu>())
                            return true;
            } catch { }

            return false;
        }
    }
    public static void CloseOpenPopUps()
    {
        try
        {
            for (int i = 0; i < StarlightEntryPoint.StarlightStuff.transform.childCount; i++)
            {
                var child = StarlightEntryPoint.StarlightStuff.transform.GetChild(i);
                if (child.HasComponent<StarlightPopUp>())
                {
                    Object.Destroy(child.gameObject);
                }
            }
        } catch { }
    }
    public static void CloseOpenMenu()
    {
        try
        {
            var menu = GetOpenMenu();
            if(menu)
                menu.Close();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }
    public static StarlightMenu GetOpenMenu()
    {
        foreach (var child in StarlightEntryPoint.StarlightStuff.GetChildren())
        {
            if (!child.activeSelf) continue;
            var menu = child.GetComponent<StarlightMenu>();
            if (menu) return menu;
        }
        return null;
    }
    
    private static Sprite _whitePillBg;
    private static Texture2D _whitePillBgTex;

    public static Sprite whitePillBg
    {
        get
        {
            if(_whitePillBg==null)
            {
                try
                {
                    _whitePillBg = Sprite.Create(whitePillBgTex,
                        new Rect(0f, 0f, _whitePillBgTex.width, _whitePillBgTex.height),
                        new Vector2(0.5f, 0.5f), 1f);
                }
                catch (Exception e) { LogError(e); }
            }

            return _whitePillBg;
        }
    }
    public static Texture2D whitePillBgTex
    {
        get
        {
            if(_whitePillBgTex==null)
            {
                foreach (var bundle in Il2CppAssetBundleManager.GetAllLoadedAssetBundles())
                    try
                    {
                        Texture2D tex = bundle.LoadAsset("Assets/UI/Textures/MenuDemo/whitePillBg.png").Cast<Texture2D>();
                        if (tex == null) continue;
                        _whitePillBgTex = tex;
                    } catch { }
                /*try
                {
                    _whitePillBgTex = Get<AssetBundle>("cc50fee78e6b7bdd6142627acdaf89fa.bundle")
                        .LoadAsset("Assets/UI/Textures/MenuDemo/whitePillBg.png").Cast<Texture2D>();
                }
                catch (Exception e)
                {
                    MelonLogger.Error(e);
                }*/
            }
            return _whitePillBgTex;
        }
    }
}