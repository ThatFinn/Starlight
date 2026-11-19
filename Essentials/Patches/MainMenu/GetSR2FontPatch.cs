using Il2CppMonomiPark.SlimeRancher.UI.ButtonBehavior;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Il2CppTMPro;

namespace Starlight.Patches.MainMenu;

[HarmonyPatch(typeof(SRButton), nameof(SRButton.Awake))]
internal static class GetSR2FontPatch
{
    internal static void Postfix(SRButton __instance)
    {
        if (__instance.GetComponent<MainMenuButton>() != null)
        {
            if (StarlightEntryPoint.Sr2FontAsset != null) return;
            var label = __instance.gameObject.GetObjectRecursively<TextMeshProUGUI>("Button_Label");
            if (label != null) StarlightEntryPoint.Sr2FontAsset = label.font;
            StarlightEntryPoint.SetupFonts();
        }
    }
}