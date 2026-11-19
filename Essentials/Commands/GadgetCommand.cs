namespace Starlight.Commands;

internal class GadgetCommand : StarlightCommand
{
    public override string ID => "gadget";
    public override string Usage => "gadget <add/set/remove/unlock/get> <gadget> [amount/show popup(true/false)]";
    //Lock is broken rn
    List<string> arg0List = new List<string> { "add","set","remove","get","unlock"/*, lock*/};
    public override CommandType type => CommandType.Cheat;
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
            return arg0List;
        if (argIndex == 1)
        {
            var list = LookupEUtil.GetGadgetDefinitionStringListByPartialName(args == null ? null : args[1], true,
                MAX_AUTOCOMPLETE.Get());
            list.Add("*");
            return list;
        }
        if (argIndex == 2)
            if(args[0]!="get")
                return new List<string> { "1", "5", "10", "20", "30", "50" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(2,3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        if(!arg0List.Contains(args[0])) return SendNotValidOption(args[0]);

        bool showPopup = true;
        int amount = 1;
        if (args.Length == 3)
        {
            if(args[0]=="get") return SendErrorToManyArgs(args[0]);
            if (args[0] == "unlock" || args[0] == "lock")
            {
                if (!TryParseBool(args[2], out showPopup)) return false;
            }
            else if(!TryParseInt(args[2], out amount,1,true)) return false;
        }
        else
            switch (args[0])
            {
                case "remove":
                case "add": amount = 1; break;
                case "set": amount = 0; break;
            }
        
        if (args[1] == "*")
        {
            bool isSilent = silent;
            foreach (var def in sceneContext.GadgetDirector._gadgetsGroup.GetAllMembersHashSet())
            {
                silent=isSilent?true:args[0]!="get";
                if(args.Length==3) Execute(new []{args[0], def.name, args[2]});
                else if(args[0]=="unlock") Execute(new []{args[0], def.name, "false"});
                else Execute(new []{args[0], def.name});
                silent=args[0]!="get";
            }
            silent = isSilent;
            switch (args[0])
            {
                case "add": SendMessage(Tr("cmd.gadget.successalladd", amount)); break;
                case "set": SendMessage(Tr("cmd.gadget.successallset", amount)); break;
                case "remove": SendMessage(Tr("cmd.gadget.successallremove", amount)); break;
                case "lock": SendMessage(Tr("cmd.gadget.successalllock")); break;
                case "unlock": SendMessage(Tr("cmd.gadget.successallunlock")); break;
            }
            return true;
        }
        string identifierTypeName = args[1];
        GadgetDefinition type = LookupEUtil.GetGadgetDefinitionByName(identifierTypeName);
        if (type == null) return SendNotValidGadget(identifierTypeName);
        string itemName = type.GetName();

        switch (args[0])
        {
            case "remove":
            case "add":
            case "set":
                int newValue = 0;
                int oldValue = sceneContext.GadgetDirector.GetItemCount(type);
                switch (args[0])
                {
                    case "set": newValue = amount; break;
                    case "add": newValue = amount + oldValue; break;
                    case "remove": newValue = oldValue - amount; break;
                }
                newValue=Mathf.Clamp(newValue, 0, int.MaxValue);
                
                if (newValue > 0 && !sceneContext.GadgetDirector.HasBlueprint(type)) sceneContext.GadgetDirector.AddBlueprint(type);
                
                //Adding one updates the new value everywhere. Not doing can causes issues
                sceneContext.GadgetDirector._model.SetCount(type,newValue-1);
                sceneContext.GadgetDirector.AddItem(type,1);
                
                switch (args[0])
                {
                    case "set": SendMessage(Tr("cmd.gadget.successset", itemName, amount)); break;
                    case "add": SendMessage(Tr("cmd.gadget.successadd",amount,itemName)); break;
                    case "remove": SendMessage(Tr("cmd.gadget.successremove",amount,itemName)); break;
                }
                break;
            case "get":
                SendMessage(Tr("cmd.gadget.successget",sceneContext.GadgetDirector.GetItemCount(type),itemName));
                break;
            /*case "lock":
                if (!sceneContext.GadgetDirector.HasBlueprint(type))
                    return SendError(translation("cmd.gadget.errorlock",itemName));
                //This function doesn't exist  :(
                sceneContext.GadgetDirector.RemoveBlueprint(type);
                SendMessage(translation("cmd.gadget.successlock",itemName));
                break;*/
            case "unlock":
                if (sceneContext.GadgetDirector.HasBlueprint(type))
                    return SendError(Tr("cmd.gadget.errorunlock",itemName));
                sceneContext.GadgetDirector.AddBlueprint(type,showPopup);
                SendMessage(Tr("cmd.gadget.successunlock",itemName));
                break;
        }
        return false;
    }
}