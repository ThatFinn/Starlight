
using System;
using System.Reflection;

namespace Starlight.Patches.General;

[HarmonyPatch("Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher", "ReportException")]
internal static class PatchIl2CppDetourMethodPatcher
{
    internal static bool Prefix(System.Exception ex)
    {
        if (!EnableIl2CppDetourExceptionReporting.HasFlag()) return true;
        LogError("During invoking native->managed trampoline", ex);
        return false;
    }
    internal static void InstallSecondPart(HarmonyLib.Harmony HarmonyInstance)
    {
        if (EnableIl2CppDetourExceptionReporting.HasFlag())
        {
            try
            {
                Type patchType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!asm.FullName.StartsWith("MelonLoader")) continue;
                    var type = asm.GetType("MelonLoader.Fixes.Il2CppInteropExceptionLog", false);
                    if (type != null)
                    {
                        patchType = type;
                        break;
                    }
                }
                var methodToPatch = patchType.GetMethod("ReportException_Prefix", BindingFlags.NonPublic | BindingFlags.Static);
                var alternativemethod = typeof(PatchIl2CppDetourMethodPatcher).GetMethod(nameof(ReportException_Prefix_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
                HarmonyInstance.Patch(methodToPatch, new HarmonyMethod(alternativemethod));
            }
            catch { }
        }
    }

    private static bool ReportException_Prefix_Prefix()
    {
        if (!EnableIl2CppDetourExceptionReporting.HasFlag()) return true;
        return false;
    }
}