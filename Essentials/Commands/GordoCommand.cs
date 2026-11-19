namespace Starlight.Commands;

internal class GordoCommand : StarlightCommand
{
    public override string ID => "gordo";
    public override string Usage => "gordo <action> [value]";
    public override CommandType type => CommandType.Cheat;

    List<string> arg0List = new List<string> { "size", "eatcount"};
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return arg0List;
        if (argIndex == 1)
            switch (args[0])
            {
                case "size": return new List<string> { "0.25", "0.5", "1", "1.5", "2", "5" };
                case "eatcount": return new List<string> { "1", "5", "10", "45" };
            }
        return null;
    }
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,2)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        if (!arg0List.Contains(args[0])) return SendNotValidOption(args[0]);
        
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            GordoIdentifiable gordo = hit.collider.gameObject.GetComponent<GordoIdentifiable>();
            GordoEat eat = hit.collider.gameObject.GetComponent<GordoEat>();
            if (gordo != null)
            {
                switch (args[0])
                {
                    case "size":
                        if (args.Length == 1) { SendMessage(Tr("cmd.gordo.size.show",gordo.identType.GetName(),eat._initScale/4)); return true; }
                        float size;
                        if (!TryParseFloat(args[1], out size, 0, false)) return false;
                        eat._initScale = size*4;
                        SendMessage(Tr("cmd.gordo.size.edit",gordo.identType.GetName(),size));
                        return true;
                    case "eatcount":
                        if (args.Length == 1) { SendMessage(Tr("cmd.gordo.eat.show",gordo.identType.GetName(),eat.GetEatenCount())); return true; }
                        int amount;
                        if (!TryParseInt(args[1], out amount, 1, true,eat.GetTargetCount(),true)) return false;
                        eat.SetEatenCount(amount);
                        if(amount>=eat.GetTargetCount()) eat.ImmediateReachedTarget();
                        SendMessage(Tr("cmd.gordo.eat.edit",gordo.identType.GetName(),amount));
                        return true;
                }
                
            }
            return SendNotLookingAtValidObject();
        }
        return SendNotLookingAtAnything();
    }
}