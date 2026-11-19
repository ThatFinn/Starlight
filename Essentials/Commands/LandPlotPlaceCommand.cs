using Starlight.Storage;

namespace Starlight.Commands;

internal class LandPlotPlaceCommand : StarlightCommand
{
    public override string ID => "landplotplace";
    public override string Usage => "landplotplace <id>";
    public override CommandType type => CommandType.LandPlot | CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        
        var cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            var scene = hit.transform.gameObject.scene;
            var id = "starlightown."+args[0];
            if(SpawnEUtil.HasCustomLandPlot(id))
                return SendErrorTr("cmd.landplotplace.already",args[0]);

            var loc = new StarlightLandPlotLocation(hit.point, scene.name, LandPlot.Id.EMPTY);
            SpawnEUtil.AddCustomLandPlot(id,loc);
            SendMessageTr("cmd.landplotplace.success");
            return true;
        }
        return SendNotLookingAtAnything();
    }
}