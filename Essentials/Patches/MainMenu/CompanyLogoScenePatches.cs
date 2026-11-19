using Il2CppMonomiPark.SlimeRancher.UI;

namespace Starlight.Patches.MainMenu;

internal static class CompanyLogoScenePatches
{
    internal static readonly List<Sprite> CustomBouncySprites = new();
    [HarmonyPatch(typeof(CompanyLogoScene), nameof(CompanyLogoScene.StartLoadingIndicator))]
    internal static class StartLoadingIndicatorPatch
    {
        internal static bool AlreadyStarted = false;
        internal static bool Prefix(CompanyLogoScene __instance)
        {
            __instance._skipWarningScreen = true;
            try
            {
                foreach (var sprite in CustomBouncySprites)
                    if(!__instance.bouncyIcons.Contains(sprite))
                        __instance.bouncyIcons = __instance.bouncyIcons.AddToNew(sprite);
                
            } catch { }
            if (AlreadyStarted) return false;
            AlreadyStarted = true;
            return true;
        }
    }
    
    [HarmonyPatch(typeof(CompanyLogoScene), nameof(CompanyLogoScene.Start))]
    internal static class StartPatch
    {
        internal static void Postfix(CompanyLogoScene __instance)
        {
            StartLoadingIndicatorPatch.AlreadyStarted = false;
            __instance.chanceToBlink = 1f;
            __instance._skipWarningScreen = true;
            try
            {
                foreach (var sprite in CustomBouncySprites)
                    if(!__instance.bouncyIcons.Contains(sprite))
                        __instance.bouncyIcons = __instance.bouncyIcons.AddToNew(sprite);
                
            } catch { }
            __instance.StartLoadingIndicator();
        }
    }
}