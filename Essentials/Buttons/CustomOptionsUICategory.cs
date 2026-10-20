using Il2CppMonomiPark.SlimeRancher.Options;
using Starlight.Buttons.Definitions;
using Starlight.Enums;
using Starlight.Managers;
using UnityEngine.Localization;

namespace Starlight.Buttons;
public class CustomOptionsUICategory
{
    private HashSet<CustomAbstractOptionsButton> _buttons = new ();
    public LocalizedString Label;
    public int InsertIndex;
    internal OptionsItemCategory Category;
    public Sprite Icon;
    public OptionsCategoryVisibleState VisibleState;
    public bool Enabled = true;

    public CustomOptionsUICategory(LocalizedString label, int insertIndex, Sprite icon, OptionsCategoryVisibleState visibleState)
    {
        this.Label = label; 
        this.InsertIndex = insertIndex;
        this.Icon = icon;
        this.VisibleState = visibleState;

        StarlightOptionsButtonManager.customOptionsUICategories.Add(this,new HashSet<CustomAbstractOptionsButton>());
    }
    

    public void AddButton(CustomAbstractOptionsButton button)
    { 
        if(!StarlightOptionsButtonManager.customOptionsUICategories[this].Contains(button))
            StarlightOptionsButtonManager.customOptionsUICategories[this].Add(button);
    }
    public void RemoveButton(CustomAbstractOptionsButton button)
    { 
        if(StarlightOptionsButtonManager.customOptionsUICategories[this].Contains(button))
            StarlightOptionsButtonManager.customOptionsUICategories[this].Remove(button);
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