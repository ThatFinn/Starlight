using Il2CppMonomiPark.SlimeRancher.UI.RanchHouse;
using Starlight.Patches.InGame;
using UnityEngine.Localization;

namespace Starlight.Buttons;

public class CustomRanchUIButton
{
    public LocalizedString Label;
    public int InsertIndex;

    internal RanchHouseItemViewModelBase Model;

    public SystemAction Action;
    public bool Enabled = true;

    public CustomRanchUIButton(LocalizedString label, int insertIndex, SystemAction action)
    {
        this.Label = label;
        this.InsertIndex = insertIndex;
        this.Action = action;

        foreach (CustomRanchUIButton entry in SR2RanchUIButtonPatch.Buttons)
            if (entry.Label == this.Label) { LogError($"There is already a button with the name {this.Label}"); return; }

        SR2RanchUIButtonPatch.Buttons.Add(this);
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