using Starlight.Managers;

namespace Starlight.Commands;

internal class LandPlotListCommand : StarlightCommand
{
    public override string ID => "landplotlist";
    public override string Usage => "landplotlist";
    public override CommandType type => CommandType.LandPlot | CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,0)) return SendNoArguments();
        if (!inGame) return SendLoadASaveFirst();

        if (StarlightSaveManager.inGameData.CustomPlots.Count == 0) return SendErrorTr("cmd.landplotlist.error");

        SendMessageTr("cmd.landplotlist.success");
        foreach (var pair in StarlightSaveManager.inGameData.CustomPlots)
            SendMessage(Tr("cmd.landplotlist.successdesc",pair.Key.Substring("starlightown.".Length),pair.Value.SceneName,pair.Value.Position.x,pair.Value.Position.y,pair.Value.Position.z));
        return true;
    }
}