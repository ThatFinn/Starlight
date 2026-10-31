using System.Collections;
using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Creators;
namespace Starlight.Commands;

internal class PediaCommand : StarlightCommand
{
    private const int BatchSize = 40;

    private static bool running;

    public override string ID => "pedia";
    public override string Usage => "pedia <lock/unlock> <entry/*/modded> [show popup(true/false)]";
    List<string> arg0List = new List<string> { "unlock", "lock"};
    public override CommandType type => CommandType.Cheat;
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return arg0List;
        if (argIndex == 1)
        {
            var identifiableTypeGroup = Resources.FindObjectsOfTypeAll<PediaEntry>().Select(x => x.name);
            List<string> list = identifiableTypeGroup.ToList();
            list.Add("*");
            list.Add("modded");
            return list;
        }
        if (argIndex == 2) return new List<string> { "true","false" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(2,3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        if (!arg0List.Contains(args[0])) return SendNotValidOption(args[0]);
        bool showPopup = args[1] != "*" && args[1] != "modded";
        if (args.Length == 3) if (!TryParseBool(args[2], out showPopup)) return false;
        if (args[1] == "*")
        {
            if (running) return SendError(Tr("cmd.pedia.busy"));

            var entries = Resources.FindObjectsOfTypeAll<PediaEntry>().Where(entry => entry != null).ToList();
            StartCoroutine(ChangeAll(args[0], entries, showPopup));
            return true;
        }
        if (args[1] == "modded")
        {
            var director = sceneContext.PediaDirector;
            foreach (var created in PrismPediaCreatorV02.CreatedEntries)
            {
                if (created == null) continue;

                if (args[0] == "unlock" && !director.IsUnlocked(created)) director.Unlock(created, showPopup);
                else if (args[0] == "lock" && director.IsUnlocked(created)) director.DebugReLock(created);
            }

            SendMessage(Tr(args[0] == "lock" ? "cmd.pedia.successmoddedlock" : "cmd.pedia.successmoddedunlock"));
            return true;
        }

        string identifierTypeName = args[1];
        PediaEntry id = Resources.FindObjectsOfTypeAll<PediaEntry>().FirstOrDefault(x => x.name.ToLower().Equals(identifierTypeName.ToLower()));
        if (id == null) return SendNotValidPedia(identifierTypeName);
        string itemName = id.name;

        switch (args[0])
        {
            case "lock":
                if (!sceneContext.PediaDirector.IsUnlocked(id))
                    return SendError(Tr("cmd.pedia.errorlock",itemName));
                sceneContext.PediaDirector.DebugReLock(id);
                SendMessage(Tr("cmd.pedia.successlock",itemName)); 
                break;
            case "unlock":
                if (sceneContext.PediaDirector.IsUnlocked(id))
                    return SendError(Tr("cmd.pedia.errorunlock",itemName));
                sceneContext.PediaDirector.Unlock(id,showPopup);
                SendMessage(Tr("cmd.pedia.successunlock",itemName)); 
                break;
        }
        return false;
    }

    private IEnumerator ChangeAll(string action, List<PediaEntry> entries, bool showPopup)
    {
        running = true;
        var changed = 0;
        var failed = 0;

        try
        {
            var director = sceneContext.PediaDirector;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                try
                {
                    if (entry == null) continue;

                    if (action == "unlock" && !director.IsUnlocked(entry))
                    {
                        director.Unlock(entry, showPopup);
                        changed++;
                    }
                    else if (action == "lock" && director.IsUnlocked(entry))
                    {
                        director.DebugReLock(entry);
                        changed++;
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    LogWarning($"Pedia: could not {action} '{entry?.name}': {ex.Message}");
                }

                if ((i + 1) % BatchSize == 0) yield return null;
            }
        }
        finally
        {
            running = false;
        }

        SendMessage(Tr(action == "lock" ? "cmd.pedia.successalllock" : "cmd.pedia.successallunlock"));
        SendMessage(Tr("cmd.pedia.alldone", changed, failed));
    }
}