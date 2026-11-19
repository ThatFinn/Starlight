using System;
using Starlight.Enums;
using Starlight.Managers;
using UnityEngine.InputSystem;

namespace Starlight;

/// <summary>
/// Abstract console command class
/// </summary>
public abstract class StarlightCommand
{
    /// <summary>
    /// If the command should not print any messages to the console
    /// </summary>
    public bool silent = false;
    /// <summary>
    /// The ID of this command (Always lowercase)
    /// </summary>
    public abstract string ID { get; }

    /// <summary>
    /// The usage info of this command
    /// </summary>
    public abstract string Usage { get; }

    /// <summary>
    /// The description of this command
    /// </summary>
    public virtual string Description => Tr($"cmd.{ID.ToLower()}.description");

    /// <summary>
    /// The full description of this command
    /// </summary>
    public virtual string ExtendedDescription
    {
        get
        {
            string key = $"cmd.{ID.ToLower()}.extendeddescription";
            string translation = Tr(key);
            return key == translation ? Description : translation;
        }
    }

    /// <summary>
    /// The type of this command
    /// </summary>
    public virtual CommandType type { get; } = CommandType.None;

    /// <summary>
    /// If the command should be hidden from the help command and autocomplete
    /// </summary>
    public virtual bool Hidden { get; }

    /// <summary>
    /// Executes the command
    /// </summary>
    /// <param name="args">The arguments passed in the console (null if no arguments are provided)</param>
    /// <returns>True if it executed, false otherwise</returns>
    public abstract bool Execute(string[] args);

    /// <summary>
    /// Gets the auto complete list (word filter is done by the system)
    /// </summary>
    /// <param name="argIndex">The index of the argument in the command string</param>
    /// <param name="args">A list of inputted arguments</param>
    /// <returns>The list of auto complete options</returns>
    public virtual List<string> GetAutoComplete(int argIndex, string[] args)
    {
        return null;
    }

    /// <summary>
    /// Allows the execution of the Command when the respective menus are open
    /// use typeof(Menu) to add
    /// </summary>
    public virtual Type[] execWhileMenuOpen { get; } = Array.Empty<Type>();

    /// <summary>
    /// Gets called every frame
    /// </summary>
    public virtual void Update()
    { }
    
    /// <summary>
    /// Gets executed every time the scene "StandaloneEngagementPrompt" gets loaded.
    /// </summary>
    public virtual void OnStandaloneEngagementPromptLoad() { }
    /// <summary>
    /// Gets executed every time the scene "StandaloneEngagementPrompt" gets unloaded.
    /// </summary>
    public virtual void OnStandaloneEngagementPromptUnload() { }
    /// <summary>
    /// Gets executed every time the scene "StandaloneEngagementPrompt" gets initialized.
    /// </summary>
    public virtual void OnStandaloneEngagementPromptInitialize() { }
    
    /// <summary>
    /// Gets executed once GameContext loads. In Postfix of the Start method
    /// </summary>
    public virtual void AfterGameContext(GameContext gameContext) { }
    
    /// <summary>
    /// Gets executed every time the scene "PlayerCore" gets loaded.
    /// </summary>
    public virtual void OnPlayerCoreLoad() { }
    /// <summary>
    /// Gets executed every time the scene "PlayerCore" gets unloaded.
    /// </summary>
    public virtual void OnPlayerCoreUnload() { }
    /// <summary>
    /// Gets executed every time the scene "PlayerCore" gets initialized.
    /// </summary>
    public virtual void OnPlayerCoreInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "UICore" gets loaded.
    /// </summary>
    public virtual void OnUICoreLoad() { }
    /// <summary>
    /// Gets executed every time the scene "UICore" gets unloaded.
    /// </summary>
    public virtual void OnUICoreUnload() { }
    /// <summary>
    /// Gets executed every time the scene "UICore" gets initialized.
    /// </summary>
    public virtual void OnUICoreInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "MainMenuUI" gets initialized.
    /// </summary>
    public virtual void OnMainMenuUILoad() { }
    /// <summary>
    /// Gets executed every time the scene "MainMenuUI" gets loaded.
    /// </summary>
    public virtual void OnMainMenuUIUnload() { }
    /// <summary>
    /// Gets executed every time the scene "MainMenuUI" gets unloaded.
    /// </summary>
    public virtual void OnMainMenuUIInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "LoadScene" gets loaded.
    /// </summary>
    public virtual void OnLoadSceneLoad() { }
    /// <summary>
    /// Gets executed every time the scene "LoadScene" gets unloaded .
    /// </summary>
    public virtual void OnLoadSceneUnload() { }
    /// <summary>
    /// Gets executed every time the scene "LoadScene" gets initialized.
    /// </summary>
    public virtual void OnLoadSceneInitialize() { }

    /// <summary>
    /// Sends the usage of the command to the in game console
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns
    public bool SendUsage()
    {
        SendMessageTr("cmd.usage", Usage);
        return false;
    }
    /// <summary>
    /// Sends the no arguments message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns
    public bool SendNoArguments() => SendErrorTr("cmd.noarguments");
    
    /// <summary>
    /// Sends the load a save first message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns
    public bool SendLoadASaveFirst() => SendErrorTr("cmd.loadasavefirst");
    /// <summary>
    /// Sends the cheats disabled message
    /// </summary>
    public bool SendCheatsDisabled() => SendErrorTr("cmd.cheatsdisabled");

    /// <summary>
    /// Display a message in the console
    /// </summary>

    public void SendMessage(string message)
    {
        if (!silent) StarlightLogManager.SendMessage(message, StarlightEntryPoint.starlightLogToMlLog);
    }
    /// <summary>
    /// Display a translated message in the console
    /// </summary>

    public void SendMessageTr(string message)
    {
        if (!silent) StarlightLogManager.SendMessage(Tr(message), StarlightEntryPoint.starlightLogToMlLog);
    }
    /// <summary>
    /// Display a translated message in the console
    /// </summary>

    public void SendMessageTr(string message, params object[] trArgs)
    {
        if (!silent) StarlightLogManager.SendMessage(Tr(message,trArgs), StarlightEntryPoint.starlightLogToMlLog);
    }
    /// <summary>
    /// Display an error in the console
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns
    public bool SendError(string message)
    {
        if (!silent) StarlightLogManager.SendError(message, StarlightEntryPoint.starlightLogToMlLog);
        return false;
    }
    /// <summary>
    /// Display a translated error in the console
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns
    public bool SendErrorTr(string message, params object[] trArgs)
    {
        if (!silent) StarlightLogManager.SendError(Tr(message,trArgs), StarlightEntryPoint.starlightLogToMlLog);
        return false;
    }
    /// <summary>
    /// Display a translated error in the console
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns
    public bool SendErrorTr(string message)
    {
        if (!silent) StarlightLogManager.SendError(Tr(message), StarlightEntryPoint.starlightLogToMlLog);
        return false;
    }
    /// <summary>
    /// Display a warning in the console
    /// </summary>
    public void SendWarning(string message)
    {
        if (!silent) StarlightLogManager.SendWarning(message, StarlightEntryPoint.starlightLogToMlLog);
    }
    /// <summary>
    /// Display a translated warning in the console
    /// </summary>
    public void SendWarningTr(string message, params object[] trArgs)
    {
        if (!silent) StarlightLogManager.SendWarning(Tr(message,trArgs), StarlightEntryPoint.starlightLogToMlLog);
    }
    /// <summary>
    /// Display a translated warning in the console
    /// </summary>
    public void SendWarningTr(string message)
    {
        if (!silent) StarlightLogManager.SendWarning(Tr(message), StarlightEntryPoint.starlightLogToMlLog);
    }

    /// <summary>
    /// Sends a "command is in maintenance" error message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendCommandMaintenance() => SendErrorTr("cmd.error.maintenance");
    /// <summary>
    /// Sends a "not a valid pedia entry" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidPedia(object obj) => SendErrorTr("cmd.error.notvalidpedia",obj);
    /// <summary>
    /// Sends a "not a valid vac mode" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidVacMode(object obj) => SendErrorTr("cmd.error.notvalidvacmode",obj);
    /// <summary>
    /// Sends a "not a valid weather" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidWeather(object obj) => SendErrorTr("cmd.error.notvalidweather",obj);
    /// <summary>
    /// Sends a "not a valid key code" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidKeyCode(object obj) => SendErrorTr("cmd.error.notvalidkeycode",obj);
    /// <summary>
    /// Sends a "not a valid integer" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidInt(object obj) => SendErrorTr("cmd.error.notvalidint",obj);
    /// <summary>
    /// Sends a "not a valid float" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidFloat(object obj) => SendErrorTr("cmd.error.notvalidfloat",obj);
    /// <summary>
    /// Sends a "not a valid double" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidDouble(object obj) => SendErrorTr("cmd.error.notvaliddouble",obj);
    /// <summary>
    /// Sends a "not a valid boolean" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidBool(object obj) => SendErrorTr("cmd.error.notvalidbool",obj);
    /// <summary>
    /// Sends a "not a valid trool" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidTrool(object obj) => SendErrorTr("cmd.error.notvalidtrool",obj);
    /// <summary>
    /// Sends a "not a valid Vector3" error message
    /// </summary>
    /// <param name="objX">The invalid X value</param>
    /// <param name="objY">The invalid Y value</param>
    /// <param name="objZ">The invalid Z value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidVector3(object objX,object objY,object objZ) => SendErrorTr("cmd.error.notvalidvector3",objX,objY,objZ);
    /// <summary>
    /// Sends a "not a valid identifiable type" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidIdentType(object obj) => SendErrorTr("cmd.error.notvalididenttype",obj);
    /// <summary>
    /// Sends a "not a valid gadget" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidGadget(object obj) => SendErrorTr("cmd.error.notvalidgadget",obj);
    /// <summary>
    /// Sends a "not a valid upgrade" error message
    /// </summary>
    /// <param name="obj">The invalid object name</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidUpgrade(object obj) => SendErrorTr("cmd.error.notvalidupgrade",obj);
    /// <summary>
    /// Sends an "integer is not at least" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The minimum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotIntAtLeast(object currValue, object obj) => SendErrorTr("cmd.error.notintatleast",currValue,obj);
    /// <summary>
    /// Sends an "integer is not above" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The minimum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotIntAbove(object currValue, object obj) => SendErrorTr("cmd.error.notintabove",currValue,obj);
    /// <summary>
    /// Sends an "integer is not under" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The maximum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotIntUnder(object currValue, object obj) => SendErrorTr("cmd.error.notintbelow",currValue,obj);
    /// <summary>
    /// Sends a "float is not at least" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The minimum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotFloatAtLeast(object currValue, object obj) => SendErrorTr("cmd.error.notfloatatleast",currValue,obj);
    /// <summary>
    /// Sends a "float is not above" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The minimum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotFloatAbove(object currValue, object obj) => SendErrorTr("cmd.error.notfloatabove",currValue,obj);
    /// <summary>
    /// Sends a "float is not under" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The maximum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotFloatUnder(object currValue, object obj) => SendErrorTr("cmd.error.notfloatbelow",currValue,obj);
    /// <summary>
    /// Sends a "double is not at least" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The minimum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotDoubleAtLeast(object currValue, object obj) => SendErrorTr("cmd.error.notdoubleatleast",currValue,obj);

    /// <summary>
    /// Sends a "double is not above" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The minimum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotDoubleAbove(object currValue, object obj) => SendErrorTr("cmd.error.notdoubleabove", currValue, obj);
    /// <summary>
    /// Sends a "double is not under" error message
    /// </summary>
    /// <param name="currValue">The current value</param>
    /// <param name="obj">The maximum value</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotDoubleUnder(object currValue, object obj) => SendErrorTr("cmd.error.notdoublebelow",currValue,obj);
    /// <summary>
    /// Sends a "SRCharacterController is null" error message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNullSRCharacterController() => SendErrorTr("cmd.error.srccnull");
    /// <summary>
    /// Sends a "TeleportablePlayer is null" error message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNullTeleportablePlayer() => SendErrorTr("cmd.error.teleportableplayernull");
    /// <summary>
    /// Sends a "KinematicCharacterMotor is null" error message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNullKinematicCharacterMotor() => SendErrorTr("cmd.error.kinematiccharactermotornull");
    /// <summary>
    /// Sends a "scene group not supported" error message
    /// </summary>
    /// <param name="obj">The unsupported scene group</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendUnsupportedSceneGroup(object obj) => SendErrorTr("cmd.error.scenegroupnotsupported",obj);
    /// <summary>
    /// Sends a "no camera found" error message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNoCamera() => SendErrorTr("cmd.error.nocamera");
    /// <summary>
    /// Sends a "not looking at a valid object" error message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotLookingAtValidObject() => SendErrorTr("cmd.error.notlookingatvalidobject");
    /// <summary>
    /// Sends a "not looking at anything" error message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotLookingAtAnything() => SendErrorTr("cmd.error.notlookingatanything");
    /// <summary>
    /// Sends an "unknown error" message
    /// </summary>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendUnknown() => SendErrorTr("cmd.error.unknown");

    /// <summary>
    /// Sends an "is a gadget, not an item" error message
    /// </summary>
    /// <param name="obj">The object that is a gadget</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendIsGadgetNotItem(object obj) => SendErrorTr("cmd.error.isgadgetnotitem", obj);
    /// <summary>
    /// Sends a "too many arguments" error message
    /// </summary>
    /// <param name="obj">The maximum number of arguments</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendErrorToManyArgs(object obj) => SendErrorTr("cmd.error.errortoomanyargs",obj);
    /// <summary>
    /// Sends a "not a valid option" error message
    /// </summary>
    /// <param name="obj">The invalid option</param>
    /// <returns>False, for easy chaining in Execute methods</returns>
    public bool SendNotValidOption(object obj) => SendErrorTr("cmd.error.notvalidoption",obj);

    /// <summary>
    /// Tries to parse a Vector3 from three string inputs
    /// </summary>
    /// <param name="inputX">The X component</param>
    /// <param name="inputY">The Y component</param>
    /// <param name="inputZ">The Z component</param>
    /// <param name="value">The output Vector3</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseVector3(string inputX, string inputY, string inputZ, out Vector3 value)
    {
        value = Vector3.zero;
        try { value = new Vector3(float.Parse(inputX),float.Parse(inputY),float.Parse(inputZ)); }
        catch { return SendNotValidVector3(inputX,inputY,inputZ); }
        return true;
    }
    /// <summary>
    /// Tries to parse a float from a string input with bounds
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output float</param>
    /// <param name="min">The minimum value</param>
    /// <param name="inclusive">If the minimum is inclusive</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseFloat(string input, out float value, float min, bool inclusive, float max)
    {
        value = 0;
        try { value = float.Parse(input); }
        catch { return SendNotValidFloat(input); }
        if (inclusive)
        {
            if (value < min) return SendNotFloatAtLeast(input, min);
        }
        else if (value <= min) return SendNotFloatAbove(input,min);
        if (value >= max) return SendNotFloatUnder(input, max);
        return true;
    }
    /// <summary>
    /// Tries to parse a float from a string input with a minimum bound
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output float</param>
    /// <param name="min">The minimum value</param>
    /// <param name="inclusive">If the minimum is inclusive</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseFloat(string input, out float value, float min, bool inclusive)
    {
        value = 0;
        try { value = float.Parse(input); }
        catch { return SendNotValidFloat(input); }
        if (inclusive)
        {
            if (value < min) return SendNotFloatAtLeast(input, min);
        }
        else if (value <= min) return SendNotFloatAbove(input,min);
        return true;
    }
    /// <summary>
    /// Tries to parse a float from a string input with a maximum bound
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output float</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseFloat(string input, out float value, float max)
    {
        value = 0;
        try { value = float.Parse(input); }
        catch { return SendNotValidFloat(input); }
        if (value >= max) return SendNotFloatUnder(input, max);
        return true;
    }
    /// <summary>
    /// Tries to parse a float from a string input
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output float</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseFloat(string input, out float value)
    {
        value = 0;
        try { value = float.Parse(input); }
        catch { return SendNotValidFloat(input); }
        return true;
    }
    /// <summary>
    /// Tries to parse an integer from a string input with bounds
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output integer</param>
    /// <param name="min">The minimum value</param>
    /// <param name="inclusiveMin">If the minimum is inclusive</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseInt(string input, out int value, int min, bool inclusiveMin, int max)
    {
        value = 0;
        try { value = int.Parse(input); }
        catch { return SendNotValidInt(input); }
        if (inclusiveMin)
        {
            if (value < min) return SendNotIntAtLeast(input, min);
        }
        else if (value <= min) return SendNotIntAbove(input,min);
        if (value >= max) return SendNotIntUnder(input, max);
        return true;
    }
    /// <summary>
    /// Tries to parse an integer from a string input with bounds
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output integer</param>
    /// <param name="min">The minimum value</param>
    /// <param name="inclusiveMin">If the minimum is inclusive</param>
    /// <param name="max">The maximum value</param>
    /// <param name="inclusiveMax">If the maximum is inclusive</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseInt(string input, out int value, int min, bool inclusiveMin, int max, bool inclusiveMax)
    {
        value = 0;
        try { value = int.Parse(input); }
        catch { return SendNotValidInt(input); }
        if (inclusiveMin)
        {
            if (value < min) return SendNotIntAtLeast(input, min);
        }
        else if (value <= min) return SendNotIntAbove(input,min);
        if (inclusiveMax)
        {
            if(value > max) return SendNotIntUnder(input, max);
        }
        else if(value >= max) return SendNotIntUnder(input, max);
        return true;
    }

    /// <summary>
    /// Tries to parse an integer from a string input with a minimum bound
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output integer</param>
    /// <param name="min">The minimum value</param>
    /// <param name="inclusive">If the minimum is inclusive</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseInt(string input, out int value, int min, bool inclusive)
    {
        value = 0;
        try { value = int.Parse(input); }
        catch { return SendNotValidInt(input); }
        if (inclusive)
        {
            if (value < min) return SendNotIntAtLeast(input, min);
        }
        else if (value <= min) return SendNotIntAbove(input,min);
        return true;
    }
    /// <summary>
    /// Tries to parse an integer from a string input with a maximum bound
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output integer</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseInt(string input, out int value, int max)
    {
        value = 0;
        try { value = int.Parse(input); }
        catch { return SendNotValidInt(input); }
        if (value >= max) return SendNotIntUnder(input, max);
        return true;
    }
    /// <summary>
    /// Tries to parse an integer from a string input
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output integer</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseInt(string input, out int value)
    {
        value = 0;
        try { value = int.Parse(input); }
        catch { return SendNotValidInt(input); }
        return true;
    }
    /// <summary>
    /// Tries to parse a boolean from a string input
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output boolean</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseBool(string input, out bool value)
    {
        value = false;
        if (input.ToLower() != "true" && input.ToLower() != "false") return SendNotValidBool(input);
        if (input.ToLower() == "true") value = true;
        return true;
    }
    /// <summary>
    /// Tries to parse a Trool (true, false, toggle) from a string input
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output Trool</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseTrool(string input, out Trool value)
    {
        value = Trool.False;
        if (input.ToLower() != "true" && input.ToLower() != "false" && input.ToLower() != "toggle") return SendNotValidTrool(input);
        if (input.ToLower() == "true") value = Trool.True;
        if (input.ToLower() == "toggle") value = Trool.Toggle;
        return true;
    }
    /// <summary>
    /// Tries to parse a KeyCode from a string input
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output KeyCode</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseKeyCode( string input, out KeyCode value)
    {
        string keyToParse = input;
        if (input.ToCharArray().Length == 1)
            if (int.TryParse(input, out int digit))
                keyToParse = "Alpha" + digit;
        KeyCode key;
        if (KeyCode.TryParse(keyToParse, true, out key)) { value = key; return true; }
        value = KeyCode.None;
        return SendNotValidKeyCode(input);
    }
    /// <summary>
    /// Tries to parse a Key from a string input
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output Key</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseKey( string input, out Key value)
    {
        string keyToParse = input;
        if (input.ToCharArray().Length == 1)
            if (int.TryParse(input, out int digit))
                keyToParse = "Digit" + digit;
        Key key;
        if (Key.TryParse(keyToParse, true, out key)) { value = key; return true; }
        value = Key.None;
        return SendNotValidKeyCode(input);
    }
    /// <summary>
    /// Tries to parse an LKey from a string input
    /// </summary>
    /// <param name="input">The string to parse</param>
    /// <param name="value">The output LKey</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TryParseLKey( string input, out LKey value)
    {
        string keyToParse = input;
        if (input.ToCharArray().Length == 1)
            if (int.TryParse(input, out int digit))
                keyToParse = "Alpha" + digit;
        LKey key;
        if (LKey.TryParse(keyToParse, true, out key)) { value = key; return true; }
        value = LKey.None;
        return SendNotValidKeyCode(input);
    }
}