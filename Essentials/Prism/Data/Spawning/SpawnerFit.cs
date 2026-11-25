namespace Starlight.Prism.Data.Spawning;

/// <summary>
/// Decides whether added content belongs in a spawner, and at which times of day
/// </summary>
internal static class SpawnerFit
{
    /// <summary>
    /// Whether <paramref name="type"/> can be added to this spawner as a new spawn
    /// </summary>
    /// <remarks>
    /// Radiant-only spawners turn whatever they pick radiant, gold spawners only hold Gold and Lucky slimes
    /// and feral spawners only hold feral largos
    /// </remarks>
    internal static bool Accepts(DirectedActorSpawner spawner, IdentifiableType type)
    {
        if (spawner == null || spawner._onlySpawnRadiant || spawner.Constraints == null) return false;

        var isLargo = type.TryCast<SlimeDefinition>()?.IsLargo ?? false;

        foreach (var constraint in spawner.Constraints)
        {
            if (constraint?.Slimeset?.Members == null) continue;
            if (constraint.Feral && !isLargo) continue;

            foreach (var member in constraint.Slimeset.Members)
                if (member?.IdentType != null && !IsGoldOrLucky(member.IdentType))
                    return true;
        }
        return false;
    }

    /// <summary>
    /// The wanted time narrowed to the times the spawner already spawns at, or null if they never overlap
    /// </summary>
    internal static PrismSpawnerActiveTime? FitTime(DirectedActorSpawner spawner, PrismSpawnerActiveTime wanted) =>
        Fit(spawner, wanted != PrismSpawnerActiveTime.DuringNight, wanted != PrismSpawnerActiveTime.DuringDay) switch
        {
            DirectedActorSpawner.TimeMode.ANY   => PrismSpawnerActiveTime.AllTheTime,
            DirectedActorSpawner.TimeMode.DAY   => PrismSpawnerActiveTime.DuringDay,
            DirectedActorSpawner.TimeMode.NIGHT => PrismSpawnerActiveTime.DuringNight,
            _                                   => null
        };

    /// <inheritdoc cref="FitTime(DirectedActorSpawner, PrismSpawnerActiveTime)"/>
    internal static SpawnTime? FitTime(DirectedActorSpawner spawner, SpawnTime wanted) =>
        Fit(spawner, wanted != SpawnTime.Night, wanted != SpawnTime.Day) switch
        {
            DirectedActorSpawner.TimeMode.ANY   => SpawnTime.Always,
            DirectedActorSpawner.TimeMode.DAY   => SpawnTime.Day,
            DirectedActorSpawner.TimeMode.NIGHT => SpawnTime.Night,
            _                                   => null
        };

    private static DirectedActorSpawner.TimeMode? Fit(DirectedActorSpawner spawner, bool day, bool night)
    {
        var spawnsByDay   = false;
        var spawnsByNight = false;

        foreach (var constraint in spawner.Constraints)
        {
            if (constraint?.Slimeset?.Members == null) continue;

            var mode = constraint.Window?.TimeMode ?? DirectedActorSpawner.TimeMode.ANY;
            spawnsByDay   |= mode != DirectedActorSpawner.TimeMode.NIGHT;
            spawnsByNight |= mode != DirectedActorSpawner.TimeMode.DAY;
        }

        day   &= spawnsByDay;
        night &= spawnsByNight;

        if (day && night) return DirectedActorSpawner.TimeMode.ANY;
        if (day) return DirectedActorSpawner.TimeMode.DAY;
        if (night) return DirectedActorSpawner.TimeMode.NIGHT;
        return null;
    }

    private static bool IsGoldOrLucky(IdentifiableType type) => type.name is "Gold" or "Lucky";
}