namespace Starlight.Commands;

internal class ClearInventoryCommand : StarlightCommand
{
    public override string ID => "clearinv";
    public override string Usage => "clearinv [slot]";
    public override CommandType type => CommandType.Cheat;
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
            return new List<string> { "1", "2", "3", "4", "5", "6"};
        return null;
    }
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        int numberOfSlots = InventoryEUtil.GetAllSlotCount() - 1;
        int slotToClear = -1;
        if (args != null && args.Length != 0)
        {
            try { slotToClear = int.Parse(args[0]); }
            catch { SendError(Tr("cmd.error.notvalidint",args[0])); return false; }
            if (slotToClear<=0) return SendNotValidInt(args[0]);
            if(slotToClear>numberOfSlots) return SendError(Tr("cmd.clearinv.error.slotdoesntexist",numberOfSlots));
            slotToClear -= 1;
        }

        if(slotToClear==-1)
        {
            foreach (var slot in InventoryEUtil.GetUnlockedSlots())
                InventoryEUtil.ClearSlot(slot);
            SendMessage(Tr("cmd.clearinv.success"));
            return true;
        }

        if (!InventoryEUtil.GetIsSlotUnlocked(slotToClear))
            return SendError(Tr("cmd.clearinv.error.slotnotunlocked",slotToClear+1));
        
        InventoryEUtil.ClearSlot(slotToClear);
        SendMessage(Tr("cmd.clearinv.successsingle",slotToClear+1));
        return true;
    }
}