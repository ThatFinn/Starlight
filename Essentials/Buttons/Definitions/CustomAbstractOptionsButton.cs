using System;
using Il2CppMonomiPark.SlimeRancher.Options;
using Starlight.Enums;

namespace Starlight.Buttons.Definitions;
// Make it public on release
internal abstract class CustomAbstractOptionsButton
{
    public OptionsButtonType Type = OptionsButtonType.OptionsUI;
    internal static List<string> UsedIds = new ();
    public int InsertIndex;
    
    public Func<bool> Visible;
    private OptionsItemDefinition _definition;
    protected virtual OptionsItemDefinition GenerateOptionsItemDefinition()
    {
        return null;
    }

    internal CustomAbstractOptionsButton()
    {
        
    }
    internal OptionsItemDefinition GetOptionsItemDef()
    {
        if (_definition != null) return _definition;
        try
        {
            _definition = GenerateOptionsItemDefinition();
            return _definition;
        } catch (Exception e) {LogError(e);}

        return null;
    }
}