using System.Diagnostics;
using System.IO;
using MelonLoader.Utils;
using Starlight.Storage;

namespace Starlight.Components.Debug;
[InjectIntoIL]
internal class DevConsoleFixer : MonoBehaviour
{
    private struct Log
    {
        public string message;
        public string trace;
        public LogType type;
    }

    private readonly List<Log> logs = new();
    private Vector2 scroll;
    public bool visible = false;

    void Start()
    {
        Application.add_logMessageReceived(new System.Action<string, string, LogType>(HandleLog));
        HandleLog("Hello World","No Trace :)",LogType.Error);
    }
    void HandleLog(string message, string trace, LogType type)
    {
        if (message.Equals(string.Empty)) return;
        if(trace.StartsWith("TMPro.TextMeshProUGUI.Rebuild (UnityEngine.UI.CanvasUpdate update)")) return;
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
            //case LogType.Warning:
                visible = true;
                logs.Add(new Log { message = message, trace = trace, type = type });
                break;
        }
    }
    int expandedIndex = -1;

    void OnGUI()
    {
        if (!visible) return;

        var marginW = Screen.width * 0.05f;
        var marginH = Screen.height * 0.05f;

        var boxWidth = Screen.width * 0.45f;
        var boxMaxHeight = Screen.height * 0.5f;

        var boxX = marginW;
        var boxBottom = Screen.height - marginH;

        float contentHeight = Mathf.Min(boxMaxHeight, 60 + logs.Count * 24 + (expandedIndex >= 0 ? 80 : 0));

        float boxY = boxBottom - contentHeight;

        var boxRect = new Rect(boxX, boxY, boxWidth, contentHeight);
        GUI.Box(boxRect, "");

        GUI.Label(
            new Rect(boxX + 10, boxY + 6, boxWidth - 20, 20),
            "Development Console"
        );

        float btnX = boxX + boxWidth + 10;
        float btnY = boxY + 8;
        float btnW = 100;
        float btnH = 22;

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Open ML Log"))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = Path.Combine(MelonEnvironment.MelonLoaderDirectory,"Latest.log"),
                UseShellExecute = true
            });
        }

        if (GUI.Button(new Rect(btnX, btnY + 30, btnW, btnH), "Clear"))
        {
            logs.Clear();
            expandedIndex = -1;
            visible = false;
        }

        if (GUI.Button(new Rect(btnX, btnY + 60, btnW, btnH), "Close"))
        {
            visible = false;
        }

        if (Event.current.type == EventType.ScrollWheel)
        {
            scroll.y += Event.current.delta.y * 20f;
            scroll.y = Mathf.Max(0, scroll.y);
            Event.current.Use();
        }

        float clipTop = boxY + 30;
        float clipBottom = boxY + contentHeight - 6;

        float y = clipTop - scroll.y;

        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];

            float entryHeight = 22;
            float expandedHeight = (expandedIndex == i) ? 80 : 0;
            float totalHeight = entryHeight + expandedHeight;

            if (y + totalHeight >= clipTop && y <= clipBottom)
            {
                GUI.contentColor = log.type switch
                {
                    LogType.Error or LogType.Exception => Color.red,
                    LogType.Warning => Color.yellow,
                    _ => Color.white
                };

                var logRect = new Rect(boxX + 6, y, boxWidth - 12, entryHeight);

                if (GUI.Button(logRect, log.message, GUI.skin.label))
                {
                    expandedIndex = expandedIndex == i ? -1 : i;
                }

                if (expandedIndex == i)
                {
                    GUI.contentColor = Color.gray;
                    GUI.Label(
                        new Rect(boxX + 12, y + entryHeight, boxWidth - 24, expandedHeight),
                        log.trace
                    );
                }
            }

            y += totalHeight + 2;
        }

        GUI.contentColor = Color.white;
    }
}