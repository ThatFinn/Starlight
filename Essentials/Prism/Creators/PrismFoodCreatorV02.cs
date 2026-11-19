using System;
using Starlight.Prism.Lib;
using Starlight.Prism.Data.Native;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// What kind of food an item is
/// </summary>
public enum FoodKind
{
    /// <summary>
    /// Grows on trees
    /// </summary>
    /// <remarks> Eaten by slimes that eat fruit </remarks>
    Fruit,

    /// <summary>
    /// Grows in the ground
    /// </summary>
    /// <remarks> Eaten by slimes that eat vegetables </remarks>
    Veggie,

    /// <summary>
    /// Meat that isn't a chicken;
    /// It has no native default and needs a <see cref="FoodRequest.Template"/>
    /// </summary>
    /// <remarks> Chickens are made with <see cref="PrismHenCreatorV02"/> </remarks>
    Meat,

    /// <summary>
    /// Food of no kind;
    /// It needs a <see cref="FoodRequest.Template"/>
    /// </summary>
    /// <remarks> No slime eats it until it is added to a diet with <see cref="PrismSlimeDietCreatorV02.AddEntry"/> </remarks>
    Other,
}

/// <summary>
/// A request to create a food item
/// </summary>
public sealed record FoodRequest
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
    /// Which food group this belongs to
    /// </summary>
    public FoodKind Kind { get; init; } = FoodKind.Fruit;

    /// <summary>
    /// Which native fruit to copy for structures when <see cref="Kind"/> is fruit
    /// </summary>
    /// <remarks> Ignored when <see cref="Template"/> is set </remarks>
    public NativeFruit FruitBase { get; init; } = NativeFruits.DefaultTemplate;

    /// <summary>
    /// Which native veggie to copy for structures when <see cref="Kind"/> is veggie
    /// </summary>
    /// <remarks> Ignored when <see cref="Template"/> is set </remarks>
    public NativeVeggie VeggieBase { get; init; } = NativeVeggies.DefaultTemplate;

    /// <summary>
    /// Explicit template, overriding <see cref="FruitBase"/> and <see cref="VeggieBase"/>
    /// </summary>
    public IdentifiableType? Template { get; init; }

    /// <summary>
    /// Whether the food can be planted in a garden
    /// </summary>
    /// <remarks> Only fruit and veggies can grow </remarks>
    public bool MakePlantable { get; init; } = true;

    /// <summary>
    /// Inventory icon
    /// </summary>
    /// <remarks> Null falls back to the template's icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// Custom model
    /// </summary>
    /// <remarks> Null keeps the template's model </remarks>
    public Mesh? Mesh { get; init; }

    /// <summary>
    /// Custom artwork applied over the native material
    /// </summary>
    /// <remarks> Null keeps the template's texture </remarks>
    public Texture2D? Texture { get; init; }

    /// <summary>
    /// Tint over the native model
    /// </summary>
    /// <remarks> Null keeps the template's color </remarks>
    public Color? Color { get; init; }

    /// <summary>
    /// Extra component changes applied to the food's prefab
    /// </summary>
    public IReadOnlyList<ComponentEdit> Components { get; init; } = Array.Empty<ComponentEdit>();

    /// <summary>
    /// What happens when something happens to one of these items
    /// </summary>
    /// <remarks> Examples: it spawns, it is vacuumed in, it is shot out </remarks>
    public ItemActions? Actions { get; init; }
}

/// <summary>
/// The food creator, used for fruit & veggies that can grow as well as other food that cannot grow
/// </summary>
public static class PrismFoodCreatorV02
{
    /// <summary>
    /// Food group, diet group, cloud group
    /// </summary>
    private static readonly (FoodKind Kind, string Diet, string Cloud)[] KindGroups =
    {
        (FoodKind.Fruit,  Groups.Names.Fruit,  Groups.Names.FruitQuantumCloud),
        (FoodKind.Veggie, Groups.Names.Veggie, Groups.Names.VeggieQuantumCloud),
        (FoodKind.Meat,   Groups.Names.Meat,   Groups.Names.MeatQuantumCloud),
    };

    /// <summary>
    /// Creates a food item from a <see cref="FoodRequest"/>
    /// </summary>
    public static CreatedItem? Create(FoodRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var crop = request.Kind is FoodKind.Fruit or FoodKind.Veggie;

        var template = request.Template ?? request.Kind switch
        {
            FoodKind.Fruit  => NativeFruits.ResolveTemplate(request.FruitBase),
            FoodKind.Veggie => NativeVeggies.ResolveTemplate(request.VeggieBase),
            _               => null
        };
        if (template == null)
        {
            LogError(crop
                ? $"Prism: Can't create food '{request.Name}': no native {KindName(request.Kind).ToLowerInvariant()} to copy"
                : $"Prism: Can't create food '{request.Name}': kind {request.Kind} has no native default, so it needs a Template");
            return null;
        }

        var created = PrismItemCreatorV02.Create(new ItemRequest
        {
            Pedia        = request.Pedia,
            FullArt      = request.FullArt,
            PediaFactSet = PediaFactSetKind.Food,
            Name         = request.Name,
            DisplayName  = request.DisplayName,
            Template     = template,
            Kind         = KindName(request.Kind),
            Icon         = request.Icon,
            Mesh         = request.Mesh,
            Texture      = request.Texture,
            Color        = request.Color,
            Components   = request.Components,
            Actions      = request.Actions,
            Groups       = DietGroup(request.Kind) is { } diet
                     ? new[] { Groups.Names.Food, diet }
                     : new[] { Groups.Names.Food }
        });

        if (created == null) return null;
        
        ApplyKindGroups(created.Type, request.Kind);
        
        PrismSlimeDietCreatorV02.RefreshAllSlimes();
        
        if (crop && request.MakePlantable) PrismGardenCreatorV02.MakePlantable(created.Type, template.ReferenceId);

        return created;
    }

    /// <summary>
    /// The suffix of a food's object name
    /// </summary>
    private static string KindName(FoodKind kind) => kind switch
    {
        FoodKind.Fruit  => "Fruit",
        FoodKind.Veggie => "Veggie",
        FoodKind.Meat   => "Meat",
        _               => "Food"
    };

    /// <summary>
    /// The group slime diets go by for a kind
    /// </summary>
    private static string? DietGroup(FoodKind kind)
    {
        foreach (var (owner, diet, _) in KindGroups)
            if (owner == kind) return diet;

        return null;
    }

    /// <summary>
    /// Puts food in its own kind's groups and out of every other kind's
    /// </summary>
    private static void ApplyKindGroups(IdentifiableType food, FoodKind kind)
    {
        foreach (var (owner, diet, cloud) in KindGroups)
        {
            if (owner == kind) Groups.AddMultiple(food, diet, cloud);
            else               Groups.RemoveAll(food, diet, cloud);
        }
    }
}