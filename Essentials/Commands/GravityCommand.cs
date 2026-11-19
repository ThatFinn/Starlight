namespace Starlight.Commands;

internal class GravityCommand : StarlightCommand
{
    public override string ID => "gravity";
    public override string Usage => "gravity <x> <y> <z>";
    public override CommandType type => CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(3,3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        Vector3 gravBase;
        if (!TryParseVector3(args[0], args[1], args[2], out gravBase)) return false;
        try
        {
            Physics.gravity = -gravBase * 9.81f;
            SendMessage(Tr("cmd.gravity.success",args[0],args[1],args[2]));
            return true;
        }
        catch { return SendNotValidVector3(args[0],args[1],args[2]); }
    }
}