using System;
using System.Linq;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Lib;

/// <summary>
/// Gordo Slime Events
/// </summary>
internal static class GordoEvents
{
    private static readonly Dictionary<IntPtr, GordoActions> Registered = new();

    internal static void Register(IdentifiableType type, GordoActions? actions)
    {
        if (actions == null) return;

        Registered[type.Pointer] = actions;
    }

    /// <summary>
    /// Tells a gordo slime's creator that it has been fed
    /// </summary>
    internal static void NotifyFeed(GordoEat gordo, GameObject? food)
    {
        if (Registered.Count == 0) return;
        if (!TryGet(gordo, out var type, out var slime, out var actions)) return;

        var foodType = food == null ? null : food.GetComponent<IdentifiableActor>()?.identType;
        var isFavourite = foodType != null && IsFavourite(slime, foodType);

        var feed = new GordoFeed(gordo.gameObject, type, slime, foodType, isFavourite);

        actions.CustomFeedAction?.Invoke(feed);

        if (isFavourite)
            actions.CustomFavouriteFeedAction?.Invoke(feed);
    }

    /// <summary>
    /// Tells a gordo slime's creator that it is bursting
    /// </summary>
    internal static void NotifyBurst(GordoEat gordo)
    {
        if (Registered.Count == 0) return;
        if (!TryGet(gordo, out var type, out var slime, out var actions) || actions.CustomBurstAction == null) return;

        actions.CustomBurstAction(new GordoBurst(gordo.gameObject, type, slime, gordo.transform.position));
    }
    
    private static bool IsFavourite(SlimeDefinition slime, IdentifiableType food)
    {
        var favourites = slime.Diet?.FavoriteIdents;
        return favourites != null && favourites.AsEnumerable().Any(favourite => favourite != null && favourite.Pointer == food.Pointer);
    }

    /// <summary>
    /// Finds the actions asked for by the gordo slime
    /// </summary>
    /// <returns> False if it isn't a created gordo slime that asked for any action </returns>
    private static bool TryGet(GordoEat gordo, out IdentifiableType type, out SlimeDefinition slime, out GordoActions actions)
    {
        type = null!;
        slime = null!;
        actions = null!;

        var identifiable = gordo.GetComponent<GordoIdentifiable>();
        var found = identifiable == null ? null : identifiable.identType;
        if (found == null || !Registered.TryGetValue(found.Pointer, out var registered)) return false;
        if (gordo.SlimeDefinition == null) return false;

        type = found;
        slime = gordo.SlimeDefinition;
        actions = registered;
        return true;
    }
}