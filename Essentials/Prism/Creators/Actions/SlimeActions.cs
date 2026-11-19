using System;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Damage;

namespace Starlight.Prism.Creators.Actions;

/// <summary> A slime has appeared in the world </summary>
/// <param name="Slime"> The slime's object </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="IsNew"> True for a slime that was just spawned or just became radiant. False for one restored from its saved state, such as when a save loads </param>
/// <param name="IsRadiant"> Whether it is radiant </param>
public sealed record SlimeSpawn(GameObject Slime, SlimeDefinition Definition, bool IsNew, bool IsRadiant);

/// <summary> A slime has been killed </summary>
/// <param name="Slime"> The slime's object (removed afterwards) </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="Position"> Where it died </param>
/// <param name="IsRadiant"> Whether it was radiant </param>
/// <param name="Source"> What killed it, if known </param>
/// <param name="SourceObject"> The object that killed it, if any </param>
public sealed record SlimeDeath(
    GameObject Slime,
    SlimeDefinition Definition,
    Vector3 Position,
    bool IsRadiant,
    DamageSourceDefinition? Source,
    GameObject? SourceObject);

/// <summary> A slime has been vacuumed into the player's vac </summary>
/// <param name="Slime"> The slime's object (removed afterwards) </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="IsRadiant"> Whether it was radiant </param>
public sealed record SlimeVacIn(GameObject Slime, SlimeDefinition Definition, bool IsRadiant);

/// <summary> A slime has been shot out </summary>
/// <param name="Slime"> The slime's object </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="IsRadiant"> Whether it is radiant </param>
/// <param name="Source"> What shot it: the player's vacpack, a cannon, etc </param>
public sealed record SlimeShootOut(GameObject Slime, SlimeDefinition Definition, bool IsRadiant, Vacuumable.LaunchSource Source);

/// <summary> A largo slime the player's vacpack has taken hold of </summary>
/// <param name="Slime"> The slime's object </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="IsRadiant"> Whether it is radiant </param>
public sealed record SlimeHold(GameObject Slime, SlimeDefinition Definition, bool IsRadiant);

/// <summary> A slime has eaten something </summary>
/// <param name="Slime"> The slime's object </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="Eaten"> What it ate: food, but also a plort or a slime for the ones that eat those </param>
/// <param name="IsFavourite"> Whether it is one of the slime's favourite foods </param>
/// <param name="WasLaunched"> Whether it was shot at the slime rather than found </param>
/// <param name="IsRadiant"> Whether the slime is radiant </param>
public sealed record SlimeAte(
    GameObject Slime,
    SlimeDefinition Definition,
    IdentifiableType Eaten,
    bool IsFavourite,
    bool WasLaunched,
    bool IsRadiant);

/// <summary> A slime has produced plorts </summary>
/// <param name="Slime"> The slime's object </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="Produced"> What it produced, or null if it couldn't be told </param>
/// <param name="Count"> How many it produced </param>
/// <param name="IsRadiant"> Whether the slime is radiant </param>
public sealed record SlimeProduce(GameObject Slime, SlimeDefinition Definition, IdentifiableType? Produced, int Count, bool IsRadiant);

/// <summary> A slime has taken damage </summary>
/// <param name="Slime"> The slime's object </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="Amount"> How much damage the slime took </param>
/// <param name="HealthLeft"> How much health the slime has after it </param>
/// <param name="IsRadiant"> Whether the slime is radiant </param>
/// <param name="Source"> What damaged it, if known </param>
/// <param name="SourceObject"> The object that damaged it, if any </param>
public sealed record SlimeDamage(
    GameObject Slime,
    SlimeDefinition Definition,
    int Amount,
    int HealthLeft,
    bool IsRadiant,
    DamageSourceDefinition? Source,
    GameObject? SourceObject);

/// <summary> A slime is being removed from the world, however that happens </summary>
/// <param name="Slime"> The slime's object (removed afterwards) </param>
/// <param name="Definition"> Which slime it is; For a radiant action, the slime that is radiant: for a largo, the half that is </param>
/// <param name="IsRadiant"> Whether the slime is radiant </param>
/// <param name="Reason"> The reason why (where in its code the removal was asked for) </param>
/// <param name="Delay"> How many seconds until it is actually removed </param>
public sealed record SlimeRemove(GameObject Slime, SlimeDefinition Definition, bool IsRadiant, string Reason, float Delay);

/// <summary>
/// What a created slime does when something happens to it
/// </summary>
/// <remarks>
/// Give them to a slime with <c>SlimeRequest.Actions</c>,
/// to its radiant form with <c>RadiantRequest.Actions</c>,
/// or to a largo as an argument of <see cref="PrismLargoCreatorV02.Create"/>.
/// </remarks>
/// <example>
/// <code>
/// Actions = new SlimeActions
/// {
///     CustomEatAction = ate => Log($"{ate.Definition.name} ate {ate.Eaten.name}"),
///     CustomDeathAction = death => Log($"killed at {death.Position}")
/// }
/// </code>
/// </example>
public sealed record SlimeActions
{
    /// <summary>
    /// Runs whenever one of these slimes appears in the world: newly spawned, or loaded from its saved state
    /// </summary>
    public Action<SlimeSpawn>? CustomSpawnAction { get; init; }

    /// <summary>
    /// Runs when one of these slimes is killed, just before it is removed
    /// </summary>
    /// <remarks> Only kills count; A slime that is eaten, vacced or despawned does not count </remarks>
    public Action<SlimeDeath>? CustomDeathAction { get; init; }

    /// <summary>
    /// Runs when one of these slimes is vacuumed into the player's vac, just before it is removed from the world
    /// </summary>
    /// <remarks> Only when it fits; A vac that fails because the ammo is full does not count </remarks>
    public Action<SlimeVacIn>? CustomVacInAction { get; init; }

    /// <summary>
    /// Runs when one of these slimes is shot out, whether by the player's vacpack, a cannon or anything else
    /// </summary>
    public Action<SlimeShootOut>? CustomShootOutAction { get; init; }

    /// <summary>
    /// Runs when the player's vacpack takes hold of one of these slimes to carry at the nozzle
    /// </summary>
    public Action<SlimeHold>? CustomHoldAction { get; init; }

    /// <summary>
    /// Runs whenever one of these slimes eats something, Favourites count too
    /// </summary>
    /// <remarks> Only eats that the player can see count </remarks>
    public Action<SlimeAte>? CustomEatAction { get; init; }

    /// <summary>
    /// Runs when one of these slimes eats one of its favourite foods
    /// </summary>
    /// <remarks> Only eats that the player can see count; Runs after <see cref="CustomEatAction"/> </remarks>
    public Action<SlimeAte>? CustomFavouriteEatAction { get; init; }

    /// <summary>
    /// Runs when one of these slimes produces plorts
    /// </summary>
    /// <remarks> This includes plorts the game produces for a slime catching up after time has passed </remarks>
    public Action<SlimeProduce>? CustomProduceAction { get; init; }

    /// <summary>
    /// Runs when one of these slimes is damaged
    /// </summary>
    public Action<SlimeDamage>? CustomDamageAction { get; init; }

    /// <summary>
    /// Runs when one of these slimes is removed from the world, however that happens (killed, eaten, vacuumed, despawned or anything else)
    /// </summary>
    /// <remarks> Runs on top of the more specific actions, which report the same removal
    /// </remarks>
    public Action<SlimeRemove>? CustomRemoveAction { get; init; }
}