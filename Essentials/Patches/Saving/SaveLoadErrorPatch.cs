using Il2CppMonomiPark.SlimeRancher;

namespace Starlight.Patches.Saving;

[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushGame))]
internal static class LoadPatch
{
    [HarmonyFinalizer]
    static Exception Finalizer(Exception __exception)
    {
        if (__exception == null) return null;
        if (IgnoreSaveErrors.HasFlag())
        {
            LogError($"Error occured while pushing saved game!\nThe error: {__exception}\n\nContinuing!");
            return null;
        }
        LogError($"Error occured while pushing saved game!\nThe error: {__exception}");
        return __exception;
    }
}
[HarmonyPatch(typeof(GameModelPullHelpers), nameof(GameModelPullHelpers.PullGame))]
internal static class SavePatch
{
    [HarmonyFinalizer]
    static Exception Finalizer(Exception __exception)
    {
        if (__exception == null) return null;
        if (IgnoreSaveErrors.HasFlag())
        {
            LogError($"Error occured while pulling saved game!\nThe error: {__exception}\n\nContinuing!");
            return null;
        }
        LogError($"Error occured while pulling saved game!\nThe error: {__exception}");
        return __exception;
    }
}