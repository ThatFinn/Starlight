using System;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

/// <summary>
/// This is used to unlock plorts on the plort market once the slime has been discovered
/// </summary>
[PrismPatch()]
[HarmonyPatch(typeof(PlortRelationships), nameof(PlortRelationships.TryGetSlimeByPlort))]
internal static class PlortSlimeLinkPatch
{
    // Plorts no slime produces
    private static readonly Dictionary<IntPtr, int> Unproduced = new();

    // Plorts whose lookup threw
    private static readonly HashSet<IntPtr> Failed = new();

    private static bool filling;

    public static void Prefix(PlortRelationships __instance, IdentifiableType plort)
    {
        if (filling || __instance == null || plort == null || !plort.IsPlort || Failed.Contains(plort.Pointer)) return;

        try
        {
            var map = __instance._slimeDefinitionsByPlort;
            if (map == null) return;

            if (map.Count == 0)
            {
                filling = true;
                try { __instance.TryGetSlimeByPlort(plort, out _); }
                finally { filling = false; }
            }

            if (map.Count == 0 || map.ContainsKey(plort)) return;

            var slimes = GameContext.Instance?.SlimeDefinitions?.Slimes;
            if (slimes == null) return;

            if (Unproduced.TryGetValue(plort.Pointer, out var checkedAgainst) && checkedAgainst == slimes.Length) return;

            var producer = FindProducer(slimes, plort);
            if (producer == null)
            {
                Unproduced[plort.Pointer] = slimes.Length;
                return;
            }

            Unproduced.Remove(plort.Pointer);
            map.Add(plort, producer);
        }
        catch (Exception ex)
        {
            Failed.Add(plort.Pointer);
            LogError($"Prism: Failed to link '{plort.name}' to its slime", ex);
        }
    }

    private static SlimeDefinition? FindProducer(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SlimeDefinition> slimes, IdentifiableType plort)
    {
        foreach (var slime in slimes)
        {
            if (slime == null || slime.IsLargo) continue;

            var produced = slime.Diet?.ProduceIdents;
            if (produced == null) continue;

            foreach (var ident in produced)
                if (ident != null && ident.Pointer == plort.Pointer)
                    return slime;
        }

        return null;
    }
}