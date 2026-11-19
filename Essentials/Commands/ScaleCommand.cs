using Il2CppMonomiPark.SlimeRancher.World;

namespace Starlight.Commands;

internal class ScaleCommand: StarlightCommand
{
    public override string ID => "scale";
    public override string Usage => "scale <x> <y> <z>";
    public override CommandType type => CommandType.Miscellaneous | CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        Vector3 scale;
        if (!TryParseVector3(args[0], args[1], args[2], out scale)) return false;
        
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            var gameobject = hit.collider.gameObject;
            if (gameobject.GetComponent<Identifiable>()) gameobject.transform.localScale = scale;
            else if (gameobject.GetComponentInParent<Gadget>())
            {
                try { gameobject.GetComponentInParent<Gadget>().transform.localScale = scale; }
                catch { }
            }
            else if (hit.collider.gameObject.GetComponent<GordoEat>() != null)
            {
                return SendError(Tr("cmd.scale.usegordocmd"));
            }
            else return SendNotLookingAtValidObject();
            SendMessage(Tr("cmd.scale.success"));
            return true;
            
        }
        return SendNotLookingAtAnything();
    }
}