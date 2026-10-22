using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Player;
using Il2CppMonomiPark.SlimeRancher.Player.Component;

namespace Starlight.Commands;

internal sealed class UpgradeComponentsCommand : StarlightCommand
{
    public override string ID => "upgradecomponents";
    public override string Usage => "upgradecomponents <add/remove/set/get> <id> [amount]";
    public override CommandType type => CommandType.Cheat;

    readonly List<string> arg0List = new List<string> { "add", "remove", "set", "get" };

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return arg0List;
        if (argIndex == 1)
        {
            var list = LookupEUtil.upgradeComponents.Select(x => x.GetCompactName()).ToList();
            list.Add("*");
            return list;
        }
        if (argIndex == 2) if (args[0] != "get") return new List<string> { "1", "5", "10", "20", "50" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(2, 3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        if (!arg0List.Contains(args[0])) return SendNotValidOption(args[0]);

        int amount = args[0] == "set" ? 0 : 1;
        if (args.Length == 3)
        {
            if (args[0] == "get") return SendErrorToManyArgs(args[0]);
            if (!TryParseInt(args[2], out amount, 0, true)) return false;
        }

        var model = sceneContext.PlayerState._model.UpgradeComponentsModel;
        if (model == null) return SendErrorTr("cmd.upgradecomponents.nomodel");

        if (args[1] == "*")
        {
            bool isSilent = silent;
            foreach (var component in LookupEUtil.upgradeComponents)
            {
                silent = true;
                if (args.Length == 3) Execute(new[] { args[0], component.GetCompactName(), args[2] });
                else Execute(new[] { args[0], component.GetCompactName() });
            }
            silent = isSilent;
            switch (args[0])
            {
                case "add": SendMessage(Tr("cmd.upgradecomponents.successalladd", amount)); break;
                case "remove": SendMessage(Tr("cmd.upgradecomponents.successallremove", amount)); break;
                case "set": SendMessage(Tr("cmd.upgradecomponents.successallset", amount)); break;
                case "get": SendMessage(Tr("cmd.upgradecomponents.successlist", ListAll(model))); break;
            }
            return true;
        }

        string componentName = args[1];
        var key = componentName.Replace(" ", "").Replace("_", "").ToUpper();
        var def = LookupEUtil.upgradeComponents.FirstOrDefault(x => x.GetCompactUpperName() == key);
        if (def == null) return SendErrorTr("cmd.upgradecomponents.notvalidcomponent", componentName);

        int current = model.GetComponentQuantity(def);
        switch (args[0])
        {
            case "add":
                Gain(model, def, amount);
                SendMessage(Tr("cmd.upgradecomponents.successadd", amount, def.GetName()));
                break;
            case "remove":
                model.LoseComponent(def, Mathf.Min(amount, current));
                SendMessage(Tr("cmd.upgradecomponents.successremove", Mathf.Min(amount, current), def.GetName()));
                break;
            case "set":
                if (amount > current) Gain(model, def, amount - current);
                else if (amount < current) model.LoseComponent(def, current - amount);
                SendMessage(Tr("cmd.upgradecomponents.successset", def.GetName(), amount));
                break;
            case "get":
                SendMessage(Tr("cmd.upgradecomponents.successget", def.GetName(), current));
                break;
        }
        return true;
    }
    
    static void Gain(UpgradeComponentsModel model, UpgradeComponent component, int count)
    {
        for (int i = 0; i < count; i++) model.GainComponent(component);
    }

    static string ListAll(UpgradeComponentsModel model)
    {
        var text = "";
        foreach (var component in LookupEUtil.upgradeComponents)
            text += $"\n{component.name}: {model.GetComponentQuantity(component)}";
        return text;
    }
}