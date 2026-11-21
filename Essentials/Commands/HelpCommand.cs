using Starlight.Managers;

namespace Starlight.Commands;

internal class HelpCommand : StarlightCommand
{
    public override string ID => "help";
    public override string Usage => "help [cmdName]";
    public override CommandType type => CommandType.Common;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, StarlightCommand> entry in StarlightCommandManager.Commands)
                if (!entry.Value.Hidden) list.Add(entry.Key);
            return list;
        }
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,1)) return SendUsage();
        if (args == null || args.Length == 0)
        {
            string currText = Tr("cmd.help.success")+"\n";

            foreach (KeyValuePair<string, StarlightCommand> entry in StarlightCommandManager.Commands)
                if (!entry.Value.Hidden)
                    currText = $"{currText}\n{entry.Value.Usage} - {entry.Value.ExtendedDescription}";
            SendMessage(currText);
            return true;
        }
        if (!StarlightCommandManager.Commands.TryGetValue(args[0], out var command))
            return SendError(Tr("cmd.help.notvalidcommand",args[0]));

        SendMessage(Tr("cmd.help.successspecific",command.Usage,command.ExtendedDescription));
        return true;
    }
}