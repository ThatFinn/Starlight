using Starlight.Patches.Options;

namespace Starlight.Commands;

internal class MaxFpsCommand : StarlightCommand
{
    public override string ID => "maxfps";
    public override string Usage => "maxfps <target>";
    public override CommandType type => CommandType.Common;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return new List<string> { "20","30","60","120","240","480","500","600","700","800","900","1000","2500","5000","10000"};
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,1)) return SendNoArguments();
        
        int duration = -1;
        if(args!=null) if(!TryParseInt(args[0], out duration, 5, true)) return false;
        
        OptionsUIRootApplyPatch.CustomMaxFPS = duration;
        SendMessageTr("cmd.maxfps.success",duration);
        OptionsUIRootApplyPatch.Apply();
        return true;
    }
}