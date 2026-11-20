namespace Starlight.Commands;

internal class SpawnCommand : StarlightCommand
{
    public override string ID => "spawn";
    public override string Usage => "spawn <object> [amount] [radiant(false/true)]";
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return LookupEUtil.GetFilteredIdentifiableTypeStringListByPartialName(args == null ? null : args[0], true, MAX_AUTOCOMPLETE.Get());
        if (argIndex == 1) return new List<string> { "1", "5", "10", "20", "30", "50" };
        if (argIndex == 2) return new List<string> { "true", "false" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        
        var cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();

        string identifierTypeName = args[0];
        var def = LookupEUtil.GetIdentifiableTypeByName(identifierTypeName);
        if (!def) return SendNotValidIdentType(identifierTypeName);
        
        var makeRadiant = false;
        int amount = 1;
        if (args.Length >= 2) if(!TryParseInt(args[1], out amount,1, true)) return false;
        if (args.Length >= 3) if (!TryParseBool(args[2], out makeRadiant)) return false;

        var radiant = SupportRadiant.HasFlag() && makeRadiant;
        if (radiant)
        {
            var slime = def.TryCast<SlimeDefinition>();
            if (slime == null || !slime.GetRadiantAppearance()) return SendError(Tr("cmd.spawn.notradiant", def.GetName()));
        }

        for (int i = 0; i < amount; i++)
        {
            if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
            {
                try
                {
                    GameObject spawned = null;
                    if (def.TryCast<GadgetDefinition>()!=null) spawned = def.TryCast<GadgetDefinition>().SpawnGadget(hit.point,Quaternion.identity).GetGameObject();
                    else spawned = def.SpawnActor(hit.point, Quaternion.identity);
                    spawned.transform.position = hit.point + hit.normal * PhysicsUtil.CalcRad(spawned.GetComponent<Collider>());
                    var delta = -(hit.point - cam.transform.position).normalized;
                    spawned.transform.rotation = Quaternion.LookRotation(delta, hit.normal);
                    if (radiant)
                        SpawnEUtil.SetAnyRadiantSlimeAppearance(spawned.GetComponent<IdentifiableActor>());
                } catch { }
            }
        }
        SendMessage(Tr("cmd.spawn.success",amount,def.GetName()));
        return true;
    }
}