using System.Linq;
using Starlight.Prism.Creators;

namespace Starlight.Commands;

internal class BlueprintCommand : StarlightCommand
{
    private static readonly List<string> Actions = new() { "add", "remove", "set", "get" };

    public override string ID => "blueprint";
    public override string Usage => "blueprint <add/remove/set/get> <gadget/*/modded> [show popup(true/false)/owned(true/false)]";
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return Actions;

        if (argIndex == 1)
        {
            var list = LookupEUtil.GetGadgetDefinitionStringListByPartialName(args?[1], true, MAX_AUTOCOMPLETE.Get());
            list.Add("*");
            list.Add("modded");

            return list;
        }

        if (argIndex == 2 && args[0] is "add" or "set") return new List<string> { "true", "false" };

        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(2, 3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        if (!Actions.Contains(args[0])) return SendNotValidOption(args[0]);

        var action = args[0];

        if (args.Length == 2 && action == "set") return SendUsage();
        if (args.Length == 3 && action is "remove" or "get") return SendErrorToManyArgs(action);

        var flag = action == "add";
        if (args.Length == 3 && !TryParseBool(args[2], out flag)) return false;

        if (args[1] is "*" or "modded")
        {
            if (action == "get") return SendNotValidOption(args[1]);

            var modded = args[1] == "modded";
            var owned  = action == "add" || (action == "set" && flag);

            foreach (var gadget in Gadgets().Where(gadget => !modded || PrismGadgetCreatorV02.IsCreated(gadget)))
                SetOwned(gadget, owned, action == "add" && args.Length == 3 && flag);

            SendMessage(Tr(owned
                ? modded ? "cmd.blueprint.successmoddedadd" : "cmd.blueprint.successalladd"
                : modded ? "cmd.blueprint.successmoddedremove" : "cmd.blueprint.successallremove"));

            return true;
        }

        var gadgetType = LookupEUtil.GetGadgetDefinitionByName(args[1]);
        if (gadgetType == null) return SendNotValidGadget(args[1]);

        var name = gadgetType.GetName();
        var has  = sceneContext.GadgetDirector.HasBlueprint(gadgetType);

        switch (action)
        {
            case "add":
                if (has) return SendError(Tr("cmd.blueprint.erroradd", name));

                SetOwned(gadgetType, true, flag);
                SendMessage(Tr("cmd.blueprint.successadd", name));
                break;
            case "remove":
                if (!has) return SendError(Tr("cmd.blueprint.errorremove", name));

                SetOwned(gadgetType, false, false);
                SendMessage(Tr("cmd.blueprint.successremove", name));
                break;
            case "set":
                SetOwned(gadgetType, flag, false);
                SendMessage(Tr(flag ? "cmd.blueprint.successadd" : "cmd.blueprint.successremove", name));
                break;
            case "get":
                SendMessage(Tr(has ? "cmd.blueprint.successgetowned" : "cmd.blueprint.successgetmissing", name));
                break;
        }

        return true;
    }

    private static List<GadgetDefinition> Gadgets()
    {
        var gadgets = new List<GadgetDefinition>();

        foreach (var member in sceneContext.GadgetDirector._gadgetsGroup.GetAllMembersHashSet())
        {
            var gadget = member?.TryCast<GadgetDefinition>();
            if (gadget != null) gadgets.Add(gadget);
        }

        return gadgets;
    }

    private static void SetOwned(GadgetDefinition gadget, bool owned, bool showPopup)
    {
        var director = sceneContext.GadgetDirector;
        if (director.HasBlueprint(gadget) == owned) return;
        
        if (owned) director.AddBlueprint(gadget, showPopup);
        else director._model.blueprints.Remove(gadget);
    }
}