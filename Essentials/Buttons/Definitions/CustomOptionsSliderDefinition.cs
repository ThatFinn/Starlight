using Il2CppMonomiPark.SlimeRancher.Options;
using Starlight.Enums;
using Starlight.Storage;

namespace Starlight.Buttons.Definitions;

[InjectIntoIL]
internal class CustomOptionsSliderDefinition : ScriptedFloatOptionDefinition
{
    internal CustomOptionsButtonSlider button;

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