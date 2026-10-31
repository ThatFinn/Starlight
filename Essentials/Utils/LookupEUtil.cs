using System;
using System.Linq;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Player.Component;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Starlight.Enums;
using Starlight.Prism.Lib;
using UnityEngine.InputSystem;

namespace Starlight.Utils;

public static class LookupEUtil
{

    internal static InputEvent CloseInput = null;
    internal static Dictionary<string, InputActionMap> ActionMaps = new ();
    internal static Dictionary<string, InputAction> MainGameActions = new ();
    internal static Dictionary<string, InputAction> PausedActions = new ();
    internal static Dictionary<string, InputAction> DebugActions = new ();
    internal static IdentifiableTypeGroupList IdentifiableTypeGroupList;

    
    public static readonly Dictionary<GameObject, (ParticleSystemRenderer, string)> FXLibrary = new ();
    public static readonly Dictionary<string, (ParticleSystemRenderer, GameObject)> FXLibraryReversable = new ();
    
    
    public static WeatherStateDefinition[] weatherStateDefinitions => autoSaveDirector._configuration.WeatherStates.items.ToArray();
    public static UpgradeComponent[] upgradeComponents => autoSaveDirector._configuration.UpgradeComponents.items.ToArray();
    public static Dictionary<string, IdentifiableTypeGroup> allIdentifiableTypeGroups {
        get
        {
            var dict = new Dictionary<string, IdentifiableTypeGroup>();
            if (IdentifiableTypeGroupList == null) return dict;
            foreach (var item in IdentifiableTypeGroupList.items) dict.TryAdd(item.name, item);
            return dict;
        }
}


    
    public static IdentifiableType[] identifiableTypes => MembersOf(autoSaveDirector?._configuration?.IdentifiableTypes).ToArray();

    public static IdentifiableType[] vaccableTypes => _FromGroupList("VaccableNonLiquids");
    
    public static IdentifiableType[] allVaccableTypes
    {
        get
        {
            var list = new List<IdentifiableType>(vaccableTypes);
            foreach (var liquid in liquidTypes)
                if (!list.Contains(liquid.Cast<IdentifiableType>()))
                    list.Add(liquid.Cast<IdentifiableType>());
            return list.ToArray();
        }
    }
    public static GadgetDefinition[] gadgetTypes => _FromGroupList<GadgetDefinition>("GadgetGroup");
    public static ToyDefinition[] toyTypes => _FromGroupList<ToyDefinition>("ToyGroup");
    public static LiquidDefinition[] liquidTypes => _FromGroupList<LiquidDefinition>("LiquidGroup");
    
    public static SlimeDefinition[] baseSlimeTypes => _FromGroupList<SlimeDefinition>("BaseSlimeGroup");
    public static SlimeDefinition[] slimeTypes => _FromGroupList<SlimeDefinition>("SlimesGroup");
    public static SlimeDefinition[] largoTypes => _FromGroupList<SlimeDefinition>("LargoGroup");
    public static IdentifiableType[] plortTypes => _FromGroupList("PlortGroup");
    public static IdentifiableType[] foodTypes => _FromGroupList("FoodGroup");
    public static IdentifiableType[] meatFoodTypes => _FromGroupList("MeatGroup");
    public static IdentifiableType[] veggieFoodTypes => _FromGroupList("VeggieGroup");
    public static IdentifiableType[] fruitFoodTypes => _FromGroupList("FruitGroup");
    public static IdentifiableType[] nectarFoodTypes => _FromGroupList("NectarFoodGroup");
    public static IdentifiableType[] chickFoodTypes => _FromGroupList("ChickGroup");
    public static IdentifiableType[] craftTypes => _FromGroupList("CraftGroup");

    
    /// <summary>
    /// Members of a group by name
    /// </summary>
    public static IdentifiableType[] GetGroupMembers(string groupName) => MembersOf(Groups.Find(groupName)).ToArray();

    static IdentifiableType[] _FromGroupList(string name) => GetGroupMembers(name);

    static T[] _FromGroupList<T>(string name) where T : IdentifiableType
    {
        var list = new List<T>();
        foreach (var type in MembersOf(Groups.Find(name)))
        {
            var def = type.TryCast<T>();
            if (def != null) list.Add(def);
        }
        return list.ToArray();
    }

    /// <summary>
    /// Every type in a group that has a usable reference id
    /// </summary>
    static IEnumerable<IdentifiableType> MembersOf(IdentifiableTypeGroup? group)
    {
        if (group == null) yield break;

        var visited = new HashSet<IntPtr>();
        var listed  = new HashSet<IntPtr>();
        var pending = new Stack<IdentifiableTypeGroup>();
        pending.Push(group);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current.Pointer)) continue;

            if (current._memberTypes != null)
            {
                foreach (var type in current._memberTypes)
                {
                    if (type == null || string.IsNullOrEmpty(type.ReferenceId)) continue;
                    if (listed.Add(type.Pointer)) yield return type;
                }
            }
            
            var runtimeMembers = RuntimeMembersOf(current);
            if (runtimeMembers != null)
            {
                foreach (var type in runtimeMembers)
                {
                    if (type == null || string.IsNullOrEmpty(type.ReferenceId)) continue;
                    if (listed.Add(type.Pointer)) yield return type;
                }
            }

            if (current._memberGroups == null) continue;
            foreach (var nested in current._memberGroups)
                if (nested != null) pending.Push(nested);
        }
    }

    /// <summary>
    /// The members the game has added to a group's runtime copy, or null if it has none yet
    /// </summary>
    static Il2CppSystem.Collections.Generic.List<IdentifiableType>? RuntimeMembersOf(IdentifiableTypeGroup group)
    {
        try { return group.GetRuntimeObject()?._memberTypes; }
        catch { return null; }
    }

    /// <summary>
    /// Finds any identifiable type by reference id
    /// </summary>
    public static IdentifiableType GetIdentifiableTypeByRefID(string referenceId) => identifiableTypes.GetEntryByRefID(referenceId);

    /// <summary>
    /// Finds a base (non-largo) slime definition by reference id
    /// </summary>
    public static SlimeDefinition GetBaseSlimeByRefID(string referenceId) => baseSlimeTypes.GetEntryByRefID(referenceId);

    public static ToyDefinition GetEntryByRefID(this ToyDefinition[]? idents, string referenceID)
    {
        if (string.IsNullOrWhiteSpace(referenceID)) return null;
        if (idents == null||idents.Length==0) return null;
        referenceID = referenceID.ToUpper();
        foreach (ToyDefinition type in idents) if (type.ReferenceId.ToUpper() == referenceID) return type;
        return null;
    }    
    public static GadgetDefinition GetEntryByRefID(this GadgetDefinition[]? idents, string referenceID)
    {
        if (string.IsNullOrWhiteSpace(referenceID)) return null;
        if (idents == null||idents.Length==0) return null;
        referenceID = referenceID.ToUpper();
        foreach (GadgetDefinition type in idents) if (type.ReferenceId.ToUpper() == referenceID) return type;
        return null;
    }    
    public static SlimeDefinition GetEntryByRefID(this SlimeDefinition[]? idents, string referenceID)
    {
        if (string.IsNullOrWhiteSpace(referenceID)) return null;
        if (idents == null||idents.Length==0) return null;
        referenceID = referenceID.ToUpper();
        foreach (SlimeDefinition type in idents) if (type.ReferenceId.ToUpper() == referenceID) return type;
        return null;
    }    
    public static LiquidDefinition GetEntryByRefID(this LiquidDefinition[]? idents, string referenceID)
    {
        if (string.IsNullOrWhiteSpace(referenceID)) return null;
        if (idents == null||idents.Length==0) return null;
        referenceID = referenceID.ToUpper();
        foreach (LiquidDefinition type in idents) if (type.ReferenceId.ToUpper() == referenceID) return type;
        return null;
    } 
    public static IdentifiableType GetEntryByRefID(this IdentifiableType[]? idents, string referenceID)
    {
        if (string.IsNullOrWhiteSpace(referenceID)) return null;
        if (idents == null||idents.Length==0) return null;
        referenceID = referenceID.ToUpper();
        foreach (IdentifiableType type in idents) if (type.ReferenceId.ToUpper() == referenceID) return type;
        return null;
    }    
    public static IdentifiableType GetEntryByName(this IdentifiableType[]? idents, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (idents == null||idents.Length==0) return null;
        name = name.ToUpper();
        foreach (IdentifiableType type in identifiableTypes) if (type.name.ToUpper() == name) return type;
        name=name.Replace("_", "").Replace(" ","");
        foreach (IdentifiableType type in idents) if (type.GetCompactUpperName() == name) return type;
        return null;
    }
    public static IdentifiableType GetEntryByRefID(this IdentifiableTypeGroup group, string referenceID)
    {
        if (string.IsNullOrWhiteSpace(referenceID)) return null;
        if (group == null) return null;
        referenceID = referenceID.ToUpper();
        foreach (var type in group.GetAllMembersHashSet()) if (type.ReferenceId == referenceID) return type;
        return null;
    }    
    public static IdentifiableType GetEntryByName(this IdentifiableTypeGroup group, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (group == null) return null;
        name = name.ToUpper();
        foreach (var type in group.GetAllMembersHashSet()) if (type.name.ToUpper() == name) return type;
        name=name.Replace("_", "").Replace(" ","");
        foreach (var type in group.GetAllMembersHashSet()) if (type.GetCompactUpperName() == name) return type;
        return null;
    }
    
    
    
    
    
    
    public static bool IsGadget(this IdentifiableType type) => type.TryCast<GadgetDefinition>() != null;
    /// <summary>
    /// Get an IdentifiableType either by its code name or localized name
    /// </summary>
    /// <param name="name"></param>
    /// <returns>IdentifiableType</returns>
    public static IdentifiableType GetIdentifiableTypeByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.ToUpper();
        if (name == "NONE" || name == "PLAYER") return null;
        var ids = identifiableTypes;
        foreach (var type in ids) if (type.name.ToUpper() == name) return type;
        name=name.Replace("_", "").Replace(" ","");
        foreach (var type in ids) if (type.GetCompactUpperName() == name) return type;
        return null;
    }

    public static GadgetDefinition GetGadgetDefinitionByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.ToUpper();
        if (name == "NONE" || name == "PLAYER") return null;
        var ids = gadgetTypes;
        foreach (var type in ids) 
            if (type.name.ToUpper() == name) 
                if(type.IsGadget())
                    return type.Cast<GadgetDefinition>();
        name=name.Replace("_", "").Replace(" ","");
        foreach (var type in ids)
            if (type.GetCompactUpperName() == name)
                if(type.IsGadget())
                    return type.Cast<GadgetDefinition>();
        return null;
    }

    public static WeatherStateDefinition GetWeatherStateDefinitionByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.ToUpper().Replace("_", "").Replace(" ","");
        foreach (var state in weatherStateDefinitions) if (state.GetCompactUpperName() == name) return state;
        return null;
    }



    static bool StartsWithOrContain(this string value,string compare,bool useContain)
    {
        if(useContain) return value.Contains(compare);
        return value.StartsWith(compare);
    }
    /// <summary>
    /// Returns a List<String> of GetCompactName() of all IdentifiableTypes
    /// This includes Slimes, Gordos, Vaccables, Gadgets, etc.
    /// The IdentifiableType "none" and "player" have been filtered out
    /// </summary>
    /// <param name="partial">The partial string</param>
    /// <param name="useContain">If true, returns every IdentifiableType which contains the partial string. If false, only if the IdentifiableType begins with the partial string</param>
    /// <param name="maxEntries">The max IdentifiableTypes you want. For a console command, MAX_AUTOCOMPLETE.Get() is recommended</param>
    /// <returns>List<string></returns>
    public static List<string> GetIdentifiableTypeStringListByPartialName(string partial, bool useContain, int maxEntries)
    {
        var types = identifiableTypes;
        int i = 0;
        maxEntries -= 1;
        //If partial string is empty, no need to match the name
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (IdentifiableType type in types)
            {
                if (i > maxEntries) break;
                if (type == null) continue;
                if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
                var name = type.GetCompactName();
                if (name.StartsWith("!")) continue;
                if(list.Contains(name)) continue;
                list.Add(name);
            }
            list.Sort();
            return list;
        }

        //Actually compare name
        partial = partial.ToUpper();
        foreach (IdentifiableType type in types)
        {
            if (i > maxEntries) break;
            if (type == null) continue;
            if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
            var name = type.GetCompactName();
            if (name.StartsWith("!")) continue;
            if(list.Contains(name)) continue;
            if(name.ToUpper().StartsWithOrContain(partial.ToUpper(),useContain))
                list.Add(name);
        }
        list.Sort();
        return list;
    }
    
    
    /// <summary>
    /// Returns a List<String> of GetCompactName() of filtered IdentifiableTypes
    /// This includes Slimes, Vaccables, Gadgets, etc.
    /// The IdentifiableType Gordos "none", "player" have been filtered out
    /// This was primarily made for commands
    /// </summary>
    /// <param name="partial">The partial string</param>
    /// <param name="useContain">If true, returns every IdentifiableType which contains the partial string. If false, only if the IdentifiableType begins with the partial string</param>
    /// <param name="maxEntries">The max IdentifiableTypes you want. For a console command, MAX_AUTOCOMPLETE.Get() is recommended</param>
    /// <param name="addStarToList">If true, adds a * to the list as an entry.</param>
    /// <returns>List<string></returns>
    public static List<string> GetFilteredIdentifiableTypeStringListByPartialName(string partial, bool useContain, int maxEntries, bool addStarToList = false)
    {
        var types = identifiableTypes;
        maxEntries -= 1;
        //If partial string is empty, no need to match the name
        var list = new List<string>();
        if(addStarToList) list.Add("*");
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (IdentifiableType type in types)
            {
                if (list.Count > maxEntries) break;
                if (type == null) continue;
                if (type.ReferenceId.ToUpper().Contains("GORDO")) continue;
                if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
                var name = type.GetCompactName();
                if (name.StartsWith("!")) continue;
                if(list.Contains(name)) continue;
                list.Add(name);
            }
            list.Sort();
            return list;
        }

        //Actually compare name
        partial = partial.ToUpper();
        foreach (IdentifiableType type in types)
        {
            if (list.Count > maxEntries) break;
            if (type == null) continue;
            if (type.ReferenceId.ToUpper().Contains("GORDO")) continue;
            if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
            var name = type.GetCompactName();
            if (name.StartsWith("!")) continue;
            if(list.Contains(name)) continue;
            if(name.ToUpper().StartsWithOrContain(partial.ToUpper(),useContain))
                list.Add(name);
        }
        list.Sort();
        return list;
    }
    
    /// <summary>
    /// Returns a List<String> of GetCompactName() of filtered IdentifiableTypes
    /// This includes Slimes, Vaccables, etc.
    /// The IdentifiableType, Gadgets, Gordos "none", "player" have been filtered out
    /// This was primarily made for commands that can't deal with Gadgets
    /// </summary>
    /// <param name="partial">The partial string</param>
    /// <param name="useContain">If true, returns every IdentifiableType which contains the partial string. If false, only if the IdentifiableType begins with the partial string</param>
    /// <param name="maxEntries">The max IdentifiableTypes you want. For a console command, MAX_AUTOCOMPLETE.Get() is recommended</param>
    /// <param name="addStarToList">If true, adds a * to the list as an entry.</param>
    /// <returns>List<string></returns>
    public static List<string> GetStrongFilteredIdentifiableTypeStringListByPartialName(string partial, bool useContain, int maxEntries, bool addStarToList = false)
    {
        var types = identifiableTypes;
        maxEntries -= 1;
        //If partial string is empty, no need to match the name
        var list = new List<string>();
        if(addStarToList) list.Add("*");
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (IdentifiableType type in types)
            {
                if (list.Count > maxEntries) break;
                if (type == null) continue;
                if (type.ReferenceId.ToUpper().Contains("GORDO")) continue;
                if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
                if (type.IsGadget()) continue;
                var name = type.GetCompactName();
                if (name.StartsWith("!")) continue;
                if(list.Contains(name)) continue;
                list.Add(name);
            }
            list.Sort();
            return list;
        }

        //Actually compare name
        partial = partial.ToUpper();
        foreach (IdentifiableType type in types)
        {
            if (list.Count > maxEntries) break;
            if (type == null) continue;
            if (type.ReferenceId.ToUpper().Contains("GORDO")) continue;
            if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
            if (type.IsGadget()) continue;
            var name = type.GetCompactName();
            if (name.StartsWith("!")) continue;
            if(list.Contains(name)) continue;
            if(name.ToUpper().StartsWithOrContain(partial.ToUpper(),useContain))
                list.Add(name);
        }
        list.Sort();
        return list;
    }
    
    
    /// <summary>
    /// Returns a List<String> of GetCompactName() of all GadgetDefinitions
    /// </summary>
    /// <param name="partial">The partial string</param>
    /// <param name="useContain">If true, returns every GadgetDefinition which contains the partial string. If false, only if the GadgetDefinition begins with the partial string</param>
    /// <param name="maxEntries">The max GadgetDefinitions you want. For a console command, MAX_AUTOCOMPLETE.Get() is recommended</param>
    /// <returns>List<string></returns>
    public static List<string> GetGadgetDefinitionStringListByPartialName(string partial, bool useContain, int maxEntries)
    {
        var types = sceneContext.GadgetDirector._gadgetsGroup.GetAllMembersHashSet();
        maxEntries -= 1;
        //If partial string is empty, no need to match the name
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (IdentifiableType type in types)
            {
                if (list.Count > maxEntries) break;
                if (type == null) continue;
                var name = type.GetCompactName();
                if (name.StartsWith("!")) continue;
                if(list.Contains(name)) continue;
                list.Add(name);
            }
            list.Sort();
            return list;
        }

        //Actually compare name
        partial = partial.ToUpper();
        foreach (IdentifiableType type in types)
        {
            if (list.Count > maxEntries) break;
            if (type == null) continue;
            var name = type.GetCompactName();
            if (name.StartsWith("!")) continue;
            if(list.Contains(name)) continue;
            if(name.ToUpper().StartsWithOrContain(partial.ToUpper(),useContain))
                list.Add(name);
        }
        list.Sort();
        return list;
    }
    
    /// <summary>
    /// Returns a List<String> of GetCompactName() of all Vaccables
    /// </summary>
    /// <param name="partial">The partial string</param>
    /// <param name="useContain">If true, returns every Vaccable which contains the partial string. If false, only if the Vaccable begins with the partial string</param>
    /// <param name="maxEntries">The max Vaccables you want. For a console command, MAX_AUTOCOMPLETE.Get() is recommended</param>
    /// <returns>List<string></returns>
    public static List<string> GetVaccableStringListByPartialName(string partial, bool useContain, int maxEntries)
    {
        var types = allVaccableTypes;
        maxEntries -= 1;
        //If partial string is empty, no need to match the name
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (IdentifiableType type in types)
            {
                if (list.Count > maxEntries) break;
                if (type == null) continue;
                if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
                var name = type.GetCompactName();
                if (name.StartsWith("!")) continue;
                if(list.Contains(name)) continue;
                list.Add(name);
            }
            list.Sort();
            return list;
        }

        //Actually compare name
        partial = partial.ToUpper();
        foreach (IdentifiableType type in types)
        {
            if (list.Count > maxEntries) break;
            if (type == null) continue;
            if (type.ReferenceId.ToUpper() == "NONE" || type.ReferenceId.ToUpper() == "PLAYER") continue;
            var name = type.GetCompactName();
            if (name.StartsWith("!")) continue;
            if(list.Contains(name)) continue;
            if(name.ToUpper().StartsWithOrContain(partial.ToUpper(),useContain))
                list.Add(name);
        }
        list.Sort();
        return list;
    }
    
    
    public static List<string> GetKeyStringListByPartialName(string partial, bool useContain, int maxEntries) 
    {
        var list = new List<string>();
        maxEntries -= 1;
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (Key key in Enum.GetValues<Key>())
            {
                if (list.Count > maxEntries) break;
                if (key != Key.None)
                    if (partial != null && key.ToString().ToUpper().StartsWith(partial.ToUpper()))
                        list.Add(key.ToString());
            }
            list.Sort();
            return list;
        }

        foreach (Key key in Enum.GetValues<Key>())
        {
            if (list.Count > maxEntries) break;
            if (key != Key.None)
                if (key.ToString().ToUpper().StartsWithOrContain(partial.ToUpper(), useContain))
                {
                    var str = key.ToString();
                    list.Add(str);
                    if(!list.Contains(str.ToLower())) list.Add(str.ToLower());
                }
        }
        
        list.Sort();
        return list;
    }
    public static List<string> GetLKeyStringListByPartialName(string partial, bool useContain, int maxEntries, bool includeExtendedLatin = false, bool includeCyrillic = false) 
    {
        var list = new List<string>();
        maxEntries -= 1;
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (LKey key in Enum.GetValues<LKey>())
            {
                if (list.Count > maxEntries) break;
                var kint = Convert.ToInt32(key);
                if (!includeExtendedLatin && kint is > 100 and < 200) break;
                if (!includeCyrillic && kint is > 900 and < 1000) break;
                if (key != LKey.None)
                    if (partial != null && key.ToString().ToUpper().StartsWith(partial.ToUpper()))
                        list.Add(key.ToString());
            }
            list.Sort();
            return list;
        }

        foreach (LKey key in Enum.GetValues<LKey>())
        {
            if (list.Count > maxEntries) break;
            if (key != LKey.None)
                if (key.ToString().ToUpper().StartsWithOrContain(partial.ToUpper(), useContain))
                {
                    var str = key.ToString();
                    list.Add(str);
                    if(!list.Contains(str.ToLower())) list.Add(str.ToLower());
                }
        }
        
        list.Sort();
        return list;
    }
    public static List<string> GetKeyCodeStringListByPartialName(string partial, bool useContain, int maxEntries) 
    {
        var list = new List<string>();
        maxEntries -= 1;
        if (string.IsNullOrWhiteSpace(partial))
        {
            foreach (KeyCode key in Enum.GetValues<KeyCode>())
            {
                if (list.Count > maxEntries) break;
                if (key != KeyCode.None)
                    if (partial != null && key.ToString().ToUpper().StartsWith(partial.ToUpper()))
                        list.Add(key.ToString());
            }
            list.Sort();
            return list;
        }

        foreach (KeyCode key in Enum.GetValues<KeyCode>())
        {
            if (list.Count > maxEntries) break;
            if (key != KeyCode.None)
                if (key.ToString().ToUpper().StartsWithOrContain(partial.ToUpper(), useContain))
                {
                    var str = key.ToString();
                    list.Add(str);
                    if(!list.Contains(str.ToLower())) list.Add(str.ToLower());
                }
        }
        
        list.Sort();
        return list;
    }
    
    
    
    
    
}