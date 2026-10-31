using Starlight.Storage;

namespace Starlight.Commands;

internal class GiveCommand : StarlightCommand
{
    public override string ID => "give";
    public override string Usage => "give <item> [amount] [radiant(true/false)] [allowOverflow(true/false)]";
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
            return LookupEUtil.GetVaccableStringListByPartialName(args == null ? null : args[0], true,MAX_AUTOCOMPLETE.Get());
        if (argIndex == 1)
            return new List<string> { "1", "5", "10", "20", "30", "50" };
        if (argIndex == 2)
            return new List<string> { "true", "false" };
        if (argIndex == 3)
            return new List<string> { "true", "false" };

        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,4)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        string identifierTypeName = args[0];
        var ident = LookupEUtil.GetIdentifiableTypeByName(identifierTypeName);
        if (!ident) return SendNotValidIdentType(identifierTypeName);
        string itemName = ident.GetName();
        if (ident.IsGadget()) return SendIsGadgetNotItem(itemName);
        
        var allowOverflow = false;
        var makeRadiant = false;
        var amount = 1;
        if (args.Length >= 2) if(!TryParseInt(args[1], out amount,1, true)) return false;
        if (args.Length >= 3) if (!TryParseBool(args[2], out makeRadiant)) return false;
        if (args.Length >= 4) if (!TryParseBool(args[3], out allowOverflow)) return false;

        var radiant = SupportRadiant.HasFlag() && makeRadiant;
        if (radiant)
        {
            var slime = ident.TryCast<SlimeDefinition>();
            if (slime == null || !slime.GetRadiantAppearance()) return SendError(Tr("cmd.give.notradiant", itemName));
        }

        var requestInfo = new StarlightSlotItemInfo()
        {
            IdentifiableType = ident,
            IsRadiant = radiant
        };
        var optimalSlot = InventoryEUtil.GetStarlightSlotItemPossibleSlot(requestInfo);
        if (optimalSlot == -1)
            return SendError(Tr("cmd.give.nospace"));
        
        var finalInfo = InventoryEUtil.GetStarlightSlotInfo(optimalSlot);
        finalInfo.IsRadiant = radiant;
        finalInfo.IdentifiableType = ident;
        finalInfo.Count += amount;
        InventoryEUtil.SetStarlightSlotInfo(optimalSlot,finalInfo);
        
        InventoryEUtil.AddSlotItemCount(optimalSlot, 0, allowOverflow);

        SendMessage(Tr("cmd.give.success",amount,itemName));
        return true;
    }
}