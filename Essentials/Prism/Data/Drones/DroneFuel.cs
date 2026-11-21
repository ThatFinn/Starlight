using System;
using Il2CppMonomiPark.SlimeRancher.Drone;
using Starlight.Prism.Data.Native;
using Starlight.Storage;

namespace Starlight.Prism.Data.Drones;

/// <summary>
/// How much a plort charges a drone station
/// </summary>
public sealed record DroneFuel
{
    /// <summary>
    /// The charge while the station is low
    /// </summary>
    public float Energy { get; init; }

    /// <summary>
    /// Up to how full the station can be for the plort to give its full charge
    /// </summary>
    public float FullUntil { get; init; } = 100f;

    /// <summary>
    /// From how full the station is on that the plort only gives its reduced charge
    /// </summary>
    public float ReducedFrom { get; init; } = 300f;

    /// <summary>
    /// The charge once the station holds <see cref="ReducedFrom"/> or more
    /// </summary>
    public float ReducedEnergy { get; init; } = 2f;

    /// <summary>
    /// The native plort to copy the charge of
    /// </summary>
    /// <remarks> This overwrites the other values </remarks>
    internal PrismNativePlort? Source { get; private init; }

    /// <summary>
    /// Charges exactly like a native plort
    /// </summary>
    public static DroneFuel Like(PrismNativePlort plort) => new() { Source = plort };
}

/// <summary>
/// Adds the charge of created plorts into the game's drone fuel table
/// </summary>
internal static class DroneFuelTable
{
    private const string MapName = "DroneFuelEnergyMap";
    private const float StationEnergyEnd = 1000f;

    private static readonly Dictionary<IdentifiableType, DroneFuel> fuels = new();

    /// <summary>
    /// Registers the charge of a plort
    /// </summary>
    internal static void Register(IdentifiableType plort, DroneFuel fuel)
    {
        ArgumentNullException.ThrowIfNull(plort);
        ArgumentNullException.ThrowIfNull(fuel);

        fuels[plort] = fuel;
        Apply(Get<DroneFuelEnergyMap>(MapName));
    }

    /// <summary>
    /// Applies the charges to the energy map
    /// </summary>
    internal static void Apply(DroneFuelEnergyMap? map)
    {
        if (map?.Map == null || fuels.Count == 0) return;

        foreach (var (plort, fuel) in fuels)
        {
            if (plort == null) continue;

            var curve = CurveFor(map, fuel);
            if (curve == null)
            {
                LogWarning($"Prism: '{plort.name}' copies the drone charge of a plort that is not in the fuel table, so it charges like a pink plort");
                continue;
            }

            var entry = EntryFor(map, plort);
            if (entry != null)
            {
                entry.EnergyCurve = curve;
                continue;
            }

            map.Map.Add(new DroneFuelEnergyMap.FuelEnergyEntry
            {
                FuelType    = plort,
                EnergyCurve = curve,
            });
        }
    }

    private static AnimationCurve? CurveFor(DroneFuelEnergyMap map, DroneFuel fuel)
    {
        if (fuel.Source is { } source)
        {
            var native = source.Find();
            return native == null ? null : EntryFor(map, native)?.EnergyCurve;
        }

        var fullUntil   = Math.Max(0f, fuel.FullUntil);
        var reducedFrom = Math.Max(fullUntil + 1f, fuel.ReducedFrom);

        return new AnimationCurve(new[]
        {
            new Keyframe(0f,               fuel.Energy),
            new Keyframe(fullUntil,        fuel.Energy),
            new Keyframe(reducedFrom,      fuel.ReducedEnergy),
            new Keyframe(StationEnergyEnd, fuel.ReducedEnergy),
        });
    }

    private static DroneFuelEnergyMap.FuelEnergyEntry? EntryFor(DroneFuelEnergyMap map, IdentifiableType plort)
    {
        foreach (var entry in map.Map)
            if (entry?.FuelType != null && entry.FuelType.Pointer == plort.Pointer)
                return entry;

        return null;
    }
}

[PrismPatch]
[HarmonyPatch(typeof(DroneFuelDepositor), nameof(DroneFuelDepositor.Awake))]
internal static class DroneFuelDepositorAwakePatch
{
    public static void Postfix(DroneFuelDepositor __instance)
    {
        try
        {
            DroneFuelTable.Apply(__instance._fuelEnergyMap);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add the drone charge of created plorts", ex);
        }
    }
}
