using Il2CppMonomiPark.SlimeRancher.UI.Refinery;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(RefineryConfiguration), nameof(RefineryConfiguration.GetItems))]
internal static class RefineryConfigurationGetItemsPatch
{
    internal static bool Prefix(ref Il2CppSystem.Collections.Generic.List<IdentifiableType> __result)
    {
        if (!UIDisplayInteractableOnInteractPatch.takeOverNextUI) return true;
        var list = new List<IdentifiableType>();
        foreach (var pair in sceneContext.PlortEconomyDirector._currValueMap)
            list.Add(pair.Key);
        __result = list.ToIl2CppList();
        return false;
    }
    
    [HarmonyFinalizer]
    static Exception Finalizer(Exception __exception)
    {
        if (!UIDisplayInteractableOnInteractPatch.takeOverNextUI) return __exception;
        return null;
    }
}