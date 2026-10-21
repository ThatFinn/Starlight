using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Starlight.Utils;

public static class InputKeyEUtil
{
    private static readonly Dictionary<KeyCode, Key> Renamed = new()
    {
        { KeyCode.Return, Key.Enter },
        { KeyCode.KeypadEnter, Key.NumpadEnter },
        { KeyCode.LeftControl, Key.LeftCtrl },
        { KeyCode.RightControl, Key.RightCtrl },
        { KeyCode.LeftApple, Key.LeftCommand },
        { KeyCode.RightApple, Key.RightCommand },
        { KeyCode.LeftWindows, Key.LeftWindows },
        { KeyCode.RightWindows, Key.RightWindows },
        { KeyCode.Print, Key.PrintScreen },
        { KeyCode.Numlock, Key.NumLock },
        { KeyCode.BackQuote, Key.Backquote },
        { KeyCode.Menu, Key.ContextMenu }
    };

    private static Dictionary<KeyCode, Key> toKey;
    private static Dictionary<Key, KeyCode> toKeyCode;

    private static Dictionary<KeyCode, Key> Forward()
    {
        if (toKey is not null) return toKey;

        toKey = new Dictionary<KeyCode, Key>();

        foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
        {
            if (Renamed.TryGetValue(code, out var renamed))
            {
                toKey[code] = renamed;
                continue;
            }
            
            var name = code.ToString();

            if (name.StartsWith("Alpha", StringComparison.Ordinal)) name = "Digit" + name.Substring(5);
            else if (name.StartsWith("Keypad", StringComparison.Ordinal)) name = "Numpad" + name.Substring(6);

            toKey[code] = Enum.TryParse<Key>(name, out var key) ? key : Key.None;
        }

        return toKey;
    }

    private static Dictionary<Key, KeyCode> Backward()
    {
        if (toKeyCode is not null) return toKeyCode;

        toKeyCode = new Dictionary<Key, KeyCode>();
        
        foreach (var pair in Forward())
            if (pair.Value != Key.None && !toKeyCode.ContainsKey(pair.Value))
                toKeyCode[pair.Value] = pair.Key;

        return toKeyCode;
    }

    /// <summary>
    /// The input system key for a <see cref="KeyCode"/>, or <see cref="Key.None"/>.
    /// </summary>
    public static Key ToKey(this KeyCode code) => Forward().TryGetValue(code, out var key) ? key : Key.None;

    /// <summary>
    /// The <see cref="KeyCode"/> for an input system key, or <see cref="KeyCode.None"/>.
    /// </summary>
    public static KeyCode ToKeyCode(this Key key) => Backward().TryGetValue(key, out var code) ? code : KeyCode.None;

    /// <summary>
    /// Whether a <see cref="KeyCode"/> can be expressed as an input system key at all.
    /// </summary>
    public static bool IsBindable(this KeyCode code) => code.ToKey() != Key.None;

    /// <summary>
    /// The mouse button index the input system and <c>Event.current.button</c> both use.
    /// </summary>
    public static int ToIndex(this MouseButton button) => (int)button;

    /// <summary>
    /// The mouse button for an index, for reading <c>Event.current.button</c>.
    /// </summary>
    public static MouseButton ToMouseButton(this int index) => (MouseButton)index;
}