using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches.Actions;

/// <summary>
/// Invokes the gordo slime custom eat action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(GordoEat), nameof(GordoEat.DoEat))]
internal static class GordoFeedPatch
{
    public static void Prefix(GordoEat __instance, GameObject obj) =>
        ActorEventPatch.Invoke("custom feed action", () => GordoEvents.NotifyFeed(__instance, obj));
}

/// <summary>
/// Invokes the gordo slime custom burst action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(GordoEat), nameof(GordoEat.ImmediateReachedTarget))]
internal static class GordoBurstPatch
{
    public static void Prefix(GordoEat __instance) =>
        ActorEventPatch.Invoke("custom burst action", () => GordoEvents.NotifyBurst(__instance));
}