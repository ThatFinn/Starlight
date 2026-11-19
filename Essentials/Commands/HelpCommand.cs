using Starlight.Managers;

namespace Starlight.Commands;

internal class HelpCommand : StarlightCommand
{
    public override string ID => "help";
    public override string Usage => "help [cmdName]";
    public override CommandType type => CommandType.Common;

    public string GetCommandDescription(string command)
    {
        if (StarlightCommandManager.Commands.ContainsKey(command))
            return StarlightCommandManager.Commands[command].ExtendedDescription;
        return string.Empty;
    }

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
        if (args == null)
        {
            string currText = Tr("cmd.help.success")+"\n";

            foreach (KeyValuePair<string, StarlightCommand> entry in StarlightCommandManager.Commands)
                if (!entry.Value.Hidden)
                    currText = $"{currText}\n{entry.Value.Usage} - {GetCommandDescription(entry.Key)}";
            SendMessage(currText);
            return true;
        }
        var desc = GetCommandDescription(args[0]);
        if (StarlightCommandManager.Commands.ContainsKey(args[0]))
        {
            SendMessage(Tr("cmd.help.successspecific",StarlightCommandManager.Commands[args[0]].Usage,desc));
            return true;
        }
        return SendError(Tr("cmd.help.notvalidcommand",args[0]));
        
    }
}