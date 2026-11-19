using Il2CppTMPro;
using Starlight.Enums.Sounds;
using Starlight.Storage;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Starlight.Components;

/// <summary>
/// "ClickableTextLink" is a MonoBehaviour you can add every TextMeshProUGUI
/// It makes text, that is marked as a link, clickable
/// By default https:// and http:// is supported
/// If you want custom actions, use "action:somekey" where somekey is a string of your choice
/// Add the an action to the dictionary "actions"
/// </summary>
[InjectIntoIL]
public class ClickableTextLink : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private Canvas _canvas;
    /// <summary>
    /// The Dictionary to specify all custom actions
    /// </summary>
    public readonly Dictionary<string, SystemAction> Actions = new ();
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        if(_text==null) Destroy(this);
        _canvas = Find<Canvas>(transform);
        if(_canvas==null) Destroy(this);
    }
    T Find<T>(Transform obj) where T : Component
    {
        var canvas = obj.gameObject.GetComponent<T>();
        if (canvas != null) return canvas;
        if(obj.GetParent()!=null) return Find<T>(obj.GetParent());
        return null;
    }
    bool IsPointerOverUI(Vector2 screenPos)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            if (r.gameObject !=gameObject) return true;
            break;
        }
        return false;
    }
    void Click(Vector2 pos)
    {
        if (IsPointerOverUI(pos)) return;
        var cam = (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _canvas.worldCamera : null;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, pos, cam);
        if (linkIndex == -1) return;
        string id = _text.textInfo.linkInfo[linkIndex].GetLinkID();

        AudioEUtil.PlaySound(MenuSound.Click);
        if (id.StartsWith("http://")||id.StartsWith("https://")) Application.OpenURL(id);
        if (id.StartsWith("action:"))
        {
            string key = id.Substring(7);
            if (Actions.ContainsKey(key))
            {
                try { Actions[key].Invoke(); }
                catch (Exception e) { LogError(e); }
            }
        }
        /*if (id.StartsWith("callstatic:"))
        {
            string full = id.Substring(11);
            Log("Try calling");
            Log(full);
            try
            {
                full = full.Replace("()", "");
                int i = full.LastIndexOf('.');
                string typeName = full[..i];
                string methodName = full[(i + 1)..];

                Type t = null;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();;
                foreach (var asm in assemblies)
                    if ((t = asm.GetType(typeName)) != null) break;
                Log(t.FullName);
                Log(methodName);
                var info = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                if(info==null)
                    Log("Couldn't run click action! Method wasn't found");
                else info.Invoke(null,null);
            }
            catch (Exception e)
            {
                LogError(e);
                
            }

        }*/
    }
    void Update()
    {
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            Click(Mouse.current.position.ReadValue());
            return;
        }
        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            Click(Touchscreen.current.primaryTouch.position.ReadValue());
    }
}