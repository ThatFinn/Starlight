using Starlight.Enums;
using Starlight.Storage;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Starlight.Components;

[InjectIntoIL]
internal class LKeyInputAcquirer : MonoBehaviour
{
    internal static LKeyInputAcquirer Instance;
    private HashSet<LKey> pressedKeys = new HashSet<LKey>();
    private HashSet<LKey> downThisFrame = new HashSet<LKey>();
    private HashSet<LKey> upThisFrame = new HashSet<LKey>();

    private static readonly (System.Func<Gamepad, ButtonControl> control, LKey key)[] GamepadBindings =
    {
        (g => g.buttonSouth,        LKey.GamepadSouth),
        (g => g.buttonNorth,        LKey.GamepadNorth),
        (g => g.buttonEast,         LKey.GamepadEast),
        (g => g.buttonWest,         LKey.GamepadWest),
        (g => g.leftShoulder,       LKey.GamepadL1),
        (g => g.rightShoulder,      LKey.GamepadR1),
        (g => g.leftTrigger,        LKey.GamepadL2),
        (g => g.rightTrigger,       LKey.GamepadR2),
        (g => g.leftStickButton,    LKey.GamepadL3),
        (g => g.rightStickButton,   LKey.GamepadR3),
        (g => g.startButton,        LKey.GamepadStart),
        (g => g.selectButton,       LKey.GamepadSelect),
        (g => g.dpad.up,            LKey.GamepadUp),
        (g => g.dpad.down,          LKey.GamepadDown),
        (g => g.dpad.left,          LKey.GamepadLeft),
        (g => g.dpad.right,         LKey.GamepadRight),
    };


    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private KeyCode tmpKeyCode = KeyCode.None;
    private Dictionary<KeyCode, LKey> keyMemory = new Dictionary<KeyCode, LKey>();

    void OnGUI()
    {
        Event e = Event.current;
        if (!e.isKey) return;

        LKey key;

        if (e.type == EventType.KeyDown)
        {
            if (InputEUtil.TryConvertEventToKey(e, out key))
            {
                // Pair tmpKeyCode if exists
                if (tmpKeyCode != KeyCode.None)
                {
                    keyMemory[tmpKeyCode] = key;
                    tmpKeyCode = KeyCode.None;
                }

                if (!pressedKeys.Contains(key))
                {
                    pressedKeys.Add(key);
                    downThisFrame.Add(key);
                }
            }
            else
            {
                if (e.keyCode != KeyCode.None)
                    tmpKeyCode = e.keyCode;
            }
        }
        else if (e.type == EventType.KeyUp)
        {
            // 1. Try paired
            if (keyMemory.TryGetValue(e.keyCode, out key))
            {
                pressedKeys.Remove(key);
                upThisFrame.Add(key);
                keyMemory.Remove(e.keyCode);
            }
            // 2. Try convert (for function keys, modifiers, arrows)
            else if (InputEUtil.TryConvertEventToKey(e, out key))
            {
                pressedKeys.Remove(key);
                upThisFrame.Add(key);
            }
        }
    }

    // Reset tmp var each frame
    void LateUpdate()
    {
        downThisFrame.Clear();
        upThisFrame.Clear();
        tmpKeyCode = KeyCode.None;

        // Safety net: check if any keyboard keys are physically pressed
        // Only clears keyboard-related keys, not gamepad keys
        if (Keyboard.current != null)
        {
            bool anyPressed = false;
            foreach (var k in Keyboard.current.allKeys)
                try
                {
                    if (k.isPressed)
                    {
                        anyPressed = true;
                        break;
                    }
                }
                catch {}

            if (!anyPressed)
            {
                var toRemove = new List<LKey>();
                foreach (var key in pressedKeys)
                    if ((int)key < 1101) // only remove non-gamepad keys
                        toRemove.Add(key);
                foreach (var key in toRemove)
                    pressedKeys.Remove(key);
                keyMemory.Clear();
            }
        }

        // Gamepad polling
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            foreach (var (getControl, lkey) in GamepadBindings)
            {
                try
                {
                    var control = getControl(gamepad);
                    if (control.wasPressedThisFrame)
                    {
                        pressedKeys.Add(lkey);
                        downThisFrame.Add(lkey);
                    }
                    else if (control.wasReleasedThisFrame)
                    {
                        pressedKeys.Remove(lkey);
                        upThisFrame.Add(lkey);
                    }
                }
                catch {}
            }
        }
        else
        {
            // No gamepad connected — clear any stale gamepad keys
            foreach (var (_, lkey) in GamepadBindings)
                pressedKeys.Remove(lkey);
        }
    }


    internal bool OnKey(LKey key) => pressedKeys.Contains(key);
    internal bool OnKeyDown(LKey key) => downThisFrame.Contains(key);
    internal bool OnKeyUp(LKey key) => upThisFrame.Contains(key);
    internal bool OnKey(LMultiKey multiKey)
    {
        foreach (var k in multiKey.keys)
            if (!OnKey(k)) return false;

        return true;
    }

    internal bool OnKeyDown(LMultiKey multiKey)
    {
        int i = 0;
        bool wasThisFrame = false;
        foreach (var key in multiKey.keys)
        {
            if (key.OnKey()) i++;
            if (wasThisFrame) continue;
            if (key.OnKeyDown()) wasThisFrame = true;
        }
        if (!wasThisFrame) return false;
        return i == multiKey.keys.Length;
    }
    internal bool OnKeyUp(LMultiKey multiKey)
    {
        int i = 0;
        bool wasThisFrame = false;
        foreach (var key in multiKey.keys)
        {
            if (key.OnKey()) i++;
            if (wasThisFrame) continue;
            if (key.OnKeyUp()) wasThisFrame = true;
        }

        if (!wasThisFrame) return false;
        return i+1 == multiKey.keys.Length;
    }
}