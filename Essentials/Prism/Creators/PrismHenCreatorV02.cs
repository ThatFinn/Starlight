using System;
using Starlight.Prism.Lib;
using Starlight.Prism.Data.Native;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create a hen
/// </summary>
public sealed record HenRequest
{
    /// <summary>
    /// The raw identifier
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The hen's display name
    /// </summary>
    public required LocalizedString DisplayName { get; init; }

    /// <summary>
    /// The hen's Slimepedia page
    /// </summary>
    /// <remarks> By default, one that says "No information" </remarks>
    public PediaInfo? Pedia { get; init; } = new();

    /// <summary>
    /// The chick's Slimepedia page
    /// </summary>
    /// <remarks> By default, one that says "No information" </remarks>
    public PediaInfo? ChickPedia { get; init; } = new();

    /// <summary>
    /// The large picture on the hen's Slimepedia page
    /// </summary>
    /// <remarks> Without one the Slimepedia page shows the icon </remarks>
    public Sprite? FullArt { get; init; }

    /// <summary>
    /// The large picture on the chick's Slimepedia page
    /// </summary>
    /// <remarks> Without one the Slimepedia page shows the icon </remarks>
    public Sprite? ChickFullArt { get; init; }

    /// <summary>
    /// Inventory icon
    /// </summary>
    /// <remarks> Null falls back to the template's icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// Hen's tint
    /// </summary>
    /// <remarks> Null keeps the template's tint </remarks>
    public Color? Color { get; init; }

    /// <summary>
    /// Custom texture over the template's material
    /// </summary>
    /// <remarks> Null keeps the template's texture </remarks>
    public Texture2D? Texture { get; init; }

    /// <summary>
    /// The chick's display name
    /// </summary>
    /// <remarks> Null keeps the hen's display name </remarks>
    public LocalizedString? ChickDisplayName { get; init; }

    /// <summary>
    /// Whether the hen is common, rare or both
    /// </summary>
    public HenRarity Rarity { get; init; } = HenRarity.Common;

    /// <summary>
    /// Whether the hen lays chicks
    /// </summary>
    /// <remarks> If you disable this, the hen's won't reproduce </remarks>
    public bool CanReproduce { get; init; } = true;

    /// <summary>
    /// What happens when something happens to one of these items
    /// </summary>
    /// <remarks> Examples: it spawns, it is vacuumed in, it is shot out </remarks>
    public ItemActions? Actions { get; init; }
}

/// <summary>
/// The game keeps common hens and rare ones in separate groups,
/// see <see cref="Groups.Names.CommonHen"/> and <see cref="Groups.Names.RareHen"/>
/// </summary>
public enum HenRarity
{
    Common,
    Rare,
    Both
}

/// <summary>
/// A hen and it's chick
/// </summary>
/// <param name="Hen"> The grown hen </param>
/// <param name="Chick"> The chick, or null if none was created </param>
public sealed record CreatedHen(CreatedItem Hen, CreatedItem? Chick);

/// <summary>
/// The chicken creator, used for hens and chicks
/// </summary>
public static class PrismHenCreatorV02
{
    /// <summary>
    /// Creates a breed of chicken from a <see cref="HenRequest"/>
    /// </summary>
    /// <param name="withChick">
    /// Whether to generate the chick;
    /// If false the hen produces the base game's chick if not disabled (<see cref="HenRequest.CanReproduce"/>)
    /// </param>
    public static CreatedHen? Create(HenRequest request, bool withChick)
    {
        ArgumentNullException.ThrowIfNull(request);

        var henTemplate = NativeChicken.CommonHen();
        if (henTemplate == null)
        {
            LogError($"Prism: Can't create '{request.Name}': the game has no hen to copy");
            return null;
        }

        var hen = PrismItemCreatorV02.Create(new ItemRequest
        {
            Pedia        = request.Pedia,
            FullArt      = request.FullArt,
            PediaFactSet = PediaFactSetKind.Food,
            Name         = request.Name,
            DisplayName  = request.DisplayName,
            Template     = henTemplate,
            Kind         = "Hen",
            Icon         = request.Icon,
            Color        = request.Color,
            Texture      = request.Texture,
            Groups       = HenGroups(request.Rarity),
            Components   = request.CanReproduce ? Array.Empty<ComponentEdit>() : new[] { ComponentEdit.Remove<Reproduce>() },
            Actions      = request.Actions
        });

        if (hen == null) return null;

        hen.Type.IsAnimal = true;

        var chick = withChick ? CreateChick(request, hen) : null;
        if (chick != null) chick.Type.IsAnimal = true;
        
        if (chick != null)
        {
            PointAt<Reproduce>(hen.Type.prefab, produce => produce.ChildPrefab = chick.Type.prefab);
            PointAt<TransformAfterTime>(chick.Type.prefab, grow => GrowInto(grow, hen.Type.prefab));

            // A chick has a chance of growing into a roostro instead
            PointAt<TransformChanceOnReproduce>(chick.Type.prefab, chance => chance.TargetPrefab = hen.Type.prefab);
        }

        return new CreatedHen(hen, chick);
    }

    /// <summary>
    /// Every group a grown hen of this rarity belongs to
    /// </summary>
    private static string[] HenGroups(HenRarity rarity) => rarity switch
    {
        HenRarity.Rare => new[] { Groups.Names.RareHen, Groups.Names.Meat, Groups.Names.Chickens },
        HenRarity.Both => new[] { Groups.Names.CommonHen, Groups.Names.RareHen, Groups.Names.Meat, Groups.Names.Chickens },
        _              => new[] { Groups.Names.CommonHen, Groups.Names.Meat, Groups.Names.Chickens }
    };

    /// <summary>
    /// Creates the chick of a hen
    /// </summary>
    private static CreatedItem? CreateChick(HenRequest request, CreatedItem hen)
    {
        var chickTemplate = NativeChicken.Chick();
        if (chickTemplate == null)
        {
            LogWarning($"Prism: '{hen.ObjectName}' has no chick of its own: the game has no chick to copy");
            return null;
        }

        return PrismItemCreatorV02.Create(new ItemRequest
        {
            Pedia        = request.ChickPedia,
            FullArt      = request.ChickFullArt,
            PediaFactSet = PediaFactSetKind.Food,
            Name         = request.Name,
            DisplayName  = request.ChickDisplayName ?? request.DisplayName,
            Template     = chickTemplate,
            Kind         = "Chickadoo",
            Color        = request.Color,
            Groups       = new[] { Groups.Names.Chick, Groups.Names.Chickens }
        });
    }

    /// <summary>
    /// Points at the hen the chick should grow into
    /// </summary>
    private static void GrowInto(TransformAfterTime grow, GameObject target)
    {
        if (grow.Options == null) return;

        foreach (var option in grow.Options)
            if (option != null) option.TargetPrefab = target;
    }

    /// <summary>
    /// Reconfigures a component on a prefab
    /// </summary>
    private static void PointAt<T>(GameObject? prefab, Action<T> configure) where T : Component
    {
        if (prefab == null) return;

        var component = prefab.GetComponent<T>();
        if (component != null) configure(component);
    }
}