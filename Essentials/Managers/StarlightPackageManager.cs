using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using MelonLoader;
using MelonLoader.Utils;
using Starlight.Enums;
using Starlight.Expansion;
using Starlight.Storage.Prefs;
using Starlight.Storage;

namespace Starlight.Managers;

public static class StarlightPackageManager
{
    private static readonly Dictionary<MelonBase, StarlightPackageInfo> MelonInfos = new();
    private static readonly HashSet<string> LoadedExpansionIds = new();
    private static readonly HashSet<string> AllDiscoveredIds = new();
    public static bool IsLocked() => StarlightCounterGateManager.packagesLocked;

    private class PendingExpansion
    {
        public Type type { get; init; }
        public Assembly assembly { get; init; }
        public StarlightPackageInfo info { get; init; }
        public HarmonyLib.Harmony harmony { get; init; }
        public StarlightExpansionVXX instance { get; init; }
    }

    public static StarlightPackageInfo? GetPackageInfoFromMelon(this MelonBase melonBase)
    {
        if (MelonInfos.TryGetValue(melonBase, out var melon)) return melon;
        var assembly = melonBase.MelonAssembly.Assembly;
        if (StarlightEntryPoint.Expansions.Keys.Any(a => a == assembly))
            return null;
        var info = new StarlightPackageInfo()
        {
            ID = "melon." + TruncateForID(melonBase.Info.Author) + "." + TruncateForID(melonBase.Info.Name),
            Name = melonBase.Info.Name,
            Author = melonBase.Info.Author,
            Version = melonBase.Info.Version,
            DLLName = new FileInfo(assembly.Location).Name,
            MainClass = melonBase,
            Type = PackageType.MelonMod
        };
        if (melonBase is MelonPlugin)
            info.Type = PackageType.MelonPlugin;
        var desc = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
        if (desc != null)
            info.Description = desc.Description;
        foreach (var meta in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (meta == null || string.IsNullOrWhiteSpace(meta.Key) || string.IsNullOrWhiteSpace(meta.Value)) continue;
            switch (meta.Key)
            {
                case StarlightModInfoAttributes.SourceCode: info.SourceCode = meta.Value; break;
                case StarlightModInfoAttributes.Nexus: info.Nexus = meta.Value; break;
                case StarlightModInfoAttributes.Discord: info.Discord = meta.Value; break;
                case StarlightModInfoAttributes.DisplayVersion: info.Version = meta.Value; break;
                case StarlightModInfoAttributes.CoAuthors: info.CoAuthors = meta.Value.Split(", "); break;
                case StarlightModInfoAttributes.Contributors: info.Contributors = meta.Value.Split(", "); break;
                case StarlightModInfoAttributes.IconB64:
                    try { 
                        info.Icon = ConvertEUtil.Base64ToTexture2D(meta.Value).Texture2DToSprite();
                    } catch { }
                    break;
            }
        }

        if (!info.Icon)
        {
            try { 
                info.Icon = EmbeddedResourceEUtil.LoadSprite("icon.png", assembly).CopyWithoutMipmaps();
            } catch { }
            if (!info.Icon)
            {
                try { 
                    info.Icon = EmbeddedResourceEUtil.LoadSprite("Assets.icon.png", assembly).CopyWithoutMipmaps(); 
                } catch { }
            }
        }
        MelonInfos.Add(melonBase, info);
        return info;
    }

    public static StarlightPackageInfo? GetPackageInfoFromExpansion(this StarlightExpansionVXX expansion)
    {
        return StarlightEntryPoint.Expansions.Values
            .SelectMany(pair => pair.Item1)
            .FirstOrDefault(pair2 => pair2.Key == expansion).Value;
    }

    public static List<Assembly> GetAllPackageAssemblies()
    {
        var assemblies = StarlightEntryPoint.Expansions.Keys.ToList();
        assemblies.AddRange(MelonBase.RegisteredMelons.Select(m => m.MelonAssembly.Assembly).Where(a => !assemblies.Contains(a)));
        return assemblies;
    }

    public static Dictionary<StarlightPackageInfo, List<object>> GetAllRottenInfos()
    {
        var list = new Dictionary<StarlightPackageInfo, List<object>>();
        foreach (var (name, assembly, message, errorMessage) in StarlightEntryPoint.BrokenExpansions)
        {
            try
            {
                list.Add(new StarlightPackageInfo { Type = PackageType.Expansion, Name = name, RunningAssembly = assembly, Version = "<unknown>", DLLName = name },
                    [assembly.Location, message, errorMessage]);
            }
            catch (Exception e) { LogError(e); }
        }

        foreach (var loadedAssembly in MelonAssembly.LoadedAssemblies) foreach (dynamic rotten in loadedAssembly.RottenMelons)
        {
            try
            {
                Assembly assembly = null; string exception = null; string errorMessage = null;
                try
                {
                    assembly = rotten.assembly;
                    exception = rotten.exception?.ToString();
                    errorMessage = rotten.errorMessage;
                } catch { try {
                        assembly = rotten.Assembly.assembly;
                        exception = rotten.exception?.ToString();
                        errorMessage = rotten.errorMessage;
                    } catch { }
                }
                if (assembly == null) break;
                list.Add(
                    new StarlightPackageInfo()
                    {
                        Type = PackageType.Expansion, Name = new FileInfo(assembly.Location).Name, RunningAssembly = assembly,
                        Version = "<unknown>", DLLName = new FileInfo(assembly.Location).Name
                    },
                    [assembly.Location, exception, errorMessage]);
            } catch (Exception e) { LogError(e); }
        }

        return list;
    }

    private static readonly List<StarlightPackageInfo> _registered = new();

    /// <summary>
    /// Registers a new package to the mods menu
    /// </summary>
    public static void RegisterPackage(StarlightPackageInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.ID)) return;

        UnregisterPackage(info.ID);
        _registered.Add(info);
    }

    /// <summary>
    /// Removes a registered entry from the mods menu
    /// </summary>
    /// <returns>
    /// True when one was removed.
    /// </returns>
    public static bool UnregisterPackage(string id)
        => id is not null && _registered.RemoveAll(info => info.ID == id) > 0;

    /// <summary>
    /// All entries registered with <see cref="RegisterPackage"/>
    /// </summary>
    public static List<StarlightPackageInfo> GetAllRegisteredInfos() => new(_registered);

    public static List<StarlightPackageInfo> GetAllInfos()
    {
        var list = GetAllExpansionInfos();
        list.AddRange(GetAllMelonInfos());
        list.AddRange(_registered);
        return list;
    }
    public static List<StarlightPackageInfo> GetAllMelonInfos() => MelonBase.RegisteredMelons.Select(GetPackageInfoFromMelon).Where(info => info.HasValue).Select(info => info.Value).ToList();
    public static List<StarlightPackageInfo> GetAllExpansionInfos() => StarlightEntryPoint.Expansions.Values.SelectMany(pair => pair.Item1.Values).ToList();
    public static List<StarlightExpansionVXX> GetAllExpansions() => StarlightEntryPoint.Expansions.Values.SelectMany(pair => pair.Item1.Keys).ToList();
    public static List<MelonBase> GetAllMelons() => MelonBase.RegisteredMelons.Where(m => !StarlightEntryPoint.Expansions.ContainsKey(m.MelonAssembly.Assembly)).ToList();

    public static StarlightPackageInfo? GetPackageInfoFromID(string id)
    {
        var found = id.StartsWith("melon.") ? GetAllMelonInfos().FirstOrDefault(info => info.ID == id) : GetAllExpansionInfos().FirstOrDefault(info => info.ID == id);
        
        return found.ID is not null ? found : _registered.FirstOrDefault(info => info.ID == id);
    }

    public static MelonBase GetMelonFromID(string id) => GetPackageInfoFromID(id)?.MainClass as MelonBase;
    public static StarlightExpansionVXX GetExpansionFromID(string id) => GetPackageInfoFromID(id)?.MainClass as StarlightExpansionVXX;
    public static StarlightExpansionV01 GetExpansionV1FromID(string id) => GetPackageInfoFromID(id)?.MainClass as StarlightExpansionV01;
    
    public static void LoadExpansions(string dllPath)
    {
        if (IsLocked())
        {
            LogError("Package manager is locked, cannot load expansions.");
            return;
        }
        DiscoverAndLoadExpansions([dllPath]);
    }

    internal static void LoadAllExpansions()
    {
        if (!AllowExpansions.HasFlag()) return;
        Log("Start Loading Expansions");
        DiscoverAndLoadExpansions(Directory.GetFiles(MelonEnvironment.ModsDirectory, "*.dll").ToList());
        Log("");
        Log("");
        Log("Continue loading mods...");
    }
    
    public static void UnloadExpansion(string id)
    {
        if (IsLocked())
        {
            LogError("Package manager is locked, cannot unload expansions.");
            return;
        }

        var info = GetPackageInfoFromID(id);
        if (info is not { Type: PackageType.Expansion })
        {
            LogError($"Expansion with ID '{id}' not found.");
            return;
        }

        var infoValue = info.Value;
        
        var isDependedOn = GetAllExpansionInfos().Any(exp => (exp.Dependencies ?? Array.Empty<string>()).Contains(id));
        if (isDependedOn)
        {
            LogError($"Cannot unload expansion '{id}' because another expansion depends on it.");
            return;
        }

        switch (infoValue.UnloadTime)
        {
            case ExpansionUnloadTime.Never:
                LogError($"Expansion '{id}' is marked as not unloadable.");
                return;
            case ExpansionUnloadTime.InMainMenu when SceneContext.Instance != null && SceneContext.Instance.GameModel != null:
                LogError($"Expansion '{id}' can only be unloaded in the main menu.");
                return;
        }

        var expansion = infoValue.MainClass as StarlightExpansionVXX;
        if (expansion is StarlightExpansionV01 v01)
        {
            try { v01.OnUnload(); }
            catch (Exception e) { LogError($"Error during OnUnload for expansion '{id}': {e}"); }
        }
        expansion._isLoaded = false;

        var assembly = infoValue.RunningAssembly;
        if (StarlightEntryPoint.Expansions.TryGetValue(assembly, out var assemblyExpansions))
        {
            if (expansion != null) assemblyExpansions.Item1.Remove(expansion);
            assemblyExpansions.Item2.UnpatchSelf();
            if (assemblyExpansions.Item1.Count == 0)
            {
                StarlightEntryPoint.Expansions.Remove(assembly);
            }
        }

        LoadedExpansionIds.Remove(id);
        AllDiscoveredIds.Remove(id);
        if(DebugLogging.HasFlag()) Log($"Unloaded expansion: {infoValue.Name} ({id})");
    }
    static bool IsSameOrNewer(string v1, string v2)
    {
        if (string.IsNullOrEmpty(v1)) return true;
        bool TryParse(string s, out int[] parts)
        {
            parts = null;
            var split = s.Split('.');
            if (split.Length != 3) return false;
            parts = new int[3];
            for (int i = 0; i < 3; i++)
                if (!int.TryParse(split[i], out parts[i]) || parts[i] < 0)
                    return false;
            return true;
        }

        if (!TryParse(v1, out var a) || !TryParse(v2, out var b)) return false;
        for (int i = 0; i < 3; i++)
        {
            if (b[i] > a[i]) return true;
            if (b[i] < a[i]) return false;
        }

        return true;
    }
    private static void DiscoverAndLoadExpansions(List<string> dllPaths)
    {
        if (!AllowExpansions.HasFlag()) return;

        Log("Loading Expansions from DLLs...");
        Log("------------------------------");
        var pendingExpansions = new List<PendingExpansion>();
        var baseType = typeof(StarlightExpansionVXX);

        foreach (var dllPath in dllPaths)
        {
            try
            {
                if (!ContainsExpansions(dllPath)) continue;
                var assembly = Assembly.LoadFrom(dllPath);
                
                if (assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == StarlightModInfoAttributes.MinimumStarlightVersion) is { } minVersionAttr)
                {
                    if(!IsSameOrNewer(minVersionAttr.Value,BuildInfo.CodeVersion))
                    {
                        StarlightEntryPoint.BrokenExpansions.Add((new FileInfo(assembly.Location).Name, assembly, $"You need a newer version of Starlight installed! A minimum of <b>Starlight {minVersionAttr.Value}</b> is required!", "Starlight too old!"));
                        continue;
                    }
                }

                var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t) && t.GetCustomAttributes(typeof(StarlightLoadExpansionAttribute), false).Any()).ToList();
                if (types.Count == 0) continue;
                Log("Assembly: "+dllPath);
                Log("Total Expansion Count: "+types.Count);

                var hInstance = new HarmonyLib.Harmony(dllPath);
                if(StarlightEntryPoint.AlreadyInitialized)
                    StarlightEntryPoint.PatchGame(hInstance,assembly);
                foreach (var type in types)
                {
                    var message = "";
                    string errorMessage = null;
                    var success = true;
                    
                    try
                    {
                        var instance = (StarlightExpansionVXX)Activator.CreateInstance(type);
                        var info = instance.StarlightInternal_GetInfo;
            
                        var gameContextStarted = StarlightEntryPoint.GameContextStarted;

                        switch (info.LoadTime)
                        {
                            case ExpansionLoadTime.Startup when gameContextStarted:
                                message += "\nCannot be loaded after game startup.";
                                break;
                            case ExpansionLoadTime.BeforeGameContext when gameContextStarted:
                                message += "\nCannot be loaded after GameContext has started.";
                                break;
                            case ExpansionLoadTime.InMainMenu when inGame:
                                message += "\nCan only be loaded in the main menu.";
                                break;
                        }

                        info.RunningAssembly = assembly;
                        info.DLLName = new FileInfo(assembly.Location).Name;
                        info.Type = PackageType.Expansion;
                        info.MainClass = instance;
                        if (MelonBase.RegisteredMelons.Any(a => a.MelonAssembly.Assembly == assembly))
                            info.HasOptionFile = true;

                        if (string.IsNullOrWhiteSpace(info.ID) || info.ID.StartsWith("melon.") || info.ID.Count(c => c == '.') != 2 || !Regex.IsMatch(info.ID, @"^[a-z0-9\.]+$"))
                            message += "\nThe expansion's ID is invalid. It must be in the format 'author.packagename.expansionname', using only lowercase letters, numbers, and two dots.";
                        else if (!AllDiscoveredIds.Add(info.ID))
                            message += $"\nAn expansion with the ID '{info.ID}' already exists. Expansion IDs must be unique.";
                        if (string.IsNullOrWhiteSpace(info.Name)) message += "\nThe expansion's name cannot be empty.";
                        if (instance is StarlightExpansionV01)
                        {
                            if (!AllowExpansionsV1.HasFlag()) message += "\nExpansionV1s are disabled!";
                            info.ExpansionVersion = 1;
                        }
                        else message += "\nInvalid or unsupported expansion version!";

                        if (!string.IsNullOrWhiteSpace(message)) success = false;
                        
                        if (success)
                        {
                            
                            StarlightEntryPoint.Instance.InjectIl2CppComponents(assembly);
                            pendingExpansions.Add(new PendingExpansion
                            {
                                type = type, assembly = assembly, info = info, harmony = hInstance, instance = instance
                            });
                        }
                        else
                        {
                            LogError($"Expansion '{type.FullName}' in {info.DLLName} was not loaded: {message}");
                            StarlightEntryPoint.BrokenExpansions.Add((type.FullName, assembly, message, errorMessage));
                        }
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                        StarlightEntryPoint.BrokenExpansions.Add((type.FullName, assembly, $"An unexpected error occurred while validating the expansion: {e.Message}", e.ToString()));
                    }
                }
            }
            catch (Exception e) { LogError($"Failed to process DLL: {dllPath}\n{e}"); }
        }

        Log("------------------------------");
        Log("");
        var allMelonIds = new Lazy<HashSet<string>>(() => [..GetAllMelonInfos().Select(i => i.ID)]);
        int loadedInPass;
        var dependencyGraph = pendingExpansions.ToDictionary(p => p.info.ID, p => new HashSet<string>(p.info.Dependencies ?? Array.Empty<string>()));
        var failedDueToCycle = new HashSet<string>();

        do
        {
            var toProcess = dependencyGraph.Keys.Except(failedDueToCycle).ToList();
            foreach (var nodeId in toProcess)
            {
                var path = new List<string> { nodeId };
                var toVisit = new Stack<string>(dependencyGraph[nodeId]);
                while (toVisit.Count > 0)
                {
                    var current = toVisit.Pop();
                    if (path.Contains(current))
                    {
                        path.Add(current);
                        var cycle = string.Join(" -> ", path);
                        LogError($"Cross dependency detected: {cycle}");
                        foreach (var id in path)
                            if (failedDueToCycle.Add(id))
                            {
                                var pending = pendingExpansions.FirstOrDefault(p => p.info.ID == id);
                                if (pending != null)
                                {
                                    StarlightEntryPoint.BrokenExpansions.Add((pending.type.FullName, pending.assembly, $"Circular dependency detected: {cycle}", "Dependency Error"));
                                    pendingExpansions.Remove(pending);
                                }
                            }
                        break;
                    }
                    if (dependencyGraph.TryGetValue(current, out var value))
                    {
                        path.Add(current);
                        foreach (var dep in value) toVisit.Push(dep);
                    }
                }
            }
            
            loadedInPass = 0;
            var remainingPending = new List<PendingExpansion>();
            foreach (var pending in pendingExpansions)
            {
                if (failedDueToCycle.Contains(pending.info.ID)) continue;

                var dependenciesMet = (pending.info.Dependencies ?? Array.Empty<string>()).All(depId =>
                    LoadedExpansionIds.Contains(depId) || (depId.StartsWith("melon.") && allMelonIds.Value.Contains(depId)));

                if (dependenciesMet)
                {
                    try
                    {
                        var instance = pending.instance;
                        var info = pending.info;
                        try
                        {
                            info.Icon = EmbeddedResourceEUtil.LoadSprite(info.IconPath, pending.assembly).CopyWithoutMipmaps();
                        } catch { }
                        
                        if (AllowPrism.HasFlag() && info.UsePrism) StarlightEntryPoint.ShouldEnablePrism = true;
                        instance._isLoaded = true;
                        if (instance is StarlightExpansionV01 v01) StarlightEntryPoint.ExpansionV01S.Add(v01);
                        baseType.GetField("_assembly", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, pending.assembly);
                        baseType.GetField("_harmonyInstance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, pending.harmony);
                        if (!StarlightEntryPoint.Expansions.TryGetValue(pending.assembly, out var assemblyExpansions))
                        {
                            assemblyExpansions = (new Dictionary<StarlightExpansionVXX, StarlightPackageInfo>(), pending.harmony);
                            StarlightEntryPoint.Expansions.Add(pending.assembly, assemblyExpansions);
                        }
                        assemblyExpansions.Item1.Add(instance, info);
                        LoadedExpansionIds.Add(info.ID);
                        loadedInPass++;
                        
                        Log("-------Expansion-Loaded-------");
                        Log(info.Name+ " v"+ info.Version);
                        if(!string.IsNullOrEmpty(info.Author)) Log("by " + info.Author);
                        Log("Prism, VXX: " + info.UsePrism+", "+info.ExpansionVersion);
                        Log("------------------------------");
                        
                        if (instance is StarlightExpansionV01 vv01)
                        {
                            try { if (!Directory.Exists(vv01.modDataPath)) Directory.CreateDirectory(vv01.modDataPath); }catch (Exception e) { LogError(e); }

                            vv01._prefs = new PackagePrefs(info.ID,Path.Combine(vv01.modDataPath,"prefs.json"));
                            try { vv01.OnCreatePrefs(); }catch (Exception e) { LogError(e); }
                            try { vv01.OnEarlyInitialize(); }catch (Exception e) { LogError(e); }
                        }
                        if(DebugLogging.HasFlag()) Log($"Loaded expansion: {info.Name} ({info.ID}) version {info.Version}");
                        if(StarlightEntryPoint.AlreadyInitialized)
                        {
                            foreach (var pair in assemblyExpansions.Item1)
                                try { if (pair.Key is StarlightExpansionV01 v1) v1.OnInitialize(); }
                                catch (Exception e) { LogError(e); }
                        }

                        if (StarlightEntryPoint.AlreadyLateInitialized)
                        {
                            foreach (var pair in assemblyExpansions.Item1)
                                try { if (pair.Key is StarlightExpansionV01 v1) v1.OnLateInitialize(); }
                                catch (Exception e) { LogError(e); }
                        }
                        
                    }
                    catch (Exception e)
                    {
                        LogError($"Failed to load expansion '{pending.info.Name} ({pending.info.ID})': {e}");
                        StarlightEntryPoint.BrokenExpansions.Add((pending.type.FullName, pending.assembly, "An exception occurred during loading.", e.ToString()));
                    }
                }
                else remainingPending.Add(pending);
            }
            pendingExpansions = remainingPending;
        } while (loadedInPass > 0 && pendingExpansions.Count > 0);

        foreach (var pending in pendingExpansions.Where(p => !failedDueToCycle.Contains(p.info.ID)))
        {
            var missingDeps = (pending.info.Dependencies ?? Array.Empty<string>()).Where(depId => !LoadedExpansionIds.Contains(depId) && !(depId.StartsWith("melon.") && allMelonIds.Value.Contains(depId)));
            var message = $"Could not load expansion due to missing dependencies: {string.Join(", ", missingDeps)}";
            LogError(message);
            StarlightEntryPoint.BrokenExpansions.Add((pending.type.FullName, pending.assembly, message, "Missing Dependencies"));
        }
    }

    public static bool ContainsExpansions(string dllPath)
    {
        var context = new AssemblyLoadContext("StarlightUnloadableContext", isCollectible: true);
        try
        {
            var assembly = context.LoadFromAssemblyPath(dllPath);
            return assembly.GetTypes().Any(t => Attribute.IsDefined(t, typeof(StarlightLoadExpansionAttribute)));
        }
        finally
        {
            context.Unload();
            for (int i = 0; i < 10 && GC.GetTotalMemory(false) > 0; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }

    internal static string TruncateForID(string input) => Regex.Replace(input, "[^a-zA-Z0-9]", "").ToLower();
}