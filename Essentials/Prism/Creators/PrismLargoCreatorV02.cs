using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Patches;
using Starlight.Prism.Lib;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// The largo creator, used to create largos
/// </summary>
public static class PrismLargoCreatorV02
{
    /// <summary>
    /// What a largo of two slimes made from the same native one is copied from, when that native one is Pink
    /// </summary>
    /// <remarks> Pink and Rock, with Rock's two components and two body parts taken out again </remarks>
    private const string GenericLargoBase = "SlimeDefinition.PinkRock";

    /// <summary>
    /// The native largo a new one is copied from, and the native slimes in it that are neither half
    /// </summary>
    private readonly record struct Template(SlimeDefinition Largo, IReadOnlyList<SlimeDefinition> Foreign);

    /// <summary>
    /// Creates a largo of two base slimes
    /// </summary>
    /// <param name="first"> The first base slime </param>
    /// <param name="second"> The second base slime </param>
    /// <param name="displayName"> The largo's display name </param>
    /// <param name="actions"> What happens when something happens to this largo, such as the vacpack taking hold of it </param>
    public static SlimeDefinition? Create(
        SlimeDefinition first,
        SlimeDefinition second,
        LocalizedString? displayName = null,
        SlimeActions? actions = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (first.IsLargo || second.IsLargo)
        {
            LogWarning($"Prism: Can't create the {first.name}/{second.name} largo: cannot turn a largo into another largo");
            return null;
        }
        
        if (first.ReferenceId == second.ReferenceId) return null;
        if (Exists(first, second)) return null;

        var resolved = ResolveTemplate(first, second);
        if (resolved == null)
        {
            LogWarning($"Prism: Can't create the {first.name}/{second.name} largo: no native largo to copy");
            return null;
        }

        var template = resolved.Value.Largo;
        var foreign = resolved.Value.Foreign;

        var name = ContentNames.Unique(first.Name + second.Name, "largo");
        var referenceId = ContentNames.SlimeReferenceId(
            ContentNames.StableName(first.Name + second.Name, string.Empty));

        var largo = UObject.Instantiate(template);
        largo.hideFlags = HideFlags.DontUnloadUnusedAsset;
        largo.name      = name;
        largo.Name      = name;
        
        PageArt.ClearFullArt(largo);
        
        largo.BaseSlimes = new[] { first, second }.ToIl2CppArray();
        
        largo._pediaPersistenceSuffix =
            $"{ContentNames.ModdedPrefix.ToLowerInvariant()}{first.name.ToLowerInvariant()}_{second.name.ToLowerInvariant()}_largo";

        // Modules carry a slime's behavior
        largo.SlimeModules = BuildModules(first, second);
        
        largo.localizedName = displayName ?? BuildName(first, second, template, name);
        
        largo.prefab = BuildPrefab(name, template, largo);
        RemoveForeignComponents(largo.prefab, foreign, first, second);

        if (foreign.Count > 0) largo.Sounds = PrismSlimeCreatorV02.NativeOf(first).Sounds;

        largo.AppearancesDefault = new[] { BuildAppearance(largo, template, first, second, name, ForeignParts(foreign, first, second)) }.ToIl2CppArray();
        largo.RadiantBase = null;
        largo.RadiantLargo0 = null;
        largo.RadiantLargo1 = null;

        largo.Diet = MergeDiets(first, second);
        
        if (!SaveRegistry.Register(largo, referenceId,
                ContentNames.LegacyReferenceId("SlimeDefinition", name, string.Empty)))
        {
            try
            {
                if (largo.prefab != null) UObject.Destroy(largo.prefab);
                UObject.Destroy(largo);
            }
            catch { }

            return null;
        }

        RegisterPairing(first, second, largo);
        
        PrismRadiantCreatorV02.CreateLargoForms(largo, first, second);
        SlimeEvents.Register(largo, actions);

        Groups.AddMultiple(largo, Groups.Names.AllIdentifiableTypes, Groups.Names.EdibleSlime, "LargoGroup");

        LinkPlortFeeding(first, second, largo);

        CreatedCount++;
        return largo;
    }

    /// <summary>
    /// Whether the game already knows a largo for this pairing
    /// </summary>
    private static bool Exists(SlimeDefinition first, SlimeDefinition second)
    {
        try
        {
            var pair = new SlimeDefinitions.SlimeDefinitionPair(first, second);
            return slimeDefinitions._largoDefinitionByBaseDefinitions.ContainsKey(pair);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Picks the native largo to copy by the native slimes the halves were made from
    /// </summary>
    private static Template? ResolveTemplate(SlimeDefinition first, SlimeDefinition second)
    {
        var a = PrismSlimeCreatorV02.NativeOf(first);
        var b = PrismSlimeCreatorV02.NativeOf(second);

        var largo = a.Pointer != b.Pointer ? NativeLargo(a, b) : null;

        if (largo == null)
        {
            var pink = PrismNativeBaseSlime.Pink.Find();
            var generic = LookupEUtil.GetIdentifiableTypeByRefID(GenericLargoBase)?.TryCast<SlimeDefinition>();

            largo = a.Pointer == pink?.Pointer ? generic
                : (pink != null ? NativeLargo(a, pink) : null) ?? NativeLargoWith(a) ?? generic;
        }

        if (largo == null) return null;

        var foreign = largo.BaseSlimes.AsEnumerable()
            .Where(half => half != null && half.Pointer != a.Pointer && half.Pointer != b.Pointer)
            .ToList();

        return new Template(largo, foreign);
    }

    /// <summary>
    /// The game's own largo of two native slimes
    /// </summary>
    private static SlimeDefinition? NativeLargo(SlimeDefinition a, SlimeDefinition b) =>
        NativeLargos().FirstOrDefault(largo =>
            (largo.BaseSlimes[0]!.Pointer == a.Pointer && largo.BaseSlimes[1]!.Pointer == b.Pointer) ||
            (largo.BaseSlimes[0]!.Pointer == b.Pointer && largo.BaseSlimes[1]!.Pointer == a.Pointer));

    /// <summary>
    /// Any of the game's own largos with this native slime as a half
    /// </summary>
    private static SlimeDefinition? NativeLargoWith(SlimeDefinition a) =>
        NativeLargos().FirstOrDefault(largo => largo.BaseSlimes[0]!.Pointer == a.Pointer || largo.BaseSlimes[1]!.Pointer == a.Pointer);

    private static IEnumerable<SlimeDefinition> NativeLargos() =>
        slimeDefinitions.Slimes.AsEnumerable().Where(largo =>
            largo != null && largo.IsLargo && largo.BaseSlimes is { Length: 2 } &&
            largo.BaseSlimes[0] != null && largo.BaseSlimes[1] != null && Pairings.All(pairing => pairing.Largo.Pointer != largo.Pointer));

    /// <summary>
    /// Removes components that came from the prefab but should not be there
    /// </summary>
    private static void RemoveForeignComponents(GameObject prefab, IReadOnlyList<SlimeDefinition> foreign, SlimeDefinition first, SlimeDefinition second)
    {
        if (foreign.Count == 0) return;

        var ours = ComponentTypes(PrismSlimeCreatorV02.NativeOf(first).prefab)
            .Concat(ComponentTypes(PrismSlimeCreatorV02.NativeOf(second).prefab))
            .ToHashSet();

        var theirs = foreign.SelectMany(half => ComponentTypes(half.prefab)).Where(type => !ours.Contains(type)).ToHashSet();
        if (theirs.Count == 0) return;
        
        foreach (var component in prefab.GetComponentsInChildren<Component>(true).AsEnumerable().Reverse())
        {
            if (component == null || !theirs.Contains(component.GetIl2CppType().FullName)) continue;

            try { UObject.DestroyImmediate(component); }
            catch (Exception ex) { LogWarning($"Prism: Couldn't remove '{component.GetIl2CppType().Name}' out of the largo '{prefab.name}': {ex.Message}"); }
        }
    }

    private static IEnumerable<string> ComponentTypes(GameObject? prefab) =>
        prefab == null
            ? Enumerable.Empty<string>()
            : prefab.GetComponentsInChildren<Component>(true).AsEnumerable()
                .Where(component => component != null)
                .Select(component => component.GetIl2CppType().FullName);

    /// <summary>
    /// The body parts of the template's other half that neither of ours has
    /// </summary>
    private static HashSet<string> ForeignParts(IReadOnlyList<SlimeDefinition> foreign, SlimeDefinition first, SlimeDefinition second)
    {
        if (foreign.Count == 0) return new HashSet<string>();

        var ours = Parts(PrismSlimeCreatorV02.NativeOf(first)).Concat(Parts(PrismSlimeCreatorV02.NativeOf(second))).ToHashSet();
        return foreign.SelectMany(Parts).Where(part => !ours.Contains(part)).ToHashSet();
    }

    private static IEnumerable<string> Parts(SlimeDefinition slime) =>
        slime.AppearancesDefault.AsEnumerable().FirstOrDefault()?.Structures.AsEnumerable()
            .Where(structure => structure?.Element != null)
            .Select(structure => PartName(structure.Element))
        ?? Enumerable.Empty<string>();

    private static string PartName(SlimeAppearanceElement element) => element.name.Replace("(Clone)", string.Empty).Trim();

    /// <summary>
    /// The largos named after their halves
    /// </summary>
    private static readonly List<(string Key, SlimeDefinition First, SlimeDefinition Second, SlimeDefinition Template)> Named = new();

    /// <summary>
    /// Names the largo after its base slimes
    /// </summary>
    private static LocalizedString BuildName(SlimeDefinition first, SlimeDefinition second, SlimeDefinition template, string name)
    {
        var key = $"prism.largo.{name.ToLowerInvariant()}";
        Named.Add((key, first, second, template));

        return Starlight.Managers.StarlightLanguageManager.AddTranslation(ComposeName(first, second, template), key);
    }

    private static string ComposeName(SlimeDefinition first, SlimeDefinition second, SlimeDefinition template)
    {
        var a = WithoutSlime(Shown(first));
        var b = WithoutSlime(Shown(second));

        return FromTemplate(template, a, b) ?? $"{a} {b} Largo";
    }

    /// <summary>
    /// Renames every largo
    /// </summary>
    internal static void RenameAll()
    {
        foreach (var (key, first, second, template) in Named)
        {
            try { Starlight.Managers.StarlightLanguageManager.UpdateTranslation(ComposeName(first, second, template), key); }
            catch (Exception ex) { LogError($"Prism: Failed to name the {first.name}/{second.name} largo", ex); }
        }
    }

    /// <summary>
    /// The template largo's name with its two halves replaced by ours
    /// </summary>
    private static string? FromTemplate(SlimeDefinition template, string first, string second)
    {
        var halves = template.BaseSlimes;
        if (halves == null || halves.Length != 2 || halves[0] == null || halves[1] == null) return null;

        var named = Shown(template);
        var firstHalf = WithoutSlime(Shown(halves[0]));
        var secondHalf = WithoutSlime(Shown(halves[1]));

        var firstAt = named.IndexOf(firstHalf, StringComparison.OrdinalIgnoreCase);
        var secondAt = named.IndexOf(secondHalf, StringComparison.OrdinalIgnoreCase);
        if (firstAt < 0 || secondAt < 0 || firstHalf.Length == 0 || secondHalf.Length == 0) return null;
        
        if (firstAt < secondAt ? firstAt + firstHalf.Length > secondAt : secondAt + secondHalf.Length > firstAt) return null;
        
        if (firstAt < secondAt)
        {
            named = named.Remove(secondAt, secondHalf.Length).Insert(secondAt, second);
            return named.Remove(firstAt, firstHalf.Length).Insert(firstAt, first);
        }

        named = named.Remove(firstAt, firstHalf.Length).Insert(firstAt, first);
        return named.Remove(secondAt, secondHalf.Length).Insert(secondAt, second);
    }

    private static string Shown(SlimeDefinition slime)
    {
        string? shown = null;

        try { shown = slime.localizedName?.GetLocalizedString(); }
        catch { }

        return string.IsNullOrWhiteSpace(shown) || shown.StartsWith("!", StringComparison.Ordinal) ? slime.name : shown.Trim();
    }

    /// <summary>
    /// A slime's shown name without its trailing "Slime" or "-Slime"
    /// </summary>
    private static string WithoutSlime(string shown)
    {
        foreach (var suffix in new[] { "-Slime", " Slime" })
            if (shown.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return shown[..^suffix.Length];
        
        foreach (var prefix in new[] { "Slime-", "Slime " })
            if (shown.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return shown[prefix.Length..];

        return shown;
    }

    /// <summary>
    /// Merges the slime behavior from both base slimes
    /// </summary>
    private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<GameObject> BuildModules(
        SlimeDefinition first, SlimeDefinition second)
    {
        var modules = new List<GameObject>();

        var firstModule = first.SlimeModules.AsEnumerable().FirstOrDefault();
        if (firstModule != null) modules.Add(firstModule);
        
        var secondModule = second.SlimeModules.AsEnumerable().FirstOrDefault();
        if (secondModule != null && (firstModule == null || secondModule.Pointer != firstModule.Pointer)) modules.Add(secondModule);

        return modules.ToIl2CppArray();
    }

    private static GameObject BuildPrefab(string name, SlimeDefinition template, SlimeDefinition largo)
    {
        var prefab = Prefabs.Create("slime" + name, template.prefab);

        var identifiable = prefab.GetComponent<Identifiable>();
        if (identifiable != null) identifiable.identType = largo;

        var eat = prefab.GetComponent<SlimeEat>();
        if (eat != null) eat.SlimeDefinition = largo;

        var applicator = prefab.GetComponent<SlimeAppearanceApplicator>();
        if (applicator != null) applicator.SlimeDefinition = largo;

        var playWithToys = prefab.GetComponent<PlayWithToys>();
        if (playWithToys != null) playWithToys.SlimeDefinition = largo;

        var reactToToys = prefab.GetComponent<ReactToToyNearby>();
        if (reactToToys != null) reactToToys.SlimeDefinition = largo;

        return prefab;
    }

    /// <summary>
    /// Builds the largo's appearance
    /// </summary>
    private static SlimeAppearance BuildAppearance(
        SlimeDefinition largo,
        SlimeDefinition template,
        SlimeDefinition first,
        SlimeDefinition second,
        string name,
        HashSet<string> foreignParts)
    {
        var templateAppearance = template.AppearancesDefault.AsEnumerable().FirstOrDefault();
        var firstAppearance = first.AppearancesDefault.AsEnumerable().FirstOrDefault();
        var secondAppearance = second.AppearancesDefault.AsEnumerable().FirstOrDefault();

        var appearance = AppearanceCloner.Clone(templateAppearance ?? firstAppearance!, name + "Default");
        appearance._appearType = SlimeAppearance.AppearanceType.DEFAULT;
        appearance._fullArt = null;
        
        if (foreignParts.Count > 0 && appearance.Structures != null)
            appearance.Structures = appearance.Structures.AsEnumerable()
                .Where(structure => structure?.Element == null || !foreignParts.Contains(PartName(structure.Element)))
                .ToIl2CppArray();

        if (firstAppearance != null)
        {
            appearance._face = firstAppearance._face;
            appearance._icon = firstAppearance._icon;
        }

        if (firstAppearance != null && secondAppearance != null)
        {
            var blended = SlimeColors.Blend(firstAppearance._colorPalette, secondAppearance._colorPalette);
            SlimeColors.Apply(appearance, blended);
            largo.color = Color.Lerp(first.color, second.color, 0.5f);
            
            appearance._dependentAppearances = new[] { firstAppearance, secondAppearance }.ToIl2CppArray();
        }

        RegisterAppearance(largo, appearance);
        return appearance;
    }

    private static void RegisterAppearance(SlimeDefinition largo, SlimeAppearance appearance)
    {
        var director = appearanceDirector;
        if (director == null) return;

        director.RegisterDependentAppearances(largo, appearance);
        director.UpdateChosenSlimeAppearance(largo, appearance);
    }

    /// <summary>
    /// Merges the diets from both base slimes
    /// </summary>
    private static SlimeDiet MergeDiets(SlimeDefinition first, SlimeDefinition second)
    {
        var diet = PrismSlimeDietCreatorV02.Create(first);
        if (first.Diet == null || second.Diet == null) return diet;

        diet.ProduceIdents = first.Diet.ProduceIdents.UnionToNew(second.Diet.ProduceIdents);
        diet.FavoriteIdents = first.Diet.FavoriteIdents.UnionToNew(second.Diet.FavoriteIdents);
        diet.AdditionalFoodIdents = first.Diet.AdditionalFoodIdents.UnionToNew(second.Diet.AdditionalFoodIdents);
        diet.MajorFoodIdentifiableTypeGroups = first.Diet.MajorFoodIdentifiableTypeGroups.UnionToNew(second.Diet.MajorFoodIdentifiableTypeGroups);

        return diet;
    }

    /// <summary>
    /// Set when a pairing has been recorded but the indexes have not been rebuilt yet
    /// </summary>
    private static bool rebuildPending;

    /// <summary>
    /// Every recorded largo pairing
    /// </summary>
    private static readonly List<(SlimeDefinition First, SlimeDefinition Second, SlimeDefinition Largo)> Pairings = new();

    /// <summary>
    /// Links a new pairing
    /// </summary>
    private static void Link(SlimeDefinition first, SlimeDefinition second, SlimeDefinition largo)
    {
        slimeDefinitions._largoDefinitionByBaseDefinitions[new SlimeDefinitions.SlimeDefinitionPair(first, second)] = largo;
        slimeDefinitions._largoDefinitionByBaseDefinitions[new SlimeDefinitions.SlimeDefinitionPair(second, first)] = largo;
    }

    private static SlimeDefinition? Lookup(SlimeDefinition first, SlimeDefinition second)
    {
        try { return slimeDefinitions.GetLargoByBaseSlimes(first, second); }
        catch { return null; }
    }

    /// <summary>
    /// The largo the game's own lookup finds for two base slimes
    /// </summary>
    public static SlimeDefinition? Find(SlimeDefinition first, SlimeDefinition second)
    {
        try { return slimeDefinitions.GetLargoByBaseSlimes(first, second) ?? slimeDefinitions.GetLargoByBaseSlimes(second, first); }
        catch { return null; }
    }

    /// <summary>
    /// Records a new pairing
    /// </summary>
    private static void RegisterPairing(SlimeDefinition first, SlimeDefinition second, SlimeDefinition largo)
    {
        try
        {
            Link(first, second, largo);
            Pairings.Add((first, second, largo));
            rebuildPending = true;
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to register the {first.name}/{second.name} largo pairing", ex);
        }
    }

    /// <summary>
    /// How many largos have been created by Prism
    /// </summary>
    public static int CreatedCount { get; private set; }

    /// <summary>
    /// Creates a largo for every pairing of a created slime with another slime
    /// </summary>
    /// <returns> How many largos were created </returns>
    public static int PairWithEverything(IReadOnlyList<SlimeDefinition> created)
    {
        ArgumentNullException.ThrowIfNull(created);
        
        var ours = created.Where(slime => slime != null && slime.CanLargofy).ToList();
        var partners = LookupEUtil.baseSlimeTypes.Where(slime => slime != null && slime.CanLargofy).ToList();

        var attempted = new HashSet<string>(StringComparer.Ordinal);
        var built = 0;

        foreach (var slime in ours)
        {
            foreach (var partner in partners)
            {
                if (slime.ReferenceId == partner.ReferenceId) continue;
                
                var pairing = string.CompareOrdinal(slime.ReferenceId, partner.ReferenceId) < 0
                    ? $"{slime.ReferenceId}|{partner.ReferenceId}"
                    : $"{partner.ReferenceId}|{slime.ReferenceId}";

                if (!attempted.Add(pairing)) continue;

                try
                {
                    if (Create(slime, partner) != null) built++;
                }
                catch { }
            }
        }

        return built;
    }

    /// <summary>
    /// Rebuilds the game's slime indexes
    /// </summary>
    public static void CommitPairings()
    {
        if (!rebuildPending && !Pairings.Any(pairing => Readable(pairing) && Find(pairing.First, pairing.Second) == null))
            return;

        rebuildPending = false;
        
        foreach (var pairing in Pairings.Where(Readable))
        {
            if (!slimeDefinitions.Slimes.Contains(pairing.Largo))
                slimeDefinitions.Slimes = slimeDefinitions.Slimes.AddToNew(pairing.Largo);
        }
        
        var unreadable = Pairings.Where(pairing => !Readable(pairing)).Select(pairing => pairing.Largo).Distinct().ToList();
        foreach (var largo in unreadable)
            slimeDefinitions.Slimes = slimeDefinitions.Slimes.Where(slime => slime.Pointer != largo.Pointer).ToIl2CppArray();

        try
        {
            slimeDefinitions.RefreshIndexes();
            slimeDefinitions.RefreshDefinitions();
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to rebuild slime indexes", ex);
        }
        
        if (unreadable.Count > 0)
            LogWarning($"Prism: {unreadable.Count} largo(s) have a half that makes no plort and were left out of the game's index");

        RestoreLostPairings();
    }

    /// <summary>
    /// Whether both halves make a plort
    /// </summary>
    private static bool Readable((SlimeDefinition First, SlimeDefinition Second, SlimeDefinition Largo) pairing) =>
        MakesPlort(pairing.First) && MakesPlort(pairing.Second);

    /// <summary>
    /// Whether a slime produces a plort
    /// </summary>
    private static bool MakesPlort(SlimeDefinition slime)
    {
        var produced = slime.Diet?.ProduceIdents;
        return produced != null && produced.Count > 0 && produced[0] != null;
    }
    
    private static void RestoreLostPairings()
    {
        foreach (var (first, second, largo) in Pairings)
        {
            if (!MakesPlort(first) || !MakesPlort(second)) continue;
            
            if (Lookup(first, second) != null && Lookup(second, first) != null) continue;

            try
            {
                if (!slimeDefinitions.Slimes.Contains(largo))
                    slimeDefinitions.Slimes = slimeDefinitions.Slimes.AddToNew(largo);

                slimeDefinitions._slimeDefinitionsByIdentifiable.TryAdd(largo, largo);
                Link(first, second, largo);
            }
            catch (Exception ex)
            {
                LogError($"Prism: Couldn't restore the {first.name}/{second.name} largo pairing", ex);
            }
        }
    }

    /// <summary>
    /// Makes feeding each slime the other's plort turn it into the largo
    /// </summary>
    private static void LinkPlortFeeding(SlimeDefinition first, SlimeDefinition second, SlimeDefinition largo)
    {
        IdentifiableType? firstPlort = null;
        IdentifiableType? secondPlort = null;

        try
        {
            foreach (var pair in slimeDefinitions._largoDefinitionByBasePlorts)
            {
                if (pair.value?.Pointer != largo.Pointer) continue;
                firstPlort = pair.Key.Plort1;
                secondPlort = pair.Key.Plort2;
                break;
            }
        }
        catch
        {
            return;
        }

        if (firstPlort == null || secondPlort == null) return;
        
        PrismSlimeDietCreatorV02.AddEntry(first, new DietEntry(secondPlort, null, largo, IsFavourite: false, 0));
        PrismSlimeDietCreatorV02.AddEntry(second, new DietEntry(firstPlort, null, largo, IsFavourite: false, 0));
    }
}