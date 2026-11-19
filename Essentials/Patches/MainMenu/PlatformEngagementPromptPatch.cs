using System.Collections;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Starlight.Patches.MainMenu;

[HarmonyPatch(typeof(PlatformEngagementPrompt), nameof(PlatformEngagementPrompt.Start))]
internal static class PlatformEngagementPromptPatch
{
    private static bool _hasRegistered = false;
    internal static void Postfix(PlatformEngagementPrompt __instance)
    {
        __instance.EngagementPromptTextUI.SetActive(false);
        __instance.OnInteract(new InputAction.CallbackContext());
        __instance.StartupClick = null;
        _hasRegistered = false;
        InputSystem.onAnyButtonPress.CallOnce(
            (System.Action<InputControl>)((ee) =>
            {
                if(_hasRegistered) return;
                _hasRegistered = true;
                ExecuteInTicks(() =>
                {
                    if (StarlightEntryPoint.MainMenuLoaded)
                        GetAnyInScene<MainMenuLandingRootUI>()?.Awake();
                }, 1);
                
            }));
        if(ForceLoadMainMenu.HasFlag())
            StartCoroutine(Load(__instance));
    }
    
    static IEnumerator Load(PlatformEngagementPrompt __instance)
    {
        yield return new WaitForSecondsRealtime(5f);
        __instance.LoadSceneGroup();
    }
}