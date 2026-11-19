using Starlight.Enums;
using Starlight.Managers;

namespace Starlight.Patches.Context;

[HarmonyPatch(typeof(SceneContext), nameof(SceneContext.Start))]
internal class SceneContextPatch
{
    internal static void Postfix(SceneContext __instance)
    {
        StarlightEntryPoint.CheckForTime();
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            try { expansion.AfterSceneContext(__instance); }
            catch (Exception e) { LogError(e); }
        StarlightCallEventManager.ExecuteWithArgs(CallEvent.AfterSceneContextLoad, ("sceneContext", __instance));
    }
}