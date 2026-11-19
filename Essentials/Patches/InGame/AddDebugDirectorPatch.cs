using Il2CppMonomiPark.SlimeRancher.Player;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(PlayerObjectDiscoveryHandler), nameof(PlayerObjectDiscoveryHandler.Start))]
internal class AddDebugDirectoyPatch
{
    internal static void Postfix(PlayerObjectDiscoveryHandler __instance)
    {
        if (__instance.gameObject.GetComponent<StarlightDebugUI>() == null)
            __instance.gameObject.AddComponent<StarlightDebugUI>();
    }
}