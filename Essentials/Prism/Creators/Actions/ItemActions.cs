using System;
using Il2CppMonomiPark.SlimeRancher;

namespace Starlight.Prism.Creators.Actions;

/// <summary> An item that has appeared in the world. </summary>
/// <param name="Item"> The item's object. </param>
/// <param name="Type"> Which item it is. </param>
/// <param name="IsNew"> True for one that was just spawned. False for one restored from its saved state, such as when a save loads. </param>
public sealed record ItemSpawn(GameObject Item, IdentifiableType Type, bool IsNew);

/// <summary> An item that has been vacuumed into the player's ammo. </summary>
/// <param name="Item"> The item's object. (Still intact, since it is removed afterward) </param>
/// <param name="Type"> Which item it is. </param>
public sealed record ItemVacIn(GameObject Item, IdentifiableType Type);

/// <summary> An item that has been shot out. </summary>
/// <param name="Item"> The item's object. </param>
/// <param name="Type"> Which item it is. </param>
/// <param name="Source"> What shot it: the vacpack, a cannon, etc. </param>
public sealed record ItemShootOut(GameObject Item, IdentifiableType Type, Vacuumable.LaunchSource Source);

/// <summary> Items that have been sold on the plort market. </summary>
/// <param name="Type"> Which item was sold. </param>
/// <param name="Count"> How many were sold in this sale. </param>
/// <param name="Price"> What the sale was paid. </param>
public sealed record ItemSell(IdentifiableType Type, int Count, int Price);

/// <summary> An item that has been eaten by a slime. </summary>
/// <param name="Type"> Which item was eaten. </param>
/// <param name="Eater"> The slime's object. </param>
/// <param name="EaterDefinition"> Which slime ate it, or null if it couldn't be told. </param>
/// <param name="IsFavourite"> Whether it is one of that slime's favourite foods. </param>
/// <param name="WasLaunched"> Whether it was shot at the slime rather than found. </param>
public sealed record ItemEaten(IdentifiableType Type, GameObject Eater, SlimeDefinition? EaterDefinition, bool IsFavourite, bool WasLaunched);

/// <summary> An item that is being removed from the world. (destroyed) </summary>
/// <param name="Item"> The item's object. (Still intact, since it is removed afterward) </param>
/// <param name="Type"> Which item it is. </param>
/// <param name="Reason"> The reason label for the destruction. </param>
/// <param name="Delay"> How many seconds until it actually gets removed. Zero = instantly. </param>
public sealed record ItemRemove(GameObject Item, IdentifiableType Type, string Reason, float Delay);

/// <summary>
/// What a created item does when something happens to it.
/// </summary>
/// <remarks>
/// Give them to an item with the <c>Actions</c> of its request:
/// <c>ItemRequest</c>, <c>PlortRequest</c>, <c>FoodRequest</c>, <c>HenRequest</c> or <c>ToyRequest</c>.
/// </remarks>
/// <example>
/// <code>
/// Actions = new ItemActions
/// {
///     CustomSellAction = sell => Log($"sold {sell.Count} for {sell.Price}"),
///     CustomEatenAction = eaten => Log($"{eaten.EaterDefinition?.name} ate it")
/// }
/// </code>
/// </example>
public sealed record ItemActions
{
    /// <summary>
    /// Runs whenever one of these items appears in the world: newly spawned, or restored from its saved state
    /// </summary>
    public Action<ItemSpawn>? CustomSpawnAction { get; init; }

    /// <summary>
    /// Runs when one of these items is vacuumed into the player's ammo (before it is removed from the world)
    /// </summary>
    /// <remarks> Only when it is added; A vac that fails because the ammo is full does not count </remarks>
    public Action<ItemVacIn>? CustomVacInAction { get; init; }

    /// <summary>
    /// Runs when one of these items is shot out, whether by the player's vacpack, a cannon or anything else
    /// </summary>
    public Action<ItemShootOut>? CustomShootOutAction { get; init; }

    /// <summary>
    /// Runs when this item is sold on the plort market
    /// </summary>
    public Action<ItemSell>? CustomSellAction { get; init; }

    /// <summary>
    /// Runs when a slime eats one of these items. The event says which slime and whether it was a favourite food
    /// </summary>
    public Action<ItemEaten>? CustomEatenAction { get; init; }

    /// <summary>
    /// Runs when one of these items is removed from the world, however that happens
    /// </summary>
    /// <remarks> Runs on top of the more specific actions, which report the same removal </remarks>
    public Action<ItemRemove>? CustomRemoveAction { get; init; }
}