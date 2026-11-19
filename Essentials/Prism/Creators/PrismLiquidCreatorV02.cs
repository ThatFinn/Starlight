using System;
using UObject = UnityEngine.Object;
using Il2CppMonomiPark.SlimeRancher;
using Starlight.Prism.Lib;
using Starlight.Prism.Data.Native;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// The colours of the liquid when shot out
/// </summary>
/// <param name="Center"> The middle of the droplet </param>
/// <param name="Base"> The base of the droplet </param>
/// <param name="Edge"> The edge of the droplet </param>
/// <param name="Emission"> The glow of the droplet </param>
public readonly record struct LiquidColors(Color Center, Color Base, Color Edge, Color Emission)
{
    /// <summary>
    /// Converts a <see cref="Color"/> to <see cref"LiquidColors"/>, adjusting the values a bit
    /// </summary>
    public static LiquidColors From(Color color) =>
        new(color * 0.3882f, new Color(color.r, color.g, color.b, 0f), color, color);
}

/// <summary>
/// A request to create a liquid
/// </summary>
public sealed record LiquidRequest
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
    /// Its Slimepedia page
    /// </summary>
    /// <remarks> By default, one that says "No information" </remarks>
    public PediaInfo? Pedia { get; init; } = new();

    /// <summary>
    /// The large picture on its Slimepedia page
    /// </summary>
    /// <remarks> Without one the page shows the icon </remarks>
    public Sprite? FullArt { get; init; }

    /// <summary>
    /// The liquid to copy for structure
    /// </summary>
    /// <remarks> Null copies water </remarks>
    public LiquidDefinition? Template { get; init; }

    /// <summary>
    /// The tank icon
    /// </summary>
    /// <remarks> Null falls back to the template's icon, recoloured to the <see cref="VacColor"/> </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// The colour in the vac tank
    /// </summary>
    /// <remarks> Null keeps the template's color </remarks>
    public Color? VacColor { get; init; }

    /// <summary>
    /// The colours of the liquid
    /// </summary>
    /// <remarks> Null falls back to <see cref="VacColor"/> with <see cref="LiquidColors.From"/> </remarks>
    public LiquidColors? Colors { get; init; }

    /// <summary>
    /// Whether the game treats it as water
    /// </summary>
    /// <remarks>
    /// Most of what reacts to being shot with liquid in the game only reacts to water
    /// </remarks>
    public bool IsWater { get; init; } = true;

    /// <summary>
    /// The effect played at the vacpack while the liquid is vacuumed in
    /// </summary>
    /// <remarks> Null keeps the template's effect </remarks>
    public GameObject? VacInFx { get; init; }

    /// <summary>
    /// The effect played at the vacpack when the liquid can't be vacuumed in, with a full tank
    /// </summary>
    /// <remarks> Null keeps the template's effect </remarks>
    public GameObject? VacFailFx { get; init; }

    /// <summary>
    /// Extra component changes applied to the droplet's prefab
    /// </summary>
    public IReadOnlyList<ComponentEdit> Components { get; init; } = Array.Empty<ComponentEdit>();

    /// <summary>
    /// What happens when something happens to one of its droplets
    /// </summary>
    /// <remarks> The remove action runs when a droplet lands </remarks>
    public ItemActions? Actions { get; init; }
}

/// <summary>
/// A liquid created by Prism
/// </summary>
/// <param name="Definition"> The liquid definition </param>
/// <param name="ReferenceId"> Its persistence id </param>
/// <param name="ObjectName"> Its object name </param>
public sealed record CreatedLiquid(LiquidDefinition Definition, string ReferenceId, string ObjectName);

/// <summary>
/// The liquid creator, used to create liquids
/// </summary>
/// <remarks>
/// Creating a liquid won't make it obtainable, you still have to add it somewhere
/// </remarks>
public static class PrismLiquidCreatorV02
{
    /// <summary>
    /// The suffix every liquid's name carries
    /// </summary>
    private const string Kind = "Liquid";

    private static readonly int ColorBase     = Shader.PropertyToID("_ColorBase");
    private static readonly int ColorCenter   = Shader.PropertyToID("_ColorCenter");
    private static readonly int ColorEdge     = Shader.PropertyToID("_ColorEdge");
    private static readonly int RimTint       = Shader.PropertyToID("_RimTint");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    /// <summary>
    /// Creates a liquid from a <see cref="LiquidRequest"/>
    /// </summary>
    /// <returns> The created liquid, or null </returns>
    public static CreatedLiquid? Create(LiquidRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = request.Template ?? NativeLiquids.Water();
        if (template == null)
        {
            LogError($"Prism: Can't create liquid '{request.Name}': the game has no template liquid");
            return null;
        }

        var colors = request.Colors ?? (request.VacColor.HasValue ? LiquidColors.From(request.VacColor.Value) : (LiquidColors?)null);

        var created = PrismItemCreatorV02.Create(new ItemRequest
        {
            Pedia        = request.Pedia,
            FullArt      = request.FullArt,
            PediaFactSet = PediaFactSetKind.Resource,
            Name         = request.Name,
            DisplayName  = request.DisplayName,
            Template     = template,
            Kind         = Kind,
            Icon         = request.Icon,
            Color        = request.VacColor ?? colors?.Edge,
            Components   = request.Components,
            Actions      = request.Actions,
            Groups       = new[] { Groups.Names.Liquid },
            CreateType   = () => ScriptableObject.CreateInstance<LiquidDefinition>()
        });

        if (created == null) return null;

        var liquid = created.Type.TryCast<LiquidDefinition>();
        if (liquid == null)
        {
            LogError($"Prism: '{created.ObjectName}' wasn't created as a liquid definition, so the vacpack can't hold it");
            return null;
        }

        liquid._isWater   = request.IsWater;
        liquid._inFX      = request.VacInFx ?? template._inFX;
        liquid._vacFailFX = request.VacFailFx ?? template._vacFailFX;
        
        if (request.IsWater) Groups.Add(liquid, Groups.Names.Water);
        else                 Groups.Remove(liquid, Groups.Names.Water);

        ApplyOnLanding(liquid);
        if (colors.HasValue) Recolour(liquid.prefab, colors.Value);

        SaveRegistry.Register(liquid, created.ReferenceId,
            ContentNames.LegacyReferenceId("LiquidDefinition", ContentNames.StableName(request.Name, Kind), Kind));

        return new CreatedLiquid(liquid, created.ReferenceId, created.ObjectName);
    }

    /// <summary>
    /// Makes the droplets apply this liquid where they land
    /// </summary>
    private static void ApplyOnLanding(LiquidDefinition liquid)
    {
        foreach (var landing in liquid.prefab.GetComponentsInChildren<DestroyOnTouching>(includeInactive: true))
            if (landing.LiquidIdentifiableType != null) landing.LiquidIdentifiableType = liquid;
    }

    /// <summary>
    /// Recolours a liquid to have the correct colors
    /// </summary>
    private static void Recolour(GameObject prefab, LiquidColors colors)
    {
        foreach (var swap in prefab.GetComponentsInChildren<WaterIdentifiableVFXSwap>(includeInactive: true))
        {
            var regular = swap._regularWater;
            var prisma  = swap._prismaWater;

            if (regular != null)
            {
                regular.SetActive(true);

                if (regular.name == "Sphere_Regular") regular.name = "Sphere";
            }

            if (prisma != null) UObject.DestroyImmediate(prisma);
            UObject.DestroyImmediate(swap);
        }

        foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
        {
            if (renderer.sharedMaterial == null) continue;
            
            var material = UObject.Instantiate(renderer.sharedMaterial);
            renderer.sharedMaterial = material;

            if (!material.HasProperty(ColorBase)) continue;

            material.SetColor(ColorBase, colors.Base);
            material.SetColor(ColorCenter, colors.Center);
            material.SetColor(ColorEdge, colors.Edge);
            material.SetColor(RimTint, colors.Edge);
            material.SetColor(EmissionColor, colors.Emission);
        }
    }
}