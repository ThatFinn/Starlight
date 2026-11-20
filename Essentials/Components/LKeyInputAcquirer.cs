using Il2CppInterop.Runtime.Attributes;
using Starlight.Enums;
using Starlight.Storage;
using UnityEngine.InputSystem;

namespace Starlight.Components;

[InjectIntoIL]
internal class LKeyInputAcquirer : MonoBehaviour
{
    internal static LKeyInputAcquirer Instance;
    private HashSet<LKey> pressedKeys = new HashSet<LKey>();
    private HashSet<LKey> downThisFrame = new HashSet<LKey>();
    private HashSet<LKey> upThisFrame = new HashSet<LKey>();

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

        // Safety net: check if any keys are physically pressed
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
                pressedKeys.Clear();
                keyMemory.Clear();
            }
        }
    }

    [HideFromIl2Cpp] internal bool OnKey(LKey key) => pressedKeys.Contains(key);
    [HideFromIl2Cpp] internal bool OnKeyDown(LKey key) => downThisFrame.Contains(key);
    [HideFromIl2Cpp] internal bool OnKeyUp(LKey key) => upThisFrame.Contains(key);
    
    [HideFromIl2Cpp]
    internal bool OnKey(LMultiKey multiKey)
    {
        foreach (var k in multiKey.keys)
            if (!OnKey(k)) return false;

        return true;
    }

    [HideFromIl2Cpp]
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
    
    [HideFromIl2Cpp]
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