using Il2CppMonomiPark.SlimeRancher;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(AutoSaveDirectorConfiguration), nameof(AutoSaveDirectorConfiguration.SaveSlotCount), MethodType.Getter)]
internal class AutoSaveDirectorSaveSlotPatch
{
    internal static void Prefix(AutoSaveDirectorConfiguration __instance, ref int __result)
    {
        __result = SAVESLOT_COUNT.Get();
    }
    internal static void Postfix(AutoSaveDirectorConfiguration __instance, ref int __result)
    {
        __result = SAVESLOT_COUNT.Get();
    }
}