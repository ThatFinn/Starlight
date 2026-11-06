using System;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches.Actions;

/// <summary>
/// Gives a radiant slime the components and custom spawn action its creator asked for
/// </summary>
[PrismPatch,FeatureFlagDependentPatch(SupportRadiant)]
[HarmonyPatch(typeof(SlimeRadiant), nameof(SlimeRadiant.Start))]
internal static class RadiantStartPatch
{
    public static void Postfix(SlimeRadiant __instance) => RadiantBehaviourPatch.Apply(__instance, isNew: false);
}

/// <summary>
/// Gives a radiant slime the components and custom spawn action its creator asked for
/// </summary>
[PrismPatch,FeatureFlagDependentPatch(SupportRadiant)]
[HarmonyPatch(typeof(SlimeRadiant), nameof(SlimeRadiant.SetRadiant), new Type[] { typeof(SlimeDefinition) })]
internal static class RadiantSetPatch
{
    public static void Postfix(SlimeRadiant __instance) => RadiantBehaviourPatch.Apply(__instance, isNew: true);
}

/// <summary>
/// Gives a radiant slime the components and custom spawn action its creator asked for
/// </summary>
[PrismPatch,FeatureFlagDependentPatch(SupportRadiant)]
[HarmonyPatch(typeof(SlimeRadiant), nameof(SlimeRadiant.SetRadiant), new Type[0])]
internal static class RadiantSetRollPatch
{
    public static void Postfix(SlimeRadiant __instance) => RadiantBehaviourPatch.Apply(__instance, isNew: true);
}

internal static class RadiantBehaviourPatch
{
    internal static void Apply(SlimeRadiant radiant, bool isNew)
    {
        try
        {
            SlimeEvents.ApplyRadiantComponents(radiant);
            SlimeEvents.NotifyRadiantSpawn(radiant, isNew);

        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to apply a radiant slime's components or custom spawn action", ex);
        }
    }
}