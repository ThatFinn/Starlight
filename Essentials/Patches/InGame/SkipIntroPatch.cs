using Il2CppMonomiPark.SlimeRancher.UI.IntroSequence;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(IntroSequenceUIRoot), nameof(IntroSequenceUIRoot.Start))]
internal class SkipIntroPatch
{
    internal static void Postfix(IntroSequenceUIRoot __instance)
    {
        if (StarlightEntryPoint.SaveSkipIntro)
        {
            __instance.EndSequence();
            Object.Destroy(__instance.gameObject);
        }
    }
}