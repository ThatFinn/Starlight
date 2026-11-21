using System;
using System.Linq;
using Starlight.Commands;
using Starlight.Enums;

namespace Starlight.Managers;

public static class StarlightCommandManager
{
    internal static Dictionary<string, StarlightCommand> Commands = new(StringComparer.OrdinalIgnoreCase);
    internal static void Start()
    {
        SetupCommands();
    }
    internal static void Update()
    {
        foreach (KeyValuePair<string, StarlightCommand> pair in Commands)
            pair.Value.Update();
        try
        {
            foreach (KeyValuePair<LKey,string> keyValuePair in StarlightSaveManager.data.keyBinds)
                if (keyValuePair.Key.OnKeyDown())
                    if (StarlightWarpManager.WarpTo == null)
                        StarlightCommandManager.ExecuteByString(keyValuePair.Value, true);
        }
        catch (Exception e) {LogError(e);}
    }
    
    /// <summary>
    /// Bind a command to a key
    /// </summary>
    /// <param name="key">The key that should be bound</param>
    /// <param name="command">The command that should be executed</param>
    public static void BindKey(LKey key, string command)
    {
        if (StarlightSaveManager.data.keyBinds.ContainsKey(key)) StarlightSaveManager.data.keyBinds[key] += ";" + command;
        else StarlightSaveManager.data.keyBinds.Add(key, command);
        StarlightSaveManager.Save();
    }
    /// <summary>
    /// Unbinds every command currently bound to a key
    /// </summary>
    /// <param name="key">The key which should be unbound</param>
    public static void UnbindKey(LKey key)
    {
        if (StarlightSaveManager.data.keyBinds.ContainsKey(key)) StarlightSaveManager.data.keyBinds.Remove(key);
        StarlightSaveManager.Save();
    }
    /// <summary>
    /// Get every command separated by a semicolon which is bound to a key
    /// </summary>
    /// <param name="key">The key to be checked</param>
    /// <returns>The command</returns>
    public static string GetBind(LKey key)
    {
        if (StarlightSaveManager.data.keyBinds.ContainsKey(key)) return StarlightSaveManager.data.keyBinds[key]; return null;
    }

    /// <summary>
    /// Returns true if a key has a command bound to it
    /// </summary>
    /// <param name="key">The key to be checked</param>
    /// <returns>bool</returns>
    public static bool IsKeyBoundToCommand(LKey key) => StarlightSaveManager.data.keyBinds.ContainsKey(key);

    private static bool _executedStartup;
    internal static void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "StandaloneEngagementPrompt": foreach (var pair in Commands) try { pair.Value.OnStandaloneEngagementPromptLoad(); } catch (Exception e) { LogError(e); } break;
            case "PlayerCore": foreach (var pair in Commands) try { pair.Value.OnPlayerCoreLoad(); } catch (Exception e) { LogError(e); } break;
            case "UICore":
                foreach (var pair in Commands) try { pair.Value.OnUICoreLoad(); } catch (Exception e) { LogError(e); }
                if (!string.IsNullOrEmpty(StarlightEntryPoint.onSaveLoadCommand))
                    ExecuteByString(StarlightEntryPoint.onSaveLoadCommand);
                break;
            case "MainMenuUI":
                foreach (var pair in Commands) try { pair.Value.OnMainMenuUILoad(); } catch (Exception e) { LogError(e); }
                if (!string.IsNullOrEmpty(StarlightEntryPoint.onMainMenuLoadCommand))
                    ExecuteByString(StarlightEntryPoint.onMainMenuLoadCommand);
                if (!_executedStartup&&!string.IsNullOrEmpty(StarlightEntryPoint.onStartupCommand))
                {
                    _executedStartup = true;
                    ExecuteByString(StarlightEntryPoint.onStartupCommand);
                }
                break;
            case "LoadScene": foreach (var pair in Commands) try { pair.Value.OnLoadSceneLoad(); } catch (Exception e) { LogError(e); } break;
        }
    }
    internal static void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "StandaloneEngagementPrompt": foreach (var pair in Commands) try { pair.Value.OnStandaloneEngagementPromptUnload(); } catch (Exception e) { LogError(e); } break;
            case "PlayerCore": foreach (var pair in Commands) try { pair.Value.OnPlayerCoreUnload(); } catch (Exception e) { LogError(e); } break;
            case "UICore": foreach (var pair in Commands) try { pair.Value.OnUICoreUnload(); } catch (Exception e) { LogError(e); } break;
            case "MainMenuUI": foreach (var pair in Commands) try { pair.Value.OnMainMenuUIUnload(); } catch (Exception e) { LogError(e); } break;
            case "LoadScene": foreach (var pair in Commands) try { pair.Value.OnLoadSceneUnload(); } catch (Exception e) { LogError(e); } break;
        }
    }
    internal static void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "StandaloneEngagementPrompt": foreach (var pair in Commands) try { pair.Value.OnStandaloneEngagementPromptInitialize(); } catch (Exception e) { LogError(e); } break;
            case "PlayerCore": foreach (var pair in Commands) try { pair.Value.OnPlayerCoreInitialize(); } catch (Exception e) { LogError(e); } break;
            case "UICore": foreach (var pair in Commands) try { pair.Value.OnUICoreInitialize(); } catch (Exception e) { LogError(e); } break;
            case "MainMenuUI": foreach (var pair in Commands) try { pair.Value.OnMainMenuUIInitialize(); } catch (Exception e) { LogError(e); } break;
            case "LoadScene": foreach (var pair in Commands) try { pair.Value.OnLoadSceneInitialize(); } catch (Exception e) { LogError(e); } break;
        }
    }
    static void SetupCommands()
    {
        foreach (var assembly in StarlightPackageManager.GetAllPackageAssemblies())
        {
            var exporters = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(StarlightCommand)) && !t.IsAbstract);
            foreach (Type type in exporters)
                try
                {
                    if(type == typeof(MenuVisibilityCommands.OpenCommand)) continue;
                    if(type == typeof(MenuVisibilityCommands.CloseCommand)) continue;
                    if(type == typeof(MenuVisibilityCommands.ToggleCommand)) continue;
                    StarlightCommand sr2Command = (StarlightCommand)Activator.CreateInstance(type);
                    if((enabledCommands & sr2Command.type) == sr2Command.type)
                    {
                        if (sr2Command is InfiniteHealthCommand && !EnableInfHealth.HasFlag()) continue;
                        if (sr2Command is InfiniteEnergyCommand && !EnableInfEnergy.HasFlag()) continue;
                        if (sr2Command.type.HasFlag(CommandType.DontLoad)) continue;
                        try { RegisterCommand(sr2Command); }
                        catch (Exception e) { LogError(e); }
                    }
                }
                catch (Exception e) { LogError(e); }
        }
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            try { expansion.OnLoadCommands(); }
            catch (Exception e) { LogError(e); }
        StarlightCallEventManager.ExecuteStandard(CallEvent.OnLoadCommands);
        
    }
    /// <summary>
    /// Registers a command to be used in the console<br />
    /// Returns true if the registration was successful<br />
    /// BE CAREFUL: Every command automatically gets registered if it doesn't have the CommandType.DontLoad type!
    /// </summary>
    /// <param name="cmd">The StarlightCommand which should be registered</param>
    /// <returns>bool</returns>
    public static bool RegisterCommand(StarlightCommand cmd)
    {
        if(cmd.ID.Contains(" "))
        {
            StarlightLogManager.SendMessage(Tr("cmd.idcontainsspace", cmd.ID.ToLowerInvariant()));
            return false;
        }
        if (Commands.ContainsKey(cmd.ID.ToLowerInvariant()))
        {
            StarlightLogManager.SendMessage(Tr("cmd.alreadyregistered", cmd.ID.ToLowerInvariant()));
            return false;
        }

        Commands.Add(cmd.ID.ToLowerInvariant(), cmd);
        List<KeyValuePair<string, StarlightCommand>> myList = Commands.ToList();

        myList.Sort(delegate(KeyValuePair<string, StarlightCommand> pair1, KeyValuePair<string, StarlightCommand> pair2)
        {
            return pair1.Key.CompareTo(pair2.Key);
        });
        Commands = myList.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        return true;
    }

    /// <summary>
    /// Registers multiple commands<br />
    /// Returns true if the registration was successful for every command<br />
    /// BE CAREFUL: Every command automatically gets registered if it doesn't have the CommandType.DontLoad type!
    /// </summary>
    /// <param name="cmds">The StarlightCommands which should be registered</param>
    /// <returns>bool</returns>
    public static bool RegisterCommands(params StarlightCommand[] cmds)
    {
        bool successful = true;
        for (int i = 0; i < cmds.Length; i++)
        {
            bool didWork = RegisterCommand(cmds[i]);
            if (!didWork)
                successful = false;
        }
        return successful;
    }
    /// <summary>
    /// Unregisters multiple commands<br />
    /// Returns true if the unregistration was successful for every command
    /// </summary>
    /// <param name="cmds">The StarlightCommands which should be unregistered</param>
    /// <returns>bool</returns>
    public static bool UnRegisterCommands(params StarlightCommand[] cmds)
    {
        bool successful = true;
        for (int i = 0; i < cmds.Length; i++)
        {
            bool didWork = UnRegisterCommand(cmds[i]);
            if (!didWork)
                successful = false;
        }
        return successful;
    }
    /// <summary>
    /// Unregisters a command to be used in the console<br />
    /// Returns true if the unregistration was successful
    /// </summary>
    /// <param name="cmd">The StarlightCommand which should be unregistered</param>
    /// <returns>bool</returns>
    public static bool UnRegisterCommand(StarlightCommand cmd)
    {
        return UnRegisterCommand(cmd.ID);
    }

    /// <summary>
    /// Unregisters a command to be used in the console<br />
    /// Returns true if the unregistration was successful
    /// </summary>
    /// <param name="cmd">The StarlightCommand's key which should be unregistered</param>
    /// <returns>bool</returns>
    public static bool UnRegisterCommand(string cmd)
    {
        if (Commands.ContainsKey(cmd.ToLowerInvariant()))
        {
            Commands.Remove(cmd.ToLowerInvariant());
            return true;
        }
        StarlightLogManager.SendMessage(Tr("cmd.notregistered", cmd.ToLowerInvariant()));
        return false;
    }

    public static readonly Dictionary<string, List<Action<string[]>>> CommandAddons = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a command addon
    /// </summary>
    /// <param name="command">The command's key</param>
    /// <param name="action">The action which should be executed</param>
    public static void RegisterCommandAddon(string command, Action<string[]> action)
    {
        if (CommandAddons.TryGetValue(command, out List<Action<string[]>> list))
            list.Add(action);
        else
            CommandAddons.Add(command, new List<Action<string[]>> { action });
    }
    
    /// <summary>
    /// Gives every argument proper case-sensitivity
    /// </summary>
    /// <param name="command"> The command whose autocomplete options are used </param>
    /// <param name="args"> The arguments, changed in place </param>
    static void MatchArgumentCase(StarlightCommand command, string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            List<string> options;
            try { options = command.GetAutoComplete(i, args.Take(i + 1).ToArray()); }
            catch { continue; }
            if (options == null || options.Contains(args[i])) continue;

            var matches = options.Where(option => string.Equals(option, args[i], StringComparison.OrdinalIgnoreCase)).Distinct().ToList();
            if (matches.Count == 1) args[i] = matches[0];
        }
    }
    
    /// <summary>
    /// Executes a string as a command
    /// </summary>
    /// <param name="input">The string which should be executed</param>
    /// <param name="silent">Whether it should write messages to the console</param>
    public static void ExecuteByString(string input, bool silent = false)
    {
        ExecuteByString(input, silent, false);
    }

    /// <summary>
    /// Execute a string as if it was a commandId with args
    /// </summary>
    /// <param name="input">The string as the console command input</param>
    /// <param name="silent">If the command is supposed to be silent</param>
    /// <param name="alwaysPlay">If the command should NOT be silent</param>
    public static void ExecuteByString(string input, bool silent, bool alwaysPlay)
    {
        string[] cmds = input.Split(';');
        foreach (string cc in cmds)
        {
            string c = cc.TrimStart(' ');
            if (!string.IsNullOrWhiteSpace(c))
            {
                bool spaces = c.Contains(" ");
                string cmd = spaces ? c.Substring(0, c.IndexOf(' ')) : c;

                if (Commands.ContainsKey(cmd))
                {
                    bool canPlay = false;
                    if (!MenuEUtil.isAnyMenuOpen)
                        if (Time.timeScale != 0)
                            canPlay = true;
                    if (!canPlay)
                    {
                        var openMenu = MenuEUtil.GetOpenMenu();
                        if (openMenu != null)
                        {
                            Type openMenuType = openMenu.GetType();
                            foreach (var type in Commands[cmd].execWhileMenuOpen)
                                if(openMenuType==type)
                                {
                                    canPlay = true;
                                    break;
                                }
                        }
                    }
                    if (!silent) canPlay = true;
                    if (alwaysPlay) canPlay = true;
                    bool successful = false;
                    if (spaces)
                    {
                        var argString = c.TrimEnd(' ') + " ";
                        List<string> split = argString.Split(' ').ToList();
                        split.RemoveAt(0);
                        split.RemoveAt(split.Count - 1);
                        if (canPlay)
                        {
                            string[] args = split.ToArray();
                            var command = Commands[cmd];
                            if (command.type.HasFlag(CommandType.Cheat) && StarlightCounterGateManager.disableCheats)
                            {
                                try { command.SendCheatsDisabled(); } catch (Exception e) { LogError($"Error in command SendCheatsDisabled!\n{e}"); }
                            }
                            else
                            {
                                command.silent = silent;
                                MatchArgumentCase(command, args);
                                try { successful = command.Execute(args); } catch (Exception e) { LogError($"Error in command execution!\n{e}"); }

                                try
                                {
                                    if (CommandAddons.TryGetValue(cmd, out List<Action<string[]>> list))
                                        foreach (var action in list)
                                            action(args);
                                }
                                catch (Exception e) { LogError($"Error in command extension execution!\n{e}"); }
                                
                                command.silent = false;
                            }
                        }
                    }
                    else if (canPlay)
                    {
                        StarlightCommand command = Commands[cmd];
                        if (command.type.HasFlag(CommandType.Cheat) && StarlightCounterGateManager.disableCheats)
                        {
                            try { command.SendCheatsDisabled(); } catch (Exception e) { LogError($"Error in command SendCheatsDisabled!\n{e}"); }
                        }
                        else
                        {
                            command.silent = silent;
                            try { successful = command.Execute([]); } catch (Exception e) { LogError($"Error in command execution!\n{e}"); }

                            try
                            {
                                if (CommandAddons.TryGetValue(cmd, out List<Action<string[]>> list))
                                    foreach (var action in list)
                                        action([]);
                            }
                            catch (Exception e) { LogError($"Error in command extension execution!\n{e}"); }
                            
                            command.silent = false;
                        }
                    }

                    if (DebugLogging.HasFlag()) Log($"Command success: {successful}");
                }
                else if (!silent)
                    if (MenuEUtil.isAnyMenuOpen)
                        StarlightLogManager.SendError(Tr("cmd.unknowncommand"));
            }
        }
    }
}