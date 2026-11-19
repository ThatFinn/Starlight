namespace Starlight.Commands;

internal class TimeScaleCommand : StarlightCommand
{
    public override string ID => "timescale";
    public override string Usage => "timescale <scale>";
    public override CommandType type => CommandType.Cheat;
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return new List<string> { ".25", ".5", "1", "2", "5" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        float speed;
        if (!TryParseFloat(args[0], out speed, 0.25f, true, 15f)) return false;

        NativeEUtil.CustomTimeScale = speed;
        SendMessage(Tr("cmd.timescale.success",speed));
        return true;
    }
}