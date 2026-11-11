using System;

namespace Starlight.Prism.Data.Spawning;

/// <summary>
/// Maps <see cref="PrismSpawnLocations"/> values to the zone names the game uses
/// </summary>
public static class SpawnLocations
{
    public static string ZoneName(this PrismSpawnLocations location) => location switch
    {
        PrismSpawnLocations.RainbowFields => "zoneFields",
        PrismSpawnLocations.StarlightStand => "zoneStrand",
        PrismSpawnLocations.EmberValley => "zoneGorge",
        PrismSpawnLocations.PowderfallBluffs => "zoneBluffs",
        PrismSpawnLocations.LabyrinthHub => "Hub",
        PrismSpawnLocations.LabyrinthWaterworks => "LabStrand",
        PrismSpawnLocations.LabyrinthLavadepths => "LabValley",
        PrismSpawnLocations.LabyrinthDreamland => "Dreamland",
        PrismSpawnLocations.LabyrinthTerrarium => "zoneLabyrinthTerrarium",
        _ => string.Empty
    };

    /// <summary>
    /// True if <paramref name="zoneName"/> belongs to any of <paramref name="locations"/>
    /// </summary>
    public static bool Matches(string zoneName, IReadOnlyCollection<PrismSpawnLocations> locations)
    {
        if (string.IsNullOrEmpty(zoneName)) return false;

        foreach (var location in locations)
        {
            var fragment = location.ZoneName();
            if (fragment.Length > 0 && zoneName.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

/// <summary>
/// When during the day something spawns
/// </summary>
public enum SpawnTime
{
    Always,
    Day,
    Night,
}