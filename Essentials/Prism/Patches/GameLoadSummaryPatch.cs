using System.Reflection;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

[PrismPatch()]
[HarmonyPatch()]
internal static class GameLoadSummaryPatch
{
    static System.Reflection.MethodBase TargetMethod()
    
        => VersionedEUtil.GetLatestSystemGameVXX().GetMethod("LoadSummaryData", BindingFlags.Instance | BindingFlags.Public);
    
    private static void Prefix()
    {
        try
        {
            foreach (var actor in PrismLibSaving.SavedIdents)
                PrismLibSaving.RefreshIfNotFound(autoSaveDirector._saveReferenceTranslation,actor.Value);
        } catch { }
    }
}