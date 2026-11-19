using System;
using System.IO;
using System.Linq;
using System.Xml;
using MelonLoader;
using Starlight.Enums.Features;
using Starlight.Managers;

namespace Starlight;

public static class StarlightFeatureFlags
{
    
    static readonly List<FeatureFlag> DefaultFlags =
    [
        CommandsLoadCommands, CommandsLoadCheat, CommandsLoadLandPlot, CommandsLoadBinding, CommandsLoadWarp,
        CommandsLoadCommon, CommandsLoadMenu, CommandsLoadMiscellaneous, CommandsLoadFun,
        AllowExpansions, AllowExpansionsV1, EnableModMenu, EnableConsole,
        InjectMainMenuButtons, InjectRanchUIButtons, InjectPauseButtons, InjectOptionsButtons, InjectTranslations,
        AddCheatMenuButton, AddModMenuButton, CheckForUpdates, AllowAutoUpdate, EnableInfHealth,
        EnableInfEnergy, EnableCheatMenu, EnableLocalizedVersionPatch, EnableThemeMenu,
        ChangeSystemContextIsModded, AllowPrism, AllowSaveExport, TryFixingInvalidSceneGroups, SupportRadiant
    ];

    private static FeatureFlag[] extraDevFlags =>
    [
        DevMode, Experiments, CommandsLoadDevOnly, CommandsLoadExperimental, IgnoreSaveErrors,
        ExperimentalKeyCodes, DevTestMenu, EnableStudioMenu, EnableEmberMode,
        //EnableRepoMenu, UseMockRepo, EnableRepoManagement
        //AddMockOptionsUIButtons
    ];
    private static FeatureFlag[] extraNightlyFlags => [None];
    
    private static List<FeatureFlag> _flagsToForceOff = new ();
    private static readonly Dictionary<FeatureIntegerValue, int> DefaultFeatureInts = new ()
    {
        {MAX_AUTOCOMPLETE,55},
        {MAX_CONSOLELINES,150},
        {SAVESLOT_COUNT,75},
        {MAX_AUTOCOMPLETEONSCREEN,6}
    };
    private static readonly Dictionary<FeatureStringValue, string> DefaultFeatureStrings = new ()
    {
        {DEFAULT_LANGUAGECODE,"en"}
    };
    
    private static CommandType _enabledCmDs;
    private static Dictionary<FeatureIntegerValue, int> _featureInts = new ();
    private static Dictionary<FeatureStringValue, string> _featureStrings = new ();
    private static List<FeatureFlag> _enabledFlags = new ();

    private static bool _initialized;
    static string flagPath => Path.Combine(StarlightEntryPoint.flagDataPath,StarlightEntryPoint.UpdateBranch+".flags");
    static void SaveToFlagFile()
    {
        var xmlDoc = new XmlDocument();
        
        var root = xmlDoc.CreateElement("StarlightFeatureFlags");
        xmlDoc.AppendChild(root);
        var flags = xmlDoc.CreateElement("FeatureFlags");
        root.AppendChild(flags);
        var ints = xmlDoc.CreateElement("FeatureIntegerValues");
        root.AppendChild(ints);
        var strings = xmlDoc.CreateElement("FeatureStringValue");
        root.AppendChild(strings);
        foreach (FeatureFlag flag in Enum.GetValues(typeof(FeatureFlag)))
        {
            //bools
            if (flag == None) continue;
            var xmlElement = xmlDoc.CreateElement(flag.ToString());
            flags.AppendChild(xmlElement);
            if(!_flagsToForceOff.HasFlag(flag))
            {
                xmlElement.SetAttribute("value", flag.HasFlag().ToString().ToLower());
                xmlElement.SetAttribute("default", flag.GetDefault().ToString().ToLower());
            }
            else
            {
                xmlElement.SetAttribute("value", "false");
                xmlElement.SetAttribute("default", flag.GetDefault().ToString().ToLower());
            }
            
            if (RequirementsMap.ContainsKey(flag)) if(RequirementsMap[flag].Length!=0)
                foreach (var req in RequirementsMap[flag])
                {
                    if (req == null) continue;
                    string name = req.GetType().Name;
                    if (req is FFRString ffrString)
                    {
                        string exist = xmlElement.GetAttribute(name);
                        if (!string.IsNullOrEmpty(exist)) exist += ",";
                        xmlElement.SetAttribute(name, exist+ffrString.String);
                    }
                    else if (req is FFRFlag ffrFlag)
                    {
                        string exist = xmlElement.GetAttribute(name);
                        if (!string.IsNullOrEmpty(exist)) exist += ",";
                        xmlElement.SetAttribute(name, exist+ffrFlag.Flag);
                    }
                }
        }
        foreach (FeatureIntegerValue value in Enum.GetValues(typeof(FeatureIntegerValue)))
        {
            //ints
            var xmlElement = xmlDoc.CreateElement(value.ToString());
            ints.AppendChild(xmlElement);
            xmlElement.SetAttribute("value",value.Get().ToString().ToLower());
            xmlElement.SetAttribute("default", value.GetDefault().ToString().ToLower());
        }
        foreach (FeatureStringValue value in Enum.GetValues(typeof(FeatureStringValue)))
        {
            //strings
            var xmlElement = xmlDoc.CreateElement(value.ToString());
            strings.AppendChild(xmlElement);
            xmlElement.SetAttribute("value",value.Get().ToLower());
            xmlElement.SetAttribute("default", value.GetDefault().ToLower());
        }

        if (true) //Save Version
        {
            var xmlElement = xmlDoc.CreateElement("LAST_StarlightVERSION");
            strings.AppendChild(xmlElement);
            xmlElement.SetAttribute("value",BuildInfo.DisplayVersion);
            xmlElement.SetAttribute("default", BuildInfo.DisplayVersion);
            xmlElement.SetAttribute("DO_NOT_EDIT", "please");
        }
        // Save the XML document to a file
        
        if (File.Exists(flagPath)) File.SetAttributes(flagPath, FileAttributes.Normal);
        xmlDoc.Save(flagPath);
        File.SetAttributes(flagPath, FileAttributes.Hidden);
    }

    static void LoadFromFlagFile()
    {
        _flagsToForceOff = new List<FeatureFlag>();
        var xmlDoc = new XmlDocument();
        
        if (!File.Exists(flagPath)) return;

        try { xmlDoc.Load(flagPath); } catch { }

        var root = xmlDoc["StarlightFeatureFlags"];
        if (root == null) return;
        
        var strings = root["FeatureStringValue"];
        if (strings != null)
        {
            if(strings["LAST_StarlightVERSION"]!=null)
            {
                if (strings["LAST_StarlightVERSION"].GetAttribute("value") != BuildInfo.DisplayVersion) return;
            }
            foreach (XmlElement stringElement in strings.ChildNodes)
                if(stringElement.Name!="FeatureStringValue")
                    if (Enum.TryParse(stringElement.Name, out FeatureStringValue stringValue))
                        if (stringValue.GetDefault() == stringElement.GetAttribute("default"))
                            _featureStrings[stringValue] = stringElement.GetAttribute("value");
        }

        var flags = root["FeatureFlags"];
        if (flags != null)
            foreach (XmlElement flagElement in flags.ChildNodes)
                if (Enum.TryParse(flagElement.Name, out FeatureFlag flag))
                    if (flag.GetDefault().ToString().ToLower() == flagElement.GetAttribute("default"))
                        if (bool.TryParse(flagElement.GetAttribute("value"), out bool isEnabled))
                            flag.SetFlag(isEnabled);
        
        var ints = root["FeatureIntegerValues"];
        if (ints != null)
            foreach (XmlElement intElement in ints.ChildNodes)
                if (Enum.TryParse(intElement.Name, out FeatureIntegerValue intValue))
                    if (intValue.GetDefault().ToString() == intElement.GetAttribute("default"))
                        if (int.TryParse(intElement.GetAttribute("value"), out int intResult))
                            _featureInts[intValue] = intResult;

        foreach (FeatureFlag flag in Enum.GetValues(typeof(FeatureFlag)))
        {
            if(flag.RequirementsMet()) continue;
            flag.DisableFlag();
            if(!_flagsToForceOff.Contains(flag));
            _flagsToForceOff.Add(flag);
        }
    }

    internal static void InitFlagManager()
    {
        if (_initialized) return;
        _initialized = true;
        _enabledFlags = new List<FeatureFlag>();
        _flagsToForceOff = new List<FeatureFlag>();
        FeatureFlag[] addedFlags = null;
        switch (StarlightEntryPoint.UpdateBranch)
        {
            case "dev": addedFlags = extraDevFlags; break;
            case "nightly": addedFlags = extraNightlyFlags; break;
        }
        if(addedFlags!=null)
            foreach (var flag in addedFlags)
                DefaultFlags.Add(flag);
        foreach (var flag in DefaultFlags)
            flag.EnableFlag();
        _featureInts = new Dictionary<FeatureIntegerValue, int>(DefaultFeatureInts);
        _featureStrings = new Dictionary<FeatureStringValue, string>(DefaultFeatureStrings);
        try
        {
            if (File.Exists(flagPath)) LoadFromFlagFile();
            SaveToFlagFile();
        } catch { }
        
        var launchArgs = Environment.GetCommandLineArgs();
        var usedArgs = new List<string>();
        foreach (var arg in launchArgs)
        {
            if (arg.StartsWith("-starlight.") && arg.Contains("="))
            {
                var split = arg.Split('=');
                if (split.Length != 2) continue;

                var argKey = split[0];
                var argValue = split[1];

                if (usedArgs.Contains(argKey)) continue;
                usedArgs.Add(argKey);
                var flagName = argKey.Replace("-starlight.", "");

                if (Enum.TryParse<FeatureFlag>(flagName, ignoreCase: true, out var matchedFlag))
                {
                    if (argValue == "true")
                        matchedFlag.EnableFlag();
                    else if (argValue == "false")
                        matchedFlag.DisableFlag();
                    
                }
            }
        }

        var changedAFlag = true;
        while (changedAFlag)
        {
            changedAFlag = false;
            foreach (FeatureFlag flag in Enum.GetValues(typeof(FeatureFlag)))
            {
                if(flag.RequirementsMet()) continue;
                flag.DisableFlag();
                if(!_flagsToForceOff.Contains(flag))
                {
                    _flagsToForceOff.Add(flag);
                    changedAFlag = true;
                }
            }

        }
        if (CommandsLoadDevOnly.HasFlag()) _enabledCmDs |= CommandType.DevOnly;
        if (CommandsLoadExperimental.HasFlag()) _enabledCmDs |= CommandType.Experimental;
        if (CommandsLoadCheat.HasFlag()) _enabledCmDs |= CommandType.Cheat;
        if (CommandsLoadLandPlot.HasFlag()) _enabledCmDs |= CommandType.LandPlot;
        if (CommandsLoadBinding.HasFlag()) _enabledCmDs |= CommandType.Binding;
        if (CommandsLoadWarp.HasFlag()) _enabledCmDs |= CommandType.Warp;
        if (CommandsLoadCommon.HasFlag()) _enabledCmDs |= CommandType.Common;
        if (CommandsLoadMenu.HasFlag()) _enabledCmDs |= CommandType.Menu;
        if (CommandsLoadMiscellaneous.HasFlag()) _enabledCmDs |= CommandType.Miscellaneous;
        if(CommandsLoadFun.HasFlag()) _enabledCmDs |= CommandType.Fun;
        
        if(DisableCheats.HasFlag())
            StarlightEntryPoint.Instance.RegisterFor_DisableCheats();
        
    }
    public static CommandType enabledCommands => _enabledCmDs;
    public static bool HasFlag(this FeatureFlag featureFlag) => _enabledFlags.Contains(featureFlag);
    public static bool HasFlag(this bool[] array,FeatureFlag featureFlag) => array[Convert.ToInt32(featureFlag)];
    public static bool HasFlag(this FeatureFlag[] array,FeatureFlag featureFlag) => array.Contains(featureFlag);
    public static bool HasFlag(this List<FeatureFlag> list,FeatureFlag featureFlag) => list.Contains(featureFlag);

    private static void SetFlag(this FeatureFlag featureFlag, bool state)
    {
        if (state) EnableFlag(featureFlag);
        else DisableFlag(featureFlag);
    }

    private static bool EnableFlag(this FeatureFlag featureFlag)
    {
        if (_enabledFlags.Contains(featureFlag)) return false;
        _enabledFlags.Add(featureFlag);
        return true;

    }
    private static bool DisableFlag(this FeatureFlag featureFlag) => _enabledFlags.Remove(featureFlag);

    public static int Get(this FeatureIntegerValue featureIntegerValue)
    {
        return _featureInts.GetValueOrDefault(featureIntegerValue, 0);
    }
    public static string Get(this FeatureStringValue featureStringValue)
    {
        return _featureStrings.GetValueOrDefault(featureStringValue, "");
    }
    public static int GetDefault(this FeatureIntegerValue featureIntegerValue)
    {
        return DefaultFeatureInts.GetValueOrDefault(featureIntegerValue, 0);
    }
    public static string GetDefault(this FeatureStringValue featureStringValue)
    {
        return DefaultFeatureStrings.GetValueOrDefault(featureStringValue, "");
    }

    public static bool GetDefault(this FeatureFlag featureFlag) => DefaultFlags.HasFlag(featureFlag);
    
    static readonly Dictionary<FeatureFlag,FFR[]> RequirementsMap = new ()
    {
        {CheckForUpdates, [new FFRDeactivated(DevMode)] },
        {AllowAutoUpdate, [new FFRDeactivated(DevMode)] },
        {DevTestMenu, [new FFRActivated(DevMode)] },
        {EnableConsole, [new FFRMelonUnInstalled("mSRML")] },
        {EnableInfHealth, [new FFRMelonUnInstalled("InfiniteHealth")] },
        {EnableInfEnergy, [new FFRMelonUnInstalled("InfiniteEnergy")] },
        {CommandsLoadExperimental, [new FFRActivated(CommandsLoadCommands), new FFRActivated(Experiments)] },
        {CommandsLoadDevOnly, [new FFRActivated(CommandsLoadCommands), new FFRActivated(DevMode)] },
        {CommandsLoadCheat, [new FFRActivated(CommandsLoadCommands),new FFRDeactivated(DisableCheats)] },
        {CommandsLoadBinding, [new FFRActivated(CommandsLoadCommands)] },
        {CommandsLoadLandPlot, [new FFRActivated(CommandsLoadCommands)] },
        {CommandsLoadWarp, [new FFRActivated(CommandsLoadCommands)] },
        {CommandsLoadCommon, [new FFRActivated(CommandsLoadCommands)] },
        {CommandsLoadMenu, [new FFRActivated(CommandsLoadCommands)] },
        {CommandsLoadMiscellaneous, [new FFRActivated(CommandsLoadCommands)] },
        {CommandsLoadFun, [new FFRActivated(CommandsLoadCommands)] },
        {EnableCheatMenu, [new FFRDeactivated(DisableCheats)] },
        {AddCheatMenuButton, [new FFRActivated(EnableCheatMenu), new FFRActivated(InjectPauseButtons)] },
        {AddModMenuButton, [new FFRActivated(InjectMainMenuButtons), new FFRActivated(InjectPauseButtons)] },
        {AllowPrism, [new FFRActivated(InjectTranslations)] },
        {AllowExpansionsV1, [new FFRActivated(AllowExpansions)] },
        {AddMockMainMenuButtons, [new FFRActivated(InjectMainMenuButtons)] },
        {RestoreDebugFPSViewer, [new FFRActivated(RestoreDebugAbilities)] },
        {RestoreDebugPlayerDebug, [new FFRActivated(RestoreDebugAbilities)] },
        {RestoreDebugDevConsole, [new FFRActivated(RestoreDebugAbilities)] },
        {RestoreDebugDebugUI, [new FFRActivated(RestoreDebugAbilities)] },
        {IgnoreWorldPopulatorErrors, [new FFRActivated(ShowWorldPopulatorErrors)] },
        {UseMockRepo, [new FFRActivated(EnableRepoManagement)] },
        {EnableRepoMenu, [new FFRActivated(EnableRepoManagement)] },
        {EnableStudioMenu, [new FFRActivated(DevMode)] },
        {SupportRadiant, [new FFRIl2CppTypeExists("MonomiPark.SlimeRancher.Slime.SlimeRadiant")] },
        
    };

    private static bool RequirementsMet(this FeatureFlag featureFlag)
    {
        if (!RequirementsMap.TryGetValue(featureFlag, out var value)) return true;
        if(value==null) return true;
        if(RequirementsMap[featureFlag].Length==0) return true;
        foreach (var req in RequirementsMap[featureFlag])
        {
            if(req==null) continue;
            if (req is FFRActivated activated)
            {
                if (!activated.Flag.HasFlag()) return false;
            }
            else if (req is FFRDeactivated deactivated)
            {
                if (deactivated.Flag.HasFlag()) return false;
            }
            else if (req is FFRMelonInstalled melonInstalled)
            {
                bool installed = false;
                foreach (var melonBase in MelonBase.RegisteredMelons)
                {
                    if (melonBase.Info.Name != melonInstalled.String) continue;
                    installed=true; break;
                }
                if(!installed) return false;
            }
            else if (req is FFRMelonUnInstalled melonUninstalled)
            {
                foreach (var melonBase in MelonBase.RegisteredMelons)
                    if(melonBase.Info.Name==melonUninstalled.String) return false;
            }
            else if (req is FFRIl2CppTypeExists il2CppTypeExists)
            {
                var type = VersionedEUtil.FindType(il2CppTypeExists.String);
                return type != null;
            }
        }
        return true;
    }
    
    public static List<FeatureFlag> featureFlags => _enabledFlags.ToArray().ToNetList();
}

// ReSharper disable once InconsistentNaming
internal class FFR //FeatureFlagRequirement
{ }
// ReSharper disable once InconsistentNaming
internal class FFRString : FFR //FeatureFlagRequirementString
{
    public string String;
}
// ReSharper disable once InconsistentNaming
internal class FFRFlag : FFR  //FeatureFlagRequirementFlag
{
    public FeatureFlag Flag;
}
// ReSharper disable once InconsistentNaming
internal class FFRDeactivated : FFRFlag //FeatureFlagRequirementDeactivated
{
    public FFRDeactivated(FeatureFlag flag)
    { this.Flag = flag; }
}
// ReSharper disable once InconsistentNaming
internal class FFRActivated : FFRFlag //FeatureFlagRequirementActivated
{
    public FFRActivated(FeatureFlag flag)
    { this.Flag = flag; }
}
// ReSharper disable once InconsistentNaming
internal class FFRMelonInstalled : FFRString //FeatureFlagRequirementMelonInstalled
{
    public FFRMelonInstalled(string melonName)
    { this.String = melonName; }
}
// ReSharper disable once InconsistentNaming
internal class FFRMelonUnInstalled : FFRString //FeatureFlagRequirementMelonInstalled
{
    public FFRMelonUnInstalled(string melonName)
    { this.String = melonName; }
}
// ReSharper disable once InconsistentNaming
internal class FFRIl2CppTypeExists : FFRString //FeatureFlagRequirementIl2CppTypeExists
{
    public FFRIl2CppTypeExists(string fullName)
    { this.String = fullName; }
}