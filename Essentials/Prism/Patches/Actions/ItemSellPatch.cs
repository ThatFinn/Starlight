using Il2CppMonomiPark.SlimeRancher.Economy;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches.Actions;

/// <summary>
/// Invokes the item custom sell action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(PlortEconomyDirector), nameof(PlortEconomyDirector.RegisterSold))]
internal static class ItemSellPatch
{
    public static void Postfix(IdentifiableType id, int count, int price)
    {
        try
        {
            ItemEvents.NotifySell(id, count, price);
        }
        catch (Exception ex)
        {
            LogError("Prism: An item's custom sell action failed", ex);
        }
    }
}