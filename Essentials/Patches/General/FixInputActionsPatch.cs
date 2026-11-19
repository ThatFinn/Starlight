using Il2CppMonomiPark.SlimeRancher.UI;
namespace Starlight.Patches.General
{
    [HarmonyPatch(typeof(BaseUI), "Awake")]
    internal class FixInputActionsPatch
    {
        internal static void Postfix(BaseUI __instance)
        {
            foreach (var input in LookupEUtil.PausedActions.Values)
                input.Enable();
        }
    }
}