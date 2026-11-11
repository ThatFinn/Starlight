using System;

namespace Starlight.Commands;

internal class FastForwardCommand : StarlightCommand
{
    public override string ID => "fastforward";
    public override string Usage => "fastforward [hour amount]";
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
            return new List<string> { "1", "2", "5", "10", "12", "18", "24" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!inGame) return SendLoadASaveFirst();
        if (!args.IsBetween(0,1)) return SendUsage();
         
        double timeToFastForwardTo = sceneContext.TimeDirector.GetNextDawn();
        if (args.Length == 1)
        {
            if (!TryParseFloat(args[0], out float duration, 0, false)) return false;
            timeToFastForwardTo = sceneContext.TimeDirector.HoursFromNow(duration);
        }
        
        double hours = sceneContext.TimeDirector.HoursUntil(timeToFastForwardTo);
        sceneContext.TimeDirector.FastForwardTo(timeToFastForwardTo);
        SendMessage(Tr("cmd.fastforward.success", Math.Round(hours, 1)));
        return true;
    }
}