using Il2CppTMPro;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class NavigationUIBlueprintV01 : UIBlueprint
{
    public List<UIBlueprint> Tabs;
    public List<UIBlueprint> ChildrenWithButtons;
    private int _activeTab;
    private List<RectTransform> _panels = new();
    private List<RectTransform> _buttons = new();
    public Vector2 TabSize = new(1330,300);
    public float ButtonHeight = 80f;
    public float TopPadding = 40f;
    public float HorizontalPaddingPercentage = 8f;
    public float ButtonSpacing = 25f;
    public UIColor ButtonTextColor = UIColor.TextButton;
    public bool ButtonDisableAutoTranslation = false;
    public UIColorBlock ButtonColorBlock = UIColorBlock.Buttons;
    public int ButtonCornerRadius = 40;
    public FontStyles ButtonFontStyle = FontStyles.Bold;

    public int ActiveTab = 0;

    /// <summary>
    /// Called with the new index whenever a tab is chosen
    /// </summary>
    public System.Action<int> OnTabChanged = null;

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {

        _panels = new List<RectTransform>();
        _buttons = new List<RectTransform>();
        _activeTab = ActiveTab;
        var panelContainer = new GameObject("Panels");
        panelContainer.transform.SetParent(obj, false);
        CustomChildHolder = panelContainer.AddComponent<RectTransform>();
        CustomChildHolder.sizeDelta = TabSize;
        CustomChildHolder.anchoredPosition = Vector2.zero;
        
        var buttonContainer = new GameObject("Tabs");
        var bcRect = buttonContainer.AddComponent<RectTransform>();
        var bcLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        bcLayout.childAlignment = TextAnchor.MiddleCenter;
        bcLayout.childControlHeight = true;
        bcLayout.childControlWidth = true;
        bcLayout.spacing = ButtonSpacing;
        bcLayout.padding = new RectOffset((int)(HorizontalPaddingPercentage/100f * Size.x * ScaleFactorX), (int)(HorizontalPaddingPercentage/100f * Size.x * ScaleFactorX), (int)(TopPadding * ScaleFactorY), 0);
        buttonContainer.transform.SetParent(obj, false);
        bcRect.anchorMin = new Vector2(0, 1);
        bcRect.anchorMax = new Vector2(1, 1);
        bcRect.sizeDelta = new Vector2(0, (ButtonHeight+TopPadding)*ScaleFactorY);
        bcRect.anchoredPosition = new Vector2(0, bcRect.sizeDelta.y / -2);

        if(Tabs!=null)
            for (var i = 0; i < Tabs.Count; i++)
            {
                var tabIndex = i;
                var buttonUIBlueprint = new ButtonUIBlueprintV01()
                {
                    OnClick = (() => SetActiveTab(tabIndex)),
                    CornerRadius = ButtonCornerRadius,
                    ButtonColors = ButtonColorBlock,
                    Children =
                    [
                        new TextUIBlueprintV01()
                        {
                            Color = ButtonTextColor,
                            TextContent = Tabs[i].Name,
                            DisableAutoTranslation = ButtonDisableAutoTranslation,
                            Alignment = TextAlignmentOptions.Center,
                            FontStyle = ButtonFontStyle,
                            FontSize = 40,
                            Anchors = new Vector4(0,0,1,1),
                        }
                    ]
                };
                _buttons.Add(buttonUIBlueprint.Render(theme, fontTheme, bcRect));
                Tabs[i].Anchors = new Vector4(CustomChildHolder.anchorMin.x, CustomChildHolder.anchorMin.y,CustomChildHolder.anchorMax.x,CustomChildHolder.anchorMax.y);
                Tabs[i].Size = CustomChildHolder.sizeDelta;
                RectTransform panel = null;
                try
                {
                    panel = Tabs[i].Render(theme, fontTheme, CustomChildHolder);
                    panel.gameObject.SetActive(i == _activeTab);
                } catch (Exception e) { LogError(e); } 
                _panels.Add(panel); 
            }

        if(ChildrenWithButtons!=null)
            for (var i = 0; i < ChildrenWithButtons.Count; i++)
                try { ChildrenWithButtons[i].Render(theme, fontTheme, bcRect); } catch (Exception e) { LogError(e); } 
        
        ExecuteInTicks(() => { try { _buttons[_activeTab].GetComponent<Button>().interactable = false; } catch { } },1);
    }

    private void SetActiveTab(int index)
    {
        _activeTab = index;
        ActiveTab = index;
        try { OnTabChanged?.Invoke(index); } catch (Exception e) { LogError(e); }
        for (var i = 0; i < _panels.Count; i++)
            if(_panels[i])
                _panels[i].gameObject.SetActive(i == _activeTab);
        for (var i = 0; i < _buttons.Count; i++)
            if(_buttons[i])
                _buttons[i].GetComponent<Button>().interactable = i != _activeTab;
    }
}