using System.Globalization;

namespace Starlight.Commands;

internal sealed class TimeCommand : StarlightCommand
{
    public override string ID => "time";
    public override string Usage => "time <get/set> [day] [hour]";
    public override CommandType type => CommandType.Cheat;

    readonly List<string> arg0List = new List<string> { "get", "set" };

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return arg0List;
        if (argIndex >= 1 && args[0] == "set")
            return new List<string> { "0:00", "6:00", "9:00", "12:00", "18:00", "22:00" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0, 3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        var timeDirector = sceneContext.TimeDirector;
        if (!timeDirector || timeDirector._worldModel == null) return SendErrorTr("cmd.time.nodirector");

        string action = args == null || args.Length == 0 ? "get" : args[0];
        if (!arg0List.Contains(action)) return SendNotValidOption(action);

        if (action == "get")
        {
            if (args != null && args.Length > 1) return SendErrorToManyArgs(action);
            SendMessage(Tr("cmd.time.successget", timeDirector.CurrDay(), FormatHour(timeDirector.CurrHour())));
            return true;
        }

        if (args.Length < 2) return SendUsage();

        int day = timeDirector.CurrDay();
        string hourArg = args[1];
        if (args.Length == 3)
        {
            if (!TryParseInt(args[1], out day, 1, true)) return false;
            hourArg = args[2];
        }

        if (!TryParseHour(hourArg, out var hour)) return SendErrorTr("cmd.time.notvalidhour", hourArg);
        
        timeDirector._worldModel.worldTime = (day - 1) * (double)TimeDirector.SECS_PER_DAY + hour * (double)TimeDirector.SECS_PER_HOUR;
        SendMessage(Tr("cmd.time.successset", day, FormatHour(hour)));
        return true;
    }

    bool TryParseHour(string input, out float hour)
    {
        hour = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var split = input.Split(':');
        if (split.Length > 2) return false;
        if (!float.TryParse(split[0], NumberStyles.Float, CultureInfo.InvariantCulture, out hour)) return false;
        if (split.Length == 2)
        {
            if (!float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var minutes)) return false;
            if (minutes < 0 || minutes >= TimeDirector.MINS_PER_HOUR) return false;
            hour += minutes / TimeDirector.MINS_PER_HOUR;
        }
        return hour >= 0 && hour < TimeDirector.HOURS_PER_DAY;
    }

    static string FormatHour(float hour)
    {
        int hours = Mathf.FloorToInt(hour);
        int minutes = Mathf.FloorToInt((hour - hours) * TimeDirector.MINS_PER_HOUR);
        return $"{hours:00}:{minutes:00}";
    }
}