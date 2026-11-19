using Il2CppMonomiPark.SlimeRancher.UI.Pause;
using Starlight.Storage;

namespace Starlight.Buttons.Definitions;

[InjectIntoIL]
internal class CustomPauseItemModel : ResumePauseItemModel
{
    internal SystemAction Action;
    public override void InvokeBehavior()
    {
        Action.Invoke();
    }
}