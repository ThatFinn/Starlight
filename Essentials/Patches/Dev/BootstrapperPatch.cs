using Il2CppMonomiPark.SlimeRancher;
using Starlight.Storage;

namespace Starlight.Patches.Dev;

[FeatureFlagDependentPatch(DevMode)]
[HarmonyPatch(typeof(Bootstrapper), nameof(Bootstrapper.Start))]
internal static class BootstrapperPatch
{
    public static bool done = false;
    public static void Prefix(Bootstrapper __instance)
    {
        if (done) return;
        var instance = Object.Instantiate(__instance.gameObject, __instance.transform.parent);
        instance.SetActive(false);
        instance.name = "BootstrapperBak";
        Object.DontDestroyOnLoad(instance);
        done = true;
    }
}