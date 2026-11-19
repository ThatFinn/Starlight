using Il2CppMonomiPark.SlimeRancher.UI.MainMenu.Model;
using Starlight.Buttons.Definitions;

namespace Starlight.Patches.MainMenu;

[HarmonyPatch(typeof(LoadGameBehaviorModel), nameof(LoadGameBehaviorModel.InvokeBehavior))]
internal class SR2MainMenuButtonPressPatch
{
    internal static bool Prefix(LoadGameBehaviorModel __instance)
    {
        if (__instance.Definition is CustomMainMenuItemDefinition definition)
        {
            if(definition.CustomAction!=null) definition.CustomAction.Invoke();
            return false;
        }
        if (__instance.Definition is CustomMainMenuSubItemDefinition definition2)
        {
            if(definition2.CustomAction!=null) definition2.CustomAction.Invoke();
            return false;
        }

        return true;
    }
}