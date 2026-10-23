using Starlight.Storage;

namespace Starlight.Components;

[InjectIntoIL]
internal class KeyCodeInputAcquirer : MonoBehaviour
{
    internal static KeyCodeInputAcquirer Instance;
    private HashSet<KeyCode> pressedKeys = new HashSet<KeyCode>();
    private HashSet<KeyCode> downThisFrame = new HashSet<KeyCode>();
    private HashSet<KeyCode> upThisFrame = new HashSet<KeyCode>();

    private HashSet<int> pressedButtons = new HashSet<int>();
    private HashSet<int> buttonsDownThisFrame = new HashSet<int>();
    private HashSet<int> buttonsUpThisFrame = new HashSet<int>();

    private void Start()
    {
        if(Instance!=null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    void OnGUI()
    {
        Event e = Event.current;

        if (e.isKey && e.keyCode != KeyCode.None)
        {
            if (e.type == EventType.KeyDown)
            {
                if (!pressedKeys.Contains(e.keyCode))
                {
                    pressedKeys.Add(e.keyCode);
                    downThisFrame.Add(e.keyCode);
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                pressedKeys.Remove(e.keyCode);
                upThisFrame.Add(e.keyCode);
            }
        }
        
        else if (e.type == EventType.MouseDown)
        {
            if (pressedButtons.Add(e.button)) buttonsDownThisFrame.Add(e.button);
        }
        else if (e.type == EventType.MouseUp)
        {
            pressedButtons.Remove(e.button);
            buttonsUpThisFrame.Add(e.button);
        }
    }

    void LateUpdate()
    {
        downThisFrame.Clear();
        upThisFrame.Clear();
        buttonsDownThisFrame.Clear();
        buttonsUpThisFrame.Clear();
    }

    internal bool OnKey(KeyCode key) => pressedKeys.Contains(key);

    internal bool OnKeyDown(KeyCode key) => downThisFrame.Contains(key);

    internal bool OnKeyUp(KeyCode key) => upThisFrame.Contains(key);

    internal bool OnMouse(int button) => pressedButtons.Contains(button);

    internal bool OnMouseDown(int button) => buttonsDownThisFrame.Contains(button);

    internal bool OnMouseUp(int button) => buttonsUpThisFrame.Contains(button);
}