using Starlight.Managers;

namespace Starlight.Commands;

internal class GraphicsCommand : StarlightCommand
{
    public override string ID => "graphics";
    public override string Usage => "graphics <mode>";
    public override CommandType type => CommandType.Fun;
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        //return new List<string>(){"InMaintenance"};
        if (argIndex == 0)
        {
            var list = new List<string> { "NORMAL" };
            list.AddRange(StarlightVolumeProfileManager.Presets.Keys);
            return list;
        }
        return null;
    }

    internal static GameObject rangeLightInstance;
    internal static bool activated = false;
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,1)) return SendUsage();
        
        if (args[0] == "NORMAL")
        {
            StarlightVolumeProfileManager.DisableProfile();
            SendMessage(Tr("cmd.graphics.success","NORMAL"));
            return true;
        }

        foreach (var preset in StarlightVolumeProfileManager.Presets.Keys)
            if (preset == args[0])
            {
                StarlightVolumeProfileManager.DisableProfile();
                StarlightVolumeProfileManager.EnableProfile(preset);
                SendMessage(Tr("cmd.graphics.success",preset));
                return true;
            }
        
        StarlightVolumeProfileManager.DisableProfile();
        SendMessage(Tr("cmd.graphics.success","NORMAL"));
        return true;
    }
}