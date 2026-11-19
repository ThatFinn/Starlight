using System;
using Starlight.EmberMode;
using UnityEngine.UI;

namespace Starlight.UI.Blueprints;

public class MenubarInteraction
{
    public SystemAction OnClick;
    public SystemAction OnHoverEnter;
    public SystemAction OnHoverExit;

    public Image TargetImage;
    public Color NormalColor;
    public Color HoverColor;
    public Color PressedColor;
}

public static class MenubarRegistry
{
    public static Dictionary<int, MenubarInteraction> Interactions = new();
}

public class MenubarEntry
{
    public string Label = "";
    public bool IsSeparator = false;
    public SystemAction OnClick = null;
    public List<MenubarEntry> SubEntries = null;
    public SystemAction OnBeforeOpen;
    public Func<bool> IsChecked;
    public bool IsDisabled;
}

public class MenubarItem
{
    public string Label = "";
    public List<MenubarEntry> Entries = new();
    public SystemAction OnBeforeOpen;
}

public class MenubarUIBlueprintV01 : UIBlueprint
{
    public List<MenubarItem> Items = new();
    public float BarHeight = 28f;
    public float ButtonPaddingH = 12f;
    public float DropdownWidth = 210f;
    public float EntryHeight = 26f;
    public float SeparatorHeight = 8f;
    public float LabelFontSize = 14f;

    private RectTransform _dropdownLayer;
    private RectTransform _blocker;
    private RectTransform _activeDropdown;
    private RectTransform _activeFlyout;
    private MenubarEntry _activeFlyoutEntry;

    protected override void OnRender(UITheme theme, FontTheme fontTheme, RectTransform obj)
    {
        var barImage = obj.gameObject.AddComponent<Image>();
        barImage.color = theme.PrimaryColor;

        obj.anchorMin = new Vector2(0, 1);
        obj.anchorMax = new Vector2(1, 1);
        obj.pivot = new Vector2(0.5f, 1f);
        obj.sizeDelta = new Vector2(0, BarHeight * ScaleFactorY);
        obj.anchoredPosition = Vector2.zero;
        obj.offsetMin = new Vector2(0, obj.offsetMin.y);
        obj.offsetMax = new Vector2(0, 0);

        var hlg = obj.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(4, 4, 0, 0);
        hlg.spacing = 0;
        
        var rootCanvas = obj.GetComponentInParent<Canvas>();
        var rootCanvasRect = rootCanvas.GetComponent<RectTransform>();

        var dlObj = new GameObject("DropdownLayer");
        _dropdownLayer = dlObj.AddComponent<RectTransform>();
        _dropdownLayer.SetParent(rootCanvasRect, false);
        _dropdownLayer.anchorMin = Vector2.zero;
        _dropdownLayer.anchorMax = Vector2.one;
        _dropdownLayer.sizeDelta = Vector2.zero;
        _dropdownLayer.anchoredPosition = Vector2.zero;
        
        var bObj = new GameObject("MenubarBlocker");
        _blocker = bObj.AddComponent<RectTransform>();
        _blocker.SetParent(_dropdownLayer, false);
        _blocker.anchorMin = Vector2.zero;
        _blocker.anchorMax = Vector2.one;
        _blocker.sizeDelta = Vector2.zero;
        _blocker.anchoredPosition = Vector2.zero;

        var bImg = bObj.AddComponent<Image>();
        bImg.color = new Color(0, 0, 0, 0);
        bImg.raycastTarget = true;

        MenubarRegistry.Interactions[bObj.GetInstanceID()] = new MenubarInteraction
        {
            OnClick = CloseAll
        };

        bObj.SetActive(false);
        
        float currentX = 4f * ScaleFactorX;

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];

            var btnWidth = (item.Label.Length * LabelFontSize * 0.62f + ButtonPaddingH * 2f) * ScaleFactorX;

            var btnObj = new GameObject($"MenuBtn_{item.Label}");
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.SetParent(obj, false);
            btnRect.sizeDelta = new Vector2(btnWidth, BarHeight * ScaleFactorY);

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0, 0, 0, 0);
            btnImg.raycastTarget = true;

            var lblObj = new GameObject("Label");
            var lblRect = lblObj.AddComponent<RectTransform>();
            lblRect.SetParent(btnRect, false);
            lblRect.anchorMin = Vector2.zero;
            lblRect.anchorMax = Vector2.one;
            lblRect.sizeDelta = Vector2.zero;
            lblRect.anchoredPosition = Vector2.zero;

            var txt = lblObj.AddComponent<Il2CppTMPro.TextMeshProUGUI>();
            txt.text = item.Label;
            txt.fontSize = LabelFontSize * ScaleFactor;
            txt.alignment = Il2CppTMPro.TextAlignmentOptions.Center;
            txt.color = theme.TextGeneralColor;
            txt.font = fontTheme.DefaultFont;
            txt.raycastTarget = false;

            var capturedItem = item;
            var capturedX = currentX;
            var capturedY = -BarHeight * ScaleFactorY;

            MenubarRegistry.Interactions[btnObj.GetInstanceID()] = new MenubarInteraction
            {
                TargetImage = btnImg,
                NormalColor = new Color(0, 0, 0, 0),
                HoverColor = theme.SecondaryColor,
                PressedColor = theme.AccentColor with { a = 0.35f },
                OnClick = () => ToggleDropdown(capturedItem, capturedX, capturedY, theme, fontTheme)
            };

            currentX += btnWidth;
        }
    }

    private void ToggleDropdown(MenubarItem item, float xPos, float yPos, UITheme theme, FontTheme fontTheme)
    {
        if (_activeDropdown)
        {
            var wasForThisItem = _activeDropdown.name == $"DD_{item.Label}";
            CloseAll();
            if (wasForThisItem) return;
        }
        OpenDropdown(item, xPos, yPos, theme, fontTheme);
    }

    private void OpenDropdown(MenubarItem item, float xPos, float yPos, UITheme theme, FontTheme fontTheme)
    {
        item.OnBeforeOpen?.Invoke();

        var totalH = CalculatePanelHeight(item.Entries);
        var panelW = CalculatePanelWidth(item.Entries);
        var panelH = totalH;

        var ddObj = new GameObject($"DD_{item.Label}");
        var ddRect = ddObj.AddComponent<RectTransform>();
        ddRect.SetParent(_dropdownLayer, false);
        ddRect.sizeDelta = new Vector2(panelW, panelH);
        ddRect.anchorMin = new Vector2(0, 1);
        ddRect.anchorMax = new Vector2(0, 1);
        ddRect.pivot = new Vector2(0, 1);
        ddRect.anchoredPosition = new Vector2(xPos, yPos);

        var bgImg = ddObj.AddComponent<Image>();
        bgImg.color = theme.SecondaryColor;

        var vlg = ddObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 2, 2);
        vlg.spacing = 0;

        var currentY = 2f * ScaleFactorY;
        foreach (var entry in item.Entries)
        {
            BuildEntry(entry, ddRect, xPos, yPos - currentY, theme, fontTheme, false);
            currentY += (entry.IsSeparator ? SeparatorHeight : EntryHeight) * ScaleFactorY;
        }

        _activeDropdown = ddRect;
        _blocker.gameObject.SetActive(true);
        _dropdownLayer.SetAsLastSibling();
    }

    private void BuildEntry(MenubarEntry entry, RectTransform parent, float parentX, float entryY, UITheme theme, FontTheme fontTheme, bool isFlyoutContent = false)
    {
        if (entry.IsSeparator)
        {
            BuildSeparator(parent, theme);
            return;
        }

        var hasChildren = entry.SubEntries is { Count: > 0 };
        var rowH = EntryHeight * ScaleFactorY;
        var panelW = parent.sizeDelta.x;

        var rowObj = new GameObject($"Entry_{entry.Label}");
        var rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.SetParent(parent, false);
        rowRect.sizeDelta = new Vector2(0, rowH);

        var rowImg = rowObj.AddComponent<Image>();
        rowImg.color = new Color(0, 0, 0, 0);
        rowImg.raycastTarget = true;

        var hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset((int)(10 * ScaleFactorX), (int)(6 * ScaleFactorX), 0, 0);
        hlg.spacing = 6f * ScaleFactorX;

        float remainingWidth = panelW - (hasChildren ? 26f : 16f) * ScaleFactorX;

        if (entry.IsChecked != null)
        {
            var checkObj = new GameObject("Check");
            var checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.SetParent(rowRect, false);

            float checkSize = 10f * ScaleFactorX;
            checkRect.sizeDelta = new Vector2(checkSize, checkSize);

            var checkImg = checkObj.AddComponent<Image>();
            checkImg.color = entry.IsChecked.Invoke() ? theme.TextGeneralColor : new Color(1, 1, 1, 0.1f);

            remainingWidth -= (checkSize + hlg.spacing);
        }

        var lblObj = new GameObject("Label");
        var lblRect = lblObj.AddComponent<RectTransform>();
        lblRect.SetParent(rowRect, false);
        lblRect.sizeDelta = new Vector2(remainingWidth, rowH);

        var txt = lblObj.AddComponent<Il2CppTMPro.TextMeshProUGUI>();
        txt.text = entry.Label;
        txt.fontSize = LabelFontSize * ScaleFactor;
        txt.alignment = Il2CppTMPro.TextAlignmentOptions.MidlineLeft;
        txt.color = entry.IsDisabled ? new Color(0.5f, 0.5f, 0.5f, 1f) : theme.TextGeneralColor;
        txt.font = fontTheme.DefaultFont;
        txt.raycastTarget = false;

        if (hasChildren)
        {
            var arrowObj = new GameObject("Arrow");
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.SetParent(rowRect, false);
            arrowRect.sizeDelta = new Vector2(20f * ScaleFactorX, rowH);

            var arrowTxt = arrowObj.AddComponent<Il2CppTMPro.TextMeshProUGUI>();
            arrowTxt.text = "►";
            arrowTxt.fontSize = (LabelFontSize - 2f) * ScaleFactor;
            arrowTxt.alignment = Il2CppTMPro.TextAlignmentOptions.MidlineRight;
            arrowTxt.color = EmberModeUITheme.TextDimColor;
            arrowTxt.font = fontTheme.DefaultFont;
            arrowTxt.raycastTarget = false;
        }

        var capturedEntry = entry;
        var capturedX = parentX + panelW;
        var capturedY = entryY;

        if (!entry.IsDisabled)
        {
            var interaction = new MenubarInteraction
            {
                TargetImage = rowImg,
                NormalColor = new Color(0, 0, 0, 0),
                HoverColor = EmberModeUITheme.HoverBackground,
                PressedColor = EmberModeUITheme.PressedBackground
            };

            if (hasChildren)
            {
                var action = (SystemAction)(() =>
                {
                    if (_activeFlyoutEntry == capturedEntry) return;
                    capturedEntry.OnBeforeOpen?.Invoke();
                    OpenFlyout(capturedEntry, capturedX, capturedY, theme, fontTheme);
                });
                interaction.OnClick = action;
                interaction.OnHoverEnter = action;
            }
            else
            {
                var onClickAction = (SystemAction)(() =>
                {
                    capturedEntry.OnClick?.Invoke();
                    CloseAll();
                });
                interaction.OnClick = onClickAction;
                if (!isFlyoutContent) interaction.OnHoverEnter = CloseFlyout;
            }

            MenubarRegistry.Interactions[rowObj.GetInstanceID()] = interaction;
        }
    }

    private void BuildSeparator(RectTransform parent, UITheme theme)
    {
        var sepObj = new GameObject("Separator");
        var sepRect = sepObj.AddComponent<RectTransform>();
        sepRect.SetParent(parent, false);
        sepRect.sizeDelta = new Vector2(0, SeparatorHeight * ScaleFactorY);

        var lineObj = new GameObject("Line");
        var lineRect = lineObj.AddComponent<RectTransform>();
        lineRect.SetParent(sepRect, false);
        lineRect.anchorMin = new Vector2(0.04f, 0.5f);
        lineRect.anchorMax = new Vector2(0.96f, 0.5f);
        lineRect.sizeDelta = new Vector2(0, 1f * ScaleFactorY);
        lineRect.anchoredPosition = Vector2.zero;

        var lineImg = lineObj.AddComponent<Image>();
        lineImg.color = EmberModeUITheme.SeparatorColor;
        lineImg.raycastTarget = false;
    }

    private void OpenFlyout(MenubarEntry parentEntry, float xPos, float yPos, UITheme theme, FontTheme fontTheme)
    {
        CloseFlyout();

        var subEntries = parentEntry.SubEntries;
        var panelW = CalculatePanelWidth(subEntries);
        var panelH = CalculatePanelHeight(subEntries);

        var flyObj = new GameObject("Flyout");
        var flyRect = flyObj.AddComponent<RectTransform>();
        flyRect.SetParent(_dropdownLayer, false);
        flyRect.sizeDelta = new Vector2(panelW, panelH);
        flyRect.anchorMin = new Vector2(0, 1);
        flyRect.anchorMax = new Vector2(0, 1);
        flyRect.pivot = new Vector2(0, 1);
        flyRect.anchoredPosition = new Vector2(xPos, yPos);

        var bgImg = flyObj.AddComponent<Image>();
        bgImg.color = theme.SecondaryColor;

        var vlg = flyObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 2, 2);
        vlg.spacing = 0;

        float currentY = 2f * ScaleFactorY;
        foreach (var sub in subEntries)
        {
            BuildEntry(sub, flyRect, xPos, yPos - currentY, theme, fontTheme, true);
            currentY += (sub.IsSeparator ? SeparatorHeight : EntryHeight) * ScaleFactorY;
        }

        _activeFlyout = flyRect;
        _activeFlyoutEntry = parentEntry;
        _activeFlyout.SetAsLastSibling();
    }

    private void CloseFlyout()
    {
        _activeFlyoutEntry = null;
        if (!_activeFlyout) return;

        foreach (var img in _activeFlyout.GetComponentsInChildren<Image>())
            MenubarRegistry.Interactions.Remove(img.gameObject.GetInstanceID());

        Object.Destroy(_activeFlyout.gameObject);
        _activeFlyout = null;
    }

    private void CloseAll()
    {
        CloseFlyout();
        if (_activeDropdown != null)
        {
            foreach (var img in _activeDropdown.GetComponentsInChildren<Image>())
                MenubarRegistry.Interactions.Remove(img.gameObject.GetInstanceID());
            Object.Destroy(_activeDropdown.gameObject);
            _activeDropdown = null;
        }

        _blocker.gameObject.SetActive(false);
    }

    private float CalculatePanelHeight(List<MenubarEntry> entries)
    {
        var h = 4f * ScaleFactorY;
        foreach (var e in entries)
            h += (e.IsSeparator ? SeparatorHeight : EntryHeight) * ScaleFactorY;
        return h;
    }

    private float CalculatePanelWidth(List<MenubarEntry> entries)
    {
        var maxLabelW = 0f;
        foreach (var e in entries)
        {
            if (e.IsSeparator) continue;
            var labelW = e.Label.Length * LabelFontSize * 0.6f;
            if (e.IsChecked != null) labelW += LabelFontSize * 1.5f;
            if (e.SubEntries is { Count: > 0 }) labelW += 20f;
            if (labelW > maxLabelW) maxLabelW = labelW;
        }
        return Mathf.Max(DropdownWidth, maxLabelW + 30f) * ScaleFactorX;
    }
}