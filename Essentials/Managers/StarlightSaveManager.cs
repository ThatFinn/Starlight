using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Starlight.Enums;
using Starlight.Storage;

namespace Starlight.Managers;

internal static class StarlightSaveManager
{
    internal static StarlightCustomInGameData inGameData;
    internal static void OnInGameLoad(StarlightCustomInGameData loadedSave, LoadingGameSessionData sessionData)
    {
        if (loadedSave == null) inGameData = new StarlightCustomInGameData();
        else inGameData = loadedSave;
        inGameData.OptionsSave = new StarlightOptionsButtonManager.CustomOptionsInGameSave();
    }

    internal static StarlightCustomInGameData OnInGameSave(SavingGameSessionData sessionData) => inGameData;
    
    
    internal static StarlightSaveData data = null;
    internal static void Start()
    {
        Load();
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Error = (sender, args) =>
        {
            if (DebugLogging.HasFlag())
                Log($"Error: {args.ErrorContext.Error.Message}");

            // Handle broken fonts
            if (args.ErrorContext.Member is string memberName && args.ErrorContext.Path.Contains(nameof(StarlightSaveData.fonts)))
                ((Dictionary<string, StarlightMenuFont>)args.ErrorContext.OriginalObject)[memberName] = StarlightMenuFont.Default;

            // Handle broken themes
            if (args.ErrorContext.Member is string memberName2 && args.ErrorContext.Path.Contains(nameof(StarlightSaveData.themes)))
                ((Dictionary<string, StarlightMenuTheme>)args.ErrorContext.OriginalObject)[memberName2] = StarlightMenuTheme.Starlight;

            // Handle broken keybinds (invalid enum key)
            if (args.ErrorContext.Path.Contains(nameof(StarlightSaveData.keyBinds)) && args.ErrorContext.Member is string brokenKey)
            {
                var dict = args.ErrorContext.OriginalObject as Dictionary<LKey, string>;
                if (dict != null)
                {
                    // Try to find the invalid entry by string match (since enum parse failed)
                    var entry = dict.FirstOrDefault(kv => kv.Key.ToString() == brokenKey);
                    if (!entry.Equals(default(KeyValuePair<LKey, string>)))
                    {
                        dict.Remove(entry.Key);
                        if (DebugLogging.HasFlag())
                            Log($"Removed broken keybind: {brokenKey}");
                    }
                }
            }

            args.ErrorContext.Handled = true;
        }
    };

    internal static void Load()
    {

        if (!File.Exists(configPath)) data = new StarlightSaveData();
        else try
            {
                data = JsonConvert.DeserializeObject<StarlightSaveData>(File.ReadAllText(configPath), JsonSerializerSettings);
            }
            catch (Exception e)
            {
                Log("Starlight save data is broken");
                LogError(e);
                data = new StarlightSaveData();
            }

        if (data.keyBinds == null) data.keyBinds = new Dictionary<LKey, string>();
        if (data.modKeys == null) data.modKeys = new Dictionary<string, LKey>();
        if (data.warps == null) data.warps = new Dictionary<string, Warp>();
        if (data.themes == null) data.themes = new Dictionary<string, StarlightMenuTheme>();
        if (data.fonts == null) data.fonts = new Dictionary<string, StarlightMenuFont>();
        foreach (var pair in data.fonts)
            if (!Enum.IsDefined(typeof(StarlightMenuFont), pair.Value))
                data.fonts[pair.Key] = StarlightMenuFont.Default;
        foreach (var pair in data.themes)
            if (!Enum.IsDefined(typeof(StarlightMenuTheme), pair.Value))
                data.themes[pair.Key] = StarlightMenuTheme.Starlight;
        foreach (var pair in new Dictionary<LKey,String>(data.keyBinds))
            if (!Enum.IsDefined(typeof(LKey), pair.Key))
                data.keyBinds.Remove(pair.Key);
        if (data.keyBinds.ContainsKey(LKey.F11))
        {
            string cmd = data.keyBinds[LKey.F11];
            if (cmd.Contains("toggleconsole") || cmd.Contains("closeconsole") || cmd.Contains("openconsole"))
                data.keyBinds[LKey.F11] = cmd.Replace("toggleconsole", "").Replace("closeconsole", "")
                    .Replace("openconsole", "");
        }

        Save();
    }

    internal static void Save() { File.WriteAllText(configPath,JsonConvert.SerializeObject(data, Formatting.Indented)); }
    
    static string configPath => Path.Combine(StarlightEntryPoint.dataPath, "starlight.data");
    
    [System.Serializable]
    public class StarlightSaveData
    {
        public Dictionary<string, Warp> warps = new ();
        public Dictionary<LKey, string> keyBinds = new ();

        public Dictionary<string, LKey> modKeys = new ();
        public Dictionary<string, StarlightMenuTheme> themes = new ();
        public Dictionary<string, StarlightMenuFont> fonts = new ();
    }
}