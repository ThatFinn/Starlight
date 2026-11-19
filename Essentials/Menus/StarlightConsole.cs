using System;
using System.Linq;
using System.Text.RegularExpressions;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Enums.Features;
using Starlight.Managers;
using Starlight.Storage;
using Starlight.UI;
using Starlight.UI.Blueprints;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Starlight.Menus;

public class StarlightConsole : StarlightMenu
{
    public new static MenuIdentifier GetMenuIdentifier() => new ("console",StarlightMenuFont.Default, StarlightMenuTheme.Black, "Console",true,true);
    protected override bool createCommands => true;
    protected override bool inGameOnly => false;

    internal static readonly LKey OpenKey = LKey.F11;
    internal static readonly LMultiKey OpenKey2 = new (LKey.Tab, LKey.LeftControl);
    
    private RectTransform _openMenu;
    private readonly List<string> _messageHistory = new ();
    private readonly List<Color> _messageHistoryColor = new ();
    
    private Vector2 _currentResolution;
    
    internal Transform ConsoleContent;
    private TMP_InputField _commandInput;
    private Transform _autoCompleteContent;
    private GameObject _autoCompleteScrollView;
    private int _selectedAutoComplete;
    private List<string> _commandHistory = new ();
    private int _commandHistoryIdx = -1;
    private Scrollbar _scrollbar;
    private bool _shouldResetTime = false;
    private bool _scrollCompletelyDown;
    
    public new static GameObject GetMenuRootObject()
    {
        var obj = new GameObject("StarlightConsoleMenu");
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        return obj;
    }
    protected override void OnAwake()
    {
        requiredFeatures = new List<FeatureFlag>() { EnableConsole }.ToArray();
        openActions = new List<MenuActions> { MenuActions.PauseGameFalse, MenuActions.DisableInput }.ToArray();
        closeActions = new List<MenuActions> { MenuActions.UnPauseGameFalse, MenuActions.EnableInput }.ToArray();
    }

    protected override void OnStart()
    {
        SendMessage(Tr("console.helloworld"));
        SendMessage(Tr("console.info"));
        StarlightLogManager.OnSendMessage += SendMessage;
        StarlightLogManager.OnSendWarning += SendWarning;
        StarlightLogManager.OnSendError += SendError;
        Send("Hello Console!", Color.green);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        OnThemeChange();
    }

    public override void OnThemeChange()
    {
        string currentText = _commandInput != null ? _commandInput.text : "";
        if(_openMenu)
            DestroyImmediate(_openMenu.gameObject);
        _currentResolution = new Vector2(Screen.width, Screen.height);
        _openMenu = menuBase.Render(currentTheme, currentFontTheme, transform);
        if (isOpen) transform.SetAsLastSibling();
        
        ConsoleContent = transform.GetObjectRecursively<Transform>("ConsoleMenuConsoleContentRec");
        _commandInput = transform.GetObjectRecursively<Transform>("ConsoleMenuCommandInputRec").GetChild(0).GetComponent<TMP_InputField>();
        _scrollbar = transform.GetObjectRecursively<Scrollbar>("ConsoleMenuConsoleScrollbarRec");
        _autoCompleteContent = transform.GetObjectRecursively<Transform>("ConsoleMenuAutoCompleteContentRec");
        _autoCompleteScrollView = transform.GetObjectRecursively<GameObject>("ConsoleMenuAutoCompleteScrollRectRec");

        _commandInput.onValueChanged.AddListener((Action<string>)((text) =>
        {
            if (text.Contains("\n")) _commandInput.text = text.Replace("\n", "");
            RefreshAutoComplete(text);
        }));

        if (!string.IsNullOrEmpty(currentText))
            _commandInput.text = currentText;
        else
            RefreshAutoComplete("");

        var texts = _messageHistory.ToArray();
        var colors = _messageHistoryColor.ToArray();
        int count = Math.Min(texts.Length, colors.Length);
        for (int i = 0; i < count; i++)
            Send(texts[i],colors[i]);
    }

    [HideFromIl2Cpp]
    private bool ResolutionChanged => Mathf.Abs(_currentResolution.x - Screen.width) > 0.01f || Mathf.Abs(_currentResolution.y - Screen.height) > 0.01f;
    
    private const float PanelHeight = 330f;
    private const float InputHeight = 27f;
    private const float Gap = 5f;
    
    [HideFromIl2Cpp]
    private UIBlueprint menuBase => new PanelUIBlueprintV01()
    {
        Name="Console",
        Anchors = new Vector4(0, 1, 1, 1),
        Pivot = new (0.5f, 1f),
        Size = new (0, PanelHeight),
        Color = UIColor.Primary,
        Children=[
            new InputUIBlueprintV01()
            {
                Name="ConsoleMenuCommandInputRec",
                Anchors = new Vector4(0, 0, 1, 0),
                Pivot = new (0.5f, 0f),
                Size = new (0, InputHeight),
                PlaceHolderContent = "Enter command...",
                CustomPlaceHolderColor = Color.white,
                FontSize = 15.5f,
                Margins = new Vector4(5, 0, 5, 0),
            },
            new VScrollUIBlueprintV01()
            {
                Anchors = new Vector4(0, 0, 1, 1),
                Pivot = new (0.5f, 0.5f),
                Size = new (-Gap * 2f, -(InputHeight + Gap * 2f)),
                Position = new Vector2(0, InputHeight / 2f),
                ContentName = "ConsoleMenuConsoleContentRec",
                ScrollBarVerticalName = "ConsoleMenuConsoleScrollbarRec",
            },
            new VScrollUIBlueprintV01()
            {
                Anchors = new Vector4(0, 0, 0, 0),
                Pivot = new (0f, 1f),
                Size = new (300, 200),
                Position = new Vector2(Gap, 0),
                Name = "ConsoleMenuAutoCompleteScrollRectRec",
                ContentName = "ConsoleMenuAutoCompleteContentRec",
                BackgroundColor = UIColor.Primary
            },
        ]
    };
    [HideFromIl2Cpp]
    private Button GetAutoCompletePrefab(string text)
        => new ButtonUIBlueprintV01()
        {
            Size = new Vector2(300, 22),
            ButtonColors = UIColorBlock.White,
            Color = UIColor.AutoCompleteBackground,
            Children = [
                new TextUIBlueprintV01()
                {
                    Anchors = new Vector4(0,0,1,1),
                    Size = Vector2.zero,
                    Margins = new Vector4(5,1,1,1),
                    TextContent = text,
                    DisableAutoTranslation = true,
                    FontSize = 15,
                    Alignment = TextAlignmentOptions.Left
                }
            ]
        }.Render(currentTheme,currentFontTheme,_autoCompleteContent).GetComponent<Button>();
    [HideFromIl2Cpp]
    private UIBlueprint GetMessagePrefab(string text, Color textColor)
        => new PanelUIBlueprintV01()
        {
            Size = new Vector2(100, 22),
            Color = UIColor.Transparent,
            Children = [
            new TextUIBlueprintV01()
            {
                Anchors = new Vector4(0,0,1,1),
                Size = Vector2.zero,
                Margins = new Vector4(5,1,1,1),
                TextContent = text,
                DisableAutoTranslation = true,
                CustomColor = textColor,
                FontSize = 15,
                Alignment = TextAlignmentOptions.Left
            }
            ]
        };
    
    public void Send(string message, Color color)
    {
        try
        {
            if (message.Contains("\n"))
            {
                foreach (string singularLine in message.Split('\n')) Send(singularLine,color);
                return;
            }

            if (!_openMenu) return;
            _messageHistory.Add(message);
            _messageHistoryColor.Add(color);
            GetMessagePrefab(message, color).Render(currentTheme, currentFontTheme, ConsoleContent);
            _scrollbar.value = 0f;
            _scrollCompletelyDown = true;
        } catch { }
    }

    private void SendMessage(string message) => Send(message, Color.white);
    private void SendError(string message) => Send(message, Color.red);
    private void SendWarning(string message) => Send(message, Color.yellow);

    protected override void OnClose()
    {
        _autoCompleteContent.DestroyAllChildren();
    }

    protected override void OnOpen()
    {
        if (ResolutionChanged)
        {
            OnThemeChange();
        }
        else
        {
            RefreshAutoComplete(_commandInput.text);
        }
        transform.SetAsLastSibling();
    }

    void RefreshAutoComplete(string text)
    {
        if (!_openMenu) return;
        _autoCompleteContent.parent.parent.GetComponent<ScrollRect>().enabled = true; // Make sure that the component is enabled

        if (_selectedAutoComplete > _autoCompleteContent.childCount - 1)
            _selectedAutoComplete = 0;
        _autoCompleteContent.DestroyAllChildren();
        if (string.IsNullOrWhiteSpace(text))
        {
            ExecuteInTicks((() =>
            {
                _autoCompleteScrollView.SetActive(_autoCompleteContent.childCount != 0);
            }), 1);
            return;
        }

        if (text.Contains(" "))
        {
            string cmd = text.Substring(0, text.IndexOf(' '));
            if (StarlightCommandManager.Commands.ContainsKey(cmd))
            {
                var argString = text;
                var split = argString.Split(' ').ToList();
                split.RemoveAt(0);
                int argIndex = split.Count - 1;
                string[] args = null;
                if (split.Count != 0)
                    args = split.ToArray();
                string containing = "";
                if (args != null) containing = args[argIndex];
                List<string> possibleAutoCompletes = null;
                try { possibleAutoCompletes = StarlightCommandManager.Commands[cmd].GetAutoComplete(argIndex, args); } catch (Exception e) { LogError($"Error in command auto complete!\n{e}"); }
                if (possibleAutoCompletes != null)
                {
                    possibleAutoCompletes = possibleAutoCompletes.Where(s => s.ToUpper().Contains(containing.ToUpper()))
                        .OrderBy(s => !s.ToUpper().StartsWith(containing.ToUpper()))
                        .ToList();
                    if (possibleAutoCompletes.Count == 0)
                        possibleAutoCompletes = null;
                }
                if (possibleAutoCompletes != null)
                {
                    int maxPredictions = MAX_AUTOCOMPLETE.Get();
                    int predicted = 0;
                    foreach (string argument in possibleAutoCompletes)
                    {
                        if (predicted >= maxPredictions) break;
                        predicted++;
                        var finalText = "";
                        if (string.IsNullOrEmpty(containing))
                            finalText =
                                "<alpha=#FF>" + argument +
                                "<alpha=#67>"; // "alpha=#FF" is the normal argument, and the "alpha=#75" is the uncompleted part. DO NOT CHANGE THE SYSTEM, ONLY THE ALPHA VALUES!!!
                        else
                        {
                            finalText = "<alpha=#67>"+new Regex(Regex.Escape(containing), RegexOptions.IgnoreCase).Replace(
                                argument,
                                "<alpha=#FF>" +
                                argument.Substring(argument.ToUpper().IndexOf(containing.ToUpper(), StringComparison.Ordinal),containing.Length)
                                + "<alpha=#67>", 1);
                        }

                        var instance = GetAutoCompletePrefab(finalText);
                        instance.gameObject.SetActive(true);
                        instance.onClick.AddListener((Action)(() =>
                        {
                            _commandInput.text = cmd;

                            if (args != null)
                            {
                                for (int i = 0; i < args.Length - 1; i++)
                                    _commandInput.text += " " + args[i];
                                _commandInput.text += " " + argument;
                            }

                            _commandInput.MoveToEndOfLine(false, false);
                        }));
                    }
                }
            }
        }
        else
        {
            var matchingCommands = StarlightCommandManager.Commands
                .Where(kv => kv.Key.ToUpper().Contains(text.ToUpper()) && !kv.Value.Hidden)
                .OrderBy(kv => !kv.Key.ToUpper().StartsWith(text.ToUpper()))
                .ThenBy(kv => kv.Key)
                .ToList();
            foreach (var valuePair in matchingCommands)
            {
                var finalText = "";
                if (string.IsNullOrEmpty(text))
                    finalText = valuePair.Key;
                else
                {
                    finalText = "<alpha=#67>"+new Regex(Regex.Escape(text), RegexOptions.IgnoreCase).Replace(
                        valuePair.Key,
                        "<alpha=#FF>" +
                        valuePair.Key.Substring(valuePair.Key.ToUpper().IndexOf(text.ToUpper(), StringComparison.Ordinal),text.Length)
                        + "<alpha=#67>", 1);
                }
                var instance = GetAutoCompletePrefab(finalText);
                instance.gameObject.SetActive(true);
                var key = valuePair.Key;
                instance.GetComponent<Button>().onClick.AddListener((Action)(() =>
                {
                    _commandInput.text = key;
                    _commandInput.MoveToEndOfLine(false, false);
                }));
            }
        }

        _autoCompleteScrollView.SetActive(_autoCompleteContent.childCount != 0);
        _autoCompleteContent.parent.parent.GetComponent<ScrollRect>().enabled = false;

    }

    protected override void OnUpdate()
    {
        try
        {
            while (_messageHistory.Count >= MAX_CONSOLELINES.Get())
            {
                _messageHistory.RemoveAt(0);
                if (_messageHistoryColor.Count > 0)
                    _messageHistoryColor.RemoveAt(0);
            }
            while (_messageHistoryColor.Count > _messageHistory.Count)
                _messageHistoryColor.RemoveAt(0);
        } catch { }

        if (!_openMenu) return;
        
        if (ResolutionChanged)
        {
            OnThemeChange();
            return;
        }

        try { if (ConsoleContent.childCount >= MAX_CONSOLELINES.Get())
            DestroyImmediate(ConsoleContent.GetChild(0).gameObject);
        } catch { }

        _commandInput.ActivateInputField();
        if (_scrollCompletelyDown)
            if (_scrollbar.value != 0)
            {
                _scrollbar.value = 0f;
                _scrollCompletelyDown = false;
            }

        if (LKey.Tab.OnKeyDown())
        {
            if (_autoCompleteContent.childCount != 0)
                try
                {
                    _autoCompleteContent.GetChild(_selectedAutoComplete).GetComponent<Button>().onClick
                        .Invoke();
                    _selectedAutoComplete = 0;
                }
                catch { }
        }

        if (LKey.Enter.OnKeyDown())
            if (_commandInput.text != "")
                Execute();

        if (_commandHistoryIdx != -1 && !_autoCompleteScrollView.active)
        {
            if (LKey.UpArrow.OnKeyDown())
            {
                _commandInput.text = _commandHistory[_commandHistoryIdx];
                _commandInput.MoveToEndOfLine(false, false);
                RefreshAutoComplete(_commandInput.text);
                _commandHistoryIdx -= 1;
                _autoCompleteScrollView.SetActive(false);
            }
        }

        if (_autoCompleteContent.childCount != 0 && _autoCompleteScrollView.active)
        {
            if (LKey.DownArrow.OnKeyDown())
                NextAutoComplete();

            if (LKey.UpArrow.OnKeyDown())
                PrevAutoComplete();
        }

        if (_selectedAutoComplete == _autoCompleteContent.childCount)
        {
            _selectedAutoComplete = 0;
        }

        if (_scrollbar)
        {
            float value = Mouse.current.scroll.ReadValue().y;
            if (Mouse.current.scroll.ReadValue().y != 0)
                _scrollbar.value =
                    Mathf.Clamp(
                        _scrollbar.value + ((value > 0.01 ? StarlightEntryPoint.consoleMaxSpeed : value < -0.01 ? -StarlightEntryPoint.consoleMaxSpeed : 0) *
                                            _scrollbar.size), 0, 1f);

        }

        try
        {
            if (_autoCompleteContent.childCount != 0)
            {
                _autoCompleteContent.GetChild(_selectedAutoComplete).GetComponent<Image>().color =
                    currentTheme.GetColor(UIColor.AutoCompleteSelected);
                if (_selectedAutoComplete > MAX_AUTOCOMPLETEONSCREEN.Get())
                    _autoCompleteContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                        _autoCompleteContent.GetComponent<RectTransform>().anchoredPosition.x,
                        (22f * UIBlueprint.ScaleFactor) * (_selectedAutoComplete - MAX_AUTOCOMPLETEONSCREEN.Get()));

                else
                    _autoCompleteContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                        _autoCompleteContent.GetComponent<RectTransform>().anchoredPosition.x, 0);
            }
        }
        catch
        {
        }
    }
    
    void NextAutoComplete()
    {
        _selectedAutoComplete += 1;
        if (_selectedAutoComplete > _autoCompleteContent.childCount - 1)
        {
            _selectedAutoComplete = 0;
            _autoCompleteContent.GetChild(_autoCompleteContent.childCount - 1).GetComponent<Image>().color =
                currentTheme.GetColor(UIColor.AutoCompleteBackground);
            _autoCompleteContent.GetChild(_selectedAutoComplete).GetComponent<Image>().color =
                currentTheme.GetColor(UIColor.AutoCompleteSelected);
        }
        else
        {
            _autoCompleteContent.GetChild(_selectedAutoComplete - 1).GetComponent<Image>().color =
                currentTheme.GetColor(UIColor.AutoCompleteBackground);
            _autoCompleteContent.GetChild(_selectedAutoComplete).GetComponent<Image>().color =
                currentTheme.GetColor(UIColor.AutoCompleteSelected);
        }
    }

    void PrevAutoComplete()
    {
        _selectedAutoComplete -= 1;

        if (_selectedAutoComplete < 0)
        {
            _selectedAutoComplete = _autoCompleteContent.childCount - 1;
            _autoCompleteContent.GetChild(0).GetComponent<Image>().color = currentTheme.GetColor(UIColor.AutoCompleteBackground);
            _autoCompleteContent.GetChild(_selectedAutoComplete).GetComponent<Image>().color =
                currentTheme.GetColor(UIColor.AutoCompleteSelected);
        }
        else
        {
            _autoCompleteContent.GetChild(_selectedAutoComplete + 1).GetComponent<Image>().color =
                currentTheme.GetColor(UIColor.AutoCompleteBackground);
            _autoCompleteContent.GetChild(_selectedAutoComplete).GetComponent<Image>().color =
                currentTheme.GetColor(UIColor.AutoCompleteSelected);
        }
    }

    void Execute()
    {
        string cmds = _commandInput.text;
        _commandHistory.Add(cmds);
        _commandHistoryIdx = _commandHistory.Count - 1;
        _commandInput.text = "";
        _autoCompleteContent.DestroyAllChildren();
        StarlightCommandManager.ExecuteByString(cmds);
    }
}