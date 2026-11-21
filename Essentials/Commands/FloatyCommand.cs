using System.Collections;

namespace Starlight.Commands;

internal class FloatyCommand : StarlightCommand
{
    public override string ID => "floaty";
    public override string Usage => "floaty [duration]";
    public override CommandType type => CommandType.Fun | CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return new List<string> { "10", "20", "50", "100"};
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        
        float duration = -1;
        if(args != null && args.Length != 0) if(!TryParseFloat(args[0], out duration, 0, false)) return false;
        
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            if (hit.rigidbody == null) return SendNotLookingAtValidObject();
            if (duration > 0)
            {
                StartCoroutine(TimeGravity(hit, duration));
                SendMessage(Tr("cmd.floaty.successduration",duration));
                return true;
            }
            SendMessage(Tr("cmd.floaty.successtoggle"));
            hit.rigidbody.useGravity = !hit.rigidbody.useGravity;
            return true;
        }
        return SendNotLookingAtAnything();
    }

    private IEnumerator TimeGravity(RaycastHit hit, float duration)
    {
        hit.rigidbody.useGravity = false;
        yield return new WaitForSecondsRealtime(duration);
        hit.rigidbody.useGravity = true;
    }
}