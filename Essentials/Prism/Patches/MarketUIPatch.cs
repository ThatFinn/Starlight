using System.Linq;
using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

[PrismPatch()]
[HarmonyPriority(-9999999),HarmonyPatch(typeof(MarketUI))]
internal static class MarketUIPatch
{
    [HarmonyPrefix,HarmonyPriority(-9999999),HarmonyPatch(nameof(MarketUI.Start))]
    public static void Prefix(MarketUI __instance)
    {
        var plortEntries = new List<PlortEntry>(__instance._config._plorts);
        foreach (var entry in __instance._config._plorts)
            foreach (var type in PrismShortcuts.RemoveMarketPlortEntries)
                if (entry.IdentType.ReferenceId == type.ReferenceId)
                {
                    plortEntries.Remove(entry);
                    break;
                }
        foreach (var pair in PrismShortcuts.MarketPlortEntries)
            if (!pair.Value)
                plortEntries.Add(pair.Key);

        var slots = 0;
        if (__instance.pricesPanels != null)
            foreach (var panel in __instance.pricesPanels)
                if (panel != null) slots += panel.entryCount;
        
        __instance._config._plorts = plortEntries.Take(slots).ToArray();
        
        PrismLibMarket.TryRefreshMarketData();
    }
}