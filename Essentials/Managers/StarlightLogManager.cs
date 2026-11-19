using System;
using System.Runtime.InteropServices;
using MelonLoader;

namespace Starlight.Managers;

public static class StarlightLogManager
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    
    /// <summary>
    /// An event which gets executed everytime a message is sent
    /// </summary>
    public static event Action<string> OnSendMessage;
    /// <summary>
    /// An event which gets executed everytime a warning is sent
    /// </summary>
    public static event Action<string> OnSendWarning;
    /// <summary>
    /// An event which gets executed everytime an error is sent
    /// </summary>
    public static event Action<string> OnSendError;
    static MelonLogger.Instance mlog;
    const string logName = "Starlight-Console";
    internal static void Start()
    {
        mlog = new MelonLogger.Instance(logName);
        MelonLogger.MsgDrawingCallbackHandler += (c1, c2, s1, s2) => { if (StarlightEntryPoint.mLLogToStarlightLog) SendMessage($"[{s1}]: {s2}", false); };
        MelonLogger.ErrorCallbackHandler += (s, s1) => { if (StarlightEntryPoint.mLLogToStarlightLog) SendError($"[{s}]: {s1}", false); };
        MelonLogger.WarningCallbackHandler += (s, s1) => { if (StarlightEntryPoint.mLLogToStarlightLog) SendWarning($"[{s}]: {s}", false); };
    }
    
    /// <summary>
    /// Display a message in the console
    /// </summary>
    public static void SendMessage(string message) => SendMessage(message, StarlightEntryPoint.starlightLogToMlLog);
    /// <summary>
    /// Display a message in the log
    /// </summary>
    public static void SendMessage(string message, bool doMLLog, bool internal_logMLForSingleLine = true)
    {
        if (HideMessage(message)) return;
        if (doMLLog&&(message.Contains("\n") || internal_logMLForSingleLine)) mlog.Msg(message);
        OnSendMessage?.Invoke(message);
    }
    
    /// <summary>
    /// Display an error in the log
    /// </summary>
    public static void SendError(string message) => SendError(message, StarlightEntryPoint.starlightLogToMlLog);
    /// <summary>
    /// Display an error in the log
    /// </summary>
    public static void SendError(string message, bool doMLLog, bool internal_logMLForSingleLine = true)
    {
        if (HideMessage(message)) return;
        if (doMLLog&&(message.Contains("\n") || internal_logMLForSingleLine)) mlog.Error(message);
        OnSendError?.Invoke(message);
    }

    /// <summary>
    /// Display an error in the log
    /// </summary>
    public static void SendWarning(string message) => SendWarning(message, StarlightEntryPoint.starlightLogToMlLog);
    /// <summary>
    /// Display an error in the log
    /// </summary>
    public static void SendWarning(string message, bool doMLLog, bool internal_logMLForSingleLine = true)
    {
        if (HideMessage(message)) return;
        if (doMLLog&&(message.Contains("\n") || internal_logMLForSingleLine)) mlog.Warning(message);
        OnSendWarning?.Invoke(message);
    }
    
    /// <summary>
    /// Displays a message box
    /// </summary>
    public static void Alert(string message)
    {
        Alert("Alert!", message,StarlightEntryPoint.starlightLogToMlLog);
    }
    /// <summary>
    /// Displays a message box
    /// </summary>
    public static void Alert(string title, string message)
    {
        Alert(title,message, StarlightEntryPoint.starlightLogToMlLog);
    }
    /// <summary>
    /// Displays a message box
    /// </summary>
    public static void Alert(string title, string message, bool doMLLog, bool internal_logMLForSingleLine = true)
    {
        if (HideMessage(message)) return;
        if (doMLLog&&(message.Contains("\n") || internal_logMLForSingleLine)) mlog.Msg(message);
        MessageBox(IntPtr.Zero, message, title, 0);
    }

    static bool HideMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return true;
        if (message.StartsWith($"[{logName}]")) return true;
        if (message.StartsWith("[UnityExplorer]")) return true;
        if (message.StartsWith("[CinematicUnityExplorer]")) return true;
        if (message.StartsWith("[Il2CppInterop]")) return true;
        if (message.StartsWith("[Il2CppICallInjector]")) return true;
        if (message.StartsWith("[]:")) return true;
        return false;
    }
}