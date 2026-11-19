using Il2CppMonomiPark.SlimeRancher.UI.MainMenu.Definition;
using Starlight.Storage;

namespace Starlight.Buttons.Definitions;

[InjectIntoIL]
internal class CustomMainMenuItemDefinition : LoadGameItemDefinition
{
    internal SystemAction CustomAction;
}