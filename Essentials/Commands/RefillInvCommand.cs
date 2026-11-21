namespace Starlight.Commands;

internal class RefillInvCommand : StarlightCommand
{
    public override string ID => "refillinv";
    public override string Usage => "refillinv [slot]";
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
            return new List<string> { "1", "2", "3", "4" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0, 1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        int numberOfSlots = InventoryEUtil.GetAllSlotCount() - 1;
        int slotToFill = -1;

        if(args != null && args.Length != 0)
            if(!TryParseInt(args[0], out slotToFill,0, false,slotToFill)) return false;
            else try
                {
                    slotToFill = int.Parse(args[0]);
                    if (slotToFill <= 0) return SendError(Tr("cmd.error.notintabove", args[0]));
                    if (slotToFill > slotToFill) return SendError(Tr("cmd.refillinv.error.slotdoesntexist", numberOfSlots));
                    slotToFill -= 1;
                }
                catch { return SendNotValidInt(args[0]); }
        if (args == null || args.Length == 0)
        {
            foreach (var slotIndex in InventoryEUtil.GetUnlockedSlots())
                InventoryEUtil.SetSlotItemCount(slotIndex,InventoryEUtil.GetSlotMaxItemCount(slotIndex));

            SendMessage(Tr("cmd.refillinv.success"));
            return true;
        }

        if (!InventoryEUtil.GetIsSlotUnlocked(slotToFill))
            return SendError(Tr("cmd.refillinv.error.slotnotunlocked", slotToFill + 1));

        if (InventoryEUtil.GetIsSlotEmpty(slotToFill))
            return SendError(Tr("cmd.refillinv.error.slotempty", slotToFill + 1));

        InventoryEUtil.SetSlotItemCount(slotToFill,InventoryEUtil.GetSlotMaxItemCount(slotToFill));
        SendMessage(Tr("cmd.refillinv.successsingle", slotToFill + 1));
        return true;
    }
}