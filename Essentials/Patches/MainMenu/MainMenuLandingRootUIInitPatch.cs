using System;
using Il2CppMonomiPark.SlimeRancher.UI.ButtonBehavior;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu.Definition;
using Il2CppTMPro;
using Starlight.Buttons;
using Starlight.Buttons.Definitions;
using Starlight.Managers;

namespace Starlight.Patches.MainMenu;

[HarmonyPatch(typeof(MainMenuLandingRootUI), nameof(MainMenuLandingRootUI.Init))]
internal static class MainMenuLandingRootUIInitPatch
{
    private static CustomMainMenuContainerButton _rootStub = null;
    internal static CustomMainMenuContainerButton rootStub
    {
        get
        {
            if (_rootStub != null) return _rootStub;
            _rootStub = new CustomMainMenuContainerButton(-1);
            return _rootStub;
        }
    }
    
    internal static readonly Dictionary<CustomMainMenuButton,HashSet<CustomMainMenuContainerButton>> Buttons = new ();
    
    internal static bool SafeLock;
    internal static bool PostSafeLock;
    internal static void Prefix(MainMenuLandingRootUI __instance)
    {
        if (InjectOptionsButtons.HasFlag()) try { StarlightOptionsButtonManager.GenerateMissingButtons(); }catch (Exception e) { LogError(e); }
        if (!InjectMainMenuButtons.HasFlag()) return;
        foreach (var pair in Buttons)
        {
            if (!pair.Value.Contains(rootStub)) continue;
            var button = pair.Key;
            if (button is CustomMainMenuContainerButton containerButton)
            {
                try
                {
                    if (containerButton.Definition2 == null) continue;
                    
                    var list = new List<ButtonBehaviorDefinition>();
                    foreach (var pair2 in Buttons)
                    {
                        if(pair2.Value.Contains(containerButton))
                        {
                            if (pair2.Key.Definition != null) list.Add(pair2.Key.Definition);
                            else if (pair2.Key.Definition2!=null) list.Add(pair2.Key.Definition2);
                        }
                    }
                    button.Definition2._subMenuItems = ScriptableObject.CreateInstance<ButtonBehaviorConfiguration>();
                    button.Definition2._subMenuItems.items = list.ToIl2CppList();
                    if (__instance._mainMenuConfig.items.Contains(containerButton.Definition2)) continue;
                    int offset = 0;
                    foreach (var item in __instance._mainMenuConfig.items)
                        if(item is LoadGameItemDefinition) if(!(item is CustomMainMenuItemDefinition)) offset=1;
                    __instance._mainMenuConfig.items.Insert(Math.Clamp(containerButton.InsertIndex+offset,0,__instance._mainMenuConfig.items.Count), containerButton.Definition2);
                }
                catch (Exception e) { LogError(e); }
            }
            else
            {
                try
                {
                    if (button.Definition != null)
                    {
                        if (__instance._mainMenuConfig.items.Contains(button.Definition))
                            continue;
                        int offset = 0;
                        foreach (var item in __instance._mainMenuConfig.items)
                            if(item is LoadGameItemDefinition) if(!(item is CustomMainMenuItemDefinition)) offset=1;
                        __instance._mainMenuConfig.items.Insert(Math.Clamp(button.InsertIndex+offset,0,__instance._mainMenuConfig.items.Count), button.Definition);
                    }
                }
                catch (Exception e) { LogError(e); }
            }

        }
    }

    static void ChangeVersionLabel()
    {
        if (EnableLocalizedVersionPatch.HasFlag())
            try
            {
                var versionLabel = Get<LocalizedVersionText>("Version Label").GetComponent<TextMeshProUGUI>();
                if(!versionLabel.text.Contains("Mel"))
                {
                    if (StarlightUpdateManager.NewVersion != null)
                        if (StarlightUpdateManager.NewVersion != BuildInfo.DisplayVersion)
                        {
                            if (StarlightUpdateManager.UpdatedStarlight) versionLabel.text = Tr("patches.localizedversionpatch.downloadedversion", StarlightUpdateManager.NewVersion, versionLabel.text);
                            else versionLabel.text = Tr("patches.localizedversionpatch.newversion", StarlightUpdateManager.NewVersion, versionLabel.text);
                        }
                    versionLabel.text = Tr("patches.localizedversionpatch.default", mlVersion, versionLabel.text);
                }
            }
            catch {  }
    }
    private static void Postfix()
    {
        ChangeVersionLabel();
        ExecuteInTicks((() => { ChangeVersionLabel();}), 1);
        ExecuteInTicks((() => { ChangeVersionLabel();}), 3);
        ExecuteInTicks((() => { ChangeVersionLabel();}), 10);
    }

    public static bool alreadyLoadedOptions = false;
    
}