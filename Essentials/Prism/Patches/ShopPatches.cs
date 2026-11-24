using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppMonomiPark.SlimeRancher.Shop;
using Il2CppMonomiPark.SlimeRancher.Shop.Runtime;
using Starlight.Prism.Lib;
using Starlight.Storage;
using UnityEngine.AddressableAssets;

namespace Starlight.Prism.Patches;

[PrismPatch]
[HarmonyPatch(typeof(ShopRuntimeAddressablesDataProvider), nameof(ShopRuntimeAddressablesDataProvider.HandleAsyncAllComplete))]
internal static class ShopTablesLoadedPatch
{
    public static void Prefix(ShopRuntimeAddressablesDataProvider __instance)
    {
        try
        {
            foreach (var pair in __instance._loadedAssets)
            {
                var handle = pair.Value;
                if (!handle.IsValid() || !handle.IsDone) continue;

                var link = handle.Result?.TryCast<ShopCategorySourceLink>();
                if (link != null) PrismLibShop.AddTo(link.LinkedSource);
            }
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add the listings to the shop", ex);
        }
    }
}

[PrismPatch]
[HarmonyPatch(typeof(ShopRuntimeItem), nameof(ShopRuntimeItem.AcquireResourcesImpl))]
internal static class ShopItemLoadPatch
{
    public static bool Prefix(ShopRuntimeItem __instance)
    {
        try
        {
            var item = PrismLibShop.ItemFor(__instance._itemEntry?.ItemCost?.ShopItem);
            if (item == null) return true;

            var acquirable = item.Cast<IAcquirable>();

            __instance._loadHandle = Addressables.ResourceManager.CreateCompletedOperation(acquirable, null);
            __instance._loadCallbacks?.Invoke(acquirable);

            return false;
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to load a listed item", ex);
            return true;
        }
    }
}
