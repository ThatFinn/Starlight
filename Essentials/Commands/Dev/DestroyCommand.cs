using Starlight.Managers;

namespace Starlight.Commands.Dev;

internal class DestroyCommand : StarlightCommand
{
    public override string ID => "destroy";
    public override string Usage => "destroy";
    public override CommandType type => CommandType.DevOnly;
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,0)) return SendNoArguments();
        if (!inGame) return SendLoadASaveFirst();
        
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        GameObject gameObject = null;
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
            gameObject = hit.collider.gameObject;
        else return SendNotLookingAtAnything();
        if (gameObject)
        {
            StarlightEntryPoint.Instance.RegisterFor_PlayerCameraDisableUseOcclusionCulling();
            GameObject.Destroy(gameObject);
            SendMessage(Tr("cmd.destroy.success"));
            return true;
        }

        return SendNotLookingAtValidObject();

    }
}