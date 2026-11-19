using Il2CppMonomiPark.SlimeRancher.UI.Framework.Components;
using Il2CppMonomiPark.SlimeRancher.UI.Refinery;
using Il2CppTMPro;
using UnityEngine.Localization.Components;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(RefineryUI), nameof(RefineryUI.Start))]
internal static class RefineryUIStartPatch
{
    internal static void Postfix(RefineryUI __instance)
    {
        if (!UIDisplayInteractableOnInteractPatch.takeOverNextUI) return;
        __instance.gameObject.GetObjectRecursively<GameObject>("Icon").SetActive(false);
        var title = __instance.gameObject.GetObjectRecursively<TextMeshProUGUI>("Title");
        title.gameObject.RemoveComponent<LocalizeStringEvent>();
        title.gameObject.RemoveComponent<LocalizeFontEvent>();
        title.text = ("Market");
        UIDisplayInteractableOnInteractPatch.takeOverNextUI = false;
    }
    
    [HarmonyFinalizer]
    static Exception Finalizer(Exception __exception)
    {
        if (!UIDisplayInteractableOnInteractPatch.takeOverNextUI) return __exception;
        return null;
    }
}