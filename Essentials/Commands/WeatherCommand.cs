using System;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Weather;
namespace Starlight.Commands;

internal class WeatherCommand : StarlightCommand
{
    public override string ID => "weather";
    public override string Usage => "weather <list/modify/set> [state] [action/intensity]";
    public override CommandType type => CommandType.Cheat;
    
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        switch (argIndex)
        {
            case 0:
                return new List<string> { "list", "modify", "set" };
            case 1:
                switch (args[0])
                {
                    case "list":
                        return new List<string> { "all", "running" };
                    case "modify":
                    {
                        var list = new List<string>();
                        foreach (var state in LookupEUtil.weatherStateDefinitions) list.Add(state.GetCompactName());
                        return list;
                    }
                    case "set":
                    {
                        var list = new List<string> { "none" };
                        foreach (var family in StateFamilies()) list.Add(family);
                        foreach (var state in LookupEUtil.weatherStateDefinitions) list.Add(state.GetCompactName());
                        return list;
                    }
                }
                break;
            case 2:
                if (args[0] == "modify") return new List<string> { "start", "stop", "toggle" };
                if (args[0] == "set") return new List<string> { "1", "2", "3" };
                break;
        }
        
        return null;
    }
    
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,3)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        
        WeatherDirector weatherDirector = Get<WeatherDirector>("WeatherVFX");
        if (weatherDirector == null) return SendError(Tr("cmd.weather.nodirector")); 

        if (args.Length >= 1 && args[0] == "set")
        {
            if (args.Length == 1) return SendError(Tr("cmd.weather.requiresmore", args[0]));
            return ExecuteSet(args);
        }

        switch (args.Length)
        {
            case 1:
                if (args[0] == "list")
                {
                    var stateNames = "";
                    foreach (var state in LookupEUtil.weatherStateDefinitions) stateNames += $"\n{state.GetName()}";
                    SendMessage(Tr("cmd.weather.successlist",stateNames));
                    return true;
                }

                if (args[0] == "modify") return SendError(Tr("cmd.weather.requiresmore",args[0]));
                
                return SendError(Tr("cmd.weather.notvalidargument",args[0]));
            case 2:
                if (args[0] == "list")
                {
                    if (args[1] == "running")
                    {
                        
                        var states = weatherDirector._runningStates;
                        var stateNames = "";
                        foreach (var state in states) stateNames += $"\n{state.GetName()}";
                        SendMessage(Tr("cmd.weather.successlistrunning",stateNames));
                        return true;
                    }
                    if (args[1] == "all")
                    {
                        var stateNames = "";
                        foreach (var state in LookupEUtil.weatherStateDefinitions) stateNames += $"\n{state.GetName()}";
                        SendMessage(Tr("cmd.weather.successlist",stateNames));
                        return true;
                    }
                    return SendError(Tr("cmd.weather.notvalidargument",args[1]));
                }
                if (args[0] == "modify")
                {
                    
                    WeatherStateDefinition def = LookupEUtil.GetWeatherStateDefinitionByName(args[1]);
                    if (def == null) return SendNotValidWeather(args[1]); 

                    bool isRunning = weatherDirector._runningStates.Contains(def.Cast<IWeatherState>());
                    if(isRunning) SendMessage( Tr("cmd.weather.currentlyrunning",$"\"{def.name.Replace(" ", "")}\""));
                    else SendMessage( Tr("cmd.weather.currentlynotrunning",$"\"{def.name.Replace(" ", "")}\""));
                    
                    return true;
                }
                return SendError(Tr("cmd.weather.notvalidargument",args[0]));
            case 3:
                if (args[0] == "list") return SendError(Tr("cmd.weather.requiresless",args[0]));
                if (args[0] == "modify")
                {
                    
                    WeatherStateDefinition def = LookupEUtil.GetWeatherStateDefinitionByName(args[1]);
                    if (def == null) return SendNotValidWeather(args[1]); 

                    bool isRunning = weatherDirector._runningStates.Contains(def.Cast<IWeatherState>());
                    
                    if (args[2] == "start")
                    {
                        if (isRunning)
                        {
                            SendMessage( Tr("cmd.weather.alreadyrunning",$"\"{def.name.Replace(" ", "")}\""));
                            return true;
                        }
                        
                        var param = new WeatherModel.ZoneWeatherParameters() { WindDirection = new Vector3(45f, 0, 30f) };
                        weatherDirector.RunState(def.Cast<IWeatherState>(), param);
                        SendMessage( Tr("cmd.weather.successstart",$"\"{def.name.Replace(" ", "")}\""));
                        return true;
                    }
                    if (args[2] == "stop")
                    {
                        if (!isRunning)
                        {
                            SendMessage( Tr("cmd.weather.alreadynotrunning",$"\"{def.name.Replace(" ", "")}\""));
                            return true;
                        }
                        var param = new WeatherModel.ZoneWeatherParameters() { WindDirection = new Vector3(45f, 0, 30f) };
                        weatherDirector.StopState(def.Cast<IWeatherState>(), param);
                        SendMessage( Tr("cmd.weather.successstop",$"\"{def.name.Replace(" ", "")}\""));
                        return true;
                    }
                    if (args[2] == "toggle")
                    {
                        var param = new WeatherModel.ZoneWeatherParameters() { WindDirection = new Vector3(45f, 0, 30f) };
                        if (isRunning)
                        {
                            weatherDirector.StopState(def.Cast<IWeatherState>(), param);
                            SendMessage( Tr("cmd.weather.successstop",$"\"{def.name.Replace(" ", "")}\""));
                        }
                        else
                        {
                            weatherDirector.RunState(def.Cast<IWeatherState>(), param);
                            SendMessage( Tr("cmd.weather.successstart",$"\"{def.name.Replace(" ", "")}\""));
                        }
                        return true;
                    }
                    
                    return SendError(Tr("cmd.weather.notvalidargument",args[2]));
                }
                return SendError(Tr("cmd.weather.notvalidargument",args[0]));
            default:
                return SendUsage();
        }
    }


    private static readonly Dictionary<int, string[]> Vocabulary = new()
    {
        { 1, new[] { "LIGHT" } },
        { 2, new[] { "MEDIUM", "MED" } },
        { 3, new[] { "HEAVY", "EXTREME" } }
    };

    private bool ExecuteSet(string[] args)
    {
        var directors = GetAllInScene<WeatherDirector>();
        if (directors.Count == 0)
        {
            var fallback = Get<WeatherDirector>("WeatherVFX");
            if (fallback == null) return SendError(Tr("cmd.weather.nodirector"));
            directors.Add(fallback);
        }

        var param = new WeatherModel.ZoneWeatherParameters { WindDirection = new Vector3(45f, 0, 30f) };

        var target = args[1].ToLower();
        if (target == "none" || target == "clear" || target == "off")
        {
            if (args.Length == 3) return SendErrorToManyArgs(args[0]);
            StopEveryState(directors, param);
            SendMessage(Tr("cmd.weather.successsetnone"));
            return true;
        }

        var intensity = 0;
        if (args.Length == 3)
            if (!TryParseInt(args[2], out intensity, 1, true, 3, true)) return false;

        var def = ResolveState(args[1], intensity);
        if (def == null) return SendNotValidWeather(intensity == 0 ? args[1] : $"{args[1]} {intensity}");

        StopEveryState(directors, param);
        foreach (var director in directors) director.RunState(def.Cast<IWeatherState>(), param);
        SendMessage(Tr("cmd.weather.successset", $"\"{def.GetCompactName()}\""));
        return true;
    }

    private static void StopEveryState(List<WeatherDirector> directors, WeatherModel.ZoneWeatherParameters param)
    {
        foreach (var director in directors)
        {
            var running = new List<IWeatherState>();
            foreach (var state in director._runningStates) running.Add(state);
            foreach (var state in running) director.StopState(state, param);
        }
    }

    private static WeatherStateDefinition ResolveState(string name, int intensity)
    {
        var key = name.ToUpper().Replace(" ", "").Replace("_", "");
        if (intensity == 0)
        {
            var exact = LookupEUtil.GetWeatherStateDefinitionByName(name);
            if (exact != null) return exact;

            WeatherStateDefinition onlyMatch = null;
            foreach (var state in LookupEUtil.weatherStateDefinitions)
            {
                if (!state.GetCompactUpperName().StartsWith(key)) continue;
                if (onlyMatch != null) return null;
                onlyMatch = state;
            }
            return onlyMatch;
        }

        foreach (var word in Vocabulary[intensity])
            foreach (var state in LookupEUtil.weatherStateDefinitions)
            {
                var compact = state.GetCompactUpperName();
                if (compact.StartsWith(key) && compact.Contains(word)) return state;
            }
        return null;
    }

    private static List<string> StateFamilies()
    {
        var families = new List<string>();
        foreach (var state in LookupEUtil.weatherStateDefinitions)
        {
            var compact = state.GetCompactName();
            if (string.IsNullOrEmpty(compact)) continue;
            foreach (var words in Vocabulary.Values)
                foreach (var word in words)
                {
                    var index = compact.ToUpper().IndexOf(word, StringComparison.Ordinal);
                    if (index <= 0) continue;
                    var family = compact.Substring(0, index);
                    if (!families.Contains(family)) families.Add(family);
                }
        }
        return families;
    }
}