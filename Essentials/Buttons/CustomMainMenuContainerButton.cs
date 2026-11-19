using UnityEngine.Localization;
using Starlight.Patches.MainMenu;

namespace Starlight.Buttons;

public class CustomMainMenuContainerButton : CustomMainMenuButton
{
    public void AddSubButton(CustomMainMenuButton button, bool removeFromCurrent = true)
    {
        if (removeFromCurrent) MainMenuLandingRootUIInitPatch.Buttons[button] = new HashSet<CustomMainMenuContainerButton>();
        MainMenuLandingRootUIInitPatch.Buttons[button].Add(this);
    }

    internal CustomMainMenuContainerButton(int THIS_IS_A_STUB) : base(THIS_IS_A_STUB)
    {
        
    }
    public CustomMainMenuContainerButton(LocalizedString label, Sprite icon, int insertIndex, SystemAction action) : base(label, icon, insertIndex, action)
    {
    }
}