using System;
using System.Reflection;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Enums.Sounds;
using Starlight.Storage;

namespace Starlight;
/// <summary>
/// Abstract popup class
/// </summary>
[InjectIntoIL]
public abstract class StarlightPopUp : MonoBehaviour
{
    internal Transform block;

    public static void PreAwake(GameObject obj,List<object> objects) {}
    private void DisableBlock()
    {
        if(block)
            Destroy(block.gameObject);
    }
    
    public new void Close()
    {
        AudioEUtil.PlaySound(MenuSound.ClosePopup);
        DisableBlock();
        MenuEUtil.OpenPopUps.Remove(this);
        Destroy(gameObject);
    }
    public virtual void ApplyFont(TMP_FontAsset font)
    {
        foreach (var text in gameObject.GetAllChildrenOfType<TMP_Text>())
            text.font = font;
    }
    public StarlightMenuTheme _theme = StarlightMenuTheme.Starlight;

    protected static void OpenSelf(Type type,StarlightMenuTheme theme,List<object> objects)
    {
        var instance = new GameObject("PopUp");
        var rect = instance.AddComponent<RectTransform>();
        rect.SetParent(StarlightEntryPoint.StarlightStuff.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        ExecuteInTicks((() =>
        {
            try
            {
                var methodInfo = type.GetMethod(nameof(StarlightPopUp.PreAwake),
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (methodInfo != null)
                    methodInfo.Invoke(null, [instance, objects]);

                var popup = instance.GetComponent<StarlightPopUp>();
                if (popup != null)
                    popup._theme = theme;
            }
            catch (Exception e)
            {
                LogError(e);
            }
            AudioEUtil.PlaySound(MenuSound.OpenPopup);
        }), 1);
    }
    public void Awake()
    {
        MenuEUtil.OpenPopUpBlock(this);
        MenuEUtil.OpenPopUps.Add(this);
    }

    private void Start()
    {
        OnOpen();
    }

    protected void Update()
    {
        OnUpdate();
    }
    
    protected virtual void OnOpen()
    {
        
    }

    protected virtual void OnUpdate()
    {
        
    }
}

