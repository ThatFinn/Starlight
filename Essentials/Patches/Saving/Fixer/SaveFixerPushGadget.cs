using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace Starlight.Patches.Saving.Fixer;

[HarmonyPriority(-99999999)]
[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushGadget))]
internal static class SaveFixerPushGadget
{
    static bool NeedsRemoving(int integer,ILoadReferenceTranslation r)
    {
        try { if (r.GetIdentifiableType(integer) == null) return true; }
        catch (Exception e) { return true; }
        return false;
    }
    internal static bool Prefix(GameModel gameModel, ref dynamic gadget, ILoadReferenceTranslation loadReferenceTranslation)
    {
        if (!StarlightEntryPoint.disableFixSaves)
            try
            {
                if(NeedsRemoving(gadget.TypeId,loadReferenceTranslation)) return false;
            }
            catch (Exception e)
            {
                if(gadget!=null) LogError(e);
                return false;
            }
        return true;
    }
}