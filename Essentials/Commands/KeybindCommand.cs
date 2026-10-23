using System;
using System.Linq;
using Starlight.Enums;
using Starlight.Managers;

namespace Starlight.Commands;

/// <summary>
/// Lists and changes keybinds mods registered
/// </summary>
internal sealed class KeybindCommand : StarlightCommand
{
    public override string ID => "keybind";
    public override string Usage => "keybind [list | set <id> <key> | press <id> | reset <id>]";
    public override CommandType type => CommandType.Binding;

    public override bool Execute(string[] args)
    {
        var action = args is { Length: > 0 } ? args[0].ToLowerInvariant() : "list";

        switch (action)
        {
            case "list":
                return List();

            case "set":
                if (args is not { Length: > 2 }) return SendUsage();
                if (!TryParseLKey(args[2], out var key)) return false;

                return Report(StarlightKeybindManager.Rebind(args[1], key), args[1]);

            case "press":
                if (args is not { Length: > 1 }) return SendUsage();

                return Report(StarlightKeybindManager.BeginCapture(args[1]), args[1]);

            case "reset":
                if (args is not { Length: > 1 }) return SendUsage();

                return Report(StarlightKeybindManager.Reset(args[1]), args[1]);

            default:
                return SendUsage();
        }
    }

    private bool List()
    {
        var keybinds = StarlightKeybindManager.All();

        if (keybinds.Count == 0)
        {
            SendMessage("No mod registered a keybind.");
            return true;
        }

        SendMessage($"{keybinds.Count} mod keybind(s):");

        foreach (var keybind in keybinds)
        {
            var moved = keybind.Key != keybind.Default ? $"  (default: {keybind.Default})" : string.Empty;
            var owner = string.IsNullOrWhiteSpace(keybind.Owner) ? string.Empty : $"{keybind.Owner}: ";

            SendMessage($"  {keybind.Id,-28} {keybind.Key,-12} {owner}{keybind.Name}{moved}");
        }

        return true;
    }

    private bool Report(StarlightError error, string id)
    {
        switch (error)
        {
            case StarlightError.NoError:
                return true;

            case StarlightError.DoesntExist:
                SendMessage($"There is no keybind called '{id}'. 'keybind list' lists all keybinds.");
                return false;

            default:
                SendMessage($"'{id}': {error}");
                return false;
        }
    }

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return ["list", "set", "press", "reset"];
        
        if (argIndex == 1 && args is { Length: > 0 } && args[0].ToLowerInvariant() is "set" or "press" or "reset")
            return StarlightKeybindManager.All().Select(keybind => keybind.Id).ToList();

        if (argIndex == 2 && args is { Length: > 0 } && string.Equals(args[0], "set", StringComparison.OrdinalIgnoreCase))
            return LookupEUtil.GetLKeyStringListByPartialName(args.Length > 2 ? args[2] : string.Empty, true, MAX_AUTOCOMPLETE.Get());

        return [];
    }
}