using System;
using System.Linq;

namespace Starlight.Prism.Data.Spawning;

/// <summary>
/// A registered request to make something spawn in the world
/// </summary>
/// <param name="Type"> What should spawn </param>
/// <param name="Locations"> Where it should spawn </param>
/// <param name="Weight"> How often it is picked relative to everything else at that spawner </param>
/// <param name="Time"> When during the day it can appear </param>
internal readonly record struct SpawnRule(
    IdentifiableType Type,
    IReadOnlyCollection<PrismSpawnLocations> Locations,
    float Weight,
    SpawnTime Time);

/// <summary>
/// Makes created content spawn in the world
/// </summary>
public static class SpawnRegistry
{
    private static readonly List<SpawnRule> Rules = new();

    /// <summary> Registers content to spawn in the given locations </summary>
    /// <param name="type"> What should spawn </param>
    /// <param name="locations"> Where it should spawn </param>
    /// <param name="weight"> Relative frequency against everything else at the same spawner </param>
    /// <param name="time"> Time of day it should spawn at </param>
    public static void MakeSpawnable(
        IdentifiableType type,
        IReadOnlyCollection<PrismSpawnLocations> locations,
        float weight = 0.2f,
        SpawnTime time = SpawnTime.Always)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (locations.Count == 0) return;
        Rules.Add(new SpawnRule(type, locations, weight, time));
    }

    /// <summary>
    /// Applies every registered spawn rule to a spawner
    /// </summary>
    internal static void ApplyTo(DirectedActorSpawner spawner)
    {
        if (spawner == null) return;
        
        if (spawner.TryCast<DirectedSlimeSpawner>() == null) return;

        var sceneName = spawner.gameObject.scene.name;

        foreach (var rule in Rules)
        {
            if (!SpawnLocations.Matches(sceneName, rule.Locations)) continue;

            try
            {
                Inject(spawner, rule);
            }
            catch (Exception ex)
            {
                LogError($"Prism: Failed to add '{rule.Type.name}' to a spawner in '{sceneName}'", ex);
            }
        }
    }

    /// <summary>
    /// Injects a spawn rule to a game's spawner
    /// </summary>
    private static void Inject(DirectedActorSpawner spawner, SpawnRule rule)
    {
        if (AlreadyPresent(spawner, rule.Type)) return;

        var constraint = new DirectedActorSpawner.SpawnConstraint
        {
            Window = new DirectedActorSpawner.TimeWindow { TimeMode = TimeModeFor(rule.Time) },
            Feral  = false,
            Weight = rule.Weight,
            Slimeset = new SlimeSet
            {
                Members = Array.Empty<SlimeSet.Member>().ToIl2CppArray()
            }
        };

        constraint.Slimeset.Members = constraint.Slimeset.Members.AddToNew(new SlimeSet.Member
        {
            _prefab   = rule.Type.prefab,
            IdentType = rule.Type,
            Weight    = rule.Weight
        });

        spawner.Constraints = spawner.Constraints.AddToNew(constraint);
    }

    /// <summary>
    /// Whether this spawner already lists the type
    /// </summary>
    private static bool AlreadyPresent(DirectedActorSpawner spawner, IdentifiableType type)
    {
        foreach (var constraint in spawner.Constraints.AsEnumerable())
        {
            foreach (var member in constraint?.Slimeset?.Members.AsEnumerable() ?? Enumerable.Empty<SlimeSet.Member>())
            {
                if (member?.IdentType == null) continue;
                if (member.IdentType.Pointer == type.Pointer) return true;
            }
        }
        return false;
    }

    private static DirectedActorSpawner.TimeMode TimeModeFor(SpawnTime time) => time switch
    {
        SpawnTime.Day => DirectedActorSpawner.TimeMode.DAY,
        SpawnTime.Night => DirectedActorSpawner.TimeMode.NIGHT,
        _ => DirectedActorSpawner.TimeMode.ANY
    };
}