using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Starlight.Buttons.Definitions;
using UnityEngine.Localization;
using Starlight.Patches.MainMenu;
namespace Starlight.Buttons;

public class CustomMainMenuButton
{
    public LocalizedString Label;
    public Sprite Icon;
    public int InsertIndex;
    internal CustomMainMenuItemDefinition Definition;
    internal CustomMainMenuSubItemDefinition Definition2;
    public SystemAction Action;

    internal CustomMainMenuButton(int THIS_IS_A_STUB)
    {
        
    }
    public CustomMainMenuButton(LocalizedString label, Sprite icon, int insertIndex, SystemAction action)
    {
        this.Label = label;
        this.Icon = icon;
        this.InsertIndex = insertIndex;
        this.Action = action;

        MainMenuLandingRootUIInitPatch.Buttons.Add(this,new HashSet<CustomMainMenuContainerButton>(){MainMenuLandingRootUIInitPatch.rootStub});

        if (this is CustomMainMenuContainerButton)
        {
            Definition = null;
            Definition2 = ScriptableObject.CreateInstance<CustomMainMenuSubItemDefinition>();
            Definition2._label = label;
            Definition2.name = label.TableEntryReference.Key;
            Definition2._icon = icon;
            Definition2.hideFlags |= HideFlags.HideAndDontSave;
            Definition2.CustomAction = action;
        }
        else
        {
            Definition = ScriptableObject.CreateInstance<CustomMainMenuItemDefinition>();
            Definition._label = label;
            Definition.name = label.TableEntryReference.Key;
            Definition._icon = icon;
            Definition.hideFlags |= HideFlags.HideAndDontSave;
            Definition.CustomAction = action;
        }
        if (StarlightEntryPoint.MainMenuLoaded)
        {
            MainMenuLandingRootUI mainMenu = Object.FindObjectOfType<MainMenuLandingRootUI>();
            if (mainMenu != null)
            {
                mainMenu.gameObject.SetActive(false);
                mainMenu.gameObject.SetActive(true);
            }
        }
    }
}