using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Data.Pedia;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Lib;

/// <summary>
/// A library of helper functions for dealing with the pedia
/// </summary>
public static class PrismLibPedia
{
    public static IdentifiablePediaEntry IdentifiablePediaEntryPrefab;
    public static RadiantSlimePediaEntry RadiantSlimePediaEntryPrefab;

    // Created things and pages whose creator asked other mods to not recolor their art
    private static readonly HashSet<System.IntPtr> NotRecolorable = new();

    /// <summary>
    /// Whether other mods may recolor the created thing's art
    /// </summary>
    public static bool CanBeRecolored(this IdentifiableType? type) => type != null && !NotRecolorable.Contains(type.Pointer);

    /// <summary>
    /// Whether other mods may recolor the page's art
    /// </summary>
    public static bool CanBeRecolored(this PediaEntry? entry) =>
        Starlight.Prism.Patches.PageArt.OwnerOf(entry) is { } owner && !NotRecolorable.Contains(owner.Pointer);

    /// <summary>
    /// Lets other mods recolor the thing's art, or not
    /// </summary>
    public static void SetCanBeRecolored(this IdentifiableType type, bool value)
    {
        if (type != null) SetCanBeRecolored(type.Pointer, value);
    }

    /// <summary>
    /// Lets other mods recolor the page's art, or not
    /// </summary>
    public static void SetCanBeRecolored(this PediaEntry entry, bool value)
    {
        if (Starlight.Prism.Patches.PageArt.OwnerOf(entry) is { } owner) SetCanBeRecolored(owner.Pointer, value);
    }

    private static void SetCanBeRecolored(System.IntPtr pointer, bool value)
    {
        if (value) NotRecolorable.Remove(pointer);
        else NotRecolorable.Add(pointer);
    }

    /// <summary>
    /// Shifts the colors of a created thing's art in the Slimepedia, the fabricator and the shop
    /// </summary>
    /// <remarks> Works for the game's own things too </remarks>
    /// <returns> Whether it was recolored; not when <see cref="CanBeRecolored(IdentifiableType)"/> is off or it has no art </returns>
    public static bool RecolorArt(this IdentifiableType type, Starlight.Utils.ColorShift shift) =>
        type.CanBeRecolored() && Starlight.Prism.Patches.PageArt.AddRecolor(type, shift);

    /// <summary>
    /// Shifts the colors of a Slimepedia page's art
    /// </summary>
    /// <returns> Whether it was recolored; not when <see cref="CanBeRecolored(PediaEntry)"/> is off or the page has no art </returns>
    public static bool RecolorArt(this PediaEntry entry, Starlight.Utils.ColorShift shift) =>
        entry.CanBeRecolored() && Starlight.Prism.Patches.PageArt.AddRecolor(entry, shift);

    internal static readonly Dictionary<PrismPediaCategoryType, PediaCategory> PediaCategories = new();
    internal static readonly Dictionary<PediaEntry, List<PrismPediaAdditionalFact>> AdditionalFactsMap = new ();
    internal static FixedPediaEntry FixedPediaEntryPrefab;
    internal static readonly Dictionary<PrismPediaCategoryType, List<PediaEntry>> PediaEntryLookup = new ()
    {
        {PrismPediaCategoryType.Slimes, new List<PediaEntry>()}, {PrismPediaCategoryType.RadiantSlimes, new List<PediaEntry>()}, {PrismPediaCategoryType.Resources, new List<PediaEntry>()}, {PrismPediaCategoryType.Blueprints, new List<PediaEntry>()},
        {PrismPediaCategoryType.World, new List<PediaEntry>()}, {PrismPediaCategoryType.Weather, new List<PediaEntry>()}, {PrismPediaCategoryType.Toys, new List<PediaEntry>()}, {PrismPediaCategoryType.Ranch, new List<PediaEntry>()},
        {PrismPediaCategoryType.Science, new List<PediaEntry>()}, {PrismPediaCategoryType.Tutorial, new List<PediaEntry>()},
    };
    private static readonly Dictionary<PrismPediaDetailType, PediaDetailSection> PediaDetailSectionLookup = new ();
    private static readonly Dictionary<PrismPediaFactSetType, PediaHighlightSet> PediaPrismFactSetLookup = new ();

    /// <summary>
    /// Adds a pedia entry to a category
    /// </summary>
    /// <returns> True when it was added </returns>
    internal static bool AddEntry(PrismPediaCategoryType type, PediaEntry entry)
    {
        if (entry == null || !PediaEntryLookup.TryGetValue(type, out var entries)) return false;
        
        var id = Id(entry);

        foreach (var existing in entries)
            if (existing != null && Id(existing) == id)
            {
                LogWarning($"Prism: Pedia entry '{id}' already exists in {type}, dropping the duplicate");
                return false;
            }

        entries.Add(entry);
        return true;
    }

    /// <summary>
    /// Whether a category already has an entry with this persistence id
    /// </summary>
    internal static bool AlreadyPresent(PediaRuntimeCategory category, PediaEntry entry)
    {
        if (category == null || entry == null) return false;
        
        var id = Id(entry);

        if (string.IsNullOrEmpty(id)) return false;

        var items = category._items;

        if (items == null) return false;

        for (var i = 0; i < items.Count; i++)
        {
            var existing = items[i];

            if (existing != null && !existing.Pointer.Equals(entry.Pointer) && Id(existing) == id) return true;
        }

        return false;
    }

    /// <summary>
    /// The key the save's translation table uses
    /// </summary>
    private static string Id(PediaEntry entry)
    {
        try { return entry.PersistenceId ?? entry.name; }
        catch { return entry.name; }
    }

    /// <summary>
    /// Gets the pedia entries for a given category
    /// </summary>
    /// <param name="type">The category to get the entries for</param>
    /// <returns>The pedia entries for the given category</returns>
    public static List<PediaEntry> GetPediaEntries(this PrismPediaCategoryType type)
    {
        if (PediaEntryLookup.ContainsKey(type)) return PediaEntryLookup[type];
        return null;
    }
    /// <summary>
    /// Gets the pedia highlight set for a given fact set type
    /// </summary>
    /// <param name="type">The fact set type to get the highlight set for</param>
    /// <returns>The pedia highlight set for the given fact set type</returns>
    public static PediaHighlightSet GetPediaHighlightSet(this PrismPediaFactSetType type)
    {
        return PediaPrismFactSetLookup.GetValueOrDefault(type);
    }
    /// <summary>
    /// Gets the pedia detail section for a given detail type
    /// </summary>
    /// <param name="type">The detail type to get the detail section for</param>
    /// <returns>The pedia detail section for the given detail type</returns>
    public static PediaDetailSection GetPediaDetailSection(this PrismPediaDetailType type)
    {
        return PediaDetailSectionLookup.GetValueOrDefault(type);
    }
    
    internal static PediaEntryHighlight ConvertToNativeType(this PrismPediaAdditionalFact prismPediaAdditionalFact) => new PediaEntryHighlight()
    {
        Icon = prismPediaAdditionalFact.Icon,
        Description = prismPediaAdditionalFact.Description,
        Label = prismPediaAdditionalFact.Title,
    };

    internal static PediaEntryHighlight ConvertToNativeType(this PrismPediaAdditionalFact? prismPediaAdditionalFact)
    {
        if (prismPediaAdditionalFact == null) return null;
        return ConvertToNativeType(prismPediaAdditionalFact.Value);
    }
    internal static PediaEntryDetail ConvertToNativeType(this PrismPediaDetail prismPediaDetail) => new PediaEntryDetail()
    {
        Contents = new Il2CppReferenceArray<PediaEntryDetailSubContent>(0),
        Section = PediaDetailSectionLookup[prismPediaDetail.Type],
        Text = prismPediaDetail.Text,
        TextGamepad = prismPediaDetail.Text,
        TextPS4 = prismPediaDetail.Text,
    };

    internal static PediaEntryDetail ConvertToNativeType(this PrismPediaDetail? prismPediaDetail)
    {
        if (prismPediaDetail == null) return null;
        return ConvertToNativeType(prismPediaDetail.Value);
    }
    
    internal static void PediaDetailTypesInitialize()
    {
        PediaDetailSectionLookup.Add(PrismPediaDetailType.Slimeology, Get<PediaDetailSection>("Slimeology"));
        PediaDetailSectionLookup.Add(PrismPediaDetailType.RancherRisks, Get<PediaDetailSection>("Rancher Risks"));
        PediaDetailSectionLookup.Add(PrismPediaDetailType.Plortonomics, Get<PediaDetailSection>("Plortonomics"));
        PediaDetailSectionLookup.Add(PrismPediaDetailType.About, Get<PediaDetailSection>("About"));
        PediaDetailSectionLookup.Add(PrismPediaDetailType.OnTheRanch, Get<PediaDetailSection>("How To Use"));
        PediaDetailSectionLookup.Add(PrismPediaDetailType.Instructions, Get<PediaDetailSection>("Instructions"));

        PediaPrismFactSetLookup.Add(PrismPediaFactSetType.None, Get<PediaHighlightSet>("TutorialPediaTemplate"));
        PediaPrismFactSetLookup.Add(PrismPediaFactSetType.Resource, Get<PediaHighlightSet>("ResourceHighlights"));
        PediaPrismFactSetLookup.Add(PrismPediaFactSetType.Slime, Get<PediaHighlightSet>("SlimeHighlights"));
        PediaPrismFactSetLookup.Add(PrismPediaFactSetType.Food, Get<PediaHighlightSet>("FoodHightlights")); //yes, there is a typo in there...

        IdentifiablePediaEntryPrefab = Get<IdentifiablePediaEntry>("Pink");
        if (IdentifiablePediaEntryPrefab == null) IdentifiablePediaEntryPrefab = GetAny<IdentifiablePediaEntry>();
        if (IdentifiablePediaEntryPrefab != null) IdentifiablePediaEntryPrefab.hideFlags = HideFlags.DontUnloadUnusedAsset;

        if(SupportRadiant.HasFlag()) RadiantStuff();
        
        FixedPediaEntryPrefab = Get<FixedPediaEntry>("PrismaPlorts");
        if (FixedPediaEntryPrefab == null) FixedPediaEntryPrefab = GetAny<FixedPediaEntry>();
        if (FixedPediaEntryPrefab != null) FixedPediaEntryPrefab.hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    private static void RadiantStuff()
    {
        RadiantSlimePediaEntryPrefab = Get<RadiantSlimePediaEntry>("RadiantPink");
        if (RadiantSlimePediaEntryPrefab == null) RadiantSlimePediaEntryPrefab = GetAny<RadiantSlimePediaEntry>();
        if (RadiantSlimePediaEntryPrefab != null) RadiantSlimePediaEntryPrefab.hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
    
    public static void PediaSlimeDietAddOverride(this PrismSlime slime, Sprite icon, LocalizedString localized)
    {
        foreach (var highlight in GetAll<DietGroupHighlight>())
        {
            highlight._slimeDietOverrideEntries.Add(new DietGroupHighlight.SlimeDietOverrideEntry
            {
                _icon=icon,_label=localized,_slime=slime
            });
        }
    }
}