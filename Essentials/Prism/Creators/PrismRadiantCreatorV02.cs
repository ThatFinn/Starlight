using System;
using System.Linq;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Lib;
using Starlight.Prism.Patches;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create a slime's radiant Slimepedia entry
/// </summary>
public sealed record RadiantRequest
{
    /// <summary>
    /// The color palette of the radiant slime
    /// </summary>
    public required SlimePalette Palette { get; init; }

    /// <summary>
    /// The Slimepedia page
    /// </summary>
    /// <remarks> By default, one that says "No information" </remarks>
    public PediaInfo? Pedia { get; init; } = new();

    /// <summary>
    /// The large picture on its Slimepedia page
    /// </summary>
    /// <remarks> Without one the page shows the icon </remarks>
    public Sprite? FullArt { get; init; }

    /// <summary>
    /// The Inventory and Slimepedia icon
    /// </summary>
    /// <remarks> Null falls back to the slime's icon, recolored to the palette </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// The color inside the Vac and the splat on the ground
    /// </summary>
    /// <remarks> Null uses the palette's middle tone </remarks>
    public Color? VacColor { get; init; }

    /// <summary>
    /// Which native slime's radiant appearance to copy
    /// </summary>
    public PrismNativeBaseSlime? AppearanceBase { get; init; }

    /// <summary>
    /// Whether the radiant form glows like native radiant slimes
    /// </summary>
    public bool Glowing { get; init; } = true;

    /// <summary>
    /// A custom slime face
    /// </summary>
    /// <remarks> Null keeps the original face </remarks>
    public FaceLook? Face { get; init; }

    /// <summary>
    /// Custom slime body structures
    /// </summary>
    public IReadOnlyList<SlimeStructureRequest> CustomStructures { get; init; } = Array.Empty<SlimeStructureRequest>();

    /// <summary>
    /// Which native slime to copy body structures from
    /// </summary>
    public PrismNativeBaseSlime? CopyStructuresFrom { get; init; }

    /// <summary>
    /// Whether the slime can spawn as radiant
    /// </summary>
    /// <remarks> If false it is only radiant when something else spawns it as radiant </remarks>
    public bool CanSpawnRadiant { get; init; } = true;

    /// <summary>
    /// How often the slime spawns as radiant compared to others
    /// </summary>
    public int RadiantRotationWeight { get; init; } = RadiantRotation.DefaultWeight;

    /// <summary>
    /// Component changes of the radiant form of this slime
    /// </summary>
    public IReadOnlyList<ComponentEdit> Components { get; init; } = Array.Empty<ComponentEdit>();

    /// <summary>
    /// What happens when something happens to a radiant of this slime
    /// </summary>
    /// <remarks>
    /// Examples: it spawns, it is vacuumed in or shot out
    /// </remarks>
    public SlimeActions? Actions { get; init; }
}

/// <summary>
/// The radiant creator, used to create radiant forms of slimes
/// </summary>
public static class PrismRadiantCreatorV02
{
    /// <summary>
    /// Gives a slime a radiant form from a <see cref="RadiantRequest"/>
    /// </summary>
    /// <param name="definition"> The slime </param>
    /// <param name="baseAppearance"> The look to derive the radiant form from </param>
    /// <param name="palette"> The body colors of the radiant form </param>
    /// <param name="name"> The object name of the radiant appearance </param>
    /// <returns> The created appearance, or null if the slime already has a radiant form (or none exist) </returns>
    public static SlimeAppearance? Create(
        SlimeDefinition definition,
        SlimeAppearance baseAppearance,
        RadiantRequest request,
        string name)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(baseAppearance);
        ArgumentNullException.ThrowIfNull(request);

        if (!SupportRadiant.HasFlag()) return null;
        
        if (definition.RadiantBase != null) return null;

        var source = request.AppearanceBase is { } native ? native.Find()?.RadiantBase : null;
        if (request.AppearanceBase != null && source == null)
            LogWarning($"Prism: '{name}': {request.AppearanceBase} has no template radiant appearance, using the slime's own");

        SpriteEUtil.Hold(request.Icon);

        var variant = AppearanceCloner.Clone(source ?? baseAppearance, name);
        variant._appearType = SlimeAppearance.AppearanceType.RADIANT_BASE;
        UseRadiantStoneForm(variant, RadiantBaseStoneForm);
        variant._icon = request.Icon ?? SpriteEUtil.Recolor(source?._icon ?? baseAppearance._icon, (source ?? baseAppearance)._colorPalette.Middle, request.Palette.Middle) ?? baseAppearance._icon;
        
        variant._fullArt = null;
        
        SlimeColors.Apply(variant, request.Palette);
        ApplyVacColor(variant, request.VacColor ?? request.Palette.Middle);

        Attempt(name, "slime's body structures", () => ApplyStructures(variant, name, request));
        Attempt(name, "slime's face",            () => ApplyFace(variant, request.Face));
        Attempt(name, "radiant glow",            () => ApplyGlow(variant, request.Glowing, request.Palette.Middle));

        definition.RadiantBase = variant;
        PageArt.RegisterRadiant(definition, request.FullArt);
        definition.AppearancesDefault = definition.AppearancesDefault.AddToNew(variant);

        Register(definition, variant, baseAppearance);

        RadiantRotation.Register(definition, request.RadiantRotationWeight, request.CanSpawnRadiant);
        SlimeEvents.RegisterRadiant(definition, request.Components, request.Actions);

        return variant;
    }

    /// <summary>
    /// Gives a largo the radiant forms its halves allow
    /// </summary>
    /// <returns> How many radiant forms the largo got </returns>
    internal static int CreateLargoForms(SlimeDefinition largo, SlimeDefinition first, SlimeDefinition second)
    {
        if (!SupportRadiant.HasFlag()) return 0;

        var defaultAppearance = largo.AppearancesDefault.AsEnumerable().FirstOrDefault(appearance => appearance != null);
        var firstDefault = first.AppearancesDefault.AsEnumerable().FirstOrDefault(appearance => appearance != null);
        var secondDefault = second.AppearancesDefault.AsEnumerable().FirstOrDefault(appearance => appearance != null);
        if (defaultAppearance == null || firstDefault == null || secondDefault == null) return 0;

        var added = 0;
        
        if (first.RadiantBase != null)
        {
            largo.RadiantLargo0 = BuildLargoForm(
                largo, defaultAppearance,
                SlimeColors.Blend(first.RadiantBase._colorPalette, secondDefault._colorPalette),
                RadiantGlow.Has(first.RadiantBase),
                SlimeAppearance.AppearanceType.RADIANT_LARGO_0, largo.name + "Radiant0");
            added++;
        }

        if (second.RadiantBase != null)
        {
            largo.RadiantLargo1 = BuildLargoForm(
                largo, defaultAppearance,
                SlimeColors.Blend(firstDefault._colorPalette, second.RadiantBase._colorPalette),
                RadiantGlow.Has(second.RadiantBase),
                SlimeAppearance.AppearanceType.RADIANT_LARGO_1, largo.name + "Radiant1");
            added++;
        }

        return added;
    }

    /// <summary>
    /// Builds one radiant form of a largo and registers it
    /// </summary>
    private static SlimeAppearance BuildLargoForm(
        SlimeDefinition largo,
        SlimeAppearance defaultAppearance,
        SlimePalette palette,
        bool glowing,
        SlimeAppearance.AppearanceType type,
        string name)
    {
        var form = AppearanceCloner.Clone(defaultAppearance, name);
        form._appearType = type;
        form._icon       = defaultAppearance._icon;
        form._fullArt    = null;

        UseRadiantStoneForm(form, RadiantLargoStoneForm);
        SlimeColors.Apply(form, palette);
        Attempt(name, "radiant glow", () => ApplyGlow(form, glowing, palette.Middle));

        largo.AppearancesDefault = largo.AppearancesDefault.AddToNew(form);
        Register(largo, form, defaultAppearance);

        return form;
    }

    private const string RadiantBaseStoneForm = "StoneFormRadiantBase";
    
    private const string RadiantLargoStoneForm = "StoneFormRadiantLargo";
    
    /// <summary>
    /// Applies the correct Statue form to the slime, so radiant ringtail slimes show the radiancy on the statue
    /// </summary>
    private static void UseRadiantStoneForm(SlimeAppearance form, string stoneFormName)
    {
        if (form._stoneFormAppearance == null) return;
        
        var radiant = Get<StoneFormAppearance>(stoneFormName);
        if (radiant == null)
        {
            LogWarning($"Prism: '{form.name}': the radiant statue '{stoneFormName}' is not loaded, so it shows a non-radiant statue");
            return;
        }
        
        form._stoneFormAppearance = radiant;
    }
    
    /// <summary>
    /// Sets the color of the vac and splat of an appearance
    /// </summary>
    private static void ApplyVacColor(SlimeAppearance variant, Color color)
    {
        color.a = 1f;

        var palette = variant._colorPalette;
        variant._colorPalette = new SlimeAppearance.Palette
        {
            Top    = palette.Top,
            Middle = palette.Middle,
            Bottom = palette.Bottom,
            Ammo   = color
        };
        variant._splatColor = color;
    }

    /// <summary>
    /// Adds body structures
    /// </summary>
    private static void ApplyStructures(SlimeAppearance variant, string name, RadiantRequest request)
    {
        if (request.CopyStructuresFrom is { } from)
        {
            var donorAppearance = from.Find()?.AppearancesDefault.AsEnumerable().FirstOrDefault(appearance => appearance != null);
            if (donorAppearance != null) SlimeStructures.Borrow(variant, donorAppearance);
            else LogWarning($"Prism: '{name}': {from} has no appearance to take body parts from");
        }

        foreach (var piece in request.CustomStructures)
            SlimeStructures.Attach(variant, name, piece);
    }

    /// <summary>
    /// Changes the face of a slime
    /// </summary>
    private static void ApplyFace(SlimeAppearance variant, FaceLook? face)
    {
        if (face == null) return;

        if (face.CopyFrom is { } from)
        {
            var donorAppearance = from.Find()?.AppearancesDefault.AsEnumerable().FirstOrDefault(appearance => appearance != null);
            if (!SlimeFaces.CopyFaceFrom(variant, donorAppearance))
                LogWarning($"Prism: '{variant.name}': {from} has no face to copy");
        }

        if (face.Colors is { } colors)
            SlimeFaces.Recolor(variant, colors.EyeRed, colors.EyeGreen, colors.EyeBlue, colors.Mouth);

        if (face.Overrides.Count > 0)
            SlimeFaces.Apply(variant, face.Overrides);
    }

    /// <summary>
    /// Gives the radiant form the glow of the native radiant slimes, or removes it
    /// </summary>
    private static void ApplyGlow(SlimeAppearance variant, bool glowing, Color lightColor)
    {
        if (!glowing)
        {
            RadiantGlow.Remove(variant);
            return;
        }

        if (!RadiantGlow.Add(variant, lightColor))
            LogWarning($"Prism: '{variant.name}': the native radiant to copy the glow from is not loaded, so it doesn't glow");
    }

    private static void Attempt(string name, string part, Action step)
    {
        try
        {
            step();
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to apply the {part} of '{name}'", ex);
        }
    }

    /// <summary>
    /// Registers an appearance
    /// </summary>
    private static void Register(
        SlimeDefinition definition,
        SlimeAppearance variant,
        SlimeAppearance defaultAppearance)
    {
        var director = appearanceDirector;
        if (director == null)
        {
            LogWarning($"Prism: No appearance director, '{variant.name}' won't exist");
            return;
        }

        director.RegisterDependentAppearances(definition, variant);
        director.UpdateChosenSlimeAppearance(definition, variant);

        director.UpdateChosenSlimeAppearance(definition, defaultAppearance);
    }
}