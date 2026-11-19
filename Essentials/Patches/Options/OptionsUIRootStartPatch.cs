using Il2CppMonomiPark.SlimeRancher.UI.Options;
using Starlight.Managers;

namespace Starlight.Patches.Options;

[HarmonyPatch(typeof(OptionsUIRoot), nameof(OptionsUIRoot.Start))]
internal static class OptionsUIRootStartPatch
{
    //"OptionsConfiguration_MainMenu"
    //"OptionsConfiguration_InGame"
    public static void Prefix()
    {
        if (!InjectOptionsButtons.HasFlag()) return;
        if(StarlightEntryPoint.MainMenuLoaded) StarlightOptionsButtonManager.LoadCustomOptionsButtons("OptionsConfiguration_MainMenu");
        else if(inGame) StarlightOptionsButtonManager.LoadCustomOptionsButtons("OptionsConfiguration_InGame");
    }
}