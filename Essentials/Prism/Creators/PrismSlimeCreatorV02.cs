using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Lib;
using Starlight.Prism.Patches;
using Starlight.Prism.Data.Native;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create a slime
/// </summary>
public sealed record SlimeRequest
{
    /// <summary>
    /// The raw identifier
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The display name
    /// </summary>
    public required LocalizedString DisplayName { get; init; }

    /// <summary>
    /// Which native slime to copy for behavior
    /// </summary>
    public PrismNativeBaseSlime Base { get; init; } = PrismNativeBaseSlime.Pink;

    /// <summary>
    /// Which native slime to copy the appearance from
    /// </summary>
    /// <remarks> Null = <see cref="Base"/> </remarks>
    public PrismNativeBaseSlime? AppearanceBase { get; init; }

    /// <summary>
    /// The slime's color palette
    /// </summary>
    /// <remarks> Null = base slime's colors </remarks>
    public SlimePalette? Palette { get; init; }

    /// <summary>
    /// A texture painted over the slime
    /// </summary>
    /// <remarks> <see cref="Palette"/> still colors the splats and effects </remarks>
    public Texture2D? Texture { get; init; }

    /// <summary>
    /// The Inventory and Slimepedia icon
    /// </summary>
    /// <remarks> Null = base slime's icon, recolored to <see cref="Palette"/> </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// Color of the vac and splat
    /// </summary>
    public Color? VacColor { get; init; }

    /// <summary>
    /// The slime's favourite slime toys
    /// </summary>
    /// <remarks> Null / Empty = none </remarks>
    public IReadOnlyList<ToyDefinition>? FavouriteToys { get; init; }
    
    /// <summary>
    /// Whether this slime can combine with others into largos
    /// </summary>
    public bool CanLargofy { get; init; }

    /// <summary>
    /// Whether tarr slimes can eat it
    /// </summary>
    public bool EdibleByTarrs { get; init; } = true;

    /// <summary>
    /// Whether the vacpack can pick it up
    /// </summary>
    public bool Vaccable { get; init; } = true;

    /// <summary>
    /// Its Slimepedia page
    /// </summary>
    /// <remarks> By default, one that says "No information" </remarks>
    public PediaInfo? Pedia { get; init; } = new();

    /// <summary>
    /// The large picture on its Slimepedia page
    /// </summary>
    /// <remarks> Without one the page shows the icon (in low quality) </remarks>
    public Sprite? FullArt { get; init; }

    /// <summary>
    /// The slime's radiant form
    /// </summary>
    /// <remarks> Null = no radiant form of the slime </remarks>
    public RadiantRequest? Radiant { get; init; }

    /// <summary>
    /// What happens when something happens to one of these slimes
    /// </summary>
    /// <remarks> Examples: it spawns, it dies, it is vacuumed in, it is shot out, it is held (largo), it eats </remarks>
    public SlimeActions? Actions { get; init; }
}

/// <summary>
/// A slime created by Prism
/// </summary>
/// <param name="Definition"> The slime definition </param>
/// <param name="Appearance"> Its default appearance </param>
/// <param name="ReferenceId"> Its persistence id </param>
public sealed record CreatedSlime(SlimeDefinition Definition, SlimeAppearance Appearance, string ReferenceId)
{
    /// <summary>
    /// The slime's object name for name-based lookups
    /// </summary>
    public string ObjectName => Definition.name;
}

/// <summary>
/// The slime creator, used to create slimes
/// </summary>
public static class PrismSlimeCreatorV02
{
    /// <summary>
    /// The native slime each created slime originated from, by the created slime's pointer
    /// </summary>
    private static readonly Dictionary<IntPtr, SlimeDefinition> NativeBases = new();

    /// <summary>
    /// The native slime a slime was made from
    /// </summary>
    /// <returns> The native slime (or itself if it is native) </returns>
    internal static SlimeDefinition NativeOf(SlimeDefinition slime) =>
        NativeBases.TryGetValue(slime.Pointer, out var native) && native != null ? native : slime;

    /// <summary>
    /// Creates a slime from a <see cref="SlimeRequest"/>
    /// </summary>
    /// <returns> The slime, or null if it couldn't be made </returns>
    public static CreatedSlime? Create(SlimeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsGameReady)
        {
            LogError($"Prism: Can't create slime '{request.Name}': the game isn't ready");
            return null;
        }

        var name = ContentNames.Unique(request.Name, "slime");

        var baseSlime = request.Base.ResolveOrFallback(out var usedFallback);
        if (baseSlime == null)
        {
            LogError($"Prism: Can't create slime '{name}': the game has no native slimes registered");
            return null;
        }
        if (usedFallback)
            LogWarning($"Prism: Native slime '{request.Base}' not found for '{name}', using Pink");

        var appearanceSource = request.AppearanceBase.HasValue
            ? request.AppearanceBase.Value.Find() ?? baseSlime
            : baseSlime;

        var baseAppearance = appearanceSource.AppearancesDefault.AsEnumerable().FirstOrDefault(a => a != null)
            ?? baseSlime.AppearancesDefault.AsEnumerable().FirstOrDefault(a => a != null);

        if (baseAppearance == null)
        {
            LogError($"Prism: Can't create slime '{name}': base '{baseSlime.name}' has no appearance");
            return null;
        }

        SpriteEUtil.Hold(request.Icon);

        var definition = UObject.Instantiate(baseSlime);
        definition.hideFlags = HideFlags.DontUnloadUnusedAsset;
        NativeBases[definition.Pointer] = baseSlime;
        definition.name = name;
        definition.Name = name;
        definition.localizedName = request.DisplayName;
        definition.CanLargofy = request.CanLargofy;
        PediaSaveIds.SetSuffix(definition, ContentNames.StableName(request.Name, string.Empty), name, "slime");

        var appearance = AppearanceCloner.Clone(baseAppearance, name + "Default");
        appearance._appearType = SlimeAppearance.AppearanceType.DEFAULT;
        if (request.Icon != null) appearance._icon = request.Icon;
        
        appearance._fullArt = request.Palette == null && request.Icon == null && request.FullArt == null
            ? PageArt.OwnFullArt(baseAppearance._fullArt)
            : null;
        
        definition.AppearancesDefault = new[] { appearance }.ToIl2CppArray();
        definition.RadiantBase = null;
        definition.RadiantLargo0 = null;
        definition.RadiantLargo1 = null;

        if (request.Palette.HasValue)
            SlimeColors.Apply(appearance, request.Palette.Value);

        if (request.Texture != null)
            SlimeColors.ApplyTexture(appearance, request.Texture);

        var vacColor = request.VacColor ?? request.Palette?.Middle ?? definition.color;
        vacColor.a = 1f;
        definition.color = vacColor;

        if (request.VacColor.HasValue)
        {
            var palette = appearance._colorPalette;
            palette.Ammo = vacColor;
            appearance._colorPalette = palette;
            appearance._splatColor = vacColor;
        }

        if (request.Icon != null) definition.icon = request.Icon;
        PageArt.ClearFullArt(definition);

        if (request.Palette == null && request.Icon == null && request.FullArt == null)
            PageArt.KeepFullArtFrom(definition, appearanceSource);
        
        if (request.Palette.HasValue && request.Icon == null && request.FullArt == null)
            PageArt.RegisterRecolor(definition, appearanceSource.FullArtReference ?? baseAppearance._fullArt, baseAppearance._colorPalette.Middle, request.Palette.Value.Middle);
        
        if (request.Palette.HasValue && request.Icon == null)
            appearance._icon = SpriteEUtil.Recolor(baseAppearance._icon, baseAppearance._colorPalette.Middle, request.Palette.Value.Middle) ?? appearance._icon;
        
        definition.FavoriteToyIdents = (request.FavouriteToys ?? Array.Empty<ToyDefinition>())
            .Where(toy => toy != null)
            .ToArray()
            .ToIl2CppArray();

        definition.Diet = PrismSlimeDietCreatorV02.Create(baseSlime);
        definition.prefab = BuildPrefab(name, baseSlime, definition, appearance);

        var referenceId = ContentNames.SlimeReferenceId(ContentNames.StableName(request.Name, string.Empty));

        if (!SaveRegistry.Register(definition, referenceId,
                ContentNames.LegacyReferenceId("SlimeDefinition", name, string.Empty)))
        {
            Discard(definition, appearance);
            return null;
        }

        RegisterGroups(definition, request);
        RegisterAppearance(definition, appearance);
        PageArt.Register(definition, request.FullArt);
        
        if (request.EdibleByTarrs)
            RefreshTarrDiet();

        SlimeEvents.Register(definition, request.Actions);

        if (request.Radiant != null)
            PrismRadiantCreatorV02.Create(definition, appearance, request.Radiant, name + "Radiant");

        PrismPediaCreatorV02.CreateFor(definition, request.Pedia, PediaCategoryKind.Slimes, PediaFactSetKind.Slime);

        PrismPediaCreatorV02.CreateRadiantFor(definition, request.Radiant?.Pedia);

        return new CreatedSlime(definition, appearance, referenceId);
    }

    /// <summary>
    /// Destroys a slime that couldn't be registered
    /// </summary>
    private static void Discard(SlimeDefinition definition, SlimeAppearance appearance)
    {
        NativeBases.Remove(definition.Pointer);

        try
        {
            if (definition.prefab != null) UObject.Destroy(definition.prefab);
            UObject.Destroy(appearance);
            UObject.Destroy(definition);
        }
        catch { }
    }

    /// <summary>
    /// Builds the prefab
    /// </summary>
    private static GameObject BuildPrefab(
        string name,
        SlimeDefinition baseSlime,
        SlimeDefinition definition,
        SlimeAppearance appearance)
    {
        var prefab = Prefabs.Create("slime" + name, baseSlime.prefab);

        var applicator = prefab.GetComponent<SlimeAppearanceApplicator>();
        if (applicator != null)
        {
            applicator.SlimeDefinition = definition;
            applicator.Appearance      = appearance;
        }

        var eat = prefab.GetComponent<SlimeEat>();
        if (eat != null) eat.SlimeDefinition = definition;

        var playWithToys = prefab.GetComponent<PlayWithToys>();
        if (playWithToys != null) playWithToys.SlimeDefinition = definition;

        var reactToToys = prefab.GetComponent<ReactToToyNearby>();
        if (reactToToys != null) reactToToys.SlimeDefinition = definition;

        var actor = prefab.GetComponent<IdentifiableActor>();
        if (actor != null) actor.identType = definition;

        return prefab;
    }

    /// <summary>
    /// Registers the slime into it's groups
    /// </summary>
    private static void RegisterGroups(SlimeDefinition definition, SlimeRequest request)
    {
        Groups.Add(definition, Groups.Names.AllIdentifiableTypes);
        Groups.Add(definition, Groups.Names.SmallSlime);
        Groups.Add(definition, Groups.Names.SlimesSinkInShallowWater);

        if (request.Vaccable) Groups.Add(definition, Groups.Names.VaccableBaseSlime);
        if (request.EdibleByTarrs) Groups.Add(definition, Groups.Names.EdibleSlime);
    }

    /// <summary>
    /// Registers an appearance
    /// </summary>
    private static void RegisterAppearance(SlimeDefinition definition, SlimeAppearance appearance)
    {
        var director = appearanceDirector;
        if (director == null)
        {
            LogWarning($"Prism: No appearance director yet, '{definition.name}' won't render correctly");
            return;
        }

        director.RegisterDependentAppearances(definition, appearance);
        director.UpdateChosenSlimeAppearance(definition, appearance);
    }

    /// <summary>
    /// Rebuilds the tarr slime's diets
    /// </summary>
    private static void RefreshTarrDiet()
    {
        var tarr = PrismNativeBaseSlime.Tarr.Find();
        if (tarr != null) PrismSlimeDietCreatorV02.Refresh(tarr);
    }
}