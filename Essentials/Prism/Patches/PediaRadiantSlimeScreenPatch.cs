using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using Starlight.Storage;
using UnityEngine.UI;

namespace Starlight.Prism.Patches;

[PrismPatch,FeatureFlagDependentPatch(SupportRadiant)]
[HarmonyPatch(typeof(PediaRadiantSlimeScreen),nameof(PediaRadiantSlimeScreen.SetCategory))]
internal static class PediaRadiantSlimeScreenPatch
{
    public static void Prefix(PediaRadiantSlimeScreen __instance)
    {
        var oldButtonList = __instance._buttons.ToNetList();
        var newButtonList = __instance._buttons.ToNetList();
        foreach (var pair in PrismShortcuts.PrismRadiantSlimePediaEntries)
            if (!pair.Value.GetPrismBaseSlime().GetIsNative()&&pair.Value.GetPrismBaseSlime().GetSlimeAppearanceRadiant()!=null)
            {
                var contains = false;
                foreach (var entry in oldButtonList)
                    try
                    {
                        if (entry._entry==pair.Value.GetRadiantSlimePediaEntry())
                        {
                            contains = true;
                            break;
                        }
                    }catch { }
                if (!contains)
                {
                    var prefab = oldButtonList[0];
                    var instance = GameObject.Instantiate(prefab.gameObject, prefab.transform.parent).GetComponent<PediaEntryIconButton>();
                    instance._entry = pair.Value.GetRadiantSlimePediaEntry();
                    newButtonList.Add(instance);
                }
            }
        //This is a workaround until the game does it on its own
        if (oldButtonList.Count != newButtonList.Count)
        {
            var gridLayout = newButtonList[0].transform.parent.gameObject.AddComponent<GridLayoutGroup>();
            ExecuteInTicks(() =>
            {
                ExecuteInTicks(() =>
                {
                    Object.Destroy(gridLayout);
                },2);
            },10);
        }
        __instance._buttons = newButtonList.ToIl2CppArray();
    }
}