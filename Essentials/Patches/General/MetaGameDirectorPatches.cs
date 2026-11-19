using Il2CppMonomiPark.SlimeRancher;

namespace Starlight.Patches.General;

[HarmonyPatch(typeof(MetaGameDirector), nameof(MetaGameDirector.ChangeGameActivity))]
internal static class MetaGameDirectorChangeGameActivityPatch
{
    public static bool Prefix()
    {
        if (StarlightEntryPoint.changedUserFolder || ForceLoadMainMenu.HasFlag()) return false;
        return true;
    }
}
[HarmonyPatch(typeof(MetaGameDirector), nameof(MetaGameDirector.SetAchievementProgress), typeof(MetaGameDirector.AchievementData), typeof(int))]
[HarmonyPatch(typeof(MetaGameDirector), nameof(MetaGameDirector.SetAchievementProgress), typeof(MetaGameDirector.Achievement), typeof(int))]
internal static class MetaGameDirectorSetAchievementProgressPatch
{
    public static bool Prefix()
    {
        if (StarlightEntryPoint.changedUserFolder || ForceLoadMainMenu.HasFlag()) return false;
        return true;
    }
}