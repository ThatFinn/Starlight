namespace Starlight.Commands;

internal class FlingCommand : StarlightCommand
{
    public override string ID => "fling";
    public override string Usage => "fling <strength>";
    public override CommandType type => CommandType.Fun | CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!inGame) return SendLoadASaveFirst();
        if (!args.IsBetween(1,1)) return SendUsage();
        
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb == null) return SendNotLookingAtValidObject();
            Transform transform = hit.transform;
            
            float strength = 0;
            if (!TryParseFloat(args[0], out strength)) return false;

            Vector3 cameraPosition = cam.transform.position;
            Vector3 moveDirection = transform.position - cameraPosition;
            moveDirection.Normalize();
            rb.velocity += (moveDirection * strength) + Vector3.up;
            SendMessage(Tr("cmd.fling.success",strength));
            return true;
        }
        return SendNotLookingAtAnything();
    }
}