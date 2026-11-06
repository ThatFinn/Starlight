using System;
using Il2CppMonomiPark.SlimeRancher;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Lib;

/// <summary>
/// Item Events
/// </summary>
internal static class ItemEvents
{
    private static readonly Dictionary<IntPtr, ItemActions> Registered = new();

    internal static void Register(IdentifiableType type, ItemActions? actions)
    {
        if (actions == null) return;

        Registered[type.Pointer] = actions;
    }

    /// <summary>
    /// Whether any created item asked for an action
    /// </summary>
    internal static bool AnyAction => Registered.Count != 0;

    /// <summary>
    /// Tells an item's creator that a slime has eaten one
    /// </summary>
    internal static void NotifyEaten(IdentifiableType? eaten, GameObject? eater, bool isFavourite, bool wasLaunched)
    {
        if (Registered.Count == 0 || eaten == null || eater == null) return;
        if (!Registered.TryGetValue(eaten.Pointer, out var actions) || actions.CustomEatenAction == null) return;

        var actor = eater.GetComponent<IdentifiableActor>();
        var definition = actor == null ? null : actor.identType?.TryCast<SlimeDefinition>();

        actions.CustomEatenAction(new ItemEaten(eaten, eater, definition, isFavourite, wasLaunched));
    }

    /// <summary>
    /// Tells an item's creator that one is being removed from the world
    /// </summary>
    internal static void NotifyRemoved(GameObject? removed, string reason, float delay)
    {
        if (Registered.Count == 0) return;
        if (!TryGet(removed, out var type, out var actions) || actions.CustomRemoveAction == null) return;

        actions.CustomRemoveAction(new ItemRemove(removed!, type, reason, delay));
    }

    /// <summary>
    /// Tells an item's creator that one has appeared
    /// </summary>
    internal static void NotifySpawn(GameObject? spawned, bool isNew)
    {
        if (Registered.Count == 0) return;
        if (!TryGet(spawned, out var type, out var actions) || actions.CustomSpawnAction == null) return;

        actions.CustomSpawnAction(new ItemSpawn(spawned!, type, isNew));
    }

    /// <summary>
    /// Tells an item's creator that one has been vacuumed into the player's ammo
    /// </summary>
    internal static void NotifyVacIn(GameObject? vacuumed)
    {
        if (Registered.Count == 0) return;
        if (!TryGet(vacuumed, out var type, out var actions) || actions.CustomVacInAction == null) return;

        actions.CustomVacInAction(new ItemVacIn(vacuumed!, type));
    }

    /// <summary>
    /// Tells an item's creator that one has been shot out
    /// </summary>
    internal static void NotifyShootOut(GameObject? shot, Vacuumable.LaunchSource source)
    {
        if (Registered.Count == 0) return;
        if (!TryGet(shot, out var type, out var actions) || actions.CustomShootOutAction == null) return;

        actions.CustomShootOutAction(new ItemShootOut(shot!, type, source));
    }

    /// <summary>
    /// Tells an item's creator that some has been sold on the plort market
    /// </summary>
    internal static void NotifySell(IdentifiableType? sold, int count, int price)
    {
        if (Registered.Count == 0 || sold == null) return;
        if (!Registered.TryGetValue(sold.Pointer, out var actions) || actions.CustomSellAction == null) return;

        actions.CustomSellAction(new ItemSell(sold, count, price));
    }

    /// <summary>
    /// Finds the actions asked for by the item
    /// </summary>
    /// <returns> False if it isn't one that asked for any </returns>
    private static bool TryGet(GameObject? item, out IdentifiableType type, out ItemActions actions)
    {
        type = null!;
        actions = null!;

        if (item == null) return false;

        var actor = item.GetComponent<IdentifiableActor>();
        var found = actor == null ? null : actor.identType;
        if (found == null || !Registered.TryGetValue(found.Pointer, out var registered)) return false;

        type = found;
        actions = registered;
        return true;
    }
}