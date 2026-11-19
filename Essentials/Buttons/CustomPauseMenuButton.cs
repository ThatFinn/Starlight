using Starlight.Buttons.Definitions;
using Starlight.Patches.InGame;
using UnityEngine.Localization;

namespace Starlight.Buttons;

public class CustomPauseMenuButton
{
    public LocalizedString Label;
    public int InsertIndex;
    internal CustomPauseItemModel Model;
    public SystemAction Action;
    public bool Enabled = true;
    public CustomPauseMenuButton(LocalizedString label, int insertIndex, SystemAction action)
    {
        this.Label = label;
        this.InsertIndex = insertIndex;
        this.Action = action;

        foreach (CustomPauseMenuButton entry in SR2PauseMenuButtonPatch.buttons)
            if (entry.Label == this.Label) { LogError($"There is already a button with the name {this.Label}"); return; }

        SR2PauseMenuButtonPatch.buttons.Add(this);
    }

    public void Remove()
    {
        Enabled = false;
    }
    public void AddAgain()
    {
        Enabled = true;
    }
}