using System;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Lib;
using Starlight.Prism.Data.Native;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create a plort
/// </summary>
public sealed record PlortRequest
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
    /// Which native plort to copy for its model and behaviour
    /// </summary>
    public PrismNativePlort Base { get; init; } = PrismNativePlort.Pink;

    /// <summary>
    /// Whether slimes and largos can eat it
    /// </summary>
    public bool Edible { get; init; } = true;

    /// <summary>
    /// Inventory icon
    /// </summary>
    /// <remarks> Null falls back to the base plort's icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// Tint of the plort
    /// </summary>
    /// <remarks> Null keeps the base plort's color </remarks>
    public Color? Color { get; init; }

    /// <summary>
    /// Custom model
    /// </summary>
    /// <remarks> Null keeps the base plort's model </remarks>
    public Mesh? Mesh { get; init; }

    /// <summary>
    /// Custom texture applied over the native material
    /// </summary>
    /// <remarks> Null keeps the base plort's texture </remarks>
    public Texture2D? Texture { get; init; }

    /// <summary>
    /// Extra component changes applied to the plort's prefab
    /// </summary>
    public IReadOnlyList<ComponentEdit> Components { get; init; } = Array.Empty<ComponentEdit>();

    /// <summary>
    /// Its Slimepedia page
    /// </summary>
    /// <remarks> None by default, since the game gives plorts no page of their own </remarks>
    public PediaInfo? Pedia { get; init; }

    /// <summary>
    /// The large picture on its Slimepedia page
    /// </summary>
    /// <remarks> Without one the page shows the icon </remarks>
    public Sprite? FullArt { get; init; }

    /// <summary>
    /// What happens when something happens to one of these items
    /// </summary>
    /// <remarks> Examples: it spawns, it is vacuumed in, it is shot out, it is sold </remarks>
    public ItemActions? Actions { get; init; }
}

/// <summary>
/// The plort creator, used to create plorts
/// </summary>
public static class PrismPlortCreatorV02
{
    /// <summary>
    /// Creates a plort from a <see cref="PlortRequest"/>
    /// </summary>
    /// <returns> The plort, or null if it couldn't be made </returns>
    public static CreatedItem? Create(PlortRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = request.Base.Find() ?? PrismNativePlort.Pink.Find();
        if (template == null)
        {
            LogError($"Prism: Can't create plort '{request.Name}': no template plort could be found ");
            return null;
        }

        var created = PrismItemCreatorV02.Create(new ItemRequest
        {
            Name         = request.Name,
            DisplayName  = request.DisplayName,
            Template     = template,
            Kind         = "Plort",
            Icon         = request.Icon,
            Color        = request.Color,
            Mesh         = request.Mesh,
            Texture      = request.Texture,
            Pedia        = request.Pedia,
            FullArt      = request.FullArt,
            PediaFactSet = PediaFactSetKind.Resource,
            Components   = request.Components,
            Actions      = request.Actions,
            Groups       = request.Edible
                ? new[] { Groups.Names.Plort, Groups.Names.EdiblePlortFood, Groups.Names.PlortDroneExplorer }
                : new[] { Groups.Names.Plort, Groups.Names.PlortDroneExplorer },
        });

        if (created == null) return null;

        created.Type.IsPlort = true;

        if (!request.Edible) Groups.Remove(created.Type, Groups.Names.EdiblePlortFood);

        if (request.Color.HasValue && request.Texture == null)
            Tint(created.Type.prefab, request.Color.Value);

        return created;
    }
    
    // Partial leftovers from SRCL below
    
    /// <summary>
    /// "Accent words"
    /// </summary>
    private static readonly string[] AccentWords = { "outline", "rim", "fresnel", "glow", "emission", "emissive", "crack", "infill", "pulse" };

    /// <summary>
    /// Tints every accent of the material
    /// </summary>
    /// <returns> How many color slots were set </returns>
    private static int TintAccents(Material material, Color color)
    {
        var shader = material.shader;
        if (shader == null) return 0;

        var written = 0;

        for (var i = 0; i < shader.GetPropertyCount(); i++)
        {
            if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Color) continue;

            var name = shader.GetPropertyName(i);
            var accent = false;
            foreach (var word in AccentWords) accent |= name.Contains(word, StringComparison.OrdinalIgnoreCase);
            if (!accent) continue;
            
            var old = material.GetColor(name);
            var strength = Mathf.Max(1f, Mathf.Max(old.r, Mathf.Max(old.g, old.b)));
            material.SetColor(name, new Color(color.r * strength, color.g * strength, color.b * strength, old.a));
            written++;
        }

        return written;
    }

    /// <summary>
    /// Whether the material paints the plort's "body material"
    /// </summary>
    private static bool IsPlortSurface(Material material) =>
        material.shader != null && material.shader.name.Contains("Plort", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Sets every color slot of an effect material to the tint
    /// </summary>
    /// <returns> How many color slots were set </returns>
    private static int TintEveryColour(Material material, Color color)
    {
        var shader = material.shader;
        if (shader == null) return 0;

        var written = 0;

        for (var i = 0; i < shader.GetPropertyCount(); i++)
        {
            if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Color) continue;

            var name = shader.GetPropertyName(i);
            var old = material.GetColor(name);
            var strength = Mathf.Max(1f, Mathf.Max(old.r, Mathf.Max(old.g, old.b)));
            material.SetColor(name, new Color(color.r * strength, color.g * strength, color.b * strength, old.a));
            written++;
        }

        return written;
    }

    /// <summary>
    /// Recolours the plort
    /// </summary>
    private static void Tint(GameObject prefab, Color color)
    {
        var shaded = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f, color.a);
        var palette = new SlimePalette(color, color, shaded);

        foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
        {
            var materials = renderer.materials;
            foreach (var material in materials)
            {
                if (material == null) continue;
                if (material.shader != null && material.shader.name.Contains("Attachment", StringComparison.OrdinalIgnoreCase)) continue;

                var gradient = material.HasProperty("_TopColor") || material.HasProperty("_RedTopColor") || material.HasProperty("_TwinTopColor");
                var effect = !gradient && !IsPlortSurface(material);

                var written = gradient ? SlimeColors.Apply(material, palette) : SlimeColors.Tint(material, color);

                written += TintAccents(material, color);
                if (effect) written += TintEveryColour(material, color);

                if (written == 0)
                    LogWarning($"Prism: The tint of '{prefab.name}' did nothing to a material using shader '{material.shader?.name}': it has none of the colour slots Prism knows");
            }
        }

        // Sparkles take their color from the particle system
        foreach (var particles in prefab.GetComponentsInChildren<ParticleSystem>(includeInactive: true))
        {
            var main = particles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
        }
    }
}