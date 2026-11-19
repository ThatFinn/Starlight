namespace Starlight.Patches.Landplot;

[HarmonyPatch(typeof(LandPlot),nameof(LandPlot.Start))]
internal static class LandPlotStartPatch
{
    [HarmonyFinalizer]
    static Exception Finalizer(LandPlot __instance,Exception __exception)
    {
        if (__exception == null) return null;
        try { if (SpawnEUtil.LandPlotLocations.Contains(__instance.transform.parent.GetComponent<LandPlotLocation>())) return null; } catch { }
        return __exception;
    }
}