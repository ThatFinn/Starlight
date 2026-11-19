using Starlight.Enums;
using Starlight.Managers;
using Starlight.Storage;

namespace Starlight.Commands;

internal class WarpCommand : StarlightCommand
{
    public override string ID => "warp";
    public override string Usage => "warp <location>";
    public override CommandType type => CommandType.Warp | CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
        {
            List<string> warps = new List<string>();
            foreach (KeyValuePair<string, Warp> pair in StarlightSaveManager.data.warps) warps.Add(pair.Key);
            return warps;
        }
        return null;
    }
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        string name = args[0];
        Warp warp = StarlightWarpManager.GetWarp(name);
        if (warp == null) return SendError(Tr("cmd.warpstuff.nowarpwithname",name));

        StarlightError error = warp.WarpPlayerThere();
        switch (error)
        {
            case StarlightError.NoError: SendMessage(Tr("cmd.warp.success",name)); return true;
            case StarlightError.NotInGame: return SendLoadASaveFirst();
            case StarlightError.PlayerNull: return SendLoadASaveFirst();
            case StarlightError.TeleportablePlayerNull: return SendNullTeleportablePlayer();
            case StarlightError.SRCharacterControllerNull: return SendNullSRCharacterController();
            case StarlightError.SceneGroupNotSupported: return SendUnsupportedSceneGroup(warp.sceneGroup);
            case StarlightError.DoesntExist: return SendError(Tr("cmd.warpstuff.nowarpwithname",name));
        }
        return SendUnknown();

    }
}