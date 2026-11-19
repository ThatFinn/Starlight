using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace Starlight.Patches.Saving.Fixer;

[HarmonyPriority(-99999999)]
[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushPlayer))]
internal static class SaveFixerPushPlayer
{
    private static bool NeedsRemoving(int integer,ILoadReferenceTranslation r)
    {
        try { if (r.GetIdentifiableType(integer) == null) return true; }
        catch (Exception e) { return true; }
        return false;
    }
    internal static void Prefix(GameModel gameModel, dynamic player, ILoadReferenceTranslation loadReferenceTranslation)
    {
        try
        {
            if (!StarlightEntryPoint.disableFixSaves)
            {
                var copyOfItemCounts = new Dictionary<int, int>();
                var enumerator = player.ItemCounts.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var kvp = enumerator._current;
                    copyOfItemCounts.Add(kvp.Key, kvp.Value);
                }
                foreach(var itemCountPair in copyOfItemCounts)
                    if (NeedsRemoving(itemCountPair.Key,loadReferenceTranslation))
                        player.ItemCounts.Remove(itemCountPair.Key);
                
                foreach(var blueprintID in MiscEUtil.ToNetList(player.Blueprints._items))
                    if (NeedsRemoving(blueprintID,loadReferenceTranslation))
                        player.Blueprints.Remove(blueprintID);
                
                foreach(var availBlueprintID in MiscEUtil.ToNetList(player.AvailBlueprints._items))
                    if (NeedsRemoving(availBlueprintID,loadReferenceTranslation))
                        player.AvailBlueprints.Remove(availBlueprintID);
                
                foreach(var favouriteGadgetID in MiscEUtil.ToNetList(player.FavoriteGadgets._items))
                    if (NeedsRemoving(favouriteGadgetID,loadReferenceTranslation))
                        player.FavoriteGadgets.Remove(favouriteGadgetID);

                try
                {
                    foreach(var viewedBluePrintID in MiscEUtil.ToNetList(player.ViewedItems.ViewedBlueprints))
                        if (NeedsRemoving(viewedBluePrintID,loadReferenceTranslation))
                            player.ViewedItems.ViewedBlueprints.Remove(viewedBluePrintID);
                }
                catch { }
                
            }
        }
        catch (Exception e) { LogError(e); }
        
    }
}