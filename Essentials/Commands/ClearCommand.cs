using Starlight.Menus;

namespace Starlight.Commands;

internal class ClearCommand : StarlightCommand
{
    public override string ID => "clear";
    public override string Usage => "clear";
    public override CommandType type => CommandType.Common;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,0)) return SendNoArguments();

        for (int i = 0; i < MenuEUtil.GetMenu<StarlightConsole>().ConsoleContent.childCount; i++) Object.Destroy(MenuEUtil.GetMenu<StarlightConsole>().ConsoleContent.GetChild(i).gameObject);

        return true;
    }
}