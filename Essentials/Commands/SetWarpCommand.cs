using Starlight.Enums;
using Starlight.Managers;
using Starlight.Storage;

namespace Starlight.Commands;

internal class SetWarpCommand : StarlightCommand
{
    public override string ID => "setwarp";
    public override string Usage => "setwarp <name>";
    public override CommandType type => CommandType.Warp;
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        string name = args[0];

        Vector3 pos = sceneContext.Player.transform.position;
        Quaternion rotation = sceneContext.Player.transform.rotation;
        string sceneGroup = sceneContext.RegionRegistry.CurrentSceneGroup.ReferenceId;

        StarlightError error = StarlightWarpManager.AddWarp(name, new Warp(sceneGroup, pos, rotation));
        if (error == StarlightError.AlreadyExists) return SendError(Tr("cmd.warpstuff.alreadywarpwithname",name));

        SendMessage(Tr("cmd.setwarp.success",name));
        return true;
    }
}