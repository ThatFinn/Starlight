using Il2CppMonomiPark.SlimeRancher.Damage;
using Il2CppMonomiPark.SlimeRancher.World;

namespace Starlight.Commands;

internal class ReplaceCommand : StarlightCommand
{
    public override string ID => "replace";
    public override string Usage => "replace <object>";
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return LookupEUtil.GetFilteredIdentifiableTypeStringListByPartialName(args == null ? null : args[0], true, MAX_AUTOCOMPLETE.Get());
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        
        string identifierTypeName = args[0];
        IdentifiableType type = LookupEUtil.GetIdentifiableTypeByName(identifierTypeName);
        if (type == null) return SendNotValidIdentType(identifierTypeName);
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();

        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            var gameobject = hit.collider.gameObject;
            string oldObjectName = "";
            bool isValid = false;
            if (gameobject.GetComponent<Identifiable>())
            {
                isValid = true;
                oldObjectName = gameobject.GetComponent<Identifiable>().identType.GetName();
                //Remove old one
                DeathHandler.Kill(gameobject, killDamage);

            }
            else if (gameobject.GetComponentInParent<Gadget>())
            {
                isValid = true;
                oldObjectName = gameobject.GetComponentInParent<Gadget>().identType.GetName();
                //Remove old one
                gameobject.GetComponentInParent<Gadget>().DestroyGadget();
            }
            if (isValid)
            {
                //Add new one
                GameObject spawned = null;
                if (type.TryCast<GadgetDefinition>()!=null) spawned = type.TryCast<GadgetDefinition>().SpawnGadget(hit.point,Quaternion.identity).GetGameObject();
                else spawned = type.SpawnActor(hit.point, Quaternion.identity);
                Vector3 position = gameobject.transform.position;
                Quaternion rotation = gameobject.transform.rotation;
                spawned.transform.position = position;
                spawned.transform.rotation = rotation;
                SendMessage(Tr("cmd.replace.success",oldObjectName,type.GetName()));
                return true;
            }
            return SendNotLookingAtValidObject();
        }
        return SendNotLookingAtAnything();
    }
}