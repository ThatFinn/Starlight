using System;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Lib;
using Starlight.Prism.Data.Native;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create a slime toy
/// </summary>
public record ToyRequest
{
    /// <summary>
    /// The name of the toy
    /// </summary>
    /// <remarks> Raw identifier </remarks>
    public required string Name { get; init; }

    /// <summary>
    /// The display name
    /// </summary>
    public required LocalizedString DisplayName { get; init; }

    /// <summary>
    /// The native toy to copy
    /// </summary>
    /// <remarks> Null = any </remarks>
    public IdentifiableType? Template { get; init; }

    /// <summary>
    /// The inventory icon
    /// </summary>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// The color in the vac & inventory
    /// </summary>
    /// <remarks> Only shows if you can somehow vac in the toy </remarks>
    public Color? Color { get; init; }

    /// <summary>
    /// A custom model
    /// </summary>
    /// <remarks> Null = template's model </remarks>
    public Mesh? Mesh { get; init; }

    /// <summary>
    /// A custom texture
    /// </summary>
    /// <remarks> Null = template's texture </remarks>
    public Texture2D? Texture { get; init; }

    /// <summary>
    /// How much calmer a slime gets from having this toy nearby
    /// </summary>
    /// <remarks> Defaults to the game's value </remarks>
    public float BaseAgitationReduction { get; init; } = 0.15f;

    /// <summary>
    /// How much calmer a slime gets when this is its favourite toy
    /// </summary>
    public float FavouriteAgitationReduction { get; init; } = 0.3f;

    /// <summary>
    /// Its Slimepedia page
    /// </summary>
    /// <remarks> By default one that says "No information"; null makes none </remarks>
    public PediaInfo? Pedia { get; init; } = new();

    /// <summary>
    /// The large picture on its Slimepedia page
    /// </summary>
    public Sprite? FullArt { get; init; }

    /// <summary>
    /// What happens when something happens to one of these toys
    /// </summary>
    /// <remarks>
    /// This is only really accessible if you use toys differently than intended for or by mods;
    /// Examples: it spawns, is vacuumed in, is shot out or sold
    /// </remarks>
    public ItemActions? Actions { get; init; }

    /// <summary>
    /// What happens when something happens to that toy
    /// </summary>
    /// <remarks> Examples: The vacpack takes hold of one </remarks>
    public ToyActions? ToyActions { get; init; }
}

/// <summary>
/// The toy creator, used to create slime toys
/// </summary>
public static class PrismToyCreatorV02
{
    /// <summary>
    /// Creates a toy from a <see cref="ToyRequest"/>
    /// </summary>
    public static CreatedItem? Create(ToyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = request.Template ?? NativeToys.Template();
        if (template == null)
        {
            LogError($"Prism: Can't create toy '{request.Name}': the game has template toy");
            return null;
        }

        var created = PrismItemCreatorV02.Create(new ItemRequest
        {
            Name          = request.Name,
            DisplayName   = request.DisplayName,
            Template      = template,
            Kind          = Kind,
            Icon          = request.Icon,
            Color         = request.Color,
            Mesh          = request.Mesh,
            Texture       = request.Texture,
            Pedia         = request.Pedia,
            PediaCategory = PediaCategoryKind.Toys,
            FullArt       = request.FullArt,
            CreateType    = () => ScriptableObject.CreateInstance<ToyDefinition>(),
            Actions       = request.Actions
        });

        if (created == null) return null;

        var toy = created.Type.TryCast<ToyDefinition>();
        if (toy == null)
        {
            LogError($"Prism: '{created.ObjectName}' wasn't created as a toy definition, so slimes won't be able to play with it");
            return null;
        }

        toy.BaseAgitationReductionFactor = request.BaseAgitationReduction;
        toy.FavoriteAgitationReductionFactor = request.FavouriteAgitationReduction;

        Register(toy, PrismPediaCreatorV02.EntryFor(toy), created.ObjectName);
        ToyEvents.Register(toy, request.ToyActions);

        return created;
    }

    /// <summary>
    /// The suffix every toy's name has
    /// </summary>
    private const string Kind = "Toy";

    /// <summary>
    /// Registers a toy
    /// </summary>
    private static void Register(ToyDefinition toy, PediaEntry? pediaEntry, string objectName)
    {
        var director = GameContext.Instance?.LookupDirector;
        if (director == null)
        {
            LogWarning($"Prism: '{objectName}' isn't listed as a toy: no lookup director yet");
            return;
        }

        try
        {
            if (pediaEntry != null)
            {
                director.AddToy(toy, pediaEntry);
                return;
            }

            var group = director._toyDefinitions;
            if (group == null)
            {
                LogWarning($"Prism: '{objectName}' isn't listed as a toy: no pedia entry and no toy group");
                return;
            }

            director.AddIdentifiableTypeToGroup(toy, group);
            LogWarning($"Prism: '{objectName}' was listed as a toy without a Slimepedia entry");
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to register toy '{objectName}'", ex);
        }
    }
}