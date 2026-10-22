using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Tutorial;

namespace Starlight.Commands;

internal sealed class TutorialsCommand : StarlightCommand
{
    public override string ID => "tutorials";
    public override string Usage => "tutorials <list/get/complete/clear> [id]";
    public override CommandType type => CommandType.Cheat;

    readonly List<string> arg0List = new List<string> { "list", "get", "complete", "clear" };

    static List<TutorialDefinition> AllTutorials()
    {
        var list = new List<TutorialDefinition>();
        var director = sceneContext ? sceneContext.TutorialDirector : null;
        if (!director || director.Tutorials == null) return list;
        foreach (var tutorial in director.Tutorials.items)
            if (tutorial) list.Add(tutorial);
        return list;
    }

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return arg0List;
        if (argIndex == 1)
        {
            if (args[0] == "list") return null;
            var list = AllTutorials().Select(x => x.name).ToList();
            if (args[0] != "get") list.Add("*");
            return list;
        }
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0, 2)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        string action = args == null || args.Length == 0 ? "list" : args[0];
        if (!arg0List.Contains(action)) return SendNotValidOption(action);

        var tutorials = AllTutorials();
        if (tutorials.Count == 0) return SendErrorTr("cmd.tutorials.nodirector");

        if (action == "list")
        {
            var text = "";
            foreach (var tutorial in tutorials)
                text += $"\n{tutorial.name} ({(IsComplete(tutorial) ? Tr("cmd.tutorials.completed") : Tr("cmd.tutorials.notcompleted"))})";
            SendMessage(Tr("cmd.tutorials.successlist", text));
            return true;
        }

        if (args.Length < 2) return SendUsage();
        string name = args[1];

        if (name == "*")
        {
            if (action == "get") return SendErrorTr("cmd.tutorials.nowildcard");
            int changed = 0;
            foreach (var tutorial in tutorials)
                if (Apply(action, tutorial)) changed++;
            ReInitTutorials();
            SendMessage(Tr(action == "clear" ? "cmd.tutorials.successclearall" : "cmd.tutorials.successcompleteall", changed));
            return true;
        }

        var def = tutorials.FirstOrDefault(x => x.name.ToLower().Equals(name.ToLower()));
        if (def == null) return SendErrorTr("cmd.tutorials.notvalidtutorial", name);

        switch (action)
        {
            case "get":
                SendMessage(Tr(IsComplete(def) ? "cmd.tutorials.successgetcompleted" : "cmd.tutorials.successgetnotcompleted", def.name));
                return true;
            case "complete":
                if (IsComplete(def)) return SendErrorTr("cmd.tutorials.alreadycompleted", def.name);
                Apply(action, def);
                SendMessage(Tr("cmd.tutorials.successcomplete", def.name));
                return true;
            case "clear":
                if (!IsComplete(def)) return SendErrorTr("cmd.tutorials.alreadynotcompleted", def.name);
                Apply(action, def);
                ReInitTutorials();
                SendMessage(Tr("cmd.tutorials.successclear", def.name));
                return true;
        }
        return false;
    }

    static bool IsComplete(TutorialDefinition tutorial)
    {
        try { return tutorial.IsComplete(); }
        catch { return false; }
    }

    static bool Apply(string action, TutorialDefinition tutorial)
    {
        try
        {
            switch (action)
            {
                case "complete":
                    if (IsComplete(tutorial)) return false;
                    tutorial.RegisterComplete();
                    return true;
                case "clear":
                    return ClearRecord(tutorial);
            }
        }
        catch (Exception e) { LogError(e); }
        return false;
    }
    
    static bool ClearRecord(TutorialDefinition tutorial)
    {
        if (tutorial.CompletionEvent == null) return false;
        var record = sceneContext.GameModel.GetEventRecordModel();
        if (record == null) return false;
        var eventKey = tutorial.CompletionEvent.KeyPrefix;
        var dataKey = tutorial.ReferenceId;
        if (string.IsNullOrEmpty(eventKey) || string.IsNullOrEmpty(dataKey)) return false;
        if (!record.table.ContainsKey(eventKey)) return false;
        return record.table[eventKey].Remove(dataKey);
    }
    
    static void ReInitTutorials()
    {
        try { sceneContext.TutorialDirector.InitTutorials(); }
        catch (Exception e) { LogError(e); }
    }
}