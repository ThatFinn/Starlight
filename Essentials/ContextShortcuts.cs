using System.Collections;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Damage;
using MelonLoader;

namespace Starlight;

public static class ContextShortcuts
{
    private static GameObject? _prefabHolder;
    
    internal static GameObject PrefabHolder
    {
        get
        {
            if (_prefabHolder) return _prefabHolder!;

            _prefabHolder = Get<GameObject>("StarlightPrefabHolder");
            if (_prefabHolder) return _prefabHolder!;

            _prefabHolder = new GameObject("StarlightPrefabHolder");
            _prefabHolder.SetActive(false);
            Object.DontDestroyOnLoad(_prefabHolder);
            return _prefabHolder;
        }
    }
    public static SystemContext systemContext => SystemContext.Instance;
    public static GameContext gameContext => GameContext.Instance;
    public static SceneContext sceneContext => SceneContext.Instance;
    internal static Damage _killDamage;
    public static Damage killDamage => _killDamage;
    public static AutoSaveDirector autoSaveDirector => gameContext.AutoSaveDirector;
    
    public static LookupDirector lookupDirector => gameContext.LookupDirector;
    
    public static SlimeDefinitions slimeDefinitions => gameContext.SlimeDefinitions;

    private static SlimeAppearanceDirector? _appearanceDirector;
    
    public static SlimeAppearanceDirector? appearanceDirector
    {
        get
        {
            if (_appearanceDirector != null) return _appearanceDirector;

            _appearanceDirector = Get<SlimeAppearanceDirector>("MainSlimeAppearanceDirector") ?? GetAny<SlimeAppearanceDirector>();
            return _appearanceDirector;
        }
    }
    
    public static bool IsGameReady
    {
        get
        {
            try
            {
                return gameContext != null
                    && autoSaveDirector != null
                    && autoSaveDirector._configuration != null
                    && lookupDirector != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public static void Log(string txt) => MelonLogger.Msg(txt);
    public static void Log(object obj) => MelonLogger.Msg(obj);
    public static void Log(string txt, params object[] args) => MelonLogger.Msg(txt, args);
    public static void LogError(string txt) => MelonLogger.Error(txt);
    public static void LogError(string txt,Exception ex) => MelonLogger.Error(txt,ex);
    public static void LogError(object obj) => MelonLogger.Error(obj);
    public static void LogError(string txt,params object[] args) => MelonLogger.Error(txt, args);
    public static void LogBigError(string nameSection,string txt) => MelonLogger.BigError(nameSection,txt);

    public static void LogWarning(string txt) => MelonLogger.Warning(txt);
    public static void LogWarning(object obj) => MelonLogger.Warning(obj);
    public static void LogWarning(string txt, params object[] args) => MelonLogger.Warning(txt, args);
    
    public static void StartCoroutine(IEnumerator routine) => MelonCoroutines.Start(routine);
    public static void StopCoroutine(object coroutineToken) => MelonCoroutines.Stop(coroutineToken);
    
    public static bool inGame
    {
        get
        {
            try
            {
                if (!sceneContext) return false;
                if (!sceneContext.PlayerState) return false;
            }
            catch
            { return false; }
            return true;
        }
    }
    public static bool IsBetween(this string[] list, uint min, int max)
    {
        if (list == null)
        {
            if (min > 0) return false;
        }
        else
        {
            if (list.Length < min) return false;
            if(max!=-1) if (list.Length > max) return false;
        }

        return true;
    }

    public static string mlVersion
    {
        get {
            if(StarlightEntryPoint.MelonVersion=="undefined")
            {
                try
                {
                    // This works on ML 0.7.2 and later
                    var propertiesBuildInfo = System.Type.GetType("MelonLoader.Properties.BuildInfo, MelonLoader");
                    StarlightEntryPoint.MelonVersion = (string)propertiesBuildInfo.GetProperty("Version").GetValue(null, null);
                }
                catch
                {
                    //Do this if ML changes MelonLoader.BuildInfo.Version again...
                    LogError("MelonLoader.BuildInfo.Version changed, if you are using not using the latest ML version, please update," + "otherwise this will be fixed in the next Starlight release!");
                    try
                    {
                        string logFilePath = Application.dataPath + "/../MelonLoader/Latest.log";
                        using (var logFileStream = new System.IO.FileStream(logFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                        using (var logFileReader = new System.IO.StreamReader(logFileStream))
                        {
                            string text = logFileReader.ReadToEnd();
                            var split = text.Split("\n");
                            if (string.IsNullOrWhiteSpace(split[0])) StarlightEntryPoint.MelonVersion = split[2].Split("v")[1].Split(" ")[0];
                            else StarlightEntryPoint.MelonVersion = split[1].Split("v")[1].Split(" ")[0];
                        }
                        
                    }
                    catch { StarlightEntryPoint.MelonVersion = "unknown"; }
                }

                var v = StarlightEntryPoint.MelonVersion;
                if(v=="0.6.0"||v=="0.6.1"||v=="0.6.2"||v=="0.6.3"||v=="0.6.4"||v=="0.6.5"||v=="0.6.6"||v=="0.7.0"||v=="0.7.1"||v=="0.7.2")
                    LogBigError("Starlight-WARNING","Your MelonLoader version is lower than 0.7.3! Problems will occur! Do not report issues to Starlight if you face issues! Please update the MelonLoader!");
            }
            return StarlightEntryPoint.MelonVersion;
        }
    }
}