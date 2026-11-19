using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace Starlight.Patches.Saving.Fixer;

[HarmonyPriority(-99999999)]
[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushPedia))]
internal static class SaveFixerPushPedia
{
    internal static void Prefix(GameModel gameModel, dynamic pedia, ILoadReferenceTranslation loadReferenceTranslation)
    {
        try {
            //Remove invalid Pedia entries
            if (!StarlightEntryPoint.disableFixSaves)
                foreach (var unlockedID in pedia.UnlockedIds.ToArray())
                    if(loadReferenceTranslation.IsUnknownPediaEntryId(unlockedID)||loadReferenceTranslation.GetPediaEntry(unlockedID)==null)
                        pedia.UnlockedIds.Remove(unlockedID);
        }
        catch (Exception e) { LogError(e); }
    }
}