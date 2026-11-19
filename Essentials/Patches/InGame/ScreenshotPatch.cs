/*
 
    BROKEN IN 1.2.0
    
using Il2CppMonomiPark.SlimeRancher.UI.Pause;
using Starlight.Menus;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(GameContext), nameof(GameContext.TakeScreenshot))]
internal static class ScreenshotPatch
{
    private static System.Collections.IEnumerator WaitForUnpause()
    {
        while (Time.timeScale == 0)
            yield return null;
        sceneContext.PlayerState.VacuumItem.gameObject.SetActive(true);
    }
    
    internal static void Prefix()
    {
        if (StarlightCheatMenu.BetterScreenshot)
        {
            sceneContext.PlayerState.VacuumItem.gameObject.SetActive(false);
            StartCoroutine(WaitForUnpause());
        }
    }
}*/