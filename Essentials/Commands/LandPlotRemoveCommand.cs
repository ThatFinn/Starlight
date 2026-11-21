using Starlight.Managers;

namespace Starlight.Commands;

internal class LandPlotRemoveCommand : StarlightCommand
{
    public override string ID => "landplotremove";
    public override string Usage => "landplotremove <name>";
    public override CommandType type => CommandType.LandPlot | CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
        {
            try {
                if (inGame)
                {
                    var plots = new List<string>();
                    foreach (var pair in StarlightSaveManager.inGameData.CustomPlots)
                        if(pair.Key.StartsWith("starlightown."))
                            plots.Add(pair.Key.Substring("starlightown.".Length));
                    return plots;
                }
            } catch  { }
        }

        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        if (args == null || args.Length == 0)
        {
            Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
            if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
            {
                var obj = hit.transform.gameObject;
                LandPlotLocation loc = null;
                while (true)
                {
                    loc = obj.GetComponent<LandPlotLocation>();
                    if (loc != null) break;
                    if (obj.transform.parent == null) break;
                    obj = obj.transform.parent.gameObject;
                }

                if (!loc) return SendNotLookingAtValidObject();
                var _id = loc._id.Substring(loc.IdPrefix().Length);
                SendMessage(_id);
                if (!SpawnEUtil.HasCustomLandPlot(_id)) return SendErrorTr("cmd.landplotremove.native");
                SpawnEUtil.RemoveCustomLandPlot(_id);
                SendMessageTr("cmd.landplotremove.success");
                return true;
            }
            return SendNotLookingAtAnything();
        }

        var id = "starlightown." + args[0];
        if(!SpawnEUtil.HasCustomLandPlot(id)) return SendErrorTr("cmd.landplotremove.no",args[0]);
        SpawnEUtil.RemoveCustomLandPlot(id);
        SendMessageTr("cmd.landplotremove.success");
        return true;
    }
}