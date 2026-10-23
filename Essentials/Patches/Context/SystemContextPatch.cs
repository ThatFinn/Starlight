using System.Linq;
using System.Reflection;
using System;
using Il2CppInterop.Runtime.Injection;
using Starlight.EmberMode;
using Starlight.Enums;
using Starlight.Managers;
using Starlight.Storage;

namespace Starlight.Patches.Context;

[HarmonyPatch(typeof(SystemContext), nameof(SystemContext.Start))]
internal class SystemContextPatch
{
    internal static bool didStart = false;
    internal static Dictionary<string, Shader> loadedShaders = new ();
    internal static Il2CppAssetBundle bundle = null;
    internal static Dictionary<string, Type> menusToInit = new ();
    
    static List<Object> assets = new (); //Prefabs are destroyed
    const string menuPath = "Assets/Menus/";
    const string popUpPath = "Assets/PopUps/";
    const string prefabSuffix = ".prefab";
    internal static string getPopUpPath(string identifier,StarlightMenuTheme currentTheme)
    {
        //now, currentTheme exists
        string extraTheme = "";
        if (currentTheme != StarlightMenuTheme.Native) extraTheme = "_"+currentTheme.ToString().Split(".")[0];
        return $"{popUpPath}{identifier}{extraTheme}{prefabSuffix}";
    }
    internal static string getMenuPath(MenuIdentifier menuIdentifier)
    {
        StarlightSaveManager.data.themes.TryAdd(menuIdentifier.saveKey, menuIdentifier.defaultTheme);
        var currentTheme = StarlightSaveManager.data.themes[menuIdentifier.saveKey];
        var validThemes = MenuEUtil.GetValidThemes(menuIdentifier.saveKey);
        if (validThemes.Count == 0) return null;
        if(!validThemes.Contains(currentTheme)) currentTheme = validThemes.First();
        StarlightSaveManager.Save();
        //now, currentTheme exists
        string extraTheme = "";
        if (currentTheme != StarlightMenuTheme.Native) extraTheme = "_"+currentTheme.ToString().Split(".")[0];
        return $"{menuPath}{menuIdentifier.saveKey}{extraTheme}{prefabSuffix}";
    }

    internal static void Prefix()
    {
        didStart = true;
    }
    internal static void Postfix(SystemContext __instance)
    {
        if(ChangeSystemContextIsModded.HasFlag()) SystemContext.IsModded = true;
        bundle = EmbeddedResourceEUtil.LoadIl2CppBundle("Assets.starlight.bundle");
        foreach (string path in bundle.GetAllAssetNames())
        {
            var asset = bundle.LoadAsset(path);
            if (asset.TryCast<Shader>()!=null)
            {
                var shader = asset.Cast<Shader>();
                shader.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                loadedShaders[asset.name] = shader;

            }
            assets.Add(asset);
            if (path.StartsWith(menuPath, StringComparison.OrdinalIgnoreCase))
                if(path.EndsWith(prefabSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    string menu = path.Substring(menuPath.Length, path.Length - menuPath.Length - prefabSuffix.Length);
                    StarlightMenuTheme theme = StarlightMenuTheme.Native;
                    var split = menu.Split("_");
                    var key = split[0];
                    if (menu.Contains("_"))
                    {
                        if (Enum.TryParse(typeof(StarlightMenuTheme), split[1], true, out object result))
                            theme = (StarlightMenuTheme)result;
                        else continue;
                    }
                    if (!MenuEUtil.ValidThemes.ContainsKey(key)) MenuEUtil.ValidThemes.Add(key,new List<StarlightMenuTheme>());
                    MenuEUtil.ValidThemes[key].Add(theme);
                }
        }
        foreach (var obj in assets)
            if (obj != null)
                if (obj.name == "AllMightyMenus")
                {
                    var instance = Object.Instantiate(obj).TryCast<GameObject>();
                    try { StarlightLogManager.Start(); }
                    catch (Exception e) { LogError(e); }
                    StarlightSaveManager.Start();
                    StarlightCommandManager.Start();
                    StarlightEntryPoint.StarlightStuff = instance;
                    instance.name = "StarlightStuff";
                    instance.SetActive(false);
                    GameObject.DontDestroyOnLoad(instance);
                    
                    ExecuteInTicks(() =>
                    {
                        foreach (var assembly in StarlightPackageManager.GetAllPackageAssemblies())
                        {
                            var exporters = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(StarlightMenu)) && !t.IsAbstract);
                            foreach (var type in exporters)
                                try
                                {
                                    var identifier = type.GetMenuIdentifierByType();
                                    if (!string.IsNullOrWhiteSpace(identifier.saveKey))
                                    {
                                        if (identifier.useNewStyle)
                                        {
                                            MenuEUtil.ValidThemes[identifier.saveKey] = new List<StarlightMenuTheme>();
                                            if (identifier.supportsThemes)
                                            {
                                                foreach (StarlightMenuTheme theme in Enum.GetValues(typeof(StarlightMenuTheme)))
                                                    if (theme != StarlightMenuTheme.None)
                                                        MenuEUtil.ValidThemes[identifier.saveKey].Add(theme);
                                            }
                                            else MenuEUtil.ValidThemes[identifier.saveKey].Add(StarlightMenuTheme.None);
                                        }
                                        var path = getMenuPath(identifier);
                                        bool assetEmpty = true;
                                        if (!string.IsNullOrWhiteSpace(path)) assetEmpty = !bundle.Contains(path);

                                        UnityEngine.Object rootObject = type.GetMenuRootObject();
                                        
                                        if (!assetEmpty&&rootObject==null)
                                        {
                                            rootObject = GameObject.Instantiate(bundle.LoadAsset(path), instance.transform);
                                        }
                                        if (rootObject == null)
                                        {
                                            var message = $"The menu under the name {type.Name} couldn't be loaded! It's root object is null!";
                                            throw new Exception(message);
                                        }

                                        var gameObj = rootObject.Cast<GameObject>();
                                        gameObj.transform.SetParent(instance.transform, false);
                                        try
                                        {
                                            var rectT = gameObj.transform.GetComponent<RectTransform>();
                                            var setAnchor = false;
                                            if (rectT.sizeDelta == Vector2.zero && (rectT.anchorMin != Vector2.zero || rectT.anchorMax != Vector2.one))
                                            {
                                                rectT.sizeDelta = rectT.GetParentSize();
                                                setAnchor = true;
                                            }
                                            if (rectT.sizeDelta.x < 0)
                                                rectT.sizeDelta = new Vector2(-rectT.sizeDelta.x, rectT.sizeDelta.y);
                                            if (rectT.sizeDelta.y < 0)
                                                rectT.sizeDelta = new Vector2(rectT.sizeDelta.x, -rectT.sizeDelta.y);
                                            if(setAnchor)
                                                rectT.anchoredPosition = Vector2.zero;
                                        } catch { }
                                        menusToInit.Add(rootObject.name, type);
                                        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(type))
                                            ClassInjector.RegisterTypeInIl2Cpp(type,
                                                new RegisterTypeOptions() { LogSuccess = false });

                                    }
                                    else LogError($"The menu under the name {type.Name} couldn't be loaded! It's MenuIdentifier is broken!");

                                }
                                catch (Exception e) { LogError(e); }
                        }

                        ExecuteInTicks(() =>
                        {
                            instance.SetActive(true);
                            foreach (var pair in new Dictionary<string, Type>(menusToInit))
                            foreach (var child in instance.GetChildren())
                                if (child.name == pair.Key)
                                {
                                    try
                                    {
                                        child.AddComponent(pair.Value);
                                        child.gameObject.SetActive(true);
                                        menusToInit.Remove(pair.Key);
                                    }
                                    catch (Exception e) { LogError(e); }
                                }
                            StarlightEntryPoint.MenusFinished = true;
                        }, 1);
                    },1);
                    break;
                }

        var lang = __instance.LocalizationDirector.GetCurrentLocaleCode();
                    
        LoadLanguage(lang);
        
        try { EmberModeUI.Initialize(__instance); } 
        catch (Exception e) { LogError(e); }
        try { if (StarlightEntryPoint.enableEmberMode) EmberModeUI.Enable(); }
        catch { }
        
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            try { expansion.AfterSystemContext(__instance); } 
            catch (Exception e) { LogError(e); }
        StarlightCallEventManager.ExecuteWithArgs(CallEvent.AfterSystemContextLoad, ("systemContext", __instance));
    }
}