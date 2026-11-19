using Il2CppMonomiPark.SlimeRancher.UI.Loading;
using Starlight.Storage;

namespace Starlight.Patches.Dev;

[FeatureFlagDependentPatch(DevMode)]
[HarmonyPatch(typeof(LoadingScreenView), nameof(LoadingScreenView.Awake))]
internal static class LoadingScreenViewPatch
{
    public static bool done = false;
    public static bool working = false;
    public static bool Prefix(LoadingScreenView __instance)
    {
        if (working) return false;
        if (done) return true;
        working = true;
        var instance = Object.Instantiate(__instance.gameObject, __instance.transform.parent);
        instance.GetComponent<LoadingScreenView>().enabled = false;
        instance.SetActive(false);
        instance.name = "LoadingScreenViewBak";
        Object.DontDestroyOnLoad(instance);
        working = false;
        done = true;
        return true;
    }
}