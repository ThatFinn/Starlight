using System;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Damage;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Lib;

/// <summary>
/// What a created slime does when something happens to it
/// </summary>
internal static class SlimeEvents
{
    /// <summary>
    /// The actions of each created slime
    /// </summary>
    private static readonly Dictionary<IntPtr, SlimeActions> Registered = new();

    /// <summary>
    /// The actions that only a created slime's radiant form has
    /// </summary>
    private static readonly Dictionary<IntPtr, SlimeActions> RegisteredRadiant = new();

    /// <summary>
    /// The components a created slime's radiant form is given
    /// </summary>
    private static readonly Dictionary<IntPtr, IReadOnlyList<ComponentEdit>> RadiantComponents = new();

    /// <summary>
    /// The radiants already announced
    /// </summary>
    private static readonly HashSet<int> AnnouncedRadiants = new();

    /// <summary>
    /// Whether any created slime asked for an action
    /// </summary>
    internal static bool AnyAction => Registered.Count != 0 || RegisteredRadiant.Count != 0;

    internal static void Register(SlimeDefinition definition, SlimeActions? actions)
    {
        if (actions == null) return;

        Registered[definition.Pointer] = actions;
    }

    internal static void RegisterRadiant(SlimeDefinition definition, IReadOnlyList<ComponentEdit> components, SlimeActions? actions)
    {
        if (actions != null)
            RegisteredRadiant[definition.Pointer] = actions;

        if (components.Count > 0)
            RadiantComponents[definition.Pointer] = components;
    }

    /// <summary>
    /// Tells a slime's creator that one has appeared
    /// </summary>
    internal static void NotifySpawn(GameObject? spawned, bool isNew)
    {
        if (!AnyAction) return;

        Dispatch(spawned, actions => actions.CustomSpawnAction,
            (definition, isRadiant) => new SlimeSpawn(spawned!, definition, isNew, isRadiant),
            includeRadiant: false);
    }

    /// <summary>
    /// Tells a slime's creator that one has been killed
    /// </summary>
    internal static void NotifyDeath(GameObject? killed, Damage? damage)
    {
        if (!AnyAction) return;

        Dispatch(killed, actions => actions.CustomDeathAction,
            (definition, isRadiant) => new SlimeDeath(
                killed!, definition, killed!.transform.position, isRadiant, damage?.DamageSource, damage?.SourceObject));
    }

    /// <summary>
    /// Tells a slime's creator that one has been vacuumed
    /// </summary>
    internal static void NotifyVacIn(GameObject? vacuumed)
    {
        if (!AnyAction) return;

        Dispatch(vacuumed, actions => actions.CustomVacInAction,
            (definition, isRadiant) => new SlimeVacIn(vacuumed!, definition, isRadiant));
    }

    /// <summary>
    /// Tells a slime's creator that one has been shot out
    /// </summary>
    internal static void NotifyShootOut(GameObject? shot, Vacuumable.LaunchSource source)
    {
        if (!AnyAction) return;

        Dispatch(shot, actions => actions.CustomShootOutAction,
            (definition, isRadiant) => new SlimeShootOut(shot!, definition, isRadiant, source));
    }

    /// <summary>
    /// Tells a slime's creator that a largo slime is held
    /// </summary>
    internal static void NotifyHold(GameObject? held)
    {
        if (!AnyAction || !IsSlime(held)) return;

        Dispatch(held, actions => actions.CustomHoldAction,
            (definition, isRadiant) => new SlimeHold(held!, definition, isRadiant));
    }

    /// <summary>
    /// Whether an object is a slime
    /// </summary>
    private static bool IsSlime(GameObject? candidate)
    {
        if (candidate == null) return false;

        var actor = candidate.GetComponent<IdentifiableActor>();
        var type = actor == null ? null : actor.identType;

        return type != null && SlimeDefinition.IsSlimeDefinition(type);
    }

    /// <summary>
    /// Tells a slime's creator that one has eaten
    /// </summary>
    internal static void NotifyAte(GameObject? eater, IdentifiableType eaten, bool isFavourite, bool wasLaunched)
    {
        if (!AnyAction) return;

        SlimeAte Make(SlimeDefinition definition, bool isRadiant) =>
            new(eater!, definition, eaten, isFavourite, wasLaunched, isRadiant);

        Dispatch(eater, actions => actions.CustomEatAction, Make);

        if (isFavourite)
            Dispatch(eater, actions => actions.CustomFavouriteEatAction, Make);
    }

    /// <summary>
    /// Tells a slime's creator that one has produced plorts
    /// </summary>
    internal static void NotifyProduce(GameObject? producer, IdentifiableType? produced, int count)
    {
        if (!AnyAction) return;

        Dispatch(producer, actions => actions.CustomProduceAction,
            (definition, isRadiant) => new SlimeProduce(producer!, definition, produced, count, isRadiant));
    }

    /// <summary>
    /// Tells a slime's creator that one has been damaged
    /// </summary>
    internal static void NotifyDamaged(GameObject? hit, Damage damage, int healthLeft)
    {
        if (!AnyAction) return;

        Dispatch(hit, actions => actions.CustomDamageAction,
            (definition, isRadiant) => new SlimeDamage(
                hit!, definition, damage.Amount, healthLeft, isRadiant, damage.DamageSource, damage.SourceObject));
    }

    /// <summary>
    /// Tells a slime's creator that one is being removed from the world
    /// </summary>
    internal static void NotifyRemoved(GameObject? removed, string reason, float delay)
    {
        if (!AnyAction) return;

        Dispatch(removed, actions => actions.CustomRemoveAction,
            (definition, isRadiant) => new SlimeRemove(removed!, definition, isRadiant, reason, delay));
    }

    /// <summary>
    /// Gives a radiant slime the components its slime asked for
    /// </summary>
    internal static void ApplyRadiantComponents(SlimeRadiant radiant)
    {
        if (RadiantComponents.Count == 0 || !radiant.IsRadiant) return;

        var radiantBase = radiant.RadiantBaseType;
        if (radiantBase == null || !RadiantComponents.TryGetValue(radiantBase.Pointer, out var components)) return;

        ComponentEdit.ApplyEdits(radiant.gameObject, components);
    }

    /// <summary>
    /// Tells a radiant slime's creator that it has appeared
    /// </summary>
    /// <param name="isNew"> Whether this is the slime becoming radiant, as against it loading already radiant </param>
    internal static void NotifyRadiantSpawn(SlimeRadiant radiant, bool isNew)
    {
        if (!TryGetRadiant(radiant, out var radiantBase, out var actions) || actions.CustomSpawnAction == null) return;

        if (!AnnouncedRadiants.Add(radiant.gameObject.GetInstanceID())) return;

        actions.CustomSpawnAction(new SlimeSpawn(radiant.gameObject, radiantBase, isNew, true));
    }

    /// <summary>
    /// Runs the action a slime's creator gave for an event
    /// </summary>
    private static void Dispatch<TEvent>(
        GameObject? slime,
        Func<SlimeActions, Action<TEvent>?> pick,
        Func<SlimeDefinition, bool, TEvent> make,
        bool includeRadiant = true)
    {
        if (slime == null) return;

        var actor = slime.GetComponent<IdentifiableActor>();
        var definition = actor == null ? null : actor.identType?.TryCast<SlimeDefinition>();
        if (definition == null) return;

        var radiant = slime.GetComponent<SlimeRadiant>();
        var isRadiant = radiant != null && radiant.IsRadiant;

        if (Registered.TryGetValue(definition.Pointer, out var actions) && pick(actions) is { } action)
            action(make(definition, isRadiant));

        if (!includeRadiant || !isRadiant) return;
        if (!TryGetRadiant(radiant!, out var radiantBase, out var radiantActions)) return;

        if (pick(radiantActions) is { } radiantAction)
            radiantAction(make(radiantBase, true));
    }

    /// <summary>
    /// Finds the radiant actions asked for by the slime
    /// </summary>
    /// <returns> False if it isn't radiant or asked for no action </returns>
    private static bool TryGetRadiant(SlimeRadiant radiant, out SlimeDefinition radiantBase, out SlimeActions actions)
    {
        radiantBase = null!;
        actions = null!;

        if (RegisteredRadiant.Count == 0 || !radiant.IsRadiant) return false;

        var found = radiant.RadiantBaseType;
        if (found == null || !RegisteredRadiant.TryGetValue(found.Pointer, out var registered)) return false;

        radiantBase = found;
        actions = registered;
        return true;
    }
}