using System;
using Starlight.Components.AssetBundle;
using UnityEngine.Rendering;

namespace Starlight.UI.Blueprints;

public abstract class UIBlueprint
{
    private static Canvas _menuCanvas;

    /// <summary>
    /// The scale factor of the canvas Starlight menus use 
    /// </summary>
    internal static float CanvasScale
    {
        get
        {
            if (!_menuCanvas)
            {
                if (!StarlightEntryPoint.StarlightStuff) return 1f;
                _menuCanvas = StarlightEntryPoint.StarlightStuff.GetComponent<Canvas>();
            }
            return _menuCanvas && _menuCanvas.scaleFactor > 0f ? _menuCanvas.scaleFactor : 1f;
        }
    }

    public static float ScaleFactor => Screen.height / CanvasScale / 1080f;
    internal static float ScaleFactorY => ScaleFactor;
    internal static float ScaleFactorX => ScaleFactor;
    [Obsolete("Please use Name instead")]public string mame = null;
    public string Name = "<MissingName>"; 
    public Vector2 Size = new(100,100);
    public Vector2 Position = Vector2.zero;
    public Vector2 Rotation = new ();
    public Vector4 Anchors = new (0.5f, 0.5f, 0.5f, 0.5f);
    public Vector2 Pivot = new (0.5f, 0.5f);
    public List<UIBlueprint> Children;
    public int CornerRadius = 0;
    protected RectTransform CustomChildHolder;
    protected bool IgnoreCorners = false;
    public List<Il2CppSystem.Type> Components;
    public RectTransform Render(UITheme theme, FontTheme fontTheme, Transform parent)
    {
        var obj = new GameObject(mame??Name);
        obj.transform.localRotation=Quaternion.Euler(Rotation.x,Rotation.y,0);
        var rectT = obj.AddComponent<RectTransform>();
        obj.transform.SetParent(parent, false);
        rectT.anchorMin = new Vector2(Anchors.x, Anchors.y);
        rectT.anchorMax = new Vector2(Anchors.z, Anchors.w);
        rectT.pivot = Pivot;
        rectT.sizeDelta = new Vector2(Size.x*ScaleFactorX, Size.y*ScaleFactorY);
        rectT.anchoredPosition = Position*ScaleFactor;
        try { OnRender(theme, fontTheme, rectT); } catch (Exception e) { LogError(e); }

        if(CornerRadius>0&&!IgnoreCorners)
        {
            var sortGroup = obj.AddComponent<SortingGroup>();
            sortGroup.enabled = false;
            sortGroup.sortingOrder = Mathf.FloorToInt(CornerRadius * ScaleFactor);
            obj.AddComponent<RoundedUIImage>().CornerRadius = CornerRadius * ScaleFactor;
        }
        
        if(Components != null)
            foreach (var comp in Components)
                try { obj.AddComponent(comp); } catch (Exception e) { LogError(e); } 
        
        if (Children != null)
            foreach (var child in Children)
                try {  child.Render(theme, fontTheme, CustomChildHolder ?? rectT); } catch (Exception e) { LogError(e); } 
        
        try { AfterRenderChildren(theme, fontTheme, rectT); } catch (Exception e) { LogError(e); }
        return rectT;
    }
    
    protected abstract void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj);
    protected virtual void AfterRenderChildren(UITheme theme, FontTheme fontTheme, RectTransform obj) { }
}