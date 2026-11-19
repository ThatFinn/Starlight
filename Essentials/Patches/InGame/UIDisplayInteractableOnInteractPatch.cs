using Il2CppMonomiPark.SlimeRancher.UI;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(UIDisplayInteractable), nameof(UIDisplayInteractable.OnInteract))]
internal static class UIDisplayInteractableOnInteractPatch
{
    internal static bool takeOverNextUI;
    internal static bool Prefix(UIDisplayInteractable __instance)
    {
        if (__instance.gameObject.name == "StarlightMarketViewerOverride")
        {
            if(sceneContext.PlortEconomyDirector.IsMarketShutdown())
                return false;
            takeOverNextUI = true;
        }
        return true;
    }
}