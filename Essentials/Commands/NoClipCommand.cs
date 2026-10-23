using Starlight.Managers;
using UnityEngine.InputSystem;

namespace Starlight.Commands;

internal class NoClipCommand : StarlightCommand
{
    public override string ID => "noclip";
    public override string Usage => "noclip";
    public override CommandType type => CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0, 0)) return SendNoArguments();
        try
        {
            if (StarlightNoClipManager.Toggle())
                SendMessage(Tr("cmd.noclip.success"));
            else
                SendMessage(Tr("cmd.noclip.success2"));
            return true;
        }
        catch { return SendError("cmd.noclip.error"); }
    }

    public static InputAction horizontal;
    public static InputAction vertical;

    public override void OnMainMenuUILoad()
    {
        horizontal = LookupEUtil.MainGameActions["Horizontal"];
        vertical = LookupEUtil.MainGameActions["Vertical"];
    }
}