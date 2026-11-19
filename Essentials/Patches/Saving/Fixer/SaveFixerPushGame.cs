using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace Starlight.Patches.Saving.Fixer;

[HarmonyPriority(-99999999)]
[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushGame))]
internal static class SaveFixerPushGame
{
    private static bool NeedsRemoving(int integer,ILoadReferenceTranslation r)
    {
        try { if (r.GetIdentifiableType(integer) == null) return true; }
        catch (Exception e) { return true; }
        return false;
    }
    static bool needsRemoving2(int typeID,ILoadReferenceTranslation r)
    {
        try
        {
            var ident = r.GetIdentifiableType(typeID);
            if (ident == null) return true;
            
            if(ident.IsGadget())
                return true;
        }
        catch (Exception e) { return true; }

        return false;
    }
    internal static void Prefix(ActorIdProvider actorIdProvider, ISaveReferenceTranslation saveReferenceTranslation, dynamic gameState, GameModel gameModel)
    {
        if (!StarlightEntryPoint.disableFixSaves)
            try {
                var loadTranslation = saveReferenceTranslation.ToNonIVariant().CreateLoadReferenceTranslation(gameState);
                foreach (var id in gameState.Drone.Cloud.IDs.ToArray())
                {
                    try {
                        if (NeedsRemoving(id,loadTranslation))
                            gameState.Drone.Cloud.IDs.Remove(id);
                    }
                    catch (Exception e) { LogError(e); }
                }
                // Gadgets in the actors array are still there from early modded SR2 and break the WorldPopulator
                foreach (var gadget in gameState.Actors.ToArray())
                {
                    try {
                        if (needsRemoving2(gadget.TypeId,loadTranslation))
                            gameState.Actors.Remove(gadget);
                    }
                    catch (Exception e) { LogError(e); }
                }
            }
            catch (Exception e) { LogError(e); }
    }
}