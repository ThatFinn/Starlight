using Starlight.Patches.Options;

namespace Starlight.Commands;

internal class FlatLookCommand : StarlightCommand
{
    public override string ID => "flatlook";
    public override string Usage => "flatlook";
    public override CommandType type => CommandType.Fun;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0, 0)) return SendNoArguments();
        if(OptionsUIRootApplyPatch.CustomMasterTextureLimit==-1)
        {
            OptionsUIRootApplyPatch.CustomMasterTextureLimit = int.MaxValue;
            SendMessage(Tr("cmd.flatlook.success"));
        }
        else
        {
            OptionsUIRootApplyPatch.CustomMasterTextureLimit = -1;
            SendMessage(Tr("cmd.flatlook.success2"));
        }
        OptionsUIRootApplyPatch.Apply();
        return true;
    }
}