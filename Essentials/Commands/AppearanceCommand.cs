namespace Starlight.Commands;

internal class AppearanceCommand: StarlightCommand
{
    public override string ID => "appearance";
    public override string Usage => "appearance";
    public override CommandType type => CommandType.Miscellaneous | CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,0)) return SendNoArguments();
        if (!inGame) return SendLoadASaveFirst();

        var cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();

        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            if(!SpawnEUtil.RotateSlimeActorAppearance(hit.transform.GetComponent<IdentifiableActor>()))
                return SendNotLookingAtValidObject();
            SendMessage(Tr("cmd.appearance.success"));
            return true;
        }
        return SendNotLookingAtAnything();
    }
}