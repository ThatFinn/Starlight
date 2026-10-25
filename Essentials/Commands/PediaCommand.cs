using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Creators;
namespace Starlight.Commands;

internal class PediaCommand : StarlightCommand
{
    public override string ID => "pedia";
    public override string Usage => "pedia <lock/unlock> <entry/*/modded> [show popup(true/false)]";
    List<string> arg0List = new List<string> { "unlock", "lock"};
    public override CommandType type => CommandType.Cheat;
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return arg0List;
        if (argIndex == 1)
        {
            var identifiableTypeGroup = Resources.FindObjectsOfTypeAll<PediaEntry>().Select(x => x.name);
            List<string> list = identifiableTypeGroup.ToList();
            list.Add("*");
            list.Add("modded");
            return list;
        }
        if (argIndex == 2) return new List<string> { "true","false" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(2,3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        if (!arg0List.Contains(args[0])) return SendNotValidOption(args[0]);
        bool showPopup = args[1] != "*" && args[1] != "modded";
        if (args.Length == 3) if (!TryParseBool(args[2], out showPopup)) return false;
        if (args[1] == "*")
        {
            bool isSilent = silent;
            foreach (PediaEntry def in Resources.FindObjectsOfTypeAll<PediaEntry>())
            {
                silent = true;
                if(args.Length==3) Execute(new []{args[0], def.name, args[2]});
                else Execute(new []{args[0], def.name, "false"});
                silent = true;
            }
            silent = isSilent;
            switch (args[0])
            {
                case "lock": SendMessage(Tr("cmd.pedia.successalllock")); break; 
                case "unlock": SendMessage(Tr("cmd.pedia.successallunlock")); break;
            }
            return true;
        }
        if (args[1] == "modded")
        {
            var director = sceneContext.PediaDirector;
            foreach (var created in PrismPediaCreatorV02.CreatedEntries)
            {
                if (created == null) continue;

                if (args[0] == "unlock" && !director.IsUnlocked(created)) director.Unlock(created, showPopup);
                else if (args[0] == "lock" && director.IsUnlocked(created)) director.DebugReLock(created);
            }

            SendMessage(Tr(args[0] == "lock" ? "cmd.pedia.successmoddedlock" : "cmd.pedia.successmoddedunlock"));
            return true;
        }

        string identifierTypeName = args[1];
        PediaEntry id = Resources.FindObjectsOfTypeAll<PediaEntry>().FirstOrDefault(x => x.name.ToLower().Equals(identifierTypeName.ToLower()));
        if (id == null) return SendNotValidPedia(identifierTypeName);
        string itemName = id.name;

        switch (args[0])
        {
            case "lock":
                if (!sceneContext.PediaDirector.IsUnlocked(id))
                    return SendError(Tr("cmd.pedia.errorlock",itemName));
                sceneContext.PediaDirector.DebugReLock(id);
                SendMessage(Tr("cmd.pedia.successlock",itemName)); 
                break;
            case "unlock":
                if (sceneContext.PediaDirector.IsUnlocked(id))
                    return SendError(Tr("cmd.pedia.errorunlock",itemName));
                sceneContext.PediaDirector.Unlock(id,showPopup);
                SendMessage(Tr("cmd.pedia.successunlock",itemName)); 
                break;
        }
        return false;
    }
}