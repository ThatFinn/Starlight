using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Ranch;

namespace Starlight.Commands;

internal sealed class VacStylesCommand : StarlightCommand
{
    private static readonly List<string> Actions = new() { "list", "unlock", "set", "reset" };

    public override string ID => "vacstyles";
    public override string Usage => "vacstyles <list/unlock/set/reset> [style/*]";
    public override string Description => Tr("cmd.vacstyles.description");
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return Actions;
        if (argIndex != 1 || args == null || args.Length == 0) return null;

        var action = args[0].ToLowerInvariant();
        if (action is not ("unlock" or "set")) return null;

        var names = Styles().Select(style => style.name).OrderBy(name => name).ToList();
        if (action == "unlock") names.Add("*");
        return names;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1, 2)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        var action = args[0].ToLowerInvariant();
        if (!Actions.Contains(action)) return SendNotValidOption(args[0]);

        var director = GetAny<PaletteDirector>();
        var target = GetAny<VacPaletteTarget>();
        if (director == null || target == null) return SendError(Tr("cmd.vacstyles.unavailable"));

        var styles = Styles();
        if (styles.Count == 0) return SendError(Tr("cmd.vacstyles.none"));

        switch (action)
        {
            case "list":
                if (args.Length > 1) return SendUsage();
                return List(director, target, styles);

            case "reset":
                if (args.Length > 1) return SendUsage();
                director.SetPalette(target, target.DefaultPalette);
                SendMessage(Tr("cmd.vacstyles.reset"));
                return true;

            default:
                if (args.Length < 2) return SendUsage();
                return action == "unlock" ? Unlock(director, styles, args[1]) : Apply(director, target, styles, args[1]);
        }
    }

    private static List<VacPalette> Styles() => GetAll<VacPalette>().Where(style => style != null).ToList();
    
    private static VacPalette Find(IEnumerable<VacPalette> styles, string text)
    {
        var wanted = Compact(text);

        return styles.FirstOrDefault(style => Compact(style.name) == wanted)
               ?? styles.FirstOrDefault(style => Compact(Shown(style)) == wanted);
    }

    private static string Compact(string text) => text.Replace(" ", "").Replace("_", "").ToLowerInvariant();

    private static string Shown(VacPalette style)
    {
        try { return style.DisplayName.GetLocalizedString(); }
        catch { return style.name; }
    }

    private bool List(PaletteDirector director, VacPaletteTarget target, List<VacPalette> styles)
    {
        var current = director.GetPaletteForTarget(target);

        foreach (var style in styles.OrderBy(style => style.name))
        {
            var marker = current != null && current.Pointer == style.Pointer ? ">" : " ";
            var locked = director.IsOwned(style) ? string.Empty : " " + Tr("cmd.vacstyles.locked");
            SendMessage($"{marker} {style.name} ({Shown(style)}){locked}");
        }

        return true;
    }

    private bool Unlock(PaletteDirector director, List<VacPalette> styles, string text)
    {
        if (text == "*")
        {
            var count = styles.Count(style => !director.IsOwned(style) && director.SetOwned(style));
            SendMessage(Tr("cmd.vacstyles.unlockedall", count));
            return true;
        }

        var style = Find(styles, text);
        if (style == null) return SendError(Tr("cmd.vacstyles.unknown", text));

        if (director.IsOwned(style)) return SendError(Tr("cmd.vacstyles.owned", style.name));

        director.SetOwned(style);
        SendMessage(Tr("cmd.vacstyles.unlocked", style.name));
        return true;
    }

    private bool Apply(PaletteDirector director, VacPaletteTarget target, List<VacPalette> styles, string text)
    {
        var style = Find(styles, text);
        if (style == null) return SendError(Tr("cmd.vacstyles.unknown", text));
        
        if (!director.IsOwned(style)) director.SetOwned(style);

        director.SetPalette(target, style);
        SendMessage(Tr("cmd.vacstyles.set", style.name));
        return true;
    }
}