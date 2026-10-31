using System;
using System.Globalization;
using System.Linq;

namespace Starlight.Commands;

internal class CurrencyCommand : StarlightCommand
{
    internal string refID = "";
    internal string name = "";

    private static readonly List<string> Verbs = new() { "add", "remove", "set", "get" };
    private static readonly List<string> Amounts = new() { "100", "1k", "10k", "100k", "1m", "10m", "-1k" };

    public override string ID => name + "s";
    public override string Usage => $"{name}s <amount> | {name}s <add/remove/set> <amount> | {name}s get";
    public override CommandType type => CommandType.DontLoad | CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return Verbs.Concat(Amounts).ToList();

        if (argIndex == 1 && args != null && args.Length > 0)
        {
            var verb = args[0].ToLowerInvariant();
            if (Verbs.Contains(verb) && verb != "get") return Amounts;
        }

        return null;
    }

    public override string Description => Tr($"cmd.currency.description", name);
    public override string ExtendedDescription
    {
        get
        {
            string key = $"cmd.currency.extendeddescription";
            string translation = Tr(key, name);
            return key == translation ? Description : translation;
        }
    }
    
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1, 2)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        var first = args[0].ToLowerInvariant();
        var hasVerb = Verbs.Contains(first);
        var verb = hasVerb ? first : "add";
        var amountText = hasVerb ? (args.Length > 1 ? args[1] : null) : args[0];

        if (!hasVerb && args.Length > 1) return SendUsage();

        var current = CurrencyEUtil.GetCurrency(refID);
        if (current < 0) return SendError(Tr("cmd.currency.error", name));

        if (verb == "get")
        {
            if (args.Length > 1) return SendUsage();

            SendMessage(Tr("cmd.currency.balance", current, name, CurrencyEUtil.GetCurrencyEverCollected(refID)));
            return true;
        }

        if (amountText == null) return SendUsage();
        if (!TryParseAmount(amountText, out var amount)) return SendNotValidInt(amountText);

        var wanted = verb switch
        {
            "set"    => amount,
            "remove" => (long) current - amount,
            _        => (long) current + amount
        };
        
        var balance = (int) Math.Clamp(wanted, 0L, int.MaxValue);

        var applied = verb == "set"
            ? CurrencyEUtil.SetCurrency(refID, balance)
            : balance == current || CurrencyEUtil.AddCurrency(refID, balance - current);

        if (!applied) return SendError(Tr("cmd.currency.error", name));
        
        SendMessage(Tr("cmd.currency.balance", balance, name, CurrencyEUtil.GetCurrencyEverCollected(refID)));
        return true;
    }

    private static bool TryParseAmount(string text, out long value)
    {
        value = 0;

        var cleaned = text.Trim().Replace("_", "").Replace(',', '.').ToLowerInvariant();
        if (cleaned.Length == 0) return false;

        var multiplier = cleaned[^1] switch { 'k' => 1e3, 'm' => 1e6, 'b' => 1e9, _ => 1d };
        if (multiplier != 1d) cleaned = cleaned[..^1];

        if (!double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)) return false;

        var scaled = Math.Round(number * multiplier);
        if (double.IsNaN(scaled) || double.IsInfinity(scaled)) return false;

        value = (long) Math.Clamp(scaled, -(double) int.MaxValue, (double) int.MaxValue);
        return true;
    }
}