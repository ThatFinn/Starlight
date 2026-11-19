using Il2CppMonomiPark.SlimeRancher.Options;
using Starlight.Enums;
using Starlight.Managers;
using Starlight.Storage;

namespace Starlight.Buttons.Definitions;

[InjectIntoIL]
internal class CustomOptionsValuesDefinition : ScriptedValuePresetOptionDefinition
{
    internal CustomOptionsButtonValues button;
    public override void ApplyPresetSelection(int index)
    {
        if(!string.IsNullOrWhiteSpace(button.saveid))
            StarlightOptionsButtonManager.SetValuesButton(button.Type,button.saveid, index);
        try { button.onModify.Invoke(index); }catch (Exception e) { LogError(e); }
    }

    private int askedForPreset = 0;
    public override int GetDefaultPresetIndex()
    {
        if (!string.IsNullOrWhiteSpace(button.saveid))
            return StarlightOptionsButtonManager.GetValuesButton(button.Type,button.saveid, _defaultValueIndex);
        return _defaultValueIndex;
    }

    public override bool ShouldDisplay()
    {
        if (button.Visible != null)
        {
            try { if (!button.Visible()) return false; } catch (Exception e) { LogError(e); }
        }
        switch (button.Type)
        {
            case OptionsButtonType.OptionsUI: return true;
            case OptionsButtonType.InGameOptionsUIOnly: return inGame;
        }
        return false;
    }
    public override bool ShouldEnable() => ShouldDisplay();

    public override string ReferenceId
    {
        get
        {
            _referenceId = button.refID;
            return button.refID;
        }
    }
}