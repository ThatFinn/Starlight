using System.Linq;
using System.Text;
using Starlight.Enums;
using Starlight.Managers;

namespace Starlight.Commands;

internal class BindCommand : StarlightCommand
{
    public override string ID => "bind";
    public override string Usage => "bind <key> <command>";
    public override CommandType type => CommandType.Binding;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
            return LookupEUtil.GetLKeyStringListByPartialName(args[0],true,MAX_AUTOCOMPLETE.Get());
        if (argIndex == 1)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, StarlightCommand> entry in StarlightCommandManager.Commands) list.Add(entry.Key);
            return list;
        }

        if (StarlightCommandManager.Commands.TryGetValue(args[1], out var command))
            return command.GetAutoComplete(argIndex - 2, args.Skip(2).ToArray());
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(2,-1)) return SendUsage();

        LKey key;
        if (!TryParseLKey(args[0], out key)) return false;
        
        StringBuilder builder = new StringBuilder();
        for (int i = 1; i < args.Length; i++) builder.Append(args[i] + " ");
        
        string executeString = builder.ToString();

        StarlightCommandManager.BindKey(key, executeString);
        SendMessage(Tr("cmd.bind.success", executeString, key));
        return true;
    }
}