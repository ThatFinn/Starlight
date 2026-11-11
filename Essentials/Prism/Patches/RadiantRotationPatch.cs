using System.Linq;
using Starlight.Prism.Lib;
using Il2CppMonomiPark.SlimeRancher.Slime;

namespace Starlight.Prism.Patches;

/// <summary>
/// Adds slimes with a created radiant variant to the game's radiant rotation
/// </summary>
[HarmonyPatch(typeof(RadiantSlimeDirector), nameof(RadiantSlimeDirector.Awake))]
internal static class RadiantRotationPatch
{
    public static void Postfix(RadiantSlimeDirector __instance)
    {
        if (!StarlightEntryPoint.prismEnableStyleVariants) return;
        if (RadiantRotation.Slimes.Count == 0) return;

        try
        {
            Register(__instance);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add created slimes to the radiant rotation", ex);
        }
    }

    private static void Register(RadiantSlimeDirector director)
    {
        var config = director._radiantSlimeConfig;
        if (config == null) return;

        var entries = config._radiantShuffleBagSizes.AsEnumerable().Where(entry => entry != null).ToList();
        var added = 0;

        foreach (var definition in RadiantRotation.Slimes)
        {
            if (definition == null || definition.RadiantBase == null) continue;
            if (entries.Any(entry => entry.Slime != null && entry.Slime.Pointer == definition.Pointer)) continue;

            entries.Add(new RadiantSlimeConfig.RadiantShuffleBagConfigEntry
            {
                Slime = definition,
                BagSize = RadiantRotation.WeightOf(definition)
            });
            added++;
        }

        if (added == 0) return;

        config._radiantShuffleBagSizes = entries.ToIl2CppArray();
    }
}