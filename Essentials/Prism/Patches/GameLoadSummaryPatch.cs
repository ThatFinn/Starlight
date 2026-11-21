using System.Reflection;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

[PrismPatch()]
[HarmonyPatch()]
internal static class GameLoadSummaryPatch
{
    private static int refreshedFrame = -1;

    static System.Reflection.MethodBase TargetMethod()
    
        => VersionedEUtil.GetLatestSystemGameVXX().GetMethod("LoadSummaryData", BindingFlags.Instance | BindingFlags.Public);
    
    private static void Prefix()
    {
        try
        {
            var frame = Time.frameCount;
            if (frame == refreshedFrame) return;

            refreshedFrame = frame;
            SaveRegistry.RefreshAll();
        } catch { }
    }
}