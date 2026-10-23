using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.UI.Gadget;
using UnityEngine.UI;

namespace Starlight.Gadgets;

/// <summary>
/// Grows the gadget menu's category strip
/// </summary>
[HarmonyPatch(typeof(NewGadgetCategoryList), nameof(NewGadgetCategoryList.Start))]
public static class GadgetCategoryStrip
{
    /// <summary>
    /// The smallest a tab can be squeezed to, in pixels
    /// </summary>
    private const float MinimumTab = 24f;

    /// <summary>
    /// Name of the viewport inserted above the strip
    /// </summary>
    private const string ViewportName = "Starlight Strip Viewport";

    /// <summary>
    /// Pixels the strip moves per mouse wheel notch
    /// </summary>
    private const float MouseWheelStep = 60f;

    /// <summary>
    /// Scrollbar thickness, in pixels
    /// </summary>
    private const float BarThickness = 6f;

    private static ScrollRect _scroll;
    private static UnityEngine.InputSystem.Mouse _mouse;

    /// <summary>
    /// Fires right before the strip is built, so mods can add their categories
    /// </summary>
    public static event Action Building;

    /// <summary>
    /// How many buttons the strip currently has, or -1 while it has not been built
    /// </summary>
    /// <remarks>
    /// <see cref="GadgetCategories.ButtonCapacity"/> only reports the preexisting count,
    /// It does not account for added buttons
    /// </remarks>
    public static int Buttons { get; private set; } = -1;

    private static void Prefix(NewGadgetCategoryList __instance)
    {
        try
        {
            foreach (var handler in Building?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                try { ((Action) handler)(); }
                catch (Exception exception) { LogError($"Gadgets: a category strip subscriber threw: {exception}"); }
            }

            Grow(__instance);
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: category strip could not be grown: {exception}");
        }
    }
    
    /// <summary>
    /// Clones buttons until there is one for every category
    /// </summary>
    private static void Grow(NewGadgetCategoryList list)
    {
        var categories = list?._configuration?.Categories?.Count ?? 0;
        var buttons = list?._buttons;

        if (list is null || buttons is null || buttons.Length == 0) return;

        Buttons = buttons.Length;

        if (categories <= buttons.Length) return;

        var wanted = categories;

        if (categories > 32)
            LogWarning($"Gadgets: {categories} categories is far more than the strip was made for.");

        var template = buttons[buttons.Length - 1];
        if (template == null) return;

        var parent = template.transform.parent;
        var grown = new Il2CppReferenceArray<NewGadgetCategoryButton>(wanted);

        var filled = 0;

        for (var i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            grown[filled++] = buttons[i];
        }

        if (filled < buttons.Length)
            LogWarning($"Gadgets: category strip had {buttons.Length - filled} empty button slot(s), they were dropped.");

        for (var i = filled; i < wanted; i++)
        {
            var clone = Object.Instantiate(template.gameObject, parent);
            clone.name = $"{template.gameObject.name} ({i})";

            var button = clone.GetComponent<NewGadgetCategoryButton>();

            if (button == null)
            {
                LogWarning("Gadgets: a cloned category button had no NewGadgetCategoryButton, destroying it.");
                Object.Destroy(clone);
                break;
            }

            grown[i] = button;
            filled = i + 1;
        }

        if (filled < wanted)
        {
            var trimmed = new Il2CppReferenceArray<NewGadgetCategoryButton>(filled);
            for (var i = 0; i < filled; i++) trimmed[i] = grown[i];
            grown = trimmed;

            Trim(list, filled);
        }

        list._buttons = grown;
        Buttons = grown.Length;

        Fit(grown, buttons.Length);
    }

    /// <summary>
    /// Removes categories that have no button
    /// </summary>
    private static void Trim(NewGadgetCategoryList list, int keep)
    {
        try
        {
            var categories = list._configuration?.Categories;
            if (categories == null || categories.Count <= keep) return;

            var dropped = categories.Count - keep;

            while (categories.Count > keep)
                categories.RemoveAt(categories.Count - 1);

            LogWarning($"Gadgets: {dropped} categories had no button and were removed.");
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: categories without buttons could not be removed: {exception}");
        }
    }
    
    private static void Fit(Il2CppReferenceArray<NewGadgetCategoryButton> buttons, int original)
    {
        if (buttons.Length < 2 || original < 2) return;

        var first = buttons[0] == null ? null : buttons[0].GetComponent<RectTransform>();
        if (first == null) return;

        if (Scroll(first, buttons, original)) return;

        var parent = first.parent;
        var container = parent == null ? null : parent.TryCast<RectTransform>();
        var group = parent == null ? null : parent.GetComponent<HorizontalOrVerticalLayoutGroup>();

        var vertical = group != null && group.TryCast<VerticalLayoutGroup>() != null;

        if (group != null && container != null)
        {
            FitWithLayoutGroup(buttons, group, container, vertical);
            LayoutRebuilder.MarkLayoutForRebuild(container);
            return;
        }

        var last = buttons[original - 1] == null ? null : buttons[original - 1].GetComponent<RectTransform>();
        if (last == null) return;

        var start = first.anchoredPosition;
        var step = (last.anchoredPosition - start) / (buttons.Length - 1);
        var along = Mathf.Max(Mathf.Abs(step.x), Mathf.Abs(step.y));

        if (along < MinimumTab)
            LogWarning($"Gadgets: {buttons.Length} tab(s) leave {along:0}px each.");

        for (var i = 0; i < buttons.Length; i++)
        {
            var rect = buttons[i] == null ? null : buttons[i].GetComponent<RectTransform>();
            if (rect == null) continue;

            rect.anchoredPosition = start + step * i;

            var size = rect.sizeDelta;
            if (vertical) size.y = along; else size.x = along;
            rect.sizeDelta = size;
        }
    }

    /// <summary>
    /// Scrolls the strip to the selected tab
    /// </summary>
    [HarmonyPatch(typeof(NewGadgetCategoryList), nameof(NewGadgetCategoryList.SetSelectedCategory))]
    [HarmonyPostfix]
    private static void AfterSelect(NewGadgetCategoryList __instance)
    {
        try
        {
            if (_scroll == null || __instance == null) return;

            var buttons = __instance._buttons;
            var index = __instance._selectedIndex;

            if (buttons == null || index < 0 || index >= buttons.Length) return;

            var button = buttons[index] == null ? null : buttons[index].GetComponent<RectTransform>();
            var content = _scroll.content;
            var viewport = _scroll.viewport;

            if (button == null || content == null || viewport == null) return;

            Reveal(content, viewport, button, _scroll.horizontal);
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: category strip could not follow the selection: {exception}");
        }
    }

    /// <summary>
    /// Scrolls the strip with the mouse wheel
    /// </summary>
    [HarmonyPatch(typeof(NewGadgetCategoryList), nameof(NewGadgetCategoryList.Update))]
    [HarmonyPostfix]
    private static void AfterUpdate()
    {
        try
        {
            if (_scroll == null) return;

            var content = _scroll.content;
            var viewport = _scroll.viewport;

            if (content == null || viewport == null) return;

            var slack = _scroll.vertical
                ? content.rect.height - viewport.rect.height
                : content.rect.width - viewport.rect.width;

            if (slack <= 0f) return;

            _mouse ??= UnityEngine.InputSystem.Mouse.current;

            if (_mouse == null) return;

            var wheel = _mouse.scroll.ReadValue().y;
            if (Mathf.Abs(wheel) < 0.01f) return;
            
            var step = Mathf.Sign(wheel) * MouseWheelStep;
            var position = content.anchoredPosition;

            if (_scroll.vertical)
                position.y = Mathf.Clamp(position.y - step, 0f, Mathf.Max(0f, content.rect.height - viewport.rect.height));
            else
                position.x = Mathf.Clamp(position.x + step, -Mathf.Max(0f, content.rect.width - viewport.rect.width), 0f);

            content.anchoredPosition = position;
        }
        catch (Exception exception)
        {
            LogWarning($"Gadgets: category strip could not be scrolled: {exception.Message}");
        }
    }

    /// <summary>
    /// Moves the content enough to put a tab fully inside the viewport
    /// </summary>
    private static void Reveal(RectTransform content, RectTransform viewport, RectTransform button, bool horizontal)
    {
        var window = viewport.rect;
        var corners = new Vector3[4];

        button.GetWorldCorners(corners);

        var min = viewport.InverseTransformPoint(corners[0]);
        var max = viewport.InverseTransformPoint(corners[2]);

        var low = horizontal ? min.x : min.y;
        var high = horizontal ? max.x : max.y;
        var windowLow = horizontal ? window.xMin : window.yMin;
        var windowHigh = horizontal ? window.xMax : window.yMax;

        var shift = 0f;

        if (low < windowLow) shift = windowLow - low;
        else if (high > windowHigh) shift = windowHigh - high;

        if (Mathf.Abs(shift) < 0.5f) return;

        var position = content.anchoredPosition;

        if (horizontal) position.x += shift; else position.y += shift;

        content.anchoredPosition = position;
    }

    /// <summary>
    /// Puts the strip inside a scroll view
    /// </summary>
    /// <returns> True when the strip is scrollable and needs no further fitting </returns>
    private static bool Scroll(RectTransform first, Il2CppReferenceArray<NewGadgetCategoryButton> buttons, int original)
    {
        try
        {
            var content = first.parent == null ? null : first.parent.TryCast<RectTransform>();
            if (content == null) return false;

            var existing = content.parent == null ? null : content.parent.GetComponent<ScrollRect>();

            if (existing != null && content.parent.name == ViewportName)
            {
                _scroll = existing;
                return Resize(existing, content, buttons, original);
            }

            var host = content.parent == null ? null : content.parent.TryCast<RectTransform>();
            if (host == null) return false;

            var viewport = new GameObject(ViewportName).AddComponent<RectTransform>();
            viewport.SetParent(host, false);

            viewport.anchorMin = content.anchorMin;
            viewport.anchorMax = content.anchorMax;
            viewport.pivot = content.pivot;
            viewport.anchoredPosition = content.anchoredPosition;
            viewport.sizeDelta = content.sizeDelta;
            viewport.localScale = content.localScale;
            viewport.SetSiblingIndex(content.GetSiblingIndex());

            viewport.gameObject.AddComponent<RectMask2D>();

            content.SetParent(viewport, false);
            content.anchoredPosition = Vector2.zero;

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            scroll.inertia = false;

            _scroll = scroll;

            var fitted = Resize(scroll, content, buttons, original);
            
            AddBar(scroll, viewport);

            return fitted;
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: category strip could not be made scrollable, falling back to fitting: {exception}");
            return false;
        }
    }

    /// <summary>
    /// Adds a bar showing where you currently are
    /// </summary>
    private static void AddBar(ScrollRect scroll, RectTransform viewport)
    {
        try
        {
            var vertical = scroll.vertical;

            var bar = new GameObject("Starlight Strip Scrollbar").AddComponent<RectTransform>();
            bar.SetParent(viewport, false);

            bar.anchorMin        = vertical ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            bar.anchorMax        = vertical ? new Vector2(1f, 1f) : new Vector2(1f, 0f);
            bar.pivot            = new Vector2(1f, 0f);
            bar.sizeDelta        = vertical ? new Vector2(BarThickness, 0f) : new Vector2(0f, BarThickness);
            bar.anchoredPosition = Vector2.zero;

            var track = bar.gameObject.AddComponent<Image>();
            track.color = new Color(0f, 0f, 0f, 0.25f);
            
            track.raycastTarget = false;

            var handle = new GameObject("Handle").AddComponent<RectTransform>();
            handle.SetParent(bar, false);
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            handle.sizeDelta = Vector2.zero;

            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color(1f, 1f, 1f, 0.55f);
            handleImage.raycastTarget = false;

            var scrollbar = bar.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = vertical ? Scrollbar.Direction.BottomToTop : Scrollbar.Direction.LeftToRight;
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;

            if (vertical) scroll.verticalScrollbar = scrollbar;
            else scroll.horizontalScrollbar = scrollbar;
        }
        catch (Exception exception)
        {
            LogWarning($"Gadgets: category strip scrollbar could not be built: {exception.Message}");
        }
    }

    /// <summary>
    /// Sizes the scrolled content to hold every tab
    /// </summary>
    private static bool Resize(ScrollRect scroll, RectTransform content, Il2CppReferenceArray<NewGadgetCategoryButton> buttons, int original)
    {
        var first = buttons[0] == null ? null : buttons[0].GetComponent<RectTransform>();
        var authored = original >= 2 && buttons[original - 1] != null
            ? buttons[original - 1].GetComponent<RectTransform>()
            : null;

        if (first == null || authored == null) return true;

        var step = (authored.anchoredPosition - first.anchoredPosition) / (original - 1);
        var horizontal = Mathf.Abs(step.x) >= Mathf.Abs(step.y);

        var pitch = horizontal ? Mathf.Abs(step.x) : Mathf.Abs(step.y);

        for (var i = 0; i < buttons.Length; i++)
        {
            var rect = buttons[i] == null ? null : buttons[i].GetComponent<RectTransform>();
            if (rect == null) continue;

            if (horizontal)
            {
                rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
                rect.anchorMax = new Vector2(0f, rect.anchorMax.y);
                rect.pivot = new Vector2(0f, rect.pivot.y);
                rect.anchoredPosition = new Vector2(i * pitch, rect.anchoredPosition.y);
            }
            else
            {
                rect.anchorMin = new Vector2(rect.anchorMin.x, 1f);
                rect.anchorMax = new Vector2(rect.anchorMax.x, 1f);
                rect.pivot = new Vector2(rect.pivot.x, 1f);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -i * pitch);
            }
        }

        var span = step * (buttons.Length - 1);

        scroll.horizontal = horizontal;
        scroll.vertical = !horizontal;
        
        var needed = Mathf.Abs(horizontal ? span.x : span.y)
                     + (horizontal ? authored.rect.width : authored.rect.height);

        var size = content.sizeDelta;

        if (horizontal) size.x = Mathf.Max(size.x, needed);
        else size.y = Mathf.Max(size.y, needed);

        if (horizontal)
        {
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot     = new Vector2(0f, 0.5f);
            content.sizeDelta = new Vector2(size.x, 0f);
        }
        else
        {
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot     = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, size.y);
        }

        content.anchoredPosition = Vector2.zero;

        return true;
    }

    /// <summary>
    /// Sizes each tab
    /// </summary>
    private static void FitWithLayoutGroup(
        Il2CppReferenceArray<NewGadgetCategoryButton> buttons,
        HorizontalOrVerticalLayoutGroup group,
        RectTransform container,
        bool vertical)
    {
        if (vertical)
        {
            group.childControlHeight = true;
            group.childForceExpandHeight = false;
        }
        else
        {
            group.childControlWidth = true;
            group.childForceExpandWidth = false;
        }

        var padding = group.padding;
        var available = (vertical ? container.rect.height - padding.vertical : container.rect.width - padding.horizontal)
                        - group.spacing * (buttons.Length - 1);

        if (available <= 0f) return;

        var each = available / buttons.Length;

        if (each < MinimumTab)
        {
            group.spacing = 0f;
            available = vertical ? container.rect.height - padding.vertical : container.rect.width - padding.horizontal;
            each          = available / buttons.Length;
        }

        if (each < MinimumTab)
            LogWarning($"Gadgets: {buttons.Length} tab(s) leave {each:0}px each.");

        foreach (var button in buttons)
        {
            if (button == null) continue;

            var element = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();

            if (vertical)
            {
                element.minHeight = each;
                element.preferredHeight = each;
                element.flexibleHeight = 0f;
            }
            else
            {
                element.minWidth = each;
                element.preferredWidth = each;
                element.flexibleWidth = 0f;
            }
        }
    }
}